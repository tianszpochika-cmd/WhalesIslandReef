# D19-A05 灯光与天气表现配置

## 结论

- 任务：D19-A05
- 日期：2026-08-14
- 结论：**PARTIAL**
- 静态配置、状态映射、边界测试和工作场景结构已完成并通过；Unity 编辑器在当前机器不可用，因此 Unity/Shader 编译、运行时截图、实际可读性和三档性能采样尚未执行。
- D19-S01 Attempt 2 已 PASS。本成果只用于 D19 工作场景和后续 D19-S02 集成，不代表 `LocalDevelopmentReady` 或 `ReleaseApproved`。

## 所有权与边界

灯光、天空、雾、色调映射、曝光、颜色调整、质量档和局部油灯接口由 D19-A05 的 `D19LightingProfileCatalog.json` 与 `D19LightingWeatherAdapter` 负责。Weather/Time 领域仍分别由 D9 `EnvironmentSystem` 和 D2 `DayNightSystem` 写入；适配器只接收快照，不修改天气概率、天气状态、时间推进或存档。

本任务没有修改 `InitialHarbor.unity`、`WeatherSystem`、`TimeSystem`、水 Shader、URP 资产、UI、Registry、存档或主相机输入。正式场景只能由 D19-S02 同步集成人员接入。工作场景为 `game/Assets/Scenes/D19/Work/D19_Lighting_Work.unity`，运行时表现晋级等待 D18-S03。

## 五个表现 Profile

配置入口：`game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json`。

| Profile | 输入语义 | 主光/环境 | 雾 | 曝光 EV | 色调映射 | 局部灯 | 外部天空参考 |
|---|---|---:|---|---:|---|---|---|
| `clear_day` | Calm，日间 | 1.10 / 1.00 | 关闭 | 0.00 | ACES | 无 | `candidate_hdri_green_point_park` |
| `overcast_rain` | Rain，非雾窗口 | 0.56 / 0.72 | 0.012，22–115m | 0.15 | Neutral | 无 | `candidate_hdri_kloppenheim_06` |
| `twilight_night` | 0.75–1.00 或 0.00–0.20 | 0.12 / 0.35 | 0.006，18–100m | 0.55 | Neutral | 时代油灯 | `candidate_hdri_joburg_central_sunset` |
| `fog` | Rain，时间 0.18–0.42 且可见度不高于 0.70 | 0.38 / 0.66 | 0.028，8–58m | 0.30 | Neutral | 无 | `candidate_hdri_kloppenheim_06` |
| `storm` | Storm，全天 | 0.22 / 0.42 | 0.022，12–72m | 0.45 | Neutral | 时代油灯 | `candidate_hdri_venice_sunset` |

所有 Profile 都记录 `owner_system=D19-A05`、`test_id`、来源说明、色彩参数、主光、环境、雾、曝光、色调、局部灯和低/中/高质量档。默认限制为：曝光 `[-4,4] EV`、对比度/饱和度 `[-100,100]`、雾密度 `[0,0.2]`、阴影强度 `[0,1]`、主光强度 `[0,2]`；景深、运动模糊和闪烁均关闭。

