# D18-A04-R02-C — isolated cat candidate packet

Conclusion: **PARTIAL**

This packet fills the verified cat-model gap with one real CC0 cat candidate. It is isolated evidence for later review; it does not create a formal pet Prefab, edit the Content Registry, change D11 pet facts, or connect a runtime consumer.

## Candidate and provenance

- Candidate: `game/Assets/Art/Candidates/CharactersAnimals/GapFill/Cat/Source/Cat.fbx`
- Original archive member: `Animal Pack Vol.2 by @Quaternius/FBX/Cat.fbx`
- Manifest: `docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/D18_A04_R02_C_manifest.json`
- Source evidence: `docs/game/design/common/assets/packs/characters_animals/gap_fill/cat/D18_A04_R02_C_SOURCE_EVIDENCE.md`
- Official page: https://opengameart.org/content/animated-animales-low-poly
- Direct download: https://opengameart.org/sites/default/files/Animal%20Pack%20Vol.2%20by%20%40Quaternius.zip
- Licence: CC0 1.0, official text at https://creativecommons.org/publicdomain/zero/1.0/
- Archive SHA-256: `6D8F6789D65DBDB352EBAD98A4209F26B41B384E87D81BC05E9CA76139DB1248`
- Selected `Cat.fbx` SHA-256: `24D03762BCBCDF5414516354B0D80C6580A0FFD7B5ABE4D8EDD26AD7BF7A0DB5`
- Downloaded: 2026-08-15.

The package has five animals and no package licence file. The official listing provides the author/license claim; the local CC0 legal-code snapshot and archive hash preserve the evidence chain. Dog, eagle, piranha and wolf files are not copied or renamed as cats.

## Unity import evidence

Audit environment: Unity `6000.0.80f1`, isolated fixture project under `game/tools/results/D18/D18-A04-R02-C/UnityAuditFixtureProject/`. The FBX was imported without changing importer settings or saving a Prefab.

| Measurement | Result |
|---|---:|
| Imported files | 1 FBX |
| Mesh sub-assets | 1 (`Cat`) |
| Vertices / triangles | 1,537 / 180 |
| Embedded materials | 3 (`White`, `Grey`, `Pink`) |
| Texture sub-assets | 0 |
| Renderer | 1 `SkinnedMeshRenderer` |
| Animation type | Generic |
| Animation clips | 2 |
| Global scale reported by importer | 1 |
| Bounds min | (-3.680693, -1.228470, -0.841662) |
| Bounds max | (3.643051, 5.461742, 0.763375) |
| Bounds size | (7.323745, 6.690212, 1.605036) Unity units |
| Import errors | 0 |

Raw candidate warnings are genuine: no Collider, no LODGroup, no Avatar, and no texture sub-assets. The material slots resolve to embedded materials. The model is materially usable for a later presentation wrapper but is not a ready-to-place pet object.

Machine results:

- `game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_UNITY_IMPORT_AUDIT.json`
- `game/tools/results/D18/D18-A04-R02-C/D18_A04_R02_C_UNITY_IMPORT_AUDIT.log`
- `game/tools/results/D18/D18-A04-R02-C/unity_cat_import_audit.log`

## Real actions and missing fallback

Unity found exactly these clips:

- `CatArmature|Idle` — available idle.
- `CatArmature|Walking` — available walking.

No native `Run`, `Rest`, `Interact`, `Feed`, `Comfort`, `CatchMouse`, `Warn`, `Lost` or `Recover` clips were found. These are not claimed. D20-A06 may propose presentation fallbacks after review:

| Pet presentation request | Candidate evidence | Boundary-safe fallback proposal |
|---|---|---|
| Idle | Native clip | Use `CatArmature|Idle`. |
| Walk | Native clip | Use `CatArmature|Walking`. |
| Run | Missing | Temporarily use rate-limited walk only if D20 review accepts it; never label it a native run. |
| Rest | Missing | Use idle only as a visible fallback; no gameplay state is inferred. |
| Interact / feed / comfort | Missing | Hold idle or walk-to-anchor; D11 system facts and events remain authoritative. |
| Catch mouse / warn | Missing | Keep the D11 action as a fact/event presentation request; do not synthesize an asset event from the model. |

## D20-A06 adaptation boundary

- D20-A06 owns any presentation-only wrapper, scale calibration, root/foot correction, forward-axis review, Collider/trigger choice, LODGroup, follow anchor and animation state mapping.
- The measured height is 6.690 Unity units at importer scale 1; a later owner should calibrate against the D20 animal target rather than silently rewriting this source. A first review estimate for a 1 m visual height is approximately 0.1495 uniform scale, pending visual QA.
- The reported bounds place the raw model origin away from a confirmed foot plane (`min.y = -1.228470`); a root/foot anchor must be reviewed in D20-A06. No pivot is changed here.
- The report records Unity Y-up / Z-forward as the project reference. The elongated raw bounds and source orientation require a D20 visual check; no automatic axis conversion is applied.
- A Generic Animator with no Avatar is expected for this animal and must not be retargeted as Humanoid.
- D11PetSystem/AnimalCareSystem continues to own pet facts, lifecycle, care, mortality, action validity and events. This candidate contributes only renderable geometry/material/animation evidence.

## Status boundary and known limitations

- Review status: `Candidate`.
- Import status: `ImportVerified` for the isolated FBX import only.
- No global admission record, catalog mutation, formal Prefab, scene reference, runtime consumer or gameplay logic was added.
- The conclusion is **PARTIAL** because the actual cat and import evidence are complete, but the source only supplies Idle/Walking and lacks Collider/LOD/texture sub-assets; formal pet integration remains out of scope and must be decided by D18-S02-R01.

