#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class D18A04R03PIsolatedImportRunner
{
    [Serializable]
    private sealed class Report
    {
        public int schema_version = 1;
        public string task = "D18-A04-R03-P";
        public string captured_at;
        public string unity_version;
        public string status;
        public string fixture_scope = "isolated_candidate_import_only";
        public bool source_candidate_files_changed = false;
        public int fbx_total;
        public int mesh_success_count;
        public int renderer_success_count;
        public int valid_humanoid_avatar_count;
        public Candidate[] candidates;
        public string[] errors;
    }

    [Serializable]
    private sealed class Candidate
    {
        public string file_name;
        public string asset_path;
        public bool file_exists;
        public string animation_type;
        public string avatar_setup;
        public bool avatar_present;
        public bool avatar_valid;
        public bool avatar_is_human;
        public int transform_count;
        public int mesh_count;
        public int material_count;
        public int texture_count;
        public int animation_clip_count;
        public float global_scale;
        public float renderer_height;
        public float renderer_width;
        public float renderer_depth;
        public string[] notes;
    }

    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var errors = new List<string>();
        var paths = AssetDatabase.FindAssets("t:Model", new[] { "Assets/Models" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var candidates = paths.Select(path => ImportAndAudit(path, errors)).ToArray();
        var validHumanoid = candidates.Count(item => item.avatar_present && item.avatar_valid && item.avatar_is_human);
        var report = new Report
        {
            captured_at = DateTime.UtcNow.ToString("o"),
            unity_version = Application.unityVersion,
            status = validHumanoid > 0 ? "PASS" : "PARTIAL",
            fbx_total = candidates.Length,
            mesh_success_count = candidates.Count(item => item.mesh_count > 0),
            renderer_success_count = candidates.Count(item => item.renderer_height > 0),
            valid_humanoid_avatar_count = validHumanoid,
            candidates = candidates,
            errors = errors.ToArray(),
        };
        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        var output = Path.GetFullPath(Path.Combine(projectRoot, "..", "D18_A04_R03_P_ISOLATED_UNITY_IMPORT.json"));
        File.WriteAllText(output, JsonUtility.ToJson(report, true) + "\n");
        Debug.Log("[D18-A04-R03-P] status=" + report.status + " fbx=" + report.fbx_total + " valid_humanoid=" + validHumanoid + " output=" + output);
    }

    private static Candidate ImportAndAudit(string path, List<string> errors)
    {
        var record = new Candidate
        {
            file_name = Path.GetFileName(path),
            asset_path = path,
            file_exists = File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, path.Replace('/', Path.DirectorySeparatorChar))),
            notes = Array.Empty<string>(),
        };
        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (!record.file_exists || importer == null)
        {
            errors.Add(path + ":missing_file_or_importer");
            return record;
        }

        if (record.file_name.Equals("Superhero_Female_FullBody.fbx", StringComparison.OrdinalIgnoreCase))
        {
            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            importer = AssetImporter.GetAtPath(path) as ModelImporter;
        }

        record.animation_type = importer.animationType.ToString();
        record.avatar_setup = importer.avatarSetup.ToString();
        record.global_scale = importer.globalScale;
        var main = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var assets = AssetDatabase.LoadAllAssetsAtPath(path) ?? Array.Empty<UnityEngine.Object>();
        record.mesh_count = assets.OfType<Mesh>().Count();
        record.material_count = assets.OfType<Material>().Count();
        record.texture_count = assets.OfType<Texture>().Count();
        record.animation_clip_count = assets.OfType<AnimationClip>().Count(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal));
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
        var renderers = main.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            errors.Add(path + ":renderer_missing");
            return record;
        }
        var bounds = renderers[0].bounds;
        for (var index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].bounds);
        record.renderer_height = bounds.size.y;
        record.renderer_width = bounds.size.x;
        record.renderer_depth = bounds.size.z;
        var notes = new List<string>();
        if (!record.avatar_present) notes.Add("No Avatar on this modular mesh");
        if (record.avatar_present && !record.avatar_valid) notes.Add("Avatar invalid");
        if (record.avatar_present && !record.avatar_is_human) notes.Add("Avatar is not human");
        notes.Add("Measured in isolated fixture; no formal Prefab or scene consumer");
        record.notes = notes.ToArray();
        return record;
    }
}
#endif
