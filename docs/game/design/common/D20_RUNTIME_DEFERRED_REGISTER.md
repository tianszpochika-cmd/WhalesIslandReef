# D20 Runtime Deferred Register

> D20-L39；本表把纯逻辑通过与后续 Unity/实机工作明确分开。

| ID | 延期事项 | 当前逻辑证据 | 后续验证 |
|---|---|---|---|
| RT-01 | 玩家/NPC/动物真实模型、材质、骨骼、Avatar | 只验证稳定 content_id 和资源 key | Unity 导入、Prefab 检查、角色镜头 |
| RT-02 | Animator、动作混合、表情、发型/服装挂点 | 只输出 action/presentation read model | Animator Controller 与状态切换实测 |
| RT-03 | 场景、Prefab、碰撞体、锚点和 NavMesh | 纯逻辑不引用 Transform/Prefab | Editor 场景烘焙、碰撞和寻路测试 |
| RT-04 | 键鼠、手柄、Input System 与交互射线 | 只接收不可变 command DTO | Player 中逐键验证、焦点与重复输入测试 |
| RT-05 | UI/HUD、面板互斥、按钮视觉反馈 | 只提供只读 projection/失败码 | Canvas/EventSystem、响应式与点击回归 |
| RT-06 | NPC/动物真实移动、群体行为和宠物跟随 | 只验证确定性状态与资格 | Player 中运行路线、障碍、动画、拥挤测试 |
| RT-07 | D19 港口真实登船、靠泊和出航表现 | 只调用 D19 公开路由 | 场景路线、物理、触发器和镜头验证 |
| RT-08 | 真实 Save 介质、崩溃恢复、云同步 | 只验证内存 DTO 往返与 owner reference | Unity SaveSystem、断电/重载和平台验证 |
| RT-09 | 画质、光照、天气特效、水体、粒子、音效 | 逻辑层不产生视觉证据 | URP/HDRP 配置、天气变体和截图验收 |
| RT-10 | 性能、GC、内存、加载、发行包 | 离线逻辑测试不代表 Player 性能 | 1920×1080 Player、Profiler、打包回归 |

## 当前阶段结论

- `LOGIC_PASS` 只表示代码、契约、确定性算法和离线测试满足门禁。
- `RuntimeApproved=0`、`ReleaseApproved=0`；不得从本表删除延期项来制造放行。
- D21-L06 的 crew projection 依赖必须由 D20/D21 公开接口真实闭环；本表不能替代该依赖。
