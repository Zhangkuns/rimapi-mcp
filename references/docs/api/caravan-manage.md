# Core 商队分拆、合并、休息、觅食与成员/装备管理（C-11）

`GET/POST /api/v1/world/caravan/manage`。只处理玩家世界 `Caravan` holder
（成员、随队装备/物品，包括携带PassengerShuttle的商队）；地图 pawn、穿梭机旅行与重力船使用各自服务。

合同状态：**普通商队代表流程及C-11带船/近距离原生合并已验证**（split/merge/rest/set_policy/equip/drop 走原生
世界商队路径；forage 为自动进度 + 只读，不提供伪造产物动作）。

## GET

`GET /api/v1/world/caravan/manage?caravan_id=275`

```json
{
 "caravan_id":275,"tile":200,"faction":"PlayerColony",
 "shuttle":null,
 "members":[{
  "pawn_id":"Human321","name":"Blean","is_colonist":true,
  "downed":false,
  "equipment":[{"thing_id":"Gun_Revolver75480","def_name":"Gun_Revolver",
                "holder":"Verse.Pawn_EquipmentTracker"}],
  "apparel":[...],
  "inventory":[{"thing_id":"Steel75481","def_name":"Steel","stack_count":10,
                "holder":"Verse.Pawn_InventoryTracker"}],
  "be_carried_by_caravan_if_sick":true,
  "food_restriction":"Lavish","drug_policy":"Social drugs",
  "baby_food":[{"def_name":"BabyFood","allowed":true}],
  "can_suckle":false
 }],
 "inventory":[{"thing_id":"Steel75478","def_name":"Steel","stack_count":10,
               "owner_pawn_id":"Human321","holder":"..."}],
 "path":[200,478],"estimated_ticks":1205,
 "moving":true,"paused":false,"night_resting":false,
 "forage_food_def":"Berries","forage_per_day":10.26,
 "mass_usage":...,"mass_capacity":...
}
```

- 精确成员 equipment/apparel/inventory 及 holder；全局 inventory 带 owner。
- `shuttle` 为null或`thing_id/label/fuel/max_fuel/owner_pawn_id`；来自原生商队Shuttle缓存及准确inventory holder。
- `be_carried_by_caravan_if_sick` 来自原生 `Pawn.health` 字段；食物/药品政策
  来自原生 tracker；baby food 来自 `foodRestriction.BabyFoodAllowed`。
- `forage_per_day` 来自 `Caravan_ForageTracker.ForagedFoodPerDay`；原版
  forage 是自动 tick，不提供 POST 强制产物。

## POST

```json
{"caravan_id":275,"action":"split","pawn_ids":["Human321"],
 "items":[{"thing_id":"Steel75478","count":10}],"confirm":true}
```

非 dry 需 `confirm:true`；`Idempotency=none`。

### split

- 复制原生 `Dialog_SplitCaravan.TrySplitCaravan` 前置：新/源都必须保留至少
  一个非倒地 player owner；`pawn_ids` 必须在商队。
- items 必须精确存在于 caravan inventory，`count>0` 且 `<=stackCount`。
- 执行 `MoveAllInventoryToSomeoneElse`、逐个 `RemovePawn`、
  `CaravanMaker.MakeCaravan`、`TransferableUtility.TransferNoSplit`；返回
  `new_caravan_id` 与 source/new 读回。
- 无 pawn/非法数量 422；未确认 400。

### merge

- `other_caravan_id` 必须是另一支玩家商队。明确绑定原生`CaravanMergeUtility.CloseToEachOther`：
  同tile，或不同tile且DrawPos距离小于`AverageTileSize * 0.5`；超距409。
- 普通及携带PassengerShuttle的商队均明确调用原生私有`MergeCaravans(List<Caravan>)`，
  原生负责成员转移、源商队销毁、穿梭机处理、缓存失效与`Notify_Merged`。
  目标取成员较多者，相同则取请求的source；返回target/merged id，被合并商队GET404。
