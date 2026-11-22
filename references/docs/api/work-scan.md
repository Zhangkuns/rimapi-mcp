# 扫描器立即发现

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.scan`。对现有地底扫描器或远程矿物扫描器触发一次原版 `DEV: Find now`（`CompScanner` 的 `DoFind(Pawn)`），暂停时也能执行。

## 反射边界（重要）

本动作**只**对两个具体类型做定向反射：

```csharp
typeof(CompDeepScanner).GetMethod("DoFind", BindingFlags.Instance | BindingFlags.NonPublic)
typeof(CompLongRangeMineralScanner).GetMethod("DoFind", BindingFlags.Instance | BindingFlags.NonPublic)
```

并以给定殖民者为参数调用一次。不遍历方法、不反射其它私有成员、不支持其它 `CompScanner` 子类；这不是通用反射 API，只是复用原版 DEV Gizmo 的入口。正常玩法里的 `Used()` 随机判定、进度累加与清零都不会发生（与 DEV Gizmo 一致）。

## 参数

```json
{"action":"work.scan","map_id":0,"thing_id":"27979","pawn_id":800,"dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：scanner 的数字 `thingIDNumber` 或完整 `ThingID`，必填。
- `pawn_id`：发现者殖民者的数字 ID，必填。
- `dry_run`：可选，默认 `false`。

## 校验（全部在 `DoFind` 前）

- 目标在指定地图上已生成，且有 `CompScanner`；
- 具体类型必须是 `CompDeepScanner` 或 `CompLongRangeMineralScanner`（其它扫描器/子类 422）；
- 目标属于玩家阵营；
- `CanUseNow.Accepted`：已生成、通电（如有 `CompPowerTrader`）、未被屋顶遮挡、未被禁止、玩家阵营；`CompDeepScanner` 还要求地图生物群系有基岩；
- 发现者存在、存活、已生成、玩家阵营、同一地图。

`dry_run` 不调用 `DoFind`，不改状态、不掷随机。

## 语义

- **CompDeepScanner**（`GroundPenetratingScanner`）：`DoFind` 随机选中心格、按 `deepCommonality` 加权选矿种、按 `deepLumpSizeRange` 取格数，经 `GridShapeMaker.IrregularLump` 写入 `map.deepResourceGrid`（只写可散布且不在 no-build edge 的格），再发一封 `PositiveEvent` 信件。
- **CompLongRangeMineralScanner**（`LongRangeMineralScanner`）：`DoFind` 用默认 `MineableGold` 组 slate，调用 `QuestScriptDefOf.LongRangeMineralScannerLump.CanRun`；为真时 `QuestUtility.GenerateQuestAndMakeAvailable` 生成任务并发送任务信件，为假时什么也不发生。

本动作不直接写 `deepResourceGrid`、不手造任务或信件；结果完全由原版 `DoFind` 产生，再从地底网格/任务栈/信件栈读回。

## 响应

- `before` / `after`：scanner 的 `kind`（`deep`/`long_range`）、`thing_id`、`def_name`、`label`、`map_id`、`x`、`z`、`spawned`、`destroyed`、`faction`、`power_on`、`forbidden`、`roofed`、`can_use_now`、`can_use_reason`、`inspect_string`（含原版进度百分比文本）。
- `worker`：发现者身份/位置/阵营/存活/生成状态。
- 地底：`deep_cells_before/after`、`new_deep_cells[]`（`x`、`z`、`def_name`、`count`）。
- 远程：`quests_before/after`、`new_quests[]`（`id`、`name`）。
- 信件：`letters_before/after`、`letters_delta`。
- `find_happened`、`native_methods`、`applied`、`changed`、`native_invoked`、`outcome`（`found`）。
- 预演：`would_scan=true`，不调用 `DoFind`，无 `find_happened`。

`days_working_since_last_finding` 是受保护字段，按照射边界不反射读取；进度可由 `inspect_string`（原版进度百分比）观察。DEV 入口直接调 `DoFind`，不会清零该进度，读数应保持不变。

## 错误与幂等

- 缺 `map_id`/`thing_id`/`pawn_id` → 400；地图/目标/工人不存在 → 404。
- 非 `CompScanner`、非白名单子类、非玩家阵营、`CanUseNow` 为假（断电/屋顶/被禁/无基岩等）、错误工人 → 422且不发现。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再发现第二个矿块/任务；同键换请求 → 409。scanner 可重复使用，没有“已发现”状态拒绝。
- `DoFind` 抛错，或调用后没有任何新增矿格/任务 → `outcome: uncertain`（500）并读回 scanner/网格/任务状态；不伪造成功。
