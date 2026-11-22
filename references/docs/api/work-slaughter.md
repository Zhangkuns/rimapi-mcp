# 宰杀指定牲畜

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.slaughter`，对一只现有合法牲畜执行原版宰杀。只结算死亡/尸体终态，**不产肉/皮**；肉/皮由 `work.butcher` 的屠宰账单另行处理。不创建或推进 180 tick 的 Job，游戏暂停也能执行；请求前后 `TicksGame` 不变。

```json
{"action":"work.slaughter","map_id":0,"thing_id":"27979","pawn_id":"27989","dry_run":true}
```

## 参数

| 参数 | 必需 | 说明 |
| --- | --- | --- |
| `map_id` | 否 | 地图唯一 ID；省略用当前地图。 |
| `thing_id` | 是 | 要宰杀的动物（spawned Pawn）。 |
| `pawn_id` | 是 | 同图、存活、未倒地、玩家阵营的 humanlike 执行者。 |
| `dry_run` | 否 | 只校验并返回计划，不改状态。 |

## 原生语义

调用公开 `ExecutionUtility.DoExecutionByCut(executioner, victim)`：

- 在受害者位置产生原版血量对应的血污（`health.DropBloodFilth()`）；
- 若开启 Ideology 且目标是动物，记录 `SlaughteredAnimal` 历史；
- 选择 execution part，施加 `ExecutionCut` 伤害并确保死亡；
- 死亡生成真实 `Corpse`（原版 `Pawn.Kill` 路径）。

本动作随后只在**确实死亡**时执行原版 `JobDriver_Slaughter` 的 `executioner.records.Increment(RecordDefOf.AnimalsSlaughtered)`；不跑 180 tick、不直接 `Kill()`、不产肉/皮、不刷尸体。`ShouldBeSlaughtered`（Slaughter 标记或自动宰杀）只作为读回信息，不强制要求——玩家阵营动物即可执行；原版 WorkGiver 的额外 Ideology 预检查不在此重复。

## 校验

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `thing_id`/`pawn_id` | 400 `bad_request` |
| 地图、受害者、执行者不存在 | 404 `not_found` |
| 受害者不是动物/人类殖民者、非玩家阵营、已死、未 spawned、aggro 精神状态；执行者是动物、非玩家阵营、死亡/倒地、无暴力能力；执行者即受害者 | 422 `invalid_target` |
| 开发者模式关闭 | 403 `forbidden` |

所有校验在修改前完成；失败不受伤、不流血、不产尸体、不写记录。`dry_run` 返回 `would_slaughter`/`would_produce_corpse`/`would_record_increment`，现场不变。

## 响应

- `victim`/`victim_after`：def/ThingID/位置、`animal`、阵营、`spawned`/`dead`/`destroyed`/`downed`、`should_be_slaughtered`；
- `executioner`/`executioner_after`：ThingID、阵营、`animals_slaughtered`；
- `corpse`：真实尸体 def/ThingID/位置/`InnerPawn`（ThingID/def）；
- `blood_before`/`blood_after`：受害者周围 5×5 的血污 filth（def/数量/位置）；
- `injuries_before`/`injuries_after`：hediff（def/severity/bleed_rate），含 `ExecutionCut`；
- `record_before`/`record_after`/`record_incremented`；
- `applied`/`native_invoked`/`changed`/`outcome`。

## 边界与幂等

- 只宰杀一只已有牲畜；不批量、不替代 `/autoslaughter`、不调用 `work.butcher`、不产生肉/皮。
- 不跑原版 180 tick 等待/动画、不授予执行者技能经验；保留原生血污/历史/尸体路径。
- `Idempotency-Key` 在本游戏实例内重放同一响应（`Idempotency-Replayed: true`），重放不再杀、不会产生第二具尸体；同键改请求（换目标/执行者）为 409。已死目标用新 key 返回 404/422。
