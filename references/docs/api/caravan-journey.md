# Core 商队路线、到达动作与世界地点经营（C-10）

`GET/POST /api/v1/world/caravan/journey`。既有只读 `/world/caravans`、
`/world/caravan/path` 保留；本路由负责正常 `Caravan_PathFollower.StartPath`、
目标类型对应的 `CaravanArrivalAction_*`，以及原生 settle/abandon。普通交易
不在本路由新开 arrival action，到达后复用 `TradeControlService`。

合同状态：**已验证**（到达选项/路径启动/玩家地图 enter/foreign settlement visit 与
attack 实际抵达/原生 settle 建图/原生 abandon；site/peace talks/escape ship
实际抵达仍保留边界）。

## GET

`GET /api/v1/world/caravan/journey?caravan_id=275[&target_tile=478][&world_object_id=110]`

只读，不打开 WorldRoutePlanner/Dialog，不改 Tile/holder。返回：

```json
{"caravan_id":275,"tile":137061,"moving":false,"paused":false,
 "next_tile":-1,"destination_tile":-1,
 "arrival_action":null,"arrival_label":null,"arrival_report":null,
 "path":[],"estimated_ticks":0,
 "mass_usage":77.4,"mass_capacity":70.0,
 "target_tile":478,
 "target":{"world_object_id":110,"def_name":"Settlement","kind":"settlement",
   "label":"Asworth","tile":478,"faction":"Arberium",
   "faction_def":"OutlanderCivil","hostile":false,"has_map":false,
   "map_id":null,"quest_tags":[]},
 "arrival_options":[
   {"arrival_action":"enter","label":"Enter Asworth","report":"Entering Asworth.",
    "valid":false,"fail_reason":null,"fail_message":null},
   {"arrival_action":"visit","label":"Visit Asworth","report":"Visiting Asworth.",
    "valid":true,"fail_reason":null,"fail_message":null},
   {"arrival_action":"attack","label":"Attack Asworth","report":"Attacking Asworth.",
    "valid":true,"fail_reason":null,"fail_message":null}
 ]}
```

- `arrival_options` 只对目标对象类型构造候选；`valid/fail_reason/fail_message`
  来自原生 `StillValid`，不是 UI 展开结果。
- `quest_tags` 是 WorldObject 原生 quest tags；site 的 parts/faction/quest
  由目标行/世界对象读取，不能把标签当任务完成。
- `target_tile` 无对象时合法（用于空白格 travel），`arrival_options:[]`。
- 世界对象不存在时 404；坐标越界 400。

## POST

```json
{"caravan_id":275,"action":"travel",
 "target_tile":478,"world_object_id":110,
 "arrival_action":"visit",
 "confirm":true,"dry_run":false}
```

### travel

- `target_tile` 必须；`world_object_id` 用于多对象/精确对象；对象存在但 tile
  不符 422。
- `arrival_action` 为 `none|enter|visit|attack|visit_site|visit_peace_talks|
  visit_escape_ship`；必须由目标类型构造并原生 `StillValid` 通过。
  - `enter`：有 map 的 `MapParent`（玩家地图/已有地图对象）
  - `visit`/`attack`：`Settlement`
  - `visit_site`：`Site`
  - `visit_peace_talks`：`PeaceTalks`
  - `visit_escape_ship`：带 `EscapeShipComp` 的 `MapParent`
- 启动调原生 `Caravan_PathFollower.StartPath(tile, arrivalAction,
  repathImmediately:true)`；无路径/启动失败 422。不直接改 `Caravan.Tile`，
  不打开路由 UI。实际到达由原生日志推进。
- `travel` 可重复以替换路线，非破坏性，不要求 confirm；`dry_run` 只校验
  `CanReach`/`StillValid` 并返回计划。
- 若 `StartPath` 在目的地立即触发 arrival（例如同格 enter），返回
  `phase:"arrived"`/`caravan_exists:false`；否则 `phase:"travelling"`。

### settle

```json
{"caravan_id":275,"action":"settle","confirm":true,"dry_run":false}
```

- 先 `TileFinder.IsValidTileForNewSettlement(caravan.Tile, reason)` 和
  `SettleUtility.PlayerSettlementsCountLimitReached`；非法格/上限 422。
