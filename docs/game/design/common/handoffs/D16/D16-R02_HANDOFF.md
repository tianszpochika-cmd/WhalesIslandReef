# D16-R02 安装更新包交接报告

## 任务信息

- 任务编号：D16-R02
- 日期：2026-08-12
- 状态：PARTIAL / IN_PROGRESS
- 归属：D16-A06 发布技术准备
- 外部动作：未上传、未发布、未发送外部消息；Steam 未执行

## 本次完成

- 加固 `tools/d16_a06_install_update.ps1` 的更新事务：新 payload 先进入隔离 staging，旧安装移入 previous；新版本切换失败时恢复 previous 并清理半成品。
- `Install` 遇到已存在安装目录时拒绝覆盖，避免把首次安装误用成更新。
- 新增 `tools/tests/test_d16_a06_install_update_script.py`，真实调用 Windows PowerShell 覆盖：首次安装、覆盖更新、已有安装拒绝、卸载和用户存档保留。
- `tools/d16_a06_install_update_smoke.py` 重新验证工作区 staging：安装 180 文件、更新 180 文件、卸载后存档哨兵保留。
- `tools/d16_a06_release_audit.py` 现在分别报告工作区部署脚本和正式 Windows 安装器产物/烟测，阻止 staging 被误报为 OS installer。

## 验证结果

| 检查 | 结果 |
|---|---|
| PowerShell 安装更新合同测试 | 1/1 PASS |
| D16-A04/A06 专项测试 | 13/13 PASS |
| 全量工具测试 | 49/49 PASS |
| workspace Install/Update/Uninstall | PASS |
| 正式 Windows installer 产物 | 未发现 |
| 正式 installer 安装/卸载/更新烟测 | 未执行 |
| Steam/外部发布 | 未授权、未执行 |

## 边界与阻塞

本任务只操作 `artifacts/` 下的工作区隔离路径和发布工具代码，不改变领域实体、存档 schema、契约、Systems 逻辑或网络行为。当前环境未发现可直接运行的 Inno Setup/WiX 工具，因此没有创建未验证的 MSI/MSIX/签名 Setup。

发布审计当前应保持：

- `workspace_deployment_script_present=true`
- `formal_installer_artifact_present=false`
- `formal_installer_smoke_evidence=false`
- 阻塞：`formal_os_installer_missing`、`steam_upload_not_authorized`

D16-A06、D16-S01、D16-S02 仍为未完成；目标设备、原生崩溃、人工隐私、A04/A05 等其他门禁也不能由本任务代替。

## 2026-08-12 当前事实刷新

此前“正式 installer 产物未发现”的内容属于早期检查点，保留用于历史追溯；当前工作区已经补齐并实际验证正式便携 Windows 安装器：

- 安装器：`artifacts/d16_a06_formal_installer/WhalesIslandReefSetup.exe`；
- 原始烟测：`tools/results/d16_a06_formal_installer_smoke_result.json`；
- 安装、非法更新拒绝且保留旧安装/用户存档、合法覆盖更新、卸载和用户存档保留均为 `true`；
- A06 当前审计：本机技术准备 **19/19 PASS**、`missing_static_evidence=[]`，正式发布清单仍为 `blocked`，保留 7 条项目级阻断。

因此 D16-R02 的准确状态为：**本机安装器子门禁 PASS，任务总体 PARTIAL / IN_PROGRESS**。仍未关闭目标机器兼容性、原生崩溃审查、最终人工隐私审查以及 Steam 未授权政策；本更新不把本机安装器烟测扩大解释为完整 A06 或 D16-S02 通过。
