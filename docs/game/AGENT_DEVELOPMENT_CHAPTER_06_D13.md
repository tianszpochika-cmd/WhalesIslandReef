# 第 6 章开发任务提示词：D13 经济、资产、金融、种植与岛屿经营

> 本章前置：D10-S01、D11-S01、D12-S01 全部通过。执行顺序：`D13-A01—D13-A07` 可按模块并行，全部完成后执行 `D13-S01`。

## D13-A01：产权与资产交易

<details>
<summary>展开/复制：D13-A01 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D13-A01。

任务编号：D13-A01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-S01、D11-S01、D12-S01 必须全部通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D8 数据/事件成果、D10-D12 验收报告和当前 Island/Property/Economy 代码。

执行内容：实现岛屿、地块、房屋、仓库、工坊、酒馆、船坞、农地、渔场的产权调查、估值、购买、租赁、共同治理、抵押、出售、继承/转让和争议。

任务约束：
- 产权状态由资产系统唯一写入；
- 价格/估值由经济系统裁决；
- 法律归属和争议由法律/势力系统裁决；
- UI 只能发起交易请求；
- Agent 不得直接改变所有权、估值或金钱；
- 不得在 Systems 中加入网络或 LLM。

验收：玩家可调查、购买、租赁、抵押、出售或转让资产，争议和法律状态可保存，所有交易有事件和来源。

结果必须保存到：
- 主成果：产权/资产交易代码、测试，以及 docs/game/design/common/D13_PROPERTY_ASSET_MARKET.md；
- 交接报告：docs/game/design/common/handoffs/D13/D13-A01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D13-A02：金融、信用、贷款与保险

<details>
<summary>展开/复制：D13-A02 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D13-A02。

任务编号：D13-A02
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-S01、D11-S01、D12-S01 必须全部通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D13-A01 产权成果、D8 配置/事件/存档成果和当前 Finance/Economy 代码。

执行内容：实现现金、赊账、贷款、担保、利息、到期、展期、保险、证据、拒赔、违约、催收、资产查封和破产保护。金融 UI 必须解释本金、利息、风险和最坏结果。

任务约束：
- FinanceSystem 唯一写入账户、债务、利息和保险状态；
- UI 不能直接改钱、利息、债务或赔付；
- Agent 只能提出谈判或申请，不能裁决金融结果；
- 金融计算必须使用统一配置和可追溯事件；
- 必须明确风险和最坏结果，不得隐藏费用；
- 不得在 Systems 中加入网络或 LLM。

验收：贷款、利息、展期、保险、拒赔、违约、催收、查封和破产保护可保存、可解释、可测试。

结果必须保存到：
- 主成果：金融系统、测试，以及 docs/game/design/common/D13_FINANCE_CREDIT_INSURANCE.md；
- 交接报告：docs/game/design/common/handoffs/D13/D13-A02_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D13-A03：农地、果园与蔬菜种植

<details>
<summary>展开/复制：D13-A03 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D13-A03。

任务编号：D13-A03
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-S01、D11-S01、D12-S01 必须全部通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11 生态/植被成果、D13-A01 产权成果、D8 配置/内容 ID 成果。

执行内容：实现地块、种子、土壤、水源、季节、播种、浇水、劳力、病虫害、风暴、收获、轮作、加工和销售。果实、蔬菜、甘蔗、烟草、咖啡、可可、香料和药草使用不同标签。

任务约束：
- 农业系统唯一写入作物生长和收获状态；
- EcologySystem 写土壤、水源、生态压力等环境状态；
- Inventory/Cargo 写种子、收成和加工品库存；
- UI 只能显示或提交操作请求；
- Agent 不得直接生成收成或修改价格；
- 不得在 Systems 中加入网络或 LLM。

验收：种植、浇水、病虫害、风暴、轮作、收获、加工和销售可保存，作物标签正确，生态后果可追踪。

结果必须保存到：
- 主成果：农业/果园/蔬菜代码、测试，以及 docs/game/design/common/D13_FARMING_ORCHARDS.md；
- 交接报告：docs/game/design/common/handoffs/D13/D13-A03_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D13-A04：牧场、家禽和基础养殖

<details>
<summary>展开/复制：D13-A04 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D13-A04。

任务编号：D13-A04
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-S01、D11-S01、D12-S01 必须全部通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D11 动物/宠物成果、D13-A01 产权成果和 D8 存档/内容 ID 成果。

执行内容：实现家禽/牲畜来源、饲料、饮水、健康、疾病、空间、产物、劳力和交易；控制繁殖复杂度，先实现可保存的单周期产出。

任务约束：
- 养殖系统唯一写入动物健康、产出和周期状态；
- 动物行为不得使用 LLM；
- UI 不能直接生成产物或改变健康；
- Agent 不能直接裁决繁殖、疾病或交易结果；
- 必须有来源、检疫和死亡处理；
- 不得在 Systems 中加入网络或 LLM。

验收：来源、饲料、饮水、健康、疾病、空间、产物、劳力和交易可用，单周期产出可保存，繁殖复杂度受控。

结果必须保存到：
- 主成果：牧场/家禽/养殖代码、测试，以及 docs/game/design/common/D13_RANCH_LIVESTOCK.md；
- 交接报告：docs/game/design/common/handoffs/D13/D13-A04_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D13-A05：岛屿治理和人口

<details>
<summary>展开/复制：D13-A05 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D13-A05。

