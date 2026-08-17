# D19 港口领域运行时延期登记表（Runtime Deferred Register）

> 归属：`docs/game/design/common/D19_RUNTIME_DEFERRED_REGISTER.md`（D19-L46 唯一延期登记产物）  
> 目的：建立 D19 面向未来 Unity 实机运行（D20—D24）的唯一待测与延期清单，明确逻辑已通过 vs 实机待测边界。

---

## 一、待测项登记矩阵

| 序号 | 延期事项 | 依赖的纯逻辑接口 | 核心风险 | 未来验收方式 (D20-D24) | 当前阶段禁止宣称通过的原因 |
|---|---|---|---|---|---|
| **RT-01** | **场景锚点真实 3D 坐标** | `D19PortAnchorCatalog` (L04) | 空间坐标漂移、锚点重叠 | D20 场景烘焙与 Gizmo 坐标目测 | 纯逻辑仅核验键名规范化，无 Transform 数据 |
| **RT-02** | **船只靠泊物理对齐与碰撞** | `D19DockingRules` (L07) / `D19BerthSystem` (L06) | 船体穿模、刚体飞天、物理卡死 | D21 Rigidbody/Collider 靠泊实机实测 | 纯逻辑仅裁决占用状态与尺寸档位匹配 |
| **RT-03** | **船员登船触发器与空间交互** | `D19BoardingWorkflow` (L08) | 交互判定距离过小、登船丢失交互框 | D21 Proximity Trigger 与交互射线测试 | 纯逻辑仅校验成员 `OnShore`/`OnBoard` 字典 |
| **RT-04** | **NPC 3D 视觉与 NavMesh 寻路** | `D19PortAvailabilitySystem` (L05) | NPC 卡在建筑碰撞体、动画不播放 | D23 NavMeshAgent 巡逻与行为树运行 | 纯逻辑仅提供 NPC 逻辑可见性与状态 |
| **RT-05** | **玩家输入映射与快捷键** | `D19PortCommandRouter` (L33) | 快捷键冲突、按键无响应 | D21 Input System 动作映射与手柄测试 | 纯逻辑仅接收不可变命令 DTO |
| **RT-06** | **UI/HUD 视图与面板互斥** | `D19PortReadModel` (L19) / 状态机 (L03) | 交易面板与酒馆面板同时弹窗穿透 | D22 UGUI Canvas 管理器与 HUD 焦点测试 | 纯逻辑仅输出只读读模型与状态指纹 |
| **RT-07** | **港口 3D 模型与 Prefab 资产** | `D19PortAnchorCatalog` (L04) | 材质丢失（洋红色）、Prefab 变体损坏 | D20 资源完整性扫描与打包验证 | 纯逻辑阶段严格零 Prefab/材质资产写入 |
| **RT-08** | **关卡光照、HDRI 与画面渲染** | 场景设计规范 | 曝光过度、阴影撕裂、性能掉帧 | D20 渲染管线（URP）实机 Profile | 纯逻辑阶段严禁生成任何截图或光照数据 |
| **RT-09** | **实机加载耗时与内存占用** | `D19PortSaveProjection` (L20) | 资产异步加载卡顿、GC Spike | D24 完整打包 Release 包实机 Profiler 压测 | 纯逻辑阶段仅验证内存 DTO 序列化往返 |

---

## 二、纯逻辑已完成 vs 实机待确认对照

1. **纯逻辑已完成（100% 闭环且不可推翻）**：
   - 港口 8 态生命周期状态机迁移表驱动；
   - 泊位占用唯一性、尺寸吃水匹配与租期过期判定；
   - 市场/仓库/酒馆/船坞适配器权限与业务规则校验；
   - 命令幂等重放控制与逆序失败补偿策略；
   - 事件游标单调递增、唯一 ID 与因果关系审计；
   - 存档 SchemaVersion=1 序列化深拷贝与历史旧档安全迁移。
2. **实机待确认（严禁在此阶段伪装通过）**：
   - 一切与 Unity 引擎生命周期（`Awake`、`Start`、`Update`、`FixedUpdate`）相关的表现；
   - 一切与图形渲染（MeshRenderer、Shader、Lightmap、ParticleSystem）相关的画面；
   - 一切物理模拟与输入设备读取。

---

## 三、登记结论

延期清单完整明确，各维度边界清晰，无混淆逻辑通过与实机可玩的表述。结论为 **`PASS`**。
