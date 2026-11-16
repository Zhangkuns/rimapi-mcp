# Ideology 教义读取与受限改信（G-30）

已实现并验收：DLC-on 独立 fixture，20 项断言（2026-11-05 复审后重跑）；证据 `Reverse/evidence/dlc-G-30.json`、
原生实测 `Reverse/evidence/dlc-G-30-runtime.json`。只覆盖读取与改信；教义编辑、仪式、风格另立合同。
瓜拉兰树连接/修剪目标/种姓策略见[树连接合同](ideology-tree.md)。
造型站的发型/胡须/纹身/染发见[造型合同](ideology-styling.md)。

## 读取

`GET /api/v1/dlc/ideology?pawn_id=321`

```json
{"pawn_id":"Human321","ideo_id":"13","ideo_name":"Boccin Creed","certainty":0.9459,
 "certainty_change_per_day":0.0175,"mood":0.4252,
 "all_ideos":[{"ideo_id":"8","name":"Mankindism","precepts":51,"is_player":false}]}
```

`ideo_id` 是原生 `Ideo.id`（int 的字符串），**按存档分配，不是跨世界稳定标识**；
ID-R16 起 `all_ideos[]` 追加 `is_player_primary`、`is_player_minor`、`is_player_faction`
（`is_player` 保留主 ideo 含义）；
调用方必须先 GET 再使用当前世界返回的 id。

## 发展/改革读取（G-85）

`GET /api/v1/dlc/ideology/development?pawn_id=321`

```json
{"pawn_id":"Human318","ideo_id":"13","ideo_name":"Boccin Creed","fluid":true,
 "points":0,"next_reformation_points":10,"can_reform_now":false,"reform_count":0,
 "colonist_believer_count":3,"can_be_developed_now":true,
 "foundation_def":"Deity","deity_count_range":{"min":0,"max":0},"deities":[]}
```

- 读取原生 `Ideo.development`（`IdeoDevelopmentTracker`）：`Points`、
  `NextReformationDevelopmentPoints`、`CanReformNow`、`reformCount`、
  `CanBeDevelopedNow` 与 `Ideo.ColonistBelieverCountCached`。
- 非 fluid ideo 时 `fluid:false`，`points/next_reformation_points/reform_count` 为 null，
  `can_reform_now/can_be_developed_now` 为 false。
- 只读，不改发展点/不执行改革；改革对话框/`ApplyChangesToIdeo` 未暴露。

G102 追加只读字段 `foundation_def`、`deity_count_range`、`deities[]`
（index/name/type/gender/related_meme/icon_path），来自 `IdeoFoundation_Deity`；
非 deity foundation 时 `deities` 为空数组。

G85 实测：Boccin Creed（fluid，ideo 13）points 0、next 10、reform_count 0、
believers 3、can_be_developed_now true。证据
`Reverse/evidence/dlc-G85-ideo-development.json`，自审
[G85](../references/reviews/2026-11-08-g85-ideo-development.md)。

## 改革：风格类别（G-90）

`POST /api/v1/dlc/ideology/reform/style`

```json
{"ideo_id":"13","style_def":"Hindu","action":"add","confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`（points ≥ next threshold）、
  `style_def` 存在、`action` 为 add/remove 且确实改变当前 `thingStyleCategories`。
- 正式：复制 `Ideo`（`IdeoGenerator.MakeIdeo` + `CopyTo`），只增/删一个
  `ThingStyleCategoryWithPriority`，再调用原生
  `IdeoDevelopmentUtility.ApplyChangesToIdeo`：points 重置 0、reform_count+1、
  style 引用更新。
- 返回 `points_before/after`、`reform_count_before/after`、
  `style_categories_before/after`、`style_present_after`；dry-run 只投影。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G90 实测：Boccin Creed points 10/10、reform_count 0、无 style；dry-run add `Hindu`
200/applied false；正式 200/applied true、points 0、reform_count 1、
`style_categories_after=["Hindu"]`，`GET development` 读回一致。见
[G90 自审](../references/reviews/2026-11-08-g90-ideo-reform-style.md)。

## 改革：移除 precept（G-95）

`POST /api/v1/dlc/ideology/reform/precept`

```json
{"ideo_id":"13","precept_id":696,"confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`、precept 属于该 ideo，且满足原生 UI
  删除条件：`def.canRemoveInUI`、issue 无默认 precept、未被当前 meme 强制要求。
