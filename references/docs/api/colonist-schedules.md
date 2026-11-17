# 完整日程查询与批量编辑

维护：Codex；核对：2026-11-16；状态：已实现并通过统一隔离验证；正式DLL未部署。

`GET /api/v1/colonists/schedules` 返回全部现存殖民者的24小时时间表；可用 `pawn_id=Human803`（或数字ID）查询一人，`map_id=0` 限定地图。返回当前tick、`complete:true` 与 `colonists[]`：每项包括人物ID/名字/地图、24项 `assignments`（下标即0..23小时）、当前本地小时及原生当前作息。不缓存旧时间表。

这与 `GET /time-assignments` 不同：后者只列可选作息定义。允许活动区域也不是作息表。

```json
{
  "pawn_ids": ["Human800", "Human803"],
  "edits": [
    {"assignment":"Anything", "all_day":true},
    {"assignment":"Sleep", "start_hour":22, "end_hour":5},
    {"assignment":"Joy", "hours":[12,13]}
  ],
  "dry_run": false
}
```

向 `POST /api/v1/colonists/schedules` 提交。`pawn_ids` 非空列表与 `all_colonists:true` 必须二选一；可选map限定整个请求。每个edit选择以下一种模式：

- `all_day:true` + `assignment`：全天。
- `start_hour/end_hour` + `assignment`：含两端，均为0..23；结束小于开始时跨午夜，例如22→5。
- `hours` 非空列表 + `assignment`：指定小时；重复小时只处理一次。
- `assignments`：完整24项定义名数组，用于整体时间表替换/粘贴，不再同时传assignment。

edit按顺序执行，重叠小时以后项为准。全部人物、定义和小时在任何写入前验证；任一无效则不改任何目标。调用UI时间格同一原生 `Pawn_TimetableTracker.SetAssignment`，正常响应返回修改后的完整实际时间表，不只返回提交成功。

dry-run返回 `applied:false`、当前实际 `colonists` 和独立 `proposed`；不写原生时间表。正式请求支持Game内 `Idempotency-Key`，重复key/body不重复写或发布operation。未知人物/作息定义404，非法选择/小时/数组400，地图范围不符或无可用时间表422；Meditate需要Royalty。

旧 `POST /colonist/time-assignment` 的JSON或query参数保持：`pawn_id/hour/assignment`；现在复用同一预检与原生写入，返回该人的完整24项实际时间表，并支持JSON `dry_run` 和幂等。不会改变区域、工作优先级或直接指派Job；作息是策略，人物实际工作还受需求、征召、精神状态等原生规则影响。

当前“全部”按既有殖民者列表覆盖地图、商队和运输中的现存殖民者；只有有原生timetable的目标可编辑。读取不到对象不代表死亡或走失。
