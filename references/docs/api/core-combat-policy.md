# Core 战斗策略与开火许可（C-15）

2026-11-13 新增：

- `GET /api/v1/pawn/combat-policy?pawn_id=`
- `POST /api/v1/pawn/combat-policy`

只覆盖已征召/可征召玩家的原生 `Pawn_DraftController`、
`Pawn_PlayerSettings.hostilityResponse` 和玩家机械体的 `CompTurretGun` 策略，以及
当前原生攻击 job 的停止/只读队列。**不提供任意目标攻击下单、任意 job 队列编辑或
通用战斗执行器**；实际目标由原生 `JobGiver_ConfigurableHostilityResponse` 选择。

## 读取

```json
GET /api/v1/pawn/combat-policy?pawn_id=321
{
  "pawn_id": "Human321",
  "spawned": true,
  "drafted": true,
  "fire_at_will": true,
  "fire_at_will_source": "draft_controller",
  "fire_at_will_applicable": true,
  "hostility_response": "Attack",
  "hostility_applies": true,
  "hostility_attack_blocked": false,
  "can_draft": true,
  "can_draft_reason": null,
  "current_job": {"def_name": "AttackStatic", "target_a": {"thing_id": 75479,
    "thing_id_string": "Muffalo75479", "def_name": "Muffalo"},
    "target_b": null, "player_forced": false},
  "queued_jobs": [],
  "current_target": null,
  "last_attack_target": null,
  "is_attacking": true,
  "blocked_reasons": ["attacking"]
}
```

- `drafted`：`Pawn_DraftController.Drafted`；无 drafter 为 null。
- `fire_at_will`：human/普通玩家人物取 `Pawn_DraftController.FireAtWill`；
  `IsColonyMechPlayerControlled` 的机械体取 `CompTurretGun` 的私有 `fireAtWill`
  （源码耦合，见下）。`fire_at_will_source` 为 `draft_controller`/`turret`/null。
- `hostility_response`：`Pawn_PlayerSettings.hostilityResponse` 的
  `Ignore`/`Attack`/`Flee`；`hostility_applies` 取原生
  `UsesConfigurableHostilityResponse`。
- `hostility_attack_blocked`：原生 `WorkTagIsDisabled(WorkTags.Violent)`；
  `Attack` 被禁时不写。
- `can_draft`/`can_draft_reason`：drafter、Downed、Deathresting、机械体
  `MechanitorUtility.CanDraftMech` 原生结果。
- `current_job`/`queued_jobs`：只读原生 `Pawn_JobTracker.curJob`/
  `jobQueue`（def/targetA/B/playerForced）。
- `current_target`/`last_attack_target`：仅 `CompTurretGun` 的原生私有字段
  `currentTarget`/`lastAttackedTarget`；非炮塔机械体为 null。
- `blocked_reasons`：稳定字符串（`downed`/`deathresting`/`mental_state`/
  `draft:<reason>`/`violent_disabled`/`no_configurable_hostility`/
  `no_fire_at_will_control`/`mech_not_player_controlled`/`attacking`）。
- 要求 pawn 已生成；未生成返回 409。

## 写入

```json
POST /api/v1/pawn/combat-policy
{"pawn_id":321,"drafted":true,"fire_at_will":false,
 "hostility_response":"Attack","stop":false,"dry_run":false}
```

- 至少给 `drafted`、`fire_at_will`、`hostility_response`、`stop` 之一。
- 先验证全部请求字段，再统一应用；任一非法返回 4xx，不部分改 policy。
- `drafted`：原生 `pawn.drafter.Drafted`；Downed/Deathresting/机械体
  `CanDraftMech` 拒绝时 409。
- human `fire_at_will`：原生 `Pawn_DraftController.FireAtWill`；未
  `drafted:true`（或 current 未 drafted 且本次未同时设为 true）返回 409。
- 玩家机械体 `fire_at_will`：仅 `IsColonyMechPlayerControlled`；用
  `CompTurretGun` 私有 `fireAtWill` 字段（该 comp 无公开 setter，源码耦合）。
  非玩家控制机械体 409。
- `hostility_response`：仅 `UsesConfigurableHostilityResponse` 的人物；violent
  WorkTag 被禁时 `Attack` 409；其它模式按原生下拉允许。
- `stop`：仅当前 `curJob.def` 为 `AttackMelee`/`AttackStatic`，或
  `CompTurretGun.currentTarget` 有效；调用原生
  `EndCurrentJob(JobCondition.InterruptForced)`，并把队列中**仅**这两类攻击 job
  `RemoveAll`；无攻击 target 返回 409。不会 broad-stop 非攻击工作，不编辑其它
  队列 job。若同时 `hostility_response:Attack`，原生 JobGiver 可能在 stop 后立即
  重新取得目标；测试用同请求 `Ignore` 验证无重取，stop 本身不承诺抑制原生策略。
- 响应 `applied`、`before/after`、`stopped_job_def`、
  `turret_target_cleared`、`cleared_queued_attacks`；`dry_run:true` 只验/投影，
  不写。
- 合同：GET observe；POST command/pawn_ai/validate/game_memory。新共享路由行由
  协调者登记。

## 源码耦合与边界

`CompTurretGun` 的 `fireAtWill`、`currentTarget`、`lastAttackedTarget` 在 1.6
没有公开属性/setter；本实现用 `AccessTools.Field` 访问这三个字段，并在成功/失败
响应中只读暴露。`fireAtWill` 的写入仅限 `IsColonyMechPlayerControlled` 的玩家
机械体；不反射其它战斗状态、不修改 `Verb`/`Stance`/伤害。

## 证据（2026-11-13）

隔离 g35 副本 tick 76787，五 DLC；fixture `tests/parallel/core-combat/C15Fixture.cs`
准备一名持原生远程武器的殖民者、一名原生 Pacifist 殖民者、一只原生 manhunter
Muffalo、一台玩家控制 Warqueen 和一台未控制 Warqueen；fixture 从不写
drafted/fire_at_will/hostility_response。

- policy GET：undrafted、fire_at_will/applicable、can_draft、queue 只读。
- 未 drafted 写 fire_at_will → 409；drafted true；fire_at_will false→true。
- hostility `Attack` 后，未征召人物由原生 `JobGiver_ConfigurableHostilityResponse`
  实际产生 `AttackStatic`，目标为 `Muffalo75479`；伤害窗口临时关闭 invulnerability
  后目标掉血（披露）。
- 通过既有 `/pawn/job` 放入一个原生 `AttackMelee` 当前 job 和一个
  `AttackStatic` 队列 job；`GET` 读回队列；`stop:true` 只结束当前攻击并清队列中的
  攻击 job，非攻击队列不动。
- pacifist `Attack` → 409；玩家 Warqueen `fire_at_will` turret 字段 false/true；
  未控制 Warqueen `fire_at_will` → 409。
- 证据：`tools/cua/.runtime/c15-run15-20261113/evidence/c15.json`（phase passed）；
  未测长时间巡逻/防御、敌人死亡/离图竞态、多 pawn 队列、AI core/任务战斗、1.5。
