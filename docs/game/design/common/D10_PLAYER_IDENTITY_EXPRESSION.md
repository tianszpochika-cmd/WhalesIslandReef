# D10 玩家身份表达

任务：`D10-A04`  
状态：已实现，可交给 D10-S01 集成  
契约边界：完整身份是单机本地权威的 game-only 存档实体；不进入 server/web，不新增私有协议。

## 1. 所有权与边界

`D10PlayerIdentitySystem` 是 `Character/Identity` 边界内的玩家身份表达、身份历史和证件记录唯一写者，路径为：

`game/Assets/Scripts/Systems/D10/D10PlayerIdentitySystem.cs`

保存层只负责把身份快照写入现有 SQLite 的 `players.location_json.identity`，不裁决任何领域结果。`PlayerState.Name` 仍保留为 D7/D8 兼容镜像，保存时以 `PlayerState.Identity.Name` 为准。

身份系统不拥有以下数据，也不复制其数值：

- 法律状态、通缉、许可和处罚；
- 债务、本金、利息、现金和信用；
- 各势力声望数值；
- CrewSystem 的关系边、好感、信任、忠诚和恋爱状态；
- AppearanceSystem 的服装所有权、耐久、价格和装备裁决。

身份历史只保存上述领域提供的不可变引用，例如 `legal:case:old_identity`、`debt:ledger:captain`、`relation:crew:old_name`。运行时字段 `PreservedHistory` 采用 merge-only 规则，永远不清空这些引用。

## 2. 创建数据

创建入口为 `SaveSystem.NewGame(NewGameParams.Identity)`，旧调用只传 `CaptainName` 时自动生成默认身份。

| 字段 | 说明 | 存档字段 |
|---|---|---|
| 性别表达 | 稳定表达 key，不把表达推导为法律结果 | `genderExpressionKey` |
| 姓名 | 可保存的玩家显示名；`PlayerState.Name` 是兼容镜像 | `name` |
| 称谓 | 中英文 key/text，支持本地化 | `title.key/zh/en` |
| 年龄段 | 只保存年龄段 key，不要求玩家提交精确年龄 | `ageBandKey` |
| 文化出身 | 文化来源 key，供表现和社会系统读取 | `cultureOriginKey` |
| 代词 | 可选稳定 key 列表 | `pronounKeys[]` |
| 长文本 | 身份备注/背景使用 `D10LocalizedText`，上限 65535 字符 | `identityNote.key/zh/en` |
| 初始历史 | 创建动作作为第一条历史事实 | `identityHistory[]` |

代码字段名对应：`GenderExpressionKey`、`AgeBandKey`、`CultureOriginKey`、`IdentityHistory`、`PreservedHistory`。

代表样例：

```json
{
  "schemaVersion": 1,
  "playerId": "player_example",
  "name": "Marisol de la Cruz",
  "genderExpressionKey": "gender_expression_androgynous",
  "pronounKeys": ["pronoun_key_they"],
  "title": {"key": "title.chart_keeper", "zh": "海图保管人", "en": "Chart Keeper"},
  "ageBandKey": "age_band_mature_adult",
  "cultureOriginKey": "culture_origin_afro_caribbean",
  "identityNote": {"key": "identity.note.long", "zh": "本地化长文本", "en": "Localized long text"},
  "identityHistory": [{"changeKind": "creation", "changeReasonKey": "identity.created"}],
  "documents": []
}
```

## 3. 港口检查与修改

`InspectAtPort(isAtPort, locationRef)` 只返回检查快照和权限提示：

- 海上：`CanEditTitle=false`、`CanEditAppearance=false`、`CanRecordDocument=false`；
- 港口：三项入口可用，但仍需通过各自 owner 的验证；
- 检查结果同时报告身份历史、证件和四类历史引用数量。

`ApplyPortUpdate` 只接受以下意图：

1. 追加新的本地化称谓；
2. 通过 `D10AppearanceOwnerWriter` 调用 `AppearanceSystem.Equip`，外观所有权、文化/职业限制和耐久仍由 AppearanceSystem 裁决；
3. 追加证件记录（类型、签发者、状态、世界日和本地化备注）。

每次成功修改都会追加 `identity_history_NNNN`，并记录变更前后的称谓/外观引用和当时仍保留的法律、债务、声望、关系引用。姓名、性别表达、年龄段、文化出身不会被港口改称谓操作隐式覆盖。

拒绝条件包括：

- `port_access_required`：不在港口；
- `identity_update_empty`：没有任何修改意图；
- `invalid_title` / `invalid_document`：字段不完整、超长或日期非法；
- `appearance_owner_unavailable`：未绑定 AppearanceSystem；
- `unknown_appearance_loadout`：外观内容不在已有 AppearanceSystem 目录或不满足其 owner 前置。

失败在所有状态修改前返回，不产生半写入。

## 4. 存档与旧档

身份数据写入现有 `players.location_json.identity`，不新增第二个 SQLite 数据库或私有存档路径：

- `SaveSystem.SaveD10Identity` 校验 `playerId` 与当前玩家一致后落库；
- `SaveSystem.LoadD10Identity` 返回深拷贝，旧 D7/D8 存档缺少身份块时按原 `players.name` 建立默认身份；
- D5 的外观快照继续写入既有 `location_json.d5.appearance`，两者只通过外观 loadout 引用联系；
- 法律、债务、声望和关系仍由各自快照/事件事实恢复，身份读档不重算也不删除它们。

## 5. 测试边界

Unity EditMode：`game/Assets/Tests/EditMode/D10PlayerIdentityTests.cs`

- 创建字段、称谓本地化和 4000 字符长文本保存/读档；
- 海上/港口检查权限；
- 港口修改称谓、外观、证件和历史追加；
- 修改后法律/债务/声望/关系引用仍存在；
- 海上访问、未知外观、非法证件拒绝且无部分写入；
- `SaveD10Identity` / `Load` 恢复姓名、表达、历史和证件。

纯 C# smoke：`tools/d10_a04_smoke/`，覆盖创建、港口门禁、修改、长文本、历史保留和拒绝路径。
