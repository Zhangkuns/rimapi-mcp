# API Conventions

Use the [API guide](../api/index.md) to find a route. The endpoint contract and the
controller/DTO define the exact wire format; the conventions below are defaults.

## Request parameters

- Paths start with `/api/v1` in direct HTTP requests. Fill any path placeholders
  with concrete values.
- Query parameters use the exact key read by the controller's `RequestParser`.
  They are not renamed by the JSON serializer. For example, `POST /api/v1/game/speed`
  reads `speed` from the query, not the body.
- For JSON bodies, C# DTO properties usually become `snake_case` (`MapId` →
  `map_id`, `PlantDef` → `plant_def`). Use explicit `[JsonProperty]` names when
  present; for example, the work-priority DTO uses `id`, `work`, and `priority`.
- The standard request parser ignores unknown JSON members. A misspelled key may
  therefore be ignored rather than rejected. Omit optional fields unless the
  endpoint contract specifically uses `null`.
- Use the ID type returned by the matching lookup. Older routes often take a
  numeric `thingIDNumber`; newer ones may require the full string `ThingID`.
  For example, [pen food](../api/animal-pens.md) takes `thing_id=PenMarker86415`.
  Do not strip a prefix or convert IDs by guesswork.
- Use exact RimWorld `defName` values where requested (`Plant_Potato`, not the
  localized display label). A map cell generally has integer `x` and `z`, and
  `y=0` when the DTO includes `y`. Check each contract for bounds and area semantics.

For POST requests with no JSON inputs, send an empty request body with a length
or send `{}` to avoid `HttpListener` returning 411. Local unauthenticated example:

```sh
curl -fsS -H 'Content-Type: application/json' -d '{}' \
  'http://localhost:8765/api/v1/game/speed?speed=0'
```

## Responses and errors

Most JSON routes use an `ApiResult` envelope:

```json
{
  "success": true,
  "data": {},
  "errors": [],
  "warnings": [],
  "timestamp": "2026-03-21T23:26:03.876Z"
}
```

Check HTTP status, `success`, and `warnings`. A partial result may be HTTP 200
with `success: true` and warnings. `data` may be omitted. The default serializer
omits null object properties, but dictionary entries can still carry explicit
nulls, e.g. `nutrition: null` for an open pen. Some legacy routes return bare
JSON or `{ "error": "..." }`. SSE is a separate HTTP stream.

When provided, `error_code` is the machine-readable reason. The declared code-to-status mapping is:

| HTTP status | Error code |
| --- | --- |
| 400 | `bad_request` |
| 401 | `unauthorized` |
| 403 | `forbidden` |
| 404 | `not_found` |
| 409 | `conflict` |
| 422 | `dlc_missing` or `invalid_target` |
| 501 | `not_supported` |
| 500 | `internal_error` |

Legacy failures without a typed code still use a message-based fallback; the
response builder also falls back for `internal_error`. A gateway or HTTP listener
can reject a request before it reaches a controller, without an API envelope.

## Gameplay, auth, and writes

Game-state operations are dispatched to the game thread. An accepted operation
does not prove a queued pawn job has finished. Read back actual target state.
Use [idempotency](../api/idempotency.md) and `dry_run` only where supported; after
a transport timeout, check state before resubmitting. See [API usage semantics](../product-specs/api-usage-semantics.md)
and [security](../SECURITY.md) for gameplay/debug distinctions and Bearer rules.
