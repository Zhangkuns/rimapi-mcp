# 按原生规则移除地板

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.remove_floor`。暂停时也能执行；逐格移除现有地板顶层，走原版 `JobDriver_RemoveFloor` 的效果，不直接 `SetTerrain` 伪造地形。

## 参数

```json
{"action":"work.remove_floor","map_id":0,"cells":[{"x":134,"z":154}],"dry_run":true}
{"action":"work.remove_floor","map_id":0,"point_a":{"x":134,"z":154},"point_b":{"x":136,"z":154}}
```

- `map_id`：整数，必填。
- `cells[]`：每项 `x`/`z`，与 `point_a`/`point_b` 二选一；矩形包含两个端点及其间所有格。
- `dry_run`：可选，默认 `false`。

## 资格（逐格，全部格先校验再改动）

- 格在地图内；
- `Map.terrainGrid.CanRemoveTopLayerAt(cell)`：临时地形可移除，或顶层可移除（`layerable`）且其下有 under 地形；
- 该格没有全占且不可通行的建筑；
- `WorkGiver_ConstructRemoveFloor.AnyBuildingBlockingFloorRemoval(cell, map)` 为假（移除后剩余地形仍能支撑该格建筑所需的 terrain affordance）。

任一格不合格 → 422，`rejected_cells` 给出每格原因，且请求不会改动任何格（无半移除）。裸土、无 under 层的地板、被墙占用的格、越界格都会被拒绝。

## 语义（与 `JobDriver_RemoveFloor.DoEffect` 相同）

逐格：

1. 若该格有可移除的临时地形（`TempTerrainAt(cell).Removable`）→ `RemoveTempTerrain(cell)`（不产生返料）；
2. 否则 `RemoveTopLayer(cell)`（默认 `doLeavings: true`）。原生实现内部还会分支：有临时地形→先移除临时层；只有 foundation 且无 under→移除 foundation；否则按顶层成本掉落返料，并把 under 提升为顶层；
3. `FilthMaker.RemoveAllFilth(cell, map)`：清掉该格全部污物；
4. 按原版驱动的 `clearSnow` 把该格雪深/沙深清零；
5. 删除该格 `RemoveFloor` 标记（原版 Job 完成后的收尾；原生地形变更效果通常已删除）。

原生 `DoTerrainChangedEffects` 还会按新地形处理植物、污物、蓝图/框架、路径与区域通知等副作用，本动作不额外补偿。未产生：拆除工作与建造经验。

`dry_run` 只读，返回目标格现状、`expected_leavings`（分支与返料范围）与 `would_remove_count`。

## 响应

- `before` / `after`：逐格 `x`、`z`、`terrain`、`foundation`、`under_terrain`、`temp_terrain`、`filth[]`（`def_name`/`count`）、`snow_depth`、`sand_depth`、`remove_floor_designated`。
- `expected_leavings[]`：逐格 `branch`（`top_layer` / `temp_terrain_explicit` / `temp_terrain_via_top_layer`）、`leaving_layer_def_name`、`resources_fraction_when_deconstructed`、`items[]`（`def_name`/`cost`/`count_low`/`count_high`）、`count_low`/`count_high`。
- `products[]`：本次实际新增的物品堆 `thing_id`、`def_name`、`stack_count`、`x`、`z`、`added`；`produced_count` 为新增总数。
- 结果：`cell_count`、`native_methods`、`applied`、`changed`、`native_invoked`、`outcome`（`removed`）。
- 失败：`rejected_cells[]`（`x`/`z`/`reason`）、`cell_count`，`applied` 与 `native_invoked` 均为 false。

## 错误与幂等

- 缺 `map_id`、既无 `cells` 也无完整 `point_a`/`point_b`、格缺 `x`/`z` → 400。
- 地图不存在 → 404。
- 越界、无可移除层、被全占不可通行建筑阻挡、建筑失去地形支撑 → 422（附 `rejected_cells`）。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再次移除、不再次返料；已无可移除层的新请求 → 422；同键换请求 → 409。
- 原生异常时返回 `outcome: uncertain` 与逐格读回，先核对再重试。
- 本动作只移除顶层/临时层，不会移除 foundation（`CanRemoveTopLayerAt` 不会让 foundation-only 格通过）；移除 foundation 走原版独立设计ator，不在本动作范围。
