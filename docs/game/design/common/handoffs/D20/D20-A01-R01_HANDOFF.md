# D20-A01-R01 交接报告

## 任务与结论

- 任务：`D20-A01-R01`
- 结论：**PARTIAL**
- 可放行的下一项：D20-A02-R01 可在隔离工作层启动，因为 `rig_interface_ready=true`。
- 不可放行：正式玩家/NPC/动物集成、D18-S03 晋级和 D20-S02-R01；D19-R02-S02 仍是 D20 正式集成硬门禁。

## 实际修改

- 新建 `game/Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_HumanoidFemale_R01.prefab`。
- 新建 `game/Assets/Art/Models/Characters/D20/Player/D20_PlayerHumanoidAvatar_R01.asset`，由准入骨架通过 Unity `AvatarBuilder` 派生，真实 `isValid=true`、`isHuman=true`。
- 新建 D20-A01-R01 专属 Editor 构建/审计入口 `game/Assets/Editor/D20A01R01/D20A01PlayerVisualBuilder.cs`；没有改正式生成器或 InitialHarbor。
- 更新只读接口 `game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json`，历史旧接口内容保存在 `D20_A01_PLAYER_VISUAL_INTERFACE_HISTORICAL_PARTIAL.json`。
- 新增项目自有中性皮肤材质绑定和候选暖肤/发色材质；没有把材质换色计为独立身体、发型或时代服装。
- 生成 front/back/three-quarter T-pose 工作层预览，共 3 张 512×768。

## 数据所有权和兼容

- D17/D2 仍唯一拥有 `D2_PlayerAvatar`、CharacterController、物理移动、出生/落地、相机和 gameplay 锚点。
- D10 AppearanceSystem 仍拥有外观槽、外观历史和缺失资源回退；D20-A01 只提供表现资源键与工作层视觉。
- 本任务没有写 Systems、AgentCore、Registry、Save Schema、contracts、server、web、主场景或 LocalDevelopmentReady/ReleaseApproved。
- Root Motion=false；候选 Prefab 无 Animator、CharacterController、MonoBehaviour，不会复制一套 gameplay 根。

## 真实证据

| 检查 | 结果 | 证据 |
|---|---:|---|
| Unity 版本 | 6000.0.80f1 | `D20_A01_R01_BUILD_V3.log` |
| Avatar | valid/human | `D20_A01_R01_EDITOR_AUDIT.json` |
| 视觉高度 | 1.79999995m | 同上 |
| Renderer / material | 3 / 1 | 同上 |
| LOD / sockets | 1 / 10 | 同上 |
| 逻辑组件 | MonoBehaviour 0、Animator 0、CharacterController 0 | 同上 |
| Focused EditMode | 6/6 PASS | `D20_A01_R01_EDITMODE_TESTS_FINAL2.xml` |
| 工作层预览 | 3/3 文件 | `D20_A01_R01_PREVIEWS.json` |
| 主工程源 FBX | Generic/NoAvatar、avatars=[] | `D20_A01_R01_SOURCE_IMPORT_AUDIT.json` |

测试命令为：

```powershell
& "C:\Program Files\Unity\6000.0.80f1\Editor\Unity.exe" -batchmode -nographics -disable-assembly-updater -projectPath "C:\Users\Administrator\Desktop\game\first01\game" -runTests -testPlatform editmode -assemblyNames D20.Wildlife.Tests -testFilter D20A01 -testResults "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D20\D20-A01-R01\D20_A01_R01_EDITMODE_TESTS_FINAL2.xml" -logFile "C:\Users\Administrator\Desktop\game\first01\game\tools\results\D20\D20-A01-R01\D20_A01_R01_EDITMODE_TESTS_FINAL2.log"
```

结果为 6 total / 6 passed / 0 failed / 0 skipped。此前失败 XML 没有删除，仍在同一结果目录。`git diff --check` 退出码 0；行尾转换提示不计为 whitespace error。

## 未完成事项

1. 只有 1/2 独立身体方向；不能用暖肤材质或同源复制冒充第二身体。
2. 只有 0/6 独立发型几何和 0/8 独立 1675 服装组合；D10 key 保留，但资源缺口仍真实存在。
3. 主工程源导入仍是 Generic/NoAvatar；本轮派生 Avatar 只证明候选工作层可消费，不改变源资源状态。
4. 无正式运行时消费者，LocalDevelopmentReady=0、ReleaseApproved=0；D18-S03 仍需验证真实消费者、保存/恢复、交互、截图和回退。
5. 未执行正式 Player 路线或性能采样；这不属于 A01 工作 Prefab 的证据范围。

## 给 D20-A02-R01 的迁移注意事项

- 只读取 `D20_A01_PLAYER_VISUAL_INTERFACE.json`，以 `rig_interface_ready=true` 作为“可建表现动作层”的唯一门槛。
- Animator 必须挂在 D20-A02 自己的工作表现包装上，保持 Root Motion=false；不要把 Animator、CharacterController 或动作裁决塞回 A01 Prefab。
- 动作缺失时回退 D17 稳定程序动作，不改变输入、移动、上/下船或存档事实。
- 不得把三张工作层预览、派生 Avatar 或 `intake_eligible` 写成 LocalDevelopmentReady/ReleaseApproved。
