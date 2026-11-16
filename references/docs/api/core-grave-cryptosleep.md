# Core 坟墓指派与冬眠舱弹出（C-31 部分）

2026-11-12 新增（C-31 仍在开放，搬运末段未闭环）：

- `GET /building/grave?thing_id`：原生 `AssignedPawn`、`AssigningCandidates`、
  corpse/holder、StorageTab 可见性。
- `POST /building/grave/assign`：传 `pawn_id` 调用
  `CompAssignableToPawn_Grave.TryAssignPawn`；省略/空 `pawn_id` 调
  `TryUnassignPawn`；dry_run 只验证。
- `GET /building/cryptosleep?thing_id`：容器、pawn/corpse。
- `POST /building/cryptosleep/eject`：原生 `Building_Casket.EjectContents`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 Grave、CryptosleepCasket、claimant 和 sleeper；坟墓候选人包含
claimant；assign 读回 claimant，unassign 读回 null；casket 放入 sleeper 后
GET 读回，dry_run 保持，eject 后空；未知 grave 404。
证据 `tools/cua/.runtime/c31-20261112/evidence/c31.json`，runner
`tests/c31_grave_casket.py`，fixture `prepare_grave_casket`。

- **仍未闭环**：他人把倒地 pawn 搬入 casket 的双目标
  `CarryToCryptosleepCasket` job 实测未完成（job 提交但未入舱），因此 C-31
  保持 G；本提交只保留已验收的 assign/eject 产品能力。

## 2026-11-13 搬入冬眠舱闭环

新增 `POST /api/v1/building/cryptosleep/carry`：

```json
{"carrier_pawn_id":"Human315","pawn_id":"Human75478",
 "casket_id":"CryptosleepCasket75481","dry_run":false}
```

- 只走原生 `JobMaker.MakeJob(JobDefOf.CarryToCryptosleepCasket, 倒地pawn,
  casket)` + `carrier.jobs.TryTakeOrderedJob(JobTag.Misc)`、count=1；不直接
  `TryAcceptThing`、不传送/强插。
- 校验：搬运者为可搬运自由殖民者（Manipulation、非倒地/精神崩溃）；目标
  Downed、非 quest lodger、无额外 host faction；`casket_id` 缺省用原生
  `FindCryptosleepCasketFor`（null 422），显式须同图/空/`Accepts`/可达可预留；
  全部在派 job 前完成，dry_run 不派 job。
- 2026-11-13 隔离 g35：native job targetA=倒地 pawn、targetB=casket、count=1；
  有界推进 tick 79240 后 casket `contained_pawn_id` 正确；非倒地/已占用/
  无空 casket 422；原生 eject 后回空；grave assign/unassign 回归通过。
  证据 `tools/cua/.runtime/c31-20261113-run2/evidence/c31.json`，runner
  `tests/c31_grave_casket.py`，fixture `tests/parallel/core-grave`。
- 未测：多搬运者竞争、drafted carry 变体、job 中途 casket 被毁、quest lodger/
  guest prisoner 负例。
