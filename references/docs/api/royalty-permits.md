# Royalty 许可执行、购买与退款（G-29）

已实现并验收（`G-29` 整体仍 **partial**，只覆盖已列动作）：

| 动作 | 端点 | 证据 |
| --- | --- | --- |
| 资源空投（仅 DropResources） | `POST /dlc/royalty/permit/use` | `dlc-permit.json`、`royalty-permit-fixture.json`（28项） |
| 购买/升级（原生 `AddPermit`） | `POST /dlc/royalty/permit/purchase` | `dlc-G-29-purchase-refund.json`（30项） |
| 退款（原生 `RefundPermits`，整派系） | `POST /dlc/royalty/permit/refund` | 同上 + `-faults.json`（37项）、`-events.json`（11项）、`-guards.json`（7项） |

劳工/穿梭机/轰炸/轨道打击的执行已有 `permit/execute` 与执行状态读取，
受限 `transport/board`、`carry`、`carry/cancel`、`loading/cancel`、`travel` 已实现；均按各自范围部分交付，完整验收尚未完成，详见本文后续章节及
`docs/references/reviews/2026-11-05-G29-transport-partial-review.md`。灵能见 `royalty-control.md`。

## 许可清单读取（G-60）

`GET /api/v1/dlc/royalty/permits?pawn_id=321` 的 `permits[]` 除
`permit_def/faction_id/last_used_tick/on_cooldown` 外，G-60 增加：
`title`、`worker_class`、`worker_supported`（仅 CallAid/CallLaborers/CallShuttle/
OrbitalStrike 四类为 true）、`cooldown_ticks`、`permit_point_cost`、`favor_cost`、
`description`、`available_for_pawn`（原生 `RoyalTitlePermitDef.AvailableForPawn`）。
这让调用方在 `permit/execute` 前知道该许可的 worker 是否受支持；未支持的 worker
仍不提供执行端点。

## 端点

`POST /api/v1/dlc/royalty/permit/use`

```json
{
  "pawn_id": "321",
  "permit_def": "SteelDrop",
  "cell": {"x": 158, "y": 0, "z": 129},
  "confirm": true,
  "dry_run": false
}
```

`dry_run` 是预览：只做校验并投影，不调用原生投放、不扣声望、不进入冷却、不开 targeter。

成功数据：

```json
{
  "pawn_id": "Human321", "permit_def": "SteelDrop",
  "cell": {"x":158,"y":0,"z":129},
  "title": "Acolyte", "faction_id": "Faction_7",
  "free": true, "description": "Free",
  "favor_before": 10, "favor_cost": 0, "cooldown_ticks": 2700000,
  "items": [{"thing_def":"Steel","count":250}],
  "applied": true, "dry_run": false,
  "favor_after": 10, "last_used_tick": 71758
}
```

`applied:true` 表示原生投放链已调用且未抛异常；**不保证物品已落地**——
空投舱有落地延迟，调用方用 `GET /map/things` 或库存读回核对。

## 校验顺序与错误码

| 情况 | 状态码/`error_code` |
| --- | --- |
| 缺 `pawn_id`/`permit_def`/`cell`，非预演缺 `confirm:true` | 400 `bad_request` |
| 未知 pawn / 未知 permit Def | 404 `not_found` |
| 未持有该许可、原生不可用（含声望不足）、已有 targeter | 409 `conflict` |
| 非 DropResources 的许可、反射成员缺失 | 501 `not_supported` |
| 未生成/非自由殖民者、越界、超出 `targetingRange`、无视线 | 422 `invalid_target` |
| Royalty 未启用 | 422 `dlc_missing` |

校验在产生任何副作用之前完成：越界与超距由本端点先判定。
**原生 `OrderForceTarget` 不校验落点**——Task0 实测越界时仍扣 4 点声望且不产出资源，
这正是端点必须先校验的原因。

## 原生链路与清理

1. 用原生 `FillAidOption`（反射，私有）取得 `free` 与描述；返回 false 或没有菜单项即 409。
2. `BeginCallResources(caller, faction, map, free)`（反射，私有）初始化共享 worker；
3. 公开 `OrderForceTarget(cell)` 执行投放（原生 `CallResources` 内扣声望/刷新冷却/生成物品）；
4. `finally`：只停止**本次**开启的 targeter（入口已存在 targeter 时直接 409 且不触碰它），
   并显式清空 worker 的 `caller/map/free/faction`。

