# D18-S02 v2 免费资源库准入验收与受控开发解锁

## 1. 结论

**D18-S02 v2：PASS。**

本轮任务的职责是“免费资源库准入验收与受控开发解锁”，不是运行时资源就绪验收。它只证明有真实许可证、哈希、候选文件、导入证据和映射的资源可以被后续 D19—D22 指定任务带入受控工作场景或 Prefab 制作。

旧版 `D18_FREE_ASSET_LIBRARY_ACCEPTANCE.md`、旧版 `D18-S02_HANDOFF.md` 和旧机器结果全部保留，结论仍为 **BLOCKED**。它们现在标注为“旧版运行时就绪验收”的历史尝试：当时把 Prefab、碰撞、锚点、LOD、Animator、运行时消费者和保存/交互证据放在 S02 门内，而这些证据由 D19—D22 产生，形成循环依赖。该 BLOCKED 是任务模型发现了依赖设计错误，不是 Agent 失败，也没有通过篡改历史结果解除阻断。

本报告不改变任何 manifest 的 `Candidate`/`ImportVerified` 等资源状态，不增加 `LocalDevelopmentReady` 或 `ReleaseApproved`，不表示 InitialHarbor 已接入，也不表示 Steam Ready。

## 2. 真实输入与审计结果

| 指标 | 真实结果 |
|---|---:|
| 实际 manifest | 25 |
| 候选文件 | 1,857，均存在 |
| 许可证/哈希/路径/导入可形成准入证据的候选文件 | 1,674 |
| 可准入 manifest | 16 |
| 被排除 manifest | 9 |
| 准入目录重复 manifest-file 引用 | 0 |
| 准入目录错误 | 0 |
| `LocalDevelopmentReady` | 0 |
| `ReleaseApproved` | 0 |

准入文件按领域统计：UI 1,224、环境 329、材质 87、音频 30、灯光参考 4；优先级为 P0 1,670、P1 4。准入目录的 1,674 条记录是文件级可控工作场景入口，不是 1,674 个独立模型，也不是 1,674 个正式内容 ID。

D18-A07 的权威映射保持可追溯：输入 2,264、输出 2,264，`mapped=1,107`、`unmapped=1,157`。映射工具和 Registry 均未被本任务改写。

## 3. 非资源状态：PrefabCandidateIntake

新增目录 `assets/d18_prefab_candidate_intake_catalog.json` 使用非资源状态的 `intake_eligible` 字段。它仅表示：

> 许可证、哈希、候选路径、导入证据和映射证据齐全，指定责任任务可以把该候选用于受控工作场景或 Prefab 制作。

它绝不等于：

- `LocalDevelopmentReady`；
- 已接入正式 InitialHarbor 或其他正式场景；
- 已有运行时消费者、碰撞、锚点、LOD、Animator、交互或保存证据；
- `ReleaseApproved` 或 Steam Ready。

只有真实运行时证据通过新的 D18-S03，才可以按原六种资源状态机的规则晋级 `LocalDevelopmentReady`。本任务没有改变那六种状态，也没有让工具自动批准任何状态。

每条准入记录包含 manifest ID、候选路径、content/domain、P0/P1、`intake_eligible`、许可证和哈希证据、ImportVerified 证据、允许任务、禁止范围、已知缺口、LocalDevelopmentReady 前置证据和责任任务。目录由只读审计工具生成，重复运行结果稳定。

## 4. P0 覆盖与真实缺口

| P0 方向 | v2 准入结论 | 后续责任 |
|---|---|---|
| 圣玛格丽特地形、植被、岩石、材质 | 有 ImportVerified 且证据完整的环境/材质候选，可受控用于工作场景 | D19-A01—A03，之后 D18-S03 |
| 码头、建筑、港口道具 | 有可准入的环境/材质候选；船港候选中仍有 Candidate 包，不得越级使用 | D19-A03—A06，之后 D18-S03 |
| 玩家与关键 NPC | 现有已导入候选可作为工作场景参考；没有女性/时代中性完整基体的证明 | D20-A01、D20-A03 |
| 动物与鱼 | 已导入部分候选可受控使用；鸟仍为源文件未导入，猫缺候选，不能冒充完整动物覆盖 | D20-A05、D20-A06 |
| 起始船、舵、帆、锚 | 既有 `asset_ph_ship_fishing` 不被替换；船港候选仍为 Candidate，缺独立舵/帆/锚及消费者证据 | D21-A01—A03 |
| 海水、天气、Shader、灯光、后处理 | 没有外部模型候选；目录明确为项目自有表现资产，不伪造为外部资源已就绪 | D19-A04、D19-A05、D21-A02 |
| 核心 UI、材质与音频 | UI、材质、音频有 ImportVerified 准入候选；正式 Canvas/Mixer/运行时消费者仍待 D22 | D22-A01—A07 |

