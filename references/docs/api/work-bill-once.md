# 工作台账单轮即时结算

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.bill_once`，对指定工作台上**已有**的生产账单结算一轮。它复用原版完成链的选料、产物、耗料、账单记数和记事件，不创建 Job、不消耗工作时间。游戏暂停时也能执行，请求前后 `TicksGame` 不变。

先用 `GET /api/v1/buildings/bills?building_id=<id>` 查工作台账单（`load_id` 即 `bill_id`），或用 `bill_index` 指定工作台账单堆里的零基序号。`thing_id` 是工作台建筑；工人用该图上的 `pawn_id`。

```json
{"action":"work.bill_once","map_id":0,"thing_id":"27981","bill_id":"1","pawn_id":"27989","dry_run":true}
```

可选 `ingredients[]` 固定本次使用的实物原料 ThingID；省略时按原版 `WorkGiver_DoBill` 的过滤器、账单材料过滤、搜索半径、禁用状态和混料规则，在该图可搬运物中选取。`recipe_def_name` 只做一致性校验。

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 工作台 ThingID 或数字 ID，必须实现 `IBillGiver`。 |
| `bill_id` | 二选一 | 账单 `load_id`（字符串形式的整数）。 |
| `bill_index` | 二选一 | 工作台账单堆零基序号。两者同时给出时必须指向同一账单。 |
| `pawn_id` | 是 | 该图上的工人数字 ID；必须是存活、未倒地、玩家阵营且满足账单与配方资格。 |
| `ingredients[]` | 否 | 指定实物原料 ThingID；每项必须满足账单过滤，数量按原版规则从此集合取足。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 结算语义

预演返回 `would_settle`、`ingredients_before`、`products_preview`（普通产物给出确定性数量；`StoneBlocks`、`Smelted`、`Butchery` 等特殊产物只给类型）和当前账单计数，不生成产物、不扣料、不改变账单。

真实执行按 `Verse.AI.Toils_Recipe.FinishRecipeAndStartStoringProduct` 的顺序：

1. 按原版 `Bill`/`WorkGiver_DoBill` 规则选料，从较大堆拆出所需数量；
2. `GenRecipe.MakeRecipeProducts` 生成产物，含品质、风格、效率、材料颜色和特殊产物；
3. 逐件 `recipe.Worker.ConsumeIngredient`；
4. `bill.Notify_IterationCompleted`；
5. `RecordsUtility.Notify_BillDone`；
6. 产物按账单存储模式落地。

响应里的 `ingredients_before[]` 是本次选中的原料（`def_name`、`count`、`thing_id`、原 `stack_count`、位置），`ingredients_consumed[]` 是实际交给 `ConsumeIngredient` 的数量与 ThingID（较大堆拆分时会给出新 ThingID，`destroyed` 为结算后读回），`ingredients_source_after[]`/`remaining[]` 是原料源堆的剩余读回。`products[]` 给出最终落点：`def_name`、本次 `count`、`thing_id`、`stack_count`、`quality`/`quality_value`、`style`、位置和 `stored`。

`bill_count_before/after` 读取账单原生 `recipe.WorkerCounter.CountProducts`：目标计数账单是地图上被该账单统计的产物数（产物入库后通常 +1），重复计数账单是同一原生计数在结算前后的读回。`bill.repeat_count_before/after` 是 `Bill_Production` 的重复次数。每次结算后刷新 `Map.resourceCounter` 以保证计数读回真实。

## 产物落点与搬运边界

- `DropOnFloor`：产物落在工作台交互格附近（可能并入已有堆），不产生搬运 Job。
- `BestStockpile` / `SpecificStockpile`：用原生 `StoreUtility.TryFindBestBetterStoreCellFor[In]` 找格后直接放入存储区或货架，不产生搬运 Job。
- 原版完成链会先让工人把第一堆产物搬到存储格并另起 Haul Job；本动作把这一步改为直接落地/入库，因此**不会**替玩家创建搬运任务。其它产物堆仍按同一目标格就近落地。
- 结算后不会自动分配新的账单工作；正常 `WorkGiver_DoBill` 派工保持原样。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`、`pawn_id`、`bill_id`/`bill_index`，或 `recipe_def_name` 不匹配 | 400 `bad_request` |
| 地图、工作台、账单、工人不存在 | 404 `not_found` |
| 目标不是账单工作台、账单属于其它工作台、账单/配方资格不足、工人倒地或非玩家、工作台无电无燃料/损坏/着火、账单暂停/挂起/计数已完成、原料不足或不符合过滤 | 422 `invalid_target` |
| 账单已被某个工人的当前 Job 占用 | 409 `conflict` |
| 使用未完成品（`UsesUnfinishedThing`）的配方，或 `Bill_Mech`/`Bill_Autonomous` | 501 `not_supported` / 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

失败在修改前完成校验；原料不足、指定原料过滤不符或账单已完成时会整体拒绝，不出现“扣料但无产物”。原生异常时返回 `outcome: uncertain`、`applied: null` 和 `after` 读回（账单计数、原料源堆、工作台状态），不要未读回就重试。

## 边界与幂等

- 只结算一轮，不移植依赖当前 Job 的 Toil；不计算工作时间、技能经验或工作履历，也不创建搬运 Job。
- 不实现 `WORK-D03A` 屠宰：带 `SpecialProductType.Butchery` 的账单被拒绝。
- 不实现 `WORK-D03B` 未完成品/半成品：`UsesUnfinishedThing` 账单被拒绝。原版 Core 的武器、服装、艺术品等带品质的手工配方都使用未完成品，因此单轮 `work.bill_once` 无法产出带品质的手工制品；品质机制仍由 `GenRecipe` 原生路径处理，但需要 D03B 才能覆盖。
- 传 `Idempotency-Key` 可在本游戏实例内重放同一响应（`Idempotency-Replayed: true`）；同键改请求为 409。重放不重复结算。已完成或已挂起的账单再次请求会被资格检查拒绝。
