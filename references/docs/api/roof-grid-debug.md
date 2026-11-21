# 即时设置和清除屋顶（ROOF-D01）

`POST /api/v1/debug/native/execute` 的 `roof.set` / `roof.clear` 直接修改地图上的实际屋顶。适用于调试，需要活动存档与开发者模式，无需 DLC。加载对应 DLL 后，通过 `GET /api/v1/debug/native/actions` 确认动作可用。

```json
{"action":"roof.set","map_id":0,"def_name":"RoofConstructed","cells":[{"x":100,"z":100},{"x":101,"z":100}],"dry_run":true}
```

```json
{"action":"roof.clear","map_id":0,"point_a":{"x":100,"z":100},"point_b":{"x":101,"z":101}}
```

- `map_id` 必填，使用 `GET /api/v1/maps` 返回的地图 ID。
- 目标二选一：非空 `cells` 格子列表，或 `point_a` / `point_b` 两个矩形角点。每点明确给出整数 `x`/`z`。矩形包含边界，角点顺序不限；重复格只处理一次。任何点越界都在写入前拒绝整个请求。
- `roof.set` 必须明确给出 `def_name`，现有屋顶可被替换。用 `GET /api/v1/defs?type=RoofDef` 查询，例如 `RoofConstructed`。`roof.clear` 不接受 `def_name`。
- `dry_run:true` 读当前状态但不写入；正式执行时去掉它或设为 `false`。可设 `expected_process_id`，用 `Idempotency-Key` 在同一个 Game 中重试，见[通用调试合同](../debug-native-actions.md)。

`before` / `after` 为选中格子的 `{x, z, roof_def}` 列表，`roof_def:null` 表示无屋顶。`cell_count` 是去重后的选中格数，`would_change_count` 是执行前与目标不同的格数。预演和所有格已符合目标时，`after` 等于 `before`，且 `applied`、`changed`、`native_invoked` 均为 `false`；实际写入并通过读回才发出 `operation` 事件。原生异常或读回不符返回 500 和 `outcome:"uncertain"`，尽可能附上已发生变化的格子读回。

参数缺失、坐标格式不全或选择方式冲突返回 400；关闭开发者模式返回 403；地图或 `RoofDef` 不存在返回 404；游戏未就绪或进程不符返回 409；目标越界返回 422。

动作调用原版开发者菜单「Edit roof (rect)」使用的 `RoofGrid.SetRoof`，不经过小人工作，也不会编辑 `BuildRoof` / `NoRoof` 区域或主动清场。实际编辑会触发原生屋顶通知；结构支撑、后续模拟及受封闭状态影响的玩法仍由游戏处理。正常划区和施工请走[区域合同](core-areas.md)。
