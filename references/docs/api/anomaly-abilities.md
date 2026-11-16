# Anomaly 代表能力效果（AN-R08）

2026-11-12 代表验收：复用既有通用能力端点
`GET/POST /api/v1/pawn/ability`（`Ability.GetJob`，与原生技能 gizmo 同路径）。
不按每个 AbilityDef 造专用端点；按语义各选一个代表，其余 Def 走同一原生路径。

## 请求与读回

`GET /api/v1/pawn/ability?pawn_id=Human321` 返回该 pawn 实际持有的
`AllAbilitiesForReading` 行：`ability_def/label/level/on_cooldown/
cooldown_ticks_remaining/charges/can_cast/cannot_reason/job_def`。

`POST /api/v1/pawn/ability`

```json
{"pawn_id":"Human321","ability_def":"MetalbloodInjection","dry_run":true}
{"pawn_id":"Human321","ability_def":"VoidTerror","target_pawn_id":"Human75478"}
```

- 自施法省略 target，原生 targeter 以 caster 为目标；`applied/job_submitted`
  以原生 `ability.GetJob(target, destination)` 后 `TryTakeOrderedJob` 为准。
- 目标可选 `target_pawn_id`/`target_thing_id`/`cell`，两阶段能力可加
  `dest_cell`/`dest_pawn_id`；世界目标能力用 `world_map_id`/`world_caravan_id`。
- 原生资格全部保留：`CanApplyOn`、`Verb.targetParams.CanTarget`、
  `Verb.ValidateTarget`、`OnCooldown`、`CanCast`、`CanQueueCast`、确认框、
  充能/冷却。2026-11-12 补：`targetParams.canTargetSelf=false` 时
  `target_pawn_id=caster` 返回 422，而不是依赖 `CanTarget` 对 pawn 目标的宽松判断
  （VoidTerror 实证；原生 Targeter 会排除 caster）。

## 代表语义与证据

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 给一名殖民者授予 `MetalbloodInjection`/`VoidTerror` 并在视线内生成一名
非玩家 Villager（准备披露）。两条语义：

1. **自身 hediff**：`MetalbloodInjection` 自施法 200/`job_submitted:true`；推进后
   caster 获得原生 `Metalblood`，该能力冷却 14388 tick。
2. **指向性强制精神状态**：`VoidTerror` 对 caster 自身 422（原生 targeter 不接受），
   对目标 pawn 200/`job_submitted:true`；推进后目标
   `MentalStateDef=TerrifyingHallucinations`，充能 5→4、冷却 6930 tick。

证据 `tools/cua/.runtime/an-r08-20261112/evidence/an-r08.json`，runner
`tests/an_r08_abilities.py`，fixture `prepare_an08_abilities`/`observe_an08`。

- 这不是全部 Anomaly 能力清单的逐项通过；其它 Def 共享同一原生 GetJob/资格路径。
- 未测：需要尸体/气体的 AoE（ReleaseDeadlifeDust）、建筑/地格目标能力、
  能力确认框文本、灵能/仪式授予来源本身、RNG 战果。

## 2026-11-13 补充：气体/尸体、攻击、地格、隐形与装备充能代表语义（AN-R08 V）

仍走同一 `GET/POST /api/v1/pawn/ability`（`Ability.GetJob`），不新增能力专用端点。
fixture 为一个可操作殖民者授予下列能力、生成一具原生 Villager 尸体与两只眩晕
Boomrat、装备原生 Hellcat 步枪（2 充能）；取得来源为 fixture 准备（已披露），
目标校验、job、效果与消耗为原生。最终复跑
`tools/cua/.runtime/an-final-20261113g/evidence/an-r08-semantics.json`。

1. `ReleaseDeadlifeDust`（无目标，`CompProperties_AbilityReleaseGas`，15 格
   DeadlifeDust）：自身施放 200/`job_submitted:true`；尸体处 gas 密度 73-180；
   原生 `CorpseGasEffectsTickRare`→`MutantUtility.ResurrectAsShambler` 后尸体消耗，
   pawn `mutant=Shambler`、faction `PlayerColony`、存活已生成。
2. `PsychicSlaughter`（血肉目标、BodySize≤2.5）：`target_pawn_id` 200；目标 dead、
   `Corpse` 销毁、`Meat_Twisted` 0→30。
3. `CorrosiveSpray`（`canTargetLocations:true`，9 格酸液）：`cell` 200/
   `target_kind:cell`；原生酸液弹造成 `AcidBurn`（目标死亡）与 9 个
   `Filth_SpentAcid`。
4. `RevenantInvisibility`（自身，`PsychicInvisibility`，15s）：200 后 hediff 出现，
   推进后消失。
5. `HellcatBurner`（装备 `CompProperties_EquippableAbilityReloadable`，2 充能）：
   两次施放 charges 2→1→0，火焰锥产生 `Filth_Ash` 3→23；第三次 409
   （`Ability.CanCast` 无充能）。`IncineratorBurner` 共用同一 FireSpew/Burner
   worker（1 充能，范围/宽度/伤害不同），源码等价，不重复跑。

披露：本 runner 在 fixture 准备前关闭产品 debug 无敌，因为该 hook 会阻止
**所有** `Pawn.Kill`/`TakeDamage`，上述真实死亡/伤害必须在关闭时观察；场地为
11×11 铺装隔离区、目标被原生 stun、尸体为 null-faction；最后一次长推进前恢复无敌，
终态四名殖民者 health 1.0 且无死亡。

- 证据同上目录 `an-r08-semantics.json`，runner
  `tests/parallel/anomaly/an_r08_semantics.py`，fixture 动作
  `prepare_an08`/`stage_an08`/`observe_an08`。
- 未测：各能力的正常取得来源（creepjoiner benefit、Ghoul 植入手术、Revenant 脊椎、
  武器制造/装备）、多目标/多尸体几何、确认框文本、随机战果；装备 Verb/装填归 S-01。
