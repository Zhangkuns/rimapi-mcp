# Biotech 基因提取器取消（BT-R02）

2026-11-12 新增：取消已选供体（未进入）和进行中的提取，分别对应原生
`Building_GeneExtractor` 的 `CommandCancelLoad` 与 `CommandCancelExtraction`
gizmo。提取开始/完成链仍走既有 `/gene/extract`。

## 状态读取

`GET /api/v1/dlc/biotech/gene/extractor?map_id=0` 的
`working/ticks_remaining/selected_pawn_id/contained`。

## 取消

`POST /api/v1/dlc/biotech/gene/extract/cancel`

```json
{"map_id":0,"extractor_id":"GeneExtractor75480","dry_run":true}
```

- `working:true` → 调用私有 `Building_GeneExtractor.Cancel()`（清 selected/
  startTick/sustainer 并 `TryDropAll`）。
- 仅 `selected_pawn_id` 已设 → 复制原生 Cancel-Load：结束 `EnterBuilding`
  job、`TryDropAll`、清 `selectedPawn/startTick/sustainerWorking`。
- 无 selected/Working/内容 → 409；返回 `cancel_kind`（load/extraction）、
  `working_before`、取消后 `selected/contained/working`、`applied`。
- 合同：POST `command/pawn_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 的 `prepare_gene_repair` 生成通电 `GeneExtractor` 与单基因供体。
开始提取后 `selected_pawn_id=Human75494`；dry 保持；正式取消（load）后
selected null、供体 job 从 EnterBuilding 变为 Clean；再次开始并
`advance_fast` 后 `working:true/contained=[Human75494]`；正式取消 extraction 后
`working:false/contained=[]/selected null`。
证据 `tools/cua/.runtime/bt-r02-20261112/evidence/bt-r02.json`，runner
`tests/bt_r02_extract_cancel.py`。

- 未测：取消后再次完整提取到 Genepack、断电/死亡/离开地图的自动取消、
  多提取器并发、原生警示框文本；`ticks_remaining` 在取消后仍保留旧值，
  故测试不把该字段当进度归零证据。
