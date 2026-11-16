# Odyssey 星球图层读取（G41）

维护：直接实施Codex；核对：2026-11-08；状态：身份/连接、代表保存读取与Cua视图选择限定通过。

`GET /api/v1/dlc/odyssey/layers`保留layer_count、surface_layer_id、orbit_layer_id、
current_map_layer_id/current_map_tile_id、defs/def_count和旧layers字段。
每层新增load_id、connections（origin_layer_id/target_layer_id/fuel_cost）。连接有方向，
连接的是整层；没有同号tile自动配对规则。无连接返回空数组。

根级selected_layer_id、world_view、selected_world_tile报告当前世界UI选择；后者为空或
准确layer_id/tile_id。它们与current_map_*不同：选择轨道不表示殖民地或人物已迁移。

`GET /api/v1/dlc/odyssey/layers/tile?layer_id=0&tile_id=137061&target_layer_id=1`

layer_id/tile_id必须显式为整数。返回准确layer_id/tile_id、layer_def、tile_type、biome_def、
世界center{x,y,z}及在该格子的已加载map_ids。可选target_layer_id增加direct_connection、
connection_fuel_cost（无直接连接为null）和原生GetClosestTile的closest_tile（无有效结果为null）。
最近格子由原生空间查询决定，不保证可定居、可降落或已经生成地图。

无Game409；缺Odyssey422；缺/非整数字段400；未知层或该层越界格子404。新旧读取均为
`observe / none / dlc:odyssey`，不写Game、推进时间或移动人物。当前普通读取不走GET缓存。

原生Tile公开PlanetTile身份；层索引对应tileId。数字只在当前世界/层内有意义，调用者必须
同时保存layer_id和tile_id，不能承诺跨不同世界稳定。

原生视图切换已由Cua点击View orbit/View planet验证；API请求定位继续复用
`POST /api/v1/ui/targets/focus`的world_tile/layer参数与既有UI所有权规则，底层CameraJumper。
不新增同义切层路由。世界建层在Scenario/世界初始化发生，不提供活动Game任意重建世界。

[本组证据](../references/reviews/2026-11-08-g41-g42-codex-identity.md)：Surface0/295732格、
Orbit1/1252格，0→1和1→0 fuel0；map0为0/137061，其轨道最近格子1/478。两进程保存读取
身份一致，真实UI选择0→1→0且地图位置不变。跨层人物/船迁移交G40，尚未验收。
旧只读证据保留于`Reverse/evidence/dlc-G-40-42-engine.json`和`dlc-G-40-42-empty.json`。
