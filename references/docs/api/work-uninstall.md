# 单个建筑即时卸装

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.uninstall`。暂停时也能执行，每次只把一个现有建筑卸装为原版 minified 物品；不替代蓝图/框架施工路径、不刷物、不重建。

## 查目标

- `GET /api/v1/map/buildings?map_id=0` 列出殖民者建筑，数字 `id` 可直接作为 `thing_id`。
- `GET /api/v1/map/things/radius?map_id=0&x=<x>&z=<z>&radius=1` 也可取到该处建筑的数字 `thing_id`；也可传完整内部 ID（如 `TableSculpting1234`）。

```json
{"action":"work.uninstall","map_id":0,"thing_id":"27979","dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：字符串，必填；数字 `thingIDNumber` 或内部 `ThingID`。
- `dry_run`：可选，默认 `false`。

## 资格

目标必须是已生成的 `Building`，并满足与原版卸载按钮相同的条件：

- `def.Minifiable`（定义里有 `minifiedDef`）；
- `Faction == Faction.OfPlayer`，或 `def.building.alwaysUninstallable`。

拒绝：蓝图/框架（走施工路径）、普通物品、不可 minify 的定义（如电力导管）、非玩家且不可卸装的建筑。工作台上若有进行中的未完成物（`Bill_ProductionWithUft.BoundUft != null`），先返回 422，避免把未完成物与工作台拆散。

## 语义

调用 `MinifyUtility.Uninstall(thing)`（与 `JobDriver_Uninstall.FinishedRemoving` 相同）：

1. `MakeMinified` 先 `DeSpawnOrDeselect` 原建筑，再创建一个 `def.minifiedDef`（通常为 `MinifiedThing`）的外层物品，把原建筑放进 `InnerThing`（堆叠数固定 1）；
2. 若建筑原本绑定安装蓝图，蓝图会改绑到新的 minified 物品；
3. 在建筑原格附近用 `GenPlace.TryPlaceThing(..., ThingPlaceMode.Near)` 放置外层物品，播放卸载音效；
4. 本动作补上原版 Job 的收尾：`RemoveAllDesignationsOn(target)`（清掉 `Uninstall` 等东西类标记）；阵营通知对玩家阵营是 no-op。

账单与内部状态随原建筑对象保留在 `InnerThing` 里，不丢失；品质、材料、HP、旋转、账单都在读回中返回。未产生：卸载工作进度、建造经验、`ThingsUninstalled` 记录，也不会自动重建或返还材料。

`dry_run` 只读，返回目标状态、`minified_def_name`、`placement_feasible`（有界空间预检）等。

## 响应

- `before` / `after`：`thing_id`、`def_name`、`stuff_def_name`、`label`、`map_id`、`x`、`z`、`rotation`、`hit_points`、`max_hit_points`、`quality`、`faction`、`minifiable`、`always_uninstallable`、`uninstall_work`、`spawned`、`destroyed`、`designations`、`bills`、`installation_blueprint_thing_id`。卸装后 `spawned=false`、`destroyed=false`、`designations=[]`。
- `minified`：外层物品 `thing_id`、`def_name`、`label`、`map_id`、`x`、`z`、`spawned`、`stack_count`，以及 `inner`（原建筑 `thing_id`/`def_name`/`stuff_def_name`/`hit_points`/`max_hit_points`/`quality`/`rotation`/`bills`）。
- `installation_blueprint_after`：原本有安装蓝图时给出蓝图与改绑对象的 ID，否则省略。
- 结果：`minified_def_name`、`placement_feasible`、`applied`、`changed`、`native_invoked`、`native_methods`、`outcome`。
- 预演：`would_uninstall=true`，`before` 与 `after` 相同。

## 错误与幂等

- 缺 `map_id` 或 `thing_id` → 400。
- 地图或目标不存在 → 404；已卸装（建筑已 despawn）后用新参数再请求同样 404，不生成第二个 minified。
- 蓝图/框架、非建筑、不可 minify、非玩家且不可卸装、存在进行中的未完成物、附近无放置格 → 422。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不生成第二个 minified；同键换请求 → 409。
- 原生放置本身不检查失败；本动作在调用前做有界空间预检。若仍出现未放置的极端情况，返回 `outcome: uncertain` 与 `after`/`minified` 读回，先核对再决定，不要盲目重试。
