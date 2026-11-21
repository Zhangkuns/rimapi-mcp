# RIMAPI MCP

[English](README.md) | 简体中文

让 AI 客户端通过现有 RIMAPI REST API 观察和操作 RimWorld。适配器在本地以 stdio MCP 运行，要求 Python 3.11 及以上，使用官方 Python MCP SDK。客户端电脑无需安装 RimWorld 或 Xvfb。

```text
AI 客户端 → 本地 stdio MCP → HTTPS / Tailscale / localhost → RimWorld API
浏览器    → Cloudflare Access → Tunnel → RimWorld 游戏画面（可选）
```

游戏电脑需先运行兼容的 RIMAPI 服务。适配器支持公网 HTTPS API、可达的 Tailscale 主机及本机 API。网页画面及其登录认证由游戏端单独配置。

## 安装

```sh
git clone https://github.com/Zhangkuns/rimapi-mcp.git
cd rimapi-mcp
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
cp .env.example .env
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
| `rimapi_search_endpoints` | 搜索所连接游戏的实时路由和合同字段 |
| `rimapi_search_reference` / `rimapi_read_reference` | 查随包合同、示例、控制器及请求 DTO |
| `rimapi_read` / `rimapi_write` | GET 与显式写入，分别传递 query、body 和可选幂等键 |
| `rimapi_screenshot` | 地图加载后返回真正的 MCP 图片 |
| `rimapi_gameplay_guide` | 阅读正常玩法说明 |

请求只连接配置的游戏，不提供 shell 或任意目标 URL 工具。写入不自动重试；超时表示结果未知，重试前先查询真实状态。HTTP 200 或 queued 不代表人物工作已完成。选择参数前先读参考，实时目录中空的 `parameters` 不代表接口没有输入。

## API 与参考更新

适配器使用通用读写工具，每次搜索接口时从所连接游戏的 `/capabilities` 发现实时路由。新增路由或 body 字段无需单独新增 MCP 工具。原生 UI 控制包括 `/game/preferences`、`/game/key-bindings`、`/game/play-settings`、`/buildings/bill/options`、`/buildings/bill/configure` 和 `/building/books` / `/building/books/eject`。搜索 `game-preferences`、`ui-option-repairs` 参考阅读合同，写入前先读取当前合法选项。

即时编辑实际屋顶可用 `rimapi_write`，搜索 `roof-grid-debug` 读参数。先用 `/debug/native/actions` 确认游戏已支持 `roof.set` 或 `roof.clear`；旧 DLL 可能有同一个路由，但没有新动作。正常施工仍用 `/order/designate/area` 的 `type:"roof"` 划区。

在 RIMAPI 源码树内，参考发现会纳入当前 API 文档及控制器/DTO；独立包读取随包的 `references/` 快照。API 改动后，应从更新后的 RIMAPI checkout 重新打包、同步客户端安装目录，再重连 MCP 载入新增参考路径。游戏也必须加载对应 DLL：更新 MCP 或参考文件不会升级运行中的游戏。实际可用性以实时接口发现为准。

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
