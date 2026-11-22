# RIMAPI API Guide

The RimWorld mod serves REST under `/api/v1`. The local stdio MCP adapter forwards
requests to the same API; MCP does not add in-game capabilities or replace the mod.

## Find the right reference

| Question | Start here |
| --- | --- |
| What does this game instance advertise? | [Live discovery](../capabilities/capabilities.md): `GET /api/v1/version`, `GET /api/v1/capabilities`, then protected `GET /api/v1/game/state`. In MCP, use `rimapi_status` and `rimapi_search_endpoints`. |
| What REST methods and paths are declared in this checkout? | [Static route index](routes.md), generated from the C# route declarations and grouped by source file. |
| What parameters, permissions, and results should I expect? | [Product contracts](../product-specs/index.md) and [API conventions](../developer_guide/api_conventions.md). If there is no detailed contract, look up the exact route in its controller and request DTO. An empty live `parameters` list is not a request schema. |
| What is deployed or already tested? | [Current state](../harness/current-state.md) and the [DLC operation matrix](../product-specs/dlc-operation-matrix.md). A source declaration is not deployment or acceptance evidence. |
| How do I connect MCP and update its references? | `README.md`, `README.zh-CN.md`, and `PLAYING.md` live under `mcp/` in the checkout and at the root of the standalone package. Use `rimapi_search_reference` and `rimapi_read_reference` to consult the bundled docs and source. |

## Domain entry points

These are starting points; the product contract index links more detailed feature and acceptance records.

| Area | Selected contracts |
| --- | --- |
| Core | [Pawn readback](core-pawn-readback.md), [construction](construction-status.md), [husbandry and feed](animal-pens.md), [storage settings](shared-storage-settings.md), [schedules](colonist-schedules.md), [trade](trade-control.md) |
| Royalty | [Psycasts](royalty-control.md), [permits](royalty-permits.md), [titles](royalty-title.md) |
| Ideology | [Beliefs](ideology.md), [rituals](ideology-ritual.md), [Archonexus](archonexus.md) |
| Biotech | [Genes](biotech-genes.md), [mechs](biotech-mechs.md), [reproduction](biotech-reproduction.md) |
| Anomaly | [Monolith](anomaly-monolith.md), [containment](anomaly-containment.md), [psychic rituals](anomaly-psychic-ritual.md) |
| Odyssey | [Gravship](odyssey-gravship.md), [layers](odyssey-layers.md), [fishing](odyssey-fishing.md), [animal training](odyssey-animal-training.md) |
| Environment and UI | [Preferences and key bindings](game-preferences.md), [native UI options](ui-option-repairs.md), [semantic observation](../ui-semantic-bridge.md), [spatial targets](../ui-spatial-targets.md) |
| Developer actions | [Native action contract](../debug-native-actions.md), [debug protection](../debug-invulnerability.md), [instant work contracts and evidence](../references/reviews/2026-11-21-work-integration.md) |

## Before calling a route

1. Choose [normal gameplay, environment tooling, or developer manipulation](../product-specs/api-usage-semantics.md) by behavior. These are not authentication roles. See the [Bearer and gateway rules](../SECURITY.md).
2. Check the exact query keys, body, target ID type and prerequisites. For a developer action, also check `GET /api/v1/debug/native/actions`: the execution route may exist without the requested action.
3. For writes, use `dry_run` and [idempotency](idempotency.md) only where the contract supports them. Read back the target state, and check it before retrying after a timeout. HTTP success or `queued` does not mean an in-game job finished.

## Upstream YAML examples

These pages are rendered from `docs/_api_macroses/controllers/`. They are useful for older examples but do not enumerate the fork or override current contracts:

- [Map and environment](map.md), [pawns and AI](pawns.md), [items and economy](things.md)
- [Game state and mechanics](game.md), [UI and camera](ui.md), [system and developer tools](system.md)
- [All YAML examples](all.md), [compact YAML index](llms-compact.md), [legacy API page](../api.md)
