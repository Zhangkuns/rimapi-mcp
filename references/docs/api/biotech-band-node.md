# Biotech 带宽节点调谐（BT-R16）

2026-11-12 新增：`GET /dlc/biotech/band-node` 读取玩家带宽节点的
`state/tuned_to/tuning_to/tuning_time_left/power_on`；
`POST /dlc/biotech/band-node/tune` 调原生 `CompBandNode.TuneTo(pawn)`。

## 调谐

```json
{"map_id":0,"node_id":"BandNode75478","pawn_id":"Human321","dry_run":true}
```

- pawn 必须是同地图的 mechanitor（原生 `MechanitorUtility.IsMechanitor`）。
- 节点必须玩家所属；同一 pawn 已 tuned/tuning 返回 409。
- 调用 `TuneTo` 后由原生 `CompTick` 在供电下递减
  `tuningTimeLeft`；完成时写 `BandNode` hediff 并增加带宽。
- 返回节点状态/`tuning_to_id`/`applied`；dry_run 不写。
- 合同：POST `command/pawn_ai/validate/game_memory`。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成通电 BandNode，并给殖民者 Human321 原生 `MechlinkImplant`。
初始 Untuned；dry 保持；正式后 `Tuning/tuning_to Human321/tuning_time_left300`；
`advance_fast` 后 `tuned_to Human321/tuning_to null/pawn_band_node_hediff true`；
同一 pawn 再调 409。
证据 `tools/cua/.runtime/bt-r16-20261112/evidence/bt-r16.json`，runner
`tests/bt_r16_band_node.py`，fixture `prepare_band_node`/`observe_band_node`。

- 未测（2026-11-12 当时）：第二个 mechanitor 的重调谐/重调时间、断电暂停、
  死亡/离开地图、带宽数值变化；本行只补 `TuneTo` 正常入口和完成读回。

2026-11-13 第二 mechanitor 重调（`bt-r16-20261113-run1`，同一 g35 副本 tick 76787
起，port 8766）：
fixture 生成一个通电玩家 `BandNode`，并给两名成年殖民者原生 `MechlinkImplant`
（Human315/Human318，初始带宽各 6）。随后只走现 Tune 端点：

- 首次调谐 A：dry 保持 Untuned；正式 `Tuning/tuning_to Human315/
  tuning_time_left 300`；tick 77092 原生完成 `Tuned/tuned_to Human315`，
  A 获得原生 `BandNode` hediff（`additional_bandwidth 1`，总带宽 6→7），
  B 无 hediff；GET band-node/mech 读回一致。
- 重调 B：正式响应 `Retuning/tuned_to Human315/tuning_to Human318/
  tuning_time_left 180000`（原生 3 天 `retuneDays`，A 保持原归属）；
  fixture 把公开的 `tuningTimeLeft` 置 300 以界定时长；tick 77416 原生完成：
  `Tuned/tuned_to Human318/tuning_to null`，B 获得 `BandNode` hediff
  （additional 1、总带宽 6→7），A 的 hediff 在原生 60-tick recache 后移除、
  总带宽回到 6；GET band-node 与 `/dlc/biotech/mech` 读回一致。
  全程无直接字段写（只有 fixture 缩短 timer）。

证据 `tools/cua/.runtime/bt-r16-20261113-run1/evidence/bt-r16.json`；runner
`tests/parallel/biotech/bt_r16_band_node_retune.py`；fixture
`prepare_band_node_retune`/`shorten_band_node_retune`/`observe_band_node_retune`。
收口 `desktop.py stop` 报告 source_save/formal_dll/global_prefs unchanged。

- 未测：断电暂停、mechanitor 死亡/离开地图、多节点叠加带宽、存档读回。