任务编号：D13-A05
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-S01、D11-S01、D12-S01 必须全部通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D10 角色/宗教成果、D11 生态成果、D12 社群成果、D13-A01 产权成果。

执行内容：实现人口、家庭、职业、迁入迁出、健康、满意度、税、工资、供水、食物、教育、宗教、治安、政策、共同体协议和代理人管理。

任务约束：
- 人口、健康、满意度和治安必须有明确写者；
- 税、工资和政策不能由 UI 或 Agent 直接修改；
- 宗教、社群和历史内容必须带标签和审查状态；
- 人口状态必须与住房、食物、供水和工作岗位关联；
- 不得把岛屿治理做成无因果的数值面板；
- 不得在 Systems 中加入网络或 LLM。

验收：人口、家庭、职业、迁移、健康、满意度、税、工资、供水、食物、教育、宗教和治安会相互影响并可保存。

结果必须保存到：
- 主成果：岛屿治理/人口代码、测试，以及 docs/game/design/common/D13_ISLAND_GOVERNANCE_POPULATION.md；
- 交接报告：docs/game/design/common/handoffs/D13/D13-A05_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D13-A06：建筑、工坊、仓库和船坞

<details>
<summary>展开/复制：D13-A06 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D13-A06。

任务编号：D13-A06
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-S01、D11-S01、D12-S01 必须全部通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D13-A01 产权成果、D11 生态成果、D8 内容 ID/目录成果和当前 Building/Shipyard 代码。

执行内容：实现建设订单、材料、工期、工人、库存、损伤、开放时间、服务价格、产出、火灾、占领、维护和地图可见变化。

任务约束：
- BuildingSystem 唯一写入建筑状态、建设进度和损伤；
- Inventory/Cargo 写材料和仓库物品；
- EconomySystem 写服务价格；
- UI 只能提交建设/维护请求；
- Agent 不能直接完成建设、改变价格或消除损伤；
- 必须支持保存、读档、火灾、占领和维护；
- 不得在 Systems 中加入网络或 LLM。

验收：建设订单、材料、工期、工人、库存、损伤、开放时间、服务、火灾、占领和维护可见且可保存。

结果必须保存到：
- 主成果：建筑/工坊/仓库/船坞代码、测试，以及 docs/game/design/common/D13_BUILDINGS_WORKSHOPS_SHIPYARDS.md；
- 交接报告：docs/game/design/common/handoffs/D13/D13-A06_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D13-A07：动态市场与运输

<details>
<summary>展开/复制：D13-A07 完整提示词</summary>

```text
你是《鲸骨航线》的执行 Agent。请直接完成 D13-A07。

任务编号：D13-A07
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D10-S01、D11-S01、D12-S01 必须全部通过

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D9 贸易成果、D11 采集成果、D13 资产/农业成果和 D8 事件/配置成果。

执行内容：实现商品生产—运输—消费—库存—价格循环；支持鱼获、农产品、加工品、船材、服装、宠物用品、工具和特殊物品的合法经济规则。

任务约束：
- EconomySystem 唯一写入价格和市场状态；
- Inventory/Cargo 唯一写入库存；
- 运输系统写运输状态和损耗；
- UI 不能直接改库存、价格或运输结果；
- Agent 不能直接裁决市场结果；
- 商品生产和消费必须有事件和来源；
- 不得在 Systems 中加入网络或 LLM。

验收：生产、运输、消费、库存和价格循环可运行，天气/法律/生态/资产状态能产生合法影响，保存读档一致。

结果必须保存到：
- 主成果：动态市场/运输代码、测试，以及 docs/game/design/common/D13_DYNAMIC_MARKET_TRANSPORT.md；
- 交接报告：docs/game/design/common/handoffs/D13/D13-A07_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。
```
</details>

## D13-S01：经济与岛屿经营集成

<details>
<summary>展开/复制：D13-S01 完整提示词</summary>

```text
你是《鲸骨航线》的同步集成 Agent。请直接完成 D13-S01。

任务编号：D13-S01
项目根目录：C:\Users\Administrator\Desktop\game\first01
前置任务：D13-A01、D13-A02、D13-A03、D13-A04、D13-A05、D13-A06、D13-A07 全部完成

开始前读取：AGENTS.md、docs/CONTEXT.md、需求/设计文档、D13 各主成果文件和 docs/game/design/common/handoffs/D13/ 下全部交接报告。

集成验收流程：玩家买地/租地，建水井和仓库，种蔬菜，支付工人，生产并出售收成，用资产贷款，因风暴申请保险，出现税费/维护/生态后果，保存和读档后状态一致。

任务约束：
- 不得复制产权、金融、农业、建筑或市场逻辑；
- UI 只能读取状态和事件；
- Agent 只能提议，不能裁决所有权、贷款、收成、价格、税费或保险；
- 必须检查资产、生态、人口、法律和存档的一致性；
- 不得在 Systems 中加入网络或 LLM。

结果必须保存到：
- 主成果：docs/game/design/common/D13_ACCEPTANCE_REPORT.md；
- 集成测试：任务允许的测试目录；
- 交接报告：docs/game/design/common/handoffs/D13/D13-S01_HANDOFF.md；
- 记忆日志：.workbuddy/memory/YYYY-MM-DD.md。

最终结论必须明确 D13 是否通过、遗留问题和是否允许 D14 使用经济/资产数据。
```
</details>
