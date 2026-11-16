# Odyssey PassengerShuttle 地图/商队装载、货舱燃料与再起飞（OD-R06）

2026-11-13 新增。`Building_PassengerShuttle`（Odyssey 玩家穿梭机）两种持有形态：
地图建筑与商队库存（`Caravan.Shuttle`）。端点读回燃料、货舱、装载队列与原生
发射阻塞原因；写路径只复刻原界面：`Dialog_LoadTransporters.TryAccept`、
`Building_PassengerShuttle` 的“从货舱加油”gizmo、`CompLaunchable.TryLaunch`
与 `CaravanShuttleUtility.LaunchShuttle`。搬运、耗油、离场 skyfaller、飞行与
商队到达全部是原生行为。

## 读取

`GET /api/v1/dlc/odyssey/shuttle`

- 无参数：当前地图/`map_id` 上的地图穿梭机 + 所有持有穿梭机的商队；
- `?shuttle_id=PassengerShuttle75478`：地图 → 商队 → 在飞 三种形态依次查找；
- `?map_id=0`：只看该地图建筑；
- `?caravan_id=276`：只看该商队。

地图行：

```json
{"shuttle_id":"PassengerShuttle75478","mode":"map","map_id":0,
 "spawned":true,"fuel":250.0,"fuel_max":400.0,
 "transport_ship_id":0,"transporter_group_id":0,
 "loading_in_progress":true,"anything_left_to_load":false,
 "left_to_load":[],
 "contents":{"pawn_ids":["Human315","Human321"],
   "items":[{"thing_id":"Steel75481","def_name":"Steel","stack_count":75}],
   "item_stack_count":250,"fuel_in_cargo":50,
   "mass_usage":181.6,"mass_capacity":500.0,"over_mass_capacity":false},
 "loading_pawns":[{"pawn_id":"Human321","job_def":"HaulToTransporter",
                   "carried_thing_id":null,"carried_count":0}],
 "pilot_present":true,"can_launch":true,"launch_block_reason":"",
 "cooldown_remaining":0,"last_launch_tick":-1}
```

商队行（`mode:"caravan"`）带 `caravan_id/tile/fuel_in_caravan/mass_usage/
mass_capacity/inventory/pawn_ids`；`inventory` 键为 defName 的 snake_case。
在飞行（`mode:"in_flight"`）返回 `transport_ship_id/holder/travelling`
（travelling_id、tile、destination_tile、arrival_action、pawn_ids、cargo）。

- `can_launch`/`launch_block_reason` 直接取原生
  `CompLaunchable.CanLaunch()` 或 `CaravanShuttleUtility.CanLaunchCaravanShuttle`
  的 `AcceptanceReport`，包括“未开始装载”“无飞行员”“燃料不足”“冷却中”
  （`On cooldown. (1 hour)`）等原生警告。
- `pilot_present` 在容器内按 `CompShuttle.HasPilot` 同款条件（自由殖民者、
  `PilotingAbility` 未禁用且 > 0.1）判断。

## 管理

`POST /api/v1/dlc/odyssey/shuttle`，`shuttle_id` 或 `caravan_id` 二选一；
实际变更需 `confirm:true`；`action:arrival_options` 只读且不要求确认。

```json
{"shuttle_id":"PassengerShuttle75478","action":"load","confirm":true,
 "pawn_ids":["Human321","Human315"],
 "items":[{"thing_ids":["Chemfuel75479","Chemfuel75480"],"count":200},
          {"thing_ids":["Steel75481","Steel75482"],"count":100}]}
{"shuttle_id":"PassengerShuttle75478","action":"cancel_load","confirm":true}
{"shuttle_id":"PassengerShuttle75478","action":"refuel","refuel_count":150}
{"caravan_id":276,"action":"refuel","refuel_count":50}
{"shuttle_id":"PassengerShuttle75478","action":"launch","confirm":true,
 "destination_tile":137060,"arrival":"form_caravan"}
{"caravan_id":276,"action":"launch","confirm":true,
 "destination_tile":71341,"arrival":"form_caravan"}
```

- `load`（仅地图）：候选 pawn/item 取原生
  `TransporterUtility.AllSendablePawns/AllSendableItems`；首次装载调用
  `TransporterUtility.InitiateLoading`，逐项 `AddToTheToLoadList`，再
  `MakeLordsAsAppropriate`，并中断该 shuttle 上旧的 `HaulToTransporter`
  job（与 TryAccept 顺序一致）。返回 `group_id/left_to_load/lord_*/contents`。
  `dry_run` 只校验并返回 `item_rows/would_load`，不写。
- `cancel_load`（仅地图）：原生 `CompTransporter.CancelLoad()`（强制
  `ShipJob_Unload` 后清 `groupID/leftToLoad` 并丢回容器内容）；返回
  `group_id:-1`、空 `left_to_load/contents`。需要有装载在进行，否则 409。
- `refuel`：地图从货舱、商队从商队库存取燃料（原版
  `ConsumeFuelFromInventory` / `ConsumeFuelFromCaravanInventory`）后
  `CompRefuelable.Refuel(count)`；`count` 省略时取 `min(可用, 容量剩余)`，
  返回 `available_fuel_in_cargo|caravan/fuel_room/max_refuel/fuel_before/
  fuel_after/refuel_count`。`dry_run` 不写。
