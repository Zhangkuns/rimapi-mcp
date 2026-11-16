# Biotech 生长舱（G-51）

2026-11-12 BT-R01 已修：`POST .../growthvat/cancel` 现区分原生 `CommandCancelGrowth`
（Working，`Finish()` + 移出内容）与 `CommandCancelLoad`（未 Working 的选择取消），
并映射胚胎未开始妊娠时的原生破坏性确认。旧进入/自然18岁Finish成果保留。

交付：读取生长舱状态，以及正常进入/取消装载。证据：G51 进入/取消
（`Reverse/evidence/dlc-G51-growthvat.json`）；G74 完成/18 岁弹出
（`Reverse/evidence/dlc-G74-growthvat-completion.json`）。

## 读取

`GET /api/v1/dlc/biotech/growthvat?map_id=0`

```json
{"map_id":0,"vat_count":1,
 "vats":[{"vat_id":"GrowthVat...","power_on":true,"working":false,
   "nutrition_stored":0.0,"nutrition_needed":0.0,"biostarvation_daily_offset":0.0,
   "selected_pawn_id":null,"selected_pawn_age_years":null,"selected_embryo_id":null,
   "embryo_gestation_pct":0.0,"vat_learning":null,
   "contained":[]}]}
```

- 来自 `Building_GrowthVat`（`Building_Enterable`）：`PowerOn/Working/NutritionStored/
  NutritionNeeded/BiostarvationDailyOffset/SelectedPawn/selectedEmbryo/EmbryoGestationPct`。
- `contained` 是 `innerContainer` 的 pawn/embryo/营养。

## 进入

`POST /api/v1/dlc/biotech/growthvat/enter`

```json
{"map_id":0,"vat_id":"GrowthVat...","pawn_id":"321",
 "carrier_id":"322","dry_run":true}
```

- 站立殖民者仍走 `Building_Enterable.SelectPawn` → `JobDefOf.EnterBuilding`；
  本端点设置 `SelectedPawn` 并排队同一 job。`applied` 只表示已提交；入舱要读回
  `working:true` 与 `contained`。
- 倒地殖民者或显式 `carrier_id` 走原生 `JobDefOf.CarryToBuilding`
  （A=vat、B=takee、actor=carrier）：carrier 必须同图、可操作、可达 vat 交互格和
  takee；找不到合适 carrier 返回 409，不静默换人（BT-09，2026-11-10）。
- 资格用原生 `CanAcceptPawn`：未工作、通电、无胚胎、年龄 <18、无 BioStarvation、
  `IsColonist && !IsQuestLodger`。原生 growth vat 只接受殖民者，囚犯/奴隶仍被
  `CanAcceptPawn` 拒绝，不由本端点绕过。
- dry-run 不写 `SelectedPawn`。实测倒地儿童 Human75485 由 Human75488 搬入
  GrowthVat75478，tick 76988 `working:true`/occupant 一致；未知 carrier 409、
  占用 422、未知 pawn 404（`Reverse/evidence/bt09-growthvat-carry-20261111.json`，
  自审 [BT-09](../references/reviews/2026-11-10-bt09-growthvat-carry.md)）。

## 胚胎装载（BT-03，2026-11-08）

`POST /api/v1/dlc/biotech/growthvat/embryo`

```json
{"map_id":0,"vat_id":"GrowthVat...","embryo_id":"HumanEmbryo...","dry_run":true}
```

- 镜像原生 vat gizmo：在 vat 同图列出已生成、未被玩家禁用的 `HumanEmbryo`，调用
  `Building_GrowthVat.SelectEmbryo(embryo)`，同时设置 `embryo.implantTarget = vat`；
  之后原生 `WorkGiver_HaulToGrowthVat` 把胚胎搬入舱内并开始妊娠。
- 前置：vat 通电、无已选 pawn/其他胚胎、未 Working；embryo 同图、已生成、未禁用。
  `embryo_id` 接受 ThingID 或数字 thingIDNumber。
- 可选 `Idempotency-Key` 使用 game_memory 合同；2026-11-10 修复精确注册遗漏，带 key 不再在协调器500。
  本轮只验证404拒绝及冻结重放，保留 BT-03 胚胎选择→搬入→working 的原运行证据。
- dry-run 只投影；响应含 `selected_embryo_after`/`implant_target_after`；入舱后读回
  `working:true`、`contained` 含胚胎、`embryo_gestation_pct` 从 0 开始增长。
- 实测 `ai_g31_fixture`（tick 74742）：fixture 生成通电 vat + 由两名殖民者来源构造的有效
  `HumanEmbryo`（9 个基因，fixture 准备已披露）；dry/正式 200、选择读回一致；推进后
  `working=true`、contained 含 `HumanEmbryo88479`、`embryo_gestation_pct=0.00077`；
  再次选择（胚胎已在容器内、不在 map lister）404。证据
  `Reverse/evidence/an05-bt03-embryo.json`，runner `tests/bt03_growthvat_embryo.py`。
