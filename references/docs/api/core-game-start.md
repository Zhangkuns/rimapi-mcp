# Core 新开局：理念选择、新建、配置与人物分配（C-04）

2026-11-13 新增。开局仍是现有 `POST /api/v1/game/start`（保留原字段），仅增加
可选 `ideo` 对象；理念在 `Find.Scenario.PostIdeoChosen()` 之前按正常开局页的
原生路径创建并分配，因此原生起始人物生成会继承该理念。**没有使用 debug 改
ideo**，也没有新增写路由。

## 选择项（只读）

`GET /api/v1/game/start/ideo/options`

- `ideology_active`：Ideology DLC 是否激活。
- `modes`：`classic`、`preset`、`fixed`、`fluid`、`load`。
- `presets`：`def_name`、`label`、`category`、`memes`、`classic_plus`。
- `saved_presets`：原生 profile `Ideos` 目录的 `.rid` 文件；行含
  `file_name/file_bytes/readable`，可读行另含
  `game_version/mods_match/saved_mods/running_mods/warnings/requires_confirmation`。
  解析 header 抛异常时含 `read_error`。
- `structure_memes` / `normal_memes`：玩家派系允许的 MemeDef 名（结构/普通）。
- `structure_meme_rows` / `normal_meme_rows`：同名行，另含 `label`、
  `exclusion_tags`（客户端可据此挑选不冲突的普通 meme）。
- `style_categories`：可选 `StyleCategoryDef`。
- `classic_mode`、`player_primary`（有存档时）。

主菜单（无进行中游戏）也可读；预设仅解析原生 meta header，不加载/分配理念。
原生枚举会在缺失时创建空 `Ideos` 目录。

## 开局请求

```json
{
  "storyteller_name": "Cassandra",
  "difficulty_name": "Medium",
  "map_size": 250,
  "ideo": {
    "mode": "fixed",
    "name": "C04 Fixed",
    "structure_meme": "Structure_Animist",
    "memes": ["MaleSupremacy", "Supremacist"],
    "styles": ["Hindu"]
  }
}
```

- `mode=classic`：按 `Page_ChooseIdeoPreset.PostOpen`/`DoClassic` 的 native
  语义生成 classic ideo，全派系 primary 指向该 ideo，`IdeoManager.classicMode=true`。
- `mode=preset`：`preset_name` 精确匹配 `IdeoPresetDef`；按原生 `DoPreset` 的
  `IdeoGenerator.GenerateIdeo(forcedMemes=preset.memes, classicPlus)` 生成。
  预设未声明的结构 meme 由原生生成器按派系权重补选（响应 `structure_meme`
  给出实际值）。
- `mode=fixed`/`fluid`：必须显式 `structure_meme`（缺失 400），`memes` 为普通
  meme（fixed 1..3，fluid 恰 1），`name` 非空；`styles` 可选。生成走
  `IdeoGenerator.GenerateIdeo(forcedMemes=结构+普通)`，fluid 再置
  `ideo.Fluid=true`（会创建 `IdeoDevelopmentTracker`）。全部按原生
  `IdeoUtility`/Def 门禁校验：DLC、meme 类别/派系许可/exclusionTags、
  fluid impact、style 存在且非 fixedOnly、名称非空。
