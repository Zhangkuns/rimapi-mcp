# RIMAPI MCP

[English](README.md) | 简体中文

让 AI 客户端通过现有 RIMAPI REST API 观察和操作 RimWorld。适配器在本地以 stdio MCP 运行，要求 Python 3.11 及以上，使用官方 Python MCP SDK。客户端电脑无需安装 RimWorld 或 Xvfb。

```text
AI 客户端 → 本地 stdio MCP → HTTPS / Tailscale / localhost → RimWorld API
浏览器    → Cloudflare Access → Tunnel → RimWorld 游戏画面（可选）
```

游戏电脑需先运行兼容的 RIMAPI 服务。适配器支持公网 HTTPS API、可达的 Tailscale 主机及本机 API。网页画面及其登录认证由游戏端单独配置。

## 安装

使用 RIMAPI 源码树中的 `mcp/`，或克隆独立包：

```sh
git clone https://github.com/Zhangkuns/rimapi-mcp.git
cd rimapi-mcp
```

在 `rimapi_server.py` 所在目录安装依赖，首次创建配置时保留已有 `.env`：

```sh
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
cp -n .env.example .env
chmod 600 .env
```

## 配置私有连接

已有 HTTPS 部署时，在 `.env` 中填写：

```dotenv
RIMAPI_BASE_URL=https://rimapi.example.com/api/v1
RIMAPI_API_TOKEN=replace-with-your-game-token
RIMAPI_VIEW_URL=https://rimworld.example.com/
RIMAPI_TIMEOUT_SECONDS=30
```

将示例域名和 Token 替换为自己的值。`RIMAPI_API_TOKEN` 是游戏 API 的 Bearer Token，不是 Cloudflare 管理 API Token。实际地址和凭据保存在私有 `.env` 中。

| 配置 | 用途 |
| --- | --- |
| `RIMAPI_BASE_URL` | HTTP(S) API 主机，可包含 `/api/v1` 后缀 |
| `RIMAPI_API_TOKEN` | 与游戏端或公网网关一致的 Token，通过 `Authorization: Bearer …` 发送 |
| `RIMAPI_VIEW_URL` | 可选的浏览器观看地址，不参与 API 请求 |
| `RIMAPI_TIMEOUT_SECONDS` | 请求超时秒数，默认 30 |

本机游戏可使用 `http://localhost:8765/api/v1`。Tailscale 部署使用可达的游戏主机名及已配置端口，例如 `http://game-host:18765/api/v1`，客户端需有对应 tailnet 访问权限。通过公网 HTTPS API 连接时，客户端无需接入 Tailscale。

默认读取 `rimapi_server.py` 旁的 `.env`，不依赖终端当前目录；也可用 `--env-file /absolute/path/to/.env` 指定其他文件。已有环境变量优先。修改后重连 MCP，让进程重新读取配置。

只有服务器允许未认证访问时，Token 才能留空。只填写客户端 Token 不会开启游戏端校验。适配器不会创建 Tunnel、DNS、Access 策略或游戏画面服务。

## HTTPS 与画面登录

部署端可通过 Cloudflare Tunnel 发布两个独立域名，并让 API 代理、noVNC 和 VNC 保持 loopback 监听：

| 入口 | 认证方式 |
| --- | --- |
| 游戏画面 | Cloudflare Access 登录，例如邮箱白名单加一次性验证码；页面、静态资源和 WebSocket 握手均需保护 |
| REST API | 每个公网 API 请求携带 Bearer Token，无需交互式网页登录 |

采用此方案时，应先配置 Access 和源站校验，再发布画面域名。公网 API 网关需统一校验所有路径，包括游戏原生免鉴权的 `/version`、`/capabilities`。这些是部署要求，适配器不会替网关启用校验。

网页登录和 API 认证分别配置。游戏 Token 不能登录画面，浏览器的 Access 会话也不会替此适配器完成认证。`RIMAPI_BASE_URL` 填 API 域名；在浏览器打开 `RIMAPI_VIEW_URL` 登录画面。适配器的 API 请求使用 `rimapi-mcp/1.0` 作为 User-Agent。

本仓库提供 **stdio MCP**，客户端需要能启动本地 Python 进程。只接受远程 HTTP MCP 地址的云端助理，还需另行实现并鉴权的 HTTP MCP 服务，或其支持的本地执行桥接。将 REST API 暴露公网不会自动生成 HTTP MCP 端点，也不代表 Dots 已接通。

## 接入 MCP 客户端

注册本地 stdio 服务，使用虚拟环境 Python、脚本及私有 `.env` 的绝对路径：

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

本机使用 Codex CLI 时也可以注册：

