# Biotech 婴儿照护（BT-11h）

## 瓶喂

`POST /api/v1/dlc/biotech/childcare/feed`

```json
{"feeder_id":"321","baby_id":"75478","food_id":"BabyFood75481","dry_run":true}
```

校验 baby 为 `DevelopmentalStage.Baby`、feeder 可操作/同图/可 Reach、food 为
`BabyFood`，调用原生 `ChildcareUtility.CanFeedBaby` 与
`ChildcareUtility.MakeBottlefeedJob`，把 job 交给 feeder。服务不直接改食物需求
或消耗物品。

实测 baby Human75478 food 0.1、feeder Human321、BabyFood75481：tick 77997
food 1.0、BabyFood 消失，feeder 后续 BringBabyToSafety（`Reverse/evidence/
bt11h-baby-feed-20261111.json`，自审
[BT-11h](../references/reviews/2026-11-10-bt11h-baby-feed.md)）。
边界：只测一次瓶喂；母乳/自动喂养/多食物/拒绝/照护后续/学习未验。


## 母乳喂养

`POST /api/v1/dlc/biotech/childcare/breastfeed`

```json
{"mother_id":"315","baby_id":"75478","dry_run":true}
```

校验 baby 为 Baby、母亲同图/存活/可操作，调用原生
`CanMomBreastfeedBabyNow` 与 `MakeBreastfeedJob(baby)`；job 交给母亲。
实测 mother Human315（Lactating）/baby Human75478 food 0.1：tick 77938 food
1.0，随后 BringBabyToSafety（`Reverse/evidence/bt11h-breastfeed-20261111.json`，
自审 [母乳](../references/reviews/2026-11-10-bt11h-breastfeed.md)）。
边界：只测一次直接母乳；母乳不足/抱送/卧床/自动喂养/拒绝未验。


## 抱回安全区

`POST /api/v1/dlc/biotech/childcare/carry`

```json
{"hauler_id":"321","baby_id":"75478","dry_run":true}
```

校验 Baby、hauler 同图/存活/可操作/可 Reach，调用原生 `CanHaulBaby` 与
`MakeBringBabyToSafetyJob`；job 交给 hauler。实测 baby Human75478/hauler
Human321：tick 76845 `carried:true`、baby `Carried`、hauler
`BringBabyToSafety`（`Reverse/evidence/bt11h-baby-carry-20261111.json`，自审
[抱送](../references/reviews/2026-11-10-bt11h-baby-carry.md)）。
边界：只测抱起；安全区落点/后续照护未验。


## 自动喂养

`POST /api/v1/dlc/biotech/childcare/autofeed`

```json
{"feeder_id":"321","baby_id":"75478","dry_run":true}
```

校验 Baby/feeder，调用 `CanFeedBaby`，用 `FindBabyFoodForBaby` 选食物，再
`MakeAutofeedBabyJob`；无可用 BabyFood 返回 422。实测 BabyFood75481 被选中，
tick 77937 baby food 1.0、food 消失
（`Reverse/evidence/bt11h-autofeed-20261111.json`，自审
[自动喂养](../references/reviews/2026-11-10-bt11h-autofeed.md)）。
边界：单食物；多食物/不可达/太饱/自动开关未验。


## 照护状态读取

`GET /api/v1/dlc/biotech/childcare?pawn_id=75478` 返回
`developmental_stage/age/dead/downed/carried/mother_id/father_id/food/rest/
joy/learning/map_id`；无父母/joy/learning 时为 null，不补 0。实测独立 Baby
Human75478：stage Baby、age 0、food 0.1、rest 0.9488、carried false，父母/
joy/learning null（`Reverse/evidence/bt11h-childcare-read-20261111.json`，自审
[照护读取](../references/reviews/2026-11-10-bt11h-childcare-read.md)）。


## 生育策略

`GET /api/v1/dlc/biotech/pregnancy-approach?pawn_id=315` 列 Lover/Spouse/Fiance
的 `approach` 与 `chance_factor`；`POST` `{pawn_id,partner_id,approach}` 接受
Normal/AvoidPregnancy/TryForBaby，调用原生 `SetPregnancyApproach` 并双向写回。
实测 Human315/Human321：AvoidPregnancy 0.25→TryForBaby 4.0、双方一致
（`Reverse/evidence/bt11h-pregnancy-approach-20261111.json`，自审
[生育策略](../references/reviews/2026-11-10-bt11h-pregnancy-approach.md)）。
边界：恋人/Human/Avoid→TryForBaby；Spouse/Fiance、实际妊娠未验。

儿童学习：7 岁 Child 的 `learning` 返回真实值（样例 0.5），baby 为 null
（`Reverse/evidence/bt11h-child-learning-20261111.json`，自审
[儿童学习](../references/reviews/2026-11-10-bt11h-child-learning.md)）。只读，不是学习控制。

母乳拒绝：移除 Lactating → 422 `MomNotLactating`；baby food 0.7 → 422
`BabyTooFull`（`Reverse/evidence/bt11h-breastfeed-refusals-20261111.json`，自审
[母乳拒绝](../references/reviews/2026-11-10-bt11h-breastfeed-refusals.md)）。

非 Baby：`autofeed`/`carry` 以成人 id 作 baby_id 均 422
`baby must be a spawned living baby`（`Reverse/evidence/bt11h-nonbaby-refusal-20261111.json`，
自审 [非 Baby](../references/reviews/2026-11-10-bt11h-nonbaby.md)）。


## 学校正常授课（BT-R12）

V 组合验证：现有 `POST /builder/blueprint`（建造）、
`POST /colonist/work-priority`（Childcare/Construction）、
`POST /colonist/time-assignment`（时间表）、`POST /map/areas` +
`POST /map/areas/cells` + `POST /pawn/area`（区域）可以排出学校；
授课本身由原生 `LearningGiver_Lessontaking`、`SchoolUtility.FindTeacher/
ClosestSchoolDesk`、`WorkGiver_Teach` 和 `JobDriver_Lessontaking/Lessongiving`
执行，没有新增学习控制端点。

2026-11-13 隔离 `g35_deathresting` 副本 run
`tools/cua/.runtime/bt-r12-20261112-run10/`（tick 76787 起）：

- fixture 生成一名原生 7 岁 Child、把 learning need 置 0.3 并选中原生
  `Lessontaking` desire（正常为随机抽取，前置披露），在清空学校区域预置一张
  原生 `SchoolDesk`。
- runner 用现有 API：教师 `Childcare=1`、child 全 24 小时 `Anything`、
  创建 `BTSchool` 区域并只限制 child、教师全 24 小时 `Work`。
- tick 76842 child `Lessontaking` 与教师 Human318 `Lessongiving` 在同一
  `SchoolDesk75481` 配对；tick 76884 教师到达并选择 `taught_skill=Animals`；
  随后 702 ticks learning 0.300→0.3069、Animals XP +61.69、教师 Social XP
  同步增长。
- 证据 `tools/cua/.runtime/bt-r12-20261112-run10/evidence/bt-r12.json`，
  runner `tests/parallel/biotech/bt_r12_school.py`，fixture
  `tests/parallel/biotech/BtFixture.cs`（`prepare_school`/`observe_school`/
  `force_lesson_desire`）。
- 未测：黑板设施、多学生、desire 随机与切换、课桌实际建造（fixture 预置）、
  睡眠/进食打断、无教师/椅子被占用。
