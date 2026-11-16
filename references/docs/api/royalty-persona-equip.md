# Royalty 人格武器确认、装备与绑定（RY-R08）

2026-11-12 修复：`POST /jobs/make/equip` 在 `EquipmentUtility.CanEquip`
对未初始化 bladelink 抛错时，回退到原生 `JobDefOf.Equip` job；装备完成由
原生装备/人格武器逻辑编码。

2026-11-12 补齐 G 项：新增专用 `GET/POST /api/v1/dlc/royalty/persona`。
读取原生 `EquipmentUtility.CanEquip` 与
`GetPersonaWeaponConfirmationText`，把绑定警告/trait 标签/描述作为结构化
`confirmation_text`，未确认时返回 400；确认后只排原生 `JobDefOf.Equip`。
本路径**不做** CanEquip 异常回退，异常返回 500 且不排 job。协调者已在
`af8c7b52` 为共享 `/jobs/make/equip` 加 gate：对 `CompBladelinkWeapon`
返回 409 并指向本端点，避免绕过确认。合并后复验见
`tools/cua/.runtime/ry-r10-20261112c/evidence/ry-r08-merged.json`。

## 专用确认/装备

`GET /api/v1/dlc/royalty/persona?pawn_id=Human321&thing_id=MeleeWeapon_PlasmaSwordBladelink75478`

```json
{"pawn_id":"Human321","weapon":{"thing_id":"...75478","thing_def":"MeleeWeapon_PlasmaSwordBladelink",
  "label":"Gersergarys, persona plasmasword (normal)","traits":[
    {"def":"NoPain","label":"Painless","description":"...",
     "equipped_hediffs":["NoPain"],"bonded_hediffs":[],"never_bond":false}],
  "expected_hediffs":["NoPain"],"biocoded":false,"coded_pawn_id":null,
  "already_bonded":false,"pawn_primary_def":null,"bonded_weapon_id":null},
 "can_equip":true,"cant_reason":null,"native_error":null,
 "requires_confirmation":true,"confirmation_text":"Once bonded ... Painless: ... Are you sure ..."}
```

`POST /api/v1/dlc/royalty/persona`
`{pawn_id,thing_id,confirm:true,dry_run:false}`

- `CanEquip` 检查：已绑定他人、biocode、已有 bondedWeapon、角色限制、服装阶段；
  不满足 422 并带 `cant_reason`。
- `confirmation_text` 非空时（未编码给该 pawn 的人格武器）必须 `confirm:true`，
  否则 400 并原样返回文本；`dry_run` 只投影 `applied:false/job_started:false`。
- 正式只 `JobMaker.MakeJob(JobDefOf.Equip, thing)` + `TryTakeOrderedJob`；编码、
  bondedWeapon 和 trait hediff 都由完成后的原生逻辑产生。
- `thing_id` 可解析地图/装备栏/背包中的同一武器；`thing` 非人格武器 501，
  pawn/thing 不在同图 422，未知 404。

2026-11-12 专用端点证据：隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成带 NoPain 的 PlasmaSwordBladelink 和空手 Human321。GET
`can_equip:true/requires_confirmation:true`，文本含 `Painless` 及确认句；
未确认 400；dry 不排 job；正式 `job_started:true/job_def:Equip`；推进 4000 tick 后
primary/codedPawn/bondedWeapon 均为该武器，`NoPain` hediff 存在；GET 再读
`already_bonded:true/requires_confirmation:false/text:null`；第二名殖民者申请 422
`bonded to someone else`。证据
`tools/cua/.runtime/ry-r08-20261112e/evidence/ry-r08.json`，runner
`tests/parallel/royalty/ry_r08_persona_confirm.py`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成玩家 `MeleeWeapon_PlasmaSwordBladelink`，选择空手殖民者。
装备 job 前 primary 为空；推进 4000 tick 后 primary 为该武器，
`equipment.bondedWeapon` 指向同一武器。biocoded 组件不适用于该人格武器。
证据 `tools/cua/.runtime/ryr08-20261112/evidence/ryr08.json`，runner
`tests/ry_r08_persona.py`，fixture `prepare_persona/observe_persona`。

- 未重跑 UI 人格武器确认文本与全部 trait 数值效果；本行 V 以正常装备 job
  完成和原生 bondedWeapon 读回为准。
