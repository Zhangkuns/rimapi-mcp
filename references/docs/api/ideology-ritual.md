# Ideology 仪式实例、资格与正常启动（G-31）

维护：当前执行 Agent；核对：2026-11-07（America/Los_Angeles，HTTP 时间为 UTC 2026-11-08）；状态：C1 Festival 与 C2 Conversion 已 accepted_with_limits；C2 LeaderSpeech 与 RoleChange 任命/默认移除已 accepted_with_limits；TreeConnection725 已 accepted_with_limits；G31 整体未完成。

2026-11-08 接续状态：以下描述包含当前工作区实现字段，并不代表全合同验收。Funeral、Sky/date、Trial726/728限定证据见 [Codex交接](../harness/codex-takeover.md)；Trial资格/结果归属/727、Execution/Duel、囚犯/动物献祭和代表刺痕已限定自审通过，见各节；C3代表合同已[限定通过](../references/reviews/2026-11-08-g31-codex-c3.md)，边界见末节。

本页描述玩家主教义（`Faction.OfPlayer.ideos.PrimaryIdeo`）中 `Precept_Ritual` 实例的读取、资格回读和正常启动。写入路径为 HTTP → 游戏主线程 → 该实例的实际 `behavior.TryExecuteOn` / 虚 `CreateLordJob`，不手造基类 Lord，不使用 `AddObligation`、调试启动或强制结算。当前有 **anytime Festival / `CelebrationPartyChat`**、**`Conversion`** 与 **`LeaderSpeech`** 三个普通正例均 accepted_with_limits；RoleChange720 任命/默认移除亦已限定验收，见 [修复记录](../references/reviews/2026-11-07-g31-c2-role-runner-repair.md)；TreeConnection725 正常连接亦已限定验收，见[修复记录](../references/reviews/2026-11-07-g31-c2-tree-fixture-repair.md)。其它角色、目标、`RitualBehaviorWorker` 子类、取消/中断和保存加载见“未覆盖”。

## 2026-11-16 结果配置、强制结果与读回边界

用户以天灯类仪式询问内容、奖励、人员/目标与最终结果控制。本次仅静态核对，结论见[核对记录](../references/reviews/2026-11-16-ritual-outcome-audit.md)；没有实现新接口或重跑游戏。

- 仪式类型：`precepts?include_add_options=true`发现合法`ritual_patterns`，新增时经`reform/precept`的`ritual_pattern`选择。`ritual/config`不修改已有实例的pattern。
- 奖励配置：本页末尾`GET/POST ritual/config`的`attachable_outcome_effect`选择附加奖励种类；GET的`attachable_outcomes`列可选Def及`can_attach/reason`。这不会保证达到奖励所要求的评级，或保证奖励worker的随机/资格分支成功。
- 人员和目标：`ritual/start`已有`thing_id/target_x/target_z`、`roles/role_assignments/spectators/exclude`；先用status/dry读回原生资格。
- **指定最终评级/保证奖励触发：当前没有对应API。** 天灯`GetOutcome`按实际质量加权随机选择结果；其正面结果的访客分支另有随机判定。正常举办与奖励配置不能固定这些选择，debug原生动作白名单也没有该控制。后续强制结果能力须单独明确debug语义，不冒称正常UI选择。
- **完整结果读回仍有缺口。** 当前准确`run_id`保存完成/取消/中断、参与者终态记忆及特定仪式结果，但通用天灯路径未保存实际抽中possibility、最终quality、附加奖励是否实际执行成功及访客/礼物对象与该run的关联。终态记忆是当时读值，不能替代完整奖励回执。
- 原生开始窗口的质量因素、预计质量范围、各评级概率与奖励适用说明尚未完整投影到status/dry；此为新增登记的观察缺口，不把只允许开始或记忆字段当完整流程覆盖。

旧只读合同（`GET /api/v1/dlc/ideology/ritual`）保持兼容；新增 `ritual_id` 和 status/start。实现入口见 [控制器](../../../Source/RIMAPI/RimworldRestApi/Controllers/System/RitualControlController.cs)、[服务](../../../Source/RIMAPI/RimworldRestApi/Services/System/RitualControl/RitualControlService.cs) 与 [请求 DTO](../../../Source/RIMAPI/RimworldRestApi/Models/DlcIdeoDtos.cs)。

## 实例键

- `ritual_id` 是原生 `Precept.Id`（int），只在当前存档/世界内稳定，不是跨世界标识。
- 同一个 `PreceptDef` 可以对应多个不同名字的仪式实例；**不能**用 `precept_def` 或 label 代替实例键。
- 列表和 status 都返回 `ritual_id`；活动行另有 `lord_id`（`Lord.GetUniqueLoadID()`，如 `Lord_3`）和 `lord_load_id`。
- 实例必须属于玩家主教义；其它 ideo 的 precept 不通过这三个入口操作。

## GET 列表（保持兼容）

`GET /api/v1/dlc/ideology/ritual?ideo_id=8`

顶层字段不变：`ideo_id`、`ideo_name`、`available_count`、`available`、`active_count`、`active`；
ID-R16 起追加 `is_primary_ideo` 与 `player_ideos`（`ideo_id/ideo_name/is_primary/is_minor`，
来自 `FactionIdeosTracker.AllIdeos`，即 primary + 由自由殖民者推出的 minor）。省略
`ideo_id` 仍读主 ideo。`active[]` 追加 `ideo_id`/`ideo_name`，非主理念的 Lord 也能准确归属。

`available[]` 保留旧字段并新增 `ritual_id`：

```json
{"ritual_id":697,"precept_def":"Festival","label":"Archonic Jubilee",
 "tip_label":"Archonic Jubilee\nCan start any time","is_date_triggered":true,
 "repeat_penalty_active":false,"ticks_since_last_performed":74743,
 "active_obligations":0,"is_anytime":true}
```

`active[]` 保留旧字段 `precept_def`（来自 `job.Ritual.def.defName`，缺失时为 null），并新增 `ritual_id`、`lord_id`、`lord_load_id`、`map_id`、`target`、`roles`、`obligation_id` 等；旧字段 `map_id`、`ritual_label`、`stage_index`、`progress`、`ticks_left`、`cancelled`、`organizer`、`participants` 保留。`available` 仍按 precept 实例而不是按 Def 去重。

`POST /dlc/ideology/ritual/start` body 也接受可选 `ideo_id`，语义同 status：显式绑定
`ritual_id` 所属玩家派系 ideo，不把非主理念的 ritual id 静默解析到主 ideo（不匹配 404）。
ID-R16 实测：minor ideo Mankindism（8）的 RoleChange（454）用 `role_changer=Human321`
（同为 ideo 8）启动，running 的 `ideo_id=8`，原生终态 `role_change_result.actual_role`
读回 ResearchSpecialist（467）；`eligibility.role_change.options[]` 追加 `leader_role`，
非主 ideo 不提供 leader 选项（原生 `RitualUtility.AllRolesForPawn`/SocialCard 规则）。

## GET status

`GET /api/v1/dlc/ideology/ritual/status`

