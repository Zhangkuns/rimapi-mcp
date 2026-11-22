# 尸体腐败只读查询（WORK-R01）

普通只读 REST，不走 `/debug/native/execute`，不要求开发者模式：

```http
GET /api/v1/map/corpse/rot?map_id=0&thing_id=27990
```

## 支持范围

- 只读取当前地图上 **spawned** 的 `Corpse`（`thing is Corpse`），按 `thingIDNumber` 在 `map.listerThings.AllThings` 查找。
- 墓穴、容器、运输舱等 **内部尸体本期不支持**：它们不在 `map.listerThings` 的 spawned 列表中，查询返回 404；后续如需要按容器搜索应另开条目。
- 唯一匹配目标；不支持批量、区域、跨地图或按名称查询。
- 只读：不写 `RotProgress`、不推进 tick、不改变温度/物品/尸体；请求在游戏主线程按普通请求队列执行。

## 参数与响应

| 参数 | 必填 | 说明 |
| --- | --- | --- |
| `map_id` | 是 | 地图 `uniqueID` |
| `thing_id` | 是 | 尸体 `thingIDNumber`（整数） |

成功返回 `CorpseRotDto`：

| 字段 | 说明 |
| --- | --- |
| `map_id`、`thing_id` | 查询坐标 |
| `def_name`、`label` | 尸体 def 与原生 label（可能含人名/职业） |
| `position` | `x/y/z`，地面 `y=0` |
| `rot_stage` | 原生 `CompRottable.Stage`：`Fresh` / `Rotting` / `Dessicated` |
| `rot_progress` | 原生 `RotProgress` |
| `rot_progress_pct` | 原生 `RotProgressPct`（相对 `TicksToRotStart`；干枯可大于 1） |
| `temperature` | 尸体当前 `AmbientTemperature` |
| `active` | 原生 `CompRottable.Active`（含 `disabled`/hatcher 规则） |
| `frozen` | `GenTemperature.RotRateAtTemperature(temperature) < 0.001`，与原生“CurrentlyFrozen”提示一致 |
| `rot_destroys` | 原生 `PropsRot.rotDestroys` |
| `ticks_to_rot_start` | 原生 `PropsRot.TicksToRotStart` 绝对阈值 |
| `ticks_to_dessicated` | 原生 `PropsRot.TicksToDessicated` 绝对阈值 |
| `hours_until_rot` | 距 `TicksToRotStart` 的估算小时，见下 |
| `hours_until_dessicated` | 距 `TicksToDessicated` 的估算小时 |
| `estimate_reason` | 无法估算时的原因；可估算/已过阈值时为 `null` |

`hours_until_*` 与 `estimate_reason` 即使为 `null` 也会显式返回 JSON `null`。

## 时间规则

- 1 游戏小时 = 2500 ticks。
- 以尸体当前环境温度、当前 `RotRateAtTemperature` 持续不变、游戏时间推进为估算；暂停时只是快照，不预报暂停期间变化。
- 若 `threshold - RotProgress <= 0`：该阈值为 `0`（即使同时冻结/不活跃）。
- 否则：
  - `active == false`：`null`，`estimate_reason="inactive"`。
  - `rate < 0.001`（冻结/不推进）：`null`，`estimate_reason="frozen"`。
  - 否则 `round((threshold - RotProgress) / rate / 2500)`。
- `hours_until_rot` 与 `hours_until_dessicated` 分别用各自阈值；干枯阈值已过时两者都可能为 `0`。
- `rot_stage` 直接取原生阶段；无 `CompRottable` 时返回 422 `invalid_target`，不会把“无组件”误报为 `Fresh`。

## 状态码

- 缺 `map_id`/`thing_id`、非整数：400 `bad_request`（由现有 RequestParser 产生）。
- 地图不存在、尸体不存在、ID 指向非尸体、容器内尸体：404 `not_found`。
- `Corpse` 没有 `CompRottable`：422 `invalid_target`（不适用）。
- 只读，无 403/409/幂等键语义；不需要 DevMode。
- 意外异常：500，服务不吞掉成空 `Fresh`。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；4 具真实 `Corpse_Human`（Fresh/Rotting/Dessicated/Frozen），原生阈值 150000/300000 ticks；`hours_until_rot` 分别 `60/0/0/null`，冻结回暖到 21°C 后恢复 `60`，查询前后尸体与物品状态不变。详见[WORK-R01 验收与自审](../references/reviews/2026-11-21-work-r01-corpse-rot.md)和[原始证据](../../tests/parallel/corpse-rot/evidence-work-r01-corpse-rot.json)。正式 DLL 未部署。
