# API 使用语义：正常玩法、环境工具与调试

维护：Codex；核对：2026-11-13；状态：当前语义说明；不新增权限角色、路由字段或接口行为。

本页供正常游玩、开发与验收选择API。先按**具体操作及参数是否属于原版玩家的合法选择**判断，再查对应合同；不能只看路径、HTTP方法或`layer`。
正常玩法范围见[纯API合同](api-playability.md)，已登记缺口与证据见[操作账本](five-dlc-remaining-checklist.md)。

## 三类用途

| 用途 | 判断依据 | 代表操作 | 结果如何记录 |
| --- | --- | --- | --- |
| 正常玩家玩法 | 原版允许玩家作出的选择；按该流程检查资源、研究、资格、目标、确认与冷却，需要工作时由原生Job完成 | 新开局/正常配置、暂停倍速/存读档、蓝图/账单/政策、征召与合法能力、交易/旅行、正常DLC交互/任务/结局 | 查询→选择→执行→真实终态；提交不等于完成 |
| 环境与运行工具 | 服务发现、进程/虚拟桌面/输入与测试现场管理；本身不证明某个玩法完成 | version/capabilities、外部`desktop.py start/stop`、截图/Cua、fixture观察与有界推进 | 单独记录运行环境、源码/DLL、现场与工具结果 |
| 调试直接变更（用户所说的rootdebug） | 绕过正常取得过程而授予/改写状态、强制事件/环境，或启用测试保护 | 刷任务、直接改hediff/基因/头衔、添加天气条件、原生debug白名单、无敌开关 | 记为debug/direct或测试前置，不能核销正常玩法流程 |

暂停、保存、载入和原版允许的难度/开局配置属于正常玩家功能；启动Xvfb、加载测试fixture属于环境工具。
“读取环境”也可以是正常玩法所需观察：例如[Odyssey真空读取](../api/odyssey-environment.md)不等于强制修改环境。
同一领域甚至同一接口可能包含不同用途，按具体method/action/参数和原生方法核对。

## 容易混淆的入口

以下为代表选择，均相对于`/api/v1`；不是全路由权限白名单。

| 正常玩法 | 对应调试/辅助入口 | 区分点 |
| --- | --- | --- |
| `POST /pawn/equipment`的`action:dress_other`，由`ForceTargetWear`实际换衣 | `POST /pawn/apparel/wear`直接装备 | [装备合同](../api/core-equipment.md)区分工作与直接holder变更 |
| `POST /pawn/surgery`正常排单、材料/医生/原生手术 | `POST /pawn/hediff`及severity直接修改 | 写疾病或身体效果不等于完成手术 |
| `POST /quests/accept`接受已存在的任务及正常奖励选择 | `POST /quests/generate`、`DELETE /quests` | 生成/移除是调试管理；[任务合同](../api/archonexus.md)另列正常选择 |
| 基因提取/装配/植入的正常设备与工作链 | `POST/DELETE /dlc/biotech/gene`、`POST /dlc/biotech/xenotype`直接编辑 | [基因合同](../api/biotech-genes.md)分别说明编辑与正常机制 |
| 正常荣誉/授衔任务与仪式、许可购买/使用 | `POST /dlc/royalty/title`直接授衔 | [头衔](../api/royalty-title.md)与[许可](../api/royalty-permits.md)不互作验收 |
| 正常理念改革、角色/仪式与改信工作 | `/debug/native/execute`直接改certainty/precept | 正常改革仍检查原版资格；[debug合同](../debug-native-actions.md)明确绕过语义 |
| `GET /map/conditions`观察当前条件 | `POST/DELETE /map/conditions`强制添加/移除 | 相同path的读取和写入用途不同 |
| 正常查询/业务API取得对象和终态 | `/ui/controls`、`/ui/targets/*`及Cua | UI观测/聚焦/点击是独立验证路径，不代替纯API合同 |

## 能力字段与实际权限

`GET /capabilities`的`layer`表示执行语义：`observe / policy / command / direct`，`executor`表示执行者。
`layer_source:contract`来自精确method/path合同；`heuristic`是旧路由推断，不能据此承诺资格、dry-run或幂等。
`direct`也可能是合法即时玩家操作，例如交易成交、认领或结束胜利字幕；`observe`也可能读取调试信息。
这些字段**不是**`player/environment/rootdebug`角色或安全等级，不能据此自动构造完整正常玩法白名单。

当前源码使用统一的可选Bearer token，没有三套角色/scope ACL。`POST /debug/native/execute`另有活动游戏、加载状态、开发者模式和显式动作白名单检查；
`POST /debug/invulnerability`启用时检查活动游戏及hook，不要求开发者模式。其他旧直接变更接口各按自身合同执行，不能笼统称全部受DevMode限制。
本轮只整理上述语义，未增加或修改鉴权。鉴权、例外与源码入口见[安全边界](../SECURITY.md)。

字段实现见[AiEndpointContracts](../../Source/RIMAPI/RimworldRestApi/Core/Ai/AiEndpointContracts.cs)、
[CapabilitiesService](../../Source/RIMAPI/RimworldRestApi/Services/System/AiCapabilities/CapabilitiesService.cs)；
重试/预演与结果承诺见[精确合同](../api/ai-control-contracts.md)和[幂等合同](../api/idempotency.md)。

## 正常游玩与取证

正常游玩按领域合同查询真实对象、合法选项与阻止原因，选择正常参数，提交原生工作并读回实际结果。
不以开DevMode、刷材料/任务、改存档或fixture完成业务来满足纯API流程。

隔离验收可用fixture准备昂贵前置、观察和保护，但必须披露准备内容、时长/难度调整及未自然取得的资格。
正常工作/资源/信号/holder终态由被测API及原生逻辑产生；debug前置、UI结果与正常链分别记录。
已验样本和未测边界各自保留，不把可选调试功能计入正常玩法完成率。
