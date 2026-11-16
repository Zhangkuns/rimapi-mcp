# 产品合同索引

维护：Codex；核对：2026-11-13；状态：当前，API语义、纯API游玩合同及Core/五DLC统一账本。

合同描述调用者可依赖的行为；完成度另见 [质量状态](../QUALITY_SCORE.md)、[DLC 操作矩阵](dlc-operation-matrix.md)。
当前剩余逐项维护在[完整清单](five-dlc-remaining-checklist.md)，概览见[缺口摘要](dlc-gap-audit.md)。API 页面中的上游 YAML 统计不等于 fork 的完整运行路由数。

| 领域 | 权威详细文档 |
| --- | --- |
| 正常玩法/环境/调试语义 | [使用语义](api-usage-semantics.md)、[鉴权实际边界](../SECURITY.md)；按行为区分，不新增角色权限 |
| 最终交付 / 剩余任务 | [纯API游玩合同](api-playability.md)、[操作账本](five-dlc-remaining-checklist.md)、[全功能范围计划](../exec-plans/active/five-dlc-full-function.md) |
| API 索引 / 控制面 | [API](../api.md)、[精确能力合同](../api/ai-control-contracts.md) |
| 重试与交易 | [幂等](../api/idempotency.md)、[交易](../api/trade-control.md) |
| Core 正常结局 | [胜利读回与结束字幕](../api/core-game-ending.md) |
| Core 服装政策 | [outfit](../api/outfit-policy.md) |
| Core 救援/俘虏/逮捕/送床/释放 | [C-30 API 合同](../api/core-take-to-bed.md) |
| Royalty | [灵能](../api/royalty-control.md)、[许可](../api/royalty-permits.md)、[头衔](../api/royalty-title.md) |
| Ideology | [教义](../api/ideology.md)、[风格](../api/ideology-style.md)、[仪式实例/资格/启动](../api/ideology-ritual.md)、[瓜拉兰树](../api/ideology-tree.md)、[造型/染发](../api/ideology-styling.md)、[神经超充](../api/ideology-supercharger.md)、[囚犯奴役](../api/ideology-slavery.md)、[任务选择/Archonexus](../api/archonexus.md) |
| Odyssey | [重力船](../api/odyssey-gravship.md)、[层](../api/odyssey-layers.md)、[环境](../api/odyssey-environment.md)、[地标](../api/odyssey-landmark.md)、[捕鱼](../api/odyssey-fishing.md)、[装备架](../api/odyssey-outfit-stand.md)、[动物训练](../api/odyssey-animal-training.md) |
| Biotech | [基因](../api/biotech-genes.md)、[机械师](../api/biotech-mechs.md)、[生命周期](../api/biotech-life.md)、[生殖链](../api/biotech-reproduction.md)、[污染](../api/biotech-pollution.md)、[次核心](../api/biotech-subcore.md)、[能力](../api/biotech-abilities.md)、[血原质包](../api/biotech-hemogen.md)、[成长时刻](../api/biotech-growthmoment.md)、[婴儿照护](../api/biotech-childcare.md) |
| Anomaly | [巨石](../api/anomaly-monolith.md)、[收容/转移/活动抑制](../api/anomaly-containment.md)、[正常心灵仪式](../api/anomaly-psychic-ritual.md)、[幽魂/蠕行者](../api/anomaly-creepjoiner.md)；其它操作从矩阵查对应计划/审阅 |
| UI / 地图 / 世界 | [语义桥](../ui-semantic-bridge.md)、[空间目标](../ui-spatial-targets.md) |
| 调试 | [原生动作](../debug-native-actions.md)、[无敌保护](../debug-invulnerability.md) |

新增或改变行为时更新对应详细文档及精确合同/运行检查。先区分直接变更、提交工作和最终完成，再说明拒绝、部分效果、重放与事件；不要只补路由名称。
