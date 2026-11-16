# Ideology 理念级外观与建筑风格编辑（ID-R05）

2026-11-12 扩展现有 `POST /api/v1/dlc/ideology/reform/style`，不新增路由。
普通模式把发型/胡须/纹身的 frequency/gender 与建筑风格槽合成**一次正常改革**；
classic 模式单独写 `IdeoManager.selectedStyleCategories`，不消耗改革点。

## 读回

`GET /api/v1/dlc/ideology/precepts?ideo_id=13&include_style=true` 追加 `style`：

```json
{"style_items":[{"kind":"hair","def_name":"BriefPonytail",
  "frequency":"Frequent","gender":"Female"}],
 "style_categories":[{"category":"Hindu","priority":3.0,"slot":0}],
 "available_style_categories":[{"def_name":"Morbid","label":"morbid"}],
 "classic_mode":false,"classic_selected_style_categories":[]}
```

`style_items` 含全部 HairDef/BeardDef/TattooDef 当前值与合法枚举；普通改革最多
3 个建筑风格槽，优先级按原生 `3 - slot`。

## 普通改革

```json
{"ideo_id":"13","style_items":[
  {"def_name":"BriefPonytail","frequency":"Frequent","gender":"Female"},
  {"def_name":"Princess","frequency":"Never","gender":"Any"}],
 "style_categories":["Hindu","Morbid"],"confirm":true,"dry_run":true}
```

- frequency：`Never/Rare/Uncommon/Normal/Common/Frequent`；gender：
  `Male/MaleUsually/Any/FemaleUsually/Female`；未知 def/枚举 400/404。
- `style_categories` 是完整有序槽列表（0..3，去重）；4 个槽 400。空列表表示清空。
- 先在 `IdeoGenerator.MakeIdeo + CopyTo` 草稿上写值、`EnsureAtLeastOneStyleItemAvailable`，
  再调用一次 `IdeoDevelopmentUtility.ApplyChangesToIdeo`：points 0、
  reform_count+1；不是循环 add/remove 多次消费改革。
- 返回 `changed_items/changed_categories`、前后槽列表、points/reform_count、
  `style_after`；dry-run 返回 `after_preview`。无变化 409。

## classic 模式

```json
{"classic_style_categories":["Hindu","Morbid"],"confirm":true}
```

- 仅 `Find.IdeoManager.classicMode == true`；否则 422。
- 直接替换 `selectedStyleCategories`（0..3、去重、未知 404、4 个 400），
  执行原生 `ResetStylesForThingDef`/`RecachePossibleBuildables`/`DesignationCategoryDef.DirtyCache`。
- 不消耗 points、不改 reform_count；返回 `points_unchanged:true` 与 `style_after`。

## 实测

隔离 `g35_deathresting` 副本（tick 76787、Boccin Creed id13）：初始无建筑风格槽；
一次改革写入 `BriefPonytail=Frequent/Female`、`Princess=Never/Any` 和
`["Hindu","Morbid"]`（priority 3/2），points 0、reform_count 1；API 与独立原生
fixture 读回一致。fixture 置 classic 后写 `["Hindu","Morbid"]`，无改革消耗；
4 槽普通/classic 均 400。证据
`tools/cua/.runtime/id-r05-run1-20261112/evidence/id-r05.json`，runner
`tests/parallel/ideology/id_r05_style_editor.py`。

边界：胡须/纹身与发型共用原生 `SetFrequency/SetGender`，未逐类重跑；理念创建
界面的风格库（C-04）不在本行；非主理念未测。
