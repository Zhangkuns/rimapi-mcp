# Biotech 机械师与机械体（G-34）

已交付限定范围：修复后DLC-on独立fixture，120项真实游戏检查通过；证据
`Reverse/evidence/dlc-mech-review.json`。仅Biotech生效，路径以`/api/v1`为前缀。

适用范围：本地图上已生成的玩家机械师（有`MechlinkImplant`的自由殖民者）与玩家机械体。
不安装mechlink、不生成机械体、不转移世界/商队中的机械体、不偷取他人已控制的机械体。

## 读取

`GET /dlc/biotech/mech?pawn_id=321`

```json
{
  "pawn_id": "Human321",
  "used_bandwidth": 6,
  "total_bandwidth": 6,
  "mechs": [
    {"mech_id": "Mech_Lifter58071", "label": "Lifter 1", "overseer_id": "Human321",
     "group_index": 1, "current_job": "GotoWander", "bandwidth_cost": 1,
     "work_mode_def": "Work"}
  ],
  "groups": [
    {"group_index": 1, "mode_def": "Work", "mech_ids": ["Mech_Lifter58071"]}
  ]
}
```

`mechs`列出本地图全部玩家机械体，未绑定的`overseer_id`为`null`，便于调用方发现可绑定目标；
`groups`列出该机械师全部编队（空组也会出现，`mech_ids`为空）。
`bandwidth_cost`来自原生`StatDefOf.BandwidthCost`；`work_mode_def`是该机械体实际生效的模式。

## 绑定（原生Job）

`POST /dlc/biotech/mech/bind`

```json
{"pawn_id":"321","mech_id":"Mech_Lifter58071","dry_run":true}
```

成功数据为
`{pawn_id,mech_id,mechanitor_id,target_mech_id,queued,changed,dry_run,used_bandwidth,total_bandwidth,reason}`。

- `queued:true`只表示**原生`ControlMech` Job已排队**，不是已绑定；`GET`的`overseer_id`才是实际归属。
- Job完成时由原生`JobDriver_ControlMech`建立Overseer关系与编队；接口不写relations、不强制转移。
- 已属于该机械师：`changed:false,queued:false`；属于其他机械师：409 `conflict`。
- `can_control`被原生拒绝（含带宽不足）：409，`reason`带原生原因。
- 机械体不在同一地图/非玩家机械体/不可控制：422 `invalid_target`。
- `dry_run:true`执行同样校验但不创建Job；不改带宽、不产生operation事件。
- 目标机械体不可达或Job被打断时，`queued:true`与最终归属无关；以`GET`为准。
- 当前或队列中该机械师已有ControlMech，或其他角色正绑定该机械体时返回409，预演也遵守此门禁。
  同一key仍重放原响应；无key/不同key不会替换尚未完成的绑定Job。

实测：本机6带宽机械师逐一绑定7台Lifter，前6台各消耗1带宽后第7台返回409；
同一`Idempotency-Key`重放返回原正文且只发一次operation；存读档后归属与编队保持。

## 首领组召唤（BT-12c）

`GET /api/v1/dlc/biotech/bossgroup` 读 `lastBossgroupCalled` 与各
`BossgroupDef` 的 `NumTimesCalledBossgroup`（只读，不触发召唤）。

使用 `POST /pawn/use-item` 对 `BurnoutMechlinkBooster` 等
`CompUsable` + `CompUseEffect_CallBossgroup` 设备：

```json
{"pawn_id":"321","thing_id":"BurnoutMechlinkBooster75478","confirm":true}
```

- 未确认时 409 返回原生 `CompUseEffect.ConfirmMessage`；确认后提交原生
  `useJob`（`TriggerObject`），由 `CompUseEffect_CallBossgroup.DoEffect`
  调用 `BossgroupWorker.Resolve`；服务不直接写调用次数/不直接生成首领。
- 修复：API 确认后不再调用会二次弹窗的 `TryStartUseJob`，而是按
  `CompUsable.StartJob` 构造同一 use job。
