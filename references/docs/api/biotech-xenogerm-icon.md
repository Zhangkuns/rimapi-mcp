# Biotech 异种胚芽图标选择（BT-R05）

2026-11-12 新增：`POST /dlc/biotech/gene/assemble` 的 `icon_def` 可选字段，
解析原生 `XenotypeIconDef` 并传给 `Building_GeneAssembler.Start(..., iconDef)`；
与 `Dialog_CreateXenogerm` 的图标选择一致。

## 请求/读回

```json
{"assembler_id":"GeneAssembler75478","genepack_ids":["Genepack75486"],
 "xenotype_name":"IconTest","icon_def":"Basic","worker_id":"Human321"}
```

- 未给 icon 时保持 `null`（旧行为）；未知 icon 400。
- dry_run 返回 `icon_def` 计划；正式 Start 返回 `icon_def_after`（原生私有
  `iconDef` 字段的 defName）、`job_def:CreateXenogerm`、`applied`。
- 取消仍用 `/gene/assemble/cancel`；合同 `command/pawn_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成通电 assembler/bank 和 `Hair_SnowWhite` 单基因包；dry 返回
`icon_def:Basic`；正式 Start applied/job CreateXenogerm/`icon_def_after:Basic`；
观察 assembler `icon_def:Basic`；取消后 working false 且地图无新 Xenogerm；
未知 icon 400。
证据 `tools/cua/.runtime/bt-r05-20261112/evidence/bt-r05.json`，runner
`tests/bt_r05_xenogerm_icon.py`，fixture `prepare_xenogerm_icon`/`observe_xenogerm_icon`。

- 未测：完成成品的图标渲染/继承、非 Basic 图标 Def、assembler 中已有成品的编辑；
  本行只补正常 Start 图标选择与取消无产物。
