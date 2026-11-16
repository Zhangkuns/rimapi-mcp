# Anomaly 变异状态读取（G-39）

交付：按地图或按 Pawn 读取 `Pawn_MutantTracker` 状态，以及对玩家所属 Pawn 的
**转化**与**复原**。

证据：`dlc-G-39-mutant.json`（17 项）、`dlc-G-39-none.json`（12 项）、
`dlc-G-39-write.json`（23 项）、`dlc-G-39-guards.json`（无 Game 409 / DLC-off 422）、
`dlc-batch3-nogame.json`。fixture：`ai_g39_mutant`
（Human321 → Ghoul、Human318 → Shambler，均由临时探针在副本中转化）。

## 请求

- `GET /api/v1/dlc/anomaly/mutation?map_id=0` —— 该地图上所有已生成的变异 Pawn
- `GET /api/v1/dlc/anomaly/mutation?pawn_id=321` —— 单个 Pawn（未变异也返回 200）

`map_id` 省略用当前地图；非整数 400，未知地图 404；未知 pawn 404。

## 响应（地图）

```json
{"map_id":0,"mutant_count":2,"mutant_defs":["Shambler","Ghoul","AwokenCorpse"],
 "mutants":[{"pawn_id":"Human321","thing_id":321,"name":"Blean","def_name":"Human",
   "is_mutant":true,"mutant_def":"Ghoul","has_turned":true,"is_passive":false,
   "rot_stage":"Fresh","hediff":"Ghoul","ability_count":0,"downed":false,"dead":false,
   "faction_id":"Faction_16","ideo_id":null,"position":{"x":155,"y":0,"z":116}}]}
```

## 响应（单个 Pawn）

```json
{"pawn":{"pawn_id":"Human315","thing_id":315,"name":"Youko","def_name":"Human",
  "is_mutant":false,"mutant_def":null,"has_turned":false,"is_passive":false,
  "rot_stage":null,"hediff":null,"ability_count":0,"downed":false,"dead":false,
  "faction_id":"Faction_16","ideo_id":"13"},
 "mutant_defs":["Shambler","Ghoul","AwokenCorpse"]}
```

单 Pawn 视图不含 `position`（可能未生成）。`mutant_defs` 是当前 Def 数据库里的
`MutantDef` 列表，供调用方发现合法值；不是“该存档已出现过的变异”。
`rot_stage` 取 `RotStage`（`Fresh|Rotting|Dessicated`），未变异为 `null`。

## 实测要点

| 场景 | 结果 |
| --- | --- |
| `SetPawnAsMutantInstantly(pawn, Ghoul, Fresh)` | `Pawn.def` 仍是 `Human`；变异是 tracker+hediff 状态，不是 ThingDef 替换 |
| 同上 | `mutant_def=Ghoul`、`has_turned=true`、`hediff=Ghoul`、`faction_id` 保持原派系 |
| Shambler 转化 | `faction_id` 变为 `Faction_10`（Shambler 的 `defaultFaction`），与原派系不同 |
| 存读档 | 两个变异 Pawn 的 `mutant_def`/`has_turned`/`rot_stage` 在重启进程后保持 |
| 未变异 Pawn | `is_mutant:false`、`mutant_def:null`、`rot_stage:null` |
| 连续两次读取 | 计数与 pawn_id 集合不变（只读） |
| 无 Game | 409 `conflict` |

## 转化

`POST /api/v1/dlc/anomaly/mutation/transform`

```json
{"pawn_id":"315","mutant_def":"Ghoul","confirm":true,"dry_run":false}
```

成功数据：`{pawn_id,mutant_def,faction_before,free_colonist_before,default_faction,
changed,dry_run,after_*}`（`after_*` 是转化后的实际身份：派系、教义、`is_free_colonist`、
`is_mutant`、`mutant_def`、`has_turned`、`rot_stage`、`hediffs`、`abilities`、`downed` 等；
写响应另含 `after_tracker_present`、`after_has_turned`、`after_hediff_present`）。

- 原生 `SetPawnAsMutantInstantly(pawn, def, RotStage.Fresh)`；`Pawn.def` 不变。
- **转化后不再 `IsFreeColonist`**；Ghoul 保留玩家派系，Shambler/AwokenCorpse 转到
  `MutantDef.defaultFaction`（实测 `Faction_10`），`ideo` 变为 null。
