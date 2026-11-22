# 可充能装备装填

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.reload`，对一件现有可充能服装/装备从真实弹药堆装填。它调用公开 `IReloadableComp.ReloadFrom(ammo)`，不创建或推进正常 Reload Job，游戏暂停也能执行；请求前后 `TicksGame` 不变。

```json
{"action":"work.reload","map_id":0,"thing_id":"27995","pawn_id":"27979","ingredients":["Chemfuel27999"]}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 已穿戴/装备的 `CompApparelReloadable` 或 `CompEquippableAbilityReloadable` 物品。 |
| `pawn_id` | 是 | 同图、存活、未倒地、玩家阵营的 owner。 |
| `ingredients` | 是 | 恰好一个弹药 ThingID；其 def 必须等于装备的 `AmmoDef`（原版 `Props.ammoDef`）。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 原生语义

- 装备通过 `IReloadableComp` 访问；owner 由 `ReloadableUtility.OwnerOf(comp)` 从原版 apparel/equipment tracker 解析，必须等于 `pawn_id`。
- 调用 `reloadable.ReloadFrom(ammo)`：原版按 `ammoCountToRefill`（整次补满）或 `ammoCountPerCharge`（按充能数）执行 `SplitOff(...).Destroy()` 并更新 `RemainingCharges`。
- 本动作不调用 `UsedOnce`、不直接写 `remainingCharges`、不刷弹、不替代正常 Reload Job。
- 弹药是原版 `ThingDef`（`IReloadableComp.AmmoDef`），不是独立的 `AmmoDef` 类；代码与合同统一按 ThingDef 处理。

`ReloadFrom` 在数量不足/已满时会静默返回；本动作在调用前用 `NeedsReload(true)` 与 `MinAmmoNeeded(true)` 校验，并在调用后检测“充能数未增加或弹药未减少”，失败返回 `uncertain` 而不是假装成功。

## 校验与错误

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`/`pawn_id`/`ingredients`、多弹药 | 400 `bad_request` |
| 地图、装备、owner、弹药不存在 | 404 `not_found` |
| 装备非 reloadable、owner 不匹配、已满、弹药 def 不匹配、弹药不足 | 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在 `ReloadFrom` 前完成；失败不扣弹药、不增加充能。

## 响应

- `gear`/`gear_after`：ThingID/def、comp 类型、owner、`remaining_charges`/`max_charges`、`ammo_def`、`needs_reload`、`min/max_ammo_needed`、`max_ammo_amount`、`label_remaining`；
- `ammo_before`/`ammo_after`：弹药 ThingID/def、`stack_count`、spawned/destroyed；
- `charges_before`/`charges_after`、`consumed`（实际减少的弹药量）、`source_destroyed`、`ammo_stack_after`；
- `min_ammo_needed`/`max_ammo_needed`、`would_consume`；
- `applied`/`native_invoked`/`changed`/`outcome`（`reloaded`/`partially_reloaded`）。

`dry_run` 还返回 `would_reload`、`would_consume`、`charges_after_preview`，不改任何状态。

## 边界与幂等

- 只装填一件已有装备；不装炮塔/穿梭机专用弹、不批量、不替代正常 Reload Job。
- `ReloadFrom` 只扣一次真实弹药；原版音效/充能更新保留，不授予技能经验。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再扣弹；同键改请求（换装备/弹药）为 409。已满装备用新 key 返回 422。
