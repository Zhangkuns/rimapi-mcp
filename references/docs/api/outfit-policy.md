# 服装政策读取与分配

已实现，1.6验收见 [Core收尾验收](../references/reviews/2026-11-04-core-closeout-acceptance.md)。
以下路径均以`/api/v1`为前缀。

`GET /pawn/outfit?pawn_id=321`返回`{pawn_id,outfit_id,label}`。
政策目录仍为现有`GET /outfits`，使用其中的整数id，不按label匹配。

`POST /pawn/outfit`：

```json
{"pawn_id":"321","outfit_id":2,"dry_run":true}
```

成功数据为`{pawn_id,previous_outfit_id,outfit_id,label,changed,dry_run}`。
dry-run只报告预计变化；真实赋值走公开`Pawn_OutfitTracker.CurrentApparelPolicy`。
同政策重设`changed:false`，不发operation。真实变化先发operation再尝试HTTP回应。
可选`Idempotency-Key`遵循 [通用幂等合同](idempotency.md)。

无Game409；缺字段/非正outfit_id400；未知pawn/政策404；
未生成、非自由殖民者或无tracker422 invalid_target。
这里只改政策；后续换装由游戏AI进行，不等于立即Wear/Remove，也不手工清理forcedHandler。
隔离局实测分配前后已穿衣物/forced列表相同，读档后政策保留；不外推所有Mod副作用。
恢复旧政策可重新POST旧id，不代表回滚期间已经发生的AI换装。
