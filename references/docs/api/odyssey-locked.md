# Odyssey 锁定门/箱合法解锁与机械继电器稳定器（OD-R09）

2026-11-13 新增。覆盖 Odyssey 古代设施的三类“锁定”对象与机械继电器结果：

- `AncientBlastDoor`（`Building_HackableDoor` + `CompHackable`，defence 1800）：
  合法解锁仍走原生 autoHack + `WorkGiver_Hack`；
- `AncientSealedCrate`（`Building_Crate : IOpenable`）：玩家 Open 工作
  （`JobDefOf.Open`，`ignoreDesignations`，与 `FloatMenuOptionProvider_OpenThing`
  相同）；
- `GrayDoor`（`Building_JammedDoor` + `CompLabyrinthDoor`）：玩家
  `JobDefOf.InteractThing`（`playerForced`，与 `CompLabyrinthDoor.OrderActivation`
  相同）；
- `MechRelay` + `MechStabilizer`：稳定器 `CompHackable`（defence 30000）与
  继电器 `CompMechRelay` 的 destabilize→crash 原生状态机。

旧 `AncientUplink` 代表证据保留在 `docs/api/odyssey-uplink.md`，本行不重跑。

2026-11-13 后补 V-03 已证明三稳定器正常 Hack 完成→真实 QuestPart 过滤→原生失活/
7500tick坠落→正常API攻击获得重力核心；见[正常链补验](../references/reviews/2026-11-13-odyssey-relay-normal-chain.md)。
测试准备将实例防度30000→600，未直接设置完成/发送信号/Deactivate/生成产物；下列旧局部证据保留原限定。

## 读取

`GET /api/v1/dlc/odyssey/locked?map_id=0[&thing_id=75478][&pawn_id=Human321]`

无 `thing_id` 时列出该地图所有可破解/可开/继电器对象；带 `thing_id` 返回单对象。
行：

```json
{"map_id":0,"thing_id":75478,"thing_id_string":"AncientBlastDoor75478",
 "def_name":"AncientBlastDoor","kind":"hackable_door","x":187,"z":140,
 "hackable":{"is_hacked":false,"progress_percent":0.0,"locked_out":false,
             "defence":1800.0,"intellectual_skill_prerequisite":0},
 "hackable_door":{"locked":true,"pawn_can_open":false},
 "jammed_door":null,"openable":null,"crate":null,"mech_relay":null,
 "pawn_id":"Human321","can_reach":false,"can_reach_touch":true,
 "manipulation_capable":true,"player_pawn":true,
 "can_hack":true,"hack_block_reason":""}
```

- `kind`：`hackable_door` / `crate` / `jammed_door` / `mech_relay` / `hackable`；
- `hackable` 为原生 `CompHackable`；`can_hack/hack_block_reason` 来自
  `CompHackable.CanHackNow(pawn)`；
- `hackable_door.pawn_can_open` 来自 `Building_HackableDoor.PawnCanOpen`；
- `jammed_door.jammed` 与 `can_interact/interact_reason` 来自
  `Building_JammedDoor.Jammed` 与 `CompLabyrinthDoor.CanInteract`；
- `crate` 暴露 `has_contents/contained_def/count`；
- `mech_relay` 暴露反射读取的原生 `active`、`inert`、
  `destabilization_tick/remaining`（创建后 active=true，Deactivate 后倒计时
  7500 tick 再坠落）。

## 操作

`POST /api/v1/dlc/odyssey/locked`，非 dry 需 `confirm:true`。

```json
{"map_id":0,"thing_id":75479,"pawn_id":"Human321","action":"open"}
{"map_id":0,"thing_id":75481,"pawn_id":"Human321","action":"force_open"}
```

- `open`：要求原生 `IOpenable` 且 `CanOpen`，pawn 可到达
  （`PathEndMode.OnCell`）且有 Manipulation；队列
  `JobDefOf.Open`（`ignoreDesignations=true`、`JobTag.Misc`）。返回
  `job_def/job_submitted/applied/state`；`dry_run` 返回 `would_open` 不写。
- `force_open`：要求 `CompLabyrinthDoor`；先过原生
  `CanInteract(pawn)`，再队列 `JobDefOf.InteractThing`（count=1、
  `playerForced=true`）。返回同上；`dry_run` 返回 `would_force_open`。
