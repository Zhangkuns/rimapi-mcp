# Biotech 污染读取与清理（BT-12d）

`GET /api/v1/dlc/biotech/pollution?map_id=0` 只读当前地图：

```json
{
  "map_id": 0,
  "polluted_count": 1,
  "pollution_percent": 0.0000167,
  "polluted_cells": [{"x":165,"z":130}],
  "pollution_clear_area_cells": 1,
  "pumps": [],
  "atomizers": []
}
```

- `polluted_cells` 最多返回 500 格；`polluted_count` 是全图总数。
- `pollution_clear_area_cells` 来自原生 `Area_PollutionClear`；pumps/atomizers 是
  同图 `CompPollutionPump` 与 `WastepackAtomizer` 的 ThingID。

## 污染泵（BT-12d）

`GET pollution` 的 `pumps` 列出同图 `CompPollutionPump` ThingID，只读；泵由
原生 `CompTick` 每 15000 tick 清一格并在每 6 次后生成 `Wastepack`。

实测 fixture 供电 `PollutionPump75478`、6 格污染、把泵 interval 缩短到 100：
tick 77475 六格全清、wastepack 0→1、pump 仍在 `pumps`
（`Reverse/evidence/bt12d-pollution-pump-20261111.json`，自审
[BT-12d 泵](../references/reviews/2026-11-10-bt12d-pollution-pump.md)）。
边界：只测一个泵/6 格；满仓/断电/取消/自动装载/搬运未验。

## 清一格

不新增清理范围端点，复用内建 area 和通用 job：

1. `POST /map/areas/cells`
   `{"map_id":0,"area":"Pollution clear","cells":[{"x":165,"y":0,"z":130}],"value":true}`
2. `POST /pawn/job`
   `{"pawn_id":321,"job_def":"ClearPollution","target_position":{"x":165,"y":0,"z":130}}`
3. 原生 `JobDriver_ClearPollution` 完成 5600 work 后清除该格并生成 `Wastepack`；
   再读 `GET pollution` 确认格已不在列表。

## 实测（BT-12d，2026-11-10）

- 1 格污染→area 1→`ClearPollution`→tick 83634 `polluted:false`、total 0、
  area 仍 1；未知 map 404（`Reverse/evidence/bt12d-pollution-20261111.json`，
  自审 [BT-12d](../references/reviews/2026-11-10-bt12d-pollution-clear.md)）。
- 边界：只测单格；`Wastepack` 搬运/`WastepackAtomizer` 消耗、污染泵、多格/取消
  未验，BT-12d 仍 partial。

## 废物读取与原子分解（BT-12d）

`GET /api/v1/dlc/biotech/waste` 读同图原子分解器和废物包：

```json
{
  "map_id": 0,
  "atomizers": [{
    "atomizer_id": "WastepackAtomizer75478",
    "power_on": true,
    "auto_load": true,
    "content_count": 0,
    "fill_percent": 0.0,
    "space_left": 10,
    "ticks_left": 0,
    "contents": []
  }],
  "wastepack_count": 2,
  "wastepacks": [{"thing_id": "Wastepack75483", "stack_count": 1, "forbidden": false}]
}
```

`POST /api/v1/dlc/biotech/waste/atomize`

```json
{"pawn_id":"321","atomizer_id":"WastepackAtomizer75478","dry_run":true}
```

- 原子分解器必须同图、供电、未满，且有可达 wastepack；否则 409/404。
- 提交原生 `JobDefOf.HaulToAtomizer`（`targetQueueB` 为候选 wastepack），
  搬运、入容器和 `CompAtomizer` 分解 tick 均由原生完成；服务不直接删除废物。
- 实测 2 个 wastepack→供电 atomizer：tick 76853 content 0、wastepacks 0；
  未知 atomizer 404、无废物 409（`Reverse/evidence/bt12d-waste-atomizer-20261111.json`，
  自审 [BT-12d 废物](../references/reviews/2026-11-10-bt12d-waste-atomizer.md)）。
- 边界：污染泵、满仓/断电/取消、多格/大量搬运、自动装载关闭未验；整单元
  仍 partial。
