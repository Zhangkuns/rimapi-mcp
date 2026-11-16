# Biotech 次核心扫描（BT-12a）

`GET /api/v1/dlc/biotech/subcore?map_id=0`

```json
{"map_id":0,"scanners":[{
  "scanner_id":"SubcoreSoftscanner75478",
  "def_name":"SubcoreSoftscanner",
  "state":"Inactive",
  "power_on":true,
  "init":false,
  "ingredients_loaded":false,
  "required":[{"def_name":"Steel","remaining":50},{"def_name":"ComponentIndustrial","remaining":4}],
  "occupant_id":null,
  "selected_pawn_id":null,
  "output_def":"SubcoreRegular",
  "destroy_brain":false,
  "interaction_cell":{"x":168,"z":174}
}]}
```

- `state` 是原生 `SubcoreScannerState`：Inactive/WaitingForIngredients/
  WaitingForOccupant/Occupied。
- `init` 读原生私有 `initScanner`，因此断电时 state 仍可能为 Inactive 但
  `init` 为 true，可继续原生 CancelLoad。
- `required` 显示每种固定材料还差多少；`occupant_id` 是舱内人物。

## 初始化 / 装料 / 进入

`POST /api/v1/dlc/biotech/subcore/init` `{"scanner_id":"SubcoreSoftscanner75478"}`

- 调用原生 `SubcoreScannerStart` gizmo；要求 Inactive 且供电，已初始化 409。
- 初始化后状态进入 WaitingForIngredients（缺料）或 WaitingForOccupant。

`POST /api/v1/dlc/biotech/subcore/load` `{"pawn_id":"321","scanner_id":"..."}`

- 选择最近的 `CanAcceptIngredient` 堆，提交原生
  `HaulAIUtility.HaulToContainerJob`；`dry_run` 只返回候选/数量。
- 没有可达材料/未初始化/非 WaitingForIngredients 时 409。

`POST /api/v1/dlc/biotech/subcore/enter` `{"pawn_id":"321","scanner_id":"..."}`

- 要求原生 `CanAcceptPawn`（材料齐、无 ScanningSickness、非婴儿）且可达；
  提交 `JobDefOf.EnterBuilding`，由原生 60 tick 进入和 7500 tick 扫描完成。
- 软扫描只增加 `ScanningSickness` 并释放人物；`SubcoreRegular` 落在
  InteractionCell 附近。
- **ripscanner**（`destroy_brain:true`）的 enter 必须带 `"confirm":true`；否则
  409 返回原生 `ConfirmRipscanPawn` 文本。扫描完成由原生 `KillOccupant` 杀死
  目标并产出 `SubcoreHigh`（软扫描器不受此门禁影响）。

`POST /api/v1/dlc/biotech/subcore/eject`

```json
{"scanner_id":"SubcoreRipscanner75478","confirm":true}
```

- 调用原生 `EjectContents`：有 occupant/材料时弹出；扫描器已 init 但空载时
  等价原生 `CancelLoad`（`initScanner=false`），返回 `cancel_kind:"cancel_load"`、
  `initialized_before/init_after`，状态回 Inactive。
- ripscanner 只有存在 occupant 时才要求 `confirm:true`；空载 CancelLoad 不弹确认。
- 完全未 init 且无内容 409，未知 scanner 404；`dry_run` 只投影。

实测 Human321：init→Steel 50（组件由殖民地 AI 补齐）→enter→tick 86570
产出 1 个 SubcoreRegular、人物释放并带 ScanningSickness；未初始化 enter 409、
未知 scanner 404（`Reverse/evidence/bt12a-subcore-20261111.json`，自审
[BT-12a](../references/reviews/2026-11-10-bt12a-subcore.md)）。

ripscanner 实测：Human321 未确认 enter 409，确认后 tick 86576 产出 1 个
SubcoreHigh 且 Human321 死亡；第二次 enter Human315 后 eject 未确认 409，
确认 eject 杀死 Human315、output 仍 1（`bt12b-ripscanner-20261111.json`，自审
[BT-12b](../references/reviews/2026-11-10-bt12b-ripscanner.md)）。

边界：只测软/剥离各一次；缺电/缺料、扫描中断电/卸载、ScanningSickness 拒绝、
多次扫描未验。

2026-11-13 BT-R09：补充“初始化后空载 CancelLoad”与倒地患者由原生 carrier 搬入。
隔离 `g35_deathresting` 副本 run `tools/cua/.runtime/bt-r09-20261112-run/`：
fixture 生成两台通电软扫描器；B 初始化后 `state=WaitingForIngredients`，空载
`POST /subcore/eject` 返回 `cancel_kind=cancel_load`/`init_after=false`/`state_after=Inactive`；
A 预装固定材料并初始化到 WaitingForOccupant，`POST /subcore/select`（倒地殖民者
Human318，`pawn_job=null`）后原生 `WorkGiver_CarryToSubcoreScanner` 派
`CarryToBuilding`（tick 77594，carrier Human315 实际抱起 Human318），tick 77600
状态 Occupied、`occupant_id=Human318` 且材料仍在舱内。证据
`tools/cua/.runtime/bt-r09-20261112-run/evidence/bt-r09.json`，runner
`tests/parallel/biotech/bt_r09_subcore.py`。未测囚犯患者、ripscanner 空载取消、
断电与扫描完成。
