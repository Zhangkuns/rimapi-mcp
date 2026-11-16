# Core 电源开关正常 Flick 与电网重连（C-25）

2026-11-12 新增：`GET/POST /api/v1/building/power-switch`。`POST` 写
`CompFlickable` 私有 `wantSwitchOn` 并调用 `FlickUtility.UpdateFlickDesignation`，
由原生 Flick 工作完成实际切换；`cancel` 清除待切换；`reconnect` 调
`CompPower.TryManualReconnect`。既有直接 `SwitchIsOn` 的接口保留独立标识。

## 读取

`GET .../power-switch?thing_id=StandingLamp75478` →
`switch_on/want_switch_on/wants_flick/flick_designation/power_on/has_power_net`。

## 设置

```json
{"thing_id":"StandingLamp75478","desired_on":false,"dry_run":true}
{"thing_id":"StandingLamp75478","cancel":true}
{"thing_id":"StandingLamp75478","reconnect":true}
```

- `desired_on` 与 `cancel` 互斥；至少给一项。
- 返回当前/请求状态和 `applied`；dry_run 不写/不派工。
- 合同：GET observe；POST command/pawn_ai/validate/game_memory。

## 证据与边界

2026-11-12 隔离 `g35_deathresting` 副本（tick 76787，五 DLC）：
fixture 生成 StandingLamp + 发电机/导线，选殖民者。初始 switch true；
desired false 后 want false/wants flick/designation true；`advance_fast` 3000 后
switch false；desired true 后 3000 tick 恢复 true；再次 desired false 后 cancel
清除 designation/want 恢复 true；reconnect 200/applied。
证据 `tools/cua/.runtime/c25-20261112/evidence/c25.json`，runner
`tests/c25_power_switch.py`，fixture `prepare_power_switch`。

- 未测：断电/断网后的真实重连成功（本 fixture power_on false，has_power_net
  true）、工作途中取消、多个 flick 组件/自动供电设备；本行以 designation 和
  实际 switch 读回为准。
