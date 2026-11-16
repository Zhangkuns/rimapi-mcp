# Odyssey 捕鱼区与策略（OD-06）

`Zone_Fishing` 有独立创建/策略；普通 growing/stockpile 区不能代替。

## 端点

| 方法 | 路径 | 作用 |
| --- | --- | --- |
| GET | `/api/v1/map/fishing-zones?map_id=` | 列出每个捕鱼区的 `zone_id`、格数、`allowed`、`should_fish_now`、重复模式/次数、目标数量/鱼群比例、暂停字段、`owned_fish_count`、`fish_type` |
| POST | `/api/v1/map/fishing-zones` | `{map_id,cells[] 或 point_a/point_b,label?,dry_run}` 创建；只接受原生 designator 同条件的格 |
| POST | `/api/v1/map/fishing-zones/policy` | `{map_id,zone_id,allowed?,repeat_mode?,repeat_count?,target_count?,target_population_pct?,pause_when_satisfied?,unpause_at_count?,dry_run}` |
| DELETE | `/api/v1/map/fishing-zones` | `{map_id,zone_id}` 删除 |

- 创建校验：Odyssey 激活、`Fishing` 研究完成、格在界内且不属于其它 zone、`GetWaterBodyType != None`、
  地形可通行、`waterBodyTracker.AnyFishPopulationAt(cell)`；无效格计数随 422 返回。
- 策略字段即原生 `Zone_Fishing` 字段；`allowed` 无 public setter，按原生暂停 gizmo 写同一私有字段。
- 精确合同：这些路由沿用现有 zone/bill 直接控制器，不声明 `game_memory` 幂等；
  `dry_run` 不建 zone、不改字段。

## 证据与边界

- 2026-11-10 自审：3×3 fishable 水域创建 9 格 zone、策略读回、land 422 与无效模式 400、
  未知 zone 404、删除后列表消失（`Reverse/evidence/od06-fishing-20261110.json`）。
- OD-06b（2026-11-10 第二次）：岸边 4 格 Freshwater 区 → 原生 Fish job →
  `owned_fish_count` 0→1 → 达到 TargetCount/1 后 `should_fish_now:false`；删除生效
  （`Reverse/evidence/od06-fishing-work-20261110.json`）。
- 边界：不同鱼种、负面钓鱼结果、冰面/盐淡水组合未穷举；首轮湖心区 9000 tick 超时
  文件保留为 failed（`od06-fishing-zone-20261110.json`）。
