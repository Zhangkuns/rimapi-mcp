# 通用地图与世界目标定位

空间定位与 UI 控件桥接配合使用：API 读回、解析和移动镜头；Cua 发送真实鼠标输入。
接口不调用目标器 action、选择回调或运输业务函数。G29 穿梭机只是调用案例。

## 接口

`GET /api/v1/ui/targets/resolve` 支持以下 query：

| kind | 必填 | 可选 | 定位对象 |
| --- | --- | --- | --- |
| map_cell | x、z | map_id（默认当前地图） | 地图格子中心 |
| thing | thing_id | map_id | 已生成的人物、建筑、物品等 Thing |
| world_tile | tile | layer（默认 0） | 指定星球图层的格子中心 |
| world_object | world_object_id | 无 | 商队、聚落等世界对象的绘制位置 |

返回物理客户端像素 `x/y`（左上原点）、PID、尺寸、帧、UI 缩放、目标身份、
`visible/clickable/reason`、`context_id`、目标器状态以及几何命中候选 ID。
有效目标在其他地图、其他图层、屏外或被遮挡时仍返回数据，但 `clickable:false`；
无效坐标/类型返回 400、对象不存在 404、无游戏或过期上下文 409。
首次读需要等 UI Repaint；`reason:ui_not_ready` 时重读。

`POST /api/v1/ui/targets/focus` 接受相同字段的 JSON，另可带 `expected_process_id`。
只请求镜头跳转，不能把响应视为镜头已稳定；后续重新 resolve。
有目标器时拒绝可能取消该流程的跨地图/视图/图层跳转。
切换镜头不选择对象，也不自动执行旅行。

点击前把上次的 `context_id` 传回 resolve，验证对象身份、视图、实际像素位置、原生命中与遮挡仍一致。
坐标按四分之一物理像素归一化，忽略不会改变命中的亚像素抖动；无关 HUD 文本更新不使空间目标失效。
解析与外部输入之间仍存在时间间隔；移动中的目标宜暂停后点击，操作后验证实际业务结果。

## Cua 用法

```bash
python3 tools/cua/targets.py --pid PID --window WINDOW resolve --kind map_cell --x 130 --z 90 --map-id 0
python3 tools/cua/targets.py --pid PID --window WINDOW click --kind thing --thing-id 66797 --focus
python3 tools/cua/targets.py --pid PID --window WINDOW click --kind world_tile --tile 137060 --layer 0 --focus
python3 tools/cua/targets.py --pid PID --window WINDOW click --kind world_object --world-object-id OBJECT_ID --focus
```

默认 `--base http://localhost:8765`；可用 `RIMAPI_TOKEN` 配置 Bearer。
首次/窗口几何变化沿用 semantic.py 的 Cua 截图校准；后续按 API 坐标点击。
CLI 拒绝跨 PID、非有限坐标、遮挡/屏外和过期上下文；不自动重发输入。
输出保存 API 原始请求/响应、Cua 输入与后续观察；点击送达不代表预期效果已成功。

## 边界

- Thing 必须在地图生成；运输容器里的乘员没有地图点击位置。
- 世界格子经过原生射线反查，拒绝星球背面或图层不符的投影。
- 候选 Thing 为命中格子占用者及附近人物；散落物品精细选择、第三方自绘图标、
  世界展开图标和目标器 validator 仍以原生实际输入结果为准。
- 世界对象重叠可能打开选择菜单；不能把对象坐标当作唯一选择保证。
- 通用定位没有飞船业务依赖；控件按钮仍使用 `semantic.py`。
