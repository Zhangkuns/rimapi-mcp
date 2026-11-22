# 深钻出一批

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.deep_drill`，让一个现有深钻机通过公开入口 `CompDeepDrill.DrillWorkDone` 产出**一份**真实矿物。它不创建 Job、不消耗工作时间，游戏暂停也能执行；请求前后 `TicksGame` 不变。

```json
{"action":"work.deep_drill","map_id":0,"thing_id":"27979","pawn_id":"27981","dry_run":true}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 已生成的深钻机（带 `CompDeepDrill` 的 Building）。 |
| `pawn_id` | 是 | 同图、存活、未倒地、玩家阵营的钻工；提供 `DeepDrillingSpeed`/`MiningYield`。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 结算语义

本动作只调用一次公开的 `CompDeepDrill.DrillWorkDone(pawn, delta)`：

- `delta = ceil((10000 - progressPct*10000 + 1) / DeepDrillingSpeed)`，保证本次跨过原生 10000 阈值恰好一次（不是循环 10000 次）；
- 原版内部累加 `portionProgress` 与 `portionYieldPct`（`速度×delta×MiningYield/10000`），跨过阈值后运行私有 `TryProducePortion(portionYieldPct, pawn)`：从 `DeepResourceGrid` 的目标格扣除 `min(格内数量, deepCountPerPortion)`，在 `InteractionCell` 附近落地真实产物，并重置进度；
- **不使用** DEV 的 `TryProducePortion(1f)`，不使用反射，不直接改私有字段；产物数量如实反映钻工的 `MiningYield`（可能高于 `deepCountPerPortion`，例如 45 → 47）。
- `portionYieldPct` 没有公开 getter，响应中的 `calculated_yield_pct` 是按 `DeepDrillingSpeed`/`MiningYield`/delta 计算的预期值，不是读取私有字段；实际产量以 `products`/网格扣减为准。

## 校验

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`/`pawn_id` | 400 `bad_request` |
| 地图、目标、工人不存在 | 404 `not_found` |
| 目标无 `CompDeepDrill`、未 spawned、`CanDrillNow()` 为 false（断电等）、原生 21 格扫描内没有可钻的**网格**资源、工人无 `DeepDrillingSpeed` | 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

`CanDrillNow()` 为 true 但只有基岩兜底资源（`GetNextResource` 返回 `next_grid=false`）时也会拒绝：本动作只结算 `DeepResourceGrid` 中真实存在的一格矿，不把无限兜底石料当作可枯竭矿脉。所有校验在调用前完成，失败不扣网格、不产出。

## 响应

- `drill`：ThingID/def/坐标/旋转/阵营/`power_on`、`can_drill_before/after`、`valuable_before/after`、`progress_pct_before/after`；
- `pawn`：`deep_drilling_speed`、`mining_yield`、采矿等级；
- `delta`、`calculated_yield_pct`；
- `resource`：`def_name`、目标格 `cell`、`count_before`、`count_after_at_before_cell`、`deep_count_per_portion`、`portion_count`、`next_grid_resource`/`next_def_name`/`next_count`/`next_cell`、`depleted_after`；
- `products`：本次新增/合并的产物堆（def、`added`、`count_after`、thingID、位置）、`product_total`；
- `applied`/`native_invoked`/`changed`/`outcome`。

## 边界与幂等

- 只钻一格一份；不批量整片矿网、不刷产物、不用扫描/挖矿标记替代、不伪造 DEV 100% 语义。
- 不计算工作 XP、不创建 Job、不播放动画；原版 `TryProducePortion` 的历史事件、枯竭消息和兜底逻辑保留。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再扣网格、不再产出；同键改请求（换钻机/工人）为 409。目标格枯竭后用新 key 返回 422。
