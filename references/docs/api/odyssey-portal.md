# Odyssey MapPortal 多人/物品装载、等待与取消（OD-R05）

2026-11-13 新增。`MapPortal`（如原生 `InsectLairEntrance`）的装载流程：
读取 `leftToLoad`/lord/装载 job，复刻
`Dialog_EnterPortal.TryAccept`（重置 `leftToLoad`、逐项 `AddToTheToLoadList`、
`EnterPortalUtility.MakeLordsAsAppropriate`），编辑单项数量，以及原生
`MapPortal.CancelLoad`。搬运、进图、lord 状态机与 pocket map 生成全部由游戏
原生工作完成，端点只发起/读回。

## 读取

`GET /api/v1/dlc/odyssey/portal?portal_id=InsectLairEntrance75478[&map_id=0]`

带 `portal_id` 返回单个 portal 详情；省略时返回当前地图（或 `map_id`）的
portal 列表。

```json
{"portal_id":"InsectLairEntrance75478","def_name":"InsectLairEntrance",
 "map_id":0,"x":169,"z":25,"enterable":true,"enter_block_reason":"",
 "load_in_progress":true,"left_to_load_count":2,
 "any_pawn_can_load_now":true,"lord_id":"2","lord_pawn_count":2,
 "left_to_load":[{"index":0,"thing_def":"Steel","label":"steel",
                  "count":30,"max_count":50,
                  "things":[{"thing_id":"Steel75479","stack_count":50}]}],
 "loading_pawns":[{"pawn_id":"Human321","job_def":"HaulToPortal",
                   "carried_thing_id":"Steel75479","carried_count":50}],
 "lord_pawn_ids":["Human321","Human315"],"lord_job_type":"LordJob_LoadAndEnterPortal",
 "pocket_map_exists":true,"pocket_map_id":1,
 "pocket_pawn_ids":["Human315"],"pocket_item_count":26}
```

- `left_to_load`：原生 `leftToLoad` 每行一项，`count/max_count` 与
  `things[].stack_count` 分开；`things` 是该 transferable 关联的精确堆。
- `loading_pawns`：当前 `HaulToPortal`/`EnterPortal` 且 driver 指向该 portal
  的 pawn，附手持物品与数量。
- `enterable`/`enter_block_reason`：调用原生 `IsEnterable(out reason)`。
- pocket map 存在时追加 `pocket_map_id`、`pocket_pawn_ids`、
  `pocket_item_count`。

## 管理

`POST /api/v1/dlc/odyssey/portal`

```json
{"portal_id":"InsectLairEntrance75478","action":"load","confirm":true,
 "pawn_ids":["Human321","Human315"],
 "items":[{"thing_ids":["Steel75479"],"count":30},
          {"thing_ids":["Silver75480"],"count":20}]}
{"portal_id":"InsectLairEntrance75478","action":"set_count","confirm":true,
 "items":[{"thing_ids":["Steel75479"],"count":10}]}
{"portal_id":"InsectLairEntrance75478","action":"cancel","confirm":true}
```

- 所有写操作需 `confirm:true`；`dry_run:true` 只校验并返回计划
  （`applied:false`、`would_load`/`would_cancel`），不产生 lord 或队列。
- `load`：要求 portal 可进入；`pawn_ids` 必须来自
  `CaravanFormingUtility.AllSendablePawns`，`items` 的 `thing_ids` 必须是该
  portal 地图上可到达的殖民地物品且 `count` 在所选堆存量内。写路径清空
  `leftToLoad` 后按顺序 `AddToTheToLoadList`，再
  `MakeLordsAsAppropriate`；返回 `pawn_ids/item_rows/lord_id/lord_pawn_ids/
  left_to_load/any_pawn_can_load_now`。
- `set_count`：恰好一行 `items`，按精确 `thing_id` 找到 `leftToLoad` 行，
  `AdjustTo(count)`；`count=0` 时从列表移除。返回 `old_count/new_count/
  left_to_load`。用于装载开始前或装载中修改计划数量。
- `cancel`：要求 `load_in_progress`；调用原生 `CancelLoad`（移除
  `LordJob_LoadAndEnterPortal` lord 并清空 `leftToLoad`），读回
  `left_to_load:[]`、`lord:null`、`any_pawn_can_load_now:false`。
- 合同：`GET` observe/none；`POST` direct/immediate/validate（待协调者登记
  `Core/Ai/AiEndpointContracts.cs`）。

错误：portal 不存在 `404`；`action` 缺失/非法、缺 `confirm`、`count` 越界
`400`；无装载可取消、无 `leftToLoad` 行 `409`；不可进入、pawn 不可派遣、
物品不可达/超过堆量 `422`；Odyssey 未激活 `422 dlc_missing`。

## 证据与边界

2026-11-13 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，私有无头桌面）：
fixture 生成原生 `InsectLairEntrance` 与玩家 Steel 50 / Silver 40 两堆，
invulnerability 开启。流程与读回：

1. dry `load` 30+20：`applied:false`、`item_rows` 正确，portal 仍空、无 lord；
2. 正式 `load` 30+20：`lord_id=2`、两行 `left_to_load`、两名 pawn 进入
   `HaulToPortal`，`any_pawn_can_load_now:true`；
3. `set_count` Steel 30→10 dry/正式：读回 10/20；
4. dry/正式 `cancel`：lord 消失、`left_to_load:[]`、`load_in_progress:false`；
   推进后装载 job 清空，两堆 cargo 仍在原图（50/40）、两名 pawn 仍在原图；
5. 重新 `load` 50+40：`lord_id=3`，两段各 1200 tick 推进后 `left_to_load:[]`、
   pocket map（`map_id=1`）出现，Steel 50/Silver 40 与两名 pawn 均在 pocket
   map，原图 cargo 不再存在，`pocket_item_stacks` 精确 50/40，lord 结束。

证据
`tools/cua/.runtime/od-r05-20261113h/evidence/od-r05-portal.json`
（dll `2fac48ec…`、fixture `80de0046…`、source `7b1e9a04`），runner
`tests/parallel/odyssey/od_r05_portal.py`，fixture
`tests/parallel/odyssey/OdysseyFixture.cs`。

- 已知瞬态：cancel 后同一 tick 立即重新 `load` 会因原生搬运 job 仍预约
  cargo 返回 `422`（`AllReachableColonyItems` 过滤已预约物品）；推进一 tick
  后正常。runner 在 reload 前先推进。
- 未测：UI 对话框路径、>2 名 pawn、禁止/存储区/不可达物品的逐项拒绝、
  lord 跨存档持久化、pocket map 返程（OD-R10）。
