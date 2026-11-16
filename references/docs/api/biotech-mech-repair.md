# Biotech 机械体自动维修策略（BT-R17）

2026-11-12 新增：读写 `CompMechRepairable.autoRepair`，即原生
`PawnColumnWorker_AutoRepair`/机械体 gizmo 的同一公开字段；一次维修工作保留。

## 读取

`GET /api/v1/dlc/biotech/mech/auto-repair?pawn_id=Mech_Scyther...`
→ `pawn_id/def_name/is_mech/faction/auto_repair/job_def`。

## 设置

`POST /api/v1/dlc/biotech/mech/auto-repair` `{pawn_id,enabled,dry_run}`：
写 `autoRepair`，返回 `before_auto_repair/requested_auto_repair/auto_repair/
applied`；无 `CompMechRepairable` 422；dry_run 不写。
合同：POST `policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 使用或生成一个玩家机械体并把 autoRepair 置 false；GET false；dry 保持；
on 后 GET true；off 后 fixture 读回 false。
证据 `tools/cua/.runtime/bt-r17-20261112/evidence/bt-r17.json`，runner
`tests/bt_r17_mech_repair.py`，fixture `prepare_mech_repair`/`observe_mech_repair`。

- 未测（2026-11-12 当时）：损坏机械体的实际自主维修循环（既有一次 RepairMech
  证据保留）、无机械体/非玩家机械拒绝、维修材料消耗；本行只补策略开关读回。

2026-11-13 自主维修服从开关（`bt-r17-20261113-run3`，同一 g35 副本 tick 76787 起，
port 8766）：
fixture 生成一名玩家 `Mech_Scyther`、用 `HediffSet.AddDirect` 加一个原生 `Cut`
损伤 severity 10（debug 无敌的 `Pawn_HealthTracker.AddHediff` hook 会拦住常规注入），
能量置满 100/100；给成年殖民者 Human321 原生 `MechlinkImplant` 并设 Smithing=1
（其他殖民者 Smithing=0），机械体 autoRepair 初始 false。

- false 推进 2500 tick：无 `RepairMech` job、损伤 severity 保持 10，能量仅自然流失
  100→99.87。
- 现 policy 端点 dry 后正式 `enabled=true`（applied/auto_repair true，GET 一致）；
  tick 79600 原生 `WorkGiver_RepairMech` 给 Human321 `RepairMech` job，
  tick 82066 原生 `JobDriver_RepairMech/RepairTick` 将损伤治到 0
  （health 100%、CanRepair 不再为真），能量 99.87→96.44（维修消耗明显大于自然流失）。
- fixture 重新加 10 severity 损伤后 `enabled=false` 再推进 2500 tick：无新 `RepairMech`
  job、损伤保持 10、能量 96.44→96.31；GET 读回 false。全程无 `/mech/repair` 手动调用。

证据 `tools/cua/.runtime/bt-r17-20261113-run3/evidence/bt-r17.json`；runner
`tests/parallel/biotech/bt_r17_mech_repair_auto.py`；fixture
`prepare_mech_repair_flow`/`damage_mech_again`/`observe_mech_repair_flow`。收口
`desktop.py stop` 报告 source_save/formal_dll/global_prefs unchanged。

- 未测：中断进行中的维修、缺失部位/武器修复、多机械体/多机械师、断电/低能量边界、
  存档读回；既有一次维修结果仍引用 2026-11-12 证据。
