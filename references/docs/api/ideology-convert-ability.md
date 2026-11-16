# Ideology 教化能力（G-47）

交付：读取 Pawn 的原生 `Convert` 能力，以及把该能力对准确目标排队执行（正常 certainty
降低/改信）。这是**正常教化**，区别于 `POST /dlc/ideology` 的直接 `SetIdeo`。
证据：G47 自由殖民者（`Reverse/evidence/dlc-G47-convert-ability.json`）；G77 殖民地囚犯（`Reverse/evidence/dlc-G77-convert-prisoner.json`）；G78 奴隶（`Reverse/evidence/dlc-G78-convert-slave.json`）；G79 失败分支
（`Reverse/evidence/dlc-G79-convert-failure.json`）。

囚犯/奴隶的**互动模式**（`Convert` + `ideoForConversion`，由狱卒 WorkGiver 执行）见
[ideology-prisoner-convert.md](ideology-prisoner-convert.md)；本页只覆盖 Convert ability。

## 读取

`GET /api/v1/dlc/ideology/convert/ability?pawn_id=321`

```json
{"pawn_id":"Human321","ability_count":1,
 "abilities":[{"def_name":"Convert","label":"convert","cooldown_ticks_remaining":0,
   "on_cooldown":false,"can_cast":true,"reason":null,"can_queue":true,
   "job_def":"CastAbilityOnThingMelee","ideo":"Boccin Creed","certainty":0.63}]}
```

- 只返回 `AbilityDef.comps` 含 `CompProperties_AbilityConvert` 的能力。
- `can_cast` 是原生 `Ability.CanCast`，`cooldown_ticks_remaining` 是原生值；本存档的
  `Moralist` AbilityGroup 没有冷却（0），重复尝试由 3 秒 warmup job 控制。

## 施放

`POST /api/v1/dlc/ideology/convert/ability`

```json
{"caster_id":"Human321","target_id":"Human66734","dry_run":true}
```

- 原生路径是 `Ability.GetJob(target)` → `JobDefOf` 能力 job（`CastAbilityOnThingMelee`）
  → warmup → `CompAbilityEffect_Convert.Apply` → `IdeoConversionAttempt`。本端点排队同一
  job；`applied:true` 只表示已提交，certainty/改信在 warmup 结束后才发生。
- dry-run 计算 `base_certainty_reduction`（`InteractionWorker_ConvertIdeoAttempt.CertaintyReduction`）、
  `certainty_reduction = base * CompProperties_AbilityConvert.convertPowerFactor`、
  `projected_certainty`（原生 `ApplyCertaintyChangeFactor` 只读反射）和 `would_convert`
  （projected ≤ 0），不创建 job。
- 资格：caster 为玩家、spawned、非倒地/精神、有 ideo；target 为 spawned、living、
  humanlike、有 ideo；`Ability.OnCooldown`/`CanCast`/`CanQueueCast`/`CanApplyOn`/verb
  `CanHitTarget` 不满足分别 409/422；caster 无 Convert 422。
- 实测：target 原 ideo `Nova-Archotechnics`、certainty 0.01；dry-run reduction 0.0192、
  `would_convert:true`；正式排队后原生 warmup 完成，target 改信 `Boccin Creed`、certainty
  重置 0.5。fixture 只授予 Convert 能力并设低 certainty（测试准备），不写能力效果。

G77：把 target 设为殖民地囚犯后重跑同一 API：dry-run/正式 200，`convert_done` 后 target
改信 caster 的 `Boccin Creed`、certainty 约 0.5，`target_is_prisoner` 仍 true。见
[G77 自审](../references/reviews/2026-11-08-g77-convert-prisoner.md)。
G78：target 设为奴隶（`GuestStatus.Slave`）后同样通过，改信后 `target_is_slave` 仍 true；
见 [G78 自审](../references/reviews/2026-11-08-g78-convert-slave.md)。
G79：target certainty 0.9 时一次尝试失败：dry-run `would_convert:false`，正式后 target
ideo 不变、certainty 下降且大于 0、出现 `FailedConvertAbilityRecipient` 心情；见
[G79 自审](../references/reviews/2026-11-08-g79-convert-failure.md)。

## 限制

- 失败分支已验一次（G79）；连续多次、失败后再成功、囚犯/奴隶失败未验；
- 殖民地囚犯/奴隶已验证；动物、无 ideo 目标、囚犯互动模式 Convert、越狱/精神、warmup 中断、
  warmup 中断未验；
- 本存档 Convert 无冷却，其它 AbilityGroup/角色授予的冷却未验；
- 未做 Cua 对照。
