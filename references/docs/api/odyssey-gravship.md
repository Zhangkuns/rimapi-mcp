# Odyssey 重力船（G40）

正常同层登船→起飞→旅行→新地图→着陆、世界与终态跨进程恢复已[限定通过](../references/reviews/2026-11-08-g40-codex-flight.md)；旧引擎只读证据仍保留。实施与验收范围见
[G40计划](../exec-plans/active/g40-gravship.md)。登船取消及活动/ready恢复亦已[限定通过](../references/reviews/2026-11-08-g40-codex-boarding.md)。
正常加注和故障维修已[限定通过](../references/reviews/2026-11-08-g40-codex-maintenance.md)。完整改装、跨层生存、随机着陆效果
尚未验收，不能从接口存在推断已通过。

所有路径以 `/api/v1` 开头。写操作走主线程协调器，支持 `dry_run`、`Idempotency-Key`
的逐字节重放/异体409及operation事件；幂等缓存仅当前Game内存，不跨进程。

## 读取

`GET /dlc/odyssey/gravship?map_id=0&run_id=gravship-…`

保留旧的controller/travelling/cutscene/landing确认、map_id、world_ships和engines字段。
world_ships保留数字engine_id/tile，增加engine_thing_id、tile_identity、destination、
准确人物与货物、ship.fuel和各thing.fuel。旅行/marker阶段燃料来自实际船内providesFuel组件，
避免离图引擎TotalFuel=0误报。数字tile须与layer_id一起解释。engine增加thing_load_id、位置、
缺失组件名称、控制台原生can_launch/reason、推进器blocked、燃料和驾驶结果。

没有当前地图时允许观察世界船，map_id为null；明确传入不存在的map_id返回404。
`landing_map_id=0`仍保留历史无目标值，准确着陆地图请结合landing_marker与run。
原生抵达会从世界列表移除船并放入landing_marker；此时world_ship_count=0不表示船丢失。

`run_id`只在当前Game的API运行记录中解析；未知404。run包含原始map/层/格子、准确Lord、
引擎/驾驶员、目标、世界船ID、状态与终态时间。`current_observation`是当前读数；
`terminal_observation`仅在观察原生着陆结束或登船取消/中断时冻结。run随Game保存，
旧存档没有记录时不推断历史。landed只由原生LandingEnded及准确目的地读回确认。

## 正常登船准备

`POST /dlc/odyssey/gravship/launch/start`

```json
{"map_id":0,"engine_id":"GravEngine88473","console_id":"PilotConsole88474",
 "pilot_id":"Human88478","crew_ids":["Human88481"],"dry_run":true}
```

map/engine/console/pilot必须明确。可选copilot_id、crew_ids；角色不能重复。省略船员不会
自动选择其它殖民者。board_colony_animals、board_colony_mechs、force_visitors_to_leave
默认false；显式true使用原生worker的对应登船/离船选项。

复用控制台CanUseNow、原生驾驶员资格、角色指派及确认检查；正常创建GravshipLaunch
的Lord。返回applied代表已提交登船。人物实际走到船上并完成原生准备后run才成为ready，
生成真实LaunchInfo，包括驾驶质量和随机负面着陆标记；API不直接写驾驶结果。API登记的准确原生结果新建选点器后会释放该TilePicker，保留
ready和LaunchInfo，允许正常保存及稍后通过takeoff指定目的地；start遇到已有TilePicker返回409。
原生无anchor额外确认分支未验收。

## 取消活动登船

`POST /dlc/odyssey/gravship/launch/cancel`

```json
{"run_id":"gravship-…","dry_run":true}
```

只取消准确run仍活动的boarding Lord，走原生Cancel信号和清理。dry返回would_cancel，
实际调用返回cancel_requested及当前run；native_cancelled和cancel_requested_tick分别标注
原生终态与请求，cancelled终态冻结/随Game保存。未知run404，非活动boarding阶段409。
ready/飞行中的动作没有返航或回滚含义；取消不会伪造驾驶结果。

