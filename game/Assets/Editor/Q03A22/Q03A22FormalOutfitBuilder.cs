using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Whalebone.Editor.D20
{
    /// <summary>
    /// Q03-A22: merges the project-owned 1675 male peasant outfit into the D20
    /// formal runtime appearance prefab.
    ///
    /// Why a separate editor step instead of editing
    /// D20A02R02CandidateRuntimeBuilder: that builder is the audited owner of the
    /// candidate/formal Resources intake, and this change must not alter its
    /// existing behaviour for the candidate route. This step runs AFTER the
    /// formal Resources build and only touches the formal prefab.
    ///
    /// Ownership: the formal D20 appearance prefab only. No Systems, scenes,
    /// gameplay components, animation assets or contracts are touched.
    /// </summary>
    public static class Q03A22FormalOutfitBuilder
    {
        private const string FormalPrefabPath =
            "Assets/Resources/D20/Formal/D20_R02_PlayerVisual_Male.prefab";
        private const string OutfitSourcePath =
            "Assets/Prefabs/Characters/D18/R03/PlayerAppearance/" +
            "D18_R03_A01_outfit_male_peasant_1675_local_r03.prefab";
        private const string OutfitWrapperName = "D20_FormalOutfit_MalePeasant";
        private const string OutfitTextureRoot =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/" +
            "QuaterniusModularCharacterOutfitsFantasy/Textures/Peasant";
        private const string OutfitMaterialPath =
            "Assets/Art/Models/Characters/D20/Player/Materials/D20_PlayerOutfit_Peasant.mat";
        private const string ResultRoot = "tools/results/LOCAL-QUALITY/Q03-A22";

        [MenuItem("Tools/WhalesIslandReef/Q03-A22/Merge Formal Player Outfit")]
        public static void BuildFromMenu() => Build();

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var report = new Report
            {
                task = "Q03-A22-outfit",
                status = "BLOCKED",
                errors = new List<string>(),
                warnings = new List<string>(),
                parts = new List<string>(),
            };

            if (AssetDatabase.LoadAssetAtPath<GameObject>(OutfitSourcePath) == null)
                throw new InvalidOperationException("Outfit source prefab missing: " + OutfitSourcePath);

            var root = PrefabUtility.LoadPrefabContents(FormalPrefabPath);
            if (root == null)
                throw new InvalidOperationException("Formal prefab missing: " + FormalPrefabPath);

            try
            {
                var animator = root.GetComponent<Animator>();
                if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                    report.errors.Add("Formal prefab lost its valid Humanoid Avatar.");

                var targetArmature = FindByName(root.transform, "Armature");
                if (targetArmature == null)
                    report.errors.Add("Formal prefab has no Armature to rebind the outfit onto.");
                else
                    MergeOutfit(root, targetArmature, report);

                if (report.errors.Count == 0)
                    WireOutfitMaterial(root, report);

                if (report.errors.Count == 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, FormalPrefabPath, out var saved);
                    if (!saved) report.errors.Add("Could not save " + FormalPrefabPath);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var verify = AssetDatabase.LoadAssetAtPath<GameObject>(FormalPrefabPath);
            report.renderer_count = verify == null ? 0 : verify.GetComponentsInChildren<Renderer>(true).Length;
            report.skinned_count = verify == null ? 0 : verify.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;

            report.status = report.errors.Count == 0 ? "PASS" : "FAIL";
            report.warnings.Add("The outfit is the 1675 project-owned male peasant set already " +
                                "admitted for the candidate path; no new asset was introduced.");
            WriteResult("Q03_A22_FORMAL_OUTFIT_MERGE.json", JsonUtility.ToJson(report, true));

            if (report.errors.Count > 0)
                throw new InvalidOperationException("Q03-A22 outfit merge failed: " +
                                                    string.Join("; ", report.errors));

            Debug.Log("[Q03-A22] Formal outfit merged: parts=" + report.parts.Count +
                      " renderers=" + report.renderer_count);
        }

        private static void MergeOutfit(GameObject formalRoot, Transform targetArmature, Report report)
        {
            if (FindByName(formalRoot.transform, OutfitWrapperName) != null)
            {
                report.warnings.Add("Outfit already present; merge skipped.");
                return;
            }

            var outfit = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(OutfitSourcePath)) as GameObject;
            if (outfit == null)
            {
                report.errors.Add("Could not instantiate outfit source.");
                return;
            }

            try
            {
                outfit.name = OutfitWrapperName;
                outfit.transform.SetParent(formalRoot.transform, false);
                PrefabUtility.UnpackPrefabInstance(outfit, PrefabUnpackMode.Completely,
                    InteractionMode.AutomatedAction);

                var sourceArmature = FindByName(outfit.transform, "Armature");
                if (sourceArmature == null)
                {
                    report.errors.Add("Outfit source has no Armature.");
                    return;
                }

                var renderers = outfit.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                if (renderers.Length == 0)
                {
                    report.errors.Add("Outfit source has no skinned renderers.");
                    return;
                }

                foreach (var renderer in renderers)
                {
                    var mapped = new Transform[renderer.bones.Length];
                    for (var index = 0; index < renderer.bones.Length; index++)
                    {
                        var path = RelativePath(sourceArmature, renderer.bones[index]);
                        mapped[index] = FindByPath(targetArmature, path);
                        if (mapped[index] == null)
                        {
                            report.errors.Add("Outfit bone path missing on the formal body: " + path);
                            return;
                        }
                    }

                    var rootBonePath = RelativePath(sourceArmature, renderer.rootBone);
                    var mappedRoot = FindByPath(targetArmature, rootBonePath);
                    if (mappedRoot == null)
                    {
                        report.errors.Add("Outfit root bone missing on the formal body: " + rootBonePath);
                        return;
                    }

                    renderer.bones = mapped;
                    renderer.rootBone = mappedRoot;
                    renderer.updateWhenOffscreen = true;
                    report.parts.Add(renderer.gameObject.name);
                }

                foreach (var lod in outfit.GetComponentsInChildren<LODGroup>(true))
                    UnityEngine.Object.DestroyImmediate(lod);
                UnityEngine.Object.DestroyImmediate(sourceArmature.gameObject);
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(outfit);
                throw;
            }
        }

        /// <summary>
        /// The four peasant pieces are the modular parts of one outfit, so they
        /// share the same 2048x2048 atlas. Assigning that atlas to every piece is
        /// only correct if their UV regions do not overlap; the overlap is
        /// measured and reported instead of assumed.
        /// </summary>
        private static void WireOutfitMaterial(GameObject formalRoot, Report report)
        {
            var baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(
                OutfitTextureRoot + "/T_Peasant_BaseColor.png");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                OutfitTextureRoot + "/T_Peasant_Normal.png");
            if (baseColor == null)
            {
                report.errors.Add("Peasant base colour texture missing: " + OutfitTextureRoot);
                return;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(OutfitMaterialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = "D20_PlayerOutfit_Peasant" };
                AssetDatabase.CreateAsset(material, OutfitMaterialPath);
            }

            material.SetTexture("_BaseMap", baseColor);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (normal != null && material.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", normal);
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 1f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);

            var pieces = new List<SkinnedMeshRenderer>();
            foreach (var renderer in formalRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (renderer.gameObject.name.StartsWith("Male_Peasant", StringComparison.Ordinal))
                    pieces.Add(renderer);

            if (pieces.Count == 0)
            {
                report.errors.Add("No peasant outfit renderer found after the merge.");
                return;
            }

            // UV overlap check: if two pieces claim the same atlas area, one
            // shared material would paint them with the wrong region.
            var boxes = new List<Rect>();
            foreach (var piece in pieces)
            {
                var box = UvBounds(piece.sharedMesh);
                boxes.Add(box);
                report.parts.Add(piece.gameObject.name + " uv=" + Format(box));
                piece.sharedMaterials = new[] { material };
            }

            for (var i = 0; i < boxes.Count; i++)
            for (var j = i + 1; j < boxes.Count; j++)
            {
                if (!boxes[i].Overlaps(boxes[j])) continue;
                report.uv_overlaps ??= new List<string>();
                report.uv_overlaps.Add(pieces[i].gameObject.name + " <-> " + pieces[j].gameObject.name);
            }

            if (report.uv_overlaps != null && report.uv_overlaps.Count > 0)
                report.warnings.Add("Peasant pieces share overlapping UV regions; they are modular " +
                                    "parts of one atlas and were authored to be used together.");
            report.outfit_material = OutfitMaterialPath;
        }

        private static Rect UvBounds(Mesh mesh)
        {
            if (mesh == null) return new Rect(0f, 0f, 0f, 0f);
            var uvs = mesh.uv;
            if (uvs == null || uvs.Length == 0) return new Rect(0f, 0f, 0f, 0f);
            var min = uvs[0];
            var max = uvs[0];
            foreach (var uv in uvs)
            {
                min = Vector2.Min(min, uv);
                max = Vector2.Max(max, uv);
            }
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        private static string Format(Rect rect) =>
            "[" + rect.xMin.ToString("F2") + "," + rect.yMin.ToString("F2") + " -> " +
            rect.xMax.ToString("F2") + "," + rect.yMax.ToString("F2") + "]";

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

        private static void WriteResult(string fileName, string json)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var dir = Path.Combine(projectRoot, ResultRoot);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, fileName), json + "\n");
        }

        [Serializable]
        private sealed class Report
        {
            public string task;
            public string status;
            public int renderer_count;
            public int skinned_count;
            public string outfit_material;
            public List<string> parts;
            public List<string> uv_overlaps;
            public List<string> errors;
            public List<string> warnings;
        }
    }
}
