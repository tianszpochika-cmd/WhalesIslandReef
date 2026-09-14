// ============================================================
// D20PlayerCandidatePresentationBridge —— D20 R02 Player presentation bridge
//
// This bridge consumes the already-authoritative D2 physical root and presents
// a D20 R02 visual child below that root. It never writes the physical root,
// movement facts, interaction facts or saves.
//
// Q03-A23: this route is now the DEFAULT for InitialHarbor, because Q03-A19
// supplied the authored Humanoid action contract that the previous procedural
// stand-in was waiting for. Pass -d2ProceduralPlayer to restore the old route.
// See D20PlayerCandidateRuntimeInstaller for the full rationale.
// ============================================================

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using WhalesIslandReef.Presentation.D20.PlayerAnimation;
using Whalebone.Systems.D2;

namespace Whalebone.Gameplay.D2
{
    /// <summary>
    /// Candidate-only visual consumer for the D20-A02 R02 work assets.
    /// D2CharacterController remains the sole writer of this component's
    /// transform; the D20 adapter only writes Animator state and parameters on
    /// the nested visual instance.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class D20PlayerCandidatePresentationBridge : MonoBehaviour
    {
        public const string CandidatePrefabResourcePath = "D20/Candidate/D20_R02_PlayerVisual_Male";
        public const string CandidateControllerResourcePath = "D20/Candidate/D20_PlayerAnimation";
        public const string FormalPrefabResourcePath = "D20/Formal/D20_R02_PlayerVisual_Male";
        public const string FormalControllerResourcePath = "D20/Formal/D20_PlayerAnimation";
        public const string ReadableSkinMaterialResourcePath = "D20/Materials/D20_PlayerSkin_Warm";

        private GameObject _appearanceInstance;
        private Animator _candidateAnimator;
        private D20PlayerAnimationPresentationAdapter _adapter;
        private D20FormalHumanoidPoseAdapter _humanoidPoseAdapter;
        private D2CharacterController _character;
        private D2CharacterAnimationController _d2Animation;
        private D2ControlRouter _router;
        private string _lastUnsupportedAction;
        private string _prefabResourcePath = CandidatePrefabResourcePath;
        private string _controllerResourcePath = CandidateControllerResourcePath;
        private bool _humanoidPoseRequested;
        private Material _readableSkinRuntimeFallbackMaterial;

        public bool IsFormalConsumer { get; private set; }
        public string ConsumerModeName => IsFormalConsumer ? "Formal" : "Candidate";

        public bool IsInstalled { get; private set; }
        public string InstallationFailure { get; private set; } = string.Empty;
        public GameObject AppearanceInstance => _appearanceInstance;
        public Animator CandidateAnimator => _candidateAnimator;
        public D20PlayerAnimationPresentationAdapter Adapter => _adapter;
        public int HiddenExistingRendererCount { get; private set; }
        public int CandidateRendererCount { get; private set; }
        public int SnapshotCount { get; private set; }
        public int UnsupportedActionCount { get; private set; }
        public bool UsesGenericPathAdapter { get; private set; }
        public bool UsesHumanoidPoseAdapter { get; private set; }
        public D20FormalHumanoidPoseAdapter HumanoidPoseAdapter => _humanoidPoseAdapter;
        public bool SwimmingVisualSupported { get; private set; }
        public string LastUnsupportedAction { get; private set; } = string.Empty;
        public string LastD2Action { get; private set; } = string.Empty;
        public D20PlayerAnimationSnapshot LastSnapshot { get; private set; }
        public D20PlayerAnimationState LastCandidateState =>
            _adapter == null ? D20PlayerAnimationState.Idle : _adapter.CurrentState;

        /// <summary>
        /// Installs the candidate on an already-created D2 physical root. The
        /// caller owns the root lifetime; this method only adds a presentation
        /// child and disables the old visual renderers.
        /// </summary>
        public static D20PlayerCandidatePresentationBridge InstallForCandidate(GameObject physicalRoot)
        {
            return InstallForMode(physicalRoot, false, false);
        }

        /// <summary>
        /// Explicit formal consumer entry point. It uses the audited R02
        /// appearance resource, but keeps D2 as the sole physical-root owner.
        /// </summary>
        public static D20PlayerCandidatePresentationBridge InstallForFormal(GameObject physicalRoot)
        {
            return InstallForMode(physicalRoot, true, false);
        }

        /// <summary>
        /// Explicit experimental formal consumer. It keeps the authored
        /// Humanoid wrapper and drives the nested visual through
        /// HumanPoseHandler. The route is intentionally opt-in and does not
        /// change the default Player or the existing generic diagnostic path.
        /// </summary>
        public static D20PlayerCandidatePresentationBridge InstallForFormalHumanoid(GameObject physicalRoot)
        {
            return InstallForMode(physicalRoot, true, true);
        }

        private static D20PlayerCandidatePresentationBridge InstallForMode(
            GameObject physicalRoot, bool formal, bool humanoidPose)
        {
            if (physicalRoot == null) return null;
            var bridge = physicalRoot.GetComponent<D20PlayerCandidatePresentationBridge>();
            if (bridge == null) bridge = physicalRoot.AddComponent<D20PlayerCandidatePresentationBridge>();
            bridge.IsFormalConsumer = formal;
            bridge._humanoidPoseRequested = humanoidPose;
            bridge._prefabResourcePath = formal ? FormalPrefabResourcePath : CandidatePrefabResourcePath;
            bridge._controllerResourcePath = formal ? FormalControllerResourcePath : CandidateControllerResourcePath;
            bridge.Install();
            return bridge;
        }

        public Bounds GetCandidateWorldBounds()
        {
            if (_appearanceInstance == null) return new Bounds(transform.position, Vector3.zero);
            var renderers = _appearanceInstance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(_appearanceInstance.transform.position, Vector3.zero);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        public void Install()
        {
            if (IsInstalled) return;
            InstallationFailure = string.Empty;
            _character = GetComponent<D2CharacterController>();
            _d2Animation = GetComponent<D2CharacterAnimationController>();
            _router = UnityEngine.Object.FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);
            if (_character == null || _d2Animation == null)
            {
                Fail("D2 physical root is missing D2CharacterController or D2CharacterAnimationController.");
                return;
            }

            var prefab = Resources.Load<GameObject>(_prefabResourcePath);
            if (prefab == null)
            {
                Fail(ConsumerModeName + " visual prefab is not available at Resources/" + _prefabResourcePath + ".");
                return;
            }

            var controller = Resources.Load<RuntimeAnimatorController>(_controllerResourcePath);
            if (controller == null)
            {
                Fail(ConsumerModeName + " AnimatorController is not available at Resources/" + _controllerResourcePath + ".");
                return;
            }

            _appearanceInstance = Instantiate(prefab);
            _appearanceInstance.name = "D20_CandidatePlayerVisual_R02";
            _appearanceInstance.transform.SetParent(transform, false);
            _appearanceInstance.transform.localPosition = Vector3.zero;
            _appearanceInstance.transform.localRotation = Quaternion.identity;
            _appearanceInstance.transform.localScale = Vector3.one;

            if (!_humanoidPoseRequested && !FlattenCandidateVisualHierarchy(_appearanceInstance.transform))
            {
                FailAndDestroyCandidate(
                    ConsumerModeName + " visual hierarchy does not expose a direct Armature path for generic D20 clips.");
                return;
            }

            var animators = _appearanceInstance.GetComponentsInChildren<Animator>(true);
            if (animators.Length != 1)
            {
                FailAndDestroyCandidate(ConsumerModeName + " visual must contain exactly one Animator; found " + animators.Length + ".");
                return;
            }

            _candidateAnimator = animators[0];
            UsesHumanoidPoseAdapter = _humanoidPoseRequested && IsFormalConsumer;
            UsesGenericPathAdapter = !UsesHumanoidPoseAdapter && HasGenericPathClip(controller);
            if (!UsesGenericPathAdapter &&
                (_candidateAnimator.avatar == null || !_candidateAnimator.avatar.isValid || !_candidateAnimator.avatar.isHuman))
            {
                FailAndDestroyCandidate(ConsumerModeName + " visual Animator does not expose a valid Humanoid Avatar.");
                return;
            }

            if (UsesGenericPathAdapter)
                _candidateAnimator.avatar = null;
            _candidateAnimator.runtimeAnimatorController = controller;
            _candidateAnimator.applyRootMotion = false;
            _candidateAnimator.updateMode = AnimatorUpdateMode.Normal;
            _candidateAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            _candidateAnimator.enabled = !UsesHumanoidPoseAdapter;

            _adapter = _appearanceInstance.GetComponent<D20PlayerAnimationPresentationAdapter>();
            if (_adapter == null) _adapter = _appearanceInstance.AddComponent<D20PlayerAnimationPresentationAdapter>();
            _adapter.ConfigureForWork(_candidateAnimator);
            _adapter.ResetSnapshotHistory(_character.IsGrounded);
            _adapter.SetLodBand(0);
            _candidateAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            // SetLodBand(0) intentionally enables normal Animator playback for
            // the generic route. Reassert the single-writer boundary after it
            // so the HumanoidPoseHandler route cannot be overwritten by
            // generic clip evaluation.
            _candidateAnimator.enabled = !UsesHumanoidPoseAdapter;
            if (UsesHumanoidPoseAdapter)
            {
                _humanoidPoseAdapter = _appearanceInstance.GetComponent<D20FormalHumanoidPoseAdapter>();
                if (_humanoidPoseAdapter == null)
                    _humanoidPoseAdapter = _appearanceInstance.AddComponent<D20FormalHumanoidPoseAdapter>();
                if (!_humanoidPoseAdapter.ConfigureForWork(_candidateAnimator))
                {
                    FailAndDestroyCandidate(
                        ConsumerModeName + " Humanoid pose adapter requires a valid Avatar and authored skeleton root.");
                    return;
                }
            }
            SwimmingVisualSupported = UsesHumanoidPoseAdapter || _adapter.HasAnimatorState("Fish");
            ApplyReadableSkinMaterial(_appearanceInstance.transform);

            HideExistingVisualRenderers(_appearanceInstance.transform);
            CandidateRendererCount = _appearanceInstance.GetComponentsInChildren<Renderer>(true).Length;
            if (CandidateRendererCount == 0)
            {
                FailAndDestroyCandidate(ConsumerModeName + " visual contains no Renderer components.");
                return;
            }

            IsInstalled = true;
            ApplySnapshot();
            // The instantiated prefab may have been initialized with its
            // source Animator binding before the candidate controller and
            // generic-path Avatar contract are assigned. Rebind after the
            // first candidate snapshot so child bone curves (not only the
            // visual-root curve) are refreshed on the real scene instance,
            // then restore the authoritative snapshot state.
            if (UsesGenericPathAdapter)
            {
                _candidateAnimator.Rebind();
                _candidateAnimator.Update(0f);
                _candidateAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            ApplySnapshot();
            Debug.Log("[D20/" + ConsumerModeName + "] R02 Player visual installed under the D2 physical root; root motion disabled; generic path adapter=" + UsesGenericPathAdapter + "; Humanoid pose adapter=" + UsesHumanoidPoseAdapter + ".");
        }

        private void LateUpdate()
        {
            if (!IsInstalled) return;
            ApplySnapshot();
        }

        private void ApplySnapshot()
        {
            if (_adapter == null || _character == null) return;
            if (_router == null) _router = UnityEngine.Object.FindFirstObjectByType<D2ControlRouter>(FindObjectsInactive.Include);

            var d2Action = _d2Animation == null ? D2CharacterAction.Idle : _d2Animation.Action;
            LastD2Action = d2Action.ToString();
            var snapshot = new D20PlayerAnimationSnapshot
            {
                grounded = _character.IsGrounded,
                speed01 = _character.SpeedNormalized,
                verticalSpeed = _character.VerticalVelocity,
                controlMode = _router == null ? D2ControlMode.Character.ToString() : _router.Mode.ToString(),
                interactionKind = InteractionFor(d2Action),
                actionIntent = IntentFor(d2Action),
                paused = Time.timeScale <= 0f,
                lodBand = 0,
                hit = false,
                recovering = false,
            };

            if (d2Action == D2CharacterAction.Swimming && !SwimmingVisualSupported)
            {
                UnsupportedActionCount++;
                LastUnsupportedAction = d2Action.ToString();
                if (!string.Equals(_lastUnsupportedAction, LastUnsupportedAction, StringComparison.Ordinal))
                {
                    _lastUnsupportedAction = LastUnsupportedAction;
                    Debug.LogWarning("[D20/" + ConsumerModeName + "] R02 controller has no Swimming state; swimming remains outside this presentation bridge.");
                }
            }

            LastSnapshot = snapshot;
            var previousCandidateState = _adapter.CurrentState;
            var resolvedState = _adapter.ApplyReadOnlySnapshot(snapshot);
            if (_humanoidPoseAdapter != null && UsesHumanoidPoseAdapter)
                _humanoidPoseAdapter.ApplySnapshot(snapshot, resolvedState);
            if (UsesGenericPathAdapter && _candidateAnimator != null &&
                _adapter.CurrentState != previousCandidateState)
            {
                // Generic-path clips need a fresh binding when the candidate
                // enters a different presentation state after the prefab has
                // already been initialized. Rebind alone resets the state;
                // play the adapter's resolved state again so the first Fish
                // frame applies both root and child-bone curves.
                _candidateAnimator.Rebind();
                _candidateAnimator.Update(0f);
                _candidateAnimator.Play(_adapter.LastStateName, 0, 0f);
                _candidateAnimator.Update(0f);
            }
            if (UsesGenericPathAdapter && _candidateAnimator != null)
                _candidateAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (UsesGenericPathAdapter && _adapter != null)
            {
                if (_adapter.CurrentState == D20PlayerAnimationState.Helm)
                    _adapter.ApplyGenericPathHelmArmCorrection();
                else if (_adapter.CurrentState == D20PlayerAnimationState.Fish)
                    _adapter.ApplyGenericPathFishArmCorrection();
                else
                    _adapter.ApplyGenericPathNeutralArmCorrection();
            }
            SnapshotCount++;
        }

        private string IntentFor(D2CharacterAction action)
        {
            if (action == D2CharacterAction.Boarding || action == D2CharacterAction.Disembarking)
                return string.Empty;
            if (_router != null && _router.Mode == D2ControlMode.Ship)
                return "helm";
            return string.Empty;
        }

        private static string InteractionFor(D2CharacterAction action)
        {
            switch (action)
            {
                case D2CharacterAction.Boarding:
                    return "boarding";
                case D2CharacterAction.Disembarking:
                    return "disembarking";
                case D2CharacterAction.Swimming:
                    // The candidate controller's Fish clip is the bounded
                    // generic presentation fallback for D2 Swimming. It is
                    // not evidence of a formally authored swimming clip.
                    return "fish";
                default:
                    return string.Empty;
            }
        }

        private void HideExistingVisualRenderers(Transform candidateRoot)
        {
            HiddenExistingRendererCount = 0;
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer == null || renderer.transform == candidateRoot || renderer.transform.IsChildOf(candidateRoot))
                    continue;
                if (renderer.enabled) HiddenExistingRendererCount++;
                renderer.enabled = false;
            }
        }

