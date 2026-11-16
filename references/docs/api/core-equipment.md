# Core 背包/装备拿取、丢弃与给他人穿衣（C-17）

2026-11-13 新增：

- `GET /api/v1/pawn/equipment?pawn_id=`
- `POST /api/v1/pawn/equipment`

只走正常原生 job/持有者路径，不直接把 `equipment`/`apparel` tracker 当搬运结果；地面
Equip/Wear 仍使用既有 `/jobs/make/equip` 与 `/pawn/apparel/wear`。

## 读取

```json
GET /api/v1/pawn/equipment?pawn_id=315
{
  "pawn_id": "Human315", "spawned": true,
  "inventory": [{"thing_id":"MedicineIndustrial...", "thing_number":75492,
    "def_name":"MedicineIndustrial", "count":3, "spawned":false}],
  "equipment": [{"thing_id":"Gun_BoltActionRifle...", "def_name":"Gun_BoltActionRifle"}],
  "apparel": [{"thing_id":"Apparel_Jacket...", "def_name":"Apparel_Jacket",
    "forced":true}],
  "current_job": {"def_name":"TakeInventory", "target_a":"...", "count":3}
}
```

- `inventory`：原生 `pawn.inventory.innerContainer` 的精确 thing/id/def/count。
- `equipment`：原生 `equipment.AllEquipmentListForReading`。
- `apparel`：原生 `apparel.WornApparel`，`forced` 来自
  `pawn.outfits.forcedHandler.IsForced`。
- `current_job`：只读 def/targetA/targetB/count/playerForced。

## 写入

```json
POST /api/v1/pawn/equipment
{"pawn_id":315,"action":"pickup","item_id":75478,"count":3}
```

- `pickup` + `item_id` + `count`：仅地面 Item、`EverHaulable`；校验原生
  `PawnUtility.CanPickUp`、`GetMaxAllowedToPickUp`、`MassUtility.
  WillBeOverEncumberedAfterPickingUp`、reach；构造 `JobDefOf.TakeInventory`，
  `job.count=count`, `checkEncumbrance=true`, `takeInventoryDelay=120`，
  `TryTakeOrderedJob`；不直接 `TryAddItem`。读回 job/背包。
- `drop` + `thing_id`：仅该 pawn 当前持有：
  - equipped → `JobDefOf.DropEquipment`；
  - worn apparel → `JobDefOf.RemoveApparel`；若是 forced，先
    `forcedHandler.SetForced(false)` 并返回 `was_forced`/`forced_cleared`；
  - innerContainer → 原生 ITab 的 `innerContainer.TryDrop`（直接落地，明确不是
    carry job），返回 dropped id/location；
  不 broad drop，不删除其它物品。
- `dress_other` + `target_pawn_id` + `apparel_id`：仅地面 Apparel；接收者须通过原生
  `TargetingParameters.ForForceWear(actor).CanTarget(target)`，并与执行者同地图且
  可到达。原生允许站立的玩家殖民者，并非必须倒地或躺床；倒地人类、受控安全囚犯
  等分支由同一原生谓词判定，动物/机械族及不可剥衣的种类被排除。另校验
  衣物 reach/burning、`target.apparel.WouldReplaceLockedApparel`、mutant
  `disableApparel`、`ApparelUtility.HasPartsToWear`、`EquipmentUtility.CanEquip`；
  构造 `JobDefOf.ForceTargetWear`（A=target pawn，B=apparel），不直接穿。
  若目标是玩家机械师且原生 `TryGetMechsLostFromDroppingThing` 表明更换会损失
  带宽，先返回 409 且 `data.requires_confirmation=true`；同请求 `confirm:true`
  才派 job。确认是 API 的显式前置，不伪造原生 Dialog。
- `dress_other` 的 `applied:true` 表示原生 job 提交成功，`target_state_after` 是提交
  当时的即时读回。完成结果通过 `GET /pawn/equipment?pawn_id=<target>` 核对精确
  `apparel[].thing_id` 与 `forced:true`；执行者的 `current_job` 离开
  `ForceTargetWear` 后再判定完成，不将提交响应当作已经穿好。
- `dry_run` 仅预检/投影，不派 job、不写 tracker；失败在派 job 前返回 4xx，不部分改。
- 合同：GET observe；POST command/pawn_ai/validate；`Idempotency=none`（drop/
  dress 非幂等），不登记 game_memory。

## 原生来源与边界

- `Pawn_JobTracker.TryTakeOrderedJob` + `TakeInventory`/`DropEquipment`/
  `RemoveApparel`/`ForceTargetWear` job；`JobDriver_ForceTargetWear` 会设置
  `forcedHandler`；本 API 不直接改 equipment/apparel tracker。
- 背包物品掉落沿用原生 `ITab_Pawn_Gear.InterfaceDrop` 的直接 `TryDrop` 行为，
  不是搬运 job；其余 drop 走 job。
- 未测：部分堆叠/多物品 pickup、倒地/囚犯接收者穿衣完成、复杂 apparel layer/locked
  组合、机械师带宽更换完成/真实 UI 确认、死亡/精神状态、1.5。

## 证据

2026-11-13 补验 V-01（1.6.4871 rev600 / Core + 五 DLC）：
`c17-dress-v01-run3-20261113/evidence/c17-dress-v01.json`，phase passed。
公开 `dress_other` 令 Human315 给站立的 Human318 穿精确新衣
Apparel_Duster75479；tick 76787→77149（362 tick）实测经过地面→执行者
`Pawn_CarryTracker`→接收者 `Pawn_ApparelTracker`，新衣 forced=true、旧
Apparel_Jacket75478 落地、执行者 carry 清空且 job 结束。独立公开 GET 与原生
holder 读回一致。不可达接收者 dry/real 均 409，不派 job、不改衣物 holder；移除
测试墙后 dry 不改状态，正常提交完成。fixture 只准备位置、征召、食物、旧衣与地面
新衣；业务结果由原生 job 完成。准确 DLL、原始证据散列、失败保留和清理见
[补验报告](../references/reviews/2026-11-13-ideology-c17-dress-completion.md)。

以下为此前提交/拿取/丢弃证据，不扩大为所有穿衣分支的完成证明：

隔离 g35 tick 76787，五 DLC；fixture `tests/parallel/core-equipment/C17Fixture.cs`
放置地面 medicine/衣物/头set，直接布置 actor 武器/forced jacket/背包 meal，并给
mechanitor 装备 AirwireHeadset + 两台受控 Warqueen；fixture 不调用产品动作。

- GET 真实 backpack/equipment/worn apparel(forced)/current job。
- pickup dry 不派 job；实际 TakeInventory count=3，推进后背包出现 3 medicine。
- drop inner（ISub 原生 TryDrop）、drop weapon（DropEquipment）、drop forced jacket
  （RemoveApparel + forced_cleared）真实推进后消失。
- dress_other 正常目标 ForceTargetWear；机械师 Airwire→Array 先 409
  `requires_confirmation=true`，confirm 后派 ForceTargetWear。
- 非法 count/非 item/未知 thing/self-target 4xx。
- 证据：`tools/cua/.runtime/c17-run10-20261113/evidence/c17.json`（phase passed）。
