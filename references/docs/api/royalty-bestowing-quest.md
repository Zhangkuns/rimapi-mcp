# Royalty 授勋任务生成/接受与仪式（G-56–G-82）

交付：读取授勋（bestowing）任务资格/当前任务，生成并接受授勋任务，在 bestower 已到达
等待点时启动原生授勋仪式（G56/G81/G82）。begin-ritual 对话框仍未走；任务接受与 shuttle
到达已由 G82 的正常流程验证。
证据：G56 任务生成（`Reverse/evidence/dlc-G56-royalty-bestowing-quest.json`）；
G69 原生 outcome（`dlc-G69-royalty-bestowing-ceremony.json`）；
G81 仪式开始（`dlc-G81-bestowing-ceremony-start.json`）；
G82 任务接受/shuttle/仪式（`dlc-G82-bestowing-quest-accept.json`）。

## 读取

`GET /api/v1/dlc/royalty/bestowing/status?pawn_id=321`

```json
{"pawn_id":"Human318","eligible":true,"faction_id":"Faction_7","faction_def":"Empire",
 "title_def":"Knight","favor":10,"next_title_def":"Praetor","next_favor_cost":10,
 "current_quest_id":null,"current_quest_name":null,"can_update":true}
```

- `eligible` 是原生 `RoyalTitleUtility.ShouldGetBestowingCeremonyQuest`：玩家、有
  royalty、favor ≥ 下一头衔 `favorCost`、且当前没有该 pawn/faction 的授勋任务。
- `current_quest_*` 来自 `GetCurrentBestowingCeremonyQuest`。
- 当前任务存在时另报 `quest_bestower_id`/`quest_target_id`/`quest_shuttle_id`
  （`QuestPart_BestowingCeremony`）、`bestower_spawned`、`ceremony_spot_valid`/`ceremony_spot`
  （原生 `TryGetCeremonySpot`）以及 `throne_room_ok`（bestower 已生成时
  `JobDriver_BestowingCeremony.AnalyzeThroneRoom`，否则 `null`）。

## 生成

`POST /api/v1/dlc/royalty/bestowing/quest` `{pawn_id, dry_run}`

- 资格不满足 409（含已有任务/无下一头衔/favor 不足）。
- 正式调用 `RoyalTitleUtility.GenerateBestowingCeremonyQuest(pawn, faction)`，再按
  quest id 差集找 `QuestScriptDefOf.BestowingCeremony` 新任务；返回 `quest_id/quest_name`，
  `applied` 表示确实新建了任务。
- dry-run 不创建任务。

## 实测

`ai_g31_fixture` 副本（tick 74742）：fixture 选无头衔殖民者，API 授 `Knight`，
fixture 设 favor 10（Knight→Praetor 需 10；测试准备）；清除游戏自动生成的授勋任务后：

- status：eligible true、next Praetor/cost 10、无当前任务。
- dry-run 200/applied false；正式 200/applied true、quest_id 非空。
- observe：current_quest_id 与新任务一致、bestowing_quests 列出该任务。

## 接受（G-82）

`POST /api/v1/dlc/royalty/bestowing/quest/accept` `{pawn_id, confirm, dry_run}`

- 要求当前授勋任务 `State == NotYetAccepted`；原生 `QuestUtility.CanAcceptQuest`
  检查王座室、目标在殖民地地图、无危险、无进行中的授勋仪式；
  `CanPawnAcceptQuest` 要求接受者是自由殖民者。
- 正式调用原生 `Quest.Accept(pawn)`：`InitiateSignal` 触发
  `QuestPart_MakeLord` 创建 `LordJob_BestowingCeremony`，shuttle job 开始；返回
  `state_before/state_after`、`lord_id`、`toil_after`。`applied` 只表示任务已接受，
  不表示 bestower 已到达或仪式已开始。
- dry-run 不改任务；非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G82 实测：任务 2 pending、bestower 未生成；dry-run 200/applied false/`would_accept` true；
正式 200/applied true、`state_after=Ongoing`、`lord_id=3`；推进后 shuttle 到达、
bestower 生成并进入 `LordToil_BestowingCeremony_Wait`（tick 81102），随后仪式开始并完成。

## 仪式开始（G-81）

`POST /api/v1/dlc/royalty/bestowing/ceremony/start`

```json
{"pawn_id":"Human318","participant_ids":["Human321"],"confirm":true,"dry_run":true}
```

- 前置：当前授勋任务存在；`QuestPart_BestowingCeremony.bestower` 已生成且与目标同图；
  bestower 已在 `LordJob_BestowingCeremony` 的 `LordToil_BestowingCeremony_Wait`；
  `JobDriver_BestowingCeremony.AnalyzeThroneRoom` 通过。
