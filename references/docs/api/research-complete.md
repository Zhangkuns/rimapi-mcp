# 即时完成指定研究

维护：Codex；核对：2026-11-20；状态：隔离验收通过，部署见[当前状态](../harness/current-state.md)。

`POST /api/v1/debug/native/execute` 的 `research.complete` 按项目的 Def 名即时完成研究。需要活动存档和开发者模式。用 `GET /api/v1/research/tree` 找 `name`，可先预演：

```json
{"action":"research.complete","def_name":"SolarPanels","dry_run":true}
```

`would_complete` 按原生处理顺序列出目标及尚未完成的显式前置研究。确认后去掉 `dry_run`，可传 `Idempotency-Key` 避免网络重试重复执行。`completed` 列出本次完成的项目；`before` 和 `after` 包含目标及受影响项目的进度、完成状态、科技蓝图和分析要求读回，以及当前主要研究和目标所属知识分类的选题（空值可能省略）。操作后也可用 `GET /api/v1/research/project?name=SolarPanels` 或 `GET /api/v1/research/finished` 确认。研究查询用原生完成标记，详情中的 `progress` 与 `research_points` 都用游戏的显示点数。

动作沿用 `ResearchManager.FinishProject`：可以绕过正常研究资格，递归完成 `prerequisites` 中尚未完成的项目，并补齐方法处理的科技蓝图与分析进度，触发原生解锁和信号。`hiddenPrerequisites` 不会自动递归。支持 Anomaly 启用时的知识研究。它不需要先选题：完成当前项目会清除该选题，无关的当前研究保持。原生发现信件仍可出现。常规 `POST /api/v1/research/target`（包括 `force=true`）仍然只负责选题。

已完成项目返回无操作，`applied:false`、`native_invoked:false`；缺少名称为 `400`，找不到项目为 `404`。开发者模式、游戏实例、预演和异常读回沿用[原生开发者合同](../debug-native-actions.md)。
