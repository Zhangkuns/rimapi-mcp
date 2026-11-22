# 完成基因提取（WORK-D27）

`POST /api/v1/debug/native/execute` 的 `work.gene_extract_finish` 完成一个正在进行的 `Building_GeneExtractor` 提取，复用原版 DEV gizmo 的 `Building_GeneExtractor.Finish()`：

```json
{"action":"work.gene_extract_finish","map_id":0,"thing_id":"GeneExtractor27979","pawn_id":800,"dry_run":true}
```

## 定向反射复用

`Finish()` 是 `Building_GeneExtractor` 的 private 参数less 方法。本动作只做一次：

```csharp
typeof(Building_GeneExtractor)
    .GetMethod("Finish", BindingFlags.Instance | BindingFlags.NonPublic)
    .Invoke(extractor, null);
```

这是复用原版 DEV “Finish extraction” 入口，不是通用反射 API：不遍历方法、不反射其它私有字段、不执行任意方法；除这一次调用外，其余读回都走公开成员（`innerContainer`、`PowerOn`、`Working`、`Genepack.GeneSet` 等）。

## 前置校验

- `map_id`、`thing_id` 必填；目标是 spawned `Building_GeneExtractor`。
- 设备 `PowerOn == true` 且 `Working == true`（正在提取）。
- `innerContainer` 中恰有供体 `Pawn`；无供体/空设备 4xx。
- 可选 `pawn_id` 仅供校验/记录：若提供，必须等于当前容器供体，否则 422；它不影响原生 `Finish` 的随机。
- `dry_run` 不调用 `Finish`、不弹出供体、不生成包、不扣任何东西。

## 原生语义

1. `Finish()` 清 `selectedPawn`/sustain，按供体 `thingIDNumber ^ startTick` 种子原生 RNG。
2. 原生按 `GeneCountChanceCurve` 选 1–4 个基因，并从供体可遗传、非 Archite、非 Melanin、biostat 合法的基因中加权抽取；可能得到 0 个可用基因。
3. 若有基因：创建真实 `Genepack`、`Initialize(genesToAdd)` 并在交互格附近 `GenPlace`；0 基因时不放置 Genepack，但仍继续弹出供体。
4. `GeneUtility.ExtractXenogerm(donor, 60,000 × 12–20 天随机)`：给供体加 `XenogermLossShock` 与 `XenogermReplicating`；从容器/交互格弹出供体。
5. 原生发送 `GeneExtractionComplete` 信件/消息；本动作不手工创建包、不跳过随机、不替换原生 `Finish`。

## 读回

- `extractor`/`extractor_after`：thingID、def、位置、spawned、power_on、working、contained_pawn、inner_count。
- `donor`/`donor_after`：thingID、def、位置、spawned/dead、`genes`、`hediffs`（含原生新加的 Xenogerm 类 hediff）。
- `donor_genes`：调用前供体基因列表。
- `genepack_before`：调用前地图上的 Genepack（thingID/def/位置/genes）。
- `genepacks_after`、`genepack`：调用后实际 Genepack；`genes` 为实际包内基因 def 列表；`genepack_generated` 标明是否有包。
- `would_finish`：dry-run 的供体/基因/供电/working/已有包；`dry_run` 的 `genepack=null`、`genes=[]`、前后不变。
- `items_total_before/after`、`applied/native_invoked/changed`。`changed` 表示完成了一次 Finish（供体弹出/设备清空/产生包）。

## 状态码与边界

- 缺 `map_id`/`thing_id`：400；地图/设备不存在：404；非 `Building_GeneExtractor`：422。
- 设备未 spawn、未供电、未 Working、无容器供体：422，不调用 `Finish`。
- 可选 `pawn_id` 与容器供体不符：422。
- 关闭开发者模式：403；进程不符/幂等键冲突：409；原生异常：500 uncertain，保留实际设备/供体/包读回。
- 同 key 重放不再第二次 Finish/不再生成第二包/不再第二次弹出；同 key 换 body 409；空设备/已提取新 key 4xx。
- 0 基因是原生合法分支：`genepack=null`、`genepack_generated=false`，但供体仍被提取/弹出、`Xenogerm*` hediff 仍写入；不能把它误报为未完成。
- 不直接创建 Genepack、不预掷/跳过随机、不批量、不替代开始/取消提取 API。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；真实 `Finish` 生成 `Genepack27981`（RobustDigestion），供体 Human800 被弹出并获得 `XenogermLossShock`/`XenogermReplicating`，设备 `working=false`/容器清空；随后空设备/断电/错供体/错目标/幂等均被拒绝且不重复生成。详见[WORK-D27 验收与自审](../references/reviews/2026-11-21-work-d27-gene-extract-finish.md)和[原始证据](../../tests/parallel/work-d27/evidence-work-d27-gene-extract-finish.json)。正式 DLL 未部署。
