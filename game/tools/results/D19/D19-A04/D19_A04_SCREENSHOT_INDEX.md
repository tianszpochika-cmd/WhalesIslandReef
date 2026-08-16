# D19-A04 水体截图与粗测索引

截图由隔离 Unity `6000.0.80f1` + URP `17.0.4` 工作工程的 `Camera.Render` 生成，尺寸均为 1280×720。截图只证明水体表现候选，不证明正式 InitialHarbor 集成。

| 编号 | 文件 | 场景检查 | 质量 |
|---:|---|---|---|
| 1 | `screenshots/D19_A04_01_sunny_deep_water.png` | 深海色、远景衰减、暗礁轮廓 | High |
| 2 | `screenshots/D19_A04_02_shallow_water.png` | 浅水颜色、透明水面、暗礁可见 | Medium |
| 3 | `screenshots/D19_A04_03_reef_visibility.png` | 暗礁顶部穿过透明水面可见 | High |
| 4 | `screenshots/D19_A04_04_shoreline_wet_edge.png` | 岸线浪花/湿边、沙岸过渡 | Medium |
| 5 | `screenshots/D19_A04_05_underwater_boundary.png` | 相机下水后的色调边界 | Medium |
| 6 | `screenshots/D19_A04_06_low_quality_mobile.png` | 低档单波、无深度纹理依赖 | Low |

截图粗测 `Camera.Render` 毫秒样本（顺序对应 1—6）：`901.68, 57.45, 38.22, 40.63, 32.56, 85.23`。首张包含 Shader/材质预热，不作为目标设备帧率结论。
