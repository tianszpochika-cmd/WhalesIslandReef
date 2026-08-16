# Quaternius Eagle — CC0 provenance evidence

Scope: `D18-A04-R02-B` isolated bird candidate only. This file is evidence, not an admission or a runtime approval.

## Source chain

- Author/publisher: Quaternius.
- Official publisher page: https://quaternius.com/packs/ultimateanimatedanimals.html
- Official license FAQ: https://quaternius.com/faq.html
- Download/source page: https://opengameart.org/content/animated-animales-low-poly
- Direct archive URL used: https://opengameart.org/sites/default/files/Animal%20Pack%20Vol.2%20by%20%40Quaternius.zip
- License text: https://creativecommons.org/publicdomain/zero/1.0/
- Downloaded at: `2026-08-15` (Asia/Shanghai).

The publisher and source page identify the pack as CC0. The archive itself has no embedded `LICENSE` file, so the evidence is deliberately the official publisher/source pages plus the CC0 legal text; no license is inferred from a host site alone.

## Archive and file identity

| Item | Value |
|---|---|
| Archive | `Animal_Pack_Vol2_Quaternius.zip` |
| Archive size | `2,069,972` bytes |
| Archive SHA-256 | `6D8F6789D65DBDB352EBAD98A4209F26B41B384E87D81BC05E9CA76139DB1248` |
| Original entry | `Animal Pack Vol.2 by @Quaternius/FBX/Eagle.fbx` |
| Isolated file | `game/Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/Models/eagle.fbx` |
| Extracted file size | `989,676` bytes |
| Extracted file SHA-256 | `C3504C8E0904EAB7B14E695609A861417F089CF724B94BDA821EA4A3BBC6F505` |

The original file name is preserved. The D18 naming audit warning is recorded rather than silently renaming the source.

## Unity evidence

Unity `6000.0.80f1`, URP project fixture, report-only audit:

- 1 imported FBX / 1 mesh / 4 material slots / 0 external texture files.
- 1 renderer, specifically 1 `SkinnedMeshRenderer`; bounds valid.
- 3 real imported clips: `EagleArmature|Flying`, `EagleArmature|Idle`, `Eagle|Idle`.
- Importer is `Generic`, `NoAvatar`; source has no formal `Animator` component.
- 33 imported transforms are reported by the D18 audit; the detail dump reports 24 skinned bones.
- No collider, `LODGroup`, Prefab, scene, Animator Controller, Registry entry, or runtime consumer was created.

The source is therefore a real Unity-imported bird candidate, but it is not a D20-ready character/animal Prefab. D20 must provide the Animator/Prefab/LOD/collision ownership and make the scale decision.

## Related downloads deliberately rejected

The downloaded Chicken and Rooster archives were inspected because they are official CC0 OpenGameArt candidates:

- `chicken_0.zip`, SHA-256 `0E958040FDB855C192903657D8A9DA2D95CF17EA23165676BAE2AE1454BAA284`: `chickenV2.blend`, `license.txt`, and two textures; no FBX/GLB/OBJ runtime import file. Rejected as `source_only`.
- `rooster.zip`, SHA-256 `A8C9716B1D1E27DF1FAA7E83DA05FB9E4C5088308116975FBDDDFA1785A35917`: `rooster.blend`, `license.txt`, and three textures; no FBX/GLB/OBJ runtime import file. Rejected as `source_only`.

Neither is copied to the candidate directory or counted as a usable model. The archive listings and rejection reasons are machine-recorded under `game/tools/results/D18/D18-A04-R02-B/`.

## Admission boundary

This evidence does not alter the global catalog and does not grant `intake_eligible`. Only `D18-S02-R01` may perform admission after reviewing this packet.
