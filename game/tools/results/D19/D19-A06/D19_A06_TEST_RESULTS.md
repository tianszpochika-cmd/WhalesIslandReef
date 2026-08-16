# D19-A06 静态测试结果

日期：2026-08-14  
结论：**PARTIAL**

## 已运行

- `python game/tools/results/D19/D19-A06/d19_a06_static_audit.py`：完成，生成 `D19_A06_STATIC_AUDIT.json`、30 条音频元数据和 5 张静态工作场景验证板。
- `python -m unittest discover -s game/tests/D19 -p 'test_*.py' -v`：**5/5 PASS**。
- `git diff --check`：**PASS**（退出码 0；仅有工作区既有 CRLF 提示，无 whitespace error）。

## 静态断言

| 项目 | 结果 |
|---|---|
| PropSet Prefab | 4 |
| VFX Prefab | 5 |
| 音频区 | 5 |
| 逐项 CC0 音频候选 | 30/30 源文件与工作副本存在 |
| 音频测量 | 通道、44.1kHz 采样率、时长、RMS 代理响度已写入索引 |
| 重复哈希 | 2 组，保留并记录为源到工作副本的有意重复 |
| 场景直接 AudioSource | 0 |
| 非触发阻挡 Collider | 0 |
| 路线安全余量 | 9/9 PASS |
| 静态验证板 | 5 |
| InitialHarbor | 未出现在本任务 git 状态变更中 |

## 未运行 / 需后续门禁

当前环境找不到 Unity executable，因此以下项目不作 PASS：

- Unity 场景/Prefab 导入、编译、EditMode/PlayMode Test Runner；
- AudioImporter 的压缩、预加载、流式设置和真实 MixerGroup 绑定；
- 运行时字幕/视觉替代、静音、环境层 ducking；
- 5—10 分钟真实玩家路线和设备 CPU/GPU/GC 性能样本；
- 烟尘/海鸟/溅水代理替换为正式 ParticleSystem；
- 正式 InitialHarbor 集成和 D18-S03 晋级。

以上缺口已写入 `D19_A06_STATIC_AUDIT.json` 和交接报告，未用占位结果冒充通过。