- `mode=load`：`preset_file` 必填，取自 `saved_presets.file_name`，也接受去掉
  `.rid` 的文件名。只允许原生目录中已经存在的文件名；含 `/` 或 `\` 的路径
  返回400，不接受任意宿主路径。不能同时指定 `preset_name/name/structure_meme/`
  `memes/styles` 创建字段。`preset` 选择内置 `IdeoPresetDef`，`load` 读取保存的
  自定义 `.rid`。例如 `"ideo":{"mode":"load","preset_file":"my-ideo.rid"}`。
  原生版本/Mod警告列在 `warnings`（`version_mismatch/mods_mismatch`）；有警告时
  须显式 `ideo.confirm_warnings:true`，否则409，尚未开始新局。同步校验只读
  header；世界/玩家派系与ID管理器就绪后才执行 `GameDataSaveLoader.TryLoadIdeo`
  → `IdeoGenerator.InitLoadedIdeo` → 原生玩家理念分配和 `PostIdeoChosen`。
  加载后的理念/教义使用目的游戏的新ID；流动理念按原生外部预设规则重置发展点数
  和改革次数，保存内容及隐藏仪式由原生 `ExposeData` 恢复。
- 省略 `ideo`：保持旧行为（不创建/分配理念）。`mode=classic` 是创建 classic，
  不是“取消/恢复”。
- 校验在长事件之前同步完成；失败返回 400（缺字段/配置不合法）、404（未知
  preset/meme/style/保存文件）、409（兼容警告未确认）、422（DLC 缺失或不可读
  header），且不会开始新局。完整文件加载失败仍可能在世界准备后报告422；
  header可读不保证正文可加载。成功响应 `data`：
  `mode`、`ideo_id`、`name`、`fluid`、`classic_mode`、`initial_player_ideo`、
  `structure_meme`、`memes`（实际普通 meme）、`styles`、`precept_count`、
  `points`（fluid 为开发点数，fixed/classic 为 null）、
  `colonist_believer_count`（此时 0；人物生成后读回）；`load` 另含 `file_name`
  和选中文件的 `saved_preset` 元数据。
- 成功后才排队 `PageUtility.InitGameStart` 生成地图；起始人物由原生
  `PostIdeoChosen` 生成并继承 primary ideo。

## 证据与边界

2026-11-13 四个独立隔离新开局（无存档启动，主菜单 → `/game/start`；fixture
`CoreStartFixture` 只读理念/殖民者并暂停，不代验理念；无 debug ideo 修改）：

| 模式 | 结果 | 关键读回 |
| --- | --- | --- |
| fixed | passed | `C04 Fixed`，Structure_Animist，普通 memes MaleSupremacy+Supremacist，styles Hindu，precept 45，3 个殖民者同 ideo |
| classic | passed | `Astropolitan`，classic_mode true，3 个殖民者同 ideo |
| preset | passed | preset 的 Individualist + 原生补结构 Structure_Archist，3 个殖民者同 ideo |
| fluid | passed | `C04 Fluid`，fluid true，points 0，Structure_Animist + MaleSupremacy，3 个殖民者同 ideo |

- run：fixed `c04-fixed-20261113-run4/evidence/c04-fixed2.json`（含非法配置
  400：缺 structure_meme）；classic 同 run `c04-classic.json`；preset/fluid
  `c04-preset-20261113-run5/evidence/c04-preset.json`、`c04-fluid.json`。
- 每个模式都用 `GET /dlc/ideology?pawn_id=<首个殖民者>`、
  `GET /dlc/ideology/development?pawn_id=...`（name/fluid/points/
  colonist_believer_count/precept_count）复核，fixture 再核 primary ideo 与
  全部起始殖民者 ideo id。
- 上述四模式旧证据不包含 `.rid` 载入；新载入验证另见
  [保存预设载入记录](../references/reviews/2026-11-13-ideology-saved-preset-load.md)。
- 未测：precept 增删 UI、deity/symbol 编辑、
  多样式槽位顺序、classic 模式下各派系 ideo 差异的逐派系读回、地图生成失败
  恢复。固定/流动普通 meme 数量按原生 `MemeCountRangeAbsolute`/Fluid 范围
  校验；preset 未声明结构时结构由原生生成器补选并披露。

## C-01 场景选择/编辑/保存载入（2026-11-13）

新增只读 `GET /api/v1/game/start/options`（新共享路由，主菜单可用）：

- `scenarios`：`ScenarioDef` 与本地自定义场景（`preset_file`），含
  `name/summary/description/category/parts/addable_parts`；part 行含
  `def_name/label/category/player_add_removable/max_uses`。
- `scenario_presets`：本地 `.rsc` 的 `preset_file/name/category/part_count`。
- `world`：正常世界参数合法值（coverage、rainfall/temperature/population、
  landmark、pollution 范围、configurable factions、map sizes、seasons、冬季/大图
  警告键）。
- `xenotypes` / `custom_xenotypes`：预置 `XenotypeDef` 与已保存 `.xtp`（C-03 使用）。

`POST /game/start` 新增可选 `scenario` 对象（旧字段保持）：

```json
{"scenario": {
  "def_name": "LostTribe",
  "preset_file": null,
  "name": "C01 Lost Tribe Edited",
  "summary": "...",
  "description": "...",
  "parts": [
    {"def_name": "StartingThing_Defined", "enabled": false},
    {"def_name": "StartingThing_Defined", "enabled": true}
  ],
  "save_as": "c01-lost-tribe-edited"
}}
```

- `def_name`/`preset_file` 二选一；预设走 `GameDataSaveLoader.TryLoadScenario`。
- 编辑时 `Scenario.CopyForEditing()`；part 通过 `ScenarioMaker.MakeScenPart` +
  `Randomize()` 增加，或从可玩 parts 删除；校验 `PlayerAddRemovable`、`maxUses`、
  `CanCoexistWith` 与 `Scenario.ConfigErrors()`。固定 part 不能通过 API 增删。
- `save_as` 走 `GameDataSaveLoader.SaveScenario` 写 `.rsc` 并 `ScenarioLister.MarkDirty`；
  不修改原 Def 场景实例。响应额外给出 `scenario`、`initial_conditions`、
  `scenario_saved_file`（仅 save_as）。
- 任何场景校验失败都会在创建/修改游戏前返回 4xx；不会留下进行中游戏。

## C-01 证据与边界

2026-11-13 独立隔离新开局（无存档启动，主菜单 → options → start，fixture 只读
`Find.Scenario`/world/init 条件并暂停）：

- `LostTribe`：初始 parts 18；经 API 移除 1 个可玩 part、增加 1 个随机 part 后，
  响应与 fixture 都读回同名 `C01 Lost Tribe Edited`/`CustomLocal`/18 parts；
  起始 5 人（响应 `starting_pawn_count:5`，fixture 5 个 selected pawn，
  `startingPawns` 可选候选 8）；map 250、season Spring、seed
  `c01-lost-tribe`、tile 读回一致。
- 非法编辑（固定 `PlayerFaction` part）400，`game/state.map_count:0`，未开始新局。
- `save_as` 后返回主菜单，`GET /game/start/options` 的 `scenario_presets` 列出
  `c01-lost-tribe-edited`；用 `preset_file` 二次开局成功，名称/parts 读回一致。
- 证据 `tools/cua/.runtime/core-start-20261113g/evidence/c01-scenario.json`，
  DLL sha256 `a0bfb98ff9aef9ef5921028225b24f675b9ab5950cbf952034720695aa01bcf7`
  （来源 `6ba0e2b0`），fixture `af0b5c1e5dc13a61b42ed6b5da49ec474f06ea4e5d534e577e4f38c8e90f0d4c`。
- 未测：逐 part 的 config 编辑页字段（如 `ScenPart_ConfigPage_*` 的数值）、
  Steam Workshop 场景、上传、非 LostTribe 场景的特定 part 组合、场景编辑的
  客户端交互式取消（API 以“校验失败不落游戏 + preset 载入”表达）。

## C-02 世界预览、选址与警告确认（2026-11-13）

新增 `POST /api/v1/game/start/world/preview`（新共享路由）：

```json
{"scenario": {"def_name": "Crashlanded"},
 "world": {"seed": "c02-preview", "planet_coverage": 0.3,
           "overall_rainfall": 3, "overall_temperature": 3,
           "overall_population": 3, "landmark_density": 3,
           "pollution": 0.05,
           "factions": [{"def_name": "OutlanderCivil", "count": 1}]},
 "tile_query": {"max_results": 32, "biome": "Desert",
                "hilliness": "Flat", "landmark": "None", "tile": "0"}}
