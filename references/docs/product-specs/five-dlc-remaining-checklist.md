# 五 DLC 与 Core 纯 API 游玩剩余清单

维护：Codex；核对：2026-11-17（America/Los_Angeles）；状态：本轮UI-01–14及UI-E01–03已修复/限定验收核销；原发现和旧阶段证据保留，正式DLL未部署。

## 2026-11-16 UI对照与本轮核销

原调查确认14G并单列3组环境/显示差集，发现时的[报告](../references/reviews/2026-11-16-ui-api-parity.md)及JSON不改写。用户随后授权全部修复，本轮已完成代码/合同、原生终态和环境持久化验证，**这批登记当前0G/0V/0A，17个ID均核销**。具体源码、两候选DLL限定、768条HTTP（含拒绝/原错误）、9组重放/事件、失败、清理及未测分支见[修复验收](../references/reviews/2026-11-16-ui-api-repair.md)和[机器证据](../references/reviews/2026-11-16-ui-api-repair.json)。这不是全游戏未知缺陷数或完成率，原教程等其它已知技术债仍保留。

| ID | 原操作族 | 状态/顺序 | 原差集（具体核销见修复报告） |
| --- | --- | --- | --- |
| UI-01 | C-27 | 已核销 / P1 | 账单任意/奴隶/机械体/非机械体模式与清除指定人物；核原生互斥标志和工作资格 |
| UI-02 | C-27 | 已核销 / P1 | 计数来源不能选单个未分组储存区，也不能恢复全部；核准确ISlotGroup身份及计数 |
| UI-03 | C-27 | 已核销 / P3 | classic账单产品全局/基础/类别风格与图案变体；核bill及实际产物 |
| UI-04 | C-35 | 已核销 / P1 | 阅读策略品质和精神崩溃概率范围缺读写；核两侧ThingFilter与读书资格 |
| UI-05 | C-37 | 已核销 / P1 | 畜栏动物过滤缺AllowMaleGender/AllowFemaleGender；核同种公母接纳与正常牵引 |
| UI-06 | C-26、C-37 | 已核销 / P1 | 风机AutoCutFilter缺配置，允许项读回还Take(50)；核过滤和真实割草标记 |
| UI-07 | S-07 | 已核销 / P1 | 塑形舱等ThingComp拥有的StorageSettings无法解析；炮塔单Def允许已有，内部gun完整过滤仍缺适配 |
| UI-08 | S-06 | 已核销 / P1 | 床用途不能切Slave，现只有ForPrisoners/Medical；核三类owner/原生副作用和实际选床 |
| UI-09 | C-12 | 已核销 / P2 | 发射器autoPlacePods持续自动补建开关缺映射；核即时及发射后原生蓝图 |
| UI-10 | C-18、ID-R05 | 已核销 / P3 | classic既有蓝图更换/清除风格缺正常适配；核entityDefToBuild、styleOverridden与建成风格 |
| UI-11 | S-07、C-35 | 已核销 / P2 | 书架实际内容/按书弹出并禁止缺映射；核准确holder/地面书ID和另一书保持 |
| UI-12 | C-14 | 已核销 / P1 | 手动工作优先级模式仅写bool，漏全体人物Notify_UseWorkPrioritiesChanged；核已缓存工作排序 |
| UI-13 | C-36 | 已核销 / P1 | 动物已指定主人不能设回None；核Master=null且跟随/训练保持 |
| UI-14 | C-05、C-06 | 已核销 / P2 | 自动存档间隔/数量、读档暂停和信件自动暂停缺设置；间隔还被读为bool，需保真及持久读回 |

UI-E01音画/显示、UI-E02语言/输入/键位、UI-E03原生底栏均已按用户追加授权实施并限定验证，仍按环境类别记录，不伪装成DLC玩法通过。真空层地面不可开启等原生资格被保留；原生Prefs/KeyPrefs文件重读和Game保存载入通过。候选125文件/555行/107 gizmo仍仅为原调查检索入口，不当覆盖率。

## 2026-11-16 补充登记：仪式结果控制与观察

用户随后明确要求核对仪式内容/奖励、人员/目标及能否控制最终结果。已有pattern选择、附加奖励配置与人员/目标入口；它们不保证原生随机结算。以下是本次静态核对新登记项，**尚未实现**，不属于上方已核销17项，也不重写旧证据。详见[核对记录](../references/reviews/2026-11-16-ritual-outcome-audit.md)及[合同边界](../api/ideology-ritual.md)。

| ID | 用途 | 当前缺口与后续验证 |
| --- | --- | --- |
| RIT-01 | 正常观察 / G | status/dry缺原生开始窗口完整质量因素、预计范围、评级概率与附加奖励条件；需对同一assignment原生预览核对，不凭自编公式替代 |
| RIT-02 | 正常结果读回 / G | 通用天灯终态缺精确possibility/quality、奖励执行及访客/礼物的run关联回执；需原生结算时捕获并保存，不能重抽结果或仅用当前记忆归因 |
| RIT-D01 | 用户提出的debug能力需求 | 无指定最终评级/保证奖励分支的API；与正常奖励种类配置分开。实现须绑定准确run、显式可发现选项和真实原生效果，不能把修改响应或刷一份奖励当仪式结果已受控 |

## 2026-11-20 补充登记：现有物品双向转移

按[转移调查](../references/reviews/2026-11-20-item-transfer-audit.md)登记。三项已实现并通过[限定验收](../references/reviews/2026-11-20-item-transfer.md)，包括 `item.move` 的普通货架坐标路径回归。观察与即时调试能力独立于正常玩法。

| ID | 用途 | 已实现与验收范围 |
| --- | --- | --- |
| ITEM-X01 | 格子储存的观察与即时调试操作 | 按货架/储存区 ID 查询真实格、堆与剩余堆位；按储存 ID 选原生合格格子 |
| ITEM-X02 | 已有物品在地图格与内部容器间双向转移 | 从地面或货架立即放进指定类型容器，从容器直接取到准确地面或货架格；各类型资格与终态独立核对 |
| ITEM-X03 | 已有物品在两个内部容器间即时直转 | 按已支持的源/目标容器和堆 ID 转移，核对两端归属及原堆数量；具体组合见验收记录 |

## 2026-11-22 补充登记：畜牧

[Cua 与 REST 原核对](../references/reviews/2026-11-22-husbandry-audit.md)确认以下差集，现均按[限定验收](../references/reviews/2026-11-22-husbandry-repair.md)核销；不重开既有 C-36/C-37 已验范围，正式 DLL 未部署。

| ID | 分类 | 确认的缺口与验收重点 |
| --- | --- | --- |
| HUSB-01 | 正常观察 / G，优先 | 畜栏每日天然营养产出、消耗及盈亏、各季估算、各物种消耗、栏内食物储备及有缺口时的预计支撑天数。以同一 tick 的原版 Food 页为准，不用可见草量代替增长率。 |
| HUSB-02 | 正常观察与设置 / G | 原版 Food 页示例物种的列出、添加/移除与单种估算容量；区分示例和实际畜群。 |
| HUSB-03 | 正常观察 / G | 动物实例生命阶段及适用组件的奶/毛/蛋生产进度、可收获状态及蛋受精状态；复用已有年龄与妊娠查询，不以即时结算回执替代。 |
| HUSB-04 | 正常选择与读回 / G | 通用标记已可添加/撤销 `Tame`/`Slaughter`；差集是原版目标资格/警告及准确动物的当前标记读回。验证与自动屠宰候选、已完成驯服或屠宰区分。 |
| HUSB-B01 | 响应缺陷 | 三个畜栏 GET 的 selector 查询均返回有效数据但误报过滤强转失败；回归正确响应及无误报，独立于营养功能。 |

## 前轮已交付与历史账本

2026-11-21 PLANT-D01 已交付：[指定树木即时砍伐](../api/plant-cut.md)支持预演、暂停时直接执行和木材落地读回，验证并堆、原版树桩及防砍保护。正常砍伐工作保持独立。

2026-11-20 RES-D01 已交付：[指定研究即时完成](../api/research-complete.md)支持预演、原生完成与结果读回；显式前置、科技蓝图与异常知识已隔离验收并验证保存载入。正常选题和研究工作保持独立。

2026-11-20 ITEM-D02 已交付：[普通货架堆位设置](../api/shelf-capacity.md)作用于当前存档所有原版双格货架，隔离验收通过 10 和 99 堆、满架拒绝及存读档；独立于正常搬运工作。

2026-11-20 ITEM-D01 已交付：[整堆移动调试动作](../api/item-move-debug.md)按同图物品 ID 和精确坐标移动原堆，22 条 HTTP、原生读回和隔离清理通过。正常搬运流程独立记录。

2026-11-20 ROOF-D01 已交付：[屋顶调试动作](../api/roof-grid-debug.md)按坐标设置、替换或清除实际 RoofDef，隔离副本 23 条 HTTP 与原生读回、重放事件和清理通过。正常建造区域接口继续使用，参见[C-21 合同](../api/core-areas.md)；本次不核销正常施工工作。

2026-11-17 BLD-D01调试能力已交付：`construction.complete`按现有蓝图/Frame ID复用原生转换/完成、安装重装沿原物，8类终态/33HTTP/dry/重放/事件通过，tick不推进，现场清理/6保护文件保持。见[合同](../api/construction-complete.md)、[限定验收](../references/reviews/2026-11-17-construction-complete.md)。这不核销正常施工/RitualSpot问题，也不关闭上方RIT-01/02/D01。

2026-11-16 附加交付：[六组API缺口统一修复](../references/reviews/2026-11-16-six-api-gaps.md)覆盖施工完整清单、畜栏与正常牵引、zone/building完整储存、24小时批量日程、人物原生Medicine携带目标、四类既有方案按ID改名。统一DLL0d7ef7a6…157HTTP通过，旧失败及未测边界保留；[完成计划](../exec-plans/completed/2026-11-16-construction-pen-readback.md)。此登记不重写旧矩阵的原日期、不把路由数或原账本归零解释为全功能完成率。

本页是[纯 API 游玩合同](api-playability.md)的统一执行账本。目标是通过 RIMAPI 查询、选择、执行和读回，
完成正常开局、Core 经营/旅行、五 DLC 玩法和结局，无需人打开窗口或补操作。
固定本机 RimWorld **1.6.4871 rev600**、Core + 五 DLC 同时启用；窗口代码用于追踪原生业务，不要求 API 创建窗口。
原66项遗漏了正常选择，也错误排除了开局与基础经营，**不再作为全部剩余的分母**。
[第二轮复核](../references/reviews/2026-11-12-api-playability-reaudit.md)及
[原生候选/路由登记](../references/reviews/2026-11-12-api-playability-surface.json)保存本次依据与核对深度。
第一轮[审计](../references/reviews/2026-11-12-five-dlc-coverage-audit.md)和原始身份清单保留历史事实。

原[53单元计划](../exec-plans/active/five-dlc-remaining.md)仍为 **51限定通过、2组合已核**；原21项成果不倒退。
2026-11-12 起按本账本实施：已验成果仍见文末“已核销”，AN-R05源码排除保留。本次[全功能复核](../references/reviews/2026-11-12-five-dlc-full-function-audit.md)发现25族仍有列明选择/终态未完成，见“限定成果仍有待办”；旧证据不扩大为整族完成。无正式 DLL 部署。**范围内后来发现的遗漏是交付缺陷，不能改叫新阶段。**

## 分类与计数

| 类别 | 含义 | 核销条件 |
| --- | --- | --- |
| G | 源码已确认缺少玩家选择映射、必要读回，或现有适配存在错误 | 最小充分实现/修复、同步合同，并证明本行原生终态；不能用 debug 改效果代验 |
| V | 现成 API/通用组合已有表达，尚缺一个独立核心语义的代表运行证据 | 复用已有入口补本行结果；失败后才按原因决定是否改源码 |
| A | 已定位具体玩家操作，但现有调用的实际覆盖仍需判定 | 先核本行明确问题，归类为 G、V、通过或有出处的范围排除 |

每行是**操作族**，不是端点数、剩余 bug 数或完成率。共享实现只记在 S 区；DLC 区交叉引用，不重复计数。
一个操作族内仍需完成列明的选择，不能只验其中一个按钮就核销整行。已有相同原生路径的 Def 不逐个重跑。
旧66行保留ID，新增40个Core族与10个DLC族；S-11、ID-R11由A判为G，BT-R15由A判为V。
AN-R05（CompPushable推格）经源码复核无正常玩家选项，由G改为源码排除，审计ID和116行历史保留；见文末。
A为0只指这三项具体疑问已判定，不表示116行已实现。另有[完整功能域收尾计划](../exec-plans/active/five-dlc-full-function.md)中的7个来源核对包，已交付；E流程代表证据已归并；不混入操作族/端点计数。

下表固定 **2026-11-13阶段核销**，不包含上方2026-11-16重新发现的UI差集，不再用作当前开放数。

| 归属（2026-11-13历史阶段） | G 确认实现/适配缺口 | V 代表验收缺口 | A 待定覆盖 | 合计 |
| --- | ---: | ---: | ---: | ---: |
| Core / 端到端 C-01–41 | 0 | 0 | 0 | 0 |
| 跨 DLC 共享 S-01–11 | 0 | 0 | 0 | 0 |
| Royalty RY-R01–10 | 0 | 0 | 0 | 0 |
| Ideology ID-R01–16 | 0 | 0 | 0 | 0 |
| Biotech BT-R01–19 | 0 | 0 | 0 | 0 |
| Anomaly AN-R01–09 | 0 | 0 | 0 | 0 (+1 排除) |
| Odyssey OD-R01–11 | 0 | 0 | 0 | 0 |
| **总计** | **0** | **0** | **0** | **0 (+1 排除)** |

历史116个唯一ID与55条早期限定成果保留；原85族批次的实际成果见核销表。
后续登记的C-11/C-09/C-17/OD-R09四族已在用户“修复这些缺口”授权下完成并以统一DLL29cd3908…复验；[四族报告](../references/reviews/2026-11-13-api-playability-gap-repair-integrated.md)固定源码/原件/清理与5组保留边界。该阶段登记开放归零保留；本轮当前开放项以页首UI差集为准。此前六项补回成果见文末接续核销。E-01–10按代表证据及其披露范围核对；验收目标与实测均为用户确认的rev600，不把归零当全部功能的百分比分母。
原53/21成果和C-30收尾不倒退。

本页后续详细表保留最初任务定义；**历史操作族状态按“原G/V行减去已核销ID，再加回限定成果仍有待办”计算，当前还须参考页首UI差集的后续核销状态**。
“限定成果仍有待办”的类别与剩余标题优先，G可转为V；核销表只证明其写明的结果。
补完差集后协调者移除对应待办行，并登记新证据；不得仅因旧核销记录存在就从队列删除它。
源码文件简称相对 `Source/RIMAPI/RimworldRestApi/`；原生类型、方法及摘录路径见审计记录和证据身份。

## Core / 开局与端到端公共操作

这些是达成用户最终目标的正常游戏依赖；上一版排除它们会留下必须人工补操作的断点。
S区已有共享实现只交叉引用，不重复计数；每行G可以复用现有API补齐差集。

