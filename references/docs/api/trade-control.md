# Native trader quotes and execution (AI fork)

These three routes use the game's native `TradeDeal`. Supported contexts are loaded-map pawn traders, orbital trade ships, and a player
world caravan trading with a settlement or NPC caravan on the same tile. The
execution scope includes ordinary Silver trades of items, pawns/animals, natural
organs and minified buildings. Gift mode has its separate confirmation contract.
Catalog rows mark native support explicitly; exact pawn and minified wrapper identity
and holder verification are specified in [shared trade categories](shared-trade-categories.md).
This is an immediate native transaction, not a pawn job: the negotiator must be
able to reach the map trader or usable comms console; world negotiators must belong
to the selected caravan. The API does not make the pawn walk or travel first.

## 1. Find the trader and list goods

`GET /api/v1/trade/traders?map_id=0` lists trader pawn IDs and availability.
Use the numeric `thing_id` as `trader_pawn_id`. Choose a free, capable colonist
on the same map as `negotiator_pawn_id` (numeric string or the full Pawn ThingID).

```json
POST /api/v1/trade/quote
{
  "map_id": 0,
  "trader_pawn_id": 123,
  "negotiator_pawn_id": "321"
}
```

With no `buy`/`sell` lines, the result contains an `items` catalog with
`buy_thing_id`, `sell_thing_id`, both side counts, native unit prices, physical
stack IDs, and `supported`. It also reports both sides' **tradeable** silver.
No executable quote ID is issued for an empty basket.

Select exactly one trader identity per quote:

| Context | Discovery | Required quote fields |
| --- | --- | --- |
| Map pawn | `GET trade/traders?map_id=0`, `trader_kind=map_pawn` | `map_id`, `trader_pawn_id`, `negotiator_pawn_id` |
| Orbital ship | Same map listing, `trader_kind=orbital_ship` | `map_id`, `trader_ship_id` (string load ID), `comms_console_id`, `negotiator_pawn_id` |
| Settlement / NPC caravan | `GET trade/traders?caravan_id=277` | `caravan_id`, `trader_world_object_id` (including valid ID 0), `negotiator_pawn_id` |

Orbital trading checks the native comms console, pawn reach/reservation and a
powered orbital beacon. Goods and silver offered by the colony still come from
the native launchable stock; a beacon does not make every map item tradeable.
World trading requires a friendly nonplayer target on the caravan's exact tile;
settlements also pass native `CaravanArrivalAction_Trade.CanTradeWith`.
Every context checks the selected negotiator's native `CanTradeWith`, including
Empire title permissions. `TradeSettlement/TradeOrbital/TradeCaravan` are title
permissions; a trade does not spend royal-aid favor or start an aid cooldown.
These selectors are part of the stored quote and its fingerprint.

## 2. Quote a basket

```json
POST /api/v1/trade/quote
{
  "map_id": 0,
  "trader_pawn_id": 123,
  "negotiator_pawn_id": "321",
  "buy": [{"thing_id": 456, "count": 2}],
  "sell": [{"thing_id": 789, "count": 5}]
}
```

Replace the example IDs with catalog values. A thing ID selects its **native
equivalence group**, not an exact stack to transfer. The requested count applies
to the whole group. Select a group only once across both lists; duplicates,
zero/negative counts, and counts exceeding stock are rejected. Silver is settled
automatically and must not be selected as a trade line.

The result includes `quote_id`, `expires_at` (120 seconds), `can_execute`,
`colony_silver_delta`, and expected tradeable balances. A negative silver delta
means the colony pays. `can_execute:false` can describe an unaffordable basket;
execution returns 409 without transferring goods. Partial trader payment is
rejected rather than accepting the native short-payment behavior.

Quoting and dry-run do not transfer items or silver. Native session preparation
can create temporary objects or messages; it is not a promise of zero internal
side effects. All four static `TradeSession` fields are restored in `finally`.
An open native trade window returns 409 and is left under its owner. Vanilla can
leave static trade fields populated after closing the window; those cached fields
alone do not block API trading, and the API restores them after its temporary session.

