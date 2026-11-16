# Core 派系/殖民地/人物命名（C-07）

2026-11-13 新增正常玩家派系/殖民地命名写入口，复用既有人物改名。

## `POST /api/v1/game/names`

新共享路由（command/direct，immediate，validate）：

```json
{
  "faction_name": "C07 Faction",
  "settlement_id": 113,
  "settlement_name": "C07 Colony",
  "dry_run": false
}
```

- `settlement_id` 是 `Settlement.ID`（世界对象 ID，可从既有
  `GET /api/v1/world/player/settlements` 取得；该 GET 仍是缓存端点）。
- `faction_name`/`settlement_name` 至少一个；`settlement_name` 必须同时给
  `settlement_id`。两者同时给出时先完整预检，任一失败不写入（原子）。
- 只接受玩家派系与玩家 Settlement；不存在 Settlement 404，非玩家/非本派系 422。
- 名称校验直接走原生 utility：
  - faction：`NamePlayerFactionDialogUtility.IsValidName`（trim 后 1..64、
    `GenText.IsValidFilename`、无 `GrammarResolver.ContainsSpecialChars`）；
  - settlement：`NamePlayerSettlementDialogUtility.IsValidName`（1..64、无
    `{ } [ ]` 特殊字符）；
  - 通过后才调用原生 `Named`；settlement 写入 `named_by_player=true`。
- `dry_run:true` 返回 `before` 与 `after`（`candidate:true`）候选，`applied:false`，
  不调用 `Named`，不触发 permadeath 自动存档名副作用。
- 正常写后响应 `faction`（load_id/def_name/name/is_player）、`settlement`
  （id/name/tile/has_map/map_id/named_by_player/faction）与 `settlements` 列表；
  均来自同一原生对象。响应附 `applied`/`dry_run`/`validation`/
  `permadeath_mode`/`permadeath_autosave_rename`。
- permadeath 模式披露：原生 `NamePlayerFactionDialogUtility.Named` 在
  `permadeathMode` 下会重命名自动存档文件；响应字段
  `permadeath_autosave_rename` 标识该副作用，`dry_run` 永不触发。本行不把该副作用
  作为已实测项。

## 正常人物命名

复用既有 `POST /api/v1/pawn/edit/basic`（`name`/`first_name`/`nick_name`/
`last_name`），不新增 pawn 路由，不重复 S-11 出生信完成语义或 OD-R07 船命名。
本次补上 `Dialog_NamePawn` 的普通人物长度门禁：`NameTriple` first/last 1..12、
nick 0..16；`NameSingle` 1..16；越界 400。直接读回可用未缓存的
`GET /api/v1/pawns/details?id=<id>` 的 `social_info.name`（`GET /colonists` 是
按 game tick 缓存端点）。

## 证据与边界

2026-11-13 独立隔离主菜单开局（`c07-names`，map250/Spring）：

- dry_run 候选 `C07 Dry Faction`/`C07 Dry Colony`，`applied:false`，读取派系仍为
  原 `New Arrivals`。
- 正式合并命名：faction `C07 Faction`、settlement id 112/`C07 Colony`，
  `named_by_player:true`，tile 96931、faction load_id 16；随后单独
  `NamePlayerFaction`/`NamePlayerSettlement` 两次改名也读回。
- 65 字符、`/` 非法文件名、settlement `[` 特殊字符均 400；缺 settlement_id 400；
  不存在 404；非玩家 settlement 422；faction 合法 + settlement 非法同时提交 400，
  派系读回保持 `C07 Faction Two`（原子失败不写）。
- 既有人物改名：`C07Nick` 经 `/pawns/details` 读回，17 字符 nick 400 且旧名保持。
- 证据 `tools/cua/.runtime/core-start-20261113u/evidence/c07-naming.json`，
  DLL sha256 `e885cd5dac651a22bff710ba6125db5c97bf81fc47539b74e21628a5913d81ce`
  （来源 `80fb0dbc`），fixture
  `3a7d09e469cbe260a925a014156f977b45ae4b7bd1f81a44fdda409fd41923de`。
- 未测：permadeath 自动存档名副作用、`Suggested`/随机命名候选、标题编辑、
  非人类 NameSingle 变体、UI 取消路径；缓存 GET 的读回以未缓存
  `/pawns/details` 和 POST 原生响应为准。
