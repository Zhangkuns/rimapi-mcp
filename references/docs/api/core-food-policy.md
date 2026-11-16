# Core 食物政策 CRUD、过滤、默认与指派（C-32）

2026-11-12 新增原生 `FoodRestrictionDatabase` 管理面：

- `GET /food/policy`：所有 policy 的 `id/label/index/is_default/in_use`。
- `POST /food/policy`：`MakeNewFoodRestriction`，可 `copy_from_id` 复制过滤，
  `label` 改名；dry_run 不建实例。
- `POST /food/policy/default`：`SetDefault` 移到默认槽。
- `POST /food/policy/delete`：`TryDelete`；被 pawn 使用时 409，不使用后删除。
  `dry_run:true` 只用同一 alive-pawn 枚举预判 `in_use`/`would_apply`，不调用
  `TryDelete`、不修改数据库。
- `POST /food/policy/filter`：`ThingFilter.SetAllow` 单项食物
  （`thing_def`）或原生特殊过滤（`special_filter`，如 `AllowRotten`）；
  `dry_run:true` 只读回 before/after 候选值。`GET /food/policy/check` 接受
  `thing_def` 和/或 `special_filter`。
- `POST /pawn/food-policy`：`pawn.foodRestriction.CurrentFoodPolicy`，要求
  `Configurable`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选 Human321 与 Beer；创建复制默认 Lavish 的 policy 11；指派后
`current_id=11`；Beer false 后 GET check false，true 后 true；删除被 409
（Blean 在用）；改回默认后删除成功，列表移除。
证据 `tools/cua/.runtime/c32-20261112/evidence/c32.json`，runner
`tests/c32_food_policy.py`，fixture `prepare_food_policy`。

2026-11-12 dry-run/特殊过滤回归（同版本隔离 `g35_deathresting` 副本）：
未使用副本 `delete dry_run=true` 返回 200/`applied:false`/`would_apply:true`
且列表仍含该 policy；指派后 dry-run 返回 409/`in_use:true`/pawn id 且 policy
仍存在；解除后真实删除成功。`special_filter=AllowRotten` false 后 check
false，dry-run true 不写入，正式 true 后读回 true。
证据 `tools/cua/.runtime/c32-35-dryrun-20261112-run4/evidence/c32-35-dryrun-3.json`，
runner `tests/c32_35_policy_dryrun.py`，隔离 DLL sha256
`a432238c402a3837e4f397c559b54e4da1265308baee835b12151b5b2f94e36b`。
- 未长跑自主进食/默认槽行为；本行仍保留该边界。


## 自主进食遵从（2026-11-13）

复用上文 CRUD/filter/assign；隔离 `g35_deathresting` 副本（tick 76787，五 DLC）
中，fixture 选一名可配置食物政策的殖民者、清 job/物品栏、工作优先级全 0、
饥饿设 0.15，在旁生成 `MealSimple` 10 与 `Pemmican` 20（其余殖民者喂饱并清 job）。

- 两项都禁用：推进 600 tick 后 job `GotoWander`、无 `Ingest`，两堆数量保持 10/20。
- 只允许 `Pemmican`：原生 `Ingest` 目标 `Pemmican`；饥饿 0.15→0.4，
  `Pemmican` 20→0，`MealSimple` 保持 10。
- 只允许 `MealSimple`：原生 `Ingest` 目标 `MealSimple`；饥饿 0.15→1.0，
  `MealSimple` 10→9，`Pemmican` 保持 0。

原生路径：`JobGiver_GetFood` → `FoodUtility.TryFindBestFoodSourceFor`/
`GetCurrentRespectedRestriction(...).Allows(food)` → `Toils_Ingest`。证据
`tools/cua/.runtime/core-policies-20261113j/evidence/c32-autonomous.json`，runner `tests/parallel/core-policies/c32_autonomous_eating.py`。
- 未测：其它食物/类别、默认槽长期行为、婴儿/囚犯/访客、营养膏分配器/植物/狩猎、
  特殊过滤的自主结果。
