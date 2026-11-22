# RimWorld MCP 游玩说明

维护：Codex；核对：2026-11-16；状态：客户端使用说明。

你控制的是 `rimapi_status` 报告的独立 RimWorld 实例。网页画面供人观看；AI 主要通过结构化 API 观察与决策，加载地图后可调用 `rimapi_screenshot` 取得真正的 MCP 图片。

## 开始

1. 调用 `rimapi_status`，确认连接、`auth_enabled`、DLC、实例状态。`credential_configured` 只表示客户端有密钥，不表示服务器已启用校验。主菜单的 `/game/state` 可能是 `NotInitialized` 默认值，不能据此宣称已载入存档。
2. `rimapi_search_endpoints(query="game start")` 或 `query="game/saves"` 查新开局/存档能力。按用户目标新开局或选择已有存档，正常游戏不使用 devquick、测试 fixture 或刷资源。
3. `rimapi_search_reference(query="/api/v1/game/start")` 找合同、控制器与请求 DTO，随后 `rimapi_read_reference` 阅读参数。运行时目录的 `parameters:[]` 不表示没有 body/query。
4. 读取实际地图、人物、资源、工作、建筑、信件和任务；按具体目标只查需要的数据，优先范围/批量 API。

## 请求例子

工具参数的 `path` 可以是 `/game/state` 或 `/api/v1/game/state`，查询参数单独放 `query`，JSON 单独放 `body`。

```json
{"path":"/game/state"}
```

暂停使用 `rimapi_write`。这个旧接口的 speed 在 query，不能猜成 JSON body：

```json
{"method":"POST","path":"/game/speed","query":{"speed":0}}
```

其他接口必须按具体合同选择参数位置。`dry_run:true` 只放 JSON body，且仅在明确支持的接口使用。旧接口不因接入 MCP 就具备预演或幂等。

正常建屋顶用 `/order/designate/area` 的 `type:"roof"` 划区并等待小人施工。仅在用户明确要求立即改屋顶时，先读 `/debug/native/actions` 确认 `roof.set` / `roof.clear` 可用，再通过 `rimapi_write` 发送请求，例如：

```json
{"method":"POST","path":"/debug/native/execute","body":{"action":"roof.set","map_id":0,"def_name":"RoofConstructed","cells":[{"x":100,"z":100}],"dry_run":true}}
```

确认地图、格子和预演后再正式写入；从 `after` 读回实际屋顶，不能用区域标记判断完工。完整参数见 `roof-grid-debug` 参考。

即时移动地面物品时，确认 `/debug/native/actions` 含 `item.move`，从 `/map/things/radius` 取得原堆 `thing_id`，用 `rimapi_write` 发送预演：

```json
{"method":"POST","path":"/debug/native/execute","body":{"action":"item.move","map_id":0,"thing_id":12345,"x":100,"z":101,"dry_run":true}}
```

正式执行后核对 `after` 的堆 ID、数量和坐标。完整参数和放置条件见 `item-move-debug` 参考。

有货架 ID 时，用 `rimapi_read` 查询 `/storage/contents?map_id=0&thing_id=Shelf27979`，再按 `item-transfer-debug` 参考用 `rimapi_write` 预演 `item.transfer`。它可以自动选储存格，也能在地图格与支持的内部容器之间双向转移。顶层 `thing_id` 是原物品堆，两端的 `thing_id` 是储存或容器的 ID。正式执行后核对 `after.at_destination`、原 ID 和数量。

要把当前存档的每座普通双格货架设为 99 堆，开启开发者模式后用 `rimapi_write` 发送 `{"method":"POST","path":"/debug/native/execute","body":{"action":"storage.shelf_capacity.set","stack_capacity":99}}`。用 `rimapi_read` 查询 `/storage/shelf-capacity`，保存存档后保留设置。详见 `shelf-capacity` 参考。

即时完成研究时，从 `/research/tree` 取得研究的 `name`，开启开发者模式，用 `rimapi_write` 发送 `{"method":"POST","path":"/debug/native/execute","body":{"action":"research.complete","def_name":"SolarPanels","dry_run":true}}`。确认 `would_complete` 后去掉 `dry_run` 执行，用 `/research/project?name=SolarPanels` 读回。详见 `research-complete` 参考。

