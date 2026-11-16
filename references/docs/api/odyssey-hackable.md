# Odyssey 可破解设备自动破解策略（OD-R04）

2026-11-12 新增：读取一台 `CompHackable` 的原生状态，并写入与 `ITab`
AutoHack 开关相同的私有 `autohack` 字段。工作本身仍由原生
`WorkGiver_Hack`（Research 工作类型、priority 130）决定；不直接调用
`HackNow`，也不制造即时解锁。

## 读取

`GET /api/v1/dlc/odyssey/hackable?map_id=0&thing_id=75478&pawn_id=Human321`

```json
{"map_id":0,"thing_id":75478,"thing_id_string":"AncientTerminal_Worshipful75478",
 "def_name":"AncientTerminal_Worshipful","x":235,"z":105,
 "autohack":false,"is_hacked":false,"locked_out":false,"progress_percent":0.0,
 "defence":180,"intellectual_skill_prerequisite":0,
 "only_remotely_hackable":false,"autohack_warning":null,
 "pawn_id":"Human321","work_available":false}
```

- `autohack/is_hacked/locked_out/progress_percent` 是 `CompHackable` 原生属性；
  `defence` 等来自 `CompProperties_Hackable`。
- 传 `pawn_id` 时追加 `work_available`，由原生
  `new WorkGiver_Hack().HasJobOnThing(pawn, thing)` 计算：未开启 autohack 时为
  false，开启后才可能为 true。
- 未知地图 404、非整数 thing_id 400、无 `CompHackable` 422。

## 设置

`POST /api/v1/dlc/odyssey/hackable/auto`

```json
{"map_id":0,"thing_id":75478,"enabled":true,"dry_run":true}
```

- 写 `AccessTools.Field(typeof(CompHackable),"autohack")`，与 gizmo 的
  `toggleAction` 相同；首次开启且 `autohack_warning` 非空时显示与 gizmo 相同的
  NegativeEvent 消息。
- 返回 `before_autohack/requested_autohack/changed/warning_shown/applied` 及完整状态；
  `dry_run` 不写、`applied:false`。
- 合同：GET observe；POST `policy/game_ai/validate/game_memory`（幂等）。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 生成两座 `AncientTerminal_Worshipful`（defence 180）并让一名空闲殖民者
Research 优先级 1、清空当前研究项目（`WorkGiver_Hack` 属 Research，公开准备，
不是结果）；第一座 `work_available:false`，dry 保持 false，正式开启后
`autohack:true/work_available:true`，正常 Hack job 推进并完成（`is_hacked:true`、
progress 1.0）；关闭后第二座仍 `work_available:false`，不吸引后续工作。
证据 `tools/cua/.runtime/od-r04-20261112/evidence/od-r04.json`，runner
`tests/od_r04_autohack.py`，fixture `prepare_hackable`/`observe_hackable`。

- 未测：lockout（失败/锁定）路径、`onlyRemotelyHackable` 的遥控终端、
  破解奖励/任务结果（归 OD-R09）、多殖民者并发与保存恢复。
