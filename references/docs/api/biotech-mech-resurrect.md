# Biotech 机械复活（G-53）

交付：读取原生 `ResurrectionMech` 能力/充能，以及对玩家机械尸体排队该能力并实际复活。
证据：`Reverse/evidence/dlc-G53-mech-resurrect.json`、`dlc-G53-run.json`。

## 读取

`GET /api/v1/dlc/biotech/mech/resurrect/ability?pawn_id=321`

```json
{"pawn_id":"Human321","has_ability":true,"ability_def":"ResurrectionMech",
 "charges_remaining":30,"can_cast":true,"on_cooldown":false,
 "cooldown_ticks_remaining":0,"job_def":"CastAbilityGoToThing"}
```

- 来自 `CompAbilityEffect_ResurrectMech`（`ChargesRemaining`/`CanCast`）。

## 施放

`POST /api/v1/dlc/biotech/mech/resurrect`

```json
{"caster_id":"321","corpse_id":"Corpse_Mech_Scyther...","dry_run":true}
```

- 原生路径是 `Ability.GetJob(corpse, castCell)` → `CastAbilityGoToThing` → warmup →
  `CompAbilityEffect_ResurrectMech.Apply` → `ResurrectionUtility.TryResurrect`。
- **关键**：`CastAbilityGoToThing` 的 target B 是前往格；传 `LocalTargetInfo.Invalid`
  会让 `Toils_Goto` 在提交后抛 `NullReferenceException`。本端点取尸体格（不可站立时取相邻
  可站立格）作为 destination。
- 资格：caster 玩家、spawned、非倒地/精神、有该能力；corpse 为玩家机械尸体、
  原生 `CanApplyOn`（重量级可复活、尸体年龄、充能、同派系）通过；verb 可达。
- dry-run 投影 `charges_before/job_def`；实际复活读回 corpse/mech/charges。

## 实测

`ai_g31_fixture` 副本（tick 74742）：fixture 生成并击杀一名玩家 `Mech_Scyther`（相邻
caster）、授予 `MechlinkImplant` 与 `ResurrectionMech`；保护开启。

- GET has_ability/charges 30/can_cast。
- dry-run 200/applied false。
- 正式 200/applied true；warmup 后 `mech_resurrected`：mech spawned/alive、health>0、
  charges 30→20（Scyther 消耗 10）；以非尸体 ID 再次提交返回 404。
- 证据：`Reverse/evidence/dlc-G53-mech-resurrect.json`。

## 限制

- 未测正常血清物品授予流程、其它重量级/尸体年龄边界、复活后的伤口分布；
- 未测机械建造（MechGestator）；
- 未做 Cua 对照。
