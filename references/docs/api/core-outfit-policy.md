# Core 服装政策 CRUD、过滤与指派（C-34）

2026-11-12 在既有 `OutfitPolicyService` 指派上补齐原生 `OutfitDatabase` 管理：

- `GET /outfit/policy`：`id/label/index/is_default/in_use`。
- `GET /outfit/policy/check`：单件 apparel 是否允许，并读回
  `min/max_hit_points_percent`、`min/max_quality` 与可选 `special_filter`。
- `POST /outfit/policy`：`MakeNewOutfit`，可 `copy_from_id`；`label` 改名。
- `POST /outfit/policy/default`、`.../delete`：默认槽与在用 409 保护。
  `delete` 的 `dry_run:true` 用 alive-pawn 枚举预判 `in_use`/`would_apply`，
  不调用 `TryDelete`、不修改数据库。
- `POST /outfit/policy/filter`：`ThingFilter` 的 `thing_def`、`special_filter`
  （如 `AllowDeadmansApparel`）、`min/max_hit_points_percent` 0..1 与
  `min/max_quality`（`QualityCategory`）；`dry_run:true` 只回读候选值。
- 指派复用既有 `POST /pawn/outfit`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选 Human321 与 `Apparel_ShieldBelt`；复制默认 Anything 创建 outfit 7；
`/pawn/outfit` 指派后读回 7；ShieldBelt false 后 check false，true 后 true；
在用删除 409（Blean），改回默认后删除成功。
证据 `tools/cua/.runtime/c34-20261112/evidence/c34.json`，runner
`tests/c34_outfit_policy.py`，fixture `prepare_outfit_policy`。

2026-11-12 dry-run/过滤回归：未使用副本 `delete dry_run=true` 返回
200/`applied:false`/`would_apply:true` 且保留；指派后 409/`in_use:true`；
解除后真实删除成功。`AllowDeadmansApparel` false、HP 0.5..1.0、
quality Good..Legendary 依次写入并由 check 读回；HP dry-run 0.1 不改变
已存 0.5。证据
`tools/cua/.runtime/c32-35-dryrun-20261112-run4/evidence/c32-35-dryrun-3.json`，
runner `tests/c32_35_policy_dryrun.py`，DLL sha256
`a432238c402a3837e4f397c559b54e4da1265308baee835b12151b5b2f94e36b`。
- 未长跑实际自主换装；奴隶拒绝另行 ID-R09，不重复。


## 自主换装遵从（2026-11-13）

复用上文 CRUD/filter/assign；隔离 `g35_deathresting` 副本（tick 76787，五 DLC）
中 fixture 选殖民者、工作 0，生成并穿上一件原生 `Apparel_CowboyHat`，重置
`mindState.nextApparelOptimizeTick=0`。帽子而非 `Apparel_ShieldBelt`：源码事实，
当殖民者携带远程武器时 `ApparelScoreGain` 对 ShieldBelt 返回 -1000（正常原生规则），
会混淆“政策允许”检查。

- 政策禁用帽子：原生 `JobGiver_OptimizeApparel` 返回 `RemoveApparel`；推进后
  `worn_belt:false`，帽子脱下并保留在地图/收纳。
- 政策允许帽子：原生 giver 返回 `Wear`（`belt_in_storage:true`、
  `apparel_score_raw:0.172`）；推进后 `worn_belt:true`。
- 测试世界原本没有 stockpile zone 时，fixture 建一个单格 `Zone_Stockpile` 让脱下
  衣物可被原生 `Wear` 选回（准备披露）。

原生路径：`JobGiver_OptimizeApparel` 的 `filter.Allows`/`IsInAnyStorage`/
`ApparelScoreGain` → `RemoveApparel`/`Wear`。证据 `tools/cua/.runtime/core-policies-20261113j/evidence/c34-autonomous.json`，
runner `tests/parallel/core-policies/c34_autonomous_outfit.py`。
- 未测：品质/HP/特殊过滤的自主结果、奴隶（ID-R09）、biocoded/locked/forced
  apparel、多殖民者、帽子以外服装类别。
