# Biotech 基因库装载、单包策略与弹出（BT-R03）

2026-11-12 新增库级 `autoLoad` 与弹出；2026-11-13 补基因包指定库装载/撤销、
单包 `AutoLoad` 策略、库开关清队列和地图基因包读回。对应原生
`ITab_ContentsGenepackHolder.DoRow`、`CompGenepackContainer.leftToLoad/CanLoadMore/
EjectContents`、`Genepack.autoLoad/targetContainer`、
`WorkGiver_HaulToGeneBank` 和 `JobDriver_CarryGenepackToContainer`。

## 读取

`GET /api/v1/dlc/biotech/gene/bank?map_id=0`

- `banks[]`：`bank_id/def_name/auto_load/power_on/full/can_load_more/max_capacity/
  contents[]/left_to_load[]/left_to_load_details[]`；`left_to_load_details` 每项含
  `thing_id/auto_load/target_container_id/holder_id`。
- `packs[]`：当前地图上 loose 原生 `Genepack`：`thing_id/label/auto_load/spawned/
  position/target_container_id/holder_id`。已入容器/被搬运的包以 `banks[].contents`
  与原生 holder 读回为准。

## 设置/装载/弹出

`POST /api/v1/dlc/biotech/gene/bank`

```json
{"map_id":0,"bank_id":"GeneBank75478","auto_load":false}
{"map_id":0,"bank_id":"GeneBank75478","load":"pack","pack_id":"Genepack75483"}
{"map_id":0,"bank_id":"GeneBank75478","load":"cancel","pack_id":"Genepack75483"}
{"map_id":0,"bank_id":"GeneBank75478","pack_auto_load":false,"pack_id":"Genepack75484"}
{"map_id":0,"bank_id":"GeneBank75478","eject":"pack","pack_id":"Genepack75483"}
{"map_id":0,"bank_id":"GeneBank75478","eject":"all","dry_run":true}
```

- `load=pack`：`pack_id` 必须是同地图上部容器的 loose 包；按原生勾选逻辑写
  `pack.targetContainer=bank` 并加入 `leftToLoad`。已在别的库队列、已在容器内、
  已排本库或容量不足返回 409；未知包 404。
- `load=cancel`：对应原生取消勾选，清 `targetContainer` 并从本库 `leftToLoad`
  移除；队列中不存在 409。
- `pack_auto_load`：写 `Genepack` 的私有 `autoLoad`；设 false 时同时清该包的
  `targetContainer` 和装载名单（与原生取消勾选/裁剪后的 `TickRare` 终态一致）。
  之后 `WorkGiver_HaulToGeneBank` 不再为它派搬运 job。
- `auto_load` 变化时清空本库 `leftToLoad`（原生 `ITab_ContentsGenepackHolder` 行为），
  返回 `cleared_left_to_load`。库级 `autoLoad` 为 true 时原生 haul 工作仍会自主
  收集该库的 loose 包。
- `eject=pack` 按准确 `pack_id` 从该库 `innerContainer` 原生 `TryDrop`；`eject=all`
  调 `EjectContents`；旧合同保留。
- 返回 `before_auto_load/before_pack/requested_*/cleared_left_to_load/pack/packs/
  applied`；`dry_run` 不写。合同：GET observe；POST direct/immediate/validate/
  game_memory。

## 证据与边界

2026-11-13 隔离 `g35_deathresting` 副本（tick 76787，五 DLC），run
`tools/cua/.runtime/bt-r03-20261112-run4/`：

- fixture 在殖民者附近生成通电玩家 GeneBank（autoLoad false、空容器）和两个 loose
  原生 Genepack（FireSpew/Hemogenic），选一名殖民者（工作优先级 UI 开启、Haul=1、
  解除区域限制、打断一次当前 job）。
- `load=pack` dry_run 保持；正式后 `left_to_load=[Genepack75483]`、
  `target_container_id=GeneBank75478`。原生 haul 于 tick 77037 前完成入柜，
  `contents=[Genepack75483]`、队列空、`targetContainer=null`。
- 第二次排队在 Fast 速度下观测到原生 `CarryGenepackToContainer`（tick 78050，
  `carried_pack=Genepack75484`），tick 78056 入柜，`contents` 两包、队列空。
- `load=cancel` 清目标与队列；库 `auto_load true` 清队列返回
  `cleared_left_to_load=1`；单包 `pack_auto_load=false` 后库 autoLoad=true 运行
  600 ticks，该包仍 loose/未入柜，恢复 true 后可再次正常排队搬运。
- 回归：单包弹出后 `contents` 剩 1 包且包落地，`eject=all` 后空、两包均 loose；
  未知包 404、未排队 cancel 409、缺 `pack_id` 400。
- 证据 `tools/cua/.runtime/bt-r03-20261112-run4/evidence/bt-r03.json`；runner
  `tests/parallel/biotech/bt_r03_genebank.py`；fixture
  `tests/parallel/biotech/BtFixture.cs`（`prepare_genebank_load`/
  `observe_genebank_load`/`advance`）。
- 未测：断电/满容量、多库竞争、包被禁用、运输中途取消的 job 失败细节；库被摧毁时
  的原生内容处理沿用旧边界。
