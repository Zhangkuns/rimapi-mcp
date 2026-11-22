# 完成次核心扫描（WORK-D28c）

`POST /api/v1/debug/native/execute` 的 `work.subcore_scan_finish` 完成一台正在占用中的 `Building_SubcoreScanner`，复用原版 DEV “Complete” + 原生占完成段：

```json
{"action":"work.subcore_scan_finish","map_id":0,"thing_id":"SubcoreSoftscanner27979","dry_run":true}
```

## 定向反射与 Tick

- 原版 DEV “Complete” 只把私有 `fabricationTicksLeft` 置 0；真正的完成逻辑在 `Building_SubcoreScanner.Tick()` 的 `Occupied` 分支（`fabricationTicksLeft-- <= 0` 时弹出占用者、生成 `def.building.subcoreScannerOutputDef`、播放完成音效）。
- 本动作**只**通过一次定向反射读写 `fabricationTicksLeft`：
  ```csharp
  typeof(Building_SubcoreScanner)
      .GetField("fabricationTicksLeft", BindingFlags.Instance | BindingFlags.NonPublic)
  ```
  置 0；不为其它私有字段建立反射。
- `Building_SubcoreScanner.Tick()` 是 protected override；本动作调用**一次公开 `Thing.DoTick()`**，它调用一次该 scanner 的 `Tick()`（并做该 Thing 的正常 tick 包装）。不遍历方法、不调用任意方法、不直接生成成品。
- 若一次 Tick 后没有按原生配置生成成品，返回 uncertain 并保留实际读回，不伪造成功。

## 前置校验

- `map_id`、`thing_id` 必填；目标是 spawned `Building_SubcoreScanner`。
- `PowerOn == true`；`State == Occupied`；`Occupant != null`；`AllRequiredIngredientsLoaded == true`。
- 空扫描器、未供电、缺料、非扫描器、错误地图/对象 4xx；不调用完成段。

## 原生完成语义

1. 设置 `fabricationTicksLeft=0`，一次 `DoTick()` 使原生 `Tick()` 把其减为 -1。
2. `EjectContents()`：给占用者加 `subcoreScannerHediff`（若配置）；rip 扫描器先 `KillOccupant()`（ExecutionCut 9999 对大脑），再弹出 occupant/尸体；销毁 `innerContainer` 中剩余材料。
3. `GenPlace.TryPlaceThing(ThingMaker.MakeThing(def.building.subcoreScannerOutputDef), InteractionCell, Map, ThingPlaceMode.Near)`：生成真实 `SubcoreRegular`（softscanner）或 `SubcoreHigh`（ripscanner）。
4. 原生完成音效/信件；设备 `initScanner=false`，`State` 归为 Inactive（材料已清空）。

## 读回

- `scanner`/`scanner_after`：thingID、def、位置、spawned、state、power_on、occupant、inner_count、destroy_brain、all_required_ingredients_loaded、`fabrication_ticks`、materials。
- `occupant`/`occupant_after`：thingID、def、位置、spawned/dead、`brain_present`、hediffs、`corpse_thing_id`。
- `fabrication_ticks_before/after`：实际字段值（典型 `7500 → -1`）。
- `materials_before/after`：容器内每个 fixed ingredient 的 steel/component stacks；完成后为 0。
- `outputs_before/after`、`product`：真实成品的 thingID/def/位置/stack；`product_generated`。
- `corpses_before`、`corpse`、`corpse_generated`：rip 扫描器的真实尸体。
- `items_total_before/after`、`applied/native_invoked/changed`。`items_total` 只统计地图 `ThingCategory.Item`；扫描器容器内的材料不计入其中，材料消耗以 `materials_before/after` 为准。
- `would_finish`：dry-run 的 state/power/occupant/materials/fabrication/output_def；`dry_run` 不置字段、不 Tick、不生成成品。

## 状态码与边界

- 缺 `map_id`/`thing_id`：400；地图/扫描器不存在：404；非扫描器：422。
- 未 spawn、未供电、非 Occupied、无 occupant、缺料：422，不完成。
- 关闭开发者模式：403；进程不符/幂等键冲突：409；原生异常或一次 Tick 未产出：500 uncertain，保留实际读回。
- 同 key 重放不再第二次 Tick/不再第二份成品/不再第二次弹 occupant；同 key 换 body 409；空扫描器/已提取新 key 422。
- 不刷次核心、不替代装载/弹出 API、不批量；不反射其它私有字段、不直接创建成品或尸体。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；同时覆盖 softscanner（`SubcoreRegular`，占用者存活 + `ScanningSickness`）与 ripscanner（`SubcoreHigh`，占用者死亡/大脑缺失/`Corpse_Human`）。断电、缺料、空设备、错目标、幂等均不重复完成。详见[WORK-D28c 验收与自审](../references/reviews/2026-11-21-work-d28c-subcore-scan-finish.md)和[原始证据](../../tests/parallel/work-d28c/evidence-work-d28c-subcore-scan-finish.json)。正式 DLL 未部署。
