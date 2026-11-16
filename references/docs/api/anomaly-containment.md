# Anomaly 收容与原生工作（G-37）

交付范围：读取地图上的收容平台、被收容/搬运中实体与逃逸状态，以及立即弹出单个已收容实体。
[正常捕获/搬运中及已收容新进程恢复](../references/reviews/2026-11-08-g37-codex-capture.md)已限定通过。
[模式与正常研究/释放/处决及结果恢复](../references/reviews/2026-11-08-g37-codex-work.md)已限定通过；
平台建造/拆除仍未交付，完整范围与边界见自审。
[正常平台间转移及活动/目标终态恢复](../references/reviews/2026-11-08-g37-codex-transfer.md)亦已限定通过。
[活动策略/正常抑制及结果新进程恢复](../references/reviews/2026-11-08-g37-codex-activity.md)已限定通过，
G37本轮正常工作链合同收口；平台内抑制等未验边界仍按报告保留。

证据：`Reverse/evidence/dlc-G-37-held.json`（20 项）、`dlc-G-37-empty.json`（14 项）、
`dlc-G-37-nogame.json`（无 Game 路径）、`dlc-G-37-release.json`（释放 16 项）。
fixture：`ai_g37_contained`（平台 + 已收容 Gorehulk + 未收容 Noctol）、
`ai_g37_release_fixture`（同一状态的可重复释放测试副本）。

## 请求

`GET /api/v1/dlc/anomaly/containment?map_id=0`

`map_id` 省略时用当前地图；非整数 400，未知地图 404。

## 响应

```json
{"map_id":0,"platform_count":1,"entity_count":82,"held_entity_count":1,"escaping_count":0,
 "platforms":[{"thing_id":66734,"def_name":"HoldingPlatform",
   "position":{"x":157,"y":0,"z":120},"occupied":true,
   "held_pawn_id":"Gorehulk66735","held_pawn_def":"Gorehulk",
   "containment_strength":0.0,"available":false,
   "has_electroharvester":false,"has_bioferrite_harvester":false}],
 "entities":[{"thing_id":66735,"thing_id_string":"Gorehulk66735","def_name":"Gorehulk",
   "position":null,"held":true,"platform_id":66734,"is_escaping":false,
   "containment_mode":"Study","can_study":true,"studied_at_platform":true,
   "can_be_captured":false,"downed":true}]}
```

## 语义与取证

- 平台行来自 `Building_HoldingPlatform`：`occupied`、`HeldPawn`、
  `CompEntityHolder.ContainmentStrength/Available`、两种采集器挂载标志。
- **被收容的实体是 despawn 状态**，不在 `map.listerThings` 中，因此实体行有两个来源：
  地图上已生成的实体，以及每个平台 `GetDirectlyHeldThings()` 内的实体；按 `thing_id` 去重。
- 实体行的 `held`/`platform_id` 来自 `CompHoldingPlatformTarget.CurrentlyHeldOnPlatform`
  与 `HeldPlatform`；`containment_mode` 是 `EntityContainmentMode`
  （`MaintainOnly|Study|Release|Execute`）；`is_escaping`、`can_study`、
  `studied_at_platform`、`can_be_captured` 都是原生属性读回。
- 被收容实体的 `position` 为 `null`（未生成在地图上），未收容实体给坐标。
- `thing_id` 是原生 `thingIDNumber`（数字），`thing_id_string` 是 `ThingID`；
  两者同一对象，跨存档只保证同一存档内的数字一致。

## 采集设施与持有实体活动（AN-02）

2026-11-08 在 `g37_studied` 副本（tick 79920，隔离 build/profile/mod）用通用
`POST /builder/blueprint` 正常放置蓝图，由殖民者完成 `Electroharvester` 与
`BioferriteHarvester` 建造（17,718 tick）；平台行追加两个采集器对象，实体行追加 `activity`：

```json
{"platforms":[{"thing_id":88473,"held_pawn_id":"Gorehulk88478","occupied":true,
  "has_electroharvester":true,"has_bioferrite_harvester":true,
  "electroharvester":{"thing_id":101849,"thing_id_string":"Electroharvester101849",
    "power_on":true,"power_output":400.0,"platform_ids":[88473]},
  "bioferrite_harvester":{"thing_id":101643,"thing_id_string":"BioferriteHarvester101643",
    "power_on":true,"contained_bioferrite":1.175,"ready_for_hauling":false,
    "unloading_enabled":true,"platform_ids":[88473]}}],
 "entities":[{"thing_id_string":"FleshmassNucleus101857","activity":{
   "activity_level":0.4013,"state":"Passive","is_dormant":true,"can_be_suppressed":true,
   "suppression_enabled":true,"suppress_if_above":0.4,"can_suppress_forced":true}}]}
```

