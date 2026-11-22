# 现有物品整堆转移

`GET /api/v1/storage/contents?map_id=0&thing_id=Shelf27979` 读取地图上玩家货架、料斗等格子储存的实际格表。查询储存区时传 `zone_id` 代替 `thing_id`。返回总 `stack_capacity`、`free_slots`，以及 `cells` 中各格的坐标、`stack_capacity`、`used_slots`、`free_slots` 和 `items`（含原堆 ID、数量及禁止状态）。堆位是原生格子容量；某种物品能否放进去还取决于储存过滤等规则。

原版 `Shelf` 两格各 3 堆，`ShelfSmall` 一格 3 堆。此查询的 `stack_capacity` 来源为格子的原生 `GetMaxItemsAllowedInCell`。可用[普通货架堆位设置](shelf-capacity.md)统一调整当前存档所有普通货架；`/storage/settings` 的过滤和优先级不改变堆位。堆位与一种物品单堆允许的数量是两个不同的限制。

通过 `POST /api/v1/debug/native/execute` 提交 `item.transfer`。例如，把地上的指定整堆放到指定货架，并让游戏选择有堆位且允许该物品的储存格：

```json
{
  "action": "item.transfer",
  "map_id": 0,
  "thing_id": "Steel28001",
  "source": {"kind": "cell", "x": 10, "z": 4},
  "destination": {"kind": "storage", "thing_id": "Shelf27979"},
  "dry_run": true
}
```

| `kind` | 位置参数 | 含义 |
| --- | --- | --- |
| `cell` | `x`, `z` | 明确的地图格，也可用来指定货架的其中一格 |
| `storage` | `thing_id` 或 `zone_id` | 玩家格子储存建筑或储存区；按 z、x 顺序选择第一个合格目标格 |
| `container` | `thing_id` | 玩家书柜、装备架、基因库、普通运输舱、乘客穿梭机或地图上符合资格的人物背包 |

两端可互换，地图格和容器之间可双向转移，两个支持的容器也可直接转移。顶层 `thing_id` 是物品堆本身，位置中的 `thing_id` 是货架、容器或人物。ID 可用完整 `ThingID` 或数字 ID。同一地图上一次只搬一整堆，不拆分、不合并，保留 ID、数量和禁止状态。对于 `storage`，同一储存里的物品不需要搬动时返回无变更；要在同一货架内换格请用 `cell` 指定落点。

预演返回 `would_transfer` 和选中坐标，可去掉 `dry_run` 正式执行。按[幂等合同](idempotency.md)使用 `Idempotency-Key`。检查 `after.item`、`after.at_source: false` 和 `after.at_destination: true`；`source` 与 `destination` 返回解析后的身份。操作受 DevMode 保护，在游戏主线程通过原生格子或容器 API 完成。来源不匹配、无可用堆位、过滤或容器资格不符会拒绝；例如基因库只接收合格基因包并计入已排队需求，运输容器遵守载重和装载状态。

查内部容器物品 ID 时使用相应领域的内容查询；`storage/contents` 只列地图格的物品。未开放任意内部容器、跨图或部分堆转移。仍可用 [item.move](item-move-debug.md) 将地图上的物品直接移到明确坐标。实测范围见[验收记录](../references/reviews/2026-11-20-item-transfer.md)。
