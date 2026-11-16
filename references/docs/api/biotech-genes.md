# Biotech 基因与异种（G-33）

已交付限定范围：修复后DLC-on独立fixture，82项真实游戏检查通过；证据
`Reverse/evidence/dlc-gene-review.json`。仅对Biotech DLC生效，路径均以`/api/v1`为前缀。

直接基因/异种变更适用范围：已生成、玩家可控的自由人类殖民者，且`pawn.genes`存在。
不生成Pawn、不植入器官、不自动升级Psylink；动物、囚犯、访客、未生成Pawn返回422。

## 读取

`GET /dlc/biotech/gene?pawn_id=321`

```json
{
  "pawn_id": "Human321",
  "xenotype_def": "Baseliner",
  "endogenes": [{"gene_def": "Skin_Melanin2", "active": true}],
  "xenogenes": []
}
```

`pawn_id`回显原生`ThingID`（`Human321`），请求仍接受`321`。
`active`是原生`Gene.Active`：基因存在但被冲突覆盖时仍列出，`active:false`。

## 单基因增删

`POST /dlc/biotech/gene`

```json
{"pawn_id":"321","gene_def":"DarkVision","xenogene":true,"dry_run":true}
```

成功数据为`{pawn_id,xenotype_def,endogenes,xenogenes,gene_def,xenogene,changed,dry_run}`。
`xenogene`选择目标层（true=外源，false=内源），不会把同Def静默改到另一层：
同层已存在返回`changed:false`且不发operation；另一层已有返回409 `conflict`。

`DELETE /dlc/biotech/gene` 请求体相同。只删除所选层里的那一个基因；
该层没有返回404，同层出现多个同Def返回409（不批量删）。
`dry_run:true`只报告`changed`并回显当前列表，不构造`Gene`、不调用`AddGene/RemoveGene`，也不发operation。

真实增删调用公开`Pawn_GeneTracker.AddGene(GeneDef,bool)`/`RemoveGene(Gene)`，
由原生维护Hediff、能力、外观等副作用；不承诺反向删除能恢复历史状态。
可选`Idempotency-Key`遵循 [通用幂等合同](idempotency.md)。

## 异种改造

`POST /dlc/biotech/xenotype`

```json
{"pawn_id":"321","xenotype_def":"Impid","confirm":true,"dry_run":false}
```

成功数据为
`{pawn_id,previous_xenotype_def,requested_xenotype_def,xenotype_def,target_genes,endogenes,xenogenes,applied,dry_run}`。
`requested_xenotype_def`回显请求，`xenotype_def`始终来自实际tracker（包括预演）；两者允许不同。
非预演缺`confirm:true`返回400；`dry_run:true`返回`applied:false`、目标Def要求与当前列表，
不创建`Gene`。真实调用公开`Pawn_GeneTracker.SetXenotype(XenotypeDef)`，
不使用`SetXenotypeDirect`冒充改造。

**这是原生改造语义，不是声明式替换。** 本机1.6实测（Baseliner→Impid，已有内源DarkVision）：

- 目标Def的18个基因全部出现在`endogenes`，`xenogenes`保持为空；
- 已有内源基因（`Skin_Melanin2`、`Hair_DarkBrown`、`DarkVision`）全部保留；
- 反向切回Baseliner不会删除上一次改造追加的内源基因，需要显式逐个删除。

因此返回值总是原生tracker的实际列表；调用方不要假设"切换即精确替换"，
重复应用可能重建基因，建议用同一`Idempotency-Key`保护一次操作。
改造会触发Hat/外观、能力等原生重算；本阶段未逐一列举所有Mod副作用。

## 正常生产链：提取基因→基因包→组装 xenogerm→植入（BT-02，2026-11-08）

新增端点（镜像原生 gizmo/工作，不做直接改基因）：

- `GET /api/v1/dlc/biotech/gene/extractor?map_id=`：提取器电源、Working、容器内 Pawn、
  `ticks_remaining`（原生私有字段读取）、progress。
