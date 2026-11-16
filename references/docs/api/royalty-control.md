# Royalty 灵能与许可（G-29a）

已交付限定范围：修复后DLC-on独立fixture，77项真实游戏检查通过；证据
`Reverse/evidence/dlc-royalty-review-main.json`；另有23项Job/资源回归，见`dlc-royalty-review-green.json`（许可有数据行SKIP）。仅Royalty生效，路径以`/api/v1`为前缀。
**整体仍未完成**：本页专用`psycast`授予/移除/施放接口保留Painblock/Stun/SolarPinhole/Skip四能力窄合同；
通用[玩家能力接口](pawn-ability.md)已扩单体、范围、地点/持续、双人物和世界GlobalTarget，
含商队Farskip立即施放；不能用本专用接口白名单推断全API只支持四项。
本轮核对：2026-11-11，具体余项见[修复自审](../references/reviews/2026-11-11-dsh-progress-repair.md)。
许可购买/退款/使用/execute、运输和头衔已在其它服务交付，分别见[许可与运输](royalty-permits.md)、
[头衔](royalty-title.md)。后续真实缺口见[五 DLC 剩余清单](../product-specs/dlc-gap-audit.md)的 RY-05（RY-01–04 保留首轮限定成果）。

本页灵能接口适用范围：本地图上已生成的玩家自由殖民者。不授予Psylink等级、不改头衔/声望、不消耗神经训练器、
不动精神熵上限；动物、囚犯、访客返回422。

## 读取

`GET /dlc/royalty/psycasts?pawn_id=321`

```json
{
  "pawn_id": "Human321",
  "psylink_level": 1,
  "entropy_value": 0.15,
  "max_entropy": 100.0,
  "psyfocus": 0.98,
  "abilities": [
    {"ability_def": "Painblock", "level": 1, "can_cast": true,
     "cooldown_ticks_remaining": 0, "on_cooldown": false, "supported": true}
  ]
}
```

`psylink_level`调原生`GetPsylinkLevel()`，读取独立保存的Hediff_Level.level。
单独改severity不能提高原生等级；MaxAbilityLevel只是常量上限。
`can_cast`取原生`Ability.CanCast.Accepted`，包含等级、精神熵、组件/地图等准入；施法时还校验目标与CanQueueCast。
`supported`表示该能力是否在本接口白名单内。

`GET /dlc/royalty/permits?pawn_id=321`

```json
{"pawn_id": "Human321", "permits": [
  {"permit_def": "TradeOrbital", "faction_id": "Faction_Empire_7", "last_used_tick": 1234, "on_cooldown": false}
]}
```

只读`Pawn_RoyaltyTracker.AllFactionPermits`；`faction_id`是原生`GetUniqueLoadID()`字符串。
本接口**不调用**`Notify_Used`、不放投资源、不进入许可执行。

## 白名单授予/移除

`POST /dlc/royalty/psycast`，`DELETE /dlc/royalty/psycast`

```json
{"pawn_id":"321","ability_def":"Painblock","dry_run":true}
```

成功数据为`{pawn_id,ability_def,changed,dry_run,psylink_level,abilities}`。
白名单按原生目标形态分组：`Painblock`/`Stun`（单 pawn）、`SolarPinhole`（单 cell）、
`Skip`（两段式：目标 + 选定目的地）。其它已知能力仍 501，未知 Def 404。

### 两段式 Skip 实测（RY-01，2026-11-08）

`ai_g28_after_grant` 副本（tick 70342）的 Blean（Human321，psylink 1）：
先验 `/psycast Skip`（1 级）422“psylink level 1 is below the ability level 4”；fixture 用原生
`ChangePsylinkLevel` 提升到 4（已披露，同时由原生授予该项等级能力），产品 grant Skip 成功。
cast 新增字段：

```json
{"pawn_id":"321","ability_def":"Skip","thing_id":"49389",
 "dest_cell":{"x":158,"y":0,"z":116},"confirm":true,"dry_run":false}
```

