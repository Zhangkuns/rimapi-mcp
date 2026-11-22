# 逐个扑灭火焰

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.extinguish`。暂停时也能执行；每次只处理一个现有 `Fire`，按原版击火效果反复施加 32 点 extinguishing 伤害，直到火自行销毁。

## 参数

```json
{"action":"work.extinguish","map_id":0,"thing_id":"27979","pawn_id":800,"dry_run":true}
```

- `map_id`：整数，必填。
- `thing_id`：`Fire` 的数字 `thingIDNumber` 或完整 `ThingID`，必填。
- `pawn_id`：执行击火的殖民者数字 ID，必填；其 `Drawer.Notify_MeleeAttackOn` 会按原版触发。
- `dry_run`：可选，默认 `false`。

目标可从 `GET /api/v1/map/things/radius` 之外的原生读回获取；本动作要求目标在指定地图上已生成且 `TicksSinceSpawn > 0`（刚生成、尚未 tick 的火不能击打，与原版一致）。

## 校验（全部在伤害前）

- `map_id`/`thing_id`/`pawn_id` 必填，否则 400；
- 地图/目标/工人不存在 → 404；目标必须已生成且未销毁；
- 目标必须是 `Fire` → 否则 422；
- `TicksSinceSpawn == 0` → 422；
- 工人必须同图、存活、玩家阵营、`!pawn.stances.FullBodyBusy` → 否则 422。

任一不合格返回 4xx，且不会造成任何伤害。

## 语义

与 `Verb_BeatFire.TryCastShot` 相同，每击：

1. `fire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 32f, 0f, -1f, pawn))`；
2. `pawn.Drawer.Notify_MeleeAttackOn(fire)`。

`DamageWorker_Extinguish` 把 `fireSize` 减去 `32 × 0.01 = 0.32`，当 `fireSize < 0.1` 时由原生代码 `fire.Destroy()`。本动作只在最多 32 击内循环，逐击记录 `fire_size_before/after` 与 `destroyed`；达到上限仍未熄灭则返回 `outcome: uncertain` 和真实读回，不假装成功。

- 不用 `Fire.Destroy()` 直接删除，不强杀。
- 不写 `FiresExtinguished` 记录、不提交 Job、不给经验/工作。
- 伤害直接施加在 `Fire` 上，因此不会给工人加 `CoveredInFirefoam`（只有以 Pawn 为 victim 才会加）。
- 附着火（`parent != null`，如人物/建筑着火）销毁时由 `AttachableThing.Destroy` 从父对象 `CompAttachBase` 移除附件，父对象不被销毁，读回中返回。

## 响应

- `before` / `after`：火 `thing_id`、`def_name`、`map_id`、`x`、`z`、`fire_size`、`hit_points`、`max_hit_points`、`ticks_since_spawn`、`spawned`、`destroyed`、`parent`（`thing_id`/`def_name`/`label`/`map_id`/`x`/`z`/`spawned`/`destroyed`/`fire_attachment_thing_id`）。
- `pawn` / `pawn_after`：工人 `thing_id`、`def_name`、`label`、`faction`、`map_id`、位置、`spawned`、`dead`、`downed`、`full_body_busy`。
- `hits[]`：`index`、`fire_size_before`、`fire_size_after`、`destroyed`；`hits_count`。
- `parent_before` / `parent_after`：附着对象状态与 `fire_attachment_thing_id`（销毁后为 `null`，字段可能省略）。
- `other_fires_before` / `other_fires_after`：其它火 `thing_id`、位置、`fire_size`、`ticks_since_spawn`、`parent_thing_id`，用于确认只熄灭目标火。
- 结果：`estimated_hits`、`native_methods`、`applied`、`changed`、`native_invoked`、`outcome`（`extinguished`/`uncertain`）。
- 预演：`would_extinguish=true`，`before` 与 `after` 相同，给出 `estimated_hits`（`(fireSize-0.1)/0.32` 向下取整加 1）。

## 错误与幂等

- 缺参数 → 400。
- 地图/目标/工人不存在、目标已销毁 → 404（已熄灭的火不再在生列表，天然 404）。
- 非 Fire、`TicksSinceSpawn == 0`、非玩家/死亡/忙碌工人 → 422；无伤害。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放返回缓存响应（`Idempotency-Replayed: true`），不再造成伤害；同键换请求 → 409。
- `fire.HitPoints/MaxHitPoints` 对 Fire 只是原始值（`useHitPoints=false`，本机 `-1/100`），火势以 `fire_size` 为准。
