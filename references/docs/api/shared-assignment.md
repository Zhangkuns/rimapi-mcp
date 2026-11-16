# Shared 普通床/王座/冥想点独立指派与床用途（S-06）

2026-11-12 新增原生 `CompAssignableToPawn` 通用面：

- `GET /building/assignable?thing_id`：组件类型、`AssignedPawnsForReading`、
  `AssigningCandidates`、床的 owner/medical 状态。
- `POST /building/assignable`：`pawn_id` 调 `CanAssignTo`+`TryAssignPawn`；
  省略 `pawn_id` 则逐个 `TryUnassignPawn`。
- `GET /building/bed`、`POST /building/bed`：`ForPrisoners`、`Medical`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 Bed、Throne、MeditationSpot 与 Human321。Bed candidates 4 人；
assign 后 assigned Human321，unassign 清空；medical true 读回；ForPrisoners
true 后 owner_type Prisoner；Throne/MeditationSpot 均 assign 成功；未知 404。
证据 `tools/cua/.runtime/s06-20261112/evidence/s06.json`，runner
`tests/s06_assignment.py`，fixture `prepare_assignable`。

- 未重跑实际睡眠/王座/冥想行为；休眠棺本身无 `CompAssignableToPawn`，其
  enter/eject 归 C-31。本行以原生 assigned/candidate 与床用途读回为准。