| ID | 类别 | 剩余操作 / 当前缺口 | 来源、下一步及可读终态 |
| --- | --- | --- | --- |
| C-01 | G | 正常场景选择、编辑与预设 | `Page_SelectScenario/Page_ScenarioEditor`、`ScenPart_*`；`GamePlayHelper.InitGameFromConfiguration`固定Crashlanded。支持正常可选场景/参数、保存载入与编辑取消；准确场景及游戏初始条件读回。 |
| C-02 | G | 世界配置、预生成查询与起始地点选择 | `Page_CreateWorldParams/Page_SelectStartingSite/Dialog_AdvancedGameConfig`；已有seed/气候/coverage/地格开局，缺先生成再查询选址、派系配置/污染等正常字段。补合法地格、地图大小/季节及警告确认；读回同一世界和选址，不要求盲填未知tile。 |
| C-03 | G | 开局人物名单、重掷、自定义异种及其预设 | `Page_ConfigureStartingPawns/Dialog_CreateXenotype/ScenPart_ConfigPage_*`；开局DTO无人物选择。按场景正常限制提供保留/交换/重掷、人物姓名与关系信息、合法异种创建/选用/保存载入，读回实际起始pawn；不以debug生成/任意加属性代替。 |
| C-04 | G | 初次开局的理念选择、新建、配置与人物分配 | `Page_ChooseIdeoPreset/Page_ConfigureIdeo/Page_ConfigureFluidIdeo`；现开局无对应请求。支持正常预设、classic/固定/流动模式及其合法配置、分配/取消；准确理念、成员与点数读回。Archonexus合法重启复用此能力，独有迁移见ID-R11。 |
| C-05 | G | 运行中叙事者、难度、Anomaly和影响玩法的选项 | `Page_SelectStorytellerInGame/StorytellerUI/Dialog_AnomalySettings/Dialog_Options`；当前settings主要读与run-in-background，缺写叙事者/自定义难度、Anomaly playstyle/威胁/研究效率、殖民地数量上限、未来人物偏好姓名等正常玩法选项。查询合法值再按原生应用、持久化读回；纯音画/摄像机排除。 |
| C-06 | G | 存档目录、元数据及开局/保存/载入的真实完成 | 已有game/start/save/load/main-menu；`GameStateService`长事件返回排队不等于完成，GameController无存档目录。补发现准确文件、原生覆盖/承诺模式语义、错误/完成读回和载入后正确Game身份；不用文件系统或人点窗口补流程。 |
| C-07 | G | 派系、殖民地及正常人物命名 | `Dialog_NamePlayerFactionAndSettlement/NamePlayerFaction/NamePlayerSettlement/Dialog_NamePawn`；补原生可选命名/重命名与正常范围校验、准确身份读回。已有pawn改名可复用；出生信的完成副作用归S-11，船命名归OD-R07，不能重复计数。 |
| C-08 | G | 灭团后原生继续、选择新流浪者和恢复经营 | `ChoiceLetter_GameEnded/GenGameEnd/Dialog_ChooseNewWanderers`；旧目录排除，本合同纳入。发现正常可用继续选项，查询/选择合法新人物，读回原生继续状态与人物/殖民地；不造任意复活代替失败恢复。 |
| C-09 | G | 正常商队组建、重组、装载和出发 | `Dialog_FormCaravan/CaravanFormingUtility`；GlobalMap只读caravan。支持人物/动物/货物数量、食物药物自动选择与手选、汇合/出口、取消和重新组建；真实工作出图生成准确caravan，holder/物资一致。 |
| C-10 | G | 商队路线、到达动作及世界地点经营 | `WorldRoutePlanner/Caravan_PathFollower/CaravanArrivalAction_*`、Settle/Abandon；无正常StartPath/进入/攻击/访问/定居/放弃适配。查询路径/资格并选择目的地与动作，涵盖和平会谈、逃生船、站点、聚落/任务目标；读回准确到达/地图/外交/任务结果。普通交易复用已验TradeControl。 |
| C-11 | G | 商队分拆、合并、休息、觅食及成员/装备管理 | `Dialog_SplitCaravan/CaravanMergeUtility/Caravan/WITab_Caravan_*`；现世界只读不表达这些写。支持合法分配数量、休息/移动、forage与世界holder装备/食物药品/喂婴策略与`beCarriedByCaravanIfSick`；读回准确成员/货物/路径/策略，复用政策服务。 |
| C-12 | G | 普通运输舱、穿梭机装载清单与到达动作 | `CompTransporter/CompLaunchable/CompShuttle`；许可机和重力船不等价普通pod。支持编组、人物/物品数量、修改/取消/卸载、自动装载、合法发射地格与trade/gift/caravan/site等到达动作及太空警告确认；准确资源/跨图holder与到达结果。PassengerShuttle专属见OD-R06。 |
| C-14 | G | 正常优先工作、完整任务参数和准确取消 | `FloatMenuOptionProvider_WorkGivers/CleanRoom`、`PriorityWork`、`HumanOvum`；通用PawnJob仅A/Queue，缺队列目标/count/原生工作上下文、准确当前/排队取消。按明确正常工作适配，不开放任意反射；覆盖清整房/附近灭火的原生目标队列、manual work-priority模式、强制优先工作与必要确认、取消准确受精任务与预约清理，真实工作终态读回。其它专用多目标族复用实现，不能各重复算端点。 |
| C-15 | G | 征召战斗策略、开火许可和停止/排队控制 | 征召已有写；缺`Pawn_DraftController.FireAtWill`、`HostilityResponseMode`与机械体`CompTurretGun.fireAtWill`等正常策略。补开关/目标/停止语义、队列和阻止原因，读回准确策略及实际动作；不把一次AttackMelee当持续策略。 |
| C-16 | G | 炮塔/迫击炮/火箭蜂群的瞄准、停火、炮弹和爆破引爆 | `Building_TurretGun/ITab_Shells/CompInteractableRocketswarmLauncher/Building_BlastingCharge`；缺强制目标/清目标/holdFire、弹药过滤/退弹和正常Detonate→StartWick。Rocketswarm的TargetLocation会开原生Targeter，generic interact不能接收落点。支持明确参数直接执行，真实炮弹/冷却/目标/引信和效果读回；不是CompExplosive的DEV瞬爆。 |
| C-17 | G | 准确数量的背包/装备正常拿取、丢弃与给他人穿衣 | `PickUpItem/DropEquipment/DressOtherPawn/ITab_Pawn_Gear/GiveItemsToPawnUtility`；普通地上Equip/Wear已有。缺内部holder物品、count、drop/forced-apparel解除与`ForceTargetWear(A=人,B=衣物)`；补正常工作/确认和读回，不能把直接tracker穿脱当作正常搬运。 |
| C-18 | G | 正常建造资格、取消退款、安装和重新安装 | 现PlaceBlueprints已有建造；误注释称PlaceBlueprintForBuild会检查资格，但原生`Designator_Build.CanDesignateCell→GenConstruct.CanPlaceBlueprintAt`在外层，当前直接生成。补研究/可建造/材料/放置的原生资格，正常取消Blueprint/Frame/unfinished退款，以及minified安装/建筑reinstall/树木重栽的目标与旋转；读回施工/退料/搬运结果，debug paste不代验。 |
| C-19 | G | 认领、收养与连矿选区/取消的原生语义 | `Designator_Claim/Adopt/MineVein`；generic DesignationDef不执行Claim/Adopt业务。支持合法ClaimableBy后SetFaction、收养及其正常信件/床分配，连矿按原生格集合选择/取消（等价查询+designation可复用）。forbid/unforbid、FillIn和普通挖矿/拆除/搬运已有表达；EmptySpace是界面占位，不列缺口。 |
| C-20 | G | 建筑/地板涂装与计划管理 | `Designator_Paint*/RemovePaint/Plan_*`、`Plan`；无正常颜色/染料施工及计划图层创建/修改/复制/删除/颜色接口。按原生合法选项提交，读回涂装/染料消费、准确计划地格与名称；纯显示overlay不算业务。 |
| C-21 | G | 区域、屋顶、Home及自动重建管理 | 已有allowed area设置/指派；GetMapAreas只返回计数，缺准确cells与正常改名/反选/颜色，Home/BuildRoof/NoRoof/IgnoreRoof/清雪/污染清理等区域及自动Home/重建策略。补可发现与编辑/删除，读回原生格集和代表工作；不是任意roof直接改写。 |
| C-22 | G | 已有种植区和建筑种植器的完整管理 | 现FarmHelper主要create，设置植物只处理Zone_Growing。补种植区改作物、`allowSow/allowCut`、扩缩/改名/删除与Building_PlantGrower合法作物；读回准确格集/策略及正常种植。已有钓鱼政策完整表达保留。 |
| C-23 | G | 温控目标与正常升降/重置 | `CompTempControl.TargetTemperature`、`Comp_AtmosphericHeater`；建筑power不等价温控目标，缺设置。查询合法目标并按原生应用，读回设定和实际温度/UnnaturalHeat.heaterTargetTemp；不强改地图最终温度。 |
| C-24 | G | 目标燃料量、自动补给与正常加燃料 | `CompRefuelable.TargetFuelLevel/allowAutoRefuel`；一次Haul/Refuel不覆盖持续策略。支持合法燃料/目标、开关与读回，正常补给遵从阈值，物资真实消耗；设备独有供料开关交叉引用不重复。 |
| C-25 | G | 电源开关的正常Flick工作与电网重连 | `CompFlickable.DoFlick/CompPower.TryManualReconnect`；BuildingHelper当前立即写SwitchIsOn，正常gizmo先改wantSwitchOn后派工。补正常期望/实际开关状态、取消待切换和重连；真实电网/工作读回。保留现直接变更接口独立标识。 |
| C-26 | G | 门/聚会点/照明/陷阱/扫描器等设施正常设置 | `Building_Door.holdOpen/CompGatherSpot.Active/CompGlower/Building_Trap.autoRearm/Building_FirefoamPopper.autoRebuild/CompLongRangeMineralScanner`；缺各正常开关、灯颜色/暗光与目标矿物。按设备可用选项直接设置/读回并核代表自主行为；灯颜色复制可组合，不要求复制纯剪贴板UI。 |
| C-27 | G | 生产账单的全部正常目标/过滤/存放配置 | 现Bill CRUD/暂停/调序已在，保留；`Dialog_BillConfig`可选特定存放区/计数来源组，现DTO SlotGroupId始终null、请求缺group。补具体stockpile存放、计数过滤组、特殊过滤/范围和正常改名/复制等差集；读回准确bill与组、数量及正常产品去处，不把所有bill列为未实现。 |
| C-28 | G | 医疗护理、自我治疗、分类默认、带药及征召医治 | `MedicalCareUtility/HealthCardUtility/Dialog_MedicalDefaults/PawnColumnWorker_Carry/DraftedTend`；现只读medCare/selfTend，tend仅裸A患者。补护理/默认、携药组数量及原生B/C药物/count/draftedTend选择；读回策略、药物消费与实际包扎。 |
| C-29 | G | 手术的准确身体部位和单张医疗账单管理 | `HealthCardUtility/Bill_Medical`；AddSurgery取同BodyPartDef首个，不能选左右实际部位，GET缺part/bill稳定身份；RemoveSurgery删全部同recipe，不能准确删/暂停/调序。补实例选项与合法排程，准确患者/部位/单bill/原生术后结果读回；已验普通手术和胚胎医生链保留。 |
| C-30 | G | 救援、俘虏、逮捕、送床与释放（已限定核销） | `66cbdbd1`及本次收尾：准确患者REST终态、已抱持送床、原生释放、非敌对俘虏外交提示和床/逮捕资格；四模式及新增三阶段证据保留。请求/字段与边界见[合同](../api/core-take-to-bed.md)、[收尾验收](../references/reviews/2026-11-12-c30-closeout.md)。 |
| C-31 | G | 坟墓指派、冬眠舱正常进入/搬入与弹出 | `Building_Grave/Building_CryptosleepCasket/CarryToCryptosleepCasket`；已补 `GET/POST /building/grave*`（assign/unassign）与 `POST /building/cryptosleep/eject`，隔离验证assign/unassign、容器/eject读回。他人搬入倒地pawn的carry job实测未入舱，故本行仍开放。 |
| C-32 | G | 食物政策管理、默认与人物指派 | `Dialog_ManageFoodPolicies/PawnColumnWorker_FoodRestriction`；无运行policy CRUD/filter/default/assign。支持正常复制/命名/删除/过滤与合法成员指派，读回运行实例和自主进食资格；Def列表不等价policy。 |
| C-33 | G | 药物政策管理、规则与人物指派 | `Dialog_ManageDrugPolicies/DrugPolicyUIUtility`；缺CRUD/default/assign及每药物频率、心情/娱乐阈值、定期服用/携带量选择。原生合法值与policy实例读回，正常摄入/库存遵从规则。 |
| C-34 | G | 服装政策创建、过滤、默认与管理 | 现OutfitPolicyService可列出现有政策并指派自由殖民者；缺CRUD/filter/default/复制。按`Dialog_ManageApparelPolicies`补正常值和人物指派后的自主穿衣结果；奴隶拒绝单列ID-R09，不重复算实现。 |
| C-35 | G | 阅读政策管理与人物指派 | `Dialog_ManageReadingPolicies/ReadingColumnUIUtility`；无CRUD/default/defFilter/effectFilter/assign映射。支持准确实例与过滤，正常自主阅读遵从；单A Reading已可下单，不称读书本身完全缺失。 |
| C-36 | G | 动物主人、跟随、放出和指定攻击/取消 | `TrainableUtility/TrainingCardUtility/Pawn_PlayerSettings/Pawn_TrainingTracker`；普通训练已有，缺master/followDrafted/followFieldwork/animalsReleased与Odyssey训练攻击attackTarget/取消。读回准确主人/训练资格/策略和实际动物任务；不以一次AttackMelee代验。 |
| C-37 | G | 畜栏、自动割草和自动屠宰策略 | `CompAnimalPenMarker/ITab_PenAnimals/PenAutoCut/WindTurbineAutoCut/Dialog_AutoSlaughter`；缺动物/植物filter、现在割草/自动割草、畜栏命名、性别年龄数量阈值与pregnant/bonded选择。准确策略读回并由正常handler/割草/屠宰遵从；单次designation不覆盖持续设置。 |
| C-38 | G | 正常派系通信、交易类型请求和援助/信息购买 | `Building_CommsConsole→FactionDialogMaker`；UseCommsConsole必须job.commTarget，generic未写；当前只有直接goodwill和已有商人交易。补合法派系/协商者/设备、商队/轨道商类型、援军与共同敌人确认、AI核心信息费用/冷却；读回goodwill/Silver/request tick及真实到访/任务。继承人复用此入口见RY-R03。 |
| C-39 | G | 世界遭遇、任务和开局业务对话的具体决策 | `IncidentWorker_CaravanDemand/CaravanMeeting/QuestPart_Dialog/ScenPart_GameStartDialog`；缺交货/战斗/交涉等真实回调和确认。结构化发现准确上下文/选项/费用，调用原生业务并读回holder/任务signal/关系；不能把关闭Dialog当选择。任务选择信S-10独立归属、通用消息确认可复用。 |
| C-40 | G | 正常主动求爱及正常社交上下文 | `FloatMenuOptionProvider_Romance/RelationsUtility.GiveRomanceJob`创建TryRomance并写interaction=RomanceAttempt；generic无该字段，ForceInteraction直接TryInteractWith不等价资格/走近/伴侣警告/成功率及冷却上下文。补合法选项与确认、正常job和真实社交/关系/冷却读回，不保证随机成功。 |
| C-41 | G | 机械族 pawn 详情查询空/失败 | 原生 `Pawn.skills` 对机械族为空；`PawnHelper.PawnToDetailedDto` 直接访问 `pawn.skills.skills`，`GET /pawns/details` 对 mechanoid 无法返回完整详情（BT-R14 机械读回时观察）。补原生可空组件遍历，返回机械族可用字段或明确不支持语义；不把 summary 当详情。 |

## 跨 DLC 共享（只计一次）

| ID | 类别 | 剩余操作 / 当前缺口 | 来源、下一步及可读终态 |
| --- | --- | --- | --- |
| S-01 | G | 选择装备/背包的 utility **Verb** 并按人物、物品或地格施放；不是 AbilityDef | 原生 `CompApparelVerbOwner*`、`CompDisruptorFlarePack`；现 `PawnControlService` 的 ability/attack_static 无装备verb选择。补准确装备+verb+目标适配，读回干扰弹、部署炮塔、Deadlife 等实际结果及次数消耗；保留已验 EMPPulse Ability。 |
| S-02 | G | 装备/背包装填、选择兼容弹药 | 原生 `FloatMenuOptionProvider_Reload`→`JobGiver_Reload.MakeReloadJob`；通用 `PawnJobService` 只有 targetA，不能表达装备/弹药队列/count。复用原生资格与job，读回准确装备剩余次数、弹药真实消耗。跨 Royalty/Anomaly/Odyssey。 |
| S-04 | G | 普通银币贸易中的人物/动物、天然器官、封装建筑 | `TradeControlService.cs:379` 的 Item/IsNaturalOrgan/Pawn 过滤明确排除；`Tradeable.AnyThing` 会解包 minified。按原生可交易清单支持各实际类别，读回准确对象/holder/银币变化；帝国贡品另见 RY-R05，不重复。 |
| S-06 | G | 普通床、王座、冥想点、休眠棺的独立指派/取消及床用途 | 原生`CompAssignableToPawn_*`与`Building_Bed`；现仅有部分入睡自动认领/只读，缺独立assign/unassign与colonist/prisoner/slave、medical切换（含动物床）。读回pawn ownership与建筑assignment一致，遵从房间/资格；不开放任意灵魂解绑。坟墓/冬眠舱见C-31。 |
| S-07 | G | 建筑储存/供料过滤、优先级、存储组及复制设置 | 扩为Core货架/书架/坟墓与成长舱/塑形舱/装备架等设备；原生`StorageSettings/StorageGroup/ITab_Storage`，现stockpile zone不等价建筑。支持各正常允许字段/特殊过滤/组关联与命名，准确设置/组/holder读回，代表搬运遵从；独有自动开关见各DLC行。 |
| S-09 | G | 可保护树木的“防止砍伐”开关 | `CompPlantPreventCutting.PreventCutting` 正常gizmo，现无字段写入；与禁止物品、区域不同。写后读回防砍状态及原生砍伐资格；不制造树手动断开接口。 |
| S-11 | G | 出生信命名的正常完成语义 | `ChoiceLetter_BabyBirth→Dialog_NamePawn`接受会写`pawn.babyNamingDeadline=-1`；当前PawnEdit只改Name，不完成命名期限/信件语义。补正常命名接受/合法延后与读回，准确婴儿姓名、deadline和信件状态；不能只改字符串核销。 |

## Royalty

| ID | 类别 | 剩余操作 / 当前缺口 | 来源、下一步及可读终态 |
| --- | --- | --- | --- |
| RY-R01 | G | 灵能热量限制器开关 | `PsychicEntropyGizmo:240–252` 写 `limitEntropyAmount`；现RoyaltyControl只支持targetPsyfocus。补开关与读回，原生施放资格遵从设置；fixture关限不算API支持。 |
| RY-R02 | G | 正常放弃头衔 | `CharacterCardUtility:223–266` 确认后 `royalty.SetTitle(faction,null,grantRewards:false)`；现 `RoyaltyTitleService.Grant` 必须非空TitleDef。补原生放弃及确认语义，读回该派系头衔/权利移除；降级不等于放弃。 |
| RY-R03 | G | 请求/更换头衔继承人 | `FactionDialogMaker:400–477` 选候选人→生成带 holder/newHeir 的 ChangeRoyalHeir任务；当前只有相关只读points，quests/accept无法构造选人请求。补正常请求与后续，读回任务选定对象及原生heir变化；直接SetHeir不代验。 |
| RY-R04 | G | 商队中使用资源投送、穿梭机许可 | 两类 `RoyalTitlePermitWorker_* .GetCaravanGizmos` 有独立世界分支；现PermitService/Execution要求Spawned/map。补商队资源投送（含负重确认）与世界目标穿梭机，读回货物/航程/费用冷却。地图许可样本保留。 |
| RY-R05 | G | 帝国贡品/荣誉币种交易 | 原生 IsFavor/Tribute 路径；现TradeControl要求Silver且排除IsFavor/Pawn。补准确贡品、接收者与原生荣誉结算，读回对象移交及royal favor增量；不以普通商品买卖代验。 |
| RY-R06 | G | 纪念碑标记的材料选择、批量放蓝图、取消纪念碑任务 | `MonumentMarker.GetGizmos:236–311`→AllowedStuffs/PlaceAllBlueprints、MonumentCancelled/清相关蓝图/卸下。现builder可造已知组件但无该任务语义。补标记/草图可发现信息与正常操作，读回匹配蓝图、完成信号或准确取消后果；不调用DEV BuildAll。 |
| RY-R07 | G | 任务奖励偏好：接受荣誉/关系奖励的开关 | `Dialog_RewardPrefsConfig` 写 faction.allowRoyalFavorRewards/allowGoodwillRewards；现无映射。设置后真实字段与后续原生奖励生成偏好一致；任务reward index选择已有，不重开。 |
| RY-R08 | V | 人格武器正常装备→绑定与特性生效 | 原生 `EquipmentUtility.CanEquip/GetPersonaWeaponConfirmationText` 与装备job；现 `ColonistService.MakeJobEquip` 可下Equip。核准确武器完成装备后bondedPawn/bondedWeapon及特性，并核正常确认合同；未证明需新端点，不把普通装备样本当人格绑定结果。 |
| RY-R09 | G | 正常应用Techprint | `CompTechprint`不继承CompUsable，原生ApplyTechprint需要A研究台/B准确印记/C台位置；generic仅A且use-item要求CompUsable。补合法项目/印记数量/台资格发现和完整正常job，真实消耗并增加techprintsApplied；不以Techprof/Transponder代验。 |

关联共享项：背包施放/装填 S-01/02，贸易 S-03/04，王座/冥想指派 S-06，任务选择信 S-10。
Farskip 为本地Royalty唯一 `targetWorldCell=true` 的AbilityDef；现有跨地图/商队Farskip成果保留，不再泛称“世界灵能未覆盖”。

## Ideology

