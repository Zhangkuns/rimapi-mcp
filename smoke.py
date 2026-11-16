#!/usr/bin/env python3
"""Exercise the official stdio MCP client against this adapter and its configured game."""
import argparse
import asyncio
import json
from pathlib import Path
import sys

from mcp import ClientSession
from mcp.client.stdio import StdioServerParameters, stdio_client

HERE = Path(__file__).resolve().parent


def payload(response):
    if response.structured_content is not None:
        return response.structured_content
    return json.loads(response.content[0].text)


async def check(env_file: Path | None = None, settings_roundtrip: bool = False):
    args = [str(HERE / "rimapi_server.py")]
    if env_file is not None:
        args += ["--env-file", str(env_file.resolve())]
    params = StdioServerParameters(command=sys.executable, args=args, cwd=HERE)
    summary = {"ok": False, "settings_roundtrip": "not_requested"}
    async with stdio_client(params) as (read, write):
        async with ClientSession(read, write, read_timeout_seconds=45) as session:
            initialized = await session.initialize()
            summary["protocol"] = initialized.protocol_version
            tools = await session.list_tools()
            summary["tools"] = [t.name for t in tools.tools]
            assert len(summary["tools"]) == 8, summary

            async def call(name, arguments=None):
                response = await session.call_tool(name, arguments or {})
                if response.is_error:
                    raise RuntimeError(f"{name}: {response.content}")
                return payload(response)

            status = await call("rimapi_status")
            assert status["ok"], status
            summary.update(api_url=status["api_url"], endpoint_count=status["endpoint_count"],
                           auth_enabled=status["auth_enabled"], credential_configured=status["credential_configured"])
            search = await call("rimapi_search_endpoints", {"query": "game/speed", "method": "POST"})
            assert any(e["path"] == "/api/v1/game/speed" for e in search["endpoints"]), search
            refs = await call("rimapi_search_reference", {"query": "/api/v1/game/speed"})
            assert refs["matches"], refs
            ref = refs["matches"][0]
            detail = await call("rimapi_read_reference", {"path": ref["path"], "start_line": ref["line"], "line_count": 20})
            assert "/api/v1/game/speed" in detail["text"], detail
            state = await call("rimapi_read", {"path": "/game/state"})
            summary["game_state"] = state["response"].get("data")
            guide = await session.call_tool("rimapi_gameplay_guide", {})
            assert not guide.is_error and "dry_run" in guide.content[0].text
            summary["reference_lookup"] = ref["path"]

            if settings_roundtrip:
                async def current_setting():
                    response = await call("rimapi_read", {"path": "/game/settings/run-in-background"})
                    value = response["response"]["data"]
                    assert isinstance(value, bool), response
                    return value

                original = await current_setting()
                try:
                    await call("rimapi_write", {"method": "POST", "path": "/game/settings/toggle/run-in-background"})
                    changed = await current_setting()
                    assert changed != original, {"original": original, "changed": changed}
                finally:
                    if await current_setting() != original:
                        await call("rimapi_write", {"method": "POST", "path": "/game/settings/toggle/run-in-background"})
                assert await current_setting() == original
                summary["settings_roundtrip"] = {"changed": True, "restored": True, "original": original}

            # Discovery remains public; a protected route and screenshot report actual errors.
            missing = await session.call_tool("rimapi_read", {"path": "/mcp-route-that-does-not-exist"})
            assert missing.is_error, missing
            summary["unknown_route_error"] = payload(missing).get("http_status")
            screenshot = await session.call_tool("rimapi_screenshot", {})
            if screenshot.is_error:
                summary["screenshot"] = {"is_error": True, "response": payload(screenshot)}
            else:
                assert any(c.type == "image" for c in screenshot.content), screenshot
                summary["screenshot"] = {"is_error": False, "image": True}
            summary["ok"] = True
    return summary


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--env-file", type=Path)
    parser.add_argument("--settings-roundtrip", action="store_true",
                        help="Temporarily toggle and restore run-in-background on the selected isolated game")
    args = parser.parse_args()
    print(json.dumps(asyncio.run(check(args.env_file, args.settings_roundtrip)), ensure_ascii=False, indent=2))
