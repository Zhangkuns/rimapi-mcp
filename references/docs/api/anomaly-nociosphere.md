# Anomaly Nociosphere 派遣（AN-06c）

`GET /api/v1/dlc/anomaly/nociosphere?map_id=&thing_id=` 读取平台内 Nociosphere 状态；
`POST /api/v1/dlc/anomaly/nociosphere/send` 把玩家点击的目的格交给原生派遣链。

## 写入

```json
{"thing_id":75479,"pawn_id":"321","cell":{"x":168,"y":0,"z":95},"dry_run":true}
```

- 读取：`on_platform`、`platform_id`、`study_progress`、`can_send`、`sent_onslaught`、
  `activity_level/state`、`unstable`、位置。平台内实体不是 `Spawned`，读取按 `AllPawns`。
- 资格：Odyssey/Anomaly 已激活；`CompNociosphere.CanSend`（平台内且 study ≥ 2）与
  `CanInteract(pawn)`；施放者必须是可用玩家殖民者。
- 目的格：原生 targeter 的 `mapBoundsContractedBy=1`、必须在界内且未起雾（422）。
- 目标被预留时 409 并返回 `reserver_id`（原生 `TryTakeOrderedJob` 会静默失败）。
- 提交前把目的格写入原生私有 `sendLocation`，再建与原生回调相同的
  `InteractThing` job；交互完成后 `NociosphereUtility.SkipTo` 与活动状态由原生执行。
- 返回 `applied`、`submitted`、`send_location`、before/after；`sent_onslaught` 需独立读回。

## 证据与边界

- 2026-11-10 自审：平台内 Nociosphere、缺 cell 400、非法格 422、dry-run 不变、
  派遣后离开平台并到达指定格、`sent_onslaught` 读回
  （`Reverse/evidence/an06c-nociosphere-20261110.json`）。
- 边界：`sendLocation` 仍依赖原生私有字段（缺失 501）；派遣后的自动破坏/活动结束与
  随机战果不是本接口的成功条件；未验证所有平台/地图层组合。