- 实测 Human321 使用 BurnoutMechlinkBooster：Warqueen 0→1、last tick 77687、
  装置随后 `DestroySelf`；未知 thing 404（`Reverse/evidence/bt12c-bossgroup-20261111.json`，
  自审 [BT-12c](../references/reviews/2026-11-10-bt12c-bossgroup.md)）。
- 边界：只测Warqueen；Diabolus/Apocriton与实际战斗未验。后续Transponder解密/MechanitorShip任务已补，见文末，不称完成科技。

## 释放/解除控制（BT-11g）

`POST /dlc/biotech/mech/release`

```json
{"pawn_id":"321","mech_id":"Mech_Lifter75478","dry_run":true}
```

- 只允许机械师释放自己控制的、同图 spawned 玩家机械体
  （`GetOverseer(mech)==pawn`）；非本人机械 422、未知 404、重复 422。
- 正式路径直接调用原生
  `MechanitorUtility.ForceDisconnectMechFromOverseer(mech)`，读回
  `overseer_after`/`used_bandwidth_after`；不手写 relations 或带宽。
- 实测 Human321 释放 Mech_Lifter75478：overseer 从 Human321→null，带宽 1→0，
  机械体仍 spawned；dry 不改状态（`Reverse/evidence/bt11g-mech-release-20261111.json`，
  自审 [BT-11g 释放](../references/reviews/2026-11-10-bt11g-mech-release.md)）。
- 边界：释放后重绑、战斗中释放、跨地图/商队未验；拆解/安装/带宽装备后续结果见本页下文。

## 修理（BT-11g）

`POST /dlc/biotech/mech/repair`

```json
{"pawn_id":"321","mech_id":"Mech_Scyther75478","dry_run":true}
```

- 机械师必须同图、带 Mechlink、**已征召**，且能到机械体的 Touch 格；目标必须是
  同图玩家机械体且 `MechRepairUtility.CanRepair`（受损、有能量）。
- 提交原生 `JobDefOf.RepairMech`；HP 恢复和机械体能量消耗由原生
  `JobDriver_RepairMech`/`RepairMechUtility.RepairTick` 完成，服务不写 HP。
- 未征召 422、未知机械 404、满血/不可修 409；`dry_run` 只校验并投影。
- 实测 BT-11g（2026-11-10）：Human321 修理 Human 的
  `Mech_Scyther75478`，HP 0.6303→1.0、energy 1.0→0.8484，tick 82908
  `can_repair:false`（`Reverse/evidence/bt11g-mech-repair-20261111.json`，自审
  [BT-11g](../references/reviews/2026-11-10-bt11g-mech-repair.md)）。
- 边界：修理只测一个Scyther；拆解/解除控制/Mechlink安装/带宽装备已由独立后续样本补证；控制组数原生自动管理。

## 整组工作模式

`POST /dlc/biotech/mech/mode`

```json
{"pawn_id":"321","group_index":1,"mode_def":"Recharge","dry_run":false}
```

成功数据为`{pawn_id,group_index,previous_mode_def,mode_def,affected_mech_ids,changed,dry_run}`。
`group_index`必须取自`GET`返回的`groups[]`，接口不猜起点；未知组404。
模式作用在整组，不是单个机械体：`affected_mech_ids`列出该组当前成员。

- 支持`Work`、`Escort`、`Recharge`、`SelfShutdown`（原生四个`MechWorkModeDef`）；
- **`Escort`不需要目标参数**：原生机械体思维树的
  `ThinkNode_ConditionalWorkMode(Escort)` 运行 `JobGiver_AIFollowOverseer`/
  `AIDefendOverseer`/`AIWaitWithOverseer`，跟随该组自己的机械师。旧版“需要目标合同”的501
  是按旧注释推断的伪门禁，已移除；不新增目标字段；
- 未知`mode_def`返回404；值相同`changed:false`且不发operation；
- 实测同一机械师的另一个空组、另一名机械师的编队均不受影响；存读档后模式保持。

### Escort 实测（BT-01，2026-11-08）

