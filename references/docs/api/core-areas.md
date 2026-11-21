# Core 区域、屋顶、Home 与自动重建（C-21）

2026-11-13 增强。只改原生 `AreaManager` 区域格集/标签/颜色与
`PlaySettings` 开关；不直接写 roof grid、不伪造工作。

正常建屋顶可以用 `POST /api/v1/order/designate/area` 配 `type:"roof"`、`map_id`、`point_a`、`point_b`，或者从 `GET /api/v1/map/areas` 取 `kind:"BuildRoof"` 的实际 `label`，再用 `POST /api/v1/map/areas/cells` 修改指定格。划区后由小人施工。需要在调试中即时读写实际屋顶时，使用独立的[屋顶调试动作](roof-grid-debug.md)。

## 读取（增强既有 GET，不加行）

`GET /api/v1/map/areas?map_id=`：每个原生 Area 返回
`area_id`/`label`/`renamable_label`/`kind`（Home/BuildRoof/NoRoof/Allowed）/
`cell_count`/exact `cells`/`color`(#RRGGBB)/`assignable`/`mutable`/`is_home`。

## 编辑

`POST /api/v1/map/areas/edit`
```json
{"map_id":0,"area_id":3,"action":"rename","new_label":"C21"}
{"map_id":0,"area_id":3,"action":"set_color","color":"#FF0000"}
{"map_id":0,"area_id":3,"action":"invert"|"clear"|"delete"}
```
- 只允许 `Area_Allowed`；Home/BuildRoof/NoRoof 等特殊区域只能走既有
  `/map/areas/cells`，edit 422。
- `area_id` 优先；label 仅作唯一回退，重复/歧义 409。rename 唯一；set_color
  校验 HTML 颜色；delete 走原生 `Area.Delete` 并清 pawn 指派；dry 不写。
- 返回 before/after label/color/exact cells；不直接改 roof grid。

## 策略

`GET /api/v1/map/area-policies?map_id=`：`Find.PlaySettings.autoHomeArea`/
`autoRebuild` + Home/BuildRoof/NoRoof 当前格集/计数。
`POST /api/v1/map/area-policies`：`{auto_home_area?,auto_rebuild?,dry_run}`，
至少一项，写原生开关并读回。本安装无 `Area_IgnoreRoof` 类（已披露）；
清雪/污染清理由代表工作原生使用这些区域，API 不直接生成工作。

## 证据与边界（run c21-20261113-run1）

- GetAreas：Home 306 格 exact、BuildRoof/NoRoof 0 格、颜色读回。
- 创建 C21 Area、2 格 exact；rename C21 Renamed、color #FF0000、invert、
  clear 0、delete 后不存在。
- 特殊区域 edit 422；非法颜色 422；未知 area 404；policy 空 400；
  auto flags true/false → false/true 读写回；dry 不写。
- 4xx 与 stop：source_save/formal_dll/global_prefs unchanged。
- 未测：BuildRoof/清雪/污染清理代表工作的实际执行（只读格集）；区域 label
  歧义 409（该存档 label 唯一）；Area_IgnoreRoof 本安装不存在。
