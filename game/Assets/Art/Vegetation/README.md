# D15-A02 植被资产配置

本目录登记 D15-A02 的可替换植被表现配置。批量条目通过 `asset_vegetation_profile_*` 引用共享配置，避免为 1,000—2,000 个内容 ID 复制同一套网格。

当前资产状态：

- 模型使用 `unity-primitive://vegetation/<plant_class>/lod2` 占位引用，状态为 `placeholder_primitive_ready`；它对应仓库约定的 Unity 原生几何体降级方案，不宣称最终美术模型已完成；
- 材质复用 `Assets/Art/Placeholder/Materials/ph_mat_*.mat`，状态为 `source_ready`；
- LOD 采用 `plant_lod2`：LOD0/LOD1/LOD2 上限为 6,000/2,000/600 三角面，远景使用群落代理；
- 近景采集碰撞使用共享交互代理，运行时资源数量、成长、采集和恢复仍由 `EcologySystem` / D11 owner 裁决。

占位资源来源和授权见 `game/Assets/Art/Placeholder/PH_ASSET_SOURCES.md` 与 `PH_QUATERNIUS_LICENSE.txt`。本批次没有新增网络下载或第三方运行时依赖。
