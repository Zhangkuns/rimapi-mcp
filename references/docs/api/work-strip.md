# 剥取可剥目标衣物

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.strip`。暂停时也能执行；对一个现有可剥目标（尸体或倒地人物等 `IStrippable`）结算原版剥衣，由原版把穿戴/持有物落地，不复制、不新建物品。

## 参数

```json
{"action":"work.strip","map_id":0,"thing_id":"27987","pawn_id":800,"dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：`Corpse` 或 `Pawn` 的数字 `thingIDNumber` 或完整 `ThingID`，必填。
- `pawn_id`：执行者殖民者的数字 ID，必填（用于资格与身份读回；不提交 Job）。
- `dry_run`：可选，默认 `false`。

## 校验（全部在原版剥衣前）

- 目标必须在指定地图上已生成且未销毁；
- 目标必须实现 `Verse.IStrippable`；
- `StrippableUtility.CanBeStrippedByColony(target)` 为真：尸体要求 `AnythingToStrip()`；人物还要求 `kindDef.canStrip`、未被搬运，并且处于 `Downed`，或是殖民者牢房中安全的囚犯；
- 工人必须在同一张地图、存活且已生成、属于玩家阵营。

任一不合格返回 4xx，且不会移动/删除任何物品。

## 语义

与原版 `JobDriver_Strip` 的完成段相同（但不跑 60 tick 等待）：

1. 记录并删除目标上的 `Strip` 标记（`DesignationDefOf.Strip`，`targetType=Thing`）；
2. 调用 `IStrippable.Strip()`：
   - `Corpse.Strip()` 直接转调 `InnerPawn.Strip()`；
   - `Pawn.Strip()` 在地图上以 `pos = Corpse?.PositionHeld ?? PositionHeld` 为落点：`equipment.DropAllEquipment(pos, forbid:false)`、`apparel.DropAll(pos, forbid:false, Destroyed)`、`inventory.DropAllNearPawn(pos)`；远行队内则转为转移到队友背包（地图上的目标不会走该分支）。

本动作不写 `BodiesStripped` 记录、不产生 Tale、不耗工作/经验；落地物是原版移动的**同一批物品对象**（thingID 不变），不是新造衣物。

倒地会先触发 `DropAndForbidEverything`：正常地图上掉落装备与背包、**保留衣物**；因此倒地人物仍有衣物可剥（响应会如实读回实际持有）。

## 响应

- `before` / `after`：目标 `thing_id`、`def_name`、`label`、`map_id`、`x`、`z`、`spawned`、`destroyed`、`is_corpse`、`is_pawn`、`inner_pawn`（`thing_id`/`def_name`/`dead`/`downed`/`faction`/`apparel_count`/`inventory_count`/`equipment_def_name`）。
- `gear_before` / `gear_after`：每件 `thing_id`、`def_name`、`label`、`count`、`from`（`apparel`/`inventory`/`equipment`）、`slot`（衣物身体部位组；装备为 `weapon`）、`layer`、`quality`、`hit_points`/`max_hit_points`。
- `dropped`：原版落地后的同一批物品 `thing_id`、`def_name`、`from`、`slot`、`count`、`x`、`z`、`spawned`、`found`。
- `strip_designation_before` / `strip_designation_after`、`worker`、`other_targets_before` / `other_targets_after`（其它可剥目标的 `thing_id`/def/位置/`gear_count`）、`native_methods`、`applied`、`changed`、`native_invoked`、`outcome`（`stripped`）。
- 预演：`would_strip=true`、`gear_count`、`before == after`。

## 错误与幂等

- 缺 `map_id`/`thing_id`/`pawn_id` → 400。
- 地图/目标/工人不存在 → 404。
- 非 `IStrippable`（墙、物品等）、不可剥（健康人物、无衣物/背包/装备的动物尸体等）、非玩家工人 → 422。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再落地第二份；已剥目标新 key → 422（`AnythingToStrip` 已为假）；同键换请求 → 409。
- 原生异常时返回 `outcome: uncertain` 与 `gear_after`/`dropped` 读回，先核对再决定；不伪造成功。