| 查询参数 | 必填 | 含义 |
| --- | --- | --- |
| `ritual_id` | 是 | 原生 `Precept.Id`，正整数 |
| `ideo_id` | 否 | 玩家派系 ideo（primary/minor）；给定时 `ritual_id` 必须属于该 ideo，否则 404；省略时在所有玩家派系 ideo 中解析 |
| `run_id` | 否 | 准确API启动记录的UUID；必须属于当前Game及指定ritual/map，否则404；省略取该范围最近一条记录 |
| `map_id` | 否 | 地图 `uniqueID`；限定运行/记录范围；省略时资格预览用当前地图（明确run_id时用该run地图） |
| `thing_id` | 否 | 目标 `ThingID`（如 `RitualSpot88473`）或数字 `thingIDNumber`；必须在所选地图已生成 |
| `x`、`z` | 否 | 无 `thing_id` 时使用的目标格坐标，两者必须同时给出；有 `thing_id` 时优先解析 thing |
| `organizer_pawn_id` | 否 | 作为开始窗口的 selectedPawn / organizer 参数参与资格构建，不直接分配角色 |
| `role_change_precept_id` | 否 | RoleChange 的角色选择预览；非负整数，语义与 start 同名字段一致 |
| `obligation_id` | 否 | 当前仪式 `active_obligations[].obligation_id` 的准确 ID（非负整数，0 有效）；有目标时还须通过该义务的原生目标资格，无目标时用于枚举该义务的候选 |

**status 的格坐标参数是 `x`、`z`；POST start 的对应字段是 `target_x`、`target_z`，两者不要混用。**

不带目标时返回实例身份、义务、运行中的 Lord 和可选目标候选：

```json
{"ritual_id":697,"ideo_id":"13","ideo_name":"Boccin Creed","precept_def":"Festival",
 "label":"Archonic Jubilee","is_anytime":true,"is_date_triggered":true,
 "repeat_penalty_active":false,"ticks_since_last_performed":74743,
 "active_obligation_count":0,"active_obligations":[],
 "running_count":0,"running":[],"last_finished_tick":-1,
 "completed_source":"no_start_record","completed":null,
 "target_candidates":[{"thing_id":"RitualSpot88473","def_name":"RitualSpot",
   "x":150,"z":95,"map_id":0}]}
```

`target_candidates` 来自 `ritual.obligationTargetFilter.GetTargets(selectedObligation, map)`，最多 50 项；它只是候选，不证明当前可提交。无目标时，显式 `obligation_id` 必须属于本仪式的 `activeObligations`；省略时仅在 active 义务唯一时自动选择，多项返回 422 要求明确 ID。`GraveWithTarget`（Funeral 尸体墓过滤器）无 active 义务返回 422，避免原生候选方法读取 null 义务；其它支持无义务的 worker 保留 `GetTargets(null, map)` 路径。不能由 `is_anytime:false` 推定必须有义务（例如 Trial 能力入口正常传 null）。无地图时保留空候选，但显式不存在的 ID 仍返回 404。

2026-11-13：明确 `run_id` 且未提供目标、organizer、角色选择或 `obligation_id` 时，
只读该准确运行的状态/终态，不再解析当前启动资格或枚举 `target_candidates`。
因此多项当前义务、原生结果已消耗原义务均不会阻止历史运行查询。
同时给出上述资格参数仍按当前状态校验；不带 `run_id` 的发现语义保持如上。

每个 `roles[]` 行含 `role_id`、`label`、`required`、`substitutable`、`max_count`、`assigned`、`candidates` 和 `forced`。`forced` 是本次请求（dry-run 或提交）强制到该角色的人物 ThingID；无强制时为 null，ResponseBuilder 可能省略 null 字段。C1 合同的旧字段和行为保持兼容。

带目标时额外返回 `target` 和 `eligibility`：

```json
{"target":{"thing_id":"RitualSpot88473","def_name":"RitualSpot","x":150,"z":95,"map_id":0},
 "obligation_id":null,
 "eligibility":{
   "target_usable":true,"target_fail_reason":null,
   "can_execute":true,"can_start":true,"can_start_reason":null,
   "blocking_issues":[],
   "roles":[{"role_id":"leader","label":"speaker","required":true,
     "substitutable":true,"max_count":1,
     "assigned":["Human321"],"forced":null,
     "candidates":["Human321","Human318"]}],
   "participants":["Human321","Human318","Human66734","Human66738","Human66742"],
   "spectators":["Human318","Human66734","Human66738","Human66742"]}}
```

`running[]` 枚举所有匹配Lord，行与列表active同形：`run_id`（非API启动为null）、`lord_id`、`lord_load_id`、`map_id`、`ritual_id`、`precept_def`、`ritual_label`、`stage_index`、`progress`、`ticks_left`、`cancelled`、`organizer`、`target`、`participants`、`roles`、`obligation_id`。

运行身份、原生终态与保存加载：

- 每次实际新建Lord生成独立`run_id`，并在当前Game的`RitualRunGameComponent`用Scribe保存身份、开始tick、map/Lord/目标、角色/参与者和结果。start对比原生调用前后Lord，只关联本次新建者。`runs[]`枚举本仪式已记录运行；`last_run`是选定记录，省略run_id时为最近一条。
- `run_state`为`active/completed/cancelled/interrupted/unknown`。`completed:true`依据准确Lord的原生`RitualFinished:true`及首次`ApplyOutcome`正常返回；不再以precept全局`last_finished_tick`或其它Lord是否存在推断。原生finished分支内部可能捕获outcome异常，所以它证明原生流程完成，具体效果仍由对应结果读回解释。
- `terminal_tick`是观察到原生终态的tick；完成时另有`finished_tick`，取消/中断时为null；`duration_ticks=terminal_tick-started_tick`。`native_cancelled`是原生参数。观察到Cancel调用且原生取消才称`cancelled`；其它原生未完成终态为`interrupted`。没有终态记录且Lord已不可见时为`unknown`、completed为null；旧存档没有API记录时为`no_start_record`、completed为null。
- `terminal_result`是结算返回时冻结并随存档保存的结果快照；其中通用`outcome_memories`声明为`participant_memories_at_native_terminal`，仅是当时人物记忆读值。`age_ticks/added_tick`来自原生记忆age（150tick批量变化），`added_tick_estimated:true`，不是精确来源证明。Trial继续使用该准确Lord的原生选择/Apply快照。
- 顶层`physical_result`、`role_change_result.actual_role`、`tree_connection_result`及`trial_result.convict_guilty`保留查询时真实状态；永久结果看terminal_result。原生已不存在的受试Pawn返回`pawn_id/role_id/available:false`，不会冒充当前存活/死亡。死者仍在存档时Scribe明确保留其引用及尸体关联。
- 保存记录不保存`game_memory`执行缓存；跨Game/重启不承诺冻结重放或exactly-once。加载不会重新执行仪式或其效果。原生UI启动的Lord仍可列出，但没有本API run_id/历史结果记录。

### RoleChange 角色选择与结果

`eligibility.role_change` 包含本次 assignment 的 `target_pawn`、`current_role`、`selection`、`selection_source` 与 `options`。角色引用包含 `precept_id/label`；options 另含 `requirements_met/is_current/holders`。省略选择时调用原生 `UpdateRoleChangeTargetRole`，强制角色不会通过原生 `TryAssign` 自动初始化选择，因此需要显式补此调用。`selection_source` 为 `explicit`、`explicit_remove`、`default_first` 或 `default_remove`。选择不存在或不满足要求的角色返回 422；重复当前角色由原生确认拒绝，当前程序集返回 409、`You must choose a role.`。

`role_change_result` 来自选定的持久 `last_run`，记录该次 `role_changer` 人物、`target_thing_id`、`selected_precept_id/selected_label/selection_source` 和起止 tick。`actual_role` 每次读取该人物当前真实角色；它不是查询时新候选人物的角色，也不保证未来一直保持该角色。`completed` 采用上面的准确Lord原生终态；同tick其它run的lastFinishedTick不构成本次证据。本次尚未完成时结果的 `finished_tick` 为 null。任命/移除由正常原生结果执行，不直接 Assign/Unassign。

