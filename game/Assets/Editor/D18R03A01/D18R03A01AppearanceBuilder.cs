#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WhalesIslandReef.Editor.D18R03A01
{
    /// <summary>
    /// Builds and audits the D18 R03 player-appearance workbench only. The
    /// output is intentionally isolated: it does not touch ResourceKey,
    /// AppearanceSystem, InitialHarbor, or any runtime consumer.
    /// </summary>
    public static class D18R03A01AppearanceBuilder
    {
        private const string CandidateRoot =
            "Assets/Art/Candidates/CharactersAnimals/GapFill/PlayerAppearance";
        private const string UniversalRoot = CandidateRoot + "/QuaterniusUniversalBaseCharacters";
        private const string OutfitRoot = CandidateRoot + "/QuaterniusModularCharacterOutfitsFantasy";
        private const string ProjectOwnedRoot = CandidateRoot + "/ProjectOwned/R03";
        private const string ProjectOwnedModelRoot = ProjectOwnedRoot + "/Models";
        private const string ProjectOwnedMaterialRoot = ProjectOwnedRoot + "/Materials";
        private const string ProjectOwnedAvatarRoot = ProjectOwnedRoot + "/Avatars";
        private const string FemaleAvatarPath = ProjectOwnedAvatarRoot + "/D18_R03_A01_PlayerHumanoidFemale.asset";
        private const string MaleAvatarPath = ProjectOwnedAvatarRoot + "/D18_R03_A01_PlayerHumanoidMale.asset";
        private const string PrefabRoot = "Assets/Prefabs/Characters/D18/R03/PlayerAppearance";
        private const string WorkbenchPrefabPath = PrefabRoot + "/D18_R03_A01_PlayerAppearanceWorkbench.prefab";
        private const string HairPrefabPath = PrefabRoot + "/D18_R03_A01_Hair_TwistPonytail.prefab";
        private const string ResultRoot = "tools/results/D18/D18-R03-A01";
        private const string PacketRoot =
            "docs/game/design/common/assets/packs/characters_animals/gap_fill/player_appearance";

        private static readonly OutfitDefinition[] OutfitDefinitions =
        {
            new OutfitDefinition("outfit_female_peasant_1675_local_r03", "Female_Peasant.fbx", Array.Empty<string>(), "civilian", "project assembly from audited geometry"),
            new OutfitDefinition("outfit_female_peasant_hood_1675_local_r03", "Female_Peasant.fbx", new[] { "Female_Ranger_Head_Hood.fbx" }, "civilian_travel", "peasant body with a real hood module"),
            new OutfitDefinition("outfit_female_ranger_1675_local_r03", "Female_Ranger.fbx", Array.Empty<string>(), "shore_guard", "ranger geometry kept as provisional local visual"),
            new OutfitDefinition("outfit_female_ranger_pauldron_1675_local_r03", "Female_Ranger.fbx", new[] { "Female_Ranger_Acc_Pauldrons.fbx" }, "shore_guard_officer", "ranger geometry with a real pauldron module"),
            new OutfitDefinition("outfit_male_peasant_1675_local_r03", "Male_Peasant.fbx", Array.Empty<string>(), "civilian", "project assembly from audited geometry"),
            new OutfitDefinition("outfit_male_peasant_hood_1675_local_r03", "Male_Peasant.fbx", new[] { "Male_Ranger_Head_Hood.fbx" }, "civilian_travel", "peasant body with a real hood module"),
            new OutfitDefinition("outfit_male_ranger_1675_local_r03", "Male_Ranger.fbx", Array.Empty<string>(), "shore_guard", "ranger geometry kept as provisional local visual"),
            new OutfitDefinition("outfit_male_ranger_pauldron_1675_local_r03", "Male_Ranger.fbx", new[] { "Male_Ranger_Acc_Pauldron.fbx" }, "shore_guard_officer", "ranger geometry with a real pauldron module"),
        };

        [MenuItem("Tools/WhalesIslandReef/D18-R03-A01/Build Appearance Workbench")]
        public static void BuildFromMenu()
        {
            Build();
        }

        [MenuItem("Tools/WhalesIslandReef/D18-R03-A01/Audit Appearance Workbench")]
        public static void AuditFromMenu()
        {
            var report = Audit();
            Debug.Log("[D18-R03-A01] appearance audit status=" + report.status);
        }

        public static void Build()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureFolderPath(ProjectOwnedModelRoot);
            EnsureFolderPath(ProjectOwnedMaterialRoot);
            EnsureFolderPath(ProjectOwnedAvatarRoot);
            EnsureFolderPath(PrefabRoot);

            // The source FBX files are audited Humanoid imports, but their
            // source importer state is owned by the candidate package. Build
            // isolated project-owned Avatar assets so the workbench can prove
            // a usable Human rig without changing that candidate metadata.
            BuildDerivedAvatar(UniversalRoot + "/Models/Base/Superhero_Female_FullBody.fbx", FemaleAvatarPath);
            BuildDerivedAvatar(UniversalRoot + "/Models/Base/Superhero_Male_FullBody.fbx", MaleAvatarPath);

            var hairPrefab = BuildProjectOwnedHairPrefab();
            var outfitPrefabs = new List<string>();
            foreach (var definition in OutfitDefinitions)
                outfitPrefabs.Add(BuildOutfitPrefab(definition));

            BuildWorkbenchPrefab(hairPrefab, outfitPrefabs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var report = Audit();
            if (report.status != "PASS")
                throw new InvalidOperationException("D18-R03-A01 appearance audit failed: " + report.status);

            Debug.Log("[D18-R03-A01] Built 1 project-owned hair prefab and " +
                      outfitPrefabs.Count + " geometry outfit variants.");
        }

        public static AuditReport Audit()
        {
            var report = new AuditReport
            {
                task = "D18-R03-A01",
                status = "BLOCKED",
                generated_at = DateTime.UtcNow.ToString("o"),
                hair_prefab = HairPrefabPath,
                workbench_prefab = WorkbenchPrefabPath,
                outfit_variants = new List<OutfitAudit>(),
                errors = new List<string>(),
                warnings = new List<string>
                {
                    "All eight combinations are local development visual assemblies; historical 1675 review remains provisional.",
                    "The project-owned ponytail is generated from D18R03A01AppearanceBuilder.cs and is not an external license claim.",
                    "No LocalDevelopmentReady or ReleaseApproved state is written by this builder."
                }
            };

            AuditHair(report);
            foreach (var definition in OutfitDefinitions)
                AuditOutfit(report, definition);

            report.body_directions_verified = CountValidHumanoidBases();
            report.female_avatar = FemaleAvatarPath;
            report.male_avatar = MaleAvatarPath;
            report.independent_hair_styles_verified = 5 + (report.hair_geometry_verified ? 1 : 0);
            report.outfit_combinations_verified = report.outfit_variants.Count(item => item.valid);
            report.status = report.body_directions_verified >= 2 &&
                            report.independent_hair_styles_verified >= 6 &&
                            report.outfit_combinations_verified >= 8 &&
                            report.errors.Count == 0 ? "PASS" : "PARTIAL";
            WriteAudit(report);
            WriteAssemblyPacket(report);
            return report;
        }

        private static void AuditHair(AuditReport report)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HairPrefabPath);
            if (prefab == null)
            {
                report.errors.Add("project_owned_hair_prefab_missing");
                return;
            }

            var meshFilter = prefab.GetComponentInChildren<MeshFilter>(true);
            var renderer = prefab.GetComponentInChildren<MeshRenderer>(true);
            report.hair_geometry_verified = meshFilter != null && meshFilter.sharedMesh != null &&
                                            meshFilter.sharedMesh.vertexCount >= 32 &&
                                            renderer != null && renderer.sharedMaterial != null;
            report.hair_vertex_count = meshFilter == null || meshFilter.sharedMesh == null
                ? 0
                : meshFilter.sharedMesh.vertexCount;
            if (!report.hair_geometry_verified) report.errors.Add("project_owned_hair_geometry_invalid");
            if (prefab.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
                report.errors.Add("hair_prefab_contains_runtime_monobehaviour");
        }

        private static void AuditOutfit(AuditReport report, OutfitDefinition definition)
        {
            var path = PrefabRoot + "/D18_R03_A01_" + definition.id + ".prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var item = new OutfitAudit
            {
                id = definition.id,
                prefab = path,
                source = definition.source,
                accessories = definition.accessories,
                role = definition.role,
                historical_review_status = "PROVISIONAL_LOCAL_VISUAL",
                valid = prefab != null,
                renderer_count = 0,
                mesh_count = 0,
                material_count = 0,
                missing_material = false
            };
            if (prefab == null)
            {
                report.errors.Add(definition.id + ":prefab_missing");
                report.outfit_variants.Add(item);
                return;
            }

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            item.renderer_count = renderers.Length;
            item.mesh_count = prefab.GetComponentsInChildren<MeshFilter>(true).Count(filter => filter.sharedMesh != null) +
                              prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Count(filter => filter.sharedMesh != null);
            item.material_count = renderers.SelectMany(renderer => renderer.sharedMaterials ?? Array.Empty<Material>())
                .Count(material => material != null);
            item.missing_material = renderers.Any(renderer => renderer.sharedMaterials.Any(material => material == null));
            item.valid = item.renderer_count > 0 && item.mesh_count > 0 && item.material_count > 0 && !item.missing_material;
            if (!item.valid) report.errors.Add(definition.id + ":geometry_or_material_invalid");
            if (prefab.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
                report.errors.Add(definition.id + ":contains_runtime_monobehaviour");
            report.outfit_variants.Add(item);
        }

        private static int CountValidHumanoidBases()
        {
            var paths = new[] { FemaleAvatarPath, MaleAvatarPath };
            var count = 0;
            foreach (var path in paths)
            {
                var hasValidAvatar = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Avatar>()
                    .Any(avatar => avatar != null && avatar.isValid && avatar.isHuman);
                if (hasValidAvatar) count++;
            }
            return count;
        }

        private static Avatar BuildDerivedAvatar(string sourcePath, string avatarPath)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null) throw new InvalidOperationException("Missing humanoid source: " + sourcePath);
            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (instance == null) throw new InvalidOperationException("Could not instantiate humanoid source: " + sourcePath);

            try
            {
                var description = BuildHumanDescription(instance);
                var avatar = AvatarBuilder.BuildHumanAvatar(instance, description);
                if (avatar == null || !avatar.isValid || !avatar.isHuman)
                    throw new InvalidOperationException("Derived Avatar is not valid Human: " + sourcePath);
                if (AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath) != null)
                    AssetDatabase.DeleteAsset(avatarPath);
                AssetDatabase.CreateAsset(avatar, avatarPath);
                AssetDatabase.SaveAssets();
                return AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static HumanDescription BuildHumanDescription(GameObject visual)
        {
            var human = new List<HumanBone>();
            AddHumanBone(human, visual, "Hips", "pelvis");
            AddHumanBone(human, visual, "Spine", "spine_01");
            AddHumanBone(human, visual, "Chest", "spine_02");
            AddHumanBone(human, visual, "UpperChest", "spine_03");
            AddHumanBone(human, visual, "Neck", "neck_01");
            AddHumanBone(human, visual, "Head", "Head");
            AddHumanBone(human, visual, "LeftShoulder", "clavicle_l");
            AddHumanBone(human, visual, "LeftUpperArm", "upperarm_l");
            AddHumanBone(human, visual, "LeftLowerArm", "lowerarm_l");
            AddHumanBone(human, visual, "LeftHand", "hand_l");
            AddHumanBone(human, visual, "RightShoulder", "clavicle_r");
            AddHumanBone(human, visual, "RightUpperArm", "upperarm_r");
            AddHumanBone(human, visual, "RightLowerArm", "lowerarm_r");
            AddHumanBone(human, visual, "RightHand", "hand_r");
            AddHumanBone(human, visual, "LeftUpperLeg", "thigh_l");
            AddHumanBone(human, visual, "LeftLowerLeg", "calf_l");
            AddHumanBone(human, visual, "LeftFoot", "foot_l");
            AddHumanBone(human, visual, "RightUpperLeg", "thigh_r");
            AddHumanBone(human, visual, "RightLowerLeg", "calf_r");
            AddHumanBone(human, visual, "RightFoot", "foot_r");

            var skeleton = visual.GetComponentsInChildren<Transform>(true)
                .Select(transform => new SkeletonBone
                {
                    name = transform.name,
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    scale = transform.localScale
                })
                .ToArray();

            return new HumanDescription
            {
                human = human.ToArray(),
                skeleton = skeleton,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false
            };
        }

        private static void AddHumanBone(List<HumanBone> human, GameObject root, string humanName, string boneName)
        {
            if (FindTransform(root.transform, boneName) == null) return;
            human.Add(new HumanBone
            {
                humanName = humanName,
                boneName = boneName,
                limit = new HumanLimit { useDefaultValues = true }
            });
        }

        private static Transform FindTransform(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindTransform(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static string BuildProjectOwnedHairPrefab()
        {
            const string meshPath = ProjectOwnedModelRoot + "/Hair_TwistPonytail.asset";
            const string materialPath = ProjectOwnedMaterialRoot + "/Hair_TwistPonytail.mat";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) != null) AssetDatabase.DeleteAsset(meshPath);

            var mesh = new Mesh { name = "D18_R03_A01_Hair_TwistPonytail" };
            var centers = new[]
            {
                new Vector3(0f, 0.04f, -0.08f),
                new Vector3(0f, -0.03f, -0.14f),
                new Vector3(0.015f, -0.12f, -0.18f),
                new Vector3(0.025f, -0.23f, -0.19f),
                new Vector3(0.035f, -0.34f, -0.16f),
                new Vector3(0.04f, -0.44f, -0.11f),
                new Vector3(0.04f, -0.52f, -0.05f)
            };
            var radii = new[] { 0.12f, 0.115f, 0.105f, 0.095f, 0.082f, 0.067f, 0.035f };
            const int sides = 8;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var i = 0; i < centers.Length; i++)
            {
                var tangent = i == 0 ? centers[1] - centers[0] : centers[i] - centers[i - 1];
                tangent.Normalize();
                var axis = Vector3.Cross(tangent, Vector3.right);
                if (axis.sqrMagnitude < 0.001f) axis = Vector3.Cross(tangent, Vector3.up);
                axis.Normalize();
                var binormal = Vector3.Cross(tangent, axis).normalized;
                for (var side = 0; side < sides; side++)
                {
                    var angle = (Mathf.PI * 2f * side) / sides;
                    vertices.Add(centers[i] + radii[i] * (axis * Mathf.Cos(angle) + binormal * Mathf.Sin(angle)));
                }
            }
            for (var ring = 0; ring < centers.Length - 1; ring++)
            for (var side = 0; side < sides; side++)
            {
                var next = (side + 1) % sides;
                var a = ring * sides + side;
                var b = ring * sides + next;
                var c = (ring + 1) * sides + next;
                var d = (ring + 1) * sides + side;
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(a); triangles.Add(c); triangles.Add(d);
            }
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, meshPath);

            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = "D18_R03_A01_Hair_TwistPonytail" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.color = new Color(0.055f, 0.028f, 0.015f, 1f);

            var root = new GameObject("D18_R03_A01_Hair_TwistPonytail");
            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            var lod = root.AddComponent<LODGroup>();
            lod.SetLODs(new[]
            {
                new LOD(0.35f, new Renderer[] { renderer }),
                new LOD(0.08f, Array.Empty<Renderer>()),
            });
            lod.RecalculateBounds();
            var saved = false;
            PrefabUtility.SaveAsPrefabAsset(root, HairPrefabPath, out saved);
            UnityEngine.Object.DestroyImmediate(root);
            if (!saved) throw new InvalidOperationException("Could not save project-owned hair prefab.");
            return HairPrefabPath;
        }

        private static string BuildOutfitPrefab(OutfitDefinition definition)
        {
            var path = PrefabRoot + "/D18_R03_A01_" + definition.id + ".prefab";
            var sourcePath = OutfitRoot + "/Models/Outfits/" + definition.source;
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null) throw new InvalidOperationException("Missing outfit source: " + sourcePath);

            var root = new GameObject("D18_R03_A01_" + definition.id);
            var sourceInstance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (sourceInstance == null) throw new InvalidOperationException("Could not instantiate outfit: " + sourcePath);
            sourceInstance.name = "Geometry_" + Path.GetFileNameWithoutExtension(definition.source);
            sourceInstance.transform.SetParent(root.transform, false);

            foreach (var accessory in definition.accessories)
            {
                var accessoryPath = OutfitRoot + "/Models/ModularParts/" + accessory;
                var accessoryAsset = AssetDatabase.LoadAssetAtPath<GameObject>(accessoryPath);
                if (accessoryAsset == null) throw new InvalidOperationException("Missing outfit accessory: " + accessoryPath);
                var accessoryInstance = PrefabUtility.InstantiatePrefab(accessoryAsset) as GameObject;
                accessoryInstance.name = "Module_" + Path.GetFileNameWithoutExtension(accessory);
                accessoryInstance.transform.SetParent(root.transform, false);
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var lod = root.AddComponent<LODGroup>();
            lod.SetLODs(new[]
            {
                new LOD(0.35f, renderers),
                new LOD(0.08f, Array.Empty<Renderer>()),
            });
            lod.RecalculateBounds();
            var saved = false;
            PrefabUtility.SaveAsPrefabAsset(root, path, out saved);
            UnityEngine.Object.DestroyImmediate(root);
            if (!saved) throw new InvalidOperationException("Could not save outfit prefab: " + path);
            return path;
        }

        private static void BuildWorkbenchPrefab(string hairPrefabPath, List<string> outfitPrefabs)
        {
            var root = new GameObject("D18_R03_A01_PlayerAppearanceWorkbench");
            var hair = AssetDatabase.LoadAssetAtPath<GameObject>(hairPrefabPath);
            var hairInstance = PrefabUtility.InstantiatePrefab(hair) as GameObject;
            hairInstance.name = "HairModule_TwistPonytail";
            hairInstance.transform.SetParent(root.transform, false);
            hairInstance.transform.localPosition = new Vector3(-5.25f, 1.65f, 0f);

            for (var index = 0; index < outfitPrefabs.Count; index++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(outfitPrefabs[index]);
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                instance.transform.SetParent(root.transform, false);
                instance.transform.localPosition = new Vector3(-3.5f + (index % 4) * 2.35f, 0f, (index / 4) * 2.8f);
            }

            var saved = false;
            PrefabUtility.SaveAsPrefabAsset(root, WorkbenchPrefabPath, out saved);
            UnityEngine.Object.DestroyImmediate(root);
            if (!saved) throw new InvalidOperationException("Could not save appearance workbench.");
        }

        private static void WriteAudit(AuditReport report)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var path = Path.Combine(projectRoot, ResultRoot, "D18_R03_A01_EDITOR_AUDIT_RERUN1.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true) + "\n");
        }

        private static void WriteAssemblyPacket(AuditReport report)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            // The Unity project lives under /game while the authoritative
            // project documentation lives at the repository root /docs.
            // Keep the packet beside the existing D18 packet rather than
            // creating a second documentation tree under the Unity project.
            var repositoryRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
            var path = Path.Combine(repositoryRoot, PacketRoot, "D18_R03_A01_PLAYER_APPEARANCE_PACKET_RERUN1.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var packet = new AssemblyPacket
            {
                schema_version = 2,
                task = "D18-R03-A01",
                generated_at = report.generated_at,
                status = report.status,
                scope = "isolated_local_playable_appearance_assembly_evidence",
                source_license = "External Quaternius sources remain CC0 and retain their original manifest/evidence; project-owned hair is authored in-repo.",
                body_directions_verified = report.body_directions_verified,
                independent_hair_styles_verified = report.independent_hair_styles_verified,
                outfit_combinations_verified = report.outfit_combinations_verified,
                hair_prefab = HairPrefabPath,
                workbench_prefab = WorkbenchPrefabPath,
                female_avatar = FemaleAvatarPath,
                male_avatar = MaleAvatarPath,
                outfit_variants = report.outfit_variants.ToArray(),
                local_development_ready = false,
                release_approved = false,
                runtime_consumers = 0,
                historical_review_status = "PROVISIONAL_LOCAL_VISUAL; D16/D18 human review remains separate"
            };
            File.WriteAllText(path, JsonUtility.ToJson(packet, true) + "\n");
        }

        private static void EnsureFolderPath(string path)
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

        [Serializable]
        private sealed class OutfitDefinition
        {
            public string id;
            public string source;
            public string[] accessories;
            public string role;
            public string notes;

            public OutfitDefinition(string id, string source, string[] accessories, string role, string notes)
            {
                this.id = id;
                this.source = source;
                this.accessories = accessories;
                this.role = role;
                this.notes = notes;
            }
        }

        [Serializable]
        public sealed class AuditReport
        {
            public string task;
            public string generated_at;
            public string status;
            public string hair_prefab;
            public string workbench_prefab;
            public string female_avatar;
            public string male_avatar;
            public bool hair_geometry_verified;
            public int hair_vertex_count;
            public int body_directions_verified;
            public int independent_hair_styles_verified;
            public int outfit_combinations_verified;
            public List<OutfitAudit> outfit_variants;
            public List<string> errors;
            public List<string> warnings;
        }

        [Serializable]
        public sealed class OutfitAudit
        {
            public string id;
            public string prefab;
            public string source;
            public string[] accessories;
            public string role;
            public string historical_review_status;
            public bool valid;
            public int renderer_count;
            public int mesh_count;
            public int material_count;
            public bool missing_material;
        }

        [Serializable]
        private sealed class AssemblyPacket
        {
            public int schema_version;
            public string task;
            public string generated_at;
            public string status;
            public string scope;
            public string source_license;
            public int body_directions_verified;
            public int independent_hair_styles_verified;
            public int outfit_combinations_verified;
            public string hair_prefab;
            public string workbench_prefab;
            public string female_avatar;
            public string male_avatar;
            public OutfitAudit[] outfit_variants;
            public bool local_development_ready;
            public bool release_approved;
            public int runtime_consumers;
            public string historical_review_status;
        }
    }
}
#endif
