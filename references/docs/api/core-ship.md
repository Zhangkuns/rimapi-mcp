# Core 飞船报告、反应堆启动与发射（C-13）

2026-11-13 ready。只走原生 `ShipUtility/CompHibernatable/ShipCountdown`；
不替代 Archonexus/Cerebrex/巨石结局。

## 路由

- `GET /api/v1/ship/report?thing_id=`：原生 `ShipBuildingsAttachedTo`、
  RequiredParts 实计、`LaunchFailReasons`、`HasHibernatingParts`、
  `ShipCountdown.CountingDown`、各 `CompHibernatable.State`、pod 满员/
  computer core/reactor。只读。
- `POST /api/v1/ship/startup` `{thing_id reactor,dry_run}`：仅 reactor 且有
  hibernating，调原生 `ShipUtility.StartupHibernatingParts`（原版 gizmo
  Confirm 路径）；无 hibernating 409、非 ship part 422；dry 不写；返回
  before/after state。
- `POST /api/v1/ship/launch` `{thing_id computer_core,dry_run}`：仅 core；
  `LaunchFailReasons` 空且不在倒计时才 `ForceLaunch`；失败 422/409，dry 不启动。
- `POST /api/v1/ship/cancel`：仅 `ShipCountdown.CountingDown` 调原生
  `CancelCountdown`，否则 409；没有取消 reactor startup 的路径。

## 真实证据（g35 隔离）

- 完整 8 部件 cardinal 链（core/reactor/3 engine/beam/sensor/casket）+ casket
  内真 pawn；report 8/8、RequiredParts 全满足。
- startup 后 reactor `Starting`；`TimeSpeed.Ultrafast` 真实推进到 tick
  916817（约 840k）后 `Running`、无 fail reason。
- launch1 200 countdown；1.5s 内 cancel 200 → countdown false；重新 launch2
  200 countdown；7.2s 后 `Screen_Credits` 打开、ship 404、colonist 5→4
  （casket 内离开人物）、游戏暂停在结局，`game_over.game_ending=false`
  但 credits 为最终状态。
- 4xx：无 hibernating 409、非 reactor 422、未完成 launch 422、无 countdown
  cancel 409；`/ui/window` 无关。
- 证据：
  `tools/cua/.runtime/c13-20261113-run15/evidence/c13.json`（840k 全跑，
  末段仅 post-ending 旧断言失败）、
  `tools/cua/.runtime/c13-20261113-run16/evidence/c13-resume.json`
  （prelaunch Autosave-4 复跑 launch/cancel/relaunch/credits，passed）。
  cleanup：source_save/formal_dll/global_prefs unchanged。
- 披露：为达 840k 使用隔离 fixture 和 Ultrafast；未 debug ticks/State。
  run15 曾设 Peaceful 避免威胁暂停（证据内有 `peaceful`），威胁期防御未作为
  C-13 验收内容。Autosave-4 是同一真实 840k 跑在发射前的原生存档。

胜利后的结构化状态与原生继续/主菜单选择见[结局合同](core-game-ending.md)。
