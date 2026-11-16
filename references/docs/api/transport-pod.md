# Core 普通运输舱/穿梭机装载与到达动作（C-12）

`GET/POST /api/v1/transport/pod`。只覆盖普通 `CompTransporter` +
`CompLaunchable_TransportPod`；`CompShuttle`/许可穿梭机/重力船和
`/dlc/odyssey/shuttle`、`/dlc/royalty/transport/*` 不属于本端点（OD-R06
专属保留）。

合同状态：**已验证**（编组/装载 holder/cancel/reload/发射/form_caravan
真实抵达）；trade/gift/attack/site arrival 已按 native 构造但未实际长跑，
auto_load 与跨层 `confirm_space` 未在本批 runner 实测，见边界。

## GET

`GET /api/v1/transport/pod?map_id=0` 或 `?pod_id=TransportPod75479`

返回所选普通 pod 或地图上全部普通 pod：

```json
{"count":1,"pods":[{
 "pod_id":"TransportPod75479","def_name":"TransportPod","map_id":0,
 "spawned":true,"tile":137061,"group_id":0,
 "group_pod_ids":["TransportPod75479"],
 "contents":[
  {"thing_id":"Steel75492","def_name":"Steel","label":"steel x50",
   "kind":"item","stack_count":50,
   "holder":"CompTransporter(parent=TransportPod75479 at=(110, 0, 83))"},
  {"thing_id":"Human321","def_name":"Human","label":"Blean",
   "kind":"pawn","stack_count":1,
   "holder":"CompTransporter(parent=TransportPod75479 at=(110, 0, 83))"}
 ],
 "contents_mass_usage":25.8,"contents_mass_capacity":150.0,
 "over_mass_capacity":false,"left_to_load":[],
 "loading":true,"fuel":100.0,"fuel_max":150.0,"has_fuel":true,
 "fueling_port_connected":true,"can_launch":true,
 "launch_block_reason":"","cooldown_remaining":0
}],"active_transporters":[]}
```

- `contents` 是原生 `CompTransporter.innerContainer` 的准确 holder；跨图
  `active_transporters` 在读图时给出 `TravellingTransporters` 的 id/
  destination/arrival/contents（若读时已抵达可为空，以 world caravan 为准）。
- `can_launch`/`launch_block_reason` 来自原生 `CompLaunchable.CanLaunch`；
  fuel/port/cooldown/overweight 都是原生读回。

## POST

```json
{"pod_id":"TransportPod75479","action":"load",
 "pawn_ids":["Human321"],"items":[{"thing_id":"Steel75480","count":50}],
 "confirm":true,"dry_run":false}
```

`action` 为 `load|cancel_load|launch`；非 dry 需 `confirm:true`；
`Idempotency=none`。

### load

- 显式 `pawn_ids`/`items` 与 `auto_load:true` 二选一；auto 取原生
  `TransporterUtility.AllSendablePawns/AllSendableItems` 全集，不反射、不
  直接写 holder。
- 先 `InitiateLoading` 建组，逐项 `AddToTheToLoadList`，再
  `MakeLordsAsAppropriate` 创建正常搬运/进入 lord；实际容器由原工作填充。
- 返回 group_id、left_to_load、row_after；真实 holder 另由 GET 读回。
- 无候选/auto 与显式混用 400；非 sendable 对象/超堆量 422。

### cancel_load

- 调原生 `CompTransporter.CancelLoad()`；返回 before/after，group 归 -1、
  contents/left_to_load 清空；无装载 409；非 dry 需 confirm。

### launch

- 先原生 `CompLaunchable.CanLaunch()`；无装载/无燃料/under roof/超重/
  cooldown 409。
- 目标格与可用 arrival 由 `CompLaunchable.GetOptionsForTile` 只读投影；
  `arrival` 支持 `form_caravan|trade|gift|visit|attack|visit_site`，按目标
  对象类型与原生 `StillValid` 过滤；不打开 UI。
- 非 dry 需 confirm；目标跨 planet layer 需额外 `confirm_space:true`，否则
  409；越界 400。
- 执行只调原生 `CompLaunchable.TryLaunch(tile, arrivalAction)`；不直接创建
  `TravellingTransporters`/不改容器。返回 `launched` 与可用的
  `travelling`/`active_transporters` 读回；真实抵达以 world caravan/map
  读回为准。

## 状态码

| 状态 | 条件 |
| --- | --- |
| 200 | GET / load setup / cancel / launch accepted |
| 400 | 非法 action、缺 target/confirm、auto 与显式混用 |
| 404 | pod 不存在 |
| 409 | 无装载、无燃料、under roof、超重、cooldown、跨层缺 confirm_space |
| 422 | 非法 target/sendable、arrival 类型不匹配、StillValid false |
| 500 | 原生异常；不自动重试 |

## 验证与边界

- 证据：`tools/cua/.runtime/c12-transport-20261113k/evidence/c12.json`
  （runner `tests/c12_transport_pod.py`，passed）。
- fixture：`tests/parallel/core-transport/CoreTransportFixture.cs` 生成原生
  PodLauncher/TransportPod/home-area Steel；仅测试准备，产品 route 不绕过
  load/launch。
- 已验：load 显式 pawn+50 Steel → holder 中出现准确 Steel/Human；cancel
  group 0→-1、contents/left 清空；reload group1、pawn holder；launch
  form_caravan 到 tile200 → world caravan 276，pawn/物资 holder 准确。
  发射后 `TravellingTransporters` 对近距离目标过短，runner 未能轮询到；以
  到达 world caravan 为权威跨图结果，未伪造始终可见的 transit。
- 未测：auto_load 全候选、trade/gift/attack/site 实际抵达、跨层
  confirm_space、permission shuttle/gravship（OD-R06 专属）、多 pod 编组
  同步装载。走普通 pod 时才使用本端点。
