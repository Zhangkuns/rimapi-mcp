# Core 药物政策 CRUD、条目规则与指派（C-33）

2026-11-12 新增原生 `DrugPolicyDatabase`/`DrugPolicy`/`DrugPolicyEntry` API：

- `GET /drug/policy`：`id/label/source_def/index/is_default/drug_count/in_use`。
- `GET /drug/policy/entry`：单药物条目全部字段。
- `POST /drug/policy`：新建（可 `policy_def` 预设或 `copy_from_id` 复制）。
- `POST /drug/policy/default`、`.../delete`：默认槽与在用 409 删除保护。
  `delete` 的 `dry_run:true` 用 alive-pawn 枚举预判 `in_use`/`would_apply`，
  不调用 `TryDelete`、不修改数据库。
- `POST /drug/policy/entry`：`allowed_for_addiction/joy`、`allow_scheduled`、
  `days_frequency`、`only_if_mood_below/joy_below`、`take_to_inventory`。
- `POST /pawn/drug-policy`：`pawn.drugs.CurrentPolicy` 指派。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选 Human321/Beer；复制默认 Social drugs 创建 policy 5；指派
current=5；Beer 设为 joy/scheduled、2.5 天、mood 0.4、joy 0.5、携带3，
GET 全部一致；在用删除 409（Blean），改回默认后删除成功。
证据 `tools/cua/.runtime/c33-20261112/evidence/c33.json`，runner
`tests/c33_drug_policy.py`，fixture `prepare_drug_policy`。

2026-11-12 dry-run 回归：未使用副本 `delete dry_run=true` 返回
200/`applied:false`/`would_apply:true` 且 policy 保留；指派后返回
409/`in_use:true` 且保留；解除后真实删除成功。证据
`tools/cua/.runtime/c32-35-dryrun-20261112-run4/evidence/c32-35-dryrun-3.json`，
runner `tests/c32_35_policy_dryrun.py`，DLL sha256
`a432238c402a3837e4f397c559b54e4da1265308baee835b12151b5b2f94e36b`。
- 未长跑自主 scheduled/joy 服药和库存消耗；本行仍保留该边界。


## 自主携药遵从（2026-11-13）

复用上文 CRUD/entry/assign；隔离 `g35_deathresting` 副本（tick 76787，五 DLC）
中 fixture 清殖民者 job/物品栏、工作 0，在旁生成 Beer 10，并将 Beer 条目设为
`allow_scheduled=false`、`take_to_inventory` 先 0 后 3。

- `take_to_inventory=0`：推进 600 tick，无 `TakeInventory`，物品栏 Beer 0。
- `take_to_inventory=3`：原生 `TakeInventory` job 目标 Beer，物品栏 Beer 0→3；
  entry 读回 `take_to_inventory:3`/`allow_scheduled:false`。
- 再设回 0 并丢下随身 Beer：推进 600 tick，物品栏 0、无再次 `TakeInventory`。

原生路径：`JobGiver_MoveDrugsToInventory` →
`Pawn_DrugPolicyTracker.AllowedToTakeToInventory` → `TakeInventory`。证据
`tools/cua/.runtime/core-policies-20261113j/evidence/c33-autonomous.json`，runner `tests/parallel/core-policies/c33_autonomous_drug_carry.py`。
- 代表选择：`take_to_inventory` 是确定性的原生自主携药行为；scheduled/joy 实际
  摄入未重跑（其 CRUD/阈值读回沿用上文旧证据）。
- 未测：成瘾/硬药意愿、物品栏容量边界、其它药物、库存搬运链。
