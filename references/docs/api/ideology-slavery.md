# Ideology 囚犯奴役（ID-08b 限定）

覆盖把**已破意志**（will=0）的玩家囚犯转换为奴隶的两条原生路径：

- `use_warden_job=true`：设置原生 Enslave 互动模式并提交
  `JobDefOf.PrisonerEnslave`，由狱卒的 `JobDriver_EnslaveOrReduceWillPrisoner`
  跑社交循环，最后一次 `Toils_Interpersonal.TryEnslave` 调
  `GenGuest.TryEnslavePrisoner`；
- 默认（false）：直接调用 `GenGuest.TryEnslavePrisoner` 的转换点，作为显式非工作路径。

两者都读取奴隶/囚犯状态；不改奴隶工作/抑制策略。

## 读取

`GET /api/v1/dlc/ideology/slavery?map_id=0`

```json
{
  "map_id": 0,
  "tick": 75153,
  "pawn_ids": ["Human88473"],
  "pawns": [{
    "pawn_id": "Human88473",
    "name": "Lissa",
    "faction": "PlayerColony",
    "guest_status": "Slave",
    "is_prisoner": false,
    "is_slave": true,
    "will": 0.0,
    "resistance": 0.0,
    "enslave_interaction": true,
    "exclusive_interaction": "Enslave",
    "suppression": 0.5,
    "downed": false,
    "awake": true
  }]
}
```

- `guest_status` 是原生 `GuestStatus`；`is_prisoner`/`is_slave` 使用
  `IsPrisonerOfColony`/`IsSlaveOfColony`。
- `suppression` 是原生 `Need_Suppression.CurLevelPercentage`（奴隶才有）。
- `enslave_interaction`/`exclusive_interaction` 来自原生互动模式。

## 奴役

`POST /api/v1/dlc/ideology/slavery/enslave`

```json
{"map_id": 0, "pawn_id": "88473", "warden_id": "318",
 "use_warden_job": true, "dry_run": false}
```

- `pawn_id` 必须是同图、存活的玩家囚犯（`GuestStatus.Prisoner`），且不是奴隶；
  已奴隶 409，非囚犯 422。
- `will` 必须为 0（原生 `InteractionWorker_EnslaveAttempt` 的转换条件）；>0 时
  422 `prisoner will is not broken; reduce will to 0 before enslaving`，不代做
  多日降意志。
- `warden_id` 省略时选同图最近、可对话、可达的玩家殖民者；显式 warden 必须同图、
  非倒地/非精神、可对话、可达囚犯；未知 404。
- 校验通过后设置原生 `Enslave` 互动模式：
  - `use_warden_job=true` 时 `warden.jobs.TryTakeOrderedJob(PrisonerEnslave)`，
    返回 `job_submitted/job_def=PrisonerEnslave/applied=false/method`；
    身份转换由后续原生 job 完成，调用方需推进游戏并复核；
  - 默认直接调用 `GenGuest.TryEnslavePrisoner(warden, prisoner)`，返回
    `method/converted/applied/after`。
  - `dry_run` 只读 `would_convert` 或 `would_submit_job`，不改身份/模式。
- 不直接写 `GuestStatus`/`Hediff`；转换、阵营/压制/释放逻辑均在原生方法内。

## 正常调用顺序

1. 用既有囚犯/教化接口保证该囚犯是玩家囚犯，并把 will 降到 0（本接口不降 will）。
2. `POST .../slavery/enslave`，读 `after.guest_status == "Slave"`。
3. `GET .../slavery` 复核 `is_slave:true`、`suppression` 与 `will`。

## 边界与证据

- 2026-11-10 实测：`Human88473`（OutlanderCivil 囚犯，will/resistance 0）→
  `GenGuest.TryEnslavePrisoner`，after/GET 均为 `Slave`、`is_slave:true`、
  suppression 0.5、faction PlayerColony；再奴役 409、殖民者 422、未知 404、
  空 body 400、未知 warden 404
  （`Reverse/evidence/id08b-slavery-20261111.json`，自审
  [ID-08b](../references/reviews/2026-11-10-id08b-slavery.md)）。
- 2026-11-11 补：`use_warden_job=true` 的正常工作链已实测——`Human88490`
  在 5×5 囚室（fixture 场景）由狱卒 `Human66734` 执行 `PrisonerEnslave`，
  tick 76197 转 Slave，`suppression=0.5`，GET/native 一致
  （`Reverse/evidence/id08b-enslave-work-20261111.json`，自审
  [ID-08b](../references/reviews/2026-11-11-id08b-enslave-work.md)）。
- 未覆盖：越狱/精神中断/奴隶反叛、持续抑制策略循环、奴隶工作类型/装备管理、
  任务囚犯；这些仍是 ID-08b 的剩余项。
- 只处理玩家囚犯；非玩家囚犯、奴隶反叛、任务囚犯未验。

解放：`POST /api/v1/dlc/ideology/slavery/emancipate`
`{map_id,slave_id,warden_id,dry_run}`，调用原生 `GenGuest.EmancipateSlave`；
响应 `released:true`，奴隶身份保留到离场。实测 Miriam/Human88473（`Reverse/evidence/id08b-emancipate-20261111.json`，
自审 [解放](../references/reviews/2026-11-10-id08b-emancipate.md)）。离场/再俘获未验。

监禁：`POST /api/v1/dlc/ideology/slavery/imprison`
`{map_id,slave_id,warden_id,dry_run}`，查找原生牢床后排队 `Arrest` job；无床 409。
实测 3000 tick 后 Prisoner/secure true、slave false（`Reverse/evidence/id08b-imprison-20261111.json`，
自审 [监禁](../references/reviews/2026-11-10-id08b-imprison.md)）。无床/中断/越狱未验。

压制：`POST /api/v1/dlc/ideology/slavery/suppress`
`{map_id,slave_id,warden_id,dry_run}`，排队原生 `SlaveSuppress`；实测 suppression
0.3→0.396、last tick 更新（`Reverse/evidence/id08b-suppress-20261111.json`，
自审 [压制](../references/reviews/2026-11-10-id08b-suppress.md)）。无 need/未调度未验。

降低意志：`POST /api/v1/dlc/ideology/prisoner/reduce-will`
`{map_id,prisoner_id,warden_id,dry_run}`，排队原生 `PrisonerReduceWill`；实测
will 1→0.5475、mode NoInteraction（`Reverse/evidence/id08b-reduce-will-20261111.json`，
自审 [ReduceWill](../references/reviews/2026-11-10-id08b-reduce-will.md)）。
will=0/未调度/无 warden 未验。