| ID | 类别 | 剩余操作 / 当前缺口 | 来源、下一步及可读终态 |
| --- | --- | --- | --- |
| ID-R01 | G | 运行中教义实例和可编辑选项的可发现读回 | `IdeologyControlService:51–167` 只有计数/部分摘要；DefQuery不等价实例。补操作所需precept_id、类型、ThingDef、角色/服饰/特殊选值和合法选项，使客户端无需fixture硬编码ID便可调用现成移除/替换；不要求全量百科。 |
| ID-R02 | G | 特殊教义添加：服饰、武器贵贱、偏好异种 | `IdeoUIUtility:1603–1807` 正常改革菜单；现 `IdeologyControlService:313–329` 白名单拒绝 Precept_Apparel/Weapon/Xenotype，DTO缺选择。支持原生合法选值、正常Apply，准确读回三类实例值；Xenotype联动只在本行计数。 |
| ID-R03 | G | 已有特殊教义编辑 | `Dialog_EditPrecept`/`Precept_RitualSeat` 允许名称/锁名、角色男女头衔/要求服饰、圣物材料、理念建筑样式、仪式座位种类；现ritual/config、具体Thing改style只覆盖子集。按原生编辑权限适配，读回准确实例及后续服饰要求/建造样式/圣物材料效果。 |
| ID-R04 | G | 添加仪式时选择合法的非基准pattern/信条替代pattern | `IdeoUIUtility:1711–1835`；现 `IdeologyControlService:365–369` 固定 `ritualPatternBase`。补可用pattern选项，读回sourcePattern/behavior/outcome/角色并正常完成所选独立模式；不逐Def重测。 |
| ID-R05 | G | 理念级发型/胡须/纹身frequency/gender、建筑风格槽及classic模式风格库 | `Dialog_EditIdeoStyleItems:700–721`、`IdeoUIUtility:790–805`；人物styling不等价理念策略，现ReformStyle只有add/remove。一次正常改革应用选值与槽顺序，读回策略、其它优先级和points/reform_count；不能循环删加多次消费改革代替。classic模式`Dialog_StyleSelection`直接编辑`IdeoManager.selectedStyleCategories`（最多3槽）也缺写，不消费改革点，应按该独立正常规则覆盖。 |
| ID-R06 | G | 神经超充自动使用模式与允许访客 | `CompNeuralSupercharger:174–191`/`Command_SetNeuralSuperchargerAutoUse`；现 `IdeologySupercharger:55–56` 只读这些值。补NoAutoUse/WithDesire/Everyone及guest开关，读回设置并观察代表自动工作资格；手动超充通过保留。 |
| ID-R07 | G | 生物塑形舱自动营养装载、自动年龄逆转 | `CompBiosculpterPod:531–559` 正常gizmo；现只有enter/eject。补自动开关及资格读回，原生自主供料/有调谐和年龄逆转需求时入舱，准确周期终态；营养filter/priority见S-07。 |
| ID-R08 | G | 他人搬倒地殖民者/囚犯进入指定塑形舱与周期 | 原生 `PrepareCycleJob/MakeCarryToBiosculpterJob` 需患者、pod及cycleKey；现 `IdeologyBiosculpter:183–185` 拒绝downed/非玩家，单target通用job不够。补正常搬入，读回准确患者contained/currentCycle和搬运者脱离抱持；保留药品与原生资格。 |
| ID-R09 | G | 对正常奴隶指派服装政策 | 原生 `PawnColumnWorker_Outfit` 允许有outfits tracker的非任务人物；现 `OutfitPolicyService:17–18` 强制IsFreeColonist。依原生资格开放奴隶，读回CurrentApparelPolicy及实际穿衣；通用工作优先级已有，不把整个奴隶管理列缺。 |
| ID-R10 | G | 运行中理念预设保存/导出 | `IdeoUIUtility:427–455` 正常Save；现无预设序列化接口。正常原生格式保存可读预设，不额外变更当前理念。正常MainTab禁止Load，加载另按下一行限定场景核。 |
| ID-R11 | G | Archonexus合法阶段的理念新建、载入、配置与分配 | `Dialog_ConfigureIdeo:90–104`明确allowLoad:true，166–176允许新建fixed/fluid及配置；现ArchonexusService只选已有IdeoManager ID/keep。复用C-04配置能力，在正常重启阶段直接API提交并读回新理念/成员与迁移；普通运行MainTab仍禁止任意Load。旧迁移/结局通过保留。 |
| ID-R12 | V | 失明等特殊仪式worker、舞蹈及消费型结果 | 通用 `RitualControlService` 无worker白名单；原有仪式样本不等价 `Mutilation/Blinding`、Dance、Skylantern/RemoveConsumableBuilding等独立语义。按确有不同worker/结果选代表，读回对应run完成及眼部/消费/结果；不穷举质量/RNG。 |
| ID-R13 | V | 角色社交能力的真实结果 | `/pawn/ability` 已通用，CombatCommand/PreachHealth已有；Reassure/Counsel社交/思想效果未有代表证据。合法角色/目标下读回真实结果与组冷却；同GiveHediff的普通变体不逐Def重跑。 |
| ID-R14 | G | 仪式完整参与名单、旁观/排除、多人角色及合法离场 | `Dialog_BeginRitual/PawnRoleSelectionWidgetBase/RitualRoleAssignments`允许列表，现Roles是每role一个string并FillPawns，无旁观/排除。补合法名单和`LordJob_Ritual.LeaveRitual`活动中非required角色/旁观者离场，读回准确assignments/pawnsForcedToLeave与终态；整场cancel不等于个人离场。 |
| ID-R15 | G | 一次正常组合改革与叙事锁 | `Dialog_ReformIdeo`用同一draft组合合法meme/structure/style、precepts/身份/外观，Done仅Apply一次；现各分项端点独立Notify_Reformed会多次耗点/+count。补一次提交/取消、descriptionLocked等正常选项，读回所有选值、点数只重置一次且reformCount+1；不开放任意culture。 |
| ID-R16 | G | 非主理念合法仪式和角色 | `SocialCardUtility`按pawn.Ideo，仅leader受primary限制；现Get/FindRitual/AssignRole固定PrimaryIdeo。补准确理念/owned precept发现与选择，正常角色/Lord/结果读回；StartRitual Ability只开窗口且Apply为空，不是REST替代。 |

关联共享项：持续囚犯/奴隶策略 S-05，舱供料 S-07，Gaumaker产物种植 S-08，任务信 S-10。
Precept_Animal/RitualSeat继承ThingDef，现add已经可表达，不列缺实现；原组合已核仍保留。

## Biotech

| ID | 类别 | 剩余操作 / 当前缺口 | 来源、下一步及可读终态 |
| --- | --- | --- | --- |
| BT-R01 | G | 运行中成长舱主动结束/提前弹出，以及胚胎选择取消 | `Building_GrowthVat.GetGizmos:643–667` 正常CancelGrowth→Finish/TryDropAll；现 `LifeControlService:1346` 拒绝Working，错误称无early eject，并要求SelectedPawn。修正适配和合同，准确内容按原生结束/弹出，状态选择清空；胚胎遵从正常确认。 |
| BT-R02 | G | 基因提取器取消选人/取消提取 | `Building_GeneExtractor.GetGizmos:363–402` 有Cancel/清选择；现 `GeneProductionService` 只有Extract开始。补两阶段取消，准确人物移出，Working=false/SelectedPawn=null，无未完成产包。 |
| BT-R03 | G | 基因库/包自动装载、装载名单、单包/全部弹出 | `ITab_ContentsGenepackHolder`/`CompGenepackContainer`/`Genepack` 有autoLoad、leftToLoad、targetContainer和Eject；现只读可装配基因包。补正常设置/搬运和弹出，准确包holder、库内容及待装名单一致。 |
| BT-R04 | G | 基因装配模板保存/加载 | `Dialog_CreateXenogerm:292–323`/`CustomXenogermUtility`；现DTO只有整包/name/worker。补原生模板保存加载，读回整包选择、名称、图标；不要求拆选包内基因。 |
| BT-R05 | G | 装配时自选异种胚芽图标 | 原生CreateXenogerm/GeneCreationDialogBase图标选择；现 `GeneProductionService:455` Start(...,null)。补合法iconDef，成品图标准确、取消不产新成品。 |
| BT-R06 | G | 死亡休眠自动唤醒开关 | `Gene_Deathrest.GetGizmos` 写autoWake；现Life GET只读。补写与资格读回，在既有完整周期基础上证明自动安全唤醒；不用重跑全部外接设备。 |
| BT-R07 | G | 血原质自动摄入阈值与允许血包开关 | `GeneGizmo_Resource.Target`→SetTargetValuePct；Hemogen header写hemogenPacksAllowed。现一次feed/Ingest不覆盖策略。设置/读回准确，自主供给遵从阈值和允许状态；囚犯供血策略见S-05。 |
| BT-R08 | G | 每对照护者/婴儿的自动喂养优先级与允许食物 | `ITab_Pawn_Feeding:145–203`→SetAutofeeder/SetBabyFoodAllowed；现AutofeedBaby只一次job。补Never/Childcare/Urgent及食物选择，读回并有原生自主选择结果。 |
| BT-R09 | G | 次核心扫描器选囚犯/倒地者并搬入；初始化后空载取消 | `Building_Enterable.SelectPawn`/CarryToBuilding、Scanner已init即能CancelLoad；现 `LifeControlService:847–925` 只让患者自己EnterBuilding，未设SelectedPawn，无contents拒绝eject。已补 `POST /dlc/biotech/subcore/select`（含 cancel）并验证选人/空载取消；倒地患者由原生 carrier 搬入的测试未完成，故本行仍开放。 |
| BT-R10 | G | 机械体派系配色（低优先级外观） | `MainTabWindow_Mechs:39–49` 正常ChooseMechColor→AllegianceColor/portrait dirty；现无配色映射。设置、真实派系颜色读回和机械体外观一致；不是阻塞其它机械链的门禁。 |
| BT-R11 | G | 原子分解器自动装载与弹出内容 | `CompAtomizer.GetGizmos:68–90` AutoLoad/EjectContents；现Life只读auto_load和一次haul。补开关/弹出，正常自主haul并消耗准确wastepack，关闭停止后续装载、弹出清容器；既有一次分解通过保留。 |
| BT-R12 | V | 学校正常授课 | `LearningGiver_Lessontaking`/`SchoolUtility` 原生找老师/桌；现建造、Childcare工作/时间表/区域可组合。披露学习欲望前置后，读回师生配对job与learning/skill增长；不造指定随机desire的setter。 |
| BT-R13 | V | 助产/分娩仪式的医生、家属与产床角色 | `Hediff_Labor`→ChildBirth ritual；通用仪式角色/目标已有，自动birth样本不代验助产。读回准确母体/床/Lord角色与原生出生结果；跨Ideology只在此计数，不保证RNG/双胞胎。 |
| BT-R14 | V | 多周期机械培养的再次加工及正常账单停止 | `Bill_Mech`/`Building_WorkTableAutonomous`；现Bill CRUD/suspend和gestator/start/eject/haul可组合，旧Militor仅一周期。选一个多周期机制，真实各周期推进/成品与一次停止；不重测全部机械Def。 |
| BT-R15 | V | 基因提取器囚犯/倒地供体由他人搬入的代表终态 | Extract先设SelectedPawn并提交患者EnterBuilding；`ThinkNode_QueuedJob`会清掉不能启动的downed job及其预约，WorkGiver可据SelectedPawn派carrier，因此不能据此断言永久阻断。现组合补实际carrier→准确供体入舱→提取产包、报告与终态读回；有实际失败再最小修复。 |
| BT-R16 | G | 带宽节点调谐与重调谐 | `CompBandNode.GetGizmos/TuneTo`正常选择mechanitor，供电tick完成；现无对应映射。补合法候选、准确tuningTo/tunedTo/剩余时间与供电资格，原生完成后的带宽hediff读回。 |
| BT-R17 | G | 机械体自动维修策略 | `CompMechRepairable/PawnColumnWorker_AutoRepair`写autoRepair；现RepairMech仅一次工作。补开关/读回，代表正常维修遵从启停；已验一次维修保留。 |
| BT-R18 | G | 战争女王载机补料阈值、资源与释放 | `CompMechCarrier.TrySpawnPawns/MechCarrierGizmo.maxToFill`；战争女王有正常培养recipe，非敌方专属。现Ability/Verb均不覆盖。补正常补料目标、库存/冷却/释放资格、资源消费与准确产出pawn/派系读回，不调用DEV Fill/Reset。 |
| BT-R19 | V | 正常由他人吸收倒地/受控供体的异种基因 | `CompAbilityEffect_ReimplantXenogerm→GeneUtility.GiveReimplantJob`提供受体AbsorbXenogerm(A=供体)路径，现generic单A job可表达；原主动caster植入不代验此独立方向。补合法供体/受体、原生致死警告的显式确认合同与实际吸收/供体/基因结果；有参数/资格差集归共享工作适配修复，不先判整个job缺失。 |

关联共享项：Bloodfeed/HemogenFarm S-05，休眠棺指派 S-06，成长舱供料 S-07，出生/儿童信 S-10/11。
死亡休眠设备自动灵魂绑定、组数随stat自动生成、家庭遗传随机，不另造手动解绑/建组/挑后代基因接口。

## Anomaly

| ID | 类别 | 剩余操作 / 当前缺口 | 来源、下一步及可读终态 |
| --- | --- | --- | --- |
| AN-R01 | G | 知识研究各类别的当前项目/进度与按类别取消 | 原生 `ResearchManager.CurrentAnomalyKnowledgeProjects/GetProject(category)/StopProject`；现ResearchHelper只GetProject(null)/停普通研究。补知识类别读回与准确停止；普通研究及设定研究目标不因此退回。 |
| AN-R02 | G | 物品/巨石/怪异尸体的持续StudyEnabled开关 | `CompStudiable.SetStudyEnabled` 正常gizmo；现Study写入仅用于固定Archonexus对象，收容mode不覆盖此类物品。补正常启停与读回，原生研究工作遵从设置；囚犯Study见S-05。 |
| AN-R03 | G | 样本分析适配的“已下单却报失败” | `CompAnalyzable.OrderForceTarget` 排 **AnalyzeItem**；现 `InteractableControl:200–201` 只认InteractThing，实际启动后返回409。修正实际提交识别/返回，读回准确样本分析、研究解锁/消耗；单target AnalyzeItem对原地分析已有组合，不称完全无法分析。 |
| AN-R04 | G | 死亡拒绝的本人自复活命令 | `Verse.Hediff_DeathRefusal.GetGizmos`→CommandSelfResurrect，消耗次数并倒计时后复活；现只覆盖授予仪式/Ghoul血清/死灵变种，未映射本命令。补原生资格与命令，准确死者复活、次数减少；不用通用debug复活代验。 |
| AN-R05 | 排除 | 推移球体/血肉核心到选定地格（源码复核：无正常玩家选项） | `CompPushable.CompFloatMenuOptions` 要求 `CompProperties_Pushable.givePushOption`；该类字段默认 false，全 Data XML 仅 Nociosphere/FleshmassNucleus 两处组件且均未设置，C#/Def 也无赋值，故1.6.4871无玩家可用的推格菜单；不为凑数制造合成接口。Nociosphere 地格移动由既有 `nociosphere/send` 表达。 |
| AN-R06 | G | 怪异尸体停用必须遵守研究解锁 | `CompDisableUnnaturalCorpse` 正常gizmo/菜单检查Tracker.CanDestroyViaResearch，但继承CanInteract不检查；现generic interact直接调用会绕过这条门禁。补准确资格，未解锁拒绝且尸体仍在，合法解锁后原生毁尸/Shard/折磨解除；已通过其它交互保持。 |
| AN-R07 | V | 迷宫门/刻字及合法研究后的怪异尸体特殊交互 | 已有interactable/pawn job可到原生组件；未有这些独立结果的代表证据。修AN-R06后按正常资格读回对应研究/状态/解除结果；门口下单或打开窗口不算终态。群体跨门货物另见OD-R05。 |
| AN-R09 | G | 收容实体手动生物铁提取开关 | `ITab_Entity`设置`CompHoldingPlatformTarget.extractBioferrite`，WorkGiver/JobDriver均检查；现无写。补启停/准确资格与策略读回，正常提取的物品、切割伤和BioferriteExtracted结果；既有harvester清运是另一语义，成果保留。 |

关联共享项：装备Verb/装填 S-01/02，囚犯Study/Interrogate S-05，Harbinger种植/防砍 S-08/09，相关选择信 S-10。

## Odyssey

| ID | 类别 | 剩余操作 / 当前缺口 | 来源、下一步及可读终态 |
| --- | --- | --- | --- |
| OD-R01 | G | Cerebrex最终核心：摧毁/搜刮/暂缓选择及真实结局 | `CompCerebrexCore.OpenDialog/StartCoreDeactivation/DeactivateCore`；generic交互至多开Dialog_NodeTree，没有业务选项。补三稳定器门禁与原生选择，摧毁读回CoreDefeated/核心替换/机械派系停用，搜刮读回CerebrexNode与credits，暂缓不启动；不是Archonexus结局。 |
| OD-R02 | G | 装备架允许搬入/移出策略及指定衣物入架 | `Building_OutfitStand.GetGizmos/SetAllowHauling`、PutApparelOnOutfitStand需衣物+架；现PawnControl只读contents、非空时swap，不能装空架；swap也未采用原生菜单关闭hauling的策略。补操作与结果，包含单件衣物弹出后forbidden状态，准确衣物holder、允许状态和后续工作一致；过滤见S-07，既有衣物/武器swap保留。 |
| OD-R03 | G | 已训练动物的自动觅食/挖掘开关及实际工作 | `PawnColumnWorker_AnimalForage/AnimalDig.SetValue` 写playerSettings；现training SetWantedRecursive只管学技能。补工作允许开关，读回原生自主Forage/Dig及产物，关闭停止后续；训练进度样本不等于已开启工作。 |
| OD-R04 | G | 可破解设备自动Hack策略 | `CompHackable.GetGizmos`→AutoHack；现只有一次Hack或地标任务接口。补autoHack与读回，原生工作遵从策略；具体锁定设备结果见OD-R09，不制造dev-only即时解锁。 |
| OD-R05 | G | MapPortal多人/物品清单装载、等待及取消 | `Dialog_EnterPortal`/MapPortal.leftToLoad/MakeLords/CancelLoad；单pawn EnterPortal不能表达多人货物队列。补合法transferables、逐项减少/修改待装count与取消，准确人物/物品跨入口、原图holder清空/取消后不继续装载；Anomaly深坑入口同族只记一次。 |
| OD-R06 | G | PassengerShuttle地图/世界商队持有时的旅行与货舱补燃料 | `Building_PassengerShuttle.GetGizmos` 正常cargo fuel slider；现 `RoyaltyTransportService:999–1004` 只允许permitShuttle旅行，拒绝此型。核现成登乘/装载后补正常目的地/货舱补给适配，读回准确ship路线、人物/货物到达及燃料消耗；许可机/重力船不代验。`Caravan:977–1025`持有Shuttle的再次起飞和从caravan inventory消耗燃料也须覆盖；VisitSpace警告需API确认。 |
| OD-R07 | G | 重力船/PassengerShuttle命名与重命名（低优先级外观） | `Building_GravEngine:74–84,454–457` 的正常命名dialog，以及 `Building_PassengerShuttle` 的IRenameable；现服务无RenamableLabel写入。补合法名称与读回，名称可由原生检视显示；不把覆盖范围/底板overlay等纯显示工具列实现缺口。 |
| OD-R08 | V | 重力船盾发生器正常Activate/Deactivate | `CompInteractable_GravshipShieldGenerator` 已继承通用CompInteractable，可在五DLC开启时调用现anomaly/interactable；尚缺代表终态。证明实际盾状态与正常供电/资格；该API硬依赖Anomaly的独立DLC兼容问题另记边界，不凭路由所属误判五DLC场景无表达。 |
| OD-R09 | V | 锁定舱门/箱、古代上行终端与机械继电器稳定器 | 原生AncientHatch/CompHackable/AncientUplink/MechRelay；现单target Hack、Open、战斗可组合。按独立结果选代表：合法开锁→可通行/落物，上行破解→scanner-tag任务，稳定器失效→原生坠落/Gravcore；不以已验未锁Open箱代表全部，也不逐地标Def跑遍。 |
| OD-R10 | V | 单人进入/返回及正常封闭入口 | `EnterMapPortal`/`CompSealable`→单target EnterPortal/Seal可表达；需核封闭的被困人物提示/确认是否现合同覆盖。准确人物到达对应图、合法封口状态及返回资格读回；若发现确认缺口才最小补适配，不先称整个Seal无法执行。 |
| OD-R11 | V | 初次检查重力引擎→解锁/认领/任务推进 | `Building_GravEngine.GetFloatMenuOptions`→InspectGravEngine/Inspect；现pawn/job可表达，起降样本引擎已检查。补从未检查前置开始的原生job，读回gravEngineInspected、faction及任务signal；不重跑既有起降。 |

