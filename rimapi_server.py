#!/usr/bin/env python3
"""Local stdio MCP adapter for an existing RimWorld HTTP API (Python >=3.11)."""
from __future__ import annotations

import argparse
import base64
from dataclasses import dataclass, field
import json
import math
import os
from pathlib import Path
import socket
from typing import Any, Literal
from urllib import error, parse, request

from dotenv import dotenv_values
from mcp import types
from mcp.server import MCPServer

HERE = Path(__file__).resolve().parent
REFERENCE_PATTERNS = (
    "docs/api/*.md", "docs/product-specs/*.md", "docs/_api_macroses/controllers/*.yml",
    "docs/ui-*.md", "docs/debug-native-actions.md", "docs/developer_guide/api_conventions.md",
    "Source/RIMAPI/RimworldRestApi/Controllers/**/*.cs",
    "Source/RIMAPI/RimworldRestApi/Models/**/*.cs",
    "Source/RIMAPI/RimworldRestApi/Core/HttpContext/RequestParser.cs",
)
INSTRUCTIONS = """Control RimWorld through its existing HTTP API. First call rimapi_status
and rimapi_gameplay_guide. Discover actual routes with rimapi_search_endpoints;
read the shipped reference and controller/DTO source before choosing parameters.
Use normal player actions unless the user explicitly requests debug changes.
Send dry_run only in the JSON body and only when the endpoint supports it.
An accepted command is not a completed pawn job. Read back the actual result.
Writes are never automatically retried; a timeout means the outcome is unknown.
Use only the configured game instance. This adapter does not expose a shell.
"""


@dataclass(frozen=True)
class Settings:
    base_url: str
    token: str = field(default="", repr=False)
    view_url: str = ""
    timeout: float = 30

    @classmethod
    def load(cls, env_file: Path | None = None) -> Settings:
        path = env_file if env_file is not None else HERE / ".env"
        if env_file is not None and not path.is_file():
            raise ValueError(f"Environment file does not exist: {path}")
        values = {**dotenv_values(path, interpolate=False), **os.environ}
        base = values.get("RIMAPI_BASE_URL", "http://localhost:8765/api/v1").rstrip("/")
        url = parse.urlsplit(base)
        if (url.scheme not in {"http", "https"} or not url.hostname or url.username
                or url.password or url.query or url.fragment or url.path not in {"", "/api/v1"}):
            raise ValueError("RIMAPI_BASE_URL must be an HTTP(S) host, optionally ending in /api/v1")
        timeout = float(values.get("RIMAPI_TIMEOUT_SECONDS", "30"))
        if not math.isfinite(timeout) or timeout <= 0:
            raise ValueError("RIMAPI_TIMEOUT_SECONDS must be a positive finite number")
        return cls(base if url.path else base + "/api/v1",
                   values.get("RIMAPI_API_TOKEN") or "", values.get("RIMAPI_VIEW_URL") or "", timeout)


