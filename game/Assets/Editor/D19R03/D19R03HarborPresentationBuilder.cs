#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Whalebone.Editor.D19R03
{
    /// <summary>
    /// Builds the R03 visual layer only. Gameplay anchors, colliders, save
    /// facts and D19-A01—A06 source scenes remain outside this prefab.
    /// </summary>
    public static class D19R03HarborPresentationBuilder
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string RootName = "D19_R03_HarborPresentation";
        private const string PrefabDirectory = "Assets/Prefabs/Environment/D19/Integration/R03";
        private const string PrefabPath = PrefabDirectory + "/D19_R03_HarborPresentation.prefab";
        private const string ShipPrefabPath = PrefabDirectory + "/D19_R03_ShipVisual.prefab";
        private const string PirateShipCandidatePath = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/ship-pirate-medium.fbx";
        private const string PirateTowerCandidatePath = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/tower-complete-small.fbx";
        private const string PirateDockCandidatePath = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/structure-platform-dock-small.fbx";
        private const string PirateBoatCandidatePath = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/boat-row-small.fbx";
        private const string PirateCannonCandidatePath = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/cannon.fbx";
        private const string PirateCrateCandidatePath = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/crate-bottles.fbx";
        private const string PirateFlagCandidatePath = "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/flag-pirate.fbx";
        private const string MaterialDirectory = "Assets/Art/Materials/D19/R03";
        private const string MeshDirectory = MaterialDirectory + "/Meshes";
        private const float PresentationUvScale = 0.20f;
        private const float IslandRadiusX = 19.4f;
        private const float IslandRadiusZ = 17.8f;
        private const int IslandSkirtSegments = 64;
        // Keep the island edge readable from the water without exposing the
        // old full-height vertical cut.  The presentation mesh and the single
        // D19_TerrainRoot MeshCollider share this profile, so the physical
        // boundary remains owned by the same R03 surface rather than gaining
        // a second shoreline collider.
        private const float IslandCoastalShelfWidth = 1.35f;
        // Keep the shelf below the lowest authored perimeter sample.  At the
        // south-west edge the deterministic relief can dip below the water
        // plane; choosing a lower shelf prevents the bevel from inverting
        // into an outward-facing lip at those samples.
        private const float IslandCoastalShelfY = -0.24f;
        private const float IslandSkirtBottomY = -0.42f;
        // ph_ship_fishing is a gameplay/boarding anchor at z=-5.2. Its
        // presentation model must sit at the physical berth instead of in
        // the market corridor. The value is world metres; InstallShipPresentation
        // converts it to ship-local coordinates so the scaled root cannot
        // multiply the berth offset.
        private static readonly Vector3 ShipVisualBerthOffset = new Vector3(0f, 0f, -3.6f);
        private static readonly Vector3 PlayerSpawnWorldOffset = new Vector3(0f, 1.1f, 2.5f);
        private static readonly Vector3 DisembarkWorldOffset = new Vector3(0f, 0f, 4.5f);
        // The verified candidate is authored at a much larger scale than the
        // small harbor service buildings. Keep the visual hull readable at
        // the dock without letting the first-person/boarding shots fill the
        // whole frame or appear to sit on top of the town.
        private const float ShipVisualFootprint = 4.8f;
        // The authored Kenney hull points toward local +Z.  The stable D2
        // sailing snapshot uses the ship root forward axis as its course, and
        // the initial berth opens toward -Z.  Keep the root and the visual
        // candidate in the same frame so positive throttle leaves the harbor
        // instead of driving through the service buildings.
        private const float InitialShipHeading = 180f;
        private const float PalmRootBurial = 0.35f;
        // CreateHouse places the foundation mesh with a 0.02m local bottom
        // offset (center 0.13m, height 0.22m). Ground the root from the same
        // authored Height function instead of a shared y=0.22 guess; the
        // island relief varies enough that the old constant visibly floated
        // the tavern/warehouse or buried them into the route.
        private const float HouseFoundationBottomOffset = 0.02f;
        // The current low-poly palm mesh has no pivot/ground-contact contract.
        // Keep the authored positions in source for the next pivot-safe asset,
        // but do not ship a renderer that the Player evidence cannot prove is
        // attached to visible land. This is presentation-only containment and
        // does not remove gameplay, collision, or navigation data.
        private static readonly bool RenderAuthoredPalms = false;
        private static int _meshSequence;

        // Keep the stylized palette readable under the project's URP lighting.
        // The previous values were clipped into large orange/white blocks in a
        // real Player, especially on the sand and roof planes.
        private static readonly Color Grass = new Color(0.13f, 0.23f, 0.13f, 1f);
        private static readonly Color GrassLight = new Color(0.25f, 0.35f, 0.18f, 1f);
        private static readonly Color Earth = new Color(0.28f, 0.17f, 0.09f, 1f);
        private static readonly Color Sand = new Color(0.57f, 0.43f, 0.25f, 1f);
        private static readonly Color SandLight = new Color(0.72f, 0.57f, 0.34f, 1f);
        private static readonly Color Stone = new Color(0.24f, 0.28f, 0.28f, 1f);
        private static readonly Color StoneLight = new Color(0.42f, 0.43f, 0.38f, 1f);
        private static readonly Color Wood = new Color(0.18f, 0.075f, 0.03f, 1f);
        private static readonly Color WoodLight = new Color(0.30f, 0.17f, 0.095f, 1f);
        private static readonly Color Plaster = new Color(0.52f, 0.43f, 0.32f, 1f);
        private static readonly Color Roof = new Color(0.20f, 0.12f, 0.075f, 1f);
        private static readonly Color Teal = new Color(0.025f, 0.25f, 0.34f, 1f);
        // Foam is a thin shoreline accent, not a second set of bright boards.
        // The old value made every segment read as a floating cyan rectangle
        // in the fixed Player shots, especially against the dark water.
        private static readonly Color Foam = new Color(0.24f, 0.46f, 0.50f, 1f);
        private static readonly Color Leaf = new Color(0.10f, 0.28f, 0.12f, 1f);
        // During a presentation rebuild, ground contact must be sampled from
        // the exact MeshCollider that is saved as D19_TerrainRoot.  The
        // analytical Height() function is useful for authored anchors, but
        // the deformed 26x26 island grid interpolates between samples; using
        // Height() for props can therefore leave them visibly above or below
        // the collider in the Player.
        private static Collider _buildGroundCollider;

        [MenuItem("Whalebone/D19 R03/Build Harbor Presentation")]
        public static void BuildFromMenu() { Build(ScenePath); }

        [MenuItem("Whalebone/D19 R03/Audit Harbor Presentation")]
        public static void AuditFromMenu() { Audit(ScenePath); }

        [MenuItem("Whalebone/D19 R03/Audit Ship Candidate")]
        public static void AuditShipCandidateFromMenu() { AuditShipCandidate(PirateShipCandidatePath); }

        [MenuItem("Whalebone/D19 R03/Audit Pirate Port Candidates")]
        public static void AuditPiratePortCandidatesFromMenu() { AuditPiratePortCandidates(); }

        public static void Build(string scenePath)
        {
            EnsureFolder("Assets/Prefabs");
            EnsureFolder("Assets/Prefabs/Environment");
            EnsureFolder("Assets/Prefabs/Environment/D19");
            EnsureFolder("Assets/Prefabs/Environment/D19/Integration");
            EnsureFolder(PrefabDirectory);
            EnsureFolder("Assets/Art");
            EnsureFolder("Assets/Art/Materials");
            EnsureFolder("Assets/Art/Materials/D19");
            EnsureFolder(MaterialDirectory);
            EnsureFolder(MeshDirectory);
            _meshSequence = 0;
            _buildGroundCollider = null;

            var grass = Material("R03_Grass", Grass, 0.0f, 0.08f);
            var grassLight = Material("R03_GrassLight", GrassLight, 0.0f, 0.08f);
            var earth = Material("R03_Earth", Earth, 0.0f, 0.12f);
            var sand = Material("R03_Sand", Sand, 0.0f, 0.15f);
            var sandLight = Material("R03_SandLight", SandLight, 0.0f, 0.15f);
            var stone = Material("R03_Stone", Stone, 0.0f, 0.18f);
            var stoneLight = Material("R03_StoneLight", StoneLight, 0.0f, 0.18f);
            var wood = Material("R03_Wood", Wood, 0.0f, 0.18f);
            var woodLight = Material("R03_WoodLight", WoodLight, 0.0f, 0.18f);
            var plaster = Material("R03_Plaster", Plaster, 0.0f, 0.12f);
            var roof = Material("R03_Roof", Roof, 0.0f, 0.10f);
            var teal = Material("R03_Teal", Teal, 0.0f, 0.45f);
            var foam = Material("R03_Foam", Foam, 0.0f, 0.25f);
            var leaf = Material("R03_Leaf", Leaf, 0.0f, 0.08f);
            var sail = Material("R03_Sail", new Color(0.78f, 0.72f, 0.56f, 1f), 0.0f, 0.10f);

            var prefabRoot = new GameObject(RootName);
            prefabRoot.transform.position = Vector3.zero;
            prefabRoot.transform.rotation = Quaternion.identity;
            prefabRoot.transform.localScale = Vector3.one;
            prefabRoot.AddComponent<Whalebone.Gameplay.D19.R03.D19R03HarborRuntimePresentation>();

            var island = CreateIslandSurface(grass, grassLight, earth, sand, sandLight, stone, stoneLight);
            island.transform.SetParent(prefabRoot.transform, false);
            // The approved Player route still needs a real walkable surface
            // between DockGround and the frozen ShoreBoundary.  Install the
            // same mesh used by the visible island on the stable D2/D6
            // walkable-surface contract node.  Keeping the MeshCollider in the
            // prefab (instead of a broad placeholder BoxCollider) makes the
            // editor audit and runtime physics agree about the surface shape.
            AddRouteGroundCollider(prefabRoot.transform, island);
            var shore = CreateShoreline(sandLight, stone, foam, teal);
            shore.transform.SetParent(prefabRoot.transform, false);
            var roads = CreateRoadNetwork(earth, sandLight, stoneLight);
            roads.transform.SetParent(prefabRoot.transform, false);
            // Keep the four service buildings inside the same frozen route
            // radii, but open clear sight lines between them.  The previous
            // nearly-collinear placement made the tavern, market and
            // warehouse occupy one wall of the first camera.
            var market = CreateHouse("MarketVisual", new Vector3(6.4f, 0.22f, 1.2f), new Vector3(5.2f, 3.6f, 4.0f), plaster, roof, wood, woodLight, sandLight, "MARKET");
            market.transform.SetParent(prefabRoot.transform, false);
            var warehouse = CreateHouse("WarehouseVisual", new Vector3(-6.4f, 0.22f, 1.2f), new Vector3(4.9f, 3.6f, 3.8f), woodLight, roof, wood, stoneLight, sandLight, "WAREHOUSE");
            warehouse.transform.SetParent(prefabRoot.transform, false);
            // Keep the tavern well clear of the stable ship's silhouette and
            // of the market facade.  The stable gameplay ship remains at
            // z=-5.2; the east-side placement gives the tavern its own
            // readable approach instead of hiding its door behind the mast.
            var tavern = CreateHouse("TavernVisual", new Vector3(10.2f, 0.22f, 8.5f), new Vector3(4.8f, 3.5f, 3.8f), plaster, roof, wood, woodLight, sandLight, "TAVERN");
            tavern.transform.SetParent(prefabRoot.transform, false);
            // Keep the shipyard on the west approach, but move its visible
            // footprint south of the warehouse.  The previous z=-2.6 layout
            // overlapped the warehouse's south-west bounds even though the
            // authored root positions looked separated in the editor.
            var shipyard = CreateHouse("ShipyardVisual", new Vector3(-9.5f, 0.22f, -4.2f), new Vector3(4.2f, 3.2f, 3.3f), woodLight, roof, wood, stoneLight, sandLight, "SHIPYARD");
            shipyard.transform.SetParent(prefabRoot.transform, false);
            var rope = Material("R03_Rope", new Color(0.32f, 0.18f, 0.075f, 1f), 0.0f, 0.12f);
            var dock = CreateDock(wood, woodLight, stone, rope);
            dock.transform.SetParent(prefabRoot.transform, false);
            var plants = CreateVegetation(wood, leaf, grassLight, stoneLight);
            plants.transform.SetParent(prefabRoot.transform, false);
            var props = CreatePortProps(wood, woodLight, stone, sandLight);
            props.transform.SetParent(prefabRoot.transform, false);
            var entry = CreateMarketEntry(wood, woodLight, stone, sandLight);
            entry.transform.SetParent(prefabRoot.transform, false);
            var backdrop = CreateHarborBackdrop(earth, stone, grassLight, leaf);
            backdrop.transform.SetParent(prefabRoot.transform, false);
            AddCandidateDetails(prefabRoot.transform, wood, leaf, stone, grassLight);
            AddPiratePortCandidateDetails(prefabRoot.transform, wood, woodLight, stone, stoneLight, sail);

            foreach (var transform in prefabRoot.GetComponentsInChildren<Transform>(true))
                transform.gameObject.isStatic = true;

            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath, out bool saved);
            if (!saved || prefab == null)
                throw new InvalidOperationException("Could not save R03 presentation prefab: " + PrefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);

            // D27 is a nested presentation layer of the R03 prefab.  A fresh
            // D19 save replaces the prefab asset, so any D27 attachment made
            // before this point would be discarded.  Rebuild and attach D27
            // only after the D19 asset is final, then reload the asset before
            // creating the scene instance.  D27's attach step removes the
            // previous node first, keeping exactly one nested instance.
            WhalesIslandReef.Editor.D27.D27R03CharacterWildlifePresentationBuilder.BuildAndAttach();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Could not reload R03 presentation prefab after D27 attachment: " + PrefabPath);

            var shipPrefab = BuildShipPrefab(wood, woodLight, stone, foam, sail);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            RemoveSceneInstance(scene, RootName);
            var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
            if (instance == null) throw new InvalidOperationException("Could not instantiate " + PrefabPath);
            instance.name = RootName;
            // The R03 meshes and the D6 route anchors share world Y=0.  The
            // previous 0.58 offset lifted the whole island, dock and wildlife
            // above the physics surfaces and was the direct cause of several
            // floating/sunken-looking objects in Player.
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            HidePresentationRenderers(scene, "D19_R02_HarborPresentation");
            HidePresentationRenderers(scene, "D19_R01_B_HarborVisualRepair");
            HideLegacyVisuals(scene);
            InstallShipPresentation(scene, shipPrefab);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[D19-R03-A02] Built and placed " + PrefabPath);
        }

        private static void AddRouteGroundCollider(Transform parent, GameObject surface)
        {
            var ground = new GameObject("D19_TerrainRoot");
            ground.transform.SetParent(parent, false);
            // The collider shares the visible surface transform.  The old
            // broad-box placement (Y=0.05, Z=5) shifted the physical surface
            // away from the visible island and created invisible movement
            // walls.
            ground.transform.localPosition = Vector3.zero;
            ground.transform.localRotation = Quaternion.identity;
            ground.transform.localScale = Vector3.one;
            ground.isStatic = true;
            var filter = surface == null ? null : surface.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
                throw new InvalidOperationException("R03 island surface mesh is required before creating D19_TerrainRoot");
            var collider = ground.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
            collider.convex = false;
            collider.isTrigger = false;
            _buildGroundCollider = collider;
        }

        public static void Audit(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var root = GameObject.Find(RootName);
            if (root == null) throw new InvalidOperationException("Missing " + RootName);
            // D35 audits the D19-owned surface layer. D27 is intentionally
            // nested under this prefab, but it has its own build and audit
            // contract; excluding that named child keeps the D19 baseline at
            // the approved 391 Renderer / 1 Collider while retaining exactly
            // one D27 attachment in the prefab.
            var renderers = new List<Renderer>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (!IsNestedD27Presentation(renderer.transform)) renderers.Add(renderer);
            var colliders = new List<Collider>();
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                if (!IsNestedD27Presentation(collider.transform)) colliders.Add(collider);
            var bounds = new Bounds(root.transform.position, Vector3.zero);
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            int vertices = 0;
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
                if (!IsNestedD27Presentation(filter.transform) && filter.sharedMesh != null)
                    vertices += filter.sharedMesh.vertexCount;
            var report = new Dictionary<string, object>
            {
                ["schema"] = "D19_R03_A02_EDITOR_AUDIT_V1",
                ["scene"] = scene.path,
                ["root"] = RootName,
                ["renderer_count"] = renderers.Count,
                ["collider_count"] = colliders.Count,
                ["mesh_vertex_sum"] = vertices,
                ["bounds_center"] = new [] { bounds.center.x, bounds.center.y, bounds.center.z },
                ["bounds_size"] = new [] { bounds.size.x, bounds.size.y, bounds.size.z },
                ["scene_root_names"] = SceneRootSummary(scene),
                ["external_candidate_models"] = new [] {
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_small_dark.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_large_rock.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass_large.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathBend.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/cliff_stepsCorner_stone.fbx",
                    "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_tallF.fbx",
                    PirateShipCandidatePath,
                    PirateTowerCandidatePath,
                    PirateDockCandidatePath,
                    PirateBoatCandidatePath,
                    PirateCannonCandidatePath,
                    PirateCrateCandidatePath,
                    PirateFlagCandidatePath
                }
            };
            string output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A02/D19_R03_A02_EDITOR_AUDIT.json";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output));
            System.IO.File.WriteAllText(output, MiniJson(report));
            Debug.Log("[D19-R03-A02] Audit written: " + output);
        }

        private static bool IsNestedD27Presentation(Transform transform)
        {
            var current = transform;
            while (current != null && current != current.root)
            {
                if (current.name == "D27_R03_CharacterWildlifePresentation") return true;
                current = current.parent;
            }
            return current != null && current.name == "D27_R03_CharacterWildlifePresentation";
        }

        public static void AuditSceneColliders()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var rows = new List<SceneColliderAuditRow>();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                {
                    if (collider == null) continue;
                    rows.Add(new SceneColliderAuditRow
                    {
                        name = collider.name,
                        hierarchy_path = HierarchyPath(collider.transform),
                        enabled = collider.enabled,
                        is_trigger = collider.isTrigger,
                        position = collider.transform.position,
                        bounds_center = collider.bounds.center,
                        bounds_size = collider.bounds.size
                    });
                }
            }
            var report = new SceneColliderAuditReport
            {
                schema = "D19_R03_A01_SCENE_COLLIDER_AUDIT_V1",
                scene = ScenePath,
                collider_count = rows.Count,
                rows = rows.ToArray()
            };
            var output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D19/D19-R03-A01/D19_R03_A01_SCENE_COLLIDER_AUDIT.json";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("[D19-R03-A01] Scene collider audit written: " + output);
        }

        public static void AuditShipCandidate(string path)
        {
            var audit = new ShipCandidateAudit
            {
                asset_path = path,
                material_names = Array.Empty<string>()
            };
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                audit.error = "AssetDatabase.LoadAssetAtPath<GameObject> returned null";
            }
            else
            {
                audit.loaded = true;
                audit.root_name = asset.name;
                var renderers = asset.GetComponentsInChildren<Renderer>(true);
                var filters = asset.GetComponentsInChildren<MeshFilter>(true);
                var bounds = new Bounds(Vector3.zero, Vector3.zero);
                var hasBounds = false;
                var materials = new List<string>();
                foreach (var renderer in renderers)
                {
                    if (!hasBounds)
                    {
                        bounds = renderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }

                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material == null)
                        {
                            audit.missing_material_count++;
                            materials.Add("<missing>");
                        }
                        else
                        {
                            materials.Add(material.name);
                        }
                    }
                }

                audit.renderer_count = renderers.Length;
                audit.mesh_filter_count = filters.Length;
                audit.material_slot_count = materials.Count;
                audit.bounds_center = new[] { bounds.center.x, bounds.center.y, bounds.center.z };
                audit.bounds_size = new[] { bounds.size.x, bounds.size.y, bounds.size.z };
                audit.material_names = materials.ToArray();
                audit.animation_clip_count = Array.FindAll(
                    AssetDatabase.LoadAllAssetsAtPath(path), assetObject => assetObject is AnimationClip).Length;
            }

            var shipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShipPrefabPath);
            if (shipPrefab != null)
            {
                audit.prefab_loaded = true;
                audit.prefab_renderer_count = shipPrefab.GetComponentsInChildren<Renderer>(true).Length;
                audit.prefab_collider_count = shipPrefab.GetComponentsInChildren<Collider>(true).Length;
                foreach (var filter in shipPrefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh != null) audit.prefab_mesh_vertex_sum += filter.sharedMesh.vertexCount;
                }
                foreach (var transform in shipPrefab.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name == "KenneyPirateShipPresentation")
                    {
                        audit.prefab_has_candidate_instance = true;
                        break;
                    }
                }
            }

            var output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D31/D31-S05/D31_S05_SHIP_CANDIDATE_AUDIT_R65.json";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, JsonUtility.ToJson(audit, true));
            Debug.Log("[D19-R03-A02] Ship candidate audit written: " + output);
        }

        public static void AuditPiratePortCandidates()
        {
            var paths = PiratePortCandidatePaths();
            var rows = new List<PiratePortCandidateAudit>();
            foreach (var path in paths)
            {
                var row = new PiratePortCandidateAudit
                {
                    asset_path = path,
                    material_names = Array.Empty<string>()
                };
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null)
                {
                    row.error = "AssetDatabase.LoadAssetAtPath<GameObject> returned null";
                    rows.Add(row);
                    continue;
                }

                row.loaded = true;
                row.root_name = asset.name;
                var renderers = asset.GetComponentsInChildren<Renderer>(true);
                var filters = asset.GetComponentsInChildren<MeshFilter>(true);
                var bounds = RendererBounds(asset);
                var materials = new List<string>();
                foreach (var renderer in renderers)
                {
                    foreach (var material in renderer.sharedMaterials)
                        materials.Add(material == null ? "<missing>" : material.name);
                }

                row.renderer_count = renderers.Length;
                row.mesh_filter_count = filters.Length;
                row.material_slot_count = materials.Count;
                row.missing_material_count = materials.FindAll(name => name == "<missing>").Count;
                row.bounds_center = new[] { bounds.center.x, bounds.center.y, bounds.center.z };
                row.bounds_size = new[] { bounds.size.x, bounds.size.y, bounds.size.z };
                row.mesh_vertex_sum = 0;
                foreach (var filter in filters)
                    if (filter.sharedMesh != null) row.mesh_vertex_sum += filter.sharedMesh.vertexCount;
                row.material_names = materials.ToArray();
                row.animation_clip_count = Array.FindAll(
                    AssetDatabase.LoadAllAssetsAtPath(path), assetObject => assetObject is AnimationClip).Length;
                rows.Add(row);
            }

            var report = new PiratePortCandidateAuditReport
            {
                schema = "D31_S05_PIRATE_PORT_CANDIDATE_AUDIT_R66",
                candidate_count = rows.Count,
                candidates = rows.ToArray()
            };
            var output = "C:/Users/Administrator/Desktop/game/first01/game/tools/results/D31/D31-S05/D31_S05_PIRATE_PORT_CANDIDATE_AUDIT_R66.json";
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            Debug.Log("[D19-R03-A02] Pirate port candidate audit written: " + output);
        }

        private static string HierarchyPath(Transform current)
        {
            var path = current == null ? string.Empty : current.name;
            while (current != null && current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }
            return path;
        }

        [Serializable]
        private sealed class SceneColliderAuditReport
        {
            public string schema;
            public string scene;
            public int collider_count;
            public SceneColliderAuditRow[] rows;
        }

        [Serializable]
        private sealed class SceneColliderAuditRow
        {
            public string name;
            public string hierarchy_path;
            public bool enabled;
            public bool is_trigger;
            public Vector3 position;
            public Vector3 bounds_center;
            public Vector3 bounds_size;
        }

        [Serializable]
        private sealed class ShipCandidateAudit
        {
            public string asset_path;
            public bool loaded;
            public string root_name;
            public int renderer_count;
            public int mesh_filter_count;
            public int material_slot_count;
            public int missing_material_count;
            public int animation_clip_count;
            public float[] bounds_center;
            public float[] bounds_size;
            public string[] material_names;
            public bool prefab_loaded;
            public int prefab_renderer_count;
            public int prefab_collider_count;
            public int prefab_mesh_vertex_sum;
            public bool prefab_has_candidate_instance;
            public string error;
        }

        [Serializable]
        private sealed class PiratePortCandidateAuditReport
        {
            public string schema;
            public int candidate_count;
            public PiratePortCandidateAudit[] candidates;
        }

        [Serializable]
        private sealed class PiratePortCandidateAudit
        {
            public string asset_path;
            public bool loaded;
            public string root_name;
            public int renderer_count;
            public int mesh_filter_count;
            public int material_slot_count;
            public int missing_material_count;
            public int mesh_vertex_sum;
            public int animation_clip_count;
            public float[] bounds_center;
            public float[] bounds_size;
            public string[] material_names;
            public string error;
        }

        private static GameObject CreateIslandSurface(params Material[] materials)
        {
            var go = new GameObject("D19_R03_IslandSurface");
            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            const int width = 26;
            const int depth = 26;
            var vertices = new List<Vector3>();
            var sub = new List<int>[materials.Length];
            for (int i = 0; i < sub.Length; i++) sub[i] = new List<int>();
            for (int z = 0; z < depth - 1; z++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    var p00 = IslandPoint((float)x / (width - 1) * 2f - 1f,
                        (float)z / (depth - 1) * 2f - 1f);
                    var p10 = IslandPoint((float)(x + 1) / (width - 1) * 2f - 1f,
                        (float)z / (depth - 1) * 2f - 1f);
                    var p11 = IslandPoint((float)(x + 1) / (width - 1) * 2f - 1f,
                        (float)(z + 1) / (depth - 1) * 2f - 1f);
                    var p01 = IslandPoint((float)x / (width - 1) * 2f - 1f,
                        (float)(z + 1) / (depth - 1) * 2f - 1f);
                    float h00 = Height(p00.x, p00.y);
                    float h10 = Height(p10.x, p10.y);
                    float h11 = Height(p11.x, p11.y);
                    float h01 = Height(p01.x, p01.y);
                    int baseIndex = vertices.Count;
                    vertices.Add(new Vector3(p00.x, h00, p00.y));
                    vertices.Add(new Vector3(p10.x, h10, p10.y));
                    vertices.Add(new Vector3(p11.x, h11, p11.y));
                    vertices.Add(new Vector3(p01.x, h01, p01.y));
                    int mat = SurfaceMaterial((p00.x + p11.x) * 0.5f,
                        (p00.y + p11.y) * 0.5f, materials.Length);
                    sub[mat].Add(baseIndex); sub[mat].Add(baseIndex + 2); sub[mat].Add(baseIndex + 1);
                    sub[mat].Add(baseIndex); sub[mat].Add(baseIndex + 3); sub[mat].Add(baseIndex + 2);
                }
            }
            AddIslandSkirt(vertices, sub, materials.Length);
            var mesh = new Mesh { name = "D19_R03_IslandSurfaceMesh" };
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.subMeshCount = sub.Length;
            for (int i = 0; i < sub.Length; i++) mesh.SetTriangles(sub[i], i, true);
            var islandUv = new Vector2[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                islandUv[i] = new Vector2(vertices[i].x, vertices[i].z) * 0.08f;
            mesh.uv = islandUv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string path = MaterialDirectory + "/D19_R03_IslandSurfaceMesh.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        /// <summary>
        /// Maps the retained 26x26 presentation grid from a square into an
        /// organic island footprint.  Keeping the grid density preserves the
        /// existing material/UV budget while removing the unmistakable square
        /// silhouette that the old max-axis coordinates produced.
        /// </summary>
        private static Vector2 IslandPoint(float normalizedX, float normalizedZ)
        {
            float diskX = normalizedX * Mathf.Sqrt(Mathf.Max(0f, 1f - normalizedZ * normalizedZ * 0.5f));
            float diskZ = normalizedZ * Mathf.Sqrt(Mathf.Max(0f, 1f - normalizedX * normalizedX * 0.5f));
            float radius = Mathf.Sqrt(diskX * diskX + diskZ * diskZ);
            if (radius <= 0.0001f) return Vector2.zero;

            float angle = Mathf.Atan2(diskZ, diskX);
            float boundaryScale = 1f
                + Mathf.Sin(angle * 3f + 0.35f) * 0.055f
                + Mathf.Cos(angle * 5f - 0.70f) * 0.035f;
            float directionX = diskX / radius;
            float directionZ = diskZ / radius;
            return new Vector2(
                directionX * radius * IslandRadiusX * boundaryScale,
                directionZ * radius * IslandRadiusZ * boundaryScale);
        }

        /// <summary>
        /// Gives the island a beveled, partially submerged coastal edge without
        /// introducing another Renderer or a second collision owner.  The old
        /// profile dropped straight from the authored terrain height to the
        /// deep skirt and read as a vertical cut in Player; the intermediate
        /// shelf now carries the waterline transition while remaining part of
        /// the same static presentation/collision mesh.
        /// </summary>
        private static void AddIslandSkirt(List<Vector3> vertices, List<int>[] sub, int materialCount)
        {
            if (materialCount <= 0) return;
            int material = Mathf.Min(3, materialCount - 1);
            for (int i = 0; i < IslandSkirtSegments; i++)
            {
                float angle0 = i / (float)IslandSkirtSegments * Mathf.PI * 2f;
                float angle1 = (i + 1) / (float)IslandSkirtSegments * Mathf.PI * 2f;
                var p0 = IslandPoint(Mathf.Cos(angle0), Mathf.Sin(angle0));
                var p1 = IslandPoint(Mathf.Cos(angle1), Mathf.Sin(angle1));
                var shelf0 = OffsetIslandBoundary(p0, IslandCoastalShelfWidth);
                var shelf1 = OffsetIslandBoundary(p1, IslandCoastalShelfWidth);
                int baseIndex = vertices.Count;
                vertices.Add(new Vector3(p0.x, Height(p0.x, p0.y) - 0.015f, p0.y));
                vertices.Add(new Vector3(p1.x, Height(p1.x, p1.y) - 0.015f, p1.y));
                vertices.Add(new Vector3(shelf1.x, IslandCoastalShelfY, shelf1.y));
                vertices.Add(new Vector3(shelf0.x, IslandCoastalShelfY, shelf0.y));
                vertices.Add(new Vector3(shelf1.x, IslandSkirtBottomY, shelf1.y));
                vertices.Add(new Vector3(shelf0.x, IslandSkirtBottomY, shelf0.y));
                sub[material].Add(baseIndex); sub[material].Add(baseIndex + 1); sub[material].Add(baseIndex + 2);
                sub[material].Add(baseIndex); sub[material].Add(baseIndex + 2); sub[material].Add(baseIndex + 3);
                sub[material].Add(baseIndex + 2); sub[material].Add(baseIndex + 4); sub[material].Add(baseIndex + 5);
                sub[material].Add(baseIndex + 2); sub[material].Add(baseIndex + 5); sub[material].Add(baseIndex + 3);
            }
        }

        private static Vector2 OffsetIslandBoundary(Vector2 point, float distance)
        {
            var direction = point.sqrMagnitude <= 0.0001f ? Vector2.right : point.normalized;
            return point + direction * Mathf.Max(0f, distance);
        }

        private static float Height(float x, float z)
        {
            float edge = Mathf.InverseLerp(19.5f, 11.5f, Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)));
            float hill = (Mathf.Sin(x * 0.26f + z * 0.13f) * 0.26f + Mathf.Cos(z * 0.31f - x * 0.11f) * 0.18f) * edge;
            float ridge = Mathf.Exp(-Mathf.Pow((x + 7.2f) / 4.6f, 2f) - Mathf.Pow((z - 6.0f) / 7.5f, 2f)) * 1.55f;
            if (z < -7.2f && Mathf.Abs(x) < 7.5f) return 0.035f + hill * 0.18f;
            if (Mathf.Abs(x) < 2.4f && z < 5.0f) return 0.08f + hill * 0.20f;
            return 0.16f + hill + ridge;
        }

        private static int SurfaceMaterial(float x, float z, int count)
        {
            // Use a radial edge band instead of the old world-Z threshold.
            // The threshold painted the whole southern half of the island as
            // one rectangular-looking sand card and also created accidental
            // sand stripes through the playable route.  The island already
            // has an authored organic footprint, so derive the beach from
            // that footprint and leave route markings to CreateRoadNetwork.
            float edgeRadius = Mathf.Sqrt(
                Mathf.Pow(x / IslandRadiusX, 2f) +
                Mathf.Pow(z / IslandRadiusZ, 2f));
            if (edgeRadius > 0.93f) return Mathf.Min(3, count - 1);
            if (edgeRadius > 0.80f) return Mathf.Min(4, count - 1);
            return 0;
        }

        private static GameObject CreateShoreline(Material sand, Material stone, Material foam, Material water)
        {
            var root = new GameObject("D19_R03_ShorelineAndWater");
            // A single flat box made the sea read as a background card from
            // every fixed camera. Keep one renderer and no collider, but use
            // a small deterministic wave grid so the water has a real
            // silhouette and catches URP light differently across the
            // surface. D28WaterMotion still provides the slow whole-surface
            // weather response at runtime.
            // Extend beyond every fixed camera so the water boundary never
            // reads as a square background card at the edge of the island.
            AddMesh(root.transform, "WaterWaveSurface", MakeWaterSurfaceMesh(96f, 96f, 20, 0.055f), water, new Vector3(0f, -0.04f, 0f));
            // Keep the historical scene-level `Beach` compatibility node
            // unambiguous.  D19S02 and older presentation adapters discover
            // that stable name to verify/retain the disabled grey-box node;
            // naming the authored R03 mesh the same way makes GameObject.Find
            // return the wrong object and falsely reports the legacy renderer
            // as visible.  The R03 mesh is presentation-owned and therefore
            // uses an explicit scoped name.
            AddMesh(root.transform, "R03_Beach", MakePathMesh(new[]
            {
                new Vector2(-18.0f, -10.2f), new Vector2(-14.5f, -11.0f),
                new Vector2(-10.5f, -11.8f), new Vector2(-6.0f, -12.4f),
                new Vector2(-1.5f, -12.7f), new Vector2(3.5f, -12.6f),
                new Vector2(8.0f, -12.2f), new Vector2(12.5f, -11.5f),
                new Vector2(16.5f, -10.7f), new Vector2(18.5f, -9.9f)
            }, 2.25f, 0.035f), sand, Vector3.zero);
            AddCoastalFoam(root.transform, foam);
            for (int i = 0; i < 9; i++)
            {
                float x = -15f + i * 3.8f;
                float z = -10.7f - (i % 2) * 0.7f;
                var safe = ClampToIslandShoreline(new Vector2(x, z));
                // Shore rocks must share the same authored Height function as
                // the island surface. The old fixed y=0.12 placed the whole
                // row above the low beach band and made the rounded rocks
                // read as floating trees/arches in the Player.
                // The outermost sample can still fall outside the deformed
                // IslandPoint footprint even when Height(x,z) looks valid.
                // Clamp the presentation point before grounding it from the
                // owned D19_TerrainRoot collider; no second collider or
                // gameplay anchor is introduced.
                AddGroundedMesh(root.transform, "ShoreRock_" + i,
                    MakeRockMesh(0.65f + (i % 3) * 0.18f), stone,
                    new Vector3(safe.x, Height(safe.x, safe.y), safe.y));
            }
            return root;
        }

        private static void AddCoastalFoam(Transform parent, Material foam)
        {
            // Place short foam strokes around the same organic perimeter used
            // by the island mesh. The strokes stay outside the walkable
            // surface and are presentation-only; D2/D6 keep their existing
            // movement and collision ownership.
            const int segments = 28;
            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                float nextAngle = (i + 1) / (float)segments * Mathf.PI * 2f;
                var point = IslandPoint(Mathf.Cos(angle), Mathf.Sin(angle));
                var next = IslandPoint(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle));
                var tangent = next - point;
                float length = Mathf.Max(0.24f, tangent.magnitude * 0.22f);
                float yaw = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                // Foam belongs to the waterline, not to the terrain height.
                // Sampling Height here lifted several strokes by more than
                // half a metre on the ridge and made them read as cyan bars
                // floating above the beach in Player.
                var outward = point.normalized * 0.16f;
                var stroke = AddMesh(parent, "FoamRing_" + i,
                    MakeBevelBoxMesh(length, 0.018f, 0.055f, 0.012f), foam,
                    new Vector3(point.x + outward.x, 0.018f, point.y + outward.y));
                stroke.transform.localRotation = Quaternion.Euler(0f, -yaw, 0f);
            }
        }

        private static GameObject CreateRoadNetwork(Material earth, Material sand, Material stone)
        {
            var root = new GameObject("D19_R03_RoadNetwork");
            // Roads are route guides, not giant floor cards. Build each one
            // from world-space points so it follows the island's shallow
            // relief and keeps the frozen D6/D19 route anchors readable.
            AddMesh(root.transform, "MainRoad", MakePathMesh(new[]
            {
                new Vector2(0.0f, -7.0f), new Vector2(0.0f, -5.4f),
                new Vector2(0.0f, -3.2f), new Vector2(0.0f, -1.1f),
                new Vector2(1.6f, 0.6f), new Vector2(3.8f, 1.2f),
                new Vector2(6.0f, 1.2f)
            }, 1.75f, 0.045f), earth, Vector3.zero);
            AddMesh(root.transform, "WarehouseRoad", MakePathMesh(new[]
            {
                new Vector2(0.0f, 0.7f), new Vector2(-2.4f, 0.8f),
                new Vector2(-4.5f, 1.0f), new Vector2(-6.4f, 1.2f)
            }, 1.55f, 0.045f), earth, Vector3.zero);
            // Retain the approved R64 market crossing and its small route-edge
            // stones. These are visual route guides only; the single
            // D19_TerrainRoot remains the collision authority.
            AddMesh(root.transform, "MarketRoad", MakeRibbonMesh(12f, 0.08f, 2.8f), sand, new Vector3(1.2f, 0.18f, 1.2f));
            for (int i = 0; i < 14; i++)
            {
                float x = -1.4f + (i % 3) * 1.4f;
                float z = -10.0f + i * 1.35f;
                AddGroundedMesh(root.transform, "RoadEdge_" + i,
                    MakeBevelBoxMesh(0.15f, 0.10f, 1.2f, 0.03f), stone,
                    new Vector3(x, 0.25f, z));
            }
            return root;
        }

        private static GameObject CreateHouse(string name, Vector3 position, Vector3 size, Material wall, Material roof, Material trim, Material accent, Material ground, string label)
        {
            var root = new GameObject(name);
            root.transform.position = new Vector3(
                position.x,
                Height(position.x, position.z) - HouseFoundationBottomOffset,
                position.z);
            float w = size.x, h = size.y, d = size.z;
            AddMesh(root.transform, "Foundation", MakeBevelBoxMesh(w + 0.24f, 0.22f, d + 0.24f, 0.06f), ground, new Vector3(0f, 0.13f, 0f));
            AddMesh(root.transform, "Body", MakeBevelBoxMesh(w, h, d, 0.18f), wall, new Vector3(0f, h * 0.5f, 0f));
            // A longitudinal gable reads as a Caribbean harbor roof from both
            // the dock and market cameras. The previous four-sided pyramid
            // made every landmark look like the same flat proxy box.
            AddMesh(root.transform, "Roof", MakeGableRoofMesh(w + 0.45f, 0.62f, d + 0.45f), roof, new Vector3(0f, h + 0.08f, 0f));
            AddMesh(root.transform, "RoofRidge", MakeBevelBoxMesh(0.22f, 0.14f, d + 0.55f, 0.05f), roof, new Vector3(0f, h + 0.76f, 0f));
            AddMesh(root.transform, "RoofFrontTrim", MakeBevelBoxMesh(w + 0.5f, 0.12f, 0.18f, 0.04f), roof, new Vector3(0f, h + 0.10f, -d * 0.5f - 0.24f));
            AddMesh(root.transform, "Door", MakeBevelBoxMesh(0.9f, 1.75f, 0.12f, 0.05f), trim, new Vector3(0f, 0.88f, -d * 0.5f - 0.08f));
            AddMesh(root.transform, "DoorStep", MakeBevelBoxMesh(1.3f, 0.14f, 0.65f, 0.04f), ground, new Vector3(0f, 0.08f, -d * 0.5f - 0.36f));
            AddMesh(root.transform, "WindowL", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(-w * 0.28f, h * 0.57f, -d * 0.5f - 0.07f));
            AddMesh(root.transform, "WindowR", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(w * 0.28f, h * 0.57f, -d * 0.5f - 0.07f));
            AddMesh(root.transform, "Eave", MakeBoxMesh(w + 0.9f, 0.16f, 0.28f), trim, new Vector3(0f, h + 0.05f, -d * 0.5f - 0.20f));
            AddMesh(root.transform, "FrontCanopy", MakeRoofMesh(w * 0.58f, 0.22f, 0.62f), roof, new Vector3(0f, h * 0.72f, -d * 0.5f - 0.30f));
            AddMesh(root.transform, "CanopyPostL", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(-w * 0.26f, h * 0.26f, -d * 0.5f - 0.34f));
            AddMesh(root.transform, "CanopyPostR", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(w * 0.26f, h * 0.26f, -d * 0.5f - 0.34f));
            AddMesh(root.transform, "SignBoard", MakeBevelBoxMesh(1.9f, 0.48f, 0.10f, 0.05f), accent, new Vector3(0f, h * 0.64f, -d * 0.5f - 0.31f));
            AddText(root.transform, "Sign", label, new Vector3(0f, h * 0.64f, -d * 0.5f - 0.38f), 0.11f);
            // The initial Player camera can approach from either side of a
            // building. Keep both elevations legible without changing the
            // gameplay anchor or adding collision to this presentation-only
            // prefab.
            AddMesh(root.transform, "BackDoor", MakeBevelBoxMesh(0.9f, 1.75f, 0.12f, 0.05f), trim, new Vector3(0f, 0.88f, d * 0.5f + 0.08f));
            AddMesh(root.transform, "BackWindowL", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(-w * 0.28f, h * 0.57f, d * 0.5f + 0.07f));
            AddMesh(root.transform, "BackWindowR", MakeBevelBoxMesh(1.1f, 0.72f, 0.1f, 0.04f), accent, new Vector3(w * 0.28f, h * 0.57f, d * 0.5f + 0.07f));
            AddMesh(root.transform, "BackCanopy", MakeRoofMesh(w * 0.58f, 0.22f, 0.62f), roof, new Vector3(0f, h * 0.72f, d * 0.5f + 0.30f));
            AddMesh(root.transform, "BackCanopyPostL", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(-w * 0.26f, h * 0.26f, d * 0.5f + 0.34f));
            AddMesh(root.transform, "BackCanopyPostR", MakeCylinderMesh(0.055f, h * 0.52f, 8), trim, new Vector3(w * 0.26f, h * 0.26f, d * 0.5f + 0.34f));
            // Keep one readable landmark label per building.  A second label
            // on the rear facade becomes visible around the side at the
            // dock-facing oblique camera angle and reads as duplicated text.
            // The rear facade remains fully modeled; navigation and gameplay
            // anchors are owned elsewhere and are not affected by this
            // presentation-only cleanup.
            AddMesh(root.transform, "CrateL", MakeBevelBoxMesh(0.65f, 0.6f, 0.65f, 0.08f), trim, new Vector3(-w * 0.38f, 0.32f, -d * 0.5f - 0.55f));
            AddMesh(root.transform, "CrateR", MakeBevelBoxMesh(0.65f, 0.46f, 0.65f, 0.08f), ground, new Vector3(w * 0.38f, 0.24f, -d * 0.5f - 0.55f));
            AddFacadeDetails(root.transform, w, h, d, trim, accent);
            return root;
        }

        private static void AddFacadeDetails(Transform parent, float width, float height, float depth, Material trim, Material accent)
        {
            // Timber framing breaks up the large plaster/wood planes without
            // adding a second collider or changing any interaction anchor.
            for (int side = -1; side <= 1; side += 2)
            {
                float facadeZ = side * (depth * 0.5f + 0.11f);
                AddMesh(parent, "FacadePostFrontBack_" + side, MakeBevelBoxMesh(0.16f, height * 0.82f, 0.14f, 0.035f), trim,
                    new Vector3(-width * 0.43f, height * 0.42f, facadeZ));
                AddMesh(parent, "FacadePostFrontBackR_" + side, MakeBevelBoxMesh(0.16f, height * 0.82f, 0.14f, 0.035f), trim,
                    new Vector3(width * 0.43f, height * 0.42f, facadeZ));
                AddMesh(parent, "FacadeBeamFrontBack_" + side, MakeBevelBoxMesh(width * 0.86f, 0.16f, 0.14f, 0.035f), trim,
                    new Vector3(0f, height * 0.82f, facadeZ));
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float facadeX = side * (width * 0.5f + 0.11f);
                AddMesh(parent, "FacadeSidePost_" + side, MakeBevelBoxMesh(0.14f, height * 0.82f, 0.16f, 0.035f), trim,
                    new Vector3(facadeX, height * 0.42f, 0f));
                AddMesh(parent, "FacadeSideBeam_" + side, MakeBevelBoxMesh(0.14f, 0.16f, depth * 0.76f, 0.035f), trim,
                    new Vector3(facadeX, height * 0.82f, 0f));
                for (int zSide = -1; zSide <= 1; zSide += 2)
                {
                    AddMesh(parent, "SideShutter_" + side + "_" + zSide, MakeBevelBoxMesh(0.12f, 0.78f, 0.92f, 0.035f), accent,
                        new Vector3(facadeX + side * 0.025f, height * 0.56f, zSide * depth * 0.22f));
                }
            }
        }

        private static GameObject CreateDock(Material wood, Material woodLight, Material stone, Material rope)
        {
            var root = new GameObject("D19_R03_DockAndBerth");
            root.transform.position = new Vector3(-1f, 0.20f, -9.3f);
            AddMesh(root.transform, "Deck", MakeBevelBoxMesh(6.6f, 0.24f, 3.1f, 0.10f), woodLight, new Vector3(0f, 0.18f, 0f));
            for (int i = 0; i < 7; i++)
                AddMesh(root.transform, "Plank_" + i, MakeBoxMesh(0.10f, 0.10f, 2.9f), wood, new Vector3(-2.7f + i * 0.90f, 0.36f, 0f));
            for (int i = 0; i < 5; i++)
            {
                float x = -2.4f + i * 1.2f;
                AddMesh(root.transform, "Post_" + i, MakeCylinderMesh(0.16f, 1.35f, 8), wood, new Vector3(x, -0.35f, 1.45f));
                var ropeMesh = AddMesh(root.transform, "Rope_" + i, MakeCylinderMesh(0.04f, 1.8f, 6), rope, new Vector3(x + 0.35f, 0.55f, 1.35f));
                // A mooring line should read as a short, angled line between
                // the post and the berth, not as a pale vertical pole.
                ropeMesh.transform.localRotation = Quaternion.Euler(0f, 0f, 62f + (i % 2) * 8f);
            }
            AddMesh(root.transform, "BerthStone", MakeBevelBoxMesh(7.3f, 0.4f, 0.55f, 0.16f), stone, new Vector3(0f, -0.02f, 1.7f));
            AddMesh(root.transform, "Ramp", MakeWedgeMesh(1.9f, 0.5f, 3.4f), wood, new Vector3(0f, 0.18f, 3.0f));
            return root;
        }

        private static GameObject CreateVegetation(Material wood, Material leaf, Material grass, Material stone)
        {
            var root = new GameObject("D19_R03_VegetationAndRocks");
            // Keep the authored palms outside the fixed market camera's near
            // frustum.  The old east-side positions put trunks almost on top
            // of the camera and made an otherwise valid shot read as a pair
            // of giant foreground poles.  This is presentation-only layout;
            // gameplay anchors, walkable geometry and domain state are owned
            // elsewhere and remain unchanged.
            // Retain the six approved R66 palm render groups, but keep all of
            // them on the west perimeter.  Scattering a canopy through the
            // north/east service area made the fixed tavern camera intersect
            // a tree even after the canopy was made volumetric. This is
            // presentation-only placement; no gameplay anchor, collider or
            // ownership contract is changed.
            var positions = new[]
            {
                // Keep every root well inside the organic island footprint.
                // Positions near the perimeter can match Height() while still
                // landing outside the rendered island after IslandPoint's
                // radial deformation, which reads as a floating tree in the
                // Player. These west-interior positions leave the route clear
                // and give the trunk a visible ground contact in fixed shots.
                new Vector3(-10.5f, 0.2f, -6.5f),
                new Vector3(-11.0f, 0.2f, -1.0f),
                new Vector3(-10.5f, 0.2f, 4.5f),
                new Vector3(-10.0f, 0.2f, 9.0f),
                new Vector3(-8.5f, 0.2f, 11.5f),
                new Vector3(-5.5f, 0.2f, 13.0f)
            };
            if (RenderAuthoredPalms)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    var position = positions[i];
                    // Bury the low-poly trunk base slightly into the authored
                    // surface so a shallow slope or skirt seam cannot read as a
                    // floating tree in the Player camera.
                    position.y = Height(position.x, position.z) - PalmRootBurial;
                    var palm = CreatePalm("Palm_" + i, wood, leaf, position, 0.9f + (i % 3) * 0.14f);
                    palm.transform.SetParent(root.transform, false);
                }
            }
            for (int i = 0; i < 14; i++)
            {
                float x = -14f + (i * 3.1f) % 28f;
                float z = 14.5f + (i % 3) * 1.0f;
                AddMesh(root.transform, "Shrub_" + i, MakeShrubMesh(0.32f + (i % 3) * 0.06f), grass,
                    GroundVegetationPosition(x, z));
            }
            for (int i = 0; i < 10; i++)
            {
                float x = -14f + i * 2.8f;
                float z = 15.0f + (i % 2) * 1.0f;
                AddMesh(root.transform, "Rock_" + i, MakeRockMesh(0.48f + (i % 3) * 0.12f), stone,
                    GroundVegetationPosition(x, z));
            }
            return root;
        }

        private static Vector3 GroundVegetationPosition(float x, float z)
        {
            // The generated island is an organic ellipse, not the rectangular
            // 26x26 authoring grid.  A position can therefore match Height()
            // while still landing outside the visible island skirt; the mesh
            // then reads as a floating shrub/rock in the Player.  Keep the
            // presentation-only vegetation inside a conservative interior
            // band before sampling the same authored height function.
            var safe = ClampToIslandInterior(new Vector2(x, z));
            return new Vector3(safe.x, Height(safe.x, safe.y) + 0.02f, safe.y);
        }

        private static Vector2 ClampToIslandInterior(Vector2 position)
        {
            const float safeRadius = 0.80f;
            float normalizedX = position.x / IslandRadiusX;
            float normalizedZ = position.y / IslandRadiusZ;
            float radius = Mathf.Sqrt(normalizedX * normalizedX + normalizedZ * normalizedZ);
            if (radius <= safeRadius || radius <= 0.0001f)
                return position;

            float scale = safeRadius / radius;
            return new Vector2(
                normalizedX * scale * IslandRadiusX,
                normalizedZ * scale * IslandRadiusZ);
        }

        private static Vector2 ClampToIslandShoreline(Vector2 position)
        {
            // Keep shoreline dressing near the island edge while leaving a
            // conservative margin for the irregular IslandPoint boundary.
            // The analytical ellipse alone is not the rendered footprint:
            // IslandPoint applies radial deformation at each boundary angle.
            const float safeRadius = 0.94f;
            float normalizedX = position.x / IslandRadiusX;
            float normalizedZ = position.y / IslandRadiusZ;
            float radius = Mathf.Sqrt(normalizedX * normalizedX + normalizedZ * normalizedZ);
            var candidate = radius <= safeRadius || radius <= 0.0001f
                ? position
                : new Vector2(
                    normalizedX * safeRadius / radius * IslandRadiusX,
                    normalizedZ * safeRadius / radius * IslandRadiusZ);

            // IslandPoint's angular deformation can still leave a point just
            // outside the actual MeshCollider at one boundary angle. Walk the
            // presentation point inward only when the owned collider confirms
            // that this specific point has no support; interior rocks keep
            // their authored coordinates.
            for (int attempt = 0; _buildGroundCollider != null && attempt < 12; attempt++)
            {
                var ray = new Ray(new Vector3(candidate.x, 64f, candidate.y), Vector3.down);
                if (_buildGroundCollider.Raycast(ray, out _, 128f))
                    break;
                candidate = Vector2.Lerp(candidate, Vector2.zero, 0.06f);
            }
            return candidate;
        }

        private static GameObject CreatePortProps(Material wood, Material woodLight, Material stone, Material sand)
        {
            var root = new GameObject("D19_R03_PortProps");
            for (int i = 0; i < 7; i++)
            {
                float x = -7.0f + i * 2.2f;
                float z = -2.0f + (i % 2) * 0.42f;
                AddGroundedMesh(root.transform, "Barrel_" + i,
                    MakeCylinderMesh(0.34f, 0.68f, 10), wood, new Vector3(x, 0.52f, z));
            }
            float noticeGroundY = GroundHeight(3.2f, -1.4f);
            AddGroundedMesh(root.transform, "NoticeBoard", MakeBevelBoxMesh(1.5f, 1.8f, 0.18f, 0.06f), woodLight,
                new Vector3(3.2f, noticeGroundY + 0.92f, -1.4f));
            AddGroundedMesh(root.transform, "NoticePost", MakeCylinderMesh(0.08f, 1.4f, 8), stone,
                new Vector3(3.2f, noticeGroundY + 0.70f, -1.4f));
            for (int i = 0; i < 4; i++)
            {
                float x = 2.0f + i * 0.05f;
                float z = 2.4f + i * 0.55f;
                AddGroundedMesh(root.transform, "StoneStep_" + i,
                    MakeBevelBoxMesh(2.3f, 0.18f, 0.8f, 0.05f), sand,
                    new Vector3(x, 0.15f + i * 0.16f, z));
            }
            for (int i = 0; i < 3; i++)
            {
                float x = 3.0f + i * 2.1f;
                // The three parts share one authored base. Shift the whole
                // stall by the terrain height instead of grounding each part
                // independently; this keeps the wall, counter and roof in a
                // coherent stack while preventing the wall from sinking into
                // or the roof from hovering above the relief.
                float groundShift = Height(x, -0.75f) - 0.15f;
                AddMesh(root.transform, "MarketStallWall_" + i, MakeBevelBoxMesh(1.55f, 1.2f, 0.16f, 0.06f), woodLight, new Vector3(x, 0.75f + groundShift, -0.75f));
                AddMesh(root.transform, "MarketStallRoof_" + i, MakeRoofMesh(2.0f, 0.42f, 1.25f), sand, new Vector3(x, 1.55f + groundShift, -0.75f));
                float counterZ = -1.18f;
                AddMesh(root.transform, "MarketStallCounter_" + i, MakeBevelBoxMesh(1.75f, 0.18f, 0.65f, 0.04f), wood, new Vector3(x, 0.95f + groundShift, counterZ));
            }
            return root;
        }

        private static GameObject CreateMarketEntry(Material wood, Material woodLight, Material stone, Material sand)
        {
            var root = new GameObject("D19_R03_MarketEntryAndStreet");
            // A clear, low-profile arrival marker gives the player a readable
            // first landmark while leaving the frozen road and interaction
            // anchors untouched.
            // Keep the marker beyond the initial player spawn. The previous
            // zero root position put EntryBeam/EntryRoof at z=-2.4, only
            // 0.30m from D2_PlayerAvatar and directly in the first camera,
            // which read as a foreground slab rather than a town entrance.
            root.transform.position = new Vector3(0f, Height(0f, 4.2f) - 0.02f, 4.2f);
            // A low sign frame keeps the street legible while leaving the
            // frozen market approach and the player camera unobstructed.
            AddMesh(root.transform, "EntryPillarL", MakeBevelBoxMesh(0.26f, 1.35f, 0.26f, 0.04f), stone, new Vector3(-2.55f, 0.70f, -2.4f));
            AddMesh(root.transform, "EntryPillarR", MakeBevelBoxMesh(0.26f, 1.35f, 0.26f, 0.04f), stone, new Vector3(2.55f, 0.70f, -2.4f));
            AddMesh(root.transform, "EntryBeam", MakeBevelBoxMesh(5.35f, 0.16f, 0.22f, 0.04f), woodLight, new Vector3(0f, 1.42f, -2.4f));
            AddMesh(root.transform, "EntryRoof", MakeRoofMesh(5.55f, 0.20f, 0.68f), wood, new Vector3(0f, 1.60f, -2.4f));
            AddMesh(root.transform, "EntrySignBoard", MakeBevelBoxMesh(2.0f, 0.34f, 0.10f, 0.04f), sand, new Vector3(0f, 1.15f, -2.58f));
            AddText(root.transform, "EntrySign", "MARKET STREET", new Vector3(0f, 1.15f, -2.67f), 0.085f);
            for (int i = 0; i < 4; i++)
            {
                float x = -4.8f + i * 3.2f;
                AddGroundedMesh(root.transform, "StreetStone_" + i,
                    MakeBevelBoxMesh(0.85f, 0.06f, 0.42f, 0.04f), stone,
                    new Vector3(x, 0.22f, -3.9f));
            }
            return root;
        }

        private static GameObject CreateHarborBackdrop(Material earth, Material stone, Material grass, Material leaf)
        {
            var root = new GameObject("D19_R03_HarborBackdrop");
            // Keep this root empty until an offshore asset has a verified
            // camera-facing ground/water contract. The previous generated
            // low-poly rock row put BackdropRock_2 directly in the swimming
            // route and its shoreline seam read as a hollow floating ring.
            // An empty presentation root is safer than shipping that defect;
            // resource candidates remain tracked by the intake audit.
            return root;
        }

        private static void AddCandidateDetails(Transform parent, Material wood, Material leaf, Material stone, Material grass)
        {
            // These are controlled, presentation-only uses of candidates that
            // are intake_eligible for D19-A01/D19-A03. The source files remain
            // Candidate/ImportVerified; this builder does not promote resource
            // state or create runtime ownership. Keep the authored palms as a
            // fallback and use the imported modules to break up the repeated
            // generated silhouettes in the Player camera.
            // Retain only candidates whose imported pivot/material layout is
            // visually safe in the current harbor frame. The Nature Kit palm
            // passed a bounded import audit: one renderer, one mesh, a zero
            // ground-contact bound and explicit bark/leaf material slots. It
            // is still a Candidate/ImportVerified presentation detail rather
            // than a formal LocalDevelopmentReady resource. Keep the
            // placement single-instance and let AddCandidate correct the
            // actual renderer bound against the authored R03 surface.
            // The same rule applies to grass_large.fbx: its imported leaf
            // cards become oversized foreground triangles from the east
            // tavern camera, so it stays catalogued but is not instantiated.
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/rock_largeF.fbx", "KenneyRockShore", new Vector3(-14.0f, 0.25f, -10.8f), new Vector3(0.95f, 0.95f, 0.95f), 10f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/ground_pathBend.fbx", "KenneyPathBendMarket", new Vector3(0.0f, 0.28f, -3.6f), new Vector3(0.92f, 0.92f, 0.92f), 0f, wood, leaf, stone, grass);
            AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palm.fbx", "KenneyNaturePalmCandidate", new Vector3(-8.6f, 0.20f, 7.8f), Vector3.one * 2.0f, -18f, wood, leaf, stone, grass);
            // rock_tallF has no safe ground-contact/pivot contract in the
            // current import.  In the Player it becomes a detached vertical
            // slab at the east edge and reads as a floating tree/trunk. Keep
            // it catalogued in the audit, but do not instantiate it until a
            // pivot-safe import is admitted. The two north-west/north-east
            // cliff candidates above are likewise omitted because the live
            // R03 collider has no support at their former positions.
        }

        private static GameObject BuildShipPrefab(Material wood, Material woodLight, Material stone, Material foam, Material sail)
        {
            var root = new GameObject("D19_R03_ShipVisual");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            // The imported candidate is intentionally kept presentation-only,
            // but it has no visible steering part. Use a warm, low-sheen wood
            // material for the generated wheel so the Player reads a wooden
            // helm instead of a black pot-like ring that occludes the operator.
            var helm = Material("R03_HelmWheel", new Color(0.34f, 0.16f, 0.055f, 1f), 0.0f, 0.16f);

            // Use the verified Kenney pirate ship as the primary presentation
            // model.  The generated mesh remains an explicit fallback so a
            // missing candidate import never removes the visible ship.  The
            // candidate is visual-only; D2/D6 keep ownership of the stable
            // ship node, movement and collision below.
            var candidate = AddShipCandidate(root.transform, PirateShipCandidatePath,
                wood, woodLight, stone, foam, sail);
            if (candidate == null)
            {
                AddGeneratedShipFallback(root.transform, wood, woodLight, stone, sail, helm);
            }
            else
            {
                // The imported pirate ship is a single presentation mesh and
                // does not expose a stable helm child.  D7's gameplay HelmPoint
                // is intentionally authored at the stern of this visual root;
                // add a small, named visual control if the candidate has no
                // semantic wheel/rudder node so boarding cannot read as the
                // player steering empty air or an unrelated prop.
                EnsureVisibleHelm(root.transform, helm, woodLight);
            }

            var collision = new GameObject("D6_ShipHullCollision");
            collision.transform.SetParent(root.transform, false);
            collision.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            var box = collision.AddComponent<BoxCollider>();
            box.size = new Vector3(4.5f, 0.84f, 7.2f);
            box.isTrigger = false;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ShipPrefabPath, out bool saved);
            if (!saved || prefab == null)
                throw new InvalidOperationException("Could not save R03 ship visual prefab: " + ShipPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void EnsureVisibleHelm(Transform parent, Material helm, Material supportWood)
        {
            bool hasVisibleSteeringNode = false;
            var transforms = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                var current = transforms[i];
                string lower = current.name.ToLowerInvariant();
                bool semanticName = lower.Contains("helm") || lower.Contains("rudder") ||
                    lower.Contains("steering") || lower.Contains("wheel") || lower.Contains("tiller");
                if (!semanticName) continue;

                var renderers = current.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    if (renderers[rendererIndex] != null && renderers[rendererIndex].enabled)
                    {
                        hasVisibleSteeringNode = true;
                        break;
                    }
                }
                if (hasVisibleSteeringNode) break;
            }

            // Keep an explicit anchor even when the candidate has an opaque
            // wheel mesh.  D7 and runtime diagnostics can then share one
            // stable, presentation-owned reference without touching control
            // or navigation ownership.
            var anchor = parent.Find("D7_VisibleHelmAnchor");
            if (anchor == null)
            {
                var anchorObject = new GameObject("D7_VisibleHelmAnchor");
                anchor = anchorObject.transform;
                anchor.SetParent(parent, false);
            }
            // The ship root's authored initial heading is 180 degrees.  Put
            // the gameplay standing point just behind the wheel's centerline:
            // the old -1.78m anchor left the player 0.67m ahead of the visible
            // wheel, so the fallback hands stopped behind the control prop.
            // D7 consumes this presentation-owned point; it does not rewrite
            // the visual hierarchy or invent a second helm position.
            anchor.localPosition = new Vector3(0f, 1.10f, -2.12f);
            anchor.localRotation = Quaternion.identity;

            if (hasVisibleSteeringNode)
            {
                // Candidate meshes may already publish a semantic HelmStand /
                // HelmWheel node, but their imported fallback material can
                // still be the generic stone material.  Keep the candidate
                // geometry and its authored transform; only normalize the
                // material owner so the control reads as part of the wooden
                // ship instead of as a stone pot or floating prop.
                ApplyVisibleHelmMaterials(parent, helm, supportWood);
                return;
            }

            // The procedural fallback's calibrated hands sit at approximately
            // y=1.89m after the visual-root scale is applied.  The previous
            // wheel center y=1.72m left the ring below the hands in the
            // close Player camera, so the character read as standing over an
            // unrelated prop.  Keep this presentation-only correction in the
            // same generated helm owner and extend the stand to meet the
            // raised wheel; D2/D7 gameplay anchors remain unchanged.
            const float generatedHelmWheelY = 1.89f;
            AddMesh(parent, "HelmStand", MakeBevelBoxMesh(0.34f, 0.82f, 0.34f, 0.06f),
                supportWood, new Vector3(0f, 1.10f, -2.45f));
            var wheel = AddMesh(parent, "HelmWheel", MakeHelmWheelMesh(0.38f, 0.10f, 12, 4),
                helm, new Vector3(0f, generatedHelmWheelY, -2.45f));
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var hub = AddMesh(parent, "HelmWheelHub", MakeCappedCylinderMesh(0.09f, 0.14f, 10),
                supportWood, new Vector3(0f, generatedHelmWheelY, -2.45f));
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void ApplyVisibleHelmMaterials(Transform parent, Material helm, Material supportWood)
        {
            var transforms = parent.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                var current = transforms[i];
                string lower = current.name.ToLowerInvariant();
                bool supportNode = lower.Contains("stand") || lower.Contains("pedestal") ||
                    lower.Contains("base");
                bool wheelNode = !supportNode && (lower.Contains("helm") || lower.Contains("rudder") ||
                    lower.Contains("steering") || lower.Contains("wheel") || lower.Contains("tiller"));
                if (!supportNode && !wheelNode) continue;

                var material = supportNode ? supportWood : helm;
                if (material == null) continue;
                var renderers = current.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    var renderer = renderers[rendererIndex];
                    if (renderer != null && renderer.enabled)
                        renderer.sharedMaterial = material;
                }
            }
        }

        private static void AddGeneratedShipFallback(Transform parent, Material wood, Material woodLight, Material stone, Material sail, Material helm)
        {
            AddMesh(parent, "Hull", MakeBevelBoxMesh(4.5f, 0.72f, 7.2f, 0.24f), wood, new Vector3(0f, 0.38f, 0f));
            AddMesh(parent, "Deck", MakeBevelBoxMesh(3.75f, 0.16f, 5.85f, 0.08f), woodLight, new Vector3(0f, 0.82f, -0.05f));
            AddMesh(parent, "Bow", MakeWedgeMesh(3.75f, 0.68f, 2.20f), woodLight, new Vector3(0f, 0.50f, 3.60f));
            AddMesh(parent, "SternRail", MakeBevelBoxMesh(3.55f, 0.12f, 0.18f, 0.04f), stone, new Vector3(0f, 1.08f, -2.80f));
            AddMesh(parent, "Mast", MakeCylinderMesh(0.13f, 3.35f, 10), wood, new Vector3(0f, 2.15f, -0.35f));
            AddMesh(parent, "Boom", MakeCylinderMesh(0.06f, 2.60f, 8), wood, new Vector3(0f, 2.20f, 0.80f)).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            AddMesh(parent, "Sail", MakeRibbonMesh(2.20f, 2.25f, 0.06f), sail, new Vector3(0f, 2.15f, 0.30f));
            AddMesh(parent, "HelmStand", MakeBevelBoxMesh(0.34f, 0.70f, 0.34f, 0.06f), woodLight, new Vector3(0f, 1.10f, 2.45f));
            var wheel = AddMesh(parent, "HelmWheel", MakeHelmWheelMesh(0.38f, 0.10f, 12, 4), helm, new Vector3(0f, 1.72f, 2.45f));
            wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var hub = AddMesh(parent, "HelmWheelHub", MakeCappedCylinderMesh(0.09f, 0.14f, 10), woodLight, new Vector3(0f, 1.72f, 2.45f));
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int i = 0; i < 4; i++)
            {
                float x = -1.65f + i * 1.10f;
                AddMesh(parent, "PortRail_" + i, MakeCylinderMesh(0.045f, 0.55f, 8), stone, new Vector3(x, 1.08f, -1.95f));
                AddMesh(parent, "StarboardRail_" + i, MakeCylinderMesh(0.045f, 0.55f, 8), stone, new Vector3(x, 1.08f, 1.95f));
            }
        }

        private static GameObject AddShipCandidate(Transform parent, string path,
            Material wood, Material woodLight, Material stone, Material foam, Material sail)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogWarning("[D19-R03-A02] Ship candidate missing; using generated fallback: " + path);
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null)
            {
                Debug.LogWarning("[D19-R03-A02] Ship candidate could not be instantiated; using generated fallback: " + path);
                return null;
            }

            instance.name = "KenneyPirateShipPresentation";
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            var bounds = RendererBounds(instance);
            float footprint = Mathf.Max(bounds.size.x, bounds.size.z);
            if (footprint <= 0.001f)
            {
                UnityEngine.Object.DestroyImmediate(instance);
                Debug.LogWarning("[D19-R03-A02] Ship candidate has no renderer bounds; using generated fallback: " + path);
                return null;
            }

            float scale = ShipVisualFootprint / footprint;
            instance.transform.localScale = Vector3.one * scale;
            bounds = RendererBounds(instance);
            instance.transform.localPosition = new Vector3(
                -bounds.center.x,
                0.06f - bounds.min.y,
                -bounds.center.z);

            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderer.sharedMaterial = ShipFallbackMaterial(renderer.name, wood, woodLight, stone, foam, sail);
                }
                else
                {
                    for (int i = 0; i < materials.Length; i++)
                    {
                        var source = materials[i];
                        bool untexturedColormap = source != null
                            && source.mainTexture == null
                            && source.name.IndexOf("colormap", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (source == null || untexturedColormap)
                            materials[i] = ShipFallbackMaterial(renderer.name, wood, woodLight, stone, foam, sail);
                    }
                    renderer.sharedMaterials = materials;
                }
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            return instance;
        }

        private static Material ShipFallbackMaterial(string rendererName,
            Material wood, Material woodLight, Material stone, Material foam, Material sail)
        {
            string lower = (rendererName ?? string.Empty).ToLowerInvariant();
            if (lower.Contains("sail") || lower.Contains("flag")) return sail;
            if (lower.Contains("mast") || lower.Contains("rope") || lower.Contains("wood")) return wood;
            if (lower.Contains("metal") || lower.Contains("cannon") || lower.Contains("wheel")) return stone;
            if (lower.Contains("water") || lower.Contains("foam")) return foam;
            return woodLight;
        }

        private static Bounds RendererBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static void InstallShipPresentation(Scene scene, GameObject shipPrefab)
        {
            if (shipPrefab == null) return;
            var ship = FindSceneObject(scene, "ph_ship_fishing");
            if (ship == null)
            {
                Debug.LogWarning("[D19-R03-A02] Stable ph_ship_fishing node was not found; ship visual was not installed");
                return;
            }

            // This is an authored initial-state contract, not a runtime
            // movement override.  D2ShipController initializes its sailing
            // heading from this root, so the root must point along the same
            // axis as the visible candidate before any player input is read.
            ship.transform.rotation = Quaternion.Euler(0f, InitialShipHeading, 0f);

            foreach (var child in ship.transform)
            {
                var childTransform = child as Transform;
                if (childTransform != null && childTransform.name == "D19_R03_ShipVisualInstance")
                    UnityEngine.Object.DestroyImmediate(childTransform.gameObject);
            }

            foreach (var renderer in ship.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.transform == ship.transform || renderer.transform.IsChildOf(ship.transform))
                    renderer.enabled = false;
            }

            var instance = PrefabUtility.InstantiatePrefab(shipPrefab, scene) as GameObject;
            if (instance == null) throw new InvalidOperationException("Could not instantiate " + ShipPrefabPath);
            instance.name = "D19_R03_ShipVisualInstance";
            instance.transform.SetParent(ship.transform, false);
            // The stable ship node remains the boarding/helm anchor.  Offset
            // only the visual child to the south berth so its hull no longer
            // cuts through the market street and service-building sightlines.
            instance.transform.localPosition = ship.transform.InverseTransformVector(ShipVisualBerthOffset);
            instance.transform.localRotation = Quaternion.identity;
            var worldScale = ship.transform.lossyScale;
            instance.transform.localScale = new Vector3(
                1f / Mathf.Max(0.001f, Mathf.Abs(worldScale.x)),
                1f / Mathf.Max(0.001f, Mathf.Abs(worldScale.y)),
                1f / Mathf.Max(0.001f, Mathf.Abs(worldScale.z)));
            EnsureShipAnchor(ship.transform, "D2_PlayerSpawnAnchor", PlayerSpawnWorldOffset);
            EnsureShipAnchor(ship.transform, "D2_DisembarkAnchor", DisembarkWorldOffset);
            Debug.Log("[D19-R03-A02] Installed presentation-only ship visual under stable ph_ship_fishing; D2/D6 ownership preserved");
        }

        private static void EnsureShipAnchor(Transform ship, string name, Vector3 worldOffset)
        {
            var anchor = ship.Find(name);
            if (anchor == null)
            {
                var go = new GameObject(name);
                anchor = go.transform;
                anchor.SetParent(ship, false);
            }

            anchor.localPosition = ship.InverseTransformVector(worldOffset);
            anchor.localRotation = Quaternion.identity;
            anchor.localScale = Vector3.one;
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (transform.name == name) return transform.gameObject;
            }
            return null;
        }

        private static GameObject AddCandidate(Transform parent, string path, string name, Vector3 position, Vector3 scale, float yaw, Material wood, Material leaf, Material stone, Material grass)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return null;
            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) return null;
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = scale;
            // Candidate FBX pivots are not consistent. Correct the placed
            // root from its actual renderer bounds so rocks/trees cannot
            // float above or sink below the authored harbor surface. Prefer
            // the exact build MeshCollider; Height() remains the fallback
            // only when the surface collider is unavailable.
            var placedBounds = RendererBounds(instance);
            if (placedBounds.size.y > 0.001f)
            {
                float targetGroundY = GroundHeight(instance.transform.position.x, instance.transform.position.z) + 0.02f;
                instance.transform.position += Vector3.up * (targetGroundY - placedBounds.min.y);
            }
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var source = renderer.sharedMaterials;
                var mapped = new Material[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    string materialName = source[i] == null ? string.Empty : source[i].name.ToLowerInvariant();
                    string lowerName = name.ToLowerInvariant();
                    bool botanical = lowerName.Contains("tree") || lowerName.Contains("grass");
                    mapped[i] = materialName.Contains("leaf") ? leaf
                        : materialName.Contains("wood") ? wood
                        : materialName.Contains("grass") ? grass
                        : botanical ? leaf
                        : stone;
                }
                renderer.sharedMaterials = mapped;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            return instance;
        }

        private static void AddPiratePortCandidateDetails(Transform parent, Material wood, Material woodLight, Material stone, Material stoneLight, Material sail)
        {
            // Restore the approved R66 port-detail group as presentation-only
            // geometry. AddCandidatePreserveMaterials removes imported
            // colliders and preserves source materials, with an explicit R03
            // fallback only for missing or colormap-only slots. D2/D6 retain
            // sole ownership of movement, collision and ship facts.
            var details = new GameObject("D19_R03_KenneyPiratePortDetails");
            details.transform.SetParent(parent, false);
            AddCandidatePreserveMaterials(details.transform, PirateTowerCandidatePath, "KenneyPirateTower",
                // The previous (-15, 13) placement sat beyond the northwest
                // island edge.  A real downward ray there found no
                // D19_TerrainRoot support, so the imported tower read as a
                // floating tree/column from the swimming camera.  Keep the
                // candidate presentation-only, but place its footprint on
                // the authored island surface.
                new Vector3(-12.8f, 0.20f, 11.8f), Vector3.one * 0.80f, 18f, stoneLight);
            AddCandidatePreserveMaterials(details.transform, PirateDockCandidatePath, "KenneyPirateDock",
                new Vector3(-10.0f, 0.20f, -8.5f), Vector3.one, -12f, woodLight);
            AddCandidatePreserveMaterials(details.transform, PirateBoatCandidatePath, "KenneyPirateBoat",
                new Vector3(7.0f, 0.04f, -11.0f), Vector3.one * 0.80f, 15f, wood);
            AddCandidatePreserveMaterials(details.transform, PirateCannonCandidatePath, "KenneyPirateCannon",
                new Vector3(12.0f, 0.20f, -4.0f), Vector3.one * 0.80f, 40f, stone);
            AddCandidatePreserveMaterials(details.transform, PirateCrateCandidatePath, "KenneyPirateCrates",
                new Vector3(5.5f, 0.20f, -4.0f), Vector3.one * 0.90f, 0f, woodLight);
            AddCandidatePreserveMaterials(details.transform, PirateFlagCandidatePath, "KenneyPirateFlag",
                new Vector3(-5.0f, 0.20f, 13.5f), Vector3.one * 0.90f, -18f, sail);
        }

        private static GameObject AddCandidatePreserveMaterials(Transform parent, string path, string name,
            Vector3 position, Vector3 scale, float yaw, Material fallback)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogWarning("[D19-R03-A02] Pirate port candidate missing; retaining generated presentation: " + path);
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null)
            {
                Debug.LogWarning("[D19-R03-A02] Pirate port candidate could not be instantiated; retaining generated presentation: " + path);
                return null;
            }

            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            instance.transform.localScale = scale;
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    renderer.sharedMaterial = fallback;
                }
                else
                {
                    for (int i = 0; i < materials.Length; i++)
                    {
                        var source = materials[i];
                        bool untexturedColormap = source != null
                            && source.mainTexture == null
                            && source.name.IndexOf("colormap", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (source == null || untexturedColormap) materials[i] = fallback;
                    }
                    renderer.sharedMaterials = materials;
                }
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            // Pirate-kit prefabs use different pivots.  Ground them from the
            // actual visible bounds so the presentation cannot float or sink
            // merely because a source root pivot is above the mesh. Use the
            // same exact collider contact as the primary candidate path.
            var placedBounds = RendererBounds(instance);
            if (placedBounds.size.y > 0.001f)
            {
                float targetGroundY = GroundHeight(instance.transform.position.x, instance.transform.position.z) + 0.02f;
                instance.transform.position += Vector3.up * (targetGroundY - placedBounds.min.y);
            }
            return instance;
        }

        private static string[] PiratePortCandidatePaths()
        {
            return new[]
            {
                PirateTowerCandidatePath,
                PirateDockCandidatePath,
                PirateBoatCandidatePath,
                PirateCannonCandidatePath,
                PirateCrateCandidatePath,
                PirateFlagCandidatePath
            };
        }

        private static GameObject CreatePalm(string name, Material wood, Material leaf, Vector3 position, float scale)
        {
            var root = new GameObject(name);
            root.transform.localPosition = position;
            root.transform.localScale = Vector3.one * scale;
            // A narrower trunk preserves the palm silhouette without turning
            // a close fixed camera into an opaque vertical foreground wall.
            AddMesh(root.transform, "Trunk", MakeCylinderMesh(0.14f, 4.1f, 8), wood, new Vector3(0f, 2.05f, 0f));
            // Individual flat frond cards read as detached green triangles in
            // perspective whenever a fixed camera sees only one side of the
            // tree. Use one closed low-poly canopy instead: it remains a
            // deliberately simple asset, but its silhouette is volumetric,
            // grounded on the trunk, and cannot turn into a floating card.
            AddMesh(root.transform, "Canopy", MakeCanopyMesh(0.88f, 1.25f), leaf,
                new Vector3(0f, 4.12f, 0f));
            return root;
        }

        private static void AddText(Transform parent, string name, string text, Vector3 position, float size)
        {
            var go = new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            // TextMesh is already oriented toward the camera-facing side of
            // this presentation prefab. Do not rotate it around Y here:
            // doing so mirrors the -Z facade labels in the Player.
            go.transform.localRotation = Quaternion.identity;
            go.AddComponent<Whalebone.Gameplay.D19.R03.D19R03LandmarkLabelVisibility>();
            var mesh = go.GetComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size * 0.72f;
            mesh.fontSize = 32;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.12f, 0.06f, 0.025f, 1f);
        }

        private static GameObject AddMesh(Transform parent, string name, Mesh mesh, Material material, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var filter = go.AddComponent<MeshFilter>();
            if (mesh != null)
            {
                string safeName = name.Replace("/", "_").Replace("\\", "_").Replace(" ", "_");
                string meshPath = MeshDirectory + "/D19_R03_" + _meshSequence.ToString("D4") + "_" + safeName + ".asset";
                _meshSequence++;
                AssetDatabase.DeleteAsset(meshPath);
                AssetDatabase.CreateAsset(mesh, meshPath);
                filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            }
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            return go;
        }

        private static GameObject AddGroundedMesh(Transform parent, string name, Mesh mesh,
            Material material, Vector3 position)
        {
            var go = AddMesh(parent, name, mesh, material, position);
            var bounds = RendererBounds(go);
            if (bounds.size.y > 0.001f)
            {
                float targetGroundY = GroundHeight(go.transform.position.x, go.transform.position.z) + 0.02f;
                go.transform.position += Vector3.up * (targetGroundY - bounds.min.y);
            }
            return go;
        }

        private static float GroundHeight(float x, float z)
        {
            if (_buildGroundCollider != null)
            {
                var ray = new Ray(new Vector3(x, 64f, z), Vector3.down);
                if (_buildGroundCollider.Raycast(ray, out var hit, 128f))
                    return hit.point.y;
            }
            return Height(x, z);
        }

        private static Material Material(string name, Color color, float metallic, float smoothness)
        {
            string path = MaterialDirectory + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            ConfigurePresentationMaterial(material, name);
            // R03's small procedural presentation meshes intentionally share
            // low-poly modules whose winding is not guaranteed on every
            // elevation. Double-sided display keeps the Player camera from
            // turning a complete facade into an apparently empty grey block.
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 2f);
            return material;
        }

        private static void ConfigurePresentationMaterial(Material material, string name)
        {
            // Plaster is an authored color material, not a terrain surface.
            // Clear maps left by older R35/R03 builds so market and tavern
            // walls do not render as a single muddy rock texture.  The color
            // assigned by Material(...) remains the source of truth here.
            if (string.Equals(name, "R03_Plaster", StringComparison.Ordinal))
            {
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", null);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", null);
                if (material.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", null);
                return;
            }

            if (string.Equals(name, "R03_Teal", StringComparison.Ordinal))
            {
                // Water is intentionally opaque and lightly specular in the
                // URP fallback path. The wave mesh supplies the shape; these
                // values make its lighting read as water instead of a dark
                // unlit rectangle without introducing a custom shader.
                if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 0f);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.05f);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.78f);
                if (material.HasProperty("_SpecularHighlights")) material.SetFloat("_SpecularHighlights", 1f);
                return;
            }

            string baseMapPath = PresentationBaseMapPath(name);
            if (string.IsNullOrEmpty(baseMapPath)) return;

            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(baseMapPath);
            if (baseMap == null)
            {
                Debug.LogWarning("[D19-R03/Materials] Candidate base map missing, keeping color fallback: " + baseMapPath);
                return;
            }

            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", baseMap);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", baseMap);
            material.mainTexture = baseMap;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);

            string normalMapPath = PresentationNormalMapPath(name);
            if (!string.IsNullOrEmpty(normalMapPath))
            {
                var normalMap = LoadNormalMap(normalMapPath);
                if (normalMap != null && material.HasProperty("_BumpMap"))
                {
                    material.SetTexture("_BumpMap", normalMap);
                    if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 0.32f);
                }
            }
        }

        private static string PresentationBaseMapPath(string name)
        {
            switch (name)
            {
                case "R03_Grass":
                case "R03_GrassLight":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_grass_rock/aerial_grass_rock_albedo.jpg";
                case "R03_Earth":
                case "R03_Plaster":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud_dry/brown_mud_dry_albedo.jpg";
                case "R03_Sand":
                case "R03_SandLight":
                    return "Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_diff_1k.jpg";
                case "R03_Stone":
                case "R03_StoneLight":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_rocks_01/aerial_rocks_01_albedo.jpg";
                case "R03_Wood":
                case "R03_WoodLight":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_planks_03/brown_planks_03_albedo.jpg";
                case "R03_Rope":
                case "R03_Roof":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rough_wood/rough_wood_albedo.jpg";
                default:
                    return string.Empty;
            }
        }

        private static string PresentationNormalMapPath(string name)
        {
            switch (name)
            {
                case "R03_Grass":
                case "R03_GrassLight":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_grass_rock/aerial_grass_rock_normal.jpg";
                case "R03_Earth":
                case "R03_Plaster":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_mud_dry/brown_mud_dry_normal.jpg";
                case "R03_Sand":
                case "R03_SandLight":
                    return "Assets/Art/Candidates/Environment/PolyHaven/Textures/sand_01_nor_gl_1k.jpg";
                case "R03_Stone":
                case "R03_StoneLight":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/aerial_rocks_01/aerial_rocks_01_normal.jpg";
                case "R03_Wood":
                case "R03_WoodLight":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/brown_planks_03/brown_planks_03_normal.jpg";
                case "R03_Rope":
                case "R03_Roof":
                    return "Assets/Art/Candidates/UIMaterialsAudio/Materials/PolyHaven/rough_wood/rough_wood_normal.jpg";
                default:
                    return string.Empty;
            }
        }

        private static Texture2D LoadNormalMap(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void HidePresentationRenderers(Scene scene, string rootName)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null || root.name != rootName) continue;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                // Older presentation prefabs can carry a broad visual-surface
                // collider (the R02 surface covered the whole harbor).  Once
                // their renderers are hidden, those colliders must not remain
                // as a second movement authority.  The R03 prefab owns the
                // walkable surface; stable D2/D6 colliders remain elsewhere.
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;
            }
        }

        private static void HideLegacyVisuals(Scene scene)
        {
            var names = new HashSet<string>(StringComparer.Ordinal)
            {
                "Beach", "Sea", "Dock", "DockDeck", "Gangplank", "HandRail",
                "Barrel", "Barrel2", "Crate", "Crate2", "Bollard", "Pile",
                "D19_R01_B_DockMarketProps_Approved", "D19_R01_B_PortModules_Approved"
            };
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null) continue;
                // The name-based legacy cleanup below predates R03 and would
                // otherwise match legitimate R03 children such as Beach,
                // Sea-facing props, and dock parts.  R03 is the sole current
                // presentation owner; never hide its renderers during the
                // legacy pass or the authored shoreline disappears from the
                // Player while the old grey-box nodes remain hidden.
                if (root.name == RootName) continue;
                // D19_SaintMargaret_Root still owns the historical grey-box renderers;
                // keep its gameplay colliders/anchors, but remove only its visuals so
                // the R03 prefab is the sole presentation layer in the Player.
                if (root.name == "D19_SaintMargaret_Root")
                {
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
                if (root.name == "ph_palm_tree" || root.name == "ph_rock" ||
                    root.name == "ph_grass" || root.name == "ph_flower")
                {
                    // Preserve the stable placeholder nodes for gameplay and
                    // old saves, but remove their grey-box renderers from the
                    // R03 presentation pass.
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
                if (root.name == "D6_HarborFoam")
                {
                    // R03 owns the shoreline presentation. The old D6 foam
                    // bands are large flat quads near the player/ship and
                    // become foreground slabs when both owners are visible.
                    // Keep the D6 root for its runtime compatibility, but
                    // remove only its duplicate renderers; no gameplay,
                    // navigation, or water-state authority is changed.
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
                if (root.name.IndexOf("D19_R01", StringComparison.Ordinal) >= 0 ||
                    root.name.IndexOf("D19_R02", StringComparison.Ordinal) >= 0 ||
                    root.name.IndexOf("D19_S02", StringComparison.Ordinal) >= 0)
                {
                    foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (!names.Contains(child.name)) continue;
                    foreach (var renderer in child.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
                }
            }
        }

        private static string[] SceneRootSummary(Scene scene)
        {
            var rows = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null) continue;
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                int enabled = 0;
                foreach (var renderer in renderers) if (renderer.enabled) enabled++;
                rows.Add(root.name + "|active=" + root.activeSelf + "|renderers=" + renderers.Length + "|enabled=" + enabled);
            }
            return rows.ToArray();
        }

        private static void RemoveSceneInstance(Scene scene, string rootName)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root != null && root.name == rootName) UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static string MiniJson(Dictionary<string, object> values)
        {
            var lines = new List<string> { "{" };
            int index = 0;
            foreach (var pair in values)
            {
                string value;
                var array = pair.Value as Array;
                if (array != null)
                {
                    var parts = new List<string>();
                    foreach (var item in array) parts.Add("\"" + item.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
                    value = "[" + string.Join(", ", parts.ToArray()) + "]";
                }
                else if (pair.Value is int) value = pair.Value.ToString();
                else value = "\"" + pair.Value.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                lines.Add("  \"" + pair.Key + "\": " + value + (index++ == values.Count - 1 ? "" : ","));
            }
            lines.Add("}");
            return string.Join("\n", lines.ToArray());
        }

        private static Mesh MakeBoxMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, y = height * 0.5f, z = depth * 0.5f;
            var v = new[] { new Vector3(-x,-y,-z), new Vector3(x,-y,-z), new Vector3(x,-y,z), new Vector3(-x,-y,z), new Vector3(-x,y,-z), new Vector3(x,y,-z), new Vector3(x,y,z), new Vector3(-x,y,z) };
            return MakeMesh(v, new[] {
                0,1,2, 0,2,3,       // bottom
                4,6,5, 4,7,6,       // top
                0,4,5, 0,5,1,       // front (-Z)
                2,6,7, 2,7,3,       // back (+Z)
                1,5,6, 1,6,2,       // right (+X)
                0,3,7, 0,7,4        // left (-X)
            });
        }

        private static Mesh MakeBevelBoxMesh(float width, float height, float depth, float bevel)
        {
            // Build a closed chamfered cuboid instead of silently falling back
            // to a hard-edged cube. R03 uses this helper for building bodies,
            // doors, shutters, crates and dock trim; the old fallback made the
            // whole harbor read as a set of unlit proxy blocks. The six inset
            // face panels, twelve chamfer strips and eight corner triangles are
            // intentionally emitted with duplicated vertices so each surface
            // has a deterministic winding and hard/soft boundary under URP.
            float halfWidth = Mathf.Abs(width) * 0.5f;
            float halfHeight = Mathf.Abs(height) * 0.5f;
            float halfDepth = Mathf.Abs(depth) * 0.5f;
            float limit = Mathf.Min(halfWidth, Mathf.Min(halfHeight, halfDepth));
            float safeBevel = Mathf.Clamp(Mathf.Abs(bevel), 0.001f, limit * 0.82f);
            if (limit <= 0.001f || safeBevel <= 0.001f)
                return MakeBoxMesh(width, height, depth);

            float innerWidth = halfWidth - safeBevel;
            float innerHeight = halfHeight - safeBevel;
            float innerDepth = halfDepth - safeBevel;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            // Main faces.
            AddQuad(vertices, triangles,
                new Vector3(halfWidth, -innerHeight, -innerDepth),
                new Vector3(halfWidth, innerHeight, -innerDepth),
                new Vector3(halfWidth, innerHeight, innerDepth),
                new Vector3(halfWidth, -innerHeight, innerDepth), Vector3.right);
            AddQuad(vertices, triangles,
                new Vector3(-halfWidth, -innerHeight, innerDepth),
                new Vector3(-halfWidth, innerHeight, innerDepth),
                new Vector3(-halfWidth, innerHeight, -innerDepth),
                new Vector3(-halfWidth, -innerHeight, -innerDepth), Vector3.left);
            AddQuad(vertices, triangles,
                new Vector3(-innerWidth, halfHeight, -innerDepth),
                new Vector3(innerWidth, halfHeight, -innerDepth),
                new Vector3(innerWidth, halfHeight, innerDepth),
                new Vector3(-innerWidth, halfHeight, innerDepth), Vector3.up);
            AddQuad(vertices, triangles,
                new Vector3(-innerWidth, -halfHeight, innerDepth),
                new Vector3(innerWidth, -halfHeight, innerDepth),
                new Vector3(innerWidth, -halfHeight, -innerDepth),
                new Vector3(-innerWidth, -halfHeight, -innerDepth), Vector3.down);
            AddQuad(vertices, triangles,
                new Vector3(-innerWidth, -innerHeight, halfDepth),
                new Vector3(innerWidth, -innerHeight, halfDepth),
                new Vector3(innerWidth, innerHeight, halfDepth),
                new Vector3(-innerWidth, innerHeight, halfDepth), Vector3.forward);
            AddQuad(vertices, triangles,
                new Vector3(innerWidth, -innerHeight, -halfDepth),
                new Vector3(-innerWidth, -innerHeight, -halfDepth),
                new Vector3(-innerWidth, innerHeight, -halfDepth),
                new Vector3(innerWidth, innerHeight, -halfDepth), Vector3.back);

            // Edge strips between each pair of adjacent face panels.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    AddQuad(vertices, triangles,
                        new Vector3(sx * halfWidth, sy * innerHeight, -innerDepth),
                        new Vector3(sx * halfWidth, sy * innerHeight, innerDepth),
                        new Vector3(sx * innerWidth, sy * halfHeight, innerDepth),
                        new Vector3(sx * innerWidth, sy * halfHeight, -innerDepth),
                        new Vector3(sx, sy, 0f).normalized);
                }

                for (int sz = -1; sz <= 1; sz += 2)
                {
                    AddQuad(vertices, triangles,
                        new Vector3(sx * halfWidth, -innerHeight, sz * innerDepth),
                        new Vector3(sx * halfWidth, innerHeight, sz * innerDepth),
                        new Vector3(sx * innerWidth, innerHeight, sz * halfDepth),
                        new Vector3(sx * innerWidth, -innerHeight, sz * halfDepth),
                        new Vector3(sx, 0f, sz).normalized);
                }
            }

            for (int sy = -1; sy <= 1; sy += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    AddQuad(vertices, triangles,
                        new Vector3(-innerWidth, sy * halfHeight, sz * innerDepth),
                        new Vector3(innerWidth, sy * halfHeight, sz * innerDepth),
                        new Vector3(innerWidth, sy * innerHeight, sz * halfDepth),
                        new Vector3(-innerWidth, sy * innerHeight, sz * halfDepth),
                        new Vector3(0f, sy, sz).normalized);
                }
            }

            // Fill the eight trihedral corner gaps.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    for (int sz = -1; sz <= 1; sz += 2)
                    {
                        AddTriangle(vertices, triangles,
                            new Vector3(sx * halfWidth, sy * innerHeight, sz * innerDepth),
                            new Vector3(sx * innerWidth, sy * halfHeight, sz * innerDepth),
                            new Vector3(sx * innerWidth, sy * innerHeight, sz * halfDepth),
                            new Vector3(sx, sy, sz).normalized);
                    }
                }
            }

            return MakeMesh(vertices.ToArray(), triangles.ToArray());
        }

        private static void AddQuad(List<Vector3> vertices, List<int> triangles,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 expectedNormal)
        {
            int start = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            Vector3 actualNormal = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(actualNormal, expectedNormal) >= 0f)
            {
                triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
                triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);
            }
            else
            {
                triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1);
                triangles.Add(start); triangles.Add(start + 3); triangles.Add(start + 2);
            }
        }

        private static void AddTriangle(List<Vector3> vertices, List<int> triangles,
            Vector3 a, Vector3 b, Vector3 c, Vector3 expectedNormal)
        {
            int start = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            Vector3 actualNormal = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(actualNormal, expectedNormal) >= 0f)
            {
                triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
            }
            else
            {
                triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1);
            }
        }

        private static Mesh MakeRoofMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, z = depth * 0.5f;
            return MakeMesh(new[] { new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,0,z),new Vector3(-x,0,z),new Vector3(0,height,0) }, new[] {
                0,4,1,       // front slope (-Z)
                1,4,2,       // right slope (+X)
                2,4,3,       // back slope (+Z)
                3,4,0,       // left slope (-X)
                0,1,2, 0,2,3 // underside
            });
        }

        private static Mesh MakeGableRoofMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, z = depth * 0.5f;
            return MakeMesh(new[]
            {
                new Vector3(-x, 0f, -z), new Vector3(x, 0f, -z),
                new Vector3(-x, 0f, z), new Vector3(x, 0f, z),
                new Vector3(0f, height, -z), new Vector3(0f, height, z)
            }, new[]
            {
                0, 4, 1,       // front gable
                2, 3, 5,       // rear gable
                0, 2, 5, 0, 5, 4,
                1, 4, 5, 1, 5, 3,
                0, 1, 3, 0, 3, 2 // closed underside
            });
        }

        private static Mesh MakeWedgeMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, z = depth * 0.5f;
            return MakeMesh(new[] { new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,0,z),new Vector3(-x,0,z),new Vector3(-x,height,z),new Vector3(x,height,z) }, new[] { 0,1,2,0,2,3,3,2,5,3,5,4,0,3,4,0,4,1,1,4,5,1,5,2 });
        }

        private static Mesh MakePathMesh(Vector2[] points, float width, float lift)
        {
            if (points == null || points.Length < 2)
                return MakeRibbonMesh(Mathf.Max(0.1f, width), 0.02f, 0.1f);

            float halfWidth = Mathf.Max(0.05f, width * 0.5f);
            var vertices = new Vector3[points.Length * 2];
            for (int i = 0; i < points.Length; i++)
            {
                var previous = i == 0 ? points[i] : points[i - 1];
                var next = i == points.Length - 1 ? points[i] : points[i + 1];
                var tangent = (next - previous).normalized;
                if (tangent.sqrMagnitude < 0.0001f) tangent = Vector2.up;
                var normal = new Vector2(-tangent.y, tangent.x);
                var left = points[i] + normal * halfWidth;
                var right = points[i] - normal * halfWidth;
                vertices[i * 2] = new Vector3(left.x, Height(left.x, left.y) + lift, left.y);
                vertices[i * 2 + 1] = new Vector3(right.x, Height(right.x, right.y) + lift, right.y);
            }

            var triangles = new int[(points.Length - 1) * 6];
            int triangle = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                int left = i * 2;
                int right = left + 1;
                int nextLeft = left + 2;
                int nextRight = left + 3;
                // The order keeps the strip normal facing up when viewed from
                // the Player camera and also avoids a back-facing seam where
                // two route segments meet.
                triangles[triangle++] = left;
                triangles[triangle++] = nextLeft;
                triangles[triangle++] = nextRight;
                triangles[triangle++] = left;
                triangles[triangle++] = nextRight;
                triangles[triangle++] = right;
            }
            return MakeMesh(vertices, triangles);
        }

        private static Mesh MakeRibbonMesh(float width, float height, float depth)
        {
            float x = width * 0.5f, z = depth * 0.5f;
            return MakeMesh(new[] { new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(x,height,z),new Vector3(-x,height,z) }, new[] { 0,1,2,0,2,3 });
        }

        private static Mesh MakeWaterSurfaceMesh(float width, float depth, int cells, float amplitude)
        {
            int safeCells = Mathf.Clamp(cells, 2, 32);
            float halfWidth = Mathf.Abs(width) * 0.5f;
            float halfDepth = Mathf.Abs(depth) * 0.5f;
            float safeAmplitude = Mathf.Clamp(Mathf.Abs(amplitude), 0f, 0.25f);
            var vertices = new Vector3[(safeCells + 1) * (safeCells + 1)];
            var triangles = new int[safeCells * safeCells * 6];

            for (int z = 0; z <= safeCells; z++)
            {
                float nz = z / (float)safeCells;
                float localZ = Mathf.Lerp(-halfDepth, halfDepth, nz);
                for (int x = 0; x <= safeCells; x++)
                {
                    float nx = x / (float)safeCells;
                    float localX = Mathf.Lerp(-halfWidth, halfWidth, nx);
                    float wave = safeAmplitude * (
                        Mathf.Sin(localX * 0.43f + localZ * 0.19f) * 0.55f +
                        Mathf.Cos(localZ * 0.31f - localX * 0.17f) * 0.30f +
                        Mathf.Sin((localX + localZ) * 0.13f + 0.8f) * 0.15f);
                    vertices[z * (safeCells + 1) + x] = new Vector3(localX, wave, localZ);
                }
            }

            int triangle = 0;
            for (int z = 0; z < safeCells; z++)
            {
                for (int x = 0; x < safeCells; x++)
                {
                    int a = z * (safeCells + 1) + x;
                    int b = a + 1;
                    int d = (z + 1) * (safeCells + 1) + x;
                    int c = d + 1;
                    // Counter-clockwise when viewed from above: normals point
                    // upward so URP lighting and reflections remain readable.
                    triangles[triangle++] = a;
                    triangles[triangle++] = c;
                    triangles[triangle++] = b;
                    triangles[triangle++] = a;
                    triangles[triangle++] = d;
                    triangles[triangle++] = c;
                }
            }
            return MakeMesh(vertices, triangles);
        }

        private static Mesh MakeRockMesh(float radius)
        {
            // Use a small deterministic rounded ring mesh instead of the old
            // six-vertex pyramid. The old silhouette read as a set of
            // floating triangular spikes in the Player, especially along the
            // shoreline. Four short rings keep the asset lightweight while
            // giving the rock a readable shoulder, a blunt top and a closed
            // base. The top is deliberately below the old 1.34r apex so the
            // generated shoreline set cannot be mistaken for floating trees.
            const int segments = 8;
            var vertices = new List<Vector3>(segments * 4 + 2);
            var triangles = new List<int>(segments * 18);
            // Keep these as low coastal stones, not domes. The previous
            // 0.80r cap was still too tall at the shoreline and repeated
            // nine times it dominated the camera as a row of half-buried
            // arches. A low shoulder leaves a grounded silhouette while
            // retaining a closed, deterministic mesh.
            var ringHeights = new[] { 0f, radius * 0.12f, radius * 0.24f, radius * 0.34f };
            var ringScales = new[] { 1.0f, 0.98f, 0.78f, 0.46f };
            for (int ring = 0; ring < ringHeights.Length; ring++)
            {
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float variation = 0.88f + 0.10f * Mathf.Sin(i * 1.73f + radius * 2.1f);
                    float r = radius * ringScales[ring] * variation;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * r, ringHeights[ring], Mathf.Sin(angle) * r));
                }
            }

            int top = vertices.Count;
            vertices.Add(new Vector3(radius * 0.02f, radius * 0.42f, -radius * 0.01f));
            int bottom = vertices.Count;
            vertices.Add(new Vector3(0f, -radius * 0.03f, 0f));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int lower = i;
                int lowerNext = next;
                int middle = segments + i;
                int middleNext = segments + next;
                int upper = segments * 2 + i;
                int upperNext = segments * 2 + next;
                int cap = segments * 3 + i;
                int capNext = segments * 3 + next;

                triangles.Add(lower); triangles.Add(lowerNext); triangles.Add(middleNext);
                triangles.Add(lower); triangles.Add(middleNext); triangles.Add(middle);
                triangles.Add(middle); triangles.Add(middleNext); triangles.Add(upperNext);
                triangles.Add(middle); triangles.Add(upperNext); triangles.Add(upper);
                triangles.Add(upper); triangles.Add(upperNext); triangles.Add(capNext);
                triangles.Add(upper); triangles.Add(capNext); triangles.Add(cap);
                // The ring is emitted counter-clockwise when viewed from above.
                // Reverse the cap winding so the upward-facing top is rendered;
                // the old order was back-face culled and read as a hollow ring
                // or floating arch in the Player.
                triangles.Add(capNext); triangles.Add(cap); triangles.Add(top);
                // Keep the generated cap visible from either side of a seam.
                // This presentation mesh has no collision authority and the
                // duplicate winding prevents a water-coloured hole from
                // reading as another floating ring at grazing camera angles.
                triangles.Add(cap); triangles.Add(capNext); triangles.Add(top);
                triangles.Add(lowerNext); triangles.Add(lower); triangles.Add(bottom);
            }
            return MakeMesh(vertices.ToArray(), triangles.ToArray());
        }

        private static Mesh MakeShrubMesh(float radius)
        {
            // Shrubs are a grounded, rounded tuft rather than a four-sided
            // pyramid. The previous pyramid was the most common source of
            // green triangular silhouettes that read as floating trees in
            // the Player. Keep the mesh closed and low-poly, but add a short
            // shoulder and blunt cap so lighting can describe its volume.
            const int segments = 8;
            var vertices = new List<Vector3>(segments * 3 + 2);
            var triangles = new List<int>(segments * 15);
            var ringHeights = new[] { 0f, radius * 0.16f, radius * 0.34f };
            var ringScales = new[] { 1.0f, 0.90f, 0.56f };
            for (int ring = 0; ring < ringHeights.Length; ring++)
            {
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float variation = 0.94f + 0.07f * Mathf.Sin(i * 1.91f + radius * 3.0f);
                    float r = radius * ringScales[ring] * variation;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * r, ringHeights[ring], Mathf.Sin(angle) * r));
                }
            }

            int top = vertices.Count;
            vertices.Add(new Vector3(-radius * 0.03f, radius * 0.46f, radius * 0.02f));
            int bottom = vertices.Count;
            vertices.Add(new Vector3(0f, -radius * 0.035f, 0f));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int lower = i;
                int lowerNext = next;
                int shoulder = segments + i;
                int shoulderNext = segments + next;
                int upper = segments * 2 + i;
                int upperNext = segments * 2 + next;

                triangles.Add(lower); triangles.Add(lowerNext); triangles.Add(shoulderNext);
                triangles.Add(lower); triangles.Add(shoulderNext); triangles.Add(shoulder);
                triangles.Add(shoulder); triangles.Add(shoulderNext); triangles.Add(upperNext);
                triangles.Add(shoulder); triangles.Add(upperNext); triangles.Add(upper);
                // Match the rock cap winding: the shrub top must face upward,
                // otherwise the low-poly tuft reads as a hollow floating loop.
                triangles.Add(upperNext); triangles.Add(upper); triangles.Add(top);
                triangles.Add(upper); triangles.Add(upperNext); triangles.Add(top);
                triangles.Add(lowerNext); triangles.Add(lower); triangles.Add(bottom);
            }
            return MakeMesh(vertices.ToArray(), triangles.ToArray());
        }

        private static Mesh MakeCanopyMesh(float radius, float height)
        {
            // Three closed octagonal rings give the canopy thickness and a
            // stable normal on every side. This intentionally avoids the
            // billboard/frond-card geometry that caused the R03 fixed-shot
            // green-triangle artefacts.
            const int segments = 8;
            var vertices = new List<Vector3>(segments * 3 + 2);
            var triangles = new List<int>(segments * 18);
            var ringRadii = new[] { radius * 0.58f, radius, radius * 0.72f };
            var ringHeights = new[] { -height * 0.24f, 0f, height * 0.28f };
            for (int ring = 0; ring < ringRadii.Length; ring++)
            {
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    vertices.Add(new Vector3(
                        Mathf.Cos(angle) * ringRadii[ring],
                        ringHeights[ring],
                        Mathf.Sin(angle) * ringRadii[ring]));
                }
            }

            int bottom = vertices.Count;
            vertices.Add(new Vector3(0f, -height * 0.42f, 0f));
            int top = vertices.Count;
            vertices.Add(new Vector3(0f, height * 0.52f, 0f));

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                var outward = new Vector3(
                    Mathf.Cos((i + 0.5f) * Mathf.PI * 2f / segments),
                    0f,
                    Mathf.Sin((i + 0.5f) * Mathf.PI * 2f / segments));
                AddQuad(vertices, triangles,
                    vertices[i], vertices[next], vertices[segments + next], vertices[segments + i], outward);
                AddQuad(vertices, triangles,
                    vertices[segments + i], vertices[segments + next], vertices[segments * 2 + next],
                    vertices[segments * 2 + i], outward);
                AddTriangle(vertices, triangles, vertices[bottom], vertices[next], vertices[i], Vector3.down);
                AddTriangle(vertices, triangles, vertices[segments * 2 + i], vertices[segments * 2 + next],
                    vertices[top], Vector3.up);
            }
            return MakeMesh(vertices.ToArray(), triangles.ToArray());
        }

        private static Mesh MakeCylinderMesh(float radius, float height, int segments)
        {
            var v = new List<Vector3>(); var t = new List<int>();
            for (int i = 0; i < segments; i++) { float a = i * Mathf.PI * 2f / segments; v.Add(new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius)); v.Add(new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius)); }
            for (int i = 0; i < segments; i++) { int n = (i + 1) % segments; t.Add(i * 2); t.Add(n * 2); t.Add(n * 2 + 1); t.Add(i * 2); t.Add(n * 2 + 1); t.Add(i * 2 + 1); }
            return MakeMesh(v.ToArray(), t.ToArray());
        }

        private static Mesh MakeCappedCylinderMesh(float radius, float depth, int segments)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float halfDepth = depth * 0.5f;
            int bottomCenter = vertices.Count;
            vertices.Add(new Vector3(0f, -halfDepth, 0f));
            int topCenter = vertices.Count;
            vertices.Add(new Vector3(0f, halfDepth, 0f));
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                vertices.Add(new Vector3(x, -halfDepth, z));
                vertices.Add(new Vector3(x, halfDepth, z));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int bottom = 2 + i * 2;
                int top = bottom + 1;
                int nextBottom = 2 + next * 2;
                int nextTop = nextBottom + 1;
                triangles.Add(bottomCenter);
                triangles.Add(nextBottom);
                triangles.Add(bottom);
                triangles.Add(topCenter);
                triangles.Add(top);
                triangles.Add(nextTop);
                triangles.Add(bottom);
                triangles.Add(nextBottom);
                triangles.Add(nextTop);
                triangles.Add(bottom);
                triangles.Add(nextTop);
                triangles.Add(top);
            }
            return MakeMesh(vertices.ToArray(), triangles.ToArray());
        }

        private static Mesh MakeHelmWheelMesh(float radius, float depth, int segments, int spokeCount)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float halfDepth = depth * 0.5f;
            float innerRadius = radius * 0.68f;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);
                vertices.Add(new Vector3(x * radius, -halfDepth, z * radius));
                vertices.Add(new Vector3(x * radius, halfDepth, z * radius));
                vertices.Add(new Vector3(x * innerRadius, -halfDepth, z * innerRadius));
                vertices.Add(new Vector3(x * innerRadius, halfDepth, z * innerRadius));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int current = i * 4;
                int following = next * 4;
                int outerBottom = current;
                int outerTop = current + 1;
                int innerBottom = current + 2;
                int innerTop = current + 3;
                int nextOuterBottom = following;
                int nextOuterTop = following + 1;
                int nextInnerBottom = following + 2;
                int nextInnerTop = following + 3;
                triangles.Add(outerBottom); triangles.Add(nextOuterBottom); triangles.Add(nextOuterTop);
                triangles.Add(outerBottom); triangles.Add(nextOuterTop); triangles.Add(outerTop);
                triangles.Add(innerBottom); triangles.Add(innerTop); triangles.Add(nextInnerTop);
                triangles.Add(innerBottom); triangles.Add(nextInnerTop); triangles.Add(nextInnerBottom);
                triangles.Add(outerBottom); triangles.Add(innerBottom); triangles.Add(nextInnerBottom);
                triangles.Add(outerBottom); triangles.Add(nextInnerBottom); triangles.Add(nextOuterBottom);
                triangles.Add(outerTop); triangles.Add(nextOuterTop); triangles.Add(nextInnerTop);
                triangles.Add(outerTop); triangles.Add(nextInnerTop); triangles.Add(innerTop);
            }

            float spokeHalfWidth = radius * 0.075f;
            float spokeStart = radius * 0.06f;
            float spokeEnd = innerRadius * 1.03f;
            for (int i = 0; i < Mathf.Max(1, spokeCount); i++)
            {
                float angle = i * Mathf.PI * 2f / Mathf.Max(1, spokeCount);
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                float tx = -sin * spokeHalfWidth;
                float tz = cos * spokeHalfWidth;
                float sx = cos * spokeStart;
                float sz = sin * spokeStart;
                float ex = cos * spokeEnd;
                float ez = sin * spokeEnd;
                int baseIndex = vertices.Count;
                vertices.Add(new Vector3(sx - tx, -halfDepth, sz - tz));
                vertices.Add(new Vector3(sx + tx, -halfDepth, sz + tz));
                vertices.Add(new Vector3(ex + tx, -halfDepth, ez + tz));
                vertices.Add(new Vector3(ex - tx, -halfDepth, ez - tz));
                vertices.Add(new Vector3(sx - tx, halfDepth, sz - tz));
                vertices.Add(new Vector3(sx + tx, halfDepth, sz + tz));
                vertices.Add(new Vector3(ex + tx, halfDepth, ez + tz));
                vertices.Add(new Vector3(ex - tx, halfDepth, ez - tz));
                triangles.Add(baseIndex); triangles.Add(baseIndex + 1); triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex); triangles.Add(baseIndex + 2); triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 4); triangles.Add(baseIndex + 6); triangles.Add(baseIndex + 5);
                triangles.Add(baseIndex + 4); triangles.Add(baseIndex + 7); triangles.Add(baseIndex + 6);
                triangles.Add(baseIndex); triangles.Add(baseIndex + 4); triangles.Add(baseIndex + 5);
                triangles.Add(baseIndex); triangles.Add(baseIndex + 5); triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 1); triangles.Add(baseIndex + 5); triangles.Add(baseIndex + 6);
                triangles.Add(baseIndex + 1); triangles.Add(baseIndex + 6); triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 2); triangles.Add(baseIndex + 6); triangles.Add(baseIndex + 7);
                triangles.Add(baseIndex + 2); triangles.Add(baseIndex + 7); triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 3); triangles.Add(baseIndex + 7); triangles.Add(baseIndex + 4);
                triangles.Add(baseIndex + 3); triangles.Add(baseIndex + 4); triangles.Add(baseIndex);
            }
            return MakeMesh(vertices.ToArray(), triangles.ToArray());
        }

        private static Mesh MakeBladeMesh(float length, float width)
        {
            return MakeMesh(new[] { new Vector3(0,0,0),new Vector3(length*0.65f,width,0),new Vector3(length,0.02f,0),new Vector3(length*0.65f,-width,0),new Vector3(length*0.55f,0,0.12f) }, new[] { 0,1,4,1,2,4,2,3,4,3,0,4 });
        }

        private static Mesh MakeMesh(Vector3[] vertices, int[] triangles)
        {
            var mesh = new Mesh { name = "D19_R03_PresentationMesh" };
            ApplyTriplanarPresentationUv(mesh, vertices, triangles);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        private static void ApplyTriplanarPresentationUv(Mesh mesh, Vector3[] sourceVertices, int[] sourceTriangles)
        {
            if (sourceVertices == null || sourceTriangles == null || sourceTriangles.Length < 3 || sourceTriangles.Length % 3 != 0)
            {
                mesh.vertices = sourceVertices ?? new Vector3[0];
                mesh.triangles = sourceTriangles ?? new int[0];
                return;
            }

            // Procedural R03 meshes do not have authored UVs. Expand each
            // triangle so its dominant face axis can receive a stable planar
            // projection; this avoids stretched one-dimensional textures on
            // walls while keeping the presentation meshes independent of any
            // gameplay collider or owner data.
            var vertices = new Vector3[sourceTriangles.Length];
            var uv = new Vector2[sourceTriangles.Length];
            var triangles = new int[sourceTriangles.Length];
            for (int i = 0; i < sourceTriangles.Length; i += 3)
            {
                var a = sourceVertices[sourceTriangles[i]];
                var b = sourceVertices[sourceTriangles[i + 1]];
                var c = sourceVertices[sourceTriangles[i + 2]];
                var normal = Vector3.Cross(b - a, c - a).normalized;
                vertices[i] = a;
                vertices[i + 1] = b;
                vertices[i + 2] = c;
                uv[i] = ProjectPresentationUv(a, normal);
                uv[i + 1] = ProjectPresentationUv(b, normal);
                uv[i + 2] = ProjectPresentationUv(c, normal);
                triangles[i] = i;
                triangles[i + 1] = i + 1;
                triangles[i + 2] = i + 2;
            }
            mesh.indexFormat = vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }

        private static Vector2 ProjectPresentationUv(Vector3 vertex, Vector3 normal)
        {
            var absolute = new Vector3(Mathf.Abs(normal.x), Mathf.Abs(normal.y), Mathf.Abs(normal.z));
            if (absolute.y >= absolute.x && absolute.y >= absolute.z)
                return new Vector2(vertex.x, vertex.z) * PresentationUvScale;
            if (absolute.x >= absolute.z)
                return new Vector2(vertex.z, vertex.y) * PresentationUvScale;
            return new Vector2(vertex.x, vertex.y) * PresentationUvScale;
        }
    }
}
#endif
