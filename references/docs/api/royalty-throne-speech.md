# Royalty 王座演讲独立结果（RY-R10）

本单元不新增路由，复用已有
`GET /api/v1/dlc/ideology/ritual`、`GET /api/v1/dlc/ideology/ritual/status`
和 `POST /api/v1/dlc/ideology/ritual/start`，验证皇室王座演讲（ThroneSpeech）
与主理念 LeaderSpeech 是两个实例，且结果、思想和冷却都来自原生。

## 正常前置

- 组织者持 `Praetor`（或更高、授予 `Speech` 能力）头衔；
- `ownership.AssignedThrone` 指向已安装王座，且 `AnyUnmetThroneroomRequirements()` 为 false
  （实际校验：Throne、面积≥40、impressiveness≥90、全铺地板、2 个点燃火盆、
  4 根柱、钢琴/竖琴类、无生产/床/祭坛等，见 `RoomRequirement` 列表）；
- `GET /dlc/ideology/ritual` 中出现 `precept_def:"ThroneSpeech"` 的实例
  （本存档 `ritual_id=724`，不是 `LeaderSpeech=716`）。

## 启动与结果

`GET /dlc/ideology/ritual/status?ritual_id=724&map_id=0&thing_id=<throne>&organizer_pawn_id=Human321`

```json
{"eligibility":{"target_usable":true,"can_execute":true,"can_start":true,
  "blocking_issues":[],"roles":[{"role_id":"speaker","required":false,...}]}}
```

`POST /dlc/ideology/ritual/start`

```json
{"ritual_id":724,"map_id":0,"thing_id":"Throne75518","organizer_pawn_id":"Human321"}
```

- dry-run 返回 `would_start:true/applied:false`，不建 Lord；
- 正式返回 `started:true/applied:true/run_id`，`running.lord_id` 存在，
  `ticks_left=5000`（原生阶段时长）；
- 运行中 fixture 反射读回 `lord.LordJob` 为 `LordJob_Joinable_Speech` 且私有
  `titleSpeech=true`，与 `RitualBehaviorWorker_ThroneSpeech` 的构造一致；
- 完成后 `status.run_state:"completed"`、`completed_source:"exact_lord_native_terminal"`、
  `native_cancelled:false`、`duration_ticks:5001`；
- `outcome_memories` 是参与者实际获得的演讲思想（本次 Human315/Human318 各
  `UninspiringSpeech`，mood -4），不是假定固定结果；
- `Speech` 能力冷却 `CooldownTicksTotal=1200000`、演讲结束后
  `CooldownTicksRemaining=1199001`，与 Praetor `speechCooldown` 的
  min=max=1200000 一致。

## 证据与边界

隔离 Xvfb `g35_deathresting` 副本（tick 76787，五 DLC）：
证据 `tools/cua/.runtime/ry-r10-20261112c/evidence/ry-r10.json`
（runner `tests/parallel/royalty/ry_r10_throne_speech.py`，fixture
`tests/parallel/royalty/fixture/RyFixture.cs`）。

fixture 仅搭建合法王座房间（9x9 大理石、门/屋顶/火盆/柱/钢琴/雕塑）；头衔授予、
王座指派、仪式启动、`LordJob`/`titleSpeech`、结果思想与冷却均为产品原生。

未重跑：LeaderSpeech（明确不以它代验）、其它头衔等级、演讲中断/取消、
冷却自然到期、多人/多派系听众的转化与灵感分支。
