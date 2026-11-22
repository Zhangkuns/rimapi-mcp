# 指定格/区域即时清洁（WORK-D01）

`POST /api/v1/debug/native/execute` 的 `work.clean_area` 直接清除明确目标格上的 `Filth`。它是调试直接变更动作，需要活动存档与开发者模式；不替代正常清洁工作，也不改变 `clean_room` 等既有派工行为。加载对应 DLL 后，通过 `GET /api/v1/debug/native/actions` 确认动作可用。

```json
{"action":"work.clean_area","map_id":0,"cells":[{"x":127,"z":142},{"x":128,"z":142}],"dry_run":true}
```

```json
{"action":"work.clean_area","map_id":0,"point_a":{"x":129,"z":142},"point_b":{"x":130,"z":142}}
```

## 参数

- `map_id` 必填，使用 `GET /api/v1/maps` 返回的地图 ID。
- 目标二选一：非空 `cells` 格子列表，或 `point_a` / `point_b` 两个矩形角点。每点明确给出整数 `x`/`z`；矩形包含边界，角点顺序不限。重复格按一次处理，选择方式冲突、缺坐标或空格表在写入前拒绝整个请求。
- 所有目标格必须位于该地图内；任一格越界时整个请求不执行。
- `dry_run:true` 只读取并计算，不调用原生写入；正式执行时省略或设为 `false`。
- 可设 `expected_process_id`；同一次游戏内重试可使用 `Idempotency-Key`，见[通用调试合同](../debug-native-actions.md)和[幂等合同](idempotency.md)。

## 原生语义

每个有污物的目标格调用原版 `FilthMaker.RemoveAllFilth(cell, map)`。该原生方法收集该格 `ThingList` 中的 `Filth` 并逐个 `Destroy()`，不动地形/地板、建筑、物品、人物或其它非 `Filth` 对象；本动作也不处理污染、积雪或火。多格污物（例如 3×3 的 `Filth_BlastMark`）在原生上是一个整体对象：以目标格为锚点的该对象会被整体移除，因此它覆盖到的目标格外格子也可能失去这一对象；其它区域外污物保持不变。

本动作是即时调试结算：不创建清洁 Job、不派工、不增加工作记录，也不检查正常清洁的路径、优先级或工作类型。

## 读回

`before` / `after` 是去重后目标格的逐格快照：

```json
{
  "x": 127, "z": 142,
  "terrain": "WoodPlankFloor", "foundation": null,
  "filth": [{"thing_id": "Filth_Blood27982", "def_name": "Filth_Blood", "thickness": 1}],
  "other_things": [{"thing_id": "Steel27979", "def_name": "Steel", "category": "Item"}]
}
```

- `filth` 逐项给出原生 `ThingID`、`defName` 与 `thickness`（int，1–5）；`other_things` 是同格非污物样本，用于证明地板/建筑/物品等保持。
- `cells_cleaned` 是本次实际清除污物的目标格数；`filth_removed` 是移除的 `Filth` 对象数（按 `ThingID` 去重），`filth_thickness_removed` 是这些对象的厚度合计。
- `would_clean_cells` / `would_remove_filth` / `would_remove_thickness` 是执行前预计值；预演时据此判断影响。
- `unchanged_outside` 是目标格相邻、不在目标集合内的最多 12 个格子样本（含地形、污物与非污物）。`outside_changed` 表示除“以目标格为锚点的多格污物整体移除”外，样本是否发生意外变化；正常成功为 `false`。
- `applied` / `native_invoked` / `changed` 均为 `true` 才表示实际调用原生并改变了污物状态。`dry_run`、目标格本无污物或重复请求均已无副作用时，它们为 `false`，且 `after` 等于 `before`。
- 原生异常或清除后读回仍有污物、非同格对象变化时返回 500 与 `outcome:"uncertain"`，尽量附上已发生的 `after` 读回；不静默报告成功。部分清除是可能的，应按读回继续处理，不要无脑换 key 重放。

## 状态码与边界

- `map_id` 缺失、格表为空、坐标不完整、选择方式冲突、格越界：400。
- 地图不存在：404。
- 关闭开发者模式：403。
- 游戏未就绪、进程不符或幂等键状态冲突：409。
- 原生异常/读回不符：500，`outcome:"uncertain"`。
- 本动作只清除目标格上的 `Filth`；不删除地板、建筑、物品，不影响区域外其它污物，不处理污染、积雪、火。正常清洁、清洁区域与保存载入请走正常玩法合同。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame` 均为 `103272`；逐格读回、区域外保持、失败路径、重放与清理见[WORK-D01 验收与自审](../references/reviews/2026-11-21-work-d01-clean-area.md)及[原始证据](../../tests/parallel/work-d01/evidence-work-d01-clean-area.json)。正式 DLL 未部署，本页只描述源码与隔离 DLL 的行为。
