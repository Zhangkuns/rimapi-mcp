# Ideology 单物品风格（G-32）

2026-11-16 修复：classic玩家蓝图按entityDefToBuild发现风格，支持style_def或basic:true，写StyleDef/styleOverridden并刷新该网格；旧confirm保留，changed事件/重放正确。真实DiningChair建成风格通过；账单未来产品选择另见[UI-03/10合同](ui-option-repairs.md)及[验收](../references/reviews/2026-11-16-ui-api-repair.md)。

已实现并验收：20 项断言（2026-11-05 复审后重跑）；证据 `Reverse/evidence/dlc-G-32.json`、
原生实测 `Reverse/evidence/dlc-G-32-runtime.json`。

## 读取

`GET /api/v1/dlc/ideology/style?thing_id=3687`

```json
{"thing_id":3687,"def_name":"Apparel_AdvancedHelmet","can_be_styled":true,
 "style_def":null,"style_source_precept":null,
 "available_styles":["Hindu_AdvancedHelmet"]}
```

`available_styles` 来自原生 `StyleCategoryDef.thingDefStyles` 中与该 `ThingDef` 配对的风格；
不可美化的物品返回 `can_be_styled:false` 与空列表。

## 应用

`POST /api/v1/dlc/ideology/style`

```json
{"thing_id":3687,"style_def":"Hindu_AdvancedHelmet","confirm":true,"dry_run":false}
```

成功数据：
`{thing_id,def_name,previous_style_def,style_def,style_source_precept_before,style_source_precept_after,available_styles,changed,applied,dry_run}`。

- 只对**单个物品**生效，不批量；`style_source_precept_*` 是该物品的原生风格来源 precept 读回。
- 不主动改 precept：实测原生 `SetStyleDef` 会**保留**已有来源 precept
  （手工链接 `SetStyleSourcePrecept` 后设为另一风格，precept 仍为 `IdeoBuilding`）。
- `style_def` 不在该 def 的可应用列表 → 422 `invalid_target`；未知 style → 404。
- 同风格 `changed:false`，不调 setter、不发事件；`dry_run` 只投影。
- 真实调用走公开 `ThingStyleHelper.SetStyleDef`，不使用裸 `Thing.StyleDef` setter。
- 非预演必须 `confirm:true`；`dry_run` 只回读当前 precept，不预测替换结果。
- 原生调用抛异常时返回 500 `uncertain:true` 并读回
  `observed_style_def`、`observed_style_source_precept`、`readback_ok`。

## 批量

原生没有独立的“批量风格”入口；风格按 `Thing` 的 `ThingStyleHelper.SetStyleDef` 应用。
对多个物品由调用方重复 `POST /api/v1/dlc/ideology/style`，API 不另设 batch 合同；本组
未单独实测多物品循环，但单物品合同（G-32）已验收。

## 未覆盖

- 视觉刷新在无头环境下未截图验证；只验证 `Thing.StyleDef` 读回与存读档。
- 未观察到 precept 非空的**自然**存档物品：precept 联动是用临时探针手工链接后验证的，
  不是由游戏正常流程生成的 fixture。
- 只读取 `ThingDef`/`StyleCategoryDef` 配对的风格；多物品循环未单独实测、风格对心情的
  影响未测。
- 1.5 与 Mod 风格未测。
