# Anomaly 巨石读取与一次性激活（G-36）

已交付：读取 16 项（level0 8 + level1 8）与一次性激活 17 项；
激活的故障注入/守卫证据单独归档（下方“证据来源”）。
证据：`Reverse/evidence/dlc-G-36-level0.json`、`dlc-G-36-level1.json`、
`dlc-G-36-runtime.json`、`dlc-G-36-activate.json`、`dlc-G-36-activate-faults.json`、
`dlc-G-36-activate-guards.json`。

## 端点

`GET /api/v1/dlc/anomaly/monolith`

```json
{"monolith_spawned":true,"level":1,"highest_level_reached":1,"anomaly_knowledge":0.0,
 "threat_tier":1,"monolith_thing_id":40083,"can_activate":true,"cannot_reason":""}
```

- 数据来自 `Find.Anomaly`（`GameComponent_Anomaly`）与 `Building_VoidMonolith.CanActivate`。
- `threat_tier` 取与该等级匹配的 `MonolithLevelDef.anomalyThreatTier`，无匹配时为 0。
- 未生成巨石时 `monolith_spawned:false`、`monolith_thing_id:0`、`can_activate:false`。
- Anomaly 未启用 422 `dlc_missing`。

## 激活（一次性、不可逆）

`POST /api/v1/dlc/anomaly/monolith/activate`

```json
{"pawn_id":"321","confirm":true,"dry_run":false}
```

成功数据：`{pawn_id,monolith_thing_id,level_before,highest_before,irreversible,changed,
applied,dry_run,level_after,highest_after,knowledge_after,threat_tier_after,
can_activate_after,readback_ok,outcome}`。

- 原生调用是公开的 `Building_VoidMonolith.Activate(pawn)`；实测 level 0→1、highest 0→1、
  threat tier 0→1、knowledge 不变（`Reverse/evidence/dlc-G-36-activate.json`，17 项）。
- **不可逆**：没有回滚，也不自动重试；`confirm:true` 必填，`dry_run` 只投影。
- 原生门禁：`CanActivate` 为 false 时 409，错误里带原生 reason；此外在副作用前调用
  原生 `ValidateTarget(new LocalTargetInfo(pawn), false)`（与 `OrderForceTarget` 同一门禁：
  倒地、精神状态、`CanCasuallyInteractNow`、`CanReach`），不满足 422，且不创建 job、不弹 UI。实测 1 级后
  `can_activate:false`，原因为“Must discover 6 more basic monolith entities”，
  因此 2 级需要先完成原生发现进度，不是本端点能绕过的。
- 结果按观察分类：`activated`（200 `applied:true`）、`not_activated`（500 `applied:false`）、
  `unknown`（读回失败 500，`applied` **缺省**而非 false）、`partial_or_contradictory`（500，缺省）；
  只有 200 的 `activated` 发事件；原生抛异常但等级确实上升时仍是 500 且 `applied:true`，
  并带 `native_error`。
- 未知 pawn 404；非自由殖民者/动物 422；无 Game 409；Anomaly 未启用 422 `dlc_missing`。
- 同 key 重放返回原正文且不二次激活；存读档后等级保持。

## 历史：首次升级取证

早期在副本 `ai_g36_level1` 中直接调用 `Activate(pawn)` 取证（level 0→1），当时未暴露端点；
现在端点已交付，证据保留在 `dlc-G-36-runtime.json` 与 `dlc-G-36-activate.json`。

### 证据来源

| 文件 | 内容 |
| --- | --- |
| `dlc-G-36-activate.json` | 17 项正常路径（dry-run、确认门、坏 pawn、激活、原生 409、重放、事件、存读档） |
| `dlc-G-36-activate-faults.json` | 注入：静默不升级、前抛、后抛、读回失败；含计数器与同 key 重放 |
| `dlc-G-36-activate-guards.json` | 新 POST 的无 Game 409 与 DLC-off 422（两张 POST 分别实测） |

## 高等级/终局读取（G-43）

`GET /api/v1/dlc/anomaly/monolith` 在原有字段上追加（2026-11-08，未删旧键）：

```json
{"level":3,"level_def":"VoidAwakened","advance_through_activation":false,
 "post_endgame":false,"questline_ended":false,"void_awakening_active":true,
 "monolith_study_completed":false,
 "next_level":{"def_name":"VoidAwakened","level":3,"threat_tier":2,
   "required_category":"Advanced","required_count":12,"discovered_count":12,
   "discovery_ready":true,"blocking_conditions":[]},
 "monolith_quest":{"def_name":"EndGame_VoidMonolith","state":"Ongoing","id":1},
 "void_structures":{"count":2,"activated_count":0,"structures":[
   {"thing_id":71095,"thing_id_string":"VoidStructure71095","def_name":"VoidStructure",
    "position":{"x":38,"y":0,"z":174},"activated":false,"progress":0.0,
    "on_cooldown":false,"faction_id":"Faction_10",
    "quest_tags":["Quest2.stageStructure.0","Quest2.stageStructure.0.0"]}]},
 "metal_hell":{"present":false,"map_id":null,"void_nodes":[],"pawn_count":0,"pawn_ids":[]}}
```

