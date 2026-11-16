# Shared 建筑储存过滤、优先级与存储组（S-07）

2026-11-12 新增：

- `GET /storage/settings?thing_id`：原生 `StorageSettings.Priority`、
  `ThingFilter` 允许项、`IStorageGroupMember.Group` 与
  `AllowedToAccept(Silver)`。
- `POST /storage/settings`：设置 priority、单 ThingDef 过滤允许/拒绝；
  `create_group/group_label` 通过 `Map.storageGroups.NewGroup` +
  `SetStorageGroup` 建立并加入存储组；`group_id` 加入已有组，`leave_group`
  按原生 `StorageGroupUtility.SetStorageGroup(null)` 退出。
- 过滤补齐 `special_filter/special_filter_allowed`、
  `min/max_hit_points_percent`、`min/max_quality`；读回增加
  `filter_allowed_count`、`filter_allowed_truncated`、
  `filter_disallowed_special` 与 HP/品质范围。
- 原生 `StorageGroupManager.Notify_MemberRemoved` 在组只剩 ≤1 名成员时解散
  该组并解除剩余成员关联；API 读回即此终态。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成玩家 Shelf 与 Silver。初始 priority Preferred、
accepts_silver true；dry 不写；set priority Low + 拒绝 Silver 后两项读回；
create_group 后 group_id 0、label RIMAPI Shelf、member_count 1；未知 404。
证据 `tools/cua/.runtime/s07-20261112/evidence/s07.json`，runner
`tests/s07_storage.py`，fixture `prepare_storage`。

2026-11-12 存储组/过滤补齐集成回归（`int-s07-20261112`）：两货架，
fixture 生成，dry 加入不改 group；正式加入后 member_count 2；设置
Steel 允许、`AllowRotten` 禁止、HP 0.5–0.8、Normal–Masterwork 后逐项读回；
`leave_group` 后组因原生 ≤1 成员规则解散，两货架 group 均为 null；
重复离组 400、未知 group_id 404。证据
`tools/cua/.runtime/int-s07-20261112/evidence/s07-integration-4.json`。

- 未重跑实际搬运遵从、货架/书架/坟墓/成长舱等全部建筑类型；本行以原生
  StorageSettings/StorageGroup 读回与原生消费者路径为代表。

## 2026-11-13 正常堆放区创建优先级修复

正常创建 stockpile 的 `priority` 使用原生数值：0 Unstored、1 Low、
2 Normal、3 Preferred、4 Important、5 Critical；省略为 Normal(2)。
旧创建入口将3–5截为2、默认0；现沿用0..5原生范围。
统一DLL真实验3→3、5→5、省略→2；保存/载入后仍保持3。
证据与生产链见[统一构建验收](../references/reviews/2026-11-13-api-playability-core-integrated.md)。