### TreeConnection 角色与结果

`eligibility.tree_connection` 读取目标真实 `CompTreeConnection`：`tree_thing_id/def_name/connector/connected/connected_pawn/connection_strength/desired_connection_strength/connection_torn`。`connector` 为本次 assignment 的人物，`connected_pawn` 才是实际连接人物。

显式强制 Tree connector 时，资格通过后将原生 forced lock 转成 `RitualRoleAssignments.TryAssign` 的实际分配；请求映射仍在响应 `forced` 中准确读回。只保留一份 native 分配，因为 `AssignedPawns` 会同时枚举 forced 与 assigned，而讲话任务 `RoleFor(pawn,false)` 只读后者。这只针对已证实的 Tree 路径，RoleChange 等既有行为不作此转换。

`tree_connection_result` 依据选定的持久 `last_run`，含 `tree_thing_id/connector/connected/connected_pawn/connection_strength/matches_connector/started_tick/finished_tick/completed`。结果读取本次准确目标的真实 Comp；完成采用准确Lord终态，不能以记忆替代连接；terminal_result保留结算当时的连接读值。

TreeConnection725 已在隔离现场完成正常 POST、虚工厂 Lord_3、74742→76743/2001ticks、原生 ConnectToTree outcome；准确树88473连接Human318、strength0.329999983，dry/拒绝/冻结重放/header/异体409/占用422/单条operation通过。[修复与限定验收](../references/reviews/2026-11-07-g31-c2-tree-fixture-repair.md)已获监督端 accepted_with_limits，不代表 C2/G31 整体完成。

### ID-R12 失明仪式终态（2026-11-13）

`BlindingCeremony` 的强制 `doer` 在原生资格通过后经 `TryAssign` 转为实际角色，
请求 `forced` 读回仍保留。其讲话任务使用 `RoleFor(pawn,false)`；只放在原生
forced lock 会导致 `JobGiver_GiveSpeechFacingTarget` 空引用，无法执行正常讲话。
目标角色、义务、原生 worker、手术伤害和结算路径均沿用正常 API/游戏逻辑。

隔离 `id12-codex-run4-20261113` 从公开世界预览选择温带地点，正常 fluid 开局后
改革加入 Blindsight。公开列表发现仪式755、义务0、目标Human370及执行者Human373，
公开 `role/assign` 指派道德家；dry-run 后正常启动。逐 tick 观察到 `GiveSpeech`、
两次 `Blind` 指向准确目标，眼部在5972/6130 tick从2→1→0，8453 tick原生完成。
`physical_result` 读回Sight0、Missing Eye×2、目标存活；自身记忆
`BlindingCeremony_Self_Elevated` 与参与者 `BoringBlinding`，义务3→2。
完整限定、自审和失败历史见[收尾记录](../references/reviews/2026-11-13-id-r12-ideology-blinding-closeout.md)。

测试 fixture 只补改革点数/仪式地点并独立观察与有界暂停；伤害阶段关闭调试无敌，
未调用旧anytime脚手架、未删除眼部或强制结算。此为有披露前置的单族终态实测，
不代替无fixture的跨域纯API连续验收。旧BurnCircle/DanceTech限定成果仍保留。

## POST start

`POST /api/v1/dlc/ideology/ritual/start`

| body 字段 | 必填 | 含义 |
| --- | --- | --- |
| `ritual_id` | 是 | 原生 `Precept.Id`，必须属于玩家主教义 |
| `map_id` | 否 | 地图 `uniqueID`；省略时用 `Find.CurrentMap` |
| `thing_id` | 否 | 目标 `ThingID` 或数字 `thingIDNumber`；必须在该地图已生成 |
| `target_x`、`target_z` | 否 | 目标格；`thing_id` 省略时使用，两者必须同时给出 |
| `organizer_pawn_id` | 否 | 开始窗口的 selectedPawn / organizer 参数；必须是该地图上的玩家人物；此项仍限玩家派系 |
| `roles` | 否 | 角色 id → **单个** PawnID 的强制映射；只接受该 behavior 的现有 role id，人物须存在并 spawn 在所选地图；不要求玩家派系，最终由原生 `assignments.PawnNotAssignableReason`（内含 `role.AppliesToPawn`）判定；不支持数组或一次多 pawn。非玩家/囚犯/动物只有在角色本身接受时才可被强制，不能据此声称所有此类操作都可执行 |
| `role_change_precept_id` | 否 | RoleChange：正数选择目标人物教义的 `Precept_Role.Id`，0 明确移除，省略使用原生默认选择；无当前角色时选择首个满足要求的角色，有当前角色时默认移除 |
| `role_assignments` | 否 | ID-R14：`[{"role_id", "pawn_id"}]` 精确角色行。`FillPawns` 后按原生 `RitualRoleAssignments.TryAssign` 应用；同一角色可重复（原生 `maxCount<=0` 表示不限），但每个 pawn 只能出现一次。显式行会替换该角色的原生自动分配；已在 `roles` 中强制的角色不能再出现于此 |
| `spectators` | 否 | ID-R14：精确旁观名单。出现该字段（含空数组）时，不在名单中的原旁观候选由原生 `RemoveParticipant` 移出参与，名单内人物用原生 `TryAssignSpectate` 分配；省略时保留 `FillPawns` 的默认旁观集合 |
| `exclude` | 否 | ID-R14：明确不参与名单，应用角色/旁观后用原生 `RemoveParticipant` 移出。organizer、forced 角色人物、extra required 及 required 角色人物不可排除；与 `role_assignments`/`spectators` 重叠、排除 organizer/forced 返回 400，排除 extra required 或 required 角色人物在原生应用阶段返回 422 `roster_error`，不会静默移出后启动 |
| `obligation_id` | 否 | 明确选择本仪式 active 义务的准确整数 ID；不存在返回 404，不替换为其它义务；省略按下面的选择语义处理 |
| `dry_run` | 否 | 只做资格与确认检查，不调用原生启动 |

不传 `map_id` 时使用当前地图；`thing_id` 接受字符串 `ThingID`，也接受可解析为 `thingIDNumber` 的数字字符串。目标在 `map/buildings` 的 numeric id 可直接作为 `thing_id` 传给本接口。

非空 `thing_id` 优先于格坐标；解析失败不会回退到坐标。GET status 不给 thing 或坐标时返回候选，POST start 不给目标则返回 400。GET status 的 organizer 查找只检查人物是否存在；POST 还检查其已 spawn 在所选地图且属于玩家派系，两者的错误码见下表。

### 原生资格链与检查次序

先解析义务，再复用开始窗口的资格构建。显式 ID 只查本 `Precept_Ritual.activeObligations`；有目标时用 `obligationTargetFilter.CanUseTarget(target, selectedObligation)` 判断 native 关联（丧葬为墓内 Corpse/InnerPawn 与死者义务的关联，不是 raw target 相等）。省略 ID 时按该原生资格筛选，唯一匹配才自动选择，多匹配返回 422 要求 ID。`GraveWithTarget` 无义务/无匹配返回 422，明确 ID 不存在返回 404、目标不匹配返回 422；这些错误在创建 assignments 或继续 native null 资格调用前返回，POST 保留 `submitted/started/applied:false`。其它原生允许 null 义务的 worker（含普通 anytime 与非 anytime Trial）无匹配时保留无义务资格路径。选择失败时 `eligibility:null`，不能将其解释为已完成资格检查。