- `next_level` 来自 `GameComponent_Anomaly.NextLevelDef`；`required_category/required_count/
  discovered_count` 来自 `Find.EntityCodex.DiscoveredCount`。2 级需要 7 个 Basic，
  3 级需要 12 个 Advanced；`can_activate` 仍由原生 `CanActivate` 决定。
- `blocking_conditions` 列出下一等级 `unreachableDuringConditions` 中当前活动的
  `GameCondition`（例如 UnnaturalDarkness）。
- `void_structures` 扫描巨石地图上的 `VoidStructure`；`activated` 是
  `CompVoidStructure.Active`，`progress` 是原生交互进度。
- `metal_hell` 扫描 `Find.World.pocketMaps` 中 `generatorDef == MetalHell` 的地图，
  并列出 `VoidNode` 与 `Find.Anomaly.metalHellPawns`。
  `pawn_ids/pawn_count` 来自原生暂存待返回列表，**不是该地图当前人物集合**；准确在图人物
  用 `GET /map/pawns?map_id=<metal_hell.map_id>` 查询。
- `void_awakening_quests` 列出真正 `EndGame_VoidAwakening` 的 `id/state` 和
  `active_delays`（原生 `label/ticks_left`）；不能把 `monolith_quest` 当觉醒任务。
- `monolith_quest` 是 `Building_VoidMonolith.quest`（激活时生成的 EndGame_VoidMonolith）。

## 实体图鉴读取（AN-01）

2026-11-08（未删旧键）：

```json
{"codex":{"anomaly_study_enabled":true,"has_performed_void_provocation":true,
 "categories":[{"def_name":"Basic","label":"basic","entry_count":8,"discovered_count":2}],
 "provocation_entries":[{"def_name":"HarbingerTree","label":"harbinger tree","category":"Basic",
   "order_in_category":100,"discovered":true,"provocation_incidents":["HarbingerTreeProvoked"]}]}}
```

- `categories` 来自 `EntityCodex.EntryCountInCategory`/`Find.EntityCodex.DiscoveredCount`，
  与 `next_level` 的发现数同源；发现新的 Basic/Advanced 实体才允许继续激活巨石。
- `provocation_entries` 只列原生 `provocationIncidents` 非空的 `EntityCodexEntryDef`，
  `discovered` 是 `Find.EntityCodex.Discovered(entry)`。VoidProvocation 的完整遭遇以该字段与
  真实生成对象判定，不以 incident 队列判定。
- `has_performed_void_provocation` 是 `GameComponent_Anomaly.hasPerformedVoidProvocation`。
- 全部为只读原生状态；没有“直接标发现”的写端点，发现只能由正常遭遇/研究产生。

实测（`Reverse/evidence/an05-an01-voidprovocation.json`）：仪式结束后 Basic 发现数 1，
原生事件触发并生成 `Plant_TreeHarbinger88599` 后变为 2，`HarbingerTree` 条目 false→true。

## 虚空结构激活（G-43）

`POST /api/v1/dlc/anomaly/monolith/void-structure/activate`

```json
{"thing_id":71095,"pawn_id":"321","map_id":0,"dry_run":false}
```

- 原生玩家入口是 `CompInteractable.OrderForceTarget`：校验 `CanInteract` 后
  `JobMaker.MakeJob(JobDefOf.InteractThing, thing)` + `TryTakeOrderedJob`。
  本端点复用同一条 job 链，**不直接调用 `Interact`**；`applied:true` 只表示已提交工作，
  完成要等原生 `ticksToActivate`（VoidStructure 2160，Gleaming 巨石 300）。
- `dry_run` 只读 `CanInteract`/目标参数并投影，不创建 job、不推进进度。
- `thing_id` 必须是 `CompInteractable`；VoidNode 返回 422 `not_supported`，
  终局选择走 `/monolith/endgame`。未知 thing 404；非玩家殖民者/倒地/精神/无操作能力 422；
  已在该目标工作 409。
- 返回 `before/current`（active、on_cooldown、progress、ticks_to_activate、quest_tags）
  与 `submitted_job`；原生拒绝时 `applied:false`；提交异常时 `applied` 缺省，调用方应读回目标状态。

