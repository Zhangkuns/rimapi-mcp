# 驱逐与待处决标记

两个正常人物角色卡指令：

- `POST /api/v1/pawn/banish`：按原版 `PawnBanishUtility.Banish` 驱逐合格殖民者。
- `POST /api/v1/pawn/execution-mark`：设置/取消 `Pawn_GuiltTracker.awaitingExecution`，只写标记，不处决。

## 驱逐

```json
{"pawn_id":"27985","confirm":true,"dry_run":false}
```

原生角色卡先弹确认；本 API 用 `confirm=false`（默认）返回 409 与 `PawnBanishUtility.GetBanishPawnDialogText` 的原文，`confirm=true` 才调用 `PawnBanishUtility.Banish(pawn)`。

前置：spawned、存活、`IsFreeColonist`、玩家阵营、非倒地、非 `IsQuestLodger`。不合格返回 422。`would_leave_to_die` 来自 `PawnBanishUtility.WouldBeLeftToDie(pawn, pawn.Tile)`，只作提示。

真实终态由原生决定：通常 `Faction=null`、`IsFreeColonist=false`，spawned/位置可能保持（不是强制离图）；也可能按原版选到其他 faction。响应读回 `faction`、`host_faction`、`spawned`、`map_id`、位置、`is_colonist`/`is_free_colonist`/`is_quest_lodger`/`downed`、`banished`、`left_player_faction`、`left_map` 和 `outcome`，不声称“必定离开地图”。不会调用 kill。

## 待处决标记

```json
{"pawn_id":"27991","awaiting_execution":true,"dry_run":false}
```

前置：spawned、存活、`IsFreeColonist`、玩家阵营、非 quest lodger、`guilt != null && guilt.IsGuilty`。`awaiting_execution=true/false` 显式设置；`false` 是取消标记。它只写 `Pawn_GuiltTracker.awaitingExecution`，不改 guilt、不造成伤害、不调用 kill。

响应 `guilt_before`/`guilt_after`：`awaiting_execution`、`is_guilty`、`ticks_until_innocent`；`applied`/`changed`/`native_invoked`；`outcome=awaiting_execution_marked|cleared`。驱逐同样返回 `applied`/`changed`/`native_invoked`。

## 错误码

| 情况 | 状态 / `error_code` |
| --- | --- |
| 缺 `pawn_id`；待处决接口缺 `awaiting_execution` 或传 null | 400 `bad_request` |
| 人物不存在 | 404 `not_found` |
| 非自由殖民者、非玩家阵营、倒地、quest lodger；未确认/不可驱逐；非有罪人物设置处决标记 | 422/409（确认缺失为 409 `conflict`，其余 422 `invalid_target`） |
| 驱逐原生异常 | 500 `internal_error` + 已读回状态 |

## 正常指令语义

两条路由都是正常玩家指令，不使用 `Idempotency-Key` 缓存：重复驱逐第一次后返回 422；重复设置同一标记是合法的再次写入（`changed=false`）；重复取消同理。没有刷人物/刷处决。
