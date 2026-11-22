# 指定树木即时砍伐

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `plant.cut`。游戏暂停时也能执行，每次处理一棵树，无需指定殖民者或先标记砍伐。

通过 `GET /api/v1/map/plants?map_id=0` 查目标。使用返回的数字 `thing_id`（转成字符串）或完整原生 ID：

```json
{"action":"plant.cut","map_id":0,"thing_id":"27979","dry_run":true}
```

- 预演返回 `would_cut`、`can_yield` 和有定义时的 `yield_def_name`。它不改变树、标记、木材或随机状态，也不承诺具体产量。
- 去掉 `dry_run` 或设为 `false` 后即时执行。`after.destroyed` 表示原树被移除，已有对象标记随原生移除清除。`after.stump` 在生成原版树桩时给出其 ID 和位置。
- 木材按原生生长、生命值和难度规则计算，落到树附近，可能并入已有堆。`after.produced_count` 是本次落地总量；`after.products[]` 返回各堆的 `thing_id`、`def_name`、`x`、`z`、本次新增 `added` 和最终 `stack_count`。未达到采集条件的普通幼树可以砍掉，产量为 0。
- 缺参数为 400，地图或树 ID 不存在为 404，非树、树桩、不可销毁或开启防砍保护的树为 422。开发者模式关闭为 403。传 `Idempotency-Key` 可在本游戏实例内重放同一执行响应；同键改请求为 409。若游戏报告 `outcome: uncertain`，先核对 `after` 中的树与已落地堆，避免再次执行造成重复产出。

该动作结算树木和产物，不计算殖民者技能增益，也不记人物经验、砍树履历或工作完成事件。正常砍伐标记、优先工作和 `/plant/cut-protection` 仍用于各自流程。
