"""Real HTTP and official MCP protocol checks for the remote adapter."""
import asyncio
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import os
from pathlib import Path
import tempfile
import threading
import time
import unittest
from unittest.mock import patch
from urllib.parse import parse_qs, urlsplit

from rimapi_server import GameAPI, References, Settings
from smoke import check

PIXEL = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aQ3cAAAAASUVORK5CYII="


class APIHandler(BaseHTTPRequestHandler):
    calls = []
    background = True

    def log_message(self, *args):
        pass

    def send(self, status, body):
        raw = json.dumps(body).encode()
        self.send_response(status)
        self.send_header("Content-Type", "application/json")
        self.end_headers()
        try:
            self.wfile.write(raw)
        except BrokenPipeError:
            pass

    def do_GET(self):
        path = urlsplit(self.path).path
        raw = self.rfile.read(int(self.headers.get("Content-Length", "0")))
        body = json.loads(raw) if raw else None
        record = {"path": self.path, "method": self.command, "body": body,
                  "authorization": self.headers.get("Authorization"), "key": self.headers.get("Idempotency-Key"),
                  "user_agent": self.headers.get("User-Agent")}
        self.calls.append(record)
        if path not in ["/api/v1/version", "/api/v1/capabilities"] and record["authorization"] != "Bearer test-secret":
            return self.send(401, {"success": False, "errors": ["unauthorized"]})
        if path == "/api/v1/redirect":
            self.send_response(302)
            self.send_header("Location", "/api/v1/redirect-destination")
            self.end_headers()
            return
        if path == "/api/v1/slow":
            time.sleep(.15)
        if path == "/api/v1/mcp-route-that-does-not-exist":
            return self.send(404, {"success": False, "errors": ["not found"]})
        if path == "/api/v1/conflict":
            return self.send(409, {"success": False, "errors": ["conflict"]})
        if path == "/api/v1/api-failure":
            return self.send(200, {"success": False, "errors": ["game rejected operation"]})
        data = record
        if path == "/api/v1/capabilities":
            data = {"auth_enabled": True, "endpoints": [{"method": "POST", "path": "/api/v1/game/speed"}], "dlc": {"core": True}}
        elif path == "/api/v1/game/state":
            data = {"game_tick": 42, "is_paused": True}
        elif path == "/api/v1/game/settings/toggle/run-in-background":
            APIHandler.background = not APIHandler.background
            data = APIHandler.background
        elif path == "/api/v1/game/settings/run-in-background":
            data = APIHandler.background
        elif path == "/api/v1/camera/screenshot":
            data = {"image": {"data_uri": "data:image/png;base64," + PIXEL}, "metadata": {"width": 1, "height": 1}}
        self.send(200, {"success": True, "data": data})

    do_POST = do_GET
    do_DELETE = do_GET
    do_PUT = do_GET
    do_PATCH = do_GET


class AdapterTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.http = ThreadingHTTPServer(("127.0.0.1", 0), APIHandler)
        cls.thread = threading.Thread(target=cls.http.serve_forever, daemon=True)
        cls.thread.start()
        cls.base = f"http://127.0.0.1:{cls.http.server_port}/api/v1"

    @classmethod
    def tearDownClass(cls):
        cls.http.shutdown()
        cls.http.server_close()
        cls.thread.join()

    def setUp(self):
        self.api = GameAPI(Settings(self.base, "test-secret"))
        APIHandler.calls.clear()

    def test_env_file_and_exported_environment_priority(self):
        with tempfile.TemporaryDirectory() as tmp:
            env = Path(tmp) / ".env"
            env.write_text(f"RIMAPI_BASE_URL={self.base}\nRIMAPI_API_TOKEN=file-secret\n")
            with patch.dict(os.environ, {}, clear=True):
                loaded = Settings.load(env)
                self.assertEqual(loaded.token, "file-secret")
                self.assertNotIn("file-secret", repr(loaded))
            with patch.dict(os.environ, {"RIMAPI_API_TOKEN": "environment-secret"}, clear=True):
                self.assertEqual(Settings.load(env).token, "environment-secret")

    def test_missing_explicit_env_file_fails(self):
        with self.assertRaises(ValueError):
            Settings.load(Path("/nonexistent/rimapi.env"))

    def test_query_body_token_and_idempotency_are_preserved(self):
        response = self.api.call("POST", "/api/v1/echo", {"speed": 0, "confirm": True, "ids": [1, 2]},
                                 {"pawn_id": "321", "dry_run": True}, "same-operation")
        self.assertTrue(response["ok"])
        record = response["response"]["data"]
        self.assertEqual(record["authorization"], "Bearer test-secret")
        self.assertEqual(record["user_agent"], "rimapi-mcp/1.0")
        self.assertEqual(record["key"], "same-operation")
        self.assertEqual(parse_qs(urlsplit(record["path"]).query), {"speed": ["0"], "confirm": ["true"], "ids": ["1", "2"]})
        self.assertEqual(record["body"], {"pawn_id": "321", "dry_run": True})

    def test_scalar_json_bodies_are_preserved(self):
        for body in [False, 0, [1, 2], "label"]:
            response = self.api.call("POST", "/echo", body=body)
            self.assertEqual(response["response"]["data"]["body"], body)

    def test_missing_and_wrong_tokens_are_errors(self):
        for token in ["", "wrong"]:
            response = GameAPI(Settings(self.base, token)).call_result("GET", "/game/state")
            self.assertTrue(response.is_error)
            self.assertEqual(response.structured_content["http_status"], 401)

    def test_http_and_api_errors_are_not_success(self):
        for path, status in [("/conflict", 409), ("/api-failure", 200)]:
            response = self.api.call_result("POST", path)
            self.assertTrue(response.is_error)
            self.assertEqual(response.structured_content["http_status"], status)

    def test_redirect_does_not_forward_credentials(self):
        response = self.api.call("GET", "/redirect")
        self.assertFalse(response["ok"])
        self.assertEqual(response["http_status"], 302)
        self.assertEqual(len(APIHandler.calls), 1)

    def test_write_timeout_is_unknown_and_not_retried(self):
        response = GameAPI(Settings(self.base, "test-secret", timeout=.03)).call("POST", "/slow")
        self.assertFalse(response["ok"])
        self.assertEqual(response["outcome"], "unknown")
        self.assertEqual(len(APIHandler.calls), 1)

    def test_requests_cannot_change_target_or_escape_api(self):
        for path in ["https://example.com/", "//example.com/", "/../.env", "/%2e%2e/.env", "/state?token=x"]:
            with self.assertRaises(ValueError):
                self.api.call("GET", path)
        self.assertEqual(APIHandler.calls, [])

    def test_reference_contract_and_secret_boundary(self):
        refs = References()
        found = refs.search("/api/v1/game/speed")
        self.assertTrue(found["matches"])
        first = found["matches"][0]
        self.assertIn("/api/v1/game/speed", refs.read(first["path"], first["line"], 10)["text"])
        with self.assertRaises(ValueError):
            refs.read(".env")

    def test_reference_index_resolves_contract_and_controller(self):
        refs = References()
        found = {hit["path"]: hit for hit in refs.search("/pen/food", limit=50)["matches"]}
        for path in ("docs/api/routes.md", "docs/api/animal-pens.md"):
            self.assertIn("/pen/food", refs.read(path, found[path]["line"], 1)["text"])
        export = refs.search("/api/v1/core/docs/export")["matches"]
        self.assertIn("Source/RIMAPI/RimworldRestApi/BaseControllers/DocumentationController.cs",
                      {hit["path"] for hit in export})

    def test_official_stdio_protocol_tools_auth_and_mutation_roundtrip(self):
        with tempfile.TemporaryDirectory() as tmp:
            env = Path(tmp) / ".env"
            env.write_text(f"RIMAPI_BASE_URL={self.base}\nRIMAPI_API_TOKEN=test-secret\n")
            with patch.dict(os.environ, {}, clear=True):
                summary = asyncio.run(check(env, settings_roundtrip=True))
        self.assertTrue(summary["ok"])
        self.assertTrue(summary["screenshot"]["image"])
        self.assertTrue(summary["settings_roundtrip"]["restored"])
        self.assertTrue(APIHandler.background)
        self.assertNotIn("test-secret", json.dumps(summary))


if __name__ == "__main__":
    unittest.main()
