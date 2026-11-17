# 原生偏好、音画、语言、键位与显示底栏

维护：Codex；核对：2026-11-16；状态：本轮实现、限定隔离验收与自审完成；正式DLL未部署。

覆盖UI-14及UI-E01–03；这些环境/显示能力由本轮用户明确纳入修复范围，不把它们充当DLC玩法通过。服务使用本机原生Prefs/KeyPrefs/PlaySettings与明确的显示/语言方法，不向客户端开放任意属性/反射/菜单执行。

## 偏好发现与设置

`GET /api/v1/game/preferences`返回：

- `values`：原生偏好当前值，含准确float `autosave_interval_days`、int `autosaves_count`，不会把天数当bool。
- `schema`：每个白名单key的原生property、类型、上下限/step、枚举或数值choices、是否需要重启。
- `screen`：实际width/height/fullscreen、ui_scale、保存的尺寸与borderless状态；实际显示提交后可在后续帧读回。
- 当前/已保存语言、可用languages（folder_name）、语言是否可编辑、原生resolutions和全屏可编辑性。
- 原生语言重载等LongEvent期间返回`loading:true`及当前/保存语言，待loading:false再依赖完整设置/定义列表。

`POST /api/v1/game/preferences`：

```json
{"values":{"volume_master":0.4,"volume_ui":0.3,"autosave_interval_days":3,
           "autosaves_count":7,"pause_on_load":true,"automatic_pause_mode":"Never"},
 "dry_run":true}
```

实际枚举值先GET schema发现；上例字符串仅用于说明字段形状。

白名单包括五通道音量、存档/暂停、1..5聚落上限、最多6个唯一preferred_names、后台运行、纹理压缩/植物摇摆/震动/镜头过渡、UI缩放/温度单位/名字/武器/高亮/时钟/帽子/细小文字/光标/心情、学习辅助、鼠标拖拽/边缘滚动/缩放、地图层缩放、样式记忆、SteamDeck键盘模式、资源分类和菜单背景。精确键与合法值以GET为准，未知键/类型/范围400，所有预检在写之前。固定背景选installed expansion def，并自动关闭随机背景；冲突请求拒绝。

`texture_compression`修改会返回`restart_required:true`，保存原生偏好但不会自动重启进程；其它实际原生属性setter负责应用，帽子/文字/缩放另做UI相同缓存通知。

### 显示与语言

```json
{"resolution":{"width":1920,"height":1080,"fullscreen":true},
 "values":{"ui_scale":1.25},"confirm_display":true}
```

- 只允许原生报告的分辨率或当前尺寸；UI缩放必须为原生选项且满足原生UIScaleSafeWithResolution。实际改变尺寸/全屏/缩放须confirm_display:true，代表接受原生确认；不遗留Dialog_ResolutionConfirm或计时回滚。
- 使用ResolutionUtility.SetResolutionRaw和原生Prefs/缓存通知；返回`display_change_submitted`与当前screen，不把同帧旧几何伪报成新尺寸，后续GET核实际帧结果。borderless模式不允许原生隐藏的全屏切换。
- `language:"English"`等使用GET返回的folder_name，只允许主菜单、LongEvent空闲时调用；Playing返回409。调用原生LanguageDatabase.SelectLanguage后返回`language_change_submitted`，轮询loading及实际language完成。
- `reset_learning_helper:true`需confirm:true，调用原生ResetPersistent；dry不重置。

这些POST可在主菜单执行，使用主线程协调器的**process_memory**幂等缓存（同进程跨菜单/Game，30分钟/1024项；重放返回当次冻结结果，不是当前快照）和operation事件；进程结束不持久缓存。首次失败或dry不冒称应用，未知结果先GET读回。

旧`GET /game/settings`保留原`autosave_interval`bool兼容，新增准确`autosave_interval_days/autosaves_count/automatic_pause_mode/volume_ui`；完整设置发现/修改使用本页入口。

## 快捷键

`GET /game/key-bindings`：每个DefName的A/B当前绑定、默认值、类别及可用Unity KeyCode。

```json
{"bindings":[{"def_name":"ToggleBeautyDisplay","slot":"A","key_code":"F11"},
             {"def_name":"ToggleBeautyDisplay","slot":"B","key_code":"None"}],"dry_run":true}
```

`POST /game/key-bindings`先克隆原生KeyPrefsData并预检全部改动；slot只能A/B，None清除，`reset:true`恢复该slot默认（不同时给key_code），`reset_all:true`原生重置全部再应用列表。实际按原生EraseConflictingBindingsForKeyCode/SetBinding规则处理冲突，返回完整before/实际bindings/proposed，使被清除的其它键位可见；提交KeyPrefsData并Save，不需要打开键位窗口。失败/preview不改当前数据或文件。同进程幂等语义同Prefs。

## 原生显示底栏

`GET/POST /game/play-settings`对应当前Game的17个显示bool：地图学习提示/区域/美观/房间/殖民者栏/屋顶/肥沃度/地面承载/污染/真空/温度，世界北向锁定/昼夜显示/重要地点/基地/地标/地理标记。

```json
{"values":{"show_zones":true,"show_beauty":false,"lock_north_up":true},"dry_run":true}
```

精确key由GET的available_flags发现，不适用项另列unavailable_flags；未知键400，缺Game409。真空层仅Odyssey且当前地图inVacuum时可开启，污染/地标按其DLC资格；不适用的开启请求422且整批不写，显式false可清除。原生UI会在地面地图重置真空层，因此接口不会返回一个下一帧必被覆盖的开启成功。直接设置PlaySettings并随原生Game存档，与RIMAPI自绘overlay分开。POST采用Game内幂等，换Game/菜单会失效，不与Prefs的process_memory混淆。

## 验证范围

按[实施计划](../exec-plans/completed/2026-11-16-ui-api-repair.md)在独立Xvfb/profile验证原生字段、键位真实输入、尺寸/缩放、保存重载与主菜单语言。只修改副本配置，不操作用户真实桌面或覆盖全局Prefs/KeyPrefs；完成后报告实际设备/显示模式及未测变体。
