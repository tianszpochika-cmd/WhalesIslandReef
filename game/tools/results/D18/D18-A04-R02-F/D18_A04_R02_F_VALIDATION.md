# D18-A04-R02-F validation log

Capture date: 2026-08-15

## Executed checks

1. Archive hash:

```powershell
Get-FileHash -Algorithm SHA256 artifacts/external_sources/Animated-Women-Pack-HHSKxnk1mY-zip--1745939261.zip
```

Observed SHA-256: `BCC6170D0175388FDF9AC7A3FC1E59B9250178E913B94B0D374B4795A1311C9F`.

2. Read-only FBX structure audit:

```powershell
python game/tools/results/D18/D18-A04-R02-F/d18_a04_r02_f_fbx_static_audit.py `
  --root game/Assets/Art/Candidates/CharactersAnimals/GapFill/FemaleNeutral/Models `
  --output game/tools/results/D18/D18-A04-R02-F/D18_A04_R02_F_FBX_STATIC_AUDIT.json
```

Result: PASS, 4/4 source FBX files parsed without audit errors. This is not a Unity import test.

3. Manifest audit:

```powershell
python tools/d18_asset_manifest_audit.py `
  --project-root . `
  --manifest docs/game/design/common/assets/packs/characters_animals/gap_fill/female_neutral/d18_a04_r02_f.manifest.json `
  --output game/tools/results/D18/D18-A04-R02-F/D18_A04_R02_F_MANIFEST_AUDIT.json
```

Result: PASS, 0 issues. The audit is read-only and did not touch Registry or asset state.

4. Cache check:

```powershell
git check-ignore -v --no-index artifacts/external_sources/Animated-Women-Pack-HHSKxnk1mY-zip--1745939261.zip
```

Result: `.gitignore:68` matches the archive. The uncompressed inspection copy was used only as a local extraction workspace; the four candidate FBX files in the task directory are the intentional deliverables.

## Unity check

No Unity Editor executable was found under `C:\Program Files\Unity\Hub\Editor`, `C:\Program Files\Unity\Editor`, or the Unity Hub tree. Therefore no Unity import, Console, Avatar, bounds, scale, axis, foot-placement, material-extraction, animation-clip, or GUID/meta verification was claimed.

When a Unity 6 Editor is available, the following is the reproducible project-level batch entry for the existing D18 pipeline audit:

```powershell
& '<UNITY_EDITOR_EXE>' -batchmode -nographics -quit `
  -projectPath 'C:\Users\Administrator\Desktop\game\first01\game' `
  -executeMethod WhalesIslandReef.Editor.D18ArtPipeline.D18ArtPipelineBatch.RunKenneyAudit `
  -logFile 'C:\Users\Administrator\Desktop\game\first01\game\tools\results\D18\D18-A04-R02-F\D18_A04_R02_F_UNITY.log'
```

That existing method is report-only and uses the checked-in D18-A02 configuration. A future D18-S02-R01 operator must explicitly point its candidate audit at this isolated manifest and inspect only `GapFill/FemaleNeutral`; this task did not change the global D18 configuration.

## Final local checks

`git diff --check` completed with no whitespace errors. Git emitted only the repository's existing line-ending normalization warnings; no candidate file is promoted by this task.
