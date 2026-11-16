# Odyssey 局部真空环境读取（OD-05a）

`GET /api/v1/dlc/odyssey/environment?map_id=&x=&z=` 返回**准确格/房间/人物**的真空状态，
不再用地图中心格代表整船。只读；不发明氧气库存，也不改 `vacuumGrid`。

```json
{"map_id":2,"layer_id":1,"is_space":true,
 "cell":{"x":120,"z":130},"vacuum":1.0,"roofed":false,
 "room_id":37,"room_role":"None","room_vacuum":1.0,"room_open_roof_cells":24,
 "pawns":[{"pawn_id":"Human321","x":120,"z":130,"vacuum":1.0,
           "vacuum_resistance":0.0,"roofed":false,"room_id":37,"room_vacuum":1.0,
           "protective_apparel":0}],
 "rooms":[{"room_id":37,"role":"None","cells":80,"vacuum":1.0,
           "open_roof_cells":24,"doorway":false,"outdoor_temperature":false}]}
```

- 真空值来自原生 `IntVec3.GetVacuum(map)`（`Room.Vacuum`，非 Odyssey 或从未进真空时为 0）。
- 人物行同时给 `VacuumResistance` 原生 stat 与随身真空防护装备数量。
- 房间行来自 `map.regionGrid.AllRooms`，含开放屋顶格数与是否通向室外温度。

## 证据与边界

- 2026-11-10 运行：`g40_maintained` 副本经生产 takeoff/land 到 layer1 平台地图 2；
  密封 DiningRoom 140 `(91,92)` 读回 vacuum 0.0/roofed/90 格/无开放屋顶，户外
  `(5,5)` 读回 vacuum 1.0/room 134/47526 开放屋顶格；引擎格 vacuum 1.0；人物
  `Human88481` vacuum 1.0/VacuumResistance 0/防护 0；map 404、越界 422、坏 query 400
  （`Reverse/evidence/od05a-environment-20261111.json`，自审
  [OD-05a](../references/reviews/2026-11-10-od05a-environment.md)）。
- 后续OD-05b/d已有原生建造/屋顶/供电和正常穿戴防护证据，见[主计划](../exec-plans/active/five-dlc-remaining.md)。
  旧OD-05c runner在rescue复活/清暴露之后判通过，该原文件保留但不能证明自然恢复。
- 本轮修runner，要求rescue之前准确对象alive/spawned、暴露真实下降；新健康对象
  Human185704在API关闭屏障电源后暴露，复电后由原生泵复压：tick140272 vacuum0.914966、
  exposure0.156598628；tick141952 vacuum0.489795923、exposure0.132857144，alive/spawned、未倒地。
  暴露在复压初期继续上升到0.5328571，降到原生阈值后再下降；不以房间压强代验人物健康。
  仍有Hypothermia与轻暴露，只声明开始恢复，不声明完全治愈。
- 新样本复用复制建造checkpoint，fixture补屏障/屋顶/燃料作为前置，并生成健康测试对象；
  此后不复活、不删hediff、不写vacuum。API电源切换、泵和健康效果是实际验收对象，
  不冒充本轮重新正常建造。见[修复自审](../references/reviews/2026-11-11-five-dlc-gap-repair.md)。
- 边界：不模拟氧气扩散/泵；`Room.Vacuum` 是原生状态，读取不证明密封或伤害后果。
