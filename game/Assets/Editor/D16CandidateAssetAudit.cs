using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Read-only Unity import audit for the D16-A05 candidate packet.
/// It never changes importer settings, the Content Registry, or asset_state.
/// </summary>
public static class D16CandidateAssetAudit
{
    private const string PacketRelativePath = "tools/results/d16_a05_candidate_packet_01_20260813.json";
    private const string OutputRelativePath = "tools/results/d16_a05_candidate_import_current_20260813.json";
    private const string CandidateAssetPrefix = "Assets/Art/Candidates/Kenney/";

    [Serializable]
    private sealed class CandidatePacket
    {
        public string status;
        public CandidateFile[] candidate_files;
    }

    [Serializable]
    private sealed class CandidateFile
    {
        public string target_id;
        public string path;
        public string role;
    }

    [Serializable]
    private sealed class CandidateImportRecord
    {
        public string target_id;
        public string path;
        public string role;
        public bool source_exists;
        public bool candidate_scope;
        public bool importer_present;
        public string main_asset_type;
        public int mesh_count;
        public int animation_clip_count;
        public bool has_primary_mesh;
        public float max_dimension;
        public Vector3 bounds_min;
        public Vector3 bounds_max;
        public float global_scale;
        public bool import_animation;
        public string animation_type;
        public bool import_materials;
        public string mesh_compression;
        public bool is_readable;
        public string[] errors;
    }

    [Serializable]
    private sealed class CandidateImportReport
    {
        public int schema_version = 1;
        public string task = "D16-A05-CANDIDATE-UNITY-IMPORT";
        public string captured_at = "2026-08-13";
        public string packet_status;
        public string status;
        public bool read_only = true;
        public bool registry_changed = false;
        public bool asset_state_changed = false;
        public CandidateImportRecord[] records;
        public string[] errors;
    }

    public static void Run()
    {
        var report = BuildReport();
        var outputPath = Path.GetFullPath(Path.Combine(ProjectRoot(), "..", OutputRelativePath));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        File.WriteAllText(outputPath, JsonUtility.ToJson(report, true));
        Debug.Log("[D16-A05 Candidate Import] status=" + report.status
            + ", records=" + (report.records == null ? 0 : report.records.Length)
            + ", output=" + outputPath);
        foreach (var error in report.errors ?? Array.Empty<string>())
            Debug.LogError("[D16-A05 Candidate Import] " + error);
    }

    private static CandidateImportReport BuildReport()
    {
        var report = new CandidateImportReport
        {
            status = "BLOCKED",
            records = Array.Empty<CandidateImportRecord>(),
            errors = new List<string>().ToArray(),
        };
        var errors = new List<string>();
        var packetPath = Path.GetFullPath(Path.Combine(ProjectRoot(), "..", PacketRelativePath));
        if (!File.Exists(packetPath))
        {
            errors.Add("candidate_packet_missing:" + packetPath);
            report.errors = errors.ToArray();
            return report;
        }

        CandidatePacket packet;
        try
        {
            packet = JsonUtility.FromJson<CandidatePacket>(File.ReadAllText(packetPath));
        }
        catch (Exception exception)
        {
            errors.Add("candidate_packet_invalid:" + exception.GetType().Name);
            report.errors = errors.ToArray();
            return report;
        }

        report.packet_status = packet == null ? string.Empty : packet.status;
        if (packet == null || packet.candidate_files == null || packet.candidate_files.Length == 0)
        {
            errors.Add("candidate_packet_has_no_files");
            report.errors = errors.ToArray();
            return report;
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var records = new List<CandidateImportRecord>();
        foreach (var candidate in packet.candidate_files)
        {
            var record = AuditCandidate(candidate, errors);
            records.Add(record);
        }

        report.records = records.ToArray();
        report.errors = errors.ToArray();
        report.status = errors.Count == 0 ? "PASS" : "BLOCKED";
        return report;
    }

    private static CandidateImportRecord AuditCandidate(CandidateFile candidate, List<string> reportErrors)
    {
        var record = new CandidateImportRecord
        {
            target_id = candidate == null ? string.Empty : candidate.target_id,
            path = candidate == null ? string.Empty : candidate.path,
            role = candidate == null ? string.Empty : candidate.role,
            errors = Array.Empty<string>(),
        };
        var errors = new List<string>();
        if (candidate == null || string.IsNullOrWhiteSpace(candidate.path))
        {
            errors.Add("candidate_path_missing");
            return FinishRecord(record, errors, reportErrors);
        }

        var normalized = candidate.path.Replace('\\', '/');
        const string gamePrefix = "game/";
        var assetPath = normalized.StartsWith(gamePrefix, StringComparison.OrdinalIgnoreCase)
            ? normalized.Substring(gamePrefix.Length)
            : normalized;
        record.candidate_scope = assetPath.StartsWith(CandidateAssetPrefix, StringComparison.Ordinal);
        if (!record.candidate_scope)
            errors.Add("candidate_path_outside_candidate_scope:" + assetPath);

        record.source_exists = File.Exists(Path.GetFullPath(Path.Combine(ProjectRoot(), assetPath)));
        if (!record.source_exists)
            errors.Add("candidate_file_missing:" + assetPath);

        var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        record.importer_present = importer != null;
        if (importer == null)
        {
            errors.Add("model_importer_missing:" + assetPath);
            return FinishRecord(record, errors, reportErrors);
        }

        var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
        record.main_asset_type = mainAsset == null ? string.Empty : mainAsset.GetType().Name;
        if (mainAsset == null)
            errors.Add("main_asset_missing:" + assetPath);

        var meshes = new List<Mesh>();
        var animationClips = new List<AnimationClip>();
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (asset is Mesh mesh) meshes.Add(mesh);
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                animationClips.Add(clip);
        }

        record.mesh_count = meshes.Count;
        record.animation_clip_count = animationClips.Count;
        record.has_primary_mesh = meshes.Count > 0;
        if (meshes.Count == 0)
        {
            errors.Add("mesh_missing:" + assetPath);
        }
        else
        {
            var initialized = false;
            var min = Vector3.zero;
            var max = Vector3.zero;
            foreach (var mesh in meshes)
            {
                var bounds = mesh.bounds;
                var boundsMin = bounds.min;
                var boundsMax = bounds.max;
                if (!initialized)
                {
                    min = boundsMin;
                    max = boundsMax;
                    initialized = true;
                }
                else
                {
                    min = Vector3.Min(min, boundsMin);
                    max = Vector3.Max(max, boundsMax);
                }
            }

            record.bounds_min = min;
            record.bounds_max = max;
            record.max_dimension = Mathf.Max(max.x - min.x, max.y - min.y, max.z - min.z);
            if (record.max_dimension <= 0f)
                errors.Add("mesh_bounds_zero:" + assetPath);
        }

        record.global_scale = importer.globalScale;
        record.import_animation = importer.importAnimation;
        record.animation_type = importer.animationType.ToString();
        record.import_materials = importer.materialImportMode != ModelImporterMaterialImportMode.None;
        record.mesh_compression = importer.meshCompression.ToString();
        record.is_readable = importer.isReadable;
        return FinishRecord(record, errors, reportErrors);
    }

    private static CandidateImportRecord FinishRecord(
        CandidateImportRecord record,
        List<string> errors,
        List<string> reportErrors)
    {
        record.errors = errors.ToArray();
        foreach (var error in errors)
            reportErrors.Add(record.path + ":" + error);
        return record;
    }

    private static string ProjectRoot()
    {
        return Directory.GetParent(Application.dataPath).FullName;
    }
}
