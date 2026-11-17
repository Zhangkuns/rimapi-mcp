# Core 运行中叙事者、难度、Anomaly 与玩法选项（C-05）

2026-11-16 修复：完整原生偏好由[game/preferences](game-preferences.md)读写，含准确存档天数/数量与自动暂停；旧game/settings保留bool兼容并新增准确数值/volume_ui。音画、语言/键位及原生显示底栏UI-E01–03已补并验证，见[验收](../references/reviews/2026-11-16-ui-api-repair.md)。

2026-11-13 新增。只覆盖影响玩法/存档的选项；音频、视频、UI 缩放、摄像机等
纯客户端选项不在此端点。

## 读取（只读）

`GET /api/v1/game/options`

- `game_active`。
- `storyteller`：当前 `def_name`/`label`、`difficulty_def`/`label`、
  `difficulty_fields`（`Difficulty` 的全部 float/bool/float? 公开字段，
  snake_case）与 `anomaly`（active、playstyle、threats_inactive/active、
  override_threat、study_efficiency、playstyles[] 及各自
  display/override/enable 标志）。
- 合法值：`storytellers[]`（def_name/label/description）、
  `difficulties[]`（def_name/label/is_custom/is_extreme/native_difficulty）。
- `max_number_of_player_settlements`（当前值/1..5）。
- `preferred_names`（当前列表/上限 6）。

无游戏时仍返回 Def 合法值，`game_active=false`、当前 storyteller 为 null。

## 应用

`POST /api/v1/game/options`（全部字段可选，但至少一项；`dry_run` 保护）

```json
{"storyteller":"Randy","difficulty":"Medium",
 "custom_difficulty":{"threat_scale":1.35,"research_speed_factor":1.8,
                       "allow_big_threats":false},
 "anomaly":{"playstyle":"Standard","threats_inactive":0.2,
            "threats_active":0.4,"study_efficiency":1.25},
 "max_number_of_player_settlements":3,
 "preferred_names":["Alice","Bob"],"dry_run":true}
```

- `storyteller`：原生 `Current.Game.storyteller.def = def` +
  `Notify_DefChanged()`。
- `difficulty`：原生 `difficultyDef = def` + `Difficulty.CopyFrom(def)`；
  随后重算 `costListForDifficulty` 与
  `RecipeDefGenerator.ResetRecipeIngredientsForDifficulty()`。
- `custom_difficulty`：只接受 `Difficulty` 现有 float/bool/float? 公开字段
  （snake_case 精确匹配，未知字段/类型错 400）；自动切到 `isCustom` 的
  `DifficultyDef`；先复制所选难度再套用字段。
- `anomaly`：要求 Anomaly DLC；playstyle 必须是当前 scenario 允许的
  `AnomalyPlaystyleDef`；inactive/active 0..1、study 0..5、override 0..1；
  语义与 `Dialog_AnomalySettings` 一致：playstyle 不 override 时清空
  override，override playstyle 缺省值为 0.15。
- `max_number_of_player_settlements`：1..5，写 `Prefs`。
- `preferred_names`：≤6、非空、去重，写 `Prefs.PreferredNames`。
- 所有校验在任何写之前完成；失败 400/404/422；`dry_run` 不写字段、不
  recache、不 `Prefs.Save()`。成功仅 Prefs 变更时 `Prefs.Save()`，返回
  `before`/`after` 实际读回。

## 证据与边界

2026-11-13 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture `CoreOptionsFixture` 只读 `Current.Game.storyteller`/`Difficulty`/
`Prefs`，不写选项。

- 初始 `Phoebe`/`Easy`；dry 请求无任何变化。
- 实际应用 `Randy`/`Medium`，Anomaly Standard + inactive 0.2/active 0.4/
  study 1.25；fixture 与 `GET /game/options` 一致；`Prefs` 上限 3、
  names Alice/Bob；`GET /game/settings` 读回上限 3。
- `custom_difficulty`：`difficulty_def=Custom`、threat_scale 1.35、
  research_speed_factor 1.8、allow_big_threats false。
- 原生 `game/save` + `game/load` 后再次读回：Randy/Custom、上限 3、
  names Alice/Bob、threat_scale 1.35 均保留。
- 4xx：未知 storyteller 404、未知难度 404、上限 6 400、anomaly 1.5 400、
  未知 custom 字段 400、bool 字段类型错 400；失败后当前值未被部分修改。
- 收口 `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。
- 未测：scenario 限制非 Standard playstyle 分支、override playstyle 的
  0..1 override 实际应用、跨进程 Prefs 读回（本局只验证 `Prefs.Save()` 后
  的 API/原生读回与游戏存读档）；音频/视频/UI/摄像机明确排除。
