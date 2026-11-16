# Ideology 瓜拉兰树连接读取与策略（ID-07a）

覆盖 `CompTreeConnection` 的只读状态，原生修剪滑杆/种姓菜单写入的两个策略值，以及树精“回治疗茧”原生 gizmo。
连接仪式、修剪工作和种姓更换仍由原版流程完成；本接口不直接加连接强度、不直接改 `Mode`、
不调用 `ConnectToPawn`。

## 读取

`GET /api/v1/dlc/ideology/tree?thing_id=12345`

```json
{
  "tree_thing_id": "Plant_TreeGauranlen12345",
  "thing_id_number": 12345,
  "def_name": "Plant_TreeGauranlen",
  "map_id": 0,
  "x": 170,
  "z": 88,
  "connected": true,
  "connected_pawn": "Human321",
  "connection_torn": false,
  "untorn_in_ticks": null,
  "connection_strength": 0.3762,
  "desired_connection_strength": 0.5,
  "connection_strength_loss_per_day": 0.0412,
  "connection_strength_gain_per_hour": 0.0103,
  "pruning_hours_to_maintain": 4.0,
  "has_production_mode": false,
  "mode": null,
  "desired_mode": null,
  "dryad_kind": "Dryad_Basic",
  "max_dryads": 2,
  "dryads": [],
  "gaumaker_pod": null,
  "modes": [
    {
      "def_name": "Carrier",
      "label": "carrier",
      "pawn_kind_def": "Dryad_Carrier",
      "previous_stage": null,
      "required_memes": [],
      "is_current": false,
      "is_desired": false,
      "available": true,
      "blocked_reason": null
    }
  ]
}
```

- `connection_strength`、`desired_connection_strength` 直接来自
  `CompTreeConnection`；目标强度写入使用原生 setter（0..1 截断）。
- `connection_strength_gain_per_hour` 与 `pruning_hours_to_maintain` 只在连接后返回，
  因为原生属性需要 `ConnectedPawn`；未连接时为 null。
- `mode` 是当前 `Mode`，`desired_mode` 是原生 `desiredMode`。两者不同表示连接者还在
  等待 `WorkGiver_ChangeTreeMode`，不是接口写入了当前种姓。
- `dryads` 读原生私有 `dryads` 列表，只用于核对身份；不生成、不删除树精。
- `modes[]` 的 `available`/`blocked_reason` 对应原版
  `Dialog_ChangeDryadCaste.MeetsRequirements` 加按钮门槛：上一阶段、所需 meme、
  连接者是否在树的地图上，以及“当前种姓不可重复选择”。

`thing_id` 必须是当前地图上 `Plant_TreeGauranlen` 的数字 ID。缺参 400、未知 404、
无 `CompTreeConnection` 422。

## 策略写入

`POST /api/v1/dlc/ideology/tree/policy`

```json
{"thing_id": 12345, "desired_strength": 0.42, "desired_mode": "Woodmaker",
 "dry_run": false}
```

- `desired_strength` 与 `desired_mode` 至少给一个；强度越界 400，未知种姓 def 404。
- 树必须已连接；原版修剪滑杆和种姓按钮只在 `Connected` 时出现，未连接返回 409，
  不会写入一个原版无法到达的策略。
- 种姓必须通过原生条件：`previousStage` 与当前 `Mode` 一致、连接者 ideo 具备
  `requiredMemes`、连接者在树的地图上；当前种姓不可重复选（原版显示 AlreadySelected）。
  不满足返回 422，并带 `blocked_reason`。
- 写入的就是 `DesiredConnectionStrength` / `desiredMode` 两个原生字段；之后由
  `WorkGiver_PruneGauranlenTree` 和 `WorkGiver_ChangeTreeMode` 执行实际工作。
- `dry_run:true` 返回 `before`、`requested`、`changed`、`applied:false`，不改字段。
- 正式响应在 `before` 之外给 `after`，`applied` 只在确实改变字段时为 true。

## 树精回治疗茧

`POST /api/v1/dlc/ideology/tree/dryad`

```json
{"thing_id": 88596, "return_to_healing_pod": true, "dry_run": false}
```

- `thing_id` 是 `Dryad_*` pawn 的 numeric thing ID；未找到 404，未连接到瓜拉兰树 422。
- 写入的就是原生 gizmo `CommandHealInHealingPod` 的 `mindState.returnToHealingPod`；
  受伤（`HealthAIUtility.ShouldSeekMedicalRest`）后由原生
  `JobGiver_CreateAndEnterHealingPod` 进 `DryadHealingPod`，完成后原生
  `CompDryadHealingPod.Complete` 治疗并放出树精。
- `dry_run:true` 返回 `before`、`requested`、`applied:false`；正式只切该 bool。

## 正常调用顺序

1. 用 [仪式启动](ideology-ritual.md) 的 `TreeConnection` 实例连接准确树/人物
   （`roles.connector`），等待原生仪式结束。
2. `GET .../tree` 确认 `connected:true`、`connected_pawn` 与连接者一致。
3. `POST .../tree/policy` 设 `desired_strength`。原生自动修剪要求当前强度低于目标
   至少 0.03、且距上次修剪超过 10000 tick；满足后由连接者执行
   `PruneGauranlenTree`，每 2500 tick 约提升基础 1%。
4. 需要换种姓时再 `POST .../tree/policy` 设 `desired_mode`；连接者完成
   `ChangeTreeMode`（120 tick 工作）后 `Mode` 才等于目标，之前读取会看到两者不同。
   已连接的旧种姓树精随后由原生 `JobGiver_CreateAndEnterCocoon` 走进
   `DryadCocoon`，到期由 `CompDryadCocoon.Complete` 调用 `GenerateNewDryad`
   生成目标种姓；`dryads` 在结茧期间会显示旧树精离开、完成后显示新树精，
   茧容器本身不在本接口返回。
5. 用读取接口和原生 comp 读回核对强度/目标/`Mode`，不把 HTTP 200 当作工作完成。

## 边界

- 未提供直接连接/断开、强制修剪 job、树精生成或结茧结果接口；这些依赖原生仪式、
  WorkGiver 和等待周期。
- `ConnectionTorn` 只能读取；原生断开有 1800000 tick 冷却，不能由此接口跳过。
- Gaumaker 种姓需要 `TreeConnection` meme 且至少 3 个 gaumaker 树精才会出现茧；
  本批只验证策略/原生工作接线，不把长期繁殖当完成条件。
- 连接者死亡/离开地图后的自动解除、跨地图树等未在本批实测范围。

2026-11-10 实测：原生仪式连接、目标 0.37、修剪到 0.3699、`Mode`/`DryadKind`
换为 Woodmaker（`Reverse/evidence/id07a-tree-20261111.json`，自审
[ID-07a](../references/reviews/2026-11-10-id07a-tree.md)）；旧 `Dryad_Basic88599`
在 `DryadCocoon88611` 内经原生 Complete 生成 `Dryad_Woodmaker88612`
（`Reverse/evidence/id07b-dryad-20261111.json`，自审
[ID-07b](../references/reviews/2026-11-10-id07b-dryad-caste.md)）。本页只描述 API 合同。