第 4 步不是可选清理：Task0 实测 `StopTargeting` 只停 targeter，**不恢复**共享 worker 字段，
残留会让下一次调用带旧上下文。共享 worker 是每 Def 一个实例，因此调用串行。

## 幂等

该端点登记为 `game_memory`：同 `Idempotency-Key` + 同请求体重放返回原正文，不二次投放；
同 key 换请求体返回 409。窗口为同 Game 30 分钟/1024 键，读档/新 Game/进程重启后失效，
**不承诺持久 exactly-once**；失效后重复投放是原生也会发生的行为，客户端需自行去重。

## 实测

| 场景 | 结果 |
| --- | --- |
| 首次免费 | 钢 +250、favor 不变、`last_used_tick` 更新、进入 45 天冷却 |
| 冷却内付费 | 钢 +250、favor −4、冷却刷新 |
| 声望不足（favor 2） | 409，钢与声望均不变 |
| 同 key 重放 | 原正文，钢不再增加 |
| 越界/超距 | 422，无副作用 |
| 存读档 | 冷却保留 |

## 未覆盖

- 其它 DropResources 许可（FoodDrop 等）只按同一 worker 类推断，未逐一运行。
- 原生 `OrderForceTarget` 内部抛异常的路径无法注入故障，未实测；`finally` 只在成功路径与
  “越界被本端点提前拒绝”路径上被观察到。
- 劳工/穿梭机/轰炸/轨道打击、被 Mod 改写的 `royalAid`、1.5 未测。


## 购买许可

`POST /api/v1/dlc/royalty/permit/purchase`

```json
{ "pawn_id": "321", "def": "CallMilitaryAidGrand", "faction_def": "Empire", "confirm": true, "dry_run": false }
```

- `faction_def` 可省略（取该 Def 的派系）；提供时必须等于 Def 的派系。
- `confirm:true` 非预演必填；`dry_run` 只出手禁与价格，不调 `AddPermit`。
- 许可点数不是扣款字段，而是派生值：`GetPermitPoints` 对每个持有许可递归扣“自身 + 前置链”成本。
  因此**升级仍消耗新 Def 自己的点数**（实测 Grand：`points 1 → 0`，与前置无关的“零净点数”是错误预期）。

成功 `data` 关键字段：`points_before/after/delta`、`favor_before/after`、`prerequisite_removed`、
`held_before/after`、`permits[]`、`ui_gates{can_draw_permit, available_for_pawn, permit_unlocked}`、
`applied`、`outcome`、`readback_ok`、`uncertain`。

`ui_gates.permit_unlocked` 是独立门禁：持有**后继**许可（替代了该前置）时，前置框仍视为已解锁，
购买必须 409（`refusal:"unlocked_by_held_upgrade"`），即使点数足够、`available_for_pawn` 仍为 true。

| 情况 | 状态码 / `refusal` |
| --- | --- |
| 缺 `pawn_id`/`def`、非预演缺 `confirm` | 400 `bad_request` |
| 无 Game | 409 `conflict`（"No active game"） |
| Royalty 未启用 | 422 `dlc_missing` |
| 未知 pawn / 未知 def / 未知 faction | 404 `not_found` |
| 非玩家自由殖民者、无 royalty、头衔不足 | 422 `invalid_target`（后两项带 `title_too_low`） |
| `faction_def` 与 Def 派系不符、无点数、已持有、前置缺失、前置被后继解锁、原生可用性拒绝 | 409 `conflict`（带 `refusal`） |
| 反射/原生 gate 成员签名变化 | 501 `not_supported`（副作用前） |

## 退款许可

`POST /api/v1/dlc/royalty/permit/refund`

```json
{ "pawn_id": "321", "faction_def": "Empire", "confirm": true, "dry_run": false }
```

原生只支持“整派系清空”，没有单条许可退款。

- `required_favor` = 原生 `TotalReturnPermitsCost(pawn)` = **8 + 所有派系中处于冷却的 royalAid 许可的
  `favorCost`**（无派系过滤，跨派系累计；SilverDrop 为 6 时 8+6=14）。
