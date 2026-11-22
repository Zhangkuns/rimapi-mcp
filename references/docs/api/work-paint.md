# 涂色/去色

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.paint`，结算**已存在**的 `PaintBuilding` / `PaintFloor` / `RemovePaintBuilding` / `RemovePaintFloor` 标记。它复用原版四个 `JobDriver` 的完成段，不创建 Job、不消耗工作时间。游戏暂停时也能执行，请求前后 `TicksGame` 不变。

```json
{"action":"work.paint","map_id":0,"thing_id":"27980","def_name":"Structure_Red","pawn_id":"27981","ingredients":["Dye27986"]}
{"action":"work.paint","map_id":0,"x":78,"z":5,"pawn_id":"27981"}
```

建筑物用 `thing_id`，地面用 `x`+`z`，二者只能给一种。`def_name` 是 `ColorDef` 时涂色；省略（或空串）时去色。

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 建筑 | 已生成的建筑（`Building`）。 |
| `x` / `z` | 地面 | 地面格坐标，二者都给。 |
| `def_name` | 涂色 | `ColorDef`，必须与标记的 `colorDef` 一致；去色时省略。 |
| `pawn_id` | 是 | 玩家殖民者；用于资格校验和去色返还 Dye 的落点。 |
| `ingredients` | 涂色 | 恰好一个 `ThingDefOf.Dye` ThingID；去色时忽略，不消耗。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 四种结算

- **建筑涂色**（`PaintBuilding`）：消耗 `ingredients[]` 中的 1 个 Dye（`SplitOff(1).Destroy()`）→ `building.ChangePaint(marker.colorDef)`（非 Building 目标走 `SetColor`，要求 `CompColorable`）→ 删除标记。
- **地面涂色**（`PaintFloor`）：消耗 1 Dye → `map.terrainGrid.SetTerrainColor(cell, marker.colorDef)` → 删除标记。
- **建筑去色**（`RemovePaintBuilding`）：`building.ChangePaint(null)` → 删除标记 → 在工人位置 `GenPlace.TryPlaceThing(MakeThing(Dye), Near)` 返还 1 Dye。
- **地面去色**（`RemovePaintFloor`）：`SetTerrainColor(cell, null)` → 删除标记 → 同一方式返还 1 Dye。

去色不消耗 `ingredients[]`；即使请求里带了 Dye 也会忽略（响应 `dye_ignored`）。

## 校验

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺目标、`thing_id` 与 `x/z` 同时给、地面缺坐标、缺工人、颜色名不是 ColorDef | 400 `bad_request` |
| 地图、目标、工人不存在 | 404 `not_found` |
| 标记不存在、颜色与标记不符、目标不是可涂色建筑/地形、已同色、拆除/燃烧/移除地板冲突、染料不是 Dye | 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

同色会被拒绝（原版 `WorkGiver` 也不会派工）；建筑要求 `def.building.paintable`，地面要求 `terrain.isPaintable`，并拒绝拆除标记、燃烧建筑和 `RemoveFloor` 标记。所有校验在修改前完成，失败不扣染料、不改颜色、不返还。

## 响应

- `mode`（paint/remove）、`target_type`（building/floor）；
- 建筑：`building_before`/`building_after`（ThingID、def、Stuff、坐标、旋转、`paint_color_def`）；
- 地面：`cell`、`terrain`、`color_before`/`color_after`；
- `designations_before`/`designations_after`：四类标记状态；
- 涂色：`dye_before`（ThingID/stack before）、`dye_consumed`（被销毁的 1 个 Dye thingID、source stack after）；
- 去色：`refund`（`placed`、thingID、位置、当前 `count`、本次 `added=1`）；返还可能与附近的 Dye 堆合并，所以 `count` 可能大于 1，`added` 才是本次返还量；
- `applied`/`native_invoked`/`changed`/`outcome`。

## 边界与幂等

- 只结算已有标记；不批量整图、不绕过标记/染料直接改颜色、不去色不返还。
- 不计算工作 XP/工作时间，不播放涂色音效/进度条；原版 `ChangePaint`/`SetTerrainColor` 的刷新副作用保留。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再扣染料/不再返还；同键改请求（含建筑/地面互换、换颜色）为 409。标记消费后用新 key 返回 422。
