# Ideology 囚犯教化互动模式（G-80）

交付：把殖民地囚犯的原生互动模式设为 `Convert`，并设置 `ideoForConversion`；随后由
原生 `WorkGiver_Warden_Convert`/`JobDriver_ConvertPrisoner` 让有对应 ideo 的狱卒执行
3 次交谈 + 一次 `ConvertIdeoAttempt`。这是正常玩家在囚犯面板选择“convert”的 API 化，
不是直接 `SetIdeo`，也不是 G47 的 Convert 能力。

## 设置

`POST /api/v1/dlc/ideology/prisoner/convert`

```json
{"pawn_id":"Human88473","ideo_id":"3","dry_run":true}
```

- 原生调用：`Pawn_GuestTracker.SetExclusiveInteraction(PrisonerInteractionModeDefOf.Convert)`
  + `guest.ideoForConversion = ideo`。
- `ideo_id` 是原生 `Ideo.id` 字符串；省略时使用玩家派系 `PrimaryIdeo`。
- 资格：spawned、living、humanlike、`IsPrisonerOfColony`、`guest.PrisonerIsSecure`、
  有 ideo；目标 ideo 必须存在且与当前 ideo 不同。非囚犯/死亡/无 ideo 422，未知 ideo 404，
  已是目标 ideo 409。
- dry-run 只返回 `mode/ideo/changed`，不写 guest；正式写入后读回
  `mode_after/ideo_after/readback_ok`。`applied:true` 只表示模式/ideo 已写入，不表示
  已经完成教化；实际改信由狱卒工作完成。
- `Idempotency-Key` 按通用 `game_memory` 合同；同 key 重放返回原正文。

## 实测

隔离 build/profile/mod，复制 `ai_g31_fixture.rws`（tick 74742），保护开启。fixture 建一个
7x7 监狱房间（墙/门/混凝土/`ForPrisoners` 床），生成一名不同 ideo、certainty 0.01 的
殖民地囚犯，选一名玩家 primary ideo、Talking 的狱卒并把 Warden 工作设为可用。

- dry-run 200/applied false/mode `Convert`；
- 正式 200/applied true/readback true，`mode_after=Convert`、`ideo_after=玩家 primary`；
- fixture observe：`prisoner_mode=Convert`、`ideo_for_conversion=玩家 primary`；
- 推进后 `prisoner_converted`：囚犯 ideo 变为玩家 primary，仍为囚犯。

证据：`Reverse/evidence/dlc-G80-prisoner-convert.json`、
`dlc-G80-prisoner-convert.json.http.json`；自审
[G80](../references/reviews/2026-11-08-g80-prisoner-convert.md)。

## 限制

- 只验殖民地囚犯；奴隶互动模式、囚犯越狱/精神/互动禁用、无狱卒/无对应 ideo 狱卒、
  多狱卒/多囚犯、自然 UI 面板未验。
- 本端点不执行狱卒 job，也不保证教化成功；成功/失败由原生 `ConvertIdeoAttempt` 决定。
- 未做 Cua 对照；不把本样本当作囚犯模块全功能验收。