- `POST /api/v1/dlc/biotech/gene/extract` `{map_id, extractor_id, pawn_id, dry_run}`：
  校验原生 `CanAcceptPawn` 后设置 `SelectedPawn` 并排 `EnterBuilding`（与生长舱 enter 同构）。
- `GET /api/v1/dlc/biotech/gene/assembler?map_id=`：装配器电源、Working、progress、
  archites、`can_be_worked_on_now` 及相连基因库中的 genepack（id/基因）。
- `POST /api/v1/dlc/biotech/gene/assemble` `{map_id, assembler_id, genepack_ids[], xenotype_name?, worker_id?, dry_run}`：
  从**通电相连基因库**解析 genepack，按原生 `NonOverriddenGenes(xenogene:true).Distinct()`
  计算 complexity/metabolism/arc；变更前检查代谢下限、前置基因、最大复杂度、Archogenetics 与地图胶囊数量，
  先解析指定/可用 worker，再调用 `Building_GeneAssembler.Start(packs,arc,name,null)`。
  复用原生 `WorkGiver_CreateXenogerm`：先 `HaulToContainer` 搬胶囊，再 `CreateXenogerm` 工作。
  `applied:true` 表示配方已经启动，`job_submitted` 表示当次是否提交工作；工作暂不可安排不会 Reset 配方。
  dry-run 返回实际 worker、`complexity`、`metabolism`、`archites_required`，不启动或提交工作。
  真实请求另返回 `working`、`job_def`、`job_submitted`、`archites_required_now`；完成须独立读回产物。
- `POST /api/v1/pawn/surgery` 的 `thing_id` 支持两类唯一配料：
  `ImplantXenogerm` 绑定地图上的 `Xenogerm` 到 `Bill_Medical.xenogerm`（与原生
  HealthCardUtility 同字段）；`ImplantEmbryo` 绑定 `HumanEmbryo` 到
  `Bill_Medical.uniqueRequiredIngredients` 并写 `HumanEmbryo.implantTarget`
  （原生 `CreateSurgeryBill(uniqueIngredients)` 同路径）。`Recipe_ImplantEmbryo.AvailableOnNow`
  恒 false，接口按生产者的专门入口校验代理孕母条件，不对普通手术机械套用该门。
- 普通手术先走 `RecipeDef.AvailableNow` 与 `recipe.Worker.AvailableReport`；
  `GetConfirmation` 非空时首次返回 409 `confirmation_required` 与原生文本，需 `confirm:true`。
  医生在创建 `Bill_Medical` 之前校验；建单后原生 job 失败会撤回该请求刚建的账单。
- 增可选 `doctor_id`：用原生 `WorkGiverDef DoBillsMedicalHumanOperation` 的
  `WorkGiver_DoBill.JobOnThing(doctor, patient, forced:true)` 生成并排医疗 DoBill job；
  若失败返回 `job_fail_reason`。原生 `Pawn.CurrentlyUsableForBills` 要求**患者已在床上**
  （`InBed()`），否则失败原因为 "not ready for surgery"——这是原生手术前置，不是 API 可绕过项。

提取/装配与胚胎选择写入口已注册精确 `game_memory` 合同，支持可选 `Idempotency-Key`。
提取/装配的 preview、重放和拒绝不发 operation；实际 applied 各发一次。详见[幂等合同](idempotency.md)。
提取资格仍来自 `CanAcceptPawn`，不沿用上方直接 tracker 变更的自由殖民者限制。

当前限定证据（2026-11-10）：

- 本轮 `Genepack75521`（原生提取 FireSpew）实际入库，与 `Genepack75514`（fixture archite 包）装配；
  原生 `HaulToContainer` 搬入并在 Finish 消耗 **1** 个胶囊，产生 `Xenogerm75599`
  （XenogermReimplanter + FireSpew），产品手术把该物品绑定 `Human66736`，两项基因与账单清空成立。
- 研究未完成、缺前置和缺胶囊均在配方变更前 422；dry 无变更；提取/装配同 key 原文重放、
  非 dry 异体409、单事件通过。代谢/覆盖规则与同版原生源码核对，不宣称所有组合实测。
