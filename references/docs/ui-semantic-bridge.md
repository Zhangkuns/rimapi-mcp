# UI 语义桥接与 Cua 输入

RIMAPI 从运行中的原生 IMGUI 绘制读取控件，Cua 向指定游戏窗口发送真实鼠标、键盘和滚轮输入。
定位依赖实际绘制的点击区域，不写死源码坐标，也不直接调用按钮业务回调。
截图用于初次坐标校准、结果检查点及未覆盖界面的人工/视觉定位。

## 使用

先按 [Cua 安装说明](../tools/cua/README.md)启动专用 driver。
游戏须加载包含 `UiSemanticController` 的新 RIMAPI 程序集；旧 DLL 不具备下列两个端点。
在仓库根目录运行，PID 和 WINDOW_ID 从实际进程及 `control.py windows` 取得：

```bash
python3 tools/cua/control.py --pid PID windows
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID list
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID click --label Options
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID click --label OK --owner Dialog_Options
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID key escape
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID snapshot
```

`--base` 默认 `http://localhost:8765`；启用鉴权时从 `RIMAPI_TOKEN` 读取 Bearer token。
`--session` 与 `--output` 支持隔离操作记录。程序不修改全局 MCP/dsh 配置。

`click` 默认精确、不区分大小写匹配可交互控件标签；同名多个控件拒绝。
使用 `--owner`（窗口类型子串或窗口 ID）、`--role` 或 `--id` 明确目标；
`--contains` 才启用标签子串匹配。图标可能只有贴图名，文本框可能无标签，先 `list` 再用 ID。

```bash
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID type --id CONTROL_ID --text hello
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID scroll --id VISIBLE_CONTROL_ID --direction down --amount 1
python3 tools/cua/semantic.py --pid PID --window WINDOW_ID click --id CONTROL_ID --button right
```

`type` 在文本框内真实点击后输入文字，语义是输入而非替换已有内容。
`scroll` 在指定的可见控件处滚动；滚动后重新读取列表，不能点击仍在裁剪区外的旧行。
右键、三击等参数使用 Cua 原生能力；其出现在 CLI 中不代表已经完成全部游戏场景验收。

首次输入会截取一次原始分辨率窗口图像，建立 Cua 会话的比例上下文。
同一窗口、会话及几何下，后续动作使用即时 API 坐标，不再逐步截图，也不携带旧截图的 capture_id。
窗口移动或大小变化后重新校准。driver 重启/会话上下文丢失时可能拒绝输入，执行 `snapshot` 再观察。
输入始终使用精确 PID/window，默认后台；API 的进程 ID 必须与目标一致，分辨率必须匹配。

每次动作保存原始 Cua 请求/结果与 API 前后读回，默认目录为 `tools/cua/.runtime/semantic`。
`layout_changed` 仅说明快照变化；须从 `after.controls` 或业务 API 检查具体预期效果。
输入超时、结果未知或读回失败不自动重试；先观察，再决定下一次操作。

## REST 合同

`GET /api/v1/ui/controls`：可选 `label` 子串、`role`、`window` 子串/ID、`limit=1..4096`
和 `include_disabled=true|false`；默认 limit 500，保留禁用控件。
首次读取开启五秒绘制采集租约，返回 `ready:false` 时短暂等待再读，不需要推进游戏 ticks。

响应沿用 `success/data/errors`，`data` 包含：

- `process_id`、`snapshot_id`、场景、绘制帧、物理分辨率、UI 缩放及时间。
- 控件 `id/role/label/value/checked`、来源、所属窗口、绘制/窗口顺序。
- `bounds` 原始点击区、`visible_bounds` 裁剪后的可见部分、`enabled/clickable/disabled_reason`。
- `coverage/limitations`、`total_controls/truncated`。未画出的控件不会凭空生成。

`GET /api/v1/ui/controls/resolve?snapshot_id=...&control_id=...`：返回可见区域内的中心点，
坐标为**游戏客户区左上角起算的物理像素**，可交给原始分辨率的 Cua 窗口输入。
缺参数 400，ID 不存在 404，快照未就绪/过期/控件禁用或不可见 409。
该接口只解析坐标，不发送输入。

ID 在控件语义与所属窗口不变时保持稳定；快照版本还包含布局与状态变化。
同语义的重复控件按绘制次序编号，列表重排后应重新读取并辨认，ID 不是跨界面持久句柄。
A→B→A 会产生新快照版本，不能凭同样的位置重放旧操作。
绘制异常、窗口栈/分辨率/缩放变化时等待完整新帧。
GUI.Window 的回调在本机 Unity 中晚于 Root.OnGUI 和 EndGUI；采集跨越其延迟绘制，
在下一帧读取时发布，避免主菜单可读而弹窗为空。

## 覆盖边界

已接入 Widgets 的普通/图标/不可见按钮、带标签复选/单选控件、文本框、静态标签，
FloatMenuOption 和 Command Gizmo 的选定绘制入口。
Subtle/Custom 按钮使用外层标签与内部实际 hitbox；不会把缓存高度计算前的矩形当作点击区。
窗口模态阻断、中心点几何遮挡和原生 GUI clip 会影响 `clickable`。

这不是整个游戏或所有 Mod 的完整 DOM。自定义绘制、无标签图标、滑条/拖拽、地图与世界目标、
复杂 Gizmo 子动作仍需适配或配合截图与业务 API。
`CustomButtonText(active:false)` 不执行内部 hitbox，当前可能完全不出现在列表。
文本框当前报告内容/可写性，不报告插入光标或完整键盘焦点。
Resolve 与 OS 输入是两个操作，界面仍可能在中间变化；不承诺原子点击。
坐标已针对原生常规等比 UIScale 校准；额外旋转或自定义 GUI.matrix 变换未验证。

运行验收与未验部分见 [验收记录](references/reviews/2026-11-07-ui-semantic-bridge.md)。
