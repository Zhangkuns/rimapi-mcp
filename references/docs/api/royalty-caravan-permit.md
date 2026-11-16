# Royalty 商队世界许可（RY-R04）

已实现：`GET /api/v1/dlc/royalty/permit/caravan` 与
`POST /api/v1/dlc/royalty/permit/caravan`。覆盖原生
`RoyalTitlePermitWorker_DropResources.GetCaravanGizmos`（商队资源投送，含超重确认）
和 `RoyalTitlePermitWorker_CallShuttle.GetCaravanGizmos`（商队世界目标穿梭机）。
地图内许可样本保留在 `/permit/use` 与 `/permit/execute`，不复用为商队结果。

## 读取

`GET /api/v1/dlc/royalty/permit/caravan?pawn_id=Human321&target_tile=200`

```json
{"pawn_id":"Human321","in_caravan":true,
 "caravan":{"caravan_id":275,"name":"Blean's caravan","tile":137061,
   "mass_usage":77.4,"mass_capacity":70.0,
   "pawn_ids":["Human321","Human315"],
   "inventory":{"steel":150,"silver":100}},
 "permits":[{"permit_def":"SteelDrop","worker":"RoyalTitlePermitWorker_DropResources",
   "faction_def":"Empire","description":"... (Free)","free":true,
   "not_enough_favor":false,"last_used_tick":-1,"cooldown_ticks":2700000,
   "favor":0,"favor_cost":4,"usable_on_world_map":true,"can_use":true,
   "drop":{"items":[{"thing_def":"Steel","count":250,"mass":125.0}],
     "added_mass":125.0,"mass_after":202.4,"mass_capacity":70.0,"overweight":true}}]}
```

- 只列当前人物持有的、`usableOnWorldMap` 且 worker 为 DropResources/CallShuttle 的许可。
- `free`/`description`/`not_enough_favor` 来自原生 `FillCaravanAidOption`；
  `cooldown_ticks` 是 Def 的 40/45 天冷却，`last_used_tick=-1` 表示从未使用。
- `drop.overweight` 与原生 `GetCaravanGizmos` 的
  `caravan.MassUsage + itemsToDrop mass > MassCapacity` 判定一致。
- 穿梭机行带 `max_launch_distance`；显式 `target_tile` 时给
  `distance/valid/has_world_object/impassable` 和原生
  `CompLaunchable.GetOptionsForTile` 的到达选项（label/disabled），不执行。
- 人物在世界穿梭机上时返回 `in_caravan:false` 和 `transport`
  （travelling_id/tile/destination_tile/arrival_action/pawn_ids/cargo），
  用于起飞后读回。

## 执行

`POST /api/v1/dlc/royalty/permit/caravan`

```json
{"pawn_id":"Human321","permit_def":"SteelDrop",
 "confirm":true,"confirm_overweight":true,"dry_run":false}
```

```json
{"pawn_id":"Human315","permit_def":"CallTransportShuttle",
 "target_tile":200,"confirm":true}
```

- 资源投送：调用原生私有 `CallResourcesToCaravan(pawn,faction,free)`；返回
  `inventory_before/after`、`last_used_tick_before/after`、`favor_before/after`、
  `free`。超重时先返回 409（`overweight:true`、`mass_after`、`mass_capacity`），
  需显式 `confirm_overweight:true`；dry-run 只投影并给
  `requires_overweight_confirmation`，不写库存。
- 穿梭机：`target_tile` 必须是**无世界对象、可通行**的世界格；用原生
  `GetOptionsForTile` 生成到达选项（空地块通常为 `FormCaravanHere`），
  `arrival_option_index` 缺省时只在唯一选项下自动选择。之后按原生
  `CallShuttleToCaravan` 的 Launch 语义把整个商队的 inventory 和 directly-held
  things 放入 `ActiveTransporterInfo`，创建 `TravelingShuttle`，设置目标格与
  `arrivalAction`，`Notify_Used()`，非免费时扣 favor。
- 返回 `caravan_destroyed`、`pawn_ids_before`、`inventory_before`、
  `transport.destination_tile`、`transport.arrival_action`、`transport.cargo`、
  `last_used_tick_after`、`favor_after`。后续航程由原生日志推进；到达后
  `GET` 读到新商队及相同货物。
- 非 dry 必须 `confirm:true`；没有许可 409，未知 Def 404，非商队 422，
  地图/定居点目标 501（需要落点/到达选择 UI，不在本端点范围），
  超出 70 格或不可通行 422，favor 不足 409。
- 使用后进入 Def 的 40 天冷却；下一次 `free:false`，favor 不足时
  `not_enough_favor:true`、`can_use:false`。

## 原生验证

隔离 Xvfb `g35_deathresting` 副本（五 DLC，tick 76787）：
Acolyte/SteelDrop 与 Acolyte→Knight/CallTransportShuttle 经现有产品端点准备；
fixture 用 `CaravanMaker.MakeCaravan` 建立普通玩家商队（Steel150/Silver100）。
SteelDrop dry-run 不写；无 `confirm_overweight` 409；正式投送后 Steel 150→400、
`last_used_tick=76787`、下一次 `free:false`/`not_enough_favor:true`/`can_use:false`。
穿梭机目标格 200（距离 13，唯一 `FormCaravan here`）：dry-run 不销毁商队；
地图格目标 501；正式发射后商队销毁，`TravellingTransporters` 276 载有
Human321/Human315 与 Steel400/Silver100，目标格 200；推进 1200 tick 后到达，
新商队 277 位于 tile 200 且货物不变。

证据：`tools/cua/.runtime/ry-r04-20261112c/evidence/ry-r04.json`
（runner `tests/parallel/royalty/ry_r04_caravan_permit.py`，fixture
`tests/parallel/royalty/fixture/RyFixture.cs`）。

未重跑：地图/定居点落点、多个到达选项、FoodDrop/SilverDrop 等其它投送 Def、
完整 40 天冷却结束后的再次免费使用、跨星球层目标。
