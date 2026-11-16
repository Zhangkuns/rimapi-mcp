# 玩家能力施放（原生 Ability）

`GET /api/v1/pawn/ability?pawn_id=` 列出该人物实际持有的原生能力；
`POST /api/v1/pawn/ability` 在主线程按原生路径施放：本地目标用 `Ability.GetJob`，
地图起点的世界目标用 `QueueCastingJob`，商队起点按其原生立即分支调用 `Activate(GlobalTargetInfo)`。
原生能力负责效果/资源；服务不替未持有的能力造入口。

核对：2026-11-11；当前运行范围与剩余见[修复自审](../references/reviews/2026-11-11-dsh-progress-repair.md)。

## 请求

```json
{"pawn_id":"75478","ability_def":"GhoulFrenzy","target_pawn_id":null,
 "target_thing_id":null,"cell":null,"dest_cell":null,"dry_run":true}
```

- 目标最多一个：`target_pawn_id` / `target_thing_id` / `cell`；都不给表示自我施放
  （与原生能力 gizmo 一致）。
- `dest_cell` 仅用于两段式能力（`CompAbilityEffect_WithDest`，如 Skip）；
  `dest_pawn_id` 用于需要第二个人物目标的两段式能力（如 WordOfLove），与
  `dest_cell` 二选一。
- 能力实例从原生 `AllAbilitiesForReading` 解析（`GetAbility(def, true)`）：角色 precept、
  hediff、装备和 royalty 授予的 Ability 与 `GET` 列表一致，不只查 base tracker。
- 资格：`CanApplyOn`、原生 `verb.ValidateTarget(target, false)`（与 gizmo 同门：
  远程 verb 查射程/视线，触击 verb 查效果 `Valid`，如 `PreachHealth` 的同 ideo 与
  生病/受伤）、`verb.targetParams.CanTarget`（原生 targeter 的目标种类过滤）、
  两段式目的地、`OnCooldown`、`CanCast`、`CanQueueCast`；非 psychic 能力不检查 psyfocus。
  `Ability.CanApplyOn` 读取惰性构建的 effect comp 列表，服务先访问 `EffectComps`
  以复现 gizmo 的初始化顺序；否则 ShapeFlesh/TransmuteSteel 等 comp 门禁会被跳过。

## 行为

- 本地目标成功返回 `ability_def`、`target`/`target_kind`、`destination`、`job_def`、`job_submitted`、
  `charges_before/after`、`cooldown_ticks_total`。
- dry-run 只做校验与计划，不调用 `GetJob`（不分配 job、不改能力状态）。
- 精确合同：`POST` 为 `policy/pawn_ai/validate/game_memory`；`GET` 为只读 observe。
- 不覆盖未经 `Ability` 实例的被动/基因效果；敌对 AI 专属能力不因此自动成为玩家入口。

## 角色能力（ID-08a）

`GET /pawn/ability` 会列出角色 precept 授予的能力；`POST` 用同一实例提交原生 job。
2026-11-10 运行验证的两条不同语义：

| 角色 | 能力 | 目标/效果 | 读回 |
| --- | --- | --- | --- |
| Leader | `CombatCommand` | 自我 aura，`CastAbilityOnThing` | `CombatCommand` hediff；Leader 组冷却 600000 tick，`WorkDrive` 同步冷却 |
| Moralist | `PreachHealth` | 同 ideo、受伤殖民者，`CastAbilityOnThingMelee` | `PreachHealth` hediff；Moralist 组冷却 ~180000 tick，`Reassure` 同步冷却 |

- 健康/无伤病目标由原生 `CompAbilityEffect_PreachHealth.Valid` 拒绝（422），
  不是 API 另写的健康检查。
- 冷却中再次施放 409；未持有 404。角色仍需先通过 `POST /dlc/ideology/role/assign`
  或正常游戏指派，能力列表才有对应条目。
- `WorkDrive`/`Reassure`/`Counsel`/`Convert` 复用同一 `Ability.GetJob` 路径，未各跑一次；
  `LeaderSpeech`/`Trial` 属仪式/工具能力，不在普通施放范围。角色/冷却/目标拒绝不做
  “任意能力”承诺。

## Psytrainer 物品（RY-06a）

`Psytrainer_<AbilityDef>` 通过普通 `POST /pawn/use-item` 使用：

```json
{"pawn_id":"315","thing_id":"Psytrainer_Burden75478"}
```

- 原生 `CompUsable`/`JobDefOf.UseNeurotrainer` 走完 job 后
  `CompUseEffect_GainAbility` 授予准确能力并消耗物品；服务不直接写能力。
