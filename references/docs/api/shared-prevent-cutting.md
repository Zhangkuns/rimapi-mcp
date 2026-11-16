# Shared 可保护树木防砍开关（S-09）

2026-11-12 新增：`GET/POST /api/v1/plant/cut-protection` 读写原生
`CompPlantPreventCutting.PreventCutting`，并读回 `CutPlant/HarvestPlant`
designation 与当前 `can_cut`。

## 设置

```json
{"thing_id":"Plant_TreeAnima75478","prevent_cutting":true,"dry_run":true}
```

- 只接受带原生 `CompPlantPreventCutting` 的树；未知/非树 404/422。
- 设置为 true 时由原生 setter 移除该树的 CutPlant/HarvestPlant designation。
- 返回 `prevent_cutting/cut_designated/harvest_designated/can_cut/applied`。
- 合同：GET observe；POST policy/game_ai/validate/game_memory。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 `Plant_TreeAnima` 并加原生 CutPlant designation；初始
prevent true/cut true。再次 true 清除 designation；false 后 can_cut true；
重新加 designation 后 true 再次清除；未知树 404。
证据 `tools/cua/.runtime/s09-20261112/evidence/s09.json`，runner
`tests/s09_prevent_cutting.py`，fixture `prepare_cut_tree`/`designate_cut`。

- 未重跑殖民者实际砍伐工作；本行以原生组件/designation 和资格读回为准。
