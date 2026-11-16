# Odyssey MapPortal 单人进入、返程与封口（OD-R10）

2026-11-13 新增。扩展现有 `POST /api/v1/dlc/odyssey/portal` 的 `action`：
`enter` 与 `seal`。两者只排队玩家浮菜单同一原生 job：

- `enter`：`JobDefOf.EnterPortal`，`playerForced=true`（与
  `FloatMenuOptionProvider_EnterMapPortal` 相同）；
- `seal`：`JobDefOf.Seal`（与 `CompSealable` 的“Collapse cave entrance”
  gizmo 确认后的动作相同）。

pocket map 生成、`CaveExit`（`PocketMapExit`）连接、`CompSealable.Seal()`
的销毁/封口全部由游戏完成。多人装载与等待见
`docs/api/odyssey-portal.md`（OD-R05）。

## 读取（新增字段）

`GET /api/v1/dlc/odyssey/portal?portal_id=...` 每行追加：

- `has_sealable`：是否存在原生 `CompSealable`；
- `sealed`：反射读取的 `CompSealable.isSealed`；
- `is_pocket_exit`：是否 `PocketMapExit`（pocket map 返回门）；
- `entrance_id`：返回门对应的地表入口 ThingID。

列表模式 `?map_id=<pocket_map_id>` 可找到 `CaveExit` 行（含上述字段）。

## 进入

```json
{"portal_id":"InsectLairEntrance75478","action":"enter",
 "pawn_id":"Human321","dry_run":true}
{"portal_id":"InsectLairEntrance75478","action":"enter",
 "pawn_id":"Human321","confirm":true}
```

- 要求 portal `IsEnterable`、pawn 存活且可 `CanReach(ClosestTouch)`、
  Manipulation 正常；`dry_run` 返回 `would_enter`，**不会生成 pocket map**
  （显式避免 `GetOtherMap()` 的生成副作用）；
- 正式提交后返回 `job_def:"EnterPortal"`、`job_submitted/applied；
  `target_map_id` 在 pocket map 尚未生成时为 `null`（生成由原生 job 完成），
  生成后 GET 返回 `pocket_map_id`。
- 在 pocket map 内对 `CaveExit` 再调用 `enter`：`pawn_map_id` 为 pocket map，
  `target_map_id` 为地表 map（返回门用 `entrance.Map`，不触发生成）。

## 封口

```json
{"portal_id":"InsectLairEntrance75478","action":"seal",
 "pawn_id":"Human321","confirm":true}
```

- 要求 portal 有 `CompSealable`、pocket map 已生成、尚未 sealed、sealer
  存活且可 `CanReach(Touch)` 未倒地；`dry_run` 返回 `would_seal`；
- 正式提交 `JobDefOf.Seal`（原生 300 tick 等待后 `CompSealable.Seal()`）；
  `InsectLairEntrance` 的 `destroyPortal=true`：pocket map 销毁、入口建筑
  移除；之后再 GET 该 portal 返回 `404`。

错误：缺 `pawn_id` `400`；pawn 不存在 `404`；不可进入、不可达、无
sealable、已 sealed、无 pocket map 分别 `422/409`。

## 证据与边界

2026-11-13 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，
invulnerability 开启）：fixture 生成原生 `InsectLairEntrance` + 两名殖民者。

1. 初始 `enterable:true/sealed:false/has_sealable:true/pocket_map_exists:false`；
2. `enter` dry：`would_enter:true`，portal 仍无 pocket map（修复前 dry 会
   生成，已修正）；正式提交 `EnterPortal`；
3. 推进后 pocket map 1 生成，Human321 位于 map 1（`pocket_pawn_ids` 含
   原巢穴生物 + Human321），Human315 仍在 map 0；
4. pocket map 列出唯一 `CaveExit83135`：`is_pocket_exit:true`、
   `entrance_id:InsectLairEntrance75478`、`enterable:true`；
5. 对 `CaveExit` 执行 `enter`：提交 `EnterPortal`，`target_map_id:0`；
   推进后 Human321 回到 map 0（(166,31)），pocket map 仍存在；
6. `seal` dry 不写；正式提交 `Seal`；推进后 portal `404`（已销毁）、
   `game/state map_count` 从 2 回到 1（pocket map 销毁），终态 paused。

证据
`tools/cua/.runtime/od-r10-20261113z/evidence/od-r10-portal.json`
（dll `4765196e…`、fixture `2af53417…`、source `8e4a5499`），runner
`tests/parallel/odyssey/od_r10_portal_return.py`，fixture
`tests/parallel/odyssey/OdysseyFixture.cs`（沿用 `prepare_portal`/
`observe_portal`）。

- 未测：多人同时进入/分批返回、封口时 pocket map 内仍有 pawn/物品的
  丢失提示、`AncientHatch` 等非销毁型封口、其它 portal 类型、封口后的
  存档/读档持久化。
