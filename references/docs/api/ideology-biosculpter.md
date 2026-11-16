# Ideology 生物塑形舱（G-46）

交付：读取地图上的 `BiosculpterPod` 状态、周期与按 Pawn 资格，正常进入/弹出，以及由另一名殖民者把倒地殖民者/囚犯搬入并选择周期（ID-R08）。
证据：`Reverse/evidence/dlc-G46-biosculpter.json`、`dlc-G46-run.json`。
fixture：`tests/g43_fixture` 的 `prepare_biosculpter`（测试专用，不属于生产）。

## 读取

`GET /api/v1/dlc/ideology/biosculpter?map_id=0&pawn_id=321`

```json
{"map_id":0,"pod_count":1,"pawn_id":"Human321",
 "pods":[{"pod_id":"BiosculpterPod...","thing_id":...,"def_name":"BiosculpterPod",
   "map_id":0,"position":{"x":...,"y":0,"z":...},
   "state":"SelectingCycle","power_on":true,"nutrition_loaded":true,
   "required_nutrition_remaining":0.0,"occupant_id":null,
   "current_cycle":null,"current_cycle_duration_days":0.0,
   "cycle_ticks_remaining":0.0,"cycle_speed_factor":1.0,
   "available_cycles":[{"key":"pleasure","label":"pleasure","duration_days":4.0,
     "missing_research":[],"can_use":true,"reason":null}]}]}
```

- `state` 是 `BiosculpterPodState`（`LoadingNutrition|SelectingCycle|Occupied`）。
- `nutrition_loaded` 来自 `RequiredNutritionRemaining <= 0`；`available_cycles` 来自
  `CompBiosculpterPod.AvailableCycles`，`missing_research` 来自 `MissingResearchLabels()`。
- 传 `pawn_id` 时每周期给出 `can_use/reason`，复用原生
  `CannotUseNowCycleReason`/`CannotUseNowPawnCycleReason`/`CanAcceptOnceCycleChosen`；
  不传时 `can_use` 为 null。
- `cycle_ticks_remaining` 只读原生私有字段（`currentCycleTicksRemaining`），是观察值。

## 进入

`POST /api/v1/dlc/ideology/biosculpter/enter`

```json
{"map_id":0,"pod_id":"BiosculpterPod...","pawn_id":"321",
 "cycle_key":"pleasure","dry_run":true}
```

- 原生玩家路径是 `CompBiosculpterPod.CompFloatMenuOptions` →
  `PrepareCycleJob(selPawn, selPawn, cycle, EnterBiosculpterJob())`。本端点复用
  `EnterBiosculpterJob`/`ConfigureJobForCycle` + `TryTakeOrderedJob`，并登记
  `SetQueuedInformation`；`applied:true` 只表示已提交进入工作，完成要等 Pawn 走到并进入。
- 修复类周期（medic/bioregeneration）的原生确认对话框被跳过；资格/材料/研究仍由原生
  `CannotUseNow*` 检查，不满足 422。
- Pod 必须玩家所属、通电且 `SelectingCycle`；已占用/营养未载入/研究缺失/材料不足/不可达
  返回 422；同 Pawn 已在去该 Pod 的路上 409。
- dry-run 只投影 `job_def=EnterBiosculpterPod` 与周期，不创建 job、不写 queued。

## 他人搬入：倒地殖民者/囚犯（ID-R08）

原生玩家路径：右键倒地殖民者或殖民地囚犯 → `FloatMenuOptionProvider_CarryToBiosculpterPod.GetOptionsFor`
→ `CompBiosculpterPod.PrepareCycleJob(hauler, biosculptee, cycle, MakeCarryToBiosculpterJob(biosculptee))`。
本端点复用同一原生 job（`CarryToBiosculpterPod`：targetA=患者、targetC=pod、
`biosculpterCycleKey`），只跳过原生确认对话框与玩家消息。

`POST /api/v1/dlc/ideology/biosculpter/enter` 增加可选 `carrier_pawn_id`：

```json
{"map_id":0,"pod_id":"BiosculpterPod...","pawn_id":"Human318",
 "carrier_pawn_id":"Human315","cycle_key":"pleasure","dry_run":true}
```

- 患者资格（对齐原生 float menu）：玩家殖民者已倒地，或殖民地囚犯（倒地或站立）；
  排除越狱中的囚犯、任务暂住者（`IsQuestLodger`）与 `GetExtraHostFaction()!=null`
  的客人/客人囚犯；搬运者必须能预留并走到患者格。
