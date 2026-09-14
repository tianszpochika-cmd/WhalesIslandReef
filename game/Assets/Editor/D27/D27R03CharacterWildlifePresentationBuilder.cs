#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Whalebone.Gameplay.D16;
using WhalesIslandReef.Presentation.D20.NpcPresentation;
using WhalesIslandReef.Presentation.D20.Wildlife;

namespace WhalesIslandReef.Editor.D27
{
    /// <summary>
    /// Adds the already audited D20 character and wildlife Prefabs to the R03
    /// harbor presentation prefab. This is presentation-only: it does not
    /// create NPC/ecology facts, movement authorities, interaction results or
    /// save data. Non-trigger colliders on the visual copies are disabled so a
    /// decorative NPC cannot block D2/D6 movement.
    /// </summary>
    public static class D27R03CharacterWildlifePresentationBuilder
    {
        private const string R03PrefabPath = "Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab";
        private const string OutputDirectory = "Assets/Prefabs/Characters/D27/R03";
        private const string OutputPrefabPath = OutputDirectory + "/D27_R03_CharacterWildlifePresentation.prefab";
        private const string OutputRootName = "D27_R03_CharacterWildlifePresentation";
        private const string NpcSourceRoot = "Assets/Prefabs/Characters/D20/NPC/";
        private const string NpcMaterialRoot = "Assets/Art/Models/Characters/D20/NPC/Materials";
        private const string WildlifeSourceRoot = "Assets/Prefabs/Animals/D20/Wildlife/";
        private const string PetSourceRoot = "Assets/Prefabs/Animals/D20/PetsRare/";
        private const string NpcControllerPath = "Assets/Animations/D20/NPC/D20_NPC_Presentation_Demo.controller";
        private const string ResultDirectory = "tools/results/D27/D27-S05";
        private const string ResultPath = ResultDirectory + "/D27_S05_R03_PRESENTATION_BUILD.json";

        private sealed class NpcDefinition
        {
            public string id;
            public string prefab;
            public string resourceKey;
            public D20NpcJobPost jobPost;
            public Vector3 position;
            public float yaw;
            public float targetHeight;
        }

        private sealed class WildlifeDefinition
        {
            public string id;
            public string prefab;
            public Vector3 position;
            public float yaw;
            public float scale;
        }

        [Serializable]
        private sealed class BuildReport
        {
            public string schema = "D27_S05_R03_CHARACTER_WILDLIFE_PRESENTATION_V1";
            public string task = "D27-S05";
            public string operation;
            public string status;
            public string r03_prefab;
            public string output_prefab;
            public int npc_count;
            public int wildlife_count;
            public int pet_count;
            public int renderer_count;
            public int collider_count;
            public int disabled_non_trigger_collider_count;
            public int registration_count;
            public bool formal_scene_modified;
            public bool systems_modified;
            public bool save_data_modified;
            public string[] placed_ids;
            public string[] errors;
        }