- `actual_favor_cost` = 原生确认回调实际传入的 `8`；本体只扣这个入参，**不再加冷却附加费**。
- 因此冷却时要求 12 而实际只扣 8 是原生语义，端点分别报告，不擅自“修正”。
- 退款会移除目标派系全部许可，并通过派生公式回升点数（`expected_points_delta` = 被移除许可的
  own+前置链成本之和）；无关派系的许可与冷却保留。

| 情况 | 状态码 / `refusal` |
| --- | --- |
| 缺 `pawn_id`、非预演缺 `confirm` | 400 |
| 无 Game | 409 |
| Royalty 未启用 | 422 `dlc_missing` |
| 未知 pawn / 未知 faction | 404 |
| 非玩家自由殖民者、无 royalty | 422 `invalid_target` |
| 派系无 royal titles | 409 `conflict`（`faction_has_no_royal_titles`） |
| 该派系无许可 | 409 `conflict`（`no_permits_in_faction`） |
| favor < `required_favor` | 409 `conflict`（`insufficient_favor`） |
| 反射成员缺失或 `ReturnPermitsCost != 8` | 501 `not_supported`（副作用前） |

## 执行许可 worker

`POST /api/v1/dlc/royalty/permit/execute`

```json
{ "pawn_id": "321", "def": "CallLaborerTeam",
  "cell": {"x": 132, "y": 0, "z": 93}, "confirm": true, "dry_run": false }
```

只支持四类原生 `RoyalTitlePermitWorker_Targeted`：`CallMilitaryAid*`、`CallLaborer*`、
`CallTransportShuttle*`、`OrbitalStrike*`。端点走真实菜单项 `GetRoyalAidOptions(...)` 的
`action()`，因此原生门禁（临时禁用、敌对 lord 等）生效；随后用 bounds + `CanTarget` +
`Begin*` 安装的 validator + `ValidateTarget` 校验，再 `OrderForceTarget`。

关键结果字段：

| 字段 | 含义 |
| --- | --- |
| `outcome` | `preview` / `submitted` / `aid_not_sent` / `aid_result_unknown` / `menu_refused` / `refused_by_target_checks` / `no_targeter_created` / `action_threw` / `order_threw` / `target_check_threw` |
| `submitted` | 原生 order 返回（`aid_not_sent` 也为 true：请求时间戳已写，`TryExecute` 返回 false） |
| `applied` | `submitted && !aid_not_sent`；提交后清理/读回失败仍为 true，并带 `uncertain` |
| `aid_executed` | `executed` / `not_sent` / `unknown`：来自对 `IncidentWorker.TryExecute` 返回值的被动捕获，不由 favor/冷却推断 |
| `aid_attempts[]` | 本次 `TryExecute` 的 Def 与 bool，以及 `TryGenerateRaidInfo` 的 out 列表（含仍在 drop pod 的 pawn） |
| `new_aid_pawn_ids` / `new_quest_pawn_ids` / `new_pawn_ids` | 援军 hook 的 pawn、劳工 quest/容器/requiredPawns 的去重 pawn、两者并集 |
| `new_transport_ship_ids` / `new_transport_ship_thing_ids` | 本次 `TransportShipManager.ships` 差集（loadID / shipThing ThingID）；读回失败时为 `null`（未知，不是空） |
| `quest_parts[]` / `transport_ship_parts[]` | 新 quest 的 part 投影（state/signals/transportShip/pawn 列表）与 ship→quest/part 关联 |
| `readback_ok` / `transport_readback_ok` / `readback_errors[]` | 逐 facet 读回状态；失败不把 before 回填成 after |
| `cleanup_ok` / `cleanup_verification` / `targeter_left_open` | 本次 targeter、dialog、worker 字段的清理与读回 |
| `confirmation_attempted` / `dialog_confirmed` / `dialog_text` | 按了确认键（可能随后抛）/ 回调正常返回 / 本次 dialog 文本 |

