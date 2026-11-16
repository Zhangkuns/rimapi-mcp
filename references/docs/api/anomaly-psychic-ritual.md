# Anomaly 正常心灵仪式（G38）

维护：直接实施Codex；核对：2026-11-08；状态：14玩家Def代表自然效果、共享取消与代表C3限定通过；完整遭遇/全部worker恢复未验。

`GET /api/v1/dlc/anomaly/psychic-ritual`保留原来的16 Def清单与活动清单字段。
新增角色、供品、研究完成、冷却可用tick及`start_supported`。当前`PleasurePulse`、`ImbueDeathRefusal`、
`Philophagy`、`Chronophagy`、`Psychophagy`、`Brainwipe`、`NeurosisPulse`、`BloodRain`、
`VoidProvocation`、`SummonAnimals`、`SummonShamblers`、`SummonPitGate`、`SkipAbductionPlayer`、
`SummonFleshbeastsPlayer`开放正常启动；两个AI专用Def仍可读，启动返回422。

## 资格与准确活动

`GET /api/v1/dlc/anomaly/psychic-ritual/status?def_name=PleasurePulse&map_id=0&thing_id=PsychicRitualSpot123`

- `map_id`省略取当前地图；`def_name`必须存在。
- `roles[]`：`role_def`、label、原生min/max_count、can_handle_offerings。
- `offering`：原生count与allowed_defs；PleasurePulse要求30Bioferrite。
- `research_finished`、`research_prerequisite`、`available_tick`来自实际研究与原生冷却管理器。
- `targets[]`列出玩家心灵仪式点及各自`blocking_issues`。根级同名字段用于请求的thing_id；省略thing_id时会报告缺目标，不表示已有活动失败。
- 有thing_id时返回`candidates[]`，逐role列出原生候选池人物、`pawn_id`、eligible与原因。不自动选择参与者。
- `active[]`保留map_id/def_name/label/participants，补充准确lord_id、psychic_ritual_id、run_id、target、roles、stage、power_percent和可用时ticks_left。
- `power_percent`实际访问链为`Lord.CurLordToil`→`LordToil_PsychicRitual.RitualData.psychicRitual.PowerPercent`；没有虚构LordJob字段。

`.../status?def_name=PleasurePulse&run_id=psychic-...`选择本Game中API创建的准确记录。
显式map_id必须匹配。不存在/错Def/错地图返回404。`runs[]`列出该Def/地图的记录，
`last_run`为选中记录或该范围最后记录。多个活动均列出，不能把同Def旧Lord冒充新run。

## 正常启动

`POST /api/v1/dlc/anomaly/psychic-ritual/start`

```json
{"def_name":"PleasurePulse","map_id":0,"thing_id":"PsychicRitualSpot123",
 "roles":{"Invoker":["Human321"],"ChanterAdvanced":["Human318"]},"dry_run":false}
```

精确合同：`command / pawn_ai / dry_run:validate / idempotency:game_memory / dlc:anomaly`。
支持`Idempotency-Key`、冻结响应重放、异体409与一次operation事件，边界见[幂等合同](idempotency.md)。
完成原生条件检查的dry-run返回`would_start:true, applied:false, outcome:validated`，
不创建Lord、不登记冷却或消耗供品。SummonAnimals/Shamblers的原生对话框依赖InitializeCast
缓存；预检暂时调用它并finally还原准确缓存与RNG状态，其它Def不增加该预览调用。

roles使用`PsychicRitualRoleDef.defName`到人物ThingID数组。Invoker必须1人，ChanterAdvanced
可0~6人。省略的可选角色保持空；不自动填充。未知角色/空列表对象返回400，重复人物、
非原生候选或不能担任指定角色返回422。人物资格使用原生候选池和确认窗口的TryAssign上下文。
响应中的动态角色键保留原生大小写，可直接用于请求。ImbueDeathRefusal另需一名
`DeathRefusalTarget`和一枚Shard；原生效果增加死亡拒绝次数并降低目标技能经验，不执行复活测试。

四种伤害/昏迷目标类型同样使用精确的target角色，不提供直接设置质量或结算结果：

