using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WhalesIslandReef.Editor.D20A02R02
{
    /// <summary>
    /// Copies the audited D20-A02-R02 work assets into an explicit Resources
    /// candidate path. This is a candidate-only runtime intake step; it does
    /// not edit InitialHarbor, the source prefab, or the formal D2 player path.
    /// </summary>
    public static class D20A02R02CandidateRuntimeBuilder
    {
        private const string SourcePrefabPath =
            "Assets/Prefabs/Characters/D20/R02/PlayerAppearance/D20_R02_PlayerVisual_Male.prefab";
        private const string SourceControllerPath = "Assets/Animations/D20/Player/D20_PlayerAnimation.controller";
        private const string CandidateOutfitSourcePath =
            "Assets/Prefabs/Characters/D18/R03/PlayerAppearance/D18_R03_A01_outfit_male_peasant_1675_local_r03.prefab";
        private const string CandidateRootPath = "Assets/Resources/D20/Candidate";
        private const string CandidatePrefabPath = CandidateRootPath + "/D20_R02_PlayerVisual_Male.prefab";
        private const string CandidateControllerPath = CandidateRootPath + "/D20_PlayerAnimation.controller";
        private const string FormalRootPath = "Assets/Resources/D20/Formal";
        private const string FormalPrefabPath = FormalRootPath + "/D20_R02_PlayerVisual_Male.prefab";
        private const string FormalControllerPath = FormalRootPath + "/D20_PlayerAnimation.controller";
        private const string ReadableSkinMaterialPath =
            "Assets/Art/Models/Characters/D20/Player/Materials/D20_PlayerSkin_Warm.mat";
        private const string ReadableSkinRuntimeMaterialPath =
            "Assets/Resources/D20/Materials/D20_PlayerSkin_Warm.mat";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string HarborScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string CandidatePlayerOutputPath =
            "tools/results/LOCAL-QUALITY/Q03/Q03_D20_R02_CANDIDATE_PLAYER_R01.exe";
        private const string CandidatePlayerLogPath =
            "tools/results/LOCAL-QUALITY/Q03/Q03_D20_R02_CANDIDATE_PLAYER_R01_BUILD.log";

        [MenuItem("Tools/WhalesIslandReef/D20-A02-R02/Build Candidate Runtime Resources")]
        public static void BuildFromMenu() => Build();

        public static void BuildFromCommandLine() => Build();

        [MenuItem("Tools/WhalesIslandReef/D20-A02-R02/Build Candidate Windows Player R01")]
        public static void BuildCandidatePlayerFromMenu() => BuildCandidatePlayer();

        [MenuItem("Tools/WhalesIslandReef/D20-A02-R02/Build Formal Runtime Resources")]
        public static void BuildFormalRuntimeResourcesFromMenu() => BuildFormalRuntimeResources();

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureFolder("Assets/Resources", "D20");
            EnsureFolder("Assets/Resources/D20", "Candidate");
            EnsureReadableSkinRuntimeMaterial();
            BuildFlattenedGenericCandidatePrefab();
            CopyReplacing(SourceControllerPath, CandidateControllerPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CandidatePrefabPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CandidateControllerPath);
            if (prefab == null || controller == null)
                throw new InvalidOperationException("Candidate runtime resources were not imported.");

            ApplyReadableSkinMaterial(CandidatePrefabPath);

            Debug.Log("[D20-A02-R02] Candidate runtime Resources built: " + CandidateRootPath);
        }

        /// <summary>
        /// Admits the audited R02 appearance/controller pair to an explicit
        /// runtime Resources path. The source prefab and its valid Humanoid
        /// Avatar remain untouched; the D2 formal bridge performs the generic
        /// path adaptation on its runtime instance because the generated D20
        /// clips are transform-path clips rather than Humanoid motion clips.
        /// </summary>
        public static void BuildFormalRuntimeResources()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureFolder("Assets/Resources", "D20");
            EnsureFolder("Assets/Resources/D20", "Formal");
            EnsureReadableSkinRuntimeMaterial();
            CopyReplacing(SourcePrefabPath, FormalPrefabPath);
            CopyReplacing(SourceControllerPath, FormalControllerPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FormalPrefabPath);
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(FormalControllerPath);
            if (prefab == null || controller == null)
                throw new InvalidOperationException("Formal runtime Resources were not imported.");

            ApplyReadableSkinMaterial(FormalPrefabPath);

            var animator = prefab.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new InvalidOperationException("Formal runtime prefab lost its audited Humanoid Avatar.");

            Debug.Log("[D20-A02-R02] Formal runtime Resources built: " + FormalRootPath);
        }

        private static void ApplyReadableSkinMaterial(string prefabPath)
        {
            var skin = AssetDatabase.LoadAssetAtPath<Material>(ReadableSkinMaterialPath);
            if (skin == null)
                throw new InvalidOperationException("Missing readable D20 skin material: " + ReadableSkinMaterialPath);

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null)
                throw new InvalidOperationException("Could not open runtime prefab for material repair: " + prefabPath);

            try
            {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                var matched = 0;
                foreach (var renderer in renderers)
                {
                    if (!string.Equals(renderer.gameObject.name, "SuperHero_Male", StringComparison.Ordinal))
                        continue;

                    var materials = renderer.sharedMaterials;
                    if (materials == null || materials.Length == 0)
                        materials = new[] { skin };
                    else
                        materials[0] = skin;
                    renderer.sharedMaterials = materials;
                    matched++;
                }

                if (matched != 1)
                    throw new InvalidOperationException(
                        "Expected exactly one SuperHero_Male renderer in " + prefabPath + ", found " + matched + ".");

                var saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out var success);
                if (!success || saved == null)
                    throw new InvalidOperationException("Could not save readable-skin repair: " + prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureReadableSkinRuntimeMaterial()
        {
            EnsureFolder("Assets/Resources/D20", "Materials");
            CopyReplacing(ReadableSkinMaterialPath, ReadableSkinRuntimeMaterialPath);
        }

        private static void BuildFlattenedGenericCandidatePrefab()
        {
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
            if (sourcePrefab == null)
                throw new InvalidOperationException("Missing source prefab: " + SourcePrefabPath);

            if (AssetDatabase.LoadMainAssetAtPath(CandidatePrefabPath) != null)
                AssetDatabase.DeleteAsset(CandidatePrefabPath);

            var instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (instance == null)
                throw new InvalidOperationException("Could not instantiate source prefab: " + SourcePrefabPath);

            try
            {
                PrefabUtility.UnpackPrefabInstance(
                    instance,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);

                var visualWrapper = FindByName(instance.transform, "Visual_player_visual_male_r02");
                if (visualWrapper == null)
                    throw new InvalidOperationException(
                        "R02 source prefab has no Visual_player_visual_male_r02 wrapper.");

                var children = new Transform[visualWrapper.childCount];
                for (var i = 0; i < visualWrapper.childCount; i++)
                    children[i] = visualWrapper.GetChild(i);
                foreach (var child in children)
                    child.SetParent(instance.transform, true);
                UnityEngine.Object.DestroyImmediate(visualWrapper.gameObject);

                AddCandidateOutfit(instance);

                var animator = instance.GetComponent<Animator>();
                if (animator == null)
                    throw new InvalidOperationException("Flattened R02 candidate has no root Animator.");

                // Q03-A19: the D20 action set is now authored Humanoid motion
                // (humanMotion=true), not generated generic transform-path
                // clips. The previous candidate-only workaround cleared the
                // Avatar so the clips could be consumed as generic paths; with
                // Humanoid clips that would leave the Animator with nothing to
                // retarget onto. Keep the audited valid Humanoid Avatar and let
                // D20PlayerCandidatePresentationBridge select its Humanoid pose
                // adapter through HasGenericPathClip() == false.
                if (animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                    throw new InvalidOperationException(
                        "Flattened R02 candidate lost its valid Humanoid Avatar; " +
                        "the Q03-A19 Humanoid action set cannot retarget without it.");

                var saved = PrefabUtility.SaveAsPrefabAsset(instance, CandidatePrefabPath, out var success);
                if (!success || saved == null)
                    throw new InvalidOperationException(
                        "Could not save flattened candidate prefab: " + CandidatePrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void AddCandidateOutfit(GameObject candidateRoot)
        {
            var outfitAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CandidateOutfitSourcePath);
            if (outfitAsset == null)
                throw new InvalidOperationException("Missing candidate outfit source: " + CandidateOutfitSourcePath);

            var outfit = PrefabUtility.InstantiatePrefab(outfitAsset) as GameObject;
            if (outfit == null)
                throw new InvalidOperationException("Could not instantiate candidate outfit: " + CandidateOutfitSourcePath);

            try
            {
                outfit.name = "D20_CandidateOutfit_MalePeasant";
                outfit.transform.SetParent(candidateRoot.transform, false);
                PrefabUtility.UnpackPrefabInstance(
                    outfit,
                    PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);

                var targetArmature = FindByName(candidateRoot.transform, "Armature");
                var sourceArmature = FindByName(outfit.transform, "Armature");
                if (targetArmature == null || sourceArmature == null)
                    throw new InvalidOperationException("Candidate outfit or R02 body is missing Armature.");

                var renderers = outfit.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                if (renderers.Length == 0)
                    throw new InvalidOperationException("Candidate outfit has no skinned renderers.");

                foreach (var renderer in renderers)
                {
                    var mappedBones = new Transform[renderer.bones.Length];
                    for (var index = 0; index < renderer.bones.Length; index++)
                    {
                        var path = RelativePath(sourceArmature, renderer.bones[index]);
                        mappedBones[index] = FindByPath(targetArmature, path);
                        if (mappedBones[index] == null)
                            throw new InvalidOperationException(
                                "Candidate outfit bone path is not present on R02 body: " + path);
                    }

                    var rootBonePath = RelativePath(sourceArmature, renderer.rootBone);
                    var mappedRootBone = FindByPath(targetArmature, rootBonePath);
                    if (mappedRootBone == null)
                        throw new InvalidOperationException(
                            "Candidate outfit root bone path is not present on R02 body: " + rootBonePath);

                    renderer.bones = mappedBones;
                    renderer.rootBone = mappedRootBone;
                    renderer.updateWhenOffscreen = true;
                }

                foreach (var lod in outfit.GetComponentsInChildren<LODGroup>(true))
                    UnityEngine.Object.DestroyImmediate(lod);
                UnityEngine.Object.DestroyImmediate(sourceArmature.gameObject);
                Debug.Log("[D20-A02-R02] Candidate-only male peasant outfit rebound to the R02 generic skeleton: renderers=" + renderers.Length);
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(outfit);
                throw;
            }
        }

        private static string RelativePath(Transform root, Transform child)
        {
            if (root == null || child == null) return null;
            var names = new List<string>();
            var current = child;
            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }

            if (current != root) return null;
            names.Reverse();
            return string.Join("/", names);
        }

        private static Transform FindByPath(Transform root, string path)
        {
            if (root == null || path == null) return null;
            if (path.Length == 0) return root;

            var current = root;
            foreach (var name in path.Split('/'))
            {
                current = current.Find(name);
                if (current == null) return null;
            }
            return current;
        }

        public static void BuildCandidatePlayer()
        {
            Build();
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var output = Path.Combine(projectRoot, CandidatePlayerOutputPath).Replace('\\', '/');
            var log = Path.Combine(projectRoot, CandidatePlayerLogPath).Replace('\\', '/');
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var options = new BuildPlayerOptions
            {
                scenes = new[] { MainMenuScenePath, HarborScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };

            Debug.Log("[D20-A02-R02] Building opt-in candidate Windows Player: " + output);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("D20-A02-R02 candidate Player build failed: " +
                    report.summary.result + "; errors=" + report.summary.totalErrors);

            File.WriteAllText(log,
                "D20-A02-R02 candidate-only Windows Player build succeeded\n" +
                "scenes=" + MainMenuScenePath + ";" + HarborScenePath + "\n" +
                "output=" + output + "\n" +
                "build_options=BuildOptions.None\n" +
                "runtime_flag=-d20Candidate\n" +
                "size_bytes=" + new FileInfo(output).Length + "\n" +
                "timestamp_utc=" + DateTime.UtcNow.ToString("O") + "\n");
            Debug.Log("[D20-A02-R02] Candidate-only Windows Player build succeeded: " + output);
        }

        private static void CopyReplacing(string sourcePath, string destinationPath)
        {
            if (AssetDatabase.LoadMainAssetAtPath(sourcePath) == null)
                throw new InvalidOperationException("Missing source asset: " + sourcePath);
            if (AssetDatabase.LoadMainAssetAtPath(destinationPath) != null)
                AssetDatabase.DeleteAsset(destinationPath);
            if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
                throw new InvalidOperationException("Could not copy asset to: " + destinationPath);
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root == null) return null;
            if (string.Equals(root.name, name, StringComparison.Ordinal)) return root;
            foreach (Transform child in root)
            {
                var found = FindByName(child, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
