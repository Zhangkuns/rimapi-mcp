# 单次包扎/治疗（WORK-D24）

`POST /api/v1/debug/native/execute` 的 `work.tend` 对一名现有病人执行**一次**原版 `TendUtility.DoTend(doctor, patient, Medicine)`。它只完成一次治疗，**不保证伤病痊愈**；原生只处理 `GetOptimalHediffsToTendWithSingleTreatment` 选中的 hediff，并写入 `TimesTendedTo`/`TimesTendedOther` 记录。

```json
{"action":"work.tend","map_id":0,"thing_id":"Human803","pawn_id":800,"ingredients":["MedicineIndustrial27979"],"dry_run":true}
```

## 参数

- `map_id` 可选；缺省当前地图（无当前地图时 400）。
- `thing_id` 病人、`pawn_id` 医生必填。
- `ingredients` 可选；恰好一个地图上的真实 `Medicine` ThingID。省略或空数组表示原版裸手治疗（`medicine=null`）；多于一个 400。
- `dry_run` 只读预演；不调 `DoTend`、不消耗药品、不改 hediff/记录。
- 可设 `expected_process_id`；同一次游戏内重试用 `Idempotency-Key`，见[通用调试合同](../debug-native-actions.md)和[幂等合同](idempotency.md)。

## 原生语义

1. 病人必须 spawned、存活、`health.HasHediffsNeedingTend()`；医生同图、spawn、存活、未倒地、玩家阵营、`Manipulation` 容量可用。
2. 药品若提供，必须是 `Medicine`（`ThingWithComps` 子类）、未销毁、`stackCount>=1`；本动作不把其它药品/食物冒充 Medicine。
3. 原生 `CalculateBaseTendQuality(doctor, patient, medicine?.def)` 计算基准质量（医生 MedicalTendQuality，无医生时默认 0.75；无药时 medicinePotency=0.3、max=0.7；有药时用药品 potency，最终 clamp 到 MedicalQualityMax），再用 `GetOptimalHediffsToTendWithSingleTreatment` 选出本次要治的 hediff（同一 `tendAllAtOnce` 或合计 ≤20 的若干 `Hediff_Injury` 可一次处理）。
4. 对选中的 hediff 调 `Tended(quality, maxQuality, index)`；`HediffComp_TendDuration` 写入带随机扰动的 `tendQuality` 和 tend 计时，但**不降低 severity**，所以一次 tend 不等于治愈。
5. 原生最后：药品 `stackCount>1` 时减 1，否则 `Destroy()`；病人 `TimesTendedTo++`、医生 `TimesTendedOther++`（医生可能等于病人）。其它原生记录/社交/任务/意识形态副作用随原版发生。

## 读回

- `patient`/`doctor`：thingID/pawnID、def、位置、spawned/dead/downed/faction、Manipulation、MedicalTendQuality、`hediffs_needing_tend`。
- `hediffs_before`/`hediffs_after`：def、part、severity、tended、tend_quality、tendable、permanent。
- `would_tend`：`GetOptimal...` 预选列表、`base_quality`、`max_quality`；`dry_run` 的核心读回。
- `treated[]`：实际被选中的 hediff、severity、tended、tend_quality、base/max quality；`treated:true`。
- `medicine_provided`、`medicine_before`/`medicine_after`、`medicine_used`（`stack_before/after`、`destroyed`、`consumed:1`）；裸手时三者均为 null/`medicine_provided=false`。
- `records_before`/`records_after`：病人 `patient_times_tended_to`、医生 `doctor_times_tended_other`。
- `items_total_before/after`、`applied/native_invoked/changed`。
- `changed` 表示本次确实选中并治疗了 hediff（即使质量被随机夹到 0）；`tended` 与 `tend_quality` 如实记录原生结果，质量 0 不等同失败。

## 状态码与边界

- 缺 `thing_id`/`pawn_id`：400；地图/病人/医生/药品不存在：404。
- 病人无待治 hediff、非病人、死亡/未 spawn；医生死亡/倒地/不同图/非玩家/无 Manipulation；药品非 Medicine/已毁/数量不足；多药或无选中 hediff：422。
- 关闭开发者模式：403；进程不符/幂等键冲突：409；原生异常：500 uncertain，保留实际 hediff/药品/记录读回。
- 同 key 重放不再治疗/扣药；同 key 换 body 409；已无待治伤病的新 key 422。
- 不批量、不治疗手术/植入、不保证痊愈、不替代正常医疗 Job；不直接改 hediff severity/tendQuality 或药品库存。

## 验收证据

隔离验收使用复制存档、独立 profile/Xvfb 与暂停现场，首尾 `TicksGame=103272`；用两处 Cut 验证一次药物 tend 同时治两处、药品 2→1；裸手治一处且不扣药（质量 0 为原生随机夹取）；最后一瓶药物 1→0 被销毁；记录 +1/次；失败路径不扣药。详见[WORK-D24 验收与自审](../references/reviews/2026-11-21-work-d24-tend.md)和[原始证据](../../tests/parallel/work-d24/evidence-work-d24-tend.json)。正式 DLL 未部署。
