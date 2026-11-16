# Royalty 头衔继承人（RY-R03）

已实现：`GET /api/v1/dlc/royalty/heir` 与 `POST /api/v1/dlc/royalty/heir`。
原生链为 `FactionDialogMaker.RequestRoyalHeirChangeOption/RoyalHeirChangeCandidates/
RoyalHeirChangeConfirm`：按原生候选过滤选择人物，再用选中的 holder/newHeir 生成
`ChangeRoyalHeir` 任务；继承人最终由 `QuestPart_ChangeHeir` 在原任务
`MonumentCompleted` 信号后 `SetHeir`，API 不直接写 heir。

## 读取

`GET /api/v1/dlc/royalty/heir?pawn_id=Human321&faction_def=Empire`

```json
{"pawn_id":"Human321","faction_id":"Faction_7","faction_def":"Empire",
 "faction_source":"existing_title+requested","has_title_in_faction":true,
 "title_def":"Acolyte","seniority":200,"can_be_inherited":true,
 "change_heir_quest_points":1200,
 "current_heir":{"pawn_id":"Human315","name":"Youko Lewis","title_def":null,
   "seniority":0,"is_prisoner":false,"spawned":true,"dead":false,"map_id":0},
 "candidates":[{"pawn_id":"Human318","name":"Patrick Kanzler","title_def":null,
   "seniority":0,"is_prisoner":false,"spawned":true,"dead":false,"map_id":0}],
 "candidate_count":2,"in_progress":null,"can_request":true,
 "cannot_request_reason":null}
```

- 候选使用原生 `PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned` 过滤：
  排除 holder 本人、当前 heir、在该派系已有同级或更高头衔者、任务滞留者
  （`IsQuestLodger`）。返回的是**原生候选列表**，不是全殖民地。
- `current_heir` 是 `Pawn_RoyaltyTracker.GetHeir(faction)` 的读回；给一个可继承头衔
  授衔时，原生 `AssignHeirIfNone` 往往已经自动指定 heir，所以 `current_heir` 可能非空。
- `can_request=false` 时 `cannot_request_reason` 给出第一条原生原因：无该派系头衔、
  头衔不可继承、holder 是任务滞留者、或已有未完成 `ChangeRoyalHeir` 任务
  （`QuestPart_ChangeHeir.done=false` 且 holder 匹配）。
- `in_progress` 返回该任务的 id/name/def/state、任务 part 的 holder/heir/done 和
  `auto_accept=true`；没有时 `null`。

## 请求/更换

`POST /api/v1/dlc/royalty/heir`

```json
{"pawn_id":"Human321","heir_id":"Human318","faction_def":"Empire",
 "confirm":true,"dry_run":false}
```

成功数据：`applied,dry_run,would_request,holder_id,heir_id,faction_id,faction_def,
faction_source,title_def,seniority,change_heir_quest_points,representative_id,
previous_heir_id,quest_id,quest_name,quest_def,quest_state,auto_accept,
quest_heir_id,quest_part_done,heir_after`。

- 原生调用与对话框确认一致：`Slate` 写入 `points=title.changeHeirQuestPoints`、
  `asker=faction.leader ?? holder`、`titleHolder`、`titleHeir`、`titlePreviousHeir`，
  然后 `QuestUtility.GenerateQuestAndMakeAvailable(ChangeRoyalHeir, slate)`；
  非 hidden 且 `sendAvailableLetter` 时发原生任务信。
- `ChangeRoyalHeir` 的 `autoAccept=true`，所以任务直接 `Ongoing`；原版 20 天纪念碑
  超时和后续 `MonumentCompleted` 都仍由原生任务节点处理。
- 请求阶段**不改** heir：`heir_after` 仍等于 `previous_heir_id`。之后必须完成原生任务，
  `QuestPart_ChangeHeir` 收到信号才 `SetHeir`。直接 `SetHeir` 不在此 API 中。
- `dry_run:true` 只做资格/候选校验和投影，不生成任务；非 dry 必须 `confirm:true`。
- 同一 holder 已有未完成任务时再次请求返回 409，并带现有 `quest_id/quest_name/
  quest_state`；holder 本人、已有同级头衔、未知人物、非可继承头衔分别返回明确的
  422/404/409，`candidate_ids` 用于回到原生候选列表。

## 原生验证

隔离 Xvfb `g35_deathresting` 副本（五 DLC，tick 76787）：
`Acolyte` 由现有授衔端点准备；GET 显示 `can_request:true` 与原生候选；dry-run 不产生任务；
正式请求生成 `ChangeRoyalHeir`（quest 1，`Ongoing`，`quest_heir_id=Human318`，
`heir_after=Human315`）；重复请求 409。fixture 将任务掉落的标记安装并把 sketch 直接
生成完成（debug 前置），`MonumentMarker.Tick` 的 `MonumentCompleted` 信号在 tick 77547
经 `QuestPart_ChangeHeir` 把 heir 改为 Human318，part `done:true`；GET 读回
`current_heir=Human318`、`in_progress:null`、`can_request:true`。

证据：`tools/cua/.runtime/ry-r03-20261112c/evidence/ry-r03.json`
（runner `tests/parallel/royalty/ry_r03_heir.py`，fixture
`tests/parallel/royalty/fixture/RyFixture.cs`）。

未重跑：真实玩家逐格建造纪念碑的完整 20 天流程、纪念碑超时/被毁分支、囚犯候选路径、
其它可继承头衔和 mod 头衔阶梯。纪念碑的正常标记/蓝图/建造交互属于 RY-R06，不以本项的
debug 加速代替。
