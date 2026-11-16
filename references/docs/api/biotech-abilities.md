# Biotech 玩家能力（BT-13a）

`GET /api/v1/pawn/ability?pawn_id=321` 列出原生能力及 `can_cast`、
冷却、充能；`POST /api/v1/pawn/ability` 通过原生 `Ability.GetJob` 施放。

```json
{"pawn_id":"321","ability_def":"FireSpew","cell":{"x":182,"y":0,"z":144},"dry_run":true}
```

- 目标三选一：`target_pawn_id`、`target_thing_id`、`cell`；两段式能力再给
  `dest_cell`。`dry_run` 只投影，不扣资源/不产生冷却。
- 若原生 `Ability.ConfirmationDialog` 返回警告（例如 donor 正在复制时的
  ReimplantXenogerm 致死警告），未带 `confirm:true` 返回 409
  `confirmation_required` 和原生文本；确认后继续。
- 端点执行 `CanApplyOn`、`verb.ValidateTarget`、冷却/`CanCast`/`CanQueueCast`；
  能力效果、hemogen/psyfocus/冷却和产物由原生 comp 产生。
- FireSpew 实测：Human321 目标格 (182,144) → `job_submitted`，tick 76848
  产生 1 个 `Filth_FlammableBile`、cooldown 299999；同目标再次 409，越界 422
  （`Reverse/evidence/bt13a-firespew-20261111.json`，自审
  [BT-13a](../references/reviews/2026-11-10-bt13a-firespew.md)）。

该早期样本只测FireSpew；后续Longjump/Coagulate的位移/止血与hemogen消费、ReimplantXenogerm非致死与致死确认均已补证，见下文。其它资源不足/冷却恢复/目标变体未验。

## Longjump

```json
{"pawn_id":"321","ability_def":"Longjump","cell":{"x":172,"y":0,"z":155}}
```

- cell 目标，原生 `CastJump` 移动；`CompAbilityEffect_HemogenCost` 扣 0.05
  hemogen，冷却由原生能力计时。
- 实测 Human321 从 (165,155) 跳到 (172,155)，hemogen 1.0→0.94998，cooldown 24；
  二次 409、远格 422（`Reverse/evidence/bt13a-longjump-20261111.json`，自审
  [Longjump](../references/reviews/2026-11-10-bt13a-longjump.md)）。
- 边界：只测一次/一个落点；两段式 dest、落点占据/无 LOS、hemogen 不足、
  冷却恢复未验。

## Coagulate

```json
{"pawn_id":"321","ability_def":"Coagulate","target_pawn_id":"315"}
```

- 目标必须是可 tend 的伤口（`ValidateHasTendableWound`）；提交原生
  `CastAbilityOnThingMelee`，由 `CompAbilityEffect_Coagulate` 以 0.4~0.8 质量
  tend 所有可 tend 伤口，并扣 0.2 hemogen。
- 实测 Human321 对 Human315 的 Cut（severity 8）：tick 76847 tended、quality
  0.61245、hemogen 1.0→0.79998；二次 422（无 tendable wound）。Coagulate 无
  冷却（`cooldown_ticks_total:0`）（`Reverse/evidence/bt13a-coagulate-20261111.json`，
  自审 [Coagulate](../references/reviews/2026-11-10-bt13a-coagulate.md)）。
- 2026-11-10 曾因 debug invulnerability 拦截 `AddHediff(Hediff_Injury)` 而
  dry 422；fixture 只在加伤窗口关闭无敌，产品校验未绕过。
- `UseAbility` 的 422 现在附带 verb/range/reach/effect 子条件，便于定位。
- 边界：只测一个伤口/一次；hemogen 不足、动物/血裔目标、施法取消未验。

ReimplantXenogerm后续证据见下文；其它Def不由这些代表样本穷尽。

## ReimplantXenogerm

```json
{"pawn_id":"321","ability_def":"ReimplantXenogerm","target_pawn_id":"315"}
```

- donor 必须有 xenogenes、target 不同 xenotype 且可接受；原生
  `GeneUtility.ReimplantXenogerm` 把 donor xenogenes 复制给 target，donor 得到
  `XenogermReplicating`/`XenogermLossShock`，target 得到 `XenogerminationComa`/
  `XenogermReplicating`。
- 实测 Human321(Sanguophage)→Human315(Baseliner)：tick 77027 target 获得 31 个
  donor xenogenes（含 FireSpew）、target coma、donor/target 均存活；二次 422
  （同 xenotype）（`Reverse/evidence/bt13a-reimplant-20261111.json`，自审
  [Reimplant](../references/reviews/2026-11-10-bt13a-reimplant.md)）。
- **致死确认已实测**：donor 已 `XenogermReplicating` 时，未确认 POST 409
  返回 “genes are currently regrowing … it will kill him”；`confirm:true`
  放行原生 Reimplant，donor 死亡、target 仍获基因并存活
  （`Reverse/evidence/bt13a-reimplant-lethal-20261111.json`，自审
  [致死确认](../references/reviews/2026-11-10-bt13a-reimplant-lethal.md)）。
- 目标敌对/downed/ideology 拒绝、quest lodger 仍未验。
