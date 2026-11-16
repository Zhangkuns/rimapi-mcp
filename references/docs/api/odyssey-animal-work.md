# Odyssey 动物觅食/挖掘策略（OD-R03）

2026-11-12 新增：读取/写入原生 `Pawn_PlayerSettings.animalForage` 与
`animalDig`（动物表 `PawnColumnWorker_AnimalForage`/`AnimalDig` 的复选框）。
工作仍由动物原生 think tree 决定：

- Forage：`ThinkNode_ConditionalTrainableCompleted(Forage)` →
  `ThinkNode_ChancePerHour_Forage` → `ThinkNode_ConditionalAnimalShouldForage`
  （读 `playerSettings.animalForage`）→ `JobGiver_Forage`。
- Dig/Mine：`ConditionalTrainableCompleted(Dig)` →
  `ThinkNode_ConditionalAnimalShouldDig`（读 `playerSettings.animalDig`）→
  `JobGiver_Mine`，需要原生采矿 designation。

## 读取

`GET /api/v1/dlc/odyssey/animal-work?pawn_id=Elephant75478`

```json
{"pawn_id":"Elephant75478","def_name":"Elephant","map_id":0,
 "forage":false,"dig":false,"learned_forage":true,"learned_dig":true,
 "job_def":null,"food_percent":0.25}
```

要求 Odyssey active、pawn 为动物；否则 404/422。`job_def/food_percent` 只作读回。

## 设置

`POST /api/v1/dlc/odyssey/animal-work`

```json
{"pawn_id":"Elephant75478","forage":true,"dig":true,"dry_run":true}
```

`forage`/`dig` 至少一个；只写请求的字段。未学会对应训练时 422（原生复选框同样不显示）；
`dry_run` 不写；返回 `before_forage/before_dig/requested_*/applied` 与完整状态。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 生成一只玩家 Elephant，并用 `Train(..., complete:true)` 完成原生 Forage/Dig
训练（正常需时间，准备披露），初始两开关 false、食物 25%、并生成一个带 Mine
designation 的 Steel 矿。实测：

- dry/Elephant：`forage:false` 保持；正式 on 后 `forage:true`；
  推进后食物 0.25→0.468（自主觅食），`dig:false` 时不动矿。
- `dig:true` 后推进，Steel 矿 HP 1500→1420（自主挖掘），两开关读回 true。
- 再把两开关设 false，读回 false；GET 与 POST 一致。

证据 `tools/cua/.runtime/od-r03-20261112/evidence/od-r03.json`，runner
`tests/od_r03_animal_work.py`，fixture `prepare_animal_work`/`observe_animal_work`。

- 未测：非动物/未训练拒绝、多个动物并发、区域/食物类型限制、挖掘产物落地与
  长时间自主循环；动物训练的时间门槛本身未重跑。
