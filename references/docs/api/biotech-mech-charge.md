# Biotech 机械充电（G-49）

交付：读取机械的 energy/自我关机/充电器，以及把自我关机的机械搬到充电器的正常 job。
证据：G49 启动/charging（`Reverse/evidence/dlc-G49-mech-charge.json`）；G71 充电完成
（`Reverse/evidence/dlc-G71-mech-charge-completion.json`）。

## 读取

`GET /api/v1/dlc/biotech/mech?pawn_id=321` 的 `mechs[]` 增加：

```json
{"mech_id":"Mech_Scyther...","energy":0.498,"is_self_shutdown":false,
 "charger_id":null,"work_mode_def":"Work",...}
```

- `energy` 是 `Need_MechEnergy.CurLevelPercentage`；`is_self_shutdown` 是原生
  `Pawn.IsSelfShutdown()`（`SelfShutdown` job + Laying）；`charger_id` 是
  `Need_MechEnergy.currentCharger`。
- 该 GET 仍要求传入一名 mechanitor 的 `pawn_id`；充电端点本身不要求 mechanitor。

## 充电

`POST /api/v1/dlc/biotech/mech/charge`

```json
{"map_id":0,"mech_id":"Mech_Scyther...","pawn_id":"321","dry_run":true}
```

- 原生路径是 `FloatMenuOptionProvider_CarryMechToCharger` →
  `JobGiver_GetEnergy_Charger.GetClosestCharger` →
  `JobMaker.MakeJob(JobDefOf.HaulMechToCharger, mech, charger, charger.InteractionCell)`。
  本端点复用同一 job；`applied` 只表示已提交，搬运/充电完成要读回 `charger_id`/`energy`。
- 资格：机械须玩家、spawned、mechanoid 且 `IsSelfShutdown()`；人物须玩家、spawned、
  非倒地/精神、可操作、可达机械与充电器；机械/充电器未被他人预留。找不到可用充电器 422。
- dry-run 只投影 `charger_id/job_def/energy`，不创建 job。

## 实测

`ai_g31_fixture` 副本（tick 74742），fixture 生成玩家 `Mech_Scyther`、通电
`StandardRecharger` + 燃料发电机/导线，启动原生 `SelfShutdown`（带目标格），选一名殖民者。
API dry-run/正式排队后，机械被搬到充电器：`charger_id` 非空、`charging:true`、
`self_shutdown:false`；再次 charge 返回 422（已非自我关机）。

G71：`charging` 后 fixture 把 energy 加速到 `MaxLevel - 0.5`，再推进到 `MechCharge` job
结束；读回 `charging:false`、`self_shutdown:false`、`energy>=0.99`、`job!=MechCharge`、
充电器 `currentCharger` 清空。见
[G71 自审](../references/reviews/2026-11-08-g71-mech-charge-completion.md)。

## 限制

- 充电完成用 fixture 加速（非自然满电全周期）；未测低能量自然自我关机、充电器断电/占用/
  多机械、机械死亡/被毁；
- 未测 mechanitor 带宽/控制组与充电的交互；
- 未做 Cua 对照。
