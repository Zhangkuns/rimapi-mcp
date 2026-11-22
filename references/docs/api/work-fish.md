# 一次钓鱼

开启游戏的开发者模式且启用 Odyssey 后，调用 `POST /api/v1/debug/native/execute` 的 `work.fish`，在指定钓鱼格完成一次原版钓鱼结算。不创建或推进正常钓鱼 Job，游戏暂停也能执行；请求前后 `TicksGame` 不变。结果随机：不保证鱼种、数量或稀有结果。

```json
{"action":"work.fish","map_id":0,"x":67,"z":20,"pawn_id":"27979","dry_run":true}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `x`、`z` | 是 | 钓鱼格；必须在 `Zone_Fishing` 内且为可钓鱼水域。 |
| `pawn_id` | 是 | 同图、存活、未倒地、玩家阵营的钓鱼者。 |
| `dry_run` | 否 | 只校验并返回计划；不掷随机数、不改状态。 |

## 原生语义

复刻 `JobDriver_Fish.CompleteFishingToil`，顺序与原生一致：

1. `FishingUtility.GetNegativeFishingOutcomes(pawn, cell)`：按 2% 概率（受冷却/`DebugSettings.alwaysNegativeCatches` 影响）得到负面结果列表。
2. `FishingUtility.GetCatchesFor(pawn, cell, animalFishing:false, out rare)`：先掷稀有（1%），否则从水体的 common/uncommon fish 按人口/`FishingYield` 计算渔获。
3. 若有负面结果：随机一个，写 `waterBodyTracker.lastNegativeCatchTick`；按 def 施加伤害（`DamageInfo`，Outside）、hediff（含 severity）、信件；**不**放置鱼、不调用 `Notify_Fished`。
4. 否则若有渔获：逐个 `GenPlace.TryPlaceThing(..., pawn.Position, map, Near)`；任一成功时：
   - 稀有：写 `lastRareCatchTick`，发稀有信件；不调用水质 `Notify_Fished`/历史；
   - 普通：`waterBodyTracker.Notify_Fished(cell, total)`，记录 `SlaughteredFish` 历史。
5. 最后 `Zone_Fishing.Notify_Fished()`。

不刷鱼、不指定鱼种/稀有结果、不替代区域策略或正常 fishing Job。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `x`/`z`/`pawn_id` | 400 `bad_request` |
| 坐标越界、地图/钓鱼者不存在 | 400 / 404 |
| 格子不在 `Zone_Fishing`、区域未允许、非可钓鱼水域、无水体、钓鱼者非玩家阵营/死亡/倒地 | 422 `invalid_target` |
| Odyssey 未启用 | 501 `not_supported` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在随机/伤害/放置前完成；失败不改健康、水体、鱼或区域。

## 响应

- `cell`、`zone`（allowed/cells/fish_type）、`water_body`（type/cell_count/population_before/max_population）；
- `pawn`（ThingID、fishing_yield/speed）；
- `negative_outcomes`/`negative_outcome_count`、`rare_roll`；
- 负面：`negative_result`（def/letter/fish_type/damage/hediff）、`last_negative_catch_tick_after`、`pawn_health_after`；
- 渔获：`catches`（def/ThingID/数量/位置/placed）、`catch_total`、`any_placed`、`history_recorded`/`rare_letter`、`outcome`（`caught`/`rare_catch`/`negative`/`no_catch`/`catch_not_placed`）；
- `water_population_after`、`last_rare_catch_tick_after`、`fish_count_before/after`、`applied`/`native_invoked`/`changed`。

`dry_run` 返回 `would_fish`、`would_consume_random=false` 和现场读回，不修改。

## 边界与幂等

- 只结算一次已有钓鱼格；不批量、不替代区域策略/正常 Job、不保证结果。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再掷随机、不再伤害/放置鱼；同键改请求（换格/工人）为 409。
- 每次新 key 都是一次新的真实钓鱼尝试，可能再次获得渔获或负面结果；这不属于批量。
