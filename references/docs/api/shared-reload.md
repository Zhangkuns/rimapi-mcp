# Shared 装备/背包换弹与兼容弹药（S-02）

2026-11-13 新增：

- `GET /api/v1/pawn/reload?pawn_id=`
- `POST /api/v1/pawn/reload`

只处理当前装备 `equipment.PrimaryEq` 与已穿 apparel 上的原生
`IReloadableComp`/`CompApparelReloadable`；不列 Ability，不做 broad reload。

## 读取

```json
GET /api/v1/pawn/reload?pawn_id=315
{
  "pawn_id": "Human315",
  "reloadables": [{
    "gear_id": "Apparel_PackJump...", "gear_number": 75478,
    "gear_def": "Apparel_PackJump", "location": "apparel",
    "ammo_def": "Chemfuel",
    "remaining_charges": 3, "max_charges": 5,
    "label_remaining": "3 / 5",
    "needs_reload": true, "min_ammo": 20, "max_ammo": 40,
    "can_use": true, "can_use_reason": null
  }],
  "current_job": {"def_name":"Reload", "target_a": ...}
}
```

- `gear_id/def/location`：装备或已穿衣物的原生 Thing；`location` 为
  `equipment`/`apparel`。
- `remaining_charges`/`max_charges`/`label_remaining`：`ICompWithCharges` 原生值。
- `needs_reload`：`IReloadableComp.NeedsReload(allowForceReload:true)`。
- `min_ammo`/`max_ammo`：`MinAmmoNeeded(true)`/`MaxAmmoNeeded(true)`。
- `can_use`/`can_use_reason`：`ICompWithCharges.CanBeUsed`。
- 不列 Ability；只读。

## 写入

```json
POST /api/v1/pawn/reload
{"pawn_id":315,"action":"reload","gear_id":75478}
```

- `gear_id` 必须精确匹配当前装备/已穿 reloadable；否则 409/422。
- 满弹或无需 reload → 409，不派空 job。
- 校验 `ReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, reloadable,
  forceReload:true)` 非空、`carryTracker.AvailableStackSpace(ammo) >=
  MinAmmoNeeded(true)`；无弹药/空间不足 409 带原因。
- 通过后只用原生 `JobGiver_Reload.MakeReloadJob(reloadable, chosenAmmo)` +
  `TryTakeOrderedJob`；不直接写 charges/ammo。
- 响应：`chosen_ammo`、`job_ammo_queue`（MakeReloadJob 构造的 targetQueueB
  ammo ids/count）、`job_count`、`before_charges`、当前 `job`；实际推进后
  读回剩余 charges 与弹药实际减少。
- `dry_run` 只校验/读回，不派 job。
- 合同：GET observe；POST command/pawn_ai/validate；`Idempotency=none`
  （reload 非幂等），不登记 game_memory。

## 原生来源与边界

- `FloatMenuOptionProvider_Reload` 的持有者资格、`FindEnoughAmmo`、
  `JobGiver_Reload.MakeReloadJob` 完全复用；不改 job 的 targetQueueB 语义。
- 装备/衣物上的 reloadable 由原生 comp 类型识别；跨 Royalty（JumpPack）、
  Anomaly/Odyssey 的 comp 走同一接口；未单独列 Ability。
- 未测：装备类 reloadable、所有 DLC reloadable 变体、carry space 边界、
  多 reloadable 同时需要、1.5。

## 证据

隔离 g35 tick 76787，五 DLC；fixture `tests/parallel/shared-reload/S02Fixture.cs`
直接穿 Royalty `Apparel_PackJump`、使用 2 次耗尽至 3/5、地面 Chemfuel 120、
装备非 reloadable gun；fixture 不调用产品动作。

- GET JumpPack 3/5、Chemfuel、needs_reload/min20/max40/can_use。
- dry 不派 job；实际 `Reload` job targetQueueB Chemfuel、job_count=40；
  推进后 charges 5/5，Chemfuel 120→80（真实消耗 40），GET needs_reload=false。
- 满弹 reload 409；错误 gear 4xx；无弹药 dry 409。
- 证据：`tools/cua/.runtime/s02-run2-20261113/evidence/s02.json`（phase passed）。
