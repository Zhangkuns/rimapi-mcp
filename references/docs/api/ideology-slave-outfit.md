# Ideology 奴隶服装政策指派（ID-R09）

2026-11-12 修正：原 `/pawn/outfit` 只允许 `IsFreeColonist`，现在按原生
`PawnColumnWorker_Outfit` 的资格放开普通奴隶；任务借宿者
（`IsQuestLodger`）仍拒绝。指派后由游戏 AI 按政策自主换衣。

## 调用

`GET /api/v1/pawn/outfit?pawn_id=` → `pawn_id/outfit_id/label`
`GET /api/v1/outfits` → 可用政策列表
`POST /api/v1/pawn/outfit` `{pawn_id,outfit_id,dry_run}` → `previous_outfit_id/
outfit_id/label/changed/dry_run`

- 要求 spawned 的殖民者或奴隶且 `pawn.outfits != null`；任务借宿者 422。
- 仅指派已有 apparel policy，不直接改服装；实际穿衣由原生 AI 执行。
- 保留既有殖民者指派合同。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 使用现有奴隶或生成一名村民并设原生 `GuestStatus.Slave`
（`created:true`，准备披露）。初始 `Anything(id1)`；dry 指派 `Worker(id2)`
返回 `changed:true/dry_run:true` 但未写；正式指派后 GET `outfit_id:2/Worker`；
随后恢复 Anything。
证据 `tools/cua/.runtime/id-r09-20261112/evidence/id-r09.json`，runner
`tests/id_r09_slave_outfit.py`，fixture `prepare_slave_outfit`/`observe_slave_outfit`。

- 未测：任务借宿者拒绝、奴隶实际自主换衣结果、囚犯/非玩家人物、装备强制层；
  本行只补资格与政策指派读回。
