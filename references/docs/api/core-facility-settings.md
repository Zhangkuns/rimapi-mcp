# Core 设施正常设置：门/聚会点/照明/陷阱/扫描器（C-26）

2026-11-16 修复：风机支持allowed_auto_cut_plants完整替换/清空及成对auto_cut_plant_def/allowed编辑，读回不截断；发射器支持auto_place_pods原生持续开关。全请求预检、dry及原生标记/补蓝图通过，见[UI-06/09合同](ui-option-repairs.md)与[验收限定](../references/reviews/2026-11-16-ui-api-repair.md)。

2026-11-12 新增 `GET/POST /api/v1/building/facility`：

- 门：`Building_Door.holdOpenInt`（gizmo 同字段），读回 `HoldOpen`。
- 聚会点：`CompGatherSpot.Active`，原生注册/注销。
- 照明：`CompGlower.GlowColor`（支持 `#RRGGBB`）与 Ideology darklight
  （`DarklightUtility.DefaultDarklight`）。
- 陷阱：`Building_Trap.autoRearm`，读回自动重置开关。
- 扫描器：`CompLongRangeMineralScanner.targetMineable`，校验为可开采
  `ThingDef`（如 `MineableGold`）。
- 灭火泡沫设备：`Building_FirefoamPopper.autoRebuild`（gizmo 同字段），
  读回 `firefoam_auto_rebuild`/`can_set_firefoam_auto_rebuild`。原生门槛为
  玩家派系、`blueprintDef` 存在且研究完成；未满足时 POST 422。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 Door、Campfire、StandingLamp、TrapSpike、
LongRangeMineralScanner。dry 不写；门 hold_open true；营火 gather false；
灯 glow_color 设置并 darklight true；陷阱 auto_rearm false（初始 true）；
扫描器 target MineableGold 读回；未知 404。
证据 `tools/cua/.runtime/c26-20261112/evidence/c26.json`，runner
`tests/c26_facility_settings.py`，fixture `prepare_facility_settings`。

2026-11-12 灭火泡沫自动重建回归（隔离 `g35_deathresting` 副本）：
fixture 补完原生研究、设 Home 区并生成玩家 FirefoamPopper（前置披露）；
创建后默认 `firefoam_auto_rebuild:true`、`can_set:true`；POST false 读回
false，dry-run true 不写；关闭后 `Destroy(KillFinalize)` 无重建蓝图；另一台
开启后销毁，原生在同格放置 `Blueprint_FirefoamPopper`（blueprint_count=1）。
证据 `tools/cua/.runtime/int-c26-20261112-run4/evidence/c26-integration.json`，
runner `tests/c26_firefoam_autorebuild.py`，fixture `prepare_firefoam_popper`/
`destroy_firefoam_popper`。
- 未重跑实际关门保持/聚会/照明/陷阱重置/扫描任务的持续行为；火灾触发路径与
  真实建造流程未重跑，本行以原生 gizmo 字段、状态与蓝图结果为准。