- preview/未确认响应包含`shuttle_ids`、`requires_confirmation`与`warnings`。
  两边都有穿梭机时warnings为`["multiple_shuttles"]`，其余为空；所有实际合并仍需`confirm:true`（缺失400）。
  `dry_run:true`返回`would_merge`及source/other快照，不执行合并。
- 穿梭机保留结果遵循当前版本原生方法，不另造“最多一艘”的规则；准确物品及owner从target inventory读回。

### rest

- 仅在 `pather.Moving` 时切换原生 `pather.Paused`；不能凭空制造休息/
  `NightResting`。返回 before/after moving/paused/night_resting。
- 不移动 409；未确认 400。

### set_policy

- body 可含 `be_carried_by_caravan_if_sick` 和/或
  `baby_food_def` + `baby_food_allowed`。
- 写 `Pawn.health.beCarriedByCaravanIfSick` /
  `foodRestriction.SetBabyFoodAllowed`；返回 before/after 成员读回。
- 不存在的成员 404；非法 def/缺 allowed 422；未确认 400。

### equip / drop

- `equip`：`thing_id` 必须是 caravan inventory 中 `ThingWithComps`；
  `EquipmentUtility.CanEquip` 通过后从 holder 移到
  `pawn.equipment.AddEquipment`。
- `drop`：仅当前成员已装备/穿着的 thing；移回
  `CaravanInventoryUtility.FindPawnToMoveInventoryTo` 的成员 inventory。
- 非上述路径 422；无接收成员 409；未确认 400。

## 状态码

| 状态 | 条件 |
| --- | --- |
| 200 | GET / split/merge/rest/set_policy/equip/drop accepted |
| 400 | 非法 action、缺 confirm/pawn_ids、缺 target |
| 404 | caravan/pawn/things 不存在 |
| 409 | 合并距离过远、商队不移动、无接收 inventory |
| 422 | 成员/数量/所有权/装备限制不合法 |
| 500 | 原生异常；不自动重试 |

## 验证与边界

- runner：`tests/c11_caravan_manage.py`（passed）；
  证据：`tools/cua/.runtime/c11-caravan-20261113b/evidence/c11.json`。
- fixture：`tests/parallel/core-caravan/CoreCaravanFixture.cs` 的
  `prepare_caravan_at`/`give_item` 仅测试准备，产品 route 不绕过。
- 已验：275 split Human321+Steel10→276，源保留 Human315；merge→275 members2、
  276 消失；真路径 moving 后 rest→paused true→再切回；set_policy false/true
  读回；equip revolver 到 equipment、drop 回 inventory；GET forage 10.26/day。
- 新合并runner：`tests/c11_merge_shuttles.py`、独立`tests/parallel/core-caravan-merge/C11MergeFixture.cs`；
  仅准备新人物/商队/货物及近距离行走进度，不执行合并。
  `c11-native-merge-20261113-run1/evidence/c11-native-merge.json`四组passed：普通人数相同取source；
  单船从小队并入已缓存null的大队后Shuttle正确；双船未确认400/preview不变，确认后两船均原生保留；
  相邻不同tile静止超距409，行走至距离0.09625<阈值0.24788后正常合并。
  每组准确成员/Steel28/船/fuel123/owner保持，被合并商队404；全程暂停于tick76787。
  DLL、准备与清理见[修复实测](../references/reviews/2026-11-13-api-playability-c11-merge.md)。
- 未测：baby 实际喂食策略（fixture 成人；`set_policy` 字段/原生分支已实现）、
  forage 强制产物（原生无公开 setter，明确不伪造）、多地图/存档重载。

原始缺口见[后续缺口登记](../references/reviews/2026-11-13-api-playability-gap-inventory.md)；用户已要求修复，本轮源码与结果另行登记，不改写旧失败证据。

## 与政策服务

食物/药品政策本身复用现有 FoodPolicy/DrugPolicy 服务；本端点只读写商队
holder 的成员字段，不复制政策数据库 CRUD。