- 设施来自平台 `CompAffectedByFacilities.LinkedFacilitiesListForReading`；`power_output` 是
  `CompPowerPlantElectroharvester` 按**占用平台**计算的原生输出（Gorehulk 400W、
  FleshmassNucleus 200W），`contained_bioferrite` 是
  `Building_BioferriteHarvester.containedBioferrite`（原生 30 才进入搬运阈值）。
  未挂载时字段为 `null`，布尔 `has_*` 保留旧形状。
- `activity` 是实体的原生 `CompActivity` 读数（无该组件为 `null`），用于平台内抑制前后读回。
- 平台内抑制复用既有 `POST /activity/suppress`（`work_target_id` 为平台，实体仍在平台内）：
  先把 `activity/policy` 的 `suppress_if_above` 设到原生下限 0.1，再强制一次工作，22,697 tick
  由 0.4013 降到 0.0108 并自然结束；低于原生 `considerMinActivity` 0.05 后不能再强制新工作，
  这是原生限制，不是接口拒绝缺陷。
- 实测链：正常释放 `Gorehulk88478`（立即弹出）→ 平台腾空 → 捕获测试生成体
  `FleshmassNucleus101857`（2,565 tick 入平台）→ 平台内抑制 → 采集器产出与电力读回。
- 测试准备与限制：复制存档的钢/组件大部分为 forbidden（早期收容测试准备），runner 用
  `/item/spawn` 在平台外补非 forbidden 材料；Nucleus 由隔离 fixture 生成（dormant、未 downed），
  捕获、模式、抑制均为产品端点。未验 Bioferrite 30 单位搬运阈值、采集器断电/占用/多平台、
  其它活动实体与长期周期。证据 `Reverse/evidence/an05-an02-facility.json`。

## fixture 与实测

| 场景 | 结果 |
| --- | --- |
| 平台 + 收容 Gorehulk | `platform_count=1`、`held_entity_count=1`，`platform_id` 与平台 `thing_id` 一致 |
| 同场景存读档 | 平台、实体、绑定关系在重启进程后保持 |
| 未收容 Noctol | `held:false`、`platform_id:0`、`containment_mode:MaintainOnly` |
| 无平台存档 | `platform_count=0`、`held_entity_count=0`、`platforms:[]` |
| 连续两次读取 | 计数与实体 id 不变（只读，无副作用） |
| 无 Game | 409 `conflict` "No active game" |

fixture 由临时探针构造（`Reverse/evidence/dlc-static/g37-probe.cs.txt`）：
生成 `HoldingPlatform`、生成并击倒 Gorehulk、despawn 后放入平台 `ThingOwner`，
再调用 `CompHoldingPlatformTarget.Notify_HeldOnPlatform`。探针已移除，fixture 存档保留。

## 立即弹出（immediate eject）

`POST /api/v1/dlc/anomaly/containment/release`

> 这是**立即弹出**，不是原生的正常释放工作：`JobDriver_ReleaseEntity` 还会
> `Escape(false)`、把 `containmentMode` 重置为 0、并通知执行者精神状态。
> 正常流程由下方`containment/work`独立提供。

```json
{"platform_id":66734,"confirm":true,"dry_run":false}
```

成功数据：
`{platform_id,entity_id,entity_def,containment_mode,platform_occupied_before,changed,applied,dry_run,
platform_occupied_after,platform_holder_count_after,released_entity_id,released_spawned,
released_position,released_held,released_platform_id,readback_ok}`。

规则（副本实测，见 `dlc-G-37-release.json`）：

- 原生路径是公开的 `Building_HoldingPlatform.EjectContents()`：实体从平台 `ThingOwner`
  回到地图，平台的 `Occupied`/`HeldPawn` 清空，`CompEntityHolder.Available` 变回 true。
- 实测释放后实体**生成在平台所在格**，`held:false`、`platform_id:0`、仍 `downed`、
  `is_escaping:false`；不主动改派系/心情，也不解除倒地。