- `arrival_options`：用 `destination_tile`（默认surface）或公开世界对象的
  `destination_world_object_id` 发现结构化 `arrival_options`；对象指定完整原生层。
  返回 index/arrival/label/world_object_id/map_id/disabled/fail_reason/fail_message。
  地图落点选项还带最多16个原生合法 `landing_cells` 建议（地图中心40格内，非完整地图列表）。
- `launch`：选择 `arrival` 或 `arrival_option_index`；两者同时提供须一致。
  选项按 MapParent/Settlement/Site/SpaceMapParent 的原生语义适配：
  `form_caravan`、`land_in_map`、`visit`、`trade`、`attack`、
  `visit_site_edge`、`visit_site_center`、`visit_space`；只有本目标和当前货物
  原生提供的选择才可用。无正常到达动作时显式 `discard` 表示原生内容丢失风险。
  `attack` 保留原生非已生成聚落地图及 shipParent 门禁，不绕过资格。
  `land_in_map` 要求 `landing_x/landing_z`，`landing_rotation`为0北/1东/2南/3西，
  缺省为原生默认旋转；完整占地和交互格走 `ShuttleCanLandHere`，再构造原生
  `TransportersArrivalAction_LandInSpecificCell(...landInShuttle:true)`。
  轨道到达要求 `confirm_space:true`，返回原生 `space_warning`。
  所有动作重查 `StillValid`；主线程通过CanLaunch和真实燃料航程后，
  地图走 `CompLaunchable.TryLaunch`，商队走 `CaravanShuttleUtility.LaunchShuttle`。
  返回distance、燃料、选项、原生在途读回；`return_fuel_warning`披露不足往返油量的原生警告，
  `contents_will_be_lost`标记原生空arrival动作。确认仅批准请求，不跳过原生资格。

```json
{"shuttle_id":"PassengerShuttle123","action":"arrival_options","destination_world_object_id":273}
{"shuttle_id":"PassengerShuttle123","action":"launch","destination_world_object_id":273,
 "arrival":"land_in_map","landing_x":125,"landing_z":125,"landing_rotation":1,"confirm":true}
```

错误：对象不存在404；缺confirm/目标/选择/落点或非法rotation为400；原生禁用、冷却、
轨道缺确认409；非法目标、不可落格、超航程422；Odyssey未激活422 dlc_missing。
本端点未声明game_memory幂等，调用方不应发送Idempotency-Key或盲重发起飞；先读真实holder。
`applied:true`是发射已提交，最终飞行、落地/卸客要继续GET确认。

## 证据与边界

2026-11-13 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，
invulnerability 开启）：fixture 生成原生 `PassengerShuttle`（燃料 100/400）
与玩家 Chemfuel 100+100、Steel 50+50。流程：

1. 初始 GET：`can_launch:false`、“Must be loaded first.”；
2. 单项装载后 `cancel_load` dry/正式：`group_id` 0→-1、`left_to_load` 清空、
   容器为空、装载 job 清空；
3. 正式 `load` 2 pawn + Chemfuel200/Steel100：lord 2、两行 pawn +
   货舱行；推进后容器 2 pawn、Chemfuel200、Steel100、
   `fuel_in_cargo:200`、`mass_usage:181.6/500`、`pilot_present:true`、
   `can_launch:true`；
4. `refuel` 150：燃料 100→250，货舱剩 Chemfuel 50；
5. 地图 `launch` 至 tile 137060（distance 1，fuel 50）：`applied:true`、
   燃料 200、建筑已离场；推进后商队 276 位于 137060，持有穿梭机、
   `fuel:200`、`fuel_in_caravan:50`、Steel100/Chemfuel50、两 pawn；
6. 立即 `launch` → `409`、`On cooldown. (1 hour)`；推进至
   `cooldown_remaining` 3150→0；
7. 商队 `refuel` 50：燃料 200→250，商队 Chemfuel 清空；
8. 商队 `launch` 至 tile 71341：`caravan_destroyed:true`，
   `TravellingTransporters 277` 载两 pawn + Steel100 + passenger_shuttle；
   推进后商队 278 位于 71341，持有穿梭机、`fuel:200`、Steel100、两 pawn。

证据
`tools/cua/.runtime/od-r06-20261113n/evidence/od-r06-shuttle.json`
（dll `eeb8cfb6…`、fixture `06694df0…`、source `14d486a5`），runner
`tests/parallel/odyssey/od_r06_shuttle.py`，fixture
`tests/parallel/odyssey/OdysseyFixture.cs`（`prepare_shuttle`/`observe_shuttle`/
`dismiss_dialog`）。

- 世界商队推进期间可能随机触发 `IncidentWorker_CaravanMeeting` 的
  force-pause 对话框；runner 在 tick 停滞时经 fixture `dismiss_dialog`
  关闭并记录（未出现在本次通过证据中）。
- 未测：地图/定居点落点与多到达选项、屋顶/天气/无飞行员发射拒绝、
  商队被攻击/合并、跨层轨道目标、穿梭机跨存档持久化、飞行员仪式加成。

2026-11-13 E-04复核补回地图/聚落固定501缺陷；本轮适配及新增落地证据见[收尾验收](../references/reviews/2026-11-13-api-playability-endings-transport.md)。上面旧证据的“未测”保留为当时范围。