class NoRedirect(request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        # Never forward a game credential to a different host after a redirect.
        return None


def result(payload: dict, is_error: bool = False) -> types.CallToolResult:
    return types.CallToolResult(
        content=[types.TextContent(text=json.dumps(payload, ensure_ascii=False))],
        structured_content=payload, is_error=is_error,
    )


class GameAPI:
    def __init__(self, settings: Settings):
        self.settings = settings
        # Direct Tailscale routing must not depend on the client's HTTP_PROXY.
        self.opener = request.build_opener(request.ProxyHandler({}), NoRedirect())

    def call(self, method: str, path: str, query: dict | None = None,
             body: Any = None, idempotency_key: str | None = None) -> dict:
        url = parse.urlsplit(path)
        if (url.scheme or url.netloc or url.query or url.fragment or not path.startswith("/")
                or "\\" in path or any(part in {".", ".."} for part in parse.unquote(path).split("/"))):
            raise ValueError("Use a relative API path such as /game/state; pass query separately")
        suffix = path[len("/api/v1"):] if path == "/api/v1" or path.startswith("/api/v1/") else path
        target = self.settings.base_url + suffix
        if query:
            pairs = []
            for key, value in query.items():
                for item in value if isinstance(value, list) else [value]:
                    if item is not None:
                        pairs.append((key, str(item).lower() if isinstance(item, bool) else str(item)))
            target += "?" + parse.urlencode(pairs)
        headers = {"Accept": "application/json", "Content-Type": "application/json"}
        if self.settings.token:
            headers["Authorization"] = "Bearer " + self.settings.token
        if idempotency_key:
            headers["Idempotency-Key"] = idempotency_key
        req = request.Request(target, method=method, headers=headers,
                              data=None if body is None else json.dumps(body).encode("utf-8"))
        try:
            try:
                response = self.opener.open(req, timeout=self.settings.timeout)
            except error.HTTPError as exc:
                response = exc
            with response:
                raw = response.read()
                content_type = response.headers.get_content_type()
                if content_type.startswith("image/"):
                    data = {"image_data": base64.b64encode(raw).decode(), "mime_type": content_type}
                else:
                    try:
                        data = json.loads(raw)
                    except (ValueError, UnicodeDecodeError):
                        data = raw.decode("utf-8", errors="replace")
                ok = 200 <= response.status < 300 and not (isinstance(data, dict) and data.get("success") is False)
                return {"ok": ok, "http_status": response.status, "response": data}
        except (error.URLError, TimeoutError, socket.timeout, OSError) as exc:
            return {"ok": False, "http_status": None,
                    "error": f"API transport failed ({type(exc).__name__}); check Tailscale and game availability.",
                    "outcome": "unknown" if method != "GET" else "unavailable",
                    "next_step": "Read current state before retrying a write; this adapter did not retry."}

    def call_result(self, method: str, path: str, query: dict | None = None,
                    body: Any = None, idempotency_key: str | None = None) -> types.CallToolResult:
        payload = self.call(method, path, query, body, idempotency_key)
        return result(payload, not payload["ok"])


class References:
    def __init__(self, root: Path | None = None):
        self.root = root or (HERE / "references" if (HERE / "references").is_dir() else HERE.parent)
        # Only the shipped API reference files can be read, never .env or arbitrary local files.
        self.files = {str(p.relative_to(self.root)): p for pattern in REFERENCE_PATTERNS
                      for p in sorted(self.root.glob(pattern)) if p.is_file()}

    def search(self, query: str, limit: int = 12) -> dict:
        needles = query.casefold().split()
        if not needles:
            raise ValueError("Provide a route, DTO, feature, or parameter to search")
        matches = []
        for name, path in self.files.items():
            lines = path.read_text(encoding="utf-8-sig").splitlines()
            for number, line in enumerate(lines, 1):
                if all(word in (name + " " + line).casefold() for word in needles):
                    matches.append({"path": name, "line": number, "text": line[:500]})
                    break
        return {"total_files": len(matches), "matches": matches[:limit]}

    def read(self, path: str, start_line: int = 1, line_count: int = 100) -> dict:
        if path not in self.files:
            raise ValueError("Reference not found; use rimapi_search_reference to find an allowed path")
        lines = self.files[path].read_text(encoding="utf-8-sig").splitlines()
        return {"path": path, "total_lines": len(lines), "start_line": start_line,
                "text": "\n".join(f"{i + 1}: {lines[i]}" for i in range(start_line - 1, min(len(lines), start_line - 1 + line_count)))}


def create_server(settings: Settings, references: References | None = None) -> MCPServer:
    api, refs = GameAPI(settings), references or References()
    server = MCPServer("RimWorld API", version="1.0.0", instructions=INSTRUCTIONS)
    readonly = types.ToolAnnotations(read_only_hint=True, open_world_hint=False)
    writes = types.ToolAnnotations(read_only_hint=False, destructive_hint=True,
                                  idempotent_hint=False, open_world_hint=False)

    @server.tool(annotations=readonly, structured_output=False)
    def rimapi_status() -> types.CallToolResult:
        """Check this game's connection, protected API access, auth flag, DLC and state. No secret is returned."""
        version = api.call("GET", "/version")
        caps = api.call("GET", "/capabilities")
        state = api.call("GET", "/game/state")
        metadata = caps["response"].get("data", {}) if caps.get("ok") else {}
        ok = version["ok"] and caps["ok"] and state["ok"]
        return result({"ok": ok, "api_url": settings.base_url, "view_url": settings.view_url,
                       "auth_enabled": metadata.get("auth_enabled"),
                       "credential_configured": bool(settings.token), "endpoint_count": len(metadata.get("endpoints", [])),
                       "dlc": metadata.get("dlc"), "version": version, "game_state": state,
                       "note": "Main-menu game/state may contain NotInitialized defaults; it does not prove a save is loaded."}, not ok)

    @server.tool(annotations=readonly, structured_output=False)
    def rimapi_search_endpoints(query: str = "", method: str = "", offset: int = 0, limit: int = 20) -> types.CallToolResult:
        """Search the live API catalog by route, description or category. Returns exact methods and feature contracts, not complete request schemas."""
        if offset < 0 or not 1 <= limit <= 100:
            raise ValueError("offset must be >=0 and limit must be 1..100")
        response = api.call("GET", "/capabilities")
        if not response["ok"]:
            return result(response, True)
        needles = query.casefold().split()
        endpoints = [e for e in response["response"]["data"]["endpoints"]
                     if (not method or e["method"] == method.upper())
                     and all(n in json.dumps(e, ensure_ascii=False).casefold() for n in needles)]
        return result({"total": len(endpoints), "offset": offset, "endpoints": endpoints[offset:offset + limit],
                       "next_offset": offset + limit if offset + limit < len(endpoints) else None,
                       "note": "Read request parameters from the reference/controller and DTO source. layer is not a gameplay/debug permission."})

    @server.tool(annotations=readonly, structured_output=False)
    def rimapi_search_reference(query: str, limit: int = 12) -> dict:
        """Find shipped API contracts, request examples, controller and DTO source. Returns paths and line numbers. Useful when live docs omit body fields."""
        if not 1 <= limit <= 50:
            raise ValueError("limit must be 1..50")
        return refs.search(query, limit)

    @server.tool(annotations=readonly, structured_output=False)
    def rimapi_read_reference(path: str, start_line: int = 1, line_count: int = 100) -> dict:
        """Read an API reference file found with rimapi_search_reference. Lines are 1-based. Source is reference, never executed on the client."""
        if start_line < 1 or not 1 <= line_count <= 250:
            raise ValueError("start_line must be >=1 and line_count must be 1..250")
        return refs.read(path, start_line, line_count)

    @server.tool(annotations=types.ToolAnnotations(open_world_hint=False), structured_output=False)
    def rimapi_read(path: str, query: dict[str, Any] | None = None) -> types.CallToolResult:
        """GET a relative API path, e.g. /game/state, with optional query parameters. Preserves HTTP status and API errors. Some legacy GETs such as /core/docs/export also write a file on the game host; consult the endpoint contract."""
        return api.call_result("GET", path, query)

    @server.tool(annotations=writes, structured_output=False)
    def rimapi_write(method: Literal["POST", "PUT", "PATCH", "DELETE"], path: str,
                     query: dict[str, Any] | None = None, body: Any = None,
                     idempotency_key: str | None = None) -> types.CallToolResult:
        """Mutate the game using an exact discovered API method/path. Query and JSON body are separate; body can be an object, list, or scalar. No automatic retries. Idempotency-Key works only on routes declaring support; queued is not completed."""
        return api.call_result(method, path, query, body, idempotency_key)

    @server.tool(annotations=readonly, structured_output=False)
    def rimapi_screenshot(width: int = 1280, height: int = 720) -> types.CallToolResult:
        """Return a game screenshot as an MCP image, including the UI. Requires a loaded map; main menu returns the actual API error. Does not advance time."""
        if not 16 <= width <= 7680 or not 16 <= height <= 4320:
            raise ValueError("Screenshot size must fit the game's 16..7680 by 16..4320 range")
        response = api.call("POST", "/camera/screenshot", body={"width": width, "height": height,
                                                               "format": "jpeg", "quality": 80, "hide_ui": False})
        if not response["ok"]:
            return result(response, True)
        data = response["response"]["data"]
        uri = data["image"]["data_uri"]
        prefix, image = uri.split(",", 1)
        return types.CallToolResult(content=[types.ImageContent(data=image, mime_type=prefix[5:].split(";")[0]),
                                            types.TextContent(text=json.dumps({"metadata": data.get("metadata"),
                                                                              "game_context": data.get("game_context")}))])

    @server.tool(annotations=readonly, structured_output=False)
    def rimapi_gameplay_guide() -> str:
        """Read the normal-play workflow, discovery sequence, authentication notes, and examples before operating RimWorld."""
        return (HERE / "PLAYING.md").read_text(encoding="utf-8")

    return server


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--env-file", type=Path, help="Defaults to .env beside this script, regardless of working directory")
    parser.add_argument("--check", action="store_true", help="Print a connection check and exit; does not start MCP")
    args = parser.parse_args()
    try:
        settings = Settings.load(args.env_file)
    except (ValueError, TypeError) as exc:
        parser.error(str(exc))
    if args.check:
        response = GameAPI(settings).call("GET", "/capabilities")
        protected = GameAPI(settings).call("GET", "/game/state")
        data = response["response"].get("data", {}) if response["ok"] else {}
        print(json.dumps({"ok": response["ok"] and protected["ok"], "api_url": settings.base_url,
                          "auth_enabled": data.get("auth_enabled"), "credential_configured": bool(settings.token),
                          "endpoint_count": len(data.get("endpoints", [])), "protected_access": protected["ok"]}, indent=2))
        raise SystemExit(0 if response["ok"] and protected["ok"] else 1)
    create_server(settings).run(transport="stdio")


if __name__ == "__main__":
    main()
