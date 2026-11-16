# Core 存档目录、元数据与保存/载入完成（C-06）

2026-11-13 新增/增强。所有数据来自原生 `GenFilePaths`、`SaveFileInfo.LoadData`、
`GameDataSaveLoader` 与 `Current.Game`；不扫描/自解析存档文件，也不以排队返回
冒充完成。

## 存档目录与元数据（只读）

`GET /api/v1/game/saves`

- `save_folder`：`GenFilePaths.SavedGamesFolderPath`。
- `files[]`：`GenFilePaths.AllSavedGameFiles` 逐项 `SaveFileInfo.LoadData()`：
  `file_name`、`file_path`、`bytes`、`last_write_time`、`game_version`、
  `readable`（原生 `???`/加载中状态判为不可读）。
- `current_game`：`game_instance`（API 载入代次：无游戏为 0，每次原生载入
  完成后递增；用于区分载入前后 Game 会话，不是进程内对象哈希）、`save_file`、
  `world_name`、`tick`、`storyteller`、`map_count`、`program_state`。
- `last_operation`：最近一次 save/load 的 `phase`（completed/queued/failed）、
  `operation`、`save_file`、`request_id`、`requested_at`/`completed_at`、
  `is_load`、`overwritten`、`commitment_mode`、`error`、`game_instance`、
  `world_name`、`tick`、`storyteller`、`map_count`。

## 保存

`POST /api/v1/game/save`

```json
{"file_name":"my-save","overwrite":false}
```

- 文件名经原生 `GenFile.SanitizedFileName`；承诺模式
  （`Current.Game.Info.permadeathMode`）强制使用原生
  `permadeathModeUniqueName`，忽略请求名并返回 `commitment_mode=true`/
  `requested_file_name`/`effective_file_name`。
- 同名存在且非承诺模式、`overwrite=false` → 409 + 原文件元数据；`true` 才
  覆盖。承诺模式已存在同名存档时任何保存都返回 409（不允许覆盖）。
- 同步调用原生 `GameDataSaveLoader.SaveGame(effectiveName)`；完成判定要求
  保存后文件存在、`SaveFileInfo.readable=true`、`bytes>0`，返回 `completed:true`
  与 `metadata`/`tick`/`game_instance`。未生成或不可读 → 500/409 且
  `completed=false`。不再只返回“已排队”。

## 载入

`POST /api/v1/game/load`

```json
{"file_name":"my-save","check_version":false,"skip_mod_mismatch":false}
```

- 先按原生 `GenFilePaths.FilePathForSavedGame` + `SaveFileInfo` 校验：
  未知 404、元数据不可读 422；`check_version=true` 且
  `BackCompatibility` 判断不兼容 → 409，均不排队。
- 通过后调用原生 `GameDataSaveLoader.LoadGame(name)`，返回
  `status:"queued"`、`request_id`、`requested_file`、`before_game_instance`、
  `save_metadata`；主线程不阻塞。
- 完成由 `GET /game/saves.last_operation` 判定：`Current.Game.InitData.gameToLoad`
  清空、`ProgramState==Playing`、`LongEventHandler` 空闲且新的
  `game_instance`/`save_file`/tick 可读；超时保持 `queued`，不伪报
  completed。载入后用 `current_game` 读回新 Game 身份。

## 证据与边界

2026-11-13 隔离 g35 副本 + 真实新承诺模式开局：
- 初始原生存档列表 `g35_deathresting.rws` 可读；`c06-test` 首次保存
  `completed=true`、bytes 16,581,749、readable；同名未 overwrite 409 且
  返回原元数据；overwrite=true 200 且 `overwritten=true`。
- 未知载入 404；fixture 写的一个不可读 `.rws` 载入 422；均未排队。
- 正常载入 `c06-test`：返回 queued+request_id；轮询 `last_operation` 到
  completed 后 `game_instance` 变化、`save_file=c06-test`、tick 与保存后
  一致；fixture 看到同一实例。
- 真实 `/game/start`（permadeath=true）后：`commitment_name` 有效；保存请求
  `ignored-request` 实际写入 `Western Lonnalior (Permadeath)`，
  `commitment_mode=true`；再次 overwrite=true 409（承诺模式不覆盖）。
- `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。
- 未测：异版本存档 409（无外部版本样本）、Autosave/Autostart 特殊名、
  载入过程中再发请求、保存时 loading-screen 状态。测试 fixture 只读回，
  唯一写入是一个故意损坏的 `.rws`（仅测试前置，已披露）。
