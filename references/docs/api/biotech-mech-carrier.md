# Biotech 战争女王载机补料与释放（BT-R18）

交付：`CompMechCarrier`（战争女王 `Mech_Warqueen` 的载机）读取、原生补料阈值写入与
原生 `TrySpawnPawns` 释放。实现文件：
`Services/System/MechControl/MechControlService.cs`、
`Controllers/System/MechControlController.cs`、`Models/DlcMechDtos.cs`。
只操作本地图上已生成、玩家控制/有 overseer 的机械体；不生成载机、不改地图/世界机械。

## 读取

`GET /api/v1/dlc/biotech/mech/carrier?map_id=0&mech_id=Mech_Warqueen75482`

- `mech_id` 省略时列出该地图所有带 `CompMechCarrier` 的 pawn。
- 字段来自原生 comp：`ingredient_def`（`fixedIngredient`）、`ingredient_count`
  （`IngredientCount`）、`max_ingredient_count`、`max_to_fill`（补料阈值）、
  `amount_to_autofill`、`percentage_full`、`cost_per_pawn`、`max_pawns_to_spawn`、
  `max_can_spawn`、`can_spawn`/`can_spawn_reason`（原生 `CanSpawn` 报告）、
  `cooldown_ticks_remaining`（私有字段反射）、`low_ingredient_count`、
  `spawn_pawn_kind`、`spawned_pawn_ids`（原生私有 `spawnedPawns` 过滤死亡）。
- 只读；不会补料、改阈值或释放。

```json
{"map_id":0,"count":1,"carriers":[{"mech_id":"Mech_Warqueen75482",
 "def_name":"Mech_Warqueen","faction":"PlayerColony","overseer_id":"Human75478",
 "colony_mech":true,"player_controlled":true,"ingredient_def":"Steel",
 "ingredient_count":30,"max_ingredient_count":600,"max_to_fill":600,
 "amount_to_autofill":570,"percentage_full":0.05,"cost_per_pawn":25,
 "max_pawns_to_spawn":3,"max_can_spawn":1,"can_spawn":true,
 "can_spawn_reason":null,"cooldown_ticks_remaining":0,"low_ingredient_count":true,
 "spawn_pawn_kind":"Mech_WarUrchin","spawned_pawn_ids":[]}]}
```

## 补料阈值

`POST /api/v1/dlc/biotech/mech/carrier/autofill`

```json
{"mech_id":"Mech_Warqueen75482","max_to_fill":100,"dry_run":true}
```

- 写原生 `CompMechCarrier.maxToFill`（检查器滑条同一字段），取值范围
  `0..max_ingredient_count`，越界 400；`dry_run` 只返回 `max_to_fill_before/target`。
- 补料本身不由本端点搬物品：阈值写入后原生 `WorkGiver_HaulResourcesToCarrier`
  （Hauling）把 `fixedIngredient` 搬到载机，直到 `AmountToAutofill<=0`；
  实际资源量要独立读回（`ingredient_count`/`amount_to_autofill`）。

## 释放

`POST /api/v1/dlc/biotech/mech/carrier/release`

```json
{"mech_id":"Mech_Warqueen75482","dry_run":true}
```

- 资格来自原生 `CanSpawn`（玩家控制、非 self-shutdown、存活 spawned、资源足够、
  冷却结束）；不满足返回 409 并在 `data.can_spawn_reason` 给出原生文本。
- dry-run 返回 `can_spawn/max_can_spawn/would_spawn/cooldown_ticks_remaining`。
- 正式调用原生 `TrySpawnPawns()`：按 `costPerPawn` 扣减载机资源、生成
  `spawnPawnKind`（默认 `Mech_WarUrchin`）、设置原生冷却（默认 900 ticks）。
  返回 `spawned_pawn_ids/spawned_count/ingredient_count_after/cooldown_ticks_after`；
  `applied` 表示实际观察到新生成机械。

## 实测（BT-R18，2026-11-13）

run `bt-r18-20261113-run2`（独立 Xvfb/profile，`g35_deathresting` 副本，tick 76787 起，
port 8766）。fixture 生成玩家 `Mech_Warqueen75482`，用原生 `Overseer` 关系/控制组绑定到
Human75478（bandwidth 5/6），把原生载机容器改成 30 Steel，地图放两个 40 Steel 堆，
并选中搬运者 Human315（Hauling=1）。全部补料/扣资源/生成/冷却为原生。

- 初始 GET：`ingredient_count=30`、`max_to_fill=600`、`amount_to_autofill=570`、
  `max_can_spawn=1`、`can_spawn=true`、无 spawned pawn。
- autofill dry：`applied=false`、before 600/target 100 且 GET 仍 600；`max_to_fill=601`
  返回 400；正式 `applied=true`、`max_to_fill_after=100`、`amount_to_autofill=70`。
- tick 77,243 原生 `HaulToContainer` 完成补料：`ingredient_count=100`、
  `amount_to_autofill=0`、`max_can_spawn=3`（100/25=4，受 `maxPawnsToSpawn` 3 限制）。
- release dry：`can_spawn=true`、`would_spawn=3`；正式 `applied=true`、
  `spawned_pawn_ids=[Mech_WarUrchin75502/75505/75508]`、
  `ingredient_count_after=25`、`cooldown_ticks_after=900`；三名 war urchin 均 spawned、
  非死亡，位置为载机格。
- 冷却中再次 release：409，`can_spawn=false`、
  `can_spawn_reason="Cooldown 15.0 seconds"`、`cooldown_ticks_remaining=900`；
  GET 同样读回 25 Steel、3 个 spawned id、冷却。
- 证据 `tools/cua/.runtime/bt-r18-20261113-run2/evidence/bt-r18-run2.json`
  （`phase=passed`）；runner `tests/parallel/biotech/bt_r18_warqueen_carrier.py`；
  fixture 命令 `prepare_warqueen_carrier`/`observe_warqueen_carrier`。

边界/披露：fixture 生成 Warqueen、绑定 overseer 并把初始化资源改为 30 Steel（原生容器
写入）；未测非玩家载机、self-shutdown/未控制拒绝、无资源下限的自动补料策略、战斗目标下
AI 自动 release、多载机、冷却自然结束后的第二次释放、存档读回。

相关：[机械师与机械体](biotech-mechs.md)、[机械培育舱](biotech-mech-gestator.md)。
