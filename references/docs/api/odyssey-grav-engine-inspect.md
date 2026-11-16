# Odyssey 重力引擎初次检查（OD-R11，代表验收）

2026-11-12 代表验收：不新增业务端点，使用既有
`POST /api/v1/pawn/job` 派原生 `InspectGravEngine` job（与引擎 gizmo
`CommandInspectGravEngine` 的 `JobMaker.MakeJob` 相同），由
`JobDriver_InspectGravEngine` 走 240 tick `WaitWith` 后调用
`Building_GravEngine.Inspect()`。

## 请求

```json
{"pawn_id":321,"job_def":"InspectGravEngine","target_thing_id":75478}
```

`pawn_id`/`target_thing_id` 为数字 thingIDNumber（既有 job 合同）。

## 正常结果（读回）

fixture 在 `g35_deathresting` 副本（tick 76787，五 DLC）生成一台玩家
`GravEngine`，并把全局 `gravEngineInspected` 置 false（仅当副本原本为 true；
准备披露），派一名空闲殖民者执行上述 job。推进后读回：

- `Find.ResearchManager.gravEngineInspected` false→true；
- 引擎派系 `Faction.def.defName` `PlayerColony`（`Inspect()` 对非玩家引擎
  调用 `SetFaction(Faction.OfPlayer)`）；
- 原生 `QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.GravEngine)`
  生成 1 个任务（`quest_count:1`）；
- job 末尾的原生 `Dialog_NodeTree` 由 `POST /ui/window/close` 关闭
  （默认关闭所有 force-pause 窗口），随后 GET `/quests` 可读任务。

证据 `tools/cua/.runtime/od-r11-20261112/evidence/od-r11.json`，runner
`tests/od_r11_inspect_grav_engine.py`，fixture `prepare_grav_engine`/
`observe_grav_engine`。

- 未测：已有起降样本上的重复检查（原生 `Inspect` 直接返回）、任务接受/完成、
  非玩家引擎的敌对/未探索变体、命名对话框阈值。
