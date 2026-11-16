# Royalty 头衔读取与受限授衔（G-28）

已实现并验收：DLC-on 两个独立 fixture，27 项 notitle + 15 项 stellarch 真实断言（2026-11-05 复审后重跑）。
证据：`Reverse/evidence/dlc-G-28-notitle.json`、`dlc-G-28-stellarch.json`、
原生实测 `Reverse/evidence/dlc-G-28-runtime.json`。`G-28` 按约定范围标 implemented：
只覆盖读取与原授衔；授勋任务/仪式由 G56/G69/G81/G82 补，法令由 G83 补；仍不含奖励内容
逐项验收、义务、其它派系。

## 读取

`GET /api/v1/dlc/royalty/title?pawn_id=321`

```json
{"pawn_id":"Human321","faction_id":"Faction_7","faction_def":"Empire","faction_source":"existing_title",
 "title_def":"Stellarch","seniority":900,"favor":10,
 "titles":[{"faction_id":"Faction_7","title_def":"Stellarch","seniority":900}]}
```

`requirements`（G-55）为当前头衔的原生要求：`required_apparel`（body part groups/
required_tags/allowed_tags/required_defs）、`required_min_apparel_quality`、
`throne_room`/`bedroom`（RoomRequirement 的 type/label）、`food`（min_quality/
allowed_types/allowed_defs）、`award_thought`/`lost_thought`、`permit_points_awarded`、
`disabled_work_tags`、`max_psylink_level`、`change_heir_quest_points`、`decree_mtb_days`、
`decree_min_interval_days`、`decree_mental_break_commonality`；无头衔时 `null`。
顶层另有 `last_decree_tick`（`RoyaltyTracker.lastDecreeTicks`）。
这些是**要求声明**，不是当前房间是否满足的评估（`RoomRequirement.Met` 需要房间）。

无头衔时 `title_def=null`、`seniority=0`、`faction_id=null`、`faction_source=null`。
皇家派系取**最资深头衔**所属派系（含 Mod 多派系头衔时确定性选择）；只有许可没有头衔时
退回许可派系，`faction_source=existing_permit`。`faction_source` 说明取值来源，
不是请求参数回显。

## 受限授衔

`POST /api/v1/dlc/royalty/title`

```json
{"pawn_id":"321","title_def":"Acolyte","grant_rewards":false,"allow_downgrade":false,
 "send_letter":false,"confirm":true,"dry_run":false}
```

成功数据：
`{pawn_id,faction_id,faction_def,faction_source,previous_title_def,title_def,previous_seniority,seniority,favor_before,favor_after,changed,applied,dry_run,grant_rewards,send_letter,downgrade,titles}`。

规则（原生实测 + 2026-11-05 复审）：

- **派系解析**：不填 `faction_def` 时用 Pawn 现有皇家派系，没有则默认 `Empire`
  （`faction_source=default`）。显式填写时必须是同一派系（`existing_title+requested`），
  否则 409 并说明当前派系；未知 defName 返回 404，**不再静默忽略**显式输入。
- **同头衔不调用原生**：`SetTitle` 对同一头衔仍会再发奖励信件，因此 defName 相同时直接
  `changed:false, applied:false`，回读实际头衔，不调 setter、不发事件。
- **同 seniority 不同 Def**：原版头衔各自 seniority 唯一，该路径只在 Mod 头衔阶梯出现；
  此类横向替换不受支持，返回 409（`current_title_def`/`requested_title_def`/`seniority`）。
- **降级需显式 opt-in**：原生降级会把 favor **重置为 0** 且保留已购许可；缺
  `allow_downgrade:true` 返回 409，避免把降级当晋升。
- **奖励与信件默认关闭**：`grant_rewards`、`send_letter` 都是 opt-in；默认只改头衔。
- 非预演必须 `confirm:true`；`dry_run` 只投影，不调用 setter，`favor_after` 等于 `favor_before`。
- 未知头衔 404；非自由殖民者/动物 422；Royalty 未启用 422 `dlc_missing`。
- 原生调用抛异常时返回 500 `uncertain:true`，并**读回实际状态**：
  `observed_title_def`、`observed_favor`、`titles`、`readback_ok`（读回本身失败时
  `readback_ok:false` + `readback_error`）。

