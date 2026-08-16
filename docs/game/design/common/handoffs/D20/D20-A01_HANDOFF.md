# D20-A01 交接报告

## 结论

**PARTIAL**。交付了 1 个真实、准入、可引用的 Kenney Blocky Characters 玩家视觉候选和只读接口；没有满足第二身体/性别方向、6 个独立发型、8 套独立时代服装、Humanoid Avatar、锚点/LOD/碰撞和 Unity 截图验证，因此不能宣称两套玩家外观完成。

## 来源与准入

- manifest：`pack_kenney_blocky_characters_d18_a04_v1`
- intake：`intake_3469c541b7a7968018ba`
- content / ResourceKey：`asset_kenney_blocky_character_a_v01`
- 文件：`game/Assets/Art/Candidates/CharactersAnimals/KenneyBlockyCharacters/Models/character-a.fbx`
- D18 证据：6 mesh、8 bones、27 source animation clips、1 material、bounds valid、Generic/NoAvatar、Avatar absent、最大尺寸 9.0m、Unity scale 1。
- SHA-256：`FF5010002CA7B42D6055691D9F781A1DFA3A1266ABB2AC32A7BF7A565BC62724`。
- ImportVerified 证据路径：`game/tools/results/D18/D18-A04/D18_A04_UNITY_IMPORT_AUDIT.json`；本任务没有重新运行 Unity 导入。
- 许可：CC0-1.0；本任务不重复写入或升级 D18 许可/准入结论。
- 女性/中性缺口：D18-S02-R01 记录 `female_or_neutral_human_gap.intake_eligible_count=0`；D18-A04-R02-F 的 4 个静态 FBX 仍为 NotImported，未使用。

## 修改文件

- `game/Assets/Prefabs/Characters/D20/PlayerVisuals/D20_PlayerVisual_Neutral.prefab` 及 `.meta`
- `game/Assets/Art/Models/Characters/D20/Player/Materials/` 下 2 个肤色和 2 个发色候选材质及 `.meta`
- `game/Assets/Tests/EditMode/D20/D20A01PlayerVisualTests.cs` 及 `.meta`
- `game/tools/results/D20/D20-A01/D20_A01_PLAYER_VISUAL_INTERFACE.json`
- `game/tools/results/D20/D20-A01/d20_a01_static_check.py`
- `game/tools/results/D20/D20-A01/D20_A01_STATIC_CHECK.json`
- `docs/game/design/common/D20_PLAYER_VISUAL_PRESETS.md`

## 接口

机器接口中的 `rig_interface_ready=false` 是有意的真实状态。D17/D2 继续拥有 `D2_PlayerAvatar`、单位物理根、相机目标和运行时锚点；D10 继续拥有 Appearance/ResourceKey 和换装裁决。D20-A02 只读 `D20_A01_PLAYER_VISUAL_INTERFACE.json`，不能回写本任务目录或提升 `intake_eligible`、LocalDevelopmentReady、ReleaseApproved。

稳定回退为 D17 的 `D2FallbackHumanoidBuilder/D2CharacterController`，触发条件是资源缺失或 Avatar 未验证；回退保留 gameplay/appearance history，不写 Save/identity。

## 测试

- 静态检查：`python game/tools/results/D20/D20-A01/d20_a01_static_check.py`，实际输出 `status=PASS`；它只证明候选边界和接口声明，不证明 Unity 导入。
- EditMode 测试：已写入 `D20A01PlayerVisualTests.cs`，覆盖候选 Prefab 可加载、无 Animator/CharacterController/MonoBehaviour、接口和回退存在、文本无运行时绑定。
- Unity：本机未找到 Unity Editor，未执行编译、PrefabUtility、Renderer/材质、Avatar、bounds/脚底、Console、截图或 PlayMode；不能把这些项目写成 PASS。
- `git diff --check`：实际退出码 0；Git 仅提示现有工作区的 CRLF/LF 转换警告，没有 whitespace error。

## 未完成项与责任

| 缺口 | 责任/建议 |
|---|---|
| 第二身体/性别方向 | D18-S02-R01/D18-A04-R02-F 提供合法且 ImportVerified 的候选后，由 D20-A01/A03 复核；当前不得用未准入 FBX |
| Humanoid Avatar 与完整骨架 | D20 角色 Prefab 消费者在 Unity 可用时验证；当前 Generic/NoAvatar |
| 6 独立发型、8 时代服装组合 | D10 逻辑 key 已保留；需要准入几何模块，不得以换色凑数 |
| Visual 包装、脚底/手头/相机锚点、碰撞、LOD | D17 物理根保持唯一写者；D18-S03/D20 后续消费 Prefab 时补证 |
| Unity 截图与性能 | Unity 环境恢复后补做正面、背面、T-Pose 和性能审计 |

未修改正式玩家生成器、CharacterSystem、Save Schema、Animator、InitialHarbor、场景、UI、Registry 或 Git 历史。
