# 驯服 / 训练 / 招募

开启游戏的开发者模式后，调用 `POST /api/v1/debug/native/execute` 的 `work.interact`，按 `mode` + `force` 对现有动物或囚犯执行一次原版互动，或在 `force=true` 时走原版 debug 强制成功入口。暂停时也能执行。

## 参数

```json
{"action":"work.interact","map_id":0,"thing_id":"27979","pawn_id":800,"mode":"tame","force":true,"dry_run":false}
```

- `map_id`：整数，必填。
- `thing_id`：目标动物/囚犯的数字 `thingIDNumber` 或完整 `ThingID`，必填。
- `pawn_id`：发起者殖民者的数字 ID，必填。
- `mode`：`tame` | `train` | `recruit`，必填。
- `force`：可选，默认 `false`；仅改变下面各 mode 的强制成功分支。
- `dry_run`：可选，默认 `false`。

## 校验（全部在互动前）

- 目标存在、已生成、未销毁、未死亡；不是 Pawn（如墙）→ 422；
- 发起者存在、已生成、存活、玩家阵营、与目标不同；
- `tame`：目标为动物且不在玩家阵营（已驯服 → 422）；
- `recruit`：目标为人类、`guest != null`、`IsPrisonerOfColony`、`guest.Recruitable`、已启用 `PrisonerInteractionModeDefOf.AttemptRecruit`，且不在玩家阵营；
- `train`：目标为玩家阵营动物、有 `training`、已生成且清醒、`NextTrainableToTrain() != null`（无下一步 → 422）。

`dry_run` 不调用任何原生互动、不掷随机。

## 语义

- **tame**
  - `force=false`：`actor.interactions.TryInteractWith(animal, InteractionDefOf.TameAttempt)`。原版 `InteractionWorker_RecruitAttempt` 按 `TameAnimalChance × Wildness` 曲线（囚室/Bond/崇兽修正）随机；失败返回 `attempt_result=true` 但动物仍野生，并可能触发原版狂躁检查；成功则调用原版 `DoRecruit(..., sendLetter:false)`。
  - `force=true`：`InteractionWorker_RecruitAttempt.DoRecruit(actor, animal, useAudiovisualEffects:true)`，原版把动物加入玩家阵营、设置 Tameness、写 `AnimalsTamed` 记录、按规则判定羁绊/Tale、发消息。
- **recruit**
  - `force=false`：`TryInteractWith(prisoner, InteractionDefOf.RecruitAttempt)`。原版按抵抗值/心情/意见/谈判能力降低 resistance；抵抗为 0 后下一次才招募。
  - `force=true`：`DoRecruit(actor, prisoner, true)`。原版招募：阵营玩家、写 `PrisonersRecruited`、`Recruited` Tale、`RecruitedMe` 记忆、任务信号、PositiveEvent 信件。
- **train**
  - `force=false`：复刻 `Toils_Interpersonal.TryTrain`：`TryInteractWith(animal, TrainAttempt)` 成功后，`TrainAnimalChance × Lerp(0,1,1.5,0.5,Wildness)`，Bond ×5，clamp01；`Rand.Value < chance` 时 `training.Train(next, actor)`、0.007 羁绊、`TrainedAnimal` Tale。
  - `force=true`：直接对 `NextTrainableToTrain()` 调用 `training.Train(next, actor)`，不掷随机。

动作不直接写 faction/training 字段，不伪造强制成功，不批量，也不替代正常驯服/招募 Job。

## 响应

- `mode`、`force`、`initiator`（id/def/label/位置/阵营/存活）。
- `target_before` / `target_after`：id、def、label、位置、`spawned/destroyed/dead/downed/awake`、阵营、`is_animal`、`humanlike`、`is_wildman`、Wildness、`is_prisoner`、`guest_status`、`recruitable`、`resistance`、`interaction_mode`、与发起者的 `bond_to_initiator`/`opinion_of_initiator`、`training`、心情。
- `training_before` / `training_after`：`next`/`next_label`/`next_steps_required`、`wanted[]`、`learned[]`、`steps{}`（按 def 的当前步数；`GetSteps` 为 internal，仅用只读反射读取，不修改）。
- `attempt_result`：非强制路径的 `TryInteractWith` 实际返回；强制路径为 null。
- `success`、`applied`、`changed`、`outcome`（`tamed`/`recruited`/`trained`/`attempted`/`attempt_failed`）。
- `native_methods`、`records_before/after`（`animals_tamed`、`prisoners_recruited`）、`letters_before/after/delta`、`relationship_before/after`。
- 训练时另有 `trained_trainable` / `trained_trainable_label` / `trainable_steps`。
- 预演：`would_attempt=true`（训练另给 `would_train`）。

## 位置与冷却注意

非强制路径完全走原版 `TryInteractWith`，因此要求发起者在目标的互动距离内（原版 `CanInteractNowWith`：目标已生成、位置可见、`CanInitiateInteraction`/`CanReceiveInteraction`），且受原版 120 tick 社交冷却约束；不满足时 `attempt_result=false`、`applied=false`、`outcome=attempt_failed`。本动作不会替调用者移动 pawn 或重置冷却。

## 错误与幂等

- 缺 `map_id`/`thing_id`/`pawn_id`/`mode`，或 `mode` 非法 → 400；地图/目标/发起者不存在 → 404。
- 非 Pawn 目标、错误 mode 目标、已驯服动物、非囚犯/不可招募、无可训练下一步 → 422。
- 开发者模式关闭 → 403。
- 同 `Idempotency-Key` 重放缓存响应（`Idempotency-Replayed: true`），不再第二次尝试/招募/训练；同键换请求 → 409。
- 强制路径若未产生预期阵营/训练变化，返回 `outcome: uncertain` 并读回，不伪造成功。