## 选择目的地并起飞

`POST /dlc/odyssey/gravship/takeoff`

```json
{"run_id":"gravship-…","layer_id":0,"tile_id":71338,"dry_run":true}
```

只接受准确run已经观察到正常登船结果的ready阶段。层/格子必须明确，复用原生层路径、
距离、燃料、signal jammer和新定居点资格。返回fuel_cost、distance、maximum_distance及
abandons_origin_map。没有GravAnchor时需显式allow_abandon_map=true承认原生放弃地图行为；
不能返回同一已放弃地图。正式动作扣原生燃料并开始原生起飞cutscene，随后原生生成世界船、
旅行和抵达；不会提前报告landed。

## 确认落区

`POST /dlc/odyssey/gravship/land`

```json
{"run_id":"gravship-…","map_id":1,"x":125,"z":125,"rotation":0,"dry_run":true}
```

只接受该run已抵达的原生landing_marker。map须是准确目的地图，rotation为0..3；坐标与
原生move-gravship designator相同，并非引擎中心。复用CanDesignateCell拒绝无效区域，
正式动作移动marker并通过BeginLanding进入原生着陆。dry临时校验后恢复当前地图和marker旋转。
最终引擎位置、人物与货物以准确run的终态/当前观察为准。

## 正常加注与故障维修

`POST /dlc/odyssey/gravship/refuel`、`POST /dlc/odyssey/gravship/repair`

```json
{"map_id":1,"engine_id":"GravEngine88473","target_id":"ChemfuelTank88475",
 "pawn_id":"Human142525","dry_run":true}
```

repair的target_id选择准确损坏组件（本组SmallThruster88476）。目标须为玩家引擎自身或其连接组件，
refuel另须为providesFuel且有CompRefuelable；不加注任意非船设施。`mode` 省略/`breakdown` 走原生
`FixBrokenDownBuilding`；`mode:"damage"` 走原生 `WorkGiver_Repair` 修复建筑HP。地图/人物/物件缺失404，
缺字段400，资格/无材料/无损伤422或409，活动飞行/重复当前维护409。

HP 修理要求目标 `useHitPoints` 且 `building.repairable`、当前 HP 小于 Max、目标在玩家 Home 区域
（原生 `WorkGiver_Repair.HasJobOnThing` 门禁），使用原生 Construction 工作类型；目标 HP 已满返回 409，
`mode` 非 `breakdown|damage` 返回 400。

人物须为可用玩家殖民者并启用对应Hauling/Construction工作；沿原生容量、禁用/禁选、可达和
WorkGiver.HasJobOnThing门禁。维修依赖原生Home区域及可用零部件。dry只验证，不创建Job或消耗资源。
正式调用原生JobOnThing和TryTakeOrderedJobPrioritizedWork，applied仅表示工作已提交；
submitted_job包含准确job_id/work_giver/target_id/material_id，非工作完成承诺。

GET的engine/components及thing增加broken_down、hit_points/max_hit_points/damage_ratio，
maintenance_jobs是当前地图的Refuel/RefuelAtomic/FixBrokenDownBuilding/Repair当前工作及准确人物/目标/材料。
`GET` 另增 `environment`：`layer_id/layer_def/is_space/biome/biome_in_vacuum/vacuum_at_center/tile`，
用于读取当前地图所在层与真空环境；它不迁移人物/船。实际完成应读fuel、broken_down、HP和资源；
job结束本身不代表效果成功。按游戏原生工作进度，可被其它命令/人物状态中断，无自动重试或资源回滚。
本组真实燃料200→250/消耗50chemfuel、损坏推进器修复/消耗1component/航程0→10及新进程恢复通过；
G-44 另验 HP 修理（控制台 150→75→150，原生 Repair job）和地表环境读取。
未宣称所有船设施/材料组合、结构改装、轨道/真空地图、跨层旅行或完整冷却周期通过。