- 正式：克隆 `Ideo`，对克隆调用 `RemovePrecept`（会连带移除 `takeNameFrom` 依赖
  precept），再调用 `ApplyChangesToIdeo`：points 0、reform_count+1、被移除 precept
  执行 `Notify_RemovedByReforming`。
- 返回 `precept_ids_before/after`、`removed_precept_ids`、`points_before/after`、
  `reform_count_before/after`；dry-run 只投影。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G95 实测：Boccin Creed points 10/10，移除 precept 696 `Funeral`；dry-run 报告
`removed_precept_ids=[696,751]`；正式后 points 0、reform_count 1、precept_count 43，
696/751 均不在 `precept_ids_after`。见
[G95 自审](../references/reviews/2026-11-08-g95-ideo-reform-precept.md)。

## 改革：新增 precept（G-96）

`POST /api/v1/dlc/ideology/reform/precept/add`

```json
{"ideo_id":"13","precept_def":"Cannibalism_Classic","confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`、`precept_def` 存在，且原生
  `ideo.CanAddPreceptAllFactions(def)` 接受。**支持类**：plain `Precept`、`Precept_Role`、
  `Precept_Ritual`、`Precept_ThingDef`（building/relic）、`Precept_Apparel`、
  `Precept_Weapon`、`Precept_Xenotype`（ID-R02）。role/ritual 由 def 自身构造，
  ritual 额外用 `def.ritualPatternBase` 作为 `fillWith`（与原生生成/缺失仪式补建路径一致）。
- building/relic 可选 `thing_def`：必须在该 Def 的原生
  `Worker.ThingDefsForIdeo` 合法集内；省略时由原生 `Precept_ThingDef.Init` 随机选一个合法 Def。
  与原生生成顺序一致：先 `AddPrecept(init:true)`，再覆盖 `ThingDef` 并 `RegenerateName()`。
- 正式：克隆 `Ideo`，`AddPrecept(MakePrecept(def), init:true, fillWith: ritual ? pattern : null)`，
  再 `ApplyChangesToIdeo`：points 0、reform_count+1。
- 返回 `precept_class`/`is_role`/`is_ritual`、`added_precept_ids`、`added_precepts`
  （id/def/class/label/active）、`precept_defs_after`、points/reform_count 前后值。

ID-02 实测（2026-11-08）：Boccin Creed points 10/10，添加 addable 且未持有的
`DateRitualConsumable`（`Precept_Ritual`、pattern `CelebrationTree`）；dry-run 200、
added id 753；正式 points 0、reform_count 1、precept_count 45→46，
`added_precepts=[{id:753,class:Precept_Ritual,label:"Fair of Archotechnics"}]`；
fixture 读回 `pattern/source_pattern=CelebrationTree`、label 非空。
`IdeoBuilding` 当时仍 422（building/relic 需玩家选 Def）。

ID-02b 实测（2026-11-08，同一存档）：添加 `IdeoRelic`（原生选 `Gun_HellcatRifle_Unique`、
label "Dowlash"）、添加 `IdeoBuilding` 并显式 `thing_def=SculptureSmall`（合法集内，生成
`Precept_Building` id755、label "Archonic Shape"、ThingDef=SculptureSmall）；ritual 带
`thing_def` 400、未知 thing_def 404。证据 `an05-id02b-building-relic.json`，runner
`tests/id02b_building_relic_add.py`。

证据 `Reverse/evidence/an05-id02-special-precept.json`，runner `tests/id02_special_precept_add.py`。
- 角色路径与 ritual 共用同一分支（源码支持），本 fixture 的 ideo 已拥有全部可加角色，
  未能在本样本运行时验证角色新增；替换仍只支持 plain Precept。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。
- dry-run 只投影；以上字段是正式提交后的读回。

G96 实测：Boccin Creed points 10/10，添加 `Cannibalism_Classic`；dry-run 报告
`added_precept_ids=[752]`；正式 added id 753、points 0、reform_count 1、
`precept_count_after=46`，`precept_defs_after` 含该 def；fixture 与 `GET development`
读回一致。见 [G96 自审](../references/reviews/2026-11-08-g96-ideo-reform-precept-add.md)。

## 特殊教义添加：服饰、武器贵贱、偏好异种（ID-R02）

`POST /api/v1/dlc/ideology/reform/precept/add`

