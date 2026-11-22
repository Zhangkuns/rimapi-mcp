# 酿酒桶装料/发酵/取酒

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.ferment`，对现有 `Building_FermentingBarrel` 结算一个阶段。`mode` 三选一：`add` 加麦汁、`finish` 设置 DEV 进度 1、`take` 取酒。不创建 Job、不消耗工作时间，游戏暂停也能执行；请求前后 `TicksGame` 不变。

```json
{"action":"work.ferment","map_id":0,"thing_id":"27979","mode":"add","pawn_id":"27986","ingredients":["Wort27992"]}
{"action":"work.ferment","map_id":0,"thing_id":"27982","mode":"finish","pawn_id":"27986"}
{"action":"work.ferment","map_id":0,"thing_id":"27983","mode":"take","pawn_id":"27986"}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 已生成的 `Building_FermentingBarrel`。 |
| `mode` | 是 | `add` / `finish` / `take`（大小写不敏感）。 |
| `pawn_id` | 是 | 同图、存活、未倒地、玩家阵营的殖民者；`take` 用于啤酒落点。 |
| `ingredients` | add | 恰好一个 `ThingDefOf.Wort` 的 ThingID；finish/take 忽略。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 三阶段语义

- **add**：调用公开 `Building_FermentingBarrel.AddWort(Thing)`。原生实现取 `min(wort.stackCount, 25 - wortCount)`，`SplitOff(num).Destroy()`，用加权平均更新 `wortCount`/`Progress`，并重置 `CompTemperatureRuinable`。桶已发酵、已满、麦汁不是 Wort 时拒绝，不半扣料。
- **finish**：对应 DEV 菜单 “Set progress to 1”，把公开 `Progress` 设为 `1f`。桶必须非空且未发酵；设置后 `Fermented=true`，**不会创建啤酒**，也不会消耗/产生物品。已发酵桶用新 key 返回 422（同 key 重放仍返回原响应）。
- **take**：仅在 `Fermented=true` 时调用公开 `TakeOutBeer()`；它返回 `stackCount=wortCount` 的 Beer 并重置桶，动作再用 `GenPlace.TryPlaceThing` 放到工人位置附近（与原版取酒 Job 一致）。未发酵或空桶拒绝。

`wort_count` 没有公开 getter。未发酵时由公开 `SpaceLeftForWort` 推导为 `25-space_left`；`finish` 响应在设置进度前读取该值并标注 `unchanged_by_finish`；`take` 的精确数量来自 `TakeOutBeer()` 返回堆。私有 `portionYieldPct`/`ruinedPercent` 之类字段不读取、不修改，`ruinable` 只读公开 `Ruined` 与公开 inspect 文本。

## 响应

- `barrel` / `barrel_after`：ThingID/def/坐标/旋转、`space_left`、`wort_count`（及来源）、`progress`、`fermented`、`empty`、`ambient_temperature`；
- `input`（add）：麦汁 ThingID、`stack_before`、`stack_after`、`destroyed`、`would_add`；
- `added_wort`（add）；
- `output`（take）：Beer 的 `added`、thingID、`placed`、`count_after`、位置；
- `ruinable_before/after`：`ruined`、公开 inspect 文本、安全温度范围；
- `dev_finish`（finish 明确标注 DEV 入口）、`native_methods`、`applied`/`native_invoked`/`changed`/`outcome`。

`dry_run` 返回 `would_add`/`would_take_wort`、`would_finish`/`would_beer_count` 或 `would_take`；`take` 的干跑不调用 `TakeOutBeer`，因此 `would_beer_count` 为 null 并附 `beer_count_unknown` 说明。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`/`pawn_id`/`mode`、模式非法、add 缺/多麦汁 | 400 `bad_request` |
| 地图、桶、工人不存在 | 404 `not_found` |
| 目标不是发酵桶、add 已发酵/已满/物品不是 Wort、finish 空桶/已发酵、take 未发酵/空桶 | 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在修改前完成；失败不扣麦汁、不推进进度、不产酒。

## 边界与幂等

- 只操作一个已有桶的一个阶段；不刷麦汁/啤酒、不直接改私有字段、不跳过 `AddWort`/`TakeOutBeer` 原生路径。
- `finish` 是 DEV 进度入口，不模拟真实 6 天发酵耗时；不授予烹饪 XP、不播放 Job 动画。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再扣麦汁/不再取第二份；同键改请求（含换 mode/桶）为 409。阶段错误或已取空用新 key 返回 422。
