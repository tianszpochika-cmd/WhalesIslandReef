# D23 Runtime Deferred 登记

> 对应任务：D23-L33
> 适用范围：D23 纯逻辑章节，不授予 `RuntimeApproved` 或 `ReleaseApproved`

## 1. 登记原则

本登记把纯逻辑门禁能够验证的内容与必须在真实运行时验证的内容分开。下列项目没有被 D23 runner 执行，也不能由编译、离线 smoke、固定夹具或结果 JSON 推断为通过。

## 2. Deferred 项

| 类别 | 明确延期内容 | 当前处理 |
|---|---|---|
| Unity runtime | Player、PlayMode、场景、Prefab、MonoBehaviour 生命周期 | 不启动；保持 Runtime Deferred |
| UI/输入 | Canvas、布局、分辨率、键鼠、手柄、触摸、真实 UI router 接线 | 不读取、不模拟；保持 Runtime Deferred |
| 视觉与音频 | 镜头、Transform、碰撞、动画、材质、音频、字幕/本地化 | 不采样；保持 Runtime Deferred |
| 设备与性能 | 真机、帧率、内存、加载、长时间运行、平台差异、Steam | 不测量；保持 Runtime Deferred |
| 外部系统 | 网络服务、LLM/Agent runtime、RTC、在线依赖 | 不连接；保持 Runtime Deferred |
| 真实存档 | SaveSystem 真实介质、文件/云同步、崩溃恢复、版本升级实介质 | 本章只验证 D19/D21/D8 projection/snapshot round-trip |

## 3. 与本章通过条件的关系

L26–L38 的 `LOGIC_PASS` 只表示固定输入下的纯逻辑 owner API、失败不变式、幂等、确定性回放、长跑和 projection 证据通过。Deferred 项不是本次逻辑结果的隐藏 blocker，但它们是后续 Runtime/Release 验收的必需工作；本登记不允许把任何 Deferred 项写成 `PASS`。

## 4. 复核入口

运行报告 `game/tools/results/D23/D23-L31/D23_L31_RUNNER_REPORT.json` 的 `scope` 字段应保持以下值为 `false`：

```text
unity_player_playmode_executed=false
scene_prefab_canvas_input_executed=false
network_llm_executed=false
performance_sampling_executed=false
```
