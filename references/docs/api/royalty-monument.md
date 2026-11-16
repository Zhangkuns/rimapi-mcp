# Royalty 纪念碑标记、材料与取消（RY-R06）

已实现：`GET /api/v1/dlc/royalty/monument` 与
`POST /api/v1/dlc/royalty/monument`。复用原生 `MonumentMarker.GetGizmos`
的材料选择、`AllowedStuffs()`/私有 `PlaceAllBlueprints(ThingDef)` 和取消路径
（`MonumentCancelled` + `RemovePossiblyRelatedBlueprints` + `Uninstall`）。
不调用 `DebugBuildAll`，也不把 sketch 直接生成成品。

## 读取

`GET /api/v1/dlc/royalty/monument?map_id=0`

```json
{"map_id":0,"marker_count":1,
 "markers":[{"marker_id":"MonumentMarker75478","position":{"x":88,"z":58},
   "size":{"x":9,"z":14},"spawned":true,"complete":false,"all_done":false,
   "entity_count":120,"missing_count":120,"blueprint_count":0,"blueprint_ids":[],
   "allowed_stuffs":["Silver","Gold","Steel","Plasteel","WoodLog","..."],
   "can_place_blueprints":true,"can_cancel":true,
   "quest_id":1,"quest_name":"Patrick's Inheritance","quest_state":"Ongoing",
   "quest_tags":["Quest1.monumentMarker"]}]}
```

- 只列已安装/已生成的 `MonumentMarker`（未安装的 minified 物品不在列表）。
- `entity_count` 是 sketch 全部实体；`missing_count` 是尚未生成成品/蓝图/框架的实体；
  `blueprint_count` 是已有 `Blueprint` 的实体；`can_cancel` 对应原生 gizmo 的
  `!AllDone`。`allowed_stuffs` 直接来自原生 `AllowedStuffs()`。
- 若 marker 由任务生成，返回其 `quest_id/name/state/tags`；独立 marker 为 null/空。

## 操作

`POST /api/v1/dlc/royalty/monument`

```json
{"map_id":0,"marker_id":"MonumentMarker75478","action":"place_blueprints",
 "stuff_def":"Steel","confirm":true,"dry_run":false}
```

```json
{"map_id":0,"marker_id":"MonumentMarker75478","action":"cancel","confirm":true}
```

- **place_blueprints**：先按原生 `AllowedStuffs()` 得到允许材料。`stuff_def`
  必须在其中；不传时，只有唯一允许材料会自动选择，多个材料返回 409 并列出
  `allowed_stuffs`（原生此处会弹 FloatMenu，API 不打开 UI）。正式执行调用原生私有
  `PlaceAllBlueprints(stuff)`，返回 `missing_before/after`、`blueprint_count_before/after`、
  `placed_count`、`blueprint_ids` 和 `selected_stuff`。`dry_run` 只投影，不放蓝图。
- **cancel**：只有未完成（`!AllDone`）的 marker 可取消；正式执行与原生确认按钮一致：
  发送 `MonumentCancelled` 任务信号、删除该 sketch 的相关蓝图、`Uninstall()`。
  返回 `marker_spawned_after:false`、`minified_thing_id`、`blueprint_count_after:0`。
- 非 dry 必须 `confirm:true`；未知 map 404、marker 不在该图/已卸下 404、
  未知 action 400、`stuff_def` 不属于 sketch 422、多个材料未指定 409。
- 取消只处理 marker 与蓝图；不负责把任务标记失败。任务本身的后续（超时/被毁）
  仍由原生 quest nodes 决定。

## 原生验证

隔离 Xvfb `g35_deathresting` 副本（五 DLC，tick 76787）：
从真实 `ChangeRoyalHeir` 任务取得 marker（fixture 只安装任务掉落的 minified marker）。
该次 sketch 120 实体（74 地形 + 46 made-from-stuff）。无 `stuff_def` 时 409 并列出
15 种材料；选 Steel 后 dry-run 不写；正式 `placed_count:120`、blueprint 0→120、
missing 120→0，fixture 读回 46 个 `Steel` 材料蓝图和 74 个地形蓝图，frame 0，
`all_done:false`（未建造）。取消 dry-run 保持 120；正式后 marker 卸下为
`MinifiedThing75617`、相关蓝图 0、列表 0，重复取消 404。
另建一墙 marker 验证材料：Steel 生成 `Blueprint_Wall` 且 `stuff=Steel`，取消后蓝图 0。

证据：`tools/cua/.runtime/ry-r06-20261112c/evidence/ry-r06.json`
（runner `tests/parallel/royalty/ry_r06_monument.py`，fixture
`tests/parallel/royalty/fixture/RyFixture.cs`）。

未重跑：`DebugBuildAll`/成品建造、纪念碑完成与被毁/超时、逐实体不同材料、
非任务 marker 的其它 skech 变体、取消后任务超时/失败状态。
