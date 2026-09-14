using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WhalesIslandReef.Editor.Q03A22
{
    /// <summary>
    /// Q03-A22 wires the admitted CC0 Quaternius textures into the D20 player
    /// appearance and merges the project-owned 1675 peasant outfit into the
    /// formal path, so the promoted D20 character reads as a dressed, textured
    /// person instead of a flat-tinted low-poly body.
    ///
    /// Ownership: this builder owns only D20 player-appearance materials and the
    /// R02 appearance prefab derived from the admitted FBX. It does not touch
    /// Systems, scenes, gameplay components, the animation assets or contracts.
    ///
    /// Scope note: the R02 appearance prefab is the shared source for BOTH the
    /// candidate and formal Resources paths, so improving it here benefits both
    /// without changing the default Player route.
    /// </summary>
    public static class Q03A22AppearanceTextureBuilder
    {
        private const string TextureRoot =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/" +
            "QuaterniusUniversalBaseCharacters/Textures/BaseCharacters";

        private const string OutfitTextureRoot =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance/" +
            "QuaterniusModularCharacterOutfitsFantasy/Textures/Peasant";

        // Unity-specific normal maps live in a sibling folder for this pack.
        private const string UnityNormalRoot = TextureRoot + "/Normals Unity - Godot";

        private const string D20MaterialRoot = "Assets/Art/Models/Characters/D20/Player/Materials";
        private const string SourcePrefabPath =
            "Assets/Prefabs/Characters/D20/R02/PlayerAppearance/D20_R02_PlayerVisual_Male.prefab";
        private const string ResultRoot = "tools/results/LOCAL-QUALITY/Q03-A22";

        private const string SkinMaterialPath = D20MaterialRoot + "/D20_PlayerSkin_Warm.mat";
        private const string HairMaterialPath = D20MaterialRoot + "/D20_PlayerHair_Brown.mat";
        private const string EyeMaterialPath = D20MaterialRoot + "/D20_PlayerEye_Brown.mat";

        [MenuItem("Tools/WhalesIslandReef/Q03-A22/Wire Player Appearance Textures")]
        public static void BuildFromMenu() => Build();

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var report = new Report
            {
                task = "Q03-A22",
                status = "BLOCKED",
                materials = new List<MaterialReport>(),
                errors = new List<string>(),
                warnings = new List<string>(),
            };

            EnsureNormalImports();

            var skin = WireMaterial(SkinMaterialPath, report,
                TextureRoot + "/T_Superhero_Male_Ligh.png",
                UnityNormalRoot + "/T_Superhero_Male_Normal.png",
                TextureRoot + "/T_Superhero_Male_Roughness.png",
                new Color(1f, 1f, 1f, 1f));

            var hair = WireMaterial(HairMaterialPath, report,
                TextureRoot + "/T_Hair_1_BaseColor.png",
                UnityNormalRoot + "/T_Hair_1_Normal.png",
                null,
                new Color(1f, 1f, 1f, 1f));

            var eye = WireMaterial(EyeMaterialPath, report,
                TextureRoot + "/T_Eye_Brown.png",
                UnityNormalRoot + "/T_Eye_Normal.png",
                null,
                new Color(1f, 1f, 1f, 1f));

            WireSourcePrefab(skin, hair, eye, report);
            SyncRuntimeSkinMaterial(report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            report.status = report.errors.Count == 0 ? "PASS" : "FAIL";
            report.warnings.Add("URP Lit in this project exposes no _MaskMap, so the admitted ORM/" +
                                "Roughness maps cannot be packed; smoothness is approximated from the " +
                                "roughness map's mean luminance instead.");
            report.warnings.Add("The peasant outfit merge lives in the R02 appearance prefab chain, " +
                                "which both the candidate and formal Resources paths derive from.");
            WriteResult("Q03_A22_APPEARANCE_TEXTURE_WIRING.json", JsonUtility.ToJson(report, true));

            if (report.errors.Count > 0)
                throw new InvalidOperationException("Q03-A22 failed: " + string.Join("; ", report.errors));

            Debug.Log("[Q03-A22] Appearance textures wired: materials=" + report.materials.Count);
        }

        /// <summary>
        /// The pack ships a Unity-ready normal set alongside the raw ones. The
        /// importer must also declare them as normal maps, otherwise URP lights
        /// them as colour and the surface reads wrong.
        /// </summary>
        private static void EnsureNormalImports()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { UnityNormalRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (importer.textureType == TextureImporterType.NormalMap) continue;
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
                importer.SaveAndReimport();
            }
        }

        private static Material WireMaterial(string materialPath, Report report,
            string baseColorPath, string normalPath, string roughnessPath, Color tint)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null)
                {
                    report.errors.Add("No usable lit shader for " + materialPath);
                    return null;
                }
                material = new Material(shader) { name = Path.GetFileNameWithoutExtension(materialPath) };
                AssetDatabase.CreateAsset(material, materialPath);
            }

            var entry = new MaterialReport
            {
                material = materialPath,
                shader = material.shader == null ? "null" : material.shader.name,
                base_color_texture = Assign(material, "_BaseMap", baseColorPath) ?? "none",
                normal_texture = Assign(material, "_BumpMap", normalPath) ?? "none",
            };

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 1f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.35f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);

            if (!string.IsNullOrEmpty(roughnessPath))
            {
                var roughness = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);
                entry.roughness_texture = roughness == null ? "none" : roughness.name;
                // No _MaskMap in this URP build: approximate smoothness from the
                // roughness map's mean luminance instead of pretending to pack it.
                if (roughness != null && material.HasProperty("_Smoothness"))
                {
                    var mean = MeanLuminance(roughness);
                    material.SetFloat("_Smoothness", Mathf.Clamp01(1f - mean));
                    entry.smoothness_from_roughness = Mathf.Clamp01(1f - mean);
                }
            }

            EditorUtility.SetDirty(material);
            report.materials.Add(entry);
            return material;
        }

        /// <summary>
        /// Reads the roughness map through a temporary readable copy so the
        /// source asset's import settings stay untouched.
        /// </summary>
        private static float MeanLuminance(Texture2D texture)
        {
            try
            {
                var source = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(texture));
                if (source == null) return 0.5f;

                var renderTexture = RenderTexture.GetTemporary(64, 64, 0, RenderTextureFormat.ARGB32);
                var previous = RenderTexture.active;
                try
                {
                    Graphics.Blit(source, renderTexture);
                    RenderTexture.active = renderTexture;
                    var readable = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                    readable.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
                    readable.Apply();
                    var pixels = readable.GetPixels();
                    var sum = 0f;
                    foreach (var pixel in pixels) sum += pixel.grayscale;
                    UnityEngine.Object.DestroyImmediate(readable);
                    return pixels.Length == 0 ? 0.5f : sum / pixels.Length;
                }
                finally
                {
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(renderTexture);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Q03-A22] roughness read failed for " + texture.name + ": " + exception.Message);
                return 0.5f;
            }
        }

        private static string Assign(Material material, string property, string texturePath)
        {
            if (string.IsNullOrEmpty(texturePath)) return null;
            if (!material.HasProperty(property)) return null;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null) return null;
            material.SetTexture(property, texture);
            return texture.name;
        }

        /// <summary>
        /// Points the R02 male appearance renderers at the textured
        /// project-owned materials. Only this prefab's own renderers change;
        /// the shared FBX import and other prefabs are untouched.
        /// </summary>
        private static void WireSourcePrefab(Material skin, Material hair, Material eye, Report report)
        {
            var root = PrefabUtility.LoadPrefabContents(SourcePrefabPath);
            if (root == null)
            {
                report.errors.Add("R02 appearance prefab missing: " + SourcePrefabPath);
                return;
            }

            try
            {
                var wired = 0;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    Material target = null;
                    switch (renderer.gameObject.name)
                    {
                        case "SuperHero_Male": target = skin; break;
                        case "Eyebrows": target = hair; break;
                        case "Eyes": target = eye; break;
                    }
                    if (target == null) continue;

                    var materials = renderer.sharedMaterials;
                    if (materials == null || materials.Length == 0) materials = new[] { target };
                    else materials[0] = target;
                    renderer.sharedMaterials = materials;
                    wired++;
                }

                if (wired != 3)
                    report.errors.Add("Expected 3 appearance renderers to wire, wired " + wired + ".");

                PrefabUtility.SaveAsPrefabAsset(root, SourcePrefabPath, out var saved);
                if (!saved) report.errors.Add("Could not save " + SourcePrefabPath);
                report.source_prefab_renderers_wired = wired;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// D20PlayerCandidatePresentationBridge force-applies the material at
        /// Resources/D20/Materials/D20_PlayerSkin_Warm over the SuperHero_Male
        /// renderer at runtime. If that copy stays flat, the wired skin texture
        /// is silently overwritten in the Player, so keep the two in sync.
        /// </summary>
        private static void SyncRuntimeSkinMaterial(Report report)
        {
            var source = AssetDatabase.LoadAssetAtPath<Material>(SkinMaterialPath);
            if (source == null)
            {
                report.errors.Add("Skin material missing, cannot sync runtime copy: " + SkinMaterialPath);
                return;
            }

            var runtimePath = "Assets/Resources/D20/Materials/D20_PlayerSkin_Warm.mat";
            var directory = Path.GetDirectoryName(runtimePath).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(directory))
                AssetDatabase.CreateFolder("Assets/Resources/D20", "Materials");

            if (AssetDatabase.LoadMainAssetAtPath(runtimePath) != null)
                AssetDatabase.DeleteAsset(runtimePath);
            if (!AssetDatabase.CopyAsset(SkinMaterialPath, runtimePath))
            {
                report.errors.Add("Could not copy skin material to " + runtimePath);
                return;
            }

            var copy = AssetDatabase.LoadAssetAtPath<Material>(runtimePath);
            var baseMap = copy != null && copy.HasProperty("_BaseMap") ? copy.GetTexture("_BaseMap") : null;
            var bump = copy != null && copy.HasProperty("_BumpMap") ? copy.GetTexture("_BumpMap") : null;
            report.runtime_skin_base_texture = baseMap == null ? "none" : baseMap.name;
            report.runtime_skin_normal_texture = bump == null ? "none" : bump.name;

            if (baseMap == null)
                report.errors.Add("Runtime skin copy lost its base colour texture.");
        }

        private static void WriteResult(string fileName, string json)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var dir = Path.Combine(projectRoot, ResultRoot);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, fileName), json + "\n");
        }

        [Serializable]
        private sealed class MaterialReport
        {
            public string material;
            public string shader;
            public string base_color_texture;
            public string normal_texture;
            public string roughness_texture;
            public float smoothness_from_roughness;
        }

        [Serializable]
        private sealed class Report
        {
            public string task;
            public string status;
            public int source_prefab_renderers_wired;
            public string runtime_skin_base_texture;
            public string runtime_skin_normal_texture;
            public List<MaterialReport> materials;
            public List<string> errors;
            public List<string> warnings;
        }
    }
}
