# S-04 原生银币贸易类别

维护：Codex odyssey 单元；核对：2026-11-13；状态：实现及地图四类别隔离验收通过，待协调者集成。

沿用[交易合同](trade-control.md)的 `POST /api/v1/trade/quote` 与 `POST /api/v1/trade/execute`，使用原生 `TradeSession` / `TradeDeal.TryExecute`。可选货物扩大至原生 trader 接受的非货币、非 favor 类别；`scope` 为 `native_trader_all_silver`。Silver 自动结算，不能作为买卖行选取。gifts 仍由专用 gift 入口选取，正常伦理、资格、库存、资金和 trader 规则继续生效。

## Catalog 与准确 ID

每行新增 `category`：普通物品 `item`、人物 `pawn`、动物 `animal`、天然器官 `natural_organ`、封装建筑 `minified_building`。`supported` 同时要求原生 `TraderWillTrade`；出现 catalog 不代表能买卖。器官通常只有正常卖出资格，API 不替 trader 制造库存或扩大原生买入规则。

继续用 `buy_thing_id` / `sell_thing_id` 选原生等价组，数量作用于组，不能在两个列表重复选同一组。一个动物组可能包含多只，原生按组内顺序决定实际对象；请求 ID 不承诺指定组内某一只。人物独立成组。全部关键拒绝与报价时效、dry-run、消费和幂等规则沿用既有合同。

封装建筑的输入 ID 是实际 `MinifiedThing` 包装物。`any_thing_id` / `any_thing_def` 描述原生 `AnyThing`，它可能已解包，不能用它替换包装物的输入 ID。每侧的 `colony_stacks` / `trader_stacks` 保留实际 `thing_id`，并增加 `inner_thing_id` / `inner_def_name`。物料 `stuff` 来自内层建筑。`holder_type` / `map_id` 与人物的 `faction_id`、`host_faction_id`、`caravan_id`、`world_pawn`、`destroyed` 供准确状态读回。

## 成交读回

`expected_holdings` / `actual_holdings` 继续是 `[colony, trader]` 的物理持有量。普通物品按 Def/Stuff，封装建筑按内层 Def/Stuff 且只算包装物一次；人物与动物按准确 Pawn ThingID。响应沿用全局 snake_case 字典序列化，例如 `silver:`、`minified:_bed:_wood_log`、`pawn:_muffalo75481`。键名用于本次前后比较，不是新的请求选择器。

地图殖民地计数包含落在 Home 外的购入物和嵌套持有物，排除地图上其它 trader 的 stock；玩家世界商队的 pawn 成员也纳入读回。vanilla 会把已卖人物隐藏于 Goods 防止立即回购，因此以确切对象的 trader Lord/商队成员/飞船直接持有者或原生 settlement 世界人物终态核对，不能只以 catalog 消失推断成交。

`transferred_things` 返回本次实际选择的人物/动物/封装物在成交后的物理 ID 与归属。普通可堆叠物品仍按聚合数量和 Silver 核对，不承诺每一份拆堆后保持原 ThingID。原生拒绝保持 409；已提交但读回不一致维持 `outcome:uncertain` 与暂停后续交易的既有保护。相同 quote/request 的重放返回原始结果，不二次成交。

## 隔离验收

`tests/parallel/shared-trade-categories/S04Fixture.cs` 只准备谈判者、原生地图 Lord 商队、库存和殖民地资产；`observe` 只读。天然 Heart 卖出，人物/动物与封装建筑分别买卖，普通 Steel/Cloth 混合买卖；所有成交均走公开 REST。

```sh
python3 tools/cua/desktop.py exec --run RUN -- python3 tests/s04_trade_categories.py \
  --evidence RUN/evidence/s04.json
```

用全新暂停的 g35 复制存档与 S04Fixture DLL。runner 检查 public catalog、包装/inner ID、数量/重复组拒绝、quote/dry-run 无转移、原生成交、独立归属/数量/Silver、幂等重放。使用独立文件保留每次失败；如需恢复本 run 的复制存档，显式传 `--reload-save g35_deathresting`，不自动重试不确定成交。

本轮只新增地图 bulk/slaver 的类别实测；orbital/settlement/world caravan 的新增类别、所有 Def/品质变体、1.5 与目标 rev598 未在本轮实测。旧上下文普通物品证据保留，不扩写为新增类别全部通过。

原始证据在本单元 worktree 的 `tools/cua/.runtime/s04-codex-20261113c/evidence/s04-final.json`（17 次 HTTP，phase passed）。全程 tick 76787 暂停：bulk 混合 Silver 21289→21913、trader 5000→4376；人物混合 Silver 21913→21652、trader 5000→5261。人物 Human75488 归玩家且离开 trader Lord，囚犯 Human75492 归 Faction_0 且加入 slaver Lord，后者被原生 Goods 隐藏但仍确实存在。封装 ID 75484（inner Bed75483）购入地图，75486（inner Stool75485）售入库存；物品、动物、Silver 与原生 expected/actual 和独立 observe 一致。两笔报价/dry-run 和重放均无额外转移；inner ID 404、零量 400、超量 409、重复组 400。版本 `1.6.4871 rev600`、五 DLC；独立 profile 的清理记录确认原存档/正式 DLL/全局 prefs 不变，游戏 exit0。
