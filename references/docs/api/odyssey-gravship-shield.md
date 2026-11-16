# Odyssey 重力船盾发生器 Activate/Deactivate（OD-R08，代表验收）

2026-11-12 代表验收：复用通用
`POST /api/v1/dlc/anomaly/interactable` 与 `CompInteractable_GravshipShieldGenerator`；
`GET` 新增 `gravship_shield` 读数。没有新增业务 POST。

## 读取

`GET /api/v1/dlc/anomaly/interactable?thing_id=<shield>&pawn_id=<pawn>`

```json
{"interactables":[{"type":"CompInteractable_GravshipShieldGenerator",
 "label":"Activate","can_interact":true,"ticks_to_activate":120,
 "gravship_shield":{"active":false,"charging":false,"hit_points":500}}]}
```

`gravship_shield` 来自同 thing 的 `CompProjectileInterceptor`：
`active`/`charging`/`currentHitPoints`。`can_interact` 仍是原生
`CompInteractable.CanInteract`，要求已连接重力船设施、未在充能、
未激活时 hit points ≥100。

## 使用

`POST /api/v1/dlc/anomaly/interactable`

```json
{"map_id":0,"thing_id":<shield>,"pawn_id":"Human321",
 "interactable_key":"CompInteractable_GravshipShieldGenerator","dry_run":true}
```

正式调用仍由 `CompInteractable` 生成 `InteractThing` job；`OnInteracted` 按原生
`CompProjectileInterceptor.Activate()/Deactivate()` 切换。`dry_run` 不派 job。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 在原生 `Substructure` 地形上生成玩家 `GravEngine` 与该盾发生器
（`CompGravshipFacility` 连接），把新生成 interceptor 的 hit points 预置为 500
（新盾默认未充能；准备披露），选一名空闲殖民者。实测 `linked>0`、
`can_interact:true`、`active:false`；dry 保持 false；正式激活后
`active:true`、GET `gravship_shield.active` 同值；再次交互后 active 变回 false。
证据 `tools/cua/.runtime/od-r08-20261112/evidence/od-r08.json`，runner
`tests/od_r08_gravship_shield.py`，fixture `prepare_gravship_shield`/
`observe_gravship_shield`。

- 未测：真实充能 10000 tick/EMP 击晕/冷却、真实弹道拦截、断电或设施连接损坏、
  两台盾并发；本行只证明正常激活/解除与状态读回。
