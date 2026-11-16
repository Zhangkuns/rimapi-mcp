# Royalty 应用科技蓝图（RY-R09）

2026-11-12 新增：`GET /dlc/royalty/techprint` 列出地图上的原生科技蓝图及
研究台；`POST /dlc/royalty/techprint/apply` 构造与
`CompTechprint.CompFloatMenuOptions` 相同的 `ApplyTechprint` job：
A=研究台、B=蓝图、C=研究台位置。600 tick 工作、消耗蓝图和
`ResearchManager.ApplyTechprint` 均由原生 JobDriver 执行。

## 请求

```json
{"map_id":0,"pawn_id":"Human321","techprint_id":"Techprint_CataphractArmor75479",
 "bench_id":"SimpleResearchBench75478","dry_run":true}
```

- 要求自由殖民者、Research/Intellectual 可用、蓝图为未完成项目；
  未给 bench 时用最近可预约研究台。
- 返回 `project_def/techprints_applied_before/techprints_required/job_def/
  submitted/pawn_job/applied`；dry_run 只验证。
- 合同：GET observe；POST command/pawn_ai/validate/game_memory。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 SimpleResearchBench 与 `Techprint_CataphractArmor`（项目
CataphractArmor，0/2）。dry 保持；正式后 job `ApplyTechprint`；`advance_fast`
1500 tick 后蓝图消失、`techprints_applied` 0→1、项目未完成，pawn 回 Skygaze。
证据 `tools/cua/.runtime/ry-r09-20261112/evidence/ry-r09.json`，runner
`tests/ry_r09_techprint.py`，fixture `prepare_techprint`/`observe_techprint`。

- 未测：后续第二张蓝图完成项目、研究台被毁/禁用研究拒绝、蓝图在地图外/
  禁用的拒绝；本行以原生 ApplyTechprint 消费与计数为准。
