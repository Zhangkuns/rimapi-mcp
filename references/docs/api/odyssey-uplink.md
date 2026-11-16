# Odyssey 古代上行终端破解与轨道任务（OD-R09，代表验收）

2026-11-12 代表验收：不新增业务端点。使用既有
`POST /dlc/odyssey/hackable/auto` 开启自动破解，再由原生
`WorkGiver_Hack` 完成 `AncientUplink`；完成时
`CompAncientUplink.Notify_Hacked` 用 `QuestGiverTag.OrbitalScanner` 的
QuestScriptDef 生成一个正常可接受任务。

## 证据

fixture 在 `g35_deathresting` 副本（tick 76787，五 DLC）铺 `Concrete` 重地形，
生成一座 `AncientUplink`（原生 `defence=6000`、Intellectual 6），选择
Intellectual 7 的空闲殖民者并把 Research 优先级设为 1、清空当前研究项目。
`GET /dlc/odyssey/hackable` 初始 `is_hacked:false/work_available:false`；
开启 autoHack 后正常 Hack job 完成，读回 `is_hacked:true/progress 1.0`；
`GET /quests` 出现 `OpportunitySite_Asteroid`（"Uranium Ore Asteroid"），
描述明确写出该殖民者破解 uplink 后发现小行星。测试用 `advance_fast`
（仅测试的 `DoSingleTick` 时间压缩）推进 6000 防度的正常破解，未直接改
progress/调用 `HackNow`。

证据 `tools/cua/.runtime/od-r09-20261112/evidence/od-r09.json`，runner
`tests/od_r09_uplink.py`，fixture `prepare_uplink`/`observe_uplink`。

- 代表范围：锁定舱门/箱与机械继电器稳定器的独立结果未在本行重跑；
  稳定器摧毁→机械派系停用/坠落归 OD-R01 与后续测试，箱子开锁归共享容器链。
- 未测：lockout 失败、`onlyRemotelyHackable`、任务接受/完成、多个上行终端。
