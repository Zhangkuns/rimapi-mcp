# Core 认领/收养/连矿（C-19）

2026-11-12 新增，全部走正常 Designator 原生语义：

- `GET /building/claim?thing_id`：只读 `Thing.ClaimableBy(Faction.OfPlayer)`，
  返回 `claimable/reason/faction`。
- `POST /building/claim`：body `thing_id`、`dry_run`；合法时
  `Thing.SetFaction(Faction.OfPlayer)`，返回读回。
- `GET /pawn/adopt?pawn_id`：只读 `Pawn.AdoptableBy(Faction.OfPlayer)`，
  返回 `adoptable/reason`。
- `POST /pawn/adopt`：body `pawn_id`、`dry_run`；合法时 SetFaction、按原生
  `Designator_Adopt` 处理床分配与 `ChoiceLetter_BabyBirth`（Adopted），
  返回 `is_colonist/bed_id`。
- `POST /map/mine-vein`：body `map_id/x/z/cancel/dry_run`；原生
  `Designator_MineVein.FloodFillDesignations` / `RemoveContiguousDesignations`，
  返回新增/移除 designation 数。

## 证据与边界

`int-c19-20261113-run2`（隔离 Xvfb/profile，`g35_deathresting` 副本
tick 76787，五 DLC）：fixture 先 `prepare_baby`，再清派系并生成 AncientSingleBed
与 2x2 MineableSteel。claimable/adoptable true；dry 不写；正式 claim/adopt 后
PlayerColony、IsColonist；连矿 +4、取消 -4；重复/未知 404/400。证据
`tools/cua/.runtime/int-c19-20261113-run2/evidence/c19-integration.json`。
未测收养信件/床分配的 UI 表现与长时采矿/搬运；收养对象为无派系 Baby。