| Def | 必需目标角色 / 供品 | 原生效果 |
| --- | --- | --- |
| Philophagy | PhilophagyTarget / 1 Shard | 从目标可转移技能取经验给Invoker；目标脑伤与DarkPsychicShock |
| Chronophagy | ChronophagyTarget / 20 Bioferrite | Invoker逆龄、目标增龄及脑伤/昏迷；原生年龄门禁和疾病/死亡可能性保留 |
| Psychophagy | PsychophagyTarget / 20 Bioferrite | 目标PsychicallyDead、脑伤/昏迷，Invoker获得有期限Psychophage |
| Brainwipe | BrainwipeTarget / 1 Shard | 原生清除适用记忆、招募抵抗/意志和教义确定度，恢复可招募；BrainwipeComa |

角色人数和资格、Philophagy无可转移技能及Chronophagy年龄等阻塞沿用原生判据。
效果可能伤害、杀死目标或留下长期状态；`completed`不保证目标存活或最终招募成功。

环境/事件组的实际效果与原生随机性：

| Def | 原生效果 |
| --- | --- |
| NeurosisPulse | 地图人物新NeurosisPulse，替换适用旧PleasurePulse |
| BloodRain | 实际BloodRain条件，带psychicRitualDef与剩余tick |
| VoidProvocation | 选择适用事件后延迟入队；AN-01 已验证事件实际触发、准确实体生成与图鉴发现，见下 |
| SummonAnimals / SummonShamblers | 延迟事件队列；动物可能为HerdMigration或FrenziedAnimals，后者为Manhunter |
| SummonPitGate | 入队PitGate，随后spawner与真实地穴门；仪式完成时不保证门已生成 |
| SkipAbductionPlayer | 原生从适用地图候选/世界fallback选择目标并移到场地、施加DarkPsychicShock；请求不指定被绑架人物 |
| SummonFleshbeastsPlayer | 先PitBurrowSpawner，后实际肉兽；kind可能叫Toughspike等，不保证以Fleshbeast命名 |

提交依次检查正常玩家Def、研究（独立于godMode）、原生CanInvoke（冷却/地图限制）、玩家
心灵仪式点与占用/阻塞/IsValidTarget、角色和Def的BlockingIssues（含供品可达）。
供品由原生AI选择和搬运，客户端不预消耗或手选堆栈。没有当前Game返回409；缺DLC返回422；
不存在Def/地图/目标返回404；缺必要字段返回400；业务资格/当前冷却拒绝返回422。

API在游戏主线程临时切到请求地图，调用原生InitializeCast→MakeNewLord，并按原生确认窗口
登记冷却，随后恢复当前地图。返回`outcome:started, applied:true`只表示已独立读回新建
Lord及其PsychicRitual；同时给出run_id、准确角色/目标、参与者与available_tick。
原生异常或部分效果按共享写操作分类返回，不能换key盲重试。

## 准确取消与中断

`POST /api/v1/dlc/anomaly/psychic-ritual/cancel`

```json
{"run_id":"psychic-...","dry_run":false}
```

沿用start的精确写合同、幂等和事件。缺run_id返回400；本Game无此记录返回404；记录已终结
或无准确活动Lord返回409。dry-run只验证，不修改活动、冷却或取消时间。实际调用
`CancelPsychicRitual`并读回原生取消；`cancel_requested_tick`记录API请求时间，原生自行中断则为null。
取消的资源/冷却后果由原生阶段决定，不保证回滚：已验PleasurePulse在尚未推进的主动取消中
保留30供品并清除冷却。场地销毁后的取消需要游戏tick；已验自然取消含原生原因。
暂不提供对无API run记录的UI仪式的取消写入。

## 终态与结果边界

`run_state`为active/completed/cancelled/unknown；`completed`分别为false/true/false/null。
完成观察准确Lord的原生RitualCompleted分支，取消观察CancelPsychicRitual（含原因）。
仅Lord消失而没有原生终态观察时为unknown。原生`succeeded`只是调用阶段状态，不能单独证明
最终效果；完成仍需业务结果读回。

`last_run`冻结开始身份，包含started_tick、terminal_tick、initial_observation和terminal_result。
终态快照保存观察tick、原生succeeded/cancelled、power_percent、供品观察总数和地图人物
PleasurePulse hediff ID/剩余tick。initial_observation是在创建Lord后采集。供品计数包含地图
可搬物、已生成地图人物的背包/搬运容器；它是当前观察，不是库存预留或精确消耗归因。
同样，既有hediff可能存在；只有独立开始基线和终态差值才能证明本次效果，不能把快照
中的全部效果自动归于当前run。cancel_reason保留原生原因文本。
`role_pawns[]`另记录每个准确角色人物的pawn_id、dead/downed、死亡拒绝hediff ID/uses_left及
技能累计经验。这些仍是观察；技能字典键在冻结快照内保留原生Def名。
另含age_biological_ticks、psychic_sensitivity、hediffs（ID/Def/部位/严重度/期限）、
memories（Def名）、certainty和recruitable/resistance/will，供上述不同效果的前后读回。

