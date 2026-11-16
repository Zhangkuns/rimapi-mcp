# Core 温控目标与升降/重置（C-23）

2026-11-13 新增。只写原生 `CompTempControl.TargetTemperature`（以及
`Comp_AtmosphericHeater` 的 override）；不写 `Map.mapTemperature` 或 cell
最终温度。供电/燃料只是 Working 状态，**不等价于温控目标**。

## 读取（只读）

`GET /api/v1/building/temperature?thing_id=`

- `has_temp_control`、`comp_type`（`CompTempControl` /
  `Comp_AtmosphericHeater`）、`target_temperature`（AtmosphericHeater 无
  condition 时为 null，默认 20）、`default_temperature`、`min_temperature`
  -273.15、`max_temperature` 1000。
- `actual_temperature`（原生 `GenTemperature.GetTemperatureForCell`）、
  `outdoor_temperature`、`map_id`/`position`。
- AtmosphericHeater：`unnatural_heat_condition`（active/heater_target_temp/
  permanent）、`powered`/`has_fuel`/`working`/`temp_offset`（只读）。
- 无温控 comp 的建筑（如仅有供电的 StandingLamp）→ 422，并明确
  “power/fuel is not a temperature target”；未知 thing 404。

## 应用

`POST /api/v1/building/temperature`

```json
{"thing_id":"Cooler75478","target_temperature":5,"dry_run":false}
{"thing_id":"Cooler75478","action":"raise_1","dry_run":false}
```

- `target_temperature` 与 `action` 二选一；同给/都不给 400。
- 绝对目标 -273.15..1000，越界 422；`action` 仅
  `raise_1`/`lower_1`/`raise_10`/`lower_10`/`reset`（reset=21），步长按
  原生偏移换算并 `Mathf.Clamp`。
- 只写 `comp.TargetTemperature`；AtmosphericHeater override 会创建/注册
  原生 `UnnaturalHeat` 并写 `heaterTargetTemp`，随后读回。
- `dry_run` 只校验并返回 before/候选 after，不写 target、不创建/注册
  condition；响应始终含 before/after target、actual、unnatural heat、
  power/fuel/working。
- actual 温度只读，API 不做房间升降温模拟。

## 证据与边界

2026-11-13 隔离 g35 副本：fixture 铺硬地并放置原生 Cooler/Heater/
AtmosphericHeater/StandingLamp（均离网、未供能），产品只写 native target。

- Cooler 初始 target 21，min/max 正确，actual 22.8125 与户外 22.9411 读回；
  Heater 21；AtmosphericHeater target/heaterTargetTemp 20、condition active。
- Cooler：绝对 5 → raise_1 6 → lower_1 5 → raise_10 15 → lower_10 5 →
  reset 21；Heater 10；每步 actual 保持 22.8125（未强改）。
- AtmosphericHeater dry_run 30 不写（condition 仍 20）；真实设置后
  target/heaterTargetTemp 均 30，actual 仍 22.8125。
- 4xx：both/neither/bad action 400；越界 -300/1001 422；Lamp GET/POST 422；
  未知 thing 404。
- `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。
- 未测：供能 Cooler/Heater 的实际房间换热（fixture 放在离网地面）、多
  AtmosphericHeater 叠加、温度驱动的最终环境变化。
