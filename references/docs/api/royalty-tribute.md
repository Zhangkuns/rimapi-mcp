# Royalty 帝国贡品 / 荣誉币种交易（RY-R05）

已实现：`GET /api/v1/dlc/royalty/tribute` 与
`POST /api/v1/dlc/royalty/tribute`。使用原生 `TradeCurrency.Favor`
（帝国 `Empire_Caravan_TributeCollector`，荣誉/royal favor 币种），而不是银币
普通买卖，也不复用 `/trade/*` 的 silver 执行。

## 读取

`GET /api/v1/dlc/royalty/tribute?map_id=0&negotiator_pawn_id=Human321`

```json
{"map_id":0,
 "traders":[{"trader_pawn_id":"Human75478","trader_kind":"Empire_Caravan_TributeCollector",
   "faction_def":"Empire","trade_currency":"Favor","can_trade_now":true,
   "favor_label":"honor"}],
 "negotiator_pawn_id":"Human321",
 "negotiators":[{"pawn_id":"Human321","name":"Evan Blean","royal_favor":0}],
 "candidates":[{"thing_id":49400,"thing_def":"Gold","label":"gold","stack_count":500,
   "royal_favor_value":0.015,"favor_value_total":7.5,"market_value":10.0}]}
```

- `traders` 只列 `traderKind.tradeCurrency == Favor` 的地图商人；当前原版即
  `Empire_Caravan_TributeCollector`。
- `candidates` 来自原生 `ITrader.ColonyThingsWillingToBuy + PlayerSellableNow`，
  排除 Pawn（奴隶贡品属于 S-04，不在本端点）；`royal_favor_value` 是该物的
  `RoyalFavorValue` 原值，不是银价。
- `negotiators` 列出地图上可谈判的自由殖民者及其当前皇家 favor；请求必须显式指定
  接收 favor 的 `negotiator_pawn_id`。

## 执行

`POST /api/v1/dlc/royalty/tribute`

```json
{"map_id":0,"trader_pawn_id":"Human75478","negotiator_pawn_id":"Human321",
 "sells":[{"thing_id":49400,"count":400}],"confirm":true,"dry_run":false}
```

- 原生链：`TradeSession.SetupWith(trader, negotiator, false)` →
  `new TradeDeal()` → 对每个 colony Tradeable `AdjustTo(-count)` →
  `deal.UpdateCurrencyCount()`（`ForceToSource`）→ `deal.TryExecute`。
  `Tradeable_RoyalFavor.ResolveTrade` 在玩家获得 favor 时调用
  `negotiator.royalty.GainFavor(faction, CountToTransferToSource)`。
- 只支持卖方向（玩家交贡品、获得 favor）；不买商品，也不卖 Pawn。贸易前后恢复
  `TradeSession` 的静态字段，正在打开玩家交易窗口时返回 409。
- 结果：`sold`（准确的 thing_def/count）、`rows`（每行 `royal_favor_value`）、
  `favor_before/favor_after/favor_delta`、`native_accepted/actually_traded`、
  `map_counts_before/after`（按 Def 的整图堆叠数）。
- **原生取整**：`favor_delta_projected` 是执行前 `CurrencyTradeable.CountToTransferToSource`
  的投影；`favor_delta` 是执行后 `royalty.GetFavor` 的真实增量。原生
  `CostToInt` 对负数成本使用 `CeilToInt`，投影可能与实际差 1（例如 400 gold
  投影 6、实际 5），所以终态以 `favor_delta`/`favor_after` 为准。
- 非 dry 必须 `confirm:true`；无该 favor 商人 422，未知 thing_id 404，
  Pawn/币种 501，非商人可买物 422，数量越界 409，零/负数量 400。
- 人物 favor 达到门槛会触发原生头衔升级/奖励（如 Freeholder），头衔本身由
  `RoyalTitleTracker.OnFavorChanged` 处理，不在本端点直接写。

## 原生验证

隔离 Xvfb `g35_deathresting` 副本（五 DLC，tick 76787）：
fixture 触发原生 `CaravanArrivalTributeCollector` 并生成普通 Gold/Silver；
GET 读到 `Human75478`/`Empire_Caravan_TributeCollector`/`Favor`，候选 Gold 500
（原存档 100 + fixture 400）；dry-run `applied:false`、投影 6、favor 保持 0；
正式交 400 gold 后 `native_accepted/actually_traded:true`、`favor_before:0`、
`favor_delta:5`、`favor_after:5`、map gold 900→500；`GET title` 读回
`Freeholder`/favor 5；未知 thing_id 404、零数量 400、无 confirm 400。

证据：`tools/cua/.runtime/ry-r05-20261112b/evidence/ry-r05.json`
（runner `tests/parallel/royalty/ry_r05_tribute.py`，fixture
`tests/parallel/royalty/fixture/RyFixture.cs`）。

未重跑：奴隶贡品（S-04）、其它 favor 商人/事件来源、礼物模式、多组不同物品混合、
无头衔/无 Empire 派系的拒绝、favor 达到更高头衔门槛的连续升级。