`operation_id` 为持久化 UUID（`permit-<def>-<32hex>`），记录写在每 Game 的
`PermitExecutionGameComponent` 并由 Scribe 随存档保存。`GET .../execute/status?operation_id=...`
同时返回 `submission`（提交时快照，after 字段在 `readback_ok:false` 时为 null）与
`live`（当前存档实际值），并重建 quest parts、船容器、`requiredPawns` 与 pawn 读回
（map spawned → 其它 map pawn → 容器 → world pawn）。

状态码：`aid_not_sent` 且清理/读回均正常 → 409（`uncertain:false`，明确未扣费/未冷却，
但请求 tick 可能已写）；清理或读回不确定 → 500 `uncertain:true`；捕获不到 bool →
500 `outcome:"aid_result_unknown"`，绝不按成功 200；`order-postfix` 后抛 → 500
`submitted:false` 但保留已生成对象并写 record。读取未在本地已知的 id → 404。

## 运输船只读清单（G-88）

`GET /api/v1/dlc/royalty/transport/ships`

- 枚举 `Find.TransportShipManager.AllTransportShips`（经既有字段校验 helper），返回
  `count` 与 `ships[]`：`transport_ship_id`、`def`、`started/disposed/waiting`、
  `leaving_soon_automatically`、`has_predetermined_destination`、`ship_thing_id`、
  `spawned/map_id/x/z`、`quest_tags`、`cur_job`（def/load_id/state/has_destination/
  interruptible/destination_tile/arrival_action_type/arrival_action_generates_map）、
  `contained_pawn_ids`、`required_pawn_ids`、`all_required_loaded`、
  `required_colonist_count`、`max_colonist_count`。
- 只读，不推进 ShipJob、不改容器；`all_required_loaded` 是当前容器事实，卸载后为 false
  不代表失败。用于确认船/任务/容器/requiredPawns，不替代飞行保存恢复。
- G88 实测：接受授勋任务并等 shuttle 落地后，清单 3 艘；bestowing 船
  `Ship_Shuttle`/`WaitForever`/`Quest2.Bestowing`，requiredPawns 含 bestower，容器已卸载。
  证据 `Reverse/evidence/dlc-G88-transport-ships.json`，自审
  [G88](../references/reviews/2026-11-08-g88-transport-ships.md)。
- G91 实测：同一 bestowing 船在 `game/save` + `game/load` 后清单仍 3 艘，ship
  `transport_ship_id`/`ship_thing_id`/`spawned`/`cur_job`/`quest_tags`/`required_pawn_ids`
  读回一致；只验地图上 `WaitForever` 船，不覆盖飞行跨进程/arrivalAction。证据
  `Reverse/evidence/dlc-G91-transport-save-load.json`，自审
  [G91](../references/reviews/2026-11-08-g91-transport-save-load.md)。
- G92 实测：`travel` 正式提交 `Shuttle66797`→tile 137060 后 `cur_job=FlyAway`、
  `spawned=false`、`container_pawn_ids=[]`、holder `ActiveTransporterInfo`；保存、退出进程、
  同 profile 重启并加载后，清单仍读到该船 `transport_ship_id=2`、`FlyAway`、
  `has_destination=true`、`waiting=false`、容器空。只验飞行中保存/跨进程恢复；
  `arrivalAction` 实际对象、caravan 源、落地卸载/取消未验。证据
  `Reverse/evidence/dlc-G92-travel-flight.json`、`dlc-G92-travel-restart.json`，自审
  [G92](../references/reviews/2026-11-08-g92-travel-flight.md)。
- G93 实测：加载 G92 飞行存档后清单读到 `cur_job.destination_tile=137060`、
  `arrival_action_type=RimWorld.Planet.TransportersArrivalAction_FormCaravan`、
  `arrival_action_generates_map=false`。只验 FormCaravan 一个实际对象；caravan 生成/抵达/
  落地卸载/其它 arrivalAction 未验。证据
  `Reverse/evidence/dlc-G93-flight-arrival-action.json`，自审
  [G93](../references/reviews/2026-11-08-g93-flight-arrival-action.md)。
