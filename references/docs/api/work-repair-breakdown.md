# 解除机器故障（免料调试修复）

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.repair_breakdown`。暂停时也能执行；对一个当前故障的设备调用原版开发者入口 `CompBreakdownable.Notify_Repaired()`，立即清除故障状态。

> **这是免料调试修复，不是正常维修。** 原版正常维修（`JobDriver_FixBrokenDownBuilding`）要搬运并消耗一个零部件、工作 1000 tick（建造技能）、再按 `FixBrokenDownBuildingSuccessChance` 判定成功；本动作不消耗任何零部件/材料、不产生工作与经验、不做成功率判定，也不改变 HP。它复用的是开发者 Gizmo “DEV: Fix breakdown” 的同一原生方法。

## 参数

```json
{"action":"work.repair_breakdown","map_id":0,"thing_id":"27979","dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：设备数字 `thingIDNumber` 或完整 `ThingID`，必填。
- `dry_run`：可选，默认 `false`。

## 校验（全部在原生调用前）

- 目标必须在指定地图上已生成；
- 目标必须带 `CompBreakdownable`（普通墙、物品、无该 comp 的建筑都拒绝）；
- 当前 `BrokenDown == true`（健康设备拒绝）。

失败返回 4xx，无任何副作用。

## 语义

调用 `CompBreakdownable.Notify_Repaired()`：

1. `brokenDownInt = false`；
2. `BreakdownManager.Notify_Repaired(parent)`：从 `brokenDownThings` 移除；
3. `parent is Building_PowerSwitch` 时请求电网重新注册该 transmitter（原版 PowerSwitch 没有 `CompProperties_Breakdownable`，纯原版下该分支不可达；带该 comp 的 mod 走同一原生分支）；
4. `UpdateOverlays()`：关闭 `OverlayTypes.BrokenDown`，故障检查文本消失。

修复后 `CompPowerTrader` 的 `PowerOn` **不会立即恢复**：故障时的 `"Breakdown"` 信号已把它置 false，`PowerNetTick` 只在 `!IsBrokenDown()` 时才重新上电；暂停现场读回会如实显示仍为 false，恢复要等下一个电网 tick。本动作不额外改电网或补 HP。

## 响应

- `before` / `after`：`thing_id`、`def_name`、`label`、`map_id`、`x`、`z`、`hit_points`、`max_hit_points`、`faction`、`broken_down`、`inspect_string`（故障时为 `Broken down`，修复后省略）、`manager_tracks_target`。
- `manager_before` / `manager`：`broken_down_count`、`broken_down_ids`（排序）。
- `power_before` / `power_after`（设备有功率/可闪烁组件时）：`has_power_comp`、`power_on`、`transmits_power_now`、`power_net_id`、`flickable_switch_on`、`battery_stored_energy`。
- `resources_before` / `resources_after`：`component_industrial`、`steel`、`wood` 全图计数（证明未消耗）。
- `other_broken_before` / `other_broken_after`：其它故障设备 `thing_id`、`def_name`、位置。
- 结果：`native_methods`、`applied`、`changed`、`native_invoked`、`outcome`（`repaired_dev`）；预演给 `would_repair=true`，`before == after`。

## 错误与幂等

- 缺 `map_id`/`thing_id` → 400；地图/目标不存在 → 404。
- 无 `CompBreakdownable`、未故障设备、物品/墙 → 422。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不重复修复；已修复目标新 key → 422；同键换请求 → 409。
- 不写 `FiresExtinguished` 之外的任何记录、不提交 Job、不批量整图；本动作只清故障标记，不改变设备 HP/材料/其它故障设备。
