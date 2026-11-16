# Odyssey Cerebrex 最终核心选择（OD-R01）

2026-11-12 新增：读取 `CompCerebrexCore` 的原生状态，并提交与
`Dialog_NodeTree` 三个选项相同的业务选择。`destroy`/`scavenge` 调用私有
`StartCoreDeactivation(bool)`；`postpone` 只关闭选择、不改核心。

2026-11-13 修正：选择前必须已由殖民者正常交互核心。`CompCerebrexCore.OnInteracted`
保存的交互人决定结局文本和摧毁核心的主角心情；API 不直接写这个字段。

## 读取

`GET /api/v1/dlc/odyssey/cerebrex-core?map_id=0&thing_id=75478`

```json
{"map_id":0,"thing_id":75478,"thing_id_string":"CerebrexCore75478",
 "def_name":"CerebrexCore","stabilizers_remaining":0,"defences_lowered":true,
 "deactivated":false,"can_interact":true,"can_interact_reason":"",
 "mechanoid_faction_deactivated":false}
```

`can_interact`/`can_interact_reason` 是原生 `CompInteractable.CanInteract`：
未降防时拒绝并给出剩余稳定器数；已停用拒绝。

追加 `interacted_pawn_id`、`choice_ready`、`choice_blocked_reason` 和
`interaction_endpoint`。尚未完成交互时，交互人为空、选择未就绪，原因是
`complete_native_interaction`；核心已停用时选择未就绪并返回原生原因。
五 DLC 同时启用的当前合同复用已有正常交互入口：

```json
POST /api/v1/dlc/anomaly/interactable
{"map_id":0,"thing_id":75478,"pawn_id":"Human321"}
```

该入口按原生资格排队 `OrderForceTarget` → `InteractThing`（检查核心需900tick），
由 `OnInteracted` 记录交互人。轮询核心至 `choice_ready:true` 后选择；无交互人时
`choose`（含 dry-run）返回409且不改变核心。`postpone` 后原生保留的交互人仍可读回。

## 选择

`POST /api/v1/dlc/odyssey/cerebrex-core/choose`

```json
{"map_id":0,"thing_id":75478,"choice":"postpone"}
{"map_id":0,"thing_id":75478,"choice":"scavenge","dry_run":true}
```

- `postpone`：返回 `postponed:true`，核心 `deactivated` 保持 false。
- `scavenge`：`StartCoreDeactivation(scavenge:true)`；300 tick 后
  `DeactivateCore(true)` 生成 `Apparel_CerebrexNode`、播放 Odyssey 制作人员
  名单；核心保留但 `deactivated:true`，机械派系仍未停用。
- `destroy`：`StartCoreDeactivation(scavenge:false)`；300 tick 后原核心被
  `CerebrexCore_Destroyed` 替换、`Faction.OfMechanoids.deactivated=true`、
  播放制作人员名单；准确交互人得到 `DestroyedMechhiveMainPawn`，其它玩家人物
  得到 `DestroyedMechhive`。
- 三个正式选择只关闭由准确核心回调捕获的 `Dialog_NodeTree`，返回
  `closed_choice_dialogs`；不关闭无关对话。`dry_run` 不关闭窗口、不写业务状态；
  `applied` 读原生 `deactivated` 字段，暂缓返回 `postponed:true`。
- 暂缓没有胜利；scavenge/destroy 都经 `GameVictoryUtility.ShowCredits` 创建原生
  `wonGame:true/exitToMainMenu:false` 的结局，可关闭制作人员名单继续当前殖民地。
  该原生淡出期间暂时没有玩家控制，调速须在淡出结束后进行。
- 合同：GET observe；POST direct/immediate/validate/game_memory。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC，Xvfb :101）：
fixture 生成两个 `CerebrexCore`，把原生 `defencesLowered=true`、
`stabilizersRemaining=0`（跳过三稳定器战斗，准备披露）。实测：
postpone 后核心未变；scavenge 后 `deactivated:true` + `Apparel_CerebrexNode`
落地且 Credits 窗口打开；destroy 后原核心消失、`CerebrexCore_Destroyed`
出现、机械派系 `deactivated:true`。测试只读原生结果，未直接调用
`DeactivateCore` 或改最终产物。
证据 `tools/cua/.runtime/od-r01-20261112/evidence/od-r01.json`，runner
`tests/od_r01_cerebrex.py`，fixture `prepare_cerebrex`/`observe_cerebrex`。

旧证据只证明产物/派系，未证明正确交互人。2026-11-13 补验见
[交互人修复自审](../references/reviews/2026-11-13-odyssey-cerebrex-actor.md)：
新fixture只准备降防核心，正常API交互、三选择和两条结局后继续移动均通过；
准确主角 `Human321`/Blean 的结局称呼、专属心情及其它人物普通心情独立读回。
证据为 Odyssey worktree 的
`tools/cua/.runtime/od-r01-actor-20261113a/evidence/od-r01-actor-run4.json`，
当前runner `tests/od_r01_cerebrex_actor.py`。

- 未测：三稳定器逐一摧毁的防御下降、真实 Mechhive 战斗/突袭及追击场景任务取得链、
  已有交互人存档恢复、其它核心变体、目标rev598/1.5。结局后菜单退出由协调者统一
  验收；本轮使用现有 `ui/window/close` 后执行公开API移动，不把窗口出现本身当继续成功。
