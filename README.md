# RIMAPI MCP

通过现有 RimWorld REST API 观察和操作游戏的本地 stdio MCP 服务。客户端支持 Python 3.11 及以上，使用官方 Python MCP SDK；无需在客户端电脑安装游戏或 Xvfb。

```text
AI 客户端 → 本地 stdio MCP → HTTP / Tailscale → RimWorld API
```

游戏电脑需要运行兼容的 RIMAPI 服务。跨机器连接时，两台电脑应能通过 Tailscale 或其他已配置的网络互通。本项目不自动发布公网端口，也不替游戏配置认证或网页画面。

## 安装与私有配置

在本目录运行：

```sh
python3 -m venv .venv
.venv/bin/python -m pip install -r requirements.txt
cp .env.example .env
chmod 600 .env
```

在 `.env` 中填写自己的连接信息：

```dotenv
RIMAPI_BASE_URL=http://game-host:18765/api/v1
RIMAPI_API_TOKEN=
RIMAPI_VIEW_URL=
RIMAPI_TIMEOUT_SECONDS=30
```

将 `game-host` 替换为游戏电脑的实际主机名或 Tailscale 地址；`RIMAPI_API_TOKEN` 必须与游戏端配置一致。游戏未启用鉴权时可留空；只填写客户端 Token 不会开启游戏端校验。`RIMAPI_VIEW_URL` 是可选的人类观看地址。

默认加载脚本旁的 `.env`，不依赖终端当前目录；也可用 `--env-file /absolute/path/to/.env` 指定。已有环境变量优先。修改配置后重连 MCP，或新建客户端任务，使进程重新读取配置。

实际地址、密钥、个人 `mcp-config.json`、`.env` 及其备份、`.venv` 和运行记录均由 `.gitignore` 排除。提交前可用 `git status --short` 检查待提交内容；仓库中的 `.env.example` 仅包含通用示例。

## 接入 MCP 客户端

配置本地 stdio 服务，命令使用本目录虚拟环境中的 Python，参数使用脚本及私有 `.env` 的绝对路径。例如：

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

支持 Codex CLI 的环境也可以注册：

```sh
codex mcp add rimapi -- \
  /absolute/path/to/rimapi-mcp/.venv/bin/python \
  /absolute/path/to/rimapi-mcp/rimapi_server.py \
  --env-file /absolute/path/to/rimapi-mcp/.env
```

该服务需要由能访问游戏电脑的本地客户端启动。云端 AI 不会自动继承另一台电脑上的 stdio 配置。

## 检查连接与使用

```sh
.venv/bin/python rimapi_server.py --check
.venv/bin/python smoke.py
```

连接检查同时读取发现接口和受保护状态，报告服务端鉴权与客户端凭据状态，不输出 Token。`smoke.py` 使用官方 MCP 客户端测试握手、工具目录、接口与参考查询及截图；游戏主菜单无地图时会如实记录截图错误。`--settings-roundtrip` 可选地切换并恢复游戏的后台运行设置。

让 AI 首先调用 `rimapi_status` 和 `rimapi_gameplay_guide`，确认准确游戏实例、存档、鉴权和正常玩法要求，然后根据用户目标查询、操作并读回结果。

| 工具 | 用途 |
| --- | --- |
| `rimapi_status` | 版本、DLC、鉴权与受保护游戏状态 |
| `rimapi_search_endpoints` | 搜索游戏实时 API 路由与合同字段 |
| `rimapi_search_reference` / `rimapi_read_reference` | 查随包接口合同、示例、控制器及请求 DTO |
| `rimapi_read` / `rimapi_write` | GET 与显式写入，分别传递 query、JSON body 和可选幂等键 |
| `rimapi_screenshot` | 地图加载后返回真正的 MCP 图片 |
| `rimapi_gameplay_guide` | [正常游玩说明](PLAYING.md) |

请求只连接配置的游戏，不提供任意目标 URL 或 shell。写入不自动重试；超时表示结果未知，需先查询真实状态。HTTP 200 或 queued 不能证明原生工作已完成。具体参数从参考工具查阅，不能把实时目录中空的 parameters 当作无参数。

## 参考、测试与来源

独立仓库的 `references/` 包含接口合同、示例及控制器/DTO 源码，只作参数参考，不在客户端执行。它们是特定 RIMAPI 版本的快照，实际可用路由以所连接游戏的目录为准。

```sh
.venv/bin/python -m unittest discover -s . -p 'test_*.py' -v
```

这些测试使用本地 HTTP 服务，无需启动游戏。该适配器来源于 RIMAPI 工程的 `mcp/`，接口参考来源为 `AppSprout-dev/RIMAPI` 及其后续开发；授权信息随独立包的 `LICENSE` 保留。
