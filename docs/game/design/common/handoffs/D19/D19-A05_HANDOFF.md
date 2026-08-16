# D19-A05 灯光与天气表现交接报告

## 结论

- 任务：D19-A05
- 日期：2026-08-14
- 状态：**PARTIAL**
- D19-S01 Attempt 2：PASS，已核对工作范围和 D18 准入目录。
- 静态配置、只读状态映射、配置合法性检查、工作场景结构和测试入口已完成。
- Unity/Shader 编译、运行时截图、运行时可读性、三档性能样本尚未执行，因为当前机器没有 Unity Editor/`Unity.exe`。

## 新增成果

- `game/Assets/Settings/D19/Lighting/D19LightingProfileModel.cs`
  - `D19LightingReadOnlyState`：只读天气/时间/可见度输入。
  - `D19LightingProfileSelector`：优先级和跨午夜窗口选择。
  - `D19LightingProfileValidator`：Profile 数量、字段、范围和禁用高风险效果检查。
- `game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json`
  - `clear_day`、`overcast_rain`、`twilight_night`、`fog`、`storm` 五个完整 Profile。
  - 每个 Profile 记录 owner、test_id、来源、颜色、主光、环境、雾、曝光、色调、局部灯和 Low/Medium/High 档。
- `game/Assets/Prefabs/Environment/D19/Lighting/D19LightingWeatherAdapter.cs`
  - 表现层适配器，只读 `D9WeatherSeaSnapshot` 与时间值。
  - 应用主光、环境光、雾、天空色、URP 色调/颜色调整、质量档和油灯接口。
- `game/Assets/Prefabs/Environment/D19/Lighting/D19LightingRoot.prefab`
  - 可复用 LightingRoot、主方向光、油灯接口和 Profiles 节点。
- `game/Assets/Scenes/D19/Work/D19_Lighting_Work.unity`
  - D19 独立工作场景；未引用 `InitialHarbor`，未触碰正式场景。
- `game/Assets/Tests/EditMode/D19LightingProfileTests.cs`
  - 五 Profile 完整性、代表映射、非法曝光/运动效果、非法映射/颜色边界拒绝和跨午夜时间窗口测试。
- `game/tools/results/D19/D19-A05/`
  - 静态审计脚本与结果、截图矩阵、可读性矩阵、性能样本、测试结果。

## Profile 与映射

| 输入/条件 | Profile |
|---|---|
| `Storm` 任意时间 | `storm` |
| `Rain`、时间 0.18–0.42、可见度 ≤0.70 | `fog` |
| 任意天气、时间 0.75–1.00 或 0.00–0.20 | `twilight_night` |
| `Rain` 其他时间 | `overcast_rain` |
| `Calm` 其他时间 | `clear_day` |

时间和天气均为读入快照；没有在 Systems 中添加渲染、网络或 LLM 代码。D9/D2 仍是天气和时间的逻辑写者，D19-A05 不裁决天气概率、时间推进或游戏状态。

## 许可与资源

Profile 只引用 D18 准入目录中的 4 个 Poly Haven HDRI 内容 ID：

- `candidate_hdri_green_point_park`
- `candidate_hdri_joburg_central_sunset`
- `candidate_hdri_kloppenheim_06`
- `candidate_hdri_venice_sunset`

来源 manifest：`docs/game/design/common/assets/packs/ui_materials_audio/polyhaven_hdri_sky_references_v1.manifest.json`；许可：CC0-1.0；manifest SHA-256：`C2646A0F27319985A0962451DF7EDB862CEF35520F362181B23A2ABDB864C71C`；官方许可证：[Poly Haven License](https://polyhaven.com/license)。本任务没有改变外部资源状态，没有标记 `LocalDevelopmentReady` 或 `ReleaseApproved`。

## 验证结果

```text
python game/tools/results/D19/D19-A05/d19_a05_static_audit.py
PASS：5 profiles；4 intake-eligible sky references；5 representative mappings；domain writers unchanged

python -m json.tool <catalog/matrix/result JSON files>
PASS：JSON 语法

git diff --check
PASS（退出码 0；仅显示既有文件的换行格式提示）
```

运行时证据：

- 截图矩阵 10 个代表镜头，实际截图 0/10，状态 NOT_RUN。
- 可读性六区域静态契约 PASS，运行时视觉检查 NOT_RUN。
- Low/Medium/High 性能样本 0/3，FPS/CPU/GPU 仍待 Unity 运行时采集。
- Unity/Shader 编译和 EditMode Test Runner：NOT RUN。
- 备用 `dotnet build ... --no-restore` 未通过环境前置：缺少 Unity 生成的 `project.assets.json`，并且本机没有 Unity 6000.0.80f1 Editor 程序集；未把它当作代码通过。

## 边界核对

未修改：`InitialHarbor.unity`、WeatherSystem、TimeSystem、水 Shader、URP 资产、UI、Registry、存档、主相机输入和任何正式 Prefab。D19-A05 的适配器只在工作范围内写表现属性；不得被 UI、Agent 或 Weather/Time 系统反向写入。

## 后续事项

1. D19-S02 在集成前保存基线，再接入工作场景配置并执行 Unity 编译、截图、可读性和性能验证。
2. 运行时质量档根据 1920×1080、60 FPS、CPU p95 ≤12ms、GPU p95 ≤15ms 目标采样；如不达标只能调整 D19-A05 表现配置，不改变天气或时间逻辑。
3. 运行时资源晋级和 `LocalDevelopmentReady` 等待 D18-S03；不要把本交付的静态 PASS 当作运行时就绪。