```json
{"ideo_id":"13","precept_def":"ApparelDesired_Strong_Subordinate",
 "apparel_def":"Apparel_CowboyHat","confirm":true,"dry_run":true}
{"ideo_id":"13","precept_def":"NobleDespisedWeapons",
 "noble_weapon_class":"Melee","despised_weapon_class":"Ranged","confirm":true}
{"ideo_id":"13","precept_def":"PreferredXenotype",
 "xenotype_def":"Baseliner","confirm":true}
```

- `Precept_Apparel` 必填 `apparel_def`：必须在该 def 的原生
  `Worker.ThingDefsForIdeo` 集合内、`CanUse` 接受，且 `ShouldSkipThing` 未拒绝
  （同理念已偏好同一服饰）。
- `Precept_Weapon` 必填 `noble_weapon_class`/`despised_weapon_class`：必须精确匹配
  一个原生 `WeaponClassPairDef`（first=noble、second=despised）；同序或反序 pair
  已存在时拒绝。
- `Precept_Xenotype` 必填 `xenotype_def`：同理念已偏好同一 xenotype，或该 def
  达到原生实例上限（默认 3）时拒绝。自定义 xenotype 无稳定 defName，不在本行。
- 创建顺序与原生 `IdeoUIUtility` 改革菜单一致：apparel 先赋 `apparelDef` 再
  `AddPrecept(init:true)`；weapon 先 `AddPrecept` 再 `Init`；xenotype 先赋
  `xenotype` 再 `AddPrecept(init:true)`。随后统一走一次
  `IdeoDevelopmentUtility.ApplyChangesToIdeo`（points 0、reform_count+1）。
- 合法选值发现：`GET /dlc/ideology/precepts?ideo_id=13&include_add_options=true`
  在原有 precept 读回外返回 `add_options.apparel/weapons/xenotypes`；取值同原生
  改革菜单的 `ThingDefsForIdeo`/`ShouldSkipThing`/`CanUse`、`WeaponClassPairDef` 与
  未偏好 `XenotypeDef` 集合。
- `GET precepts` 行及 `added_precepts` 读回 `apparel_def`/`target_gender`、
  `noble_weapon_class`/`despised_weapon_class`、`xenotype`/`custom_xenotype`。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

ID-R02 实测（2026-11-12，隔离 `g35_deathresting` 副本）：Boccin Creed 45 precept、
points 0、reform_count 0。`include_add_options` 发现 44 个服饰、5 个武器 pair、
12 个异种。三次正常改革依次添加 `Apparel_CowboyHat`（id753，target_gender None）、
`Melee/Ranged`（id755）、`Baseliner`（id757）；每次 points 归零、reform_count 0→3。
API GET 与独立原生 fixture 读回三类实例值一致；跨 pair 武器组合 422、
重复服饰/异种 409 且未消耗改革。证据
`tools/cua/.runtime/id-r02-20261112d/evidence/id-r02.json`，runner
`tests/parallel/ideology/id_r02_special_precept_add.py`。fixture 在每次改革前把
该 fluid ideo 补到原生改革阈值（已披露，不改 precept）。边界：自定义 xenotype、
服饰性别编辑（ID-R03）和偏好异种的实际心情/确定性后果未测。

## 仪式非基准 pattern 选择与完成（ID-R04）

`POST /api/v1/dlc/ideology/reform/precept/add` 的 `Precept_Ritual` 请求可选
`ritual_pattern`（`RitualPatternDef.defName`）；省略时仍用
`def.ritualPatternBase`。合法非基准 pattern 与原生 `IdeoUIUtility` 改革菜单同源：

- 同 `patternGroupTag` 的其它 pattern；
- 当前 ideo 的 meme `replaceRitualsWithTags` 命中 def 基础 pattern tags 时，该
  meme 的 `replacementPatterns`；
- 都须通过 `RitualPatternDef.CanFactionUse(FactionForRandomization)`；带消耗型
  建筑目标的 pattern 还须 ideo 拥有对应 meme（`ignoreConsumableBuildingRequirement`
  除外）。

发现：`GET /api/v1/dlc/ideology/precepts?include_add_options=true` 的
`add_options.ritual_patterns` 每行含 `precept_def`、`pattern_def`、`label`、
`source`（`base`/`group`/`meme:<def>`）、`is_base`、`can_add`、`reason`。

