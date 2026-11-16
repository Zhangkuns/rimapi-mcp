# Shared 囚犯/奴隶持续互动策略（S-05）

`GET/POST /api/v1/guest/policy`。新原生策略/调度路由；旧
`/pawn/guest-policy` 保持兼容不改。

## GET

`GET /api/v1/guest/policy?pawn_id=Human75478`

返回原生 `Pawn_GuestTracker`：
- `guest_status`、`exclusive`、`enabled_non_exclusive`、`slave_mode`；
- `scheduled_for_interaction`、`scheduled_for_slave_suppression`、
  `current_job`/`current_job_target`；
- `prisoner_modes[]`：原生 Def 全量（`def_name/label/non_exclusive/
  enabled/eligible/blocker`），包括 ReduceWill、Convert、Enslave、
  Study、Interrogate、Bloodfeed、HemogenFarm；
- `slave_modes[]`：NoInteraction/Suppress/Imprison/Emancipate/Execute。

只列原生 Def；blocker 来自 awake/recruitable/bloodfeeder/hemogenic/
wildman 等原生字段。不伪造 eligibility。

## POST

```json
{"pawn_id":"Human75478","action":"start","exclusive":"Study","confirm":true}
```

`action=set|start|stop`；`exclusive`/`toggle`+`toggle_enabled`/
`slave_mode` 按原生 setter：
- `set`/`start`：`SetExclusiveInteraction`、
  `ToggleNonExclusiveInteraction`、`slaveInteractionMode`；
- `stop` prisoner：`SetNoInteraction`（MaintainOnly）或取消指定
  non-exclusive；slave：NoInteraction/Suppress/Execute。
- 不手造 warden job；`start` 只设置模式并读回 `ScheduledForInteraction`/
  当前 job。实际自适应工作由游戏 WorkGiver/warden 决定，若窗口内未出现，
  响应 `applied:true` 但不声称已调度。
- dry 只校验/读回，不写；非 dry 需 confirm；Execute 设置需 confirm，
  不作实际不可逆执行。
- 错误：非法/未知模式 400/422；无 guest 422；未知 pawn 404；无 confirm
  400；不可用/失效 409。

## 验证与边界

- runner：`tests/s05_guest_policy_native.py`（phase passed）；
  证据：`tools/cua/.runtime/s05-guest-20261113i/evidence/s05.json`。
- fixture：`tests/parallel/shared-guest-policy/CoreGuestPolicyFixture.cs`
  生成原生 prisoner/床/warden，并可将目标 pawn 置为 slave；不造 job。
- 已验：GET 全模式/eligibility；Interrogate start/dry；真实 warden 在安全
  监狱房自主接取 `PrisonerInterrogateIdentity` job（warden Human75498 →
  prisoner Human75495）；stop 后 exclusive MaintainOnly，后续窗口无新互动 job。
  Convert/ReduceWill/Enslave stop 均 MaintainOnly；slave Suppress→stop
  NoInteraction；Execute dry；未知 pawn/模式、无 confirm 4xx。
- 未验：Bloodfeed/HemogenFarm 实际终态、Convert/ReduceWill/Enslave 终态、
  Execute 实际不可逆；但原生调度/停用代表已成立。
