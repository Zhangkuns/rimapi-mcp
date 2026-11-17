# 原生UI选择补齐：账单、过滤、床、设施、书架与蓝图

维护：Codex；核对：2026-11-16；状态：本轮实现、限定隔离验收与自审完成；正式DLL未部署。

对应[UI调查](../references/reviews/2026-11-16-ui-api-parity.md)的UI-01–13；环境与UI-14见[原生偏好/输入/显示](game-preferences.md)。主线程执行，正常资格与准确对象由服务检查；以下新POST支持dry和Game内Idempotency-Key，成功/operation只表示配置应用或指定动作提交，不能冒称后续工作完成。

## 账单（UI-01–03）

`GET /api/v1/buildings/bill/options?building_id=...&bill_id=...` 返回当前bill、`worker_restrictions`、原生可选人物/禁用原因标签、`count_sources`的精确kind/id/名称与是否可存本配方产品，以及classic可选产品风格和`graphic_indices`。`any`对mechanitor-only配方仍表示任意合法mechanitor，不绕过原生限制。

`POST /api/v1/buildings/bill/configure`：

```json
{"building_id":123,"bill_id":4,"worker_restriction":"any",
 "count_source":{"kind":"all"},"dry_run":true}
```

- worker模式为`any/pawn/slave/mech/non_mech`，仅接受options中可用项。`pawn`须提供`pawn_restriction_id`；其它模式不能同时给人物ID。原生SetPawnRestriction/SetAny*负责清除三个互斥标志；any可清除既有指定人物。
- `count_source.kind`为`zone/building/group/all`；前三者必须有各自稳定数值id，all不得带id。来源必须在该bill的地图上并符合原生产品存放资格。分组成员按原生UI选择命名组，未分组zone/building可直接选。
- `product_style`为`{"mode":"global"}`、`{"mode":"basic"}`或`{"mode":"specific","style_def":"...","graphic_index":0}`。仅classic且产品有原生风格选项时可编辑；Graphic_Random须选一个options列出的index，其它风格不接收index。global/basic不带style_def/index。
- 全部新选择先预检，失败400/404不写bill；dry返回当前`bill`与`proposed`，不创建新bill或消耗ID。正式返回同一bill的实际读回。
- 旧create/update也接受这些选择字段，并保留原响应形状；它们维持原有非幂等合同，统一在异步读body后回主线程。旧`include_group_id`仅指命名组，与新`count_source`互斥。要使用dry/重放，使用configure。
- 既有GET bill新增`worker_restriction`、精确`count_source`和`product_style`。不以显示名替代来源实例ID。

## 过滤与取消（UI-04–07、13）

- `POST /reading/policy/filter`新增`min_quality/max_quality`（QualityCategory）及`min_mental_break_chance/max_mental_break_chance`（0..1，Anomaly）。分别写ReadingPolicy的defFilter和effectFilter；GET policy与check均返回范围。全部字段先校验，dry不写，非法范围不会连带改变书种/效果勾选。
- `POST /pen/policy`新增`animal_special_filters:{"AllowMaleGender":true,"AllowFemaleGender":false}`，对应原生同种动物雌雄接纳。GET同名字段保留原生DefName大小写；其它特殊名400。自动屠宰的性别阈值仍是独立策略。
- `POST /building/facility`新增`allowed_auto_cut_plants`完整替换（空数组清空），或成对`auto_cut_plant_def/auto_cut_plant_allowed`增删；合法植物由GET的`auto_cut_valid_plants`发现。现允许项完整排序、不再Take(50)。同请求先改过滤再执行`auto_cut_now`。
- `GET/POST /storage/settings`原身份保持；除建筑自身接口外，解析其IStoreSettingsParent组件，并对炮塔取内部gun的CompChangeableProjectile。GET新增`settings_owner_type`；塑形舱Nutrition与炮塔完整过滤可用同一设置合同，现有allow_shell保留。
- `POST /animal/settings`新增`clear_master:true`；与非空master_id或attack_target_id同时给400。null/省略master_id仍表示不修改；clear仅把原生Master设null，不改跟随/训练。

## 床、补舱、蓝图与书架（UI-08–11）

- `POST /building/bed`新增`owner_type:Colonist|Prisoner|Slave`，与for_prisoners互斥；medical仍独立。按原生房间规则联动同房间囚犯床、清原生ownership并通知District/Room；不依赖当前UI选中，也不打开窗口。若其它床有人类主人会丢失指派，返回409和`affected_beds/requires_confirmation`，调用方显式confirm:true；dry列出影响但不写。
- `POST /building/facility`新增`auto_place_pods`；仅玩家发射器，改变时调用原生ToggleAutoBuildTransportPods，启用会立即按原生资格检查补蓝图，后续发射由原生Notify继续。关闭不自动取消已存在蓝图。
- `GET /dlc/ideology/style?thing_id=`识别Blueprint_Build的entityDefToBuild并返回其合法风格、`blueprint_style_editable/style_overridden`。POST可给合法style_def或`basic:true`二选一，仍需既有confirm；仅玩家classic合法蓝图，写原生StyleDef/styleOverridden并刷新该网格。成品旧行为保留。
- `GET /building/books?thing_id=&map_id=`完整列出玩家书架内实际书ID、holder、数量、品质及风险。`POST /building/books/eject`接收thing_id/book_id，可选map_id与dry；原生TryDrop准确一本后设Forbidden=true，其余书留在架内。未知书或不属于该书架404，真实失败不返回成功；同key重放不重复弹出。

## 工作模式（UI-12）

`POST /pawn/work-queue`的`action:set_manual_priorities`改变值时，通知全部玩家人物的原生workSettings缓存失效；同值与dry不触发额外写。真实排序由原生getter重建，不能仅根据bool读回判断。

## 验证

统一fixture只准备对象和独立观察；真实API配置、产物风格、蓝图建成、过滤牵引与环境持久化按[实施计划](../exec-plans/completed/2026-11-16-ui-api-repair.md)验证。本轮已按[限定验收报告](../references/reviews/2026-11-16-ui-api-repair.md)核销，保留准确DLL、前置、原始失败、真实终态、清理和未测范围。
