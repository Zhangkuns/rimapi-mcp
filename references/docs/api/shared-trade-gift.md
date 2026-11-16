# Shared 贸易赠礼模式（S-03）

2026-11-13 新增：

- `GET /api/v1/trade/gift?map_id=&trader_pawn_id=&negotiator_pawn_id=`
- `POST /api/v1/trade/gift`

只使用原生 `TradeSession.SetupWith(..., giftMode:true)` 和
`TradeDeal.TryExecute`/`FactionGiftUtility`；不手改 goodwill/货物，不重载
现有 `/trade/quote`、`/trade/execute`，普通 Silver 买卖路径保持不变。

## 读取

```json
GET /api/v1/trade/gift?map_id=0&trader_pawn_id=75478&negotiator_pawn_id=Human318
{
  "gift_mode": true,
  "recipient_faction": {"name":"Arberium","id":"Faction_0","goodwill":0,"relation":"Neutral"},
  "items": [{"def_name":"Cloth","sell_thing_id":49390,
             "colony_count":101,"sell_price":1.089,"supported":true}],
  "expected_goodwill_change": 0,
  "gift_can_carry": true,
  "can_execute": false
}
```

- gift-mode 只列殖民地可赠货物（sell 侧；原生 `TradeDeal` 只加入 colony
  tradeables），不列/不允许 buy、Silver、pawns、自然器官。
- `expected_goodwill_change`：`FactionGiftUtility.GetGoodwillChange`；无选择
  时为 0。
- `gift_can_carry`：原生 `FactionGiftUtility.CheckCanCarryGift`。
- `recipient_faction.goodwill/relation` 为当前外交状态；只读。

## 报价/执行

```json
POST /api/v1/trade/gift
{"action":"quote","map_id":0,"trader_pawn_id":75478,
 "negotiator_pawn_id":"Human318",
 "sell":[{"thing_id":49390,"count":60}]}
```

返回 `quote_id`、`items`（sell 计数）、`can_execute`、预计
`expected_goodwill_change`、`gift_can_carry`、`quote_id`。

```json
POST /api/v1/trade/gift
{"action":"execute","quote_id":"...","request_id":"s03-gift-1","confirm":true}
```

- `action=quote`：复用 TradeControl 的 `InSession`/`Prepare`，但
  `TradeSession.SetupWith(..., true)`；只允许 sell；空/不可赠/超量/不接受
  400/404/409/422；返回 `quote_id` 和过期时间。
- `action=execute`：校验 quote 未过期/未消费、giftMode、货物/仓位/
  `CheckCanCarryGift`；再调用原生 `TryExecute`；读回货物离开殖民地
  （物理 holdings 校验）、recipient faction、goodwill before/after/actual
  change、`native_accepted`/`native_actually_traded`。
- gift-mode 的 `Tradeable.PositiveCountDirection` 是 Destination，赠礼计数
  使用正数（原生滑条方向）；只写 `CountToTransfer`，不直接清理货物/goodwill。
- `Idempotency=game_memory`：execute 依赖 quote_id+request_id，重放返回原结果，
  quote 过期/已消费 409。
- 响应 `scope` 字段沿用 TradeControl 的普通 scope 字符串；gift 由
  `gift_mode:true` 标识。

## 原生来源与边界

- `TradeSession.SetupWith(trader, negotiator, giftMode:true)`、`TradeDeal`
  (gift 只加入 colony 可售物)、`TradeDeal.TryExecute`、
  `FactionGiftUtility.GetGoodwillChange`/`GiveGift`/`CheckCanCarryGift`。
- 不写 `Faction.PlayerGoodwill`、不 `Destroy` 货物；原生 `GiveGift` 负责。
- 未测：caravan/world trader 赠礼、faction goodwill 100 上限、pawns/器官/
  Silver 的完整排除矩阵、1.5；普通 Silver 买卖未在本轮重跑，代码路径未改。

## 证据

隔离 g35 tick 76787，五 DLC；fixture `tests/parallel/shared-trade-gift/S03Fixture.cs`
生成原生地图 trader（含 Steel 库存、wantsToTrade）、capable negotiator、Home
内 30 Cloth；fixture 不写赠礼结果。

- GET：gift_mode、Arberium neutral goodwill 0、Cloth 商品、gift_can_carry。
- quote 60 Cloth：can_execute true、expected goodwill（原生值）。
- execute：Cloth 101→41（60 离开殖民地）、goodwill 0→1、native_accepted true；
  request_id 重放返回 replayed=true；缺失 quote 4xx。
- 证据：`tools/cua/.runtime/s03-run10-20261113/evidence/s03.json`（phase passed）。
