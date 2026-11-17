# Core 正常工作队列、优先工作与受精取消（C-14）

2026-11-16 修复：set_manual_priorities实际改变值后通知全部玩家人物workSettings缓存，dry/同值不重复刷新；控制器异步读body后通过协调器回主线程，兑现既有Game幂等/事件合同。原生排序、dry与重放通过，见[UI-12合同](ui-option-repairs.md)与[验收](../references/reviews/2026-11-16-ui-api-repair.md)。

2026-11-13 新增：

- `GET /api/v1/pawn/work-queue?pawn_id=`
- `POST /api/v1/pawn/work-queue`

复用型路由：暴露**命名原生动作**，不提供任意 job、任意目标或任意反射。清整房、
附近灭火的队列目标由原生对应 `FloatMenuOptionProvider` 的资格与
`Job.targetQueueA`/`AddQueuedTarget` 生成。

## 读取

```json
GET /api/v1/pawn/work-queue?pawn_id=321
{
  "pawn_id": "Human321",
  "spawned": true,
  "manual_priorities": false,
  "priority_work": {"active": false, "cell": null, "work_giver": null},
  "current_job": {"def_name": "TendPatient", "target_a": {"thing_id": 321,
    "thing_id_string": "Human321", "def_name": "Human"},
    "target_b": null, "queued_target_count": 0, "count": -1,
    "player_forced": true},
  "queued_jobs": [],
  "fertilize": {"ovum_id": "HumanOvum75497", "ovum_thing_number": 75497,
    "inbred_chance": 0.8, "inbred_relation": "Child",
    "requires_confirmation": true, "can_cancel": true},
  "blocked_reasons": []
}
```

- `manual_priorities`：全局 `Find.PlaySettings.useWorkPriorities`，只读。
- `priority_work`：原生 `Pawn_MindState.priorityWork` 的 active/cell/workGiver。
- `current_job`/`queued_jobs`：原生 `Pawn_JobTracker.curJob/jobQueue` 的 def、
  targetA/B、`queued_target_count`、count、playerForced；不返回可编辑队列。
- `fertilize`：当前 job 为原生 `FertilizeOvum` 时，ovum id、原生
  `PregnancyUtility.InbredChanceFromParents` 的 chance/relation、
  是否需确认、是否可取消；否则 null。
- `blocked_reasons`：`downed`/`mental_state`/`cleaning_disabled`/
  `firefighting_disabled`/`manual_dumb_disabled` 等只读原因。

## 写入动作

```json
POST /api/v1/pawn/work-queue
{"pawn_id":321,"action":"clean_room","cell":{"x":132,"z":112}}
```

- `clean_room` + `cell`：复刻 `FloatMenuOptionProvider_CleanRoom` 的资格：
  房间 proper/非 huge/非地图边缘/非室外/非 fogged，Cleaning 工作可用，
  room 内 filth 非 forbidden、在 Home 区、可到达、可预约；构造
  `JobDefOf.Clean` 并 `AddQueuedTarget(TargetIndex.A, filth)`。返回实际 targets
  和 count；资格不满足 409。
- `extinguish_fires` + `cell`：复刻 `FloatMenuOptionProvider_ExtinguishFires`：
  cell 正在燃烧、Firefighter 工作可用、allowed area、可达；用
  `GetFiresNearCell` 的 Fire 目标构造 `JobDefOf.ExtinguishFiresNearby` 并
  `AddQueuedTarget`。返回 targets/count；不满足 409。
- `prioritize` + `work_giver`（精确 DefName）+ `cell`/`thing_id` 二选一：
  要求 `def.directOrderable` 且 `def.Worker is WorkGiver_Scanner`；按原生
  `PotentialWorkThingRequest/PotentialWorkThingsGlobal/PotentialWorkCellsGlobal`、
  `ShouldSkip(forced:true)`、`HasJobOnThing/Cell(forced:true)`、
  `JobOnThing/Cell(forced:true)`、工作类型优先级、WorkTag、MissingRequiredCapacity、
  forbidden/allowed area/CanReach 检查；成功后设置 `job.workGiverDef` 并调用
  `pawn.jobs.TryTakeOrderedJobPrioritizedWork(job, scanner, priorityCell)`
  （其内部按 `prioritizeSustains` 写 `PriorityWork`）。读回
  `priority_work`/job；不做任意反射。
