# Biotech 基因装配模板保存/加载（BT-R04）

2026-11-12 新增：`GET /dlc/biotech/gene/template` 列出
`Find.CustomXenogermDatabase` 中的原生模板；`POST .../template/save` 调用
`CustomXenogermUtility.SaveXenogermTemplate`；`POST .../template/load` 用
`GetMatchingGenepacks` 把模板解析为当前 assembler 连接基因库中的准确包 ID。

## 保存

```json
{"map_id":0,"assembler_id":"GeneAssembler75478","name":"RIMAPI Template",
 "icon_def":"Basic","genepack_ids":["Genepack75486"],"dry_run":true}
```

- 包必须来自该 assembler 已连接、通电的基因库；未知包 404；模板名必填。
- 返回 `name/icon_def/genepack_ids/gene_set_count/template_index/applied`；
  dry_run 不写。
- 合同：GET observe；save direct/immediate/validate/game_memory；
  load observe。

## 加载（解析）

```json
{"map_id":0,"assembler_id":"GeneAssembler75478","template_index":0}
```

返回模板 `name/icon_def/gene_set_count`、`matched_genepack_ids` 和
`all_matched`；调用方随后用既有 `/gene/assemble` 提交准确包。加载本身不改
assembler 状态。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 通电 assembler/bank + `Hair_SnowWhite` 单基因包；dry 保持；正式保存
template_index0；GET 模板含 genes/complexity/metabolism/archites；load 精确
匹配 `Genepack75486/all_matched:true`；未知 index 404。
证据 `tools/cua/.runtime/bt-r04-20261112/evidence/bt-r04.json`，runner
`tests/bt_r04_gene_template.py`，fixture `prepare_xenogerm_icon`。

- 未测：模板持久化跨存档重载、重复名/覆盖、图标渲染、模板在多 assembler 间
  部分匹配；本行以原生模板数据库和匹配函数为准。
