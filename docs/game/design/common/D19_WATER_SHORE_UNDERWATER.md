# D19-A04 水体、岸线、浅水、暗礁与水下表现

- 任务：`D19-A04`
- 日期：`2026-08-14`
- 结论：**PARTIAL（隔离 URP 水体模块验证通过；主工程整体编译被其他 D19 任务的既有缺失类型阻断）**
- 工作范围：只写 D19 水体工作场景、WaterRoot Prefab、水体 Shader/材质/测试、结果和文档；没有修改正式 `InitialHarbor`。

## 1. 冻结接口与所有权

| 接口 | D19-A04 行为 | 所有者/禁止事项 |
|---|---|---|
| 海平面 | WaterRoot 与水面网格固定 `Y=0`，提供 `IsSeaLevelConsistent` 检查 | D19-S01/世界布局冻结；不写潮汐、吃水或碰撞 |
| 风浪 | `D19WaterFrameParameters` 接收风向、风速、波长、振幅、速度 | D21/天气适配器未来只提供表现参数；Shader 不裁决天气 |
| 航行/浮力 | 没有任何船速、浮力、锚泊或航道逻辑 | ShipSystem、SailingSystem、MooringSystem 唯一写者 |
| 水体事实 | 水面颜色、透明度、浑浊度、波纹和泡沫仅为表现参数 | 不写存档、Registry、事件或天气状态 |
| UI | 工作场景只含 Screen Space Overlay 测试标签，排序在水下色调之后 | 不修改正式 Canvas、EventSystem 或 UI 数值 |

冻结空间依据 `d19_saint_margaret_layout_v1.json`：`SM_PORT_ORIGIN=(0,0,0)`、1 Unity 单位=1m、Y-up、+Z 内陆、-Z 海/出航、海平面 `Y=0`。水体工作场景使用主码头 `(0,0.2,-12)`、泊位 `(0,0,-30)`、浅水/暗礁区 `(-16,-1.5,-66)`、离港航道 `(0,-4,-112)` 的视觉代理，不替代这些锚点的强逻辑所有权。

## 2. 交付入口

- 水体工作场景：`game/Assets/Scenes/D19/Work/D19_Water_Work.unity`
- 水体根 Prefab：`game/Assets/Prefabs/Environment/D19/Water/WaterRoot.prefab`
- 运行时表现组件：`game/Assets/Prefabs/Environment/D19/Water/Scripts/D19WaterPresentation.cs`
- Shader：`game/Assets/Shaders/D19/D19WaterSurface.shader`、`D19WaterFoam.shader`、`D19UnderwaterVolume.shader`
- 材质：`game/Assets/Prefabs/Environment/D19/Water/Materials/`
- 测试：`game/Assets/Tests/EditMode/D19Water/`
- 机器结果：`game/tools/results/D19/D19-A04/`

WaterRoot 包含：1 个海面 Renderer、1 个岸线泡沫 Renderer、1 个水下色调体积 Renderer、5 个低面数暗礁 Renderer。测试船、测试码头和沙岸是工作场景中的 `VisualOnly` 代理，不是 ShipSystem/建筑系统实体。

## 3. 表现方案

### 海面

`D19WaterSurface` 使用 URP 17 Forward 透明 Pass：`Blend SrcAlpha OneMinusSrcAlpha`、`ZWrite Off`、`ZTest LEqual`、双面渲染。颜色由深海色、浅水色、浑浊度、距离衰减和 Fresnel 近似组成；水网格按海岸到深海的顶点颜色给出浅水梯度。波纹由 Shader 内的确定性解析波叠加产生，风向/风速只是外部参数，不包含天气枚举或概率。

因为当前 URP 全局配置未开启 Opaque/Depth Texture，本模块没有偷偷修改 URP 设置，也没有依赖屏幕深度采样。透明深度控制采用 `ZTest`、水面深度排序、距离 Alpha 衰减和低/中/高波形开关；D19-S02/D18-S03 如需真实场景深度淡化，应由正式渲染集成者评估全局深度成本。

### 岸线湿边与浪花

