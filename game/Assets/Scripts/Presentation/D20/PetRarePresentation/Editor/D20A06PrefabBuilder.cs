using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhalesIslandReef.Presentation.D20.PetRare;

namespace WhalesIslandReef.Editor.D20.PetRare
{
    /// <summary>
    /// Builds only the D20-A06 work Prefabs and their controllers. It never opens or saves a
    /// formal scene and never writes Registry, Save, Pet, Ecology or Reward data.
    /// </summary>
    public static class D20A06PrefabBuilder
    {
        private const string ResultRoot = "tools/results/D20/D20-A06";
        private const string PrefabRoot = "Assets/Prefabs/Animals/D20/PetsRare/";
        private const string AnimationRoot = "Assets/Animations/D20/PetsRare/";

        private sealed class Candidate
        {
            public string PrefabName;
            public string ModelPath;
            public string SpeciesId;
            public string ResourceKey;
            public string Kind;
            public bool RareObservationOnly;
            public float TargetHeight;
            public string[] AllowedActions;
        }

        [Serializable]
        private sealed class BuildRecord
        {
            public string prefab_path;
            public string model_source;
            public string controller_path;
            public string species_id;
            public string resource_key;
            public string presentation_kind;
            public bool rare_observation_only;
            public float target_height_m;
            public float source_height_units;
            public int renderer_count;
            public int material_count;
            public int animation_clip_count;
            public string[] source_animation_clips;
            public string[] controller_states;
            public bool has_animator;
            public bool has_lod_group;
            public bool has_trigger_collision;
            public bool has_follow_anchor;
            public bool has_interaction_anchor;
            public bool has_boarding_anchor;
            public bool has_rest_anchor;
            public bool has_discovery_hint_anchor;
        }

        [Serializable]
        private sealed class BuildReport
        {
            public string task = "D20-A06";
            public string status = "PASS";
            public string scope = "isolated work Prefabs and presentation adapters only";
            public bool formal_scene_modified;
            public bool registry_modified;
            public bool save_schema_modified;
            public bool pet_ecology_reward_logic_modified;
            public BuildRecord[] records;
        }

        private static readonly string[] ControllerStates =
        {
            "Idle", "Walk", "Run", "Follow", "Stay", "Rest", "Sit", "Interact", "Play", "Eat",
            "Travel", "Alert", "Startled", "Flee", "Observe", "Observed", "Unavailable",
            "DiscoverySuccess", "DiscoveryFailure"
        };

        private static readonly Candidate[] Candidates =
        {
            new Candidate
            {
                PrefabName = "D20_PetsRare_cat_core",
                ModelPath = "Assets/Art/Candidates/CharactersAnimals/GapFill/Cat/Source/Cat.fbx",
                SpeciesId = "animal_quaternius_cat_pack_vol2_v01",
                ResourceKey = "pet_animal_quaternius_cat_pack_vol2_v01_visual",
                Kind = "pet_cat",
                RareObservationOnly = false,
                TargetHeight = 0.28f,
                AllowedActions = new[] { "Idle", "Walking" }
            },
            new Candidate
            {
                PrefabName = "D20_PetsRare_dog_shiba",
                ModelPath = "Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedAnimals/Models/ShibaInu.fbx",
                SpeciesId = "animal_quaternius_shiba_inu_v01",
                ResourceKey = "pet_animal_quaternius_shiba_inu_v01_visual",
                Kind = "pet_dog",
                RareObservationOnly = false,
                TargetHeight = 0.55f,
                AllowedActions = new[] { "Idle", "Walk", "Gallop", "Eating", "Idle_HitReact" }
            },
            new Candidate
            {
                PrefabName = "D20_PetsRare_eagle_observation",
                ModelPath = "Assets/Art/Candidates/CharactersAnimals/GapFill/Bird/Models/eagle.fbx",
                SpeciesId = "animal_quaternius_eagle_v01",
                ResourceKey = "wildlife_animal_quaternius_eagle_v01_visual",
                Kind = "rare_eagle_observation",
                RareObservationOnly = true,
                TargetHeight = 0.85f,
                AllowedActions = new[] { "Idle", "Flying" }
            },
            new Candidate
            {
                PrefabName = "D20_PetsRare_fox_observation",
                ModelPath = "Assets/Art/Candidates/CharactersAnimals/QuaterniusAnimatedAnimals/Models/Fox.fbx",
                SpeciesId = "animal_quaternius_fox_v01",
                ResourceKey = "wildlife_animal_quaternius_fox_v01_visual",
                Kind = "rare_fox_observation",
                RareObservationOnly = true,
                TargetHeight = 0.55f,
                AllowedActions = new[] { "Idle", "Walk", "Gallop", "Eating", "Idle_HitReact" }
            }
        };

