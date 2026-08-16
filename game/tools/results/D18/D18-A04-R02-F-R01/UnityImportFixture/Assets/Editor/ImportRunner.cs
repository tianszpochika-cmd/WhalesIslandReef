#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class D18A04R02FR01IsolatedImportRunner
{
    private static readonly string[] Paths =
    {
        "Assets/Models/Female_Alternative.fbx",
        "Assets/Models/Female_Casual.fbx",
        "Assets/Models/Female_Dress.fbx",
        "Assets/Models/Female_TankTop.fbx",
    };

    [Serializable]
    private sealed class Report
    {
        public int schema_version = 1;
        public string task = "D18-A04-R02-F-R01";
        public string captured_at;
        public string unity_version;
        public string status;
        public bool any_humanoid_valid_avatar;
        public bool project_isolated = true;
        public bool source_candidate_files_changed = false;
        public Candidate[] candidates;
        public string[] errors;
    }

    [Serializable]
    private sealed class Candidate
    {
        public string source_file;
        public string isolated_asset_path;
        public string animation_type;
        public string avatar_setup;
        public bool file_exists;
        public bool avatar_present;
        public bool avatar_valid;
        public bool avatar_is_human;
        public int human_bone_count;
        public int skeleton_bone_count;
        public int transform_count;
        public int mesh_count;
        public int animation_clip_count;
        public int material_count;
        public int texture_count;
        public float global_scale;
        public float renderer_height;
        public float renderer_width;
        public float renderer_depth;
        public Vector3 renderer_bounds_min;
        public Vector3 renderer_bounds_max;
        public bool has_head;
        public bool has_hips;
        public bool has_left_foot;
        public bool has_right_foot;
        public bool has_spine_or_chest;
        public string[] notes;
    }

    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var errors = new List<string>();
        var records = Paths.Select(path => ImportAndAudit(path, errors)).ToArray();
        var validCount = records.Count(record => record.avatar_present && record.avatar_valid && record.avatar_is_human);
        var report = new Report
        {
            captured_at = DateTime.UtcNow.ToString("o"),
            unity_version = Application.unityVersion,
            status = validCount > 0 ? "PASS" : "PARTIAL",
            any_humanoid_valid_avatar = validCount > 0,
            candidates = records,
            errors = errors.ToArray(),
        };
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var output = Path.Combine(projectRoot, "..", "D18_A04_R02_F_R01_ISOLATED_UNITY_IMPORT.json");
        File.WriteAllText(output, JsonUtility.ToJson(report, true) + "\n");
        Debug.Log("[D18-A04-R02-F-R01] status=" + report.status + " valid_avatar=" + validCount + " output=" + output);
    }

    private static Candidate ImportAndAudit(string path, List<string> errors)
    {
        var record = new Candidate { isolated_asset_path = path, source_file = Path.GetFileName(path), notes = Array.Empty<string>() };
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        record.file_exists = File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, path.Replace('/', Path.DirectorySeparatorChar)));
        if (!record.file_exists || importer == null)
        {
            errors.Add(path + ":missing_file_or_importer");
            return record;
        }

        importer.importAnimation = true;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.SaveAndReimport();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        importer = AssetImporter.GetAtPath(path) as ModelImporter;
        record.animation_type = importer.animationType.ToString();
        record.avatar_setup = importer.avatarSetup.ToString();
        record.global_scale = importer.globalScale;

        var main = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var assets = AssetDatabase.LoadAllAssetsAtPath(path) ?? Array.Empty<UnityEngine.Object>();
        record.mesh_count = assets.OfType<Mesh>().Count();
        record.animation_clip_count = assets.OfType<AnimationClip>().Count(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));
        record.material_count = assets.OfType<Material>().Count();
        record.texture_count = assets.OfType<Texture>().Count();
        if (main == null)
        {
            errors.Add(path + ":main_asset_missing");
            return record;
        }

        record.transform_count = main.GetComponentsInChildren<Transform>(true).Length;
        var animator = main.GetComponentInChildren<Animator>(true);
        record.avatar_present = animator != null && animator.avatar != null;
        record.avatar_valid = record.avatar_present && animator.avatar.isValid;
        record.avatar_is_human = record.avatar_present && animator.avatar.isHuman;
        var human = importer.humanDescription.human ?? Array.Empty<HumanBone>();
        var skeleton = importer.humanDescription.skeleton ?? Array.Empty<SkeletonBone>();
        var humanNames = new HashSet<string>(human.Select(item => item.humanName ?? string.Empty), StringComparer.OrdinalIgnoreCase);
        record.human_bone_count = human.Length;
        record.skeleton_bone_count = skeleton.Length;
        record.has_head = humanNames.Contains("Head");
        record.has_hips = humanNames.Contains("Hips");
        record.has_left_foot = humanNames.Contains("LeftFoot");
        record.has_right_foot = humanNames.Contains("RightFoot");
        record.has_spine_or_chest = humanNames.Contains("Spine") || humanNames.Contains("Chest");

        var renderers = main.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            errors.Add(path + ":renderer_missing");
            return record;
        }
        var bounds = renderers[0].localBounds;
        for (var index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].localBounds);
        record.renderer_bounds_min = bounds.min;
        record.renderer_bounds_max = bounds.max;
        record.renderer_height = bounds.size.y;
        record.renderer_width = bounds.size.x;
        record.renderer_depth = bounds.size.z;
        var notes = new List<string>();
        if (!record.avatar_present) notes.Add("Avatar missing");
        if (record.avatar_present && !record.avatar_valid) notes.Add("Avatar invalid");
        if (!record.avatar_is_human) notes.Add("Avatar is not human");
        notes.Add("Importer settings and generated Avatar were measured in the isolated fixture, not in the formal game project");
        record.notes = notes.ToArray();
        return record;
    }
}
#endif
