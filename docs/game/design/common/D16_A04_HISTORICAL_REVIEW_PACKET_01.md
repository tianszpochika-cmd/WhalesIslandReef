# D16-A04 第一批史料研究包（不得直接批准）

日期：2026-08-13  
范围：9 条 `historical_tag=historical` 的 Pending 内容  
状态：`RESEARCH_READY / HUMAN_DECISION_REQUIRED`

> 本文件是给责任历史/内容审核者的研究准备材料，不是批准决定，也不是游戏数值依据。
> 研究包不能替代逐条审核；在责任审核者写入决定前，Registry 必须保持 `historical_review=Pending`。

## 共同结论

- 9 条记录均为项目自创内容，当前 `license.status=internal-authored`，但历史标签的真实性、1675 年加勒比语境和表现方式仍未由责任审核者确认。
- 公开博物馆/植物学资料可以证明“某类物品或植物在某个时代/地区存在”，不能自动证明本项目的具体名称、外观、用途、加勒比传播路径或玩法表达。
- 本包不修改 `historical_tag`、`historical_review`、`authorization`、`asset_state`、价格、伤害、奖励或任何系统数值。

## 逐条研究表

| ID | 当前内容 | 研究线索 | 目前能支持的范围 | 仍必须人工决定 |
|---|---|---|---|---|
| `item_firearm_matchlock_carbine` | 火绳枪卡宾枪 | The Met 的火绳枪藏品（晚 16—早 17 世纪）；The Met 的 17 世纪护具条目提到卡宾枪/火绳枪语境 | 火绳点火枪械和卡宾枪类别在早期近代欧洲语境有博物馆记录 | 是否适用于 1675 年加勒比、具体型号/称谓、是否需要武器警告与低强度表现；不得由本包推导伤害/弹药数值 |
| `item_toxin_datura` | 曼陀罗毒性植物货物 | Kew Plants of the World Online 的 Datura 物种记录 | Datura 的毒性、药用/毒物用途和部分加勒比引入分布有植物学记录 | 项目物种是否对应 1675 年港口、名称和用途是否应保留；必须明确警告、不可误食表达和跳过策略 |
| `outfit_hat_straw_deck` | 甲板草帽 | The Met 的英国 17 世纪帽藏品 | 17 世纪草/草编帽存在实物记录 | 是否能外推到加勒比船员、帽型/材质是否准确、是否为 `inferred` 而非 `historical` |
| `outfit_bottom_canvas_trousers` | 帆布长裤 | RMG 的水手长裤藏品为 19 世纪早期 | 只能说明后世水手服装存在帆布/条纹长裤样本，不能证明 1675 年 | 是否降级为 `inferred`/`fictional` 或继续 Pending；不能用 19 世纪样本直接批准 1675 年设定 |
| `outfit_career_fisher_handline` | 渔夫手线装备 | 当前仅有项目设计文档，没有已核验的 1675 年加勒比一手/博物馆来源 | 暂无可引用的外部历史结论 | 需要渔业史料或明确改为项目推定；应确认装备名称、使用方式和表现等级 |
| `outfit_coat_rain_oilskin` | 雨天油布/油皮外套 | 当前仅有项目设计文档；本批没有找到足以证明 1675 年加勒比船员使用该具体形式的来源 | 暂无足够的时间/地区证据 | 是否改名、改标签或保持 Pending；不能把后世油布服装外推成 1675 年事实 |
| `outfit_gloves_ropework` | 绳作手套 | 当前仅有项目设计文档 | 暂无足够的 1675 年加勒比船员证据 | 是否是功能性推定而非史实；需要材质、使用和可见性依据 |
| `outfit_shoes_deck_boots` | 甲板靴 | The Met 有 18 世纪英国 jackboots，但不是 1675 年甲板靴证据 | 早期近代皮靴/防水处理的宽泛语境可查 | 具体靴型、船员使用和 1675 年适配；不能把 18 世纪骑乘靴直接当作甲板靴批准 |
| `outfit_top_linen_deckhand` | 亚麻甲板水手上衣 | RMG 的非制式水手衫条目为 1807 年 | 后世海员服装中有棉/亚麻衫记录 | 是否有 1675 年前后更直接来源；需确认称谓、剪裁、地区和 `historical/inferred` 标签 |

## 已核验的外部研究入口

以下链接仅作为研究入口；链接存在不等于对应 Registry 条目已经通过审核。

1. [The Metropolitan Museum of Art：Matchlock Gun](https://www.metmuseum.org/art/collection/search/33874) —— 晚 16—早 17 世纪火绳枪藏品。
2. [The Metropolitan Museum of Art：Buff Coat](https://www.metmuseum.org/art/collection/search/23347) —— 17 世纪护具条目中的卡宾枪/火绳枪语境。
3. [Kew：Datura stramonium，Plants of the World Online](https://powo.science.kew.org/taxon/urn%3Alsid%3Aipni.org%3Anames%3A314738-2/general-information) —— 物种、毒性和用途研究入口。
4. [The Metropolitan Museum of Art：17 世纪 British Hat](https://www.metmuseum.org/art/collection/search/81227) —— 17 世纪帽类实物记录。
5. [Royal Museums Greenwich：Rating's trousers](https://www.rmg.co.uk/collections/objects/rmgc-object-71306) —— 只能作为后世水手裤对照，条目年代为 19 世纪早期。
6. [The Metropolitan Museum of Art：Jackboots](https://www.metmuseum.org/art/collection/search/157647) —— 18 世纪靴类对照，不是 1675 年甲板靴证明。
7. [Royal Museums Greenwich：Non-regulation shirt](https://www.rmg.co.uk/collections/objects/rmgc-object-71295) —— 条目年代为 1807 年，只能作为后世海员上衣对照。

## 责任审核者填写区

每条记录必须单独填写以下字段后，才能使用 `tools/d16_a04_apply_review_decision.py` 写入 Registry：

```text
reviewer_id: <责任审核者>
decision: Approved | ApprovedWithWarning | Rejected | Pending
historical_basis: <具体史料、时间、地区和不确定性，至少 20 字>
presentation_level: Full | Contextual | LowIntensity | Omitted
warning_ids: <如适用>
skip_allowed: true | false
review_date: YYYY-MM-DD
```

本批建议默认保持 `Pending`，直到完成“1675 年 + 加勒比 + 具体物品/服装 + 游戏表现”的逐条核验。任何 `ApprovedWithWarning` 都必须同时确认警告、低强度/跳过路径和本地化文案；任何决定都不能写玩法数值。

## 可复核的当前事实

- 该包对应的 9 条记录仍处于 Pending；没有自动批准。
- 当前完整 `game` 根目录 A04 审计仍为 3,139 条记录、结构错误 0、Pending 267。
- 本研究包不能作为“人工审查已完成”的替代证据；它只把第一批审核问题和来源入口整理成可交接材料。
