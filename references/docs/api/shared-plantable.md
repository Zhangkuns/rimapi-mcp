# Shared 种荚/种子指定地格种植与取消（S-08）

2026-11-13 新增：

- `GET /api/v1/plant/plantable?seed_id=`
- `POST /api/v1/plant/plantable`

只操作原生 `CompPlantable` 种子（HarbingerSeed、GauranlenSeed、ArcheanSeed 等）；
不直接生成 Plant，不做 broad plant。Gaumaker pod / Gauranlen 产荚旧证据保留，
不新造生成端点。

## 读取

```json
GET /api/v1/plant/plantable?seed_id=75478
{
  "seed_id": "HarbingerSeed75478", "seed_number": 75478,
  "seed_def": "HarbingerSeed", "stack_count": 2,
  "plant_def_to_spawn": "Plant_TreeHarbinger",
  "cells": [{"x":156,"z":132}], "selected_count": 1,
  "max_cells": 2,
  "can_plant": true, "invalid_cells": []
}
```

- `cells/selected_count`：原生 `CompPlantable.PlantCells` 的精确选择。
- `max_cells`：当前 seed 的 `stackCount`（原生 gizmo 的上限语义）。
- `can_plant/invalid_cells`：按原生 `CanEverPlantAt`（canWipePlantsExceptTree:true,
  checkMapTemperature:false）与 `PlantUtility.AdjacentSowBlocker` 检查；不把
  本 seed 自身选择产生的 min-spacing 误判为不可种。
- 不生成 Plant，只读。

## 写入

```json
POST /api/v1/plant/plantable
{"seed_id":75478,"action":"set_cells","cells":[{"x":156,"z":132}]}
```

- `set_cells` + `cells`：逐格调用原生 `CompPlantable.CanPlantAt`
  （CanEverPlantAt/地形肥力/roof/adjacent/min spacing/PlantGrower），去重、
  数量 ≤ `seed.stackCount`；通过后清空并写公有 `PlantCells`（与原生 gizmo
  同一状态）；读回 before/after。
- `cancel`：清空 `PlantCells`；读回空。
- `plant` + `pawn_id` + `cell`（cell 必填且必须已在选中列表，无 cell 不猜）：
  校验 seed 仍在、`CanEverPlantAt`/adjacent blocker、`CanReserve(seed,1,1)`、
  reach/forbidden；构造 `JobMaker.MakeJob(JobDefOf.PlantSeed, seed, cell)`，
  `plantDefToSow=comp.Props.plantDefToSpawn`，`count=1`，`TryTakeOrderedJob`；
  不直接生成 Plant。推进后读回 seed 消耗/精确格原生 plant。
- `dry_run` 不写/不派 job；错误 400/404/409/422。
- 合同：GET observe；POST command/immediate/validate；`Idempotency=none`
  （set/cancel/plant 非幂等）。

## 原生来源与边界

- `CompPlantable.PlantCells`（公有 list，原生 Cancel gizmo 同状态）、
  `CanPlantAt`、`DoPlant`；`JobDriver_PlantSeed` 使用 targetA=seed、
  targetB=cell、count=1，并检查 `CanEverPlantAt`/adjacent blocker/选中 cell。
- `HarbingerSeed`/`GauranlenSeed`/`ArcheanSeed` 均走同一 `CompPlantable`；Gaumaker
  pod 产物/Gauranlen 产荚历史证据保留，不在本路由重复生成。
- 未测：多 seed stack 同 job、Gauranlen 连接强度/间距全部边界、无 reach 隔离格、
  1.5；不做直接 Plant 生成。

## 证据

隔离 g35 tick 76787，五 DLC；fixture `tests/parallel/shared-plantable/S08Fixture.cs`
生成原生 HarbingerSeed stack2、两个 `CanPlantAt` 合法空地格和一个拒绝格；fixture
不调用产品动作。

- GET seed stack2、Plant_TreeHarbinger、空 cells、max2。
- set_cells dry 不写；单格 set 读回；cancel 清空；重复/超 stack/非法格 4xx；
  未选 cell/无 cell 4xx。
- plant 原生 `PlantSeed` job（seed/cell/count1），推进后 seed 2→1，精确
  `valid_cell` 出现 `Plant_TreeHarbinger`；无直接 Plant 生成。
- 证据：`tools/cua/.runtime/s08-run7-20261113/evidence/s08.json`（phase passed）。