即时砍树时，从 `/map/plants?map_id=0` 查树的 `thing_id`，开启开发者模式，用 `rimapi_write` 发送 `{"method":"POST","path":"/debug/native/execute","body":{"action":"plant.cut","map_id":0,"thing_id":"27979","dry_run":true}}`，将示例 ID 换成目标。确认后去掉 `dry_run` 执行，读 `after.produced_count` 和 `after.products`。详见 `plant-cut` 参考。

WORK-I01 的即时工作结算动作同属 `/debug/native/execute`。先读 `/debug/native/actions` 确认本局 DLL 已列出动作，再对真实对象预演；不要用动作替代正常派工，也不要据同步响应宣称正常 Job 已完成。

| 动作 | 主要参数 | 原生结果读回 |
| --- | --- | --- |
| `work.clean_area` | `map_id` + `cells[]` 或 `point_a`/`point_b` | 逐格污物清除、地形/物品/建筑与区域外保持 |
| `work.mine_ore` | `map_id`、`thing_id`（Mineable）、`pawn_id` | 矿体销毁、实际产物堆、HP 贡献、邻格保持 |
| `work.bill_once` | `map_id`、`thing_id`（工作台）、`bill_id` 或 `bill_index`、`pawn_id` | 实耗材料、产物/品质、账单计数；UFT/屠宰走 D03A/D03B |
| `work.butcher` | `map_id`、`thing_id`（屠宰台）、`bill_id`/`bill_index`、`pawn_id`、`ingredients:[corpse_id]` | 尸体消费、肉/皮实际堆、账单一轮 |
| `work.finish_unfinished` | `map_id`、`thing_id`（UnfinishedThing）、`pawn_id` | 品质产物、材料只扣一次、绑定账单清空 |
| `work.harvest` | `map_id`、`thing_id`（成熟 Plant）、`pawn_id` | 实际作物/额外产物、植物销毁或回退生长 |
| `work.deconstruct` | `map_id`、`thing_id`（建筑） | 原生返料堆、目标移除、标记清理 |
| `work.uninstall` | `map_id`、`thing_id`（可卸装建筑） | MinifiedThing 外层/InnerThing、账单与品质保留 |
| `work.sow` | `map_id`、`x`、`z`、`def_name`（PlantDef） | 真实幼苗、growth/sown、物品总数不变（核心无种子物品） |
| `work.remove_floor` | `map_id`、`cells[]` 或矩形 | 顶层/临时地形移除、污物清格、返料、底层读回 |
| `work.smooth` | `map_id`、墙面 `thing_id` 或地面 `x`/`z` | 平滑后 def/地形、邻墙连带、标记清理 |
| `work.paint` | `map_id`、建筑 `thing_id` 或地面 `x`/`z`、`def_name`（ColorDef，省略=去色）、`pawn_id`、染料 `ingredients[]` | 颜色、染料实耗/返还、标记清理 |
| `work.extinguish` | `map_id`、`thing_id`（Fire）、`pawn_id` | 逐次 Extinguish 命中、火/附着物终态、其它火保持 |
| `work.refuel` | `map_id`、设备 `thing_id`、可选燃料 `ingredients[]` | 燃料 before/after、真实燃料扣除 |
| `work.repair_breakdown` | `map_id`、设备 `thing_id` | BrokenDown 清除（免料 DEV 修复，不产零部件） |
| `work.deep_drill` | `map_id`、钻机 `thing_id`、`pawn_id` | 矿网存量、实际产物、枯竭状态 |
| `work.ferment` | `map_id`、桶 `thing_id`、`mode=add|finish|take`、`pawn_id`、麦汁 | 麦汁实耗、发酵状态、啤酒落地 |
| `work.dispense_paste` | `map_id`、营养膏机 `thing_id`、`pawn_id` | 料斗实耗、营养膏配料与落点 |
| `work.gather` | `map_id`、动物 `thing_id`、`pawn_id` | 满度、原生随机成功/浪费、奶/毛实得 |
| `work.slaughter` | `map_id`、牲畜 `thing_id`、`pawn_id` | 尸体/死亡终态（不产肉皮） |
| `work.fish` | `map_id`、`x`、`z`、`pawn_id` | 实际渔获/稀有/负面结果、水体状态 |
| `work.reload` | `map_id`、装备 `thing_id`、`pawn_id`、弹药 `ingredients[]` | 充能数、弹药实耗、已满/不足 |
| `work.strip` | `map_id`、目标 `thing_id`、`pawn_id` | 穿戴/持有物落地、Strip 标记清理 |
| `work.clear_pollution` | `map_id`、`x`、`z` | 清理格、剩余污染、废料包 |
| `work.scan` | `map_id`、扫描器 `thing_id`、`pawn_id` | 地底矿网格/远程任务/信件、一次发现 |
| `work.tend` | `map_id`、病人 `thing_id`、医生 `pawn_id`、可选药品 | 实际治疗的 hediff/质量、药品实耗 |
| `work.gene_extract_finish` | `map_id`、提取器 `thing_id`、可选 `pawn_id` | 真实基因包/基因、供体 hediff/弹出 |
| `work.biosculpter_finish` | `map_id`、雕塑舱 `thing_id` | 周期完成、占用者弹出/效果、污物/历史 |
| `work.mech_gestation_finish` | `map_id`、孵化器 `thing_id` | 机械体 Formed、容器/废料、账单状态 |
| `work.subcore_scan_finish` | `map_id`、扫描器 `thing_id` | 次核心成品、供体/尸体、材料 |
| `work.surgery` | `map_id`、病人 `thing_id`、`bill_id`/`bill_index`、医生、可选材料 | 手术实际结果、耗材、账单删除 |
| `work.interact` | `map_id`、目标 `thing_id`、发起者、`mode=tame|train|recruit`、`force` | 阵营/驯服/训练/招募、抵抗/关系 |
| `work.lay_egg` | `map_id`、动物 `thing_id` | 蛋 def/数量/位置、受精/hatcher、进度清零 |
| `work.extract_bioferrite` | `map_id`、关押平台 `thing_id`、`pawn_id` | 生物铁实得、SurgicalCut/BioferriteExtracted、实体终态 |
| `work.gene_assembler_finish` | `map_id`、组装器 `thing_id` | Xenogerm 实物/基因、Archite 消耗、Working 复位 |