```

- 世界参数走正常 `Page_CreateWorldParams` 的 `WorldGenerator.GenerateWorld`（含
  configurable faction 列表与 pollution），`Find.Scenario.PostWorldGenerate` 仍由
  生成器调用；不生成地图。响应 `world`（seed/coverage/enums/pollution/
  faction_count/configured_faction_count/factions/tile_count）、`tiles`（`tile`/
  `biome`/`biomes`/`hilliness`/`landmark`/`valid`/`reason`）、`scanned_tiles`/
  `valid_tiles`；`tile_query.tile` 可精确查询单个地格。
- 预览结果保存在同一 API 会话的 preview cache；`POST /game/start` 不带 `world`
  对象时使用该 preview 的 seed/params/factions/pollution，重新生成同一
  （seed 确定）世界并应用 `site` 的 tile/map_size/season；带 `world` 对象则按新参数
  重新生成。返回 `prepared_world:true`；主菜单/重开新局会清空 cache。
- `prepare_only:true` 同样复用预览世界和未被覆盖的场景，保留选定地点；人物准备后
  由 prepared Game 承接世界、人物和后续开局，不要求客户端重复提交 `world`。
- `POST /game/start` 新增可选 `site`：

```json
{"site": {"tile": "71029", "map_size": 300, "season": "Winter"},
 "confirm_warnings": true}