- 空平台释放 409 `conflict`（"no contained entity"），未知平台 404，缺 `confirm` 400。
- `dry_run` 只投影，不调用 eject；同 key 重放返回原正文，不二次 eject、不发第二条事件。
- **不以“无异常”当成功**：`EjectContents` 忽略 `ThingOwner.TryDropAll` 的返回值，
  执行后按观察分类：
  `released`（实体已不在原 holder、平台空、实体在新地图上且关系清空 → 200 `applied:true`）、
  `not_released`（仍被收容且未生成 → 500 `applied:false`）、
  `partial_or_contradictory`（500，`applied` **缺省**）、
  `unknown`（读回失败 500，`applied` **缺省**，`readback_ok:false`）。
  缺省表示“未知”，不是“未执行”；`dry_run` 的 `applied:false` 才是未执行的准确投影。
- 归属与身份判据：`original_holder_contains_entity`、`released_on_platform_map`
  与 `released_held`/`released_platform_id` 一起决定是否 `released`。
- 原生抛异常仍返回 500 并缓存，但观察到的已执行事实保留（`applied:true` + `native_error`）。
- 非玩家可控平台 422 `invalid_target`（`Faction==player`，或无派系但 `Map.IsPlayerHome`）；
  dry-run 同规则。
- 原生调用抛异常时返回 500 `uncertain:true` 并读回实际状态。
  注入“调用后抛”时读回显示平台已空、实体在地图上——请求体不能当结果证据。
- 存读档后保持已释放状态。

### 证据来源（分别归档，不混算）

| 文件 | 内容 |
| --- | --- |
| `dlc-G-37-release.json` | 16 项集成断言（dry-run、确认门、未知/空平台、重放、事件、存读档），**不含故障注入** |
| `dlc-G-37-release-faults.json` | 注入：静默不弹出、前抛、后抛、读回抛、非玩家派系拒绝；含计数器与重放不二次执行 |
| `dlc-G-37-release-guards.json` | 新 POST 的无 Game 409 与 DLC-off 422 |

## 历史读取/立即弹出证据的限制

- 历史立即弹出未观测`is_escaping:true`（该调用不执行`CompHoldingPlatformTarget.Escape`）。
- 原生实体捕获Job为`CarryToEntityHolder`；`Capture`实际上是Core囚犯捕获，旧表述在此更正。
  该历史证据不含处决（`EntityContainmentMode.Execute` / `JobDriver_ExecuteEntity`）终态。
- 历史收容模式（MaintainOnly/Study/Release/Execute）只读；实测仅把字段改成 `Release`
  并推进 300 tick **不会**自动释放，因此没有以模式切换冒充释放。
- 历史证据未测正常释放/处决（`Release`/`Execute` 模式）后的状态迁移。
- 平台被摧毁、实体被搬运到另一平台等失效/迁移状态未测。
- `containment_strength` 只观测到 0.0（无墙体/门加成）；非零值未构造。
- DLC-off 拒绝见批次末的批量验证记录，不在本模块单独重启验证。

## 正常捕获（限定通过）

`POST /api/v1/dlc/anomaly/containment/capture`

```json
{"map_id":0,"platform_id":88473,"entity_id":"Gorehulk88478","pawn_id":"Human88474","dry_run":true}
```

map/platform数字ID、entity/pawn准确ThingID均须明确。未知对象404，缺字段400；原生CanBeCaptured/
StudiedAtHoldingPlatform、Manipulation、可用殖民者、空玩家平台及双方禁选/预留/可达门禁不满足422。
已收容、当前正在捕获同实体或目标平台占用409。当前范围为地图上未被搬运的实体到空平台；
其它carrier交接不在capture范围；平台间transfer由下方独立端点提供。

dry不设置targetHolder、不创建Job。正式请求复用原生Capture菜单/StudyUtility选定平台的designation赋值，
再JobMaker(CarryToEntityHolder,platform,entity)/count1→TryTakeOrderedJob。applied/submitted只证明工作提交；
submitted_job返回准确job_id、pawn/platform/entity。不会直接Despawn/写入ThingOwner/Notify_HeldOnPlatform。
若原生拒绝或异常发生在designation之后，返回错误并保留target_holder_id；applied缺省表示不能据此断言无副作用。
不承诺回滚，先读取实际实体与工作，再决定后续命令；同key重放不重新执行。

