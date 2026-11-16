# Core 精确手术部位与单张医疗账单（C-29）

2026-11-12 扩展手术 API：

- `GET /pawn/surgery` 每张账单增加稳定 `bill_id`（原生 `Bill.loadID`）、
  `order`、`part_id`（`BodyPartRecord.Index`）与 `part_label`。
- `GET /pawn/surgery/options?pawn_id&recipe_def` 返回该配方在该患者上
  可选的准确部位（含左右 `custom_label`、health、missing）。
- `POST /pawn/surgery` 支持 `body_part_id`；优先于同名 `body_part`。
- `DELETE /pawn/surgery` 支持 `bill_id`，只删除该账单；仍保留 recipe_def
  兼容路径。
- `POST /pawn/surgery/bill` 按 `bill_id` 暂停/恢复（`suspended`）或
  上/下移（`move:-1|1`，`BillStack.Reorder`）。

合同：options GET；bill POST command/pawn_ai/validate/game_memory。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 选择已卧床患者 Human66736 与可用双侧 `RemoveBodyPart`；options
返回 8 个部位（含左右肺/肾）。以 `body_part_id` 5/6 分别建两张账单，
GET 读回准确 part_id；暂停 bill1；bill2 上移 order 1→0；DELETE bill1
只移除一张；再移除 bill2 后为空。证据
`tools/cua/.runtime/c29-20261112/evidence/c29.json`，runner
`tests/c29_surgery_bills.py`，fixture `prepare_surgery_parts`。

- 未重跑手术执行/术后结果；普通手术与胚胎医生链由既有证据保留。本行只补
  部位实例选择、稳定账单身份和单账单排程管理。
