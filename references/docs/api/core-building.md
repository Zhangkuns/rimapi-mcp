# Core 建造资格、取消退款与安装/重装（C-18）

2026-11-13 新增。资格检查等价原生 `Designator_Build` 可见性门 + 
`GenConstruct.CanPlaceBlueprintAt`；取消只对玩家 Blueprint/Frame 调原生
`Destroy(DestroyMode.Cancel)`；安装/重装用原生 `PlaceBlueprintForInstall/
Reinstall`，不 debug paste。

## 路由

1. `GET /api/v1/builder/qualification?x=&z=&def_name=&stuff_def=&rotation=`
   - research/techLevel/AllowedToBuild/PlaceWorker/buildingPrerequisites/
     discovery/gravEngine/monolith + `CanPlaceBlueprintAt`；返回 accepted/
     reason/native_reason/cost/stuff/rotation/cell。
2. `POST /api/v1/builder/cancel` `{thing_ids:[...],dry_run}`
   - 全量预检玩家 Blueprint/Frame，任一非法 4xx 不部分写；`Destroy(Cancel)`；
     返回 cancelled/refunded 读回。Frame 是原生未完工施工节点；无独立
     unfinished Building 类。
3. `POST /api/v1/builder/install` `{thing_id,x,z,rotation,dry_run}`
   - MinifiedThing（含 MinifiedTree）或 Minifiable Building；原生
     `CanPlaceBlueprintAt(..., thingToIgnore=source, thing=inner)` +
     `PlaceBlueprintForInstall`/`Reinstall`；返回 blueprint/目标/旋转。

## 证据与边界（run c18-20261113-run5）

- qualification 清地 accepted；越界 422；未知 def 404。
- cancel dry 不写；blueprint/frame 均 cancelled=true；Frame 预置 5 WoodLog
  后取消，refunded 读回 `{"wood_log":5}`。Blueprint 尚无已交付材料，原生
  `Blueprint.Destroy(Cancel)` 的 leavings 不产生材料（源码：`GenLeaving.
  CanBuildingLeaveResources` 仅 Building；Blueprint 无 resourceContainer），
  因此其 refunded 为空是原生语义，不是漏退料。
- minified bed install applied，正常施工完成 `install_bed_built=true`
  （WoodLog 300→375 为施工/退款混合读回）；minified tree install applied
  （仅蓝图）；reinstall bed 在独立清地目标 applied（Blueprint_Install_Bed）。
- 4xx：qualification 越界 422/未知 404；cancel 混合非法 404 不部分写。
- 未测：tree 重栽真实施工完成、reinstall 真实施工/搬运、Frame 多资源部分
  退款比例。fixture 只布置/披露，真实施工由 work giver。