        private void ApplyReadableSkinMaterial(Transform candidateRoot)
        {
            var skin = Resources.Load<Material>(ReadableSkinMaterialResourcePath);
            if (skin == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    Debug.LogWarning("[D20/" + ConsumerModeName + "] readable skin material and URP Lit fallback are unavailable.");
                    return;
                }

                skin = new Material(shader)
                {
                    name = "D20_RuntimeReadableSkin_Fallback"
                };
                var fallbackColor = new Color(0.72f, 0.42f, 0.25f, 1f);
                if (skin.HasProperty("_BaseColor")) skin.SetColor("_BaseColor", fallbackColor);
                if (skin.HasProperty("_Color")) skin.SetColor("_Color", fallbackColor);
                _readableSkinRuntimeFallbackMaterial = skin;
            }

            var renderers = candidateRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer == null || !string.Equals(renderer.gameObject.name, "SuperHero_Male", StringComparison.Ordinal))
                    continue;

                var materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                    materials = new[] { skin };
                else
                    materials[0] = skin;
                renderer.sharedMaterials = materials;
            }
        }

        private static bool HasGenericPathClip(RuntimeAnimatorController controller)
        {
            if (controller == null) return false;
            foreach (var clip in controller.animationClips)
            {
                if (clip != null && !clip.humanMotion && clip.hasGenericRootTransform)
                    return true;
            }
            return false;
        }

        private static bool FlattenCandidateVisualHierarchy(Transform candidateRoot)
        {
            if (candidateRoot == null) return false;
            var directArmature = candidateRoot.Find("Armature");
            if (directArmature != null) return true;

            var wrapper = FindByName(candidateRoot, "Visual_player_visual_male_r02");
            if (wrapper == null) return false;
            var children = new Transform[wrapper.childCount];
            for (var i = 0; i < wrapper.childCount; i++) children[i] = wrapper.GetChild(i);
            foreach (var child in children) child.SetParent(candidateRoot, true);
            Destroy(wrapper.gameObject);
            return candidateRoot.Find("Armature") != null;
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

        private void Fail(string message)
        {
            InstallationFailure = message ?? string.Empty;
            Debug.LogError("[D20/" + ConsumerModeName + "] " + InstallationFailure);
        }

        private void FailAndDestroyCandidate(string message)
        {
            Fail(message);
            if (_appearanceInstance != null) Destroy(_appearanceInstance);
            _appearanceInstance = null;
            _candidateAnimator = null;
            _adapter = null;
            _humanoidPoseAdapter = null;
            UsesGenericPathAdapter = false;
            UsesHumanoidPoseAdapter = false;
        }

        private void OnDestroy()
        {
            if (_readableSkinRuntimeFallbackMaterial != null)
                Destroy(_readableSkinRuntimeFallbackMaterial);
            _readableSkinRuntimeFallbackMaterial = null;
        }
    }

    /// <summary>
    /// Native Player installer for the D20 R02 player presentation.
    ///
    /// Q03-A23 promotion: as of this revision the formal Humanoid route is the
    /// DEFAULT. The reason the route used to be opt-in was that
    /// D2RuntimeBootstrap had to quarantine the unverified imported skinned
    /// mesh and fall back to a procedural low-poly stand-in, because no audited
    /// action-axis contract existed for the controllable player. Q03-A19
    /// supplied exactly that contract: the D20 action set is now authored
    /// Humanoid motion (15/15 clips, humanMotion=true, genericRoot=0) driven
    /// through a valid Humanoid Avatar, so the quarantine condition no longer
    /// holds and the procedural stand-in is what produced the reported
    /// distorted-action and cannot-swim defects.
    ///
    /// Escape hatch: pass -d2ProceduralPlayer to restore the previous
    /// procedural stand-in route without a rebuild. The explicit
    /// -d20Candidate / -d20Formal / -d20FormalHumanoid flags still select a
    /// specific route for QA.
    /// </summary>
    internal static class D20PlayerCandidateRuntimeInstaller
    {
        private const string CandidateFlag = "-d20Candidate";
        private const string FormalFlag = "-d20Formal";
        private const string FormalHumanoidFlag = "-d20FormalHumanoid";
        private const string RevertToProceduralFlag = "-d2ProceduralPlayer";
        private static bool _formalRequested;
        private static bool _formalHumanoidRequested;
        private static bool _explicitRoute;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallIfRequested()
        {
            if (HasFlag(RevertToProceduralFlag))
            {
                Debug.Log("[D20/Default] -d2ProceduralPlayer supplied; retaining the procedural stand-in route.");
                return;
            }

            var candidateRequested = HasFlag(CandidateFlag);
            _formalRequested = HasFlag(FormalFlag);
            _formalHumanoidRequested = HasFlag(FormalHumanoidFlag);
            _explicitRoute = candidateRequested || _formalRequested || _formalHumanoidRequested;

            // With no explicit route flag, promote the formal Humanoid route.
            if (!_explicitRoute)
            {
                _formalRequested = true;
                _formalHumanoidRequested = true;
                Debug.Log("[D20/Default] No route flag supplied; promoting the formal Humanoid player route.");
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static bool HasFlag(string flag)
        {
            foreach (var argument in Environment.GetCommandLineArgs())
                if (string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (UnityEngine.Object.FindFirstObjectByType<D20PlayerCandidateRuntimeInstallerBehaviour>(FindObjectsInactive.Include) != null)
                return;
            var installer = new GameObject("D20PlayerCandidateRuntimeInstaller").AddComponent<D20PlayerCandidateRuntimeInstallerBehaviour>();
            installer.Configure(_formalRequested || _formalHumanoidRequested, _formalHumanoidRequested, _explicitRoute);
        }
    }

    [DefaultExecutionOrder(900)]
    internal sealed class D20PlayerCandidateRuntimeInstallerBehaviour : MonoBehaviour
    {
        private float _deadline;
        private bool _formal;
        private bool _humanoidPose;
        private bool _explicitRoute;

        public void Configure(bool formal, bool humanoidPose, bool explicitRoute)
        {
            _formal = formal;
            _humanoidPose = humanoidPose;
            _explicitRoute = explicitRoute;
        }

        private void Awake()
        {
            _deadline = Time.realtimeSinceStartup + 20f;
        }

        private void Update()
        {
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<D2RuntimeBootstrap>(FindObjectsInactive.Include);
            if (bootstrap == null || !bootstrap.IsReady) return;

            var player = GameObject.Find("D2_PlayerAvatar");
            if (player == null) return;

            var bridge = _humanoidPose
                ? D20PlayerCandidatePresentationBridge.InstallForFormalHumanoid(player)
                : _formal
                    ? D20PlayerCandidatePresentationBridge.InstallForFormal(player)
                : D20PlayerCandidatePresentationBridge.InstallForCandidate(player);
            if (bridge != null && bridge.IsInstalled)
            {
                Debug.Log("[D20/" + bridge.ConsumerModeName + "] installer completed (" +
                          (_explicitRoute ? "explicit route flag" : "default route") + ").");
                Destroy(gameObject);
                return;
            }

            if (Time.realtimeSinceStartup >= _deadline)
            {
                Debug.LogError("[D20/" + (_humanoidPose ? "FormalHumanoid" : _formal ? "Formal" : "Candidate") + "] command-line installer timed out: " +
                    (bridge == null ? "no bridge" : bridge.InstallationFailure));
                Destroy(gameObject);
            }
        }
    }
}
