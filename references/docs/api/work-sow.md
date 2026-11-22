# 指定格即时播种（WORK-D07）

`POST /api/v1/debug/native/execute` 的 `work.sow` 在明确格位即时生成一株核心植物，并直接取得原版播种完成态。它是调试直接变更动作，需要活动存档与开发者模式；不创建播种 Job，也不替代正常种植工作。加载对应 DLL 后，通过 `GET /api/v1/debug/native/actions` 确认动作可用。

```json
{"action":"work.sow","map_id":0,"x":0,"z":0,"def_name":"Plant_Rice","pawn_id":800,"dry_run":true}
```

```json
{"action":"work.sow","map_id":0,"x":1,"z":0,"def_name":"Plant_Potato"}
```

## 参数

- `map_id`、整数 `x`/`z`、`def_name`（`PlantDef`）必填。
- 可选 `pawn_id`：给了就校验工人已 spawn 在同图、存活、Plants 技能不低于该植物的 `sowMinSkill`（机甲使用原生固定技能等级）；不给则只做环境/格位校验，响应 `worker_used:false` 并注明未使用工人。
- `dry_run:true` 只读取并校验，不生成植物；正式执行时省略或设为 `false`。
- 可设 `expected_process_id`；同一次游戏内重试可用 `Idempotency-Key`，见[通用调试合同](../debug-native-actions.md)和[幂等合同](idempotency.md)。

## 原生校验与完成态

按原版 `JobDriver_PlantSow` 与 `WorkGiver_GrowerSow` 的规则：

1. `def.CanNowPlantAt(cell, map)`：`CanEverPlantAt(... checkMapTemperature:false)` 检查地图边界、肥力、污染、terraformable、野生地形标签/黑名单、格内阻挡物、门与不可通行对象；再拒绝 Uninstall/Deconstruct designation 与 reinstall blueprint。
2. `PlantUtility.GrowthSeasonNow(cell, map, def)`：要求格位有 room/adjacent region，且格温在植物 `minGrowthTemperature` 与 `maxGrowthTemperature` 之间。
3. `PlantUtility.AdjacentSowBlocker(def, cell, map) == null`：8 邻格不能有 `blockAdjacentSow` 的植物，或本 def 的 `blockAdjacentSow` 且邻株已 `sown`。
4. 同 def 植物、真空 `>=0.5`、`diesToLight` 洞穴植物的屋顶/永久黑暗与 `GroundGlowAt(..., ignoreCavePlants:true)==0`、`interferesWithRoof` 的屋顶规则均拒绝。
5. `GenSpawn.Spawn(def, cell, map)` 后设置 `plant.sown=true`、`plant.Growth=0.0001f`（原版 sow toil 完成值），并刷新地图网格。可选工人时按原版记录 `PlantsSown` 与 `SowedPlant`/`SowedHumanFoodPlant` 历史事件。

**种子语义**：核心 1.6 `PlantProperties` 没有 `seedDef` 字段，普通作物播种**不消耗任何实物种子**；本动作不扣物品，`items_before_total` 与 `items_after_total` 只会相同。第三方 Mod 的种子系统不伪造支持：本动作按核心植物处理，未建模的 Mod 种子规则列入未测范围。响应 `seed_semantics` 明确此点。

## 读回

`before`/`after` 均包含 `cell`、`plant`、`items_total`：

```json
{
  "cell": {
    "x": 0, "z": 0, "map_id": 0, "terrain": "SoilRich",
    "fertility": 1.4, "temperature": 23.0, "glow": 0.0,
    "roofed": false, "vacuum": 0.0, "polluted": false,
    "can_now_plant_at": true, "growth_season_now": true,
    "adjacent_sow_blocker": null, "blocking_thing": null,
    "same_def_plant": null, "items_total": 3468
  },
  "plant": null,
  "items_total": 3468
}
```

- `can_now_plant_at`、`growth_season_now`、`adjacent_sow_blocker`、`blocking_thing`、`same_def_plant` 是原生校验读回；`terrain`/`fertility`/`temperature`/`glow`/`roofed`/`vacuum`/`polluted` 是格位环境。
- `plant`（`after`）给出 `thing_id`、`def_name`、`x`/`z`、`growth=0.0001`、`sown=true`、`life_stage`（通常 `Growing`）、`spawned`/`destroyed`、HP。`before.plant` 为 null；`dry_run` 的 `after` 等于 `before`。
- `items_before_total` / `items_after_total` / `items_unchanged` 是整图 `ThingCategory.Item` 的堆数量合计，用来证明没有扣种子/材料；核心正常为不变。
- `worker_used`、`worker_validation`、`plants_skill`、`sow_min_skill`、`skill_ok` 描述可选工人的校验；未给工人时 `worker_used=false`、`plants_skill=null`。
- `would_plant` 为预演标记；`applied`/`native_invoked`/`changed` 均为 `true` 才表示实际生成植物。

## 状态码与边界

- 缺 `map_id`/`x`/`z`/`def_name`、坐标越界：400。
- 地图或 `PlantDef` 不存在：404。
- 非 `PlantDef`、不可种植地形/肥力、季节温度不符、相邻播种阻挡、同 def 已存在、真空、洞穴光照/屋顶不符、工人不同图/死亡/技能不足：422。
- 关闭开发者模式：403。
- 游戏未就绪、进程不符或幂等键状态冲突：409。
- 原生异常/生成后读回不符：500，`outcome:"uncertain"`；不静默报告成功，生成的植物由 `after` 读回。
- 只生成一株植物；不批量播种、不播种区域、不扣种子、不派正常 Job，也不改 `work.harvest`/`plant.cut`/`clean_room`。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame` 均为 `103272`；sow dry-run、rice/potato 完成态、无工人、季节/洞穴/水面/相邻/同 def/低技能拒绝、幂等与物品总数保持见[WORK-D07 验收与自审](../references/reviews/2026-11-21-work-d07-sow.md)及[原始证据](../../tests/parallel/work-d07/evidence-work-d07-sow.json)。正式 DLL 未部署，本页只描述源码与隔离 DLL 的行为。