- `fertilize` + `thing_id`（可选 `confirm`）：验证原生男性、年龄、非 sterile、
  manipulation、可达；若 `InbredChanceFromParents > 0`，真实原生流程要求
  `Dialog_MessageBox` 确认，因此无 `confirm:true` 返回 409；确认后构造
  `JobDefOf.FertilizeOvum`（count=1）并用 `TryTakeOrderedJob`。该动作仅用于
  测试/正常受精入口；不设置 `HumanOvum` 私有 `fertilizingMan`（无公开 setter）。
- `cancel_fertilize` + optional `thing_id`：**仅**当前 job 为原生
  `FertilizeOvum` 且 targetA 为该 ovum 时 `EndCurrentJob(InterruptForced)`；
  读写 `ReservationManager.ReservedBy` 返回 reservation 清理；不 broad-cancel，
  非受精/不同 ovum 409。
- `set_manual_priorities` + `enabled`：写全局
  `Find.PlaySettings.useWorkPriorities` 并读回；影响整局，必须披露。
- 所有动作支持 `dry_run`；失败在写前返回 409/422/400，不部分改 job。

合同：GET observe；POST command/pawn_ai/validate/game_memory。合同行由协调者
登记。

## 原生来源与边界

- `FloatMenuOptionProvider_WorkGivers` 只处理 `directOrderable` scanner；
  `TryTakeOrderedJobPrioritizedWork` 仅在 `prioritizeSustains` 时写
  `PriorityWork`；其它工作只启动当前 job。
- `FloatMenuOptionProvider_CleanRoom`、`_ExtinguishFires` 的目标资格已复刻；
  未支持 pocket map 的特殊 `canBeCleaned` 分支、跨地图/多选、UI 右键菜单。
- `HumanOvum.CanFertilizeReport` 的私有资格已按可见条件复刻；`fertilizingMan`
  私有字段不写，原生 gizmo 的“正在受精”显示可能不同；取消以当前 job 为准。
- 不开放任意 job/queue 编辑、任意 work giver 反射、任意 target 选择；正常
  工作仍由原生 think tree/job driver 执行，不把一次 job 当持续策略。
- 未测：长时间多工作循环、多人队列竞争/阻挡、 incapacity 全分支、全部
  WorkGiverDef、真实 UI FloatMenu、pocket map、1.5。

## 证据

隔离 g35 tick 76787，五 DLC；fixture `tests/parallel/core-work/C14Fixture.cs`
准备原生 proper Home 房间、两块原生 dirt、原生 Fire、带 sibling 关系的原生
HumanOvum、空燃料原生 TorchLamp；从不执行产品动作。

- `manual_priorities` on/off 读回；clean_room dry/actual：`Clean` 队列 2 个
  filth，job 队列 count 读回，推进后真实结束；extinguish_fires：原生
  `ExtinguishFiresNearby` 队列 1 个 Fire，推进后 fire destroyed。
- `prioritize` `DoctorTendToSelf` + worker：原生 `TendPatient` job，
  `PriorityWork.active=true`、workGiver/cell 读回。
- `fertilize` 无 confirm 409、dry-run applied=false、confirm 后
  `FertilizeOvum` 真实当前 job；错误 ovum cancel 409；正确 cancel
  `reservation_released=true`、当前 job 不再 fertilize。
- 证据：`tools/cua/.runtime/c14-run19-20261113/evidence/c14.json`（phase passed）；
  run18/run16 也 passed；run19 为最终代码（含 forbidden/allowed 检查）的最终证据。