- 搬运者资格：存活、已生成、未倒地、无精神状态、玩家殖民者且具有 Manipulation。
- Pod/周期仍走原生 `CannotUseNowReason`/`CannotUseNowCycleReason`/
  `CannotUseNowPawnCycleReason(hauler, biosculptee, cycle)`/`CanAcceptOnceCycleChosen(patient)`；
  研究或材料不足返回 422 并带原生原因。
- 材料（如 bioregeneration 需要 2 个 `MedicineUltratech`）按原生
  `CanReachOrHasIngredients` 处理：患者/搬运者已携带则直接进入，否则由搬运者
  用 `WorkGiver_DoBill.TryFindBestFixedIngredients` 选取（job `targetQueueB`），
  原生完成转移并在 `TryAcceptPawn` 时从患者背包扣除；不会伪造或跳过材料。
- 成功返回 `mode:"carry"`、`submitted_job`（`carried_pawn_id`/`pod_id`/`cycle_key`）、
  `chosen_ingredients`、`carrier_before/after` 与 `after`（`queued_pawn_id`/
  `queued_cycle_key`）。`carrier_pawn_id` 与 `pawn_id` 指向同一 Pawn 时 400；
  被拒绝/已排队 422/409。省略 `carrier_pawn_id` 保持原自助进入路径（现在拒绝囚犯）。

`GET /api/v1/dlc/ideology/biosculpter?map_id=0&pawn_id=...&carrier_pawn_id=...`

- `pods[]` 新增 `occupant_contained`（患者确实在 pod `innerContainer` 中）、
  `queued_pawn_id`、`queued_cycle_key`；`available_cycles[]` 新增 `pawn_spawned`。
- 传入 `carrier_pawn_id` 时返回 `carrier`：`carrying_thing_id`/`carrying_pawn_id`、
  当前 `job_def`/`job_target_a`/`job_target_c`/`job_cycle_key`/`job_ingredient_targets`。
- 患者被搬运或已在 pod 内时处于 despawn 状态（`Map==null`），按 Pawn 的
  `can_use` 返回 `null`（`pawn_spawned:false`），不再在原生材料搜索上空引用。

### 实测（2026-11-12，g35 副本 tick 76787）

隔离 build/profile/mod，`debug/invulnerability` 全程开启（仅下地那一次同步调用暂停后
立即恢复）。fixture 生成玩家 pod/供电/6 餐，原生 `HealthUtility.DamageUntilDowned`
把 Human318 打成倒地（Stab/Malnutrition/Bruise/Crack），原生
`SetGuestStatus(Prisoner)` 把 Human321 变为囚犯，另生成 2 个 `MedicineUltratech`，
其余自由殖民者被征召以免救援抢走患者（测试准备）。

- Human315 搬 Human318（`carrier_pawn_id`）cycle `pleasure`：提交后原生 job
  `CarryToBiosculpterPod`（job 26057），搬运途中读回 `carrying_pawn_id=Human318`；
  终态 pod `Occupied`、`occupant_id=Human318`、`occupant_contained=true`、
  `current_cycle=pleasure`，搬运者 `carrying_*=null`；原生 observe
  `contained_in_pod=true`、`carried_by=null`。中断弹出后 Pod 回 `LoadingNutrition`。
- Human321（囚犯）同法搬入 cycle `bioregeneration`：预览
  `chosen_ingredients=[MedicineUltratech x2]`，原生搬运并转移材料；终态
  `occupant_id=Human321`、`occupant_contained=true`、`current_cycle=bioregeneration`、
  搬运者释放；地图+背包的 `MedicineUltratech` 由 8 降到 6（原生消耗 2）。
- 资格门：研究未完成时 `bioregeneration` 422 `Missing required research Bioregeneration`；
  完成研究并消耗 2 个材料后再次请求 422 `Cannot find cycle ingredients
  (2x glitterworld medicine)`；囚犯当搬运者 422（carrier 资格）；囚犯自助进入 422
  （需 `carrier_pawn_id`）。每次拒绝后 pod 仍 `SelectingCycle`、无排队/占用。
- 证据：`tools/cua/.runtime/id-r08-run4-20261112/evidence/id-r08.json`；runner
  `tests/parallel/ideology/id_r08_biosculpter_carry.py`；fixture
  `tests/parallel/ideology/IdR08Fixture.cs`。