- 可破解门的解锁继续用 `POST /dlc/odyssey/hackable/auto` 开启 autoHack，
  由原生 `WorkGiver_Hack` 完成，再读回 `locked:false/pawn_can_open:true`。

错误：地图/对象 `404`；非 IOpenable / 非 JammedDoor `422`；`CanOpen` false、
`CanInteract` 拒绝 `409`；缺 `confirm`/`action`/`pawn_id` `400`。

## 历史证据与边界

2026-11-13 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，
invulnerability 开启）：
fixture 在殖民者附近生成原生 `AncientBlastDoor`、`AncientSealedCrate`
（原生战利品 44×GravlitePanel）与 `GrayDoor`，再生成 `MechRelay` 与
2 个 `MechStabilizer`（CompHackable defence 30000），并用与 Gravcore relay
任务相同的原生 `QuestPart_PassAny` → `QuestPart_Filter_AllThingsHackedOrDestroyed`
→ `QuestPart_MechRelay` 接线。

1. blast door：`locked:true/pawn_can_open:false`，`can_hack:true`；开启
   autoHack 后原生 Hack job 完成，读回 `is_hacked:true/locked:false/
   pawn_can_open:true`；
2. sealed crate：`can_open:true`，dry 不写；正式 `open` 提交 `Open` job，
   推进后 `has_contents:false/can_open:false`（战利品落地）；
3. GrayDoor：`jammed:true`；dry/正式 `force_open` 提交 `InteractThing`，
   推进后 `jammed:false`；
4. relay：`active:true/inert:false`；2 个 stabilizer 开启 autoHack 后原生
   `WorkGiver_Hack` 实际产生进度（tick 85974/87269 progress 0.006→0.037）；
   由于本存档仅 2 名殖民者满足 Intellectual 7，30000 防度完整破解远超本轮
   窗口，fixture 直接调用原生 `CompMechRelay.Deactivate()`（与
   `QuestPart_MechRelay` 相同的入口）验证结果：`active:false`、
   `destabilization_tick=94781`、剩余 7476；推进后 relay 消失、
   `MechRelay_Crashed75742` 落地（原生坠落结果）。

证据
`tools/cua/.runtime/od-r09-20261113w/evidence/od-r09-locked.json`
（dll `6e5c9aa2…`、fixture `2af53417…`、source `fec532d2`），runner
`tests/parallel/odyssey/od_r09_locked.py`，fixture
`tests/parallel/odyssey/OdysseyFixture.cs`（`prepare_locked`/`prepare_relay`/
`deactivate_relay`/`observe_locked`）。

- 披露边界：完整 3 稳定器→QuestPart 过滤→Deactivate 的**全程合法**
  链路未跑到 30000 防度完成（仅验证原生 Hack job 确实推进，并保留同链路
  QuestPart 接线）；destabilize/crash 由 fixture 调原生 Deactivate 触发。
  `AncientSecurityTerminal` 的门/炮塔控制、lockout 失败路径、自由任务点
  生成、继电器守卫袭击与 gravcore 提取未测。
- 现场清理：`game_exit=0`、`source_save/formal_dll/global_prefs unchanged`、
  `mod_link_removed=true`。

## 三稳定器正常链补验（V-03）

`tests/od_r09_relay_chain.py` 仅开启既有 autoHack：原生 `WorkGiver_Hack` 自主派发，
`JobDriver_Hack` 达到实例防度后发准确 `<questTag>.Hacked`。与原任务相同的
`QuestPart_PassAny` → `QuestPart_Filter_AllThingsHackedOrDestroyed` → `QuestPart_MechRelay`
真实接收信号；标签必须带本任务 `Quest{id}.` 前缀，后者是原生任务接收边界。
不增加直接失活业务入口。

隔离 run `od-r09-chain-20261113b`（rev600）passed：三台分别在77834/79011/80270tick
由Human321正常Hack完成，前两台时继电器仍active；第三台发唯一All信号，
原生设87770tick失活期限。87830读回原位200HP残骸，公开API征召/AttackMelee后90686
读回准确Gravcore1/GravlitePanel165/ChunkSlagSteel4，残骸消失。公开地图与独立观察一致。

原件 `tools/cua/.runtime/od-r09-chain-20261113b/evidence/relay-chain.json`、身份/失败/清理及
缩短工作量的完整披露见上方补验页。fixture准备母地图对象/原生QuestParts，未构造整座站点、
旅行、守卫/袭击、任务奖励/End；不将该正常核心链提升为自然取得任务后的完整战斗。
