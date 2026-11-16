# Biotech 生育与死亡休眠读取（G-35）

已交付：读取 25 项（clean/pregnant/deathrest）、死亡休眠唤醒 11 项、死亡休眠入睡 19 项。
证据：`Reverse/evidence/dlc-G-35-clean.json`、`dlc-G-35-pregnant.json`、`dlc-G-35-start.json`。

## 端点

`GET /api/v1/dlc/biotech/life?pawn_id=315`

```json
{"pawn_id":"Human315","pregnant":true,"gestation_progress":0.001,
 "pregnancy_attitude":null,"deathresting":false,"deathrest_hediff":false}
```

- `pregnant`/`gestation_progress`/`pregnancy_attitude` 来自原生 `Hediff_Pregnant`；
  未怀孕时 `gestation_progress=-1`。
- `deathresting` 是原生 `Pawn.Deathresting`；`deathrest_hediff` 表示是否存在
  `Hediff_Deathrest`。
- BT-11e：`deathrest_capacity`/`deathrest_capacity_used` 取原生
  `Gene_Deathrest.DeathrestCapacity`/`CurrentCapacity`；`deathrest_linked_devices` 列出
  绑定该人物的设备（`thing_id/def_name/presence_ticks/powered/fuel/can_increase_presence`）。
  `deathrest_ticks/deathrest_min_ticks/deathrest_progress` 与 `deathrest_auto_wake`
  直接读取原生 gene；最短时间受设备效率影响，不固定为四天。
  绑定并不保证设备可工作：供电与血原质燃料决定 presence；完整周期唤醒时原生按
  presence 比例授予加成。该读取不提供连接、计时或加成 setter。
  2026-11-11 已补完整原生周期与安全唤醒，详见[修复自审](../references/reviews/2026-11-11-five-dlc-gap-repair.md)。
- 动物也可读（`pregnant:false`）；未知 pawn 404，缺 `pawn_id` 400，
  Biotech 未启用 422 `dlc_missing`。

## 自动分娩（G-64，fixture 加速）

正常人类分娩是两段式：`Hediff_Pregnant.TickInterval` 在 `GestationProgress>=1` 时
`StartLabor()` 添加 `PregnancyLabor`，随后劳动/推出（`Hediff_LaborPushing`）走原生
`PregnancyUtility.ApplyBirthOutcome` 生成**一个**新生儿；`DoBirthSpawn` 是
`Hediff_Pregnant.TickInterval` 的非人形分支（使用 `RaceProps.litterSizeCurve`）。
没有“手动分娩”正常端点；本项只验证自动路径。

fixture 用 `HediffMaker.MakeHediff(PregnantHuman)` + `PostDebugAdd()` 创建孕期并把
severity 设到 0.999，游戏自动进入 labor 并在约 6000 tick 后生下孩子。实测：
`GET life` 从 `pregnant:true/progress 0.999` 变为 `pregnant:false`；
`mother.relations.Children` 从 0 变为 1（`Human88596`）。mother 在产后 health 变化会触发
fixture 的 damage guard（测试工具停止条件），但终态读回确认出生。

## 唤醒死亡休眠

`POST /api/v1/dlc/biotech/life/deathrest/wake`

```json
{"pawn_id":"Human66736","confirm":true,"dry_run":false}
```

- 原生 `Gene_Deathrest.Wake()`；实测 `deathresting true→false`、`downed` 清除、
  `Hediff_Deathrest` 消失并出现 `InterruptedDeathrest`（早醒是中断，不是完成休眠）。
- 仅玩家所属、存活、有死亡休眠基因且正在休眠的 Pawn；非休眠 409，缺 confirm 400，
  dry-run 不调用原生，同 key 重放不二次唤醒，结果按 `woke/not_woke/unknown` 分类。
- 证据 `dlc-G-35-wake.json`（11 项）。
- **BT-04 真实 casket 建设/绑定（2026-11-08）**：`ai_g35_start_fixture` 副本（tick 70389）用产品
  `/builder/blueprint` 正常建造 `DeathrestCasket`（材料 `/item/spawn` 准备；fixture 只给该
  1×2 格补一个 constructed roof，已披露），建成 `DeathrestCasket75447` 后 `/deathrest/start`
  原生订单进入：`deathresting=true`、`CompDeathrestBindable.BoundPawn=Human66736`、
  `current_bed/assigned_casket` 均为新 casket；`/deathrest/wake` 后 `deathresting=false`、
  出现 `InterruptedDeathrest`，绑定保留（原生）。证据 `an05-bt04-casket.json`，runner
  `tests/bt04_deathrest_casket.py`。