- G94 实测：继续推进 G92/G93 飞行存档，`world/caravans` 出现 tile 137060、玩家控制、
  含 Human315 的 caravan；该船 `cur_job=null`、`spawned=false`、`disposed=false`
  （已落地/等待销毁）。只验 FormCaravan 一次抵达；地图生成型 arrivalAction、落地卸载、
  其它资格/取消未验。证据 `Reverse/evidence/dlc-G94-flight-arrival-caravan.json`，自审
  [G94](../references/reviews/2026-11-08-g94-flight-arrival-caravan.md)。

## 登船（受限）

`POST /api/v1/dlc/royalty/transport/board`

```json
{ "pawn_id": "341", "transport_ship_id": "1", "confirm": true, "dry_run": false }
```

按 `TransportShipManager.ships` 的 load id 或 `ship_thing_id` 定位已注册且已 spawn 的船，
只对玩家自由殖民者排队原生 `JobDefOf.EnterTransporter`。副作用前拒绝：倒地（`pawn_down`，
搬运/救援尚未实现）、不可达、船未落地、无 `CompTransporter`、原生
`CompShuttle.IsRequired` 与 `IsAllowedNow` 均为 false、已超载。同时给两个 ID 时必须解析到同一艘船，否则 422；任一 ID 未找到都 404。
dry-run 只投影 job 名，不分配 native job id。`job_stage` 只写实际发生的阶段：
`created`（MakeJob 成功）、`returned`（TryTakeOrderedJob 返回）或 `take_threw`（调用抛出）；
MakeJob 抛时为 `make_job_threw` 且 `job_attempted:false`（从未调用 TryTakeOrderedJob）。
`TryTakeOrderedJob` 抛异常时 `return_observed:false`、`job_accepted:null`（未取得返回值，不伪造 false）；
`queued_readback` 只匹配本次 `job_id`+target ship 与 curJob/queue 的同一实例，同 pawn 旧 job 不计，
读回自身失败时为 null 并带独立 `readback_error`，不覆盖 `job_error`。`job_queued_uncertain` → `applied:true`
（本次 job 已观察入队），`job_not_queued_uncertain` → `applied:false`，`job_unknown` → `applied:null`；
三者均 500/uncertain，且按协调器合同不发布 operation 事件。设计细节见
[运输登船 spec](../design-docs/history/2026-11-05-rimapi-G-29-transport-board-design.md)。
成功排单为 `outcome:board_queued` 且 `queued:true, applied:true, readback_ok:true`；若原生返回 true
但读回自身失败，为 500 `board_queued_readback_unknown`（`queued:null, applied:null, readback_ok:false`）；
若原生返回 true 但读回明确找不到本次 job，为 500 `board_returned_not_observed`（`queued:false, applied:null,
readback_ok:true`）。原生对同 def/同 target 的当前 job 可能提前返回 true 而不安放新实例，所以返回 true 本身
不能证明本次 job 已入队。
500 `job_queued_uncertain` 同样可能为 `queued:true, applied:true`，表示观察到本次 job 而原生返回值未取得。`queued:true` 不代表入舱完成，真实入舱/离场必须用
`GET .../permit/execute/status` 的 `transport_ships.container_pawn_ids` 与后续样本证明。
原生命中：`CompShuttle.IsRequired/IsAllowed/IsAllowedNow`、`requiredColonistCount`、
`maxColonistCount`、`CompTransporter.OverMassCapacity`；已运行验证非 required 殖民者被 422 拒绝。
已构建阈值样本经 native WaitTime 后由四参数 `SendQuestTargetSignals` postfix 捕获
`SentSatisfied`（SUBJECT=ship、SENT=四 ID），但该证据带 constructed 护理，不是自然四天全周期。

## 倒地搬运（受限）

`POST /api/v1/dlc/royalty/transport/carry`

```json
{ "actor_pawn_id": "Human66734", "target_pawn_id": "Human66746",
  "transport_ship_id": "1", "ship_thing_id": "Shuttle66751",
  "confirm": true, "dry_run": false }
```