外部天空只引用 D18 准入目录中的 4 个 Poly Haven HDRI 内容 ID，不复制资源状态，不把候选晋级为运行时资源。许可证证据、哈希和本地路径见 `docs/game/design/common/assets/packs/ui_materials_audio/polyhaven_hdri_sky_references_v1.manifest.json`；官方许可证入口为 [Poly Haven License](https://polyhaven.com/license)。

## 状态映射

适配器入口是 `Whalebone.Game.D19Presentation.D19LightingWeatherAdapter.ApplyReadOnly(...)`。它读取 D9 `D9WeatherSeaSnapshot` 与只读时间值，并按优先级选择 Profile：

| 优先级 | Weather | 时间窗口 | 可见度上限 | 结果 |
|---:|---|---|---:|---|
| 100 | Storm | 全天 | 1.0 | `storm` |
| 90 | Rain | 0.18–0.42 | 0.70 | `fog` |
| 80 | 任意 | 0.75–1.00 或 0.00–0.20 | 1.0 | `twilight_night` |
| 50 | Rain | 全天 | 1.0 | `overcast_rain` |
| 10 | Calm | 全天 | 1.0 | `clear_day` |

时间值被限制到 `[0,1)`，首尾完整窗口按全天处理，跨午夜窗口按环绕区间处理。映射器不产生天气、不推进时间，也不改变 `D9WeatherSeaSnapshot`。

## 场景与适配器

- 工作场景：`game/Assets/Scenes/D19/Work/D19_Lighting_Work.unity`
- 可复用根 Prefab：`game/Assets/Prefabs/Environment/D19/Lighting/D19LightingRoot.prefab`
- 适配器：`game/Assets/Prefabs/Environment/D19/Lighting/D19LightingWeatherAdapter.cs`
- Profile 数据模型、选择器和合法性检查：`game/Assets/Settings/D19/Lighting/D19LightingProfileModel.cs`
- 根节点包含主方向光、`D19_OilLamp_Interface` 局部灯光接口、`D19_Lighting_Profiles` 配置节点和 `D19_Readability_Anchors` 表现验收锚点。

局部灯只提供暖色、低频率、可限制数量的油灯/烛灯接口；不引入霓虹、电灯或高频闪烁。局部灯的真实拥有者仍是场景/建筑消费者，适配器只按照 Profile 的 `local_light_limit` 和质量档应用表现限制。

## 质量档

| 档位 | 阴影距离 | 级联 | 雾 | 后处理 | 夜间/风暴局部灯上限 |
|---|---:|---:|---|---|---:|
| Low | 28–35m（夜间关闭） | 0–1 | 保留必要雾 | 关闭 | 0–1 |
| Medium | 28–55m | 1–2 | 开启 | 开启 | 0–2 |
| High | 45–80m | 2–4 | 开启 | 开启 | 0–4 |

质量档不改 Weather/Time 逻辑；配置中没有强制景深、运动模糊或高闪烁。D16 目标为 1920×1080、60 FPS、CPU p95 ≤12ms、GPU p95 ≤15ms，仍需在 D19-S02/运行时环境采样确认。

## 可读性契约

五类状态均要求检查：玩家、NPC、建筑入口、码头边缘、海面和交互提示。静态配置已保证夜间仍有环境光/油灯接口，雾状态有明确起止距离，且不使用模糊和闪烁制造氛围。实际截图中的轮廓、入口、边缘和提示对比度尚未由 Unity 运行时确认，不能提前声明通过。

## 测试与结果

- 静态审计：`python game/tools/results/D19/D19-A05/d19_a05_static_audit.py`，PASS；5 Profile、5 个映射结果、4 个准入 HDRI、工作场景边界和只读领域源检查通过。
- EditMode 测试入口：`game/Assets/Tests/EditMode/D19LightingProfileTests.cs`，包含五 Profile 完整性、五种代表映射、非法曝光/运动效果拒绝、非法映射/颜色边界拒绝、跨午夜时间窗口测试。
- JSON 语法检查：Profile catalog、截图矩阵、可读性矩阵、性能样本和静态结果均应使用 `python -m json.tool` 检查。
- Unity/Shader 编译：NOT RUN，当前机器未发现 Unity Editor/Unity.exe。
- 备用 .NET 项目构建也未能启动到编译阶段：Unity 生成的 `project.assets.json` 和 Unity Editor 程序集不在当前环境；结果保留在 D19-A05 测试报告中。
- 截图：10 个代表镜头（五状态的昼/夜覆盖）已登记在 `game/tools/results/D19/D19-A05/D19_A05_SCREENSHOT_MATRIX.json`，实际截图 0/10。
- 性能：Low/Medium/High 样本和目标登记在 `D19_A05_PERFORMANCE_SAMPLES.json`，实际采样 0/3。
- 可读性：静态契约 PASS，运行时视觉检查 NOT RUN，详见 `D19_A05_READABILITY_MATRIX.json`。

## 后续接管

1. D19-S02 在预集成快照后，将本适配器/工作配置接入正式场景，并保留 D19-A05 只读映射和唯一写者边界。
2. 在可启动 Unity 的机器上运行 EditMode/Shader 编译、10 张截图、六区域可读性检查和三档性能采样。
3. D18-S03 完成真实运行时消费者、碰撞/LOD/回退和运行时晋级前，不得标记 `LocalDevelopmentReady` 或 `ReleaseApproved`。
