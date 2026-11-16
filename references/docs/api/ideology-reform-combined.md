# Ideology 一次组合改革与叙事锁（ID-R15）

2026-11-13 新增 `POST /api/v1/dlc/ideology/reform/combined`。该入口按原生
`Dialog_ReformIdeo` 的真实草稿模型工作：在 `IdeoGenerator.MakeIdeo +
ideo.CopyTo` 的**一份**草稿上组合合法改动，Done 时只调用一次
`IdeoDevelopmentUtility.ApplyChangesToIdeo`（`Notify_PreReform` →
`CopyTo` → `Notify_Reformed` 一次），因此 points 只归零一次、reform_count 只 +1。
不开放 culture/foundation 任意替换。

## 原生草稿规则

原生改革对话框把改动分成两组：

1. **choose-one（只能选一组）**：structure meme、普通 meme 增删、建筑风格槽。
   同时请求两组返回 400，不会静默丢弃。
2. **change-any（可任意组合）**：名称/形容词/成员名、图标/颜色、叙事文本 +
   `descriptionLocked`、precept 增删、外观（发型/胡须/纹身 frequency/gender）。

`confirm:true` 才实际应用；`dry_run:true` 只构建并读回草稿，不改变 ideo，也不消耗
改革点。`confirmations`：原生 `ConfirmChangesToIdeo` 会以
`TryGetLostByReformingWarning` 显示确认；API 把它们放在
`confirmation_warnings`/`confirmation_required`，`confirm:true` 即视为确认。

## 请求

| body 字段 | 必填 | 含义 |
| --- | --- | --- |
| `ideo_id` | 否 | 玩家派系理念 id；省略用主教义。必须 fluid 且 `CanReformNow` |
| `structure_meme` | 否 | choose-one：替换 structure meme；与现有其它 meme 的 exclusionTags 冲突返回 409 |
| `memes_add` / `memes_remove` | 否 | choose-one：普通 meme 增删。原生 fluid 改革最多移除 1 个普通 meme，且移除后不能再新增原本不在 ideo 的 meme；不能移除玩家派系 requiredMeme、最后 1 个普通 meme，普通 meme 数须在 1..4；meme 必须对玩家派系合法且未 hidden |
| `style_categories` | 否 | choose-one：完整有序建筑风格槽（0..3，去重）；空数组清空；与真实 3 槽规则一致 |
| `style_items` | 否 | change-any： `[{def_name,frequency,gender}]`，枚举同 `reform/style`；草稿上设值并 `EnsureAtLeastOneStyleItemAvailable` |
| `precepts_add` | 否 | change-any：普通 `PreceptDef` 列表（`preceptClass` 为 `Precept`）；经原生 `CanAddPreceptAllFactions`/实例上限校验后 `AddPrecept(init:true)`。apparel/weapon/xenotype/ritual/role 等专用类仍走各自端点 |
| `precepts_remove` | 否 | change-any：现有 precept id 列表；同 `reform/precept` 的 `canRemoveInUI`/默认precept/被 meme 强制校验；原生 `RemovePrecept` 的 `takeNameFrom` 级联可能连带移除同一仪式组的另一 precept，响应准确读回实际移除 id |
| `name` / `adjective` / `member_name` | 否 | 身份文本；空串 400；变更后草稿同步 `RegenerateAllPreceptNames` |
| `icon_def` / `color_def` | 否 | 图标/颜色；未知 def 404；颜色变化清 `primaryFactionColor` |
| `description` | 否 | 自定义叙事，写入后 `descriptionLocked=true`（`descriptionTemplate=null`） |
| `description_locked` | 否 | 显式叙事锁；覆盖 `description` 的隐式 true。锁定时后续自动生成不会覆盖自定义叙事 |
| `confirm` / `dry_run` | — | 见上 |

## 响应

dry-run 与 apply 均返回同一草稿读回：`choose_one`、
`structure_meme_before/after`、`meme_defs_before/after/added/removed`、
`style_categories_before/after`、`style_items`（每项 before/after frequency/gender）、
`precept_ids_before/after/added/removed`、`precept_defs_after`、
`name/adjective/member_name/icon_def/color_def/description/description_locked`
的 before/after、`confirmation_warnings/confirmation_required`、`points_before`、
`next_reformation_points`、`reform_count_before`、`would_apply`。

apply 追加：`applied:true`、`points_after`、`reform_count_after`、
`single_apply`（reform_count=before+1 且 points=0）、以及真实 ideo 的
`meme_defs_readback`、`structure_meme_readback`、`style_readback`、
`style_items_readback`、`precept_ids_readback`、`precept_defs_readback`、
身份/图标/颜色/叙事读回。precept id 会被原生 `CopyTo` 按新实例重建，跨 apply 的
id 不保证不变；用 def_name/顺序或 apply 后的读回定位。

## 错误

| 情况 | HTTP / error_code |
| --- | --- |
| 缺 body、未知/空字段、同时两组 choose-one、重复/互相包含条目、空身份文本、4+ 风格槽 | 400 `bad_request` |
| 未知 meme/precept/style/icon/color def、precept id 不存在 | 404 `not_found` |
| 无实际变化、meme 已存在/不存在、最后普通 meme、meme 不相容、meme 数量越界、precept 不可移除/被 meme 强制/实例上限、precept 在草稿中消失、玩家派系 requiredMeme 移除 | 409 `conflict` |
| ideo 非 fluid、meme/图标对派系非法、precepts_add 含专用 precept 类 | 422 `invalid_target` |
| 无活动 Game、points 未达阈值、confirm 缺失 | 409 `conflict` / 400 |
| 原生 Apply 异常 | 500 `internal_error`，data `uncertain:true` |

## 实测（ID-R15）

g35 fluid deity ideo（Boccin Creed，13）tick 76787，`debug/invulnerability` 开启。
A：`memes_add=MaleSupremacy` + `BriefPonytail Frequent/Any` +
`precepts_add=Cannibalism_Classic` + name `Boccin Creed Reforged` + 自定义叙事
（隐式 lock）。dry-run 不变；apply 后 points 10→0、reform_count 0→1、
`single_apply:true`，meme/precept/name/叙事/lock/style 读回一致（新 precept id
由 CopyTo 重建，按 def 读回）。B：`style_categories=[Hindu]` + remove precept 696
+ adjective + 解锁叙事；points 0、reform_count 1→2、槽/移除/形容词/锁读回一致；
原生 `takeNameFrom` 连带移除 751，读回准确给出 [696,751]。负例：两组 choose-one
400、未知 meme 404、无变化 409。culture/foundation 全程未变。

证据：`tools/cua/.runtime/id-r15-run7-20261113/evidence/id-r15.json`（phase=passed）。
限制：structure-meme choose-one、combined 中的专用 precept 类、deity/symbol、
culture/foundation 与原生确认对话框 UI 未测；ID-R15 的预置分数为测试脚手架。