只接受同一地图上玩家自由殖民者搬运**倒地** pawn，排入原生
`JobDefOf.HaulToTransporter`（targetA=被搬运者，targetB=船），设置
`ignoreForbidden=true`、`count=1`。若船尚未 loading，先调用
`TransporterUtility.InitiateLoading`；这会写 `CompTransporter.groupID`，是 MakeJob 前的副作用。
响应分开报告：`loading_attempted/loading_return_observed/loading_applied/group_id_before/
group_id_after/group_readback_error/loading_error` 与 `job_stage/job_id/job_target/job_ship_target/
return_observed/job_accepted/queued/queued_readback/job_applied`；`applied` 是任一确认生效的总体事实，
未知为 null。`loading_only_job_rejected` 表示 loading 已生效但 job 被拒，不能当零副作用。

dry-run 不调用 InitiateLoading、不写 groupID、不 MakeJob；`loading_attempted:false`、
`job_attempted:false`。原生菜单的 `CanReserveAndReach`/囚犯/动物/非倒地分支未实现，
本端点不声称与菜单资格完全等价。

已运行正向样本（constructed 麻醉倒地）：`groupID -1→1`、job23551 A=Human66746
B=Shuttle66751、MakeJob/Take/accepted 各1、15 段护理后入舱；Codex 限定复核 40 项
（`20261105-turn15-carry-positive-independent-review.json`）。已补 5 类故障：loading prefix/postfix 抛、MakeJob 前抛、Take prefix/postfix 抛；group 部分生效、
loading/job applied 分开、每例 fault_hits=1 且计数匹配。fresh carry 的读回故障未触发（新 job 直接
curJob，不枚举 queue），如实未测；第16轮用同 def/A/B 旧 carry job23544 真实触发：新 job23545
原生 true 但不在 current，queue 枚举抛错，500 `carry_queued_readback_unknown`，loading
false/group 不变，job/applied/queued null，旧 job 仍 current（12/12）。四例（loading-prefix/MakeJob/Take-prefix/Take-postfix）已同 Game 批量冻结重放：各自新副本、
首次 500 raw/计数、重放原 raw+replayed 头、计数/group/current/容器不增、409 不增、SSE 0 且连接
200/error None/reader alive；每 case finally pause/disarm/去麻醉核 removed/hediff（97/97）。
loading-postfix 已同 Game 冻结重放：首次 500 group -1→1、loading true、无 job；重放原
raw/status+replayed 头、四项计数与 group 不增；409 不增；SSE 0（14/14）。其余四例已补齐；readback 重放 15/15：旧 job23544 current、新23545 读回抛错，重放原 raw+header、计数/状态不增、409、SSE 0 且连接 200/error None/alive。取消/中断、carry/loading 跨进程恢复、
显式 carrier ID（后续样本补）。

## 取消（受限端点已实现，部分边界待验）

- Task3 新证据：承载副本上 `/pawn/job` Wait 真实替换 job23544 → Wait/24095，旧 carrying 清空、target 落回 Map；CancelLoad 后 group -1/loading false/容器空但立即仍 carrying，3 段短 tick 后 actor 转 GoForWalk/24096、carrying null、target Map（downed）。单船证据，不推论全组。verdict `dlc-G-29-task3-replace-cancel-verdict.json`；首跑 partial 因脚本 5/3 解包错误，非游戏失败，cleanup reset 未在该跑复核。

已取原生取消证据（`dlc-G-29-task3-cancel-native2-g29-t23-cancel2-1793943057.json`）：carried 中间态 actor Human66734、job23544 A=Human66746/B=Shuttle66751、target ParentHolder=Pawn_CarryTracker、group1/loading true/容器空。
- `EndCurrentJob(InterruptForced,false,true)`：job23544 清空，target 真实落回 map (109,56)，carrying null，group/loading 不变。
- `CompTransporter.CancelLoad()`：返回 true，group 1→-1、loading false、容器空；target 仍被 actor Carrying、job23544 仍 current，说明取消装载不等于取消搬运/放下目标。
正式端点已实现：

- `POST /api/v1/dlc/royalty/transport/carry/cancel`：actor_pawn_id、target_pawn_id、ship_thing_id、expected_job_id、confirm、dry_run，取消精确当前HaulToTransporter任务。
- `POST /api/v1/dlc/royalty/transport/loading/cancel`：ship_thing_id、nullable expected_group_id、confirm、dry_run，取消匹配的装载组。

