# Biotech 废物包原子分解器自动装载与弹出（BT-R11）

2026-11-12 新增：`POST /dlc/biotech/waste/atomizer/policy` 写
`CompAtomizer.autoLoad` 或调用原生私有 `EjectContents`（gizmo 同一动作）；
一次 haul/atomize 链保留。

## 请求

```json
{"map_id":0,"atomizer_id":"WastepackAtomizer75478","auto_load":false}
{"map_id":0,"atomizer_id":"WastepackAtomizer75478","eject":true,"dry_run":true}
```

- 至少给 `auto_load` 或 `eject`；玩家所属 atomizer；`EjectContents` 经
  `AccessTools.Method` 固定调用，掉落到交互格并清 ticksAtomized。
- 返回 `before_auto_load`、`auto_load`、`total_stack_count/space_left/
  fill_percent/ticks_left_until_all_atomized/empty` 和 `applied`；dry_run 不写。
- 合同：POST `policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成玩家 WastepackAtomizer，放入 4 个原生 wastepack（power off）；
GET auto_load true/count4；auto_load false 后 GET false/count4；eject 后
count0/empty/地图 scatter 4；空字段 400。
证据 `tools/cua/.runtime/bt-r11-20261112/evidence/bt-r11.json`，runner
`tests/bt_r11_atomizer.py`，fixture `prepare_atomizer`/`observe_atomizer`。

- 未测（2026-11-12 当时）：实际自主 haul/atomize 与 wastepack 消耗、断电/满容量、
  污染变化；既有一次原子分解链保留。

2026-11-13 自主装载/消耗（`bt-r11-20261113-run2`，同一 g35 副本 tick 76787 起，
port 8766）：
fixture 生成玩家 `WastepackAtomizer`（油发发电+导线，ticksPerAtomize 30000）、
4 个散落原生 wastepack 和一名搬运殖民者，并把其他自由殖民者的 Hauling work
设为 0 以隔离单一自主搬运者。随后只走现 policy 端点：

- `auto_load=false` 推进 2500 tick：无 `HaulToAtomizer` job（该搬运者及任意 pawn），
  atomizer content 0、地图仍 4 个 wastepack，empty；GET 读回 false。
- `auto_load=true` 后原生 `WorkGiver_HaulToAtomizer` 在 tick 80677 给 Human321
  `HaulToAtomizer` job，tick 82375 装载完成：content 4、地图散落 0（4 个全部入机）。
- fixture 把私有原生 `ticksAtomized` 置 `TicksPerAtomize-2000`（只缩短等待）；
  tick 84373 原生 `CompAtomizer.CompTick -> DoAtomize` 消耗 1 个 wastepack：
  content 4→3（同一堆 stack 3）、总 wastepack 4→3、auto_load 仍 true，
  GET 读回 content 3/ticks_left 89995。无 `/waste/atomize` 调用；旧 eject
  字段/行为不重跑。

证据 `tools/cua/.runtime/bt-r11-20261113-run2/evidence/bt-r11.json`；runner
`tests/parallel/biotech/bt_r11_atomizer_auto.py`；fixture
`prepare_atomizer_auto`/`shorten_atomizer`/`observe_atomizer_auto`。收口
`desktop.py stop` 报告 source_save/formal_dll/global_prefs unchanged。

- 未测：断电后的装载/分解、满容量、污染与长期分解、多台 atomizer、存档读回；
  既有 eject 结果仍引用 2026-11-12 证据。