- 后续证据已补容量血清、自动绑定及218187tick完整原生周期/presence/GlucosoidRush安全唤醒，
  见[2026-11-11修复自审](../references/reviews/2026-11-11-five-dlc-gap-repair.md)；旧阈值样本仍仅证明其原范围。
  自动唤醒开关当前只读、独立棺指派尚缺映射，见[BT-R06/S-06](../product-specs/five-dlc-remaining-checklist.md)。
  地面入睡由原生 `JobGiver_GetDeathrest` 自动路径处理，不新增手动setter。
- 结果按 `woke/not_woke/unknown` 分类，`applied` 缺省=未知；注入证据见
  `dlc-G-39-D1-G35-D2-faults.json`（本动作静默不唤醒/前抛/后抛/读回失败/非玩家 422），
  守卫见 `dlc-G-35-wake-guards.json`（无 Game 409、DLC-off 422）。

## 进入死亡休眠（玩家床订单）

`POST /api/v1/dlc/biotech/life/deathrest/start`

```json
{"pawn_id":"Human66736","bed_id":"Bed71074","dry_run":false}
```

原生路径与玩家右键菜单同源：`JobMaker.MakeJob(JobDefOf.Deathrest, bed)`、
`job.forceSleep=true`、`pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc)`。
`JobDriver_Deathrest` 先 `ClaimBedIfNonMedical`，走到床后躺下并 `AddHediff(Deathrest)`；
因此**下单不等于已入睡**，响应用 `entry_pending`/`deathresting_after` 明示，客户端轮询
`GET /dlc/biotech/life` 直到 `deathresting=true`（实测约 2 秒加速后进入）。

前置（与原生菜单一致）：活动 Game、Biotech 启用、玩家派系、存活、非倒地、
未在休眠、有 `Gene_Deathrest` 且 `GeneUtility.CanDeathrest`；目标是同图 `Building_Bed`、
`bed_humanlike`、未禁用、可达、`CompAssignableToPawn.CanAssignTo` 通过、
`HasFreeSlot || BedOwnerWillShare || IsOwner`、`OccupiedRect` 全部有屋顶、
`RestUtility.IsValidBedFor` 通过、未被他人预留；同床已有 Deathrest 订单时 409。
露天床按菜单顺序优先返回 422（不会留下 claim 副作用）。

预留预检与 `JobDriver_Deathrest.TryMakePreToilReservations` 同参：
`ReservationUtility.CanReserve(pawn, bed, bed.SleepingSlotsCount, 0, null,
ignoreOtherReservations:false)`（不是 `Pawn.CanReserve(bed)` 的 maxPawns=1/stackCount=-1
默认值）。`IsValidBedFor` 自身也带 `CanReserve(SleepingSlotsCount, 0)` 检查，因此双人床
两个槽位都被占时先由它返回 422（`bed is not a valid bed`）；独立的 409 预留门只覆盖
两次检查之间的状态变化。

结果：`applied:true` 仅表示**订单被接受**（`curJob` 或队列中存在与提交 `job_load_id`
相同 loadID、`Deathrest` def、目标为该床的 job）或下单当次即已干净进入
（前置 false→后 true、有 `Deathrest` Hediff、且在该床）。队列里已排队的同一 job 也算接受。
若原生调用后 job 已结束但 pawn 处于无对应订单的休眠，返回 `partial_or_contradictory`
（500，`applied` 缺省、`uncertain:true`）；读回失败同样缺省。原生异常 → 500，保留观察。

响应字段：`bed_id`、`bed_def`、`distance_cells`、`deathresting_before/after`、
`entry_pending`、`job_load_id`、`current_job`、`current_job_target_id`、
`current_bed_after`（读回时的 `RestUtility.CurrentBed`，可空）、
`order_state`（current/queued/none/unknown）、`ordered`、`entered_deathrest`、
`deathrest_started_without_live_order`。

