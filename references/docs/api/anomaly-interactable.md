# Anomaly 原生交互（AN-06）

读取并提交一个 `CompInteractable` 组件的原生玩家交互：方尖碑触发/碎片停用、金立方停用、
幽魂脊柱等。写入口不绕过原生资格、材料与 job 形状。

## 读取

`GET /api/v1/dlc/anomaly/interactable?map_id=&thing_id=&pawn_id=`

返回该物体的全部 `CompInteractable`：

```json
{"thing_id":75487,"def_name":"WarpedObelisk_Mutator",
 "interactables":[
   {"interactable_key":"Interactor","type":"CompObeliskTriggerInteractor","label":"Trigger mutation",
    "active":false,"on_cooldown":false,"ticks_to_activate":180,"progress":0.0,"can_interact":true},
   {"interactable_key":"Deactivation","type":"CompObeliskDeactivationInteractor",
    "label":"Deactivate obelisk","can_interact":true,"interact_reason":null}],
 "special":{"obelisk":{"study_level":2,"study_finished":true,"activated":false,"deactivated":false}}}
```

金立方另有 `deactivatable`、受影响地图/商队人物与地图 Shard 数。
`CompAnalyzable` 另有 `analysis`（`analysis_id`/`required`/`times_done`/`satisfied`/
`can_study_in_place`/`allow_repeat`/`destroyed_on_analyzed`）；怪异尸体另有
`unnatural_corpse`（`haunted_pawn_id`/`can_destroy_via_research`/
`haunted_has_corpse_torment`/Shard 数）。`native_gate` 是原生菜单在
`CanInteract` 之外执行的门禁；当前非空表示该交互尚不可用，原因同时写入
`interact_reason`。

## 写入

`POST /api/v1/dlc/anomaly/interactable`

```json
{"thing_id":75487,"pawn_id":"321","interactable_key":"Deactivation","confirm":false,"dry_run":true}
```

- 同一物体多个可交互组件时必须给 `interactable_key`（省略 400 并返回 `interactable_keys`）。
- 目标/资格：玩家殖民者、无 lord、可操作；`targetingParameters.CanTarget` 与
  `CompInteractable.CanInteract(pawn)`。被其它人物预留时 409 并返回 `reserver_id`
  （原生 `TryTakeOrderedJob` 会因 `TryMakePreToilReservations` 静默失败）。
- 金立方：`confirm` 前返回 409 与 `would_affect_map_pawns`/`would_affect_caravan_pawns`；
  确认后按原生回调建 `InteractThing` + 1 Shard。
- 其它组件调用 `CompInteractable.OrderForceTarget`。不同子类会排不同 job：
  方尖碑停用是 `InteractThing` + Shard 队列/count；`CompAnalyzable` 排
  **`AnalyzeItem`**（A=样本、B=研究台、C=台格或无效格）。因此适配按“新排入且
  目标为该物体”的 job 识别提交，不再假定 job def 必须是 `InteractThing`，
  避免样本分析已启动却返回 409（AN-R03）。
- 怪异尸体（`CompDisableUnnaturalCorpse`）的研究门禁只在原生 gizmo/右键包装层检查；
  适配在 `CanInteract` 之后补同一门禁：未解锁返回 422 且尸体保留。尸体
  `PostMake` 默认 forbidden，正常右键也要求先取消禁用；调用方先用
  `POST /things/set-forbidden`，再提交交互（AN-R06）。
- 返回 `applied`、`submitted`、`job_def`、`queued_jobs`、before/after 状态；
  job 未被接受时 409。
- 精确合同：`POST` 为 `command/pawn_ai/validate/game_memory`，`GET` 为只读 observe。

## 证据与边界

- 2026-11-10 自审：`WarpedObelisk_Mutator` 双 Shard 停用后 `deactivated=true`、Shard 2→0；
  停用后触发 422；`WarpedObelisk_Duplicator` 触发后地图人物 78→79
  （`Reverse/evidence/an06a-obelisk-20261110.json`）。
- RevenantSpine：`CompRevenantSpine` 交互 → 原生 `SmashThing` → 脊柱销毁并留下 1 Shard
  （`Reverse/evidence/an06b-spine-an07b-20261110.json`）。
- 触发结果是原生随机效果，本轮只验证复制者生成一个复制人物；金立方实际停用（受影响人物
  狂暴/驱逐）与 Nociosphere 目的地尚未运行。`confirm:true` 后的金立方后果不可撤销。
- 2026-11-12 AN-R03/AN-R06（隔离 `g35_deathresting` 副本，tick 76787，五 DLC）：
  GrayFleshSample 因已下单 `AnalyzeItem` 返回 200/`applied:true`/`job_def:AnalyzeItem`，
  推进后 `AnalysisManager.times_done` 0→1 且样本被原生销毁（原误报 409 已回归）。
  怪异尸体未解锁时 GET 返回 `can_interact:false` + `native_gate`，POST 422 且尸体/
  Shard 不变；fixture 以 DEV 研究解锁作 debug 前置后（已披露），取消禁用→正常
  `InteractThing` 完成，尸体销毁、Shard +1、CorpseTorment 移除、tracker 注销。
  证据 `tools/cua/.runtime/fix3-20261112/evidence/fix3-first-three.json`（本地运行产物），
  runner `tests/fix3_first_three.py`，fixture 动作 `prepare_analyzable`/
  `prepare_unnatural_corpse`/`unlock_unnatural_corpse`/`observe_anomaly_item`。

## 2026-11-13 补充：研究台分析分支（AN-R03 V）

- 源码事实：1.6.4871 的三个 Anomaly `CompProperties_CompAnalyzableBiosignature`
  全部 `canStudyInPlace=true`；唯一 `false` 的 shipped `CompAnalyzable` 是
  Biotech SignalChip 家族（`CompProperties_CompAnalyzableUnlockResearch`，
  `analysisID 92349061`，`requiresMechanitor`），与 Anomaly 样本共用
  `CompAnalyzable`/`JobDriver_AnalyzeItem` 基类。
- 隔离 `g35_deathresting` 副本（tick 76787）：fixture 生成原生
  `SimpleResearchBench` + `SignalChip`，给一名殖民者装原生 `MechlinkImplant`
  （正常取得为 mechlink 物品，已披露）。
- 非 mechanitor POST 422 `requires mechanitor`，样本/进度不变；dry-run 200
  `applied:false`；正式 POST 200/`applied:true`/`job_def:AnalyzeItem`。原生 job
  形状实测 A=样本、B=研究台、C=台格 (160,105)；推进后
  `AnalysisManager.times_done 0→1/required 1/satisfied:true`，
  `StandardMechtech.analyzed_completed 0→1`（`requiredAnalyzed` 门禁满足，项目仍受
  自身前置限制），chip 未销毁并落在台格；重复 POST 422 `already analyzed`。
- 证据 `tools/cua/.runtime/an-final-20261113g/evidence/an-r03-bench-analysis.json`，
  runner `tests/parallel/anomaly/an_r03_bench_analysis.py`，fixture 动作
  `prepare_bench_analysis`/`observe_bench_analysis`。
- 未测：其它 bench-required 子类、MultiAnalyzer 设施、`analysisRequiredRange>1`、
  `allowRepeatAnalysis=true` 的 Def；Anomaly 原地样本分析（G 行 GrayFleshSample）
  保留原证据，不重跑。
