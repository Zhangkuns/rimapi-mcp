# 屠宰尸体一轮

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.butcher`，用指定屠宰台/屠宰点上的**现有屠宰账单**处理一具指定尸体并结算一轮。它复用原版 `GenRecipe.MakeRecipeProducts` 的 `SpecialProductType.Butchery` 分支（`Corpse.ButcherProducts`），不创建 Job、不消耗工作时间。游戏暂停时也能执行，请求前后 `TicksGame` 不变。

```json
{"action":"work.butcher","map_id":0,"thing_id":"27979","bill_id":"0","pawn_id":"27986","ingredients":["Corpse_Muffalo27994"],"dry_run":true}
```

尸体通过共享 DTO 的 `ingredients[]` 传入，**必须恰好一个** Corpse ThingID；账单用 `bill_id` 或 `bill_index` 指定，`thing_id` 是屠宰台/屠宰点的 ThingID 或数字 ID。

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 屠宰台/屠宰点，必须实现 `IBillGiver`。 |
| `bill_id` / `bill_index` | 二选一 | 该台/点上的账单；两者同时给出时必须指向同一账单。 |
| `pawn_id` | 是 | 同图、存活、未倒地、玩家阵营、满足账单限制与技能的工人。 |
| `ingredients` | 是 | 恰好一个 Corpse ThingID（字符串数组）。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 结算语义

真实执行按原版 `Toils_Recipe.FinishRecipeAndStartStoringProduct` 的顺序：

1. `GenRecipe.MakeRecipeProducts` 走 `SpecialProductType.Butchery`：对尸体调用 `Corpse.ButcherProducts(worker, efficiency)`，生成肉、皮和可能的身体部位；效率来自 `ButcheryFleshEfficiency` × 工作台效率；
2. `recipe.Worker.ConsumeIngredient(corpse, recipe, map)` 销毁尸体（默认 `Destroy(Vanish)`，不是 `Destroy(Cancel)`）；
3. `bill.Notify_IterationCompleted(pawn, ingredients)` 递减重复次数/发完成消息；
4. `RecordsUtility.Notify_BillDone`；
5. 产物按账单存储模式落地/入库（`DropOnFloor` 就近，`BestStockpile`/`SpecificStockpile` 直接入存储）。

`Corpse.ButcherProducts` 本身会在工人脚下留下血污，对人形尸体会记历史事件和故事；这些是原版副作用，见下。

## 响应

- `corpse`：ThingID/def、`inner_pawn_def`、腐烂阶段、肉的 def、皮的 def、位置；
- `efficiency`：本次 `ButcherProducts` 使用的原版效率值；
- `bill`：`bill_id`、配方、重复模式、`repeat_count_before/after`、存储模式；
- `products`：每堆的 def、本次 `count`、`thing_id`、最终 `stack_count`、位置、`stored`；
- `corpse_after`：尸体 `destroyed`/`spawned`、`inner_pawn_present`；
- `bill_count_before/after`：原生 `WorkerCounter.CountProducts`（屠宰账单统计存储区生肉）；`applied`/`native_invoked`/`changed`/`outcome`。

肉/皮可能被拆成多堆（例如超过堆叠上限），`products[].count` 是本次新增总量，`stack_count` 是最后落点堆的现存数量；用独立地图读取核对总量更可靠。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`/`pawn_id`、`ingredients[]` 不是恰好一个、`bill_id`/`bill_index` 缺失或冲突 | 400 `bad_request` |
| 地图、工作台、账单、尸体、工人不存在 | 404 `not_found` |
| 目标不是账单工作台、账单不属于该台/点、不是屠宰配方、尸体不是 `Corpse`/spawned/有内体、腐烂或不符合配方/账单过滤、活体、工作台不可用、账单完成/挂起、工人不满足限制 | 422 `invalid_target` |
| 尸体已被某个工人的当前 Job 占用 | 409 `conflict` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在修改前完成；失败不会销毁尸体、产肉或记账单。原版 `ButcherCorpseFlesh` 的 `fixedIngredientFilter` 已排除腐烂、机械族和不产肉的尸体，本动作直接复用它。

## 边界与幂等

- 只结算一具已有尸体的一轮屠宰；不接 `/autoslaughter`，不刷肉，不放宽 `work.bill_once` 的 `Butchery` 拒绝，也不处理 D03B 半成品。
- 不计算工作 XP/工作时间，不创建搬运 Job；尸体只被 `ConsumeIngredient` 销毁一次。`ButcherProducts` 的血污/历史/故事副作用保留原版行为。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不重复产肉/扣尸；同键改请求（含换尸体）为 409。尸体消费后用同一 ThingID 请求返回 404。