- 实测 Human315 使用 `Psytrainer_Burden`：tick 76887 物品消失、获得 Burden；
  draft 后对相邻 Human75479 施放，tick 77209 目标获 `PsychicBurden`，
  psyfocus 0.7494→0.7394、entropy 0→7.97
  （`Reverse/evidence/ry06a-psytrainer-20261111.json`，自审
  [RY-06a](../references/reviews/2026-11-10-ry06a-psytrainer.md)）。
- 已学拒绝、等级不足409确认门与确认后学习已补证，见
  [训练器确认](../references/reviews/2026-11-10-ry06a-psytrainer.md)。
  技能Neurotrainer、叠放与持久化未验；同worker物品不逐个重测。

## 尸体/物品目标（AN-07a）

2026-11-11 以两个不同目标种类核 Anomaly 玩家能力：

| 能力 | 原生目标 | API 结果 |
| --- | --- | --- |
| `ShapeFlesh` | `onlyTargetCorpses` + `CompAbilityEffect_FleshbeastFromCorpse` | 对 Steel 物品 422（targetParams）；对准确 Villager 尸体提交 `CastAbilityOnThing`，尸体销毁并生成敌意 Trispike；冷却 2400 |
| `TransmuteSteel` | 物品（Steel/ChunkSlagSteel）+ `CompAbilityEffect_Transmute.elementRatios` | 对 Silver 422（effect comp）；对 50 Steel 提交 `CastAbilityOnThingMelee`，原物销毁并同格生成 Gold×50；冷却 50000 |

- 422 拒绝返回 `ability_def/target_kind/target`，ShapeFlesh 另附 targetParams 读回；
  拒绝不销毁目标、不生成产物。
- 冷却是原生 `Ability.OnCooldown`；API 不写冷却或产物。
- 证据 `Reverse/evidence/an07a-ability-semantics-20261111.json`（由 runtime 证据导出，见自审
  [AN-07a](../references/reviews/2026-11-11-an07a-ability-semantics.md)）。

## 双人物目标（RY-05d）

```json
{"pawn_id":"321","ability_def":"WordOfLove",
 "target_pawn_id":"315","dest_pawn_id":"318"}
```

- `target_pawn_id` 是 listener（获得 `PsychicLove` 的人），`dest_pawn_id` 是
  love object；必须是同图 spawned/未死人物，`dest_cell` 与 `dest_pawn_id`
  不能同时给。
- 服务构造 `LocalTargetInfo(destPawn)`，在 `CompAbilityEffect_WithDest` 校验
  `CanHitTarget` 后走原生 `Ability.GetJob`；不写 hediff/psyfocus。
- 实测 Human321 对 Human315→Human318：tick 77014 listener 获
  `PsychicLove`，`love_target_id:Human318`，psyfocus 1.0→0.4998；同
  target=dest 422（`Reverse/evidence/ry05d-word-of-love-20261111.json`，自审
  [RY-05d](../references/reviews/2026-11-10-ry05d-word-of-love.md)）。
- 边界：只测这一对；性别/asexual/未成年/mental state/已有 PsychicLove 替换/
  关系后果未验；其它需要二次选择的能力未验。

## 范围灵能（RY-05b）

`BlindingPulse` 等 `canTargetLocations` 能力用 `cell` 目标：

```json
{"pawn_id":"321","ability_def":"BlindingPulse","cell":{"x":167,"y":0,"z":140}}
```

- 原生 `Ability_EffectRadius`/`CompAbilityEffect_GiveHediff` 结算范围内所有有效目标；
  服务不枚举/写范围 hediff，也不随机选择目标。
- 实测 Human321（psylink 6）对 (167,140) 施放，范围内 3 个 Villager 均获得
  `PsychicBlindness`，psyfocus 1.0→0.99、neural heat 0→20；无死亡
  （`Reverse/evidence/ry05b-area-psycast-20261111.json`，自审
  [RY-05b](../references/reviews/2026-11-10-ry05b-area-psycast.md)）。
- 后续已补VertigoPulse实际范围效果（`ry05b-vertigo-20261111.json`）；
  Berserk/Manhunter/ChaosSkip等随机变体、范围外/友伤/好感未穷举。
  通用入口按持有实例和原生准入开放，代表实测不等于逐Def运行。

## 地点/生成物（RY-05c）

`Wallraise` 等 `canTargetLocations` 能力用 `cell` 目标：

```json
{"pawn_id":"321","ability_def":"Wallraise","cell":{"x":166,"y":0,"z":175}}
```

- 原生 `CompAbilityEffect_Wallraise` 在 5 格 pattern 生成 `RaisedRocks`，服务不写
  地块/物品。
- 实测 Human321（psylink 4，native neural heat limiter 关闭）施放：
  `RaisedRocks` 0→5、psyfocus 1.0→0.98、entropy 0→35；第二发同点 422
  （`Reverse/evidence/ry05c-wallraise-20261111.json`，自审
  [RY-05c](../references/reviews/2026-11-10-ry05c-wallraise.md)）。