读回：`GET precepts` 的仪式行现在 `ritual_pattern` 是实例实际
`sourcePattern`，并新增 `base_ritual_pattern`、`behavior`、`outcome`、`roles`；
`added_precepts` 同步这些字段。

ID-R04 实测（2026-11-12，隔离 `g35_deathresting` 副本）：选择 addable 的
`Festival` + 非基准 `CelebrationSkyLanterns`，正常改革一次添加 id753
（points 0、reform_count 1）；`ritual/config` 设 `is_anytime=true` 后，以
unroofed `RitualSpot` + 木料为 target 通过 `ritual/start` 启动精确实例，
7502 tick 后原生完成（terminal_tick 84289），三名参与者获得
`UnimpressiveSkyLanterns` 结果记忆；非法跨组 `FuneralStandard` 422。
证据 `tools/cua/.runtime/id-r04-run3-20261112/evidence/id-r04.json`，runner
`tests/parallel/ideology/id_r04_ritual_pattern.py`。fixture 只补改革阈值、
unroofed spot 与木料；未逐 pattern 组重测，日期义务路径未纳入本样本。

## 理念级外观与建筑风格（ID-R05）

`POST /api/v1/dlc/ideology/reform/style` 扩展 `style_items`/`style_categories`：
一次正常改革同时应用发型/胡须/纹身 frequency/gender 与最多 3 个建筑风格槽；
`classic_style_categories` 按 classic 模式独立规则写
`IdeoManager.selectedStyleCategories`，不消耗改革点。
`GET precepts?include_style=true` 返回 `style` 块。详见
[ideology-style-editor.md](ideology-style-editor.md)。

## 运行中理念预设保存（ID-R10）

`GET /api/v1/dlc/ideology/presets` 读取原生 `Ideos` 目录预设文件与
`SaveFileInfo` meta header；`POST /api/v1/dlc/ideology/presets/save` 用原生
`GameDataSaveLoader.SaveIdeo` 把运行中玩家理念写成 `<file_name>.rid`，支持
显式 `overwrite`、`dry_run` 与可选玩家派系 `ideo_id`，不打开对话框、不加载预设。
请求/响应/错误见 [ideology-presets.md](ideology-presets.md)。

## 角色社交能力的真实结果（ID-R13）

`POST /pawn/ability` 以原生 `Ability.GetJob` 路径施放角色能力；`GET /pawn/ability`
读回能力列表、组冷却与可用性。道德向导（Moralist）角色授予 Reassure/Counsel/
PreachHealth，三者共享 `AbilityGroupDef Moralist` 的 180000 tick 组冷却；真实
结果与证据见 [ideology-social-abilities.md](ideology-social-abilities.md)。

## 已有特殊教义编辑（ID-R03）

`GET /api/v1/dlc/ideology/precept/edit-options` +
`POST /api/v1/dlc/ideology/precept/edit` 按原生 `Dialog_EditPrecept` 权限编辑
名称/锁名、领导男女头衔、角色服饰要求、圣物材料、建筑样式；
`Precept_RitualSeat` 另支持座位种类替换。编辑是就地修改，不消耗发展点或
`reform_count`。请求/字段/权限矩阵见
[ideology-precept-edit.md](ideology-precept-edit.md)。

## 改革：替换 precept（G-97）

`POST /api/v1/dlc/ideology/reform/precept/replace`

```json
{"ideo_id":"13","precept_id":702,
 "replacement_def":"Nudity_Female_Mandatory","confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`、旧 precept 是带 issue 的 plain
  `Precept` 且未被 meme 强制要求；replacement 是同 issue 的 plain `Precept`，
  且 `ideo.CanAddPreceptAllFactions(replacement)` 接受。
- 正式：克隆 `Ideo`，先 `AddPrecept(replacement, init:true)`，再
  `RemovePrecept(old, replacing:true)`，最后 `ApplyChangesToIdeo`：points 0、
  reform_count+1。
- 返回旧/新 precept id、`issue`、`added_precept_ids`、`removed_precept_ids`、
  `precept_ids_before/after`、`precept_defs_after`、points/reform_count 前后值。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G97 实测：Boccin Creed points 10/10，precept 702
`Nudity_Female_UncoveredGroinOrChestDisapproved` 替换为 `Nudity_Female_Mandatory`
（issue `Nudity_Female`）；dry-run added 752/removed 702；正式 added 753/removed 702、
points 0、reform_count 1，`precept_defs_after` 含新 def、不含旧 def。见
[G97 自审](../references/reviews/2026-11-08-g97-ideo-reform-precept-replace.md)。

