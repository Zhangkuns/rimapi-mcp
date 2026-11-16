# Biotech 机械培育舱读取（G-59）

交付：读取 `Building_MechGestator` 的电源、账单（配方/状态/周期/绑定 pawn/开始 tick）、
培育中的机械、复活中的尸体与可成形状态。证据：
`Reverse/evidence/dlc-G59-mech-gestator.json`、`dlc-G59-run.json`。

## 读取

`GET /api/v1/dlc/biotech/mech/gestator?map_id=0`

```json
{"map_id":0,"gestator_count":1,
 "gestators":[{"gestator_id":"MechGestator...","power_on":true,
   "active_bill":{"recipe":"Scyther","produced_def":"Mech_Scyther","state":"Gathering",
     "gestation_cycles_completed":0,"bound_pawn_id":null,"started_tick":-1},
   "bills":[{"recipe":"Scyther","produced_def":"Mech_Scyther","state":"Gathering",
     "gestation_cycles_completed":0,"bound_pawn_id":null,"started_tick":-1,"suspended":false}],
   "gestating_mech_id":null,"gestating_mech_def":null,"gestating_mech_age_ticks":null,
   "resurrecting_corpse_id":null,"bound_pawn_state_allows_forming":null}]}
```

- `active_bill` 是 `ActiveMechBill`（当前激活账单）；`bills` 是 `BillStack` 里的
  `Bill_Mech` 列表（未激活时 `active_bill=null`，`bills` 仍可见）。
- `gestating_mech_*` 来自 `GestatingMech`；`resurrecting_corpse_id` 来自
  `ResurrectingMechCorpse`；`bound_pawn_state_allows_forming` 只在 `ActiveMechBill`
  非空时读取（原生属性在 active bill 为 null 时会 NRE）。

## 启动培育（G-61）

`POST /api/v1/dlc/biotech/mech/gestator/start`

```json
{"gestator_id":"MechGestator...","mechanitor_id":"321","dry_run":true}
```

- 原生路径是 `JobDefOf.DoBill`（`job.bill = Bill_Mech`）；mechanitor 做该账单时
  `Bill_Mech.Notify_DoBillStarted` 绑定并进入 `Forming`。本端点设置 `job.bill` 并排队。
- 资格：gestator 有 `Bill_Mech` 且状态不是 `Forming/Formed`；mechanitor 为玩家、
  spawned、非倒地/精神、`MechanitorUtility.IsMechanitor`、`HasBandwidthForBill`、可到达；
  同地图。dry-run 不排队。
- 配料由正常搬运/预装提供；本端点不写配料。`applied` 只表示已提交，形成状态要读回。

## 实测

`ai_g31_fixture` 副本（tick 74742）：fixture 生成通电 `MechGestator` + 燃料发电机/导线，
并加入一个 `Scyther` 的 `Bill_ProductionMech`。GET：1 个 gestator、power_on true、
账单 `Scyther`/`Gathering`、无培育机械；fixture observe 交叉核对一致。

## 实测（BT-05 完整阶段生产，2026-11-08）

`ai_g31_fixture` 副本（tick 74742）：fixture 生成通电 gestator、Militor 账单
（Steel 50 + SubcoreBasic 1 已在地图）和带 mechlink 的 mechanitor（fixture 准备披露）。

- 起始：bill state `Gathering`、cycles 0、无 active_bill。
- 产品 `gestator/start`（DoBill）后原生推进：tick 78,066 `Forming`、
  `bound_pawn_id=Human321`、`started_tick=74742`、`bound_pawn_state_allows_forming=true`。
- tick 158,065 `Formed`、`gestation_cycles_completed=1`、
  `gestating_mech_id=Mech_Militor89571`（age 0）；`gestator/eject` 后 mech 在
  (168,96) spawned、未死亡，`gestating_mech_id` 清空。
- 全程无直接改状态/直接完成 fixture；保护开启。证据
  `Reverse/evidence/an05-bt05-gestator.json`，runner `tests/bt05_gestator_staged.py`。
- 边界：多周期/复活账单的阶段差异、断电中断、bill 取消/重排未验。

## 实测（启动）

`ai_g31_fixture` 副本：fixture 生成通电 gestator + `Scyther` 账单、预装配料、给一名
殖民者加 `MechlinkImplant`。GET 账单 `Gathering`；start dry 200/applied false；正式
200/applied true；`gestator_forming` 后账单 `Forming/Formed`、`BoundPawn` 为该 mechanitor；
再次 start 409。

## 培育完成（G-62，fixture 加速）