非 `work.*` 的补充读取/编辑：

| 入口 | 参数 | 读回 |
| --- | --- | --- |
| `GET /map/corpse/rot` | `map_id`、`thing_id` | 原生阶段、距腐烂/干枯小时、冻结/不活跃原因；仅地图上 spawned 尸体 |
| `POST /pawn/edit/recruitable` | `pawn_id`、`recruitable` | setter 值与原生有效资格、难度覆盖 |
| `POST /pawn/edit/trait/remove` | `pawn_id`、`def_name`、`degree`、`allow_gene_source` | 实际删除/未持有/压制/基因来源 |
| `POST /pawn/edit/memory/remove` | `pawn_id`、`kind`、`def_name`、`index` | 删除的单条记忆与同 Def 保留 |
| `POST /pawn/edit/inspiration/end` | `pawn_id` | 当前灵感结束、精神状态保持 |
| `POST /pawn/edit/backstory` | `pawn_id`、`slot`、`def_name` | 目标槽、另一槽、禁用工作 |
| `POST /pawn/edit/prisoner-state` | `pawn_id`、`resistance`/`will` | 两值与招募资格保持 |

人物详情队尾（PAWN）：

| 入口 | 参数 | 读回 |
| --- | --- | --- |
| `GET /pawns/log` | `pawn_id`、`scope`、`since_tick`/`until_tick`、`limit` | 合并日志、ticks_abs/age_ticks、命中/偏转/部位 |
| `GET /pawns/gear` | `pawn_id` | 负重/上限、适温、总护甲、逐件 mass/耐久/品质 |
| `GET /pawns/social` | `pawn_id` | 名单、实际关系、双向好感、身份/职位、囚犯理念 |
| `GET /pawns/ideology` | `pawn_id` | 理念/强度/每日变化/教化模式；支持殖民地囚犯 |
| `GET /pawns/bio` | `pawn_id` | 年龄、背景、特质等级/来源、工作限制及来源 |
| `GET /pawns/needs` | `pawn_id` | AllNeeds 原生值/阈值/趋势；缺需求 422 |
| `GET /pawns/health` | `pawn_id` | 能力、疼痛、流血、失血时间 |
| `GET /pawn/surgery/available` | `pawn_id` | 可用手术配方与部位 |
| `POST /pawn/inventory/ingest` | `pawn_id`、`thing_id`、`dry_run` | 原生背包食用 Job 提交，实际食用需后续 ticks |
| `POST /pawn/edit/item-attributes` | `pawn_id`、`thing_id`、`hit_points`/`quality` | 同一实例耐久/品质，另一件不变 |
| `POST /pawn/ideo/set` | `pawn_id`、`ideo_id`、可选 `dry_run` | 囚犯实际理念/强度、仍囚禁 |
| `POST /pawn/memory/add` | `pawn_id`、`def_name`；社交记忆须给 `other_pawn_id` | 区分接受与实例保留，读回目标、心情或好感 |
| `POST /pawn/edit/skill-xp` | `pawn_id`、`skills[]`（`skill_name`、`xp_since_last_level`） | 等级内经验、门槛、等级/热情保持 |
| `POST /pawn/edit/needs` | `pawn_id`、`need_def`+`value` 或 `needs` | 实际需求值；mood 不连带 joy |
| `POST /pawn/hediff/remaining` | `pawn_id`、`load_id`、`ticks_to_disappear` | 单条消失组件剩余时间 |
| `POST /pawn/banish` | `pawn_id`、`confirm`、`dry_run` | 原版驱逐资格/离队状态 |
| `POST /pawn/execution-mark` | `pawn_id`、`awaiting_execution` | 有罪标记，不执行处决 |

