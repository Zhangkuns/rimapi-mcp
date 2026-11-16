# Core 阅读政策 CRUD、过滤与指派（C-35）

2026-11-12 新增原生 `ReadingPolicyDatabase`/`ReadingPolicy`：

- `GET /reading/policy`：`id/label/index/is_default/in_use`。
- `GET /reading/policy/check`：`thing_def` 与可选 `special_filter`。
- `POST /reading/policy`：`MakeNewReadingPolicy`，可 `copy_from_id`。
- `POST /reading/policy/default`、`.../delete`（在用 409）；`delete` 的
  `dry_run:true` 用 alive-pawn 枚举预判 `in_use`/`would_apply`，不调用
  `TryDelete`、不修改数据库。
- `POST /reading/policy/filter`：`defFilter` 的 `SetAllow`，或
  `effectFilter` 的特殊过滤。
- `POST /pawn/reading-policy`：`pawn.reading.CurrentPolicy`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选 Human321/TextBook；复制默认 Anything 创建 policy 8；指派后
current=8；TextBook false 后 check false，true 后 true；在用删除 409；
改回默认后删除成功。
证据 `tools/cua/.runtime/c35-20261112/evidence/c35.json`，runner
`tests/c35_reading_policy.py`，fixture `prepare_reading_policy`。

2026-11-12 dry-run 回归：未使用副本 `delete dry_run=true` 返回
200/`applied:false`/`would_apply:true` 且保留；指派后 409/`in_use:true`；
解除后真实删除成功。证据
`tools/cua/.runtime/c32-35-dryrun-20261112-run4/evidence/c32-35-dryrun-3.json`，
runner `tests/c32_35_policy_dryrun.py`，DLL sha256
`a432238c402a3837e4f397c559b54e4da1265308baee835b12151b5b2f94e36b`。
- 未长跑自主阅读/效果过滤；本行仍保留该边界。


## 自主阅读与 def/effect 过滤（2026-11-13）

复用上文 CRUD/defFilter/effectFilter/assign；隔离 `g35_deathresting` 副本
（tick 76787，五 DLC）中 fixture 选成年、有视力、可阅读的殖民者，工作 0、
joy 0.02，生成一本原生 `Novel` 或 `TextBook`（强制 Shooting 技能），并禁止
其它所有书籍。

- 允许 `Novel`：正常 joy think tree 在 600 tick 内选中 `Reading` job，目标
  `Novel`（`Giver=JobGiver_GetJoy`，非 fixture 代下单）。
- 禁用 `Novel` defFilter：原生 `JoyGiver_Read.TryGiveJob` 返回
  `job_available:false`；推进 400 tick 后 job 为其它 joy（`Skygaze`），未读该 Novel。
- `TextBook` 强制 Shooting、允许 `AllowBookShooting`：正常 think tree 再次选中
  `Reading`，目标 `TextBook`。
- 禁用 effectFilter `AllowBookShooting`：原生 giver 返回 `job_available:false`。

原生路径：`JoyGiver_Read`/`LearningGiver_Reading` →
`BookUtility.TryGetRandomBookToRead`（`CurrentPolicy.defFilter.Allows(book)` 且
`effectFilter.Allows(book)`）→ `JobDriver_Reading`。证据
`tools/cua/.runtime/core-policies-20261113j/evidence/c35-autonomous.json`，runner `tests/parallel/core-policies/c35_autonomous_reading.py`。
- 未测：儿童 `LearningGiver_Reading`、书架/库存书、8000 tick 完整读完后的效果
  结算、mental break/quest outcome、其它书/effect Def。