native前/后故障与读回故障、500冻结重放已有[第32轮](../references/reviews/2026-11-05-G29-turn32-cancel-fault-review.md)和[第33轮](../references/reviews/2026-11-05-G29-turn33-readback-review-and-persistence.md)限定证据；完整事件rows、部分headers、409后计数/holder及代表性guards仍待补。
[第35轮](../references/reviews/2026-11-05-G29-turn35-three-state-review.md)已分别核验待执行/抱持/入舱跨进程恢复，[第36轮](../references/reviews/2026-11-05-G29-turn36-pending-and-full-shuttle-review.md)恢复pending后实际抱起入舱。不能把这些证据推为飞行恢复或完整运输。

## 许可船旅行（首片已实现，未完成验收）

`POST /api/v1/dlc/royalty/transport/travel`

- 请求：`ship_thing_id`、nullable `destination_tile`、`confirm`、`dry_run`，可选 `arrival_action`（`attack`/`visit`/`trade`；Settlement 默认敌对方 `attack`、否则 `visit`）。缺tile为400；tile0可表达并继续原生资格检查，不因0直接拒绝。实际tile0运行样本未验。
- 首片仅permitShuttle、已loading/容器非空、精确当前Wait及enabled Launch，目的地须通过原生距离/CanForm/唯一enabled FormCaravan选项。其他原生目的地分支仍待后续实现。
- 已有targeter/装载dialog拒绝；preview共享签名/目标检查，不调用Launch或菜单action。内部查询缓存的严格不变门禁尚未验证，不能声称所有内部状态零写入。
- 正式本次Launch → WorldTargeter目标校验/单次callback → 实际job/holder读回；`submitted/applied`不是抵达判据。正常目标拒绝422，原生兼容预检不支持501；异常/读回/清理不确定500，保留已知事实和错误。
- 第49轮曾有正式正向到Caravan276/tile137060/Human315的单样本；[正式旅行复审](../references/reviews/2026-11-05-G29-turn49-formal-travel-review.md)P1–P5尚未全部关闭。第53–63轮修复预检与owner，P1/P2和修复后正式fault/重放409/SSE仍待验，不能据首样本宣称已完成。
- **RY-02 正常装载→旅行→抵达（2026-11-08）**：`ai_g29_shuttle` 副本的已生成玩家
  `Shuttle66735`，先用产品 `/transport/board` 正常登入 Human321（原生 `accept_colonists=false`
  但 `is_allowed_now=true`，容器随后含该 pawn），再用 `/transport/travel` `arrival_action=visit`
  到 Blue Cervexa（tile 47063，distance 27）：preview/applied 均 `settlement_landing=true`、
  `settlement_arrival=visit`、container 保持 Human321；提交后 ship `FlyAway`/destination 47063/
  `interruptible=false`（原生），推进后 Caravan 276 在 tile 47063、成员 [321]，ship 不再生成。
  证据 `Reverse/evidence/an05-ry02-transport.json`，runner `tests/ry02_transport_load_travel.py`。
  边界：多人/货物混合装载、取消后的释放、贸易抵达与 FlyAway 取消仍按各自原生限制/既有证据。
- G103 扩展：目的地为敌对方、无地图的 `Settlement` 时，`travel` 走原生
  `AttackShuttle` 单选项与确认对话框，提交
  `TransportersArrivalAction_TransportShip`（`GeneratesMap=true`）。实测
  `Shuttle66797`→tile 156244（Thaludig）：`applied/submitted` true、新地图生成、
  船落在新地图、`ShipJob_Unload` 后 Human315 出现在新地图。只覆盖敌对方、无地图、
  `CanAttack`、单一 AttackShuttle 选项；非敌对 Visit/Trade、多选项 FloatMenu、
  飞行取消/中断未验。见 [G103 自审](../references/reviews/2026-11-08-g103-transport-settlement-landing.md)。
- G105 扩展：非敌对 Settlement 可用 `arrival_action:"visit"` 走原生 FloatMenu 的
  `VisitSettlement` 选项，提交 `TransportersArrivalAction_VisitSettlement`
  （`GeneratesMap=false`），抵达后形成玩家 caravan。实测 `Shuttle66797`→tile 47063
  （Blue Cervexa，距离 27）：`applied/submitted` true、选项标签 `Visit Blue Cervexa`、
  船离图，tick 75494 形成 caravan276/tile47063/Human315；飞行取消/中断未验。见
  [G105 自审](../references/reviews/2026-11-08-g105-transport-settlement-visit.md)。