- 主目标三选一：`target_pawn_id`、`cell`、`thing_id`（ThingID 或数字 thingIDNumber）；其它组合 400。
- 两段式能力必须给 `dest_cell`（缺失 400）；非两段式给 `dest_cell` 400。
- 目的地按原生 `CompAbilityEffect_WithDest.CanHitTarget/CanPlaceSelectedTargetAt` 校验
  （越界/占位/不满足射程或视线 422），再把 `GetJob(target, destination)` 排队。
- 实测：dry-run 返回 `target_kind=thing`、destination、job_def CastAbilityOnThing、psyfocus 0.02；
  正式提交后 Silver49389 由 (154,128) 移动到 (158,116)，即原生传送生效；保护仍启用。
- 无目的地 400、40 格外 422 均实测。`psyfocus/cooldown` 仍由原生 Job 结算。

证据 `Reverse/evidence/an05-ry01-skip.json`，runner `tests/ry01_psycast_skip.py`。
边界：世界/GlobalTarget 能力、多个目的地、跨地图与其它能力族未验；`Skip` 的 psylink 4
在正常玩法由授勋提升，本轮用 fixture 提升并披露。


- 授予只调`Pawn_AbilityTracker.GainAbility`，**不提升Psylink**；`psylink_level`低于
  能力`level`时422。
- 重复授予`changed:false`且不发operation；`dry_run:true`不改tracker。
- 移除只调`RemoveAbility`。能力来自基因时422（基因会再次授予，假装移除此后会静默回退）；
  只剩临时能力时同样422；该层没有该能力404。
- 可选`Idempotency-Key`遵循 [通用幂等合同](idempotency.md)。

实测：psylink插件加入时会由原生随机授予1个1级能力（本机为Painblock），
本接口只在该结果之上增删，不改变原生授予结果。

## 施法Job

`POST /dlc/royalty/psycast/cast`

```json
{"pawn_id":"321","ability_def":"Painblock","target_pawn_id":"318","confirm":true,"dry_run":false}
```

成功数据为
`{pawn_id,ability_def,target,queued,job_def,dry_run,psyfocus_cost,cooldown_ticks_total}`。

- `target_pawn_id`与`cell`恰有一个；两个或都没有400；非预演缺`confirm:true`400。
- 目标Pawn必须与施法者同地图且已生成（异图/未生成422）；越界格422。
- 校验顺序：冷却409 → 原生Psylink等级不足422 → `CanApplyOn`422 → 原生verb命中/射程422 →
  精神力不足409 → 原生`CanCast/CanQueueCast`拒绝409。
  非预演才调用`GetJob`（为空501）与`TryTakeOrderedJob`（失败409）。
- 只排队原生Job（`job_def`通常是`CastAbilityOnThing`），不调用`Activate`/
  `QueueCastingJob`/UI targeter；精神力与精神熵由原生Job扣除。
- `queued:true`不是"已施放"；`dry_run:true`不构造Job、不消耗全局Job ID、不产生operation。
- 1级灵能没有冷却，**不同key的重试会再次施放**；去重必须使用同一`Idempotency-Key`。

实测：Painblock施给同地图殖民者，推进后目标出现原生`PsychicAnesthesia`，
历史71项只验证精神熵上升，不能单凭该项证明一次消耗。修复回归分别比较实际psyfocus差额与成本、
暂停状态下重放前后的nextJobID/当前及队列Job/资源；同key返回原正文且只发一次operation。
存读档后目标Hediff仍在。1级灵能在本机`cooldown_ticks_total`为0，因此没有冷却可触发。

## 错误与DLC门

| 情况 | 状态码/`error_code` |
| --- | --- |
| 缺字段、双目标、缺confirm | 400 `bad_request` |
| 未知pawn/能力/目标Pawn | 404 `not_found` |
| 白名单外能力、无原生施法Job | 501 `not_supported` |
| 冷却中、精神力不足、Job无法排队 | 409 `conflict` |
| 灵能等级不足、基因来源移除、动物/囚犯、异图目标、越界 | 422 `invalid_target` |
| Royalty未启用 | 422 `dlc_missing` |