- 已变异体再次 transform 返回 409（实测会叠加 Hediff，故不提供叠加）。
- **完成判据不只看 `is_mutant`**：`IsMutant` 仅是 tracker 非空；只有 `has_turned` 为 true
  且 Def 的 Hediff 已存在才算 `mutated`。tracker 已建但未完成 Turn → `partial_or_contradictory`
  （500，`applied` 缺省）。
- **目标限人形**：玩家所有非人形目标（实测 Muffalo+Ghoul）在原生 `ResolveGraphics` 抛 NRE，
  且抛错前已写入 tracker/Hediff；因此非人形 422。动物蠕行者属于原生
  `ResurrectAsShambler` 流程，未实现。
- 副作用：`Turn` 会按 Def 加 Hediff、终止妊娠、移除/替换部分健康状态并 `RegenerateHealth`；
  `Revert` **不是完整回滚**（不恢复妊娠、伤口或原有 Hediff）。
- 非玩家派系 422；死亡/未生成 422；未知 def/pawn 404；缺 `confirm` 400。

## 复原

`POST /api/v1/dlc/anomaly/mutation/revert`

```json
{"pawn_id":"321","confirm":true,"dry_run":false}
```

- 原生 `Pawn_MutantTracker.Revert(false)`；实测恢复殖民者身份、原派系与原教义
  （Shambler 也能回到玩家派系）。
- 允许条件：当前为玩家派系，或 `originalFaction`（只读非公开字段）是玩家；
  野生变异体 422。非变异体 409。
- 结果按观察分类：`reverted`（200 `applied:true`）、`not_reverted`（500 `applied:false`）、
  `unknown`/`partial`（500，`applied` 缺省）。

## 未覆盖

- `Turn(clearLord)`、`ResurrectAsShambler`（尸体重生）、死亡时 `clearMutantStatusOnDeath` 未测。
- 转化后立即复原的中间态（如派系成员关系清理）未逐项读回；只验证身份/派系/教义/hediff。
- `Rotting`/`Dessicated` 阶段、死亡后的变异状态（`clearMutantStatusOnDeath`）未观测。
- `Hediff`/`ability_count` 只观测到 Ghoul、Shambler、AwokenCorpse 且 `ability_count=0`；
  有武器/能力的变异体未构造。
- 地图外（商队、gravship）的变异 Pawn 不在列表视图内；单 Pawn 视图可读。
- DLC-off 拒绝见批次末批量验证，不在本模块单独重启验证。

## 尸体重生为蠕行者（G-43）

`POST /api/v1/dlc/anomaly/mutation/resurrect`

```json
{"map_id":0,"corpse_id":"Corpse_Human75418","confirm":true,"dry_run":false}
```

- 覆盖原生 `MutantUtility.ResurrectAsShambler(pawn, lifespanTicks, faction)` 流程，即
  `SetPawnAsMutantInstantly` 之外的非人形/尸体分支；`lifespan_ticks` 省略时用原生默认（无限期）。
- 资格：地图上已生成的 `Corpse`，`MutantUtility.CanResurrectAsShambler(corpse)` 为真
  （flesh、`canBecomeShambler`、非变异、无进行中的 DeathRefusal、不在有屋顶的正式房间）。
  未生成/找不到 404；不满足资格 422；已变异 409；缺 `confirm` 400。
- 原生调用后立即读回 `after`：`dead`、`is_mutant`、`mutant_def`（Shambler）、`has_turned`、
  `rot_stage`、`shambler_hediff`、`rising`、派系与位置。`applied:true` 判据是
  **未死亡 + IsMutant + Def=Shambler + Shambler hediff**；`has_turned` 不适用于此原生分支。
- 蠕行者会在原生 5–10 秒内完成 `Rising`；提交成功不等于动画结束。派系按
  `MutantDef.defaultFaction`（实测 `Faction_10`），不是玩家派系。
- 实测：fixture 在无屋顶空地生成一个一次性 Villager 并杀死，只有该 pawn 被加入测试伤害豁免；
  dry-run 不改，正式重生后 `mutant_def=Shambler`、`dead=false`、`shambler_hediff=true`，
  `GET mutation?pawn_id=` 读回一致。证据：`Reverse/evidence/dlc-G43-containment-cancel-mutation.json`。
- 未覆盖：墓穴/容器内尸体、动物目标、`lifespan_ticks` 到期死亡、蠕行者战斗/生存周期。
