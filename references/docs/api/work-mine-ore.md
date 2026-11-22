# 单个矿体即时结算

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.mine_ore`。暂停时也能执行，每次只结算**一个现有矿体的剩余生命值**，不继续挖邻格、不批量整条矿脉、不启动挖矿工作。

## 查目标

- `GET /api/v1/map/ore?map_id=0` 按矿体 def 给出格子索引（`(z * map_width) + x`）与该格 HP。
- `GET /api/v1/map/things/radius?map_id=0&x=<x>&z=<z>&radius=1` 返回该处原生对象，其中的数字 `thing_id` 可直接作为参数；也可传完整内部 ID（如 `MineableSteel1234`）。

```json
{"action":"work.mine_ore","map_id":0,"thing_id":"27979","pawn_id":800,"dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：字符串，必填；数字 `thingIDNumber` 或内部 `ThingID`。
- `pawn_id`：整数，必填；必须与矿体同图、存活、未倒地且属于玩家阵营。工人的 `MiningYield` 参与本次结算。
- `dry_run`：可选，默认 `false`。

## 语义

按原版 `JobDriver_Mine.DoDamage` 的最终一击处理剩余 HP，不提交或接管任何 Job：

1. `Mineable.Notify_TookMiningDamage(remainingHP, miner)` 把本次贡献累加到矿体已保存的 `yieldPct`；
2. `Mineable.HitPoints = 0`；
3. `Mineable.DestroyMined(miner)` 按累计 `yieldPct` 结算一次产物、清理该格可挖标记并记录原生 `Mined` 事件。

因此**先前累计**（之前挖掉的部分、先前工人的贡献）会保留，本次只补上剩余 HP 这一段。`DestroyMined` 走 `base.Destroy(KillFinalize)`，不会触发 `Mineable.Destroy` 的额外产出；`GenLeaving` 对矿体无额外掉落。

产物数量由原生公式决定：先过 `def.building.mineableDropChance` 随机门槛，再取 `max(1, RoundRandom(EffectiveMineableYield × yieldPct))`。资源岩（如 `MineableSteel`）`dropChance=1`、`wasteable=true`，数量随 `yieldPct` 变化；天然石（如 `Sandstone`）`wasteable=false`、`dropChance=0.25`，只按固定产量判定是否掉石渣，与 `yieldPct` 无关。产物落在矿体格附近，可能并入已有堆。

`Mine`/`MineVein` 是格子标记并带 `removeIfBuildingDespawned`，随建筑 despawn 清除；本动作不做矿脉 flood fill，所以邻格标记与邻格矿体保持。

不产生：采矿经验、工作记录（`CellsMined`）、`StruckMineable` 消息/故事点、矿脉连挖、深钻或扫描。开发模式、主线程、`Idempotency-Key` 幂等由现有 `/debug/native/execute` 接线负责。

## 响应

- `before` / `after`：`thing_id`、`def_name`、`map_id`、`x`、`z`、`hit_points`、`max_hit_points`、`spawned`、`destroyed`、`mine_designated`、`mine_vein_designated`。
- 矿体与贡献：`mineable_thing`、`mineable_yield`、`effective_mineable_yield`、`mineable_drop_chance`、`mineable_yield_wasteable`、`is_resource_rock`、`worker_id`、`worker_number`、`worker_mining_yield`、`remaining_hit_points`、`hp_contribution_fraction`、`hp_contribution_yield_pct`。
- `yield_pct_readback: false` 加 `yield_pct_note`：`yieldPct` 是 `Mineable` 私有字段，本动作不做反射读回；先前累计以产物堆实际数量和本次 HP 贡献核对。
- 结果：`product_def_name`、`products[]`（`thing_id`、`def_name`、`stack_count`、`x`、`z`、本次新增 `added`）、`produced_count`、`applied`、`changed`、`native_invoked`、`native_methods`、`outcome`。
- 预演返回 `would_settle`，`before` 与 `after` 相同，不调用原生链、不改 HP/标记/产物。

产物读回是执行后在整张地图上对比同 def 物堆的数量差，新堆与并入已有堆都会出现在 `products[]`；`produced_count` 为本次实际落地总量。

## 错误与幂等

- 缺 `map_id`、`thing_id` 或 `pawn_id` → 400。
- 地图、矿体或工人不存在 → 404；矿体已销毁后再用新参数请求同样返回 404（`listerThings` 已无该对象），不刷产物。
- 目标不是存活 `Mineable`、工人死亡/倒地、工人不在该图或不是玩家阵营 → 422。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再结算；同键换请求 → 409。
- `outcome: uncertain`（原生链抛错）时先按响应的 `after` 与产物读回核对，再决定是否重试。
