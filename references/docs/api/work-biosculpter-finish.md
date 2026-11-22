# 完成生物雕塑周期

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.biosculpter_finish`，完成一个占用中的生物雕塑舱周期。离线/暂停时也能执行。

## 反射边界（重要）

本动作**只**对以下 private 方法做一次定向反射：

```csharp
typeof(CompBiosculpterPod).GetMethod("CycleCompleted", BindingFlags.Instance | BindingFlags.NonPublic)
```

然后以无参数方式调用一次。不遍历方法、不反射或修改任何私有字段（包括 `currentCycleTicksRemaining`）、不推进全局 tick；不是通用反射 API。native “DEV: complete cycle” Gizmo 只是把剩余 tick 设为 10 再等 `CompTick`，本动作不复用该捷径。

## 参数

```json
{"action":"work.biosculpter_finish","map_id":0,"thing_id":"27979","dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：雕塑舱的数字 `thingIDNumber` 或完整 `ThingID`，必填。
- `dry_run`：可选，默认 `false`。

## 校验（全部在调用前）

- 目标在指定地图上已生成，且有 `CompBiosculpterPod`；
- `State == Occupied`（有占用者）；
- `PowerOn` 为真；
- `Occupant != null`、`CurrentCycle != null`。

错误目标/空舱/断电/无周期返回 4xx，且不完成、不弹出、不产生污物。

## 语义（原版 `CycleCompleted` 顺序）

1. `SetBiotuned(occupant)`：舱绑定到占用者；
2. `currentCycle.CycleCompleted(occupant)`：实际周期效果（`medic` 治愈非永久伤势等）；
3. `EjectContents(interrupted:false, playSounds:true)`：占用者落到 `InteractionCell` 附近，清空周期与营养，不施加中断 sickness；
4. 占用者食物/休息恢复到 100%（如存在）；
5. 有 `gainThoughtOnCompletion` 时加对应记忆；
6. 记录 `HistoryEventDefOf.UsedBiosculpterPod`；
7. `drugs.Notify_LeftSuspension(elapsed)` 结算药物悬挂。

不伪造周期效果，不批量，不替代进入/弹出 API。

## 响应

- `pod`：完成前的 `thing_id`、`def_name`、位置、`state`、`power_on`、`occupant`、`current_cycle`/`current_cycle_label`、`inspect_string`。
- `occupant_before`：占用者的 id/def/位置/生成/死亡/倒地/年龄/食物/休息与 `hediffs[]`（def、label、severity、part、permanent）。
- `after`：完成后的 pod 状态（`LoadingNutrition` 等）、`power_on`、`occupant`（为空）、`current_cycle`（为空）、`inspect_string`（含 `Biotuned to: …`）。
- `occupant_after`、`ejected`：弹出后的占用者及其位置/存活。
- `healed_hediffs[]`：完成前后消失的 hediff。
- `filth_before/after/delta`：`InteractionCell` 上的 `Filth_PodSlime` 数量；`filth_stacks_*` 为其 `Thing.stackCount`；`filth_nearby_*` / `filth_nearby_stacks_*` 为以该格为中心 Chebyshev 半径 1 内的同一统计。
- `history_before/after/delta`、`native_methods`、`applied`、`changed`、`native_invoked`、`outcome`（`completed`）。
- 预演：`would_complete=true`，不调用 `CycleCompleted`。

`Filth` 的实际厚度是私有字段（`MaxThickness=5`），按反射边界不读取；原版 `FilthMaker.TryMakeFilth(..., 3–6)` 会在同一 Filth 上增厚或向邻格扩散，因此读回的是**污物对象数量/stackCount**，不是厚度。

## 错误与幂等

- 缺 `map_id`/`thing_id` → 400；地图/目标不存在 → 404。
- 非 `CompBiosculpterPod`、空舱、断电、无占用者/周期 → 422。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再完成第二周期；已完成/空舱的新 key → 422；同键换请求 → 409。
- `CycleCompleted` 抛错时返回 `outcome: uncertain` 与 pod 读回，先核对再决定。
