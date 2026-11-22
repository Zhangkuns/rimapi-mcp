# 成熟产蛋（WORK-D29）

`POST /api/v1/debug/native/execute` 的 `work.lay_egg` 让一只成熟、可产蛋的动物调用一次原生 `CompEggLayer.ProduceEgg()`，并按原版 `JobDriver_LayEgg` 把真实蛋落在动物附近。

```json
{"action":"work.lay_egg","map_id":0,"thing_id":"Chicken27979","dry_run":true}
```

## 参数与前置

- `map_id`、`thing_id`（动物）必填；`dry_run` 只预览。
- 目标必须 spawned、存活、是 `Pawn` 且 `RaceProps.Animal` 有 `CompEggLayer`。
- 必须 `CompEggLayer.CanLayNow == true`（`CanLayNow` 已包含原版 `Active`、成熟/可产蛋 life stage、性别、Sterile 等条件）；否则 422。
- 本动作没有蛋箱参数：蛋按原版 `JobDriver_LayEgg` 落到动物 `Position` 附近；不会走 `CompEggContainer` 收集分支。

## 原生语义

1. `ProduceEgg()` 原生随机 `Props.eggCountRange.RandomInRange` 作为 stack；0 时返回 null。
2. 受精计数 >0 时用 `eggFertilizedDef`，并按随机数扣减 `fertilizationCount`；否则 `eggUnfertilizedDef`。
3. 原生设置 `eggProgress=0`；有 `CompHatcher` 的蛋写入 `hatcheeFaction=parent.Faction`、`hatcheeParent=parent`、`otherParent=fertilizedBy`。
4. 动作按 `JobDriver_LayEgg` 使用 `GenPlace.TryPlaceThing(..., ThingPlaceMode.Near)`；非玩家 faction 的蛋设置 forbidden。不直接 `MakeThing`、不追加额外蛋、不批量。

## 读回

- `animal`/`animal_after`：thingID、def、位置、性别、spawned/dead、faction、`can_lay_now`、`egg_progress`、`next_egg_type`、`egg_count_range`、`fully_fertilized`。
- `egg_progress_before/after`：从原生 `CompInspectStringExtra()` 的 “Egg progress: N%” 文本解析；解析失败时按 `CanLayNow` 回退（可产即 1，否则 0）。
- `next_egg_type`/`next_egg_type_after`、`egg_count_range`、`fully_fertilized`/`fully_fertilized_after`。
- `egg`：实际落地的蛋（thingID、def、quantity、remaining_stack、位置、是否与旧堆叠）；`egg_generated`、`placement_succeeded`。
- `fertilized`：实际蛋 def 是否 `eggFertilizedDef`（未受精蛋没有 `CompHatcher`，所以 `hatcher=null` 是正确的）。
- `hatcher`：仅当蛋有 `CompHatcher` 时给出 `hatchee_faction`、`hatchee_parent`、`other_parent`、`fertilized`、`egg_def`；未受精蛋为 null。
- `items_total_before/after`、`applied/native_invoked/changed`；`would_lay` 为 dry-run 的原生预览（next type/range/fully fertilized/progress）。
- 鸡蛋 `eggCountRange` 为 1；原版其它物种可能随机多枚，`quantity` 以实际值为准。

## 状态码与边界

- 缺 `map_id`/`thing_id`：400；地图/动物不存在：404；非动物、无 `CompEggLayer`、`CanLayNow=false`（未成熟/已产/不可产）：422。
- 关闭开发者模式：403；进程不符/幂等键冲突：409；原生异常或产出后无法落地：500 uncertain，保留实际蛋/动物/物品读回。
- 同 key 重放不再产第二枚；同 key 换 body 409；已产空/未成熟新 key 4xx。
- 不直接 `MakeThing` 伪造、不刷蛋、不批量、不替代蛋箱收集/孵化；一次只调用一次原生 `ProduceEgg`。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；母鸡 Chicken27979 原生 DEV 进度回满后，未受精蛋 `EggChickenUnfertilized27982`（quantity 1）与受精蛋 `EggChickenFertilized27983`（rooster 为 other parent）均由 `ProduceEgg` 真实生成；hatcher 阵营 PlayerColony。详见[WORK-D29 验收与自审](../references/reviews/2026-11-21-work-d29-lay-egg.md)和[原始证据](../../tests/parallel/work-d29/evidence-work-d29-lay-egg.json)。正式 DLL 未部署。