- 非 dry 必须 `confirm:true`；调原生 `SettleInEmptyTileUtility.Settle(caravan)`
  （AddNewHome + async map generation + CaravanEnterMapUtility.Enter）。
- 响应是异步 `phase:"queued"`/`queued:true`/`completed:false`；立即的
  `settlement_id`/`settlement_tile` 来自原生 `AddNewHome`，`map_id` 在 map
  生成前为 null。不得立即报 completed；以
  `GET /maps`、`GET /world/player/settlements`、地图 holder 读回为准。

### abandon

```json
{"caravan_id":275,"action":"abandon","target_tile":200,
 "world_object_id":276,"confirm":true,"dry_run":false}
```

- 目标必须是玩家 `Settlement` 的 `MapParent`，且
  `SettlementAbandonUtility.AllColonistsThere` 不为 true（正常 UI 禁用条件）；
  非玩家/前置不满足 422。
- 非 dry 必须 `confirm:true`；调原生 `MapParent.Abandon(false)` +
  `Find.GameEnder.CheckOrUpdateGameOver()`；不直接 Destroy/改 Faction。
- 返回 `phase:"abandoned"`、`completed:true`、`exists_after`；地图/定居点
  读回以世界/地图接口为准。

## 状态码

| 状态 | 条件 |
| --- | --- |
| 200 | GET 读回 / travel 已启动或立即到达 / settle queued / abandon 完成 |
| 400 | 缺 target_tile、非法 action/arrival_action、坐标格式错误 |
| 404 | caravan/world object 不存在 |
| 409 | 无意义/重复状态（当前实现：无） |
| 422 | 目标类型不匹配、StillValid false、无路径、settle 非法格/上限、abandon 前置不满足 |
| 500 | 原生异常；不自动重试 |

`Idempotency=none`；不使用 game_memory 缓存。所有 mutation 在主线程执行。

## 验证与边界

- 证据：`tools/cua/.runtime/c10-caravan-20261113b/evidence/c10.json`
  （测试 run；fixture 仅准备玩家商队并披露 direct-tile shortcut）。
- 正常到达：caravan 275 同格 `enter` 玩家 Colony → 原生地图 enter，世界
  caravan 消失、地图/pawn 读回；Asworth 的 visit/attack 起始和 path/ETA/
  arrival action 读回。
- settle：fixture 在空白格 200 准备商队，原生 `settle` 返回 queued/
  settlement_id 276；推进后 map_count 2、`GET /world/player/settlements` 和
  `/maps` 显示 276/新 map。
- abandon：同格准备第二支商队，原生 `abandon` 返回 completed/
  `exists_after:false`；世界对象消失、map_count 1。fixture 的 CaravanMaker/
  直接摆 tile 是测试准备，产品 route 未使用。
- foreign settlement 实际抵达：`c10-caravan-20261113l/evidence/c10.json`。
  fixture 在 tile101493 准备商队275（20 Steel/10 Silver，总质量低于容量）；
  `POST travel visit` 原生 `StartPath` 到 Asworth tile478，native path
  `[478,478]`/ETA1205，有界推进到 tick79473 后 caravan275 到达 tile478、
  moving false/destination -1；原生 `VisitSettlement` 只发信不生成地图，
  faction 保持 Neutral/goodwill0。随后同格 `POST travel attack` 触发原生
  `AttackSettlement`：敌基地 map1、Human321/315 进入 map1、caravan275 从
  `Find.WorldObjects.Caravans` 消失，Arberium Neutral/goodwill0 →
  Hostile/goodwill -87。fixture direct tile 仅测试准备并披露。
- 未测：site/peace talks/escape ship 实际到达（只验 settlement）；settle/
  abandon 多地图/存档重载、caravan 被事件中断；交易复用 TradeControl。


- 可复跑 runner：`tests/c10_caravan_journey.py`（`--base/--profile/--evidence`；fixture 的 `CoreCaravanFixture`）。2026-11-13 runner 在隔离 g35 上 **passed**：settle→map2，abandon→map1，foreign visit 从 101493 单步到 478，attack→enemy map、Arberium -87、caravan 消失。