## 限定成果仍有待办

当前无待办覆盖行。原登记两条实现差异、三条核心验证缺口已完成，
见[统一DLL验收](../references/reviews/2026-11-13-api-playability-gap-repair-integrated.md)。
原登记B-01–B-05及加载过渡响应等实测边界保留，不等同新增未实现操作族；发现具体独立语义缺陷时归原ID。

## 已核销（本轮）

每行记录已取得的限定成果、提交、调用、对象、终态和边界；本表保留历史证据。
同ID若出现在“限定成果仍有待办”，其剩余选择仍开放；其它ID才从当前队列减去。

| ID | 实现提交 | 调用/准确对象 | 独立终态 | 证据与未测边界 |
| --- | --- | --- | --- | --- |
| C-11 | `4c780cc4`、统一DLL29cd3908… | 原生CloseToEachOther/MergeCaravans；普通/单船/双船/近距离不同tile | 精确人员/货物/船fuel及owner保持；target空Shuttle缓存正确更新；未确认/dry/超距不写入、源404 | [四族集成](../references/reviews/2026-11-13-api-playability-gap-repair-integrated.md)，G-01/G-02闭合；两船均原生保留，近距离路径进度为披露前置；baby/arrival边界保持 |
| C-17 | `9c580092`、统一DLL29cd3908… | ForForceWear目标资格/同图reach；正常dress_other | 362tick新Duster75479进Human318 apparel/forced，旧Jacket75478落地，Human315释放；不可达dry/real409 | 同报告V-01闭合，准确公开GET/原生holder；fixture只布前置，不扩大为囚犯/倒地/复杂layer全部实跑 |
| C-09 | `fc8f768b`、`02f644db`、统一DLL29cd3908… | 无派系地图发现、原生dialog质量、正常retained reform | 两人+Steel20/Silver10→caravan277，地图/遭遇移除，moving99861恢复，预览/终态11.68/70 | 同报告V-02闭合，全程暂停，fixture原生入遭遇前置；部分选择/多遭遇/途中重载未测 |
| OD-R09 | `dbd7811f`、`eb62a35a`、统一DLL29cd3908… | 三次正常Hack→真实QuestPart/原生失活/7500tick坠落→公开AttackMelee | 第三台才唯一All信号；准确残骸/Gravcore1/Panel165/slag4，独立公开读回 | 同报告V-03闭合；破解实例防度缩短50倍，Quest接线前置披露，未自然取得站点/守卫战/后续袭击 |
| AN-R03 | `1b3c0623` | `GET /dlc/anomaly/interactable?thing_id=&pawn_id=` 读 `analysis`；`POST` 同路径（GrayFleshSample，分析者 Human321） | 200/`applied:true`/`job_def:AnalyzeItem`；推进后 `AnalysisManager.times_done` 0→1，`destroyedOnAnalyzed` 样本从地图消失 | 隔离 `g35_deathresting` 副本（tick76787，五DLC）`fix3-first-three.json`；未测研究台模式（`canStudyInPlace=false`）及其它 analyzable Def |
| AN-R01 | `88688143` | `GET /dlc/anomaly/knowledge`；现有 `POST /research/target?name=BioferriteExtraction`；`POST /dlc/anomaly/knowledge/stop` | 正常巨石激活0→1后候选 `can_start_now:true`/`is_hidden:false`；target 200 后 GET `current_project` 准确；stop dry 200 保留、正式 200/`after_project:null`、再次409 | 隔离 Xvfb g35 副本（tick76787）`an-r01-r02.json`；未测知识进度增长/项目完成/多类别并行/EntityCodex逐项发现 |
| AN-R02 | `88688143` | `GET/POST /dlc/anomaly/study`（WarpedObelisk_Mutator，等级0/1） | 等级0 `ever_studiable:false` 且 POST 422；正常巨石激活后 `ever_studiable:true`，dry `applied:false`、on `study_enabled:true`/`currently_studiable:true`、off 均 false、未知 404 | 同上，无 debug 字段写；未测平台/囚禁/研究台特异资格与自动 Study 完整周期 |
| AN-R08 | `f89b0a9a` | `GET/POST /pawn/ability`（MetalbloodInjection自施法、VoidTerror目标Villager；fixture授予两能力和视线内目标） | Metalblood hediff/冷却14388；VoidTerror自标422、目标TerrifyingHallucinations/充能5→4/冷却6930；补caster排除规则 | 隔离 Xvfb g35 副本 `an-r08-20261112/evidence/an-r08.json`；非全Def清单，未测AoE/确认框/授予来源 |
| AN-R07 | `f4dbe875` | `GET /dlc/anomaly/interactable` 的 `labyrinth_door`/`floor_etching`；`POST .../interactable`（GrayDoor+FloorEtchingRambling，fixture） | 门：3600tick 后 jammed false/hold_open true/PawnCanOpen true；刻字：2500tick 后 deciphered true/216字消息；GET同值 | 隔离 Xvfb g35 副本 `an-r07-20261112/evidence/an-r07.json`；尸体门禁见AN-R06；未测墙内门/多门并发/刻字方向 |
| AN-R04 | `96c47cfe` | `GET /dlc/anomaly/death-refusal?pawn_id=`；`POST .../death-refusal/resurrect`（fixture授予hediff并杀死登记殖民者） | dry 200/`applied:false`；正式 200/`applied:true`/uses1→0/`in_progress:true`；推进后 dead false、spawned、DeathRefusalSickness；重复422 | 隔离 Xvfb g35 副本（tick76787）`an-r04-r09.json`；未测墓穴/容器尸体、二次使用链、仪式授予路径 |
| AN-R09 | `96c47cfe` | `GET/POST /dlc/anomaly/containment/extract-bioferrite`；`POST .../containment/work` action=extract（Gorehulk，Doctor） | 研究/采集器门禁；dry保持false，on 200/applied；work 200/`job_def:ExtractBioferrite`；推进后 `BioferriteExtracted`、地图生物铁0→12、≥3伤势；关开关后422 | 同上；未测断电/多实体/无医生逐项 |
| AN-R06 | `1b3c0623` | `GET` 读 `native_gate`/`unnatural_corpse`；`POST /things/set-forbidden` 取消默认禁用；`POST /dlc/anomaly/interactable`（准确尸体+另一健康殖民者 Human315） | 未解锁 POST 422 且尸体/Shard 不变；debug 解锁（已披露）后 200/`applied:true`，尸体销毁、Shard 0→1、CorpseTorment 移除、tracker 注销、haunted 存活 | 同上；解锁为 fixture DEV 前置，正常研究累积到阈值未单独重跑；其它交互/研究变体保留 AN-R07 |
| BT-R10 | `b8772b72` | `GET/POST /dlc/biotech/mech/color` | #00BCD8→#FF8800精确回读；dry保持；非法hex400；mechs_marked_dirty0 | 隔离 Xvfb g35 副本 `bt-r10-20261112/evidence/bt-r10.json`；未测真实机械体肖像 |
| BT-R17 | `b8772b72` | `GET/POST /dlc/biotech/mech/auto-repair`（Mech_Scyther） | false→true→false，GET/fixture一致 | 隔离 Xvfb g35 副本 `bt-r17-20261112/evidence/bt-r17.json`；未测实际维修周期/材料消耗 |
| BT-R07 | `d66d46be` | `GET /dlc/biotech/life` + `POST .../hemogen/policy`（Hemogenic基因） | target0.5/packs true→0.25/false→0/true，GET一致；1.5→400 | 隔离 Xvfb g35 副本 `bt-r07-20261112/evidence/bt-r07.json`；未测自主摄取/血包消费/囚犯供血 |
| BT-R06 | `dd412f4c` | `POST /dlc/biotech/life/deathrest/auto-wake`（已有死亡休眠殖民者） | false→true→false；GET同值；deathresting保持true | 隔离 Xvfb g35 副本 `bt-r06-20261112/evidence/bt-r06.json`；未测完整安全唤醒周期/无基因拒绝 |
| BT-R01 | `1b3c0623` | `POST /dlc/biotech/growthvat/cancel`（`mode` 自动区分；胚胎需 `confirm`） | Working 人物取消 200/`cancel_growth`/working false/contained 空；胚胎选择取消 200/`cancel_load`、`implant_target:null` 且胚胎保留；胚胎入舱未妊娠未确认 409，`confirm:true` 后 200 并清空 | 同上；`Finish()` 为原生私有方法，经 Harmony `AccessTools` 固定调用；未测妊娠完成点出生、营养耗尽/断电/多舱 |
| RY-R09 | `80ba99e4` | `GET /dlc/royalty/techprint` + `POST .../techprint/apply`（fixture研究台+CataphractArmor蓝图） | dry保持；job ApplyTechprint；1500tick后蓝图消失、techprints_applied 0→1、项目未完成 | 隔离 Xvfb g35 副本 `ry-r09-20261112/evidence/ry-r09.json`；未测第二张完成项目/研究台拒绝 |
| RY-R02 | `17abe64a` | `GET /dlc/royalty/title` + `POST .../title/relinquish`（正常授予Freeholder准备） | Freeholder→null；dry保持；GET无头衔/权利。 | 隔离 Xvfb g35 副本 `ry-r02-20261112/evidence/ry-r02.json`；未测真实授予仪式/许可逐步撤销/后续思想 |
| RY-R07 | `22d0a33e` | `GET/POST /dlc/royalty/reward-prefs`（有头衔阵营） | 两开关取反后GET一致；dry保持；随后恢复 | 隔离 Xvfb g35 副本 `ry-r07-20261112/evidence/ry-r07.json`；未测后续任务生成偏好/多阵营 |
| RY-R01 | `71c4c1b3` | `GET /dlc/royalty/meditation` + `POST .../limiter`（fixture psylink1） | true→false→true，GET/fixture一致；dry保持 | 隔离 Xvfb g35 副本 `ry-r01-20261112/evidence/ry-r01.json`；未测超限施放/不同等级 |
| BT-R08 | `e4609be8` | `GET/POST /dlc/biotech/baby/feeding`（fixture age0 Baby+Milk） | Urgent后GET/observer一致；allowed false读回false；Never恢复 | 隔离 Xvfb g35 副本 `bt-r08-20261112/evidence/bt-r08.json`；未测真实自主喂食/消费 |
| C-25 | `16eb4f1e` | `GET/POST /building/power-switch`（fixture StandingLamp） | desired false→designation→job切off；true→on；cancel清designation；reconnect 200 | 隔离 Xvfb g35 副本 `c25-20261112/evidence/c25.json`；未测断网重连成功/途中取消 |
| C-29 | `ee5f058e` | `GET/POST /pawn/surgery` + `options` + `bill`（fixture双侧RemoveBodyPart） | body_part_id 5/6两账单；bill_id暂停/上移；DELETE仅删一张 | 隔离 Xvfb g35 副本 `c29-20261112/evidence/c29.json`；未重跑手术执行 |
| S-07 | `ccda772f` | `GET/POST /storage/settings`（fixture Shelf/Silver） | priority Preferred→Low、Silver拒绝、accepts false；group member_count1 | 隔离 Xvfb g35 副本 `s07-20261112/evidence/s07.json`；未重跑实际搬运/全建筑类型 |
| S-05 | `f3dc353e` | `GET/POST /pawn/guest-policy`（fixture player prisoner） | Convert exclusive、Bloodfeed non-exclusive、slave Suppress读回 | 隔离 Xvfb g35 副本 `s05-20261112/evidence/s05.json`；未跑实际工作/停用后行为 |
| C-27 | `1ed2ecc0`、`3455bf4f` | `POST/PUT` 账单组字段；`POST/PUT /buildings/bill*` 的 `ingredient_special_filter(_allowed)`、`ingredient_min/max_hit_points_percent`、`ingredient_min/max_quality`（fixture Stockpile+Shelf/命名组、FueledStove+CookMealSimple） | 旧：slot_group_id→SpecificStockpile/Stockpiles、storage_group_id→Groups、include_group_id；新：`AllowRotten` 禁、HP 0.4..0.9、quality Good..Legendary 读回，`allowed_materials` 保持，成品 range 不变；未知 404/非法 400 | 组证据 `c27-20261112/evidence/c27.json`；过滤证据 `int-c27-20261112-run2/evidence/c27-integration.json`；[RunGiver 源码路径] `WorkGiver_DoBill` 使用 `bill.ingredientFilter.Allows`；未长跑实际生产/运输 |
| S-09 | `de5cd015` | `GET/POST /plant/cut-protection`（fixture Anima树+designation） | true清designation；false→can_cut；重加后true再清；未知404 | 隔离 Xvfb g35 副本 `s09-20261112/evidence/s09.json`；未重跑实际砍伐 |
| C-32 | `e8398b81` | `GET/POST /food/policy*` + `POST /pawn/food-policy`（fixture Human321/Beer） | 创建policy11；指派current11；Beer false/true；在用删除409；改回默认后删除 | 隔离 Xvfb g35 副本 `c32-20261112/evidence/c32.json`；未长跑自主进食 |
| C-33 | `c2482053` | `GET/POST /drug/policy*` + `POST /pawn/drug-policy`（fixture Human321/Beer） | 创建policy5；指派current5；joy/scheduled/2.5天/mood0.4/joy0.5/携带3读回；在用删除409；改回默认后删除 | 隔离 Xvfb g35 副本 `c33-20261112/evidence/c33.json`；未长跑自主服药 |
| C-34 | `b142e043` | `GET/POST /outfit/policy*` + `POST /pawn/outfit`（fixture Human321/ShieldBelt） | 创建outfit7；指派读回7；ShieldBelt false/true；在用删除409；改回默认后删除 | 隔离 Xvfb g35 副本 `c34-20261112/evidence/c34.json`；未长跑自主换装 |
| C-35 | `b52efe5f` | `GET/POST /reading/policy*` + `POST /pawn/reading-policy`（fixture Human321/TextBook） | 创建policy8；指派current8；TextBook false/true；在用删除409；改回默认后删除 | 隔离 Xvfb g35 副本 `c35-20261112/evidence/c35.json`；未长跑自主阅读/effect filter |
| S-11 | `1461e7c4` | `GET/POST /pawn/baby-name*`（fixture generated baby+birth letter） | deadline136787→delay196787；命名后-1/naming_open false/letter消失 | 隔离 Xvfb g35 副本 `s11-20261112/evidence/s11.json`；未重跑真实出生信全生命周期 |
| C-36 | `9bc08c39` | `GET/POST /animal/settings`（fixture Bear_Grizzly+Silver） | Obedience true；master/follow/released/attackTarget读回；缺失master422 | 隔离 Xvfb g35 副本 `c36-20261112/evidence/c36.json`；未跑真实跟随/攻击，Attack训练项未出现 |
| C-37 | `d78e91f7`、`de2fe4d2` | `GET/POST /pen/policy`+`/autoslaughter`+`building/facility`（fixture 封闭畜栏/WindTurbine） | 旧：name/auto_cut/filter、max1/pregnant/bonded；新：封闭 pen `cut_now` +9 designation、未封闭 422；autoslaughter dry 不建 config、正式 `max_total=3`；WindTurbine auto_cut 开关、`auto_cut_now` +4 designation | 旧 `c37-20261112/evidence/c37.json`；集成复跑 `int-c37-20261112-run3/evidence/c37-integration.json`；未长跑自主割草/屠宰派工 |
| S-06 | `3f75a2f9` | `GET/POST /building/assignable`+`/bed`（fixture Bed/Throne/MeditationSpot） | Bed assign/unassign/medical/ForPrisoners读回；Throne/MeditationSpot assign；未知404 | 隔离 Xvfb g35 副本 `s06-20261112/evidence/s06.json`；未跑实际睡眠/王座/冥想；休眠棺见C-31 |
| C-26 | `07ad79f7`、`35cabb83` | `GET/POST /building/facility`：旧 door/campfire/lamp/trap/scanner；新 `firefoam_auto_rebuild`（fixture 补研究/Home 并生成玩家 popper） | 旧：hold_open/gather/glow+darklight/auto_rearm/MineableGold；新：默认 true、false/true 读回、dry 不写；关闭销毁无蓝图；开启销毁同格原生 `Blueprint_FirefoamPopper` | 集成复跑 `int-c26-20261112-run4/evidence/c26-integration.json`；未跑火灾触发/真实建造/持续行为 |
| C-40 | `db0b6a98` | `GET/POST /pawn/romance`（fixture unrelated pair） | job后PlayLog出现romance attempt及拒绝；dry不写；校验血亲/既有恋爱 | 隔离 Xvfb g35 副本 `c40-20261112/evidence/c40.json`；随机接受/关系成立未追求 |
| RY-R08 | `be7bc3b5` | 既有 `POST /jobs/make/equip`（fixture PlasmaSwordBladelink） | 装备后primary与bondedWeapon均为该武器；CanEquip异常回退Equip job | 隔离 Xvfb g35 副本 `ryr08-20261112/evidence/ryr08.json`；未跑UI确认与全部trait效果 |
| BT-R04 | `23958597` | `GET /gene/template`、`POST .../save`、`POST .../load`（fixture通电assembler单包） | 保存模板index0；GET genes；load匹配Genepack75486/all_matched true；未知index404 | 隔离 Xvfb g35 副本 `bt-r04-20261112/evidence/bt-r04.json`；未测跨存档持久化/重名/多assembler部分匹配 |
| BT-R05 | `bf4d0856` | `POST /dlc/biotech/gene/assemble` 的 `icon_def`（fixture通电assembler+单基因包） | Basic→Start CreateXenogerm，iconDef Basic；dry保持；取消无新Xenogerm；未知icon400 | 隔离 Xvfb g35 副本 `bt-r05-20261112/evidence/bt-r05.json`；未测成品图标渲染/其它Def |
| BT-R11 | `ab34d8d7` | `GET /dlc/biotech/waste` + `POST .../waste/atomizer/policy`（fixture原子分解器4包） | autoLoad true→false；eject后count0/empty/地图4；空字段400 | 隔离 Xvfb g35 副本 `bt-r11-20261112/evidence/bt-r11.json`；未测自主haul/atomize/断电 |
| BT-R16 | `cc555a3b` | `GET /dlc/biotech/band-node` + `POST .../band-node/tune`（fixture通电节点+mechanitor） | Untuned→Tuning(300ticks)→Tuned(Human321)+BandNode hediff；dry保持；同pawn409 | 隔离 Xvfb g35 副本 `bt-r16-20261112/evidence/bt-r16.json`；未测第二mechanitor重调/断电/带宽数值 |
| BT-R03 | `e101a8d4`、`c5966909` | `GET/POST /dlc/biotech/gene/bank`：旧 `auto_load`/`eject`；新 `load=pack|cancel`、`pack_auto_load`，`GET` 增 `packs[]`/`left_to_load_details[]`（fixture 通电 GeneBank 两包+Haul1 殖民者） | 旧：autoLoad true→false，单包/全部弹出落地；新：`load=pack` 后原生 haul（CarryGenepackToContainer tick78050；入柜 tick78056）、`cancel` 清 target/队列、单包 `auto_load=false`+库自动开 600t 未入柜 | 隔离 Xvfb g35 副本 `bt-r03-20261112-run4/evidence/bt-r03.json`；集成构建复跑 `int-bt-r03-20261112/evidence/bt-r03-integration.json` 终态两包入柜（runner 中间 carry 采样未命中，属观察窗口）；[源审计](../references/reviews/2026-11-12-five-dlc-biotech-source-audit.md)；未测断电/满容量/多库竞争/中途取消 |
| BT-R02 | `0dceca8d` | `GET /dlc/biotech/gene/extractor` + `POST /gene/extract/cancel`（fixture通电提取器+供体） | 已选取消→selected null/job Clean；进行中取消→working false/contained空；dry保持 | 隔离 Xvfb g35 副本 `bt-r02-20261112/evidence/bt-r02.json`；未测取消后再完整提取/断电自动取消 |
| ID-R01 | `957a66c0` | `GET /dlc/ideology/precepts?ideo_id=`（Boccin Creed id13） | 45 precept/ID唯一；Funeral id696/roles；RitualSeat id700/Pew；显式ideo同集 | 隔离 Xvfb g35 副本 `id-r01-20261112/evidence/id-r01.json`；只读发现，未穷举子类/具体改革 |
| ID-R07 | `96fce2bd` | `GET /dlc/ideology/biosculpter` + `POST .../biosculpter/policy`（fixture通电pod） | true/false→false/true→恢复true/false，GET一致；dry保持 | 隔离 Xvfb g35 副本 `id-r07-20261112/evidence/id-r07.json`；未测自动供料搬运/年龄逆转入舱/周期完成 |
| ID-R09 | `16b2b1b5` | `GET /pawn/outfit`、`GET /outfits`、`POST /pawn/outfit`（奴隶Human75478） | Anything→Worker；dry changed true不写；正式后GET Worker；恢复Anything | 隔离 Xvfb g35 副本 `id-r09-20261112/evidence/id-r09.json`；未测实际自主换衣/借宿者拒绝 |
| ID-R06 | `66efaace` | `GET /dlc/ideology/supercharger` + `POST .../supercharger/policy`（fixture玩家超充台） | AutoUseWithDesire/false→NoAutoUse/false→AutoUseForEveryone/true，GET一致；非法模式400 | 隔离 Xvfb g35 副本 `id-r06-20261112/evidence/id-r06.json`；未测供电/充能自主使用/客人资格 |
| C-30 | `4c5de16f`、`66cbdbd1`及收尾修复 | `GET/POST /pawn/take-to-bed`，四模式、非敌对俘虏、已抱持与release | 准确患者GET独立于actor job；入床/guest/holder；落地后spawned且carrier空；提交/释放事件与重放/拒绝 | 七阶段新回归；[收尾验收](../references/reviews/2026-11-12-c30-closeout.md)记录DLL/fixture/失败及边界；未穷举Def/RNG/跨图 |
| OD-R01 | `1adc3dd6` | Cerebrex postpone/scavenge/destroy | 延后保持；原生停用后产物/销毁核心/机械派系停用 | `od-r01-20261112/evidence/od-r01.json`；[合同](../api/odyssey-cerebrex.md)；稳定器战斗为fixture前置，未重验 |
| OD-R02 | `42a45067` | 装备架搬入/移出与衣物 | hauling开关、Put正常入架、eject落地 | `od-r02-20261112/evidence/od-r02.json`；[合同](../api/odyssey-outfit-stand.md)；具体边界沿原证据 |
| OD-R03 | `f8b65c2a` | 动物forage/dig策略 | 觅食后食物增长、挖掘矿HP下降、关闭读回 | `od-r03-20261112/evidence/od-r03.json`；[合同](../api/odyssey-animal-work.md)；训练为fixture前置 |
| OD-R04 | `f8b65c2a` | autoHack开关 | 准确设备开关及正常Hack完成，其它设备不变 | `od-r04-20261112/evidence/od-r04.json`；[合同](../api/odyssey-hackable.md)；不代表全部设备变体 |
| OD-R07 | `f8b65c2a` | IRenameable命名 | PassengerShuttle两次改名、标签一致、非法长度拒绝 | `od-r07-20261112/evidence/od-r07.json`；[合同](../api/odyssey-renameable.md)；GravEngine阈值/存档重载未测 |
| OD-R08 | `e65356b1` | 盾发生器原生交互 | Activate/Deactivate与盾状态读回 | `od-r08-20261112/evidence/od-r08.json`；[合同](../api/odyssey-gravship-shield.md)；供电为fixture前置 |
| OD-R09 | `dbc936ed` | AncientUplink自动破解 | 完成Hack后准确轨道任务可查询 | `od-r09-20261112/evidence/od-r09.json`；[合同](../api/odyssey-uplink.md)；锁箱/稳定器独立结果未重跑 |
| OD-R11 | `77d38538` | InspectGravEngine | inspected false→true、玩家派系、正常任务生成 | `od-r11-20261112/evidence/od-r11.json`；[合同](../api/odyssey-grav-engine-inspect.md)；未重复已有起降/其它任务后续 |

