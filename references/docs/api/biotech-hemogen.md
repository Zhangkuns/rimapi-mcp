# Biotech 血原质自动摄入阈值与血包策略（BT-R07）

2026-11-12 新增：读写 `Gene_Hemogen` 的原生资源策略。
`targetValue`/`SetTargetValuePct` 是 `GeneGizmo_Resource` 的同一个阈值，
`hemogenPacksAllowed` 是 Hemogen gizmo 的允许血包开关。

## 读取

`GET /api/v1/dlc/biotech/life?pawn_id=Human321` 新增：

```json
{"has_hemogen_gene":true,"hemogen_value":0.4,"hemogen_max":1.0,
 "hemogen_percent":0.4,"hemogen_target_percent":0.5,
 "hemogen_packs_allowed":true}
```

`hemogen_value`/`hemogen_max` 是原生 `Gene_Resource.Value/Max` 的内部量纲（0..1）；
游戏 UI 的 `ValueForDisplay` 是 `value*100`（例如 0.4 → 40 / 100）。

## 设置

`POST /api/v1/dlc/biotech/life/hemogen/policy`

```json
{"pawn_id":"Human321","target_percent":0.25,"packs_allowed":false,"dry_run":true}
```

- `target_percent` 必须在 0..1；写 `gene.SetTargetValuePct`。原生
  `Gene_Hemogen.SetTargetValuePct` 会把 target 夹到 `Max - MaxLevelOffset`
  （Hemogenic 的 `MaxLevelOffset=0.1`，故有效上限 0.9），以保证资源能超过阈值。
  响应 `target_percent` 是夹取后的有效值；请求值被夹取时 `target_clamped=true`
  （`1.0` → 有效 `0.9`，仍为 `applied=true`）。
- `packs_allowed` 写 `hemogenPacksAllowed`；至少提供一个字段。
- 返回 `before_*`、`target_percent`、`target_clamped`、`packs_allowed`、当前
  Value/Max 和 `applied`；dry_run 不写。要求原生 `Gene_Hemogen`，否则 422。
- 合同：POST `policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 使用或添加原生 `Hemogenic` gene，初始 target 0.5/packs true；GET 读回一致；
dry 保持；正式 `target=0.25, packs=false` 后 GET 一致；`target=1.5` 400；
再设 target=0/packs=true 后读回 0/true。
证据 `tools/cua/.runtime/bt-r07-20261112/evidence/bt-r07.json`，runner
`tests/bt_r07_hemogen.py`，fixture `prepare_hemogen`/`observe_hemogen`。

2026-11-13 自动摄入遵从（`bt-r07-20261113-run1`，同一 g35 副本 tick 76787 起，
port 8766；RIMAPI.dll `dad1b3ba…`，BtFixture.dll `08bb7c67…`）：
fixture 在非 bloodfeeder 成年殖民者 Human315 上加原生 `Hemogenic` endogene，
把原生 hemogen 置 0.05 并在旁边生成 1 个玩家 `HemogenPack`；policy 正式
`target=0.75,packs=true` 后原生 think tree 取 `JobGiver_GetHemogen`：

- tick 77448 原生 `JobDefOf.Ingest` 目标为该血包；tick 78366
  `IngestionOutcomeDoer_OffsetHemogen` 完成，value 0.05→0.24947（+0.1995，
  一份血包 +0.2），该血包销毁、地图血包数 2→1；target 0.75/packs true 保持，
  GET 读回一致。全程无 `/hemogen/feed` 调用。
- 关闭 packs（`packs_allowed=false`）、value 重置 0.05 后再推进 2500 tick：
  无 Ingest 目标该血包、无消费、血包仍在，value 仅自然流失到 0.04917
  （`resourceLossPerDay 0.02`）；期间 saw Clean/Skygaze/Wait_Wander 多次重新决策。
- 把 value 置 0.6、target 0.3、packs true 后推进 2500 tick：阈值以上不取血包，
  value 0.59916，血包仍在。
- `target_percent=1.0` → `applied=true`、有效 `target_percent=0.9`、
  `target_clamped=true`；GET 读回 0.9；`1.5` 仍 400。

证据 `tools/cua/.runtime/bt-r07-20261113-run1/evidence/bt-r07.json`；runner
`tests/parallel/biotech/bt_r07_hemogen_auto.py`；fixture
`prepare_hemogen_ingest`/`stage_hemogen_pack`/`set_hemogen_value`/
`observe_hemogen_ingest`。收口 `desktop.py stop` 报告
source_save/formal_dll/global_prefs unchanged。

- 未测：囚犯供血策略（S-05）、血包提取、其它 hemogen 增减来源、存档读回。
