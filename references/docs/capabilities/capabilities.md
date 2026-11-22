# Runtime Capabilities

Use discovery against the game you intend to operate. For a local instance with
authentication off and the default port:

```sh
curl -fsS http://localhost:8765/api/v1/version
curl -fsS http://localhost:8765/api/v1/capabilities
curl -fsS http://localhost:8765/api/v1/game/state
```

- `version` identifies the running API/game version reported by the mod.
- `capabilities` reports the running game's documentation catalog, DLC and `auth_enabled`.
  Find the exact HTTP method and path. Some metadata comes from explicit contracts;
  other entries use heuristics. Neither is a complete request schema or an ACL.
- `game/state` checks access to a protected route. Main-menu defaults such as
  `NotInitialized` are not evidence that a save is loaded.

The native game router exempts `version` and `capabilities` from Bearer auth. A gateway
may still require it on all paths; see [security](../SECURITY.md). In MCP, use
`rimapi_status` for the initial check and `rimapi_search_endpoints` for a fresh,
filterable view of `capabilities`.

Route presence does not prove that a save is loaded, a DLC is active, an object is
eligible or an action is supported. In particular, use `GET /api/v1/debug/native/actions`
for developer action names, and read the relevant product contract before writing.

The catalog is not a dump of every registered route. For example, `GET /api/v1/events`
is manually registered for Server-Sent Events. Use a direct streaming HTTP client
for it (e.g. `curl -N` with the required auth); MCP's `rimapi_read` buffers the response.

For the checkout rather than the running game, see the [static route index](../api/routes.md).
For behavior and tested coverage, see the [product contracts](../product-specs/index.md),
[DLC operation matrix](../product-specs/dlc-operation-matrix.md), and [current state](../harness/current-state.md).