正常培育要多个游戏日。fixture 用原生 `Bill_Mech.ForceCompleteAllCycles()` 把
`gestationCycles` 设为配方周期、`formingTicks=0`，下一 tick 原生
`BillTick`/`Notify_FormingCompleted` 把账单置 `Formed` 并生成机械（仍在 gestator 内，
`GestatingMech`）。实测：账单 `Formed`、`gestation_cycles_completed=3`、
`GestatingMech=Mech_Scyther...`；GET 读回 `Formed`；再次 start 409。
**未做**成品机械的 eject/激活/移出 gestator（正常玩家后续动作）。

## 搬运入舱（G-66 / G-68）

`POST /api/v1/dlc/biotech/mech/gestator/haul` `{gestator_id, thing_id, pawn_id, dry_run}`

- 原生 `HaulAIUtility.HaulToContainerJob(pawn, thing, gestator)`（`HaulToContainer`）。
- 复活账单（`Bill_ResurrectMech`，G-66）：thing 须为玩家机械尸体；搬入后
  `ResurrectingMechCorpse` 指向该尸体。
- 生产账单（`Bill_ProductionMech`，G-68）：thing 须为该配方允许的配料
  （`bill.IsFixedOrAllowedIngredient`）；搬入后进入 gestator `innerContainer`，账单保持
  `Gathering/Preparing` 直到配料齐。
- 资格：gestator 有上述账单；pawn 玩家、spawned、非倒地/精神、可操作、同地图；thing 不在
  容器内。dry-run 不排队；`applied` 只表示已提交，容器/账单状态要读回。
- 实测：复活尸体 haul→`ResurrectingMechCorpse`/容器包含；生产配料（Militor 的钢等）
  haul→容器包含；非尸体/非配料 422。

### 配料齐后启动（G-73）

G68 只搬一个配料；G73 逐个搬运 Militor 的全部配料（fixture 给搬运者 `MechlinkImplant`
作为 mechanitor），全部进容器后账单仍 `Gathering`，再调 `gestator/start`：dry
`applied:false`、正式 `applied:true`，`gestator_forming` 读回账单 `Forming/Formed`、
`bound_pawn_id` 为该 mechanitor。证据 `Reverse/evidence/dlc-G73-mech-haul-start.json`，
见 [G73 自审](../references/reviews/2026-11-08-g73-mech-haul-start.md)。

## 取出成形机械（G-65）

`POST /api/v1/dlc/biotech/mech/gestator/eject` `{gestator_id, dry_run}`

- 取 `gestator.GestatingMech`（生产或复活账单成形后的机械）；无则 409，已 spawned 则 409。
- 正式：`innerContainer.Remove(mech)` + `GenSpawn.Spawn(mech, gestator.InteractionCell, map)`；
  返回 `mech_spawned/mech_dead`，`applied` 表示实际 spawned 且非 dead。
- 这是把成形机械从 gestator 内取出到地图的 API 动作；正常玩家的 UI 路径是检查器内容选择，
  本端点不依赖该 UI。
- 实测（G-63 复活账单完成后）：dry 200/applied false；正式 200/applied true、mech spawned、
  gestator `GestatingMech` null；再次 eject 409。

## 取出后的控制状态（G-67）

`Bill_Mech.CreateProducts` 已把成形/复活机械的 `Overseer` 关系设为绑定的 mechanitor；
G-65 取出后 `GET /api/v1/dlc/biotech/mech?pawn_id=<mechanitor>` 能列出该机械：
`overseer_id` 等于 mechanitor、`work_mode_def` 非空。实测（G-63 复活→G-65 eject 后）
均通过；无需额外“激活”端点，控制/工作模式沿用已有 mech 端点。

## 复活账单（G-63）

`ResurrectLightMech` 的 `Bill_ResurrectMech` 走同一 `gestator/start`（`DoBill`）路径。
fixture 击杀玩家 Scyther、**先 `corpse.DeSpawn()` 再加入 gestator `innerContainer`**
（直接 `TryAdd` 一个仍 spawned 的尸体不会进入容器），加 mechanitor 后 start：

- 账单 `ResurrectLightMech`/`Gathering` → start dry/正式 → `Forming` → `ForceCompleteAllCycles`
  → `Formed`；
- 原生 `ResurrectionUtility.TryResurrect(dontSpawn:true)` 复活尸体，`GestatingMech` 非空、
  机械 `Dead=false`、health>0；GET gestator 读回 `Formed`；再次 start 409。

首版“未复活”是 fixture 把仍 spawned 的尸体直接 `TryAdd`（未进入容器）导致；despawn 后通过。
能力路径复活见 `biotech-mech-resurrect.md`（G-53）。

## 多周期培育、账单暂停与正常停止（BT-R14，2026-11-13）

