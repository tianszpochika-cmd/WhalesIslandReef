# D18-A04-R01 角色/动物资源证据复审交接

任务编号：`D18-A04-R01`  
日期：2026-08-15  
结论：**PARTIAL**

## 1. 本轮实际修改

只新增/更新了本复审专属证据范围：

- `tools/d18_a04_r01_review_audit.py`：只读复审工具；读取既有 manifest、D18-A04 TSV/Unity JSON、准入目录和 ready catalog，不修改它们。
- `docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.md`：可读复审报告。
- `docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json`：25 条逐候选机器证据、输入文件哈希、技术字段、缺口和消费分类。
- `game/tools/results/D18/D18-A04-R01/`：本轮 manifest/准入审计、JSON 校验、unittest、工具输出和结果证据。
- 本交接文件与当日 `.workbuddy/memory/2026-08-15.md`。

未修改：全局 `d18_prefab_candidate_intake_catalog.json`、任何原始 manifest、候选资源、Registry、asset_state、Prefab、场景、Systems、存档、contracts/server/web 和 Git 历史。没有创建 D20 Prefab 或正式集成。

## 2. 真实复审数量

| 指标 | 结果 |
|---|---:|
| 角色/动物 manifest | 5 |
| manifest 候选记录 | 25 |
| Unity 6 隔离夹具导入 FBX | 24 |
| Blender-only 鸟类 | 1 |
| 独立模型身份 | 23 |
| Mesh | 30 |
| 动画片段 | 228 |
| 材质 | 121 |
| Unity 导入错误 | 0 |
| Unity 导入命名 warning | 25 |
| 本地候选文件存在 | 25/25 |
| 候选 `.meta` 存在 | 25/25 |
| 本地许可证证据存在 | 5/5 包 |
| 保留归档 SHA-256 与 manifest 匹配 | 5/5 包 |
| 本轮对应全局 `PrefabCandidateIntake` | 0 |
| `LocalDevelopmentReady` | 0 |
| `ReleaseApproved` | 0 |

## 3. 五类 D20 消费结论

- `player_visual_core`：PARTIAL。Kenney `character-a.fbx` 可作为中性视觉基体的受控工作复核输入；Generic/NoAvatar、原始 MaxDim=9m、无 Prefab/Avatar/LOD/碰撞/锚点，不能直接接入玩家。
- `npc_visual_core`：PARTIAL。4 个男性候选均已导入，但 3 个需要时代服饰和 Humanoid 复核，`Male_Suit` 不适合直接表达 1675 年角色。
- `wildlife_core`：PARTIAL。9 个独立陆生候选和 7 个水生候选有 Generic 导入证据；物种语义、尺度、水深、碰撞、LOD、锚点和生态只读接入未完成。
- `pet_core`：PARTIAL。Husky、ShibaInu 是狗候选；没有猫，不能替代或冒充猫。
- `animation_core`：PARTIAL。24 个 FBX/228 个片段是真实原始导入证据；无 Animator/Avatar 绑定、fallback、Prefab 或运行时消费者。

## 4. 明确缺口与责任

| 缺口 | 真实结论 | 责任入口 |
|---|---|---|
| `female_or_neutral_human_gap` | 女性候选不存在；Kenney 中性视觉基体不满足 D20 Humanoid/Prefab 条件 | D18-A04-R02-F、D20-A01 |
| `cat_gap` | 5 个包中无猫；狗不可替代猫 | D18-A04-R02-C、D20-A06 |
| `bird_gap` | 1 个 Blender 源文件有来源/许可证/归档哈希，但没有 Unity 模型/动画导入证据 | D18-A04-R02-B、D20-A05 |
| `humanoid_avatar_gap` | 24 个导入记录均为 Generic/NoAvatar | D20-A01、D20-A02 |
| `prefab_runtime_evidence_gap` | 无 Prefab、Animator、LODGroup、碰撞、交互锚点、运行时消费者、保存/恢复或编辑器预览截图 | D20-A01—A06、D18-S03 |

所有候选仍是 manifest `Candidate`；鸟类同时是 `NotImported`。本轮“推荐进入受控工作 Prefab 复核”只表示证据优先级，不表示准入或 LocalDevelopmentReady。

## 5. 验证命令与结果

1. `python tools/d18_asset_manifest_audit.py --project-root . --manifest-root docs/game/design/common/assets/packs/characters_animals --output game/tools/results/D18/D18-A04-R01/D18_A04_R01_MANIFEST_AUDIT.json`  
   结果：`PASS`，5 manifests，0 errors，0 issues。
2. `python tools/d18_prefab_candidate_intake_audit.py --project-root . --catalog docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json --output game/tools/results/D18/D18-A04-R01/D18_A04_R01_INTAKE_CATALOG_AUDIT.json`  
   结果：`PASS`，全局 25 manifests / 1,857 candidates / 1,674 intake entries；资源状态和 catalog 未变。
3. `python tools/d18_a04_r01_review_audit.py --project-root . --output docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json`  
   结果：`PARTIAL`，25 条逐候选记录、24 个隔离 FBX 导入、1 个 source-only 鸟类、0 个本轮全局准入条目。
4. `python -m unittest -v tools.tests.test_d18_asset_manifest_audit tools.tests.test_d18_prefab_candidate_intake`  
   结果：`11/11 PASS`。
5. `python -m pytest ...`  
   结果：当前环境没有 pytest（`No module named pytest`），已保留该事实；unittest 入口已执行。
6. 三个 JSON（全局准入目录、ready catalog、R01 复审 JSON）通过 `python -m json.tool`。
7. `git diff --check`：最终复验应为 PASS；无提交/推送。

## 6. 后续准入注意事项

- D18-S02-R01 是唯一允许刷新全局准入结论的验收入口；本任务不能把 `Candidate` 改成 `PrefabCandidateIntake`、`LocalDevelopmentReady` 或 `ReleaseApproved`。
- D20-A01/A03/A05/A06 若开始工作场景，只能先读取本复审的证据建议，并等待 D18-S02-R01 的正式准入裁决；不得把本复审 JSON 当作运行时批准。
- D20-A01/A02 必须重新验证 Humanoid Avatar、骨骼映射、D17 1.80m 视觉高度、脚底、Root Motion=false 和六个旧动作 ID；Generic/NoAvatar 不能静默重定向。
- D20-A05/A06 必须保留鸟、猫缺口；不能用狗替猫、Blender 源文件冒充 Unity 鸟类、或用缩放/换色制造新物种。
- 所有 Prefab 级碰撞、LOD、锚点、Animator、运行时消费者、保存恢复和截图证据应由 D20 责任任务产生，并由 D18-S03 统一复核。

## 7. 结果路径

- 主报告：[D18_A04_R01_CHARACTER_ANIMAL_REVIEW.md](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.md)
- 机器复审：[D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/assets/review_packets/D18-A04-R01/D18_A04_R01_CHARACTER_ANIMAL_REVIEW.json)
- 机器结果目录：[D18-A04-R01 results](C:/Users/Administrator/Desktop/game/first01/game/tools/results/D18/D18-A04-R01)
- 本交接：[D18-A04-R01_HANDOFF.md](C:/Users/Administrator/Desktop/game/first01/docs/game/design/common/handoffs/D18/D18-A04-R01_HANDOFF.md)

最终结论保持：**PARTIAL**。本轮证据足以明确可复用候选和真实缺口，但不足以宣称五类 D20 工作已就绪或资源已获正式准入。
