# 打磨墙面/地面

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.smooth`，即时结算一个现有天然墙或可打磨地面的打磨。它复用原版 `SmoothableWallUtility` / `JobDriver_SmoothFloor` 的完成流程，不创建 Job、不消耗工作时间。游戏暂停时也能执行，请求前后 `TicksGame` 不变。

```json
{"action":"work.smooth","map_id":0,"thing_id":"Sandstone27979","pawn_id":"27981"}
{"action":"work.smooth","map_id":0,"x":78,"z":7}
```

`thing_id` 与 `x`+`z` 只能给一种；墙面用 `thing_id`，地面用 `x`+`z`。

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 墙面 | 已生成的天然墙 `Building`（例如 `Sandstone`）。 |
| `x` / `z` | 地面 | 地面格坐标，二者都给。 |
| `pawn_id` | 否 | 玩家殖民者；墙面用于新墙阵营和 `Notify_SmoothedByPawn`，缺省时新墙归玩家。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 墙面语义

1. `SmoothableWallUtility.SmoothWall(target, pawn)`：`target.Destroy(DestroyMode.WillReplace)`；用 `ThingMaker.MakeThing(def.building.smoothedThing, target.Stuff)` 在**原位置、原旋转**生成新墙；阵营取 `pawn.Faction`（缺省玩家）；移除该格的 `SmoothWall` 标记；
2. `SmoothableWallUtility.Notify_SmoothedByPawn(newWall, pawn)`：按原版拐角规则，**可能连带打磨相邻的可平滑墙**。每个被连带墙同样走 `SmoothWall`，所以响应会列出真实发生的邻墙变化。

旧墙被替换销毁（不是拆除），不会掉落材料；新墙保留原 Stuff/旋转/格位。该动作不要求预先有 `SmoothWall` 标记；有标记时随原生流程移除。

## 地面语义

按 `JobDriver_SmoothFloor` 完成时的顺序：

1. `clearSnow=true`：雪深与沙深归零；
2. `TerrainGrid.SetTerrain(cell, terrain.smoothedTerrain)`；
3. `FilthMaker.RemoveAllFilth(cell, map)` 清除该格全部污物；
4. 移除该格的 `SmoothFloor` 标记。

本加载中只有奥德赛（Odyssey）的 `VolcanicRock` 等天然地形带 `smoothedTerrain`；没有该字段的地形（如 `Soil`）会被拒绝。临时地形也拒绝。

## 响应

墙面：

- `wall_before` / `wall_after`：ThingID、def、Stuff、坐标、旋转、阵营、`is_smoothable`/`is_smoothed`、标记；
- `old_destroyed`/`old_spawned`；
- `neighbors_before` / `neighbors_after` / `neighbors_changed`：2 格半径内的墙状态，`neighbors_changed` 只列实际变化（含 Notify 连带平滑的邻墙）；
- `designation_before`/`designation_after`、`applied`/`native_invoked`/`changed`/`outcome`。

地面：

- `cell`、`floor_before`/`floor_after`：terrain、`smoothed_terrain`、`temporary`、雪/沙深度、污物列表、`SmoothFloor` 标记；
- `filth_cleared`（本次实际清除的污物数量）、`expected_terrain`（dry_run）。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺目标、`thing_id` 与 `x/z` 同时给出、地面只给一个坐标 | 400 `bad_request` |
| 地图、墙、工人不存在 | 404 `not_found` |
| 非 Building/蓝图/框架、无 `smoothedThing`/不可平滑/已平滑墙、地面无 `smoothedTerrain`/临时地形/已平滑 | 422 `invalid_target` |
| 地面坐标越界 | 400 `bad_request` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在替换/改地形前完成；失败不会拆墙、改地形、清污或连带邻墙。`dry_run` 只返回目标、预期 smoothed def/terrain 和当前状态，不评估 Notify 可能连带哪些邻墙。

## 边界与幂等

- 不直接 `SetTerrain` 处理墙面，不跳过 `Notify_SmoothedByPawn` 的邻墙副作用，不重复清污；不计算工作 XP/工作时间，不生成拆除材料。
- 原版平滑墙的替换/关联规则、地面污物/雪沙清理都保留；人走过后再产生污物属于后续自然行为。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再拆墙/改地形/连带邻墙；同键改请求（含墙/地面互换）为 409。已平滑目标用新 key 返回 422；旧墙 ThingID 已销毁返回 404。