`ai_mech_fixture`（tick 70392，7 个未受控玩家 Mech_Lifter、两名机械师空组）：先用产品
`/mech/bind` 正常 ControlMech 绑定 `Mech_Lifter58071` 到 `Human321`（组 1），再
`mode_def=Escort`（previous Work → Escort，affected 该组）；用 `/pawn/order` goto 让机械师走到
(171,127)，推进 3,700 tick 后机械体在 (166,129)、`mode=Escort`、与机械师距离 5，
原生 `Wait_Wander` 陪伴工作；组 2 仍为 `Work`，保护仍启用。证据
`Reverse/evidence/an05-bt01-escort.json`，runner `tests/bt01_mech_escort.py`。
边界：跨地图/商队 Escort、编队重排、战斗防御分支未验。

## 错误与DLC门

| 情况 | 状态码/`error_code` |
| --- | --- |
| 缺`pawn_id`/`mech_id`/`group_index`/`mode_def` | 400 `bad_request` |
| 未知pawn/机械体/组/模式Def | 404 `not_found` |
| 已被他人控制、带宽不足、绑定Job待完成、Job无法排队 | 409 `conflict` |
| 非机械师、动物、非玩家或跨地图机械体 | 422 `invalid_target` |
| 未知/不支持的 `mode_def` | 501 `not_supported` |
| Biotech未启用 | 422 `dlc_missing` |

## 未覆盖

- 未在1.5或任意Mod下验证；Escort正常桌面跟随已验，跨地图/商队/世界及战斗防御仍未验。
  mechlink安装与控制组成员迁移/阈值已有后续成果，见[原53计划BT-11f/g](../exec-plans/active/five-dlc-remaining.md)；
  不再列为未实现。组数由原生stat自动管理，不造任意建组/删组端点；明确的新剩余见[BT-R10等](../product-specs/five-dlc-remaining-checklist.md)。
- 未注入敌方目标拒绝、同目标由其他机械师竞争的运行分支；代码有派系与待完成Job门禁。
- Job中断只以`queued:true`后`GET`仍为未绑定表达；未做真实"打断后重试"注入。
- 机械师带宽与编队数量由原生决定，接口不修改；`SelfShutdown`的后续休眠由原生执行。

## 控制组员移动与充电阈值（BT-11f，2026-11-10）

`GET /api/v1/dlc/biotech/mech/groups?pawn_id=`

返回 `total_bandwidth`/`used_bandwidth`、每个 `control_groups` 的
`index/work_mode/members/recharge_min/recharge_max`，以及地图上全部玩家机械体的
`overseer_id/group_index/mode`。组由原生带宽/控制组规则创建；本接口不创建或删除组。

`POST /api/v1/dlc/biotech/mech/group/assign`

```json
{"pawn_id":"321","mech_id":"Mech_Scyther75479","group_index":2,"dry_run":false}
```

- 机械体必须由该机械师实际监管（`MechanitorUtility.GetOverseer`），且 `group_index`
  必须是 `GET` 返回的已有组；其它值 404。
- 确认后调用原生 `MechanitorControlGroup.Assign(mech)`：从其它组移除、加入目标组，
  并应用该组的原生工作模式；返回 `previous_group_index`、`changed`、before/after 与全组列表。
- 已在目标组时 `changed:false`，不重复写。

`POST /api/v1/dlc/biotech/mech/group/recharge`

```json
{"pawn_id":"321","group_index":1,"recharge_min":0.25,"recharge_max":0.75,"dry_run":true}
```

- 写原生 `MechanitorControlGroup.mechRechargeThresholds`（`Dialog_RechargeSettings` 同字段）；
  要求 `0 <= min <= max <= 1`，否则 400；dry-run 只回显 before/计划值。
- 精确合同：GET 为 `observe`；两个 POST 为 `policy/pawn_ai/validate/game_memory`。

证据：`Reverse/evidence/bt11f-mech-groups-20261110.json`（正常绑定两只机械体、
组 1/2 读取、真实移动到另一组并移除原组成员、组模式继承、阈值写入/读回、dry-run、
非法范围 400、未知组 404）。边界：组创建/删除与跨机械师移动仍按原生规则，不提供 API；
跨地图/商队中的机械体未覆盖。


## 机械体拆解（BT-11g）

`POST /api/v1/dlc/biotech/mech/disassemble`

```json
{"pawn_id":"321","mech_id":"Mech_Scyther75478","dry_run":true}
```

