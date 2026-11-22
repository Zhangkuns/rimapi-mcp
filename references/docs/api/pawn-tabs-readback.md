# 人物详情六页签读回（PAWN-Q01–Q06）

只读查询补全。全部在游戏主线程执行，直接读原版对象/方法，不改游戏状态。请求体为无（GET），响应统一 `ApiResult`；未知人物 404，缺参数 400，无适用对象/需求 422。所有路由以 `/api/v1` 为前缀。

## Q01 日志

`GET /pawns/log?pawn_id=<id>&limit=50&scope=all&since_tick=&until_tick=`

- 合并 `Find.PlayLog` 与 `Find.BattleLog`，顺序与 `ITab_Pawn_Log_Utility` 一致：按原生 `LogEntry.Age` 升序（新在前），同年龄按绝对 tick。
- `scope=all|social|battle`（默认 all）；`since_tick`/`until_tick` 是绝对 tick（`TicksAbs`）；`limit` 1..500（默认 50）。
- 每条：`kind`、`entry_type`、`log_id`、`tick_abs`、`age_ticks`、`age_days`、`time_ago`、`text`（`ToGameStringFromPOV`，以该人物视角）、双方 id/名字、`interaction_def_name/label`、`battle_id/name/index`、`hit`、`deflected`、`damaged_parts`、`show_in_compact_view`。
- 战斗命中：`LogEntry_DamageResult` 的 `deflected` 或非空 `damagedParts` 为 true；失手为空且未偏转。`RangedFire` 等无伤害结果的条目 `hit=null`。
- 时间基准：`time_ago` 用 `entry.Age`（`TicksAbs - entry.Ticks`），返回 `TicksGame`、`TicksAbs`、`time_basis="ticks_abs"`、`negative_age_count`。旧社交 GET `GET /pawns/interactions/log` 保留，同样改用 `Age`，并新增 `tick_abs`/`age_ticks`；`ticks` 仍为绝对 tick。
- 错误：`scope` 非法或 `since_tick > until_tick` → 400；人物不存在 → 404。

## Q02 装备

`GET /pawns/gear?pawn_id=<id>`

- 与 `ITab_Pawn_Gear` 同一原生输入：`MassUtility.GearAndInventoryMass`、`Capacity`、`EncumbrancePercent`、`IsOverEncumbered`；`StatDefOf.ComfyTemperatureMin/Max`（°C）。三类总护甲按原版装备页计算：人物自身护甲与穿戴层叠加，按各部位覆盖权重汇总。
- `items[]`：背包堆叠、穿戴、装备逐件 `thing_id`、def、label、category、`stack_count`、原生 `Mass × stack_count`、耐久、品质、逐件护甲。
- 不把单件重量/护甲当人物总值。

## Q03 社交与理念

`GET /pawns/social?pawn_id=<id>`

- 原版名单：该地图所有其他 humanlike（不只殖民者），带 `identity`（colonist/prisoner/slave/guest/other）、faction、`PawnRelationDef` 实际关系（不再用好感阈值伪造 Friend/Rival）、`opinion_of_other` 与 `opinion_of_me`、Ideo 角色 def/label。
- `beliefs` 同下；`direct_relations` 保留原生直接关系。

`GET /pawns/ideology?pawn_id=<id>`

- 任意有理念的人物，包括殖民地囚犯：`ideo_id`、`ideo_name`、`certainty`、`certainty_percent`、`certainty_change_per_day`、`certainty_change_percent_per_day`、`prisoner`、`slave`、`conversion_mode`（`ExclusiveInteractionMode`）、`conversion_enabled`（`IsInteractionEnabled(Convert)`）、`role_def_name/label`。
- 无理念 → 422；人物不存在 → 404。普通 GET，不要求开发者模式；教化前后可对同一 `ideo_id` 比较 `certainty`，ID 变化即改信，不能跨理念直接相减。

## Q04 档案

`GET /pawns/bio?pawn_id=<id>`

- `biological_age`、`biological_age_float`、`chronological_age`/`chronological_age_ticks`。
- `childhood`/`adulthood`：`BackstoryDef` def、原版 `TitleCapFor(gender)`、槽、该背景的 `disabled_work_types`。
- `traits[]`：def、label、`degree`、`suppressed`、`source`（natural/gene/scenario）、`source_gene`、`disabled_work_tags`、`disabled_work_types`。
- `work_restrictions[]`：每条工作限制及来源（`backstory_childhood`/`backstory_adulthood`/`trait`/`gene`/`ideology_role`）和来源 Def/label；`disabled_work_types` 为原生 `Pawn.GetDisabledWorkTypes()`。
- 聚合详情 `GET /pawns/details` 与其技能字段不变。

## Q05 需求

`GET /pawns/needs?pawn_id=<id>&need_def=<optional Def>`

- `needs[]` 来自 `pawn.needs.AllNeeds`：def、label、`cur_level`、`max_level`、`level_percent`、`cur_instant_level/percent`、`show_on_need_list`、原生 `change_arrow`/`trend`、`major`、`list_priority`、`fall_per_day`、`tip`。
- `food`：`Need_Food` 的 level/max/percentage、`HungerCategory`、`starving`、`food_fall_per_tick`、`ticks_until_hungry_when_fed`、`nutrition_wanted`、饥饿阈值。
- 无该 `need_def` → 422 `no_such_need`（不是 0）；存在但数值为 0 时正常返回 0。
- 保留 `mood_details` 及其分组不变。

## Q06 健康与手术

`GET /pawns/health?pawn_id=<id>`

- `dead`、`downed`、`summary_health`、`pain_total`/`pain_percent`、`bleed_rate_total`、`bleeding`、`ticks_until_death_blood_loss`（原生 `HealthUtility.TicksUntilDeathDueToBloodLoss`，无流血时 null）、`blood_loss_time`。
- `capacities[]`：`PawnCapacityDef.CanShowOnPawn` 可见项，按 `listOrder`：def、label、`level`、`percent`、`capable`（>= `minForCapable`）、`min_for_capable`。

`GET /pawn/surgery/available?pawn_id=<id>`

- 从该人物的 `def.AllRecipes` 取当前可用手术，并应用原版可用报告与材料可见性规则。指定部位的手术只列 `GetPartsToApplyOn` 且 `AvailableOnNow` 的部位；无需部位的手术可返回空 `parts[]`，不会因此被过滤。`parts[]` 非空时含 part id、def、label、custom label、生命和缺失状态。
- 只读菜单；不建账单、不做手术、不移动人物。
- 旧的 `GET /pawn/surgery/options` 保持原样；其缺 `recipe_def` 的 500 由 biotech 在控制器层修复，不在本组重复实现。

## 错误与限制

- 未知人物 404；缺 `pawn_id` 由现有 `RequestParser` 返回 400；无理念/无该需求/不可手术对象 422。
- 只读，不注册 Idempotency-Key；查询前后 `TicksGame` 不变。
- 不伪造 UI 文本或数值；没有 UI 截图 OCR 对照，使用同一原生对象与 REST 投影对照。
