# Core 商队组建/取消/重组（C-09）

`GET/POST /api/v1/world/caravan/forming`。同一路径的 GET 只读、POST 直接执行
命令。既有 `GET /api/v1/world/caravans` 仍用于读取已经出图的 `Caravan`。

合同状态：**已验证**（组建/真实出图/取消/重建/4xx；2026-11-13 补真实 retained
caravan 地图的正常 `reform` 出发及路线恢复，见[补验与自审](../references/reviews/2026-11-13-api-playability-core-c09-reform.md)）。实现走原生
`Dialog_FormCaravan` 的 `CalculateAndRecacheTransferables`、
`Notify_ChoseRoute`、`TryFormAndSendCaravan` → `CaravanFormingUtility.StartFormingCaravan`；
取消走 `CaravanFormingUtility.StopFormingCaravan`。不调用
`CaravanMaker.MakeCaravan`，不直接改 holder/inventory。

## GET：当前 forming lord

`GET /api/v1/world/caravan/forming[?map_id=0]`

`map_id` 是选择器；省略时取当前地图，否则取指定玩家地图。只读，不构造
dialog、不创建 lord。

```json
{"success":true,"data":{
  "map_id":0,"forming":true,"count":1,
  "lords":[{
    "lord_id":2,"phase":"Gathering animals","caravan_sent":false,
    "all_items_loaded":false,
    "destination_tile":137060,"starting_tile":137060,
    "exit_spot":{"x":117,"z":0},"meeting_point":{"x":186,"z":208},
    "mass_usage":2.4,"mass_capacity":154.0,
    "auto_travel_supplies":true,
    "blocking_reasons":[],
    "transferables":[
      {"thing_id":"Silver49389","def_name":"Silver","label":"silver",
       "kind":"item","count":100,"max_count":487,"selected":true,
       "mass":0.008,"holder":"Map-0-PlayerHome","spawned":true},
      {"thing_id":"Human321","def_name":"Human","label":"Blean",
       "kind":"colonist","count":1,"max_count":1,"selected":true,
       "mass":60.79998,"holder":"Map-0-PlayerHome","spawned":true},
      {"thing_id":"Muffalo75478","def_name":"Muffalo","label":"Muffalo 1",
       "kind":"animal","count":1,"max_count":1,"selected":true,
       "mass":144.000015,"holder":"Map-0-PlayerHome","spawned":true}
    ]
  }]
}}
```

- `transferables` 在 forming 阶段同时列原生 job 的物品 transferables 和
  `lord.ownedPawns`/`downedPawns`；`kind` 为 colonist/animal/mechanoid/item/
  food/drug/medicine。
- `mass_usage` 为物品加已入 lord 的 pawn 质量；`mass_capacity` 为原生
  `CapacityLeft` 还原的总容量。没有 forming lord 时 `forming:false`、
  `lords:[]`，不返回 404。
- `blocking_reasons` 为只读粗判：`no_able_colonist`、`overweight`、
  `no_destination`；不是完整原生错误列表。

## POST：form / cancel / reform

### form

```json
{"map_id":0,"action":"form",
 "pawn_ids":["Human321","Human315"],
 "animal_ids":["Muffalo75478"],
 "items":[{"thing_id":"Silver49389","count":100}],
 "destination_tile":137060,
 "starting_tile":137058,
 "meeting_point":{"x":120,"z":130},
 "auto_travel_supplies":true,
 "travel_supplies":[{"thing_id":"MealSimple75480","count":5}],
 "confirm":true,"dry_run":false}
```

- `pawn_ids` 是人物，`animal_ids` 是动物；两者都必须是该地图原生
  `Dialog_FormCaravan.AllSendablePawns` 的实际身份。允许传原生
  `ThingID`（如 `Muffalo75478`）或数字 ID 字符串（如 `"75478"`）。
- `items` 是原生 reachable/allowReserved 物品；`count` 必须 ≤ 原生
  `GetMaximumToTransfer()`。不可达对象 422，未知对象 404，非法数量/坐标 400/422。
- `destination_tile` 是正常世界目的地；`starting_tile` 省略时由
  `Notify_ChoseRoute` 调 `CaravanExitMapUtility.BestExitTileToGoTo`；
  `meeting_point` 省略时使用原生 packing spot。显式世界格必须在当前 layer
  范围。
- `auto_travel_supplies` 默认 true。true 时调原生
  `SelectApproximateBestTravelSupplies`（药物/食物/床/hemogen 规则），
  `travel_supplies` 被忽略；false 时按 `travel_supplies` 写 transferable
  count，正常 UI 的 auto 复选框同义。
- `dry_run:true` 只构造 dialog/transferables、跑原生 `CheckForErrors` 并返回
  `transferables/mass_usage/mass_capacity/valid/errors`，不建 lord。
- 非 dry 必须 `confirm:true`。成功返回 `applied:true`、`lord_id` 和 forming
  快照；之后由正常搬运/集合工作真实出图，完成状态读
  `GET /world/caravans`。