`entry_pending` 只表示**本次提交**的订单仍然存活（curJob/队列中同 `job_load_id`）且 pawn
尚未 `Deathresting`。dry-run/未提交时固定 false；只有成功读回并确认本次订单不在
current/队列（未接受或已取消）时才为 false。HTTP 500 本身不否定订单存活：例如提交后
抛异常（`post_submit`）时订单仍 current，响应为 500/`applied:true`/`entry_pending:true`。
读回抛异常时订单是否存活未知：`order_state` 返回 `unknown`，`ordered`/`entry_pending`/
`entered_deathrest` 等布尔字段省略，`applied` 继续缺省，`uncertain:true`。
dry-run 不提交，用 `would_order`/`would_entry_pending` 表达投影，`ordered`/`entry_pending`
保持 false。

错误：400 缺 `pawn_id`/`bed_id`；404 未知 pawn/目标；409 无 Game、已休眠、同床已有订单、
被他人占用/预留；422 DLC 未启用、非玩家/倒地/死亡/精神态、无休眠基因、目标非床/
非人形床/异图/不可达/禁用/不可分配/露天/`IsValidBedFor` 拒绝。
`dry_run` 只验证并返回 `outcome:"validated"`、`applied:false`，不调用原生；
`game_memory` 幂等，重放不重复下单；只有 200（order accepted/entered）发 `operation` 事件。

证据：`dlc-G-35-start.json`（21 项，含 dry-run 的 would 投影、GET 活跃订单、
真实 `Deathresting`、存读档、重复订单、重放无重复事件、唤醒后订单清空）、
`dlc-G-35-faults.json`（22 项：提交前抛/提交后抛/读回抛，各含 500 字段合同、
同 key 冻结正文重放、entry/body 计数 1/0、1/1、1/1、无成功 SSE）、
`dlc-G-35-shared-bed.json`（13 项：单人 owner+空槽双人床一槽被占可下单、两槽全占
原生 `Reserve`/`CanReserve` 与 `IsValidBedFor` 均拒绝、dry-run/拒绝后 job 与 owner 不变、
双非恋人 owner 被 `IdeoligionForbids` 拒绝的边界）、`dlc-G-35-queue.json`（6 项：
原生排队→当前→取消的读回）、`dlc-G-35-start-guards.json`（无 Game 409、DLC-off 422）。
临时故障/双人床探针源码归档于 `Reverse/evidence/dlc-static/g35-fault-bed-probe.cs.txt`，
运行 JSON 见上四个文件；探针已从 DLL 移除，路由恢复 260。

`GET /dlc/biotech/life` 另外回读 `in_bed`、`current_job`、`current_job_target_id`、
`current_job_load_id`，并按 `job_load_id` 列出 `deathrest_orders`
（`state: current|queued`、`bed_id`、`target_id`、`force_sleep`）与 `deathrest_pending`
（存在订单且未 `Deathresting`），用于区分“订单排队中/当前/已取消/已入睡”。排队用原生
`TryTakeOrderedJob(..., requestQueueing:true)` 构造；取消用原生 `ClearQueuedJobs`，
排队→当前由原生 `EndCurrentJob` 提升，未手改 `JobQueue`。

## fixture

`ai_g35_deathrest`：由临时探针生成 `Sanguophage` PawnKind（原生 xenotype 为 Sanguophage）+
`HediffMaker.MakeHediff("Deathrest")`，读回 `deathresting:true`、`deathrest_hediff:true`、
`downed:true`；存读档后两字段仍为 true（`dlc-G-35-deathrest.json`）。
`Gene_Deathrest` 没有公开的 `StartDeathrest`，休眠由 `Hediff_Deathrest` 承载，
因此这是“休眠中”状态的实测正例，而不是完整入睡流程。

`ai_g35_pregnant`：由临时探针在 `Human315` 上执行 `HediffMaker.MakeHediff("Pregnant")` +
`Hediff_Pregnant.PostDebugAdd()` 生成（探针已删除，源码留档
`Reverse/evidence/dlc-static/life-probe.cs.txt`）。这是**调试入口**，不是正式受孕流程。

## 自然受孕（原生 Lovin 收尾）