        [MenuItem("Tools/WhalesIslandReef/D20-A06/Build Isolated Pet Rare Prefabs")]
        public static void BuildAll()
        {
            BuildAllInternal();
        }

        public static void BatchBuild()
        {
            BuildAllInternal();
            EditorApplication.Exit(0);
        }

        [MenuItem("Tools/WhalesIslandReef/D20-A06/Render Isolated Preview Sheet")]
        public static void BuildPreview()
        {
            BuildPreviewInternal();
        }

        public static void BatchPreview()
        {
            BuildPreviewInternal();
            EditorApplication.Exit(0);
        }

        private static void BuildAllInternal()
        {
            EnsureDirectory(PrefabRoot);
            EnsureDirectory(AnimationRoot);
            EnsureDirectory(ResultRoot);

            var records = new List<BuildRecord>();
            foreach (var candidate in Candidates)
                records.Add(BuildCandidate(candidate));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var report = new BuildReport { records = records.ToArray() };
            string resultPath = ProjectPath(ResultRoot + "/D20_A06_PREFAB_BUILD.json");
            File.WriteAllText(resultPath, JsonUtility.ToJson(report, true) + "\n");
            Debug.Log("[D20-A06] built=" + records.Count + " report=" + resultPath);
        }

        private static void BuildPreviewInternal()
        {
            string previewRoot = ResultRoot + "/previews";
            EnsureDirectory(previewRoot);
            var rendered = new List<string>();

            foreach (var candidate in Candidates)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot + candidate.PrefabName + ".prefab");
                if (prefab == null)
                    throw new InvalidOperationException("D20-A06 preview Prefab missing: " + candidate.PrefabName);

                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                var cameraObject = new GameObject("D20_A06_PreviewCamera");
                var lightObject = new GameObject("D20_A06_PreviewLight");
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                try
                {
                    instance.name = candidate.PrefabName + "_Preview";
                    instance.transform.position = Vector3.zero;
                    var lodGroup = instance.GetComponent<LODGroup>();
                    if (lodGroup != null)
                        lodGroup.ForceLOD(0);
                    var previewShader = Shader.Find("Sprites/Default") ??
                        Shader.Find("Unlit/Color") ?? Shader.Find("Universal Render Pipeline/Unlit");
                    foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                    {
                        renderer.enabled = true;
                        if (previewShader != null)
                        {
                            var previewMaterial = new Material(previewShader)
                            {
                                color = candidate.RareObservationOnly
                                    ? new Color(0.74f, 0.82f, 0.9f, 1f)
                                    : new Color(0.76f, 0.55f, 0.3f, 1f)
                            };
                            renderer.sharedMaterials = Enumerable.Repeat(previewMaterial,
                                Mathf.Max(1, renderer.sharedMaterials.Length)).ToArray();
                        }
                    }
                    var bounds = CalculateBounds(instance);
                    float height = Mathf.Max(0.25f, bounds.size.y);
                    Vector3 focus = new Vector3(0f, height * 0.5f, 0f);

                    ground.name = "D20_A06_PreviewGround";
                    ground.transform.position = Vector3.zero;
                    ground.transform.localScale = Vector3.one * Mathf.Max(0.5f, height * 1.8f);
                    var groundRenderer = ground.GetComponent<Renderer>();
                    if (groundRenderer != null)
                        groundRenderer.enabled = false;

                    cameraObject.transform.position = new Vector3(0f, height * 0.65f, -height * 4.2f);
                    cameraObject.transform.LookAt(focus);
                    var camera = cameraObject.AddComponent<Camera>();
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0.12f, 0.18f, 0.24f, 1f);
                    camera.fieldOfView = 35f;
                    camera.nearClipPlane = 0.01f;
                    camera.farClipPlane = 100f;

                    lightObject.transform.position = new Vector3(-2f, height * 3f, -3f);
                    lightObject.transform.LookAt(focus);
                    var light = lightObject.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.intensity = 1.2f;
                    RenderTexture target = new RenderTexture(640, 480, 24, RenderTextureFormat.ARGB32);
                    target.Create();
                    camera.targetTexture = target;
                    camera.Render();
                    RenderTexture previous = RenderTexture.active;
                    RenderTexture.active = target;
                    var image = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
                    image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                    image.Apply();
                    string imagePath = ProjectPath(previewRoot + "/" + candidate.PrefabName + ".png");
                    File.WriteAllBytes(imagePath, image.EncodeToPNG());
                    rendered.Add(imagePath);
                    RenderTexture.active = previous;
                    camera.targetTexture = null;
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(image);
                    UnityEngine.Object.DestroyImmediate(target);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(ground);
                    UnityEngine.Object.DestroyImmediate(lightObject);
                    UnityEngine.Object.DestroyImmediate(cameraObject);
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }

            File.WriteAllText(ProjectPath(ResultRoot + "/D20_A06_PREVIEW_EVIDENCE.json"),
                JsonUtility.ToJson(new PreviewReport { images = rendered.ToArray() }, true) + "\n");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        [Serializable]
        private sealed class PreviewReport
        {
            public string[] images;
        }

        private static BuildRecord BuildCandidate(Candidate candidate)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(candidate.ModelPath);
            if (source == null)
                throw new InvalidOperationException("D20-A06 intake model missing: " + candidate.ModelPath);

            string controllerPath = AnimationRoot + candidate.PrefabName + ".controller";
            var controller = BuildController(candidate, controllerPath, source);

            var modelInstance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (modelInstance == null)
                throw new InvalidOperationException("Could not instantiate intake model: " + candidate.ModelPath);

            var instance = new GameObject(candidate.PrefabName);
            modelInstance.transform.SetParent(instance.transform, false);
            // Keep the FBX root name and animation paths intact; the consumer root remains at
            // scale one and the visual model receives the measured presentation scale.
            modelInstance.name = source.name;

            try
            {
                float sourceHeight = CalculateBounds(instance).size.y;
                if (sourceHeight <= 0.0001f)
                    throw new InvalidOperationException("Source bounds are empty: " + candidate.ModelPath);
                modelInstance.transform.localScale = Vector3.one * (candidate.TargetHeight / sourceHeight);

                var sourceRenderers = instance.GetComponentsInChildren<Renderer>(true);
                // The wrapper is a fresh GameObject. Add the presentation Animator explicitly so
                // a missing component carried by an imported FBX root cannot be mistaken for the
                // wrapper's controller host.
                var animator = instance.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

                var visual = NewChild("Visual", instance.transform);
                NewChild("LOD0", visual.transform);
                NewChild("LOD1", visual.transform);
                var lod2 = NewChild("LOD2", visual.transform);
                var fallback = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fallback.name = "RemoteFallback_" + candidate.PrefabName;
                fallback.transform.SetParent(lod2.transform, false);
                fallback.transform.localPosition = new Vector3(0f, candidate.TargetHeight * 0.5f, 0f);
                fallback.transform.localScale = new Vector3(candidate.TargetHeight * 0.6f, candidate.TargetHeight, 1f);
                var fallbackCollider = fallback.GetComponent<Collider>();
                if (fallbackCollider != null)
                    UnityEngine.Object.DestroyImmediate(fallbackCollider);
                var fallbackRenderer = fallback.GetComponent<Renderer>();
                if (fallbackRenderer != null)
                {
                    fallbackRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    fallbackRenderer.receiveShadows = false;
                }
                fallback.SetActive(false);

                var collisionProxy = NewChild("CollisionProxy", instance.transform);
                var capsule = collisionProxy.AddComponent<CapsuleCollider>();
                capsule.isTrigger = true;
                capsule.radius = Mathf.Max(0.08f, candidate.TargetHeight * 0.22f);
                capsule.height = Mathf.Max(capsule.radius * 2f, candidate.TargetHeight * 0.8f);
                capsule.direction = 1;
                capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);

                var sockets = NewChild("Sockets", instance.transform).transform;
                var follow = NewChild("FollowAnchor", sockets).transform;
                follow.localPosition = new Vector3(0f, candidate.TargetHeight * 0.25f, -candidate.TargetHeight * 0.9f);
                var interact = NewChild("InteractionAnchor", sockets).transform;
                interact.localPosition = new Vector3(0f, candidate.TargetHeight * 0.35f, candidate.TargetHeight * 0.6f);
                var boarding = NewChild("BoardingAnchor", sockets).transform;
                boarding.localPosition = new Vector3(0f, 0f, -candidate.TargetHeight * 0.5f);
                var rest = NewChild("RestAnchor", sockets).transform;
                rest.localPosition = new Vector3(0f, 0f, 0f);
                var discovery = NewChild("DiscoveryHintAnchor", sockets).transform;
                discovery.localPosition = new Vector3(0f, candidate.TargetHeight * 0.55f, 0f);

                var adapter = instance.AddComponent<D20PetRarePresentationAdapter>();
                adapter.ConfigureForEditor(candidate.SpeciesId, candidate.ResourceKey, candidate.Kind,
                    candidate.RareObservationOnly, lod2, follow, interact, boarding, rest, discovery);
                adapter.CachePresentationParts();
                adapter.SetLodBand(D20PetRareLodBand.Near);

                var lodGroup = instance.AddComponent<LODGroup>();
                lodGroup.fadeMode = LODFadeMode.CrossFade;
                lodGroup.animateCrossFading = false;
                lodGroup.SetLODs(new[]
                {
                    new LOD(0.6f, sourceRenderers),
                    new LOD(0.25f, sourceRenderers),
                    new LOD(0.08f, new[] { fallbackRenderer })
                });
                lodGroup.RecalculateBounds();

