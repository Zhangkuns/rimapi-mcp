# 人物特质与状态直接编辑（P01）

六个开发者/调试编辑接口，沿用 `/api/v1/pawn/edit/...` 风格，全部在游戏主线程执行，直接写原版字段或调用原版公开方法；不开放任意反射或任意字段。请求体为 JSON，响应为统一 `ApiResult`，错误码沿用 `ApiErrorCode`。

所有接口都接受 `pawn_id`（数字 ID）。没有 Idempotency-Key 语义（与现有 `/pawn/edit/*` 一致）。

## 1. 单人解除/设置死忠

`POST /api/v1/pawn/edit/recruitable`

```json
{"pawn_id": 27979, "recruitable": false}
```

- 目标必须是殖民地的囚犯（`guest != null && IsPrisonerOfColony`），否则 422；`recruitable` 必须显式给出 bool，否则 400。
- 语义：写 `Pawn_GuestTracker.Recruitable` setter。setter 存储的值与原生 `Recruitable` 有效值可能被故事导演难度（`unwaveringPrisoners`）覆盖，响应分别给出。
- 响应：`prisoner`、`guest_status`、`before`（有效值）、`set_value`（写入值）、`effective`（写入后的原生有效值）、`changed`（有效值是否变化）、`difficulty_unwavering`、`difficulty_overrides_value`。
- 不改阵营、互动模式、记忆、抵抗、意志；不自动招募。

## 2. 精确删除普通特质

`POST /api/v1/pawn/edit/trait/remove`

```json
{"pawn_id": 27990, "def_name": "Nerves", "degree": 1, "allow_gene_source": false}
```

- 从 `TraitSet.allTraits`（包含被压制和基因派生条目）按 `def_name` + `degree` 精确选中；`degree` 缺省时仅当该 Def 只有一个等级才取 0，否则 400。
- 结构化错误（422）：`unknown_def`（Def 不存在）、`not_held`（没有该 Def+degree）、`suppressed`（被压制）、`gene_source`（由基因派生且未允许）。
- `allow_gene_source=false` 时不删除基因派生特质；`true` 时调用原版 `RemoveTrait`，同时移除其来源基因（原版行为，不伪造）。
- 响应：`removed`（def/label/degree/suppressed/source_gene）、`traits_before/after`、`genes_before/after`、`gene_removed`、`changed`。

## 3. 定向删除一条心情/社交记忆

`POST /api/v1/pawn/edit/memory/remove`

```json
{"pawn_id": 27996, "kind": "mood", "def_name": "KnowGuestExecuted", "index": 0}
```

- `kind` 仅接受 `mood` 或 `social`；`situational`/`environment` 等实时想法不是记忆，422 `unsupported_memory_kind`。
- 选择方式：在 `MemoryThoughtHandler.Memories` 中按 kind + `def_name` 过滤后的当前列表里取零基 `index`（缺省 0）。列表在同一次主线程请求内稳定。
- 无匹配 → 422 `no_matching_memory`；index 越界 → 422 `index_out_of_range`（附 count）。
- 只调用 `RemoveMemory` 删除选中一条；响应 `removed`、`same_def_before/after`、`memories_before/after`，同 Def 的其它记忆保留可读回。

## 4. 结束当前灵感

`POST /api/v1/pawn/edit/inspiration/end`

```json
{"pawn_id": 27999}
```

- 无 inspiration handler 或无当前灵感 → 422 `no_active_inspiration`（或 handler 缺失）。
- 调用 `InspirationHandler.EndInspiration(current)`，验证 `CurState` 已清空；响应 `inspiration_before/after`、`mental_state_before/after`、`mental_state_preserved`。

## 5. 修改童年/成年背景

`POST /api/v1/pawn/edit/backstory`

```json
{"pawn_id": 28002, "slot": "childhood", "def_name": "AbandonedChild30"}
```

- `slot` 仅 `childhood`/`adulthood`；Def 必须存在且 `BackstoryDef.slot` 与请求槽一致，否则 422 `wrong_slot`/`unknown_def`。
- 写原版 `Pawn_StoryTracker.Childhood`/`Adulthood` setter，并调用 `Pawn.Notify_DisabledWorkTypesChanged()` 刷新 work 禁用。
- 响应：`slot`、`before/after`、`other_slot`、`other_before/after`、`disabled_work_types_before/after`、`changed`。

## 6. 单独设置囚犯抵抗/意志

`POST /api/v1/pawn/edit/prisoner-state`

```json
{"pawn_id": 27984, "resistance": 2.5, "will": 9}
```

- 目标必须是殖民地囚犯；至少给 `resistance` 或 `will`；两者都必须 >= 0，否则 400；非囚犯 422。
- 直接写原版 `guest.resistance` / `guest.will`，只改给出的项。
- 响应：`resistance_before/after`、`will_before/after`、`resistance_changed`、`will_changed`、`recruitable_before/after`、`faction_before/after`、`recruited=false`。不触发招募。

