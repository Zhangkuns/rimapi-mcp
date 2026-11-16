# Royalty 任务奖励偏好（RY-R07）

2026-11-12 新增：读写阵营的 `allowRoyalFavorRewards` 与
`allowGoodwillRewards`，即 `Dialog_RewardPrefsConfig` 的两个原生开关。

## 读取

`GET /api/v1/dlc/royalty/reward-prefs` → `factions[]`（玩家阵营除外）：
`faction_id(def unique load id)/def_name/name/has_royal_titles/
allow_royal_favor_rewards/allow_goodwill_rewards/goodwill`。

## 设置

`POST /api/v1/dlc/royalty/reward-prefs`

```json
{"faction_id":12,"allow_royal_favor_rewards":false,
 "allow_goodwill_rewards":true,"dry_run":true}
```

- 至少给一个开关；`faction_id` 是 `Faction.GetUniqueLoadID` 数值部分？
  实际按 `loadID` 整数匹配；未知 404；玩家阵营 422。
- 返回更新前后字段和 `applied`；dry_run 不写。
- 合同：GET observe；POST policy/game_ai/validate/game_memory。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
读取有皇家头衔的阵营；dry 保持；正式取反两个开关后 GET 精确一致；随后恢复。
证据 `tools/cua/.runtime/ry-r07-20261112/evidence/ry-r07.json`，runner
`tests/ry_r07_reward_prefs.py`。

- 未测：后续原生任务生成真正按偏好避开/提供奖励（`RewardsGenerator` 行为）、
  多阵营、无头衔阵营；本行只补偏好字段与读回。
