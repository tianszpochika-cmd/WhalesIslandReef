# D20-L40 交接

- 状态：`LOGIC_PASS`。
- D20-L01—L35 结果全部通过；L36 审计、L37 统一 runner、L38 接口文档、L39 延期登记全部完成。
- D20-L40 已补齐船员公共只读投影：D10/Crew owner 事实经严格 schema/ID/范围/重复校验后，发布可 Clone、确定性排序和 fingerprint 的 `D20CrewRosterProjection`；D20 不写回 owner，也不解释岗位语义。
- D21 已按公开接口真实重跑：D21-L06 岗位/士气/疲劳/健康规则和 D21-L24 Start 聚合均为 `LOGIC_PASS`；未知岗位、缺失/非法投影、船只/修订不匹配均保持 fail-closed。D21-L27 补给性能文件未在本次修改。
- 本次新增/更新证据：D20 crew projection 14/14、D21 crew projection integration 16/16，联合 D21 离线 runner 退出码 0 且 failure=0。
- 仍然禁止把本验收解释为 Unity Player、画面、性能、真机或发行通过。
