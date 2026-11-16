# Odyssey 重力船/客运穿梭机命名（OD-R07）

2026-11-12 新增：对实现原生 `IRenameable` 的船体读取/写入 `RenamableLabel`。
写入的是原生命名对话框最后落到 `Building_GravEngine.gravshipName` /
`Building_PassengerShuttle.shuttleName` 的同一属性；不覆盖范围/底板等纯显示层。

## 读取

`GET /api/v1/dlc/odyssey/renameable?map_id=0&thing_id=75478`

```json
{"map_id":0,"thing_id":75478,"thing_id_string":"PassengerShuttle75478",
 "def_name":"PassengerShuttle","renameable_label":"Passenger shuttle",
 "inspect_label":"Passenger shuttle","base_label":"Passenger shuttle",
 "name_hidden":null}
```

- `renameable_label`/`inspect_label` 是 `IRenameable` 原生值；`base_label` 是
  `def.LabelCap`。GravEngine 追加 `name_hidden`（原生首次命名对话框标志）。
- 非 `IRenameable` 422，未知 thing 404。

## 设置

`POST /api/v1/dlc/odyssey/renameable`

```json
{"thing_id":75478,"name":"Dawn Treader","dry_run":true}
```

- 名称规则与 `Dialog_NamePlayerGravship.IsValidName` 一致：非空、≤64 字符、
  `GrammarResolver.ContainsSpecialChars` 为 false；否则 400。
- GravEngine 写入后同时置 `nameHidden=false`（与 `Named()` 相同），避免原生首次
  命名对话框重复弹出；PassengerShuttle 无此字段。
- 返回 `before_name/requested_name/renameable_label/inspect_label/applied`；
  dry_run 不写。合同：GET observe，POST `direct/immediate/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 生成一艘玩家 `PassengerShuttle`；初始 `Passenger shuttle`；空名/65 字符 400；
dry 保持；正式改名 `Dawn Treader` 后 GET `renameable_label` 与 `inspect_label`
均为 `Dawn Treader`，再改 `Second Wind` 同样读回。
证据 `tools/cua/.runtime/od-r07-20261112/evidence/od-r07.json`，runner
`tests/od_r07_renameable.py`，fixture `prepare_renameable`。

- 未测：GravEngine 首次命名对话框的阈值触发、特殊字符样例、存档重载后的名称保留、
  多船/重命名冲突；未把范围/底板显示覆盖计入本行。
