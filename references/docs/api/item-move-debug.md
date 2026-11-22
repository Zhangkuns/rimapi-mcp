# 即时移动一整堆地面物品（ITEM-D01）

`POST /api/v1/debug/native/execute` 的 `item.move` 把同一张地图上的既有物品整堆立即移到指定格。它是 Core 调试动作，需要活动存档与开发者模式。先用 `GET /api/v1/debug/native/actions` 确认已加载的 DLL 支持该动作。

```json
{"action":"item.move","map_id":0,"thing_id":12345,"x":100,"z":101,"dry_run":true}
```

- 通过 `GET /api/v1/maps` 取得 `map_id`，通过 `GET /api/v1/map/things/radius?map_id=0&x=98&z=100&radius=3` 查看附近物品的 `thing_id`。接受数字 ID 或完整 `ThingID`（如 `Steel12345`）。`x`、`z` 是目标格，均必填。
- 目标格必须在地图内、允许该物品放置且有空余堆位；如果是储存格，还要通过该处的过滤规则。普通地面已有一堆就会拒绝；有多堆位的储存格若仍有空位，可以保留为独立的一堆。动作不会合堆、拆堆、清场或改用附近的格子。
- `dry_run:true` 验证参数并返回预览；正式执行时删除该字段或设为 `false`。可附 `expected_process_id` 防止误投到重启后的进程，使用 `Idempotency-Key` 在同一 Game 内重试，见[通用调试合同](../debug-native-actions.md)。

`before` 和 `after` 返回原堆的 `thing_id`、`thing_number`、`def_name`、`stack_count`、`forbidden`、`spawned`、`map_id`、`x`、`z`。无禁止组件的物品返回 `forbidden:null`。预演和同格请求不写入，`applied`、`changed`、`native_invoked` 为 `false`；同格 `would_move:false`。实际移动且读回成功才发出 `operation` 事件，重放不重复发出。

缺少参数返回 400；未开开发者模式返回 403；地图或已在该地图生成的对象找不到返回 404；进程不符或游戏未就绪返回 409；非物品、越界、目标格不能完整容纳或原堆数量超出原生限制返回 422。原生调用异常或终态不符返回 500 与 `outcome:"uncertain"`，尽可能附上实际读回。

动作通过原生 `Thing.DeSpawn` 和 `Thing.SpawnSetup` 重新登记同一个对象，触发原生移除与加入的通知。适用于已生成在地图格上的物品，包括储存格上的堆；容器或人物身上的未生成物品、跨地图、部分数量和正常小人搬运不在此合同内。

普通货架的物品仍在地图格上。已知实际储存格坐标时，`item.move` 可用于地面→货架、货架→地面和货架→货架，三条路径均已[隔离验证](../references/reviews/2026-11-20-item-transfer.md)。本动作只接收目标坐标；按货架或储存区 ID 查格、自动选格，以及书柜和装备架等内部容器的进出，请使用 [item.transfer](item-transfer-debug.md)。