- `bt-gene-repair-20261110-attempt2.json` 在术后预期昏迷被 guard 停止，**仍为 failed**；
  实际业务终态经 `postop-capture` 留证。修复仅准确患者的麻醉/昏迷例外后，
  `postop-guard` 副本 91949→92049 回归 passed，基因/空账单保留、健康和需求探针仍触发停止。
  这是同一生产物贯通的证据与后续工具回归，不能把原 runner 改称 passed。
- fixture 仅准备对象/资源/研究，缩提取计时至300、加速 DoWork；不调用 Finish 或写植入结果。
  复制存档/隔离 DLL 身份、失败与清理见[修复自审](../references/reviews/2026-11-10-biotech-gene-repair.md)。
- 旧 `an05-bt02-gene-chain.json` failed（88951/89044）和独立 `an05-bt02-implant.json` passed
  （fixture Xenogerm75482）原样保留。旧文字89004/89541不代替现存JSON身份；
  [旧证据清单](../references/reviews/2026-11-10-five-dlc-evidence-manifest.json)与[归并报告](../references/reviews/2026-11-10-five-dlc-reconciliation.md)可追溯。
- 复制中 xenogerm 提取（BT-10，2026-11-10）：`confirm_replicating:true` 后走原生
  `GeneUtility.ExtractXenogerm`；供体 Human75494 提取后 alive、genes 清空、
  `XenogermReplicating/XenogermLossShock`，Genepack 1；未确认仍 422
  （`Reverse/evidence/bt10-replicating-extract-20261111.json`，自审
  [BT-10](../references/reviews/2026-11-10-bt10-replicating-extract.md)）。普通清醒床、完整术后恢复、
  提取后遗症/囚犯提取未验；其它生殖/设施组合见[剩余计划](../exec-plans/active/five-dlc-remaining.md)。

### 囚犯/倒地供体由他人搬入（BT-R15，2026-11-13）

`POST /api/v1/dlc/biotech/gene/extract` 增可选 `carrier_id`。供体是囚犯或倒地时无法自己
执行 `EnterBuilding`（原生 `WorkGiver_EnterBuilding` 明确排除囚犯，倒地 pawn 也没有
工作队列）：接口改为设置 `SelectedPawn` 并排原生 `JobDefOf.CarryToBuilding`
（A=extractor、B=供体、actor=carrier，`JobDriver_CarryToBuilding` → `TryAcceptPawn`），
与生长舱 enter 的 carrier 分支同构。carrier 须同图、存活、未倒地/精神、可操作，且可
reserve/reach extractor 交互格与供体；省略 `carrier_id` 取最近可达殖民者，显式
`carrier_id` 不可达返回 422。响应含 `carry/carrier_id/pawn_down/pawn_prisoner/job_def`
（`CarryToBuilding`）；dry-run 只回上述字段不排队。供体本身不再收到 `EnterBuilding`。

实测 run `bt-r15-20261113-run1`（g35 副本，tick 76787 起，port 8766）：fixture 生成
OutlanderRoughPig 人类 pawn 并原生捕获为囚犯（`GuestStatus.Prisoner`），加原生
`Anesthetic` 到 sedated 阶段（倒地），另备搬运者 Human315。dry-run 回
`carry=true/carrier_id=Human315/job_def=CarryToBuilding/pawn_down=true/pawn_prisoner=true`；
正式 200 `applied=true/submitted=true`。Human315 的 `CarryToBuilding` 实际抱起
Human75478（`carried_thing` 读回），tick 77,024 容器内 Human75478、
`working=true/ticks_remaining=29998`；tick 107,034 原生 `Finish`：
`Genepack75939`（RobustDigestion、Hands_Pig）落在 (187,129)，供体被弹出且 alive，
`XenogermLossShock/XenogermReplicating`（`/pawns/details` 同样读回），extractor
`working=false/contained=[]/selected_pawn_id=null`。供体 job 记录从未出现
`EnterBuilding`。证据
`tools/cua/.runtime/bt-r15-20261113-run1/evidence/bt-r15.json`（`phase=passed`）；
runner `tests/parallel/biotech/bt_r15_extractor_carrier.py`，fixture 命令
`prepare_extractor_carrier`/`observe_extractor`。

