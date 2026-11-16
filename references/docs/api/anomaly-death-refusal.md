# Anomaly 死亡拒绝自体复活（AN-R04）

2026-11-12 新增：读取死者身上的原生 `Hediff_DeathRefusal`，并使用与死者 gizmo
相同的 `CommandSelfResurrect` 命令。正常游玩中该 hediff 来自 Imbue Death Refusal
灵能仪式；本接口不制造 hediff，只调用原生复活命令。

## 读取

`GET /api/v1/dlc/anomaly/death-refusal?pawn_id=Human66736`

```json
{"pawn_id":"Human66736","dead":true,"spawned":false,"map_id":0,
 "parent_holder":"Corpse","uses_left":1,"max_uses":2,"in_progress":false,
 "player_controlled":true,"ai_enabled":true,"has_sickness":false,
 "corpse_spawned":true}
```

- `dead/parent_holder/corpse_spawned` 来自 pawn 与 `ParentHolder`；
- `uses_left/max_uses/in_progress/player_controlled/ai_enabled` 来自
  `Hediff_DeathRefusal`（`PostAdd` 原生 1 次，`MaxUses` 2）；
- `has_sickness` 是 `HediffDefOf.DeathRefusalSickness`，复活完成后由原生
  `Resurrect()` 添加；
- 无该 hediff 时 422，未知 pawn 404。

## 使用

`POST /api/v1/dlc/anomaly/death-refusal/resurrect` `{pawn_id, dry_run}`

- 原生资格与死者 gizmo 一致：`pawn.Dead && PlayerControlled && !InProgress &&
  UsesLeft>0`；不满足返回 409/422。
- 实际调用私有 `Hediff_DeathRefusal.Use()`（固定 `AccessTools` 调用，不含任意反射入口）：
  次数 1→0、启动 3 秒原生复活计时器并播放效果；`applied` 要求 `InProgress` 且次数减少。
- `dry_run` 只返回当前/计划状态，不调用 `Use()`。
- 3 秒计时结束后原生 `Resurrect()`：`dead=false`、复活人物倒地、
  `DeathRefusalSickness` 添加；次数为 0 时 hediff 自身被清除。再次提交返回 422
  "no native Hediff_DeathRefusal"。
- 合同：`GET` observe；`POST` direct/immediate/validate，`game_memory` 幂等。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，独立 Xvfb :101，
1920×1080 全屏允许）：

- fixture 给一名已登记殖民者添加 `Hediff_DeathRefusal` 并杀死该 pawn（准备已披露），
  尸体 `Corpse_Human75478` 已生成。
- dry 200/`applied:false`，次数仍 1；正式 200/`applied:true`、`uses_left:1→0`、
  `in_progress:true`；推进后 `dead:false`/`spawned:true`/`has_sickness:true`，
  再次提交 422。
- 证据 `tools/cua/.runtime/an-r04-r09-20261112/evidence/an-r04-r09.json`（运行产物），
  runner `tests/an_r04_r09_actions.py`，fixture `prepare_death_refusal`/
  `observe_death_refusal`。
- 未测：尸体在坟墓/容器中、尸体已销毁、2 次上限的使用链、灵能仪式授予路径本身、
  复活后的长期伤势/疤痕随机结果；不保证随机疤痕。