选择成功后的构建顺序：

1. 以与 `Precept_Ritual.GetRitualBeginWindow` 等价的 `Dialog_BeginRitual.PawnFilter` 调 `Dialog_BeginRitual.CreateRitualRoleAssignments`，再 `RitualRoleAssignments.FillPawns`；带强制角色时先执行一次已有 `PawnNotAssignableReason` 校验，原生已拒绝者不进入 FillPawns（本版动物 connector 会触发遍历时修改列表的原生异常）；filter 排除已有 Lord 的人物、按角色检查动物、subhuman 与教义例外。
2. `ritual.obligationTargetFilter.CanUseTarget(target, obligation)` → `target_usable` / `target_fail_reason`。
3. `behavior.CanStartRitualNow(target, ritual, organizer, forcedRoles)` → `can_start` / `can_start_reason`（仅这一项原生方法的结果）。
4. `behavior.CanExecuteOn(target, obligation)` → `can_execute`。
5. 服务用精确私有签名 `(RitualRoleAssignments, Precept_Ritual, TargetInfo, RitualOutcomeEffectDef)` 构造脱离 `WindowStack` 的 `Dialog_BeginRitual`，执行受保护 virtual `BlockingIssues` → `blocking_issues`。它使用与开始/提交同一份 assignment 和 target，不打开窗口、不 `PostOpen/Start`、不结束 Job、不随机结算；覆盖参与者、温度、观众、outcome、target、分组必需角色数量、人物资格和额外必需人物等确认条件。
6. 若请求带 `roles`，资格构建在 FillPawns 前执行 `ValidateForcedRoles`，HTTP 在 `target_usable`/`can_execute` 通过后、同实例冲突检查前返回其已记录的拒绝结果：以原生 `assignments.PawnNotAssignableReason(pawn, role)` 判断该人物能否填入该角色；不通过 → 422 `invalid_target`，data 返回 `forced_role`、`forced_pawn`、`forced_role_reason`，`submitted/started/applied:false`，不调用 `TryExecuteOn`。
7. 全部通过后才 `behavior.TryExecuteOn(target, organizer, ritual, obligation, assignments, true)`；虚 `CreateLordJob` 创建该实例对应的 LordJob，`LordMaker.MakeNewLord` 和 `PreparePawns` 走正常链。

**`can_start:true` 不等于可以提交。** 必需但可替代（required + substitutable）的角色会让 `CanStartRitualNow` 返回 null，但原生确认仍可能要求至少一名参与者或实际 speaker。负例中三个字段分别为 `target_usable:true`、`can_execute:true`、`can_start:true`、`can_start_reason:null`，而 `blocking_issues` 为：

```json
["You need at least one participant.","You need a speaker.",
 "Need at least one Archotechnist participating."]
```

提交条件必须同时满足：`target_usable && can_execute && can_start_reason == null && blocking_issues` 为空，且强制角色（如有）通过原生 assignment 资格。服务先返回义务选择错误（不存在 404，其它选择错误 422），再按以下次序拒绝：目标不可用 → 422 `invalid_target`；`can_execute:false` → 422 `invalid_target`；强制角色被原生 assignment 拒绝 → 422 `invalid_target`（带 `forced_role/forced_pawn/forced_role_reason`）；同实例已在运行 → 409 `conflict`；`can_start_reason` 非空 → 409 `conflict`；`blocking_issues` 非空 → 409 `conflict`；然后才是 dry-run 或实际提交。**检查次序以源码为准**：同实例占用同一目标时，目标可用性先失败，因此该样本返回 422 而不是 409；这是真实的资格结果，不表示第二次启动成功。

### 响应

资格构建后生成的响应 data 字段包括：`ritual_id`、`ideo_id`、`ideo_name`、`precept_def`、`label`、`is_anytime`、`is_date_triggered`、`repeat_penalty_active`、`ticks_since_last_performed`、`active_obligation_count`、`map_id`、`target`、`obligation_id`、`dry_run`、`submitted`、`started`、`eligibility`。  `eligibility` 另含完整名单读回：`roster`（`role_assignments` 实际应用行、`spectators_exact`、`requested_spectators`、`excluded`）、`participants`、`spectators`、`non_participants`（候选池中未参与者）、`extra_required_pawns` 与 `forced_roles`；`roles[].assigned` 是应用后的准确角色分配。start 成功响应的 `running` 同样带 `spectators/non_participants/extra_required_pawns/forced_roles`。早期参数、身份或 forcedRoles 预检错误可能没有 data；ResponseBuilder 忽略 null 字段，不能要求所有失败都含这些字段或三个 false。

- **dry-run**：HTTP 200，`data.would_start:true`、`submitted/started/applied:false`、`outcome:"would_start"`；没有 Lord、没有 operation 事件，也不写幂等执行缓存。
- **成功提交**：HTTP 200，`applied:true`、`submitted:true`、`started:true`、`outcome:"started"`，附 `run_id`、`started_tick`、`running`、`last_run`、`readback_ok:true`、`uncertain:false`。`applied` 只表示原生创建新Lord且读回准确关联本次run，不表示仪式完成。
- **资格构建后的拒绝**：`applied/submitted/started:false`、`outcome:"rejected"`；资格与错误码见上。确认阻塞时 `blocking_issues` 原样返回，不调用 `TryExecuteOn`，不发事件。早期预检错误不承诺此 data 形状。

执行后的结果由服务观察值交给 `AiActionResults.Finish` 分类。`submitted` 仅表示 `TryExecuteOn` 正常返回，`started` 表示读回找到了本次新建的Lord；500 不等于 `uncertain:true`。以下异常/静默不启动分支是**源码语义，未做故障运行覆盖**：

| 原生调用与读回 | HTTP / `error_code` | data 字段 |
| --- | --- | --- |
| 抛异常，但后续读回找到 Lord 并正常完成响应构建 | 500 `internal_error` | `submitted:false`、`started:true`、`applied:true`、`outcome:"started"`、`readback_ok:true`、`uncertain:false`，含 `native_error`；已观察到启动，不应重试 |
| 正常返回，但读回没有 Lord | 500 `internal_error` | `submitted:true`、`started:false`、`applied:false`、`outcome:"not_started"`、`readback_ok:true`、`uncertain:false` |
| 抛异常，后续读回未找到 Lord | 500 `internal_error` | `submitted:false`、`started:false`、`outcome:"partial_or_contradictory"`、`readback_ok:true`、`uncertain:true`，含 `native_error`；`applied` 缺省，不表示已知无效果 |

新Lord枚举或后续 DTO/角色读回自身抛错时，服务不能完成上述 Finish 分类；POST 协调器捕获该异常时返回一般 500 `internal_error`，其 data 只有 `uncertain:true`，不保证 submitted/started/applied、outcome 或其它完整读回字段。其它一般错误也不保证有 data。此服务传给 Finish 的 `ReadbackOk` 固定为 true，不能把 Finish 通用的 `ReadbackOk:false / outcome:unknown` 分支当作本入口读回异常的承诺。未知结果先独立读状态，不换 key 盲重试。

`running.organizer` 是原生 `LordJob_Ritual.Organizer`（本样本为 null），**不是** leader 角色。本样本 `organizer_pawn_id:"321"` 时 `roles.leader:["Human321"]`，但 `running.organizer:null`；不要用角色分配推断 Organizer。

### 幂等与事件

