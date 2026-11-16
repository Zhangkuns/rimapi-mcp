# Core 医疗默认与个体护理设置（C-28 部分）

2026-11-12 新增：

- `GET /medical/defaults`：`PlaySettings` 11 类默认医疗等级。
- `POST /medical/defaults`：按 colonist/prisoner/slave/tamed_animal/
  friendly/neutral/hostile/no_faction/wildlife/entities/ghouls 设置。
- `GET/POST /pawn/medical-settings`：个体 `medCare`、`selfTend`。

## 证据与边界

隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：初始 colonist Best；
dry 不写；设置 colonist HerbalOrWorse/prisoner NoCare 读回；个体
med_care Best/self_tend true 读回；非法类别 400。
证据 `tools/cua/.runtime/c28-20261112/evidence/c28.json`，runner
`tests/c28_medical_policy.py`。

## 带药 tend

`POST /pawn/medical/tend` 新增 `medicine_thing_id`、`count`、`drafted_tend`：
按病人地图定位药物，构造 `TendPatient` 的 targetB、必要时 targetC，写入
count/draftedTend，仍由原生 job 执行。

2026-11-12 隔离副本：关闭无敌后对生成病人造成 Cut 10，卧床且
`cut_tendable:true`；显式 medicine count 1 的 job 完成后 `tended:true`，
药物 9→8，cut severity 10→6.44。证据
`tools/cua/.runtime/c28t-20261112/evidence/c28t.json`。

- 未重跑征召状态下的 draftedTend 分支；默认/个体/带药/消费已读回。