## 通用错误

- 缺字段/非法数值/非法 mode/slot/kind → 400（`bad_request`）。
- Pawn 不存在 → 404（`not_found`）。
- 未知 Def、非囚犯、无记忆、非当前槽、受压制等 → 422（`invalid_target`）。
- 未预期异常 → 500（`internal_error`）；不吞掉为成功。

---

# 人物详情写入补全（PAWN-W02–W05）

以下为调试/直接写入端点；均为 POST，在游戏主线程执行。未知人物 404，缺参/非法值 400，目标不适用/无该需求/未持有 422。写入成功返回前后读回；技能和需求可针对同一人物提交多项，不创建替代物。

## W02 装备实例属性

`POST /api/v1/pawn/edit/item-attributes`

```json
{"pawn_id":27979,"thing_id":"Apparel_CowboyHat27992","hit_points":42,"quality":"Excellent"}
```

- `thing_id` 必须是该人物当前背包、穿戴或装备中的同一实例（完整 `ThingID` 或数字 ID）；不存在/未持有 422。
- 仅使用耐久的物品可设置 `hit_points`，值须在 `0..MaxHitPoints`。`quality` 必须是已定义的 `QualityCategory` 名称或整数；仅当物品有 `CompQuality` 时调用 `SetQuality`，否则 422。
- 不接受 Def、新物品或替换物；至少给 `hit_points`/`quality` 之一。
- 响应：同一 `thing_id`/def、耐久/品质前后、`changed`、同 Def 另一件的耐久/品质前后（证明未变）、按原生重算的 `gear_after`（负重/上限/适温/总护甲）。

## W03 理念与记忆

### 设置殖民地囚犯理念

`POST /api/v1/pawn/ideo/set`

```json
{"pawn_id":"27985","ideo_id":"12","dry_run":false}
```

- 只接受殖民地囚犯（`IsPrisonerOfColony`）且有 `ideo` tracker；现有 `/dlc/ideology` 的 Convert 仍要求自由殖民者，互不替代。
- 目标 `ideo_id` 必须是本存档已有理念；`dry_run` 不写。
- 原生 `SetIdeo`；读回实际 id/name、certainty 前后、`still_prisoner`、阵营、resistance、will 前后；不招募、不释放。

### 添加单条心情/社交记忆

`POST /api/v1/pawn/memory/add`

```json
{"pawn_id":27979,"def_name":"TerribleSpeech","kind":"social","other_pawn_id":27982}
```

- `def_name` 必须是 `ThoughtDef` 且能生成 `Thought_Memory`；`kind=mood|social` 可省略（按 Def 类型推断），社交记忆必须给 `other_pawn_id`。
- 通过原生 `MemoryThoughtHandler.TryGainMemory` 添加，原版决定接受/合并/堆叠；不直接改关系或心情字段。
- 响应含 kind/def/other、前后总数与匹配数、心情和好感。`accepted` 表示原生接纳，`retained` 表示这次传入的记忆实例仍在列表中；合并后可能 `accepted=true`、`retained=false`，此时 `memory=null`。`merged_or_stacked` 表示已接纳且操作前后均有同类匹配，不区分原生采用合并还是堆叠。

## W04 技能等级内经验

`POST /api/v1/pawn/edit/skill-xp`

```json
{"pawn_id":27979,"skills":[{"skill_name":"Shooting","xp_since_last_level":50}]}
```

- 只写 `SkillRecord.xpSinceLastLevel`；值必须是 `[0, XpRequiredForLevelUp)` 内的有限数，否则 400（不跨级）。整批先校验，再写入。
- 等级与热情保持不变；未知/不适用技能 422。
- 响应逐技能：`level_before/after`、`xp_before/after`、`xp_required`、`xp_progress_percent`、`level_descriptor`、`passion_before/after`、`level_unchanged`、`passion_unchanged`。
- 旧 `POST /pawn/edit/skills` 的 `level`/`passion` 行为不变；本端点不接收这两个字段。

## W05 单项需求值

`POST /api/v1/pawn/edit/needs`

旧格式：
```json
{"pawn_id":27979,"food":0.8,"rest":0.9,"mood":0.2}
```
新格式二选一：
```json
{"pawn_id":27979,"need_def":"Food","value":0.8}
{"pawn_id":27979,"needs":{"Food":0.8,"Rest":0.9}}
```
- 值按 0..1 的 `CurLevelPercentage` 解释；越界或非有限值返回 400。
- 所有请求的 NeedDef 先校验，任何一项不是该人物实际持有/适用的需求 → 422 `no_such_need`，不做部分写入。
- `mood` 只写 `Need_Mood`，不再连带写 `Need_Joy`（修复旧行为）。
- 不写实时环境修正、阈值、趋势或目标值。
- 响应：`needs_before/after` 全量原生值、`changes[]` 每项 before/after/percent/changed。
