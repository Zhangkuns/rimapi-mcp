# Biotech 成长时刻（BT-11h）

`GET /api/v1/dlc/biotech/growthmoment` 列出待选 `ChoiceLetter_GrowthMoment`：

```json
{"count":1,"letters":[{"letter_id":4,"pawn_id":"Human321","growth_tier":0,
 "choice_made":false,"passion_options":["Construction"],
 "trait_options":["Beauty"],"passion_gains_count":1}]}
```

`POST /api/v1/dlc/biotech/growthmoment/choose`

```json
{"letter_id":4,"passion_defs":["Construction"],"trait_def":"Beauty","dry_run":true}
```

- 选项必须来自该 letter 的原生候选；passion 数量等于 `passion_gains_count`；
  trait 候选非空时必须给 `trait_def` 或合法 `no_trait`。dry-run 只校验。
- 正式调用原生 `ChoiceLetter_GrowthMoment.MakeChoices` 并 `RemoveLetter`；服务
  不直接写技能热情/特质。
- 实测 letter 4：Construction None→Minor、+Beauty、letter 移除；同 letter 再选
  404（`Reverse/evidence/bt11h-growth-moment-20261111.json`，自审
  [BT-11h](../references/reviews/2026-11-10-bt11h-growth-moment.md)）。
- 该初始样本是合成原生pending letter；后续已补真实13岁生日的trait-only信件，
  原生选择Nerves后人物获得Delicate（`bt11h-real-birthday-20261111.json`，
  [生日自审](../references/reviews/2026-11-10-bt11h-real-birthday.md)）。
- 婴儿喂养/抱送与生育策略已有独立结果，见[照护合同](biotech-childcare.md)。
  后续已核原生随机NatureRunning desire→实际job，learning0.3→0.3326；fixture降need并缩短desire等待，未指定随机结果（[学习自审](../references/reviews/2026-11-10-bt11h-child-learning.md)）。玩家可控排班/区域/工作/照护/成长信，不能选active desire；Tutor/学校与no-trait/sexuality等选择语义未全验，不要求每个年龄重复同一语义。当前归并见[执行计划](../exec-plans/active/five-dlc-remaining.md)。
