# 动物采奶/剪毛（WORK-D17）

`POST /api/v1/debug/native/execute` 的 `work.gather` 对一只现有动物的所有 `CompHasGatherableBodyResource` 中 `ActiveAndFull` 的 comp 逐个调用原生 `Gathered(doer)`。原生负责 `AnimalGatherYield` 随机、产物数量与落地；本动作不预掷随机、不改 `fullness`、不手工生成产物。

```json
{"action":"work.gather","map_id":0,"thing_id":"Cow27979","pawn_id":800,"dry_run":true}
```

## 参数

- `map_id`、`thing_id`（动物）、`pawn_id`（采集者）必填。
- 动物必须 spawn、未死、`RaceProps.Animal`；工人必须同图、spawn、存活、属于玩家阵营。
- `dry_run:true` 只读预演，给出 `would_gather`；不调用 `Gathered`，`fullness`/物品不变。
- 可设 `expected_process_id`；同一次游戏内重试可用 `Idempotency-Key`，见[通用调试合同](../debug-native-actions.md)和[幂等合同](idempotency.md)。

## 原生语义

1. 收集动物全部 `CompHasGatherableBodyResource`，只对调用开始时 `ActiveAndFull` 的 comp 采集；没有则 422，不触碰任何 comp。
2. 每个 comp 调用 `Gathered(doer)`：
   - `Rand.Chance(doer.GetStatValue(StatDefOf.AnimalGatherYield))` 决定成功/浪费；
   - 失败会显示 `TextMote_ProductWasted`，不产生产物；
   - 成功时 `GenMath.RoundRandom(ResourceAmount * fullness)` 生成真实 `ResourceDef`，在**工人位置**附近 `GenPlace.TryPlaceThing`；
   - 无论成功/浪费，原生最后把该 comp 的 `fullness=0`。
3. 本动作**不直接改 fullness、不刷产物、不绕过随机**；`succeeded`/`wasted` 是调用后按整图物品 before/after 差异推断（原生 `Gathered` 返回 void），不是预掷结果。
4. 一次请求按 comp 顺序逐个采集，每个 comp 单独记录；多 comp 时逐个结算。

## 读回

- `animal`/`animal_after`：thingID、def、位置、性别、life stage、comps（type、resource_def、resource_amount、fullness、active_and_full）。
- `animals`/`animals_after`：目标与地图上其它可采集动物，用于证明其它动物保持。
- `gathers[]`：每个 active-full comp 的 `comp_type`、`resource_def`、`resource_amount`、`fullness_before/after`、`gather_yield`、`succeeded`、`wasted`、`products`。
- `products`：实际产物扁平列表（def、quantity、thingID、remaining_stack、位置、是否与旧堆叠）；`gather_succeeded`/`wasted` 为本次是否有任一成功/浪费。
- `items_total_before/after`、`applied/native_invoked/changed`；`dry_run` 另有 `would_gather[*]`（type/resource/fullness/resource_amount/gather_yield），`gathers=[]`、`products=[]`、前后不变。
- 产物落在**工人位置附近**，不是动物位置；具体 `thingID/位置`以读回为准。

## 状态码与边界

- 缺 `map_id`/`thing_id`/`pawn_id`：400。
- 地图、目标动物、工人不存在：404。
- 非动物、动物未 spawn/已死、无 gatherable comp、无 ActiveAndFull、工人不同图/死亡/非玩家阵营：422，且不动 fullness。
- 关闭开发者模式：403；进程不符/幂等键冲突：409。
- 原生异常：500 uncertain，保留实际 animals/items 读回。
- 一次只处理一只动物，不批量全图；不替代 D18 宰杀，不直接修改奶/毛库存或 fullness。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；原生随机实际产生一次 Milk 成功、一次 Milk 浪费、一次 WoolMuffalo 成功（100+20 两堆），并验证同键重放/换动物 409、空/死/非动物拒绝；见[WORK-D17 验收与自审](../references/reviews/2026-11-21-work-d17-gather.md)及[原始证据](../../tests/parallel/work-d17/evidence-work-d17-gather.json)。正式 DLL 未部署。
