# 完成机械孵化

开启游戏的开发者模式且启用 Biotech 后，调用 `POST /api/v1/debug/native/execute` 的 `work.mech_gestation_finish`，完成机械孵化器中当前活动的机械体账单。游戏暂停也能执行；请求前后 `TicksGame` 不变。只完成已存在的账单，不创建新账单或机械体。

```json
{"action":"work.mech_gestation_finish","map_id":0,"thing_id":"27982","dry_run":true}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | spawned `Building_MechGestator`。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 前置与校验

- Biotech 启用。
- 目标是 spawned、未销毁的 `Building_MechGestator`。
- `ActiveMechBill` 存在，State 为 `Preparing` 或 `Forming`（`Gathering` 未开始、`Formed` 已完成均拒绝）。
- 账单未 suspended；孵化器 `PoweredOn`；账单 `BoundPawn` 存在、spawned、未死亡/未 suspended、玩家阵营。
- recipe 有 `gestationCycles > 0`。

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id` | 400 `bad_request` |
| 地图/孵化器不存在 | 404 `not_found` |
| 无活动账单、Gathering/Formed、未供电、无绑定 pawn、绑定 pawn 不可用 | 422 `invalid_target` |
| Biotech 未启用 | 501 `not_supported` |
| 开发者模式关闭 | 403 `forbidden` |

## 原生语义

1. `Bill_Mech.ForceCompleteAllCycles()`：`gestationCycles = recipe.gestationCycles`、`formingTicks = 0`（即游戏内 DEV “Complete all cycles”）。
2. 若账单处于 `Preparing`，调用公开 `Notify_BillWorkFinished(BoundPawn)` 让原版状态机进入 `Forming` 并设置 `formingTicks = recipe.formingTicks`；`Forming` 则直接进入下一步。
3. `Bill_Mech.BillTick()`：递减 `formingTicks`，`gestationCycles` 达到 recipe 值时设置 `Formed` 并调用 `Gestator.Notify_FormingCompleted()`。
4. `Notify_FormingCompleted()` 通过 `Bill_ProductionMech.CreateProducts()` 生成真实机械体，清空容器，把机械体加入 `innerContainer`，再按 `WastepacksPerRecharge` 生成废料包，播放完成音效并显示消息。

本动作不直接 `MakeThing`、不刷机械体、不跳过配方/材料、不批量。DEV 的 `ForceCompleteAllCycles` 本身只改计数/时间，不会完成最终状态；本动作补的是原版公开状态迁移与 `BillTick`，没有使用反射。

## 响应

- `gestator`/`gestator_after`：ThingID/def/位置、spawned/destroyed、`powered_on`、`gestating_mech`；
- `bill_before`/`bill_after`：recipe、`state`、`gestation_cycles_completed`、`gestation_cycles_recipe`、`forming_ticks`、suspended、bound pawn、`can_finish_now`；
- `contents_before`/`contents_after`：容器内物品/机械体（def/ThingID/数量/容器/位置）；
- `product`：真实机械体 def/kind/ThingID、容器、spawned/dead/destroyed、位置；
- `waste`：废料包 def/ThingID/数量/容器；
- `formed`、`applied`/`native_invoked`/`changed`/`outcome`（`formed`）。

`dry_run` 返回 `would_complete`、`would_force_cycles`，不修改账单/容器/机械体/废料。

## 边界与幂等

- 只完成一只已存在的机械体账单；不创建账单、不批量、不替代正常 gestator Job。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再完成第二只；同键改请求（换孵化器）为 409。完成后同账单/新 key 因 State=Formed 返回 422。
