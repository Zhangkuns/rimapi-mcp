# 单株成熟植物即时收获（WORK-D04）

`POST /api/v1/debug/native/execute` 的 `work.harvest` 结算一株成熟植物的一次收获。它是调试直接变更动作，需要活动存档与开发者模式；不创建收获 Job，也不替代正常收获工作。加载对应 DLL 后，通过 `GET /api/v1/debug/native/actions` 确认动作可用。

```json
{"action":"work.harvest","map_id":0,"thing_id":"Plant_Rice27979","pawn_id":800,"dry_run":true}
```

```json
{"action":"work.harvest","map_id":0,"thing_id":"Plant_Rice27979","pawn_id":800}
```

## 参数

- `map_id`、`thing_id`（`Plant` 的 `ThingID` 或数字 `thingIDNumber`）、`pawn_id`（同图、存活、玩家阵营）均必填。
- `dry_run:true` 只读取并预演，不写状态；正式执行时省略或设为 `false`。
- 可设 `expected_process_id`；同一次游戏内重试可用 `Idempotency-Key`，见[通用调试合同](../debug-native-actions.md)和[幂等合同](idempotency.md)。
- 目标必须是已 spawn、未销毁、`Harvestable`、growth 超过 `harvestMinGrowth`、未 blighted、`harvestYield>0` 且带 `harvestedThingDef` 的 `Plant`。

## 原生结算

动作按 `JobDriver_PlantWork` / `JobDriver_PlantHarvest` 的完成段执行：

1. `plant.YieldNow()`：按 growth、HP 与难度 `cropYieldFactor` 计算基础产量（原生 `RoundRandom`）。
2. 选择 `plant.def.plant.harvestedThingDef.IsDrug || drugForHarvestPurposes ? StatDefOf.DrugHarvestYield : StatDefOf.PlantHarvestYield`，取工人 `GetStatValue`；仅当值 `>1` 时按 `GenMath.RoundRandom(yield * statValue)` 加成（与原生一致，`<=1` 不再缩减）。
3. 主产物 `ThingMaker.MakeThing(harvestedThingDef)`，`stackCount` 为产量，`GenPlace.TryPlaceThing(..., pawn.Position, map, ThingPlaceMode.Near)`；成功落地后按原版通知 `Find.QuestManager.Notify_PlantHarvested` 并增加 `PlantsHarvested` 记录。
4. 若 `plant.HarvestableNow`，遍历 `plant.AllComps.GetAdditionalHarvestYield()`，同样在工人位置附近落地额外产物；额外产物计入 `extra_products`。
5. `plant.PlantCollected(pawn, PlantDestructionMode.Chop)`：`HarvestDestroys` 植物销毁（树木按原生生成树桩），否则回退到 `harvestAfterGrowth`；随后移除目标上的 `HarvestPlant` designation。

**随机边界**：本动作不掷 humanlike `harvestFailable` 的失败随机，响应 `debug_semantics` 明确写明“跳过收获失败随机；产物按 YieldNow×收获倍率结算”。生长、HP、难度、倍率、额外 comp 和植物销毁/回退生长仍按原版。

预演字段 `would_products` / `would_extra_products` 使用会随游戏 RNG 状态变化的原生 `RoundRandom` 估算，**不保证与正式执行的 `products` 数量逐点相同**；正式执行的 `yield_now`、`products`、`products_total` 才是实际结算结果。

## 读回

`before` / `after` 是目标植物的逐项快照：

```json
{
  "thing_id": "Plant_Rice27979", "def_name": "Plant_Rice", "map_id": 0,
  "x": 133, "z": 142, "spawned": true, "destroyed": false,
  "growth": 1.0, "hit_points": 85, "max_hit_points": 85,
  "blighted": false, "harvestable": true, "harvestable_now": true,
  "can_yield_now": true, "harvest_min_growth": 0.65, "harvest_yield": 6.0,
  "harvest_after_growth": 0.0, "harvest_destroys": true,
  "harvested_def_name": "RawRice", "is_tree": false, "life_stage": "Mature",
  "harvest_designated": true, "harvest_stat": "PlantHarvestYield",
  "harvest_multiplier": 1.10627, "products": [], "stump": null, "yield_now": 5
}
```

- `yield_now`：`before` 为预演值，顶层和 `after` 为正式结算值。
- `harvest_stat` / `harvest_multiplier`：实际使用的收获属性与工人倍率。
- `would_products` / `would_products_total` / `would_extra_products`：预演列表与合计。
- `products`：实际落地的产物栈，含 `thing_id`、`def_name`、`added`（本次新增数量）、`stack_count`（落地后该栈总数）、`x`/`z` 与 `sources`（`main` 或 `additional`；同栈合并时可能为 `main+additional`）。
- `extra_products`：实际 `sources` 含 `additional` 的产物子集。
- `plant_destroyed`、`growth_after`（未销毁时为回退后的 growth，销毁时为 null）、`stump`（树木原生产生的树桩；非树木为 null）。
- `harvest_designation_before` / `harvest_designation_removed` / `harvest_designation_after`：目标 `HarvestPlant` 标记的清理结果。
- `applied` / `native_invoked` / `changed` 均为 `true` 才表示实际调用原生并改变状态。`dry_run` 或全部拒绝路径为 `false`，`dry_run` 的 `after` 等于 `before`。

原生异常或落地/读回不符返回 500 与 `outcome:"uncertain"`，附尽可能的 `after` 读回；不静默报告成功。产物落地失败发生在 `PlantCollected` 之前时，植物保持原状，但已消耗的 RNG/预演不视为业务成功。

## 状态码与边界

- 缺 `map_id`/`thing_id`/`pawn_id`：400。
- 地图、目标或工人不存在：404。
- 目标不是 `Plant`、已销毁/未 spawn、未成熟/不可收获、blighted、`harvestYield<=0`、无 `harvestedThingDef`：422。
- 工人不在指定地图、已死亡或不属于玩家阵营：422。
- 关闭开发者模式：403。
- 游戏未就绪、进程不符或幂等键状态冲突：409。
- 原生异常/读回不符：500，`outcome:"uncertain"`。
- 只结算一株植物；不批量收获、不砍树、不屠宰、不播种，也不改变正常收获 Job/工作优先级。`plant.cut` 仍用于指定树木即时砍伐；`clean_room` 等正常派工不受影响。
- 当前 catalog 的 `native_method` 文案写作 `PlantCollected(pawn, Cut)`；实现按 `JobDriver_PlantHarvest` 使用 `Chop`，以本页和实际读回为准。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame` 均为 `103272`；rice/smokeleaf/agave、额外 comp 产物、拒绝路径、幂等与清理见[WORK-D04 验收与自审](../references/reviews/2026-11-21-work-d04-harvest.md)及[原始证据](../../tests/parallel/work-d04/evidence-work-d04-harvest.json)。正式 DLL 未部署，本页只描述源码与隔离 DLL 的行为。
