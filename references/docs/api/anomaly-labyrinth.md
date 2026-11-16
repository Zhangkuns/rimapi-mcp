# Anomaly 迷宫门与地面刻字（AN-R07）

2026-11-12 代表验收：复用通用 `POST /api/v1/dlc/anomaly/interactable`，由原生
`CompInteractable` 子类执行 `InteractThing`；不新增业务端点。

## 读取

`GET /api/v1/dlc/anomaly/interactable?thing_id=75478&pawn_id=Human321`

```json
{"interactables":[{"interactable_key":"CompLabyrinthDoor","type":"CompLabyrinthDoor",
 "label":"Force this door open","active":false,"can_interact":true,
 "ticks_to_activate":3600,"progress":0.0,
 "labyrinth_door":{"jammed":true,"hold_open":false}}]}
```

- `CompLabyrinthDoor`：`labyrinth_door.jammed` 来自原生 `Building_JammedDoor.Jammed`；
  `hold_open` 来自 `Building_Door.HoldOpen`。原生菜单只在 `Jammed` 时出现，
  `ticks_to_activate` 3600；开门后 `UnlockAndOpenDoor()` 置 `jammed=false`、
  `holdOpen=true`，并给殖民者做出视野泛光。
- `CompFloorEtchingRambling`（及其子类 `CompFloorEtching`）：追加
  `floor_etching.deciphered` 与 `floor_etching.message`；原生交互 2500 tick 后
  `deciphered=true` 并生成信件/文法文本。
- 未知 thing/pawn 404；无该组件返回空 `interactables`。

## 使用

`POST /api/v1/dlc/anomaly/interactable`

```json
{"map_id":0,"thing_id":75478,"pawn_id":"Human321",
 "interactable_key":"CompLabyrinthDoor","dry_run":true}
```

实际路径仍是 `CompInteractable.OrderActivation`→`JobMaker.MakeJob(InteractThing)`；
`applied` 以新 job 的目标为该具体对象为准。dry_run 不派 job。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101 1920×1080）：
fixture 生成一扇 GrayDoor（`unlockedChance` 可能原生解锁，fixture 将测试初始状态固定为
`jammed=true`）和一座 `FloorEtchingRambling`，并选一名空闲殖民者。
门：GET `jammed:true`→POST 200/`job_def:InteractThing`→推进后 `jammed:false`、
`hold_open:true`、`PawnCanOpen:true`，GET 同值。
刻字：GET `deciphered:false`→POST 200→推进后 `deciphered:true`、216 字消息，
GET 返回同一文本。
证据 `tools/cua/.runtime/an-r07-20261112/evidence/an-r07.json`，
runner `tests/an_r07_labyrinth.py`，fixture `prepare_labyrinth_items`/
`observe_labyrinth_items`。

- 怪异尸体解除研究门禁的独立结果已由 AN-R06（`1b3c0623`）覆盖，不在本行重复。
- 群体跨门货物、深坑入口的多人/货物装载归 OD-R05，不在此行。
- 未测：门在墙/门框中的真实放置、多个门并发、刻字方向文本（`CompFloorEtching` 的
  direction）与信件队列的后续交互。
