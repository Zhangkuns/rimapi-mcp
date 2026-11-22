# 完成基因组装

开启游戏的开发者模式并启用 Biotech 后，调用 `POST /api/v1/debug/native/execute` 的 `work.gene_assembler_finish`，完成一台**已经开始的**基因组装器作业。游戏暂停也能执行；请求前后 `TicksGame` 不变。不负责开始组装、选择基因包或自动补 Archite 胶囊。

```json
{"action":"work.gene_assembler_finish","map_id":0,"thing_id":"27979","dry_run":true}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | spawned `Building_GeneAssembler`。 |
| `dry_run` | 否 | 只校验并返回计划；不调用 `Finish()`。 |

## 前置与校验

- Biotech 启用；目标是 spawned、未销毁的 `Building_GeneAssembler`。
- `Working == true`（已有开始中的组装）。
- `CanBeWorkedOnNow.Accepted`：供电、`ArchitesRequiredNow <= 0`、连接基因库/处理设施供电、`MaxComplexity() >= TotalGCX`。
- `ArchitesRequiredNow <= 0`（所需 ArchiteCapsule 已在 `innerContainer`）。
- `genepacksToRecombine` 非空：该字段是 private，动作只做**只读反射**校验/读回；不写它、不调用 `Start`。

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id` | 400 `bad_request` |
| 地图/组装器不存在 | 404 `not_found` |
| 未开始（Working=false）、不可工作（供电/设施/复杂度/archite）、已消耗/缺少 archite、无 genepacks、已完成 | 422 `invalid_target` |
| Biotech 未启用 | 501 `not_supported` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在 `Finish()` 前完成。

## 原生语义

`Building_GeneAssembler.Finish()`：

1. 若 `genepacksToRecombine` 非空：播放 `GeneAssembler_Complete`，`ThingMaker.MakeThing(ThingDefOf.Xenogerm)`，`Xenogerm.Initialize(genepacks, xenotypeName, iconDef)`，在 `InteractionCell` 用 `GenPlace.TryPlaceThing` 放置并显示完成消息。
2. 若 `architesRequired > 0`：从 `innerContainer` 逐个 `SplitOff` 并 `Destroy()` ArchiteCapsule，直到满足。
3. `Reset()`：`Working=false`、清空进度/选包/异种名/图标，`innerContainer.TryDropAll` 剩余内容。

动作只调用这个 public 方法，不直接 `MakeThing` 冒充、不跳过 archite 消耗、不跳过 `Reset()`。

## 响应

- `assembler`/`assembler_after`：ThingID/def/位置、`working`、`progress_percent`、`powered_on`、`archites_count`、`archites_required_now`、`can_work_now/reason`、`interaction_cell`；
- `genepacks`：选中包 ThingID/def/基因数/destroyed（只读）；
- `archites_before/after`、`contents_before/after`；
- `xenogerm`：实际 def/ThingID/位置/spawned、`xenotype_name`、`icon_def`、`genes`/`gene_count`；
- `xenogerm_generated`、`new_xenogerm_count`、`outcome`（`xenogerm_generated`）；
- `applied`/`native_invoked`/`changed`。

若 `Finish()` 后没有真实新 Xenogerm（例如 `GenPlace` 失败），返回 `uncertain`，数据中 `xenogerm_generated=false`，不伪造成功。`dry_run` 返回 `would_complete`、`would_consume_archites`，不调用 `Finish`。

## 边界与幂等

- 只完成已开始的组装；不 Start、不选基因包、不自动补 Archite、不批量、不替代正常 Job。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再产出第二只/不再扣 archite；同键改请求（换组装器）为 409。完成后新 key 因 `Working=false` 返回 422。
- 原版 `Finish` 的放置失败/音效/消息如实发生；动作只读回，不补偿、不回滚。