## 发展点自然获得与 style+relic 组合（ID-04，2026-11-08）

- 自然来源：正常 Convert 能力施放使囚犯改信（原生 `Pawn_IdeoTracker` → certainty≤0 →
  `SetIdeo` → `Ideo.Notify_MemberGainedByConversion`），fluid ideo 的 development points +1。
  实测 `ai_g31_fixture`（tick 74742）Human66734 由 Nova-Archotechnics 改为 Boccin Creed，
  points 0→1。
- fixture 随后把同一 ideo 补到改革阈值（已披露，不冒充自然累计）；两次合法改革：
  `reform/style` 添加 `Hindu`（points 10→0、reform_count 0→1），再 `reform/precept/add`
  添加 `IdeoRelic`（id753、label "Lightfocus"、ThingDef `RelicInertSwordHandle`，
  reform_count 1→2）。最终 points 0、reform_count 2、style_categories ['Hindu']。
- 证据 `Reverse/evidence/an05-id04-development.json`，runner `tests/id04_development_combo.py`。
- 边界：自然累计只测了 1 次改信；未跑到“完全自然攒满 10 点再改革”的完整循环；
  relic 的生成/收集、style 的逐项建筑风格未验。

## 改革：normal meme add/remove（G-98）

`POST /api/v1/dlc/ideology/reform/meme`

```json
{"ideo_id":"13","meme_def":"MaleSupremacy","action":"add","confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`、目标 `MemeDef.category == Normal`；
  add/remove 必须实际改变当前 normal memes；normal 数量须在
  `IdeoFoundation.MemeCountRangeAbsolute`（1..4）内；新集合不得有 exclusionTags 冲突；
  不能移除最后一个 normal meme。
- 正式：克隆 `Ideo`，改 `memes` 并 `SortMemesInDisplayOrder`，调用
  `IdeoFoundation.EnsurePreceptsCompatibleWithMemes`（移除/补齐相关 precept）和
  `RecachePrecepts`，再 `ApplyChangesToIdeo`：points 0、reform_count+1。
- 返回 `meme_defs_before/after`、`precept_defs_before/after`、points/reform_count
  前后值；dry-run 用克隆计算 precept 变化，不改当前 ideo。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G98 实测：Boccin Creed normal memes `Raider`，添加 `MaleSupremacy`；dry-run
`meme_defs_after_clone=["Structure_Archist","MaleSupremacy","Raider"]`；正式
points 0、reform_count 1，`meme_defs_after` 含 `MaleSupremacy`，fixture/`GET
development` 读回一致。见
[G98 自审](../references/reviews/2026-11-08-g98-ideo-reform-meme.md)。

## 改革：structure meme（G-99）

`POST /api/v1/dlc/ideology/reform/structure-meme`

```json
{"ideo_id":"13","meme_def":"Structure_Animist","confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`、目标 `MemeDef.category == Structure`、
  与当前 structure meme 不同、且不与其它 meme 的 exclusionTags 冲突。
- 正式：克隆 `Ideo`，移除旧 structure meme、加入新 def，`SortMemesInDisplayOrder`，
  调用 `IdeoFoundation.EnsurePreceptsCompatibleWithMemes`、`RecachePrecepts`，再
  `ApplyChangesToIdeo`：points 0、reform_count+1。
- 返回 `structure_meme_before/after`、`meme_defs_before/after`、
  `precept_defs_before/after`、points/reform_count 前后值；dry-run 用克隆计算。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G99 实测：Boccin Creed structure `Structure_Archist` 替换为 `Structure_Animist`；
dry-run `meme_defs_after_clone=["Structure_Animist","Raider"]`；正式 points 0、
reform_count 1、`structure_meme_after=Structure_Animist`，fixture/`GET development`
读回一致。见
[G99 自审](../references/reviews/2026-11-08-g99-ideo-reform-structure-meme.md)。

## 改革：identity（G-100）

`POST /api/v1/dlc/ideology/reform/identity`

```json
{"ideo_id":"13","name":"Boccin Creed II",
 "description":"G100 reformed identity","confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`；至少一个字段实际改变；显式提供的
  `name/adjective/member_name/description` 不得为空。
- 正式：克隆 `Ideo`，设置 `name/adjective/memberName/description`，再
  `ApplyChangesToIdeo`：points 0、reform_count+1。