## 未覆盖

- 本专用灵能端点不修改Psylink/头衔/声望或执行许可；正常授勋、头衔、许可已有各自服务。
  其余灵能、双pawn目标和跨地图/商队世界目标走通用[pawn/ability](pawn-ability.md)，
  未因此扩大本端点授予/移除白名单。
- 未测：临时来源能力移除、冷却409（1级灵能无冷却）、任意Mod、1.5；精神熵/专注不足拒绝已在独立fixture副本验证。
- 合法cell目标已验证预演与Job提交，未据此宣称目标效果完成。
- 许可行只在无头衔角色上观察到空列表；有许可时的字段取值未经真实数据验证。


## 冥想与目标专注（RY-06b）

`GET /api/v1/dlc/royalty/meditation?pawn_id=` 读 `psylink_level`、`psyfocus`、
`target_psyfocus`、`time_assignment`、`spot_id`/`focus_id`（原生
`MeditationUtility.FindMeditationSpot`）和 `current_job`。

`POST /api/v1/dlc/royalty/meditation` `{pawn_id,target_psyfocus,dry_run}` 调用原生
`Pawn_PsychicEntropyTracker.SetPsyfocusTarget`；0..1 越界 400，无 psylink 422。
它不改当前 psyfocus、不生成冥想点、不改日程。

初始样本Human321（psylink1）在`MeditationSpot75478`+`SculptureSmall75479`的
Meditate日程中，0.45→0.5400（tick84523），目标0.55；此时仍`current_job:Meditate`，
该样本只证明专注增长；后续退出观察见下节（`Reverse/evidence/ry06b-meditation-20261111.json`，自审
[RY-06b](../references/reviews/2026-11-10-ry06b-meditation.md)）。


### 冥想停止语义（RY-06b）

`target_psyfocus`只设置原生目标，不承诺到某个数值精确结束工作。
`JobDriver_Meditate`有多条退出/工作替换路径；其中`ignoreJoyTimeAssignment && !flag`
分支每4000tick检查`CurrentPsyfocus >= max(TargetPsyfocus+0.05,0.99)`并请求工作替换。
另有冥想排班变化、工作时长和娱乐等条件，不能把上述Max公式读成总是target+0.05。

历史Anything样本target0.55，tick105787读到psyfocus0.6074、`Wait_MaintainPosture`，
证明原生冥想曾增长并已退出；该数值不足以证明由0.60阈值分支触发。
原[停止阈值自审](../references/reviews/2026-11-10-ry06b-meditation-stop.md)保留，
本轮更正其因果结论，不改写原JSON（`ry06b-meditation-stop-20261111.json`）。
自然联结/`LinkPsylinkable`仍是独立正常链待办。


## 头衔房间要求评估（RY-07a）

`GET /api/v1/dlc/royalty/title/room-requirements?pawn_id=321` 返回当前头衔的
原生王座/卧室 requirement：`label`、`met`、`disabled`，以及
`assigned_throne`/`throne_room_id`/`has_personal_bedroom`/`grace_period`。
`met:null` 表示没有 assigned throne/room，**不是已满足**。

实测 Human321（Acolyte、无王座/卧室）：王座 10 项、卧室 6 项均 `met:null`、
`disabled:false`、`grace_period:true`
（`Reverse/evidence/ry07a-room-requirements-20261111.json`，自审
[房间要求](../references/reviews/2026-11-10-ry07a-room-requirements.md)）。
该初始样本仅未分配房间；下文后续样本已补已分配满足/不满足，豁免/不同头衔未穷举。

已分配样本：Human321 的 8×7 房间（6×5 内室、30 格、ThroneRoom、Throne +
两支火盆）使 10 项王座要求全部 `met:true`；无个人卧室时卧室 6 项仍
`met:null`（`Reverse/evidence/ry07a-throne-room-20261111.json`，自审
[王座房](../references/reviews/2026-11-10-ry07a-throne-room.md)）。