| BT-R09 | `bf38e6d5` | `GET/POST /dlc/biotech/subcore`（init 读私有 `initScanner`；已 init 空载 CancelLoad）；`POST .../subcore/select`（fixture 两扫描器/材料/倒地患者） | init false→WaitingForIngredients；空载 eject→Inactive/init false；select 倒地 Human318 后原生 `CarryToBuilding` tick77594 由 Human315 抱起、tick77600 Occupied；ripscanner 仅 occupant 要求 confirm | 集成复跑 `int-bt-r09-20261112/evidence/bt-r09-integration.json`（phase passed）；未测囚犯患者（同 WorkGiver/carry 路径）、ripscanner 空载取消、断电/扫描完成 |
| ID-R02 | `d825e31e`、`de34e545` | `POST /dlc/ideology/reform/precept/add` 的 `apparel_def`/`noble_weapon_class+despised_weapon_class`/`xenotype_def`；`GET precepts?include_add_options=true` | Boccin Creed 45→48；三次正常改革 points→0、reform_count 0→3；id753 `Apparel_CowboyHat`、id755 Melee/Ranged、id757 Baseliner；非法 pair 422、重复服饰/异种 409 未耗改革 | 集成复跑 `int-id-r02-20261112/evidence/id-r02-integration.json`（phase passed）；未测自定义 xenotype、服饰性别编辑（ID-R03）、偏好异种心情后果 |
| RY-R03 | `7f917533`、`679121b3` | `GET/POST /dlc/royalty/heir`（原生 `RoyalHeirChangeCandidates`；Slate `ChangeRoyalHeir` autoAccept） | Acolyte holder Human321；quest Ongoing、heir 仍 Human315；重复 409；原生 `MonumentCompleted` 后 current_heir Human318、part done；self 422/no-confirm 400/unknown 404/Freeholder 409 | 集成复跑 `int-ry-r03-20261112/evidence/ry-r03-integration.json`（phase passed）；未测囚犯候选/其它可继承头衔/纪念碑超时；正常建造由 fixture 直接完成（归 RY-R06） |

| ID-R03 | `84c1f23f` | `GET /dlc/ideology/precept/edit-options`；`POST /dlc/ideology/precept/edit`（名称/锁名、领导男女头衔、角色服饰增删、圣物材料、建筑样式、`Precept_RitualSeat` 座位种类） | id13 45 precept：领导732→War Chief/War Mother、Moralist733→Test Cleric+lock、Burka→Cape、建筑736→Hindu_IdeogramA、圣物738 Gold→Silver、座位700 Pew→KneelSheet；after/GET/原生 fixture 一致；points 0/reform_count 0 不变；422/400 拒绝 | 集成复跑 `int-id-r03-20261112/evidence/id-r03-integration.json`（phase passed）；未测 ritual anytime/reward/date、restrictToSupremeGender、classic 风格库（ID-R05）、非主理念（ID-R16） |
| RY-R04 | `b0b11172`、`71f36bf3` | `GET/POST /dlc/royalty/permit/caravan`：`FillCaravanAidOption` + 私有 `CallResourcesToCaravan`；穿梭机按 `GetOptionsForTile`/`Launch` 建 `TravelingShuttle` | SteelDrop dry 不写、超重无确认 409、正式 Steel150→400、last_used=76787、free:false/not_enough_favor:true；穿梭机地图格501，dry 不销毁，正式销毁商队并载 Human321/315+Steel400/Silver100 到 tile200，1200tick 到达且货物不变 | 集成复跑 `int-ry-r04-20261112/evidence/ry-r04-integration.json`（phase passed）；未测地图/定居点落点、多到达选项、其它投送Def、完整40天冷却 |

| RY-R05 | `c4832e04`、`ed1b95ac` | `GET/POST /dlc/royalty/tribute`（仅 `Tradeable_RoyalFavor` 币种商人；原生 `TradeSession/TradeDeal/GainFavor`） | 400 Gold：dry 投影6/favor0；正式 accepted，favor 0→5、map gold 900→500、Freeholder/favor5；未知东西 404、零数量 400、无 confirm 400；projected 与实际 favor_delta 分列 | 集成复跑 `int-ry-r05-20261112/evidence/ry-r05-integration.json`（phase passed）；未测奴隶贡品（S-04）、礼物模式、其它 favor 商人、多组混合、更高头衔连续升级 |

| ID-R04 | `fffd7a3e` | `POST /dlc/ideology/reform/precept/add` 的 `ritual_pattern`；`GET precepts?include_add_options=true` 的 `ritual_patterns` | `Festival`+`CelebrationSkyLanterns` id753；正常改革一次 points0/reform_count1；anytime 启动精确实例，7502 tick 原生完成（terminal 84289），3 名参与者获 `UnimpressiveSkyLanterns`；非法跨组 422 | 集成复跑 `int-id-r04-20261112/evidence/id-r04-integration.json`（phase passed）；未测其它 pattern 组、日期义务路径、结果质量档位 |
| ID-R05 | `56e646ee` | `POST /dlc/ideology/reform/style` 的 `style_items`（Hair/Beard/Tattoo frequency+gender）与 `style_categories`（0..3 槽/顺序/priority）；`classic_style_categories` 在 `IdeoManager.classicMode` 直写；`GET precepts?include_style=true` | `BriefPonytail Frequent/Female`、`Princess Never/Any`、`["Hindu","Morbid"]`；points0/reform_count1；classic 无改革；4 槽 400 | 集成复跑 `int-id-r05-20261112/evidence/id-r05-integration.json`（phase passed）；未测胡须/纹身单类样本与 C-04 创建界面 |

| RY-R06 | `fdfdfd33`、`f65f9d0c` | `GET/POST /dlc/royalty/monument`：marker/缺失/蓝图/`AllowedStuffs`，`place_blueprints` 调私有 `PlaceAllBlueprints`，`cancel` 发 `MonumentCancelled`+删蓝图+Uninstall | 真实 ChangeRoyalHeir marker 120 实体；多材料 409、dry 不写、正式 blueprint 0→120/missing 120→0（46 Steel+74 地形，frame 0）；取消后 minified、蓝图0、列表0、重复 404；另建墙 marker Blueprint_Wall stuff=Steel | 集成复跑 `int-ry-r06-20261112/evidence/ry-r06-integration.json`（phase passed）；未测成品建造/完成/被毁/超时/逐实体材料 |
| BT-R12 | `41dd454e` | V 组合现 API：teacher Childcare=1、child 24h Anything、教师 24h Work、BTSchool 区域限制 child；fixture 生成 7 岁 Child、Lessontaking desire、SchoolDesk | run10：tick76842 child Lessontaking 与 Human318 Lessongiving 同 Desk 配对；tick76884 taught_skill=Animals；702t learning 0.300→0.3069、Animals XP+61.69、教师 Social+101.75 | 集成复跑 `int-bt-r12-20261112/evidence/bt-r12-integration.json`（phase passed）；未测黑板/多学生/desire 随机/课桌实际建造/睡眠进食打断 |

| RY-R08 | `82f4446a`、`16d89443` | `GET/POST /dlc/royalty/persona`：原生 `EquipmentUtility.CanEquip`/`GetPersonaWeaponConfirmationText`；未确认 400，确认后只排原生 `JobDefOf.Equip`；通用 `POST /jobs/make/equip` 对 bladelink 返回 409 并指向本端点 | PlasmaSwordBladelink/NoPain：GET `can_equip/requires_confirmation:true` 且文本含 Painless；未确认 400、dry 不排 job；正式 job Equip，4000t 后 primary/coded/bonded 均为该武器且 NoPain hediff 在；再读 already_bonded；他人 422 bonded to someone else | 集成复跑 `int-ry-r08-20261112/evidence/ry-r08-integration.json`（phase passed）；[合同](../api/royalty-persona-equip.md)；未测其它 Def/全部 trait/biocoding/击杀效果 |

| C-28 | `a932f1a8`、`2c6bb08b` | `GET/POST /pawn/medical*` + `POST /pawn/medical/tend` 的 `drafted_tend`（fixture Cut 患者/MedicineIndustrial/征召医生） | 默认/个体/带药既有证据保留；征召：调用前 drafted true、tended false；`drafted_tend:true` 200；4000t 后 tended true、medicine 再减1、医生仍 drafted 并回 Wait_Combat | 集成复跑 `int-c28-20261112/evidence/c28-drafted-tend.json`（phase passed）；[记录](../references/reviews/2026-11-12-c28-drafted-tend.md)；未重跑非征召带药，未穷举药物/多人/中断 |

| RY-R10 | `1b6c1122`、`d992ace3` | 复用 `GET/POST /dlc/ideology/ritual` + status；fixture `prepare_throne/observe_throne/observe_speech` | Praetor+已指派合法王座（81 格、impressiveness119、要求全 met）；ritual 724 dry 不建 Lord、正式 started/Lord2/ticks_left5000；运行中 LordJob_Joinable_Speech 且 titleSpeech=true；5001t 后 completed，Human315/318 各得 UninspiringSpeech(-4)；Speech 冷却 total=1200000/remaining=1199001 | 集成复跑 `int-ry-r10-20261112/evidence/ry-r10-integration.json`（phase passed）；[合同](../api/royalty-throne-speech.md)；未测 LeaderSpeech/其它头衔/中断/冷却到期 |

| S-07 | `ccda772f`、`int-s07` | `GET/POST /storage/settings`：`group_id` 加入已有组、`leave_group`、special/HP/品质与未截断标记；fixture 两 Shelf | 原：priority Preferred→Low、Silver 拒绝、create 组 member_count1。新：dry 加入不改；正式 member_count2；Steel 允许+AllowRotten 禁止+HP0.5–0.8+Normal–Masterwork 读回；离组后原生 ≤1 成员解散、两 Shelf group null；重复离组400、未知组404、互斥400 | 集成复跑 `int-s07-20261112/evidence/s07-integration-4.json`（phase passed）；[记录](../references/reviews/2026-11-12-s07-storage-groups.md)；未重跑实际搬运/全建筑类型 |

| BT-R13 | `cb90e574`、`int-bt-r13` | V 组合现仪式 API：`GET/POST /dlc/ideology/ritual` + status，fixture 母体/医生/伴侣/产床；共享 `LordJob_Ritual_ChildBirth.RitualFinished` hook 修复 | tick77862 labor 无自动 Lord；dry would_start；live start 200 applied/started，Lord_2 stage0 target Bed75487、mother Human75478/doctor Human75484、Lover Human75481 participant；max stage3、母体床位预约 true、医生 CarryToMomAfterBirth；tick96643/集成复跑 97450 新生 Human75799，母体 PostpartumExhaustion/Lactating、labor 清除，婴儿 health1.0+Parent；status completed=true、native_cancelled=false | 集成复跑 `int-bt-r13-20261113/evidence/bt-r13-integration.json`（phase passed）；[记录](../references/reviews/2026-11-12-biotech-bt-r13-childbirth.md)；未测双胞胎/死产/婴儿疾病/母体死亡/取消/存读档 |

| ID-R08 | `4ecd4c4b`、`int-id-r08` | `POST /dlc/ideology/biosculpter/enter` 可选 `carrier_pawn_id`，复用原生 `MakeCarryToBiosculpterJob`/`PrepareCycleJob` 与材料选择；GET pod 读回 | 倒地殖民者 Human318→pleasure、囚犯 Human321→bioregeneration 均 Occupied/occupant_contained/current_cycle 正确，搬运者释放（途中 carrying_pawn_id 实测）；材料 8→6；未研究/缺材料/囚犯自助 422 且不改状态；两次 eject 恢复 SelectingCycle | 集成复跑 `int-id-r08-20261113/evidence/id-r08-integration.json`（phase passed）；未测越狱/暂住者/客人囚犯/biotuned/medic/ageReversal/自然完成/多 Pod/float menu |

