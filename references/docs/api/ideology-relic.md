# Ideology 圣物（G-48）

交付：读取圣物 precept/地图圣物/圣物龛，以及正常安装/取出圣物。
证据：`Reverse/evidence/dlc-G48-relic.json`、`dlc-G48-run.json`。

## 读取

`GET /api/v1/dlc/ideology/relic?map_id=0`

```json
{"map_id":0,
 "relic_precepts":[{"ideo":"Boccin Creed","label":"...","thing_def":"...","stuff":"...",
   "can_generate":false,"generated":{...},"in_player_possession":true}],
 "reliquary_count":1,
 "reliquaries":[{"thing_id":"Reliquary...","empty":true,"full":false,"contained":null}],
 "relic_count":1,
 "relics":[{"thing_id":"...","def_name":"...","is_relic":true,"spawned":true,"position":{...}}]}
```

- precept 来自 `Precept_Relic`（`ThingDef/stuff/CanGenerateRelic/GeneratedRelic/RelicInPlayerPossession`）。
- 圣物龛来自地图上的 `ThingDefOf.Reliquary` 与其 `CompRelicContainer`；地图圣物用
  `CompRelicContainer.IsRelic` 识别。

## 安装

`POST /api/v1/dlc/ideology/relic/install`

```json
{"map_id":0,"relic_id":"...","reliquary_id":"...","pawn_id":"321","dry_run":true}
```

- 原生玩家路径是 `FloatMenuOptionProvider_Relic` →
  `JobMaker.MakeJob(JobDefOf.InstallRelic, relic, reliquary, reliquary.InteractionCell)`。
  本端点排队同一 job；`applied` 只表示已提交，完成要等 300 tick 安装工作。
- 先检查圣物龛：未知 404、非玩家 422、已满 409；再检查圣物：未知 404、非圣物/未生成 422；
  人物须玩家、可操作、可达且未禁选/预留。dry-run 不创建 job。

## 取出

`POST /api/v1/dlc/ideology/relic/extract`

```json
{"map_id":0,"reliquary_id":"...","pawn_id":"321",
 "storage_x":120,"storage_z":130,"dry_run":false}
```

- 原生 `JobDefOf.ExtractRelic`；`storage_x/storage_z` 可选，省略时用原生
  `StoreUtility.TryFindBestBetterStorageFor` 找存储格（无存储 422）。显式格子须在界内且可站立。
- 空龛 409；成功排队后由原生工作把圣物搬到目标格。
- 实测：安装后龛 `full/contained` 准确；满龛再装 409；取出后龛 empty、圣物重新生成；
  空龛再取 409。

## 限制

- 未测 `CompRelicContainer` 的其它容器、圣物任务/生成流程、圣物在仪式中的心情/转换加成；
- 取出时原生存储搜索未在真实仓库中单独验收（fixture 用显式格）；
- 未做 Cua 对照。