## 跨层起飞与轨道到达（G-45 限定）

`takeoff` 的 `layer_id/tile_id` 已按原生 `GravshipUtility.TryGetPathFuelCost` 处理层路径，
不需要新端点。G-45 实测地表→轨道：

- 登船 ready 后 `takeoff layer_id=1,tile_id=478`：原生层路径 fuel_cost 50、distance 0；
  `max_launch_distance=10`、目标层 `rangeDistanceFactor=20` 时 `max_distance=0`，`distance > max_distance`
  不成立，原生允许起飞；燃料 250→200。
- 原生旅行生成 Orbit 地图（layer1、`is_space=true`、`biome=Orbit`、`in_vacuum=true`、vacuum 1.0），
  世界船/人物/货物迁移到 tile478，`landing_marker` 出现，run 进入 `landing_confirmation`。
- 旅行期间原生会弹 force-pause `Dialog_MessageBox`；无人值守调用方需用
  `POST /ui/window/close`（`window_types:["Dialog_MessageBox"]`）关闭后继续 tick。
- **空轨道地图是 Space 地形，原生 `land`/move-gravship designator 返回 422
  “Must be connected to solid ground”**。空轨道格不是可降落目的地；要完成跨层着陆，目的地需有
  solid ground（轨道站/另一地表层/带 MapParent 的站点），本轮未构造。
- 结论：跨层“起飞+到达+人物/船/燃料/环境”限定通过；跨层“着陆”未通过，不把到达算完整迁移。

证据：`Reverse/evidence/dlc-G45-cross-layer.json`、`dlc-G45-run.json`。

## 跨层合法着陆与返航（OD-01，2026-11-08）

空 Space 格不可降落是原生限制；本轮用有实体地面的轨道平台完成双向跨层迁移。

- 目标准备：隔离 fixture 用原生 `SiteMaker.MakeSite` 在 layer1 tile 478 生成一个
  `OrbitalAncientPlatform` site（世界生成级准备，已披露）；起飞/降落本身仍走产品端点。
- 地表→轨道：`launch/start` 正常登船 → ready → `takeoff layer_id=1,tile_id=478`：
  fuel_cost 50、distance 0；到达后生成 Orbit 地图 map2（layer1、is_space、vacuum 1.0），
  `landing_marker` 在 map2 生成；用 `land`（原生 `Designator_MoveGravship.CanDesignateCell` 校验）
  落在开阔平台地面：run `landed`、engine 在 map2、pilot 与货物在船体 substructure 上；燃料 250→200。
- 平台返航：在 map2 重新 `launch/start` → ready → `takeoff layer_id=0,tile_id=137061`
  （fuel_cost 50、distance 5、max 10）→ 到达既有殖民地地图 map0（layer0、TemperateForest），
  `landing_confirmation_in_progress=true`（该情形 marker 未生成、`landing_map`=map0）；
  `land` 指定清晰地面格：run `landed`、engine map0 (125,125)、pilot 同图；燃料 200→150。
- `GET` 增 `landing_map_present`：`landing_map_id=0` 既可能是 map0 也可能是无 landingMap，
  该布尔区分；既有地图到达时 marker 未生成，新地图到达时 marker 由 `GenStep_GravshipMarker`
  生成在玩家起点。
- 两段都要求降落足迹内无建筑/屋顶：墙落在船体内部会切断 substructure，使推进器脱离引擎，
  之后登船返回会报 "No unblocked functioning thrusters"。这是原生后果，不是接口缺陷；
  fixture 的 `landing_cells` 只用原生 `CanDesignateCell` 加清晰区筛选。
- 原生观察：fixture 生成的 OrbitalAncientPlatform 地图在生成时记录
  `GenStep_OrbitalPlatform`/`RoomContents_NarrowHalls` NullReferenceException（平台结构不完整，
  但实体地面与降落仍有效）；该 site 生成路径的真实缺陷留给后续取证，本轮不把它算作 OD-01 失败。

