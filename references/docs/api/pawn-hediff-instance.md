# 伤病实例精确编辑（WORK-W06）

在现有伤病入口上增加按实例 `load_id` 的精确删除、严重度修改和消失组件剩余时间设置；旧 Def/部位请求保持原语义。只有调试编辑，不新增伤病、不批量、不替代正常医疗。

## 入口

### 删除

```http
DELETE /api/v1/pawn/hediff
```

```json
{"pawn_id":"Human800","load_id":269}
```

- `load_id` 存在时优先精确匹配该人物上的这一条，只删它；`hediff_def`/`body_part` 被忽略，`all` 被忽略。
- 不带 `load_id`：`hediff_def`/`body_part` 匹配；`all=false` 删除首条，`all=true` 删除全部匹配（旧语义）。
- 删除后读回目标 before（after 为 null）、匹配列表、同 Def 其它实例、健康快照。

### 严重度

```http
POST /api/v1/pawn/hediff/severity
```

```json
{"pawn_id":"Human800","load_id":268,"severity":0.2}
```

- `load_id` 存在时只改该条；否则改所有 Def/部位匹配项（旧语义）。
- `severity` 任意浮点，沿用旧入口不额外限制。

### 消失组件剩余时间

```http
POST /api/v1/pawn/hediff/remaining
```

```json
{"pawn_id":"Human800","load_id":271,"ticks_to_disappear":2500}
```

- 只接受 `load_id` 精确定位；写原生 `HediffComp_Disappears.ticksToDisappear`，不修改 `disappearsAfterTicks`。
- 与 `Dialog_DebugSetHediffRemaining` 语义一致：1h=2500、6h=15000、12h=30000、1d=60000 ticks；本接口允许任意 `>=0` 的整数。
- 目标无 `HediffComp_Disappears`（含其子类以外）→ 422 `invalid_target`。
- `ticks_to_disappear=0` 是合法边界；不保证同一 tick 内消失，原版 tick 推进时才移除。

## 参数

| 字段 | 适用 | 说明 |
| --- | --- | --- |
| `pawn_id` | 全部 | 必填；数字或 `ThingID` |
| `load_id` | 全部 | 伤病实例 `Hediff.loadID`；删除/严重度可选，剩余时间必填 |
| `hediff_def` | 删除/严重度 | 无 `load_id` 时的旧 Def 匹配 |
| `body_part` | 删除/严重度 | 无 `load_id` 时的旧部位匹配 |
| `all` | 删除 | 无 `load_id` 时是否删全部；默认 false |
| `severity` | 严重度 | 目标严重度 |
| `ticks_to_disappear` | 剩余时间 | 必填，>=0 |
| `dry_run` | 全部 | 只校验/投影，不写 |

## 读回

- `before`/`after`：单条 `HediffInstanceDto`（load_id、def、label、part、severity、是否有消失组件、ticks、disappears_after_ticks）；删除的 after 为 null。
- `matching_before`/`matching_after`：精确模式按相同 Def 和实际部位实例（包括无部位），旧模式沿用请求的 Def/部位筛选。`matching_before` 冻结于修改前。
- `other_instances_before`/`other_instances_after`：匹配列表中未被选中编辑的实例，用于证明未误改。
- `health_before`/`health_after`：`pain_total`、`bleed_rate_total`、`summary_health`、`downed`、`dead`、`hediff_count`、`hediff_defs`。
- `pawn_alive`、`removed`/`updated`、`matched_by`（`load_id`/`def_body_part`）、`dry_run`、`applied`、`changed`。
- dry-run 的 `after`/`matching_after` 是**投影值**；严重度按原生基础 setter 的上下限与致死上限投影，不触发阶段副作用。`health_after` 仍是未写入的实际值，`applied/changed=false`。实际写入时，严重度或剩余时间未变化则 `changed=false`。

## 错误

| 情况 | 状态/错误码 |
| --- | --- |
| 缺 `pawn_id`、缺 `load_id`（remaining）、缺/负 `ticks_to_disappear` | 400 `bad_request` |
| 人物不存在 | 404 `not_found` |
| `load_id` 不存在 | 404 `not_found` |
| `load_id` 属于另一个人物 | 409 `conflict`（消息带另一个 ThingID） |
| 删除/严重度没有 Def/部位匹配（无 `load_id`） | 404 `not_found` |
| remaining 目标无 `HediffComp_Disappears` | 422 `invalid_target` |

`load_id` 为 32 位整数；现有 `GET` 人物详情的 `hediffs[].load_id` 可直接使用。

## 旧合同

- 删除不带 `load_id` 且 `all=false`：删除匹配列表首条；`all=true`：删除全部匹配。
- 严重度不带 `load_id`：设置全部匹配项。
- `POST /api/v1/pawn/hediff`（新增）保持不变；本任务不新增/批量伤病。
- `load_id` 优先于 Def/部位；带 `load_id` 时不会回退到按 Def/部位匹配。

## 验收证据

隔离现场目标人物有 3 条同 Def 同部位 `Cut`（load_id 268/269/270）和 1 条 `Anesthetic`（271，`ticksToDisappear=50000`），另有其他人物 Cut（272）。单条严重度只改 268；单条删除只删 269；旧严重度改 3 条；旧删除先删首条再删剩余；Anesthetic 从 50000 改为 2500；健康/流血随删除变化；所有请求前后暂停且 `TicksGame` 不变。详见[WORK-W06 验收与自审](../references/reviews/2026-11-22-pawn-w06.md)和[原始证据](../../tests/parallel/pawn-w06/evidence-work-w06-hediff-instance.json)。
