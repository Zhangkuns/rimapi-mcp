# Ideology 造型站：造型、染发与染衣（ID-08c）

覆盖原版造型站的正常外观变更：发型、胡须、面部/身体纹身、染发及单件整衣染色。造型接口写入
`Pawn_StyleTracker.next*`（与原版 `Dialog_StylingStation` 相同），再提交原生
`UseStylingStation`；染发由原生 `DyeHair` job 消耗 1 个染料。它不直接改
`pawn.story.hairDef`/`HairColor`。染衣另走下述原生 `RecolorApparel`。

## 读取

`GET /api/v1/dlc/ideology/styling?pawn_id=321`

```json
{
  "pawn_id": "Human321",
  "ideo": "Boccin Creed",
  "gender": "Male",
  "hair": {"def_name": "Artist", "label": "Artist"},
  "beard": {"def_name": "NoBeard", "label": "No beard"},
  "face_tattoo": {"def_name": "NoTattoo_Face", "label": "None"},
  "body_tattoo": {"def_name": "NoTattoo_Body", "label": "None"},
  "hair_color": "#52351D",
  "skin_color": "#F2D7B8",
  "next": {"hair": null, "beard": null, "face_tattoo": null,
           "body_tattoo": null, "hair_color": null},
  "station": {"thing_id": "StylingStation88473", "thing_id_number": 88473,
              "x": 157, "z": 75, "forbidden": false, "reachable": true,
              "reservable": true, "interaction_cell_standable": true},
  "dye_count": 0,
  "hair_options": [{"def_name": "Lackland", "label": "Lackland"}],
  "beard_options": [],
  "face_tattoo_options": [],
  "body_tattoo_options": []
}
```

- `hair_options`/`beard_options`/纹身选项按原生 `PawnStyleItemChooser.WantsToUseStyle`
  过滤（性别/年龄/基因/ideology 风格频率/胡须能力），不是全部 Def。
- `dye_count` 是**存放中**染料数量（`Map.resourceCounter`，与原版造型站提示一致）；
  地面散落染料可被染色 job 使用，但不计入该字段。
- `next.*` 是尚未被造型 job 应用的待变更值；染发完成后 `next.hair_color` 可能仍保留
  与当前色相同的值（原生 `FinalizeHairColor` 不清空），颜色以 `hair_color` 为准。

## 造型/染发

`POST /api/v1/dlc/ideology/styling`

```json
{"pawn_id": "321", "hair_def": "Lackland", "hair_color": "#3373D9", "dry_run": false}
```

- 至少给 `hair_def`/`beard_def`/`face_tattoo_def`/`body_tattoo_def`/`hair_color` 之一。
- 定义必须存在且通过原生 `WantsToUseStyle`；纹身必须分别匹配 Face/Body 类型。
  `hair_color` 为 `#RRGGBB`。
- 未指定 `station_id` 时选同图最近、可达、可预留的造型站；有变更却没有造型站 409。
- 只有颜色变更时需要同图可达染料；没有染料 409，且不写入任何待变更字段。
- 写入与原版确认按钮相同的五个 `next*` 值，提交 `UseStylingStation` job；
  如同时改色，再把原生 `DyeHair` job（目标=造型站+染料，count=1）排在其后。
  这样即使人物处于原版 idle giver 不会进入的区域/事务，也会执行同一原生染色 job。
- `dry_run:true` 只回显 `before`/`requested`/`station`；`changed:false` 时不提交 job。
- 成功 `applied:true` 表示 job 已提交，**不是外观已完成**；实际外观在 job 结束
  （发型 300 tick，染发再 300 tick）后才变。响应 `after.pending:true` 明确这一点。

## 正常调用顺序

1. `GET .../styling` 确认当前发型/颜色、可用选项、造型站与染料。
2. `POST .../styling` 设 `hair_def`/`beard_def`/纹身和/或 `hair_color`。
3. 等待原生 `UseStylingStation` 完成（`FinalizeLookChange` 写入发型/胡须/纹身），
   再等待 `DyeHair` 完成（消耗 1 Dye，`FinalizeHairColor` 写入颜色）。
4. 重新 `GET .../styling` 核对 `hair`/`beard`/`hair_color` 与 `dye_count`。

## 单件整衣染色（已实现）

同一 `POST /api/v1/dlc/ideology/styling`：

```json
{"pawn_id":"321","apparel_id":"Apparel_Pants...","apparel_color":"#D94C40","dry_run":false}
```

- `apparel_id` 必须是该人物当前穿着、有CompColorable的准确ThingID；`apparel_color`是 `#RRGGBB`。
- 与发型/胡须/纹身/发色字段互斥；不存在的衣物404，缺少字段/非法颜色400。
- 设置 `Apparel.DesiredColor`，复用 `JobGiver_OptimizeApparel.TryCreateRecolorJob` 提交原生 `RecolorApparel`；
  没有可用造型站/染料409，恢复原desired值。dry-run不提交job并恢复desired值。
- 响应 `applied/job_submitted` 是提交结果，`color_target`不是完成证明；等待job后从衣物实际颜色读回，
  并核对准确染料消耗。styling GET仍是人物造型摘要，不额外声称返回了衣物颜色清单。
- 2026-11-11限定实测：一件worn colorable pants实际变为 `#D94C40`，染料7→6，404/400负例；
  准备造型站/衣物/染料前置见[自审](../references/reviews/2026-11-10-id08c-styling.md)。

## 边界

- 原生造型站肤色/体型/头型变更受 `devEditMode` 限定，不是正常玩法缺口；不把未知面部配件扩成正常玩家操作。
- 理念级外观frequency/gender与人物造型不同；尚缺的正常配置见[ID-R05](../product-specs/five-dlc-remaining-checklist.md)。
- 不做 `Notify_StyleItemChanged` 之外的直接 `story` 写入；外观最终由原生 job 写。
- 染发/单衣已分别验消耗1染料；批量染衣、其它apparel与造型站供电/研究门槛未在这些样本实测。
- `PawnStyleItemChooser` 的可选集合会随基因/年龄/ideology 变化，调用方必须以
  `hair_options` 等返回为准，不能假设全 Def 可用。

2026-11-10 实测：`Human321` Artist/`#52351D` → Lackland/`#3373D9`，原生
`UseStylingStation` 与排队的 `DyeHair` 完成，地面染料 2→1
（`Reverse/evidence/id08c-styling-20261111.json`，自审
[ID-08c](../references/reviews/2026-11-10-id08c-styling.md)）。