校验 mechanitor 是该 mech 的 overseer、同图、存活、可 Touch；返回
`JobDefOf.DisassembleMech` 与 `MechanitorUtility.IngredientsFromDisassembly`。
正式调用原生 job：300 tick 后掉 Steel 30/Plasteel 30/Component 2/Subcore 1，
mech 死亡且尸体销毁。实测 Scyther75478 tick 77844 销毁，Steel 1175→1205、
Component 30→32（`Reverse/evidence/bt11g-disassemble-20261111.json`，自审
[BT-11g](../references/reviews/2026-11-10-bt11g-disassemble.md)）。
边界：只测一只Scyther/一次，取消/中断未验；Mechlink/带宽和控制组容量后续结果见下文。


## Mechlink 安装与带宽（BT-11g）

`POST /api/v1/pawn/use-item` 可直接使用原生 `Mechlink` 物品：

```json
{"pawn_id":"321","thing_id":"Mechlink75478","confirm":true,"dry_run":true}
```

产品走 `CompUseEffect_InstallImplantMechlink` 的 `InstallMechlink` job；完成后
`MechlinkImplant` 与 `Pawn_MechanitorTracker` 由原生 Hediff 创建。`GET
/dlc/biotech/mech?pawn_id=321` 读回基础 `total_bandwidth`/`used_bandwidth`。
实测 Human321 tick 77552 安装、item 消耗、total 6/used 0
（`Reverse/evidence/bt11g-mechlink-20261111.json`，自审
[BT-11g mechlink](../references/reviews/2026-11-10-bt11g-mechlink.md)）。
边界：基础带宽成立；后续装备/控制组容量见下文。MechbandDish是Apocriton召唤建筑；已有Mechlink/禁用确认未验。


### 带宽装备（BT-11g）

`POST /api/v1/pawn/apparel/wear` 穿原生 `Apparel_ArrayHeadset`
（`equippedStatOffsets/MechBandwidth=6`）后，`GET /dlc/biotech/mech` 的
`total_bandwidth` 由 6 升到 12。实测 Human321、headset75500
（`Reverse/evidence/bt11g-bandwidth-20261111.json`，自审
[带宽装备](../references/reviews/2026-11-10-bt11g-bandwidth.md)）。
后续Airwire/Integrator穿戴后带宽分别为9/15，脱卸均回6（`bt11g-bandwidth-variants-20261111.json`，
[自审](../references/reviews/2026-11-10-bt11g-bandwidth-variants.md)）；wear为direct_native装备调用，不冒充穿戴job。
其它装备/强制脱卸/人物变体未验；MechbandDish是Apocriton召唤建筑，不是带宽设施。


### 控制组容量（BT-11g）

`GET /dlc/biotech/mech` 返回 `total_available_control_groups`，取
`Pawn_MechanitorTracker.TotalAvailableControlGroups`（即
`StatDefOf.MechControlGroups`）。组数由 stat 变化自动补齐/移除，原生没有
“新增/删除控制组”玩家菜单；可控操作是 `POST .../group/assign` 把机械体移入
已有组（BT-11f）。实测 Human321：total 2、used 0、groups 1/2 Work
（`Reverse/evidence/bt11g-control-groups-20261111.json`，自审
[控制组](../references/reviews/2026-11-10-bt11g-control-groups.md)）。
边界：基础组数；stat 改变后的自动迁移未验。

## Transponder 解密与研究核心的区别（BT-12c）

`POST /pawn/use-item {pawn_id, thing_id, confirm:true, dry_run?}` 可使用准确的
`MechanoidTransponder`。原生 `CompUseableDatacore.GetExtraTarget` 选择可达、可预留的
研究台，`ReadDatacore` 的 target A 是物品、target B 是研究台；没有研究台返回 422。
工作完成后物品消耗并生成 `MechanitorShip` 任务，且更新原生 datacore 历史。
这条链没有“完成当前科技”的效果。`TechprofSubpersonaCore` 才调用研究完成效果；
旧 Smithing 样本属于 Techprof，不能代替 Transponder/研究台/任务链。
新样本与调试前置见[缺口修复自审](../references/reviews/2026-11-11-five-dlc-gap-repair.md)。
