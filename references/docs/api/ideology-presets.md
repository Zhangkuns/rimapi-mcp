# Ideology 理念预设保存、发现与正常载入（ID-R10 / C-04 / ID-R11）

2026-11-12 新增两个接口，复用原版预设文件读写，不打开窗口、不进入 `SavingLongEvent`：

```text
GET  /api/v1/dlc/ideology/presets
POST /api/v1/dlc/ideology/presets/save
```

原生玩家路径：`IdeoUIUtility.DoIdeoSaveLoad`（Save 按钮）→
`Dialog_IdeoList_Save.DoFileInteraction` → `GenFile.SanitizedFileName` +
`GenFilePaths.AbsPathForIdeo` → `GameDataSaveLoader.SaveIdeo`。本接口直接复用
`SaveIdeo` 主线程同步写入，只跳过文件对话框、长事件界面和提示消息。
载入遵循原生 `Dialog_IdeoList_Load` 的正常上下文：通过现有新开局与 Archonexus
重开接口选择 `.rid`。运行中的普通殖民地没有任意替换理念的写入口。

## 正常载入

在主菜单或 Archonexus 选择理念时查询 `GET /api/v1/game/start/ideo/options`，
从 `saved_presets` 获取实际文件名及原生兼容性警告。该查询仅解析 meta header；
不需要活动游戏，也不消耗理念/教义ID。原 `GET dlc/ideology/presets` 仍遵守下面
的活动游戏门禁，保留旧保存/导出合同。

新开局：`POST /api/v1/game/start` 中传
`"ideo":{"mode":"load","preset_file":"my-preset.rid"}`。
Archonexus：`POST /api/v1/dlc/ideology/archonexus/ideology` 中传同一 `ideo`，
另含 `quest_id` 与正式迁移的 `confirm:true`；`dry_run:true` 只检查 header和门禁，
不加载/分配理念、不启动迁移。`primary_ideo_id` 选择已在 IdeoManager 的理念，
不能代替保存文件载入。

`preset_file` 仅接受原生 `Ideos` 目录现有 `.rid` 的文件名或去扩展名的文件名，
路径400、未知文件404、不可读header422。若 `saved_presets` 行的
`requires_confirmation:true`，审核 `warnings/saved_mods/running_mods/game_version`
后显式传 `ideo.confirm_warnings:true`，否则409。不能混入创建字段。
原生全文件加载仍可能失败；header可读不保证正文可加载。

确认后复用 `GameDataSaveLoader.TryLoadIdeo` + `IdeoGenerator.InitLoadedIdeo`，
为目的游戏分配新理念/教义ID，设置style所属理念，并沿原生新开局或
`MakeIdeoPrimary`/Next分配人物。流动预设的发展点数/改革次数按原生规则重置。
接口详情见 [新开局](core-game-start.md) 与 [Archonexus](archonexus.md)。

## GET 预设列表

`GET /api/v1/dlc/ideology/presets`

```json
{"preset_folder":".../Ideos","preset_count":1,
 "presets":[{"preset_name":"my-preset","file_name":"my-preset.rid",
   "file_path":".../Ideos/my-preset.rid","file_bytes":48368,
   "last_write_time":"2026-11-13T07:21:12.6496624Z",
   "game_version":"1.6.4871","readable":true}]}
```

- 行来自原生 `GenFilePaths.AllCustomIdeoFiles`（`Ideos` 目录下 `.rid`，按
  `LastWriteTime` 倒序），和 `Dialog_IdeoList` 看到的是同一批文件。
- `preset_name` 是去扩展名的文件名；`file_bytes`/`last_write_time` 是宿主文件观察值。
- `game_version`/`readable` 来自原生 `SaveFileInfo.LoadData()` meta header；
  `GameVersion == "???"` 判为不可读（`readable:false`），解析异常时
  `game_version:null` 并附 `read_error`。
- 原生枚举会在 `Ideos` 不存在时创建空目录，因此 GET 可能创建该目录。
- 无活动游戏返回 409 `conflict`；Ideology 未启用返回 422 `dlc_missing`。

## POST 保存

`POST /api/v1/dlc/ideology/presets/save`

```json
{"file_name":"my-preset","ideo_id":"13","overwrite":false,"dry_run":true}
```