准入的 P0 规则是“有真实导入候选，或有明确的项目自有表现资产责任任务”。因此缺少外部候选的海水/天气/Shader/灯光由 `D19-A04 / D19-A05` 负责创建和记录所有权；既有 `asset_ph_ship_fishing` 继续作为起始船权威资产，由 D21 保持稳定 ID 并在 S03 复验；缺少女性角色、鸟类、猫狗、独立船锚/舵/帆的缺口被保留并登记到责任任务。没有使用错误资源凑数，也没有把 Candidate 或 NotImported 改成准入。

9 个 manifest 被排除：8 个仍为 `Candidate`（角色、鱼、动物、船港等包），以及 OpenGameArt Bird 同时为 `Candidate` 和 `NotImported`。排除清单、文件数和原因保存在准入目录，不删除源证据。

## 5. D19—D24 依赖修复

新的顺序为：

```text
D18-S02-v2 PASS
  ├─ D19—D22 各自创建受控工作场景、Prefab、材质、动画、UI 与项目自有表现资产
  ├─ D19-S02 / D20-S02 / D21-S02 / D22-S02 保留真实运行时证据
  └─ D18-S03：汇总运行时消费者与 LocalDevelopmentReady 晋级验收
       └─ D23 及后续最终集成门禁
```

D19—D22 的工作场景只能使用 `d18_prefab_candidate_intake_catalog.json` 中 `intake_eligible=true` 的外部资源；项目自有水体、Shader、灯光等只能由其责任任务创建，并记录来源与所有权；未准入候选不得直接进入正式场景。D19-S01 在本 v2 通过后解锁为“可重新执行的布局/工作场景冻结任务”，不自动解锁正式 InitialHarbor 集成；此前 D19-S01 BLOCKED 历史仍保留。

### D18-S03：运行时消费者与 LocalDevelopmentReady 晋级验收

D18-S03 是从旧版 S02 移出的运行时门。它必须等待 D19—D22 的 Prefab、工作场景、正式集成和 PlayMode 回归证据，并实际检查：Prefab/工作场景引用、碰撞、锚点、LOD、Animator（适用时）、运行时消费者、交互、保存/恢复、截图、性能和回退入口。只有这些证据真实存在且通过，才允许更新 `d18_local_development_ready_catalog.json` 中的 `LocalDevelopmentReady`；任何工具或 Agent 都不能代签 `ReleaseApproved`。D23 及后续最终集成明确依赖 D18-S03 PASS，而不是旧版 S02 的运行时证据。

## 6. 未改变的边界

- 未修改 25 个 manifest 的真实状态、Registry、`asset_state`、Prefab、Unity 场景、InitialHarbor、Systems、存档、contracts、server、web 或构建/发布配置；
- 未下载、删除、复制资源，也未加入新的 Git 历史；
- 未将任何资源写入正式运行时消费者；
- `d18_local_development_ready_catalog.json` 仍为 `BLOCKED`，`local_development_ready_count=0`、`release_approved_count=0`；
- D16 人工 `ReleaseApproved`、目标设备和发布阻断保持原状。

## 7. 验证

本轮新增的准入审计工具是只读的，写入范围仅限本任务准入目录和机器结果：

```text
python tools/d18_prefab_candidate_intake_audit.py --project-root . --write-catalog --catalog docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json --output game/tools/results/D18/D18-S02/D18_S02_V2_MACHINE_RESULT.json
  -> PASS; 25 manifests; 1,857 candidates; 1,674 intake-eligible; 16 eligible manifests; 9 excluded; 0 errors

python -m unittest tools.tests.test_d18_prefab_candidate_intake -v
  -> 3/3 PASS

python tools/d18_asset_manifest_audit.py --project-root . --manifest <each of the 25 actual *.manifest.json> --output game/tools/results/D18/D18-S02/manifest_audits_v2/<name>.json
  -> 25/25 PASS; 0 errors; 0 issues

python -m json.tool docs/game/design/common/assets/d18_prefab_candidate_intake_catalog.json
python -m json.tool docs/game/design/common/assets/d18_local_development_ready_catalog.json
python -m json.tool game/tools/results/D18/D18-S02/D18_S02_V2_MACHINE_RESULT.json
  -> all valid JSON

git diff --check
  -> PASS
```

Unity PlayMode 和运行时消费者测试不属于 v2 准入门；它们必须由 D19—D22 产生并在 D18-S03 复验。本报告因此没有把“未运行”写成“已通过”。

## 8. 后续入口

1. 允许重新执行 D19-S01，并启动 D19—D22 的受控工作场景/Prefab 任务；
2. 各任务只从准入目录取外部资源，并在自己的工作场景留下真实导入、Prefab、碰撞、锚点、LOD、动画、交互、保存和回退证据；
3. D18-S03 汇总证据并决定 `LocalDevelopmentReady` 晋级；
4. D23 及后续最终集成等待 D18-S03 PASS；
5. 继续保留所有 Candidate、NotImported、未映射和授权/历史审查风险，不以数量下降代替证据完成。