```sh
codex mcp add rimapi -- \
  /absolute/path/to/rimapi-mcp/.venv/bin/python \
  /absolute/path/to/rimapi-mcp/rimapi_server.py \
  --env-file /absolute/path/to/rimapi-mcp/.env
```

## 检查连接与游玩

```sh
.venv/bin/python rimapi_server.py --check
.venv/bin/python smoke.py
```

`--check` 同时读取发现接口和受保护游戏状态，报告鉴权及凭据状态，不输出 Token。`smoke.py` 检查官方 MCP 握手、工具目录、接口与参考查询以及真实 MCP 截图；未加载地图时会如实记录截图错误。可选的 `--settings-roundtrip` 会切换并恢复游戏的后台运行设置。

让 AI 首先调用 `rimapi_status` 和 `rimapi_gameplay_guide`，确认游戏实例、已加载存档和鉴权，再观察、操作并读回结果。详见[正常游玩说明](PLAYING.md)。

| 工具 | 用途 |
| --- | --- |
| `rimapi_status` | 版本、DLC、鉴权与受保护游戏状态 |
| `rimapi_search_endpoints` | 按路径、方法或功能搜索所连接游戏的 `/capabilities` |
| `rimapi_search_reference` / `rimapi_read_reference` | 搜随包合同、静态路由表、控制器及请求 DTO；按文件路径和行号读取 |
| `rimapi_read` / `rimapi_write` | GET 或显式 POST/PUT/PATCH/DELETE；`query` 与 JSON `body` 分开传 |
| `rimapi_screenshot` | 地图加载后返回真正的 MCP 图片 |
| `rimapi_gameplay_guide` | 阅读正常玩法说明 |

`rimapi_status` 后先查实时目录，再按具体路由查参考，读清合同后调用并读回。
目录中 `parameters` 为空不代表无参数；开发者动作还要查 `/debug/native/actions`。

`rimapi_read` 发送 GET，但不保证无副作用（`/core/docs/export` 会在游戏主机写文件）。
SSE（`/api/v1/events`）请用直接 HTTP 流式客户端。写入不自动重试；超时后先查真实状态，
只在路由支持时使用 `idempotency_key`。HTTP 200 或 `queued` 不代表人物工作已完成。
畜牧及其他示例见[游玩指南](PLAYING.md)。

## API 与参考更新

新增 REST 路由无需逐条加 MCP 工具；通用读写工具可转发，但游戏必须加载实现它的 DLL。
更新 MCP 或参考不会升级游戏。先核所连接实例的版本和实时目录。

源码版读取工作区内白名单文档及 C# 文件，独立包读取 `references/` 快照。
安装 MCP 依赖后，从更新后的 RIMAPI 仓库根目录生成一份新包：

```sh
python3 tools/generate_api_routes.py --check
mcp/.venv/bin/python mcp/package.py /absolute/path/to/new-rimapi-mcp
```

客户端更新时保留私有 `.env`，重新连接 MCP 后才会索引新增参考路径。
可用参考工具搜索 `docs/api/index.md` 或 `docs/api/routes.md`。

## 排障

| 现象 | 检查内容 |
| --- | --- |
| API 返回 401 | Token 是否与游戏/网关一致，是否重连读取 `.env`，是否有环境变量覆盖 |
| API 返回 302 或登录页 | 是否填对 API 域名和网关认证方式；适配器不会进行 Access 网页登录或跟随重定向 |
| API 返回 403 或 Cloudflare 1010 | 更新适配器，并检查对应 Cloudflare 安全事件 |
| 连接失败 | 游戏、API 网关、Tunnel 是否运行，域名是否可达；使用 tailnet 地址时检查 Tailscale |
| AI 无法启动 MCP | 客户端是否支持本地 stdio 服务，所填绝对路径是否正确 |

## 隐私、测试与来源

`.gitignore` 排除 `.env` 及备份、个人 `mcp-config.json`、`.venv` 和运行记录；示例文件仅包含通用值。实际域名、IP、邮箱、Token 和 Tunnel 凭据不要提交，发布前用 `git status --short` 检查。

独立仓库的 `references/` 包含接口文档及控制器/DTO 源码快照，只作参数参考，不在客户端执行。实际可用路由取决于所连接游戏的版本。

```sh
.venv/bin/python -m unittest discover -s . -p 'test_*.py' -v
```

测试使用本地 HTTP fixture，无需启动游戏。`package.py DESTINATION` 生成含双语 README 和接口参考的独立副本，不打包私有配置或环境。适配器来自 RIMAPI 工程的 `mcp/`；接口参考来源为 `AppSprout-dev/RIMAPI` 及其后续开发。独立包的 `LICENSE` 保留原 GPL 授权条款。
