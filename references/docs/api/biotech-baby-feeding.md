# Biotech 婴儿自动喂食者与允许食物（BT-R08）

2026-11-12 新增：`GET/POST /dlc/biotech/baby/feeding` 读写
`Pawn_MindState.SetAutofeeder/AutofeedSetting` 与
`Pawn_FoodRestrictionTracker.SetBabyFoodAllowed/BabyFoodAllowed`，即原生
`ITab_Pawn_Feeding` 的自动喂食下拉与食物勾选。

## 读取

`GET /api/v1/dlc/biotech/baby/feeding?baby_id=Human75478` →
`developmental_stage/can_autofeed/feeders[{feeder_id,mode}]`。
`feeders` 只列非 `Never` 的原生设置；`Childcare` 是原生默认值。
策略读写要求 baby 当前 spawned：原生 `BringBabyToSafety` 抱起期间返回
422 `invalid_target`，落回地图后可再读。

## 设置

```json
{"baby_id":"Human75478","feeder_id":"Human321","mode":"Urgent","dry_run":true}
{"baby_id":"Human75478","food_def":"Milk","allowed":false}
```

- `mode` 为 `Never|Childcare|Urgent`，与 `feeder_id` 同时给；
- `food_def` 必须是 `ITab_Pawn_Feeding.BabyConsumableFoods` 中的原生食物，
  与 `allowed` 同时给；至少一组。
- 返回 `feeders_before/mode_before/allowed_before` 和 `applied`；dry_run 不写。
- 合同：GET observe；POST policy/game_ai/validate/game_memory。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 age0 Baby（Human75478）与喂食者 Human321，选 `Milk`；初始
feeders 含多名 Childcare；dry 保持；设 Urgent 后 GET/fixture 均 Urgent；
Milk false 后读回 false；再设 Never。
证据 `tools/cua/.runtime/bt-r08-20261112/evidence/bt-r08.json`，runner
`tests/bt_r08_baby_feeding.py`，fixture `prepare_baby`/`observe_baby`。

2026-11-13 自主喂养遵从（`bt-r08-20261113-run6`，同一 g35 副本 tick 76787 起，
port 8766）：
fixture 生成玩家 Baby Human75478、选成年非血原质照护者 Human321 放在旁边，
把 baby 食物需求置 1%（原生 `Need_Food` 上限 0.125）、生成 20 个 `BabyFood`
并**关闭所有自由殖民者的原生 Childcare 工作类型**（该 work type 的
`WorkGiver_BottleFeedBaby` 与每照护者策略无关，会干扰隔离）。随后只走现 policy
端点：

- `mode=Urgent` + `BabyFood` 允许：tick 76800 原生 think tree
  `JobGiver_Autofeed` 给 Human321 `BottleFeedBaby`（targetA=baby、
  targetB=该 BabyFood 堆），tick 80970 喂饱：baby food 0.01→0.11（0.125 上限的
  88%）、该堆 20→18、baby 不再 hungry；随后原生 `BringBabyToSafety` 抱起，
  落回地图后 GET 读回仍 Urgent。全程无 `childcare/feed|autofeed|breastfeed` 调用。
- `mode=Never` 后再推进 2500 tick（baby 仍饿、同图仍有可用 `BabyFood`）：
  该照护者及任意殖民者均无 `BottleFeedBaby` job，baby food 不变，
  食物堆 20 不变；期间仅有 Clean/GoForWalk 等无关决策。
- `mode=Urgent` 但唯一可选食物 `BabyFood` 设为 `allowed=false` 后推进 2500 tick：
  仍无任何 `BottleFeedBaby` job、baby food 不变、食物堆 20 不变，说明食物策略
  独立于照护者模式抑制了喂养工作。

证据 `tools/cua/.runtime/bt-r08-20261113-run6/evidence/bt-r08.json`；runner
`tests/parallel/biotech/bt_r08_autofeed.py`；fixture
`prepare_autofeed`/`stage_autofeed`/`observe_autofeed`。收口 `desktop.py stop`
报告 source_save/formal_dll/global_prefs unchanged。

- 未测：`Childcare` 模式的低优先级工作语义（本行关闭该 work type 以隔离策略）、
  多照护者/多婴儿组合、囚犯/客人婴儿、其他 `babiesCanIngest` 食物、母乳路径、
  存档读回。