边界/披露：fixture 用原生捕获+原生 Anesthetic 构造囚犯倒地供体（生成 pawn 属测试前置）；
本轮只验一名囚犯供体与显式 carrier，未测无 carrier_id 时原生
`WorkGiver_CarryToGeneExtractor` 自动分配、倒地殖民者变体、断电/死亡/取消、多提取器与
复制中供体致死路径。

## 错误与DLC门

| 情况 | 状态码/`error_code` |
| --- | --- |
| 缺`pawn_id`/`gene_def`/`xenotype_def` | 400 `bad_request` |
| 未知pawn | 404 `not_found` |
| 未知基因/异种Def | 404 `not_found` |
| 同Def存在于另一层（增）/该层有多条同Def（删） | 409 `conflict` |
| 动物/囚犯/无tracker | 422 `invalid_target` |
| Biotech未启用 | 422 `dlc_missing` |

无Game时返回409。DLC门在写入前判定；DLC关闭的加载局不触碰基因数据。

## 未覆盖

- 未在1.5或任意Mod组合下验证；DLC-off矩阵见验收记录，不能推成全部DLC组合已测。
- 不允许跨层"移动"基因、不允许一次请求改多个基因；批量操作未设计。
- 未承诺可用性/兼容性判定（基因冲突由原生覆盖规则处理，接口只读回结果）。

### 装配完成实测（BT-13b，2026-11-10）

- 用 `prepare_gene_repair` 的供电 assembler/bank，加入 Hemogenic 包，选择
  Hemogenic+LongjumpLegs；dry complexity 2/metabolism -1/archites 0；正式
  `CreateXenogerm` job。
- 只缩短 assembler work timer（fixture 披露），tick 77173 产出
  `Xenogerm75511`（Hemogenic+LongjumpLegs），assembler idle
  （`Reverse/evidence/bt13b-gene-assembler-20261111.json`，自审
  [BT-13b](../references/reviews/2026-11-10-bt13b-gene-assembler.md)）。
- 该批只测非archite两基因；后续已补单/双胶囊的正常搬运、精确消耗与产物、单胶囊产物植入、断电暂停/恢复、取消与已入胶囊归还，见下述证据。复杂混合archite/多worker等变体未验。

取消：`POST /api/v1/dlc/biotech/gene/assemble/cancel`
`{map_id,assembler_id,dry_run}`，调用原生 `CancelXenogerm` 同一 `Reset`；
非 working 409。实测 working true→false、progress 0、genepacks 保留，再次
取消 409（`Reverse/evidence/bt13b-assembly-cancel-20261111.json`，自审
[取消](../references/reviews/2026-11-10-bt13b-cancel.md)）。后续`bt13b-cancel-delivered-20261111.json`已补两胶囊搬入后取消、装配器2→0/地图0→2。

后续证据：单胶囊/植入见[archite](../references/reviews/2026-11-10-bt13b-archite.md)，双胶囊见[archite2](../references/reviews/2026-11-10-bt13b-archite2.md)，断电暂停/恢复见[电力](../references/reviews/2026-11-10-bt13b-gene-power.md)；原JSON的失败与前置披露保留。

## 正常选择与家庭遗传边界（2026-11-11 核对）

原生 `Dialog_CreateXenogerm` 选择整份 genepack，调用
`Building_GeneAssembler.Start(selectedGenepacks, archites, name, icon)`。
`gene/assemble` 的 `genepack_ids[]` 表达同一生产选择；不能拆包挑其中单个基因。
外观图标、模板保存/加载尚无对应生产参数，不把这些界面功能写成任意基因编辑能力。
家庭生殖走母体取卵、指定父体受精、胚胎植入及生育策略；后代内源基因由
`PregnancyUtility.GetInheritedGenes` 的原生遗传规则与随机过程决定。
原版正常玩法没有“任意选择后代每个基因”的菜单；直接 gene 增删属于调试/直接变更。
本次为源码核对，已有取卵/受精/装配/植入证据沿用，未伪称新增遗传实测。
