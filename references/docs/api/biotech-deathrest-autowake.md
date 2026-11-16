# Biotech 死亡休眠自动唤醒（BT-R06）

2026-11-12 新增：读取/写入 `Gene_Deathrest.autoWake`，即死亡休眠 gizmo
`AutoWake` 的同一个公开字段。已有 `/dlc/biotech/life` 读回
`deathrest_auto_wake`；本轮补写端点。

## 读取

`GET /api/v1/dlc/biotech/life?pawn_id=Human66736` 的
`deathrest_auto_wake`（以及 `deathresting`/`deathrest_ticks`/`min_ticks`/
`progress`）。

## 设置

`POST /api/v1/dlc/biotech/life/deathrest/auto-wake`

```json
{"pawn_id":"Human66736","enabled":true,"dry_run":true}
```

- 要求 pawn 持有原生 `Gene_Deathrest`，否则 422。
- 写 `gene.autoWake`，返回 `before_auto_wake/requested_auto_wake/auto_wake/
  deathresting/ticks/min_ticks/progress/applied`；dry_run 不写。
- 合同：POST `policy/game_ai/validate/game_memory`；已有 `wake` 手动唤醒保留。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选择一名已在死亡休眠的 `Deathrest` 基因殖民者（无新增基因），
`autoWake=false`；GET 读回 false；dry 保持 false；正式 on 后 gene/GET 均为 true；
再 off 后 false。`deathresting:true` 全程保持。
证据 `tools/cua/.runtime/bt-r06-20261112/evidence/bt-r06.json`，runner
`tests/bt_r06_autowake.py`，fixture `prepare_auto_wake`/`observe_auto_wake`。

## 完整周期后的原生安全自动唤醒（BT-R06，2026-11-13 补验）

run `bt-r06-20261113-run2`（隔离 g35 副本，tick 76787 起，port 8765）。fixture 选择
存档中已在死亡休眠的 `Deathrest` 基因殖民者 Human66736，把原生 `deathrestTicks` 置为
`MinDeathrestTicks-800`（240000→239200）并把 `autoWake` 置 false；其余周期、阈值判断
与 `Gene_Deathrest.Wake()` 均为原生。

- autoWake=false 推进到 239905（progress 0.9996）仍 `deathresting:true`，未提前唤醒。
- 现有 `auto-wake` 端点 dry（applied=false）后正式 on：`auto_wake:true`，
  `deathresting:true`。
- 再推进跨过 240000：tick 77584 原生 `TickDeathresting` 在 `DeathrestPercent>=1` 且
  autoWake=true 时调用 `Wake()`；终态 `deathresting:false`、`deathrestTicks=0`、
  progress 0、`autoWake:true`、`notifiedWakeOK:true`，`Deathrest` hediff 移除且没有新增
  `InterruptedDeathrest`（存档原有的一条保留）；GET `/dlc/biotech/life` 读回
  `deathresting:false/deathrest_auto_wake:true/deathrest_ticks:0`。全程没有调用手动
  `/deathrest/wake`（http 记录核对）。
- 证据 `tools/cua/.runtime/bt-r06-20261113-run2/evidence/bt-r06.json`（`phase=passed`）；
  runner `tests/parallel/biotech/bt_r06_autowake_cycle.py`，fixture 命令
  `prepare_deathrest_autowake/observe_deathrest_autowake`。

披露/未测：fixture 把原生 `deathrestTicks` 推进到阈值前 800 tick 以缩短 4 游戏日周期；
未测从死亡休眠开始到完整周期的全过程、`InterruptedDeathrest` 中断周期、手动 wake、
无基因/囚犯拒绝、长期副作用与存档读回。
