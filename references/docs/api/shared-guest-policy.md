# Shared 囚犯/奴隶持续互动策略（S-05）

2026-11-12 新增：

- `GET /pawn/guest-policy?pawn_id`：`GuestStatus`、exclusive interaction、
  已启用的 non-exclusive 模式、slave mode，以及原生可选模式列表。
- `POST /pawn/guest-policy`：`SetExclusiveInteraction`、
  `ToggleNonExclusiveInteraction`、`slaveInteractionMode`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成玩家囚犯。初始 Prisoner；dry 不写；exclusive=Convert、
non-exclusive Bloodfeed=true 后读回；slave_mode=Suppress 读回。
证据 `tools/cua/.runtime/s05-20261112/evidence/s05.json`，runner
`tests/s05_guest_policy.py`，fixture `prepare_guest_policy`。

- 未重跑转换/减意志/Bloodfeed 的实际工作和停用后的后续行为；本行以原生
  interaction policy 读回为代表。