        private static readonly NpcDefinition[] Npcs =
        {
            new NpcDefinition { id = "npc_d20_dock_clerk_01", prefab = "npc_d20_dock_clerk_01.prefab", resourceKey = "npc_d20_dock_clerk_01", jobPost = D20NpcJobPost.Dock, position = new Vector3(-1.8f, 0.22f, -6.5f), yaw = 18f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_fisher_01", prefab = "npc_d20_fisher_01.prefab", resourceKey = "npc_d20_fisher_01", jobPost = D20NpcJobPost.Dock, position = new Vector3(2.2f, 0.22f, -7.6f), yaw = -28f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_deckhand_01", prefab = "npc_d20_deckhand_01.prefab", resourceKey = "npc_d20_deckhand_01", jobPost = D20NpcJobPost.Dock, position = new Vector3(-4.8f, 0.22f, -7.1f), yaw = 35f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_net_mender_01", prefab = "npc_d20_net_mender_01.prefab", resourceKey = "npc_d20_net_mender_01", jobPost = D20NpcJobPost.Dock, position = new Vector3(4.1f, 0.22f, -7.0f), yaw = -16f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_shore_trader_01", prefab = "npc_d20_shore_trader_01.prefab", resourceKey = "npc_d20_shore_trader_01", jobPost = D20NpcJobPost.Market, position = new Vector3(6.0f, 0.22f, -0.2f), yaw = 180f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_shore_trader_02", prefab = "npc_d20_shore_trader_02.prefab", resourceKey = "npc_d20_shore_trader_02", jobPost = D20NpcJobPost.Market, position = new Vector3(7.4f, 0.22f, 0.8f), yaw = 154f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_tavern_cook_01", prefab = "npc_d20_tavern_cook_01.prefab", resourceKey = "npc_d20_tavern_cook_01", jobPost = D20NpcJobPost.Tavern, position = new Vector3(10.2f, 0.22f, 9.8f), yaw = 200f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_boatbuilder_01", prefab = "npc_d20_boatbuilder_01.prefab", resourceKey = "npc_d20_boatbuilder_01", jobPost = D20NpcJobPost.Shipyard, position = new Vector3(-9.2f, 0.22f, -2.5f), yaw = 42f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_sailmaker_01", prefab = "npc_d20_sailmaker_01.prefab", resourceKey = "npc_d20_sailmaker_01", jobPost = D20NpcJobPost.Shipyard, position = new Vector3(-10.2f, 0.22f, -3.6f), yaw = 12f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_rigger_01", prefab = "npc_d20_rigger_01.prefab", resourceKey = "npc_d20_rigger_01", jobPost = D20NpcJobPost.Shipyard, position = new Vector3(-8.4f, 0.22f, -1.8f), yaw = -25f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_harbor_watch_proxy", prefab = "npc_d20_harbor_watch_proxy.prefab", resourceKey = "npc_d20_harbor_watch_proxy", jobPost = D20NpcJobPost.Dock, position = new Vector3(13.6f, 0.22f, 6.6f), yaw = -140f, targetHeight = 1.72f },
            new NpcDefinition { id = "npc_d20_common_resident_01", prefab = "npc_d20_common_resident_01.prefab", resourceKey = "npc_d20_common_resident_01", jobPost = D20NpcJobPost.Tavern, position = new Vector3(-10.2f, 0.22f, 7.0f), yaw = 80f, targetHeight = 1.72f },
        };

        private static readonly WildlifeDefinition[] Wildlife =
        {
            new WildlifeDefinition { id = "D20_Wildlife_fox_small_land", prefab = "D20_Wildlife_fox_small_land.prefab", position = new Vector3(-12.4f, 0.24f, 7.4f), yaw = 35f, scale = 1.0f },
            new WildlifeDefinition { id = "D20_Wildlife_cow_livestock", prefab = "D20_Wildlife_cow_livestock.prefab", position = new Vector3(-10.0f, 0.24f, 10.4f), yaw = -35f, scale = 1.0f },
            new WildlifeDefinition { id = "D20_Wildlife_eagle_coastal_bird", prefab = "D20_Wildlife_eagle_coastal_bird.prefab", position = new Vector3(8.5f, 3.0f, 10.8f), yaw = -30f, scale = 1.0f },
            // WaterWaveSurface is authored at Y=-0.04.  Aquatic roots must be
            // below that plane so their silhouettes read as submerged rather
            // than as fish hovering over the beach.
            new WildlifeDefinition { id = "D20_Wildlife_fish_nearshore", prefab = "D20_Wildlife_fish_nearshore.prefab", position = new Vector3(10.5f, -0.22f, -8.2f), yaw = 25f, scale = 1.0f },
            new WildlifeDefinition { id = "D20_Wildlife_fish_shallow_school", prefab = "D20_Wildlife_fish_shallow_school.prefab", position = new Vector3(-8.4f, -0.20f, -10.0f), yaw = -42f, scale = 1.0f },
            new WildlifeDefinition { id = "D20_Wildlife_manta_large_aquatic_equivalent", prefab = "D20_Wildlife_manta_large_aquatic_equivalent.prefab", position = new Vector3(14.0f, -0.32f, -5.8f), yaw = 145f, scale = 0.72f },
            new WildlifeDefinition { id = "D20_Wildlife_shark_nearshore_predator", prefab = "D20_Wildlife_shark_nearshore_predator.prefab", position = new Vector3(-14.0f, -0.38f, -6.8f), yaw = 62f, scale = 0.64f },
            new WildlifeDefinition { id = "D20_Wildlife_whale_distant_aquatic", prefab = "D20_Wildlife_whale_distant_aquatic.prefab", position = new Vector3(2.0f, -0.45f, 15.3f), yaw = 180f, scale = 0.58f },
        };