- 后续已补Chunkskip/Waterskip/Smokepop/BulletShield/Flashstorm/Neuroquake，
  共七种地点/持续效果语义（`Reverse/evidence/ry05c-*.json`）；
  具体数值/RNG/完整天气寿命未穷举。limiter关闭是fixture准备的原生选项，不是API自动绕过。

## 世界目标（RY-05e）

`Farskip` 的目标是世界对象，两个字段恰选一个：

```json
{"pawn_id":"321","ability_def":"Farskip","world_map_id":0,"confirm":true}
```

也可指定 `world_caravan_id`（原生 `Caravan.ID`）。服务分别构造
`GlobalTargetInfo(Map.Parent)` 或 `GlobalTargetInfo(caravan)`，按原生
`ValidateGlobalTarget`、`CanApplyOn`、`CanCast`、`OnCooldown`、`CanQueueCast` 校验。
目标地图/商队不存在404，字段组合错误400，原生无效目标422，施放状态冲突409。
原生效果决定成员和落点，客户端不能借此指定任意世界格或直接搬人物。

| 施法者起点 | 原生执行 | 返回与完成判断 |
| --- | --- | --- |
| 已生成的地图人物 | `QueueCastingJob(GlobalTargetInfo)` | `cast_mode:"job"`、`job_started`、`job_def`；工作开始后仍须读实际迁移/专注，不能当即判完成 |
| 商队成员（未生成在地图） | 原生队列函数的商队分支：资格/确认后 `Activate(GlobalTargetInfo)` | `cast_mode:"immediate"`、`job_started:false`、`job_def:null`，用`applied`和独立读回判断；无job是正常立即分支 |

商队分支如原生 `ConfirmationDialog` 要求确认，返回409及
`data.confirmation_required:true`、`confirmation`原文；重发`confirm:true`后施放，
服务不打开游戏确认窗口。原生无确认时不额外要求确认。
地图分支继续原生队列路径；本轮仅适配原版商队Farskip的确认，不承诺所有mod世界能力的UI回调。

`dry_run:true`执行资格/确认检查，只返回目标计划、`source_caravan_id`和`cast_mode`，
不调用Activate/QueueCastingJob，不改变商队/地图归属和专注。立即分支dry-run的`job_def`也为null。
正式立即施放返回`psyfocus_before/after`、`map_id_after`、`caravan_id_after`；
`Activate`返回false则409附实际结果，客户端先读回，不换key盲重试。
`Idempotency-Key`沿[game_memory合同](idempotency.md)冻结重放；同key异体409。

已保留的实际迁移：同图、map1→map0、map→caravan275（`ry05e-farskip/crossmap/caravan-20261111.json`）。
本轮修复商队起点原先被spawned门禁拒绝的问题，实测Human321+Human66734由caravan275回map0，
原商队销毁、两人存活/无商队归属，psyfocus1→0.3；tick74742立即完成。
确认409、确认dry无副作用、无确认窗口、同keyraw重放不再扣费和异体409均过；地图→商队工作路径也回归通过。
证据身份及fixture披露见[本轮自审](../references/reviews/2026-11-11-dsh-progress-repair.md)。

边界：商队→商队合并、无盟友目的地及更多成员/容器变体未实测；已有原生校验不等于逐项游戏验收。
fixture只准备复制存档中的人物、psylink/能力/资源和商队，不制造被验收的返回效果。

## 证据与边界

- 2026-11-10 自审：ghoul 自我施放 `GhoulFrenzy` → 原生 job、`GhoulFrenzy` hediff、1800 tick
  冷却拒绝；`GET` 同时列出 `UnnaturalHealing`/`Painblock`/`FireSpew`
  （`Reverse/evidence/an07a-abilities-20261110.json`）。
- RY-05：`Burden` 经同一入口施放，目标获得 `PsychicBurden`，psyfocus 1.0→0.99、
  neural heat 0→8、重复施放 409；`Invisibility` 在同状态 `can_cast=false`（原生原因空），
  记录为门槛而未绕过（`Reverse/evidence/ry05-psycasts-20261110.json`）。
- ID-08a：指派 Leader/Moralist 后，`CombatCommand` 自我 aura 与 `PreachHealth`
  受伤同 ideo 目标均产生原生 hediff；组冷却与二次 409、健康目标 422、未持有 404 成立
  （`Reverse/evidence/id08a-role-abilities-20261111.json`，自审
  [ID-08a](../references/reviews/2026-11-10-id08a-role-abilities.md)）。
- 后续目标pawn、双人物目的地、世界目标以及`UnnaturalHealing`（目标伤8→7.479）均已有代表结果；
  更多有不同资源/后果的能力仍按计划核差集。能力支持范围按“实际持有且原生允许”声明，
  不把通用源码可达自动说成所有Def实测。
