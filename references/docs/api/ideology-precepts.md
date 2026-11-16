# Ideology 运行中教义实例发现（ID-R01）

2026-11-12 新增：`GET /api/v1/dlc/ideology/precepts?ideo_id=` 返回具体
`Precept` 实例的稳定 `precept_id`、def/class/issue、可编辑所需类型信息和仪式
角色；`ideo_id` 省略时使用玩家主理念。

## 读回字段

```json
{"ideo_id":13,"ideo_name":"Boccin Creed","precept_count":45,
 "precepts":[{"precept_id":696,"def_name":"Funeral","label":"...",
  "class":"Precept_Ritual","issue":"Ritual","is_ritual":true,
  "ritual_pattern":"FuneralStandard","role_count":1,
  "roles":[{"role_id":"moralist","required":false}]},
  {"precept_id":700,"def_name":"IdeoRitualSeat","class":"Precept_RitualSeat",
   "issue":"IdeoRitualSeat","is_thing":true,"thing_def":"Pew"}]}
```

- `precept_id` 是原生 `Precept.Id`，可直接用于既有 reform/add/remove；不是
  PreceptDef 名称。
- `is_thing` + `thing_def` 暴露 ThingDef/座位等具体实例目标；
  `is_ritual` + role 列表暴露仪式角色。
- 特殊实例读回（ID-R02）：`Precept_Apparel` 行含 `apparel_def`/`target_gender`，
  `Precept_Weapon` 行含 `noble_weapon_class`/`despised_weapon_class`，
  `Precept_Xenotype` 行含 `xenotype`/`custom_xenotype`。
- `include_add_options=true`（也接受 `1`）时追加 `add_options`：
  `apparel[{precept_def,apparel_def,label}]`、`weapons[{precept_def,pair_def,
  noble_weapon_class,despised_weapon_class,noble_label,despised_label}]`、
  `xenotypes[{precept_def,xenotype,label}]`、
  `ritual_patterns[{precept_def,pattern_def,label,source,is_base,can_add,reason}]`，
  只列当前 ideo 原生可加值。
- `include_style=true`（也接受 `1`）时追加 `style`：全部发型/胡须/纹身
  frequency/gender、建筑风格槽与 classic 选择，供 ID-R05 编辑使用。
- 合同：GET observe/none/none；`include_add_options` 只读、不写游戏状态。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
玩家主理念 Boccin Creed `id13`，45 个 precept，ID 唯一；Funeral 实例
`precept_id696`/pattern FuneralStandard/role moralist；RitualSeat 实例
`precept_id700`/thing_def Pew；显式 `ideo_id=13` 返回同一 ID 集。
证据 `tools/cua/.runtime/id-r01-20261112/evidence/id-r01.json`，runner
`tests/id_r01_precepts.py`。

- 本行只补发现读回；具体 add/remove/replace 的特殊选值、解锁条件与正常
  reform 结算仍归 ID-R02/R03/R04/R15；未穷举所有 Precept 子类字段。
- ID-R02 已补三类特殊选值与实例读回；2026-11-12 隔离 `g35_deathresting` 副本实测
  `include_add_options` 返回 44 个服饰、5 个武器 pair、12 个异种，见
  [ideology.md](ideology.md) 的 ID-R02 段。