- `file_name`：必填，经原生 `GenFile.SanitizedFileName` 清洗；清洗后为空返回
  400 `bad_request`。写入 `GenFilePaths.AbsPathForIdeo(file_name)`，即
  `<file_name>.rid`。原生清洗按宿主无效字符集（Linux 仅 `/`），其它字符行为
  与游戏内对话框一致。
- `ideo_id`：可选字符串整数；必须是玩家派系理念
  （`Faction.OfPlayer.ideos.AllIdeos`），否则 404 `not_found`；非整数 400。
  省略时用玩家主理念（`PrimaryIdeo`）。
- `overwrite`：默认 `false`。目标已存在且未显式 overwrite 时返回 409
  `conflict`，`data` 带 `applied:false`、`existing:true`、`before`，磁盘文件不动。
- `dry_run`：只返回预览（`applied:false`、`would_write:true`、`existing`、
  `file_name`、`file_path`、`ideo_id`、`before`），不调用 `SaveIdeo`、不写文件。

正式保存返回：

```json
{"applied":true,"dry_run":false,"would_write":true,"existing":false,
 "file_name":"my-preset","file_path":".../Ideos/my-preset.rid",
 "ideo_id":13,"before":{...},"after":{...},
 "file_sha256":"df20...159c","preset_readable":true,
 "preset":{"preset_name":"my-preset","file_bytes":48368,
   "game_version":"1.6.4871","readable":true}}
```

- `before`/`after` 是 `ideo_id`/`name`/`file_name`/`meme_defs`/
  `structure_meme`/`precept_count`/`points`/`reform_count` 快照。
- 原生 `SaveIdeo` 会把 `ideo.fileName` 设为预设名（`<name>.rid` 去扩展名），
  与游戏内保存一致；其它理念内容不变。
- `preset` 行是写入后重新解析 `SaveFileInfo` meta header 的结果；
  `file_sha256` 是宿主端 SHA-256；`file_bytes` 只在 `preset` 内（顶层不重复）。
- 原生 `SaveIdeo` 自己吞异常并写日志，因此服务在调用后检查文件存在与
  meta header：写入抛异常返回 500 `internal_error` + `uncertain:true` +
  `native_error`；无文件返回 500 + `uncertain:true`；meta header 不可读返回
  500 并附 `preset` 行。客户端应重新 GET 后再决定是否重试。
- 非 dry 请求走 AI mutation coordinator：当前共享
  `AiEndpointContracts` 尚未登记本路径，携带 `Idempotency-Key` 会返回 500
  “no game-memory idempotency contract”；登记前 AI 客户端按无 key 使用。
  dry-run 不发送 AI operation 事件。

## 错误与 DLC 门

| 情况 | 码 |
| --- | --- |
| 无活动游戏 | 409 `conflict` |
| Ideology 未启用 | 422 `dlc_missing` |
| 缺 `file_name` / 清洗后为空 / `ideo_id` 非整数 | 400 `bad_request` |
| `ideo_id` 不是玩家派系理念 | 404 `not_found` |
| 目标已存在且未 overwrite | 409 `conflict`（附 before，文件不动） |
| 原生保存异常 / 无文件 / header 不可读 | 500 `internal_error` + `uncertain:true` |

## 证据与未覆盖

ID-R10 实测（隔离 `g35_deathresting` 副本 tick 76787）：新建预设 48368 字节、
响应 hash 与磁盘一致、原生列表可读（1.6.4871）、原生 `TryLoadIdeo` 解析出的
Boccin Creed 内容与实时一致（45 precept、points 0、memes
`Structure_Archist`+`Raider`），解析/保存均未应用到游戏；显式 overwrite 覆盖成功，
未带 overwrite 的重复保存 409 且文件 hash 不变；`file_name` 为
`id r10/bad:name.*` 的 dry-run 清洗为 `id r10bad:name.*`（Linux 仅 `/` 无效）且不写文件。
证据 `tools/cua/.runtime/id-r10-run2-20261112/evidence/id-r10.json`，
runner `tests/parallel/ideology/id_r10_preset_save.py`，
fixture `tests/parallel/ideology/IdR10Fixture.cs`。

上述旧 ID-R10 样本未覆盖载入。2026-11-13 的新增保存→新开局→Archonexus
载入、Mod差异确认及损坏header验证与限制见
[载入记录](../references/reviews/2026-11-13-ideology-saved-preset-load.md)。

其余未覆盖：跨版本实际加载、损坏正文（可读header）、
显式指定非主玩家理念的正向分支、Windows 无效字符、界面对话框与长事件本身。
