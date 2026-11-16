# Core 正常主动求爱（C-40）

2026-11-12 新增：

- `GET /pawn/romance?pawn_id&target_id`：原生 love 关系、opinion、
  `CanInteractNowWith(RomanceAttempt)`、`SuccessChance`。
- `POST /pawn/romance`：校验人类、同图、存活、非血亲、无既有恋爱关系、
  可触及和原生交互上下文；创建 `JobDefOf.TryRomance` 并设置
  `job.interaction = InteractionDefOf.RomanceAttempt`，走正常 job。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选无血亲/无恋爱关系的两名殖民者，opinion 44。dry 不写；job 提交；
推进 3000 tick 后 PlayLog 出现 `romance attempt: Patrick->Youko ...
turned him down`，证明原生 RomanceAttempt 交互完成。拒绝/接受按原生随机。
证据 `tools/cua/.runtime/c40-20261112/evidence/c40.json`，runner
`tests/c40_romance.py`，fixture `prepare_romance/observe_romance`。

- 未追求随机接受/关系成立；本行以正常 job 与原生交互日志为准。
