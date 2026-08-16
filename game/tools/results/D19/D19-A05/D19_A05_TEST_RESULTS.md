# D19-A05 测试结果

日期：2026-08-14

## 已执行

```text
python game/tools/results/D19/D19-A05/d19_a05_static_audit.py
PASS
  profile_count=5
  intake_eligible_sky_reference_count=4
  sample_mapping=clear_day/overcast_rain/twilight_night/fog/storm
  domain_writer_files_unchanged_by_scope=true

python -m json.tool game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json
python -m json.tool game/tools/results/D19/D19-A05/D19_A05_SCREENSHOT_MATRIX.json
python -m json.tool game/tools/results/D19/D19-A05/D19_A05_PERFORMANCE_SAMPLES.json
python -m json.tool game/tools/results/D19/D19-A05/D19_A05_READABILITY_MATRIX.json
python -m json.tool game/tools/results/D19/D19-A05/D19_A05_STATIC_AUDIT.json
PASS（JSON 语法）

git diff --check
PASS（退出码 0；仅显示既有文件的换行格式提示）
```

## 未执行/待 Unity 环境

- Unity/Shader 编译：NOT RUN；当前机器未发现 Unity Editor 或 `Unity.exe`。
- `D19LightingProfileTests` EditMode：NOT RUN；测试入口已建立，但没有可用 Unity Test Runner。
- 测试覆盖：五 Profile/映射、跨午夜窗口、曝光/运动效果、映射时间和颜色边界非法配置。
- 截图：0/10；矩阵已登记五状态的昼/夜代表检查和六个关键区域。
- 性能：0/3；Low/Medium/High 目标和限制已登记，实际 FPS、CPU p95、GPU p95 需运行时采样。
- 可读性：静态契约 PASS；运行时视觉检查 NOT RUN。
- 备用 `dotnet build game/Whalebone.Game.D19.Lighting.Runtime.csproj --no-restore`：环境阻断；缺少 Unity 生成的 `Temp/obj/.../project.assets.json`，且项目引用本机不存在的 Unity 6000.0.80f1 Editor 程序集。未将此结果冒充为编译通过。

因此 D19-A05 交付结论为 **PARTIAL**，不是 Unity 运行时 PASS。
