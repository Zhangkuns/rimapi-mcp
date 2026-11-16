# Royalty 灵能热量限制器（RY-R01）

2026-11-12 新增：`POST /dlc/royalty/meditation/limiter` 切换
`Pawn_PsychicEntropyTracker.limitEntropyAmount`，即原生
`PsychicEntropyGizmo` 的 Limited/Unlimited 按钮；`GET /dlc/royalty/meditation`
新增 `limit_entropy_amount` 读回。

## 请求

```json
{"pawn_id":"Human321","enabled":false,"dry_run":true}
```

- 要求 pawn 有原生 psylink（`GetPsylinkLevel() > 0`）；写公开字段。
- 返回 `before_limit_entropy_amount/requested_*/limit_entropy_amount/
  entropy_value/max_entropy/applied`；dry_run 不写。
- 合同：POST `policy/pawn_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 使用或添加原生 `PsychicAmplifier` psylink（Human321 level1），初始
limiter true；GET true；dry false 保持；正式 false 后 fixture/GET false；
再 true 后 true。`entropy_value 0/max 30`。
证据 `tools/cua/.runtime/ry-r01-20261112/evidence/ry-r01.json`，runner
`tests/ry_r01_entropy_limiter.py`，fixture `prepare_psylink`/`observe_psylink`。

- 未测：超过 entropy 上限时的原生施放拒绝、不同 psylink 等级、灵能仪式/冥想
  对限制器的依赖；本行只补字段读回。
