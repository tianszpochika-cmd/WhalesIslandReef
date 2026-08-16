# D18-A04-R02-C handoff

任务：D18-A04-R02-C（猫资源补齐，隔离候选与证据）
日期：2026-08-15
结论：**PARTIAL**

## 前置与写入边界

- D18-S02-v2：已按现有交接核对为 PASS；D18-S02-R01 仍是唯一的后续准入裁决者。
- 已阅读并对齐：`AGENTS.md`、`docs/CONTEXT.md`、D18-S01/A01/A02/S02-v2、D18-A04、D20-S01、D11 宠物设计、D18 manifest schema/审计规则及现有狗/动物候选。
- 现有动物候选中有 Husky、ShibaInu 等狗，但没有猫；本任务只补猫，不复制、重命名或改写其他动物。
- 只写入 Cat 隔离候选目录、Cat 隔离 manifest/证据目录、D18-A04-R02-C 结果目录、本交接和当日记忆；未修改全局 catalog、Registry、正式场景、Prefab、Systems、存档、contracts/server/web 或 Git 历史。

## 包、许可、哈希与真实文件

| 项目 | 结果 |
|---|---|
| 包 | Quaternius `Animal Pack Vol.2`，只选原始 `FBX/Cat.fbx` |
| 官方页 | https://opengameart.org/content/animated-animales-low-poly |
| 直接下载 | https://opengameart.org/sites/default/files/Animal%20Pack%20Vol.2%20by%20%40Quaternius.zip |
| 许可 | CC0 1.0；官方页面标注 CC0，官方 CC0 legal-code 快照保存在候选目录 |
| 下载日期 | 2026-08-15 |
| 归档 | `game/tools/results/D18/D18-A04-R02-C/Animal Pack Vol.2 by @Quaternius.zip`，2,069,972 bytes |
| 归档 SHA-256 | `6D8F6789D65DBDB352EBAD98A4209F26B41B384E87D81BC05E9CA76139DB1248` |
| 选择文件 | `game/Assets/Art/Candidates/CharactersAnimals/GapFill/Cat/Source/Cat.fbx`，365,628 bytes |
| 选择文件 SHA-256 | `24D03762BCBCDF5414516354B0D80C6580A0FFD7B5ABE4D8EDD26AD7BF7A0DB5` |
| 许可快照 | `game/Assets/Art/Candidates/CharactersAnimals/GapFill/Cat/Source/LICENSE_CC0_1.0.txt` |

完整归档文件清单和页面快照见 `game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_ARCHIVE_FILE_LIST.md` 与 `official_page_snapshot.html`。归档只保留一份，临时解压目录已清理；候选目录只保留猫 FBX 和许可证据。

## 导入结果与动作

Unity `6000.0.80f1` 隔离夹具实际导入：

- 导入文件：1；网格：1；嵌入材质：3（White/Grey/Pink）；贴图子资源：0。
- Renderer：1 个 `SkinnedMeshRenderer`；Generic；Avatar：无。
- 顶点/三角形：1,537 / 180；bounds：`(7.323745, 6.690212, 1.605036)` Unity units；非零 bounds。
- 动画：2 个真实 clip，`CatArmature|Idle`、`CatArmature|Walking`。
- 导入错误：0；警告：3（无 Collider、无 LODGroup、无 Avatar）。
- 未伪报 Run、Rest、Interact、Feed、Comfort、CatchMouse、Warn、Lost、Recover；这些动作在 FBX 中未出现。

机器结果：

- `game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_UNITY_IMPORT_AUDIT.json`
- `game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_UNITY_IMPORT_AUDIT.log`
- `game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_MANIFEST_AUDIT.json`
- `game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_EVIDENCE_AUDIT.json`

## D20-A06 适配边界

- 后续由 D20-A06 决定 presentation wrapper、目标比例、根/脚底校准、前向轴复核、Collider/触发器、LODGroup、跟随锚点和动作映射；本任务不创建正式宠物 Prefab。
- 原始高度 6.690 Unity units，若视觉目标暂按 1 m 高度评估，均匀缩放估算约 0.1495；这只是复核起点，不是本任务写回的缩放。
- `min.y=-1.228470` 说明原点不是已确认脚底；必须在 D20-A06 视觉复核中校准，不改写源 FBX。
- 按项目标准以 Y-up / Z-forward 记录；原始 bounds X 方向较长，轴向/朝向必须由 D20 复核，未自动改轴。
- Generic、无 Avatar 是动物候选事实，不应按 Humanoid 重定向。
- D11PetSystem/AnimalCareSystem 继续拥有宠物事实、生命周期、照料、死亡、动作有效性和事件；该候选只提供几何、材质和已有动画证据。

## 验证命令与结果

```text
python tools/d18_asset_manifest_audit.py --project-root . --manifest docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/D18_A04_R02_C_manifest.json --output game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_MANIFEST_AUDIT.json
=> PASS, 1 manifest, 0 errors, 0 issues

python -c "jsonschema.Draft202012Validator(schema).validate(manifest)"
=> PASS

Unity 6000.0.80f1 -batchmode -nographics -projectPath <isolated fixture> -executeMethod CatImportAudit.Run -quit
=> import errors 0; warnings 3; 1 mesh / 3 materials / 2 clips; result copied to task result directory

git diff --check
=> exit 0

python -m unittest tools.tests.test_d18_asset_manifest_audit
=> 8/8 PASS
```

## 已知边界与结论

实际猫模型、许可、归档哈希和 Unity 导入证据已一一对应；但是只有 Idle/Walking，且 raw FBX 无 Collider、LODGroup、Avatar、贴图子资源，比例/脚底/轴向还需 D20-A06 评估。因此结论为 **PARTIAL**：猫缺口已得到真实候选补齐，但不代表宠物集成完成。D18-S02-R01 必须在后续复核后决定是否进入项目准入流程。
