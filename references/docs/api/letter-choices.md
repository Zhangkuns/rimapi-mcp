# Shared 任务/成长选择信（S-10）

`GET/POST /api/v1/letter/choices`。只扫描 `Find.LetterStack` 中的原生
`ChoiceLetter`；不扫任意 `Letter`。

合同状态：**AcceptJoiner（accept/reject）、ChoosePawn、BabyToChild 原生
回调已验**；AcceptVisitors/BetrayVisitors/RansomDemand 已做源码审计，未逐个
实际构造。

## GET

`GET /api/v1/letter/choices`

```json
{"count":1,"letters":[{
 "letter_id":10003,"type":"ChoiceLetter_AcceptJoiner",
 "label":null,"title":"S10 joiner","text":"A joiner asks to join.",
 "quest_id":0,"archived":false,"can_dismiss":false,"look_targets":[],
 "options":[
  {"option_id":"letter:0:Accept|enabled|RimWorld.ChoiceLetter_AcceptJoiner|...",
   "text":"Accept","disabled":false,"disabled_reason":null,
   "has_action":true,"action_type":"...","action_method":"...",
   "has_link":false,"resolve_tree":true,"selectable":true},
  {"text":"Postpone","has_action":false,"selectable":false}
 ]}]}
```

- `letter_id` 为原生 `Letter.ID`；`option_id` 包含 index/文本/action 签名，
  POST 需原样回传，变化/过期 409/404。
- 关闭/推迟/跳转等无 action 选项仍列出，但 `selectable:false`；
  关闭窗口不是选择。

## POST

```json
{"letter_id":10003,"option_id":"letter:0:...","confirm":true}
```

- 重新枚举并校验 option 签名；只执行原生 `DiaOption.action`。
- 不手发 signal、不改钱款、不移除信；action 自身
  `Find.SignalManager.SendSignal`/`Find.LetterStack.RemoveLetter` 后如实读回
  `applied/present_after/removed`。
- 不构造任意委托、不打开任意 UI、不强行关闭窗口。
- 非 dry 需 `confirm:true`；dry 只读回；disabled/无 action 409；letter/option
  过期 404；缺 option/confirm 400；Idempotency=none。

## 状态码

| 状态 | 条件 |
| --- | --- |
| 200 | GET / dry-run / native action |
| 400 | 缺 letter_id/option_id/confirm |
| 404 | letter/option 不存在 |
| 409 | option 变化、disabled、无 action |
| 500 | 原生 action 抛错（附 present_after） |

## 验证与边界

- runner：`tests/s10_choice_letters.py`（passed）；
  证据：`tools/cua/.runtime/s10-letters-20261113h/evidence/s10.json`。
- fixture：`tests/parallel/shared-choice-letters/CoreChoiceLetterFixture.cs`
  构造真实 `ChoiceLetter_AcceptJoiner`/`_ChoosePawn` 并加入 LetterStack；只准备，
  不执行 choice。
- 已验：AcceptJoiner Accept/Reject 原生 action → `S10.Accept`/`S10.Reject`
  signal，letter 移除；ChoosePawn 候选 action → `S10.Choose`，letter 移除；
  Postpone 无 action 不可选；dry 不发 signal；缺 option 400/未知 404。
- 已验 BabyToChild：选择原生 `Enslave` action 后 letter 移除，
  pawn GuestStatus → Slave（`guest_status:Slave`）。
- 源码审计未实测：AcceptVisitors、BetrayVisitors、RansomDemand；其
  `Choices` 机制同原生 `DiaOption.action`，但未逐个构造/读回钱款/访客结果；
  不报已验。
- 不重做 S-05/S-01/C-39；GrowthMoment/CreepJoiner 专用处理保留。