已分配卧室样本：8×7 房/6×5 内室、原生 DoubleBed75504、ClaimBed 后
OwnedRoom=16、room role Bedroom，6 项卧室要求全 `met:true`；无王座时王座 10 项
仍 `met:null`（`Reverse/evidence/ry07a-bedroom-20261111.json`，自审
[卧室](../references/reviews/2026-11-10-ry07a-bedroom.md)）。


## 帝国许可选项读取（RY-07a）

`GET /api/v1/dlc/royalty/permit/options?pawn_id=321` 列出
`RoyalTitlePermitDef` 的 `label/min_title/cost/prerequisite/worker_class/
royal_aid/cooldown`，并调用原生 `AvailableForPawn`；已持有许可单列 `held`。
单个 def 的原生异常会落在该行 `available_error`，不导致整个读取 500。

实测 Human321 Acolyte/Empire 1 点：15 个定义、4 个可用（SteelDrop/FoodDrop/
CallLaborerTeam/CallMilitaryAidSmall）；TradeSettlement/TradeOrbital/TradeCaravan
该旧样本的原生 `AvailableForPawn` NRE 当时记为不可用；当前源码已处理minTitle-null，见下述后续样本
（`Reverse/evidence/ry07a-permit-options-20261111.json`，自审
[许可选项](../references/reviews/2026-11-10-ry07a-permit-options.md)）。
该样本只读；后续已补购买/退款/Trade读取。本轮已补轨道/聚落/NPC世界商队实际交易，更多任务专属回调仍未穷尽；RoyalTitle无obligation字段，不挂虚假待办。

不满足样本：销毁两支火盆之一后 `brazier 1/2` 返回 `met:false`，其余 9 项仍
true，王座归属/房间不变（`Reverse/evidence/ry07a-throne-unsatisfied-20261111.json`，
自审 [不满足](../references/reviews/2026-11-10-ry07a-throne-unsatisfied.md)）。

不满足卧室样本：DoubleBed→原生单 `Bed` 后 `double bed 0/1` 返回 `met:false`，
其余 5 项仍 true，has_personal_bedroom true
（`Reverse/evidence/ry07a-bedroom-unsatisfied-20261111.json`，自审
[不满足卧室](../references/reviews/2026-11-10-ry07a-bedroom-unsatisfied.md)）。

Trade 读取：原生 `AvailableForPawn` 在 `minTitle==null` 时对已授衔 pawn NRE；
选项读取对这三个def按同一门槛计算并标记
`available_source:"min_title_null"`，不再返回 `available_error`
（`Reverse/evidence/ry07a-permit-trade-20261111.json`，自审
[Trade 读取](../references/reviews/2026-11-10-ry07a-permit-trade.md)）。

退款样本：SteelDrop 购买后设 Empire favor 100，`POST /permit/refund` 返回
required/actual 8、favor 100→92、points 0→1、permits_after []、outcome
`refunded`（`Reverse/evidence/ry07a-permit-refund-20261111.json`，自审
[退款](../references/reviews/2026-11-10-ry07a-permit-refund.md)）。


## Trade资格与实际结算范围

`TradeSettlement`/`TradeOrbital`/`TradeCaravan`由`RoyalTitleDef.permitRequiredForTrading`引用，
表示帝国贸易头衔资格；普通贸易不花援助favor，也不启动援助冷却。
`permit/purchase`拒绝cost<=0的被动定义，`permit/options`现有minTitle-null分支已避免上述旧NRE。
本轮交易对所有上下文调用原生`Pawn.CanTradeWith`；头衔规则来自原生资格，不另写假费用。

[交易合同](trade-control.md)已扩地图商人、轨道船、玩家商队与世界聚落/NPC商队，
保留原生TradeSession/TradeDeal结算。本轮三类新增上下文及旧地图买入回归均读回真实物品/Silver和重放，
见[修复自审](../references/reviews/2026-11-11-five-dlc-gap-repair.md)。只覆盖普通物品/Silver；
不声称人物、器官、礼物及全部任务专属菜单已完成。
