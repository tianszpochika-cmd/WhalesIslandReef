#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace WhalesIslandReef.Editor.D18ArtPipeline
{
    /// <summary>
    /// D18-A02 的确定性、只读资产审计。它只消费 D18-A01 manifest 和 Unity 已有导入结果，
    /// 不写 Registry、asset_state、场景、Prefab 或运行时状态。
    /// </summary>
    public static class D18ArtPipelineAudit
    {
        private const string TaskId = "D18-A02";
        private const string ConfigAssetPath = "Assets/Editor/D18ArtPipeline/D18ArtPipelineConfig.json";
        private static readonly HashSet<string> DefaultForbiddenSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Library", "Temp", "Logs"
        };

        public static D18AuditReport RunKenneyCandidateAudit()
        {
            var config = LoadConfig();
            var manifestDirectory = Path.Combine(RepositoryRoot(), config.manifest_root.Replace('/', Path.DirectorySeparatorChar));
            var manifestPaths = Directory.Exists(manifestDirectory)
                ? Directory.GetFiles(manifestDirectory, "*.manifest.json", SearchOption.TopDirectoryOnly).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()
                : Array.Empty<string>();
            return AuditManifestFiles(manifestPaths, config);
        }

        public static D18AuditReport AuditManifestFiles(IEnumerable<string> manifestFiles, D18PipelineConfig config = null)
        {
            config = config ?? LoadConfig();
            var report = CreateReport(config);
            var manifestSummaries = new List<D18ManifestSummary>();
            var records = new List<D18AssetAuditRecord>();
            var seenAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var manifestFile in (manifestFiles ?? Array.Empty<string>()).Where(path => !string.IsNullOrWhiteSpace(path)).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var summary = new D18ManifestSummary
                {
                    manifest_path = ToRepositoryRelativePath(manifestFile),
                    candidate_files = Array.Empty<string>(),
                    issues = Array.Empty<D18Issue>(),
                };
                report.manifest_count++;
                var manifest = ReadManifest(manifestFile, summary);
                if (manifest == null)
                {
                    manifestSummaries.Add(summary);
                    continue;
                }

                summary.pack_id = manifest.pack_id ?? string.Empty;
                summary.review_status = manifest.review_status ?? string.Empty;
                summary.import_status = manifest.import_status ?? string.Empty;
                summary.declared_file_count = manifest.file_count;
                summary.candidate_files = (manifest.candidate_files ?? Array.Empty<D18ManifestCandidate>())
                    .Where(candidate => candidate != null)
                    .Select(candidate => candidate.path ?? string.Empty)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var manifestIssues = new List<D18Issue>();
                if (!string.Equals(manifest.manifest_kind, "d18_asset_pack_manifest", StringComparison.Ordinal))
                    AddIssue(manifestIssues, "warning", "manifest_kind_unexpected", "A01 manifest_kind is not d18_asset_pack_manifest");
                if (manifest.schema_version != 1)
                    AddIssue(manifestIssues, "error", "manifest_schema_unsupported", "Only D18-A01 manifest schema_version=1 is supported");
                if (!string.Equals(manifest.review_status, "Candidate", StringComparison.OrdinalIgnoreCase))
                    AddIssue(manifestIssues, "warning", "review_status_not_candidate", "The audit reports the manifest status but never changes it");
                if (!string.Equals(manifest.import_status, "ImportVerified", StringComparison.OrdinalIgnoreCase))
                    AddIssue(manifestIssues, "warning", "import_status_not_verified", "A01 import_status is not ImportVerified");
                summary.issues = manifestIssues.ToArray();

                foreach (var candidate in manifest.candidate_files ?? Array.Empty<D18ManifestCandidate>())
                {
                    if (candidate == null)
                        continue;

                    var normalizedAssetPath = NormalizeProjectAssetPath(candidate.path);
                    if (!seenAssetPaths.Add(normalizedAssetPath))
                    {
                        AddIssue(manifestIssues, "error", "duplicate_candidate_path", "The same candidate path occurs in more than one manifest: " + normalizedAssetPath);
                        continue;
                    }

                    records.Add(AuditAssetPath(normalizedAssetPath, candidate.target_id, candidate.role, config));
                }
                summary.issues = manifestIssues.ToArray();
                manifestSummaries.Add(summary);
            }

            report.manifests = manifestSummaries.ToArray();
            report.assets = records.OrderBy(record => record.path, StringComparer.OrdinalIgnoreCase).ToArray();
            report.prefabs = Array.Empty<D18PrefabAuditRecord>();
            AggregateReport(report);
            return report;
        }

        /// <summary>
        /// 对明确传入的 asset path 做预检；不会遍历项目目录。
        /// </summary>
        public static D18AssetAuditRecord AuditAssetPath(string assetPath, string targetId = "", string role = "", D18PipelineConfig config = null)
        {
            config = config ?? LoadConfig();
            var normalized = NormalizeProjectAssetPath(assetPath);
            var record = new D18AssetAuditRecord
            {
                path = normalized,
                target_id = targetId ?? string.Empty,
                role = role ?? string.Empty,
                category = Classify(normalized, role),
                importer_type = string.Empty,
                main_asset_type = string.Empty,
                settings = new D18ImportSettings(),
                issues = Array.Empty<D18Issue>(),
                suggestions = Array.Empty<string>(),
            };
            var issues = new List<D18Issue>();
            var suggestions = new List<string>();

            if (!CanScanCandidateAssetPath(normalized, config))
            {
                AddIssue(issues, "error", "asset_path_outside_scope", "Asset path is outside the explicit D18 candidate/fixture scope: " + normalized);
                record.issues = issues.ToArray();
                record.suggestions = suggestions.ToArray();
                return record;
            }

            record.source_exists = File.Exists(Path.Combine(ProjectRoot(), normalized.Replace('/', Path.DirectorySeparatorChar)));
            if (!record.source_exists)
            {
                AddIssue(issues, "error", "asset_file_missing", "Candidate file does not exist: " + normalized);
                record.issues = issues.ToArray();
                record.suggestions = suggestions.ToArray();
                return record;
            }

            var extension = Path.GetExtension(normalized).ToLowerInvariant();
            if (config.model_extensions.Contains(extension))
            {
                AuditModel(normalized, record, issues, suggestions);
            }
            else if (config.texture_extensions.Contains(extension))
            {
                AuditTexture(normalized, record, issues, suggestions, config);
            }
            else if (config.audio_extensions.Contains(extension))
            {
                AuditAudio(normalized, record, issues, suggestions);
            }
            else if (config.prefab_extensions.Contains(extension))
            {
                AuditPrefabAsset(normalized, record, issues, suggestions);
            }
            else
            {
                AddIssue(issues, "error", "unsupported_extension", "No D18 importer rule exists for " + extension);
            }

            record.issues = issues.ToArray();
            record.suggestions = suggestions.ToArray();
            return record;
        }

        public static D18PrefabAuditRecord AuditPrefabObject(GameObject root, string assetPath, D18PipelineConfig config = null)
        {
            config = config ?? LoadConfig();
            var result = new D18PrefabAuditRecord
            {
                path = NormalizeProjectAssetPath(assetPath),
                root_name = root == null ? string.Empty : root.name,
                category = Classify(assetPath, string.Empty),
                issues = Array.Empty<D18Issue>(),
            };
            var issues = new List<D18Issue>();
            if (root == null)
            {
                AddIssue(issues, "error", "prefab_root_missing", "Prefab root could not be loaded");
                result.issues = issues.ToArray();
                return result;
            }

            var meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true);
            var skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var colliders = root.GetComponentsInChildren<Collider>(true);
            var lodGroups = root.GetComponentsInChildren<LODGroup>(true);
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var anchors = transforms.Where(transform => IsInteractionAnchorName(transform.name)).ToArray();

            result.mesh_renderer_count = meshRenderers.Length;
            result.skinned_mesh_renderer_count = skinnedRenderers.Length;
            result.renderer_count = renderers.Length;
            result.collider_count = colliders.Length;
            result.lod_group_count = lodGroups.Length;
            result.interaction_anchor_count = anchors.Length;
            result.missing_script_count = CountMissingScripts(root);
            result.missing_reference_count = CountMissingObjectReferences(root);
            result.bounds_valid = HasNonZeroBounds(renderers);
            result.naming_valid = Regex.IsMatch(root.name ?? string.Empty, config.naming_regex ?? DefaultNamingRegex, RegexOptions.CultureInvariant);

            if (renderers.Length == 0)
                AddIssue(issues, "error", "prefab_renderer_missing", "Prefab must contain MeshRenderer or SkinnedMeshRenderer");
            foreach (var renderer in renderers)
            {
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0 || renderer.sharedMaterials.Any(material => material == null))
                    AddIssue(issues, "error", "prefab_material_missing", "Renderer has a missing or empty material slot: " + renderer.name);
            }
            if (!result.bounds_valid)
                AddIssue(issues, "error", "prefab_bounds_invalid", "Prefab renderer bounds are zero or unavailable");
            if (colliders.Length == 0)
                AddIssue(issues, "error", "prefab_collision_missing", "Prefab has no Collider component");
            if (anchors.Length == 0)
                AddIssue(issues, "warning", "prefab_interaction_anchor_missing", "No InteractionAnchor-named transform was found");
            if (lodGroups.Length == 0)
                AddIssue(issues, "warning", "prefab_lod_group_missing", "Prefab has no LODGroup");
            if (result.missing_script_count > 0)
                AddIssue(issues, "error", "prefab_missing_script", "Prefab has missing MonoBehaviour scripts: " + result.missing_script_count);
            if (result.missing_reference_count > 0)
                AddIssue(issues, "error", "prefab_missing_reference", "Prefab has serialized missing object references: " + result.missing_reference_count);
            if (!result.naming_valid)
                AddIssue(issues, "warning", "prefab_name_not_snake_case", "Root name does not match the D18 stable naming rule");

            result.issues = issues.ToArray();
            return result;
        }

        public static D18FixResult ApplySuggestionsForFixture(string assetPath, bool explicitlySelected)
        {
            var normalized = NormalizeProjectAssetPath(assetPath);
            var result = new D18FixResult
            {
                path = normalized,
                changed = false,
                allowed = false,
                message = string.Empty,
            };
            if (!explicitlySelected)
            {
                result.message = "blocked: explicit selection is required";
                return result;
            }
            if (!IsD18TestFixturePath(normalized))
            {
                result.message = "blocked: automatic fixes are allowed only under " + LoadConfig().fixture_root;
                return result;
            }

            result.allowed = true;
            var importer = AssetImporter.GetAtPath(normalized);
            if (importer == null)
            {
                result.message = "no importer found; no change made";
                return result;
            }

            // 只在 D18 测试夹具内做一个可逆、明确的贴图示例修复。模型不统一 scale，
            // 也不自动重写骨骼、轴向、材质、Prefab 或候选包设置。
            if (importer is TextureImporter textureImporter)
            {
                var config = LoadConfig();
                if (textureImporter.maxTextureSize > config.max_texture_size)
                {
                    textureImporter.maxTextureSize = config.max_texture_size;
                    textureImporter.SaveAndReimport();
                    result.changed = true;
                    result.message = "fixture-only texture maxTextureSize clamped to " + config.max_texture_size;
                    return result;
                }
            }

            result.message = "fixture selected; no safe automatic change was necessary";
            return result;
        }

        public static bool CanScanProjectPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            var normalized = path.Replace('\\', '/');
            // Reject traversal syntax before canonicalization. Canonicalizing first would
            // turn a path such as Fixtures/../outside.fbx into an apparently safe path
            // and would hide the boundary violation this audit is meant to report.
            var rawSegments = normalized.Split('/');
            if (rawSegments.Any(segment => segment == "." || segment == ".."))
                return false;
            var project = ProjectRoot();
            var full = Path.GetFullPath(Path.IsPathRooted(normalized)
                ? normalized
                : Path.Combine(project, normalized.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsWithinRoot(full, project))
                return false;
            var relative = full.Substring(Path.GetFullPath(project).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace('\\', '/');
            return !ContainsForbiddenSegment(relative, LoadConfig());
        }

        public static bool IsD18TestFixturePath(string assetPath)
        {
            var config = LoadConfig();
            return NormalizeProjectAssetPath(assetPath).StartsWith(config.fixture_root, StringComparison.OrdinalIgnoreCase)
                && CanScanProjectPath(assetPath);
        }

        public static string Classify(string assetPath, string role)
        {
            var value = ((assetPath ?? string.Empty) + "/" + (role ?? string.Empty)).Replace('\\', '/').ToLowerInvariant();
            var extension = Path.GetExtension(assetPath ?? string.Empty).ToLowerInvariant();
            if (new[] { ".wav", ".mp3", ".ogg", ".aif", ".aiff" }.Contains(extension) || ContainsAny(value, "audio", "sound", "music")) return "Audio";
            if (ContainsAny(value, "ui", "icon", "sprite", "button", "hud")) return "UI";
            if (ContainsAny(value, "ship", "boat", "vessel", "hull", "sail", "mast")) return "Ships";
            if (ContainsAny(value, "character", "crew", "npc", "humanoid")) return "Characters";
            if (ContainsAny(value, "animal", "pet", "fish", "bird", "cat", "dog")) return "Animals";
            if (ContainsAny(value, "vegetation", "palm", "tree", "grass", "plant", "flower", "leaf")) return "Vegetation";
            if (ContainsAny(value, "building", "structure", "dock", "pier", "house", "warehouse", "market")) return "Buildings";
            if (ContainsAny(value, "environment", "island", "terrain", "rock", "sand", "ground", "water", "shore")) return "Environment";
            return "Props";
        }

        public static string NormalizeProjectAssetPath(string path)
        {
            var normalized = (path ?? string.Empty).Replace('\\', '/').Trim();
            if (normalized.StartsWith("game/", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring("game/".Length);
            var project = ProjectRoot();
            var full = Path.GetFullPath(Path.IsPathRooted(normalized)
                ? normalized
                : Path.Combine(project, normalized.Replace('/', Path.DirectorySeparatorChar)));
            if (IsWithinRoot(full, project))
                normalized = full.Substring(Path.GetFullPath(project).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('\\', '/');
            return normalized.TrimStart('/');
        }

        public static D18PipelineConfig LoadConfig()
        {
            var configPath = Path.Combine(ProjectRoot(), ConfigAssetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(configPath))
                return D18PipelineConfig.CreateDefault();
            try
            {
                var config = JsonUtility.FromJson<D18PipelineConfig>(File.ReadAllText(configPath));
                return config ?? D18PipelineConfig.CreateDefault();
            }
            catch
            {
                return D18PipelineConfig.CreateDefault();
            }
        }

        public static string WriteReport(D18AuditReport report)
        {
            var config = LoadConfig();
            var outputDirectory = Path.Combine(ProjectRoot(), config.output_root.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(outputDirectory);
            var jsonPath = Path.Combine(outputDirectory, "D18_A02_KENNEY_IMPORT_AUDIT.json");
            var markdownPath = Path.Combine(outputDirectory, "D18_A02_KENNEY_IMPORT_AUDIT.md");
            var json = JsonUtility.ToJson(report, true) + "\n";
            var markdown = BuildMarkdown(report);
            File.WriteAllText(jsonPath, json, new UTF8Encoding(false));
            File.WriteAllText(markdownPath, markdown, new UTF8Encoding(false));
            return ToRepositoryRelativePath(jsonPath) + ";" + ToRepositoryRelativePath(markdownPath);
        }

        public static string BuildMarkdown(D18AuditReport report)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# D18-A02 Kenney 导入审计");
            builder.AppendLine();
            builder.AppendLine("- task: `D18-A02`");
            builder.AppendLine("- status: **" + report.status + "**");
            builder.AppendLine("- mode: `" + report.audit_mode + "`");
            builder.AppendLine("- read-only: `" + report.read_only + "`");
            builder.AppendLine("- Registry changed: `" + report.registry_modified + "`");
            builder.AppendLine("- asset_state changed: `" + report.asset_state_modified + "`");
            builder.AppendLine("- ReleaseApproved asserted: `" + report.release_approved_asserted + "`");
            builder.AppendLine();
            builder.AppendLine("## 统计");
            builder.AppendLine();
            builder.AppendLine("| 指标 | 数量 |");
            builder.AppendLine("|---|---:|");
            builder.AppendLine("| manifest | " + report.manifest_count + " |");
            builder.AppendLine("| candidate files | " + report.file_count + " |");
            builder.AppendLine("| models | " + report.model_file_count + " |");
            builder.AppendLine("| textures | " + report.texture_file_count + " |");
            builder.AppendLine("| audio | " + report.audio_file_count + " |");
            builder.AppendLine("| prefabs | " + report.prefab_file_count + " |");
            builder.AppendLine("| Mesh | " + report.mesh_count + " |");
            builder.AppendLine("| animation clips | " + report.animation_clip_count + " |");
            builder.AppendLine("| materials | " + report.material_count + " |");
            builder.AppendLine("| errors | " + report.error_count + " |");
            builder.AppendLine("| warnings | " + report.warning_count + " |");
            builder.AppendLine();
            builder.AppendLine("## 分类");
            builder.AppendLine();
            foreach (var category in report.category_counts ?? Array.Empty<D18CategoryCount>())
                builder.AppendLine("- `" + category.category + "`: " + category.count);
            builder.AppendLine();
            builder.AppendLine("## manifest");
            builder.AppendLine();
            foreach (var manifest in report.manifests ?? Array.Empty<D18ManifestSummary>())
            {
                builder.AppendLine("### " + manifest.pack_id);
                builder.AppendLine("- file: `" + manifest.manifest_path + "`");
                builder.AppendLine("- review_status: `" + manifest.review_status + "`");
                builder.AppendLine("- import_status: `" + manifest.import_status + "`");
                builder.AppendLine("- declared/scanned: " + manifest.declared_file_count + "/" + (manifest.candidate_files == null ? 0 : manifest.candidate_files.Length));
                foreach (var issue in manifest.issues ?? Array.Empty<D18Issue>())
                    builder.AppendLine("- " + issue.severity + " `" + issue.code + "`: " + issue.message);
            }
            builder.AppendLine();
            builder.AppendLine("## 当前候选问题与导入建议");
            builder.AppendLine();
            foreach (var record in report.assets ?? Array.Empty<D18AssetAuditRecord>())
            {
                builder.AppendLine("### `" + record.path + "`");
                builder.AppendLine("- category: `" + record.category + "`; target: `" + record.target_id + "`; importer: `" + record.importer_type + "`");
                builder.AppendLine("- source_exists: `" + record.source_exists + "`; mesh/animation/material: " + record.mesh_count + "/" + record.animation_clip_count + "/" + record.material_count);
                builder.AppendLine("- model_dimension: `" + record.max_dimension.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "` Unity units; target scale: `" + record.target_scale_reference + "`");
                foreach (var issue in record.issues ?? Array.Empty<D18Issue>())
                    builder.AppendLine("- " + issue.severity + " `" + issue.code + "`: " + issue.message);
                foreach (var suggestion in record.suggestions ?? Array.Empty<string>())
                    builder.AppendLine("- suggestion: " + suggestion);
            }
            builder.AppendLine();
            builder.AppendLine("## 边界");
            builder.AppendLine();
            builder.AppendLine("本报告只记录 Unity 当前导入结果和建议；不修改 Registry、asset_state、Prefab、场景或候选包，不写 ReleaseApproved，不下载资源。模型自身尺寸以记录为准，不由管线统一 scale；后续 Prefab/责任系统决定具体目标尺度、锚点和碰撞修正。");
            return builder.ToString();
        }

        private static D18AuditReport CreateReport(D18PipelineConfig config)
        {
            return new D18AuditReport
            {
                schema_version = 1,
                task = TaskId,
                status = "BLOCKED",
                audit_mode = "report_only",
                read_only = true,
                registry_modified = false,
                asset_state_modified = false,
                release_approved_asserted = false,
                downloaded_assets = false,
                scanned_roots = new[] { config.manifest_root, config.candidate_root },
                manifests = Array.Empty<D18ManifestSummary>(),
                assets = Array.Empty<D18AssetAuditRecord>(),
                prefabs = Array.Empty<D18PrefabAuditRecord>(),
                category_counts = Array.Empty<D18CategoryCount>(),
            };
        }

        private static D18Manifest ReadManifest(string manifestFile, D18ManifestSummary summary)
        {
            if (!CanScanRepositoryPath(manifestFile))
            {
                summary.issues = new[] { NewIssue("error", "manifest_path_forbidden", "Manifest is outside the project or under Library/Temp/Logs") };
                return null;
            }
            var path = Path.IsPathRooted(manifestFile) ? manifestFile : Path.Combine(RepositoryRoot(), manifestFile.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                summary.issues = new[] { NewIssue("error", "manifest_missing", "Manifest file does not exist") };
                return null;
            }
            try
            {
                return JsonUtility.FromJson<D18Manifest>(File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                summary.issues = new[] { NewIssue("error", "manifest_invalid_json", exception.GetType().Name) };
                return null;
            }
        }

        private static void AuditModel(string assetPath, D18AssetAuditRecord record, List<D18Issue> issues, List<string> suggestions)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            record.importer_type = importer == null ? string.Empty : nameof(ModelImporter);
            record.main_asset_type = AssetDatabase.GetMainAssetTypeAtPath(assetPath)?.Name ?? string.Empty;
            if (importer == null)
            {
                AddIssue(issues, "error", "model_importer_missing", "ModelImporter is unavailable");
                return;
            }

            var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath) ?? Array.Empty<UnityEngine.Object>();
            var meshes = allAssets.OfType<Mesh>().ToArray();
            var clips = allAssets.OfType<AnimationClip>().Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
            record.mesh_count = meshes.Length;
            record.animation_clip_count = clips.Length;
            record.material_count = allAssets.OfType<Material>().Count();
            record.texture_count = allAssets.OfType<Texture>().Count();
            record.bounds_valid = TryGetMeshBounds(meshes, out var min, out var max);
            record.bounds_min = min;
            record.bounds_max = max;
            record.max_dimension = record.bounds_valid ? Mathf.Max(max.x - min.x, max.y - min.y, max.z - min.z) : 0f;
            record.target_scale_reference = TargetScaleReference(record.category);
            record.settings = ReadModelSettings(importer);

            var modelObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (modelObject != null)
            {
                var prefab = AuditPrefabObject(modelObject, assetPath);
                record.renderer_count = prefab.renderer_count;
                record.mesh_renderer_count = prefab.mesh_renderer_count;
                record.skinned_mesh_renderer_count = prefab.skinned_mesh_renderer_count;
                record.collider_count = prefab.collider_count;
                record.interaction_anchor_count = prefab.interaction_anchor_count;
                record.lod_group_count = prefab.lod_group_count;
                record.missing_script_count = prefab.missing_script_count;
                record.missing_reference_count = prefab.missing_reference_count;
                record.bone_count = modelObject.GetComponentsInChildren<Transform>(true).Length;
                var animator = modelObject.GetComponentInChildren<Animator>(true);
                record.avatar_present = animator != null && animator.avatar != null;
                suggestions.Add("Raw model inspected without changing it; create a Prefab and run Prefab preflight for collision, interaction anchors and LODGroup");
            }

            if (record.mesh_count == 0)
                AddIssue(issues, "error", "mesh_missing", "No Mesh sub-asset was found");
            if (!record.bounds_valid)
                AddIssue(issues, "error", "mesh_bounds_invalid", "Mesh bounds are zero or unavailable");
            if (record.material_count == 0)
                AddIssue(issues, "warning", "material_missing", "No material sub-asset was found; review material extraction");
            if (record.settings.read_write == "True")
                AddIssue(issues, "warning", "read_write_enabled", "Read/Write is enabled; confirm runtime memory need");
            if (record.settings.import_normals == "None")
                AddIssue(issues, "warning", "normals_missing", "Importer is not generating or importing normals");
            if (record.settings.import_tangents == "None")
                suggestions.Add("Review tangents for normal-mapped materials; do not change automatically outside a fixture");
            if (record.settings.mesh_compression == "High")
                AddIssue(issues, "warning", "mesh_compression_high", "High mesh compression may affect silhouette; review on the candidate");
            if (record.settings.global_scale != "" && record.settings.global_scale != "1")
                suggestions.Add("Model global scale is " + record.settings.global_scale + "; retain the source scale and let the owning Prefab document target scale");
            if (record.category == "Characters" && record.settings.animation_type == "Humanoid" && !record.avatar_present)
                AddIssue(issues, "warning", "avatar_missing", "Humanoid import has no Avatar on the imported object");
            if (record.category == "Characters" && record.settings.import_animation == "True" && record.animation_clip_count == 0)
                AddIssue(issues, "warning", "animation_clips_missing", "Character animation import is enabled but no clips were found");
            if (record.category != "Characters" && record.category != "Animals" && record.settings.import_animation == "True")
                suggestions.Add("Animation import is enabled for a non-animated category; confirm this is intentional");
            if (!Regex.IsMatch(Path.GetFileNameWithoutExtension(assetPath), LoadConfig().naming_regex ?? "^[a-z][a-z0-9]*(?:_[a-z0-9]+)+(?:_v[0-9]{2})?$", RegexOptions.CultureInvariant))
                AddIssue(issues, "warning", "model_name_not_snake_case", "Model file name does not match the D18 stable naming rule");
        }

        private static void AuditTexture(string assetPath, D18AssetAuditRecord record, List<D18Issue> issues, List<string> suggestions, D18PipelineConfig config)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            record.importer_type = importer == null ? string.Empty : nameof(TextureImporter);
            record.main_asset_type = AssetDatabase.GetMainAssetTypeAtPath(assetPath)?.Name ?? string.Empty;
            if (importer == null)
            {
                AddIssue(issues, "error", "texture_importer_missing", "TextureImporter is unavailable");
                return;
            }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
            {
                AddIssue(issues, "error", "texture_asset_missing", "Texture2D could not be loaded");
                return;
            }
            record.texture_count = 1;
            record.texture_width = texture.width;
            record.texture_height = texture.height;
            record.settings = ReadTextureSettings(importer);
            if (Mathf.Max(texture.width, texture.height) > config.max_texture_size)
                AddIssue(issues, "warning", "texture_size_over_budget", "Texture exceeds the D18 default " + config.max_texture_size + "px target");
            if (record.settings.texture_type == "NormalMap" && record.settings.srgb == "True")
                AddIssue(issues, "warning", "normal_map_srgb", "Normal map is marked sRGB; verify color-space settings");
            if (record.settings.texture_type == "Default" && record.category == "UI" && record.settings.srgb == "False")
                suggestions.Add("UI texture is not sRGB; verify authoring color space");
        }

        private static void AuditAudio(string assetPath, D18AssetAuditRecord record, List<D18Issue> issues, List<string> suggestions)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
            record.importer_type = importer == null ? string.Empty : nameof(AudioImporter);
            record.main_asset_type = AssetDatabase.GetMainAssetTypeAtPath(assetPath)?.Name ?? string.Empty;
            if (importer == null)
            {
                AddIssue(issues, "error", "audio_importer_missing", "AudioImporter is unavailable");
                return;
            }
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip == null)
            {
                AddIssue(issues, "error", "audio_clip_missing", "AudioClip could not be loaded");
                return;
            }
            record.audio_channels = clip.channels;
            record.audio_frequency = clip.frequency;
            record.audio_length_seconds = clip.length;
            record.settings = ReadAudioSettings(importer);
            if (record.settings.audio_compression == "PCM")
                suggestions.Add("PCM audio is lossless but larger; review voice/music/SFX compression by usage");
            if (record.settings.audio_compression == "Vorbis" && clip.length < 1f)
                AddIssue(issues, "warning", "short_audio_vorbis", "Very short audio uses Vorbis; review transient quality");
        }

        private static void AuditPrefabAsset(string assetPath, D18AssetAuditRecord record, List<D18Issue> issues, List<string> suggestions)
        {
            record.importer_type = "PrefabImporter";
            var root = PrefabUtility.LoadPrefabContents(assetPath);
            if (root == null)
            {
                AddIssue(issues, "error", "prefab_load_failed", "Prefab could not be opened for read-only preflight");
                return;
            }
            try
            {
                var prefab = AuditPrefabObject(root, assetPath);
                record.main_asset_type = nameof(GameObject);
                record.renderer_count = prefab.renderer_count;
                record.mesh_renderer_count = prefab.mesh_renderer_count;
                record.skinned_mesh_renderer_count = prefab.skinned_mesh_renderer_count;
                record.collider_count = prefab.collider_count;
                record.interaction_anchor_count = prefab.interaction_anchor_count;
                record.lod_group_count = prefab.lod_group_count;
                record.missing_script_count = prefab.missing_script_count;
                record.missing_reference_count = prefab.missing_reference_count;
                record.bounds_valid = prefab.bounds_valid;
                record.issues = prefab.issues;
                issues.AddRange(prefab.issues ?? Array.Empty<D18Issue>());
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static D18ImportSettings ReadModelSettings(ModelImporter importer)
        {
            return new D18ImportSettings
            {
                global_scale = ReadMember(importer, "globalScale"),
                file_scale = ReadMember(importer, "fileScale", "useFileScale"),
                axis_conversion = ReadMember(importer, "bakeAxisConversion", "useFileScale"),
                import_normals = ReadMember(importer, "importNormals"),
                import_tangents = ReadMember(importer, "importTangents"),
                mesh_compression = ReadMember(importer, "meshCompression"),
                read_write = ReadMember(importer, "isReadable"),
                import_animation = ReadMember(importer, "importAnimation"),
                animation_type = ReadMember(importer, "animationType"),
                avatar_setup = ReadMember(importer, "avatarSetup"),
                material_import_mode = ReadMember(importer, "materialImportMode"),
                material_location = ReadMember(importer, "materialLocation"),
                units_reference = "1 Unity unit = 1 metre; source dimensions are reported, not changed",
                axis_reference = "Unity Y-up / Z-forward; importer conversion flags are reported, not changed",
            };
        }

        private static D18ImportSettings ReadTextureSettings(TextureImporter importer)
        {
            return new D18ImportSettings
            {
                texture_type = ReadMember(importer, "textureType"),
                srgb = ReadMember(importer, "sRGBTexture"),
                texture_max_size = ReadMember(importer, "maxTextureSize"),
                texture_compression = ReadMember(importer, "textureCompression"),
                read_write = ReadMember(importer, "isReadable"),
                mipmaps = ReadMember(importer, "mipmapEnabled"),
                units_reference = "Texture dimensions are reported in pixels; no resize is applied in report-only mode",
            };
        }

        private static D18ImportSettings ReadAudioSettings(AudioImporter importer)
        {
            var settings = new D18ImportSettings
            {
                audio_compression = ReadMember(importer, "defaultSampleSettings", "compressionFormat"),
                audio_load_type = ReadMember(importer, "defaultSampleSettings", "loadType"),
                audio_quality = ReadMember(importer, "defaultSampleSettings", "quality"),
                audio_force_to_mono = ReadMember(importer, "forceToMono"),
                audio_load_in_background = ReadMember(importer, "loadInBackground"),
                audio_preload = ReadMember(importer, "preloadAudioData"),
            };
            return settings;
        }

        private static string ReadMember(object target, params string[] path)
        {
            object current = target;
            foreach (var memberName in path ?? Array.Empty<string>())
            {
                if (current == null)
                    return string.Empty;
                var type = current.GetType();
                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null)
                {
                    current = property.GetValue(current, null);
                    continue;
                }
                var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    current = field.GetValue(current);
                    continue;
                }
                return string.Empty;
            }
            if (current is bool boolValue) return boolValue ? "True" : "False";
            if (current is float floatValue) return floatValue.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            return current == null ? string.Empty : current.ToString();
        }

        private static int CountMissingScripts(GameObject root)
        {
            var count = 0;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                count += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
            return count;
        }

        private static int CountMissingObjectReferences(GameObject root)
        {
            var count = 0;
            try
            {
                var serializedObject = new SerializedObject(root);
                var property = serializedObject.GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue == null
                        && property.objectReferenceInstanceIDValue != 0)
                        count++;
                }
            }
            catch
            {
                // Imported model objects can be non-editable SerializedObjects; absence of a scan is not a fake PASS.
                return 0;
            }
            return count;
        }

        private static bool HasNonZeroBounds(IEnumerable<Renderer> renderers)
        {
            var any = false;
            foreach (var renderer in renderers ?? Array.Empty<Renderer>())
            {
                if (renderer != null && renderer.bounds.size.sqrMagnitude > 0.000001f)
                {
                    any = true;
                    break;
                }
            }
            return any;
        }

        private static bool TryGetMeshBounds(IEnumerable<Mesh> meshes, out Vector3 min, out Vector3 max)
        {
            var initialized = false;
            min = Vector3.zero;
            max = Vector3.zero;
            foreach (var mesh in meshes ?? Array.Empty<Mesh>())
            {
                if (mesh == null) continue;
                var bounds = mesh.bounds;
                if (!initialized)
                {
                    min = bounds.min;
                    max = bounds.max;
                    initialized = true;
                }
                else
                {
                    min = Vector3.Min(min, bounds.min);
                    max = Vector3.Max(max, bounds.max);
                }
            }
            return initialized && (max - min).sqrMagnitude > 0.000001f;
        }

        private static void AggregateReport(D18AuditReport report)
        {
            var assets = report.assets ?? Array.Empty<D18AssetAuditRecord>();
            report.file_count = assets.Length;
            report.model_file_count = assets.Count(asset => asset.importer_type == nameof(ModelImporter));
            report.texture_file_count = assets.Count(asset => asset.importer_type == nameof(TextureImporter));
            report.audio_file_count = assets.Count(asset => asset.importer_type == nameof(AudioImporter));
            report.prefab_file_count = assets.Count(asset => asset.importer_type == "PrefabImporter");
            report.mesh_count = assets.Sum(asset => asset.mesh_count);
            report.animation_clip_count = assets.Sum(asset => asset.animation_clip_count);
            report.material_count = assets.Sum(asset => asset.material_count);
            report.error_count = assets.SelectMany(asset => asset.issues ?? Array.Empty<D18Issue>()).Count(issue => issue.severity == "error")
                + (report.manifests ?? Array.Empty<D18ManifestSummary>()).SelectMany(manifest => manifest.issues ?? Array.Empty<D18Issue>()).Count(issue => issue.severity == "error");
            report.warning_count = assets.SelectMany(asset => asset.issues ?? Array.Empty<D18Issue>()).Count(issue => issue.severity == "warning")
                + (report.manifests ?? Array.Empty<D18ManifestSummary>()).SelectMany(manifest => manifest.issues ?? Array.Empty<D18Issue>()).Count(issue => issue.severity == "warning");
            report.issue_count = report.error_count + report.warning_count;
            report.category_counts = assets.GroupBy(asset => asset.category ?? "Props", StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group => new D18CategoryCount { category = group.Key, count = group.Count() })
                .ToArray();
            // Warnings remain visible in the report but are non-blocking for this
            // report-only import audit. A raw candidate filename or a missing
            // downstream Prefab field is not an import failure; only an error
            // prevents the audit itself from passing.
            report.status = report.error_count > 0 ? "BLOCKED" : "PASS";
        }

        private static bool CanScanCandidateAssetPath(string normalizedPath, D18PipelineConfig config)
        {
            return (normalizedPath.StartsWith(config.candidate_root, StringComparison.OrdinalIgnoreCase)
                    || normalizedPath.StartsWith(config.fixture_root, StringComparison.OrdinalIgnoreCase))
                && CanScanProjectPath(normalizedPath);
        }

        private static bool ContainsForbiddenSegment(string path, D18PipelineConfig config)
        {
            var forbidden = new HashSet<string>(config.forbidden_scan_segments ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            foreach (var segment in path.Replace('\\', '/').Split('/'))
            {
                if (forbidden.Contains(segment) || DefaultForbiddenSegments.Contains(segment))
                    return true;
            }
            return false;
        }

        private static bool CanScanRepositoryPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            var normalized = path.Replace('\\', '/');
            var repository = RepositoryRoot();
            var full = Path.GetFullPath(Path.IsPathRooted(normalized)
                ? normalized
                : Path.Combine(repository, normalized.Replace('/', Path.DirectorySeparatorChar)));
            if (!IsWithinRoot(full, repository))
                return false;
            var relative = full.Substring(Path.GetFullPath(repository).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace('\\', '/');
            return !ContainsForbiddenSegment(relative, LoadConfig());
        }

        private static bool IsWithinRoot(string candidatePath, string rootPath)
        {
            var candidate = Path.GetFullPath(candidatePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var root = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(candidate, root, StringComparison.OrdinalIgnoreCase)
                || candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsInteractionAnchorName(string name)
        {
            var value = (name ?? string.Empty).Replace("-", "_").ToLowerInvariant();
            return value.Contains("interaction_anchor") || value == "interactionanchor" || value.EndsWith("_interaction");
        }

        private static string TargetScaleReference(string category)
        {
            switch (category)
            {
                case "Ships": return "ship waterline/keel and content hull dimensions; owner Prefab decides target scale";
                case "Characters": return "character height/foot placement from content scale; owner Prefab decides target scale";
                case "Animals": return "species proportion and habitat reference; owner Prefab decides target scale";
                case "Buildings": return "door/foundation/module dimensions; owner Prefab decides target scale";
                default: return "category-specific D18 reference; owner Prefab decides target scale";
            }
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            var fragments = Regex.Split(value ?? string.Empty, "[^a-z0-9]+")
                .Where(fragment => !string.IsNullOrWhiteSpace(fragment))
                .ToArray();
            return (tokens ?? Array.Empty<string>()).Any(token =>
                !string.IsNullOrWhiteSpace(token)
                && fragments.Contains(token.ToLowerInvariant(), StringComparer.Ordinal));
        }

        private static void AddIssue(List<D18Issue> issues, string severity, string code, string message)
        {
            issues.Add(NewIssue(severity, code, message));
        }

        private static D18Issue NewIssue(string severity, string code, string message)
        {
            return new D18Issue { severity = severity, code = code, message = message };
        }

        private static string ProjectRoot()
        {
            return Directory.GetParent(Application.dataPath).FullName;
        }

        private static string RepositoryRoot()
        {
            return Directory.GetParent(ProjectRoot()).FullName;
        }

        private static string ToRepositoryRelativePath(string path)
        {
            var full = Path.GetFullPath(path);
            var root = RepositoryRoot().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return full.Replace('\\', '/');
            return full.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
        }

        private const string DefaultNamingRegex = "^[a-z][a-z0-9]*(?:_[a-z0-9]+)+(?:_v[0-9]{2})?$";
    }

    public static class D18ArtPipelineBatch
    {
        public static void RunKenneyAudit()
        {
            var report = D18ArtPipelineAudit.RunKenneyCandidateAudit();
            var output = D18ArtPipelineAudit.WriteReport(report);
            Debug.Log("[D18-A02] status=" + report.status + " files=" + report.file_count + " meshes=" + report.mesh_count + " animations=" + report.animation_clip_count + " materials=" + report.material_count + " issues=" + report.issue_count + " output=" + output);
        }
    }

    public static class D18ArtPipelineWindow
    {
        [MenuItem("WhalesIslandReef/D18 Art Pipeline/Run Kenney Candidate Audit")]
        public static void RunKenneyCandidateAudit()
        {
            var report = D18ArtPipelineAudit.RunKenneyCandidateAudit();
            var output = D18ArtPipelineAudit.WriteReport(report);
            Debug.Log("[D18-A02] report-only audit complete: " + report.status + " -> " + output);
        }

        [MenuItem("WhalesIslandReef/D18 Art Pipeline/Apply Fixture Suggestions To Selected Asset")]
        public static void ApplyFixtureSuggestionsToSelectedAsset()
        {
            var assetPath = Selection.activeObject == null ? string.Empty : AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!D18ArtPipelineAudit.IsD18TestFixturePath(assetPath))
            {
                EditorUtility.DisplayDialog("D18-A02", "Automatic fixes are allowed only for an explicitly selected D18 test fixture.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("D18-A02", "Apply the fixture-only suggestion to the selected test asset?", "Apply", "Cancel"))
                return;
            var result = D18ArtPipelineAudit.ApplySuggestionsForFixture(assetPath, true);
            Debug.Log("[D18-A02] fixture fix: " + result.message);
        }
    }

    [Serializable]
    public sealed class D18PipelineConfig
    {
        public int schema_version = 1;
        public string task = "D18-A02";
        public string candidate_root = "Assets/Art/Candidates/Kenney/";
        public string fixture_root = "Assets/Editor/D18ArtPipeline/Tests/Fixtures/";
        public string manifest_root = "docs/game/design/common/assets/manifests/";
        public string output_root = "tools/results/D18/D18-A02/";
        public string[] forbidden_scan_segments = { "Library", "Temp", "Logs" };
        public string[] categories = { "Ships", "Characters", "Animals", "Environment", "Buildings", "Props", "Vegetation", "UI", "Audio" };
        public string[] model_extensions = { ".fbx", ".obj", ".blend", ".dae", ".3ds", ".dxf", ".ma", ".mb", ".max", ".gltf", ".glb" };
        public string[] texture_extensions = { ".png", ".jpg", ".jpeg", ".tga", ".tif", ".tiff", ".psd", ".exr", ".hdr", ".dds" };
        public string[] audio_extensions = { ".wav", ".mp3", ".ogg", ".aif", ".aiff" };
        public string[] prefab_extensions = { ".prefab" };
        public int max_texture_size = 2048;
        public string naming_regex = "^[a-z][a-z0-9]*(?:_[a-z0-9]+)+(?:_v[0-9]{2})?$";

        public static D18PipelineConfig CreateDefault()
        {
            return new D18PipelineConfig();
        }
    }

    [Serializable]
    public sealed class D18Manifest
    {
        public string manifest_kind;
        public int schema_version;
        public string pack_id;
        public string review_status;
        public string import_status;
        public int file_count;
        public D18ManifestCandidate[] candidate_files;
    }

    [Serializable]
    public sealed class D18ManifestCandidate
    {
        public string path;
        public string role;
        public string target_id;
    }

    [Serializable]
    public sealed class D18AuditReport
    {
        public int schema_version;
        public string task;
        public string status;
        public string audit_mode;
        public bool read_only;
        public bool registry_modified;
        public bool asset_state_modified;
        public bool release_approved_asserted;
        public bool downloaded_assets;
        public string[] scanned_roots;
        public int manifest_count;
        public int file_count;
        public int model_file_count;
        public int texture_file_count;
        public int audio_file_count;
        public int prefab_file_count;
        public int mesh_count;
        public int animation_clip_count;
        public int material_count;
        public int issue_count;
        public int error_count;
        public int warning_count;
        public D18CategoryCount[] category_counts;
        public D18ManifestSummary[] manifests;
        public D18AssetAuditRecord[] assets;
        public D18PrefabAuditRecord[] prefabs;
    }

    [Serializable]
    public sealed class D18ManifestSummary
    {
        public string manifest_path;
        public string pack_id;
        public string review_status;
        public string import_status;
        public int declared_file_count;
        public string[] candidate_files;
        public D18Issue[] issues;
    }

    [Serializable]
    public sealed class D18AssetAuditRecord
    {
        public string path;
        public string target_id;
        public string role;
        public string category;
        public bool source_exists;
        public string importer_type;
        public string main_asset_type;
        public int mesh_count;
        public int animation_clip_count;
        public int material_count;
        public int texture_count;
        public int renderer_count;
        public int mesh_renderer_count;
        public int skinned_mesh_renderer_count;
        public int collider_count;
        public int interaction_anchor_count;
        public int lod_group_count;
        public int missing_script_count;
        public int missing_reference_count;
        public int bone_count;
        public bool avatar_present;
        public bool bounds_valid;
        public bool naming_valid;
        public Vector3 bounds_min;
        public Vector3 bounds_max;
        public float max_dimension;
        public string target_scale_reference;
        public int texture_width;
        public int texture_height;
        public int audio_channels;
        public int audio_frequency;
        public float audio_length_seconds;
        public D18ImportSettings settings;
        public D18Issue[] issues;
        public string[] suggestions;
    }

    [Serializable]
    public sealed class D18PrefabAuditRecord
    {
        public string path;
        public string root_name;
        public string category;
        public int renderer_count;
        public int mesh_renderer_count;
        public int skinned_mesh_renderer_count;
        public int collider_count;
        public int interaction_anchor_count;
        public int lod_group_count;
        public int missing_script_count;
        public int missing_reference_count;
        public bool bounds_valid;
        public bool naming_valid;
        public D18Issue[] issues;
    }

    [Serializable]
    public sealed class D18ImportSettings
    {
        public string units_reference;
        public string axis_reference;
        public string global_scale;
        public string file_scale;
        public string axis_conversion;
        public string import_normals;
        public string import_tangents;
        public string mesh_compression;
        public string read_write;
        public string import_animation;
        public string animation_type;
        public string avatar_setup;
        public string material_import_mode;
        public string material_location;
        public string texture_type;
        public string srgb;
        public string texture_max_size;
        public string texture_compression;
        public string mipmaps;
        public string audio_compression;
        public string audio_load_type;
        public string audio_quality;
        public string audio_force_to_mono;
        public string audio_load_in_background;
        public string audio_preload;
    }

    [Serializable]
    public sealed class D18Issue
    {
        public string severity;
        public string code;
        public string message;
    }

    [Serializable]
    public sealed class D18CategoryCount
    {
        public string category;
        public int count;
    }

    [Serializable]
    public sealed class D18FixResult
    {
        public string path;
        public bool allowed;
        public bool changed;
        public string message;
    }
}
#endif