        private static readonly WildlifeDefinition[] Pets =
        {
            new WildlifeDefinition { id = "D20_PetsRare_cat_core", prefab = "D20_PetsRare_cat_core.prefab", position = new Vector3(2.2f, 0.23f, 4.8f), yaw = -50f, scale = 1.0f },
            new WildlifeDefinition { id = "D20_PetsRare_dog_shiba", prefab = "D20_PetsRare_dog_shiba.prefab", position = new Vector3(3.6f, 0.23f, 3.7f), yaw = 115f, scale = 0.92f },
        };

        [MenuItem("WhalesIslandReef/D27/Build R03 Character and Wildlife Presentation")]
        public static void BuildFromMenu()
        {
            BuildAndAttach();
        }

        [MenuItem("WhalesIslandReef/D27/Audit R03 Character and Wildlife Presentation")]
        public static void AuditFromMenu()
        {
            WriteAudit("audit");
        }

        [MenuItem("WhalesIslandReef/D27/Repair NPC Materials for URP")]
        public static void RepairNpcMaterialsForUrpFromMenu()
        {
            RepairNpcMaterialsForUrp();
        }

        public static void BatchBuildAndAttach()
        {
            try
            {
                BuildAndAttach();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void BuildAndAttach()
        {
            RepairNpcMaterialsForUrp();
            EnsureFolder(OutputDirectory);
            EnsureFolder(ResultDirectory);

            var root = new GameObject(OutputRootName);
            var placed = new List<string>();
            try
            {
                root.transform.position = Vector3.zero;
                root.transform.rotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;
                root.isStatic = true;

                foreach (var npc in Npcs)
                {
                    AddNpc(root.transform, npc);
                    placed.Add(npc.id);
                }
                foreach (var animal in Wildlife)
                {
                    AddWildlife(root.transform, animal);
                    placed.Add(animal.id);
                }
                foreach (var pet in Pets)
                {
                    AddPet(root.transform, pet);
                    placed.Add(pet.id);
                }

                var saved = PrefabUtility.SaveAsPrefabAsset(root, OutputPrefabPath, out bool saveSucceeded);
                if (!saveSucceeded || saved == null)
                    throw new InvalidOperationException("D27 character/wildlife presentation prefab could not be saved: " + OutputPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            AttachToR03Prefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var report = BuildReportForPrefab("build", placed.ToArray());
            WriteReport(report);
            Debug.Log("[D27-S05] R03 character/wildlife presentation attached: npc=" + report.npc_count +
                ", wildlife=" + report.wildlife_count + ", pets=" + report.pet_count);
        }

        /// <summary>
        /// Migrates the D20 NPC material owner from Unity's built-in Standard
        /// shader to the project's URP Lit shader. The source material remains
        /// the single owner; this method only preserves its authored color and
        /// scalar values while changing the render-pipeline implementation.
        /// </summary>
        public static int RepairNpcMaterialsForUrp()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
                throw new InvalidOperationException("Universal Render Pipeline/Lit shader is not available.");

            var migrated = 0;
            var guids = AssetDatabase.FindAssets("t:Material", new[] { NpcMaterialRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) continue;

                var color = material.HasProperty("_BaseColor")
                    ? material.GetColor("_BaseColor")
                    : material.HasProperty("_Color")
                        ? material.GetColor("_Color")
                        : Color.white;
                var metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
                var smoothness = material.HasProperty("_Smoothness")
                    ? material.GetFloat("_Smoothness")
                    : material.HasProperty("_Glossiness")
                        ? material.GetFloat("_Glossiness")
                        : 0.25f;

                if (material.shader != urpLit)
                {
                    material.shader = urpLit;
                    migrated++;
                }

                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
                EditorUtility.SetDirty(material);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[D27-S05] NPC URP material repair complete: scanned=" + guids.Length +
                ", migrated=" + migrated + ", shader=Universal Render Pipeline/Lit");
            return migrated;
        }

        private static void AttachToR03Prefab()
        {
            var r03 = PrefabUtility.LoadPrefabContents(R03PrefabPath);
            if (r03 == null)
                throw new InvalidOperationException("R03 presentation prefab could not be loaded: " + R03PrefabPath);

            try
            {
                var previous = r03.transform.Find(OutputRootName);
                if (previous != null)
                    UnityEngine.Object.DestroyImmediate(previous.gameObject);

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OutputPrefabPath);
                if (prefab == null)
                    throw new InvalidOperationException("D27 presentation prefab could not be loaded: " + OutputPrefabPath);

                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                    throw new InvalidOperationException("D27 presentation prefab could not be instantiated: " + OutputPrefabPath);
                instance.name = OutputRootName;
                instance.transform.SetParent(r03.transform, false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                EditorSceneManagerMarkDirty(r03);
                PrefabUtility.SaveAsPrefabAsset(r03, R03PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(r03);
            }
        }

        private static void AddNpc(Transform parent, NpcDefinition definition)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(NpcSourceRoot + definition.prefab);
            if (source == null)
                throw new InvalidOperationException("D27 NPC source prefab missing: " + NpcSourceRoot + definition.prefab);

            var wrapper = new GameObject("D27_NPC_" + definition.id);
            wrapper.transform.SetParent(parent, false);
            wrapper.transform.localPosition = definition.position;
            wrapper.transform.localRotation = Quaternion.Euler(0f, definition.yaw, 0f);
            wrapper.transform.localScale = Vector3.one;
            wrapper.isStatic = true;

            var visual = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (visual == null)
                throw new InvalidOperationException("D27 NPC source prefab could not be instantiated: " + definition.prefab);
            visual.name = definition.id + "_Visual";
            visual.transform.SetParent(wrapper.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            var bounds = RendererBounds(wrapper);
            if (bounds.size.y <= 0.001f)
                throw new InvalidOperationException("D27 NPC has empty renderer bounds: " + definition.prefab);
            visual.transform.localScale = Vector3.one * (definition.targetHeight / bounds.size.y);
            AlignToGround(wrapper.transform, definition.position.y);

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(NpcControllerPath);
            foreach (var animator in visual.GetComponentsInChildren<Animator>(true))
            {
                if (animator.runtimeAnimatorController == null && controller != null)
                    animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }

            DisableBlockingColliders(visual);
            var fallback = FindChild(visual.transform, "FarProxy");
            var adapter = wrapper.AddComponent<D20NpcReadOnlyPresentationAdapter>();
            adapter.ConfigureForEditor(
                "d27_scene_" + definition.id,
                definition.resourceKey,
                definition.jobPost,
                fallback == null ? null : fallback.gameObject,
                null);
            adapter.CachePresentationParts();
            ConfigurePerformanceRegistration(wrapper, "d27.presentation.npc." + definition.id,
                isAgent: true, isAnimal: false);

            var anchor = new GameObject("PresentationAnchor_" + definition.jobPost);
            anchor.transform.SetParent(wrapper.transform, false);
            anchor.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            var anchorComponent = anchor.AddComponent<D20NpcPresentationAnchor>();
            anchorComponent.ConfigureForEditor(
                D20NpcPresentationAnchorIds.ForJob(definition.jobPost),
                definition.jobPost,
                "npc_interaction_" + definition.jobPost.ToString().ToLowerInvariant());
        }

        private static void AddWildlife(Transform parent, WildlifeDefinition definition)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(WildlifeSourceRoot + definition.prefab);
            if (source == null)
                throw new InvalidOperationException("D27 wildlife source prefab missing: " + WildlifeSourceRoot + definition.prefab);

            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("D27 wildlife source prefab could not be instantiated: " + definition.prefab);
            instance.name = "D27_Wildlife_" + definition.id;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = definition.position;
            instance.transform.localRotation = Quaternion.Euler(0f, definition.yaw, 0f);
            instance.transform.localScale = Vector3.one * definition.scale;
            DisableBlockingColliders(instance);
            RestorePresentationRenderers(instance);
            if (IsAquatic(definition.id))
                AlignBelowWater(instance.transform, -0.04f);
            else
                AlignToGround(instance.transform, definition.position.y);

            if (instance.GetComponent< D20WildlifePresentationAdapter>() == null)
                throw new InvalidOperationException("D27 wildlife source has no read-only presentation adapter: " + definition.prefab);
            ConfigurePerformanceRegistration(instance, "d27.presentation.wildlife." + definition.id,
                isAgent: false, isAnimal: true);
        }

        private static void AddPet(Transform parent, WildlifeDefinition definition)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(PetSourceRoot + definition.prefab);
            if (source == null)
                throw new InvalidOperationException("D27 pet source prefab missing: " + PetSourceRoot + definition.prefab);

            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("D27 pet source prefab could not be instantiated: " + definition.prefab);
            instance.name = "D27_Pet_" + definition.id;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = definition.position;
            instance.transform.localRotation = Quaternion.Euler(0f, definition.yaw, 0f);
            instance.transform.localScale = Vector3.one * definition.scale;
            DisableBlockingColliders(instance);
            RestorePresentationRenderers(instance);
            AlignToGround(instance.transform, definition.position.y);
            if (instance.GetComponent<WhalesIslandReef.Presentation.D20.PetRare.D20PetRarePresentationAdapter>() == null)
                throw new InvalidOperationException("D27 pet source has no read-only presentation adapter: " + definition.prefab);
            ConfigurePerformanceRegistration(instance, "d27.presentation.pet." + definition.id,
                isAgent: false, isAnimal: true);
        }

        private static void ConfigurePerformanceRegistration(GameObject target, string stableId,
            bool isAgent, bool isAnimal)
        {
            var registration = target.AddComponent<D16PresentationRegistration>();
            registration.ConfigureRuntime(
                stableId,
                isAgent,
                isAnimal,
                usesAudio: false,
                playerRelevant: false,
                activeInteraction: false);
        }

        private static void DisableBlockingColliders(GameObject root)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (collider != null && !collider.isTrigger)
                    collider.enabled = false;
            }
        }