- 边界：胚胎 540,000 tick 妊娠到出生未跑；倒地/囚犯搬运原生入口为 WorkGiver_CarryToGrowthVat→CarryToBuilding，本端点
  未实现；多胚胎/禁用/无电拒绝路径部分未逐项。

## 取消/提前结束

`POST /api/v1/dlc/biotech/growthvat/cancel` `{map_id, vat_id, confirm, dry_run}`

镜像 `Building_GrowthVat.GetGizmos` 的两条原生路径，按 vat 当前状态自动选择：

| vat 状态 | 原生路径 | API 行为 |
| --- | --- | --- |
| `working:true`，已选人物 | `CommandCancelGrowth`→`Finish()`→`TryDropAll` | 提前结束生长并弹出人物/内容，`mode:"cancel_growth"` |
| `working:true`，胚胎已入舱且尚未开始妊娠（`EmbryoGestationTicksRemaining>0`） | 原生 `Dialog_MessageBox` 确认后同一 `Finish()` | 未带 `confirm:true` 返回 409 + `confirmation_required:true`；确认后原生销毁胚胎并清空 vat |
| `working:true`，胚胎妊娠已到完成点 | 原生无确认，`Finish()` 走出生 | 直接出生并清空，`confirmation_required:false` |
| `!working` 且已选人物/胚胎 | `CommandCancelLoad`：移出内容、中断 `EnterBuilding`、清选择 | `mode:"cancel_load"`；胚胎仍存在，`implantTarget` 一并清空 |

- 响应带 `mode`、`confirmation_required`、`before`/`after`（`GrowthVatRow`）、`applied`；
  `applied` 要求 `working:false` 且 `selected_pawn_id`/`selected_embryo_id` 均为空。
- `dry_run` 只返回 `would_cancel` 与将要走的 `mode`，不调用原生 `Finish()`。
- 已实测：Working 人物取消 200/applied、`working:false`、`contained:[]`；胚胎选择取消
  200/`cancel_load`、胚胎仍 spawned 且 `implant_target:null`；胚胎入舱后未确认 409、
  `confirm:true` 后 200 并清空（`tools/cua/.runtime/fix3-20261112/evidence/fix3-first-three.json`，
  runner `tests/fix3_first_three.py`）。fixture 只生成通电 vat/胚胎并推进 tick；
  `Finish()` 是原生私有方法，适配通过 Harmony `AccessTools` 固定调用，不含任意反射入口。

## 实测

`ai_g31_fixture` 副本（tick 74742），fixture 生成通电 `GrowthVat` + 燃料发电机/导线、
一名 5 岁玩家殖民者；保护开启。

- GET：power_on true、working false。
- enter dry 200/applied false 且未选人；enter 200/applied true，selected 准确、
  job `EnterBuilding`。
- cancel dry/正式 200/applied true，selected 清空、job 不再是 EnterBuilding。
- 再次 enter 后 `growthvat_entered`：working true、contained 含该 pawn、
  `VatLearning` true；working 时 cancel 现在 200/applied 并走原生 `Finish()`。

G74：fixture 把入舱者设为 17.9 岁并在舱内放 50 个营养膏；API enter 后推进到原生
`growthvat_completed`：vat working false、selected 清空、pawn spawned/alive、
`AgeBiologicalYearsFloat >= 18`、API vat `working:false`。见
[G74 自审](../references/reviews/2026-11-08-g74-growthvat-completion.md)。

## 限制

- 用 17.9 岁起始加速完成，不是从 5 岁自然多日生长；营养只验证舱内有食物，不验营养耗尽/
  BioStarvation/断电/多舱/胚胎路径、倒地/囚犯搬运、取消竞态；
- 未做 Cua 对照。


## 胚胎出舱出生（BT-11d）

`POST /dlc/biotech/growthvat/embryo` 选入并通过原生 haul 后，vat 按
`EmbryoGestationTicksRemaining` 推进；剩余 0 时原生 `FinishEmbryo` 调
`PregnancyUtility.ApplyBirthOutcome` 生成 baby。服务不生成 baby/写父母基因。

实测 GrowthVat88473 + HumanEmbryo88479（11 genes，mother Human66734/father
Human321）：仅缩短 `startTick` 到 1，tick 76749 vat 清空、baby Human88508（~0 岁、
11 genes、父母一致、存活）（`Reverse/evidence/bt11d-growthvat-birth-20261111.json`，
自审 [BT-11d 出生](../references/reviews/2026-11-10-bt11d-growthvat-birth.md)）。
边界：只测一胚胎/缩短剩余妊娠；自然妊娠/早产/双胞胎/出生信件/营养断电未验。
