# 营养膏机出一份（WORK-D16）

`POST /api/v1/debug/native/execute` 的 `work.dispense_paste` 让一台现有 `Building_NutrientPasteDispenser` 用**真实料斗食材**出一份营养膏，并走原生 `TryDispenseFood()`；原生只返回未落地的 `MealNutrientPaste`，本动作负责在交互格/相邻回退格用 `GenPlace.TryPlaceThing` 落地。它不创建烹饪/进食 Job，也不替代正常账单或派工。

```json
{"action":"work.dispense_paste","map_id":0,"thing_id":"NutrientPasteDispenser27979","pawn_id":800,"dry_run":true}
```

## 参数

- `map_id`、`thing_id`（dispenser）、`pawn_id` 必填。
- 工人须 spawn 在同图、存活、属于玩家阵营；本动作用它做资格与落地回退，不派正常 Job、不检查物理可达/预约。
- `dry_run:true` 只预演；省略/false 正式出餐。
- 可设 `expected_process_id`；同一次游戏内重试可用 `Idempotency-Key`，见[通用调试合同](../debug-native-actions.md)和[幂等合同](idempotency.md)。通用合同规定 `dry_run:true` 预览绕过执行缓存；因此同 key 重放/换 body 的 409 冲突针对**正式**请求，dry-run 仍每次只读预演。

## 原生语义

1. 目标必须 spawn、存在 `CompPowerTrader`、`PowerOn` 为真，且 `Building_NutrientPasteDispenser.CanDispenseNow` 为真；`HasEnoughFeedstockInHoppers()` 与 `FindFeedInAnyHopper()` 都必须成功，否则 422。
2. `TryDispenseFood()` 自己按 `def.building.nutritionCostPerDispense - 0.0001` 从 `FindFeedInAnyHopper()` 反复取料：`Mathf.Min(stackCount, CeilToInt(remaining / Nutrition))`，扣 stack/destroy，登记 `CompIngredients`，返回 `MealNutrientPaste`。
3. 本动作不手工扣料、不绕过料斗、不直接生成餐；真实路径只调用原生 `TryDispenseFood()`。
4. 原生返回值**未落地**。本动作按顺序尝试 `InteractionCell`、`InteractionCells`、机器四邻格、工人位置，用 `GenPlace.TryPlaceThing(..., ThingPlaceMode.Near)` 落地；实际位置读回 `placement.placed_cell`。通常应落在机器交互格。
5. 若 `TryDispenseFood()` 返回 null 但料斗已有实际扣除，返回 500 `uncertain` 并附实际料斗/物品读回，不假装成功；若落地失败也返回 uncertain，不丢餐或伪报成功。

## 读回

- `nutrition_before/after`：料斗合格食材总营养（count × Nutrition）。
- `hopper_food_before/after`：每个料斗的 def、count、thingID、单份营养、总营养；`hoppers` 给出料斗格、料斗 ThingID 与所有物品。
- `nutrition_cost`（0.3）、`consumed`（实际扣料：def、数量、thingID、剩余 stack、位置）、`consumed_total`。
- `product`：产物 def、thingID、stack、位置、nutrition、`CompIngredients.ingredients` 实际登记食材。
- `machine`：power_on、power_net、power_net_energy、power_output、can_dispense_now、has_enough_feedstock、interaction_cell、nutrition_cost。
- `items_total_before/after`、`applied/native_invoked/changed`；`dry_run` 另有 `would_consume`、`would_ingredients`、`would_product`，且 `consumed=[]`、`product=null`、前后不变。
- `placement`：preferred_cell、placed_cell、mode、actual（interaction_cell/near）。

## 状态码与边界

- 缺 `map_id`/`thing_id`/`pawn_id`：400。
- 地图、设备、工人不存在：404。
- 非 dispenser、未 spawn/已毁、无 power comp、断电、料斗不足、料斗无合格食材、工人死亡/不同图/非玩家阵营、`CanDispenseNow=false`：422。
- 关闭开发者模式：403；进程不符/幂等键冲突：409。
- 原生返回 null 但有扣料、落地失败、读回不一致：500 uncertain，保留实际料斗/物品/产物读回。
- 只出一份；不批量、不刷食物、不绕过料斗、不替代普通烹饪/账单，也不做 `EjectFuel` 或穿梭机专用路径。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；真实 `TryDispenseFood` 扣 6 份肉料并生成 1 份营养膏，落点在交互格；dry-run、料斗不足/错料、断电、幂等、毁机拒绝见[WORK-D16 验收与自审](../references/reviews/2026-11-21-work-d16-dispense-paste.md)及[原始证据](../../tests/parallel/work-d16/evidence-work-d16-dispense-paste.json)。正式 DLL 未部署。
