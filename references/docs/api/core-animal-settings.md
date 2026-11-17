# Core 动物主人、跟随、放出与攻击目标（C-36）

2026-11-16 修复：新增clear_master:true明确恢复Master=null，与非空master_id/attack_target_id冲突400；null/省略master_id保持不修改，不改变跟随/训练。见[UI-13合同](ui-option-repairs.md)与[原生读回](../references/reviews/2026-11-16-ui-api-repair.md)。

2026-11-12 新增：

- `GET /animal/settings?animal_id`：原生 `playerSettings.Master`、
  `followDrafted`、`followFieldwork`、`animalsReleased`、
  `training.attackTarget`、Obedience/AttackTarget 学习状态和可训练项。
- `POST /animal/settings`：设置 master、follow、released、attack_target；
  master 要求同图自由殖民者且动物已学 Obedience；未知/不合格 master 422。

2026-11-13 攻击目标与取消（最小实现，端点/注册表不变）：

```json
{"animal_id":"Bear_Grizzly75478","attack_target_id":"Muffalo75479","dry_run":true}
{"animal_id":"Bear_Grizzly75478","clear_attack_target":true}
```

- `attack_learned` 以 Odyssey `TrainableDef` **`AttackTarget`** 为准（旧实现误查
  `Attack`，永远 false；`Attack` 仅作缺 def 时的兼容回退）。
- `attack_target_id` 必须走原生 `Pawn_TrainingTracker` 的 AnimalAttackTarget
  gizmo 路径：动物已学 `AttackTarget`、master 已分配且同图 spawned、目标距
  master ≤25.9 格、`FloatMenuUtility.GetMeleeAttackAction(..., ignoreControlled:
  true)` 能给出 action。成功后执行该 action（原生 `AttackMelee` +
  `TryTakeOrderedJob`）并写 `training.attackTarget` + `mindState.enemyTarget`；
  任一校验失败返回 422 + 原生 failStr/说明，且不写这两个字段。
- `clear_attack_target` 可选、默认 false；与 `attack_target_id` 同时给返回 400。
  为 true 时清 `training.attackTarget`，仅当在途 job 为 `AttackMelee` 且
  `targetA` 等于旧目标时才 `EndCurrentJob(InterruptForced)`（镜像原生 Cancel
  gizmo），不误中断其它 job。
- `dry_run` 仍不写字段、不派 job；新增字段同样受 dry 保护。
- 合同：GET observe；POST `policy/game_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 Bear_Grizzly（Advanced 训练性）并完成 Obedience（native Train
+ 学习表修正），生成 Silver 目标。GET 读回 master=Human321、Obedience true；
dry 不写；set 后 master/follow_drafted/follow_fieldwork/attack_target 全部
读回；released true 读回；缺失 master 422。
证据 `tools/cua/.runtime/c36-20261112/evidence/c36.json`，runner
`tests/c36_animal_settings.py`，fixture `prepare_animal_settings`。

2026-11-13 真实任务（`c36-20261113-run3`，同一 g35 副本 tick 76787 起，
port 8766；RIMAPI 源码含上面的最小实现；fixture
`CoreAnimalFixture.dll`）：
fixture 生成玩家 Bear_Grizzly（native `Train` 完成 Obedience/Release/
AttackTarget）、master Human315、野生 Muffalo75479 目标，并把它们放到可达位置。

- 跟随：`follow_drafted=true`（dry 不写）后由 fixture 用原生 draft 征召 master；
  动物 10 格外取原生 `FollowClose`（targetA=master），tick 76914 距离 4.12，
  GET 读回一致。关闭 `follow_drafted` 并解除征召后推进 900 tick（跳过前 300
  tick 的旧 job 过期）：无新 `FollowClose`，动物回到 GotoWander。
- 攻击：`attack_target_id=Muffalo75479`（dry 不写、不派 job）后正式响应
  `attack_target_id/enemy_target_id` 均为目标、`attack_action_started=true`；
  原生 job 为 `AttackMelee` 且 targetA=目标；因 debug 无敌 hook 拦截
  `Thing.TakeDamage`，仅在攻击掉血窗口临时 `debug/invulnerability=false`：
  tick 77880 目标 health 1.0→0.84947（掉血），随即将无敌恢复 true。
- 取消：`clear_attack_target=true` 后 `attack_target_id` 为空、`AttackMelee`
  结束；再推进 900 tick 无新攻击，目标 health 0.84947→0.85045（自然波动
  <0.001），动物回到 GotoWander。
- 负例：`attack_target_id` + `clear_attack_target` 同时给 400；把
  AttackTarget 学习临时置 false 后 dry 攻击返回 422
  `animal has not learned AttackTarget`。
  全程无手动 job 端点；收口 `desktop.py stop` 报告
  source_save/formal_dll/global_prefs unchanged。

- 未测：released/释放半径与找回、followFieldwork 分支、敌对阵营目标、多动物/
  多主人切换、攻击中目标死亡、存档读回。

单元交接与现场命令见 [core-animal.md](../exec-plans/active/parallel/core-animal.md)。