```

  - `tile` 支持数字或 `"auto"`；数字走 `TileFinder.IsValidTileForNewSettlement`，
    越界/非法返回 400；地图大小只接受 `Dialog_AdvancedGameConfig` 的
    `[200,225,250,275,300,325]`；季节接受 `Season` 名。
  - `NewGameSiteWarnings`：冬季 → `MapWinterWarning`，`map_size>280` →
    `MapSizePerformanceWarning`。存在警告且 `confirm_warnings` 不为 true 时返回
    409，`data.requires_confirmation:true` + `data.warnings`，不生成地图、不开始；
    `confirm_warnings:true` 才继续（正常页面警告的显式 API 等价）。
  - 成功响应新增 `site`/`warnings`，并保留 `world`/`scenario`/
    `initial_conditions`；`initial_conditions` 在 `PostIdeoChosen` 后、地图生成前快照。

## C-02 证据与边界

2026-11-13 独立隔离主菜单开局（无存档）：

- `c02-preview`（coverage 0.3、pollution 0.05、OutlanderCivil+TribeCivil 各 1）：
  preview 200、map0、tile_count 119904、tiles 全部 valid；实际 faction 3
  （2 configurable + PlayerColony），`configured_faction_count:2`。
- start 带 preview 的 tile、map_size 300、Winter、`confirm_warnings:false`：
  409/`requires_confirmation:true`/两条警告，map0；随后
  `confirm_warnings:true` 200，same seed/world name/3 factions，site tile 0/Desert/
  Winter/300；fixture 读回 map 300、tile 0、biome Desert、Winter。
- 非法 tile `-1` 返回 400 且 map0；preview 后仍可继续用合法 tile 启动。
- 证据 `tools/cua/.runtime/core-start-20261113j/evidence/c02-world.json`，
  DLL sha256 `8cafb598d8f19b1a7c305ce576204b4b9723d276740ccfa40fa4c0651e045ce6`
  （来源 `1833b660`），fixture
  `3a7d09e469cbe260a925a014156f977b45ae4b7bd1f81a44fdda409fd41923de`。
- 未测：settlement proximity/biome settle-warning 文案、Odyssey orbit 起始层、
  所有 tile query 组合、preview 的客户端取消 UI、污染/派系的更多组合。

## C-03 起始人物名单、重掷/保留与异种（2026-11-13）

`POST /game/start` 新增 `prepare_only:true`：按正常 `PostIdeoChosen` 生成
`Find.GameInitData.startingAndOptionalPawns`，**不生成地图**，响应返回实际名单：

```json
{"prepare_only": true,
 "world": {...}, "site": {"tile": "auto", "map_size": 250, "season": "Spring"},
 "starting_pawns": {
   "keep_indices": [2, 0, 1],
   "reroll_indices": [0],
   "xenotypes": [
     {"index": 0, "def_name": "Dirtmole"},
     {"index": 1, "custom": {"name": "C03 Custom",
                             "genes": ["Eyes_Gray", "DarkVision"],
                             "inheritable": false}}
   ]}}