- 正式：把 `participant_ids` 中的自由殖民者与 target（若尚未在 lord）加入 lord 与
  `LordJob_BestowingCeremony.colonistParticipants`，再发送原生
  `MemoCeremonyStarted`；target 由原生 `DutyDefOf.Bestow` 获得
  `JobDriver_BestowingCeremony`，参与者和 target 会被解除征召/唤醒。之后原生
  `JobDriver_BestowingCeremony`/`RitualOutcomeEffectWorker_Bestowing` 完成 outcome。
  `applied` 只表示仪式已开始，不表示已授衔；返回的 `participant_ids` 是实际加入 lord 的
  pawn（含自动加入的 target）。
- 参与者须 spawned、非倒地/精神、玩家自由殖民者、同图、无 lord；bestower/target 不可作为
  参与者。非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。
- 未创建 lord：若 bestower 尚未被任务/shuttle 流程放入 lord 或尚未到 Wait，返回 409。

G81 实测：API 生成/自动获得授勋任务，fixture 建王座室并生成任务 bestower 到 Wait；
dry-run 200/applied false；正式 200/applied true、`toil_after` Perform、participant_count 1；
推进后原生 outcome 授 `Acolyte`、psylink 2、API title/psycasts 读回一致，任务完成
（`current_quest_id` 清空）。见
[G81 自审](../references/reviews/2026-11-08-g81-bestowing-ceremony-start.md)。

G82 实测：`quest/accept` 后由原生 shuttle 把 bestower 送到 Wait；参与者先征召，
`ceremony/start` 后自动解除征召并加入 lord（`SpectateCeremony`），target 自动加入并执行
`BestowingCeremony` job；仪式完成后 Acolyte/psylink 2、status 读回 title Acolyte。见
[G82 自审](../references/reviews/2026-11-08-g82-bestowing-quest-accept.md)。

## 取消（G-84）

`POST /api/v1/dlc/royalty/bestowing/ceremony/cancel` `{pawn_id, confirm, dry_run}`

- 前置：当前授勋任务、bestower 在 `LordJob_BestowingCeremony`、toil 为 Wait/Perform，
  且 `ceremonyStarted=true`；否则 409。
- 正式发送原生 `CeremonyFailed` 信号（与取消按钮同一信号）：任务以 `EndedFailed` 结束，
  `QuestPart_BestowingCeremony.Cleanup` 再发 `QuestEnded`，lord 进入
  `LordToil_EnterShuttleOrLeave`，参与者/ target 被移出 lord；头衔不变。
- 返回 `quest_state_before/after`、`toil_before/after`、`title_after`、`target_in_lord`；
  dry-run 只投影。非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G84 实测：接受任务→shuttle 到达→start（Perform）后，dry-run 200/applied false；正式
`quest_state_after=EndedFailed`、`toil_after=LordToil_EnterShuttleOrLeave`、title 仍
Yeoman、target 已移出 lord；status 的 `current_quest_id` 清空且 title 读回 Yeoman。见
[G84 自审](../references/reviews/2026-11-08-g84-bestowing-cancel.md)。

## 仪式原生 outcome（G69 限定）

G69 时端点只到任务生成；G81 已补仪式开始（见上节），但 begin-ritual 对话框仍未走。
G69 用 test-only fixture 直接
创建原生 `LordJob_BestowingCeremony`，并在 bestower 到达 spot 后发送原生
`MemoCeremonyStarted`，验证了原生 `RitualOutcomeEffectWorker_Bestowing`：

- 目标 `Yeoman`+favor 6 → 仪式完成后 `Acolyte`，favor 6→0（`UpdateRoyalTitle` 扣除
  Acolyte 的 6 点；本样本 quality 未达额外 honor 档）；
- `GetPsylinkLevel` 0→2（Acolyte `maxPsylinkLevel=2`）；
- `GainedTitle` awardThought 出现（直接 `SetTitle` 不添加，见 G54）；
- bestower 的 `PsychicAmplifier` 1→0（outcome 消耗）；
- `RoomRequirement` 逐项 met（35/24 area、all floored、2/2 brazier、all lit、无
  production/beds）；API `GET /dlc/royalty/title`/`psycasts` 读回 Acolyte/level 2。

证据：`Reverse/evidence/dlc-G69-royalty-bestowing-ceremony.json`，自审
[G69](../references/reviews/2026-11-08-g69-royalty-bestowing-ceremony.md)。

## 限制

- G69/G81/G82 都未走 begin-ritual 对话框/`RitualRoleAssignments`；参与者是显式
  `participant_ids` 列表，不是角色分配界面，未验观众角色/参与人数对 quality 的影响。
- G82 走通任务接受→shuttle 到达→bestower Wait→仪式开始/完成；G84 走通已开始仪式的
  `CeremonyFailed` 取消（EndedFailed/离场/头衔不变）。只验 Yeoman→Acolyte、Empire 一派系；
  未测其它头衔/派系、取消前 Wait 阶段、`CeremonyExpired`（30000 tick）、不可达/危险中断、
  `CeremonyDone/Failed` 义务/decree。
- G81 fixture 仍直接生成任务 bestower/lord，用于隔离验证仪式开始；正常流程以 G82 为准。
- 未做 Cua 对照。

