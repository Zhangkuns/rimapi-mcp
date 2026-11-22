# 畜栏、动物生产与正常牵引

维护：Codex；核对：2026-11-22；状态：既有牵引与新增畜牧接口分别通过隔离验证；正式 DLL 未部署。

本页使用 `/api/v1` 前缀，人物/动物/建筑ID采用原生字符串ThingID，例如 `Donkey4080`。GET不改变游戏规则或动物位置。写操作通过主线程协调器和命名原生Job执行，不直接写rope tracker或传送动物。

## 地图畜栏清单

`GET /animal/pens?map_id=0` 返回 `map_id/game_tick/complete:true/pens/hitching_spots`。只枚举建成畜栏标记和原生拴畜点；`includes_construction_plans:false` 明确排除蓝图和施工架，待建对象使用[施工查询](construction-status.md)。未知地图404。

每个pen包括原有[策略字段](core-pen-autoslaughter.md)、位置/派系以及：

- `enclosed/passable_doors/has_outside_access`：原生 `PenMarkerState`，不是按栅栏数量猜测封闭。
- `cells/cell_count`：封闭畜栏的原生ConnectedRegions精确格子并去重；未封闭时均为null，不把开放区域等同于安全畜栏。
- `animal_ids`：原生mapPawns中实际位于该封闭范围的动物；`accepted_animal_ids` 是其中被当前动物过滤器接受的子集。物理在栏内和策略允许是不同事实。多个标记可能对应同一个连通畜栏。

每个hitching_spot给出 `thing_id/def_name/position/faction_id/animal_ids`，最后一项来自动物原生 `RopedToHitchingSpot`，不是距离判断。

`complete:true` 只针对本清单，不代表牧草充足。营养收支用下面的 Food 接口，不用可见草量或 `accepted_animal_ids` 推断。

## 原版 Food 页营养与示例

`GET /pen/food?thing_id=PenMarker86415` 返回 `thing_id/map_id/game_tick/enclosed/nutrition`。开放畜栏 `nutrition:null`；已封闭时在游戏线程重新读原版 `PenFoodCalculator`，同 tick 的饲料变动也可见。

| `nutrition` 字段 | 含义 |
| --- | --- |
| `growth_per_day / consumption_per_day / balance_per_day / deficit` | 当日天然营养估算产出、原版消费量、产出减消耗、是否有缺口；单位为营养/天，不是草或干草件数 |
| `seasonal_growth` | 四个季度的 `quadrum/season/growth_per_day`，按地图纬度标记当地季节 |
| `animals` | Food 页实际动物按 `def_name/count/consumption_per_day` 汇总 |
| `stockpiled_nutrition / stockpiled_food` | 原版计算识别的栏内可食饲料及其总营养，分项 `def_name/count/nutrition` |
| `stockpile_days_remaining` | 仅有储备且当日有缺口时，按现有缺口计算的预计用尽天数并向下取整；否则为 `null`，不是长期供给预测 |
| `example_animals / available_example_defs` | 已显示示例的 `def_name/consumption_per_animal_per_day/estimated_capacity`，以及 Food 页还可添加的物种。容量是单物种用当日天然增长估算，不是扣掉现有动物后的余量 |

`POST /pen/food/example` 接受 `{"thing_id":"PenMarker86415","animal_def":"Turkey","displayed":true,"dry_run":true}`。添加或移除原版示例，不改动物或畜栏过滤器。响应含 `requested_displayed/displayed/dry_run/would_change/changed`；`displayed` 是实际状态，可用 `GET /pen/food` 独立核对。字段缺失 400、未知标记 404、开放栏或无效示例 422。

## 动物生产与标记

`GET /animal/production?animal_id=Yak6272` 返回 `life_stage`、动物身份/地图/tick 和 `milk/wool/eggs`。缺少对应组件为 `null`；奶和毛返回 `resource_def/progress/ready_to_gather`，蛋返回 `progress/can_lay_now/next_egg_def/fertilized/fully_fertilized`。进度为原生比例，不是百分数；就绪是组件状态，不保证有殖民者可执行收获工作。本接口不推进生产或收割。

