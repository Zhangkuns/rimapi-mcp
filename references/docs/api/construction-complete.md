# 指定蓝图/施工架即时完成（调试）

维护：Codex；核对：2026-11-17（America/Los_Angeles）；状态：实现、隔离限定验收与自审完成；正式DLL未部署。

复用`POST /api/v1/debug/native/execute`，新增白名单动作`construction.complete`。与`builder/paste`直接生成建筑分开：本动作定位现有施工对象，沿其原生转换/结算链完成，不接受成品Def或任意坐标来生成替代品。

```json
{"action":"construction.complete","thing_id":"Frame12345",
 "map_id":0,"pawn_id":321,"dry_run":true}
```

`thing_id`必填，接受ThingID或数字ID字符串，从`GET /builder/constructions`取得。可选`map_id`限定已加载地图；省略按准确ID搜索已加载地图。只接受玩家所属且已spawn的`Blueprint_Build`（含储存蓝图）、`Blueprint_Install`或`Frame`。

可选`pawn_id`为同图存活玩家人物数字ID，需原生skills；省略按ID选首名同图存活自由殖民者，并返回`worker_id/worker_number/worker_selection`。人物用于品质、艺术、建造/安装记录与通知，不派发工作、不要求走到现场；无可用人物404，不生成工人。

## 原生执行与结果

- Blueprint调用虚`TryReplaceWithSolidThing(worker,out created,out jobEnded)`；新建Frame或输入Frame再调用`CompleteConstruction(worker)`。
- 重装先用原生`MinifyUtility.MakeMinified`卸下既有建筑并更新该蓝图引用，再走安装工厂。安装/重装移动原物，保留其身份和已有品质，不是克隆。
- 这是debug：跳过缺失材料和剩余工量；Frame已有材料由原生结算消耗，不返还或补发缺失材料。品质、艺术、风格/储存设置、HP比例与通知由原生链处理，不保证固定品质。
- 先用`GenConstruct.FirstBlockingThing`检查阻挡，422拒绝且不结束所选人物当前工作；不执行paste的主动清场。原生Spawn的占格、移动/退料及通知语义仍存在，不承诺任意重叠对象完全不变。
- 成功需源对象已销毁/离场，实际成品或地形层匹配目标。返回`completed/applied:true`、实际`native_methods`、`before/after`和`result`。Thing结果含成品准确ID/Def/材料/风格/品质/HP/位置/旋转；地形含真实Def、层、坐标和map_id。
- `dry_run:true`只预检与读回，`would_complete:true`、`applied/native_invoked:false`，不生成Frame、结算或消耗材料。

沿用原debug入口的活动Game/非加载、DevMode与可选`expected_process_id`检查，主线程、`game_memory`幂等和operation。相同`Idempotency-Key`与请求冻结重放，源对象已消失也不会重复执行；同key异体409。新请求再用已消失源ID返回404，不按位置猜测另一个对象。

缺thing_id400；未知地图/目标/人物404；错误类型/敌对目标/阻挡或不合格人物422；未开DevMode403；进程不匹配/无Game409。原生执行或终态读回异常500，`applied:null/outcome:uncertain`并附源对象/中间对象已知状态；可能已有部分效果，不回滚或自动重试。

实施与验证见[完成计划](../exec-plans/completed/2026-11-17-construction-complete.md)和[限定验收](../references/reviews/2026-11-17-construction-complete.md)：8类原生终态、33HTTP、8次重放/一次operation通过，副本tick未推进。地基/临时地形层与重植变体未另取样。此调试能力不用于核销正常施工材料、搬运与小人工作流程。
