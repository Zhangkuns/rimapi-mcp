# 正常物品使用（CompUsable / CompTargetable）

`POST /api/v1/pawn/use-item` 镜像原生物品浮动菜单：资格、确认、可选目标与 `count=1` 的
原生 job。它不是直接改 hediff/物品；效果与消耗由原生 job 完成。

## 请求

```json
{"pawn_id":"321","thing_id":"VoidsightSerum75550","target_thing_id":null,"confirm":false,"dry_run":true}
```

| 字段 | 说明 |
| --- | --- |
| `pawn_id` | 使用者；必须是已生成、可操作且能触达物品的玩家殖民者 |
| `thing_id` | 物品 ThingID 或数字 thingIDNumber；先查地图，再查人物背包 |
| `target_thing_id` | `CompTargetable` 物品必填（如食尸鬼复活血清的目标尸体）；其它物品不可带 |
| `confirm` | 原生 `CompUseEffect.ConfirmMessage` 非空时必须为 true |
| `dry_run` | 只做资格/目标/确认文本计算，不建 job、不设原生 selectedTarget |

## 行为

- 无 `CompUsable` 或 `useJob` 400；资格拒绝来自 `CompUsable.CanBeUsedBy(pawn, forced:false)`，
  含可达、预留、变体、星球层与所有 `CompUseEffect.CanBeUsedBy`（422）。
- 目标物品先 `CompTargetable.ValidateTarget`（如 `CompTargetable_SingleCorpse` 的
  corpse/mutantFilter/flesh 条件，422），再通过原生 `OrderForceTarget` 写入
  `selectedTarget` 并提交 job。
- 确认文本非空且未确认时 409，`data.confirmation_required=true` 与 `data.confirmation`。
- 提交后返回 `job_def`、`job_started`、`job_queued`；两者皆无 409。
- 精确合同：`policy/game_ai/validate/game_memory`；带 `Idempotency-Key` 的重放返回原 raw，
  同 key 异体 409，成功写发布一条 operation。

## 证据与边界

- 2026-11-10 自审：`VoidsightSerum` 由正常账单生产、原生 `Ingest` 消耗并获得 `Voidsight`；
  食尸鬼复活血清对准确 Ghoul 尸体完成原生 `UseItem`→`Resurrect`、血清消耗与 `ResurrectionComa`
  （`Reverse/evidence/an05ab-serum-20261110.json`、`an05d-ghoul-resurrect-20261110.json`）。
- 边界：目标选择依赖原生 `CompTargetable.caster` 私有字段（缺失时 501）；未逐个验证全部
  `CompTargetable` 子类；敌对/不可达目标只走原生拒绝。
