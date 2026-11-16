# Biotech 生殖链（BT-11）

取卵复用现有 `POST /api/v1/pawn/surgery`，不需要新路由：

```json
{"pawn_id":"Human66736","recipe_def":"ExtractOvum",
 "doctor_id":"Human75481","dry_run":false}
```

- `recipe_def=ExtractOvum` 由 `PawnControl.AddSurgery` 走原生
  `Recipe_ExtractOvum.AvailableReport`：ChildrenAllowed、女性、未怀孕、年龄≥
  `minAllowedAge`、非不孕/已取卵；随后创建 `Bill_Medical` 并由指定医生的原生
  `DoBill` job 执行。
- 成功由 `Recipe_ExtractOvum.OnSurgerySuccess` 生成 `HumanOvum`，通过
  `CompHasPawnSources.AddSource(母体)` 记录来源；母体获得原生 `OvumExtracted`
  hediff。服务不直接生成卵或写来源。
- 手术账单需要病人在床上（原生 DoBill 返回“not ready for surgery”）；普通站立
  病人不会被本端点伪装为可手术。研究前置为 `FertilityProcedures`，未完成时
  `recipe.AvailableNow` 返回 422。
- `dry_run` 只返回资格/计划；正式请求返回 `queued/job_submitted/job_def=DoBill`。

## 实测（BT-11a，2026-11-10）

- 存档副本 `g35_deathresting`（tick 76787）；fixture 在隔离副本完成
  `FertilityProcedures`，使用存档中已在床上的 46.9 岁女性 Human66736（非不孕；
  当时带有 `Deathrest/XenogermReplicating` 等既有 hediff），生成 Medicine 15 医生
  Human75481、男性对照 Human75478 和 5 份 medicine。
- dry 200、正式 200（`job_submitted:true`、`DoBill`）；tick 78994 原生完成，
  `HumanOvum75527.sources=[Human66736]`，母体新增 `Anesthetic/OvumExtracted`。
- 男性 422（性别）；同一母体再次取卵 422（`OvumExtracted`）。
  证据 `Reverse/evidence/bt11a-ovum-20261111.json`，自审
  [BT-11a](../references/reviews/2026-11-10-bt11a-ovum.md)。

## 受精（BT-11b）

`POST /api/v1/dlc/biotech/ovum/fertilize`

```json
{"map_id":0,"ovum_id":"HumanOvum75526","father_id":"Human75478",
 "confirm_inbred":false,"dry_run":false}
```

- 父亲资格来自原生私有 `HumanOvum.CanFertilizeReport`（男性、非任务借宿者、
  可达、≥14、非不孕、非倒地且可操作）；有亲缘时先返回 409 +
  `confirmation_required`/关系/概率，`confirm_inbred:true` 后才继续。
- 原生 `JobDefOf.FertilizeOvum`（count=1）由父亲执行 180 tick，完成后
  `HumanOvum.ProduceEmbryo` 生成 `HumanEmbryo` 并记录父母来源；服务不直接生成
  胚胎或写 GeneSet。
- 实测 BT-11b（2026-11-10）：正常 BT-11a 的 `HumanOvum75526` + father
  Human75478，tick 79973 生成 `HumanEmbryo75565`，mother/father 与 genes
  `Hair_DarkReddish/Hair_DarkBlack/Skin_Melanin4` 一致；女性父亲 422、未知父亲/
  卵 404（`Reverse/evidence/bt11b-fertilize-20261111.json`，自审
  [BT-11b](../references/reviews/2026-11-10-bt11b-fertilize.md)）。
- 边界：未测近亲确认分支（本样本 inbred_chance=0）、未成年/不孕/借宿者/倒地父亲、
  取消/中断、多个卵/父亲。

## 边界

- 仅一个存档内母体样本；未测怀孕/不孕/未成年/任务借宿者拒绝、手术失败、取消账单、
  多次取卵间隔；受精的边界见上。
- 母体是已有 Deathrest/XenogermReplicating 的存档人物，不是“完全健康母体”外推；
  取卵资格由原生 AvailableReport 决定。
- 取卵复用既有手术合同；受精的父亲/卵选择由上述独立入口表达，均由原生工作完成。


## 胚胎植入（BT-11c）

`POST /api/v1/pawn/surgery` `{pawn_id,recipe_def:"ImplantEmbryo",thing_id,confirm}`：
服务绑定 `Bill_Medical.uniqueRequiredIngredients` 并设 `implantTarget`；无
`doctor_id` 时由 colony Doctor WorkGiver 原生 HaulToCell→DoBill，最终
`Recipe_ImplantEmbryo.ApplyOnPawn` 生成 `Hediff_Pregnant`。服务不直接写妊娠。

实测 Human66736（30 岁 bedded，male Human75478，医生）植入
HumanEmbryo75554：tick 85976 `PregnantHuman` severity 0.001、父母/genes 一致、
胚胎消耗（`Reverse/evidence/bt11c-implant-embryo-20261111.json`，自审
[BT-11c](../references/reviews/2026-11-10-bt11c-implant-embryo.md)）。
边界：显式 doctor_id 仍只排 HaulToCell、无二段 DoBill；自然受孕/失败/双胞胎/
早产/囚犯未验。

## 助产/分娩仪式（BT-R13）

