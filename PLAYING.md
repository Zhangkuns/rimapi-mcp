# RimWorld MCP 游玩说明

维护：Codex；核对：2026-11-16；状态：客户端使用说明。

你控制的是 `rimapi_status` 报告的独立 RimWorld 实例。网页画面供人观看；AI 主要通过结构化 API 观察与决策，加载地图后可调用 `rimapi_screenshot` 取得真正的 MCP 图片。

## 开始

1. 调用 `rimapi_status`，确认连接、`auth_enabled`、DLC、实例状态。`credential_configured` 只表示客户端有密钥，不表示服务器已启用校验。主菜单的 `/game/state` 可能是 `NotInitialized` 默认值，不能据此宣称已载入存档。
2. `rimapi_search_endpoints(query="game start")` 或 `query="game/saves"` 查新开局/存档能力。按用户目标新开局或选择已有存档，正常游戏不使用 devquick、测试 fixture 或刷资源。
3. `rimapi_search_reference(query="/api/v1/game/start")` 找合同、控制器与请求 DTO，随后 `rimapi_read_reference` 阅读参数。运行时目录的 `parameters:[]` 不表示没有 body/query。
4. 读取实际地图、人物、资源、工作、建筑、信件和任务；按具体目标只查需要的数据，优先范围/批量 API。

## 请求例子

工具参数的 `path` 可以是 `/game/state` 或 `/api/v1/game/state`，查询参数单独放 `query`，JSON 单独放 `body`。

```json
{"path":"/game/state"}
```

暂停使用 `rimapi_write`。这个旧接口的 speed 在 query，不能猜成 JSON body：

```json
{"method":"POST","path":"/game/speed","query":{"speed":0}}
```

其他接口必须按具体合同选择参数位置。`dry_run:true` 只放 JSON body，且仅在明确支持的接口使用。旧接口不因接入 MCP 就具备预演或幂等。

## 观察、操作、读回

先查询准确实例 ID/资格/资源，再决定和提交动作。把接受、排队、原生工作执行中、完成与失败分别处理；HTTP 200 或 operation 事件不是建设、手术、研究等工作的完成证据。

需要推进时间时先设定终态、tick 与墙钟上限，选择正常速度，短段推进、查询威胁/人物状态并暂停再决策。不要一次放任游戏运行很久。暂停/继续是正常玩法，不借调试直接授予材料、治疗、奖励或触发任务来完成正常目标。

写入超时的结果未知，适配器不会自动重发。先读真实状态；只有该 method/path 声明支持时才按 Idempotency-Key 合同重放相同操作。已有 key 的请求体不得修改。

正常玩法、环境工具与 debug 按具体行为区分，不能只看路由前缀或 `layer`；例如 direct 并不总是作弊，GET 目录也不证明后续操作符合正常取得流程。只有用户明确要求调试时才使用调试直接变更。

## 连接边界

MCP 以本地 stdio 运行，由客户端电脑启动；API 请求经已配置的 HTTPS、Tailscale 或 localhost 发往游戏。公网 API 可达不代表云端助理已有 HTTP MCP 端点；云端客户端还需支持本地执行桥接或另行部署的 HTTP MCP 服务。MCP 不公开新的监听端口，不提供 shell 或任意目标 URL。

网页画面是可选的人类观看入口；使用 Cloudflare Access 时先在浏览器登录。画面登录和 API Bearer 分别校验，`RIMAPI_VIEW_URL` 不参与 API 请求。

`.env` 默认从适配器所在目录读取，与终端当前目录无关；已有环境变量优先。Token 不放入提示词、工具参数或工具结果。游戏端是否启用 Bearer 由游戏 Mod 设置/游戏进程环境决定，客户端填 Token 不会替服务器开启鉴权。
