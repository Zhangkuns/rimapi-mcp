# Core 派系通信、交易请求、援助与 AI 核心信息（C-38）

2026-11-13 新增。复用 `TradeControlService` 的结构，但请求动作走原生
`FactionDialogMaker` 的 `DiaOption` action（与游戏内通信台对话点击同一条路径），
没有直接改 goodwill/Silver 或伪造队列。

## 读取选项（只读）

`GET /api/v1/faction/comms?map_id=<id>&pawn_id=<id>&faction_id=<optional>`

- `map_id` 缺省/0：当前地图；`pawn_id` 缺省：该地图第一个可谈判自由殖民者
  （Talking、未禁用 Social、非倒地/精神崩溃）。`faction_id` 可精确过滤
  `GetUniqueLoadID()`/名称/defName。
- `consoles`：玩家通信台 id、`can_use_now`（供电/太阳耀斑）、
  `reachable`、`reservable`。
- `factions[]`：派系 id/名称/关系/goodwill；`can_request_trader` /
  `can_request_orbital_trader` / `can_request_military_aid` /
  `can_request_ai_core` 及 `*_blockers`；可请求的 caravan/orbital
  `TraderKindDef`（含 title_required）；冷却剩余 tick 与上次请求 tick；
  军事援助 `mutual_enemies`/`requires_mutual_enemy_confirmation`；
  AI 核心门禁（goodwill≥40、可发射 Silver≥1500、ShipRelated 研究、
  AI core stash、`OpportunitySite_ItemStash.CanRun`）。

## 请求

`POST /api/v1/faction/comms/request`

```json
{"map_id":0,"faction_id":"Faction_7","negotiator_pawn_id":"Human318",
 "comms_console_id":75478,"request_type":"open_job","dry_run":true}
```

`request_type`：

- `open_job`：校验后真实创建 `JobDefOf.UseCommsConsole`（targetA=控制台，
  `job.commTarget`=派系），返回 job def/target/comm_target；dry_run 不派 job。
- `trader` / `orbital_trader`：必须给 `trader_kind`（来自读取项，精确
  defName）。使用原生 `RequestTraderOption`/`RequestOrbitalTraderOption`
  生成的 `DiaOption`，按原生 `action` 排队 `TraderCaravanArrival`（+120000
  tick）或 `OrbitalTraderArrival`（1–2 小时），写
  `lastTraderRequestTick`/`lastOrbitalTraderRequestTick` 并按原生 goodwill
  调整；盟友、温度、冷却、派系 flags 由原生 option 的 disabled 原因映射 4xx。
- `military_aid`：原生 `RequestMilitaryAidOption`。存在共同敌人时先返回
  409 `requires_confirmation` + `mutual_enemies` + 原生确认文本，不写；
  `confirm:true` 才执行原生 `CallConfirm`（goodwill、`lastMilitaryAidRequestTick`、
  真实 `RaidFriendly`/`LordJob_AssistColony`）。无共同敌人时直接执行。
- `ai_core`：原生 `RequestAICoreQuest`：goodwill≥40、无可访问 AI core stash、
  研究/任务可运行、可发射 Silver≥1500；成功扣 1500 Silver、生成真实
  `OpportunitySite_ItemStash` 任务，返回 `quest_id`/`quest_root`。
- 所有校验在写之前同步完成；失败 400/404/409/422。`dry_run` 不写、不派 job、
  不排队列。成功响应含 `before`/`after`（goodwill、可发射 Silver、请求 tick、
  incident 队列、任务、盟友 lord/信件等）。

## 证据与边界

2026-11-13 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 铺硬地并生成玩家通信台/供电轨道信标、5×500 Silver、把 The Fallen
Empire 设为盟友并清请求 tick、完成 ShipCryptosleep 研究、加一个可请求的
商队/轨道商 kind；共同敌人阶段生成 Arberium 袭击 lord。product 请求仍走原生
option/action 与真实队列/任务/到访。

- `open_job`：job `UseCommsConsole`，targetA `CommsConsole75478`，
  `comm_target Faction_7`。
- AI core：Silver 2500→1000，`quest_id=1`、root
  `OpportunitySite_ItemStash`。
- trader：tick=当前 tick，goodwill 100→81（原生实际 -19），
  `TraderCaravanArrival` +120000 tick。
- orbital：goodwill 81→43（原生 -38），队列 1–2h；推进 8000 tick 后
  `passing_ships=1`（真实船到访）。
- military 确认分支：未确认 409，不写 tick；`confirm:true` 后 tick 写入、
  goodwill 43→12（原生 -31），推进后出现 `Faction_7` 的
  `LordJob_AssistColony`（9 人）及 “Friendlies: The Fallen Empire” 信件。
- military 直接分支：清空所有活跃敌对威胁并重置 tick 后，无确认直接执行
  `RaidFriendly` 成功。
- 4xx：缺 `trader_kind` 400；即时重复 trader 冷却 422；非盟友 422；
  AI core（Silver 1000/已有 stash）422；未确认军援 409。
- 收口 `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。

披露/未测：fixture 仅布置前置（含临时改派系 def flags/关系、清活跃威胁以分离
直接/确认分支），product 不写 goodwill/Silver/tick；商队未推进 +120000 tick
（只验 tick/队列），未测 title-required 商队拒绝、派系 leader 不可用、待处理
请求的存读档。