strength/minimum_containment_strength/safely_contains如实返回；原生允许低强度捕获，API不将其伪装为安全收容。
GET沿地图、平台ThingOwner及搬运者carryTracker读取同实体，增加spawned/dead/target_holder_id/carrier_id；
capture_jobs展示当前地图准确工作。最终以平台HeldPawn、实体held/platform_id和未死亡共同确认收容；
carrier_id有值且spawned=false是搬运中，不是已收容。

本组74742→74901搬起/活动保存，新进程75336完成收容，再新进程准确恢复已收容关系。
原生完成后临时target_holder_id会清空，应以held/platform_id读回；低强度0/最低60不等同长期安全收容。
其它实体/长期逃逸及新POST的DLC-off现场未验，不沿用历史其它POST守卫证据。

## 模式策略

`POST /api/v1/dlc/anomaly/containment/mode`

```json
{"map_id":0,"platform_id":88473,"entity_id":"Gorehulk88478","mode":"Study","dry_run":true}
```

map/platform数字ID与entity准确ThingID必填，mode只接受MaintainOnly、Study、Release、Execute，大小写准确，
不接受数字或组合。选择准确玩家平台中当前存活实体；缺参数/非法mode400，未知map/platform404，
空平台或实体身份不符409，非玩家平台/原生canBeExecuted=false时的Execute422。
在游戏主线程按原生ITab_Entity单选按钮赋值containmentMode；Execute无额外自创确认门。
该策略可供普通worker随后执行；设置成功不代表释放、研究或处决完成，也不主动取消已有Job。

返回previous_mode、当前mode、requested_mode及准确身份。dry不写；正式仅发生变化时applied=true并发operation，
重复设置当前模式applied=false。game_memory同key冻结重放，异体409；缓存不跨Game/进程。

## 正常研究、释放和处决

`POST /api/v1/dlc/anomaly/containment/work`

```json
{"map_id":0,"platform_id":88473,"entity_id":"Gorehulk88478","pawn_id":"Human97157","action":"study","dry_run":true}
```

action明确release、study或execute，须已选择对应模式。沿用mode的准确持有身份校验，再校验地图上可用殖民者，
原生工作类型分配/能力、drafted、requiredCapacity、平台禁选/可达及HasJobOnThing预留等门禁。
缺pawn/action或非法action400，未知pawn404，人物/模式/原生工作资格不满足422，当前同人物同平台工作409。
study通过原生CurrentlyStudiable，禁用/冷却/无知识等原生拒绝不会被强行清除。

dry只校验，不创建Job。正式JobOnThing→标记workGiverDef→TryTakeOrderedJobPrioritizedWork，
分别为ReleaseEntity(platform,entity)、StudyInteract(platform,null,entity)和ExecuteEntity(platform)。
成功applied仅指提交，submitted_job包含准确job_id/pawn/platform及target_b/target_c。共享主线程协调器提供
game_memory逐字节重放、异体409和成功单条operation；异常不承诺回滚，先读当前工作与实体状态。

GET新增work_jobs（当前平台工作）；实体新增can_be_executed与study：enabled、currently_studiable、study_points、
progress_percent、last_studied_tick、ticks_til_next_study、study_interactions、anomaly_knowledge_gained、knowledge_category。
它们是当前原生读值，没有冻结为运行结果。原生研究完整一轮含五次交互，期间知识/点数可先增长，完整结束才更新
last_studied_tick与冷却，并把study_interactions重置0；不能用该计数必须大于0判定完成。

正常Release由JobDriver调用Escape(false)，生成实体、清空holder、is_escaping=true、模式MaintainOnly。
麻醉未结束时仍可downed；不要求它立刻起身/攻击才证明释放。Execute由JobDriver调用DoExecutionByCut，
须实际死亡/准确尸体和平台清空；GET把地图尸体的InnerPawn也纳入实体读取，新增corpse_id/corpse_spawned，
死亡后不因地图上已无活Pawn而丢失该身份。未保持尸体的销毁/搬运情况不由本接口承诺永久身份日志。

## 正常平台间转移

`POST /api/v1/dlc/anomaly/containment/transfer`

```json
{"map_id":0,"platform_id":88473,"destination_platform_id":97154,
 "entity_id":"Gorehulk88478","pawn_id":"Human97151","dry_run":true}
```

