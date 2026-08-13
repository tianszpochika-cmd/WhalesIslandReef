using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Whalebone.Content
{
    [Serializable]
    public sealed class ContentRegistryDefinition
    {
        public int schema_version;
        public string registry_id;
        public string display_name;
        public string[] prefixes;
        public ContentPrefixDefinition[] prefix_definitions;
        public string[] roots;
    }

    [Serializable]
    public sealed class ContentPrefixDefinition
    {
        public string prefix;
        public string entity_type;
        public string directory;
        public string owner_system;
    }

    [Serializable]
    public sealed class ContentSourceMetadata
    {
        public string kind;
        public string author;
        public string reference;
    }

    [Serializable]
    public sealed class ContentLicenseMetadata
    {
        public string status;
        public string holder;
        public string reference;
        public bool commercial_use;
        public bool attribution_required;
        public bool modification_allowed;
    }

    [Serializable]
    public sealed class ContentIntegrationMetadata
    {
        public string system;
        public string entrypoint;
        public string canonical_write_owner;
    }

    [Serializable]
    public sealed class D15AnimalAuthorizationMetadata
    {
        public string authorization_id;
        public string rights_holder;
        public string license_ref;
        public string source_url_or_path;
        public string acquired_at;
        public bool commercial_use;
        public bool attribution_required;
        public bool modification_allowed;
        public string expiry_or_scope;
        public string evidence_ref;
    }

    [Serializable]
    public sealed class D15AnimalHabitatProfile
    {
        public string primary;
        public string[] secondary;
        public float min_depth_m;
        public float max_depth_m;
        public string zone;
    }

    [Serializable]
    public sealed class D15AnimalRefreshProfile
    {
        public string mode;
        public int interval_days;
        public string seed_domain;
        public string window;
    }

    [Serializable]
    public sealed class D15AnimalFoodChainProfile
    {
        public string role;
        public string[] prey_tags;
        public string[] predator_tags;
    }

    [Serializable]
    public sealed class D15AnimalDangerProfile
    {
        public string level;
        public string[] tags;
        public string telegraph;
    }

    [Serializable]
    public sealed class D15AnimalLegalProfile
    {
        public string status;
        public string rule_id;
        public bool permit_required;
        public string source_ref;
    }

    [Serializable]
    public sealed class D15AnimalPerformanceBudget
    {
        public int lod0_triangles;
        public int lod1_triangles;
        public int lod2_triangles;
        public int lod3_triangles;
        public int active_distance_m;
        public int update_hz;
        public int max_active;
    }

    [Serializable]
    public sealed class ContentRegistryEntry
    {
        public string id;
        public int schema_version;
        public string name;
        public string display_name_key;
        public string category;
        public string[] tags;
        public string description;
        public string gameplay_role;
        public string[] valid_regions;
        public string[] valid_habitats;
        public string[] valid_weather;
        public string[] dependencies;
        public string[] references;
        public string[] asset_refs;
        public string[] source_refs;
        public string source_note;
        public ContentSourceMetadata source;
        public ContentLicenseMetadata license;
        public string historical_tag;
        public string phase;
        public string status;
        public string owner_system;
        public string test_id;
        public string reviewer;
        /// <summary>D9 逻辑别名；不是新的内容主键或前缀。</summary>
        public string logic_ref;

        // D15-A03 immutable content metadata. These fields describe the
        // handoff to EcologySystem; they do not contain runtime state or
        // behavior formulas.
        public string production_batch;
        public string representative_sample;
        public string production_stage;
        public string d15_category;
        public string content_category;
        public string batch_role;
        public string behavior_level;
        public string behavior_profile;
        public D15AnimalHabitatProfile habitat_profile;
        public D15AnimalRefreshProfile refresh_profile;
        public D15AnimalFoodChainProfile food_chain_profile;
        public D15AnimalDangerProfile danger_profile;
        public D15AnimalLegalProfile legal_profile;
        public D15AnimalPerformanceBudget performance_budget;
        public string asset_state;
        public string model_ref;
        public string material_ref;
        public string collision_ref;
        public string lod_profile;
        public string[] test_cases;
        public string historical_review;
        public string[] review_evidence_refs;
        public string[] content_warning_ids;
        public string context_note_key;
        public bool skip_allowed;
        public string[] presentation_levels;
        public string quality_level;
        public string interaction_entry;
        public string function_entry;
        public ContentIntegrationMetadata integration;
        public D15AnimalAuthorizationMetadata authorization;
    }

    public sealed class ContentRegistrySnapshot
    {
        private readonly Dictionary<string, ContentRegistryEntry> entriesById;

        public ContentRegistryDefinition Definition { get; }
        public IReadOnlyList<ContentRegistryEntry> Entries { get; }

        internal ContentRegistrySnapshot(ContentRegistryDefinition definition, List<ContentRegistryEntry> entries)
        {
            Definition = definition;
            Entries = entries;
            entriesById = new Dictionary<string, ContentRegistryEntry>(StringComparer.Ordinal);
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.id))
                    throw new InvalidDataException("Content registry contains an entry without an ID.");
                if (entriesById.ContainsKey(entry.id))
                    throw new InvalidDataException("Duplicate content ID: " + entry.id);
                entriesById.Add(entry.id, entry);
            }
        }

        public ContentRegistryEntry Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            entriesById.TryGetValue(id, out var entry);
            return entry;
        }
    }

    public static class ContentRegistryReader
    {
        private const string ResourceRoot = "ContentRegistry";

        public static ContentRegistrySnapshot LoadFromResources()
        {
            var manifest = Resources.Load<TextAsset>(ResourceRoot + "/registry");
            if (manifest == null)
                throw new InvalidDataException("Missing Resources content registry manifest: " + ResourceRoot + "/registry");

            var manifestDefinition = JsonUtility.FromJson<ContentRegistryDefinition>(manifest.text);
            if (manifestDefinition == null)
                throw new InvalidDataException("Content registry manifest is invalid JSON.");

            var entryJson = new List<string>();
            var loadedAssetNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var prefix in manifestDefinition.prefix_definitions ?? Array.Empty<ContentPrefixDefinition>())
            {
                if (prefix == null || string.IsNullOrEmpty(prefix.directory)) continue;
                var folder = prefix.directory.Replace("entries/", string.Empty).Replace("\\", "/");
                var entryAssets = Resources.LoadAll<TextAsset>(ResourceRoot + "/entries/" + folder);
                foreach (var asset in entryAssets)
                {
                    if (asset != null && loadedAssetNames.Add(asset.name))
                        entryJson.Add(asset.text);
                }
            }
            return Load(manifest.text, entryJson);
        }

        public static ContentRegistrySnapshot LoadFromDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("A content registry directory is required.", nameof(directory));
            var manifestPath = Path.Combine(directory, "registry.json");
            if (!File.Exists(manifestPath))
                throw new FileNotFoundException("Missing content registry manifest.", manifestPath);

            var entryJson = new List<string>();
            foreach (var path in Directory.GetFiles(Path.Combine(directory, "entries"), "*.json", SearchOption.AllDirectories))
                entryJson.Add(File.ReadAllText(path));
            return Load(File.ReadAllText(manifestPath), entryJson);
        }

        public static ContentRegistrySnapshot Load(string manifestJson, IEnumerable<string> entryJson)
        {
            if (string.IsNullOrEmpty(manifestJson))
                throw new InvalidDataException("Content registry manifest is empty.");
            var definition = JsonUtility.FromJson<ContentRegistryDefinition>(manifestJson);
            if (definition == null || definition.schema_version != 1 || string.IsNullOrEmpty(definition.registry_id))
                throw new InvalidDataException("Content registry manifest must use schema_version 1 and a registry_id.");

            var entries = new List<ContentRegistryEntry>();
            foreach (var json in entryJson ?? Array.Empty<string>())
            {
                if (string.IsNullOrEmpty(json)) continue;
                var entry = JsonUtility.FromJson<ContentRegistryEntry>(json);
                if (entry == null || string.IsNullOrEmpty(entry.id))
                    throw new InvalidDataException("Content registry contains invalid JSON entry data.");
                entries.Add(entry);
            }
            if (entries.Count == 0)
                throw new InvalidDataException("Content registry contains no entries.");
            return new ContentRegistrySnapshot(definition, entries);
        }
    }
}
