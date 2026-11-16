# Core 目标燃料量、自动补给与正常加燃料（C-24）

2026-11-13 新增。全部基于原生 `CompRefuelable` 与
`RefuelWorkGiverUtility`；不直接 `Refuel()`、不凭空生成燃料。设备独有供料
开关（如 gravship 的 `/dlc/odyssey/gravship/refuel`）不重复，本组只做通用
`CompRefuelable`。

## 读取（只读）

`GET /api/v1/building/fuel?thing_id=`

- `has_refuelable`、`fuel`、`fuel_capacity`、`target_fuel_level`、
  `target_configurable`、`initial_target_fuel_level`、
  `fuel_percent_of_target/max`、`is_full`、`has_fuel`。
- `allow_auto_refuel`、`show_auto_refuel_toggle`、`should_auto_refuel_now`、
  `auto_refuel_percent`、`atomic_fueling`、`consume_fuel_only_when_used`。
- `fuel_filter`（summary/any_allowed_def/thing_defs）。
- 无 CompRefuelable → 422，明确电力/其他供料开关不是燃料策略；未知 404。

## 策略

`POST /api/v1/building/fuel`

```json
{"thing_id":"PodLauncher75478","target_fuel_level":75,"allow_auto_refuel":true}
```

- `target_fuel_level`：0..capacity，越界 422；`targetFuelLevelConfigurable=false`
  → 422；写原生 `TargetFuelLevel`。
- `allow_auto_refuel`：仅 `showAllowAutoRefuelToggle=true` 可写，否则 422；
  写原生 `allowAutoRefuel`。
- 至少一个字段，都无 400；dry_run 不写；返回 before/after 的 target/auto/
  fuel/status。一次 refuel 不重置 target/auto。

## 正常加燃料

`POST /api/v1/building/fuel/refuel`

```json
{"thing_id":"PodLauncher75478","pawn_id":"Human315","forced":false,"dry_run":false}
```

- 原生 `RefuelWorkGiverUtility.CanRefuel(pawn, thing, forced)` 校验；失败 422
  + 原生 `JobFailReason`。`forced=false` 遵从 auto/阈值/IsFull/燃料过滤；
  `forced=true` 走原生手动补给（auto 关闭也可）。
- 通过后 `RefuelWorkGiverUtility.RefuelJob(pawn, thing, forced)` +
  `TryTakeOrderedJob(JobTag.Misc)`；返回 job def/targets 与当前燃料策略。
- dry_run 不派 job；真实燃料由 native `JobDriver_Refuel` 消耗
  `fuelFilter` 物品，不直接写 `fuel`。

## 证据与边界

2026-11-13 隔离 g35：fixture 放原生 PodLauncher（targetConfigurable/
showToggle=true）、WoodFiredGenerator（target 不可配置）、Turret_MiniTurret
（无 auto toggle）、StandingLamp（无 comp）与 150 Chemfuel、搬运者。

- 初始 fuel 0/target 150/auto true；target 75 与 auto off/on 读回。
- auto off 时正常 refuel 422；auto on 后 native `Refuel` job 消耗 75，
  fuel=75、剩余 Chemfuel 75，target 75/auto 保持。
- target 150 + auto off：forced=false 422；forced=true dry 不派 job，真实
  forced job 启动；因搬运容量消耗 68，fuel=143、剩 7 Chemfuel，target 150/
  auto false 保持（原生 job 行为，未直接补满）。
- 4xx：target 200/-1 422；无字段 400；非配置 generator target 422；无 toggle
  turret auto 422；lamp GET/POST 422；未知 404。
- `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。
- 未测：`RefuelAtomic`、powered-only 消耗、多搬运者竞争、gravship 设备专用
  供料路由（文档交叉引用，不重复）。