- 入口合同为 `command` / `pawn_ai` / `validate` / `game_memory`。`Idempotency-Key` 在当前进程/当前 Game 内有效。
- 同 key 同 body：返回首个完整响应原文和 HTTP 状态，附 `Idempotency-Replayed:true`；不重复执行、不重复发事件；独立 status 仍是同一个 Lord。
- 同 key 不同 body：409 `conflict`（`Idempotency key: Conflict`）。
- dry-run 绕过执行缓存；失败响应也会冻结在该 key 下。
- `operation` 事件仅在响应 `success:true` 且 `applied:true` 的非 dry-run 执行发布，路径为 `/api/v1/dlc/ideology/ritual/start`；dry-run、资格拒绝、重放均不发。异常但已观察到 Lord 的 500 即使含 `applied:true` 也不发（源码语义，未故障实测）。本样本 SSE 连接收到恰一条该入口 operation。

## POST cancel

`POST /api/v1/dlc/ideology/ritual/cancel`，body为`{"run_id":"ritual-…","dry_run":false}`。run_id必填；只解析当前Game已有API记录，并核对记录的ritual/map/Lord仍活动，然后调用该原生`LordJob_Ritual.Cancel()`信号。不按label或首个Lord取消。

缺run_id为400，当前Game无该记录为404，记录已终止或Lord不再活动为409。dry返回`would_cancel:true`且submitted/applied为false，不改变记录或发事件。正常提交返回`submitted/applied:true`、`outcome:cancel_requested`、准确run/ritual/map/Lord、`cancel_requested_tick`和当前`run_state/native_cancelled`；提交信号与实际原生终态分开读回。异常与未知结果沿用AiActionResults，不表示自动回滚。

本入口也是command/pawn_ai/validate/game_memory；同key同体冻结重放、异体409，dry/拒绝/重放静默，仅成功applied发布一条自身路径operation。不同key请求已结束记录409。此入口暂不接管没有API运行记录的原生UI仪式。

## POST leave（ID-R14 个人离场）

`POST /api/v1/dlc/ideology/ritual/leave`，body为`{"run_id":"ritual-…","pawn_id":"Human318","dry_run":false}`。只接受当前Game已有API记录的准确run；`pawn_id`必须是该Lord当前owned participant。该入口复现原生 `LordJob_Ritual.GetPawnGizmos` 的"Leave ritual"动作，顺序一致：把人物加入受保护的`pawnsForcedToLeave`、调用`lord.Notify_PawnLost(pawn, PawnLostCondition.ForcedByPlayerAction)`、`pawn.jobs.EndCurrentJob(JobCondition.InterruptForced)`。不调用整场`Cancel()`，不调用`LeaveRitual`（原生没有该方法）。

与原生gizmo相同的拒绝条件：pawn是organizer、在`ExtraRequiredPawnsForReading`（含forced角色人物）、其角色`required`，或`lord.ownedPawns.Count < 2`（最后一名参与者）。拒绝返回422 `invalid_target`，data含`leave_block_reason`、`role_id`、`spectator`、`lord_pawn_count`，不改变状态。

普通dry-run返回200 `would_leave:true`、`applied:false`。实际执行返回`left`、`pawns_forced_to_leave`、`participants`、`spectators`、`roles`、`non_participants`、`extra_required_pawns`、`forced_roles`、`lord_pawn_count`、`run_state`与`native_terminal`；`applied:true/outcome:"left"`只在读回该pawn已不在lord/assignments且已进入`pawnsForcedToLeave`时给出。个人离场后run仍可继续；`pawnsForcedToLeave`阻止该pawn再次自愿加入。整场取消仍走`POST /ritual/cancel`，两者不是同一操作。

该入口需要共享`Core/Ai/AiEndpointContracts.cs`登记`POST dlc/ideology/ritual/leave`（command/pawn_ai/validate/game_memory）；登记前带`Idempotency-Key`会按未登记路由500，本项实测在登记前不带key调用。run_id/pawn_id缺失400；当前Game无该记录404；记录已终止或Lord不再活动、pawn非本run owned participant409；无活动Game409；Ideology未启用422；原生读回矛盾500。

### 错误码

| 情况 | HTTP / `error_code` |
| --- | --- |
| GET status 缺/非正整数 `ritual_id`，或非整数 `map_id` | 400 `bad_request` |
| POST start 缺请求体或缺/非正数 `ritual_id` | 400 `bad_request` |
| GET status 负数或非整数 `obligation_id` | 400 `bad_request` |
| 显式 `obligation_id` 不在本仪式 active 义务中（GET 有/无目标及 POST） | 404 `not_found` |
| `GraveWithTarget` 无 active 义务、目标与所选义务不匹配、自动选择歧义 | 422 `invalid_target` |
| GET status / POST start 未知 ritual 实例，或显式给出但找不到的整数 `map_id` | 404 `not_found` |
| GET status 的 thing 不存在、未 spawn 或不在所选地图 | 404 `not_found` |
| POST start 的 thing 不存在、未 spawn 或不在所选地图（`TryResolveStartTarget` 失败） | 400 `bad_request` |
| GET status 无 thing 时，`x/z` 只给一项、非整数或越界 | 400 `bad_request` |
| POST start 无 thing 时，缺目标、`target_x/target_z` 只给一项或格坐标越界 | 400 `bad_request` |
| POST start 的 forcedRoles 预检：空 role/PawnID、未知 role、未知 pawn、pawn 未 spawn/跨图 | 400 `bad_request` |
| POST start 的强制角色被原生 assignment 拒绝（角色不接受该人物） | 422 `invalid_target`；data 含 `forced_role`、`forced_pawn`、`forced_role_reason` |
| 原生角色资格返回 false 但没有 reason，后续确认资格检查抛错（本版无 Ideo 动物充当 RoleChange `role_changer`） | 409 `conflict`；`eligibility.can_start:false`、`can_start_reason` 为 `eligibility check failed: ...`，`submitted/started/applied:false`；不会启动 Lord |
| GET status / POST start 的 organizer 未找到（进入 organizer 查找时） | 404 `not_found` |
| POST start 的 organizer 未 spawn、跨图或非玩家派系 | 422 `invalid_target` |
| 无活动 Game | 409 `conflict` |
| POST start 未给 `map_id` 且没有当前地图 | 409 `conflict` |
| 同 key 异体 | 409 `conflict` |
| 同实例已运行 | 409 `conflict` |
| `can_start_reason` 非空或 `blocking_issues` 非空 | 409 `conflict` |
| POST start 的实例无 behavior worker、目标不可用或 `can_execute:false` | 422 `invalid_target` |
| Ideology 未启用 | 422 `dlc_missing` |
| 原生异常、正常返回未启动、读回异常 | 500 `internal_error`；data 和 uncertain 按上方具体分支，不能统一视为 uncertain:true |
| ID-R14 `role_assignments`/`spectators`/`exclude` 结构错误、未知role、人员不存在/未spawn/跨图、重复或重叠、排除organizer/forced | 400 `bad_request` |
| ID-R14 原生拒绝角色/旁观分配（`PawnNotAssignableReason`、`maxCount`、`TryAssignSpectate`）或排除required/extra required | 422 `invalid_target`；data含`roster_role`/`roster_pawn`/`roster_error` |
| POST leave 缺 `run_id`/`pawn_id`、未找到记录或pawn | 400/404 |
| POST leave 记录不活动、pawn非本run owned、最后一名参与者 | 409 `conflict` |
| POST leave organizer/required角色/extra required | 422 `invalid_target`；data含`leave_block_reason` |
| POST leave 原生`pawnsForcedToLeave`字段不可用或读回异常 | 500 `internal_error` |

