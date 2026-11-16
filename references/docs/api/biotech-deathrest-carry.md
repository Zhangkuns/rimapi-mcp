# Biotech 死亡休眠搬运（G-52，限定）

交付：读取死亡休眠 pawn 的搬运资格/候选床，以及排队原生 `DeliverToBed` job。
证据：G52 资格/候选床/dry/提交（`Reverse/evidence/dlc-G52-deathrest-carry.json`）；
G70 普通 Bed 回退实际送达（`Reverse/evidence/dlc-G70-deathrest-carry-delivery.json`）；
G72 构造 DeathrestCasket 实际送达（`Reverse/evidence/dlc-G72-deathrest-casket-delivery.json`）。

## 读取

`GET /api/v1/dlc/biotech/life/deathrest/carry?pawn_id=321`

```json
{"pawn_id":"Human321","deathresting":true,"in_bed":false,"carried_by":null,
 "current_bed":null,"can_carry":true,"candidate_carrier":"Human318",
 "candidate_bed":"Bed..."}
```

- 资格同原生 `FloatMenuOptionProvider_CarryDeathrestingToCasket`：`Deathresting`、
  不在床上、是殖民者或殖民地囚犯、有可到达的搬运者。
- 候选床优先玩家 `DeathrestCasket`（`RestUtility.IsValidBedFor`），否则
  `RestUtility.FindBedFor` 回退。

## 搬运

`POST /api/v1/dlc/biotech/life/deathrest/carry`

```json
{"pawn_id":"321","carrier_id":"318","dry_run":true}
```

- 原生路径是 `JobMaker.MakeJob(JobDefOf.DeliverToBed, pawn, bed)`，count 1；
  本端点排队同一 job。`applied` 只表示已提交，实际送达要读回 `in_bed/current_bed`。
- dry-run 投影 `bed_id/job_def`，不创建 job。
- 已上床 409、非死亡休眠/非殖民者或囚犯 422、无候选床 422。

## 实测与限制

G52：`ai_g31_fixture` 副本（tick 74742）通过 GET deathresting/can_carry/candidate_bed、
carry dry/正式 200/applied true；但当时 fixture 的普通 `Bed` 只给一格加屋顶，1x2 床的
另一格在阳光下，原生 `JobDriver_Deathrest.GotoBed` 的 sunlight 失败条件使 Deathrest job
`Incompletable`，pawn 被丢在床格未上床。历史失败保留。

G70：修正 fixture（对 `Bed.OccupiedRect()` 每格加 `RoofConstructed`）后重跑，普通 `Bed`
回退实际送达：`in_bed=true`、`current_bed` 为提交 bed、`pawn_job=Deathrest`、
`deathresting=true`、搬运者 job 结束、bed `roofed=true`/`in_sunlight=false`；
非死亡休眠 pawn 再次 carry 422。见
[G70 自审](../references/reviews/2026-11-08-g70-deathrest-carry-delivery.md)。

G72：`bed_def:"DeathrestCasket"` 构造玩家 casket，`candidate_bed` 为该 casket，carry
dry/正式通过，`in_bed=true`、`current_bed_def=DeathrestCasket`、`pawn_job=Deathrest`；
非死亡休眠 422。见 [G72 自审](../references/reviews/2026-11-08-g72-deathrest-casket-delivery.md)。

限制：构造 casket 是 fixture 直接 spawn，不是正常建造/安装/归属；`CompDeathrestBindable`
绑定、囚犯/倒地搬运、送达后的死亡休眠建筑连接、完整 4 天周期、Cua 对照均未验。
