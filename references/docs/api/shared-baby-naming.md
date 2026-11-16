# Shared 出生信婴儿命名完成语义（S-11）

2026-11-12 新增：

- `GET /pawn/baby-name`：`Name`、名字三段、`baby_naming_deadline`、当前
  tick、`naming_open`、原生 `ChoiceLetter_BabyBirth` 是否存在。
- `POST /pawn/baby-name`：校验 deadline 未过期，写 `NameTriple`，将
  `babyNamingDeadline=-1`（与 `Dialog_NamePawn` 接受一致）。
- `POST /pawn/baby-name/delay`：在未过期时把 deadline 延后 1–3 天，保留
  letter 可命名状态。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 baby、`Baby` 名称和原生 birth letter（deadline 136787）。
GET naming_open/letter 均 true；dry 不写；delay 2 天后 deadline 196787；
命名 Rim/Rimmy/Tester 后 deadline -1、naming_open false，随后 letter 不再
present。证据 `tools/cua/.runtime/s11-20261112/evidence/s11.json`，runner
`tests/s11_baby_naming.py`，fixture `prepare_baby_naming`。

- 未重跑真实出生/成长信完整生命周期；本行以原生 deadline、NameTriple 和
  letter 状态读回为准。
