# 原生开发者功能 API

调用路径为 **HTTP API → 游戏主线程 → 明确指定的原生组件方法**。不打开开发者菜单，不依赖 Cua，也不向客户端暴露任意反射执行。现有头衔、基因、异种、教义转换、风格和变异接口继续使用各自专用 API。

- `GET /api/v1/debug/native/actions`：动作清单、底层方法、参数、当前教义及 precept 实例 ID、既有等价接口。
- `POST /api/v1/debug/native/execute`：执行动作。要求活动存档、开启开发者模式；接口不会替用户修改该选项。

例如将人物确定度设为 0.8：

```json
{"action":"pawn.certainty.set","pawn_id":66734,"value":0.8,"dry_run":false}
```

| action | 参数 | 原生入口 |
|---|---|---|
| `pawn.certainty.set` | `pawn_id`, `value`（0..1） | `Debug_ReduceCertainty(before - value)` |
| `construction.complete` | `thing_id`；可选`map_id/pawn_id` | 蓝图`TryReplaceWithSolidThing`、Frame`CompleteConstruction`；重装另走`MinifyUtility.MakeMinified`，见[准确合同](api/construction-complete.md) |
| `roof.set` / `roof.clear` | `map_id`；`cells` 或 `point_a` + `point_b`；设置时传 `def_name` | `RoofGrid.SetRoof`；[屋顶合同](api/roof-grid-debug.md) |
| `item.move` | `map_id`, `thing_id`, `x`, `z` | `Thing.DeSpawn`、位置赋值、`Thing.SpawnSetup`；[整堆移动合同](api/item-move-debug.md) |
| `ideo.precept.add` | `ideo_id`, `def_name` | `PreceptMaker.MakePrecept` + `Ideo.AddPrecept` |
| `ideo.precept.remove` | `ideo_id`, `precept_id` 或唯一 `def_name` | `Ideo.RemovePrecept` |
| `anomaly.level.set` | `def_name`（MonolithLevelDef），需已生成巨石 | `Find.Anomaly.SetLevel(def, false)` |
| `world.landmark.add/remove` | `tile`, `layer`（默认 0）；添加需 `def_name` | `WorldLandmarks.AddLandmark/RemoveLandmark` |
| `world.tile.mutator.add/remove` | `tile`, `layer`（默认 0）, `def_name` | `Tile.AddMutator/RemoveMutator` |

确定度与precept项要求 Ideology，巨石要求 Anomaly，世界项要求 Odyssey；施工完成、屋顶编辑和整堆移动为 Core 调试动作。Def 可通过 `/api/v1/defs?type=PreceptDef`、`MonolithLevelDef`、`LandmarkDef`、`TileMutatorDef`、`RoofDef` 查询。移除指定 precept 时从动作清单取得 `precept_id`，同名多个实例不会任意选一个。重复添加相同 precept 为无操作；地标占位冲突需显式移除。

通用参数 `dry_run:true` 仅检查请求和已有状态，不生成 precept、不调用变更方法、不运行地图变体 worker、不消耗随机数。世界添加的 worker 地形适用性只在实际执行前检查，因此预检成功不保证地形适用。可选 `expected_process_id` 防止调用到重启后的进程。

执行响应包含 `before`、`after`、`applied` 和 `native_invoked`。原生调用异常时可能已有部分效果，返回失败并保留实际读回，`applied:null` 表示结果不确定；不会假装回滚。传 `Idempotency-Key` 可在当前游戏实例内重放冻结响应，避免网络重试再次执行；同键更改请求返回冲突。预检不占用该键。

这些是显式调试变更：直接改 precept 不经过正常改革；巨石等级会通知研究/事件等组件，降低等级不撤销历史；增加地标可能消耗随机数并增加变体；增加变体可能替换冲突项、调用 worker。移除地标保留变体，移除变体不撤销 worker 效果，也不会重建已经生成的地图。不能以这些 API 成功代替正常玩法的解锁、仪式、旅行等流程验收。

运行验证记录见 [本批次交付记录](references/reviews/2026-11-07-debug-native-actions.md)。正式 DLL 尚未部署。
