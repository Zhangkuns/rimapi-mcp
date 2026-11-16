# Biotech 异种基因再植入（BT-R19）

交付：原生 `CompAbilityEffect_ReimplantXenogerm → GeneUtility.GiveReimplantJob`
的“他人吸收倒地/受控供体异种基因”路径。实现：
`Services/System/GeneControl/GeneProductionService.ReimplantXenogerm`、
`Controllers/System/GeneProductionController.cs`、
`Models/GeneProductionDtos.cs`。只处理本地图 pawn；不生成/不直接改基因。

## 端点

`POST /api/v1/dlc/biotech/gene/reimplant`

```json
{"map_id":0,"donor_id":"Human75478","absorber_id":"Human315","confirm":false,"dry_run":true}
```

- 供体资格来自原生 `GeneUtility.CanAbsorbXenogerm(donor)`：有 actives 的
  `XenogermReimplanter` 基因、处于倒地或安全囚犯状态、且至少有一个 xenogene。
- 受体资格：是玩家控制的殖民者、有 `genes`、与供体不同 xenotype
  （`GeneUtility.SameXenotype` 为假）、`CompAbilityEffect_ReimplantXenogerm
  .PawnIdeoCanAcceptReimplant` 允许、可触及供体。
- 原生致死警告：供体带 `XenogermReplicating` 时
  `GeneUtility.PawnWouldDieFromReimplanting(donor)` 为真；未带 `confirm:true`
  返回 409，`data.confirmation_required/lethal_warning/warning` 给出原生提示文本，
  不排 job。`confirm:true` 才继续。
- 正式提交调用原生 `GeneUtility.GiveReimplantJob(absorber, donor)`
  （`JobDefOf.AbsorbXenogerm`，A=供体），返回 `submitted/applied/job_def/
  submitted_job` 与双方当前 xenotype/xenogenes。`applied` 只表示 job 已入队/开始，
  终态须独立读回。
- 原生终态语义（必须按此验收）：`ReimplantXenogerm` **复制**供体 xenogenes 到受体
  （供体保留自身基因），受体 xenotype 变为供体 xenotype 并获
  `XenogerminationComa`；供体获 `XenogermLossShock` + `XenogermReplicating`。
  （若供体当时带 replicating，则供体 xenotype 先重置为 Baseliner。）

## 实测（BT-R19，2026-11-13）

run `bt-r19-20261113-run3`（独立 Xvfb/profile，g35 副本 tick 76787 起，port 8765）。
fixture 生成殖民者供体，原生 `SetXenotype(Sanguophage)`（自带
`XenogermReimplanter` 与 30 个 xenogenes），用原生 `Anesthetic` 使其倒地；受体为
Human315（baseliner）。

- 临时给供体加 `XenogermReplicating`：dry `gene/reimplant` 返回 409，
  `confirmation_required=true/lethal_warning=true`，`warning` 为原生
  “WarningPawnWillDieFromReimplanting” 文本；基因未变。移除该 hediff 后 dry 200，
  `lethal_warning=false/confirmation_required=false/job_def=AbsorbXenogerm`。
- 正式提交 `applied=true/submitted=true`，`donor_xenotype=Sanguophage`。
- tick 76,865 原生 `AbsorbXenogerm` job 完成（采样含
  `absorber_absorb_job_seen=true`）：受体 xenotype `Sanguophage`、30 个 xenogenes
  与供体一致，获 `XenogerminationComa`（倒地）；供体保留 30 个 xenogenes，获
  `XenogermLossShock/XenogermReplicating`。
- `GET /dlc/biotech/gene?pawn_id=` 双方读回与 fixture 原生行一致；
  `GET /pawns/details` 读回受体 `XenogerminationComa`、供体
  `XenogermReplicating`。最终 `game/state.is_paused=true`、invulnerability 开启。
- 证据 `tools/cua/.runtime/bt-r19-20261113-run3/evidence/bt-r19-run3.json`
  （`phase=passed`）；runner `tests/parallel/biotech/bt_r19_xenogerm_absorb.py`；
  fixture 命令 `prepare_reimplant_pair/observe_reimplant/reimplant_set_replicating`。

边界/披露：fixture 生成 Sanguophage 供体并临时加/删 replicating hediff 仅用于验证
警告合同；未测囚犯供体、confirm=true 的致死供体实际结果、同 xenotype/ideoligion
拒绝、多人连续吸收、存档读回。共享 `Core/Ai/AiEndpointContracts.cs` 的本路由合同行
由协调者登记。

相关：[基因](biotech-genes.md)、[基因提取器取消](biotech-gene-extractor-cancel.md)。
