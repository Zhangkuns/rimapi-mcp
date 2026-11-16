# Core 灭团后原生继续与新流浪者（C-08）

2026-11-13 新增。只使用原生 `GameEnder`、`ChoiceLetter_GameEnded`、
`Dialog_ChooseNewWanderers` 与 `IncidentDefOf.GameEndedWanderersJoin`；不复活
旧 pawn、不直接 `GenSpawn`、不手工造人物。

## 状态（只读）

`GET /api/v1/game/game-over`

- `game_ending`、`new_wanderers_created_tick`、`can_spawn_new_wanderers`。
- `letter`：GameEnded 信件是否存在、`arrival_tick`；`hours_remaining =
  (20000 - (TicksGame - arrivalTick))/2500`。
- `home_map`/`home_map_root_surface`。
- `options.keep_watching`、`options.create_new_wanderers`（`enabled` +
  原生状态推导的 `disabled_reason`）、`options.main_menu`。
- `prepared`/`candidates[]`：仅在已 prepare 的原生 dialog 状态时读回候选
  （thing_id/name/gender/age/title/traits/skills）。
- 不打开 dialog、不生成人物。

## 准备候选

`POST /api/v1/game/game-over/wanderers/prepare`（空 body `{}`）

- 仅当 `gameEnding`、GameEnded 信件存在、`CanSpawnNewWanderers()`、有根表面
  home map、`hours_remaining<=0`、无已打开 dialog 时允许；否则 409 + 原生
  原因（等待小时/仍有存活/无殖民地/无目的地/已打开）。
- 通过后 `Find.WindowStack.Add(new Dialog_ChooseNewWanderers())`，由原生
  `PreOpen` 生成 3 名 PlayerStarter 候选到
  `Find.GameInitData.startingAndOptionalPawns`；返回候选读回。
  **不生成殖民地人物、不发 incident**；重复 prepare 409。

## 确认

`POST /api/v1/game/game-over/wanderers`

```json
{"pawn_ids":["Human75822"],"confirm":true}
```

- 只接受当前 prepared 候选子集（1..6、去重、全属候选），否则 422；
  `confirm=false` 只返回 `selected`，不写。
- 确认时执行原生 Dialog Confirm 语义：先把选中候选写回
  `GameInitData.startingAndOptionalPawns`/`startingPawnCount`，再
  `Find.Storyteller.TryFire(FiringIncident(GameEndedWanderersJoin,
  target=Find.AnyPlayerHomeMap, forced=true))`；成功后才移除 GameEnded
  letter、`gameEnding=false`、`newWanderersCreatedTick=TicksGame`、关闭
  dialog、`Current.Game.InitData=null`。incident 失败不清理状态。
- 返回 `applied`、`selected`、`spawned`（新殖民者 ThingID/姓名）、
  `game_ending=false`、`letter_present=false`、new tick、`after` 状态。
  重复 confirm 409。

## 证据与边界

2026-11-13 隔离 g35 副本：关闭 debug 无敌后 fixture 用原生 `Pawn.Kill(null)`
击杀全部玩家 humanlike（披露）；有界推进到 GameEnded letter（tick 77188，
arrival 77187），再推进 20000 tick。

- 状态 0：`game_ending=true`、letter 存在、`can_spawn=true`、
  `hours_remaining=7.9996`、create disabled "about 8 hours"；prepare 409。
- 20000 tick 后 `hours_remaining=-0.002`、create enabled；prepare 生成 3 候选
  （Human75822/75825/75829），殖民者为 0；重复 prepare 409。
- 非法/重复/空候选 422；`confirm=false` 不写。
- 确认 Human75822：incident 成功、letter 移除、`game_ending=false`、
  new tick 97192、spawned=Human75822；fixture 自由殖民者仅该员、dialog 关闭；
  `game/state` Playing、map_count=1；再次 confirm/prepare 409。
- `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。
- 未测：keep-watching（原生 no-op）、main-menu 选项、death-refusal 阻塞路径、
  多次连续 game-over 循环。fixture 只做原生击杀与有界推进，无传送/复活。