| ID-R10 | `d7a822d9`、`int-id-r10` | `GET /dlc/ideology/presets`（原生 `AllCustomIdeoFiles`+`SaveFileInfo`，仅列不 apply）；`POST /dlc/ideology/presets/save`（`GameDataSaveLoader.SaveIdeo`，默认主理念/可选玩家派系 ideo_id、原生清洗、显式 overwrite、dry_run、不加载） | g35 副本 tick76787：新建 `.rid` 48368B，响应 hash=磁盘；原生列表 1.6.4871 readable，`TryLoadIdeo` 解析 Boccin Creed 与实时一致且未应用；重复保存 409 且文件 hash 不变，显式 overwrite 成功；缺名 400/未知 ideo 404/非整数 400；run2 passed，run1 KeyError 现场保留 | 集成复跑 `int-id-r10-20261113/evidence/id-r10-integration.json`（phase passed）；[合同](../api/ideology-presets.md)；未测预设加载应用（主 Tab 禁止 Load），未把列表当 apply |

| C-19 | `int-c19` | `GET/POST /building/claim`、`GET/POST /pawn/adopt`、`POST /map/mine-vein`；原生 `ClaimableBy`/`AdoptableBy`/`Designator_MineVein` | AncientSingleBed claimable true→SetFaction PlayerColony；无派系 Baby adoptable true→SetFaction/IsColonist，重复 400；2x2 MineableSteel 连矿 +4 designation、取消 -4；dry 不写；未知 404、坏 map 404 | 集成复跑 `int-c19-20261113-run2/evidence/c19-integration.json`（phase passed）；未测收养信件/床分配的实际 UI、长时采矿/搬运；fixture 先 `prepare_baby` 再清派系并生成远古床/矿脉 |

| OD-R05 | `62d352e1`、`int-od-r05` | `GET/POST /dlc/odyssey/portal`（MapPortal leftToLoad/MakeLords/CancelLoad）；fixture 原生 InsectLairEntrance + Steel50/Silver40 + 2 pawn | dry would_load；正式 load 建 Lord、left_to_load 30/20；set_count 30→10；cancel 清空且不继续装载；reload 50/40 有界推进到 pocket_map_exists true（tick79806，pocket_pawn_ids 原生昆虫群）；2 pawn 跨入口 | 集成复跑 `int-od-r05-20261113/evidence/od-r05-integration.json`（phase passed）；未测 Anomaly 深坑入口/敌对/中途打断；合同计数 355 |

| OD-R06 | `f60c7504`、`int-od-r06` | `GET/POST /dlc/odyssey/shuttle`：地图/商队/在飞三形态；load/cancel_load/refuel/launch；原生 `CompLaunchable.TryLaunch`/`CaravanShuttleUtility.LaunchShuttle` | 地图 2 pawn+Chemfuel200/Steel100、mass181.6/500、pilot true、fuel100→250、launch tile137060 fuel→200；商队276 持 shuttle fuel200+Chemfuel50+Steel100，冷却→0、refuel 200→250；再起飞 71341→商队278 fuel200/Steel100/2 pawn，caravan_destroyed+TravellingTransporters277；cancel_load dry/正式 | 集成复跑 `int-od-r06-20261113/evidence/od-r06-integration.json`（phase passed）；[合同](../api/odyssey-shuttle.md)；未测地图/定居点落点、屋顶/天气/无飞行员拒绝、商队战斗/合并、跨层/存读档 |

| BT-R14 | `16488469`、`int-bt-r14` | V 组合现 API：LargeMechGestator + 原生 `Bill_ProductionMech(Scyther,2 cycles,RepeatCount1,DropOnFloor)`；work-tables/bills、gestator/haul/start、PUT bill suspend | tick80012 Forming 冻结 600t；resume same bill；tick104848 cycles1 Preparing；tick107116 cycle2 Forming；tick131361 Formed cycles2（Scyther+2 Wastepack）；tick132765 native collect→Mech_Scyther76255，overseer Human75478、6 bandwidth used2；bill Reset/cycles0/repeat0；再810t 无新周期 | 集成复跑 `int-bt-r14-20261113/evidence/bt-r14-integration.json`（phase passed）；fixture 补 Mechlink/MechFormfeeder 6 以容纳 120000t 周期并披露；未测断电/多 gestator/其它 recipe |

| BT-R15 | `1dae6514`、`int-bt-r15` | V 组合：`GeneProductionDtos.carrier_id` + `GeneProductionService` 对囚犯/倒地供体设 `SelectedPawn` 并由显式/最近 carrier 排原生 `CarryToBuilding` | dry carry=true/carrier Human315/job CarryToBuilding；正式 Human315 实际搬 Human75478（carried_thing 读回）；tick77024 contained/working ticks29998；tick107034 原生 Finish→Genepack75939 (RobustDigestion/Hands_Pig)，供体 XenogermLossShock/Replicating，extractor working=false/contained=[]；donor job history 无 EnterBuilding；不可达 carrier 422 | 集成复跑 `int-bt-r15-20261113/evidence/bt-r15-integration.json`（phase passed）；fixture 原生捕获 OutlanderRoughPig 囚犯+Anesthetic 已披露；未测无 carrier_id 自动派工/倒地殖民者/断电/死亡/取消/多提取器 |

| C-41 | `3550f73f` 后 lead 修复、`int-c41` | `GET /pawns/details` 空组件安全：`PawnHelper.PawnToDetailedDto` 对 needs/skills/playerSettings/relations 判空，`PawnInfoService` 在 DTO 仍空时明确失败；fixture 原生 Scyther | 机械族 Scyther 200：`work_info.skills:[]`、energy50、medical health1.0/hediffs[]、mood_details.available=false；不再 success-with-null；普通殖民者路径未重跑（旧证据保留） | 集成复跑 `int-c41-20261113-run2/evidence/c41-integration-2.json`（phase passed）；未测动物/机械战斗/库存查询；修复只覆盖空组件读回 |

| BT-R18 | `f8f77e2f`、`int-bt-r18` | `GET /dlc/biotech/mech/carrier` + `POST .../carrier/autofill` + `.../carrier/release`：原生 `CompMechCarrier`/`TrySpawnPawns`；autofill 写 maxToFill；release 先 CanSpawn | Warqueen75482 overseer Human75478（5/6 bw），container30 Steel、max_to_fill600/autofill570/max_can_spawn1；autofill dry 不变、601→400、正式→100（autofill70）；tick77243 HaulToContainer 补到100（autofill0/max_can_spawn3）；release dry would_spawn3、正式 WarUrchin75502/75505/75508、ingredients100→25、cooldown900；立即二次 release 409 “Cooldown 15.0 seconds”；GET 25/3ids/900/can_spawn false | 集成复跑 `int-bt-r18-20261113/evidence/bt-r18-integration.json`（phase passed）；fixture 生成 warqueen 并改写原生 container 为30 steel（披露）；未测非玩家 carrier/自关机/战斗自动释放/多 carrier/冷却到期/存读档 |

| OD-R09 | `504b0e5d`、`int-od-r09` | `GET/POST /dlc/odyssey/locked`：CompHackable/HackableDoor/IOpenable crate/JammedDoor/CompMechRelay；POST 原生 `JobDefOf.Open`/`InteractThing`，dry/confirm；旧 Uplink 证据保留 | AncientBlastDoor locked→hacked/pawn_can_open；AncientSealedCrate open→44 GravitePanel 落地；GrayDoor force_open→jammed false；MechRelay active→原生 Deactivate→destabilization 94781/剩余7476→MechRelay_Crashed75742；stabilizer Hack 进度 0.006→0.037 | 集成复跑 `int-od-r09-20261113-run2/evidence/od-r09-integration.json`（phase passed；首跑 gray door 采样失败保留）；未测 AncientSecurityTerminal/lockout/任务点/守卫袭击；fixture 保留原生 QuestPart 接线并直接 Deactivate 验证链路 |

| ID-R13 | `ab360174`、`int-id-r13` | V 复用 `POST /pawn/ability`：Cleric role/assign 后 Reassure/Counsel/PreachHealth；生产代码零改动 | Reassure Human321 certainty 0.447569→0.7377491（0.2×NegotiationAbility 1.4509）；Counsel Human315 成功，Counselled+10 抵消 AteRottenFood−10；PreachHealth Brain severity0.5，InjuryHealing 1.0→1.25、Immunity 0.7→0.875；首施后 180000t Moralist 组冷却，GET 读回、POST 409 | 集成复跑 `int-id-r13-20261113/evidence/id-r13-integration.json`（phase passed）；fixture 两次施放间 ResetCooldown 以一次会话读全三效果（披露）；未测冷却到期/其它 Def/精神状态/异理念/囚犯目标 |

| BT-R19 | `6d1cfbf5`、`int-bt-r19` | `POST /dlc/biotech/gene/reimplant`：原生 `GeneUtility.GiveReimplantJob`（A=供体）；合法供体/受体、原生致死警告确认 | 临时 replicating donor→409 confirmation_required（原生警告文本），genes 不变；移除探针 hediff 后 dry 200 lethal_warning=false job AbsorbXenogerm；正式 tick76865 完成：受体 Human315 变 Sanguophage+30 xenogenes+XenogerminationComa；供体保留 30 genes 并得 XenogermLossShock/Replicating；gene/details 一致 | 集成复跑 `int-bt-r19-20261113/evidence/bt-r19-integration.json`（phase passed）；fixture 生成 Sanguophage donor+Anesthetic、临时 replicating 探针（披露）；未测 prisoner donor/confirm=true 致死/同-xenotype 或 ideo 拒绝/重复吸收/存读档 |

| OD-R10 | `72768404`、`int-od-r10` | `POST /dlc/odyssey/portal` 新增 `enter`（原生 EnterPortal playerForced）与 `seal`（原生 Seal）；GET 追加 has_sealable/sealed/is_pocket_exit/entrance_id；修 dry-run GetOtherMap 生成 pocket 的副作用 | 初始 enterable/sealed false；enter dry 不生成；正式 EnterPortal→pocket map1、Human321 在内；CaveExit is_pocket_exit/entrance_id/enterable；exit enter→target_map_id0、Human321 回 map0；seal dry 不写、正式 Seal→portal404、map_count2→1（pocket 销毁）；终态 tick79983 | 集成复跑 `int-od-r10-20261113/evidence/od-r10-integration.json`（phase passed）；未测多人分批/封口时 pocket 内仍有 pawn 或物品/AncientHatch 非销毁型/其它 portal/封口后存读档 |

| BT-R06 | `f49b3d7c`、`int-bt-r06` | V 复用 `GET /dlc/biotech/life` + `POST .../deathrest/auto-wake`，验证原生 `Gene_Deathrest.TickDeathresting` 分支 | fixture 选已 deathresting 的 Deathrest-gene Human66736，native deathrestTicks=239200、autoWake=false；推进 239905 仍 deathresting（0.9996）不早醒；auto-wake dry/formal 后仍 deathresting；跨 240000 tick77584 原生 Wake→deathresting false/ticks0/hediff removed/notifiedWakeOK，无 `/deathrest/wake` 调用；GET 读回 | 集成复跑 `int-bt-r06-20261113/evidence/bt-r06-integration.json`（phase passed）；fixture 把 ticks 移到 Min 前 800t 以缩短 4 天周期（披露）；未测完整周期/中断/手动 wake/无 gene/囚犯拒绝/长期副作用/存读档 |

| BT-R07 | `168775e5`、`int-bt-r07` | `GET/POST /dlc/biotech/hemogen(+policy/feed)`；policy 按原生夹取校验（1.0→0.9/target_clamped，不再409）；GET hemogen_value 0..1 | run1：原生 Ingest tick77448→consumed78366，0.05→0.24947，血包销毁/地图2→1，无 /feed；packs=false 2500t 无 Ingest；value0.6>target0.3 不取包；1.5→400 | 集成复跑 `int-bt-r07-20261113/evidence/bt-r07-integration.json`（phase passed）；fixture 加/选原生 Hemogenic 并置 value/血包（披露）；未测囚犯供血(S-05)/血包提取/其它增减/存读档 |

| ID-R14 | `52990a91`、`int-id-r14` | `POST /ritual/start` 增 role_assignments/spectators/exclude；`POST /ritual/leave` 复现 Leave ritual；eligibility/running 增 roster/participants/spectators/non_participants/extra_required/forced_roles | Festival697：显式 leader Human318+旁观 Human321+排除 Human315，读回一致；leave Human321 200 left、owned2→1、130t 未回场；leave required leader 422；required 对照 native lost→call-off terminal76935 interrupted；Conversion709 两角色读回一致、required convertee leave 422、整场 cancel terminal76982 cancelled；未知 role/重叠/organizer 400 | 集成复跑 `int-id-r14-20261113/evidence/id-r14-integration.json`（phase passed）；fixture 无顶 RitualSpot/改 1 人 ideo 为测试脚手架、debug invuln；未测 organizer leave/最后参与者/required 死亡或崩溃/leave 后存读档/同 role 多 pawn/modded |

| BT-R08 | `a72e2845`、`int-bt-r08` | 复用 `GET/POST /dlc/biotech/baby/feeding`；未改 API；文档补自主喂养与 baby spawned 前置 | fixture Baby75478+照护者 Human321、food1%、20 BabyFood、关闭自由殖民者 Childcare；Urgent+允许：tick76800 BottleFeedBaby(targetB 该堆)，tick80970 food0.01→0.11（上限88%）、堆20→18、不 hungry；Never 2500t 无 job/无变化；唯一食物 allowed=false 2500t 无 job | 集成复跑 `int-bt-r08-20261113/evidence/bt-r08-integration.json`（phase passed）；未测 Childcare 低优先级/多照护者或婴儿/囚犯客人婴儿/其它 babiesCanIngest/母乳/存读档 |

| ID-R15 | `f9ae2027`、`int-id-r15` | `POST /dlc/ideology/reform/combined`：一份原生 `Dialog_ReformIdeo` 草稿（MakeIdeo+CopyTo），Done 只 Apply 一次；choose-one/change-any/fluid meme 规则；confirm 才应用 | A：memes_add=MaleSupremacy+BriefPonytail+precept Cannibalism_Classic+name+叙事锁；dry 无变化；apply points10→0、reform0→1、single_apply、读回一致（自动补 required precept）。B：style Hindu+remove precept696+adjective+description_locked=false；points0、reform1→2、takeNameFrom 级联移除[696,751]。负例两组 choose-one 400、未知 meme404、无变化409；culture/foundation 未变 | 集成复跑 `int-id-r15-20261113/evidence/id-r15-integration.json`（phase passed）；fixture 每次顶满 fluid points（测试脚手架）、invuln；未测 structure-meme choose-one/combined 专用 precept/deity/symbol/同改革多 meme/确认 UI |

| AN-R01 | `d929fdf6`、`int-an-r01` | 复用 `GET/POST /dlc/anomaly/knowledge(+stop)`；原生 monolith activate + DarkStudy | monolith 0→1；BasicPsychicRituals 0→5（DarkStudy 1.82/1.96/2.26，Advanced→Basic overflow），progress5/5 后 slot 清空；Advanced 合法起并写 0.427，Basic 保持空；全 StudyEnabled off+monolith 关→600t 无 StudyInteract/progress0 | 集成复跑 `int-an-r01-20261113/evidence/an-r01-r02-integration.json`（phase passed）；未测同时两类别/EntityCodex 逐项/平台囚禁研究/其它 studiable |

| AN-R02 | `d929fdf6`、`int-an-r01` | 复用 `GET/POST /dlc/anomaly/study`；原生 WorkGiver StudyInteract | off→currently_studiable false、无 job/增长；enable→真实 StudyInteract、类别增长；post ticks_til_next_study≈119920–119977（120000 频率）；GET 四项 study_enabled true | 同上证据；未测平台/囚禁/其它 studiable |

| AN-R03 | `d929fdf6`、`int-an-r03` | 复用 `GET/POST /dlc/anomaly/interactable`；原生 AnalyzeItem（A=sample/B=bench/C=pos） | 仅 shipped canStudyInPlace=false 的 SignalChip+SimpleResearchBench+MechlinkImplant；非 mechanitor422、dry200、real200/AnalyzeItem；targets A/B/C(160,105)；times_done0→1/satisfied、StandardMechtech.analyzed_completed0→1，chip 保留，repeat422 already analyzed；三 biosignature Defs 均 canStudyInPlace=true（源码事实） | 集成复跑 `int-an-r03-20261113/evidence/an-r03-integration.json`（phase passed）；未测其它 bench analyzable/MultiAnalyzer/range>1 |

| AN-R08 | `d929fdf6`、`int-an-r08` | 复用 `GET/POST /pawn/ability`；原生 Ability 效果 | ReleaseDeadlifeDust gas73–180→PlayerColony Shambler（corpse consumed）；PsychicSlaughter 尸体毁/Meat_Twisted0→30；CorrosiveSpray AcidBurn+9 filth/目标死；RevenantInvisibility PsychicInvisibility 出现后过期；HellcatBurner charges2→1→0、ash3→23、三发409；IncineratorBurner 同 FireSpew/Burner 源组 | 集成复跑 `int-an-r08-20261113/evidence/an-r08-integration.json`（phase passed）；debug invuln 分段开关、paved arena/stunned animals 披露；未测正常能力获取/多目标 RNG/确认文本 |

| BT-R11 | `3d399bda`、`int-bt-r11` | 复用 `GET /dlc/biotech/waste` + `POST .../atomizer/policy`；原生 WorkGiver haul/DoAtomize | autoLoad=false 2500t 无 HaulToAtomizer/content0/地图4包；true 后 tick80677 原生 job，tick82375 content4/散落0；shorten ticks 后 tick84373 DoAtomize content4→3/总包4→3；GET 一致，无 /waste/atomize | 集成复跑 `int-bt-r11-20261113/evidence/bt-r11-integration.json`（passed）；未测断电/满容量/污染/多台/存读档 |

| BT-R16 | `a51ac415`、`int-bt-r16` | 复用 `GET /dlc/biotech/band-node` + `POST .../tune`；原生 TuneTo | A Tuning300→tick77092 Tuned，A hediff/+1/带宽6→7；重调 B 返回 Retuning(tunedA/tuningB/180000)，fixture 置 300 后 tick77416 Tuned B：B+1/带宽7，A hediff 经原生60t recache 移除/带宽回6；GET 一致 | 集成复跑 `int-bt-r16-20261113/evidence/bt-r16-integration.json`（passed）；未测断电/死亡离图/多节点/存读档 |