```

- 名单行：`slot/selected/pawn_id/name/def_name/kind_def/xenotype_def/
  xenotype_label/unique_xenotype/xenogenes/endogenes/relations`；
  `starting_pawn_count` 为实际入队人数，`starting_pawns_count` 为含可选候选总数。
- `reroll_indices` 只调用 `StartingPawnUtility.RandomizePawn`；`keep_indices` 先重排
  实际列表并同步 `StartingPawnUtility.ReorderRequests`，至少保留 1 人；索引越界/
  重复返回 400。
- `xenotypes`：预置 `XenotypeDef` 走 `GetGenerationRequest` →
  `ForcedXenotype` → `SetGenerationRequest` → `RandomizePawn`；自定义走
  `Dialog_CreateXenotype` 的门禁（基因存在、唯一、prerequisite、两两
  `ConflictsWith`、`GeneTuning.BiostatRange.TrueMin`、最多 200 个），保存正常
  `.xtp` 到 `GenFilePaths.AbsFilePathForXenotype`，并 `ForcedCustomXenotype`。
  仅 Biotech 可用；自定义异种保存后出现在 `GET /game/start/options` 的
  `custom_xenotypes`。C-01/C-02 的 `prepare_only` 先在会话缓存保留该 Game；
  后续 `POST /game/start`（不带 world/scenario）重新挂载同一 prepared Game，
  再应用最后的 `starting_pawns`/site 并走原生 `PageUtility.InitGameStart`。

## C-03 证据与边界

2026-11-13 独立隔离主菜单开局（无存档）：

- Crashlanded prepare：`starting_pawn_count:3`、`starting_pawns_count:8`、
  map0；名单实际读回 8 个候选。
- `reroll_indices:[0,1]`：前两位 `pawn_id` 从 `Human195/198` 变为
  `Human219/223`，第三位 `Human201` 保持不变；仍为 3 selected/8 total。
- `xenotypes:[{0:Dirtmole},{1:custom C03 Custom}]`：slot0 `xenotype_def:Dirtmole`；
  slot1 `unique_xenotype:true`、标签 `C03 Custom`、10 个自定义基因读回一致；
  `.xtp` 文件与 options `custom_xenotypes` 均出现。
- `keep_indices:[2,0,1]`：selected 顺序变为 `Human201,Human226,Human229`；
  最终 start 200，map250/Spring，response 与 fixture 的 selected 顺序、Dirtmole
  与自定义基因全部一致，fixture 3 colonists。
- 证据 `tools/cua/.runtime/core-start-20261113m/evidence/c03-pawns.json`，
  DLL sha256 `87526c641eba88ce0c90a743cc3ffd57525628b79b91ee11ed1672456164990c`
  （来源 `52834674`），fixture
  `3a7d09e469cbe260a925a014156f977b45ae4b7bd1f81a44fdda409fd41923de`。
- 未测：逐 pawn 外观/背景故事编辑、异种图标选择、自定义基因的全部合法组合、
  不可遗传+archite 等原生警告确认、UI 取消路径。

后续派系/殖民地/普通人物命名见 [core-naming.md](core-naming.md)（C-07）。

### C-39 `started_from_entry`

`POST /api/v1/game/start` 新增可选 `started_from_entry`（默认 `false`）。为 true 时在
新开局/`prepare_only` 正式启动的 native `GameInitData` 上设置
`startedFromEntry=true`，由原生 `Game.InitNewGame` → `Scenario.PostGameStart`
打开真实 `ScenPart_GameStartDialog`；默认 false 保持现有行为，不手设 Dialog。
