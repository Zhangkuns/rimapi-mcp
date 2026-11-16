# 角色社交能力的真实结果（ID-R13）

验证 `GET/POST /api/v1/pawn/ability` 在合法道德向导（Moralist）角色与目标下执行
原生社交能力，读回真实结果与组冷却。生产代码无改动；测试见
`tests/parallel/ideology/id_r13_social_abilities.py` 与 `IdR13Fixture.cs`。

## 原生依据

- 角色 `IdeoRole_Moralist`（`Precept_RoleSingle`）授予 `Reassure`、`Counsel`、
  `PreachHealth`；三者 `groupDef=Moralist`，组冷却 `cooldownTicks=180000`。
- `Reassure`：`CompAbilityEffect_Reassure` 对同理念、无精神状态的 target 调用
  `ideo.Reassure(baseCertaintyGain(0.2) * NegotiationAbility)`。
- `Counsel`：`CompAbilityEffect_Counsel` 按 Social 技能与意见值掷骰；成功时把
  最差记忆（`MoodOffset <= -10`）替换为 `Counselled`（抵消其心情），无负面记忆时
  加 `Counselled_MoodBoost`；失败时加 `CounselFailed`。
- `PreachHealth`：`CompAbilityEffect_GiveHediff` 给 target 加 `PreachHealth`
  hediff（Brain，severity 0.5），阶段提供 `InjuryHealingFactor +0.25` 与
  `ImmunityGainSpeed +0.25`（后者为因子，实际增量 = 当前值 × 0.25）。
- 服务端 `POST /pawn/ability` 已复用原生 targeting/ValidateTarget/CanCast/
  OnCooldown，成功后 `TryTakeOrderedJob`，返回 `job_submitted` 与充能读回。

## 请求/响应（现状）

`GET /pawn/ability?pawn_id=318` → 200，`data` 为数组，每项：
`ability_def`、`label`、`level`、`on_cooldown`、`cooldown_ticks_remaining`、
`charges`、`can_cast`、`cannot_reason`、`job_def`。角色/hediff/装备授予的能力也在
`AllAbilitiesForReading` 中返回。

`POST /pawn/ability` body：
`pawn_id`、`ability_def`，目标三选一 `target_pawn_id`/`target_thing_id`/`cell`
（自施可省略），可选 `dest_cell`/`dest_pawn_id`/`world_map_id`/
`world_caravan_id`/`confirm`/`dry_run`。
`dry_run=true` 只返回原生开火计划；真实调用返回 `job_submitted`。

错误：缺参 400；找不到 pawn/ability 404；无该能力 404；原生 targeting/
ValidateTarget 拒绝 → 409 `invalid_target`；`on_cooldown` → 409
`Conflict`（`ability is on cooldown for N ticks`）；无 cast job → 501/409。

## 实测（g35 副本，tick 76787，五 DLC，ID-R13 run8）

- 通过 `POST /api/v1/dlc/ideology/role/assign` 给 Human318 指派 Cleric 后，
  GET 读回 Reassure/Counsel/PreachHealth 三个能力且未冷却。
- Reassure 对 Human321：施放成功，certainty `0.447569 → 0.7377491`
  （= 0.2 × NegotiationAbility 1.4509）。
- Counsel 对 Human315（预置 `AteRottenFood -10`）：成功，新增
  `Counselled mood_offset=+10`（原 -10 被抵消），无 `CounselFailed`。
- PreachHealth 对 Human315（预置 Cut 6）：新增 `PreachHealth` severity 0.5
  （Brain）；`InjuryHealingFactor 1.0 → 1.25`；`ImmunityGainSpeed 0.7 → 0.875`。
- 组冷却：Reassure 后 GET 三个能力均 `on_cooldown=true`、
  `cooldown_ticks_remaining≈180000`；对 Counsel 的 POST 返回 409。
  测试 fixture 在两次施放之间清除组冷却（仅测试脚手架）以在一次会话读全三种效果；
  冷却本身已按上句读回并被 409 拦截。

## 限制

- Counsel 为原生 RNG，本次命中成功分支；失败分支（`CounselFailed`）未在本次证据
  出现，但 runner 接受真实分支并记录推断概率。
- 冷却到期（180000 tick 后）未实测；其他社交/角色能力 Def 未逐一重跑；未测精神
  状态、异理念、被囚/访客目标、warmup 中断与关系增益。
- fixture 为注入 Cut 伤临时关闭过一次 debug invulnerability（注入后立即恢复）；
  属测试脚手架，不是正常玩法路径。
