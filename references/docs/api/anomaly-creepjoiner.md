# Anomaly 幽魂/蠕行者响应（G-87）

触发入口复用通用事件端点：

`POST /api/v1/incident/trigger` `{"name":"CreepJoinerJoin"}`

原生 `IncidentWorker_GiveQuest` 生成隐藏任务 `CreepJoinerArrival`，立即生成一个
`Pawn_CreepJoinerTracker` 并打开邀请信。读取与资格：

`GET /api/v1/dlc/anomaly/creepjoiner?map_id=0&pawn_id=CreepJoiner88473`

两个筛选均可省略，地图默认当前图。返回 `joiners`：准确 pawn/quest/speaker、派系、
`spawned/carrier_id/is_prisoner/in_prison_bed`，以及三种 `actions` 的 `eligible/reason`。
包含搬运者抱起中的加入者。未知指定 pawn/map 404，无 Game 409，DLC 缺失 422。

响应端点：

`POST /api/v1/dlc/anomaly/creepjoiner/respond`

```json
{"pawn_id":"CreepJoiner88473","action":"accept","confirm":true,"dry_run":true}
```

- `action` 为 `accept`/`reject`/`capture`；服务从
  `Pawn.creepjoiner.quest` 的 `QuestPart_CreepJoinerOutcomes` 读取对应原生信号
  （`Quest<id>.Accept/Reject/Capture`），正式用 `Find.SignalManager.SendSignal` 发送。
- `accept`：原生回调把 pawn 设为玩家派系并 `EndedSuccess`；`reject`：原生拒绝回调并
  `EndedFail`；`capture`：`CaptureUtility.OrderArrest` 下单，任务结束为 Unknown。
- 返回 `quest_id`、`signal`、`form/benefit/downside/aggressive/rejection`、
  `faction_before/after`、`is_prisoner_before/after`、`guest_status_after`、
  `quest_state_after`；dry-run 不发送信号。
- 只响应存活且已生成、对应准确 pawn 的 Ongoing 邀请；任务结束后再决策 409。
- capture 先要求同地图可用的玩家谈话者，再复用原生 `CaptureUtility.CanArrest`；
  没谈话者或合法囚犯床时 422，拒绝发生在发送信号前，邀请保留。
- capture 的 `applied:true/submitted:true` 只表示观察到准确 `Arrest` 工作；
  `arrest_job_id/prison_bed_id` 给出工作与目标床。入狱完成另读 `is_prisoner/in_prison_bed`。
  未知/部分效果 500 时不返回 `applied:false`，先读回再决定下一请求。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G87 实测：触发 `CreepJoinerJoin` 生成 `CreepJoiner88473`（LeatheryStranger/
ShamblerOverlord/Aggressive/SightstealerAttack/AggressiveRejection，quest 2）；
dry-run 200/applied false；正式 accept 200/applied true、`quest_state_after=EndedSuccess`、
`faction_before=null`→`Faction_16`、`guest_status_after=Guest`；fixture `workers`
列出该 pawn。证据 `Reverse/evidence/dlc-G87-creepjoiner-respond.json`，自审
[G87](../references/reviews/2026-11-08-g87-creepjoiner-respond.md)。

G89 实测：同一存档触发两次；reject 200/applied true/`quest_state_after=EndedFailed`、
`faction_after=null`、非囚犯；capture 200/applied true/
`quest_state_after=EndedUnknownOutcome`，响应时 `is_prisoner_after=false`（只下了原生
`OrderArrest`，未等逮捕/入狱完成）。证据
`Reverse/evidence/dlc-G89-creepjoiner-reject-capture.json`，自审
[G89](../references/reviews/2026-11-08-g89-creepjoiner-reject-capture.md)。

## 限制

- G89 为历史信号证据；本次复核已补完整逮捕入床、同 key 冻结重放/单事件、
  缺谈话者/床拒绝，以及 accept/reject/capture 后重复决策 409。
  证据 `Reverse/evidence/anomaly-review-20261108-joiner.json`；越狱/长期行为未验。
- 只验一个随机 form/benefit/downside/aggressive/rejection 组合；其它 CreepJoiner Def、
  Metalhorror 变体、加入后的 downsides/aggressive/rejection 长期行为未验。
- 触发依赖通用 `incident/trigger` 的 `forced` parms；原生 refire/earliestDay 门禁未验。
- 未做 Cua 对照；不把本样本当作 Anomaly 幽魂/蠕行者全功能验收。