表中目标/organizer 错误码指已经通过更早检查并进入对应分支的情况。GET status 未指定地图且没有当前地图时，返回实例/运行信息和空 `target_candidates`，不进入目标或 organizer 检查。POST 的 `map_id/target_x/target_z` 是 DTO 整数字段；上表坐标分支指 body 已成功解析后的整数坐标检查。

## C1 / C2 实测范围

最终正例使用 DLL SHA256 `6b9e4a53b285d96663cd4cce36e7e81d1133949ffd33fa6ed5bc34a7dc413d79`，源存档/复制 SHA256 均为 `13d61c9b5097d9878f6a43100564901d402ca79102099f8217bdd765098e1840`，PID 2987972：

- 实例 697，Boccin Creed，`Festival` / `Archonic Jubilee`，anytime，map 0，目标 `RitualSpot88473` (150,95)。
- dry-run 200、`would_start`、零 Lord、零 operation；正常 start 200，`Lord_3`，五名参与者 `Human321/318/66734/66738/66742`，`roles.leader:["Human321"]`，`organizer:null`。
- 同 key 重放 raw 逐字节相同并带 `Idempotency-Replayed:true`；同 key 异体 409；不同 key 在占用目标上 422、`submitted/started/applied:false`，独立 status 仍是唯一 `Lord_3`。
- 原生完成：tick 74742→80994，duration 6252；`completed:true`，`last_finished_tick=finished_tick=80994`；五人均为原生 `FunFestival`、`mood_offset:5`、`added_tick:80994`；最终 `active_count:0`。

证据：

- 正例：[ritual.json](../../../tools/cua/.runtime/g31-c1-eligibility-repair-final/evidence/ritual.json)、[load-control.json](../../../tools/cua/.runtime/g31-c1-eligibility-repair-final/evidence/load-control.json)、[loaded-identity.json](../../../tools/cua/.runtime/g31-c1-eligibility-repair-final/evidence/loaded-identity.json)。
- 确认门禁负例（全部地图人物被真实 Lord 占用）：[ritual.json](../../../tools/cua/.runtime/g31-c1-eligibility-repair-negative/evidence/ritual.json)，原生 `CanBegin:false`、`participants:[]`，GET 三阻塞项，dry 与 start 均 409、无副作用、零 operation。
- 旧成功与失败证据保留：[runner 最终正例](../../../tools/cua/.runtime/g31-c1-runner-repair-final/evidence/ritual.json)、[289 次场地误生成失败](../../../tools/cua/.runtime/g31-c1-run1/evidence/ritual.json)、[422 断言失败](../../../tools/cua/.runtime/g31-c1-runner-repair/evidence/ritual.json)。

### C2 改信样本（accepted_with_limits）

最终 C2 样本使用隔离 RIMAPI DLL SHA256 `45b4acfaf5bd71778c0209939d3307091dbadce0a21b2d0b5cdbf85f4e3ddadc`，PID 3044340；源存档/复制 SHA256 同为 `13d61c9b5097d9878f6a43100564901d402ca79102099f8217bdd765098e1840`，tick 74742。

- 实例 709，Boccin Creed，`Conversion` / `conversion ritual`，anytime，map 0，目标 `RitualSpot88473` (150,95)；worker 为 `RitualBehaviorWorker_Conversion`。
- fixture 通过原生 `Precept_RoleSingle.Assign` 准备 moralist `Human321`；convertee 固定为 `Human66734`（Ideo 7 Nova-Archotechnics，玩家派系劳工）。正常 POST 通过原生前置，`roles.moralist:["Human321"]`、`roles.convertee:["Human66734"]`、`forced.convertee:"Human66734"`；`Lord_3`，五名参与者 Human321/318/66734/66738/66742，`organizer:null`。
- dry-run 200、`would_start`、无 Lord/operation；正常 start 200、`submitted/started/applied:true`；同 key raw 逐字节冻结重放、`Idempotency-Replayed:true`；同 key 异体 409；新 key 占用目标 422、无副作用，仍唯一 `Lord_3`；成功 start 恰一条 operation。
- tick 74742→80189（5447 ticks）原生完成，`completed:true`、`last_finished_tick=finished_tick=80189`；guard `ritual_ended` 当 tick 暂停，五人未死/未倒地。
- 受仪者 certainty `0.6825158→0.384681225`，仍为 Ideo 7；三位观众 Human318/66738/66742 读到原生 `EffectiveConversion`（Effective、mood_offset 3、added_tick 80189）。不声称已改信，也不强求某个随机等级。
- 相关拒绝：非场地格 422 `invalid_target`；强制非人 convertee `LabradorRetriever3688` 422 `invalid_target`，data `forced_role:"convertee"`、`forced_pawn:"LabradorRetriever3688"`、`forced_role_reason:"convertee must be a human."`；完成后解除 moralist，start 409 `conflict`，`data.eligibility.can_start:false`，原因 `You must assign the role of Cleric in order to start this ritual.`，阻塞 `You need a speaker.`；三次拒绝均无 Lord/推进/operation。

证据：[ritual.json](../../../tools/cua/.runtime/g31-c2-runner-repair-final/evidence/ritual.json)、[final-readback.json](../../../tools/cua/.runtime/g31-c2-runner-repair-final/evidence/final-readback.json)、[cleanup.json](../../../tools/cua/.runtime/g31-c2-runner-repair-final/evidence/cleanup.json)、[监督端清理复核](../../../tools/cua/.runtime/g31-c2-runner-repair-final/evidence/supervisor-cleanup-verification.json)。run1 原始失败保留在 [g31-c2-run1/evidence/ritual.json](../../../tools/cua/.runtime/g31-c2-run1/evidence/ritual.json)，未改写为成功。

### C2 LeaderSpeech 样本（accepted_with_limits）

最终样本使用隔离 RIMAPI DLL SHA256 `45b4acfaf5bd71778c0209939d3307091dbadce0a21b2d0b5cdbf85f4e3ddadc`，PID 3091420；源存档/复制 SHA256 `13d61c9b5097d9878f6a43100564901d402ca79102099f8217bdd765098e1840`，tick 74742。

- 实例 716，Boccin Creed，`LeaderSpeech` / `leader speech`，anytime，map 0，目标 `RitualSpot88473` (150,95)；worker `RitualBehaviorWorker_Speech`，outcome `AttendedSpeech` / `RitualOutcomeEffectWorker_Speech`。**role id 是 `speaker`，不是 `leader`。**
- fixture 通过原生 `Precept_RoleSingle.Assign` 准备 `IdeoRole_Leader` 的持有者 `Human321`（`RequirementsMet:true`），未改其它 precept；正常 POST 通过全部原生前置。`roles.speaker:["Human321"]`、参与者 Human321/318/66734/66738/66742、目标准确；本样本原生 `organizer:"Human321"`（非 null；不要据此把所有仪式 Organizer 都当成非 null）。
- `CreateLordJob` 的虚工厂按静态证据返回 `LordJob_Joinable_Speech`（生产 `RunningDto` 不暴露 CLR 类型，故这是 `native_factory_expected_type`/源码推定）；test-only fixture 的 `lord_job_clr_type` 实际观测为 `RimWorld.LordJob_Joinable_Speech`。两者分开记录，不把硬编码类型当作生产读回。
- dry-run 200、`would_start`、无 Lord/operation；正常 start 200、`submitted/started/applied:true`、`Lord_3`；同 key raw 冻结重放、同 key 异体 409；新 key 占用目标 422、无副作用，仍唯一 `Lord_3`；成功 start 恰一条 operation。
- tick 74742→79743（5001 ticks）原生完成，`completed:true`、`last_finished_tick=finished_tick=79743`；guard `ritual_ended` 当 tick 暂停，五人未死/未倒地。
- 原生结果：四名听众 Human318/66734/66738/66742 得 `InspirationalSpeech`（`outcome_label:"Inspirational"`，mood_offset 分别为 10/5/5/5，added_tick 79743）；演讲者本人无同款记忆。随机质量如实记录，不强求等级。
- 拒绝：非场地格 422 `invalid_target`；强制非 Leader 的 `speaker`（Human318）422 `invalid_target`，`forced_role:"speaker"`、`forced_pawn:"Human318"`、`forced_role_reason:"Patrick must hold one of these roles: Great Raider"`；完成后解除 leader，start 409 `conflict`，`data.eligibility.can_start:false`、原因 `You must assign the role of Great Raider in order to start this ritual.`、阻塞 `You need a speaker.`；均无 Lord/推进/operation。