| BT-R17 | `ce3185ff`、`int-bt-r17` | 复用 `GET/POST /dlc/biotech/mech/auto-repair`；原生 RepairMech | Scyther + AddDirect Cut10 + 能量100；false 2500t 无 RepairMech/损伤10/能量99.87；on 后 tick79600 原生 RepairMech，tick82066 治到0/health100%/能量96.44；再加损伤后 off 2500t 无新 job；无 /mech/repair 调用 | 集成复跑 `int-bt-r17-20261113/evidence/bt-r17-integration.json`（passed）；未测在途中断/缺部位或多机械/低能量/存读档 |

| ID-R16 | `f854988b`、`int-id-r16` | 复用 ritual GET/status/start + role/assign；新增可选 `ideo_id`（玩家派系 minor），绑定 ritual 所属 ideo；无新增路由 | GET ritual?ideo_id=8：is_primary=false、RoleChange454 可 anytime、默认仍 ideo13；RoleChange dry selection467/无 leader 选项，start running.ideo_id=8、原生终态 completed/actual_role467 ResearchSpecialist、Human321 current_role467；role/assign ideo8 precept465 Human315 200 changed/清除200；负例多持清除409/minor leader422/跨 ideo pawn422/status 不匹配404；修 RoleState null Abilities/ChosenPawns 500 | 集成复跑 `int-id-r16-20261113/evidence/id-r16-integration.json`（passed）；fixture 改 3 名自由殖民者 ideo 并 Notify_ColonistChangedIdeo（测试脚手架）、invuln；未测未 tracked ideo/minor leader/逐 pawn unassign/quality<0.5/多 minor 并存/save-load |

| C-36 | `5e39b697`、`int-c36` | `GET/POST /animal/settings`（端点未变）；`attack_learned` 改查 Odyssey `AttackTarget`（旧 Attack 永远 false）；`attack_target_id` 走原生 AnimalAttackTarget gizmo；新增 `clear_attack_target` | follow：FollowClose targetA=Human315，10→4.12 格；关 follow_drafted+解除征召后 900t 无新 FollowClose。attack：dry 不写；正式 action_started/attack_target/enemy_target=Muffalo75479，AttackMelee targetA=目标；掉血窗口临时关无敌，health1.0→0.84947。cancel：target 清空/job 结束，900t 无新攻击（health 0.84947→0.85045 自然波动<0.001）。负例两字段同给400、未学 AttackTarget 422 | 集成复跑 `int-c36-20261113/evidence/c36-integration.json`（passed）；fixture 生成训练完成 Bear/master/野生目标、draft 前置、掉血窗口关无敌（披露）；未测 released/释放半径/followFieldwork/敌对目标/多动物/目标死亡/存读档 |

| C-04 | `124d917a`、`int-c04` | 新 `GET /game/start/ideo/options`；`POST /game/start` 可选 `ideo`（classic/preset/fixed/fluid）；正常开局页原生路径，无 debug ideo | fixed：C04 Fixed/Structure_Animist/MaleSupremacy+Supremacist/Hindu，45 precepts，3 殖民者同 ideo；缺 structure_meme 400 未开局。classic：Astropolitan classic_mode=true。preset：Individualist + 原生补 Structure_Archist。fluid：C04 Fluid fluid=true/points0；四局 game/state+ideology+development 读回一致 | 四次独立隔离主菜单启动 `int-c04-*`（fixed/classic/preset/fluid）均 passed；fixture 只读回/暂停；未测 Load(ID-R11)/precept UI/deity/symbol/多样式顺序/classic 各派系差异/地图生成失败恢复 |

| ID-R06 | `2131babc`、`int-id-r06` | 复用 `GET/POST /dlc/ideology/supercharger/policy`；原生 CompRechargeable/JobGiver_GetNeuralSupercharge | 隔离台 power_on=false 600t recharge_ticks 仍60000；有电台同窗口递减约59467t 达 charged。AutoUseWithDesire 空闲 Human321 600t 无 hediff/台仍 charged；AutoUseForEveryone 后原生 job→脑部 hediff/last_received142379/charged=false/ticks59941。客人 Human76384：WithDesire+allowGuests=true 不用（无 desire）；Everyone+allowGuests=false 不用（拦截）；allowGuests=true 后自动用第二台（hediff/last_received148067/ticks59889）；未调 /supercharger/use | 集成复跑 `int-id-r06-20261113/evidence/id-r06-integration.json`（passed）；fixture 供电/lodger 为脚手架、原生充能与 JobGiver；debug invuln；未测掉电/EMP/flicked/breakdown/其它 guest 类别/囚犯奴隶/30000t 冷却/存读档 |

| C-32 | `baa21896`、`int-c32` | 复用 `GET/POST /food/policy*`、`/pawn/food-policy`；原生 JobGiver_GetFood/FoodUtility | 双禁用无 Ingest/两堆10/20不变；只允许 Pemmican→原生 Ingest Pemmican，饥饿0.15→0.4、Pemmican20→0、MealSimple 保持10；只允许 MealSimple→Ingest MealSimple，饥饿0.15→1.0、MealSimple10→9、Pemmican 0 | 集成复跑 `int-c32-20261113/evidence/c32-integration.json`（passed）；fixture 优先级0/喂饱其余殖民者（测试准备）；未测默认槽长期/其它食物/特殊过滤/婴儿囚犯访客/营养膏/植物/狩猎 |

| C-33 | `baa21896`、`int-c33` | 复用 `GET/POST /drug/policy*`、`/pawn/drug-policy`；原生 JobGiver_MoveDrugsToInventory | Beer allow_scheduled=false；take_to_inventory=0→无 TakeInventory/物品栏0；=3→原生 TakeInventory Beer 0→3；再设0并丢下→无再次拾取/0 | 集成复跑 `int-c33-20261113/evidence/c33-integration.json`（passed）；未测 scheduled/joy 实际摄入/成瘾硬药意愿/容量/库存搬运/其它药物 |

| C-34 | `baa21896`、`int-c34` | 复用 `GET/POST /outfit/policy*`、`/pawn/outfit`；原生 JobGiver_OptimizeApparel | 原生 CowboyHat 穿上；政策禁用→RemoveApparel/worn false；允许→giver Wear（belt_in_storage true、apparel_score_raw0.172）、重新穿上 | 集成复跑 `int-c34-20261113/evidence/c34-integration.json`（passed）；用帽子避开远程武器对 ShieldBelt 的 -1000 源码事实；未测品质/HP/特殊过滤/奴隶/生物编码/多殖民者 |

| C-35 | `baa21896`、`int-c35` | 复用 `GET/POST /reading/policy*`、`/pawn/reading-policy`；原生 JoyGiver_Read/LearningGiver_Reading | 允许 Novel→think tree 选中 Reading/Novel；禁 Novel defFilter→giver job_available false/未读；TextBook 强制 Shooting 且 AllowBookShooting→Reading/TextBook；禁 effectFilter→giver false | 集成复跑 `int-c35-20261113/evidence/c35-integration.json`（passed）；未测儿童 LearningGiver/书架库存/8000t 完整读完/mental break/quest outcome/其它书 |

| C-01 | `cd38c1d4`、`int-c01` | 新 `GET /game/start/options`（scenario/preset/parts/world/xenotype 合法值）；`POST /game/start` 可选 `scenario`（def_name/preset_file/name/summary/parts 增删/save_as） | LostTribe 18 parts，移除/增加各1后响应与 fixture 一致；map250/Spring/seed、5 起始/8 候选；非法固定 part 400 且 map0；save_as `.rsc` 后 options 列出、preset_file 二次开局成功 | 集成复跑 `int-c01-20261113/evidence/c01-integration.json`（passed）；独立主菜单启动、fixture 只读回；未测逐 part config/Steam Workshop/上传/场景编辑取消 UI |

| ID-R07 | `9b1a878d`、`int-id-r07` | 复用 `GET/POST /dlc/ideology/biosculpter/policy`；原生 WorkGiver_HaulToBiosculpterPod/JobGiver_GetAgeReversal | 双 pod 供料：pod1 true 自动搬入并液化→SelectingCycle/nutrition_loaded true/remaining0；pod2 false 1200t 仍 LoadingNutrition/6 餐未动。auto_age false 强制到期 600t 不入舱；true 后原生入舱 Occupied/occupant Human315/ageReversal/remaining≈479996。缩短 currentCycleTicksRemaining→20 后真实 CycleCompleted：回 LoadingNutrition、弹出、年龄47→46、AgeReversalReceived、deadline3599954、无 BiosculptingSickness | 集成复跑 `int-id-r07-20261113/evidence/id-r07-integration.json`（passed）；fixture 供电/餐/PreceptMaker 补 AgeReversal_Demanded/过去 ageReversalDemandedAtAgeTicks（脚手架）、缩短周期（披露）、invuln；未测完整8天/重调谐/多 pod 争 pawn/掉电/被毁/medic或 bioregeneration/存读档 |

| C-38 | `6a6b4a9d`、`int-c38` | 新 `GET /faction/comms` + `POST /faction/comms/request`（open_job/trader/orbital_trader/military_aid/ai_core）；原生 FactionDialogMaker DiaOption action，dry_run | open_job comm_target Faction_7；AI core Silver2500→1000/quest_id1/OpportunitySite_ItemStash；trader tick=当前/goodwill100→81(-19)/TraderCaravanArrival+120000；orbital 81→43(-38)/8000t passing_ships1；军援未确认409 tick未写，confirm 后43→12(-31)/LordJob_AssistColony 9人/信件；清场后直接军援成功；4xx 缺 kind400/冷却422/非盟友422/AI门禁422/未确认409 | 集成复跑 `int-c38-20261113/evidence/c38-integration.json`（passed）；fixture 铺硬地/通信台/供电/5×500 Silver/盟友/清 tick/ShipRelated/临时 kind/共同敌人 lord（披露），product 不写 goodwill/Silver/tick；未测商队物理到访(+120000t 仅队列)/title-required 拒绝/leader 不可用/待处理存读档 |

| C-02 | `3c49d029`、`int-c02` | 新 `POST /game/start/world/preview`（scenario/world/tile_query，返回 world/tiles，不生成地图）；start 复用 preview world + site tile/map_size/season，warn 需 confirm_warnings | preview 200 map0/tiles 全 valid；非法 tile 400 map0；Winter+map300 无 confirm→409 requires_confirmation + MapWinterWarning/MapSizePerformanceWarning、map0；确认后 200，world 同 seed/name/3 factions、pollution.05、site tile0/Desert/300/Winter；fixture 读回一致 | 集成复跑 `int-c02-20261113/evidence/c02-integration.json`（passed）；独立主菜单、fixture 只读回；未测 proximity/biome settle warning/orbit 起始层/全 tile query/preview 取消 UI |

| C-05 | `0dd3f5d8`、`int-c05` | 新 `GET/POST /game/options`：storyteller/difficulty/custom_difficulty/anomaly playstyle+比例+study/colony cap 1..5/preferred_names≤6；原生 Notify_DefChanged/CopyFrom+recache/Dialog_AnomalySettings/Prefs.Save | 初始 Phoebe/Easy；dry 无变化；Randy/Medium + anomaly 0.2/0.4/1.25 + cap3 + Alice/Bob，fixture 与 GET 一致、/game/settings cap3；custom threat1.35/research1.8/allow_big_threats false；原生 save+load 后保留；4xx 未知 storyteller/难度404、cap6 400、anomaly1.5 400、未知 custom 字段400、bool 类型错400，失败后值不变 | 集成复跑 `int-c05-20261113/evidence/c05-integration.json`（passed）；fixture 只读；未测非 Standard scenario 限制/override playstyle 实际应用/跨进程 Prefs 读回 |

| C-03 | `570ffed7`、`int-c03` | 复用 `POST /game/start` 的 `prepare_only:true` + `starting_pawns`（keep/reroll/xenotypes/custom）；原生 StartingPawnUtility/RandomizePawn/SetGenerationRequest/ReorderRequests/Dialog_CreateXenotype | Crashlanded prepare_only 3 起始/8 候选/map0；reroll [0,1] Human195/198→219/223，第三位201不变；Dirtmole+自定义 C03 Custom（10基因）读回一致、.xtp 保存出现在 options；keep [2,0,1] 最终 map250/Spring、selected Human201/226/229、3 colonists；无 debug 人物/基因编辑 | 集成复跑 `int-c03-20261113/evidence/c03-integration.json`（passed）；独立主菜单、fixture 只读回；未测逐 pawn 外观背景/异种图标/全部自定义基因组合/archite 警告确认/UI 取消 |

| C-06 | `a9ee8da8` + lead 身份修复、`int-c06-20261113-run5` | 新 `GET /game/saves`；扩展现有 `POST /game/save`（overwrite/承诺模式/同步完成读回）、`POST /game/load`（原生校验/queued/last_operation 完成）；`game_instance` 改为 API 载入代次 | g35 原生列表；c06-test 保存 completed/16,581,749B/readable；同名409、overwrite200/overwritten；未知load404、坏rws422 不排队；load queued→completed，instance 递增、save_file=c06-test、tick76787；承诺新开局 commitment_name 生效、忽略请求名、再次 overwrite409 | 集成复跑 `int-c06-20261113-run5/evidence/c06-integration.json`（phase passed；前两次因 RuntimeHelpers 哈希复用失败，改为载入代次并保留失败现场）；fixture 只读+故意损坏 c06-corrupt.rws（披露）；未测异版本409/Autosave 特殊名/载入中重入/loading-screen |

| ID-R11 | `9d0cd997`、`int-id-r11` | 复用 `POST /dlc/ideology/archonexus/ideology`（body 新增可选 `ideo`）；校验沿用 C-04 `ValidateNewGameIdeo`；原生 GenerateIdeo/MakeIdeoPrimary/customOrLoadedIdeo + Dialog_ConfigureIdeo.forArchonexusRestart | create fixed：Archonexus Reforged（Structure_Animist+MaleSupremacy+Hindu）迁移 tile4、primary=created、2 pawn ideo=created、旧 map 移除；fluid：Archonexus Fluid fluid=true 成 primary 并迁移；load existing：选已有 ideo + pawn_ideos 显式转换、迁移后 primary/转换/旧 map 移除；dry 无变化 | 集成三次独立复跑 `int-id-r11-20261113`（create/fluid）、`...-run2`（fluid）、`...-run3`（load）均 passed；fixture 构造最小 Archonexus quest + 私有 TileChosen 打开真实 restart 对话框（不生成/接受任务、不走 carry/过场），apply 后原生 InitMoveColony；未测自然 offer/carry/过场/classic/preset 重开/多对话框/取消/1.5 |

| C-31 | `d6c11767`、`int-c31` | 新 `POST /building/cryptosleep/carry`：原生 `JobMaker.MakeJob(CarryToCryptosleepCasket, 倒地pawn, casket)` + TryTakeOrderedJob，count=1；不 TryAcceptThing/不传送；casket 缺省原生 FindCryptosleepCasketFor | grave assign/unassign applied；空 casket 读数；dry 不派 job、非倒地422；正式 job def/targetA=Human75478/targetB=CryptosleepCasket75481/count1，tick79240 contained_pawn_id=Human75478；已占用再搬422、无空 casket422；原生 eject applied/casket空/pawn仍倒地 | 集成复跑 `int-c31-20261113/evidence/c31-integration.json`（passed）；fixture 原生 Anesthetic+forceDowned 布倒地 pawn（披露）；未测多搬运者竞争/drafted carry/job 中 casket 被毁/quest lodger/guest prisoner |

| C-07 | `84a7dfc6`、`int-c07` | 新 `POST /game/names`（faction_name/settlement_id/settlement_name/dry_run）；原生 NamePlayerFaction/SettlementDialogUtility.IsValidName + Named；双字段原子预检；pawn 改名复用 `/pawn/edit/basic` | 隔离新开局 map250/Spring：dry 候选 applied false；combined C07 Faction + settlement112/C07 Colony，named_by_player true/tile71659/faction load_id16；单独 faction/settlement 改名读回；65字符/文件名/特殊字符400、缺id400、不存在404、非玩家422；合法 faction+非法 settlement 400 且派系保持；pawn C07Nick 读回、17字符400 且旧名保持 | 集成复跑 `int-c07-20261113-run2/evidence/c07-integration.json`（passed；首跑误从 g35 启动失败现场保留）；独立主菜单启动；未测 permadeath 自动存档名副作用/dry 不触发已写；未重复 S-11/OD-R07 |

| C-08 | `d19eebf3`、`int-c08` | 新 `GET /game/game-over` + `POST .../wanderers/prepare` + `POST .../wanderers`；原生 GameEnded letter/Dialog_ChooseNewWanderers/GameEndedWanderersJoin forced | tick77187 letter hours7.9996；早期 prepare 409 “about 8 hours”；20000t 后 hours=-0.002/create enabled；prepare 3 候选 Human75822/75825/75829、殖民者0；重复 prepare409；非法/重复/空候选422；confirm=false 不写；确认 Human75822 → applied/letter消失/gameEnding=false/new tick97192/spawned Human75822、dialog关、state Playing；再次 confirm/prepare409 | 集成复跑 `int-c08-20261113/evidence/c08-integration.json`（passed）；fixture 原生击杀全部 humanlike+有界推进（披露），无传送/复活/手造；未测 keep-watching/main-menu/death-refusal 阻塞/多次连续 game-over |

| C-23 | `6ad6cf6f`、`int-c23` | 新 `GET/POST /building/temperature`：原生 CompTempControl target/default/-273.15..1000；AtmosphericHeater override + UnnaturalHeat；不写 map/cell 温度 | Cooler/Heater 初始21；AtmosphericHeater20+condition；Cooler 5→raise1 6→lower1 5→raise10 15→lower10 5→reset21；Heater10；actual 保持22.8125；Atmospheric dry30 不写，真实设置后 target/heaterTargetTemp30、actual22.8125；both/neither/bad action400、-300/1001 422、Lamp GET/POST 422、未知404 | 集成复跑 `int-c23-20261113/evidence/c23-integration.json`（passed）；fixture Cooler/Heater 离网（powered=false），验证供电≠目标；未测供能后房间换热/多 AtmosphericHeater 叠加/最终环境变化 |

| C-24 | `b3b8b03f`、`int-c24` | 新 `GET/POST /building/fuel` + `POST /building/fuel/refuel`：原生 CompRefuelable target/auto/CanRefuel/RefuelJob（forced 可选）；不直接 Refuel/不造燃料 | PodLauncher 初始 fuel0/target150/auto true；target75/auto off/on 读回；auto on 原生 job 消耗75→fuel75/剩75、target/auto 保持；target150+auto off forced=false422、forced=true 真实 job 消耗68→fuel143/剩7、target/auto保持；target200/-1 422、无字段400、非配置/无toggle 422、lamp422、未知404 | 集成复跑 `int-c24-20261113/evidence/c24-integration.json`（passed）；fixture 只布置，真实 job 由原生 work giver；未测 RefuelAtomic/powered-only 消耗/多搬运者竞争/gravship 专用供料 |

