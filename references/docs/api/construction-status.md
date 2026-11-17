# 施工计划查询

维护：Codex；核对：2026-11-16；状态：已实现并通过统一隔离验证；正式DLL未部署。

`GET /api/v1/builder/constructions?map_id=0` 读取整张地图；传齐 `min_x/min_z/max_x/max_z` 则读取矩形，例如：

```text
GET /api/v1/builder/constructions?map_id=0&min_x=125&min_z=141&max_x=149&max_z=153
```

端点在游戏主线程遍历该地图当前已生成的 `Blueprint` 和 `Frame` 原生对象。没有派系、可见性、建筑类别过滤，没有分页或静默截断。矩形两角顺序可颠倒，坐标含端点；对象的实际占地与矩形有交集即纳入，每个对象ID只出现一次。

## 响应

- `map_id/game_tick/scope/rectangle`：本次快照的地图、tick与规范化范围。
- `complete:true`：指定范围的当前Blueprint/Frame已完整枚举；不是“全部工程已建成”。空范围成功返回 `count:0/items:[]/complete:true`。
- `count/blueprint_count/frame_count/items`：总数、两个阶段数量及全部对象。
- 每项 `thing_id/thing_number/def_name` 是实际蓝图或施工架身份；`target_def` 是最终建筑、地板或植物的定义，`target_kind` 区分三者。
- `stage` 为 `blueprint/frame`；`construction_kind` 为 `build/install/reinstall`。按真实类型判断，不靠名称前缀。成品不在本列表中。
- `position/rotation/occupied_rect/stuff_def/faction_id/forbidden` 给出位置、实际占地、材料、派系和禁用状态。`source_thing_id` 仅安装/重装时指向已有对象。
- `materials[]` 每种材料含 `def_name/required/delivered/remaining`。交付量只统计Frame已收进原生resourceContainer的材料，不把运输途中或全图库存当作已交付；蓝图交付量为0。
- `work.done/total/remaining/progress_fraction` 读取Frame原生 `workDone/WorkToBuild`；建造蓝图为0与原生预计工作量。总工量为0时fraction为null，避免用NaN或100%伪装完工。
- 安装/重装消耗已有对象，`materials:[]/materials_complete:null/work:null`；不调用原生Blueprint_Install不适用的材料查询，也不臆造原生安装工作尚未暴露的进度。

参数缺失/格式错误、只传部分矩形坐标返回400；未知地图404；矩形超出地图422；读回异常返回失败，不返回 `complete:true` 的部分结果。

## 与旧查询的区别

`GET /map/things` 是可搬运物资列表；`GET /map/things/radius` 只返回可绘制的Item/Building/Plant，不含Pawn或Blueprint类别。它们不能用来证明没有蓝图。`GET /map/things-at` 可读取一个格子上的蓝图，但需要GET JSON body且只覆盖一格。

`GET /map/plans` 是原生规划画线，不是待施工建筑清单；`POST /builder/blueprint` 是放置施工蓝图，成功提交不代表完工。典型火把蓝图为 `def_name:Blueprint_TorchLamp/target_def:TorchLamp/stage:blueprint`。