- 未测：越狱中/任务暂住者/客人囚犯的排除分支、biotuned pod、medic/ageReversal
  周期、4–25 天自然完成、多 Pod、界面 float menu 操作本身。

## 弹出

`POST /api/v1/dlc/ideology/biosculpter/eject`

```json
{"map_id":0,"pod_id":"BiosculpterPod...","interrupted":true,"dry_run":false}
```

- 原生 `CompBiosculpterPod.EjectContents(interrupted, playSounds:true)`。
  `interrupted` 默认 true，会按原生加 `BiosculptingSickness` 和 `SoakingWet`；
  传 false 表示非中断弹出（不完成周期效果）。
- 空 Pod 返回 409；成功返回 `ejected_pawn_id/ejected_spawned/biosculpting_sickness`
  与 `after`（状态回到 `LoadingNutrition`、occupant null、营养清零）。
- 周期自然完成不在本端点：原生 `CycleCompleted` 由 tick 驱动，4–25 天，未在限定样本内跑完。

- 周期自然完成由原生 tick 驱动（4–25 天），不是本端点的写操作；ID-03 已用 fixture 仅缩短
  `currentCycleTicksRemaining`（已披露）让原生 `CycleCompleted` 跑完，验证终态与效果。

## 周期自然完成实测（ID-03，2026-11-08）

`ai_g31_fixture` 副本（tick 74742），fixture 生成 Pod/供电/6 餐并液化、选 Human321：

- **pleasure**：产品 `enter cycle_key=pleasure` → Occupied（remaining 239,866）；fixture 把剩余
  缩短到 5 tick（只改时长）；原生 `CycleCompleted` → Pod `LoadingNutrition`、occupant 生成、
  **无** `BiosculptingSickness`（非中断完成路径）、mood factor `BiosculpterPleasure` 出现。
- **ageReversal**：补餐后 `enter cycle_key=ageReversal` → Occupied（remaining 479,945）→
  缩短到 5 tick → 原生完成：occupant 生成、年龄 19→18（原生 −1 年）、mood factor
  `AgeReversalReceived` 出现。
- 保护全程开启。fixture 只缩短周期剩余 tick 与补餐，不写完成状态或效果。
- 边界：medic/bioregeneration 的材料/研究门禁与治疗效果、多 Pod、断电中断仍留未测；自然 4–25 天
  时长未运行。
- 证据：`Reverse/evidence/an05-id03-biosculpter.json`，runner `tests/id03_biosculpter_complete.py`。

## 实测

`ai_g31_fixture` 副本（tick 74742），fixture 生成玩家 BiosculpterPod + 燃料发电机/导线、
6 个简单餐并液化、选一名殖民者；API 读取 SelectingCycle/nutrition/power/pleasure eligible，
enter dry-run/正式提交后原生走到并 Occupied（cycle=pleasure、remaining>0），
eject interrupted 后 Pawn 生成、BiosculptingSickness、Pod LoadingNutrition；
再次 eject 409。未测其它周期的材料/研究拒绝与多 Pod；pleasure/ageReversal 自然完成见上。

## medic 与 bioregeneration（ID-08d）

- `medic` 走既有 `POST .../biosculpter/enter`：原生 `CompBiosculpterPod_MedicCycle`
  治疗新鲜伤口/失血并随机治一种感染。2026-11-10 实测：fixture 给 `Human321`
  一个 Cut（health 0.9467、bleed 0.24），正常进入 medic，原生 6 天一周期完成后
  自动弹出，health 1.0、bleed 0、Cut 消失，Malnutrition/PsychicAmplifier 保留；
  营养被消耗，pod 进入 LoadingNutrition。
- `bioregeneration` 需要研究 `Bioregeneration` 和 2 个超科技药物；本存档未研究，
  `GET .../biosculpter` 的 `available_cycles` 给出 `can_use:false`、
  `missing_research:["Bioregeneration"]`；补满营养后 POST 仍 422
  `Missing required research Bioregeneration`。未伪造研究解锁，也未运行该周期。
- 限制：多 Pod、断电中断、超科技药物实际消耗、永久伤/缺失部位恢复未验。
- 证据 `Reverse/evidence/id08d-biosculpter-supercharger-20261111.json`，自审
  [ID-08d](../references/reviews/2026-11-10-id08d-biosculpter-supercharger.md)。
