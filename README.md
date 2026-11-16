# RIMAPI MCP

English | [简体中文](README.zh-CN.md)

Let an AI client observe and control RimWorld through an existing RIMAPI REST API. This adapter runs locally over stdio, requires Python 3.11 or later, and uses the official Python MCP SDK. The client computer does not need RimWorld or Xvfb installed.

```text
AI client → local stdio MCP → HTTPS / Tailscale / localhost → RimWorld API
Browser   → Cloudflare Access → Tunnel → RimWorld game viewer (optional)
```

The game computer must already run a compatible RIMAPI service. The adapter can connect to a public HTTPS API, a reachable Tailscale host, or localhost. The game viewer and its authentication are configured separately on the game host.

## Install

```sh
git clone https://github.com/Zhangkuns/rimapi-mcp.git
cd rimapi-mcp
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
cp .env.example .env
chmod 600 .env
```

## Configure your private connection

For an existing HTTPS deployment, edit `.env`:

```dotenv
RIMAPI_BASE_URL=https://rimapi.example.com/api/v1
RIMAPI_API_TOKEN=replace-with-your-game-token
RIMAPI_VIEW_URL=https://rimworld.example.com/
RIMAPI_TIMEOUT_SECONDS=30
```

Replace the example hostnames and Token with your own values. `RIMAPI_API_TOKEN` is the game API's Bearer Token, not a Cloudflare management API Token. Keep real addresses and credentials in the private `.env`.

| Setting | Purpose |
| --- | --- |
| `RIMAPI_BASE_URL` | HTTP(S) API host, optionally ending in `/api/v1` |
| `RIMAPI_API_TOKEN` | Token matching the game or public gateway; sent as `Authorization: Bearer …` |
| `RIMAPI_VIEW_URL` | Optional browser viewing URL; does not affect API requests |
| `RIMAPI_TIMEOUT_SECONDS` | Request timeout in seconds; defaults to 30 |

For a local game, use `http://localhost:8765/api/v1`. For a Tailscale deployment, use a reachable game hostname and its configured API port, such as `http://game-host:18765/api/v1`; the client needs tailnet access. A public HTTPS API does not require Tailscale on the client.

The default `.env` is loaded from beside `rimapi_server.py`, regardless of the working directory. Use `--env-file /absolute/path/to/.env` to select another file. Exported environment variables take precedence. Reconnect the MCP service after changing configuration so its process reads the new values.

An empty Token works only when the server permits unauthenticated access. Setting a client Token does not enable authentication on the game server. The adapter does not create a tunnel, DNS records, an Access policy, or a game viewer.

## HTTPS and viewer authentication

A deployment can publish two separate hostnames through Cloudflare Tunnel while keeping the API proxy, noVNC, and VNC listeners on loopback:

| Entry point | Authentication |
| --- | --- |
| Game viewer | Cloudflare Access login, for example an email allowlist with one-time codes; protect the page, assets, and WebSocket handshake |
| REST API | Bearer Token on every public API request, without an interactive browser login |

For that deployment, configure Access and origin validation before publishing the viewer hostname. Enforce Bearer authentication at the public API gateway on all paths, including `/version` and `/capabilities`, which the native game server exempts. These are deployment requirements; the adapter does not enforce the gateway's configuration.

Browser login and API authentication are separate. The game Token cannot log in to the viewer; an Access browser session does not authenticate this adapter. Point `RIMAPI_BASE_URL` at the API hostname, and open `RIMAPI_VIEW_URL` in your browser to log in to the viewer. The adapter uses `rimapi-mcp/1.0` as its User-Agent for API requests.

This repository provides **stdio MCP**. The client must be able to launch the local Python process. A cloud-only assistant that accepts a remote HTTP MCP URL needs a separately implemented and authenticated HTTP MCP service or a supported local execution bridge. Making the REST API public does not turn it into an HTTP MCP endpoint or automatically connect Dots.

## Connect an MCP client

Register a local stdio server using absolute paths to the virtual environment's Python, the script, and the private `.env`:

```json
{
  "mcpServers": {
    "rimapi": {
      "command": "/absolute/path/to/rimapi-mcp/.venv/bin/python",
      "args": [
        "/absolute/path/to/rimapi-mcp/rimapi_server.py",
        "--env-file",
        "/absolute/path/to/rimapi-mcp/.env"
      ]
    }
  }
}
```

For a local Codex CLI installation:

```sh
codex mcp add rimapi -- \
  /absolute/path/to/rimapi-mcp/.venv/bin/python \
  /absolute/path/to/rimapi-mcp/rimapi_server.py \
  --env-file /absolute/path/to/rimapi-mcp/.env
```

## Check the connection and play

```sh
.venv/bin/python rimapi_server.py --check
.venv/bin/python smoke.py
```

`--check` reads discovery and protected game state, reporting authentication and credential status without printing the Token. `smoke.py` checks the official MCP handshake, tool discovery, API/reference lookup, and an actual MCP screenshot. With no map loaded, it reports the screenshot error. The optional `--settings-roundtrip` flag changes and restores the game's run-in-background setting.

Ask the AI to call `rimapi_status` and `rimapi_gameplay_guide` first, confirm the game instance, loaded save, and authentication, then observe, act, and read back the result. The [gameplay guide](PLAYING.md) is currently in Chinese.

| Tool | Purpose |
| --- | --- |
| `rimapi_status` | Version, DLC, authentication, and protected game state |
| `rimapi_search_endpoints` | Search the connected game's current routes and contract metadata |
| `rimapi_search_reference` / `rimapi_read_reference` | Find bundled contracts, examples, controllers, and request DTOs |
| `rimapi_read` / `rimapi_write` | GET and explicit writes, with separate query/body and optional idempotency key |
| `rimapi_screenshot` | Return an actual MCP image when a map is loaded |
| `rimapi_gameplay_guide` | Read the normal-play instructions |

Requests only target the configured game; there is no shell or arbitrary-URL tool. Writes are never automatically retried. A timeout means the outcome is unknown: read current state before retrying. HTTP 200 or a queued operation does not prove a pawn job has finished. Read the reference before selecting parameters; an empty runtime `parameters` list does not mean an endpoint has no inputs.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| API returns 401 | Token matches the game/gateway; `.env` was reloaded; exported variables are not overriding it |
| API returns 302 or a login page | API hostname and gateway authentication; the adapter does not complete browser Access login or follow redirects |
| API returns 403 or Cloudflare 1010 | Update the adapter and inspect the relevant Cloudflare security event |
| Connection fails | Game, API gateway, and tunnel are running; hostname is reachable; Tailscale is connected if using a tailnet address |
| AI cannot start the MCP service | Client supports local stdio servers and uses valid absolute paths |

## Privacy, tests, and source

`.gitignore` excludes `.env` and its backups, personal `mcp-config.json`, `.venv`, and runtime data. The example file contains only generic values. Keep real hostnames, IPs, emails, Tokens, and tunnel credentials out of commits; inspect `git status --short` before publishing.

The standalone `references/` folder contains API documentation and controller/DTO source snapshots for parameter lookup. The client does not execute those sources. Available routes depend on the connected game's version.

```sh
.venv/bin/python -m unittest discover -s . -p 'test_*.py' -v
```

Tests use a local HTTP fixture and do not require a running game. `package.py DESTINATION` creates a standalone copy with both READMEs and the API references; private configuration and environments are excluded. The adapter comes from the RIMAPI project's `mcp/` directory; references originate from `AppSprout-dev/RIMAPI` and subsequent development. The standalone package retains the GPL terms in `LICENSE`.
