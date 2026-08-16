# D18-A04-R02-B validation record

- JSON Schema: `python` + `jsonschema.Draft202012Validator` against `tools/schemas/d18_asset_pack_manifest.schema.json`: **PASS** (`SCHEMA_PASS`).
- Result JSON parse: all `*.json` under this result directory parsed: **PASS** (`JSON_SCHEMA_AND_RESULT_JSON_PASS`).
- Manifest audit: `python tools/d18_asset_manifest_audit.py --project-root . --manifest docs/game/design/common/assets/packs/characters_animals/gap_fill/bird/quaternius_animal_pack_vol2_bird_d18_a04_r02_b.manifest.json --output game/tools/results/D18/D18-A04-R02-B/D18_A04_R02_B_MANIFEST_AUDIT.json`: **PASS**, 1 manifest, 0 errors, 0 issues, read-only.
- Unity import audit: Unity `6000.0.80f1` report-only `RunKenneyAudit`: **PASS**, 0 errors, 1 naming warning; 1 FBX, 1 mesh, 4 materials, 3 clips. Detail dump confirms `Generic/NoAvatar`, Animator 0, valid bounds, Renderer 1.
- Candidate duplicate hash check: **PASS**, no duplicate SHA-256 among candidate files.
- `git diff --check`: **PASS**, exit code 0.
- Unity audit fixtures were moved to the OS temporary directory after evidence was copied; no fixture project remains in the repository result directory.
