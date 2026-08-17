# D18 二进制与导入延期审计（D18-L48）

> 任务：`D18-L48`（波次 4，第 2 项；前置 `D18-L47=LOGIC_PASS`）
> 审计时间：2026-08-16 23:41（UTC+8）
> 性质：**只读审计**。确认本轮 D18 逻辑阶段（L01—L47）没有下载/导入外部二进制、没有创建/编辑 Prefab、场景、Canvas、Build Settings、Player 或运行时 Registry；并列出未来 Runtime 阶段必须验证的项目。

## 1. 审计方法（只读命令）

| 检查 | 命令 | 结果 |
|---|---|---|
| 工作区变更类型分布 | `git status --porcelain` + 扩展名统计 | 仅 `.md`(70) / `.py`(57) / `.json`(4) / 目录(54)；**无任何二进制扩展名** |
| 禁入文件类型 | 过滤 `.fbx/.png/.jpg/.zip/.rar/.7z/.blend/.gltf/.obj/.tga/.ogg/.wav/.prefab/.unity/.unitypackage/.mat/.mp3/.exe/.dll` | **0 命中** |
| L 系列结果目录 | `game/tools/results/D18/D18-L*/` 全文件扫描 | 61 个文件全部 `.json`，零二进制 |
| 禁区路径 | `InitialHarbor` / `Systems` / `Prefabs` / `.unity` / `contracts` | git 无改动 |
| 未跟踪目录明细 | 无扩展名条目逐项核验 | 54 项全部为目录（结果/夹具目录），非文件 |

## 2. 越界写入检查结论

- **未发现任何越界写入**；本轮逻辑阶段交付物类型限定为：Markdown 文档、Python 只读工具/测试、JSON 数据（Schema/夹具/机器结果/目录）。
- 历史说明：`game/Assets/Art/Candidates/` 下的外部候选二进制（`.fbx/.png/.ogg` 等，共 2,124 个非 .meta 文件）是 **D18-A/S/R 历史基线（SUPERSEDED）** 的已提交内容，本轮 L 系列未新增、未修改、未导入它们。
- `game/tools/results/D18/D18-A0x` 下的 Unity 导入 fixture（`_UnityAuditFixtureProject`、`UnityAppearanceFixture` 等）同为历史 SUPERSEDED 证据，且已被 `.gitignore` 排除，非本轮产物。

## 3. 未把"静态记录完整"表述为"资源可用"

- D18 逻辑阶段的产出全部是**静态数据与只读工具**：来源目录（161 条记录）、L42 编译产物（100 合法 + 61 rejected + 4 Pending）、Schema、验证器、审计报告；
- 任何 `IntakeEligible` 只表示"证据完备可受控使用"，不表示导入成功、Prefab 可用、运行时可用或发布可用（L01 基线 §2.1 / L03 状态机冻结语义）；
- `RuntimeApproved` / `ReleaseApproved` 持有者均为 0；`represents_imported_asset` 恒为 false。

## 4. 未来 Runtime 阶段必须验证的项目（延期清单）

### 4.1 模型与网格
- 真实顶点/三角面/网格读取（本阶段只验证描述器声明，`D18-L26` 明确延期）
- 尺度单位换算（cm/inch/feet → 米）与轴向变换（Y-up/Z-forward 之外的声明轴）
- 网格数量级与 bounds 实测比对（L26 `bounds_ratio_suspect` P1 项）

### 4.2 贴图与材质
- 颜色空间、压缩、mipmap、导入预设（L27 只做描述器静态验证）
- PBR 贴图组（albedo/normal/roughness/ao）完整性（L23 polyhaven 目录 91 条记录）
- 4 条 HDRI 记录 `sha256=null` 的逐文件补证（L06/L31 对真实文件计算）

### 4.3 骨架与动画
- 骨架类型验证（humanoid/generic/animal/fish/static，L28 描述器语义）
- Humanoid Avatar 装配、骨骼映射、root-motion 提取（`root_motion=required/derived` 需真实 clip）
- 动画 clip 实际提取与时长/循环验证

### 4.4 碰撞
- 碰撞体生成与验证（`collision_intent`：capsule_agent/convex_mesh/simple_box/terrain）
- 交互锚点、角色/船/动物碰撞边界实测

### 4.5 LOD
- LOD 生成/烘焙（`generate_automatically` 声明）与切换距离
- 高/中/低档几何验证（1—5 级）

### 4.6 UI 与音频
- Canvas 消费、图集、输入提示图标、字体（OFL 政策已静态审计，实际渲染延期）
- AudioMixer/音量/播放验证、字幕键渲染（38 个 audio.* 本地化键已静态核对）

### 4.7 性能与整体
- FPS/1% Low/加载时间/显存/质量档（旧 R03 性能样本仅作历史诊断，不继承计数）
- 保存/恢复、输入路线、截图与人工视觉验收
- 最终逐文件 SHA-256 完整性校验与发行包审计

## 5. 结论

- 本轮 D18 逻辑阶段**无二进制、无 Prefab/场景/Canvas/Build Settings/Player/运行时 Registry 改动**；
- 静态治理产出完整（45/45 任务证据矩阵全绿，见 [D18-L47 证据矩阵](../../../../game/tools/results/D18/D18-L47/D18_L47_EVIDENCE_MATRIX.json)）；
- 所有资源级验证按 §4 延期至 Runtime 阶段；本审计不产生任何 Release/Runtime 通过结论。

## L53 收尾复核（2026-08-17）

- L51—L53 收尾变更复核：git 变更仅 .md/.py/.json（Schema、细分类映射、来源目录迁移、D19LightingProfileCatalog 配置键修复 5 行、工具与测试、文档）；零二进制、零 .unity/Prefab/Canvas/Build Settings/Registry 改动。
- L52 对 game/Assets/Settings/D19/Lighting/D19LightingProfileCatalog.json 的修改为授权配置键修复（candidate_hdri_* → asset_hdri_*），不改变天气/颜色/数值/许可证/资源状态。