- 返回各字段 before/after、points/reform_count 前后值、`*_readback`；dry-run 只投影。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G100 实测：Boccin Creed points 10/10，name→`Boccin Creed II`、description→
`G100 reformed identity`；dry-run 200/applied false；正式 points 0、reform_count 1，
`name_readback`/`description_readback` 与 fixture 读回一致。见
[G100 自审](../references/reviews/2026-11-08-g100-ideo-reform-identity.md)。

## 改革：symbol（icon/color）（G-101）

`POST /api/v1/dlc/ideology/reform/symbol`

```json
{"ideo_id":"13","icon_def":"AnimalsA","color_def":"LightGrey",
 "confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`；`icon_def`/`color_def` 至少提供一个，
  且至少一个实际改变；未知 defName 返回 404，未改变返回 409。
- 正式：克隆 `Ideo`，设置 `iconDef/colorDef`；颜色改变时清 `primaryFactionColor`
  （与原生 `Dialog_ChooseIdeoSymbols.TryAccept` → `SetIcon(..., clearPrimaryFactionColor)`
  一致），再 `ApplyChangesToIdeo`：points 0、reform_count+1。
- 返回 icon/color before/after、points/reform_count 前后值、`*_readback`；
  dry-run 只投影。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。

G101 实测：Boccin Creed points 10/10，icon `SpiralsC`→`AnimalsA`、color
`LightMauve`→`LightGrey`；dry-run 200/applied false；正式 points 0、reform_count 1，
fixture 与 `GET development` 读回一致。见
[G101 自审](../references/reviews/2026-11-08-g101-ideo-reform-symbol.md)。

## 改革：deity（G-102）

`POST /api/v1/dlc/ideology/reform/deity`

```json
{"ideo_id":"13","deity_index":0,"name":"G102 Deity Renamed",
 "type":"Goddess of Tests","gender":"Female",
 "confirm":true,"dry_run":true}
```

- 前置：fluid ideo、`development.CanReformNow`、foundation 为
  `IdeoFoundation_Deity`、`deity_index` 有效；`name/type/gender` 至少一个实际改变；
  `gender` 只接受 `None/Male/Female`。
- 正式：克隆 `Ideo`，编辑克隆 foundation 的目标 deity，调用
  `RegenerateAllPreceptNames`/`RegenerateDescription`，再 `ApplyChangesToIdeo`：
  points 0、reform_count+1。
- 返回 name/type/gender before/after、points/reform_count 前后值、`*_readback`；
  dry-run 只投影。
- 非 dry 必须 `confirm:true`；`Idempotency-Key` 按 `game_memory`。
- `action`（默认 `edit`）：
  - `edit`：改 `name/type/gender`，见上。
  - `remove`：删除 `deity_index`，要求数量 > `DeityCountRange.min`；只
    `RegenerateDescription`。
  - `randomize_all`：原生 `GenerateDeities()` 重建全部 deity（对应
    RandomizeDeities 按钮），再 `RegenerateAllPreceptNames`/`RegenerateDescription`。
  - `add`：要求 `name/type` 非空、数量 < `DeityCountRange.max`；用公开 `Deity`
    字段追加（`icon_path=UI/Deities/DeityGeneric`，`related_meme` 取未使用 meme），
    再 `RegenerateAllPreceptNames`/`RegenerateDescription`。这是“随机 AddDeity +
    EditDeity”的组合，不调用私有 `GenerateNewDeity`/`FillDeity`。

G102 实测：fixture 在 fluid Boccin Creed 上 seed `Structure_TheistAbstract` 与一个
deity（基础存档为 Archist、无 deity）；编辑 `G102 Deity/God of Tests/Male` →
`G102 Deity Renamed/Goddess of Tests/Female`；dry-run 200/applied false；正式 points 0、
reform_count 1，fixture 与 `GET development` 读回一致。见
[G102 自审](../references/reviews/2026-11-08-g102-ideo-reform-deity.md)。

G104 实测：seed 两个 deity 后 `remove` index 0 → 1 个（`G104 Deity 2`），points 0、
reform_count+1；再 `randomize_all` → 原生重建 1–4 个、points 0、reform_count+1；
数量等于最小值时 remove 409。见
[G104 自审](../references/reviews/2026-11-08-g104-ideo-reform-deity-actions.md)。

