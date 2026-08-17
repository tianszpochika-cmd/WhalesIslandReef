# D19 港口领域逻辑接口文档（供 D20—D23 消费）

> 归属：`docs/game/design/common/D19_LOGIC_PORT_INTERFACE.md`（D19-L42 唯一公开接口文档）  
> 性质：只读契约与公开 API 规范。后续章节（D20 港口场景/资源、D21 港口交互/表现、D22 UI/HUD、D23 NPC/社会模拟）**只能消费本接口，严禁修改 D19 内部领域实现或私自写改 D19 拥有的状态**。

---

## 一、核心设计铁律与消费约束

1. **唯一写者与数据所有权**：
   - 访问生命周期（`D19PortVisitState`）仅由 `D19PortVisitSystem` 裁决；
   - 泊位占用与租期仅由 `D19BerthSystem` 裁决；
   - 港口服务可用性由 `D19PortAvailabilitySystem` 裁决；
   - 外部业务数据（金钱、商品库存、船舶结构改装、船员属性）仍由 D3/D4/D9 拥有，D19 适配器仅作命令校验与请求透传，绝无直接改写路径。
2. **唯一命令入口**：
   - D20—D23 所有港口业务请求必须封装为 `D19RoutedPortCommand`，统一通过 `D19PortCommandRouter.Route(cmd)` 执行；
   - 严禁绕过路由器直接调用底层领域系统。
3. **确定性与无 Unity 依赖**：
   - D19 领域层为纯 C# 实现，不引用 `UnityEngine`、`Transform`、`GameObject`、`Canvas`、`NavMesh` 或真实系统时钟；
   - 所有时间推进以 `worldDay` 传入，所有操作满足幂等律。

---

## 二、公开数据模型与命令接口

### 1. 唯一路由命令载体
```csharp
// 命名空间：Whalebone.Systems.D19
public sealed class D19RoutedPortCommand
{
    public string CommandId;         // 稳定命令 ID（跨重试不变）
    public D19PortCommandKind Kind;  // 命令种类（EnterPort / Dock / AssignBerth / StartService / CompleteService / Board / Depart / Return / Compensate）
    public string ActorId;           // 请求者 ID
    public string ShipId;            // 目标船只 ID
    public string PortId;            // 港口锚点键（如 "port_santa_margarita"）
    public string BerthId;           // 泊位锚点键（如 "berth_santa_margarita_dock_01"）
    public string ServiceId;         // 服务锚点键（如 "service_santa_margarita_market"）
    public int WorldDay;             // 当前世界日
    public long SaveRevision;        // 存档修订号
    public D19AuthorityClaims Claims;// 授权声明（L23 权限校验）
    public Dictionary<string, string> Parameters; // 业务附加参数
}
```

### 2. 统一执行结果
```csharp
public sealed class D19RoutedCommandResult
{
    public bool Success;             // 是否成功
    public bool IsDuplicate;         // 是否为幂等重放
    public string CommandId;         // 命令 ID
    public D19PortCommandKind Kind;  // 命令种类
    public string ReasonCode;        // 稳定原因 ID（失败时非空，如 "d19.berth.occupied"）
    public string LocKey;            // 本地化键（如 "d19.failure.berth_occupied"）
    public D19FailureRetryPolicy RetryPolicy; // 重试建议（NoRetry / RetrySafe / RetryAfterCorrection）
    public D19PortVisitState StateAfter;     // 执行后生命周期状态
    public WorldEvent EventDraft;            // 生成的 L16 领域事件草案（可为 null）
    public D19CompensationDraftSaveEntry CompensationDraft; // 失败时的补偿草案（供 L34 消费）
}
```

### 3. 生命周期状态枚举 (`D19PortVisitState`)
- `None` (0): 未进入港口访问；
- `PortVisit` (1): 已到港，进入港口范围；
- `Docked` (2): 船体已靠码头，泊位尚未分配；
- `Berthed` (3): 泊位分配成立，持有效 `BerthId`；
- `InService` (4): 港口服务进行中（市场/仓库/酒馆/船坞）；
- `Boarded` (5): 船员登船就绪，允许启动离港；
- `Departed` (6): 离港出航；
- `Returned` (7): 航程结束返港。

---

## 三、只读读模型与持久化契约

### 1. 只读读模型 (`D19PortReadModel`)
D21/D22 UI/HUD 渲染只需订阅只读读模型：
- `ReadState(shipId)`: 获取当前 `D19PortVisitState`；
- `ReadPortId(shipId)` / `ReadBerthId(shipId)`: 获取当前停靠港与泊位；
- `ReadFingerprint(shipId)`: 获取状态快照哈希，用于局部刷新判断。

### 2. 存档持久化与迁移 (`D19PortSaveProjection`)
- 投影版本：`SchemaVersion = 1`；
- 包含字段：访问状态、泊位分配表、成员逻辑位置、挂起命令、补偿草案、幂等记录与事件游标；
- 兼容迁移：由 `D19PortSaveMigration.Migrate` 自动提供合法缺失默认值（`CreateDefault` 语义）与旧别名规范化，不可能状态安全拒绝，绝不伪造泊位或数据。

---

## 四、Runtime Deferred 清单（留给 D20—D24 实现）

以下内容明确不在 D19 领域逻辑责任内，属于运行时延迟交付项（Runtime Deferred）：
1. **世界坐标与物理 Transform**：泊位、码头、建筑的 3D 坐标与空间位置；
2. **物理碰撞与 NavMesh 寻路**：船只靠泊物理对齐、船员岸上寻路网格；
3. **输入系统与快捷键**：键盘/手柄/鼠标交互输入映射；
4. **UI/HUD 视觉组件**：Canvas、UGUI 面板、字号、图标、材质表现；
5. **NPC 视觉与动画表现**：NPC 3D 模型、骨骼动画、行为树驱动；
6. **场景与关卡**：Unity Scene 资源、Prefab 打包与 Build Settings 配置。

---

## 五、测试与审计证据链接

- 路由器与编排测试：`game/Assets/Tests/EditMode/D19/D19DeterministicPortScenarioTests.cs` (D19-L36, PASS)
- 模糊不变式测试：`game/Assets/Tests/EditMode/D19/D19PortFuzzInvariantTests.cs` (D19-L37, PASS)
- 保存加载往返测试：`game/Assets/Tests/EditMode/D19/D19PortSaveLoadTests.cs` (D19-L38, PASS)
- 历史旧档迁移测试：`game/Assets/Tests/EditMode/D19/D19LegacySaveTests.cs` (D19-L39, PASS)
- 逻辑边界静态审计：`tools/d19_logic_boundary_audit.py` (D19-L40, PASS)
- 统一测试运行器：`tools/d19_logic_test_runner.py` (D19-L41, PASS)