map_id、源platform_id、destination_platform_id、entity_id、pawn_id均须明确；源必须当前持有准确存活实体，
目标必须是同地图另一空玩家平台。缺参数400、未知对象404、同源目标或无玩家归属/工作资格422，
源实体不符、目标占用或当前正在转移同实体409。人物必须具备并分配Hauling/Manipulation；源/目标/实体
禁选、预留和可达沿用原生优先工作条件。低收容强度仍按原生允许，返回真实strength/minimum/safely_contains。

dry不改targetHolder、不创建Job；因原生HasJobOnThing要求已有designation，干跑按明确源/目标/实体核对应门禁。
正式设置targetHolder→原生WorkGiver_TransferEntity.HasJobOnThing/JobOnThing→
TransferBetweenEntityHolders(source,destination,entity)/count1→TryTakeOrderedJobPrioritizedWork。
原生driver负责取出/搬运/再次绑定；不直接Eject/Despawn/插入holder。applied仅证明工作提交。
submitted_job和GET当前work_jobs增加destination_platform_id，target_b为目标平台ThingID，target_c为准确实体。

designation后拒绝/异常可能留下普通hauler可执行的目标，错误响应保留target_holder_id并省略applied；
不将其当无副作用或承诺回滚。共享game_memory缓存冻结同key原文、异体409，成功仅一条operation。
途中GET实体仍沿carrier读取；准确完成须源空、目标HeldPawn/ThingOwner/实体HeldPlatform一致且实体存活。
原生完成清空target_holder_id；该字段空不是失败。活动/终态恢复范围见本阶段证据，不承诺任意第三方holder或地图间转移。

## 实体活动度与正常抑制

`GET /api/v1/dlc/anomaly/activity?map_id=0&target_id=WarpedObelisk_Duplicator97171`

map省略用当前图，target省略列出该图所有CompActivity，包括地图生成物、平台内实体与人物载体。
非整数map400、未知map/target404、指定物无CompActivity422，无Game409/DLC缺失422。
targets含准确target_id/def/map/platform/spawned、activity_level（0..1）、state、is_active/is_dormant/deactivated、
can_be_suppressed、suppression_enabled、suppress_if_above、can_suppress_forced、activity_research_factor；
suppression_jobs展示当前Job/pawn/work_target_id与原生driver绑定的target_id。state是原生枚举字符串，
本版Passive对应is_dormant=true；使用明确布尔字段判资格，不把Dormant当作必然的字符串值。

`POST /api/v1/dlc/anomaly/activity/policy`

```json
{"map_id":0,"target_id":"WarpedObelisk_Duplicator97171","enabled":true,"suppress_if_above":0.25,"dry_run":true}
```

map/target、enabled与阈值均明确；阈值遵从原生ActivityGizmo滑条0.1..0.9（含端点），缺失/非法400。
原生策略控件只在非deactivated的Dormant目标且CanBeSuppressed时可用，不满足422。
游戏主线程写原生suppressionEnabled/suppressIfAbove，返回before/current/请求值；dry不改，正式仅实际变化applied=true。
它控制普通worker自动选择，设置不降低活动度、不保证任何worker会工作，也不主动取消当前优先命令。

`POST /api/v1/dlc/anomaly/activity/suppress`

```json
{"map_id":0,"target_id":"WarpedObelisk_Duplicator97171","pawn_id":"Human97168","dry_run":true}
```

准确map/target/pawn必填；未知404，缺字段400。须可用玩家殖民者、分配且具备Warden/Manipulation、非不允许的drafted状态，
原生ActivitySuppressionRate未禁用且大于0。ActivitySuppressionUtility(target,true,true)检查当前非激活态、
可抑制及至少原生最低活动度；需要平台的Pawn须当前被收容，倒地等沿用原生拒绝。
支持地图生成物与平台内实体，按work_target（平台或实体自身）检查禁选/预留/可达及邻接交互格，不满足422；
同人物正在该目标执行ActivitySuppression返回409。

显式优先命令采用playerForced=true，因此可绕过自动策略开关及阈值，不绕过激活态/最低活动/人物能力。
dry只校验，无Job分配。该scanner继承默认HasJobOnThing→JobOnThing，故预检直接复用原生utility并核对应工作条件，
不调用会分配Job的默认HasJob。正式只调用一次WorkGiver_Warden_SuppressActivity.JobOnThing，
标记workGiverDef后TryTakeOrderedJobPrioritizedWork；返回submitted_job、速率与当时activity，applied仅代表提交。
原生JobDriver逐tick降低活动，正常低于0.01后结束；实际结果以GET当前值/工作及独立终态读回为准，不由提交响应冒充。
生产实现不直接SetActivity/AdjustActivity/Deactivate或强制完成，暂未提供激活/终局工作。