        private static void RestorePresentationRenderers(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                renderer.enabled = true;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                if (transform != null && string.Equals(transform.name, name, StringComparison.Ordinal)) return transform;
            return null;
        }

        private static void AlignToGround(Transform root, float groundY)
        {
            var bounds = RendererBounds(root.gameObject);
            var position = root.localPosition;
            position.y += groundY - bounds.min.y;
            root.localPosition = position;
        }

        private static void AlignBelowWater(Transform root, float waterY)
        {
            var bounds = RendererBounds(root.gameObject);
            if (bounds.size.y <= 0.001f) return;
            var position = root.localPosition;
            // Keep the highest visible vertex just below the water plane. This
            // prevents low-poly aquatic proxies from appearing to hover over
            // the beach while retaining their authored underwater placement.
            position.y += (waterY - 0.04f) - bounds.max.y;
            root.localPosition = position;
        }

        private static bool IsAquatic(string id)
        {
            var lower = (id ?? string.Empty).ToLowerInvariant();
            return lower.Contains("fish") || lower.Contains("manta")
                || lower.Contains("shark") || lower.Contains("whale");
        }

        private static Bounds RendererBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static BuildReport BuildReportForPrefab(string operation, string[] placed)
        {
            var report = new BuildReport
            {
                operation = operation,
                status = "PASS",
                r03_prefab = "game/" + R03PrefabPath,
                output_prefab = "game/" + OutputPrefabPath,
                npc_count = Npcs.Length,
                wildlife_count = Wildlife.Length,
                pet_count = Pets.Length,
                formal_scene_modified = false,
                systems_modified = false,
                save_data_modified = false,
                placed_ids = placed ?? Array.Empty<string>(),
                errors = Array.Empty<string>()
            };

            var root = PrefabUtility.LoadPrefabContents(OutputPrefabPath);
            if (root == null)
            {
                report.status = "BLOCKED";
                report.errors = new[] { "output_prefab_load_failed" };
                return report;
            }

            try
            {
                report.renderer_count = root.GetComponentsInChildren<Renderer>(true).Length;
                report.collider_count = root.GetComponentsInChildren<Collider>(true).Length;
                report.registration_count = root.GetComponentsInChildren<D16PresentationRegistration>(true).Length;
                var disabled = 0;
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                    if (collider != null && !collider.enabled && !collider.isTrigger) disabled++;
                report.disabled_non_trigger_collider_count = disabled;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            return report;
        }

        private static void WriteAudit(string operation)
        {
            EnsureFolder(ResultDirectory);
            var report = BuildReportForPrefab(operation, null);
            WriteReport(report);
            Debug.Log("[D27-S05] R03 character/wildlife presentation audit: " + report.status);
        }

        private static void WriteReport(BuildReport report)
        {
            var absolute = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ResultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute));
            File.WriteAllText(absolute, JsonUtility.ToJson(report, true) + "\n");
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void EditorSceneManagerMarkDirty(UnityEngine.Object asset)
        {
            if (asset != null) EditorUtility.SetDirty(asset);
        }
    }
}
#endif
