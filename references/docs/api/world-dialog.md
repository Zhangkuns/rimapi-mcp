# Core 世界遭遇/任务/开局业务对话决策（C-39）

`GET/POST /api/v1/world/dialog`。只发现并执行原生业务窗口
`Dialog_NodeTree`、`Dialog_NodeTreeWithFactionInfo`、`Dialog_MessageBox`
（当前打开的实例）；通用设置/退出/模组窗口不在白名单。

合同状态：**QuestPart_Dialog、CaravanMeeting 与
`ScenPart_GameStartDialog`（`started_from_entry:true`）已验**；
CaravanDemand Give/Fight 未在正式 runner 固化。

## GET

`GET /api/v1/world/dialog` 列出当前打开的业务对话；`?dialog_id=...` 读单个。

```json
{"count":1,"dialogs":[{
 "dialog_id":"26501b94","type":"Dialog_NodeTreeWithFactionInfo",
 "title":"Caravan demand","text":"A caravan demands payment.",
 "context":{"kind":"node_tree","interactive":true,
            "faction":"PlayerColony","option_count":1},
 "options":[{
   "option_id":"node:0:Accept (pay 100 silver)|enabled|RimWorld.QuestPart_Dialog+...|...",
   "index":0,"text":"Accept (pay 100 silver)","disabled":false,
   "disabled_reason":null,"has_action":true,
   "action_type":"RimWorld.QuestPart_Dialog+<>c__DisplayClass15_1",
   "action_method":"<Notify_QuestSignalReceived>b__1",
   "has_link":false,"resolve_tree":true,"selectable":true
 }]
}]}
```

- `option_id` 包含文本/disabled/action 方法签名；POST 必须原样回传，若窗口
  重开/选项变化则 409。
- `has_action:false` 的关闭/继续/链接选项仍可发现，但 `selectable:false`；
  **关闭不是选择**。
- `context.faction` 来自原生 faction 窗口；没有伪造 quest signal/费用。

## POST

```json
{"dialog_id":"26501b94","option_id":"node:0:...","confirm":true}
```

- 只执行原生 `DiaOption.action`；不直接发 quest signal、不改关系、不伪造
  费用/奖励、不强行关闭。`resolveTree` 只记录，不代替原生 action 或替玩家点
  关闭；action 自身关闭窗口时会如实返回 `open_after:false`。
- `Dialog_MessageBox` 只执行 A/B/C 的真实回调；无回调按钮不可选。
- 非 dry 需 `confirm:true`；`dry_run:true` 只读回选中 option，不执行 action。
- 错误：dialog/option 过期/不存在 404；disabled/无 action/未 interactive 409；
  缺 option 400；action 抛错 500，附 `open_after` 状态读回，不 broad 关闭。
- 不归本路由：S-10 任务选择信；通用消息确认可复用 MessageBox 通道，
  但当前路由只暴露 NodeTree/MessageBox 业务实例。

## 状态码

| 状态 | 条件 |
| --- | --- |
| 200 | GET / dry-run / executed action |
| 400 | 缺 option_id 或 confirm |
| 404 | dialog_id/option 不存在（含已关闭） |
| 409 | option 变化、disabled、无 action、未 interactive |
| 500 | 原生 action 抛错 |
| Idempotency | none |

## 验证与边界

- runner：`tests/c39_dialog_decision.py`（passed）；
  证据：`tools/cua/.runtime/c39-dialog-20261113c/evidence/c39.json`。
- fixture：`tests/parallel/core-dialog/CoreDialogFixture.cs` 打开真实
  `QuestPart_Dialog`（inSignal `C39.Open`、option outSignal `C39.Accepted`）
  并记录 signal；仅测试准备。
- 已验：GET 发现 faction/option/action 签名；dry-run 不发 signal；缺 confirm
  400；非法 option 404/409；正式执行后 `open_after:true`（未强行关），
  原生 SignalManager 收到 `C39.Accepted`。
- 已验 CaravanMeeting：fixture 用真实 `IncidentWorker_CaravanMeeting` 在玩家
  商队上生成 faction 对话框（Trade disabled with native reason / Attack /
  Move on）；POST 执行原生 MoveOn action，`applied:true`、`open_after:true`，
  关系表读回正常，未伪造 signal。
- `ScenPart_GameStartDialog` 已验：`POST /game/start` 新增
  `started_from_entry:true`（shared `GamePlayHelper` 设置
  `GameInitData.startedFromEntry`），原生 `Game.InitNewGame` 调
  `Scenario.PostGameStart`，真实 Crashlanded dialog 出现；text 读回，唯一
  option 是 native OK（`has_action:false`），POST 409，窗口仍开。默认 false
  对照无该 dialog。
- 未测：CaravanDemand Give/Fight、ScenPart_GameStartDialog 真实新开局入口；
  S-10 选择信独立归属。关闭选项仍不作为选择。
