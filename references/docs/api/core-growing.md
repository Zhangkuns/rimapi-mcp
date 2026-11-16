# Core 种植区与建筑种植器管理（C-22）

2026-11-13 新增：

- `GET /api/v1/plant/growing?map_id=&zone_id=`（Zone_Growing，二选一）
- `GET /api/v1/plant/growing?map_id=&thing_id=`（Building_PlantGrower，二选一）
- `POST /api/v1/plant/growing`

只管理**已存在**的 `Zone_Growing` 与 `Building_PlantGrower`；现有
`POST /api/v1/map/zone/growing` 创建路由与钓鱼策略保持不变，不新增 create/钓鱼路由。

## 读取

```json
GET /api/v1/plant/growing?map_id=0&zone_id=0
{
  "map_id": 0, "kind": "zone", "zone_id": 0, "thing_id": null,
  "label": "C22 Zone",
  "cells": [{"x":156,"z":121}, ...], "cell_count": 9,
  "plant_def": "Plant_Potato",
  "allow_sow": true, "allow_cut": true, "can_accept_sow": true,
  "valid_plants": [{"def_name":"Plant_Rose","label":"rose",...}],
  "plants": [{"thing_id":"Plant_Rose...","def_name":"Plant_Rose",
    "growth":0.0,"harvestable":false,"blighted":false,"sown":true,
    "position":{"x":158,"z":121}}],
  "season_note": "..."
}
```

- zone：精确 `cells`（x,z）、`cell_count`、`label`、`plant_def`、
  `allow_sow`/`allow_cut`、`can_accept_sow`、`valid_plants`、实际 `plants` 摘要。
- grower：精确 `OccupiedRect` cells、`thing_id`/`def_name`、`plant_def`、
  `can_accept_sow`（原生 `CanAcceptSowNow`）、`valid_plants`、`plants`；
  `allow_sow`/`allow_cut` 为 null（该类型无此策略）。
- `valid_plants`：原生 `PlantUtility.ValidPlantTypesForGrowers` +
  `PlantUtility.CanSowOnGrower`，再按可见的科研 `sowResearchPrerequisites`
  与 `mustBeWildToSow` 过滤；`Command_SetPlantToGrow.IsPlantAvailable` 为 private，
  只复刻可见部分（永久黑暗资格在当前引用程序集不可见，已披露）。
- `can_accept_sow` 为原生状态（zone 的 allowSow / grower 的 `CanAcceptSowNow`），
  不等于季节投影。

## 写入

```json
POST /api/v1/plant/growing
{"map_id":0,"zone_id":0,"action":"set_crop","plant_def":"Plant_Rice"}
```

- `set_crop`（zone/grower）：
  - 原生 `CanSowOnGrower`（sowTag、`PollutionUtility.CanPlantAt`）；
  - 可见科研/野生资格；
  - `interferesWithRoof` 与目标 cell `Roofed` 不兼容时 409；
  - 所有目标 cell 必须满足原生 `PlantUtility.GrowthSeasonNow`；
  - zone 每个 cell 还必须通过原生 `CanEverPlantAt`（肥力/地形/光照；允许先清掉
    非树植物）；
  - 通过 `IPlantToGrowSettable.SetPlantDefToGrow` 写入；不直接越过资格写私有字段。
- `set_policy`（仅 zone）：`allow_sow` / `allow_cut` 至少一个，写原生字段并读回。
- `resize`（仅 zone）：`add_cells`/`remove_cells` 精确 cell 列表；校验
  InBounds、加 cell 不在本区且不属于其它 zone、删 cell 属于本区、最终不为空；
  先全量校验再 `Zone.AddCell/RemoveCell`；读回精确 cells。
- `rename`（仅 zone）：非空、trim、长度 ≤64，写 `Zone.label`。
- `delete`（仅 zone）：原生 `Zone.Delete()`，响应 `zone_exists_after`；不 broad-delete。
- `dry_run` 不写；任何失败在写前返回 400/404/409，不部分应用。
- POST 合同 `command/pawn_ai/validate`，**Idempotency=none**（resize/rename/delete
  非幂等，不登记 game_memory）；GET 为 observe。

## 原生来源与边界

- `Zone_Growing`：`PlantDefToGrow`/`SetPlantDefToGrow`、`allowSow`、`allowCut`、
  `Cells`、`Zone.AddCell/RemoveCell/Delete`。
- `Building_PlantGrower`：`IPlantToGrowSettable`、`Get/SetPlantDefToGrow`、
  `CanAcceptSowNow`、`OccupiedRect`、`PlantsOnMe`。
- 未测：跨 zone 并发编辑、全部 plant def、屋顶/季节极端、hydroponics 断电、
  多地图、1.5；C-22 不覆盖创建（保留既有 create）与钓鱼区。

## 证据

隔离 g35 tick 76787，五 DLC；fixture `tests/parallel/core-growing/C22Fixture.cs`
创建空的 native `Zone_Growing`（9 格）、native `PlantPot`，并设置一名可种植
殖民者的 Growing 优先级；fixture 只布置，从不执行 set_crop/policy/resize/rename/delete。

- GET zone 9 格/Plant_Potato/allowSow+allowCut/can_accept_sow/valid_plants；
  GET grower 1 格/Plant_Daylily/valid_plants/plants 只读。
- set_policy 关/开、set_crop dry（不写）+真实（Plant_Rose）、读回；实际推进后
  worker `Sow` 并出现 `Plant_Rose`，真实种植终态。
- resize +1/-1 精确 cell，越界/同区占用/缩到 0 4xx；rename 读回；delete 后
  GET 404；grower 合法作物 set/readback，grower policy/delete 4xx。
- 证据：`tools/cua/.runtime/c22-run14-20261113/evidence/c22.json`（phase passed，
  使用主工作树集成 runner `RIMAPI/tests/c22_growing.py`）。
