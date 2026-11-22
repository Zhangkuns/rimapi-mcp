# 背包食用

`POST /api/v1/pawn/inventory/ingest` 是人物装备页「食用」的正常玩家指令：对指定人物**背包中现有**的普通食物/药品启动原版 `Ingest` Job。它不经过 `CompUsable`，也不接受地图物品冒充背包物品。

```json
{"pawn_id":"27979","thing_id":"MealSimple28003","dry_run":true}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `pawn_id` | 是 | 人物 ThingID 或数字 ID。 |
| `thing_id` | 是 | 该人物 `inventory.innerContainer` 中现有物品的 ThingID/数字 ID。 |
| `dry_run` | 否 | 只校验并返回计划；不启动 Job。 |

## 原生语义

- 物品必须在该人物的 `Pawn_InventoryTracker.innerContainer` 中；地图上同 def 的同 ID/不同 ID 物品均不算。
- 资格走 `FoodUtility.WillIngestFromInventoryNow(pawn, thing)`（`WillEat`/`CanTakeDrug` + `IngestibleNow`）。
- 与 `FoodUtility.IngestFromInventoryNow` 相同地创建 `JobDefOf.Ingest`，`count = min(stackCount, WillIngestStackCountOf(pawn, def, NutritionForEater))`，调用 `Pawn_JobTracker.TryTakeOrderedJob`，直接读取其接受结果，并核对本次 Job 是否启动或排队。
- 这是正常指令，不是瞬时结算：`committed` 表示原生接受，`job_started`/`job_queued` 是即时观察，不把不相干的旧 Job 算作本次提交；`settled=false`。实际消耗、营养/需求、心情、食物中毒等由后续真实 ticks 结算。API 不推进全局时间。
- 重复请求没有幂等缓存；每次都是新的正常指令。若同一堆仍有剩余，后续请求可再次启动 Ingest（与原版点击一致），不会凭空刷食物。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `pawn_id`/`thing_id` | 400 `bad_request` |
| 人物/物品不在背包 | 404 `not_found` |
| 人物未 spawned、非玩家可控殖民者、死亡/倒地/未清醒/无意识；物品不可摄入；原版 `WillIngest...` 拒绝 | 422 `invalid_target` |
| 原生 Job 未被接受 | 409 `conflict` |

所有校验在 Job 提交前完成。

## 响应

- `pawn_id`/`pawn_number`；
- `thing`：ThingID/def、`stack_count` before、`ingestible`/`drug`、`food_need`、`tick`、`settled=false`；
- `would_consume`、`committed`、`job_started`、`job_queued`、`job_def`；
- 提交后立即的 `stack_after`、`food_need_after`、`tick_after`、`settled=false`；
- `applied`/`changed`/`native_invoked`/`outcome=ingest_committed`；若正在执行原生判为同一任务的 Job，接受不代表新建，`changed=false`。原版启动时可能已拆份，仍不代表完成结算。

`dry_run` 返回 `would_start_job=true`、`would_consume`，不改背包/需求/Job。

## 有界推进验收

提交后只在隔离副本推进最多 **600** 个原生 ticks（`TickManager.DoSingleTick`），触发食用终态即停止；记录 `TicksGame` 前后、原堆剩余、`needs.food`、是否销毁、Job 是否结束。原版 `Ingest` 可能在启动时 `SplitOff` 一份，Job 中的食物实例可能变成新 ID；以实例读回为准。

## 边界

- 不替代 `/pawn/use-item`（`CompUsable`），不处理地图物品、不指定目标、不批量。
- 不保证食用一定成功/有足够营养；原版拒绝和 Job 中断如实读回。
