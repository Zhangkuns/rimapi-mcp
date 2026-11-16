# Odyssey 动物特殊训练（OD-07b）

`GET/POST /api/v1/dlc/odyssey/animal/training` 覆盖原生动物训练复选框与强制训练
job，用于 Forage 等 `specialTrainable`。读取只投影原生
`Pawn_TrainingTracker`；写入走 `SetWantedRecursive`，训练由原生
`JobDriver_Train`→`TryTrain`→`Train()` 完成。

## 读取

`GET /api/v1/dlc/odyssey/animal/training?animal_id=Gorilla151340`

```json
{
  "animal_id": "Gorilla151340",
  "name": "Gorilla 1",
  "race": "Gorilla",
  "trainability": "Advanced",
  "trainables": [{
    "def_name": "Forage",
    "label": "Forage",
    "special": true,
    "required_trainability": null,
    "steps": 3,
    "current_steps": 2,
    "wanted": false,
    "learned": false,
    "can_assign": true,
    "visible": true,
    "reason": null
  }]
}
```

- 只列 `RaceProps.specialTrainables`，不把普通 Tameness/Obedience 当本接口目标。
- `current_steps` 通过原生私有 `steps` DefMap 只读投影；`can_assign`/`visible` 来自
  `CanAssignToTrain`（含 Odyssey specialTrainable 的种族过滤）。

## 设置/训练

`POST /api/v1/dlc/odyssey/animal/training`

```json
{"animal_id": "Gorilla151340", "trainable": "Forage",
 "wanted": true, "train_now": true, "trainer_id": "Human88481", "dry_run": false}
```

- `wanted` 写原生 `SetWantedRecursive`（与训练复选框相同；会递归补齐前置）。
- `train_now:true` 额外提交原生 `JobDefOf.Train`；trainer 必须是同图、可对话、
  Animals>0、可达、背包内有 `RawTasty` 或更差生食的玩家殖民者。显式训练会打断
  trainer 当前非玩家可中断的 joy/工作 job（`EndCurrentJob` 后 `StartJob`），这是
  “玩家强制训练”的语义。
- 不直接写 `steps`/`learned`；训练结果由原生 job 的概率、动物野性、羁绊和
  `TrainAnimalChance` 决定。
- `dry_run` 只返回 before，不写 wanted、不提交 job。
- 失败：未知动物/训练项 404；非玩家动物或不可见训练项 422；无合适 trainer 409。

## 正常调用顺序

1. `GET .../animal/training` 确认 `Forage` 的 `can_assign/visible` 与 `steps`。
2. `POST .../animal/training` 设 `wanted:true`，可带 `train_now:true`。
3. 等待原生 Train job 完成（约 4×270 tick + 行走/喂食）；重新 GET 读
   `learned:true`/`current_steps==steps`。
4. 后续觅食是动物 AI 行为，不由本接口强制。

## 证据与边界

- 2026-11-10 实测：Gorilla151340 Forage 2/3，wanted false；`train_now` 提交
  `Train` 给 Human88481（Animals 10、RawBerries 30、羁绊），tick 78533 原生
  `Train()` 后 `learned:true`、`current_steps:3`，食品 30→20；未知动物/训练项/
  殖民者 404（`Reverse/evidence/od07b-forage-20261111.json`，自审
  [OD-07b](../references/reviews/2026-11-10-od07b-forage.md)）。
- 未覆盖：其它 specialTrainable（Dig/AttackTarget 等）、非 Gorilla 种族、训练失败
  概率分支、Forage 实际觅食产出、武器附加能力。