例如在已确认的暂停副本上预演清除两格污物：

```json
{"method":"POST","path":"/debug/native/execute","body":{"action":"work.clean_area","map_id":0,"cells":[{"x":127,"z":142},{"x":128,"z":142}],"dry_run":true}}
```

确认 `would_*` 与 `before` 后去掉 `dry_run` 执行，再用独立查询或夹具读回 `after`/区域外保持。完整参数见 `work-clean-area`、`work-mine-ore`、`work-bill-once`、`work-butcher`、`work-finish-unfinished`、`work-harvest`、`work-deconstruct`、`work-uninstall`、`work-sow` 参考。

## 畜牧与饲料

用 `GET /maps` 获取地图 ID，再用 `GET /animal/pens?map_id=…` 查封闭范围和实际栏内动物；`GET /animal/pen?animal_id=…` 查单只动物的入栏与拴绳状态。`GET /pen/policy?thing_id=…` 返回标记策略，`GET /autoslaughter?map_id=…` 返回自动屠宰配置和候选。路径使用 `/api/v1` 前缀。查动物物种与位置可用 `GET /map/animals?map_id=…`。

用 `GET /pen/food?thing_id=…` 对照原版 **Food** 页：看 `nutrition.growth_per_day`、`consumption_per_day`、`balance_per_day` 和饲料库存；开放栏为 `nutrition:null`。预计用尽天数只按当前缺口计算，不是长期预测。`POST /pen/food/example` 用 `animal_def` 和 `displayed` 切换示例；示例容量不是扣除已有动物后的余量。

MCP 查询示例（先完成 `rimapi_status`）：

1. `rimapi_search_endpoints`：`{"query":"/api/v1/pen/food","method":"GET"}`。
2. `rimapi_search_reference`：`{"query":"animal-pens /pen/food"}`，用 `rimapi_read_reference` 从命中行读合同。
3. 地图 ID 从 `/maps` 取；用 `rimapi_read` 调 `{"path":"/animal/pens","query":{"map_id":0}}` 查 `pens[].thing_id`。`0` 仅为示例。
4. 用标记原 ID 调 `rimapi_read`：`{"path":"/pen/food","query":{"thing_id":"PenMarker86415"}}`，检查 `response.data.nutrition`；不要把标记的字符串 ID 转成数字。