根级`map_effects`及冻结观察中的同名字段包含：`queued_incidents`（Def/fire_tick/points/
point_multiplier/forced）、`conditions`（ID/Def/ticks_left/psychic_ritual_def）、`pulse`（人物/
hediff ID/Def/期限）、`map_pawns`（ID/kind/race/faction/downed/mental_state/位置）及
`objects`（PitGate/PitGateSpawner/PitBurrow/PitBurrowSpawner的ID/Def/位置）。
这些是当前地图范围观察，不是自动归因；已有事件、条件和人物须与独立开始基线区分。
仪式终态快照不会随延迟事件到达改写，实际到达应再查根级当前字段。

记录随Game序列化。稳定接续键为当前Game的map_id/Lord load ID/Def与API run_id。
原生`PsychicRitual.ExposeData`不保存自身loadID，重启后可变化；`last_run.psychic_ritual_id`和
冻结identity保留开始时ID，`active[].psychic_ritual_id`报告当前原生ID。不能以两者不等判成另一活动。
ImbueDeathRefusal已验证活动跨进程接续、自然效果和终态再次载入；不外推其它worker。
game_memory缓存不跨Game或进程持久化；UI创建的仪式可读活动，不自动获得API run历史。

## VoidProvocation 完整遭遇（AN-01）

2026-11-08 在 `ai_g31_fixture` 副本（tick 74742，隔离 build/profile/mod）用本端点正常开始
VoidProvocation：仪式在 tick 79936 结束，`Find.Anomaly.hasPerformedVoidProvocation` 为 true，
原生 `PsychicRitualToil_VoidProvocation.ApplyOutcome` 把 `HarbingerTreeProvoked` 以
fire_tick 85063 入队（原生 2~4 小时延迟）。推进到 85063 后原生
`IncidentWorker_HarbingerTreeProvoked` 实际执行：队列清空、`Plant_TreeHarbinger88599` 在
(223,205) 生成（growth 0.15），`EntityCodex` 的 Basic 已发现数 1→2、`HarbingerTree` 由未发现变已发现。

- 图鉴发现是该 Def 的业务结果（发现新实体才推进巨石/研究），因此“完整遭遇”以
  `GET /dlc/anomaly/monolith` 的 `codex.provocation_entries[].discovered` 与真实生成对象判定，
  不以 incident 入队判定。
- 同场 Invoker `Human88474` 获得原生随机 `DarkPsychicShock`（downed，生命 1.0），属该 Def 的原生
  随机结果；未把 downed 当失败。
- 证据 `Reverse/evidence/an05-an01-voidprovocation.json`；runner
  `tests/g38_environment_integration.py` 的 VoidProvocation 分支新增
  `wait_effect`→`effect_observed` 段，fixture 仅记录计划中的 def/fireTick 对并等待其离开队列。
- 本次存档原生只选到 HarbingerTree；Ghoul/Gorehulk/Fleshbeast/Shambler 等其它随机挑衅事件、
  长期波次与实体后续行为未逐个复现。

## 已确认原生问题与待验范围

1.6.4871原生HoldRequiredOfferings会在遍历背包时转移整堆供品，导致集合修改异常和取消。
RIMAPI针对该方法使用背包集合副本，保留原生供品筛选、计数和TryTransferToContainer逻辑；
正常UI也经此修复。修复前自然取消证据保留，不能算正常施放通过。

C1自然闭环、关键拒绝和修复回归已[限定通过](../references/reviews/2026-11-08-g38-codex-c1.md)。
C2目标/碎片、取消/场地中断及代表C3已[限定通过](../references/reviews/2026-11-08-g38-codex-c2-c3.md)。
四种伤害/昏迷目标自然效果亦已[限定通过](../references/reviews/2026-11-08-g38-codex-target-effects.md)。[环境/事件/召唤八类](../references/reviews/2026-11-08-g38-codex-environment-effects.md)已限定通过；
肉兽首版runner漏识别、绑架两次失败及各自限制保留。完整遭遇、所有随机分支、各worker分别恢复、
跨地图并行与完整Cua施放未验。旧只读证据保留于`Reverse/evidence/dlc-G-31-38.json`。