证据：[ritual.json](../../../tools/cua/.runtime/g31-c2-speech-run1/evidence/ritual.json)、[load-control.json](../../../tools/cua/.runtime/g31-c2-speech-run1/evidence/load-control.json)、[cleanup.json](../../../tools/cua/.runtime/g31-c2-speech-run1/evidence/cleanup.json)。加载日志显示 `RedEyeDev.RIMAPI.G31SpeechTest` 与 RIMAPI server 初始化；未捕获该轮 `/proc/<pid>/maps`，加载身份由隔离 mod 路径、磁盘 DLL hash、行为/合同响应共同支持，未把硬编码当运行类型观测。

### 义务选择与 Funeral696 限定样本

- `GET /api/v1/dlc/ideology/ritual/status` 支持可选 `obligation_id`；`POST .../start` 支持可选 body `obligation_id`。ID 是当前 `Precept_Ritual.activeObligations[].ID` 的实际整数，允许 **0**；不是 label、列表位置或目标 Thing。
- 省略 `obligation_id`：无 active 义务时保持普通 anytime/无义务兼容；有义务时按具体原生 `obligationTargetFilter.CanUseTarget(target, obligation)` 自动匹配；多个合法义务时返回 `multiple active obligations match; obligation_id is required`，不静默选一个。
- 显式 ID：必须是本仪式当前 active 义务；不存在 404；目标与义务不匹配 422；不回退其它义务。`GraveWithTarget` 的原生目标关系是墓内 `Corpse` 或该 `Corpse.InnerPawn` 对应 `obligation.targetA`，不是要求 raw target 相等。
- `active_obligations[]` 含 `obligation_id`、`target_a/b/c`、`valid_targets`（原生 `GetTargets`，并为 GraveWithTarget 的 InnerPawn 形状补入实际墓）。Funeral696 无 active 义务且过滤为 GraveWithTarget 时仍返回 `no active obligation is available for this ritual`（422），避免向必须义务的过滤传 null；普通无义务 worker 不受此统一门禁。
- **Funeral696 限定样本**：fixture 暂停下新增一名主教义 colonist，短暂关闭保护后仅原生 `Kill` 该新增人物，自然 `Notify_MemberDied` 生成真实义务（未直接 AddObligation、未显式补发）；该人物 `ClaimGrave` 并原生 `TryAcceptThing` 入墓（先 `DeSpawn`）。义务 ID 0、targetA 死者 Human88474、墓 Grave88473；正常 start 通过，`Lord_3`，moralist Human321，74742→79743（5001 ticks）自然完成，`active_obligation_count:0`，五名参与者获 `GoodFuneral`（Good，mood 5）。证据 `tools/cua/.runtime/g31-c2-funeral-run1/evidence/funeral696-start.json`、`funeral696-result.json`。
- **Funeral751 限定样本**：另一新增主教义 colonist 原生 `Kill` 后尸体原生 `Destroy`，自然 `MemberCorpseDestroyed` 生成义务 ID 1、targetA 死者 Human88474；原生空墓候选为既有 `Sarcophagus3607`（不是 fixture 新建的 Grave88473）。正常 start 通过，`Lord_3`，moralist Human321，74742→79743（5001 ticks）自然完成，`active_obligation_count:0`，五名参与者获 `GoodFuneral`。证据 `tools/cua/.runtime/g31-c2-funeral751/evidence/funeral751-start.json`、`funeral751-final.json`。
- **终态读回差异**：Funeral696 完成后无 active 义务且 GraveWithTarget 必须具体义务，GET 仍返回 422，但 data 带 `completed/last_finished_tick/last_run/outcome_memories`；Funeral751 的 AnyEmptyGrave 在 obligation=null 时原生目标资格可用，完成后 GET 返回 200。runner 按本次 `last_run.lord_id/target` 和 `completed` 读取，不把其它 422 当完成。
- **顶层与 active 候选差异**：顶层无目标 `target_candidates` 仍只调用原生 `GetTargets(selectedObligation,map)`；active 行的 `valid_targets` 对 GraveWithTarget 的 `InnerPawn` 形状补入实际墓。二者范围不同，active 行为本组结果读回；不据此改其它 worker 的 null 义务语义。
- **限制**：本组完成时最后一次独立 GET 因 active 义务已清零返回 422 `no active obligation is available for this ritual`，但 data 仍带 `completed/last_finished_tick/outcome_memories`；这是当前 status 对“义务已原生完成”的读回语义，未改源码。Funeral751（空墓/无尸体）与 SkyLantern698/日期义务未运行，不能用本样本替代；同 key 异体冲突在 Funeral 运行前/后被目标资格优先，未取得 409 代表样本。

### SkyLantern698 / 真实 Date.Tick 限定样本

- 运行时实例 698 的 `is_anytime:false`（源存档无 `isAnytime`，原生 `Precept_Ritual.ExposeData` 缺省 false）；`canStartAnytime` 只是能力，不代表实例值。`RitualObligationTrigger_Date.Tick` 因实例非 anytime 正常执行。
- fixture 原生 spawn `Campfire88473`（fuel 20）、441 个半径为 12 的露天空格、300 木材；披露的日期干预把 `triggerDaysSinceStartOfYear` 从 28 改为 2，`CurrentTickRelative:89742`、`OccursOnTick:115000`、预算 25258 ticks；正常 `Date.Tick` 在 tick 100233 观察到新增义务 0（native `CurrentTickRelative==OccursOnTick` 公式点为 100000；100233 是 HTTP 观察/暂停 tick），未 AddObligation。
- 无 Lord 日期等待由 `pre_arm` guard 从加载起护理 5 名玩家人物并设 70000 tick/420s 上限；Date 义务出现后暂停。正常 GET/dry/start：`Lord_3`、Campfire88473、五名参与者；tick 100233→107735（7502）自然完成，`active_obligation_count:0`，五人 `UnimpressiveSkyLanterns`（Unimpressive，mood -1，随机等级如实）。同 key 非 dry 同体 replay 200/`Idempotency-Replayed:true`；异体 409 `conflict`。
- 限制：本组 SSE 只收到 connected/gameState，未观测 operation 事件（start 200 与 replay/409 已实际读回）；日期只设单次下一触发点，不代表年度全周期；多义务探针未完成。
- 证据 `tools/cua/.runtime/g31-c2-skylantern/evidence/sky698b.json`、`sky698b-result.json`、`cleanup.json`。

