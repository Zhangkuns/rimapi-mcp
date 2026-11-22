# 完成现有半成品

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.finish_unfinished`，完成一个已存在的原版 `UnfinishedThing`（半成品，UFT）。它复用 UFT 自己保存的材料、配方和绑定账单，走原版完成链；不创建 Job、不消耗工作时间。游戏暂停时也能执行，请求前后 `TicksGame` 不变。

```json
{"action":"work.finish_unfinished","map_id":0,"thing_id":"27992","pawn_id":"27981","dry_run":true}
```

`thing_id` 是 UFT 的 ThingID 或数字 ID；`pawn_id` 必须是该 UFT 的创建者（原版 `WorkGiver_DoBill.FinishUftJob`/`BoundWorker` 也这样绑定）。账单由 `uft.BoundBill` 推导，**不接受**另传 `bill_id`；配方来自账单和 UFT 自身字段。

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 已生成的 `UnfinishedThing`；必须 spawned、未销毁。 |
| `pawn_id` | 是 | 该图上的工人数字 ID，必须是 UFT 创建者，且存活、未倒地、玩家阵营、满足账单/配方资格。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 结算语义

原版 `Verse.AI.Toils_Recipe.CalculateIngredients` 对 UFT 目标的分支是：

1. `recipe.Worker.ConsumeIngredient(uft, recipe, map)`：销毁 UFT。默认 `DestroyMode.Vanish`，**不**触发 `UnfinishedThing.Destroy(Cancel)` 的 75% 材料返还；
2. 取 UFT 保存的 `ingredients` 列表；
3. `GenRecipe.MakeRecipeProducts`（品质、风格、效率、副产物）；
4. 逐件 `recipe.Worker.ConsumeIngredient(ingredient, recipe, map)`（不二次扣 UFT 材料）；
5. `bill.Notify_IterationCompleted(pawn, ingredients)`：`Bill_ProductionWithUft` 先清 UFT 绑定，`Bill_Production` 再递减重复次数/发完成消息；
6. `RecordsUtility.Notify_BillDone`；
7. 产物按账单存储模式落地。

本动作按同一顺序执行，产物可能被 `CompQuality` 赋予品质，`Bow_Short` 等固定材料配方的产物 `stuff_def_name` 为 null 是原版正常结果。

预演返回 `would_finish`、`ingredients_before`（def、数量、ThingID）、`products_preview`（普通产物确定性数量）和 `bill_count_before`；不生成产物、不销毁 UFT/材料、不改账单。

真实响应：

- `unfinished`：UFT def/recipe/创建者/workLeft/位置；
- `bill`：`bill_id`、重复模式、`repeat_count_before/after`、存储模式；
- `workbench`：绑定工作台；
- `ingredients_before`、`ingredients_consumed`（实际数量与 ThingID，结算后 `destroyed` 读回）；
- `products`：def、本次数量、`thing_id`、最终 `stack_count`、`quality`/`quality_value`、`style`、位置、`stored`；
- `uft_after`：UFT `destroyed`/`spawned`、`bound_bill_cleared`；
- `bill_count_before/after`（原生 `WorkerCounter.CountProducts`）、`applied`/`native_invoked`/`changed`/`outcome`。

`bill_count` 在产物入库后才会计入资源计数；`DropOnFloor` 时计数可能不变，这是原版 `ResourceCounter` 只统计存储区的语义。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`/`pawn_id` | 400 `bad_request` |
| 地图、UFT、工人不存在 | 404 `not_found` |
| 目标不是 UFT、UFT 无绑定账单/配方、账单不属于工作台、材料为空或已销毁、工作台无电无燃料/损坏/着火、账单完成/挂起/暂停、工人不是创建者或资格不足 | 422 `invalid_target` |
| UFT 已被某个工人的当前 Job 占用 | 409 `conflict` |
| 配方不 `UsesUnfinishedThing`、屠宰等非本动作配方 | 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在修改前完成；失败不销毁 UFT、不扣材料、不记账单。原生异常时返回 `outcome: uncertain`、`applied: null` 和 `after` 读回（UFT/账单/材料状态），不要未读回就重试。

## 边界与幂等

- 只完成已经存在的 UFT；不创建 UFT、不修改 `MakeUnfinishedThingIfNeeded` 的绑料流程，也不放宽 `work.bill_once` 的 `UsesUnfinishedThing` 拒绝。
- 不调用 `Destroy(DestroyMode.Cancel)`，不会返还 75% 材料；材料只被 `ConsumeIngredient` 销毁一次。
- 不计算工作 XP/工作时间，不创建搬运 Job；产物按账单存储模式直接落地/入库。正常 `WorkGiver_DoBill` 派工保持原样。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不重复结算；同键改请求为 409。完成后再用同一 UFT ThingID 请求返回 404（对象已销毁）。
- 只支持原版 `Bill_ProductionWithUft`；`Bill_Medical`/`Bill_Mech`/调试生成的其他账单不在本动作范围。