用 `GET /animal/production?animal_id=…` 读生命阶段及奶、毛、蛋进度。用 `GET /animal/designation?animal_id=…` 核原版驯服或手动屠宰资格与标记；`POST /animal/designation` 用 `designation:"tame"|"slaughter"` 和 `enabled` 添加或撤销，先 `dry_run:true`，再提交并读回。已标记不等于工作完成，手动标记也不等于自动屠宰候选。新接口先用 `rimapi_search_endpoints` 核当前实例是否已部署。

## 原生 UI 选项与环境/显示

先用 `rimapi_search_endpoints` 确认运行中游戏已提供所需路由，再搜索 `game-preferences` 或 `ui-option-repairs` 参考读取合同。通用 `rimapi_read` / `rimapi_write` 可直接调用这些接口：

| 原生选项 | 路由与发现方式 |
| --- | --- |
| 音量、自动存档、输入偏好、语言、分辨率与 UI 缩放 | `GET/POST /game/preferences`；先读 `schema`、语言 folder_name 和实际显示模式 |
| 快捷键 A/B 槽 | `GET/POST /game/key-bindings`；先读原生 DefName、当前/默认值与合法 KeyCode |
| 地图/世界显示底栏 | `GET/POST /game/play-settings`；先读 `available_flags` / `unavailable_flags`，与自绘 overlay 分开 |
| 账单操作者、计数来源、产品风格 | `GET /buildings/bill/options` 与 `POST /buildings/bill/configure` |
| 书架内容与准确一本书弹出 | `GET /building/books` 与 `POST /building/books/eject` |

偏好/键位可在主菜单使用，幂等为 `process_memory`；原生语言切换仅在主菜单，显示变更需合同规定的 `confirm_display`。显示底栏要求活动 Game，幂等为 `game_memory`，可开启项受 DLC 和当前地图资格限制。其它过滤、床用途、补舱、蓝图与清除主人字段见 `ui-option-repairs`。环境与显示控制的成功单独记录。

源码树参考与独立包快照的更新方式见 [MCP 说明](README.zh-CN.md)。参考中出现新接口不代表所连接游戏已升级；新增参考路径需重连 MCP 后载入。

## 观察、操作、读回

先查询准确实例 ID/资格/资源，再决定和提交动作。把接受、排队、原生工作执行中、完成与失败分别处理；HTTP 200 或 operation 事件不是建设、手术、研究等工作的完成证据。

需要推进时间时先设定终态、tick 与墙钟上限，选择正常速度，短段推进、查询威胁/人物状态并暂停再决策。不要一次放任游戏运行很久。暂停/继续是正常玩法，不借调试直接授予材料、治疗、奖励或触发任务来完成正常目标。

写入超时的结果未知，适配器不会自动重发。先读真实状态；只有该 method/path 声明支持时才按 Idempotency-Key 合同重放相同操作。已有 key 的请求体不得修改。

正常玩法、环境工具与 debug 按具体行为区分，不能只看路由前缀或 `layer`；例如 direct 并不总是作弊，GET 目录也不证明后续操作符合正常取得流程。只有用户明确要求调试时才使用调试直接变更。

## 连接边界

MCP 以本地 stdio 运行，由客户端电脑启动；API 请求经已配置的 HTTPS、Tailscale 或 localhost 发往游戏。公网 API 可达不代表云端助理已有 HTTP MCP 端点；云端客户端还需支持本地执行桥接或另行部署的 HTTP MCP 服务。MCP 不公开新的监听端口，不提供 shell 或任意目标 URL。

网页画面是可选的人类观看入口；使用 Cloudflare Access 时先在浏览器登录。画面登录和 API Bearer 分别校验，`RIMAPI_VIEW_URL` 不参与 API 请求。

`.env` 默认从适配器所在目录读取，与终端当前目录无关；已有环境变量优先。Token 不放入提示词、工具参数或工具结果。游戏端是否启用 Bearer 由游戏 Mod 设置/游戏进程环境决定，客户端填 Token 不会替服务器开启鉴权。