| C-16 | `ef737992`、`int-c16` | 新 `GET/POST /building/turret` + `GET/POST /building/explosive`：原生 OrderAttack/holdFire/LoadShell/RemoveShell/SetAllow/Rocketswarm OrderAttack+TryActivateBurst/StartWick；不 DEV 瞬爆 | MiniTurret can_force=false→422/hold切；Mortar force/hold/clear/refire、装/退 Shell_HighExplosive、filter；Rocketswarm 显式落点激活；真实上炮 MannedNow、shell1→0、cooldown1680、target有效；StartWick 真实爆炸 destroyed=true；pawn+cell400/无目标400/越界422/非炮塔422/未知404 | 集成复跑 `int-c16-20261113/evidence/c16-integration.json`（passed）；根因屋顶阻挡+原生 ManTurret 装弹前置，已修正；未验 rocketswarm 弹着伤害；BlastingCharge 无 Def 实例，爆炸用 Mortar CompExplosive（披露）；未用 DEV 瞬爆 |

| C-20 | `59500fa0`、`int-c20-run2` | 新 `GET/POST /map/paint` + `GET/POST /map/plans`：原生 PaintFloor/RemovePaintFloor/PaintBuilding/RemovePaintBuilding designations；原生 Plan create/add/remove/rename/recolor/copy/delete；不写最终颜色/overlay | floor Structure_Red 有界推进 floor_color 改变/Dye5→4；remove 后 null；Plan A create2→add3→remove2→rename→recolor→copy(2)→delete(1) exact 读回；非法颜色/越界422、未知plan404、重复cells422；building 涂装施工未测 | 集成复跑 `int-c20-20261113-run2/evidence/c20-integration.json`（passed；首跑 DBus 启动失败现场保留）；fixture 只布置；未测 building 真实施工/Dye 消费、旋转复制/多色 overlay |

| C-09 | `5b30f677`、`int-c09-run3/run5` | 新 `GET/POST /world/caravan/forming`：原生 Dialog_FormCaravan→StartFormingCaravan、StopFormingCaravan、TryReformCaravan；修复 `/world/caravans`、`/world/caravan/path` GameTick 缓存（180/60 tick） | success：form→真实出图 caravan275（Human321/315/Muffalo75478+Silver100），mass2.84/154，map Silver减少；cancel：重复form409、StopFormingCaravan→forming false、重复cancel409、reform前置422；缺confirm/非法action400、不可达/超重422 | 集成 `int-c09-20261113-run3/run5` 均 passed；runner 两处测试缺陷由 lead 修正（success 未定义 lord、动物数字ID比较），失败现场保留；未测 native reform=true retained-caravan 完整出图/多地图/lord/forming 存读档/旅途事件 |

| C-15 | `c33eed53`、`int-c15` | 新 `GET/POST /pawn/combat-policy`：drafted/fire_at_will/hostility/stop；原生 Pawn_DraftController.FireAtWill、玩家控制 mech CompTurretGun、hostilityResponse、攻击 job stop；无任意 target/队列编辑 | 未drafted写fire_at_will409；drafted后false→true；hostility Attack 后未征召 shooter 原生 AttackStatic 目标 Muffalo75479（伤害窗口关无敌，披露）；stop 只结束 AttackMelee 并清队列 AttackStatic、非攻击队列不动；无攻击target409；Pacifist Attack409；玩家控制 Warqueen fireAtWill 读回、未控制409 | 集成复跑 `int-c15-20261113/evidence/c15-integration.json`（passed）；CompTurretGun 私有字段 AccessTools 耦合仅限玩家控制 mech；未测长巡逻/敌人死亡离图竞态/多pawn队列/队友阻挡/AI core/其它turret Def/1.5 |

| C-14 | `f300a383`、`int-c14` | 新 `GET/POST /pawn/work-queue`：clean_room/extinguish_fires 原生目标队列、prioritize 原生 WorkGiver、cancel_fertilize、manual priorities；不 broad-cancel/任意反射 | clean_room Clean+2 filth 推进清理；extinguish 1 Fire 推进 destroyed；prioritize DoctorTendToSelf→TendPatient/PriorityWork active；fertilize inbred0.8 无confirm409、confirm后 FertilizeOvum；cancel精确ovum reservation_released；manual on/off；cell+thing400/未知giver404/资格409 | 集成复跑 `int-c14-20261113/evidence/c14-integration.json`（passed）；fixture 原生 room/dirt/Fire/HumanOvum/TorchLamp（披露）；未测长循环/多人队列竞争/incapacity全分支/全部WorkGiverDef/真实UI FloatMenu/pocket map/1.5；HumanOvum fertilizingMan 未写 |

| C-10 | `c0409e6e`、`int-c10-run3` | 新 `GET/POST /world/caravan/journey`：native StartPath/arrival action、SettleInEmptyTileUtility.Settle、MapParent.Abandon(false)；settle/abandon confirm；不直接改 Tile/holder | foreign Asworth visit path/ETA、attack 真实 map1/Arberium Hostile -87/caravan消失/Human321/315入图；Colony enter 真图；settle queued→settlement276/map2；abandon→exists false/map1；4xx | 集成复跑 `int-c10-20261113-run3/evidence/c10-integration.json`（passed；首次 runner 缺脚本、后补 `tests/c10_caravan_journey.py`）；fixture 商队准备披露；未测 site/peace-talks/escape-ship 实际抵达、visit 信件 API、多地图存读档 |

| C-18 | `4a544c86`、`int-c18` | 新 `GET /builder/qualification` + `POST /builder/cancel` + `POST /builder/install`：原生 Designator_Build/GenConstruct 资格、Destroy(Cancel) 退款、PlaceBlueprintForInstall/Reinstall；不 debug paste | qualification accepted/越界422/未知404；Blueprint cancel；Frame 预置5 WoodLog 取消 refunded wood_log5；minified bed install 真实施工 install_bed_built=true；minified tree/独立清地 reinstall 返回 Blueprint_Install；混合非法 cancel 404 不部分写 | 集成复跑 `int-c18-20261113/evidence/c18-integration.json`（passed）；Blueprint refunded 空为原生语义（Frame 才退料）；未测 tree 重栽真实施工、reinstall 真实施工/搬运、Frame 多资源比例退款；fixture 只布置 |

| C-21 | `3a9ee704`、`int-c21` | 增强 `GET /map/areas`（area_id/kind/exact cells/color/mutable）；新 `POST /map/areas/edit`（Allowed rename/color/invert/clear/delete）；新 `GET/POST /map/area-policies`（autoHomeArea/autoRebuild）；不写 roof grid | Home306 exact cells、BuildRoof/NoRoof0、颜色读回；C21 Area 2 cells→rename/color/invert/clear0/delete；特殊edit422、非法颜色422、未知404、policy空400、auto flags true/false写读、dry不写 | 集成复跑 `int-c21-20261113/evidence/c21-integration.json`（passed）；特殊area只走cells；未测BuildRoof/清雪/污染清理代表工作实际执行；本安装无 Area_IgnoreRoof；fixture只暴露清地 |
| C-12 | `1c2d0d6e`、`int-c12` | 新 `GET/POST /transport/pod`：普通 CompTransporter/CompLaunchable_TransportPod；load/cancel_load/launch 原生 InitiateLoading/AddToTheToLoadList/CancelLoad/GetOptionsForTile/TryLaunch；不直接 holder/TravellingTransporters | load Human321+Steel50→left2→holder Steel50+Human321/can_launch；cancel group0→-1/contents空、重复cancel409；reload group1；launch form_caravan target200→applied/launched、world caravan276抵达tile200、pawn holder准确；非法action400/缺target400/未知pod404 | 集成 `int-c12-20261113/evidence/c12-integration.json`（passed）；runner 只实测form_caravan；trade/gift/attack/site 按 native options过滤未长跑；auto_load/跨层confirm_space未测；travelling 过短记录 false；OD-R06未重跑 |

| C-11 | `1a0cbc6f`、`int-c11` | 新 `GET/POST /world/caravan/manage`：split 原生 Dialog_SplitCaravan 语义、merge CaravanMergeUtility、rest pather.Paused、set_policy beCarried/baby food、equip/drop 世界装备；forage 只读自动 | 275 split Human321+Steel10→276/源留315；merge→275 members2/276 404；真 StartPath 后 rest paused false→true→false；set_policy false/true 读回；equip revolver→equipment holder、drop→inventory；非法action400/缺pawn_ids400/未知404 | 集成复跑 `int-c11-20261113/evidence/c11-integration.json`（passed）；baby 实际喂食未跑（fixture 无baby）；forage 只自动进度；shuttle merge 501（OD-R06）；C-10/C-12 未重做 |

| C-22 | `1411ee04`+`056c2fb0`、`int-c22-run3` | 新 `GET/POST /plant/growing`：zone/grower exact cells/plant_def/allowSow/allowCut/valid_plants；set_crop/set_policy/resize/rename/delete；原生 CanSowOnGrower+CanEverPlantAt | 修复后 crop Plant_Rose，worker 实际 Sow，zone plants Plant_Rose@(160,121) tick77236；zone exact8/9格/rename/resize/delete；grower合法作物；非法作物/越界/占用/缩0 4xx | 集成复跑 `int-c22-20261113-run3/evidence/c22-integration.json`（passed）；首跑 fixture 随机3x3落在 gravel/rock 导致不播，修复为 fertile/reachable/building-free 并加 CanEverPlantAt 产品校验；未测跨zone并发/全部plant def/屋顶季节极端/水培断电/多地图/1.5 |

| C-39 | `73ca5b78`/`756684dd`/`30a01d54`+lead `26724f93`、`int-c39-entry` | 新 `GET/POST /world/dialog`：白名单 NodeTree/NodeTreeWithFactionInfo/MessageBox；只执行原生 DiaOption.action；`/game/start` 新增 `started_from_entry` 由原生 PostGameStart 开 GameStartDialog | QuestPart Accept/outSignal；CaravanMeeting Trade disabled/Attack/MoveOn applied；GameStartDialog true 出现原生 dialog/OK has_action=false/POST409；false 对照 count0；CaravanDemand 手动 Give applied | 集成 `int-c39-20261113`、`int-c39-entry-...` true/false 均 passed；CaravanMeeting fixture NRE 已披露；CaravanDemand 未正式 runner；S-10 独立；未强关窗口 |

| C-17 | `80b7ff58`、`int-c17` | 新 `GET/POST /pawn/equipment`：innerContainer/equipment/worn apparel；pickup 原生 TakeInventory、drop equipment/apparel/inner、dress_other 原生 ForceTargetWear；机械师带宽确认 409→confirm | pickup count3→背包medicine3；drop inner TryDrop/equipped/forced jacket was_forced+forced_cleared；dress_other A=target/B=apparel；机械师 Airwire→Array 409 requires_confirmation→confirm job；4xx count超stack/非item/未知thing/self | 集成 `int-c17-20261113/evidence/c17-integration.json`（passed）；fixture 直接布置装备/背包/forced/机械师（披露）；未测部分堆叠/多物品pickup/复杂apparel layer/真实UI带宽确认/死亡精神状态/1.5 |

| S-01 | `5eac8274`、`int-s01` | 新 `GET/POST /utility/verbs`：只列装备 CompApparelVerbOwner 的原生 Verb；原生 Verb.TryStartCastOn；target cell/pawn/thing；不构造 Ability、不直接改 charges | DisruptorFlarePack 6→5；PackTurret 1→0 装备销毁；DeadlifePack 1→0；非法 pawn 422、未知 pawn/无效 verb 404、无 confirm 不施放 | 集成 `int-s01-20261113/evidence/s01-integration.json`（passed）；fixture 生成/穿戴装备；未测其它 utility verb 家族/reload role lock/LOS 边缘/商队世界 holder |

| S-02 | `befc3748`、`int-s02` | 新 `GET/POST /pawn/reload`：PrimaryEq/worn apparel reloadable、FindEnoughAmmo/MakeReloadJob；不直接写 charges/ammo | JumpPack3/5+Chemfuel min20/max40；dry不派job；真实job charge5/5、Chemfuel120→80；满弹409、错误gear/无弹药/错误action 4xx | 集成 `int-s02-20261113/evidence/s02-integration.json`（passed）；fixture 直接穿 Royalty JumpPack/Chemfuel（披露）；未测装备类reloadable/Anomaly/Odyssey变体/carry space边界/多堆竞争/1.5 |

| S-05 | `da44bdbc`+`4bd90949`、`int-s05` | 新 `GET/POST /guest/policy`：原生 GuestTracker 全模式/eligibility/ScheduledForInteraction；set/start/stop；旧 `/pawn/guest-policy` 不改 | 5x5 监狱房/assign床/仅Warden殖民者；start Interrogate 真实 warden job target正确；stop 后5窗口无新job；Convert/ReduceWill/Enslave set→stop MaintainOnly；slave Suppress→NoInteraction；Execute dry false；422/400/404 | 集成 `int-s05-20261113/evidence/s05-integration.json`（passed）；fixture 直接生成 prisoner/床/warden（披露）；未验实际 Convert/ReduceWill/Enslave 终态、Bloodfeed/HemogenFarm终态、Execute实际不可逆 |

### 2026-11-13 接续集成复验

| ID | 新成果与证据 | 限定 |
| --- | --- | --- |
| S-04 | `a2b41336`、`5c41b398`；统一DLL四类正常Silver交易、人物/wrapper holder与银币、dry/replay/拒绝通过；[合同](../api/shared-trade-categories.md) | `int-s04-codex-20261113a/evidence/s04-integration.json`，17 HTTP passed、tick76787不变、清理完整；原失败保留。 |
| ID-R12 | `f6410f54`、`e625e1de`；run-only status与原生doer角色修复；双眼2→1→0、Sight0、completed；[收尾](../references/reviews/2026-11-13-id-r12-ideology-blinding-closeout.md) | 统一DLL `id12-integrated-20261113/evidence/id12.json`、tick8→8524、0job异常、清理完整；旧舞蹈/燃烧终态复用。 |

### 2026-11-13 E流程六项差集核销

[集成报告与机读原件散列](../references/reviews/2026-11-13-api-playability-endings-transport.md)固定各自DLL、fixture、失败原件及续验结果；全部现场精确清理。

| ID | 新增限定结果 | 验收范围 |
| --- | --- | --- |
| C-04 / ID-R11 | `.rid`保存/发现→菜单正常新开局载入三人→Archonexus再次加载新实例、两人迁移；79HTTP passed | `fb712624`；统一DLL a94d4841…；旧create/fluid/keep不倒退；warning/XML独立样本见保存理念报告 |
| OD-R01 | 正常InteractThing保留Human321，再postpone/scavenge/destroy；主角/旁观memory、node/毁core/停用机械派系、具名credits和后续Goto | `80229ccf`；最终统一DLL39e177f…、255HTTP passed；三稳定器战斗是披露前置 |
| OD-R06 | 新结构化arrival_options、typed原生到达/地图格/旋转；两人+Steel20→聚落商队277→返map0(122,125)，fuel250→200→150 | 最终统一DLL39e177f…、176HTTP passed；旧501恢复项关闭；attack/site/space按具体native adapter审阅，不冒称逐个运行 |
| C-12 | 固定原生任务机AutoLoad/Send/Dismiss，Steel20真搬入、FlyAway、卸客返回地图与replay；RoyalAscent EndedSuccess | 8eaf7bde…；shuttle-actions passed；结局原failed与后续continued passed并列 |
| C-13 | 新game/ending与finish：Core/RoyalAscent继续Playing、原生tick及保存；Archonexus退出Entry、可发现开局/存档 | 8eaf7bde…；Core38HTTP/Archonexus10HTTP/皇室续验8HTTP passed；失败继续复用已验C-08；不创造旧世界继续权利 |

### 源码排除（分类修订）

| ID | 修订 | 依据与影响 |
| --- | --- | --- |
| AN-R05 | G → 源码排除，不计入剩余G | `CompProperties_Pushable.givePushOption` 默认 false；`Data/**/*.xml` 仅 Nociosphere/FleshmassNucleus 两处 `CompProperties_Pushable`，均只设 `offsetDistance`；`CompPushable.cs:26` 在 false 时 `yield break`，全代码无赋值点。原审计把未启用的 float menu 当成正常操作；不新增接口。Nociosphere 的既有 `nociosphere/send` 已覆盖其原生 TargetLocation 地格移动。 |


## 范围与明确排除

完整范围以[纯 API 游玩合同](api-playability.md)为准。**开局、自定义异种/场景、Core经营/旅行、原生失败继续都已纳入，不再排除。**

| 类型 | 处理 |
| --- | --- |
| 已有限定成果 | 原53单元和原21项的实现/代表终态保留；新增差集不是把这些全部重开。 |
| 原生自动/无正常任意选择 | 家庭遗传、学习desire、灵魂绑定、机械组数等沿原生流程发生；不制造任意指定随机结果的正常setter。 |
| 仅DEV/敌对专属 | 强制扫描找信号、Terraformer转换、古代风口开关、瞬间充电/污染抽取、dev造型等不作正常玩家接口；正常供电/破解/时间/战斗效果仍须可观测。 |
| 同路径变体 | 相同原生方法/参数语义的Def复用实现和充分证据，不追加全部RNG、品质、首领/动物种类、所有失败组合矩阵。具体不同正常选择仍属合同。 |
| 纯显示/导航 | 镜头、音画、overlay、百科跳转不要求复制；业务参数和资格所需数据必须有结构化API，不能以UI可点替代。 |
| 版本/第三方 | 仅固定1.6和五DLC组合；1.5/未来版/任意Mod与正式部署不自动在本轮声明内。 |

## 实施顺序与核销

先修确认错误 **AN-R03、AN-R06、BT-R01**；Core开局/世界旅行和共享数据、任务、策略能力可以独立并行推进，
不以G29或某结局作所有DLC前置。外观/预设可后置，不能删出账本。
按正常业务复用现有入口，不要求每行新增路由、不要求打开原生窗口。

每行核销记录提交、API请求/原生链、准确对象与独立终态、证据身份、debug前置和未测边界；
部分选择完成仍保持开放。G实现后要有代表正常结果；V复用现成API补验，不先重写。
全表完成并满足合同E-01–10连续流程及原生候选语义核对，才可声明本版本纯API游玩交付完成。
若发现范围内漏项或实现错误，归入本次交付缺陷并修复，不以“仍未穷尽”推迟定义完成。

同步[当前状态](../harness/current-state.md)、[质量](../QUALITY_SCORE.md)和相关合同；旧53表只在其声明变化时修改。
文档检查验证链接/结构，不能验证行为；本轮静态核对不等于这些操作已能运行。
