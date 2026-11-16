# Core 畜栏自动割草/过滤与自动屠宰策略（C-37）

2026-11-12 新增：

- `GET /pen/policy?thing_id`：`CompAnimalPenMarker` 的 `RenamableLabel`、
  `autoCut`、动物/割草过滤器允许项、`CanDesignatePlants`。
- `POST /pen/policy`：设置名称、`autoCut`，并对 plant/animal 调
  `ThingFilter.SetAllow`；`cut_now` 调用原生 `DesignatePlantsToCut`
  （需围栏封闭），返回新增 `CutPlant` designation 数；dry_run 只读。
- `GET/POST /building/facility` 增加通用 `CompAutoCut`：
  `auto_cut`、`auto_cut_can_designate`、`auto_cut_filter_allowed` 与
  `auto_cut_now`；风电机与其它 `CompAutoCut` 设备共用，立即割草返回新增
  designation 数。
- `GET /autoslaughter`：`Map.autoSlaughterManager.configs` 全部字段与当前
  `AnimalsToSlaughter`。
- `POST /autoslaughter`：按动物设置 `max_total/max_males/max_females/
  max_*_young/allow_pregnant/allow_bonded`，并 `Notify_ConfigChanged`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成玩家 PenMarker，plant=Glowstool、animal=Bear_Grizzly。初始化
auto_cut false；dry 保持；set 后名称 RIMAPI Pen、auto_cut true、两个 filter
均排除；autoslaughter 找到 Bear_Grizzly 配置，设 max_total/males/females=1、
allow_pregnant/bonded true，GET 全部读回；未知 pen 404。
证据 `tools/cua/.runtime/c37-20261112/evidence/c37.json`，runner
`tests/c37_pen_policy.py`，fixture `prepare_pen_policy`。

2026-11-12 立即割草/风机/屠宰 preview 回归（隔离 `g35_deathresting` 副本）：
fixture 建围栏封闭畜栏（墙+门+玩家 marker+野生可割植物）与玩家 WindTurbine
及原生割草范围内植物。封闭 pen `cut_now` 新增 9 个 CutPlant
designation；未封闭 pen 422。`autoslaughter` 对未配置动物 dry_run 返回
`created:true/applied:false` 且 GET 配置仍无该项；正式设置后 `max_total=3`
读回。WindTurbine `auto_cut` false→true→false，`auto_cut_now` 新增 4 个
designation。证据
`tools/cua/.runtime/int-c37-20261112-run3/evidence/c37-integration.json`，
runner `tests/c37_pen_auto_cut.py`，fixture `prepare_enclosed_pen`/
`prepare_wind_turbine_cut`。
- 未长跑自主割草/自动屠宰派工；本行以原生 designation 数、config 和
  拒绝/预览语义为准。