- 同地图已有 forming lord 时再 form 返回 **409**，不是缓存旧响应；没有
  idempotency 缓存（`none`）。
- 超重/原生 CheckForErrors 拒绝 422；非法 action/缺 confirm 400；
  map/对象缺失 404。

### cancel

```json
{"map_id":0,"action":"cancel","lord_id":2,"confirm":true}
```

- 省略 `lord_id` 时仅在地图恰有一个 player forming lord 时解析；多个时 409。
- 非 dry 必须 `confirm:true`；`dry_run:true` 只返回 `before`，不停止。
- 调 `CaravanFormingUtility.StopFormingCaravan(lord)`；成功 `applied:true`，
  随后 GET `forming:false`。没有 forming lord 返回 409。
- 与动物/返回队伍相关的原生返回 lord 不由本接口创建或取消。

### reform

```json
{"map_id":1,"action":"reform","pawn_ids":["Human321","Human315"],
 "items":[{"thing_id":"Steel75478","count":20},
          {"thing_id":"Silver75479","count":10}],
 "auto_travel_supplies":false,"confirm":true}
```

- 只有地图存在原生 retained-caravan 目的地前置时，才构造
  `Dialog_FormCaravan(reform:true)` 并调 `TryReformCaravan` 的
  `ExitMapAndCreateCaravan`/`CheckRemoveMapNow`；不构造 fake map。
- 前置不成立返回 422。正常地图上“取消后重新组建”应使用 `form`，
  不等同原生 `reform:true`。
- 临时遭遇地图可由 `GET /maps` 发现：`is_temp_incident_map:true`，无派系地图
  的 `faction_id:null`。再从 `map/pawns` 与 dry plan 的 `transferables` 选准确人物/货物；
  `pawn_ids` 也接受该公开查询的数字 ID 字符串。人物库存物品同样可在 transferables
  中选择，不能根据 `spawned:false` 过滤掉。
- `auto_travel_supplies` 对 reform 默认 false；`dry_run:true` 只验选择，不离图。
  plan 的质量/容量直接读原生 `Dialog_FormCaravan.MassUsage/MassCapacity`，避免
  把人物库存和其独立物品 transferable 重复计重。
- 成功响应 `applied:true`、`caravan_id` 与 `caravan` 是同步完成的原生出发终态。
  `caravan.pawn_ids` 为准确 ThingID；`caravan.inventory` 为按 Def 分组的物品数量，
  JSON 字典键会转 snake case（例如 `steel`/`silver`）。正常读回继续用
  `GET /world/caravans` 与 `GET /world/caravan/journey?caravan_id=...`。
- 路线由原生 retained data 恢复，`destination_tile` 不替换 retained 目的地；需要改路时，
  出发后使用 caravan journey 的正常 travel 操作。全部人员离开后由原生
  `CheckRemoveMapNow` 移除遭遇地图。

## 状态码

| 状态 | 条件 |
| --- | --- |
| 200 | form started / cancel applied / reform completed / dry plan |
| 400 | 缺 confirm、非法 action、非法坐标/数量、格式错误 |
| 404 | map/pawn/animal/item 不存在 |
| 409 | 重复 form、无唯一 forming lord、重复 cancel 后无 lord |
| 422 | 未通过原生 sendable/reachable/CheckForErrors、超重、reform 前置不成立 |
| 500 | 原生回调异常；不自动重试 |

## 验证与边界

- 证据：`tools/cua/.runtime/c09-caravan-20261113e/evidence/c09-success.json`
  （隔离 Xvfb g35 副本，真实出图 caravan 275：Human321/Human315/
  Muffalo75478，Silver100；同批次取消/重建请求）。
- 错误/取消记录：`tools/cua/.runtime/c09-caravan-20261113h/evidence/c09.json`
  （缺 confirm 400、非法 action 400、不可达 422、超重 422、重复 form 409、
  无 forming 409、reform 前置 422；该自动 runner 在等待阶段游戏进程退出，
  未把退出当成产品通过）。
- fixture 布置：Muffalo 和测试物品通过普通 product endpoint 生成，不是
  `CaravanMaker`；`g35_deathresting` 隔离副本 tick 76787。
- retained reform 补验：`RIMAPI-worktrees/biotech/tools/cua/.runtime/c09-reform-20261113-run5/evidence/c09-reform.json`
  完整 passed；地图1/Ambush276 → caravan277，Human321/Human315 与 Steel20/Silver10
  准确，地图移除、目的地99861恢复；tick76787全程暂停，预览/终态质量11.68、容量70。
  缺 confirm400、dry不离图及无派系地图公开发现均已覆盖。DLL/fixture/失败原件/清理见
  上述自审及其散列清单。fixture仅创建移动商队、准备空格/物资并调用原生遭遇进入；
  不写 retained 字段，不执行 reform，不写离图终态。
- 未测：部分人员/货物选择、多 lord/多遭遇地图；旅途随机事件；forming 中存档/读档；跨 DLC 商队成员特殊
  行为。既有 RY-R04 的 `CaravanMaker` 证据只作相邻商队读取/许可背景，不
  代替本正常组建路径。
