# Biotech 机械体派系配色（BT-R10）

2026-11-12 新增：读写玩家派系的 `AllegianceColor`，与
`MainTabWindow_Mechs` 的 ChooseMechColor → `Dialog_ChooseColor` 回调一致；
写入后对所有玩家机械体调用 `PortraitsCache.SetDirty`。

## 读取

`GET /api/v1/dlc/biotech/mech/color` → `{r,g,b,hex}`。

## 设置

`POST /api/v1/dlc/biotech/mech/color` `{hex:"#FF8800",dry_run}`

- 接受 `#RRGGBB`/`RRGGBB`，写入 `Faction.OfPlayer.AllegianceColor`。
- 返回 `r/g/b/hex`、`requested_hex`、`mechs_marked_dirty`、`applied`；
  dry_run 不写；非法 hex 400。
- 合同：POST `policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
原色 `#00BCD8`；dry 保持；正式设 `#FF8800` 后 GET 精确回读；非法 `#GGGGGG` 400；
随后恢复原色。本副本没有玩家机械体，`mechs_marked_dirty:0`，因此未观察真实
机械体肖像刷新。
证据 `tools/cua/.runtime/bt-r10-20261112/evidence/bt-r10.json`，runner
`tests/bt_r10_mech_color.py`。

- 未测：有机械体时的肖像 dirty/实际渲染、机械体派系切换、颜色持久化重载；
  这是低优先级外观行，未作为其它机械链门禁。
