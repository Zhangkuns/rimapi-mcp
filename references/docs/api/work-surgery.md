# 即时手术

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.surgery`，按现有医疗/手术账单结算一次手术。游戏暂停也能执行；请求前后 `TicksGame` 不变。手术结果（含原版失败副作用）按实际读回，不保证成功。

```json
{"action":"work.surgery","map_id":0,"thing_id":"27979","bill_id":"0","pawn_id":"28004",
 "ingredients":["HemogenPack28001"]}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 病人（`Pawn`，其 `health.surgeryBills` 承载手术账单）；也接受有该病人占用的床。 |
| `bill_id` | 二选一 | 账单整数身份（来自现有账单列表）。 |
| `bill_index` | 二选一 | 病人手术账单栈的 0-based 索引。 |
| `pawn_id` | 是 | 同图、存活、未倒地、玩家阵营、humanlike、有 Manipulation 且满足配方技能要求的医生。 |
| `ingredients` | 否 | 实际药品/植入物 ThingID 列表。省略时从地图上的真实可搬运物中自动选择；提供时必须全部是该账单需要的材料。 |
| `dry_run` | 否 | 只校验并返回计划，不消耗、不应用。 |

`Bill_Medical` 在 1.6 中挂在病人的 `BillStack`（`health.surgeryBills`）上；床只是病人所在位置，不是账单所有者。传床时动作取该床的占用病人。

## 原生语义

1. 解析 `Bill_Medical`，病人由 `GiverPawn` 推导，部位由 `Bill.Part` 推导。
2. 校验账单未 `iterationCompleted`、未删除/暂停、`CompletableEver`、`ShouldDoNow`、recipe worker `AvailableOnNow(patient, part)`；医生技能/Manipulation/阵营/同图；部位在 `GetPartsToApplyOn` 中。
3. 用真实 Thing 构建材料计划：`uniqueRequiredIngredients` 每个 1 件；`recipe.ingredients` 按 `IngredientCount.CountRequiredOfFor` 计算数量；不足/不匹配/多余材料 4xx，不半扣。
4. 按原生顺序 `recipe.Worker.ConsumeIngredient(portion, recipe, map)`（默认销毁）→ 将实际消耗的药品写入 `Bill_Medical.consumedMedicine` → 调用公开 `bill.Notify_IterationCompleted(doctor, ingredients)`。
5. 该原生方法调用 `recipe.Worker.ApplyOnPawn(patient, part, doctor, ingredients, bill)`，成功后按 flesh 增加 `OperationsReceived`/`OperationsPerformed`，并 `billStack.Delete(bill)`。

不伪造手术结果、不直接 `MakeThing` 植入、不跳过 `ApplyOnPawn`、不批量。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`/`pawn_id`、`bill_id`/`bill_index` 缺失或格式错误 | 400 `bad_request` |
| 地图/病人/医生/材料/账单不存在 | 404 `not_found` |
| 非医疗账单、非本病人账单、已结算/已删除/暂停、`CompletableEver` 或 `AvailableOnNow` 失败、部位无效、医生不合格（动物/非玩家/无 Manipulation/技能不足）、材料不足/不匹配/多余 | 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在消耗/应用前完成。

## 响应

- `patient`/`patient_after`：ThingID/def/位置、spawned/dead、hediffs（def/part/severity/bleed）、bills、手术记录；
- `doctor`/`doctor_after`：ThingID/label/阵营/Manipulation/`operations_performed`；
- `bill_before`/`bill_after`：bill_id/index、recipe/worker、state、`iteration_completed`、`deleted`、`completable_ever`、giver/part；
- `part`：def/label/index；
- `ingredients_plan`、`consumed`（def/数量/ThingID/destroyed/源堆剩余）、`consumed_medicine`；
- `records_before`/`records_after`：`patient_operations_received`、`doctor_operations_performed`；
- `bill_present`、`applied`/`native_invoked`/`changed`/`outcome`（`completed_bill_deleted`）。

`patient_after.hediffs` 是实际结果：新增/移除 hediff、BloodLoss 变化、原版失败副作用都按现场读回，不强制成功。`dry_run` 返回 `would_complete`、`would_consume`，不扣料/不改账单/不应用。

## 边界与幂等

- 只结算一次已有账单；不添加/取消手术、不批量、不刷药品、不保证手术成功。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再扣料/不再手术；同键改请求（换账单/病人/材料）为 409。已删除账单用新 key 返回 404/422。