两个写端点经共享协调器提供主线程、game_memory逐字节重放、异体409和成功单条operation；缓存不跨Game/进程。
异常不承诺回滚，先读实际策略、活动值及当前工作。其它Activity worker、平台内抑制、危险激活与活动工作恢复的
实测范围单独留档，不把单个方尖碑的成功算全部Anomaly生命周期通过。

## 取消待执行捕获/转移（G-43）

`POST /api/v1/dlc/anomaly/containment/cancel`

```json
{"map_id":0,"entity_id":"Gorehulk75410","dry_run":false}
```

- 只取消**捕获/转移**这一对一次性命令：`CarryToEntityHolder`（targetB=实体）与
  `TransferBetweenEntityHolders`（targetC=实体）。`release/study/execute` 是模式驱动的重复工作，
  结束一个 job 后原生 work giver 会立刻重新接单，因此不纳入 cancel；要停止这些工作请改
  `containment/mode` 或征召执行者。
- 实现顺序：先清 `CompHoldingPlatformTarget.targetHolder`（原生 designation），再
  `EndCurrentJob(JobCondition.InterruptForced)` 当前 job、`JobQueue.Extract` 排队 job 并
  `Cleanup`。若先结束 job 再清 designation，work giver 会在同一 tick 重建同名 job（G-43 实测到的缺陷）。
- `applied:true` 需要：无剩余捕获/转移 current/queued job 且 `targetHolder == null`。
  dry-run 只报告 `current_jobs/queued_jobs/pending_target_holder_id` 与 `would_cancel`。
- 无待取消命令（例如实体已被收容、只剩正常研究 job）返回 409，不弹内容、不改 holder。
- 实测：新 fixture 平台 75405/实体 Gorehulk75410/人物 Human75406；提交捕获后 cancel 清空
  designation 与 job，再次提交同一捕获可正常完成收容（跨 tick 恢复），对已收容实体 cancel 返回 409。
  证据：`Reverse/evidence/dlc-G43-containment-cancel-mutation.json`。
- 取消后不自动恢复被取消前的其它工作；被抱起的实体会由 `EndCurrentJob` 正常放下。

### 本次复核补证（2026-11-08）

取消返回行按原生 job 目标映射：capture 的 targetB 为实体；transfer 的 targetC 为实体、
targetB 为目的平台，`entity_id/destination_platform_id` 分别报告，避免把平台当实体。
已验证捕获抱起中取消、转移提交后和抱起中取消、清空 designation/job 后再次捕获到准确目的平台。
工作可能在提交当下就抱起实体；取消会放下，源平台不会自动恢复，后续按实际读回重新捕获。
证据 `Reverse/evidence/anomaly-review-20261108-containment.json`。

## 生物铁采集器卸载/弹出

`POST /api/v1/dlc/anomaly/containment/bioferrite`

```json
{"map_id":0,"harvester_id":"BioferriteHarvester75478","action":"eject","dry_run":true}
```

`action` 为 `toggle_unloading` 或 `eject`。toggle 写原生
`unloadingEnabled`；eject 调用原 gizmo 的 `EjectContents`，由
`TakeOutBioferrite` 生成并落地生物铁。旧样本 45→0、地图 0→45；非法 action 400
（`Reverse/evidence/an07b-bioferrite-20261111.json`，自审
[生物铁](../references/reviews/2026-11-10-an07b-bioferrite.md)）。
2026-11-11 已补真实平台链：6 个原生 `HoldingPlatform` 各收容一只 Nociosphere、
原生设施连接与供电，采集器 Tick 从 0 实际累积到 30.06（`bioferrite_per_day=43.2`），
`ready_for_hauling` 由 false→true，原生 `WorkGiver_TakeBioferriteOutOfHarvester`
取整 30 并搬运到 Important 原生存储区（合并后 stack 32、`in_storage:true`），
采集器余 0.76。自审见
[AN-07b](../references/reviews/2026-11-11-an07b-surgery-interrogation-bioferrite.md)。
边界：仍只覆盖一条平台/一种产出实体；断电、多台、不可达存储的拒绝未验。

