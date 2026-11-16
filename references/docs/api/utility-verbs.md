# Shared 装备/背包 utility Verb（S-01）

`GET/POST /api/v1/utility/verbs`。只暴露 pawn 当前穿着装备上的原生
`CompApparelVerbOwner*` verb（不是 AbilityDef，也不构造任意 Verb）。

合同状态：**已验证**（DisruptorFlarePack、PackTurret、DeadlifePack 均实际
`Verb.TryStartCastOn` 并读回次数；EMPPulse Ability 保持独立）。

## GET

`GET /api/v1/utility/verbs?pawn_id=321`

```json
{"pawn_id":"Human321","worn_apparel":["Apparel_DisruptorFlarePack75478"],
 "count":1,"verbs":[{
   "verb_id":"Apparel_DisruptorFlarePack75478:0",
   "apparel_thing_id":"Apparel_DisruptorFlarePack75478",
   "apparel_def":"Apparel_DisruptorFlarePack",
   "verb_index":0,"label":"deploy flare",
   "verb_class":"Verb_LaunchProjectileStaticPsychic",
   "target_type":"cell",
   "can_target_pawns":false,"can_target_buildings":false,
   "can_target_locations":true,
   "range":22.9,"warmup_seconds":0.5,
   "can_be_used":true,"unusable_reason":null,
   "available":true,"remaining_charges":6,"max_charges":6,
   "needs_reload":false,"caster_pawn_id":"Human321"
 }]}
```

- `verb_id` = `apparel.ThingID:verbIndex`，稳定定位当前装备实例上的 verb。
- 只枚举 `pawn.apparel.WornApparel` 上 `CompApparelVerbOwner` 且
  `Wearer == pawn` 的 `AllVerbs`；没有装备/weapon Verb 泛化。
- charges/reload 来自 `CompApparelVerbOwner_Charged`/
  `CompApparelReloadable`；target 类型来自 `VerbProperties.targetParams`。

## POST

```json
{"pawn_id":321,"verb_id":"Apparel_DisruptorFlarePack75478:0",
 "target_type":"cell","x":168,"z":150,"confirm":true}
```

- `target_type` 为 `cell|pawn|thing`；cell 用 `x/z`（y=0），pawn/thing 用
  `target_id`；二选一。
- 先校验：verb 仍存在、targetParams 允许该类型、目标在同地图、
  `CompApparelVerbOwner.CanBeUsed`、`Verb.Available`、`Verb.CanHitTarget`；
  再调用原生 `Verb.TryStartCastOn`。不直接改 charges/不构造 Ability。
- 非 dry 需 `confirm:true`；`dry_run` 只校验并读回选中 verb，不施放。
- 返回 `cast_started`、`remaining_charges_before/after`、warmup；实际消耗由
  原生 warmup/cast 完成，GET 再读回。一次性 pack（turret/deadlife）耗尽后
  装备销毁，GET 不再列出该 verb。

## 状态码

| 状态 | 条件 |
| --- | --- |
| 200 | GET / dry-run / native cast started |
| 400 | 缺 target_type 字段、pawn/thing 参数不完整、缺 confirm |
| 404 | pawn/verb_id/target 不存在 |
| 409 | CanBeUsed false、charges/ammo 不足、不可用 |
| 422 | targetParams 不允许、跨地图、CanHitTarget false |
| 500 | 原生异常；Idempotency none |

## 验证与边界

- runner：`tests/s01_utility_verbs.py`（passed）；
  证据：`tools/cua/.runtime/s01-verbs-20261113b/evidence/s01.json`。
- fixture：`tests/parallel/shared-utility-verbs/CoreUtilityVerbsFixture.cs`
  只原生生成/穿上 apparel；不施放、不改 charges。
- 已验各一：DisruptorFlarePack cell cast 6→5；PackTurret 1→0 装备消失；
  DeadlifePack 1→0 装备消失；pawn/cell 非法 422；未知 pawn/verb 404。
- 未测：其它装备 verb 家族、reload/role lock、目标 LOS 边界、商队/世界
  holder、EMPPulse Ability（保持已验独立）。