G106 实测：seed 1 个后 `add`（`G106 Deity/God of Additions/Female`）→ 2 个、points 0、
reform_count+1，`deities_after=[G102 Deity,G106 Deity]`；seed 4 个（等于 max）时 add
409，缺 type 400。见
[G106 自审](../references/reviews/2026-11-08-g106-ideo-reform-deity-add.md)。

## 角色 precept 指派（ID-06a，2026-11-10）

`POST /api/v1/dlc/ideology/role/assign`

```json
{"ideo_id":"8","precept_id":733,"pawn_id":"Human321","dry_run":true}
```

- `ideo_id` 可选，必须是玩家派系 ideo（primary 或 `IdeosMinor`），省略沿用主 ideo；
  非玩家派系 ideo 404。ID-R16 起允许玩家派系的非主（minor）理念。
- `precept_id` 必须是所选 ideo 现有的 `Precept_Role`（`GET /dlc/ideology` 的 precept 或
  fixture 读取均可定位）；不存在 404。
- `pawn_id` 非空：必须是已生成、存活、自由殖民者且 `pawn.Ideo == ideo`（422）；已是该角色
  返回 `changed:false`。`pawn_id` 为空表示清除单持角色（`Precept_RoleSingle`）；
  清除 leader 角色且 faction 已有 leader 时 409；多持角色（`Precept_RoleMulti`，如专精）
  没有整体清除语义，返回 409，须按原生 `Precept_Role.Unassign` 逐 pawn 处理。
- `Precept_RoleMulti` 与非主 ideo 的 leader：`leaderRole` 只在主 ideo 可选/可指派（422），
  与 `SocialCardUtility`/`RitualUtility.AllRolesForPawn` 一致。
- 提交调用原生 `Precept_Role.Assign(pawn, addThoughts:true)`，读回 `ideo_id`、
  `is_primary_ideo`、`chosen_pawns`、`abilities`、`faction_leader`。dry-run 不改。
- 精确合同：`policy/pawn_ai/validate/game_memory`。
- 添加新的角色 precept 仍走 `POST /dlc/ideology/reform/precept/add`；本轮样本的 fluid ideo
  已持全部角色，role add 的运行样本仍缺（准备不足，不判业务失败）。

证据：`Reverse/evidence/id06a-role-assign-20261110.json`（Moralist/Cleric 指派到
`Human321`、五项角色能力读回、清除后空）；`id06a-role-add-20261110.json` 为准备不足的
failed 尝试。ID-R16：玩家派系 minor ideo（Mankindism，8）的 `Precept_RoleSingle`
Moralist 465 指派/清除，`Precept_RoleMulti` ResearchSpecialist 通过其 ideo 的 RoleChange
ritual 指派并由原生结果读回；见
[ID-R16 自审](../references/reviews/2026-11-13-id-r16-non-primary-ideo.md)。

## 改信

`POST /api/v1/dlc/ideology`

```json
{"pawn_id":"321","ideo_id":"8","confirm":true,"dry_run":false}
```

成功数据：`{pawn_id,previous_ideo_id,ideo_id,ideo_name,certainty_before,certainty_after,mood_before,mood_after,changed,applied,dry_run}`。

- 同教义：`changed:false, applied:false`，不调 setter、不发事件。
- 真实改信调用公开 `Pawn_IdeoTracker.SetIdeo(Ideo)`，立即替换信念。
- **确定度不会固定重置**：两次实测分别得到 0.946→0.889、0.946→0.739，
  改写后按原生规则继续变化；差值规则未推导，API 只回读观测值。
- `mood_after` 是改信后立即读到的需求值；实测该 tick 与改信前相同，
  因此**不能**把它当作 precept/心情重算完成的证据。
- 非预演必须 `confirm:true`；`dry_run` 只投影，`certainty_after`/`mood_after` 等于 before 值。
- 原生 `SetIdeo` 抛异常时返回 500 `uncertain:true` 并读回
  `observed_ideo_id`、`observed_ideo_name`、`observed_certainty`、`readback_ok`。

## 错误与 DLC 门

| 情况 | 码 |
| --- | --- |
| 缺字段 / 缺 confirm | 400 `bad_request` |
| 未知 pawn / 未知 ideo id | 404 `not_found` |
| 非自由殖民者 / 动物 | 422 `invalid_target` |
| Ideology 未启用 | 422 `dlc_missing` |

## 未覆盖

