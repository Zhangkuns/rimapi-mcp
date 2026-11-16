# Anomaly 知识研究与研究开关（AN-R01 / AN-R02）

2026-11-12 新增：读取/停止原生异常知识类别研究，以及读写具体 `CompStudiable` 的
正常“研究”开关。窗口只用于定位，调用方不需要打开研究页或 gizmo。

## AN-R01：知识类别项目

`GET /api/v1/dlc/anomaly/knowledge?category=Basic`

```json
{"monolith_level":1,"highest_level_reached":1,"anomaly_study_enabled":true,
 "category_count":1,
 "categories":[{"category":"Basic","label":"basic",
   "current_project":"BioferriteExtraction","current_label":"bioferrite extraction",
   "current_progress":0.0,"current_cost":5.0,"current_finished":false,
   "overflow_category":null,
   "candidates":[{"def_name":"BioferriteExtraction","label":"bioferrite extraction",
     "progress":0.0,"cost":5.0,"progress_percent":0.0,"is_finished":false,
     "can_start_now":true,"is_hidden":false,"prerequisites":[]}]}]}
```

- 来自 `ResearchManager.CurrentAnomalyKnowledgeProjects` / `GetProject(category)` /
  `GetProgress`；`candidates` 是该 `KnowledgeCategoryDef` 下全部
  `ResearchProjectDef`，含原生 `CanStartNow`/`IsHidden`/前置，调用方无需硬编码 ID。
- 起始项目复用现有 `POST /research/target?name=<defName>`；原生
  `ResearchManager.SetCurrentProject` 会把项目写入对应知识类别槽。
- `category` 省略时列出所有类别；未知类别 400。

`POST /api/v1/dlc/anomaly/knowledge/stop`

```json
{"category":"Basic","dry_run":true}
```

- 走原生 `ResearchManager.StopProject(GetProject(category))`；只有该类别当前项目非空时
  可取用，否则 409。返回 `before_project`/`before_progress`/`after_project`/`applied`；
  `dry_run` 不调用原生停止。
- 幂等合同：`game_memory`；`GET` 为只读 observe。

## AN-R02：单对象研究开关

`GET /api/v1/dlc/anomaly/study?map_id=&thing_id=`

```json
{"thing_id":"WarpedObelisk_Mutator75478","thing_num":75478,
 "def_name":"WarpedObelisk_Mutator","study_enabled":false,
 "ever_studiable":true,"currently_studiable":false,
 "study_points":0.0,"progress_percent":0.0,"completed":false,
 "anomaly_knowledge_gained":0.0,"study_interactions":0,
 "ticks_til_next_study":-9956786,"knowledge_category":"Advanced",
 "frequency_ticks":120000,"requires_holding_platform":false,
 "requires_imprisonment":false,"show_toggle_gizmo":true}
```

- 对象查找：地图 `listerThings`，以及平台/容器内仍在 `mapPawns.AllPawns` 的实体 pawn。
- `ever_studiable` 调用原生 `CompStudiable.EverStudiable(out reason)`；未解锁研究/巨石等级
  或平台/囚禁资格不满足时为 false。`currently_studiable` 另含 `study_enabled`、
  研究频率与原生资格。
- `study_points`/`progress_percent`/`completed`/`anomaly_knowledge_gained`/
  `study_interactions`/`ticks_til_next_study` 来自原生组件或 def。

`POST /api/v1/dlc/anomaly/study`

```json
{"map_id":0,"thing_id":75478,"enabled":true,"dry_run":true}
```

- 直接调用 `CompStudiable.SetStudyEnabled(bool)`（原生 gizmo 的写入点）。未通过
  `EverStudiable` 时返回 422 并在 `data` 中带回当前状态；未知对象 404。