- G107 扩展：非敌对 Settlement 可用 `arrival_action:"trade"` 走原生
  `TradeWithSettlement` 选项，提交 `TransportersArrivalAction_Trade`
  （`GeneratesMap=false`）；抵达形成 caravan 后原生打开 `Dialog_Trade`，API 在提交后
  返回，不等待交易窗口。实测 Shuttle66797（补登 Human321）→Blue Cervexa tile47063：
  `applied/submitted` true、选项标签 `Trade with Blue Cervexa`、船离图，caravan 抵达后
  `trade_dialog_open=true`，fixture 关闭后 false。见
  [G107 自审](../references/reviews/2026-11-08-g107-transport-settlement-trade.md)。

详细边界及最新续接见[旅行设计](../design-docs/history/2026-11-05-rimapi-G-29-full-shuttle-travel-design.md)和[DSH-NEXT](../references/session-history/DSH-NEXT.md)。

## 失败语义与幂等

- 购买：只有“新增目标 + 旧前置按预期替换 + 点数 = −ownCost + 好感不变 + 无关许可/冷却不变”才 `applied:true`。
- 退款：只有“目标派系清空且 removed 恰等于 before 集合 + 扣费 = actual + 无关派系保留 + 派生点数吻合”才 `applied:true`。
- 执行前抛 → 500 且 `applied:false`；执行后抛 → 500 且 `applied:true` + `native_error`，不要重试；
  读回抛 → 500、`outcome:"unknown"`、`uncertain:true`、无 `applied`。
- 两端点均 `game_memory` 幂等：同 key 同 body 重放原状态码/正文（含 500 正文）+ `Idempotency-Replayed:true`；
  响应头不冻结，重放时重新生成。不二次执行；同 key 换 body → 409。窗口同 Game 30 分钟/1024 键，
  读档/新 Game/重启失效。
- 成功执行会发一条 operation 事件；预演与拒绝不发（实测 11 项事件断言）。

### 执行状态中的不可观测 pawn

当记录中的 pawn 当前无法解析时，`observable:false`、`resolution:unknown`，
`spawned/dead/player_faction/free_colonist` 为 `null`。缺失不表示死亡或存活；
不能用 `dead != true` 判断返程存活。该修正已对已离场存档的四个指定 ID 独立运行验证，见
`Reverse/evidence/dsh-supervision/new-session-turn9-null-facets-independent-review.json`。


## 正常任务穿梭机发送、自动装载和遣返（C-12）

`GET /api/v1/dlc/royalty/transport/ships` 新增每船 `actions`（可用与
`disabled_reason`）、`auto_load`、`accept_colonists`、`required_items`、
有效 `required_pawn_ids`、准确 `contained_things/contained_pawn_ids`。

`POST /api/v1/dlc/royalty/transport/ships/action`：

```json
{"ship_thing_id":"Shuttle123","action":"send","confirm":true}
{"ship_thing_id":"Shuttle123","action":"dismiss","confirm":true}
{"ship_thing_id":"Shuttle123","action":"auto_load","auto_load":true}
```

固定复用 `CompShuttle.CompGetGizmosExtra` 与 `ShipJob_Wait` 的正常原生命令，
按原生 ShowGizmos/Disabled 资格执行。`send/dismiss` 实际执行要求confirm；
`auto_load` 必须提供布尔目标值，相同目标值不再次翻转。`dry_run:true` 不调用命令。
动作当前不存在/原生禁用409，缺参数400，对象不存在404。支持game_memory幂等重放。

`applied` 表示原生命令已执行，返回 `ship_job_before/after`、`auto_load` 和容器；
发送、卸客、离场的最终结果需后续查询船、人物、任务信号。
皇家结局招待完成后装载/发送同一路径，胜利后续走[结局合同](core-game-ending.md)。
真实 Steel20 自动搬入、Send离场、已登机许可客人Dismiss卸客离场与
Royal Ascent EndedSuccess 见[验收及身份](../references/reviews/2026-11-13-api-playability-endings-transport.md)。