## 巨石终局：disrupt / embrace（G-43）

`POST /api/v1/dlc/anomaly/monolith/endgame`

```json
{"choice":"disrupt","pawn_id":"321","confirm":true,"dry_run":false}
```

- 前置：`Find.Anomaly.LevelDef == Gleaming`（4 级）、`monolith.quest` 存在、
  pawn 为存活且已生成的玩家殖民者，在 MetalHell 口袋地图且该地图有 `VoidNode`。不满足分别 422/409/404。
- 原生 UI 在 `CompVoidNode` 对话框里选择 Disrupt/Embrace；两个选项最终都调用公开的
  `VoidAwakeningUtility.DisruptTheLink(pawn)` / `EmbraceTheVoid(pawn)`。本端点直接执行
  这两个原生结果方法；仅观察到目标终局等级与 questline 结束后，才补做收尾：发送 `NodeClosed` 结束
  EndGame_VoidAwakening 任务、销毁 VoidNode，避免第二次选择。
- **不可逆**：`confirm:true` 必填；disrupt 后 6 级 `Disrupted`、embrace 后 5 级 `Embraced`；
  两者都 `questline_ended:true` 并显示原生 credits 窗口（可用 `ui/window/close` 关闭）。
- 返回 `level_before/after`、`level_def_after`、`quest_state_before/after`、
  `void_awakening_active_after`、`void_node_destroyed_after`、`metal_hell_present_after`、
  `pawn_hediffs`。`applied:true` 需要目标等级达到 5/6、questline 结束、节点销毁、觉醒任务不再活动且 MetalHell 已关闭。
  原生结果前抛异常保留节点和觉醒任务，已知无效果为 500/applied false；
  部分效果或读回未知则省略 applied。收尾保留并恢复原先的全局销毁开关。
- 直接调用结果方法不播放 VoidNode 的淡出动画，也不等待对话框关闭；资源/任务后果由原生方法执行。

### 结局后继续游玩

两种选择均显示原生胜利 credits，`exitToMainMenu=false`，殖民地可以继续。
原生结果暂时使人物 unspawned，并在300–600tick后把同一人物生成回母地图巨石附近；
`applied:true` 表示终局已完成，不表示这段返回等待已经结束。

通过 `POST /ui/window/close {"window_types":["Screen_Credits"]}` 关闭 credits，正常继续时间，
轮询 `GET /map/pawns?map_id=<母地图ID>` 的准确人物 ID 和 position 后即可继续正常工作/征召/移动。
`GET /game/game-over` 的 `game_ending:false` 可补充确认继续状态。
`colonists/positions` 有1800tick缓存且仅当前地图，终局跨图后的即时位置读回使用前述实时地图查询。

2026-11-13 结局返回差异补验、原生暂存/返回身份及后续 API 动作见
[补验记录](../references/reviews/2026-11-13-anomaly-ending-continuation.md)；
完整觉醒仍复用下列历史加速原生序列，不能把晚期 fixture 准备当完整通关。

### G-43 证据

`Reverse/evidence/dlc-G43-monolith-disrupt.json`（disrupt：1→2→3 正常激活、
阶段 1 两个 VoidStructure 正常激活、Gleaming 正常交互、metal hell/void node、
终局 6 级）与 `dlc-G43-monolith-embrace.json`（embrace：5 级、VoidTouched/Inhumanized）。
`dlc-G43-run.json` 记录隔离构建、profile、prepared fixture、prefs/DLL 清理。
实测 tick/ID：`ai_g36_level1` 副本 71098 起；平台/结构/节点 ID 见证据正文。


## 未覆盖

- 本次复核已验证原生序列：阶段 1 两个结构→阶段 2 三个结构（合计五个）→
  阶段 3 巨石变为 Gleaming→MetalHell→disrupt；没有“阶段 3 另一批结构”。
  fixture 准备发现数，并将觉醒任务中 >1000 tick 的原生等待缩短为原值的 1/100，
  不写等级/阶段/信号。此为加速序列证据，不是原版 24–48h 耐久或随机波验收。
  原 G43 使用 prepare_endgame 的历史证据保留，不冒充原生全序列。
  证据 `Reverse/evidence/anomaly-review-20261108-monolith.json`。
- 阶段 1 的实体波/黑暗随机后果未逐项断言；`UnnaturalDarkness` 仅作为原生条件存在。
- 跨地图/多人同时激活、VoidStructure 被摧毁分支、Mod 改写存档未验。
- 终局只验“同一 pawn 在 metal hell 内选择”的主流程；对话框 UI 本身（Cua 点击）未作为 API 证据。
- 未验证升级后的存档兼容性。