## 3. Validate or execute the quote

```json
POST /api/v1/trade/execute
{
  "quote_id": "value-returned-by-quote",
  "request_id": "my-trade-001",
  "confirm": true,
  "dry_run": true
}
```

`dry_run:true` rechecks the current quote and funds without consuming it. Set
`dry_run:false` (or omit it) to execute; `confirm:true` and a nonempty `request_id`
(at most 128 characters) are required. Dry-run lives in the JSON body, not the
query string. The service rebuilds the native deal and compares inventory,
prices and balances with the quote; stale quotes return 409 and require a fresh
quote. This is deliberately conservative: changes to other catalog goods can
also invalidate a quote.

Success reports `executed:true`, `verified:true`, `outcome:"completed"`, and
`expected_holdings`/`actual_holdings`. Readback compares affected def/material
quantities in the player caravan inventory, or on the physical map and nested
holders (including incoming orbital drop pods), plus trader stock.
Purchased goods can land outside Home/storage; these are counted even when
not immediately eligible for resale. The verification covers quantity and money,
not every per-item quality/comp field or every downstream trade side effect.

## Retry and lifecycle rules

- A quote authorizes at most one native execution. Repeating the same quote and
  `request_id` returns the stored outcome with `replayed:true`; `executed:true`
  in a replay describes that original transaction. It does not mean another trade ran.
- Reusing a consumed quote with a different request ID, or dry-running it, returns 409.
  Idempotency is scoped to a quote, not a global account of all request IDs.
- Quotes are process- and game-local. Save/load or restart invalidates them;
  unknown/expired/evicted quote IDs return 404 and cannot execute.
- Unconsumed quotes last 120 seconds; results remain available up to 30 minutes
  from quote creation. The cache holds at most 128 entries; completed results
  may be evicted earlier to admit new quotes. Eviction never recreates a token.
- Only the first verified execution emits an SSE `operation` event. Quotes,
  dry-runs, rejections and replayed results emit no trade operation event.
- If native execution throws or readback disagrees, return 500 with
  `outcome:"uncertain"`. The quote stays consumed and all further execution in
  that game is blocked. Inspect the game and reload a known save before trading;
  never create a fresh quote to blindly repeat that uncertain transaction.
  There is no automatic single-trade rollback.

## Verification

Build `Release-1.6`, restart the game and load a paused throwaway test save with
a Silver trader, available silver and sale items in Home/storage. The test
**executes** real trades; use the IDs from that test game:

```bash
python3 tests/trade_control_integration.py --trader 123 --negotiator 321 \
  --buy-def Steel --sell-def Cloth --check-window --check-expiry \
  --reload-save ai_trade_test_fixture --output /tmp/trade-test.json
```

The test also needs a buy basket exceeding colony funds and a sell basket
exceeding trader funds, so prepare suitable test stock. It verifies both fund
rejections, quote/dry-run invariance, buy/sell/mixed settlement, stock readback,
stale quotes, retries and SSE counts. Optional flags also exercise a native player
trade window (including API recovery after close), actual 120-second expiry and
save/load invalidation. The named reload save must be that disposable fixture.
The complete run passed 64/64 checks; HTTP evidence is in the local reverse archive
at `evidence/trade-runtime-integration.json`. No claim of
arbitrary mid-settlement exception recovery or RimWorld 1.5 support is made.

## Context repair verification (2026-11-11)

The isolated regression is `tests/dlc_repair_integration.py --case trade`; use a
disposable profile with the G43 fixture. It prepares stock and contexts, then
compares independent native item/Silver counts before/after preview, execution
and replay. Evidence and remaining scope are recorded in the
[repair review](../references/reviews/2026-11-11-five-dlc-gap-repair.md).
