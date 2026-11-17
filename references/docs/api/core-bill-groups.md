# Core 生产账单存放组/计数组（C-27）

2026-11-16 修复：UI-01–03已补操作者互斥模式/清除、精确count_source与all恢复、classic产品风格/图案。旧include_group_id仍仅指命名组；新configure支持预检、dry、重放，旧创建/更新响应兼容。见[当前合同](ui-option-repairs.md)与[限定验收](../references/reviews/2026-11-16-ui-api-repair.md)。

2026-11-12 扩展既有账单 CRUD：

- `GET` 账单新增 `slot_group_id/slot_group_label`、`storage_group_id/
  storage_group_label`、`include_group_id/include_group_label`。
- `POST /buildings/bills/add` 与 `PUT /buildings/bill/update` 接受
  `slot_group_id`（具体堆放区父对象：`Building_Storage.thingIDNumber` 或
  `Zone.ID`）、`storage_group_id`（命名 StorageGroup loadID，优先）与
  `include_group_id`（计数来源组）。
- 设置具体组时调用原生 `SetStoreMode(BillStoreModeDefOf.SpecificStockpile,
  group)`；计数组调用 `SetIncludeGroup`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选现有工作台/配方，创建 Stockpile zone；建房 `Shelf` 并链接命名
StorageGroup。创建账单带 `slot_group_id` 后读回 `SpecificStockpile`、
Stockpiles label 和 id；PUT 切到命名组后读回 `storage_group_id/label` 与
`include_group_id/label`；最后删除账单。证据
`tools/cua/.runtime/c27-20261112/evidence/c27.json`，runner
`tests/c27_bill_groups.py`，fixture `prepare_bill_group`。

- 未在长时间生产中重跑实际产品运输/计数停止条件；存放/计数只补原生组配置和
  准确账单读回。

## 原料过滤：特殊过滤、品质与耐久

2026-11-12 补齐 `bill.ingredientFilter` 的正常选择（与成品 `hp_range/
quality_range` 区分）：

- `POST /buildings/bills/add`、`PUT /buildings/bill/update` 新增
  `ingredient_special_filter` + `ingredient_special_filter_allowed`（原生
  `ThingFilter.SetAllow(SpecialThingFilterDef, bool)`）、
  `ingredient_min/max_hit_points_percent`（0..1）与
  `ingredient_min/max_quality`（`QualityCategory`）。
- 账单读取新增 `ingredient_disallowed_special_filters`、
  `ingredient_min/max_hit_points_percent`、`ingredient_min/max_quality`；
  `allowed_materials` 仍是允许的 ThingDef 列表。
- 越界/未知值分别返回 404/400；设置原料过滤不会改写成品 `hp_range/
  quality_range`。

隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：fixture 生成玩家
`FueledStove` 并选原生 `CookMealSimple`/`Meat_Twisted`；创建账单带
`allowed_materials`；PUT 后读回 `AllowRotten` 被禁、HP 0.4..0.9、
quality Good..Legendary 且 `allowed_materials` 保持；未知 special 404、
非法品质/反向范围 400；成品 range 不变。证据
`tools/cua/.runtime/int-c27-20261112-run2/evidence/c27-integration.json`，
runner `tests/c27_bill_ingredient_filter.py`。
- 原生 `WorkGiver_DoBill` 的 `TryFindBestBillIngredientsInSet_*` 直接使用
  `bill.ingredientFilter.Allows(def)`，本次未另跑长生产；该消费路径由原生源码
  证明，实际生产运输仍保留既有边界。

## 2026-11-13 自定义账单名称修复

创建/更新中的 `player_custom_name` 通过原生 `Bill.RenamableLabel` setter
写入 `playerCustomName` 并由原生存档持久化；移除对不存在的 `Bill.label` 字段反射。
统一DLL创建 `REST Created Blocks`、更新 `REST Updated Blocks`，GET与保存/
载入后名称准确；独立正常链完成20块板岩砖生产，账单名 `REST Haven Blocks`
保持。见[统一构建验收](../references/reviews/2026-11-13-api-playability-core-integrated.md)。