正常受孕来自 `JobDriver_Lovin.<MakeNewToils>b__12_4` 收尾动作：以
`0.05 * PregnancyUtility.PregnancyChanceForPartners(mother, father)` 判定，命中后
`GetInheritedGeneSet` → `HediffMaker.MakeHediff(PregnantHuman, mother)` →
`SetParents(null, father, geneSet)` → `AddHediff`；没有独立“受孕”入口。

副本实测（`ai_g35_start_fixture` 的隔离内存副本，未保存原局）：固定生成 22 岁女性与
25 岁男性（`PawnGenerationRequest` 固定性别/年龄/同 ideo）、配偶关系、屋顶木质双人床；
先用原生 `LayDown`（targetA=床，`forceSleep`，低 Rest 作为 fixture 条件）让双方实际
`in_bed`，再调用真实 `JobGiver_DoLovin.TryGiveJob`。5 次提交的 `Lovin` job 全部
`Succeeded`（女性）/`Incompletable`（男性收尾），共 10 次收尾动作回调（每对 2 次，
按 pawn/job 分账）；第 5 次命中受孕：目标女性 `Hediff_Pregnant=true`、
`SetParents` 观察 1 次、`Mother=null`（原生传 null，载体 pawn 才是母体）、
`Father=Human75411`、继承基因 2 个、`gestation_progress≈0.0011`、attitude `Negative`。
收尾动作回调不等于成功互动：这里单列 `Succeeded` 与 `Incompletable`，不能把 2 个回调
算作 2 次独立受孕试验。

受孕后副本另存 `ai_g35_conceived_after_lovin`。读回与存读档验证（7 项）：父亲、继承基因
列表、孕期进度在重载前后一致；`life` 返回 `pregnant:true` 与进度。受孕后仍可对同一
`Hediff_Pregnant` 调原生 `StartLabor`（强制阶段入口）：它**只**新增 `PregnancyLabor`，
不移除 `PregnantHuman`，本次在早期孕期调用且只观察到该 hediff，**出生未观测**（不写
“窗口内无出生”）；完整自然妊娠→`PregnancyLabor`(1700–25000 ticks)→
`PregnancyLaborPushing`(800–5000)→`ApplyBirthOutcome` 已在 G64 用 severity 0.999 加速验证到
出生（children 0→1）；G35 的早期 `StartLabor` 样本保留。本轮探针（未形成独立证据）把
`RaceProps.litterSizeCurve` 临时设为 2，humanlike 出生仍只有 1 个孩子——该曲线只用于
非人形 `DoBirthSpawn`，1.6 humanlike 路径是单胎。

证据：`dlc-G-35-conception-repeat.json`（5 提交/5 Succeeded/1 受孕/父亲字段/遗传数据）、
`dlc-G-35-pregnant-after-lovin.json`、`dlc-G-35-pregnancy-persistence.json`（7 项）、
`dlc-G-35-labor.json`（强制 StartLabor 入口，出生未观测）；探针源码归档
`Reverse/evidence/dlc-static/g35-preg-probe.cs.txt`，已从 DLL 移除（路由恢复 260）。
限制：`PregnancyUtility` 静态构造若在 Def 未就绪时被 Harmony 触碰会永久初始化失败
（本轮一次实证，移除该观察补丁后恢复）；未修改 Rand、未私调收尾回调。

## 当前边界（后续证据归并）

- 无直接受孕/分娩/流产setter；自然受孕由原生Lovin收尾，胚胎植入由正常手术产生妊娠。G64已加速验证自动出生；humanlike原生单胎，多胎仅适用于非人形DoBirthSpawn。
- 休眠舱绑定/容量血清/外接泵已验；本轮完整原生周期218187tick及GlucosoidRush安全唤醒成立，见[修复自审](../references/reviews/2026-11-11-five-dlc-gap-repair.md)。其它设备/中断变体未验；旧阈值fixture不代表完整周期。
- 地面入睡没有端点（原生 giver 自动路径）；菜单 `RestUtility.IsValidBedFor` 的
  访客/囚犯分支未逐项构造。
- `Paused` 等 `Hediff_Deathrest` 子状态未暴露。
- G64已补加速原生出生；BT-11a/c/d有取卵/AI及显式医生胚胎植入/生长舱出生证据，详见[主计划](../exec-plans/active/five-dlc-remaining.md)。完整自然多日妊娠和全部失败分支未验。
- 1.5 与 Mod 未测。