- 渐进式、可失败的教化已有 `convert/ability`（G47/G77–G79）与囚犯 Convert 互动（G80），
  不等于直接 `SetIdeo`；warmup/冷却等变体仍未验，不再记为整个入口未暴露。
- 自然发展点获取→改革的组合、全局殖民地心情重算、NPC
  改信未测；G90 覆盖 style add/remove，G95 覆盖可删 precept removal，G96 覆盖 plain
  precept add，G97 覆盖 plain precept replace，G98 覆盖 normal meme add/remove，G99
  覆盖 structure meme replace，G100 覆盖 name/adjective/member_name/description，
  G101 覆盖 icon/color，G102 覆盖现有 deity 的 name/type/gender 编辑，G104 覆盖 remove/randomize_all
  （使用原生 GenerateDeities），G106 覆盖显式 name/type/gender 的 add。
  单个 add 的原生生成/规则一致性、特殊 precept 配置见[剩余清单](../product-specs/dlc-gap-audit.md) ID-01/02。
  G96/G97 只依赖原生 `CanAddPreceptAllFactions`，未额外拦截 incompatible-pair 警告/
  重复 def；G99 只替换 memes，不重建 foundation/deities。
- 读到意外状态（`Find.IdeoManager` 或玩家派系缺失）时 `all_ideos` 返回空列表而不是报错；
  该分支未在真实损坏存档上验证。

## G31 仪式读取兼容、确认资格与 C1/C2 限定实现

`GET /api/v1/dlc/ideology/ritual` 的 `active[]` 保留旧字段 `precept_def`：来自活动仪式的 `Ritual.def.defName`，缺仪式或 Def 时为 null。
首版 status/start 的角色构建沿用开始窗口的 PawnFilter（已有 Lord、动物角色、subhuman 和按角色区分的教义限制），再交给原生资格检查；不改变普通 `game/load` 的异步排队语义。

指定目标的 `GET /api/v1/dlc/ideology/ritual/status`、start 的 dry-run 和真实提交共同执行原生 `Dialog_BeginRitual.BlockingIssues`，使用本次构建的同一份 assignment 和 target。`eligibility.blocking_issues` 包括参与者、温度、观众、outcome、目标、分组必需角色及额外必需人物等原生确认条件；`can_start` 仍单独反映 `CanStartRitualNow`，不能独立视为允许提交。阻塞项非空时 start 返回 409 `conflict`、`outcome:rejected`、`submitted/started/applied:false`，不调用 `TryExecuteOn`，不发 operation 事件。资格读取不打开窗口、不结束人物 Job、不随机结算。

完整的实例键、`GET status`/`POST start` 参数与响应、检查次序和限定证据见 [仪式实例/资格/启动合同](ideology-ritual.md)。

此前读取兼容和加载控制的限定证据见 [C1 修复记录](../references/reviews/2026-11-07-g31-c1-repair.md)。C1 原生确认门禁、真实拒绝及普通 Festival 开始/原生完成的回归见 [资格修复记录](../references/reviews/2026-11-07-g31-c1-eligibility-repair.md) 和 [C1 限定交付](../references/reviews/2026-11-07-g31-c1-acceptance.md)。

C2 已增加原生强制角色资格（`assignments.PawnNotAssignableReason`）与 `Conversion` 样本：709、`Lord_3`、moralist/convertee、tick74742→80189、certainty 下降仍 ideo7、三观众 `EffectiveConversion`，见 [C2 限定交付](../references/reviews/2026-11-07-g31-c2-acceptance.md)。C2 LeaderSpeech 716 本组已运行（`speaker`、`Lord_3`、74742→79743、四听众 `InspirationalSpeech`，fixture 观测 `LordJob_Joinable_Speech`），见 [Speech 限定交付](../references/reviews/2026-11-07-g31-c2-speech-acceptance.md)，已由监督端限定验收。当前只覆盖 C1 Festival、C2 Conversion 与 C2 LeaderSpeech 三个普通样本；其它仪式类型、非玩家/囚犯/动物正向角色、日期义务、取消/中断、保存加载和 G31 整体尚未完成验收。

## F-ID 来源核对

Ideology 全部 F-ID 行的 native 入口 → 合法参数/资格 → API → 原生结果/证据对账，
以及未测/差集（`.rid` 载入、Blinding、Gaumaker 等）见
[2026-11-13 F-ID 来源核对包](../references/reviews/2026-11-13-f-ideology-source-audit.md)。