证据：`Reverse/evidence/an05-od01-cross-layer.json`，runner `tests/od01_cross_layer_return.py`。
边界：轨道站内部可居住区/密封、真空伤害、其它站点类型、跨层活动接续与结构改装（OD-02/04）未验。

## 结构改装：建造影响船能力的组件（OD-02，2026-11-08）

正常玩家建造路径：`POST /builder/blueprint` 放蓝图 → 殖民者建造 → 原生设施链接到引擎。
实测 `g40_maintained`（tick 76222，船在地表 map1）：引擎基线 `max_launch_distance=10`、
组件 SmallThruster/ChemfuelTank/PilotConsole、121 个 valid substructure 格；fixture 仅用原生
`GenConstruct.CanPlaceBlueprintAt` + `CompGravshipThruster.IsBlocked/IsOutdoors` 选格
（124,120,rot0），材料用产品 `/item/spawn` 准备（180 steel + 4 components，测试准备已披露）。
建成 `SmallThruster151490` 后引擎组件 4 个、`max_launch_distance` 10→20（原生
`GravshipRange` 设施偏移 +10）；再用产品 `/order/designate` `Deconstruct` 指定该组件，
拆除后组件回到 3 个、范围回到 10。`environment` 读回地表 layer0、vacuum 0。

- 关键原生链：`Building_GravEngine.MaxLaunchDistance => StatDefOf.GravshipRange`，
  设施偏移经 `CompAffectedByFacilities.GetStatOffset` 且仅当 `CompGravshipThruster.CanBeActive`
  （链接、在有效船体、未阻塞、露天）时生效；放置位置由原生 place worker 校验。
- 本项同时证明“结构改装”可完全由通用 builder/designate 表达，无需专用改装端点。
- 边界：密封可居住区/增压、墙体与门对真空的影响、拆除后材料返还、多个大型推进器组合未验。

证据：`Reverse/evidence/an05-od02-retrofit.json`，runner `tests/od02_ship_retrofit.py`。

## 地标探索：正常旅行并生成地图（OD-03，2026-11-08）

`GET /dlc/odyssey/landmark` 提供世界地标；fixture 用原生
`GravshipUtility.TryGetPathFuelCost` 选最近的可达地标（只读查询）。实测最近为
`LakeWithIsland`/“Hattilium Lake”（layer0 tile 101491，distance 9、fuel 90、max 10）：

- 正常登船→`takeoff layer 0,tile 101491`（fuel 90，abandons origin map）→生成新地图 map2，
  `landing_marker` 生成；`land` 落在原生 designator 接受的清晰格（本例为浅水 (125,125)，
  原生允许；深水/空 Space 的原生拒绝保留）→run landed、engine 在 map2。
- 读回：`landmark/tile` 仍为 `LakeWithIsland`/mutator `LakeWithIsland`；环境 layer0、
  tile 101491、TemperateForest；新地图地形含 `WaterShallow` 15,187 格与 Soil/Sand 等，
  与湖中岛地标一致；`landmark` 身份在降落后不变。
- 不使用调试 `world.landmark.add/remove`；目标来自世界地标本身。
- 边界：其它地标类型/组合地标、地标特殊内容（非地形）的逐项核对、跨层地标与探索奖励未验。

证据：`Reverse/evidence/an05-od03-landmark.json`，runner `tests/od03_landmark_explore.py`。

## 历史只读证据

`Reverse/evidence/dlc-G-40-42-engine.json`、`dlc-G-40-42-empty.json`：旧fixture仅生成
无结构/无燃料引擎，valid_substructure_cells=0、missing_component_count=3、world_ship_count=0；
这不算正常飞行证据。原探针留档`Reverse/evidence/dlc-static/g40-probe.cs.txt`。