run `bt-r14-20261113-run5`（独立 Xvfb/profile，`g35_deathresting` 副本，tick 76787 起，
port 8765），fixture 生成玩家 `LargeMechGestator`（thing 75481）+ 燃料发电机/导线、
地图上的 Steel75/Plasteel75/ComponentIndustrial4/SubcoreRegular1、带原生 mechlink 的
mechanitor（Human75478，fixture 生成）与一名搬运者，并使用原生 `Bill_ProductionMech`
（`Scyther`、RepeatCount=1、DropOnFloor、load_id 1）。全部形成周期、工作、成品生成与
收集均由游戏原生驱动；产品端点只做读取、配料搬运、一次启动和账单暂停/恢复：

- `GET /map/work-tables` 列出该 gestator（`id=75481`），`GET /buildings/bills` 读回
  `recipe_def_name=Scyther`、`repeat_mode=RepeatCount`、`repeat_count=1`、
  `store_mode=DropOnFloor`、`suspended=false`；`GET /dlc/biotech/mech/gestator` 读回
  `power_on=true`、`bills[0].state=Gathering`、cycles 0、无培育机械。
- `POST /gestator/haul` 逐配料搬运：Steel 75 因搬运容量分两趟（68+7），Plasteel 同样
  68+7，Component*4 与 SubcoreRegular*1 各一趟；容器读回 75/75/4/1，账单保持 Gathering。
- `POST /gestator/start` 一次（`applied=true`，提交 `DoBill`）。tick 80,012 账单
  `Forming`、`bound_pawn_id=Human75478`、`forming_ticks=119985.14`。
- `PUT /buildings/bill/suspend {suspended:true}`：账单 `suspended=true`。暂停 600 ticks 后
  `forming_ticks` 仍 119985.141、`current_forming_percent` 仍 0.000124、cycles 仍 0
  （`Bill_Autonomous.BillTick` 对 `suspended` 直接返回，形成计时冻结）；`suspended:false`
  恢复后仍是同一账单并继续形成。
- tick 104,848 第一周期完成：cycles 1、`state=Preparing`、`forming_ticks` 重置 120000。
  未再调用 start；原生 `WorkGiver_DoBill` 对非 Gathering 的
  `Bill_Autonomous` 直接返回裸 `DoBill`（`WorkOnFormedBill`），tick 107,116 起第二周期
  `Forming`（cycles 1、f=119990.09，mechanitor 的 `DoBill` job 可见）。
- tick 131,361 `state=Formed`、cycles 2，容器内 `Mech_Scyther` 与两个 `Wastepack`(5)。
  原生 WorkGiver 的 300 work 收集后，tick 132,765 成品 `Mech_Scyther76255` 在 (117,156)
  spawned、`overseer_id=Human75478`、`bandwidth_cost=2`；`GET /dlc/biotech/mech?pawn_id=`
  读回 `used_bandwidth=2/6`、`group_index=1`。账单 Reset 为
  `Gathering`/cycles 0/`bound_pawn_id=null`，`RepeatCount=1` 已耗尽（`repeat_count=0`）。
- 再推进 810 ticks：账单仍 `Gathering`/cycles 0/`active_bill=null`，无第二个成品
  ——RepeatCount 耗尽后正常停止，不需要取消端点。

证据 `tools/cua/.runtime/bt-r14-20261113-run5/evidence/bt-r14-run5c.json`（`phase=passed`）；
runner `tests/parallel/biotech/bt_r14_multicycle_gestator.py`，fixture 命令
`prepare_gestator`/`observe_gestator` 在 `tests/parallel/biotech/BtFixture.cs`。

披露与边界：fixture 生成 mechanitor 并给原生 `MechFormfeederImplant` 到 def 自身上限
severity 6，使两个 120000-tick 形成期落在有界推进内；周期数、每次工作与暂停/停止语义
不变。`GET /pawns/details` 对机械体返回 success 但无 data（共享
`PawnHelper.PawnToDetailedDto` 读取空 `pawn.skills`），本项以 mech 端点与 fixture 原生
Pawn 行读回，未改共享文件。未测断电、多 gestator、超重/复活账单的多周期差异。

## 限制

- 已验：读取、生产/复活账单启动、ForceComplete 完成、尸体/生产配料搬运、成品 eject 与控制
  读回、配料齐后 start（G73）、两周期正常培育与成品原生收集、账单 suspend/恢复及
  RepeatCount 耗尽停止（BT-R14）；未验断电、多 gestator、带宽不足、开始后中断、
  超重/复活账单的多周期、其它配方/配料组合；
- 复活账单的“先 haul 再 start”组合未单独串联（G63 预装尸体、G66 单独 haul）；
- 未做 Cua 对照。
