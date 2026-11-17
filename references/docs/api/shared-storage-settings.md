# Shared 地面储存区、建筑过滤、优先级与存储组（S-07）

维护：Codex；核对：2026-11-16；状态：zone/building/组件统一设置已实现并通过限定隔离验证；正式DLL未部署。

2026-11-16 修复：现在解析建筑自身及ThingComp的IStoreSettingsParent，并对炮塔转入内部gun；塑形舱Nutrition等可设置，GET给出settings_owner_type，敌对目标写入拒绝。成长舱/书架旧路径保持；书架实际内容/按书弹出另补接口。见[UI-07/11合同](ui-option-repairs.md)与[原生验收](../references/reviews/2026-11-16-ui-api-repair.md)。

## 当前完整设置入口

`GET /api/v1/storage/settings?zone_id=12&map_id=0` 或 `?thing_id=Shelf123&map_id=0` 读取地面储存区或建筑；两个身份必须且只能传一个。map可省略，默认当前地图；未知地图/目标404、非储存目标422，缺身份/双身份/格式错误400。

读回保留原priority/group字段，并给出 `zone_id/name/map_id`、完整且排序的 `filter_allowed`、数量、`filter_allowed_truncated:false/filter_complete:true`、全部原生特殊过滤的 `special_filters` 字典和 `filter_disallowed_special`、耐久/品质范围、配置能力标志、书籍精神崩溃概率范围及 `parent_filter_allowed`。后者表示原生容器自身限制；复制允许集不会绕过父过滤器。此处完整性指当前原生允许项/特殊过滤/公开范围，不再只返回前200项。

`special_filters` 的键是原生DefName，例如 `AllowRotten`，作为数据保留大小写，不随JSON字段snake_case转换；可将此字典原样用于修改请求。

`POST /storage/settings` 的目标身份相同，支持以下 `action`：

| action | 语义 |
| --- | --- |
| `patch`（默认） | 修改显式提供的字段；缺省字段不变 |
| `allow_all` | 调原生 `ThingFilter.SetAllowAll(parentFilter)`，允许父过滤器支持的项 |
| `disallow_all` | 调原生 `SetDisallowAll`，明确禁止全部普通允许项 |
| `replace` | `allowed_item_defs` 和/或 `allowed_item_categories` 必须至少出现一个；先原生禁止全部，再按列表启用。显式空数组表示清空；null/省略不表示清空 |
| `copy` | 提供 `copy_from_zone_id` 或 `copy_from_thing_id` 二选一，可选 `copy_from_map_id`，用原生 `StorageSettings.CopyFrom` 复制完整过滤器和优先级；支持zone↔zone、zone↔building。名称和存储组关系不随设置复制 |

原生允许/禁止全部会按UI方法重置可配置特殊过滤；不自动重置HP/品质范围。replace同样先调用原生禁止全部，因此不是“仅删除数组元素”的补丁。要重放完整设置，可同时传入读回的 `special_filters`、范围和普通允许项；或直接使用copy。复制同一目标不会清空自身。

所有action执行基础操作后，再应用显式字段：

- `name` 仅重命名地面区；`priority` 接受原生0..5或名称。
- `add_item_defs/remove_item_defs/add_item_categories/remove_item_categories` 批量增删，类别使用原生分类过滤；同组冲突先删后加。
- 旧 `filter_def/filter_allowed`、`special_filter/special_filter_allowed` 继续支持；新 `special_filters` 可批量设置新鲜/腐烂、干净/尸污等原生Def开关。旧单项特殊过滤字段在批量字典后覆盖同名项。
- `min/max_hit_points_percent`、`min/max_quality`、`min/max_mental_break_chance`；数值范围必须0≤min≤max≤1，品质为原生QualityCategory，非法输入返回400，不静默截断或忽略。
- 仅建筑支持既有存储组字段。新建组先用原生 `InitFrom` 保留来源设置；加入已有组时先采用该组设置，再应用本次显式修改。组共享与≤1成员解散规则仍由原生执行。

完整请求先在独立原生设置副本上验证，拒绝或 `dry_run:true` 不改目标、名称或组。dry返回当前实际读回与 `proposed` 设置预览；正式提交使用原生CopyFrom触发haul/UI通知，并返回实际设置。支持既有Game内幂等和operation事件。

旧 `POST /map/zone/stockpile` 创建接口保持原语义：只有非空白名单才先清空；其空数组不是清空命令。完整后续编辑使用本页入口，不把创建参数猜成replace。

## 历史交付

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
