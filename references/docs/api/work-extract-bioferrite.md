# 实体抽取生物铁（WORK-D30）

`POST /api/v1/debug/native/execute` 的 `work.extract_bioferrite` 对关押在 `Building_HoldingPlatform` 上的实体结算一次原版 `JobDriver_ExtractBioferrite` 完成段：

```json
{"action":"work.extract_bioferrite","map_id":0,"thing_id":"HoldingSpot27979","pawn_id":800,"dry_run":true}
```

## 前置

- `map_id`、`thing_id`（平台）、`pawn_id`（抽取者）必填。
- 平台 spawned；`HeldPawn` 存在且未销毁；实体有 `CompHoldingPlatformTarget`，`containmentMode != Release` 且 `extractBioferrite == true`。
- 工人同图、spawned、存活、玩家阵营。
- `HoldingSpot` 与 `HoldingPlatform` 都使用 `Building_HoldingPlatform`，本动作只按 `Building_HoldingPlatform` 类型校验。
- `dry_run` 只计算/预览，不生成、不伤害、不加 hediff。

## 原版完成段

按 `JobDriver_ExtractBioferrite` 的 `Toils_General.Do`：

1. `num = Mathf.FloorToInt(CompProducesBioferrite.BioferritePerDay(innerPawn) * 4f)`。
2. `num > 0` 时创建真实 `ThingDefOf.Bioferrite`，`stackCount=num`，在**工人位置**附近 `GenPlace.TryPlaceThing`。
3. 对 `innerPawn` 执行 `TakeDamage(new DamageInfo(DamageDefOf.SurgicalCut, 4f))`。
4. `innerPawn.health.AddHediff(HediffDefOf.BioferriteExtracted)`。

不跳过随机外的任何步骤（本段无随机）、不刷生物铁、不替代正常 Job/收集器取出 API；一次只处理一个平台。

## 读回

- `platform`/`platform_after`：thingID、def、位置、spawned、held_pawn、occupied、inner_count。
- `held`/`held_after`：thingID、def、位置、spawned/dead/downed、faction、containment_mode、extract_bioferrite、has_comp_produces_bioferrite。
- `worker`/`worker_after`：pawn_id、def、位置、spawned/dead/faction。
- `bioferrite_per_day`（调用前）、`computed_count`、`bioferrite_per_day_after`（抽取后通常为 0，因 `BioferriteExtracted`）。
- `health_before/after`：summary_health、downed/dead、bioferrite_extracted；`hediffs_before/after`：def/severity/part；`hediff_added`。
- `bioferrite`：实际产物 thingID/def/quantity/位置；`bioferrite_generated`、`placement_succeeded`。
- `items_total_before/after`、`applied/native_invoked/changed`。
- `would_extract`：dry-run 的 per-day/count/mode/flag/worker 预览。
- 实体仍留在平台上；本动作不弹出/不销毁 held pawn。

## 状态码与边界

- 缺 `map_id`/`thing_id`/`pawn_id`：400；平台/工人不存在：404；非平台：422。
- 平台空/未 spawn、实体销毁、无 `CompHoldingPlatformTarget`、`Release`、`extractBioferrite=false`：422，不生成/不伤害。
- 工人不同图/死亡/非玩家：422。
- 关闭开发者模式：403；进程不符/幂等键冲突：409；原生异常：500 uncertain，保留实际实体/平台/生物铁读回。
- 同 key 重放不再产第二堆/不再二次伤害；同 key 换 body 409；空平台/Release 新 key 4xx。
- `num=0` 是合法分支：不生成生物铁，但仍执行 SurgicalCut 4 + BioferriteExtracted；不能把“无产物”误报为未完成。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；Sightstealer 在 HoldingSpot 上 `BioferritePerDay=1.6`、`computed_count=6`，原生完成段生成 `Bioferrite27982`（6），实体获得 `Cut 4`/`BioferriteExtracted`，健康 1.0→0.9288889；平台仍持有实体。详见[WORK-D30 验收与自审](../references/reviews/2026-11-21-work-d30-extract-bioferrite.md)和[原始证据](../../tests/parallel/work-d30/evidence-work-d30-extract-bioferrite.json)。正式 DLL 未部署。