### Trial726 / TrialPrisoner728 限定样本

- Trial 的目标是 `IdeoBuildingOrRitualSpot`；leader 是 `IdeoRole_Leader` holder，convict 由 `roles.convict` 强制。`eligibility.trial` 给出 `variant/leader/convict/convict_prisoner/convict_mental/convict_guilty`；`trial_error` 拒绝同一人物兼任、不可走、非 humanlike、不属于殖民者/本殖民地囚犯/奴隶、已 guilty、variant 与人物状态不一致。TrialPrisoner 要求囚犯，TrialMentalState 要求精神状态，普通 Trial 不接受囚犯/精神状态。
- `RitualRoleForced` 的 convict 可能不在 `assigned` 列表；结果归属使用本次 `job.PawnWithRole("leader"/"convict")`，不是 forced 字典冒充 assigned。
- 新增 `trial_result`：`variant/leader/convict/convict_guilty/convict_convicted_memory/convict_exonerated_memory/leader_failed_memory/started_tick/finished_tick/completed`；新增 `outcome`（convicted/exonerated）及 `outcome_applied_tick`。按准确 Game/ritual/Lord 观察原生 Trial.GetOutcome 与 Apply 正常返回；三个 memory 字段为该次结算产生/刷新记忆的快照，排除旧记忆及后来其它 Trial 记忆，不依赖通用 `outcome_memories`。`convict_guilty` 是当前原生状态，不能当作永久定罪结果；不写 guilt/记忆、不强制随机结果。run与快照现随当前Game存档保存，具体跨进程验收见C3限定记录。
- 726 普通：Human321/Human318，`Lord_3`，74742→80009（5267），guilty true + TrialConvicted。728 囚犯：新增 Human88473 经 `guest.SetGuestStatus(...,Prisoner)`，`Lord_3`，74742→80834（6092），guilty true + TrialConvicted。两例 dry/replay/单 operation 通过；只出现 Positive，不宣称 Negative。
- [Codex接续限定自审](../references/reviews/2026-11-08-g31-codex-trial-closeout.md)：727新增人物原生Wander_Psychotic，Human321/Human88473/RitualSpot88476/Lord_3，74742→79743自然定罪。披露的旧三种记忆及结算后再次加入三种记忆均不污染结果。真实访客、错variant/leader422，dry/冻结重放/非dry异体409/单条operation通过。修复Trial leader lock使原生演讲任务看不到角色的异常，改用原生TryAssign实际分配。Negative自然样本、取消/中断/持久化仍未验；本版forced convict实际owned=true，guard始终按准确角色人物护理，不要求其在Lord中。

### Execution / GladiatorDuel / Sacrifice / Scarification 身体结果

这些分支的 `physical_result` 含 `completed` 和 `subjects[]`；每个人物含 `role_id/pawn_id/dead/downed/health/killed_by_ritual/scarification_count/sight/missing_parts/corpse_id/corpse_spawned/corpse_holder`。人物来自本次原生 Lord 的 `prisoner/duelist1/duelist2/animal/target`，死亡后仍保留准确 Pawn/Corpse 关联。身体字段是查询时状态；`completed` 沿用本次 run 完成判据。该关联与终态快照随当前Game存档保存。

[Codex 限定自审](../references/reviews/2026-11-08-g31-codex-execution-duel.md)：711 正常护送/讲话/处决，74742→76412，新增囚犯自然死亡并生成准确尸体，五人 SatisfyingExecution；699 正常原生决斗，74742→82237，观测实际战斗及一人倒地，七人 TerribleDuel。两场 dry、关键角色拒绝、冻结重放、异体409、单条启动事件和保护清理通过；不要求随机出现死亡或特定质量。

[献祭/刺痕限定自审](../references/reviews/2026-11-08-g31-codex-sacrifice-scarification.md)：新增实例752的SacrificePrisoner与SacrificeAnimal分别正常献祭准确新囚犯/牛，74742→83252，原生死亡、准确尸体及五人BoringSacrifice；Scarification753准确义务2指向新人物，74742→83998，scarification_count从0到1、人物存活，六人BoringScarification，送床和结算自然完成。dry、错误受试角色422、冻结重放/409、单条启动事件均通过。fixture新增精确pattern和受试者；刺痕替换副本Scarification issue并调用原生Notify_MemberGained触发原生义务重算，没有施加被测效果或直接添加义务。身体字段仍是当前读值，run和终态快照随存档保存。

## 仪式实例配置（ID-06b）

`GET/POST /api/v1/dlc/ideology/ritual/config` 只覆盖原版 ideoligion 编辑器
`ApplyChanges` 写入的实例字段，角色/目标/obligation 仍走 `ritual/start`。

```json
{"ritual_id": 725, "name": "Boccin Grove Covenant", "is_anytime": true,
 "attachable_outcome_effect": "RechargePsyfocus",
 "trigger_days_since_start_of_year": 33, "dry_run": false}
```

- `name` → `Precept.SetName`；空字符串 400。
- `is_anytime` → `Precept_Ritual.isAnytime`；仅 `can_be_anytime` 且来源 pattern 非
  `alwaysStartAnytime` 时可改，否则 422。
- `attachable_outcome_effect` → `Precept_Ritual.attachableOutcomeEffect`；只接受原生
  `CanAttachToRitual` 通过的 def，未知 404，空字符串清除，仪式不支持 422。
- `trigger_days_since_start_of_year` → `RitualObligationTrigger_Date`；0..59，非日期
  仪式或无 trigger 422；只改 trigger，不重排已有义务到期 tick。
- 读取含 `can_be_anytime/is_anytime/source_pattern/always_start_anytime`、当前与
  可选附带奖励（`can_attach/reason`）、`date_trigger{days,date_string}`、active 义务数。
- 2026-11-10 实测：725 改名后由生产 start 正常开始，`running.ritual_label` 为新名并完成；
  Festival 698 切换 Anytime/日期/奖励读回一致
  （`Reverse/evidence/id06b-ritual-config-20261111.json`，自审
  [ID-06b](../references/reviews/2026-11-10-id06b-ritual-config.md)）。角色/目标/obligation
  仍以既有 start/status 合同为准。

## 未覆盖

- 当前限定样本含 Festival、Conversion、LeaderSpeech、RoleChange、TreeConnection、Funeral、SkyLantern/date、Trial、Execution/Duel、Prisoner/AnimalSacrifice、代表Scarification及Blinding，范围见各节；其它身体改造变体未验，ThroneSpeech等其它DLC分支另计。已验样本不代表同类型全部人物与场地组合。
- 强制角色囚犯正向已在Trial/Execution/Duel限定样本验证；动物正向已在Cow献祭样本验证，全部非玩家组合未验。不得据此声称所有囚犯/动物/非玩家强制操作可用。
- 日期义务及真实obligation目标已有上述限定样本；C3有披露启用allowOtherInstances后的两Lord身份/准确取消样本，不代表默认Def并发或两场自然并行完成。
- C3准确取消/非伤害中断、条件并发身份及动物献祭活动保存→新PID续行→终态保存→新PID读回已[限定通过](../references/reviews/2026-11-08-g31-codex-c3.md)。其它worker分别保存加载、跨地图并行与原生UI运行历史未取样；不承诺跨进程幂等缓存。
- DLC-off 的 start、1.5、第三方 mod、其它程序集版本未在本次运行覆盖；不要从本样本推定 G31 全功能或五 DLC 完成。
