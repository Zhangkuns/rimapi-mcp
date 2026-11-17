# 畜栏、拴绳状态与正常牵引

维护：Codex；核对：2026-11-16；状态：已实现并通过统一隔离验证；正式DLL未部署。

本页使用 `/api/v1` 前缀，人物/动物/建筑ID采用原生字符串ThingID，例如 `Donkey4080`。GET不改变游戏规则或动物位置。写操作通过主线程协调器和命名原生Job执行，不直接写rope tracker或传送动物。

## 地图畜栏清单

`GET /animal/pens?map_id=0` 返回 `map_id/game_tick/complete:true/pens/hitching_spots`。只枚举建成畜栏标记和原生拴畜点；`includes_construction_plans:false` 明确排除蓝图和施工架，待建对象使用[施工查询](construction-status.md)。未知地图404。

每个pen包括原有[策略字段](core-pen-autoslaughter.md)、位置/派系以及：

- `enclosed/passable_doors/has_outside_access`：原生 `PenMarkerState`，不是按栅栏数量猜测封闭。
- `cells/cell_count`：封闭畜栏的原生ConnectedRegions精确格子并去重；未封闭时均为null，不把开放区域等同于安全畜栏。
- `animal_ids`：原生mapPawns中实际位于该封闭范围的动物；`accepted_animal_ids` 是其中被当前动物过滤器接受的子集。物理在栏内和策略允许是不同事实。多个标记可能对应同一个连通畜栏。

每个hitching_spot给出 `thing_id/def_name/position/faction_id/animal_ids`，最后一项来自动物原生 `RopedToHitchingSpot`，不是距离判断。

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
