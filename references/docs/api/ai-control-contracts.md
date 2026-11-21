# AI fork 当前接口合同与后续修订边界

2026-11-07更新：本页保留 Core 基础合同，补齐后续 DLC、UI 与 debug 导航；当前验证与部署状态见 [状态页](../harness/current-state.md)。历史独立审阅及修复见[记录](../references/reviews/2026-11-05-dlc-independent-review.md)。
所有路径带前缀 `/api/v1`，JSON 使用 snake_case；API基址以实际配置/run记录为准，`http://localhost:8765`仅为示例。
fork的精确method/path合同集合以`Core/Ai/AiEndpointContracts.cs`和实际能力发现为准；未声明路由保留heuristic，不能推断其DLC或预演能力。详细分域合同见[产品索引](../product-specs/index.md)。

## 当前请求与读回

| 操作 | method/path | body 或查询字段 | 成功含义 / 读回 |
| --- | --- | --- | --- |
| Def 类型 | GET /defs/types | 无 | data.types，含 count/dlc/active |
| Def 列表 | GET /defs | type,offset,limit,fields | data.items；fields 为 def_name,label,def_type 子集 |
| Def 详情 | GET /defs/detail | def_name,fields | data 中的详情投影；另支持 description,short_hash |
| Def 搜索 | GET /defs/search | q,types,limit | 返回匹配项；不提供 `/defs/{defName}` |
| designation | POST /order/designate | map_id,designation_def,cells/thing_ids 或 point_a/point_b,remove,dry_run | 标记已应用/取消，不等于 pawn 已完成劳动 |
| Hediff | POST /pawn/hediff | pawn_id,hediff_def,body_part,severity,count,dry_run | 直接增加；count 默认1，severity 默认-1即保留Def默认 |
| Hediff 删除 | DELETE /pawn/hediff | pawn_id,hediff_def,body_part,all,dry_run | 删除匹配项；不恢复历史效果 |
| 严重度 | POST /pawn/hediff/severity | pawn_id,hediff_def,body_part,severity,dry_run | 直接设置严重度 |
| 精神状态 | POST /pawn/mental-state | pawn_id,state_def,reason,force,dry_run | 尝试进入指定状态 |
| 停止状态 | DELETE /pawn/mental-state | pawn_id,dry_run | 恢复当前精神状态；不回滚期间行为 |
| 灵感 | POST /pawn/inspiration | pawn_id,inspiration_def,reason,dry_run | 尝试触发灵感 |
| 命令 | POST /pawn/order | pawn_id,order,cell/thing_id/target_pawn_id,dry_run | order 为 goto/attack/attack_static；提交 Job，不提供 queue 字段 |
| 区域读取 | GET /map/areas | map_id | label,assignable,cell_count,is_home |
| 区域新建/删除 | POST/DELETE /map/areas | map_id,label,dry_run | 新建 allowed area；删除按 label 匹配并清除已生成 pawn 指派；不承诺所有 Area 子类都可删 |
| 区域单元格 | POST /map/areas/cells | map_id,area,cells 或 point_a/point_b,value,dry_run | area 为标签；value 默认true；当前代码忽略越界单元格，去重后设置，不支持 rect 对象 |
| 角色活动区域 | POST /pawn/area | pawn_id,area,dry_run | 空/null area 取消限制；对应 AreaRestrictionInPawnCurrentMap |
| 条件列表 | GET /map/conditions | map_id | condition_def,label,ticks_left,permanent |
| 条件启动 | POST /map/conditions | map_id,condition_def,duration_ticks/duration_days,dry_run | 重复条件409；无有效时长默认60000 tick |
| 条件结束 | DELETE /map/conditions | map_id,condition_def,all,dry_run | 指定条件或 all:true；不承诺撤销已发生的环境损伤 |
| 穿戴列表 | GET /pawn/apparel | pawn_id | thing_id,def_name,label,forced |
| 穿戴/脱下 | POST /pawn/apparel/wear 或 /remove | pawn_id,thing_id,dry_run | 即时操作；冲突409；当前错误文案不列具体冲突物品 |
| 强制穿戴标记 | POST /pawn/apparel/force | pawn_id,thing_id,forced,dry_run | forced 默认true；修改强制标记，不等于服装政策分配 |
| 服装政策 | GET/POST /pawn/outfit | GET pawn_id；POST pawn_id,outfit_id,dry_run | 读取/分配政策；changed:false不发事件；后续换装由AI执行 |
| Bill 列表 | GET /pawn/surgery | pawn_id | 当前全部 Bill，按 is_medical 识别医疗单 |
| 医疗排单 | POST /pawn/surgery | pawn_id,recipe_def,body_part,thing_id,doctor_id,confirm,dry_run | 原生资格（`AvailableNow`/`AvailableReport`）+ 原生确认；`thing_id` 仅 ImplantXenogerm/ImplantEmbryo；医生在动账单栈前校验，拒绝不留单；queued 不等于完成，见 [物品使用](pawn-use-item.md) 与手术合同 |
| 取消医疗单 | DELETE /pawn/surgery | pawn_id,recipe_def,dry_run | recipe_def 可省略以取消医疗单；不移除普通制作单 |
| 正常物品使用 | POST /pawn/use-item | pawn_id,thing_id,target_thing_id,confirm,dry_run | `CompUsable` 原生资格/确认/目标与 count=1 job；见[合同](pawn-use-item.md) |
| Anomaly 原生交互 | GET/POST /dlc/anomaly/interactable | thing_id,pawn_id,interactable_key,confirm,dry_run | 方尖碑/金立方/脊柱等 `CompInteractable`；见[合同](anomaly-interactable.md) |
| Nociosphere 派遣 | GET/POST /dlc/anomaly/nociosphere[/send] | thing_id,pawn_id,cell,dry_run | 平台内 study≥2、原生目的格与 SkipTo；见[合同](anomaly-nociosphere.md) |
| 玩家能力施放 | GET/POST /pawn/ability | pawn_id,ability_def,target_*,cell,dest_cell,dry_run | 原生 `Ability.GetJob`；自我/单目标/两段式；见[合同](pawn-ability.md) |
| 捕鱼区 | GET/POST/DELETE /map/fishing-zones、POST .../policy | map_id,zone_id,cells,point_a/point_b,policy 字段 | 原生 `Zone_Fishing` 创建与策略；见[合同](odyssey-fishing.md) |
| 局部真空环境 | GET /dlc/odyssey/environment | map_id,x,z | 按格/房间/人物读原生 `Room.Vacuum`；见[合同](odyssey-environment.md) |
| 瓜拉兰树读取 | GET /dlc/ideology/tree | thing_id | 原生 `CompTreeConnection` 连接人物/强度/目标强度/`Mode`/树精；见[合同](ideology-tree.md) |
| 瓜拉兰树策略 | POST /dlc/ideology/tree/policy | thing_id,desired_strength,desired_mode,dry_run | 仅已连接；写原生目标强度/目标种姓，实际修剪/换种姓仍由 WorkGiver 执行 |
| 树精回治疗茧 | POST /dlc/ideology/tree/dryad | thing_id,return_to_healing_pod,dry_run | 仅连接到瓜拉兰树的树精；写原生 mindState.returnToHealingPod，治疗/回树仍由原生 JobGiver 执行 |
| 动物训练读取 | GET /dlc/odyssey/animal/training | animal_id | specialTrainables 的 wanted/learned/steps；见[动物训练合同](odyssey-animal-training.md) |
| 动物训练 | POST /dlc/odyssey/animal/training | animal_id,trainable,wanted,train_now,trainer_id,dry_run | 原生 SetWantedRecursive + 可选 Train job |
| 奴隶/囚犯读取 | GET /dlc/ideology/slavery | map_id | 身份/will/resistance/抑制/互动模式；见[奴隶合同](ideology-slavery.md) |
| 囚犯奴役 | POST /dlc/ideology/slavery/enslave | map_id,pawn_id,warden_id,dry_run | will=0 时走原生 `GenGuest.TryEnslavePrisoner`；不降 will |
| 神经超充读取 | GET /dlc/ideology/supercharger | map_id | 充能/通电/自动使用模式/交互格；见[超充合同](ideology-supercharger.md) |
| 神经超充使用 | POST /dlc/ideology/supercharger/use | map_id,thing_id,pawn_id,dry_run | 原生 `GetNeuralSupercharge`；放电与 hediff 由 driver 完成 |
| 仪式配置读取 | GET /dlc/ideology/ritual/config | ritual_id | 名称/Anytime/来源 pattern/附带奖励/日期触发；见[仪式合同](ideology-ritual.md#仪式实例配置id-06b) |
| 仪式配置写入 | POST /dlc/ideology/ritual/config | ritual_id,name,is_anytime,attachable_outcome_effect,trigger_days_since_start_of_year,dry_run | 只写原版编辑器字段；不代替 start/roles/obligation |
| 造型读取 | GET /dlc/ideology/styling | pawn_id | 当前发型/胡须/纹身/发色、可选集合、造型站与存放染料；见[合同](ideology-styling.md) |
| 造型/染发 | POST /dlc/ideology/styling | pawn_id,hair_def,beard_def,face_tattoo_def,body_tattoo_def,hair_color,station_id,dry_run | 写原生 `next*` 并提交 UseStylingStation/DyeHair；染发消耗 1 个染料 |
| 商队管理 | GET/POST /world/caravan/manage | 见[合同](caravan-manage.md) | split/merge/rest/policy/gear；OD-R06 不属 |
| 任务选择信 | GET/POST /letter/choices | 见[合同](letter-choices.md) | 只扫 ChoiceLetter；只执行原生 DiaOption.action |
| 囚犯/奴隶策略 | GET/POST /guest/policy | 见[合同](guest-policy-native.md) | 新原生策略/调度；旧 /pawn/guest-policy 保留 |
| 装备 utility Verb | GET/POST /utility/verbs | 见[合同](utility-verbs.md) | 只列装备 CompApparelVerbOwner；不是 Ability |
| 业务对话决策 | GET/POST /world/dialog | 见[合同](world-dialog.md) | 只执行原生 DiaOption.action；关闭不是选择 |
| 普通运输舱 | GET/POST /transport/pod | 见[合同](transport-pod.md) | 普通 CompTransporter/CompLaunchable_TransportPod；见详细合同 |
| 商队路线/到达/定居 | GET/POST /world/caravan/journey | 见[合同](caravan-journey.md) | 原生 `Caravan_PathFollower.StartPath`/arrival actions/Settle/Abandon；见详细合同 |
| 商队组建/取消/重组 | GET/POST /world/caravan/forming | 见[合同](caravan-forming.md) | 原生 `Dialog_FormCaravan`/`CaravanFormingUtility`；dry/form/cancel/reform 前置与 4xx 见详细合同 |
| 正常任务选择/接受 | GET /quests/options；POST /quests/accept | quest_id,pawn_id,choices,confirm,dry_run | 原生资格、显式奖励选择和 Quest.Accept；与调试生成分开；见[任务/Archonexus合同](archonexus.md) |
| Archonexus正常控制 | GET /dlc/ideology/archonexus；POST 同路径下 select/tile/ideology/cancel/study/activate | 见详细合同 | 原生携带确认/迁移/研究开关/核心工作，实际终态单独读取；不是queued即完成 |
| 任务生成 | POST /quests/generate | script_def,points,confirm,dry_run | 没有 confirm:true 或 dry_run:true 时生成但不加入 QuestManager |
| 任务移除 | DELETE /quests | quest_id,dry_run | 从管理器移除；不是恢复任务已触发的全部副作用 |
| 语义控件 | GET /ui/controls、GET /ui/controls/resolve | 见 [UI 合同](../ui-semantic-bridge.md) | 观察真实区域和上下文；不执行按钮业务回调 |
| 空间目标 | GET /ui/targets/resolve、POST /ui/targets/focus | 见 [空间合同](../ui-spatial-targets.md) | 四类目标投影/聚焦；不等于点击或业务完成 |
| 原生开发者动作 | GET /debug/native/actions、POST /debug/native/execute | 见 [debug 合同](../debug-native-actions.md) | 白名单直接调用原生方法，包括[即时屋顶编辑](roof-grid-debug.md)；显式调试变更，不走 Cua |
| 调试保护 | GET/POST /debug/invulnerability | POST enabled | 当前 Game 内开关，加载重置；[保护范围](../debug-invulnerability.md) |

新控制 DTO 的 pawn_id 是 string，例 `"321"`；thing_id 和 map_id 按 DTO 是整数。
通用 Def 接口对具体 DLC Def 的可用性仍取决于本机已加载内容。

## dry-run 的边界

**只在 JSON body 放 `dry_run:true`。** 查询串 `?dry_run=1` 没有为这些控制器提供保护，
单独发送它可能执行真实操作。响应字段按服务定义，不统一包成 would_change。

普通校验型 dry-run 不执行所请求的业务写入；Quest 预览会创建对象、可能影响内部状态或随机数，
只承诺不加入管理器，confirm 请求会重新生成。交易预览只承诺无物品/白银转移，
完整合同见 [交易接口](trade-control.md)。反向操作都不是自动回滚。

## SSE、幂等与能力发现现状

operation表示成功提交/应用接口操作，可能没有实际变化；不能用它判断医生、Job或剧情完成。
业务结果先冻结/缓存，再发布operation，再尝试HTTP回应；失败、预演和缓存重放静默。
订阅客户端无必达/持久重放保证。此前“先回复即断连漏事件”并非已证实故障，本轮是顺序强化与独立验证。

能力字段：layer_source:contract表示该method/path有显式合同；覆盖数量以实际能力发现为准。dry_run_mode为none/validate/preview，
idempotency为game_memory、process_memory（Prefs/键位跨菜单环境设置）、quote（交易execute）或none，具体入口以精确合同为准，不能将支持扩展到全部写 API。
旧heuristic的未知字段为空/省略；dlc:null在显式通用合同表示无固定单一DLC要求，具体Def按已加载内容；
在heuristic则表示未声明，不能反推Core-only。

通用幂等见[Idempotency-Key合同](idempotency.md)；game_memory限当前进程/同Game，process_memory跨同进程菜单/Game，两者均30分钟缓存且进程结束不持久。
[服装政策合同](outfit-policy.md)、[验收记录](../references/reviews/2026-11-04-core-closeout-acceptance.md)。
DLC及Core详细合同从[产品索引](../product-specs/index.md)进入；当前开放项与核销看[统一账本](../product-specs/five-dlc-remaining-checklist.md)，阶段结果见[交付记录](../exec-plans/completed/2026-11-13-api-playability.md)。`layer/executor`是执行方式，不是正常玩家/环境/rootdebug角色；按[API使用语义](../product-specs/api-usage-semantics.md)及具体原生流程区分，旧操作矩阵不作当前队列。