- 返回 `before_enabled`/`study_enabled`/`currently_studiable`/`applied`。`dry_run`
  只投影。策略层合同：`policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，独立 Xvfb :101，
1920×1080 全屏允许；窗口/尺寸/清理见 `display-evidence.json`、`cleanup.json`）：

- 巨石等级 0 时 `WarpedObelisk_Mutator` 的 `ever_studiable:false`，POST 启用 422；
  通过**正常** `POST /dlc/anomaly/monolith/activate` 升到等级 1（`applied:true`，
  `level_before:0→level_after:1`，无 debug 字段写入）后 `ever_studiable:true`。
- `study` 启用 dry 200/`applied:false`；正式 200/`applied:true`/`study_enabled:true`/
  `currently_studiable:true`；关闭后 `study_enabled:false`/`currently_studiable:false`，
  `ever_studiable` 仍为 true；未知 thing_id 404。
- AN-R01：激活后 `BioferriteExtraction` 的 `can_start_now:true`/`is_hidden:false`；
  现有 research target 200 后 GET 读回 `current_project:BioferriteExtraction`；
  停用 dry 200 且保留项目，正式 200/`applied:true`/`after_project:null`；再次停止 409。
- 证据 `tools/cua/.runtime/an-r01-20261112/evidence/an-r01-r02.json`（运行产物），
  runner `tests/an_r01_r02_knowledge_study.py`，fixture `prepare_studiable`/
  `prepare_monolith_actor`。
- 未测：知识进度真实增长/项目完成、多类别同时进行、`EntityCodex` 条目逐项发现、
  研究台/囚禁/平台特异资格、自动 Study 工作消耗完整周期；这些不改变本行读/写与
  原生 `CanStartNow`/`EverStudiable` 门禁的结论。

## 2026-11-13 补充：真实知识增长/完成与 StudyEnabled 对工作的控制（AN-R01/R02 V）

隔离 `g35_deathresting` 副本（tick 76787，五 DLC，最终复跑目录
`tools/cua/.runtime/an-final-20261113g/`）。fixture 生成四个原生
`WarpedObelisk_Mutator`，给两名空闲殖民者 `DarkStudy` 优先级 1；巨石用正常
`POST /dlc/anomaly/monolith/activate` 0→1；普通研究与知识项目全部走现有端点。

- 负向控制：四个 obelisk 的 `StudyEnabled` 保持原生默认 false，并用产品
  `POST /dlc/anomaly/study` 把同样可研究的激活巨石关闭后，推进 600 tick：
  无 `StudyInteract` job、Basic 项目进度保持 0。
- 正面：逐个开启 obelisk；原生 `WorkGiver_DarkStudyInteract`/`StudyInteract`
  实际工作，每一项 5-interaction job 贡献 `anomaly_knowledge_gained`
  1.8176/1.9643/2.2645，Basic 项目经原生 `Advanced.overflowCategory=Basic`
  增长到 `current_progress 5.0` 并完成。完成读回：`current_project:null`，
  `BasicPsychicRituals` 候选 `progress 5.0/cost 5.0/progress_percent 1.0/
  is_finished:true`；完成对话框由 harness 关闭后游戏继续。
- 独立类别：Basic 完成后 `AdvancedPsychicRituals` 合法
  `can_start_now:true` 并起始；第 4 个 obelisk 的原生研究把 0.4277 记入
  Advanced 槽，Basic 槽仍为空、项目仍 finished。类别独立性源码依据见
  `ResearchManager.SetCurrentProject/GetProject/ApplyKnowledge(category)` 与
  `KnowledgeCategoryProject` 每类别槽（F-AN 包）。
- job 结束后 `ticks_til_next_study≈119920-119977`，说明原生 120000 tick 频率
  阻止重复研究；`GET /dlc/anomaly/study` 读回四对象 `study_enabled:true`。
- 证据 `tools/cua/.runtime/an-final-20261113g/evidence/an-r01-r02-growth.json`，
  runner `tests/parallel/anomaly/an_r01_r02_growth.py`，fixture
  `tests/parallel/anomaly/AnomalyFixture.cs`（`prepare_growth`/`observe_growth`）。
- 未测：两类别同一时刻并行研究（同一 `ApplyKnowledge(category)` 路径，源码核对，
  未逐项目重测）、EntityCodex 逐项发现、其它 studiable Def、平台/囚禁研究资格；
  完成对话框文本未逐字断言。此前旧的读/停/开关证据仍有效，不重跑。

本 lane 交接与 F-AN 四行来源核对包见
[parallel/anomaly.md](../exec-plans/active/parallel/anomaly.md)、
[2026-11-13-f-anomaly-source-audit.md](../references/reviews/2026-11-13-f-anomaly-source-audit.md)。
