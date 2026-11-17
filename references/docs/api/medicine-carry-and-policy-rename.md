# 随身治疗药品与已有方案重命名

维护：Codex；核对：2026-11-16；状态：已实现并通过统一隔离验证；正式DLL未部署。

## 人物随身治疗药品

`GET /api/v1/pawn/medicine-carry?pawn_id=Human803` 返回原生Carry列对应的 `Pawn_InventoryStockTracker` / Medicine组状态：

- `medicine_def/target_count`：希望人物保持的药品类型和数量。
- `inventory_count`：当前背包中该类型的实际数量；目标值不代表已拾取。
- `available_medicine_defs/min_count/max_count`：原生下拉选项和数量范围，不硬编码所有版本的最大值。
- `entry_initialized`：是否已有原生stock entry。缺entry时返回原生默认值；GET不会调用会创建entry的原生getter。

```json
{"pawn_id":"Human803","medicine_def":"MedicineIndustrial","target_count":2,"dry_run":false}
```

提交到 `POST /pawn/medicine-carry`，类型和数量至少提供一个。目标须为有库存储备tracker的存活殖民者；支持数字或字符串人物ID。先验证原生Medicine组选项/范围，再调用UI同一 `SetThingForGroup/SetCountForGroup`。dry返回当前状态和请求值，不创建stock entry、不改设置；正式写后返回实际值。

未知人物/药品定义404，不属于原生组选项的物品或不合格人物422，缺字段/越界数量400。支持Game内幂等和operation事件。

此入口不会生成药品、立即搬入背包或强制治疗；后续取药由原生 `JobGiver_TakeForInventoryStock` 和库存/工作状态决定。用药方案的携带规则、医疗照护等级、一次性的带药治疗Job都不是本设置。

## 已存在方案按ID重命名

以下均为POST，前缀 `/api/v1`：

| 路径 | 方案 |
| --- | --- |
| `/outfit/policy/rename` | 着装 |
| `/food/policy/rename` | 食物 |
| `/drug/policy/rename` | 用药 |
| `/reading/policy/rename` | 阅读 |

```json
{"policy_id":0,"label":"外出方案","dry_run":false}
```

ID必须显式提供，0是有效ID而非缺省目标；outfit可改用既有术语 `outfit_id`，但不能同时给两个身份。名称不能为空白，不要求名字全局唯一。

只写该原生方案对象的label，不新建/复制/删除方案，不改过滤器、药物条目、默认顺序或人物引用。响应给出 `kind/policy_id/previous_label/label/requested_label/dry_run/applied`；outfit另有outfit_id。dry时label是当前实际名称，requested_label是预览；正式写后label为实际读回。

缺ID/双ID/空白名称400，未知ID404，缺活动游戏409。四条路由都通过游戏主线程协调器，支持Game内幂等及一次正式提交operation事件。既有列表GET会立即显示新名称；不靠新建/复制时的命名代替已有方案重命名。
