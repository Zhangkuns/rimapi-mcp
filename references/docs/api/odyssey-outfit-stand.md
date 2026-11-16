# Odyssey 装备架搬入/移出与指定衣物（OD-R02）

2026-11-12 扩展：原端点只读 contents、非空时 swap；本轮补
`allowRemovingItems` 开关、把指定衣物放入空架、单件弹出并保留 forbidden 状态。
放入走原生 `PutApparelOnOutfitStand` 工作（与"强制穿戴→点装备架"同一 job），
不是直接改容器。

## 读取

`GET /api/v1/dlc/odyssey/outfit-stands?map_id=0`

每行新增 `allow_removing_items`，`held_items` 每项新增 `forbidden`：

```json
{"stand_id":75478,"stand_thing_id":"Building_OutfitStand75478",
 "held_items":[{"thing_id":"Apparel_Tuque75479","def_name":"Apparel_Tuque",
                "label":"tuque","forbidden":false}],
 "allow_removing_items":true}
```

## 管理

`POST /api/v1/dlc/odyssey/outfit-stand/manage`

```json
{"map_id":0,"stand_id":75478,"action":"set_hauling","enabled":true}
{"map_id":0,"stand_id":75478,"action":"put",
 "pawn_id":"Human321","apparel_id":"Apparel_Tuque75479"}
{"map_id":0,"stand_id":75478,"action":"eject","apparel_id":"Apparel_Tuque75479"}
```

- `set_hauling`：调用原生私有 `SetAllowHauling(bool)`（与
  `CommandAllowRemovingApparel` 相同），触发 `RecalculateAllInHaulSource`；
  返回 `before/after allow_removing_items`。`dry_run` 不写。
- `put`：要求 apparel 是地图上的原生 `Apparel`、`CanEverStoreThing`、
  未燃烧、pawn 可到达/可预约；队列
  `JobMaker.MakeJob(JobDefOf.PutApparelOnOutfitStand, apparel, stand)`。
  返回 `job_submitted/job_def`；衣物实际进入架子由 job 完成并读回 holder。
- `eject`：只对架子当前持有的精确 item 调原生
  `TryDrop(item, Position, ThingPlaceMode.Near, 1, out dropped)`，随后
  `SetForbidden(true)`；返回 `ejected_id/ejected_forbidden`。
- 合同：POST `command/pawn_ai/validate/game_memory`；已有 `outfit-stand/use`
  swap 保留。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 生成空玩家装备架 + 一件未禁用 Tuque，选一名空闲殖民者。
`allow_removing_items:false`→dry 保持→true；put dry 保持→正式 `job_submitted`；
推进后 Tuque 的 `ParentHolder=Building_OutfitStand`、held 列表精确匹配；
eject 后 held 空、Tuque 落回地图且 `forbidden:true`；再关 hauling 读回 false。
证据 `tools/cua/.runtime/od-r02-20261112/evidence/od-r02.json`，runner
`tests/od_r02_outfit_stand.py`，fixture `prepare_outfit_stand`/`observe_outfit_stand`。

- 未测：多件/武器与存储组、囚犯/访客衣物、不可达/预约冲突的逐项拒绝、
  长时间自主 hauling 循环；交换现有衣物仍由旧 `outfit-stand/use` 覆盖。