## 颁布法令（G-83）

`POST /api/v1/dlc/royalty/decree` `{pawn_id, confirm, dry_run}`

- 前置：spawned 自由殖民者、清醒、当前头衔带 `decreeTags`，且原生
  `RoyaltyTracker.PossibleDecreeQuests` 非空；否则 409/422。
- 正式调用原生 `RoyaltyTracker.IssueDecree(causedByMentalBreak:false)`：从原生
  `QuestScriptDef` 法令池按 `decreeSelectionWeight` 选一个，`GenerateQuestAndMakeAvailable`
  生成任务（`DecreeBase.autoAccept=true`，通常直接 Ongoing），更新 `lastDecreeTicks` 并发信。
- 返回 `possible_decree_count/possible_decrees`、`quest_id/quest_name/quest_def/quest_state`、
  `last_decree_tick_before/after`；dry-run 只列候选，不生成任务。
- 这是**直接触发**：不模拟原生 833-tick 的 `decreeMtbDays`/`conceited`/
  `decreeMinIntervalDays` 门禁；正常游戏仍由 `RoyalTitle.RoyalTitleTick` 随机触发。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G83 实测：Human318 授 `Knight`（`last_decree_tick=-999999`），dry-run 候选 4 个
（`Decree_ProduceItem`/`Decree_HarvestCrop`/`Decree_HuntAnimal`/`Decree_BuildMonument`）；
正式生成 `Decree_BuildMonument`（“Monument Decree”，quest 2，Ongoing），
`last_decree_tick` 74742，`GET title`/`GET quests` 读回一致。见
[G83 自审](../references/reviews/2026-11-08-g83-royalty-decree.md)。

## 幂等

`game_memory`：同 key + 同体重放返回原正文，不二次授衔、不二次发奖；
改请求体同 key 返回 409。窗口为同 Game 30 分钟/1024 键，读档/新 Game/进程重启后失效。

## 实测要点

| 场景 | 结果 |
| --- | --- |
| 无头衔授 Acolyte（奖励关/信件关） | 头衔 Acolyte、seniority 200、favor 0、信件不变 |
| 同场景奖励开/信件开 | 信件 3→4，银/物品不变（Acolyte 未定义物品奖励） |
| 同头衔再授（奖励开） | 信件 4→5 —— 原生不幂等，故 API 直接拒绝 |
| Stellarch → Acolyte（opt-in） | favor 10→0，许可 SteelDrop/FoodDrop 保留 |
| 存读档 | 头衔保留 |
| 显式 `faction_def:"PlayerColony"`（已有 Empire 头衔） | 409，不调用原生 |
| 显式 `faction_def:"NoSuchFaction"` | 404 `not_found`（复审前被静默忽略） |
| 原生 `SetTitle` 前抛异常（Harmony 注入） | 500 `uncertain`，`observed_title_def` 仍为旧头衔，调用计数 1 |
| 原生 `SetTitle` 后抛异常 | 500 `uncertain`，`observed_title_def` 已为新头衔 —— 请求值不是结果证据 |
| 同 key 重放异常结果 | 回放同一 500，原生调用计数仍为 1 |

## 未覆盖

- `awardThought/lostThought` 的心情变化、授衔产生的义务未读回。
- G83 只验 Knight 一次 `Decree_BuildMonument`；法令任务的完成/失败、后续心情/关系效果、
  其它头衔/派系未验，且直接触发绕过原生 MTB/conceited/interval 门禁。
- 只测 Acolyte/Stellarch 两个头衔；其它派系、Mod 改写的头衔未测。
- 奖励物品只观察到“无物品”的 Acolyte；有物品奖励的头衔未验证发放数量。
- 同 seniority 不同 Def 的 409 只是静态可判定路径；原版无此数据，未做原生运行验证。
- 显式 `faction_def` 指向玩家派系等“非皇家派系”时不做额外限制，行为等同原生 `SetTitle`。