                string prefabPath = PrefabRoot + candidate.PrefabName + ".prefab";
                if (File.Exists(ProjectPath(prefabPath)))
                    AssetDatabase.DeleteAsset(prefabPath);
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                if (prefab == null)
                    throw new InvalidOperationException("Prefab save failed: " + prefabPath);

                return new BuildRecord
                {
                    prefab_path = "game/" + prefabPath,
                    model_source = "game/" + candidate.ModelPath,
                    controller_path = "game/" + controllerPath,
                    species_id = candidate.SpeciesId,
                    resource_key = candidate.ResourceKey,
                    presentation_kind = candidate.Kind,
                    rare_observation_only = candidate.RareObservationOnly,
                    target_height_m = candidate.TargetHeight,
                    source_height_units = sourceHeight,
                    renderer_count = sourceRenderers.Length,
                    material_count = CountMaterials(sourceRenderers),
                    animation_clip_count = LoadSourceClips(candidate.ModelPath).Length,
                    source_animation_clips = LoadSourceClips(candidate.ModelPath).Select(clip => clip.name).ToArray(),
                    controller_states = ControllerStates,
                    has_animator = prefab.GetComponentInChildren<Animator>(true) != null,
                    has_lod_group = prefab.GetComponent<LODGroup>() != null,
                    has_trigger_collision = prefab.GetComponentsInChildren<Collider>(true).All(c => c.isTrigger),
                    has_follow_anchor = prefab.transform.Find("Sockets/FollowAnchor") != null,
                    has_interaction_anchor = prefab.transform.Find("Sockets/InteractionAnchor") != null,
                    has_boarding_anchor = prefab.transform.Find("Sockets/BoardingAnchor") != null,
                    has_rest_anchor = prefab.transform.Find("Sockets/RestAnchor") != null,
                    has_discovery_hint_anchor = prefab.transform.Find("Sockets/DiscoveryHintAnchor") != null,
                };
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static AnimatorController BuildController(Candidate candidate, string path, GameObject source)
        {
            if (File.Exists(ProjectPath(path)))
                AssetDatabase.DeleteAsset(path);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var stateMachine = controller.layers[0].stateMachine;
            var clips = LoadSourceClips(candidate.ModelPath);
            if (clips.Length == 0)
                throw new InvalidOperationException("Source has no real animation clips: " + candidate.ModelPath);

            var idle = PickClip(clips, "Idle", "idle") ?? clips[0];
            foreach (string stateName in ControllerStates)
            {
                var state = stateMachine.AddState(stateName, new Vector3(240f, stateMachine.states.Length * 55f, 0f));
                state.writeDefaultValues = false;
                state.motion = PickClipForState(clips, stateName, idle);
            }
            stateMachine.defaultState = stateMachine.states[0].state;
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimationClip PickClipForState(AnimationClip[] clips, string stateName, AnimationClip idle)
        {
            switch (stateName)
            {
                case "Walk":
                case "Follow":
                    return PickClip(clips, "Walking", "Walk", "walking") ?? idle;
                case "Run":
                case "Flee":
                    return PickClip(clips, "Gallop", "Run", "gallop") ?? idle;
                case "Interact":
                case "Eat":
                case "Play":
                    return PickClip(clips, "Eating", "Interact", "Attack") ?? idle;
                case "Rest":
                case "Sit":
                    return PickClip(clips, "Idle_2_HeadLow", "Idle_2", "Rest", "Sit") ?? idle;
                case "Travel":
                case "Alert":
                case "Startled":
                    return PickClip(clips, "Flying", "Idle_HitReact", "Gallop", "Walk") ?? idle;
                default:
                    return idle;
            }
        }

        private static AnimationClip PickClip(AnimationClip[] clips, params string[] terms)
        {
            foreach (string term in terms)
            {
                var exact = clips.FirstOrDefault(clip => string.Equals(clip.name, term, StringComparison.OrdinalIgnoreCase));
                if (exact != null) return exact;
                var contains = clips.FirstOrDefault(clip => clip.name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                if (contains != null) return contains;
            }
            return null;
        }

        private static AnimationClip[] LoadSourceClips(string modelPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(modelPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(root.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static int CountMaterials(IEnumerable<Renderer> renderers)
        {
            return renderers.SelectMany(renderer => renderer.sharedMaterials ?? Array.Empty<Material>())
                .Where(material => material != null)
                .Distinct()
                .Count();
        }

        private static GameObject NewChild(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void EnsureDirectory(string assetPath)
        {
            string absolute = ProjectPath(assetPath);
            if (!Directory.Exists(absolute))
                Directory.CreateDirectory(absolute);
        }

        private static string ProjectPath(string path)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