分娩本身由 Biotech 的 `Hediff_Labor` → `LordJob_Ritual_ChildBirth` 原生链执行，仪式入口复用
现有 G-31 `POST /api/v1/dlc/ideology/ritual/start`，不新增路由：

```json
{"ritual_id":708,"map_id":0,"thing_id":"Bed75487",
 "roles":{"mother":"Human75478","doctor":"Human75484"},"dry_run":false}
```

- `ritual_id` 是该存档玩家主教义中唯一的 `ChildBirth` precept 实例。目标 `thing_id`
  必须是 `Building_Bed`（真实 `Bed75487`），由原生
  `RitualObligationTargetWorker_ChildBirth` 检查产床可用性和 `PregnancyLabor`；
  `mother`/`doctor` 是 `RitualBehaviorDef` 中仅有的两个角色。
- `roles` 强制母体与医生；服务先以原生 `assignments.PawnNotAssignableReason` 校验
  （母体角色 `allowDowned`，但流血/温度/忙碌等仍按原生拒绝），再调用虚拟
  `behavior.TryExecuteOn`；`LordJob_Ritual_ChildBirth` 由原生
  `RestUtility.GetBedSleepingSlotPosFor(mother, bed)` 建 Lord。
- 性伴侣没有独立角色。原生 `ShouldInitAsSpectator` 把母体的 Lover 加入观众；
  本样本观众 Human75481 同时在 Lord participants 中，出生后婴儿与他是 `Parent`。
- 出生效果由原生 `Hediff_LaborPushing.PreRemoved` → `LordJob_Ritual.ApplyOutcome(1f)`
  → `RitualOutcomeEffectWorker_ChildBirth.Apply` → `PregnancyUtility.ApplyBirthOutcome`
  执行；服务不生成新生儿、不写父母关系、不修改产后 hediff。

### 实测（2026-11-13，run3 重载）

- 存档副本 g35_deathresting（tick 76787，map 0）；fixture 生成三名健康 25 岁
  colonist（母体/伴侣/医生）、加 `PregnantHuman` severity 0.999、生成玩家单床
  `Bed75487` 并把床指派给母体；这些生成/关系/传送是测试前置，劳动、Lord、角色、
  床位预约和出生均为原生。
- tick 77862 原生 `PregnancyLabor` 出现；此时无任何 Lord（`lord_seen:false`），
  证明 labor 本身不自动起仪式。
- 启动前 status 的 `eligibility`：`target_usable/can_execute/can_start:true`、
  `blocking_issues:[]`；dry-run 200 `would_start`，强制母体/医生已进入
  `roles.assigned`，伴侣在 `spectators`。
- 正式 start 200、`submitted/started/applied:true`；`Lord_2` stage 0，
  `target.thing_id=Bed75487`，`roles.mother=[Human75478]`、
  `roles.doctor=[Human75484]`，participants 含伴侣 Human75481；fixture 读回
  `RimWorld.LordJob_Ritual_ChildBirth` 与相同角色。
- 原生推进：max stage 3、母体床位预约 `true`、医生原生
  `CarryToMomAfterBirth`、tick 96643 第一个新子女 Human75809 出现；母体
  `PostpartumExhaustion/Lactating`、labor hediff 清除、仍在 Bed75487；
  婴儿健康 1.0、无异常 hediff、`mother/father` 关系准确。医生是否执行
  `DeliverToBed` 取决于母体是否已自行上床；本样本为 `CarryToMomAfterBirth`。
- 产品读回：`GET /pawns/details?id=75809` 200（body_size 0，携带中也解析）；
  `GET /pawns/relations?pawn_id=75809` 返回 `Parent` Human75478/Human75481；
  `GET /dlc/ideology/ritual/status?...&run_id=ritual-…` 的 `last_run` 角色/目标准确，
  `terminal_tick=96634`、`native_cancelled:false`。
- 证据 `tools/cua/.runtime/bt-r13-20261113-run3/evidence/bt-r13-final.json`；
  自审 [BT-R13](../references/reviews/2026-11-12-biotech-bt-r13-childbirth.md)。

### 已知跨域缺口（非本行验收）

`GET /dlc/ideology/ritual/status` 对本次分娩返回 `run_state:"interrupted"`、
`completed:false`（但 `terminal_tick` 存在、`native_cancelled:false`、婴儿/母体结果已
落地）。原因是 `LordJob_Ritual_ChildBirth` 覆写了虚拟 `LordJob_Ritual.RitualFinished`，
而共享 `Hooks/RitualTerminalHooks.cs` 只 patch 基类方法；Harmony 不会拦截覆写实现，
`NativeFinished` 未被观察。该 hook 属共享/Ideology 入口，已在交接中请求最小修复
（新增 `LordJob_Ritual_ChildBirth.RitualFinished` postfix 调
`RitualControlService.ObserveNativeFinish`）；本行不以该分类字段代替出生结果。

### 边界

- 单样本、单床、单医生；未测双胞胎/多胎、仍死产、婴儿疾病、母体死亡、难产处置、
  取消/中断、保存加载后的分娩 Lord。
- 未伪造 RNG；`babiesAreHealthy` 等难度结果不在本页保证。
- 使用生成人物而非 g35 原有受伤/营养不良 colonist；理由与披露见上。
