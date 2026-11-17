# 五 DLC 与 Core API 游玩范围摘要

维护：Codex；核对：2026-11-16；状态：本轮UI及环境差集已修复/限定核销；逐项状态只在统一账本维护。

目标按[纯API合同](api-playability.md)：正常开局、Core经营/旅行、五DLC正常机制/结局与失败继续，可通过查询、选择、调用及结果读回完成。
正常玩家操作与环境/调试API按[使用语义](api-usage-semantics.md)区分，不以刷任务、改值或UI输入代替正常业务链。

| 信息 | 权威入口 | 限定 |
| --- | --- | --- |
| 当前开放项与核销 | [统一账本](five-dlc-remaining-checklist.md) | 页首UI差集已限定核销；不另维护重复计数，不掩盖其它技术债 |
| UI可调整选项调查与修复 | [原对照](../references/reviews/2026-11-16-ui-api-parity.md)、[修复验收](../references/reviews/2026-11-16-ui-api-repair.md) | 原发现保留；14项/3环境组实现、原生结果及未测范围分别记录 |
| 本阶段交付 | [交付记录](../exec-plans/completed/2026-11-13-api-playability.md) | 七来源包、E矩阵、正常连续链与四族修复 |
| 最新四族复验 | [统一DLL报告](../references/reviews/2026-11-13-api-playability-gap-repair-integrated.md) | C-11/C-09/C-17/OD-R09，准确对象与真实终态 |
| 验证边界 | [质量](../QUALITY_SCORE.md)、[技术债](../exec-plans/tech-debt-tracker.md) | B-01–B-05、持续SSE及加载过渡响应等保留 |
| 当前部署/现场 | [状态页](../harness/current-state.md) | 源码/隔离验收已完成，正式DLL未替换 |

验收固定用户确认的1.6.4871 rev600/Core+五DLC。原66项、53/21实施组织、116个历史ID与2026-11-12的85族/25项补回，均是不同阶段的目录或快照，不作为当前待办或完成率分母。
[第二轮复核](../references/reviews/2026-11-12-api-playability-reaudit.md)、[候选/路由登记](../references/reviews/2026-11-12-api-playability-surface.json)与[历史操作矩阵](dlc-operation-matrix.md)保留来源及原限定；静态归属不单独证明业务通过。

[原缺口登记](../references/reviews/2026-11-13-api-playability-gap-inventory.md)保留两条实现差异、三个核心验证缺口及发现时安排；后续用户授权修复，四族已在同一DLL补验核销。
[原最终报告](../references/reviews/2026-11-13-api-playability-final.md)的无fixture491HTTP链保留旧DLL身份，不冒称新DLL整链重跑。

固定范围内发现具体遗漏时归原ID登记修复；不将全部自然前置、Def/RNG或未来版本/任意Mod穷举作为新门禁，也不把正常随机/自动结果改为玩家可任意控制。
