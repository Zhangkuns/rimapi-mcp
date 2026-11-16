# Core 救援、俘虏、逮捕、送床与释放（C-30）

维护：Codex；核对：2026-11-12；状态：正常 API 与隔离游戏代表流程已验；正式 DLL 未部署。

路径前缀为 `/api/v1`，人物和床使用字符串 `ThingID`，例如 `Human75503`、`Bed75502`。
业务在游戏主线程执行，送床依赖原生 `JobDriver_TakeToBed`，无需人工打开窗口。

## 查询

`GET /pawn/take-to-bed?patient_id=Human75503` 按准确患者查询当前位置、存活/倒地、
`patient_spawned`、`patient_in_bed`、`patient_bed_id`、`patient_guest`、
`patient_carried_by` 和 `patient_holder`。搬运者换工作后仍可查询。
`patient_bed_id` 只表示当前实际床；尚未入床时为 null，不能把请求目标床当作当前床。
被抱持患者会暂时 despawn，须结合 carrier/holder 判断位置，不能据此判断丢失或死亡。

`GET /pawn/take-to-bed?actor_id=Human321` 保留当前 job/目标与抱持物读回；
`bed_id` 是工作目标床，不能代替患者实际床。两个选择字段同时给出时，`patient_id` 优先。
未知人物返回 404。GET 不提交工作，不发 operation 事件。

## 提交

```json
{"actor_id":"Human321","patient_id":"Human75503","bed_id":"Bed75502","mode":"rescue","dry_run":true}
```

向 `POST /pawn/take-to-bed` 提交：

| mode | 正常操作 |
| --- | --- |
| rescue（默认） | 合法倒地患者送到普通床，原生 Rescue |
| capture | 合法俘虏目标送到 prison cell 内囚犯床，原生 Capture |
| arrest | 合法逮捕目标送到囚犯床，原生 Arrest；普通目标需要征召搬运者 |
| escort_slave | 倒地殖民地奴隶送到奴隶床，原生 Rescue |
| release | 征召搬运者释放其正在抱持的准确患者，不要求 bed_id |

送床复用原生患者资格、床用途/可用性和寻路/预约检查，job 的 A 为患者、B 为床、count 为 1。
已抱持准确患者时复用 TakeToBed 的原生跳转，避免要求患者重新 spawned。
逮捕保留原生征召、倒地且有罪及额外共同阵营限制；原生抵抗与工作失败仍可能发生。

送床响应 `submitted:true` 只表示 job 已接受。之后查询准确 `patient_id`，以实际床、guest、
holder 判断结果。`dry_run:true` 校验同一资格，返回 `applied:false`，不提交或改状态。
`diplomatic_risk`、`target_faction`、`target_faction_hostile` 表达原生外交提示，
arrest 另返回 `arrest_accept_chance`；普通外交警告无需打开原生教程窗口或新增确认框架。

release 调用原生 `TryDropCarriedThing`，终止仍指向该患者的当前工作，返回
`applied:true`、`patient_spawned:true`、`patient_carried_by:null` 与 `carrying_after:null`。
未抱持准确患者或未征召时 409；无效正常目标/床为 422，未知对象 404，未知 mode 为 400。

正式成功提交/释放各发一个 `operation` SSE 事件，代表操作已提交/应用，不是入床完成。
dry run、拒绝和同 key 重放不发事件。支持 `Idempotency-Key` 的当前 Game 内重放；
同 key 返回首次冻结响应，终态仍应 GET 读回，见[幂等合同](idempotency.md)。

## 验证与边界

回归入口 `tests/c30_take_to_bed.py`；原四模式、非敌对俘虏、已抱持逮捕送床和释放共七阶段。
fixture 只准备房间/床、麻醉倒地患者和已抱持前置；提交、原生工作和释放由产品 API 执行。
准确终态、事件、DLL 身份、历史启动失败和清理见[收尾验收](../references/reviews/2026-11-12-c30-closeout.md)。
不声明所有床 Def、所有外交/RNG 结果、多搬运者竞争、跨地图/商队患者和其它游戏版本均已实测。