`GET /animal/designation?animal_id=Turkey30623` 分别返回 `tame`、`slaughter` 的 `marked/can_add/reason`。`POST /animal/designation` 接受 `{"animal_id":"Turkey30623","designation":"tame","enabled":true,"dry_run":true}`；`designation` 也可为 `slaughter`，`enabled:false` 只撤销这只动物的指定标记。新增要求活体、已生成、当前地图且不在迷雾中，并通过原版工具资格。不自动切地图。缺参数 400、查不到动物 404、目标不合资格 422。

标记响应含两种实际标记和 `designation/requested_enabled/dry_run/would_change/changed`。正式新增保留原版副作用及警告（例如绑定动物屠宰警告，订阅 `/api/v1/events` 中的 `message_received`）；预演不添加。标记不等于动物已驯服或屠宰，也不等于自动屠宰候选。

两个新写接口支持 Game 内 [`Idempotency-Key`](idempotency.md)；同 key/body 重放保留原回执。只在实际 `changed:true` 时发一次 operation。见[隔离验收](../references/reviews/2026-11-22-husbandry-repair.md)。

## 单只动物状态

`GET /animal/pen?animal_id=Donkey4080` 读取现有解析器可找到的动物，包括其位置、地图/holder和阵营；未找到404，非动物422。

- `spawned/dead/downed/is_player_owned/map_id/position/holder_type`：身份与位置；未spawned时不推断已走失。
- `rope_managed/needs_rope_management`：原生物种资格和当前管理需求。
- `current_pen_id`：原生 `GetCurrentPenOf(...,false)` 的当前可用封闭畜栏；`current_pen_including_unenclosed_id` 是允许未封闭时的结果。
- `physically_inside_pen_ids`：实际所在的封闭连通范围，不受标记过滤器影响；因此可能在栏内但 `current_pen_id:null`。
- `is_roped/roped_by_pawn_id/rope_target/hitching_spot_id`：原生rope tracker；能区分被人牵着、拴在固定点与未拴住。
- `roaming_mental_state/can_reach_map_edge/current_job/handler_jobs`：实际Roaming精神状态、可达地图边缘、当前工作及指向该动物的牵引工作。它们不是将来走失保证；接口不返回推测的 `lost` 标志。

`needs_rope_management` 是原生管理资格判断，已入栏时仍可为true，不表示需要立即再牵一次。`can_reach_map_edge` 是原生寻路结果，拴在固定点时也可能为true，不能替代拴绳或入栏字段判断实际安置状态。

`pen/policy` 的既有读写路径保持，读取额外给出map/enclosed/passable_doors/has_outside_access；查不到标记蓝图不表示没有施工计划。`animal/settings` 的主人/跟随设置也不能作为入栏证据。

## 正常牵引

```json
{"animal_id":"Donkey4080","handler_id":"Human803","action":"to_pen","dry_run":true}
```

`POST /animal/rope` 只支持 `to_pen/to_hitch`。handler须为同地图可行动、未征召、无精神状态且启用Handling、具备Manipulation的自由殖民者；动物须为当前地图己方原生围栏动物。调用原生AnimalPenUtility的互动/预留/可达/过滤/封闭资格与目的地选择，沿用正常牵引不要求驯服训练技能的规则；不抢走另一handler正在牵着的动物。

`to_pen` 用原生 `WorkGiver_TakeToPen.MakeJob` 构造 `RopeToPen`；`to_hitch` 用原生 `RopeRoamerToHitchingPost`。目的地由原生选择；可选 `target_id` 只约束该结果，不允许绕过原生选择强制任意目标。已在合适畜栏中等情况下，原生没有待执行工作时返回拒绝。

响应含 `animal_id/handler_id/target_id/job_def/dry_run/submitted/animal_state`。dry-run验证相同资格，不派Job，并保护原生随机状态。正式 `submitted:true` 只代表原生工作已接受；须继续GET准确动物，核对实际 `current_pen_id/physically_inside_pen_ids` 或 `hitching_spot_id`，不能将请求中的target当作到达结果。

未知action/缺必需字段400；未知对象404；不合资格/没有合适目的地/目标约束不匹配422；另一handler已牵引或Job提交失败409。支持Game内 `Idempotency-Key`；相同key/body重放冻结响应，不再次派Job，正式成功提交仅发一次operation事件。见[幂等合同](idempotency.md)。
