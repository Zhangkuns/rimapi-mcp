# 正常胜利读回与结束字幕

维护：Codex；核对：2026-11-13；状态：Core/Royal Ascent 继续与 Archonexus 返回菜单已实测，实际 rev600。

`GET /api/v1/game/ending` 可在游戏及主菜单调用，返回 `game_active`、
`program_state`、`ship_countdown`、`archonexus_countdown`、`colonists_launched`、
`victory_credits`、`can_finish` 和 `destination`。`destination` 只在原生胜利字幕
存在时为 `playing` 或 `main_menu`；主菜单的普通制作名单不计胜利。

`POST /api/v1/game/ending/finish`：

```json
{"confirm":true,"dry_run":false}
```

`dry_run:true` 仅检查；实际操作要求 `confirm:true`，否则400。没有待处理的原生
胜利字幕返回409。执行复用 `Screen_Credits.Close()`，其 `PostClose` 决定原生去向，
并恢复结束动画的透明淡入和正常时间请求。返回 `applied/dry_run/after`；
菜单切换是异步场景变化，以后续 GET 的 `program_state/game_active` 确认。
淡入期间原生可能暂缓时间速度变更；调用方可在动画结束后正常设置 `game/speed`。

同一游戏内支持 `Idempotency-Key` 重放；同键同请求不会再次执行。
每次独立胜利使用新键，不能用前次胜利的键请求处理新结局。
Archonexus 的原生去向是主菜单，不提供原世界继续选择；失败后的新流浪者走
[失败继续合同](core-game-over.md)。

实际证据和测试前置见[结局与穿梭机验收](../references/reviews/2026-11-13-api-playability-endings-transport.md)。
测试中的巨核供应、皇家任务资格及缩短招待等待均作为披露前置；API 不创建这些资格，
不直接写胜利状态或强制改变原生结局。
