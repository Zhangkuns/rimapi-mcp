# 普通设备即时补燃料（WORK-D12）

`POST /api/v1/debug/native/execute` 的 `work.refuel` 用**真实燃料物品**给一个现有 `CompRefuelable` 设备补燃料，并走原版 `CompRefuelable.Refuel(List<Thing>)`。它不创建搬运/补燃料 Job，也不替代正常派工；加载对应 DLL 后，通过 `GET /api/v1/debug/native/actions` 确认动作可用。

```json
{"action":"work.refuel","map_id":0,"thing_id":"Campfire27979","ingredients":["WoodLog27980","WoodLog27981"],"dry_run":true}
```

```json
{"action":"work.refuel","map_id":0,"thing_id":"Campfire27979"}
```

## 参数

- `map_id`、`thing_id`（设备 ThingID 或数字 thingIDNumber）必填。
- 可选 `ingredients[]`：显式燃料 ThingID 列表；省略或空数组时从设备同图按 `Props.fuelFilter` 选择燃料堆（按到设备距离、ThingID 排序）。
- `dry_run:true` 只完成选择/预检并读回，不扣燃料、不改 `fuel`。
- 可设 `expected_process_id`；同一次游戏内重试可用 `Idempotency-Key`，见[通用调试合同](../debug-native-actions.md)和[幂等合同](idempotency.md)。

## 原生语义

1. 目标必须 spawn 在同图、存在 `CompRefuelable`、`Props.fuelFilter` 非空且未满（`CompRefuelable.IsFull`）。已满、非 refuelable、未 spawn/已毁、错误地图/ThingID 均 4xx。
2. `required = CompRefuelable.GetFuelCountToFullyRefuel()`：`atomicFueling=true` 时是容量/难度燃料倍率（要求整容量），否则是 `TargetFuelLevel - Fuel` 的缺口（至少 1）。所有选中燃料堆的 `stackCount` 合计必须 >= `required`；不足直接 422，**不半补**。
3. 显式 `ingredients[]` 的每一项必须同图、spawn、非毁、是 `ThingCategory.Item`、被 `fuelFilter.Allows` 允许且不重复；否则 404/422，不扣任何燃料。
4. 真实执行只调用 `CompRefuelable.Refuel(List<Thing>)`：原生会从列表尾部 `Pop()`，按 `Mathf.Min(num, thing.stackCount)` 计量，`Refuel(num2)` 更新 `fuel`，再 `thing.SplitOff(num2).Destroy()`。本动作不调用 `Refuel(float)` 伪造，也不 `EjectFuel`。
5. 不模拟工作派发的 `allowAutoRefuel`、阵营、可达/预约/禁用和 `allowRefuelIfNotEmpty` 门；这是显式调试直接变更，只执行设备 filter、原子/缺口预检与同图约束。选中燃料可含被禁用物，仍按 filter 允许处理。

## 读回

```json
{
  "fuel_before": 0.0,
  "fuel_after": 20.0,
  "fuel_capacity": 20.0,
  "target_fuel_level": 20.0,
  "minimum_fueled_threshold": 0.0,
  "fuel_filter": ["WoodLog"],
  "fuel_filter_summary": "wood",
  "atomic_fueling": false,
  "fuel_multiplier": 1.0,
  "required_fuel": 20,
  "available_fuel": 20,
  "selection": "ingredients",
  "would_consume": [{"thing_id":"WoodLog27981","def_name":"WoodLog","quantity":5}],
  "consumed": [{"thing_id":"WoodLog27980","def_name":"WoodLog","quantity":15,"remaining_stack":0}],
  "items_total_before": 3528,
  "items_total_after": 3508,
  "items_delta": 20,
  "applied": true, "native_invoked": true, "changed": true
}
```

- `before`/`after` 各含 `device`、`fuel`、`items_total`；`fuel` 再含 `current/capacity/target/minimum/percent_of_max/is_full/filter/filter_summary/atomic_fueling/fuel_multiplier`。
- `consumed` 是事后按预留 Thing 引用对比 `stackCount` 的实际扣除：`def_name`、`quantity`、`thing_id`、`remaining_stack`、位置；`consumed_total` 与 `items_delta` 为总扣除量。
- `dry_run` 返回 `would_consume`、`fuel_before=fuel_after`、`consumed=[]`，`before=after`，物品总数不变；不生成/不销毁任何燃料堆。
- `applied/native_invoked/changed` 均 true 才表示真实扣料；`changed` 以实际扣除量为准。

## 状态码与边界

- 缺 `map_id`/`thing_id`：400。
- 地图、设备、显式燃料 Thing 不存在：404。
- 未 spawn/已毁、无 `CompRefuelable`、无 filter、已满、燃料总量不足、错误过滤、重复 ingredient、跨图 ingredient、原子缺口不足：422。
- 关闭开发者模式：403；进程不符/幂等键冲突：409。
- 原生异常或读回不一致：500，`outcome:"uncertain"`，`after` 保留现场；不静默报成功。
- 只处理普通设备一次补燃料；不做 `EjectFuel`、不做穿梭机专用路径、不做正常派工/批量补料。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；普通 `Refuel(List)` 扣 15+5 WoodLog、原子容量要求 20 并扣 10+10、自动选择、dry-run、幂等、已满/不足/错料/无燃料/非设备/毁设备拒绝见[WORK-D12 验收与自审](../references/reviews/2026-11-21-work-d12-refuel.md)及[原始证据](../../tests/parallel/work-d12/evidence-work-d12-refuel.json)。正式 DLL 未部署。