`D19WaterFoam` 是独立透明网格，沿冻结海岸方向形成可复用湿边，使用 UV 中心软边与滚动脉冲，不依赖粒子系统、不写水位。水面网格和泡沫网格分离，便于 D21 接入风浪或关闭泡沫。

### 暗礁可见性

暗礁为水体工作场景中的项目自有低面数岩体网格，使用准入的 Poly Haven Coral Mud 贴图作为材质输入；暗礁顶部高于海床、低于海面，透过透明水面保持可见。暗礁不添加航行碰撞，不改变安全航道或 ShipSystem 事实。

### 水下边界与 UI

`D19UnderwaterVolume` 是只在相机 `Y < seaLevelY` 时输出的透明色调体积，队列在水面之后、正式 UI 之前；相机在水面上方时片元丢弃，避免整屏异常染色。工作场景的 Screen Space Overlay 测试标签不依赖后处理，作为 UI 不被水下色调影响的排序检查入口。正式相机代码没有修改。

## 4. 外部资源与准入

只引用 D19-S01 准入目录中已登记的项目外部资源：

- `pack_polyhaven_sand_01_diff_jpg_d18_a03_v1`
- `pack_polyhaven_sand_01_nor_gl_jpg_d18_a03_v1`
- `pack_polyhaven_sand_01_rough_jpg_d18_a03_v1`
- `pack_polyhaven_coral_mud_01_diff_jpg_d18_a03_v1`
- `pack_polyhaven_coral_mud_01_nor_gl_jpg_d18_a03_v1`
- `pack_polyhaven_coral_mud_01_rough_jpg_d18_a03_v1`

水体 Shader、网格、材质实例、岸线泡沫和水下表现均标记为项目自有表现，不把候选资源伪装成 `LocalDevelopmentReady` 或 `ReleaseApproved`。没有引入渲染插件，没有接入 D18 HDRI，避免跨任务抢写灯光/天空配置。

## 5. 三档质量预算

| 档位 | 水面 | 反射/深度 | 泡沫 | 降级策略 |
|---|---|---|---|---|
| Low | 1 个解析波 | 无反射探针、无屏幕深度依赖、Alpha 水面 | 可关闭 | 移动端/低档默认；保留海平面和暗礁轮廓 |
| Medium | 2 个解析波 | Fresnel 近似、`ZTest LEqual`、距离淡出 | 保留岸线湿边 | 工作场景默认验证档 |
| High | 3 个解析波 | 更高波纹细节，无 SSR/后处理 | 保留岸线湿边 | 近景展示；目标设备仍需 D18-S03 实测 |

当前水体不申请全局深度纹理、不新增 Reflection Probe、不写 Renderer Feature。粗测为 Editor `Camera.Render` 1280×720：6 张截图耗时约 32.6—901.7ms，首帧包含导入/编译预热，不能视为目标设备帧时间；RTX 4070 Ti 上的真实目标设备 GPU/帧率仍由 D18-S03/D19-S02 复核。

## 6. 验证边界

- 隔离 Unity URP 工程：Unity `6000.0.80f1`、URP `17.0.4`，Shader 编译错误 0，场景脚本编译错误 0。
- 隔离工程 EditMode：`D19WaterPresentationTests` 共 3 项，`3 passed / 0 failed`；结果见 `game/tools/results/D19/D19-A04/D19_A04_WATER_EDITMODE_TESTS.xml`。
- 水体审计：海平面一致、UI Overlay 排序入口、水下相机守卫均通过。
- 截图：晴天深海、浅水、暗礁、岸线湿边、水下、低画质共 6 张。
- 主工程尝试编译时，发现其他 D19 模块的既有缺失类型：`WhalesIslandReef.D19A01`、`WhalesIslandReef.D19`、`Whalebone.Game.D19`。D19-A04 未修改这些模块；因此本任务整体不能声明主工程 PASS。
- 主工程阻塞证据见 `game/tools/results/D19/D19-A04/D19_A04_MAIN_PROJECT_GATE.json`；主工程状态不得与隔离水体模块 PASS 混写。

主工程解除阻塞后，必须按顺序重跑：水体 Runtime asmdef 编译 → D19Water EditMode 测试 → D19 工作场景重新导入 → 6 张截图/低中高性能粗测 → D19-S02 集成前锚点和海平面复核。
