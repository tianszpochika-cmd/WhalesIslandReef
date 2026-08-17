# D18-L02 资源逻辑记录夹具（fixtures）

本目录是 `tools/schemas/d18_logic_asset_record.schema.json` 的固定校验夹具（D18-L02 唯一写入范围）。

## 声明

- 全部夹具为**合成数据**，`canonical_asset_id` 一律以 `asset_fixture_` 开头，来源为
  `example.invalid` 保留域；**不对任何真实候选补造数据**，也不宣称任何真实资产状态。
- `valid.json` 的 `sha256` 是空输入串的公开已知摘要
  `e3b0c442…2b855`（SHA-256 of empty input），仅用于满足 64 位十六进制格式，**不代表任何真实文件哈希**。
- 校验方式与仓库既有用法一致：Python `jsonschema.Draft202012Validator`（同 D18-A04-R02-B 验证记录）。

## 文件与预期结果

| 文件 | 预期 | 失败字段 |
|---|---|---|
| `valid.json` | VALID | —（13 个必填字段全部合法；state=Candidate） |
| `invalid_missing_license.json` | INVALID | 缺 `license`（required） |
| `invalid_bad_sha256.json` | INVALID | `sha256` 不匹配 `^[0-9a-fA-F]{64}$` |
| `invalid_state_jump.json` | INVALID | `state=ReleaseApproved` 缺 `human_reviewer`/`human_review_date`/`release_evidence_path`（条件 required，越级状态） |
| `invalid_empty_source.json` | INVALID | `source_id=""` 不匹配 `^source_[a-z0-9_]+$`；缺 `source_url`（空来源） |
| `invalid_absolute_path.json` | INVALID | `evidence_paths[0]` 为 Windows 绝对路径，含 `:`（路径越界） |
| `invalid_unknown_state.json` | INVALID | `state="Approved"` 不在七个冻结状态枚举内（未知批准状态） |
| `invalid_empty_content_ids.json` | INVALID | `content_ids` 为空数组（minItems 1） |
| `invalid_rights_incomplete.json` | INVALID | `rights` 缺 `distribution_with_game`（四项权利必须显式记录） |
| `valid_taxonomy_category.json` | VALID | `taxonomy_category="terrestrial_animal"`（Schema 层合法；语义映射由 L39/L42 工具校验，D18-L51 双层契约） |
| `invalid_taxonomy_category_format.json` | INVALID | `taxonomy_category` 不匹配 `^[a-z0-9_]+$`（D18-L51 双层契约） |

## 校验命令（可复跑）

```powershell
python -c "import json;from jsonschema import Draft202012Validator;s=json.load(open('tools/schemas/d18_logic_asset_record.schema.json',encoding='utf-8'));Draft202012Validator.check_schema(s);print('schema ok')"
python -c "import json,glob;from jsonschema import Draft202012Validator;s=json.load(open('tools/schemas/d18_logic_asset_record.schema.json',encoding='utf-8'));v=Draft202012Validator(s);[print(p, 'VALID' if v.is_valid(json.load(open(p,encoding='utf-8'))) else 'INVALID') for p in sorted(glob.glob('tools/tests/d18_logic_asset_record_fixtures/*.json'))]"
```