## 实体生物铁提取开关与正常提取（AN-R09）

`GET /api/v1/dlc/anomaly/containment/extract-bioferrite?map_id=&platform_id=&entity_id=`

```json
{"map_id":0,"platform_id":75489,"platform_thing_id":"HoldingPlatform75489",
 "entity_id":"Gorehulk75490","entity_def":"Gorehulk","extract_bioferrite":false,
 "containment_mode":"Study","can_be_captured":false,"research_finished":true,
 "harvester_attached":false,"bioferrite_per_day":2.0,"already_extracted":false,
 "bioferrite_on_map":0}
```

`POST /api/v1/dlc/anomaly/containment/extract-bioferrite`

```json
{"map_id":0,"platform_id":75489,"entity_id":"Gorehulk75490","enabled":true,"dry_run":true}
```

- 写的是 `ITab_Entity` 的 `CompHoldingPlatformTarget.extractBioferrite`；研究
  `BioferriteExtraction` 未完成或平台挂有 `BioferriteHarvester` 时返回 422
  （原生复选框同样禁用）。`dry_run` 只投影；`applied` 要求字段等于请求值。
- 正常提取复用 `POST /containment/work` 的 `action:"extract"`：经原生
  `WorkGiver_ExtractBioferrite`（Doctor）生成 `ExtractBioferrite` job，要求开关为 on
  且 containment mode 不是 Release；结果按原生 `JobDriver_ExtractBioferrite`：
  落地 `floor(BioferritePerDay*4)` 生物铁、对实体造成 4 点 `SurgicalCut`、
  添加 `BioferriteExtracted`（之后 per-day 归 0、不能重复提取）。
- 合同：GET observe；POST 策略层 `policy/game_ai/validate/game_memory`；
  `action:"extract"` 复用既有 work 合同。

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 生成玩家平台 + Gorehulk、完成研究（准备已披露）；GET 读回
`extract_bioferrite:false`/`per_day:2.0`；dry 200 保持 false；正式 on 200/applied；
`containment/work` action=extract 返回 `job_def:ExtractBioferrite`；推进后
`already_extracted:true`、地图生物铁 0→12、实体确有 ≥3 伤势（原生 4 点 `SurgicalCut`
经 `HealthUtility.GetHediffDefFromDamage` 按该实体身体映射为 `Crack`；测试只为让
该伤害落在 fixture 生成实体上使用 G31 伤害豁免，正常玩法不需要）；关闭开关后再次 extract 422。
证据 `tools/cua/.runtime/an-r04-r09-20261112/evidence/an-r04-r09.json`，
runner `tests/an_r04_r09_actions.py`，fixture `prepare_bioferrite_extract`/
`observe_bioferrite_extract`。未测：断电/多实体/无医生资格拒绝的逐项矩阵、
重复提取的 UI 拒绝本身。

## 身份审问

`POST /api/v1/dlc/anomaly/prisoner/interrogate` `{map_id,warden_id,prisoner_id,dry_run}`：
要求囚犯已设原生 Interrogate 模式且 scheduled；通过原生
`WorkGiver_Warden_InterrogateIdentity` 排队 `PrisonerInterrogateIdentity`。实测
Human75478 target、job_submitted true（`Reverse/evidence/an07b-interrogate-20261111.json`，
自审 [审问](../references/reviews/2026-11-10-an07b-interrogation.md)）。另推进 1500 tick 后 warden Clean、prisoner Wait、interactions_today 1，job 已执行。

2026-11-11 已补感染检测终态：三名经 `MetalhorrorUtility.Infect` 的囚犯、
Social 20 warden（`SocialImpact 1.37`，单次命中率 `0.2055`），API 提交原生 job 后
同一 job 的 3 次原生 `InteractionWorker_InterrogateIdentity` 命中
`Human75714`：`Hediff_MetalhorrorImplant.Visible=true`、`KnowsDetected=true`、
`Emerging=true`（tick 92560）。API 不写感染/检测；本轮没有实际金属恐怖生成，
因为 `Emerge` 的伤害/死亡被调试保护阻断。
未验：多日重试、涌现后战斗/死亡、其它感染与掩饰检查。
自审见 [AN-07b](../references/reviews/2026-11-11-an07b-surgery-interrogation-bioferrite.md)。
