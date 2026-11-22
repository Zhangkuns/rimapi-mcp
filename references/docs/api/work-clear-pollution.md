# 清理污染地面

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.clear_pollution`。需要 Biotech（污染系统）；暂停时也能执行。以一个污染格为 root，按原版 `JobDriver_ClearPollution` 的规则清理最多 6 格污染，并生成 1 个废料包。

## 参数

```json
{"action":"work.clear_pollution","map_id":0,"x":147,"z":155,"dry_run":true}
```

- `map_id`：整数，必填。
- `x`、`z`：root 格坐标，必填（整数）。
- `dry_run`：可选，默认 `false`。

## 校验（全部在修改前）

- root 在地图内；
- `map.pollutionGrid.IsPolluted(root)` 为真（Biotech 未启用时恒假）；
- `map.areaManager.PollutionClear[root]` 为真（root 在污染清理区域内）。

任一不合格返回 4xx，且不会清除污染或生成废料包。

## 语义（与 `JobDriver_ClearPollution.ClearPollutionAt` 相同，但不跑 5600 工作）

1. 取 `PollutionClear.ActiveCells` 中 `DistanceToSquared(root) < 100` 的格，按距离平方排序；
2. 逐格按原版 `CanUnpollute` 顺序检查：未污染 → 跳过；其它殖民者正在清理该格（`CurJobDef == ClearPollution` 且目标格相同）→ 跳过；该格有房间且与 root 房间不同 → 跳过；距离 > 100 → 跳过。通过则 `IntVec3.Unpollute(map)`（即 `PollutionGrid.SetPolluted(cell,false)`），最多 6 格；
3. 不足 6 格时用 `GenRadial.RadialCellsAround(root, 10, true)` 补齐：首轮同样跳过“其它殖民者正在清理”的格；仍不足时再走一轮忽略该跳过；
4. 生成 `ThingDefOf.Wastepack`：`CellFinder.FindNoWipeSpawnLocNear(root, map, Wastepack, Rot4.North)`，位置无效则回退到 root，`GenSpawn.Spawn` 恰好 1 个。

响应中的 `skipped[]` 记录实际跳过与原因（`not_polluted`/`other_pawn_cleaning`/`other_room`/`too_far`），`cleared[]` 记录实际清除格与来源（`area`/`radial`/`radial_ignore_pawns`）。

不修改 terrain/filth，不批量整图，不替代普通污物清洁（D01），不刷废料包。

## 响应

- `before` / `after`：root 的 `x`、`z`、`polluted`、`clear_area_member`、`room_id`。
- `cleared[]`：`x`、`z`、`distance_squared`、`source`；`planned[]` 为执行前选择（dry_run 也返回）。
- `skipped[]`：`x`、`z`、`reason`、`source`（最多 128 条）。
- `total_pollution_before` / `total_pollution_after`；`remaining_samples[]`（root 半径 10 内仍污染的样本，最多 10 条）。
- `wastepack`：`thing_id`、`def_name`、`x`、`z`、`spawned`、`used_root_fallback`；`wastepack_count_before/after`。
- `native_methods`、`applied`、`changed`、`native_invoked`、`outcome`（`cleared`）。
- 预演：`would_clear`（计划格数）、`would_wastepack=true`，`before == after`，不生成废料包。

## 错误与幂等

- 缺 `map_id`/`x`/`z` → 400；地图不存在 → 404。
- root 越界、未污染、不在 PollutionClear 区域 → 422。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再清格、不再生成第二包；已清 root 新 key → 422（root 已不污染）；同键换请求 → 409。
- 原生异常时返回 `outcome: uncertain` 与 root/总数读回，先核对再决定。
