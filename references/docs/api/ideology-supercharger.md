# Ideology 神经超充自动使用策略（ID-R06）

2026-11-12 新增：`POST /dlc/ideology/supercharger/policy` 读写
`CompNeuralSupercharger.autoUseMode` 与 `allowGuests`，即原生
`Command_SetNeuralSuperchargerAutoUse`/allow-guests gizmo 的同一字段；
手动超充 `/supercharger/use` 保留。

## 请求

```json
{"thing_id":"NeuralSupercharger75478","auto_use_mode":"AutoUseForEveryone",
 "allow_guests":true,"dry_run":true}
```

- `auto_use_mode`：`NoAutoUse|AutoUseWithDesire|AutoUseForEveryone`，大小写不敏感；
  非法值 400。
- `allow_guests` 是独立开关；至少提供一个字段。
- 要求玩家所属的原生 `CompNeuralSupercharger`，否则 422/404。
- 返回 `SuperchargerRow`（含 `auto_use_mode/allow_guests/charged/power_on`）、
  `before_*`、`requested_*`、`applied`；dry_run 不写。
- 合同：POST `policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成一台玩家 NeuralSupercharger，初始 `AutoUseWithDesire/false`；
dry 保持；设 `NoAutoUse/false` 后 GET 同值；设 `AutoUseForEveryone/true` 后 GET
同值；非法模式 400。
证据 `tools/cua/.runtime/id-r06-20261112/evidence/id-r06.json`，runner
`tests/id_r06_supercharger.py`，fixture `prepare_supercharger`。

## 真实自动使用与客人资格（ID-R06 V，2026-11-13）

旧边界只覆盖字段读写；本轮用真实供电/充能复验原生自主路径。fixture 在玩家地图
生成真实电力网（伐木发电机+导线，燃料 75）供两台玩家 `NeuralSupercharger`，另生成
一台无任何电力连接的隔离超充台；充能仍由原生 `CompRechargeable.CompTick` 在
`CompPowerTrader.PowerOn` 时递减，不强制 charge 字段。

- 隔离超充台 `power_on=false`，600 tick 后 `recharge_ticks` 不变；两台有电超充台
  `power_on=true`，同样窗口内开始递减；到 `charged=true` 用了 59467 tick。
- 主理念（Boccin Creed，13）**不含** `NeuralSupercharge_Preferred`：`AutoUseWithDesire`
  下空闲殖民者 600 tick 不自动使用、超充台仍 charged；改
  `AutoUseForEveryone` 后殖民者 Human321 自动完成原生
  `GetNeuralSupercharge`，脑部 `HediffDefOf.NeuralSupercharge` 出现，
  `lastReceivedNeuralSuperchargeTick` 等于当前 tick，超充台 `charged=false`、
  `recharge_ticks` 重置到 59941。
- 客人资格（原生只对 `Pawn.IsQuestLodger()` 生效）：fixture 用原生
  `QuestPart_ExtraFaction`（HomeFaction 指向现存非玩家派系）把一名玩家殖民者标成
  quest lodger（ideo 0，无 desire precept）。`AutoUseWithDesire` + allowGuests=true
  时不对其自动使用（欲望缺失）；`AutoUseForEveryone` + allowGuests=false 时仍不用
  （客人被拦截）；改成 allowGuests=true 后同一 lodger 自动使用第二台超充台，
  hediff/`last_received`/放电与重置同前（59889）。
- 原生依据：`CompNeuralSupercharger.CanAutoUse`（NoAutoUse false；
  AutoUseWithDesire 要求 `ideo.HasPrecept(NeuralSupercharge_Preferred)`；
  AutoUseForEveryone true；`!allowGuests` 拦截 `IsQuestLodger`），
  `JobGiver_GetNeuralSupercharge`（要求 `Charged && CanAutoUse && !forbidden`），
  `JobDriver_GetNeuralSupercharge`（`Discharge()` + 脑部 hediff）。手动
  `/supercharger/use` 是另一条 float-menu 路径，本轮未改。
- 证据：`tools/cua/.runtime/id-r06-run9-20261113/evidence/id-r06.json`
  （phase=passed）；runner `tests/parallel/ideology/id_r06_supercharger_auto.py`，
  fixture `tests/parallel/ideology/IdR06Fixture.cs`。
- 未测：电池/太阳能/电网掉电/断电、flicked-off/breakdown/EMP、非 quest-lodger 的
  其他访客（原生 `CanAutoUse` 没有其他 guest 分支）、囚犯/奴隶、跨天 30000 tick
  单 pawn 冷却、策略与充能状态的 save-load。
