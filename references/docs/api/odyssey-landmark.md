# Odyssey 地标读取（G42）

维护：直接实施Codex；核对：2026-11-11；状态：准确定位/分页与代表身份保存恢复限定通过。

`GET /api/v1/dlc/odyssey/landmark?limit=100&offset=0&layer_id=0`

limit默认100，范围1..5000；offset默认0，为要跳过的地标行数，须为非负整数。
layer_id省略仍取根地表，显式值必须是存在的层。旧字段surface_layer_id、scanned_tiles、
landmark_tiles、row_limit、rows_truncated、landmarks、counts_by_def、def_count保留。
新增layer_id、row_offset和next_offset（还有后页时为下一偏移，否则null）。
计数始终覆盖选定全层，分页只影响返回行。offset越过尾部返回空列表。

每行保留tile_index/def_name/name/is_combo，增加准确layer_id/tile_id。
原生Tile公开tile字段，层内列表直接按tileId索引，当前原生世界tile_index等于tile_id。
旧文档“遍历序号不是PlanetTile id、没有公开ID”的说明已更正；仍不承诺跨不同世界数字稳定。

`GET /api/v1/dlc/odyssey/landmark/tile?layer_id=0&tile_id=1734`

layer_id/tile_id必须显式给整数。返回准确身份、landmark（def_name/name/is_combo，
该格无地标为null）及当前mutators Def名数组。定位直接使用WorldLandmarks的PlanetTile键，
不会把同号的轨道格子当地表格子。列表也支持轨道层；不存在地标如实返回空结果。

无Game409；缺Odyssey422；错误格式/limit/offset400；未知层或该层越界格子404。
新旧读取精确合同为`observe / none / dlc:odyssey`，不推进时间或生成地图，当前不走GET缓存。

地标原生写方法实际存在：已有`POST /api/v1/debug/native/execute`提供
world.landmark.add/remove，详见[开发者合同](../debug-native-actions.md)。添加会生成名字、
消耗RNG、添加适用mutators及可能的组合变体；移除仅清地标项，保留mutators和已生成地图。
这些是调试变更，不是正常探索/发现。没有新增同义写路由，不把Landmark仅有ExposeData误作无写能力。

[本组证据](../references/reviews/2026-11-08-g41-g42-codex-identity.md)：657地标的分页/计数/原生
字典一致；首例0/1734为Wetland/Marulo Fen，mutators为Muddy、Wetland、Fish_Increased。
暂停保存后新进程准确身份、全部地标及计数一致。正常目的地旅行/地图生成、自然组合及调试
地标变更跨进程未验；沿G40和矩阵继续。旧只读证据保留于`Reverse/evidence/dlc-G-40-42-engine.json`。


## 正常探索与设施组合

OD-03已正常旅行到LakeWithIsland并生成地图，地标/mutator身份保留；原只读批次的未验不再代表当前全状态。
OD-07c已验AncientComplex奖励选择/接受及终端Hack任务；首台正常工作，余两台HackNow缩时，限定EndedSuccess。
本轮补`SealedCrate`和`AncientSecurityCrate`：
`POST /api/v1/pawn/job`指定`pawn_id`、`job_def:"Open"`、`target_thing_id`，
经原生`Building_Crate`开箱并清空容器，Steel3真实落图，无需新增同义路由。
箱体/内容是fixture准备，结果由原生工作产生，见[修复自审](../references/reviews/2026-11-11-five-dlc-gap-repair.md)。

这不覆盖带hackable的锁定变体、所有地标/警报战斗或专属确认窗口；debug增删地标仍不是正常探索。
