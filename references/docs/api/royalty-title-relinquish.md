# Royalty 放弃头衔（RY-R02）

2026-11-12 新增：`POST /dlc/royalty/title/relinquish` 调用原生
`pawn.royalty.SetTitle(faction, null, grantRewards:false)`，与
`CharacterCardUtility` 的放弃确认同一业务语义；`GET /dlc/royalty/title`
读回头衔/权利。

## 请求

```json
{"pawn_id":"Human321","faction_def":"Empire","confirm":true,"dry_run":true}
```

- 要求 spawned free colonist + royalty tracker，且该阵营当前有头衔；
  无头衔 409。
- 非 dry 必须 `confirm:true`；返回 `title_before/seniority/favor/` 和
  `title_after/favor_after/applied`。`faction_def` 省略时用现有头衔阵营。
- 合同：POST `command/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 给 Human321 psylink 后，测试用正常标题 API 授予 Empire `Freeholder`
（准备，非本行结果）；GET 读回 Freeholder；dry relinquish 保持；正式
`confirm:true` 后 `title_after:null`，GET 无头衔/权利/requirements。
证据 `tools/cua/.runtime/ry-r02-20261112/evidence/ry-r02.json`，runner
`tests/ry_r02_relinquish.py`。

- 未测：真实授予仪式/信使/奖励、许可撤销的逐步 UI、不同头衔等级/多阵营、
  放弃后的负面思想与后续任务；本行以原生 `SetTitle(null)` 和读回为准。
