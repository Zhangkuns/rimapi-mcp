# Core 涂装与计划管理（C-20）

2026-11-13 新增。只写原生 Paint 设计ation 与 `PlanManager`；纯显示 overlay
不单独立业务。

## 涂装

`GET /api/v1/map/paint?x=&z=&thing_id=`：地图/合法 `ColorType.Structure`
颜色、floor terrain/paintable/floor_color、building paintable/paint_color、
Paint/Remove 设计ation；floor 需 x/z，building 需 thing_id；非 paintable/
非建筑 422，未知 404。

`POST /api/v1/map/paint`：
```json
{"x":156,"z":145,"target":"floor","action":"paint","color":"Structure_Red"}
{"thing_id":"Wall...","target":"building","action":"remove"}
```
只加/替换 `DesignationDefOf.PaintFloor/RemovePaintFloor/PaintBuilding/
RemovePaintBuilding`；校验 bounds/paintable/Structure 颜色；dry 不写；API 不写
terrain/building 最终颜色，真实施工由原生 work giver 消耗 `Dye`。

## 计划

`GET /api/v1/map/plans`：`PlanManager.AllPlans` 每个 plan 的 plan_id/label/
color/cell_count/exact cells；`plan_colors` 为 `ColorType.Planning` defNames。

`POST /api/v1/map/plans`：`action` create/add_cells/remove_cells/rename/
recolor/copy/delete；原生 `new Plan/AddCell/RemoveCell/Delete/RenamableLabel`；
copy 用原生 Plan 新建并复制 cells（可 offset），不靠 overlay；cells 需地图内、
去重；label 非空；颜色 Planning；未知 plan 404；dry 不建/不改。

## 证据与边界

2026-11-13 隔离 g35：fixture 铺可涂 PavedTile、5 Dye、有 Painting 工作的
殖民者、非涂装墙。
- 涂装：dry 不写；paint designation=Structure_Red；有界推进后
  floor_color=Structure_Red、Dye 5→4（真实施工消费）；remove 后颜色 null。
- 计划：初始 0；create Plan A（2 格）→ add 3 → remove 2 → rename →
  recolor → copy（2 plans）→ delete（1 plan）exact 读回。
- 4xx：非法 paint 颜色/越界 422；未知 plan 404；非法 plan 颜色/重复 cells/
  越界 422。
- `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。
- 未测：building 涂装真实施工与 Dye 消费（只验证非涂装墙 422）、计划旋转
  复制/多色 overlay 显示。fixture 只布置/披露。
