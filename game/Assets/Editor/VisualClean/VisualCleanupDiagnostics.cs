#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Whalebone.Editor.VisualClean
{
    /// <summary>
    /// Read-only scene inventory used to identify visible proxy meshes during
    /// visual cleanup. It does not mutate the scene, prefabs or gameplay.
    /// </summary>
    public static class VisualCleanupDiagnostics
    {
        private const string ScenePath = "Assets/Scenes/InitialHarbor.unity";
        private const string RootName = "D19_R03_HarborPresentation";
        private const string OutputPath = "tools/results/VISUAL-CLEAN/VC-A09_SCENE_RENDERER_DIAGNOSTICS.json";

        [Serializable]
        private sealed class Report
        {
            public string schema = "VISUAL_CLEAN_SCENE_RENDERER_DIAGNOSTICS_V1";
            public string scene;
            public string root;
            public int renderer_count;
            public Row[] renderers;
            public Row[] scene_renderers;
        }

        [Serializable]
        private sealed class Row
        {
            public string hierarchy_path;
            public bool enabled;
            public string[] materials;
            public string[] material_base_colors;
            public bool[] material_has_texture;
            public Vector3 world_position;
            public Vector3 bounds_center;
            public Vector3 bounds_size;
        }

        [MenuItem("Whalebone/Visual Clean/Write Renderer Diagnostics")]
        public static void WriteFromMenu()
        {
            Write();
        }

        public static void BatchWrite()
        {
            try
            {
                Write();
                UnityEditor.EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                UnityEditor.EditorApplication.Exit(1);
            }
        }

        public static void Write()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var root = GameObject.Find(RootName);
            if (root == null) throw new InvalidOperationException("Missing " + RootName);

            var rows = new List<Row>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                rows.Add(CreateRow(renderer, root.transform));
            }

            var sceneRows = new List<Row>();
            foreach (var sceneRoot in scene.GetRootGameObjects())
            {
                foreach (var renderer in sceneRoot.GetComponentsInChildren<Renderer>(true))
                    sceneRows.Add(CreateRow(renderer, sceneRoot.transform));
            }

            rows.Sort((left, right) =>
            {
                var leftArea = left.bounds_size.x * left.bounds_size.y * left.bounds_size.z;
                var rightArea = right.bounds_size.x * right.bounds_size.y * right.bounds_size.z;
                return rightArea.CompareTo(leftArea);
            });
            sceneRows.Sort((left, right) =>
            {
                var leftArea = left.bounds_size.x * left.bounds_size.y * left.bounds_size.z;
                var rightArea = right.bounds_size.x * right.bounds_size.y * right.bounds_size.z;
                return rightArea.CompareTo(leftArea);
            });

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var output = Path.Combine(projectRoot, OutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, JsonUtility.ToJson(new Report
            {
                scene = scene.path,
                root = RootName,
                renderer_count = rows.Count,
                renderers = rows.ToArray(),
                scene_renderers = sceneRows.ToArray(),
            }, true));
            Debug.Log("[VISUAL-CLEAN] renderer diagnostics written: " + output);
        }

        private static string HierarchyPath(Transform transform, Transform root)
        {
            var names = new List<string>();
            var current = transform;
            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }
            names.Add(root.name);
            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        private static Row CreateRow(Renderer renderer, Transform root)
        {
            var materialNames = new List<string>();
            var materialColors = new List<string>();
            var materialTextures = new List<bool>();
            foreach (var material in renderer.sharedMaterials)
            {
                materialNames.Add(material == null ? string.Empty : material.name);
                if (material == null)
                {
                    materialColors.Add(string.Empty);
                    materialTextures.Add(false);
                    continue;
                }

                Color color = Color.white;
                if (material.HasProperty("_BaseColor")) color = material.GetColor("_BaseColor");
                else if (material.HasProperty("_Color")) color = material.GetColor("_Color");
                materialColors.Add("#" + ColorUtility.ToHtmlStringRGBA(color));
                materialTextures.Add(material.mainTexture != null);
            }
            return new Row
            {
                hierarchy_path = HierarchyPath(renderer.transform, root),
                enabled = renderer.enabled,
                materials = materialNames.ToArray(),
                material_base_colors = materialColors.ToArray(),
                material_has_texture = materialTextures.ToArray(),
                world_position = renderer.transform.position,
                bounds_center = renderer.bounds.center,
                bounds_size = renderer.bounds.size,
            };
        }
    }
}
#endif
