# Ideology 已有特殊教义编辑（ID-R03）

2026-11-12 新增两个原生编辑接口（合同由协调者登记）：

```text
GET  /api/v1/dlc/ideology/precept/edit-options?precept_id=733&ideo_id=13
POST /api/v1/dlc/ideology/precept/edit
```

`precept_id` 是原生 `Precept.Id`；`ideo_id` 省略时用玩家主理念。POST 非 dry
必须 `confirm:true`，`Idempotency-Key` 按 `game_memory`。编辑是原版
`Dialog_EditPrecept.ApplyChanges` 的就地修改，不消耗发展点、不改 `reform_count`。

## POST 请求

```json
{"precept_id":732,"leader_title_male":"War Chief",
 "leader_title_female":"War Mother","confirm":true}
{"precept_id":733,"name":"Test Cleric","name_locked":true,"confirm":true}
{"precept_id":733,"remove_apparel_requirement_index":0,"confirm":true}
{"precept_id":733,"add_apparel_requirement_index":3,"confirm":true}
{"precept_id":736,"building_style":"Hindu_IdeogramA",
 "building_style_category":"Hindu","confirm":true}
{"precept_id":738,"relic_stuff":"Silver","confirm":true}
{"precept_id":700,"ritual_seat_def":"KneelSheet","confirm":true}
```

- `name`：非领导角色 precept 的名称；1–32 字符，字符集与原生
  `Dialog_EditPrecept.ValidSymbolRegex`（字母、数字、空格、`'`、`-`）一致。
- `name_locked`：原生名称锁；领导角色 precept 不显示该按钮，传了会 422。
- `leader_title_male`/`leader_title_female`：仅 `def.leaderRole`；男性头衔同时
  `SetName` 为该 precept 名称，与原生一致。
- `add_apparel_requirement_index`/`remove_apparel_requirement_index`：仅
  `Precept_Role`；索引分别指向 `addable_apparel_requirements` 与当前
  `apparel_requirements`。新增走
  `Precept_Role.AllPossibleRequirements(desperate:true)` +
  `CanAddRequirement`，并检查与现有要求不可同穿；移除后同样检查剩余要求重叠。
- `relic_stuff`：仅 `Precept_Relic` 且 `ThingDef.MadeFromStuff`；必须在该圣物
  `GenStuff.AllowedStuffsFor(ThingDef)` 集合内。
- `building_style`/`building_style_category`：仅 `Precept_Building` 且原生样式
  列表多于 1；`style_def` 为 `ThingStyleDef.defName`，同名样式跨类别时需带
  `building_style_category`。应用调用
  `ideo.style.SetStyleForThingDef(building.ThingDef, pair)`。
- `ritual_seat_def`：仅 `Precept_RitualSeat`；必须在该 def 的
  `Worker.ThingDefs` 内，应用即原生 `ThingDef` setter（会重设该座位样式）。
- 一次请求可带多个字段；`add`/`remove` 服饰要求不能同时提交。
- 返回 `before`/`requested`/`changed`/`dry_run`/`applied`，正式成功再返回
  `after`；dry-run 返回 `after_preview`。原生异常时 `uncertain:true` +
  `native_error`，客户端应重新读取。

## GET 选项

返回当前 `label`/`name_locked`/`leader_role`、`editable` 布尔表，以及按类适用的
`apparel_requirements`、`addable_apparel_requirements`（含 `can_add`/`reason`）、
`allowed_relic_stuffs`、`building_styles`（`style_def`/`category`）、
`ritual_seat_defs`。选项只列原生当前合法值；不打开任何窗口。

## 原生权限矩阵

| 类 | 名称/锁 | 领导头衔 | 服饰要求 | 圣物材料 | 建筑样式 | 座位种类 |
| --- | --- | --- | --- | --- | --- | --- |
| `Precept_Role` | 是（领导角色仅头衔） | 仅 `leaderRole` | 是 | 否 | 否 | 否 |
| `Precept_Building` | 是 | 否 | 否 | 否 | 是（>1 样式） | 否 |
| `Precept_Relic` | 是 | 否 | 否 | 仅 `MadeFromStuff` | 否 | 否 |
| `Precept_Ritual` | 是 | 否 | 否 | 否 | 否 | 否 |
| `Precept_RitualSeat` | 否 | 否 | 否 | 否 | 否 | 是 |

仪式名称/`is_anytime`/附加结果/日期仍走既有 `POST /dlc/ideology/ritual/config`；
本接口补的是 `Dialog_EditPrecept` 的其余字段与座位替换菜单。普通 plain
precept 在原生 UI 没有编辑对话框，故拒绝名称/头衔编辑。

## 实测

隔离 `g35_deathresting` 副本（tick 76787、五 DLC、Boccin Creed id13、45 precept）：

- 领导角色 732：`War Chief`/`War Mother`，precept label 与
  `ideo.leaderTitleMale/Female` 同步；
- Moralist 733：改名 `Test Cleric`、`name_locked=true`；移除 Burka、添加 Cape；
- 建筑 736：`Morbid_IdeogramB` → `Hindu_IdeogramA`（类别 Hindu）；
- 圣物 738（`RelicInertCube`）：材料 Gold → Silver；
- 仪式座位 700：Pew → KneelSheet；
- 每次 POST 的 `after`、`GET edit-options`、独立原生 fixture 三方一致；
  points 0、`reform_count` 0 全程不变；
- 拒绝：领导角色 `name_locked` 422、非法名称字符 400、非法圣物材料 422。

结果：`tools/cua/.runtime/id-r03-run1-20261112/evidence/id-r03.json`（phase passed）；
runner `tests/parallel/ideology/id_r03_precept_edit.py`；fixture
`tests/parallel/ideology/IdR03Fixture.cs`。
