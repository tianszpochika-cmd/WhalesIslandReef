# D18-A04-R02-B source and gap evidence

## Existing bird is not usable as a D20 model

The existing bird candidate at `game/Assets/Art/Candidates/CharactersAnimals/OpenGameArtBird/Source/bird.blend` is explicitly `NotImported` in its existing manifest. Its Unity object/model count is `0`; it is Blender-only, has no FBX conversion evidence, and no Unity mesh, Renderer, animation import, bounds, or Animator audit. D20-A05 therefore cannot treat it as a usable near- or far-distance model.

## Source search and decision

| Source | Evidence | Decision |
|---|---|---|
| Quaternius Animal Pack Vol.2 | Official CC0 publisher page, Quaternius FAQ, OpenGameArt source page, direct archive and SHA recorded | `ACCEPTED_CANDIDATE`: Eagle FBX only, Unity verified |
| CDmir / TinyWorlds Chicken | OpenGameArt page states CC0 and animated, but downloaded archive is Blend + license + textures only | `REJECTED_SOURCE_ONLY`: no Unity runtime model |
| CDmir / TinyWorlds Rooster | OpenGameArt page states CC0 and animated, but downloaded archive is Blend + license + textures only | `REJECTED_SOURCE_ONLY`: no Unity runtime model |

Source pages:

- Quaternius publisher: https://quaternius.com/packs/ultimateanimatedanimals.html
- Quaternius FAQ: https://quaternius.com/faq.html
- Quaternius OpenGameArt source: https://opengameart.org/content/animated-animales-low-poly
- Chicken: https://opengameart.org/content/chicken-animated
- Rooster: https://opengameart.org/content/rooster-animated
- CC0 legal text: https://creativecommons.org/publicdomain/zero/1.0/

No source with uncertain licensing was copied or counted. No Blender source was imported into the candidate directory.

## Downloaded archive evidence

Downloaded date for all entries: `2026-08-15` (Asia/Shanghai).

| Archive | Bytes | SHA-256 | Archive contents relevant to decision |
|---|---:|---|---|
| `Animal_Pack_Vol2_Quaternius.zip` | 2,069,972 | `6D8F6789D65DBDB352EBAD98A4209F26B41B384E87D81BC05E9CA76139DB1248` | `FBX/Eagle.fbx` plus FBX/OBJ/Blend files for Cat, Dog, Piranha, Wolf and a preview; no embedded license file |
| `chicken_0.zip` | 6,214,921 | `0E958040FDB855C192903657D8A9DA2D95CF17EA23165676BAE2AE1454BAA284` | `chickenV2.blend`, `license.txt`, `textures/ChickenNorm.png`, `textures/UV-Texture.png` |
| `rooster.zip` | 8,023,955 | `A8C9716B1D1E27DF1FAA7E83DA05FB9E4C5088308116975FBDDDFA1785A35917` | `rooster.blend`, `license.txt`, `textures/Rooster-UV.png`, `textures/Rooster-UV_norskin.png`, `image_AO.png`, `image_h.png` |

The complete machine-readable listings are in `archive_file_lists.json`.

## Accepted file identity

- Original archive member: `Animal Pack Vol.2 by @Quaternius/FBX/Eagle.fbx`.
- Isolated repository file: `game/Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/Models/eagle.fbx`.
- File size: `989,676` bytes.
- SHA-256: `C3504C8E0904EAB7B14E695609A861417F089CF724B94BDA821EA4A3BBC6F505`.
- No file was renamed or modified after extraction.
