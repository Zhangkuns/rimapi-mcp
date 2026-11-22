# 全局设置普通货架堆位

`GET /api/v1/storage/shelf-capacity` 读取当前存档的普通货架上限，无须开发者模式。返回每座的 `stack_capacity`、两格的 `cell_capacities`、原版 `default_stack_capacity` 和 `overridden`。`GET /api/v1/storage/contents?map_id=…&thing_id=…` 可核对具体货架各格的实际堆位与原物品。

在活动存档开启开发者模式后，向 `POST /api/v1/debug/native/execute` 提交：

```json
{"action":"storage.shelf_capacity.set","stack_capacity":99}
```

`stack_capacity` 是**每座普通双格货架的总堆数**，可填 2 至 1000 的整数。填 `10` 表示两格各 5 堆；填 `99` 表示 50 + 49，按地图坐标先 z 后 x 排序的第一格多 1 堆；填 `6` 恢复原版默认值。设置立即对当前存档所有地图上已有和以后建造的普通 `Shelf` 生效，保存该存档后仍然有效。不要传 `thing_id`、`map_id` 或 `def_name`。小货架、书柜、其他储存及一堆里能装多少件物品保持原规则。

调低时如果任一现有格里的堆数超过新上限，返回 `409`，并给出货架 ID、地图、坐标和要先移出的堆数。容量与已有物品保持不变；移出后可再设置。可传 `dry_run:true` 预演，查看 `proposed`，此时 `before` 与 `after` 保持原值。正式执行后查询上限，或检查 `after.stack_capacity`。重复设置同一数值无变更；网络重试按[幂等合同](idempotency.md)使用 `Idempotency-Key`。

本功能是调试规则变更。它接到原生格子容量和储存总容量读取，原版放置、搬运资格和 `item.move` / `item.transfer` 共用该上限；物品过滤仍然生效。见[隔离验收](../references/reviews/2026-11-20-shelf-capacity.md)。
