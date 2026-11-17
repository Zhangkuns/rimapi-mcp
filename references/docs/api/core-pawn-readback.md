# Core 人物状态与地图生物汇总

维护：Codex；核对：2026-11-16；状态：人物详情漏填和敌人计数遗漏已修复，隔离验证通过；正式 DLL 未部署。见[本轮计划](../exec-plans/completed/2026-11-16-pawn-readback.md)。

均为只读查询，在游戏主线程读取原生对象，不改变健康、工作、时间或收容状态。

## 人物详情

`GET /api/v1/pawns/details?id=19644` 支持按现有查询器可解析的 Pawn ID 读取人物、动物、机械体和实体。
`GET /api/v1/colonist/detailed?id=` 及 `/api/v2/colonist/detailed?id=` 只查询殖民者；对应复数列表使用同一映射。

| 字段 | 原生来源与语义 |
| --- | --- |
| `body_size` | `Pawn.BodySize`，当前体型，包含生命阶段等影响，不使用固定的种族基础体型 |
| `medical_info.is_dead` | `Pawn.Dead`，当前死亡状态 |
| `medical_info.is_downed` | `Pawn.Downed`，当前倒地状态，不从工作名称或某项能力推断 |
| `medical_info.consciousness` | `Pawn.health.capacities.GetLevel(Consciousness)`，原生意识能力值 |
| `medical_info.moving` | `Pawn.health.capacities.GetLevel(Moving)`，原生移动能力值；不额外钳制为 0–1 |

`medical_info` 下其余伤势、医疗策略和自疗字段保持原有来源。v1 colonist 响应的医疗对象名为 `colonist_medical_info`；v2 响应嵌套在既有 `detailes` 字段中，字段名与形状保持兼容。

`is_downed` 是当前事实，不保证后续不会恢复。`Wait_Downed` 是当前工作名称；`GET /api/v1/pawn/work-queue?pawn_id=` 的 `blocked_reasons:downed` 则来自原生 `Pawn.Downed` 判断，详见[工作队列](core-work-queue.md)。

两个版本的复数 colonist 详情仍使用既有 1800 game-tick 缓存；需要战斗或医疗的实时读回时，用单个人物详情或工作队列。此次修复不更改缓存期限。

## 地图生物汇总

`GET /api/v1/map/creatures/summary?map_id=0` 返回既有六个计数：殖民者、囚犯、敌人、动物、虫族、机械体。

`enemies_count` 统计该地图 `AllPawnsSpawned` 中原生 `HostileTo(Faction.OfPlayer)` 为真的所有 Pawn；包括符合敌对条件的非类人实体、机械体、动物及倒地者，不按隐身或玩家画面可见性过滤。分类不是互斥分区，一个敌对动物也可以计入 `animals_count`。

此计数限定原生 `mapPawns` 登记集合，不表示能立即攻击的数量；不包含尸体、尚在容器内/未 spawned 的人物及将来事件。原生 lister 中尚有未登记在 `mapPawns` 的对象时，也不在该计数范围内。因此 `0` 不能单独证明整个聚落长期安全；完整现场调查不能只读此汇总。

未知 `map_id` 返回 `404 / success:false`，读取异常返回 `500 / success:false`；不将失败转换为成功的全零计数。参数缺失或格式错误继续沿用现有统一参数错误处理。

## 修复范围

2026-11-16 补齐共享 DTO 的五项漏填字段；此前这些字段会返回默认 `false/0`。地图敌人计数此前额外限定 `RaceProps.Humanlike`，此次移除该限制。没有增加路由或新字段，不改变原生人物状态和战斗规则。
