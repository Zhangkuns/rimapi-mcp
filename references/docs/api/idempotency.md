# 通用幂等第一阶段

已实现，覆盖31条fork写入口。可选HTTP头`Idempotency-Key`：1–128个可见ASCII字符，不含空白。
现有无头调用继续执行原业务合同；解析失败/缺失JSON body统一400，无Game409。

## 覆盖

- AiControlController：8条，designation、Hediff、精神状态/灵感、命令。
- Phase2Controller：11条，区域、条件、穿戴、医疗排单。
- QuestControlController：2条，任务生成/删除。
- POST `/api/v1/pawn/outfit`：1条。
- Biotech：基因增/删、异种3条；机械绑定/模式2条。
- Royalty：能力授予/移除、施法3条；资源空投许可执行1条。

能力合同`idempotency:game_memory`才代表实际接入；交易execute为`quote`，沿用quote_id/request_id，
不叠加此缓存。GET、交易quote和旧上游写接口未接入；显式合同无幂等为none，旧heuristic未声明字段为空/省略。

## 调用与返回

同一key在同进程/当前Game内全局共享，指纹包括method、规范匹配路径和解析后DTO的规范JSON。
对象属性按ordinal排序、数组保序，SHA256；路径大小写及尾斜杠别名映射回同一合同常量。
未知JSON成员仍按现有解析器忽略，不参与DTO指纹；查询串不参与本阶段写接口指纹。

- 首次真实操作先预留key，再执行业务；先冻结完整响应与HTTP码，再发operation，再尝试HTTP发送。
- 同key同指纹返回首次HTTP状态码和完整原正文（含timestamp），附`Idempotency-Replayed:true`，不重复执行/发事件。
- 同key不同method/path/DTO返回409；reserved返回409；首次业务失败也缓存并重放。
- 使用 `AiActionResults` 的写入口在执行后按真实观察重建 `applied`：
  达成 true、已知无效果 false、未知/部分效果省略；预览 false 不沿用到未知执行结果。
  失败响应仍冻结在同 key 下，不因之后恢复正常而重试执行。
- 业务抛异常返回500 internal_error，data.uncertain:true；结果仍锁定此key，不自动回滚或盲目换key重试。
- 鉴权在读取缓存前；没有/错误token仍401，不能借重放读取结果。
- body dry_run:true及Quest未confirm预览绕过执行缓存；每次仍运行预览。
  Quest预览可能有对象/RNG内部副作用，此缓存不承诺预览幂等。

## 生命周期

容量1024，从首次reserved起保留30分钟，有效结果不提前淘汰；容量满且无过期项时409且不执行业务。
API监听器重启在同进程/同Game下保留key；主菜单、读档、新Game和进程重启清空。
超过30分钟或缓存失效后，旧key可能作为新请求再次执行。客户端必须先读回状态再决定动作，不能盲目重试旧key。
这不是持久exactly-once，也不保护所有上游接口。G-05因此仍为部分完成。

[验收与证据](../references/reviews/2026-11-04-core-closeout-acceptance.md)：
容量/过期/uncertain/监听器重建用确定性单测；游戏内验证创建只一次、冲突、冻结正文、SSE、鉴权、客户端reset、读档失效。
客户端reset发生在独立读回业务完成之后，不能证明服务器恰好处于发送失败时点；该异常路径另由纯helper测试覆盖。
