# Ideology 生物塑形舱自动供料/自动年龄逆转（ID-R07）

2026-11-12 新增：读写 `CompBiosculpterPod.autoLoadNutrition` 与私有
`autoAgeReversal`（公开只读 `AutoAgeReversal`），即原生 pod gizmo 的
“自动装载营养”和“自动年龄逆转”开关；进入/弹出循环保留。

## 读取

`GET /api/v1/dlc/ideology/biosculpter?map_id=0` 的每个 pod 新增
`auto_load_nutrition`、`auto_age_reversal`（以及既有 nutrition/cycle/occupant）。

## 设置

`POST /api/v1/dlc/ideology/biosculpter/policy`

```json
{"map_id":0,"pod_id":"BiosculpterPod75478",
 "auto_load_nutrition":false,"auto_age_reversal":true,"dry_run":true}
```

- 至少给一个字段；pod 必须是玩家所属；`auto_age_reversal` 经
  `AccessTools.Field` 写原生私有字段。
- 返回 `before_*`、`requested_*`、更新后的 pod 状态和 `applied`；dry_run 不写。
- 合同：POST `policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成通电 BiosculpterPod（6 份简单食物、SelectingCycle）。初始
`auto_load_nutrition:true/auto_age_reversal:false`；dry 保持；正式设
false/true 后 GET 一致；再恢复 true/false。
证据 `tools/cua/.runtime/id-r07-20261112/evidence/id-r07.json`，runner
`tests/id_r07_biosculpter.py`，fixture `prepare_biosculpter`。

## 真实自动供料与自动年龄逆转（ID-R07 V，2026-11-13）

旧边界只覆盖两个策略字段；本轮用真实供电/食物复验原生调度，生产代码未改。

fixture 生成玩家伐木发电机（燃料 75）+ 导线供两台玩家 `BiosculpterPod`，
pod1 旁 10 份简单餐、pod2 旁 6 份；pod2 先设 `auto_load_nutrition=false`。

- **自动供料**：pod1 保持 `auto_load_nutrition=true`、通电且
  `LoadingNutrition`，一名空闲殖民者由原生
  `WorkGiver_HaulToBiosculpterPod` 自动搬运并液化食物，pod1 到
  `SelectingCycle`、`nutrition_loaded=true`、`required_nutrition_remaining=0`；
  pod2 的 6 份餐被 fixture 设为 forbidden 后保持 6 份，pod2 在 1200 tick
  内仍 `LoadingNutrition`/未载入（`auto_load_nutrition=false` 不再调度）。
- **自动年龄逆转**：g35 所有理念都没有 `AgeReversal_Demanded` precept，而原生
  pod tick 在该 precept 缺失时会强制关掉 `autoAgeReversal`；fixture 通过原生
  `PreceptMaker.MakePrecept + Ideo.AddPrecept` 给目标 ideo 补该 precept
  （测试脚手架，披露）。pod1 `SetBiotuned(Human315)` 后：`auto_age_reversal=false`
  时强制需求已到期仍 600 tick 不入舱；设 true 后原生
  `JobGiver_GetAgeReversal` 让 Human315 自行走入 pod1，终态
  `Occupied/occupant=Human315/current_cycle=ageReversal/remaining≈479996`，
  营养被消耗。
- **周期终态**：fixture 只把原生 `currentCycleTicksRemaining` 缩到 20
  （披露，与 ID-03 同法），真实 `CycleCompleted` 跑完：pod 回
  `LoadingNutrition`、入住者弹出生成、生物年龄 47→46、原生
  `AgeReversalReceived` 心情记忆出现、`AgeReversalDemandedDeadlineTicks`
  重置为 3599954、**无** `BiosculptingSickness`（非中断完成）。
- 原生依据：`WorkGiver_HaulToBiosculpterPod` 要求 `PowerOn` +
  `LoadingNutrition` + `autoLoadNutrition` + 允许的 nutrition + 可达食物；
  `JobGiver_GetAgeReversal` 要求 biotuned pod、`AutoAgeReversal`、adult、
  `AgeReversalDemandedDeadlineTicks<=0`、`CanAcceptOnceCycleChosen`、
  `PawnCanUseNow`；低期望值 pawn 每 2500 tick 会 +2500 到 demand deadline
  （fixture 把原生 deadline 设在过去以稳定触发，披露）。
- 证据：`tools/cua/.runtime/id-r07-run7-20261113/evidence/id-r07.json`
  （phase=passed）；runner `tests/parallel/ideology/id_r07_biosculpter_auto.py`，
  fixture `tests/parallel/ideology/IdR07Fixture.cs`。
- 未测：完整 8 天自然周期、生物调谐倒计时/重调谐、多 pod 争同一 pawn、
  搬运/周期中掉电、pod 被毁、medic/bioregeneration 自动行为、策略与营养
  save-load。
