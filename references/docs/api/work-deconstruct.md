# 单个建筑即时拆除

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.deconstruct`。暂停时也能执行，每次只拆除**一个现有建筑**，走原版拆除路径并保留原生返料与清理；不拆蓝图或框架，不刷料、不批量。

## 查目标

- `GET /api/v1/map/buildings?map_id=0` 列出殖民者建筑，数字 `id` 可直接作为 `thing_id`。
- `GET /api/v1/map/things/radius?map_id=0&x=<x>&z=<z>&radius=1` 返回该处建筑/物品/矿体（蓝图不在其中），数字 `thing_id` 同样可用；也可传完整内部 ID（如 `Wall1234`）。

```json
{"action":"work.deconstruct","map_id":0,"thing_id":"27979","dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：字符串，必填；数字 `thingIDNumber` 或内部 `ThingID`。
- `dry_run`：可选，默认 `false`。

## 资格

目标必须是已生成的 `Building`，且 `Building.DeconstructibleBy(Faction.OfPlayer)` 接受。该原生判定依次为：有 comp 强制可拆 → `def.building.IsDeconstructible`（`alwaysDeconstructible`，或非天然岩且 `deconstructible`）→ 开发者上帝模式 → 建筑属于玩家阵营 → `alwaysDeconstructible` → 玩家可认领。天然岩/矿体因此不可拆。

蓝图与框架不属于本动作：它们走施工路径（`construction.complete`），本动作返回 422。

## 语义

调用与 `JobDriver_Deconstruct.FinishedRemoving` 相同的 `Building.Destroy(DestroyMode.Deconstruct)`：

1. `Building.Destroy` 先执行原生拆墙通知（平滑墙会触发邻墙去平滑规则），再进入 `Thing.Destroy`；
2. `Thing.Destroy` 先 despawn（`Building.DeSpawn` 清理格子标记与登记），再由 `GenLeaving.DoLeavingsFor` 按拆除规则返料，最后清掉该建筑上的东西类标记——包括 `Deconstruct`；
3. 之后 `Building.Destroy` 取消该建筑的安装蓝图、播放拆除音效。

不会：提交拆除 Job、扣材料、重建图纸（自动重建只在 `KillFinalize` 触发）、记录 `ThingsDeconstructed`、加建造经验。拆除模式下的返料不会被禁拾取（那是 `KillFinalize` 的规则）。

## 返料规则（原生）

- 只有 `def.resourcesFractionWhenDeconstructed != 0` 才有返料；该值在 `BuildableDef` 上，默认 `0.5`。核心示例：电力导管与营火为 `0`（拆了什么都不返），加热器用默认值。
- 成本表使用 `CostListAdjusted()`：`costList` 加上 `costStuffCount` 对应材料（按材料体积换算），与建造消耗一致。
- 每项：`forcedCostLeavings` 全量返还；`leavingsBlacklist` 完全不返；其余取 `min(RoundRandom(count × fraction), count)`，即 `count × fraction` 向下取整或再 +1。
- 返料落在建筑占用格（外扩 `killedLeavingsExpandRect` 格）的随机格，使用 `ThingPlaceMode.Near`，可能并入已有同 def 物堆。
- `dry_run` 只返回成本表与预计区间，不改变任何状态。

## 响应

- `before` / `after`：`thing_id`、`def_name`、`stuff_def_name`、`label`、`map_id`、`x`、`z`、`rotation`、`hit_points`、`max_hit_points`、`spawned`、`destroyed`、`designations`（该建筑上的标记名）。拆除后 `destroyed=true`、`spawned=false`、`designations=[]`。
- 成本与预期：`cost_list`、`resources_fraction_when_deconstructed`、`forced_cost_leavings`、`leavings_blacklist`、`expected_leavings[]`（`def_name`、`cost`、`forced`、`blacklisted`、`count_low`、`count_high`、`before_total`）。
- 实际返料：`products[]`（`thing_id`、`def_name`、`stack_count`、`x`、`z`、本次新增 `added`）、`produced_count`。
- 结果：`applied`、`changed`、`native_invoked`、`native_methods`、`outcome`。
- 预演：`would_deconstruct=true`，`before` 与 `after` 相同。

`products` 是执行前后整图同 def 物堆的数量差，新堆与并入已有堆都会出现；`produced_count` 为本次实际新增总量。返料数量是原版 `RoundRandom` 结果，应在 `expected_leavings` 区间内核对，不要当成固定值。

## 错误与幂等

- 缺 `map_id` 或 `thing_id` → 400。
- 地图或目标不存在 → 404；已销毁目标用新参数再请求同样 404，不重复返料。
- 非建筑、蓝图/框架、天然岩等不可拆目标、`DeconstructibleBy` 拒绝 → 422（错误文本带原生拒绝原因）。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再返料；同键换请求 → 409。
- `outcome: uncertain` 时先按 `after` 与产物读回核对，再决定是否重试。
