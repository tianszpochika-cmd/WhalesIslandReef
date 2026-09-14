using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.LocalQuality.Q03
{
    /// <summary>
    /// Q03 candidate-only runtime gate. It proves that the audited D20 R02
    /// visual can sit below the existing D2 physical root and consume real D2
    /// action transitions without becoming a second movement owner. It is not
    /// formal D20 or VC-G01 acceptance; Swimming is only mapped to the
    /// existing Fish candidate pose and is not a formal authored resource.
    /// </summary>
    public sealed class Q03D20CandidateIntegrationPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator R02CandidateVisualConsumesD2MovementAndBoardingWithoutMovingPhysicalRoot()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var characterType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterController");
            var animationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            var routerType = FindLoadedType("Whalebone.Gameplay.D2.D2ControlRouter");
            var bridgeType = FindLoadedType("Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge");
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var hudCoordinatorType = FindLoadedType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            var d7CoordinatorType = FindLoadedType("Whalebone.Gameplay.D7.D7ControlCoordinator");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(characterType, "D2CharacterController 未加载。");
            Assert.IsNotNull(animationType, "D2CharacterAnimationController 未加载。");
            Assert.IsNotNull(routerType, "D2ControlRouter 未加载。");
            Assert.IsNotNull(bridgeType, "D20 candidate bridge 未加载。");
            Assert.IsNotNull(inputRouterType, "D16InputRouter 未加载。");
            Assert.IsNotNull(hudCoordinatorType, "RuntimeHudCoordinator 未加载。");
            Assert.IsNotNull(d7CoordinatorType, "D7ControlCoordinator 未加载。");
            var setGameplayInputBlocked = inputRouterType.GetMethod("SetGameplayInputBlocked",
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(setGameplayInputBlocked, "D16InputRouter 缺少 gameplay input ownership reset。");
            setGameplayInputBlocked.Invoke(null, new object[] { false });
            hudCoordinatorType.GetMethod("RestorePrimaryHud", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);
            hudCoordinatorType.GetMethod("SetPaused", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { false });

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var isReady = bootstrapType.GetProperty("IsReady");
            var deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)isReady.GetValue(bootstrap)) && Time.realtimeSinceStartup < deadline)
            {
                bootstrap = UnityEngine.Object.FindFirstObjectByType(bootstrapType, FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2RuntimeBootstrap 没有创建。");
            Assert.IsTrue((bool)isReady.GetValue(bootstrap), "D2 运行时闭环没有完成，不能执行 candidate-only 集成门禁。");

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var existingPlayerRenderers = player.GetComponentsInChildren<Renderer>(true);
            var character = player.GetComponent(characterType);
            var animation = player.GetComponent(animationType);
            var router = UnityEngine.Object.FindFirstObjectByType(routerType, FindObjectsInactive.Include) as Component;
            Assert.IsNotNull(character, "玩家缺少 D2CharacterController。");
            Assert.IsNotNull(animation, "玩家缺少 D2CharacterAnimationController。");
            Assert.IsNotNull(router, "InitialHarbor 缺少 D2ControlRouter。");
            var setInputEnabled = characterType.GetMethod("SetInputEnabled");
            Assert.IsNotNull(setInputEnabled, "D2CharacterController 缺少 SetInputEnabled。");
            setInputEnabled.Invoke(character, new object[] { true });

            var install = bridgeType.GetMethod("InstallForCandidate", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(install, "candidate bridge 缺少显式安装入口。");
            var bridge = install.Invoke(null, new object[] { player }) as Component;
            Assert.IsNotNull(bridge, "candidate bridge 没有创建。");
            var installed = bridgeType.GetProperty("IsInstalled");
            var failure = bridgeType.GetProperty("InstallationFailure");
            var appearance = bridgeType.GetProperty("AppearanceInstance");
            var candidateAnimatorProperty = bridgeType.GetProperty("CandidateAnimator");
            var adapterProperty = bridgeType.GetProperty("Adapter");
            var candidateStateProperty = bridgeType.GetProperty("LastCandidateState");
            Assert.IsTrue((bool)installed.GetValue(bridge), failure.GetValue(bridge) as string);
            var appearanceObject = appearance.GetValue(bridge) as GameObject;
            Assert.IsNotNull(appearanceObject, "candidate visual instance 缺失。");
            var candidateAnimator = candidateAnimatorProperty.GetValue(bridge) as Animator;
            Assert.IsNotNull(candidateAnimator, "candidate Animator 缺失。");
            var adapter = adapterProperty.GetValue(bridge) as Component;
            Assert.IsNotNull(adapter, "candidate presentation adapter 缺失。");
            Assert.IsFalse(candidateAnimator.applyRootMotion, "candidate Animator 不得开启 root motion。");
            Assert.AreEqual(AnimatorCullingMode.AlwaysAnimate, candidateAnimator.cullingMode,
                "generic candidate clips 必须在没有相机裁剪时持续更新。");
            Assert.IsNull(candidateAnimator.avatar,
                "D20 candidate 的 generic path adapter 必须显式不使用不兼容的 Humanoid retargeting。");
            Assert.IsNotNull(appearanceObject.transform.Find("Armature"),
                "候选资源必须把 Armature 放到 Animator 根下，才能解析 generic clip path。");
            Assert.IsNull(appearanceObject.transform.Find("Visual_player_visual_male_r02"),
                "候选资源不得保留会破坏 clip path 的额外 visual wrapper。");
            Assert.IsTrue((bool)bridgeType.GetProperty("UsesGenericPathAdapter").GetValue(bridge),
                "候选 D20 clip contract 必须显式走 generic path adapter。");
            Assert.IsTrue((bool)bridgeType.GetProperty("SwimmingVisualSupported").GetValue(bridge),
                "candidate 必须至少有一个显式的 Swimming presentation fallback state。");
            Assert.Greater((int)bridgeType.GetProperty("CandidateRendererCount").GetValue(bridge), 0,
                "candidate visual 没有 Renderer。");
            Assert.IsNotNull(appearanceObject.transform.Find("D20_CandidateOutfit_MalePeasant"),
                "candidate visual 必须包含已通过骨骼路径重绑定的 1675 male peasant outfit。");
            var candidateBody = FindByName(appearanceObject.transform, "SuperHero_Male");
            var candidateBodyRenderer = candidateBody == null ? null : candidateBody.GetComponent<Renderer>();
            Assert.IsNotNull(candidateBodyRenderer, "candidate visual 缺少 SuperHero_Male renderer。");
            Assert.AreEqual("D20_PlayerSkin_Warm", candidateBodyRenderer.sharedMaterial.name,
                "D20 presentation owner 的 readable skin 不得被 D7 material polish 覆盖。");
            Assert.GreaterOrEqual(appearanceObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length, 7,
                "candidate visual 必须包含 body 与 outfit 的 skinned renderers。");
            Assert.Greater((int)bridgeType.GetProperty("HiddenExistingRendererCount").GetValue(bridge), 0,
                "候选视觉启用后必须隐藏旧的 fallback renderer，不能同时显示两个人物。");

            var settleDeadline = Time.realtimeSinceStartup + 1f;
            while (!(bool)characterType.GetProperty("IsGrounded").GetValue(character) ||
                   candidateStateProperty.GetValue(bridge).ToString() != "Idle")
            {
                if (Time.realtimeSinceStartup >= settleDeadline)
                    break;
                yield return null;
            }

            Assert.IsTrue((bool)characterType.GetProperty("IsGrounded").GetValue(character),
                "候选视觉门禁必须在落地后建立物理根基线。");
            Assert.AreEqual("Idle", candidateStateProperty.GetValue(bridge).ToString(),
                "候选视觉门禁必须在落地后的 Idle 状态建立物理根基线。");
            var physicalPosition = player.transform.position;
            var physicalRotation = player.transform.rotation;
            yield return null;
            for (var i = 0; i < existingPlayerRenderers.Length; i++)
                Assert.IsFalse(existingPlayerRenderers[i].enabled,
                    "D27 presentation restore 不得重新启用 D20 candidate 已隐藏的旧 player Renderer：" +
                    existingPlayerRenderers[i].name);
            Assert.That(Vector3.Distance(physicalPosition, player.transform.position), Is.LessThanOrEqualTo(0.02f),
                "D20 adapter 不能移动 D2 physical root；允许 2cm 仅用于一帧 CharacterController 落地校准。");
            Assert.That(Quaternion.Angle(physicalRotation, player.transform.rotation), Is.LessThanOrEqualTo(0.1f),
                "D20 adapter 不能旋转 D2 physical root。");
            Assert.AreEqual("Idle", candidateStateProperty.GetValue(bridge).ToString(),
                "静止落地时 candidate 必须处于 Idle。");
            Assert.IsFalse((bool)adapter.GetType().GetProperty("LastApplyUsedFallback").GetValue(adapter),
                "Idle 不应落入缺状态 fallback。");

            var leftThigh = appearanceObject.transform.Find("Armature/root/pelvis/thigh_l");
            Assert.IsNotNull(leftThigh, "候选 Armature 缺少 thigh_l generic animation target。");
            candidateAnimator.Play("Idle", 0, 0.5f);
            candidateAnimator.Update(0.01f);
            var idleThighRotation = leftThigh.localRotation;
            candidateAnimator.Play("Walk", 0, 0.5f);
            candidateAnimator.Update(0.01f);
            Assert.Greater(Quaternion.Angle(idleThighRotation, leftThigh.localRotation), 0.5f,
                "候选 generic Walk clip 没有改变 thigh_l 姿态，不能把状态名消费当作动作消费。");

            Keyboard keyboard = null;
            try
            {
                keyboard = InputSystem.AddDevice<Keyboard>();
                keyboard.MakeCurrent();
                Press(keyboard.wKey, queueEventOnly: true);
                InputSystem.Update();
                yield return null;
                yield return null;
                Assert.AreEqual("Walk", candidateStateProperty.GetValue(bridge).ToString(),
                    "真实 W 移动没有映射到 D20 Walk。" + DescribeMovementInputState(
                        inputRouterType, hudCoordinatorType, d7CoordinatorType, characterType,
                        routerType, character, router, bridge, keyboard));

                Press(keyboard.leftShiftKey, queueEventOnly: true);
                InputSystem.Update();
                deadline = Time.realtimeSinceStartup + 1f;
                while (candidateStateProperty.GetValue(bridge).ToString() != "Run" &&
                       Time.realtimeSinceStartup < deadline)
                    yield return null;
                Assert.AreEqual("Run", candidateStateProperty.GetValue(bridge).ToString(),
                    "真实 Shift+W 移动没有映射到 D20 Run。");

                Release(keyboard.wKey, queueEventOnly: true);
                Release(keyboard.leftShiftKey, queueEventOnly: true);
                InputSystem.Update();
                setInputEnabled.Invoke(character, new object[] { false });
                yield return null;
                var physicalPositionBeforeCandidateAnimatorUpdate = player.transform.position;
                candidateAnimator.Update(0.1f);
                Assert.AreEqual(physicalPositionBeforeCandidateAnimatorUpdate, player.transform.position,
                    "D20 candidate Animator.Update 不得移动 D2 physical root。");
                setInputEnabled.Invoke(character, new object[] { true });
                yield return null;
                Assert.IsTrue((bool)characterType.GetProperty("IsGrounded").GetValue(character),
                    "登船动作前玩家必须落地。");

                Assert.IsTrue((bool)routerType.GetMethod("TryToggleShip").Invoke(router, null),
                    "candidate-only 门禁无法触发上船。");
                yield return null;
                Assert.AreEqual("Board", candidateStateProperty.GetValue(bridge).ToString(),
                    "D2 Boarding 过渡没有映射到 D20 Board。");
                Assert.IsFalse((bool)adapter.GetType().GetProperty("LastApplyUsedFallback").GetValue(adapter),
                    "Board 不应落入缺状态 fallback。");
            }
            finally
            {
                if (keyboard != null && keyboard.added)
                {
                    Release(keyboard.wKey);
                    Release(keyboard.leftShiftKey);
                    InputSystem.Update();
                    InputSystem.RemoveDevice(keyboard);
                }
            }

            Assert.Greater((int)bridgeType.GetProperty("SnapshotCount").GetValue(bridge), 0,
                "candidate bridge 没有消费 D2 snapshot。");
            Assert.AreEqual(0, (int)bridgeType.GetProperty("UnsupportedActionCount").GetValue(bridge),
                "当前 candidate controller 已提供 Fish 作为 D2 Swimming 的 bounded presentation fallback。");
        }

        [UnityTest]
        public IEnumerator CandidateFishRuntimePoseProbeReportsAppliedStateAndBoneAxes()
        {
            var bootstrapType = FindLoadedType("Whalebone.Gameplay.D2.D2RuntimeBootstrap");
            var bridgeType = FindLoadedType("Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge");
            var inputRouterType = FindLoadedType("Whalebone.Gameplay.UI.D16.D16InputRouter");
            var hudCoordinatorType = FindLoadedType("Whalebone.Gameplay.UI.RuntimeHudCoordinator");
            Assert.IsNotNull(bootstrapType, "D2RuntimeBootstrap 未加载。");
            Assert.IsNotNull(bridgeType, "D20 candidate bridge 未加载。");

            inputRouterType.GetMethod("SetGameplayInputBlocked", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { false });
            hudCoordinatorType.GetMethod("RestorePrimaryHud", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);
            hudCoordinatorType.GetMethod("SetPaused", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { false });

            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            Component bootstrap = null;
            var isReady = bootstrapType.GetProperty("IsReady");
            var deadline = Time.realtimeSinceStartup + 8f;
            while ((bootstrap == null || !(bool)isReady.GetValue(bootstrap)) && Time.realtimeSinceStartup < deadline)
            {
                bootstrap = UnityEngine.Object.FindFirstObjectByType(bootstrapType, FindObjectsInactive.Include) as Component;
                yield return null;
            }

            Assert.IsNotNull(bootstrap, "D2RuntimeBootstrap 没有创建。");
            Assert.IsTrue((bool)isReady.GetValue(bootstrap), "D2 运行时闭环没有完成，不能执行 Fish pose probe。");

            var player = GameObject.Find("D2_PlayerAvatar");
            Assert.IsNotNull(player, "InitialHarbor 玩家对象没有创建。");
            var install = bridgeType.GetMethod("InstallForCandidate", BindingFlags.Public | BindingFlags.Static);
            var bridge = install.Invoke(null, new object[] { player }) as Component;
            Assert.IsNotNull(bridge, "candidate bridge 没有创建。");
            Assert.IsTrue((bool)bridgeType.GetProperty("IsInstalled").GetValue(bridge),
                bridgeType.GetProperty("InstallationFailure").GetValue(bridge) as string);

            var appearance = bridgeType.GetProperty("AppearanceInstance").GetValue(bridge) as GameObject;
            var animator = bridgeType.GetProperty("CandidateAnimator").GetValue(bridge) as Animator;
            var adapter = bridgeType.GetProperty("Adapter").GetValue(bridge) as Component;
            Assert.IsNotNull(appearance, "candidate visual instance 缺失。");
            Assert.IsNotNull(animator, "candidate Animator 缺失。");
            Assert.IsNotNull(adapter, "candidate presentation adapter 缺失。");

            var fishClip = Array.Find(animator.runtimeAnimatorController.animationClips,
                clip => clip != null && clip.name.IndexOf("Fish", StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.IsNotNull(fishClip, "candidate controller 未提供可定位的 Fish clip。");

            var armature = appearance.transform.Find("Armature");
            var genericRoot = appearance.transform.Find("Armature/root");
            var pelvis = appearance.transform.Find("Armature/root/pelvis");
            var upperArmL = appearance.transform.Find(
                "Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l");
            var upperArmR = appearance.transform.Find(
                "Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r");
            var thighL = appearance.transform.Find("Armature/root/pelvis/thigh_l");
            var thighR = appearance.transform.Find("Armature/root/pelvis/thigh_r");
            Assert.IsNotNull(armature, "candidate Armature 缺失。");
            Assert.IsNotNull(genericRoot, "candidate generic root 缺失。");
            Assert.IsNotNull(pelvis, "candidate pelvis 缺失。");
            Assert.IsNotNull(upperArmL, "candidate upperarm_l 缺失。");
            Assert.IsNotNull(upperArmR, "candidate upperarm_r 缺失。");
            Assert.IsNotNull(thighL, "candidate thigh_l 缺失。");
            Assert.IsNotNull(thighR, "candidate thigh_r 缺失。");

            var physicalPosition = player.transform.position;
            var physicalRotation = player.transform.rotation;
            var animatorInventory = string.Join(";", Array.ConvertAll(
                player.GetComponentsInChildren<Animator>(true), item =>
                    item.gameObject.name + "|enabled=" + item.enabled + "|controller=" +
                    (item.runtimeAnimatorController == null ? "null" : item.runtimeAnimatorController.name)));
            var d2AnimationType = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            var d2Animation = player.GetComponent(d2AnimationType);
            var visualRootField = d2AnimationType.GetField("_visualRoot", BindingFlags.Instance | BindingFlags.NonPublic);
            var d2VisualRoot = visualRootField == null ? null : visualRootField.GetValue(d2Animation) as Transform;
            animator.Play("Idle", 0, 0.5f);
            animator.Update(0.01f);
            var idle = CapturePose("Idle", animator, appearance.transform, armature, genericRoot,
                pelvis, upperArmL, upperArmR, thighL, thighR, fishClip.name);

            var bridgeBehaviour = bridge as Behaviour;
            Assert.IsNotNull(bridgeBehaviour, "candidate bridge must be a Behaviour for the isolated pose sample.");
            bridgeBehaviour.enabled = false;
            var apply = adapter.GetType().GetMethod("ApplyReadOnlySnapshot");
            Assert.IsNotNull(apply, "candidate adapter 缺少只读 snapshot 入口。");
            var snapshotType = apply.GetParameters()[0].ParameterType;
            var fishSnapshot = Activator.CreateInstance(snapshotType);
            snapshotType.GetField("grounded").SetValue(fishSnapshot, true);
            snapshotType.GetField("speed01").SetValue(fishSnapshot, 0f);
            snapshotType.GetField("verticalSpeed").SetValue(fishSnapshot, 0f);
            snapshotType.GetField("interactionKind").SetValue(fishSnapshot, "fish");
            snapshotType.GetField("controlMode").SetValue(fishSnapshot, "Character");
            snapshotType.GetField("paused").SetValue(fishSnapshot, false);
            snapshotType.GetField("lodBand").SetValue(fishSnapshot, 0);
            apply.Invoke(adapter, new object[] { fishSnapshot });
            animator.Play("Fish", 0, 0.5f);
            animator.Update(0.01f);
            var fishImmediate = CapturePose("FishImmediate", animator, appearance.transform, armature, genericRoot,
                pelvis, upperArmL, upperArmR, thighL, thighR, fishClip.name);
            yield return null;
            animator.Update(0.01f);
            var fish = CapturePose("Fish", animator, appearance.transform, armature, genericRoot,
                pelvis, upperArmL, upperArmR, thighL, thighR, fishClip.name);
            animator.Play("Idle", 0, 0.5f);
            animator.Update(0.01f);
            animator.Play("Fish", 0, 0.5f);
            animator.Update(0.01f);
            yield return null;
            animator.Update(0.01f);
            var fishAfterIdleReset = CapturePose("FishAfterIdleReset", animator, appearance.transform, armature, genericRoot,
                pelvis, upperArmL, upperArmR, thighL, thighR, fishClip.name);
            var isolatedPrefab = Resources.Load<GameObject>("D20/Candidate/D20_R02_PlayerVisual_Male");
            Assert.IsNotNull(isolatedPrefab, "candidate Resources prefab 缺失，无法建立 Animator 对照样本。");
            var isolated = UnityEngine.Object.Instantiate(isolatedPrefab);
            var isolatedAnimator = isolated.GetComponent<Animator>();
            Assert.IsNotNull(isolatedAnimator, "candidate 对照实例缺少 Animator。");
            isolatedAnimator.avatar = null;
            isolatedAnimator.runtimeAnimatorController = animator.runtimeAnimatorController;
            isolatedAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            isolatedAnimator.Play("Fish", 0, 0.5f);
            isolatedAnimator.Update(0.01f);
            yield return null;
            isolatedAnimator.Update(0.01f);
            var isolatedPose = CapturePose("Fish", isolatedAnimator, isolated.transform,
                isolated.transform.Find("Armature"), isolated.transform.Find("Armature/root"),
                isolated.transform.Find("Armature/root/pelvis"),
                isolated.transform.Find("Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l"),
                isolated.transform.Find("Armature/root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r"),
                isolated.transform.Find("Armature/root/pelvis/thigh_l"),
                isolated.transform.Find("Armature/root/pelvis/thigh_r"), fishClip.name);
            UnityEngine.Object.Destroy(isolated);
            fishClip.SampleAnimation(appearance, 0.5f);
            var sampled = CapturePose("SampledFish", animator, appearance.transform, armature, genericRoot,
                pelvis, upperArmL, upperArmR, thighL, thighR, fishClip.name);

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            var result = new FishPoseProbeResult
            {
                generatedUtc = DateTime.UtcNow.ToString("O"),
                candidateInstalled = (bool)bridgeType.GetProperty("IsInstalled").GetValue(bridge),
                usesGenericPathAdapter = (bool)bridgeType.GetProperty("UsesGenericPathAdapter").GetValue(bridge),
                candidateAnimatorAvatarIsNull = animator.avatar == null,
                rootMotionEnabled = animator.applyRootMotion,
                animatorSpeed = animator.speed,
                animatorInTransition = animator.IsInTransition(0),
                animatorInventory = animatorInventory,
                d2VisualRootName = d2VisualRoot == null ? "null" : d2VisualRoot.name,
                adapterState = adapter.GetType().GetProperty("CurrentState").GetValue(adapter).ToString(),
                adapterLastStateName = adapter.GetType().GetProperty("LastStateName").GetValue(adapter) as string,
                adapterFallback = (bool)adapter.GetType().GetProperty("LastApplyUsedFallback").GetValue(adapter),
                animatorFishState = stateInfo.IsName("Fish"),
                animatorStateName = stateInfo.IsName("Fish") ? "Fish" : stateInfo.fullPathHash.ToString(),
                fishClipName = fishClip.name,
                fishClipLength = fishClip.length,
                physicalRootPositionBefore = Format(physicalPosition),
                physicalRootPositionAfter = Format(player.transform.position),
                physicalRootRotationBefore = Format(physicalRotation.eulerAngles),
                physicalRootRotationAfter = Format(player.transform.rotation.eulerAngles),
                idle = idle,
                fishImmediate = fishImmediate,
                fish = fish,
                fishAfterIdleReset = fishAfterIdleReset,
                isolatedControl = isolatedPose,
                sampled = sampled
            };

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            var resultPath = Path.Combine(projectRoot, "tools/results/LOCAL-QUALITY/Q03-A07/Q03_A07_RUNTIME_FISH_POSE_PROBE.json");
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            File.WriteAllText(resultPath, JsonUtility.ToJson(result, true) + Environment.NewLine);
            Debug.Log("[Q03-A07/FishPoseProbe] " + JsonUtility.ToJson(result));

            Assert.IsTrue(result.candidateInstalled, "candidate bridge 在 pose probe 中丢失安装状态。");
            Assert.IsTrue(result.usesGenericPathAdapter, "pose probe 必须覆盖 generic path adapter。");
            Assert.IsTrue(result.candidateAnimatorAvatarIsNull, "generic candidate Animator 必须保持 Avatar=null。");
            Assert.IsFalse(result.rootMotionEnabled, "candidate Animator 不得启用 root motion。");
            Assert.AreEqual("Fish", result.adapterState, "只读 fish snapshot 未进入 Fish presentation state。");
            Assert.IsFalse(result.adapterFallback, "Fish 不应落入 Idle fallback。");
            Assert.IsTrue(result.animatorFishState, "Animator 在 probe 采样时没有实际处于 Fish state。");
            Assert.AreEqual(physicalPosition, player.transform.position, "Fish clip 不得移动 D2 physical root。");
            Assert.AreEqual(physicalRotation, player.transform.rotation, "Fish clip 不得旋转 D2 physical root。");
        }

        [Serializable]
        private sealed class FishPoseProbeResult
        {
            public string generatedUtc;
            public bool candidateInstalled;
            public bool usesGenericPathAdapter;
            public bool candidateAnimatorAvatarIsNull;
            public bool rootMotionEnabled;
            public float animatorSpeed;
            public bool animatorInTransition;
            public string animatorInventory;
            public string d2VisualRootName;
            public string adapterState;
            public string adapterLastStateName;
            public bool adapterFallback;
            public bool animatorFishState;
            public string animatorStateName;
            public string fishClipName;
            public float fishClipLength;
            public string physicalRootPositionBefore;
            public string physicalRootPositionAfter;
            public string physicalRootRotationBefore;
            public string physicalRootRotationAfter;
            public FishPoseSnapshot idle;
            public FishPoseSnapshot fishImmediate;
            public FishPoseSnapshot fish;
            public FishPoseSnapshot fishAfterIdleReset;
            public FishPoseSnapshot isolatedControl;
            public FishPoseSnapshot sampled;
        }

        [Serializable]
        private sealed class FishPoseSnapshot
        {
            public string label;
            public string animatorState;
            public float normalizedTime;
            public string appearanceLocalEuler;
            public string armatureLocalEuler;
            public string genericRootLocalEuler;
            public string pelvisLocalEuler;
            public string upperArmLLocalEuler;
            public string upperArmRLocalEuler;
            public string thighLLocalEuler;
            public string thighRLocalEuler;
            public string clipName;
        }

        private static FishPoseSnapshot CapturePose(string label, Animator animator,
            Transform appearance, Transform armature, Transform genericRoot, Transform pelvis,
            Transform upperArmL, Transform upperArmR, Transform thighL, Transform thighR,
            string clipName)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            return new FishPoseSnapshot
            {
                label = label,
                animatorState = state.IsName(label) ? label : state.fullPathHash.ToString(),
                normalizedTime = state.normalizedTime,
                appearanceLocalEuler = Format(appearance.localEulerAngles),
                armatureLocalEuler = Format(armature.localEulerAngles),
                genericRootLocalEuler = Format(genericRoot.localEulerAngles),
                pelvisLocalEuler = Format(pelvis.localEulerAngles),
                upperArmLLocalEuler = Format(upperArmL.localEulerAngles),
                upperArmRLocalEuler = Format(upperArmR.localEulerAngles),
                thighLLocalEuler = Format(thighL.localEulerAngles),
                thighRLocalEuler = Format(thighR.localEulerAngles),
                clipName = clipName
            };
        }

        private static string Format(Vector3 value)
        {
            return value.x.ToString("F4") + "," + value.y.ToString("F4") + "," + value.z.ToString("F4");
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindByName(child, name);
                if (found != null) return found;
            }

            return null;
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }

        private static string DescribeMovementInputState(Type inputRouterType, Type hudCoordinatorType,
            Type d7CoordinatorType, Type characterType, Type routerType, Component character,
            Component router, Component bridge, Keyboard keyboard)
        {
            var axis = (Vector2)inputRouterType.GetMethod("MoveAxis").Invoke(null, null);
            var blocked = (bool)inputRouterType.GetProperty("GameplayInputBlocked").GetValue(null);
            var primary = (bool)hudCoordinatorType.GetProperty("PrimaryInputAllowed").GetValue(null);
            var gameplay = (bool)d7CoordinatorType.GetProperty("GameplayInputAllowed").GetValue(null);
            var moving = (bool)characterType.GetProperty("IsMoving").GetValue(character);
            var speed = (float)characterType.GetProperty("CurrentSpeed").GetValue(character);
            var source = characterType.GetProperty("MovementInputSource").GetValue(character);
            var legacy = (bool)characterType.GetProperty("LegacyMovementFallbackActive").GetValue(character);
            var mode = routerType.GetProperty("Mode").GetValue(router);
            var candidateState = bridge.GetType().GetProperty("LastCandidateState").GetValue(bridge);
            var adapter = bridge.GetType().GetProperty("Adapter").GetValue(bridge);
            var adapterType = adapter == null ? null : adapter.GetType();
            var adapterState = adapterType == null ? "none" : adapterType.GetProperty("CurrentState").GetValue(adapter).ToString();
            var adapterName = adapterType == null ? "none" : adapterType.GetProperty("LastStateName").GetValue(adapter) as string;
            var adapterFallback = adapterType != null && (bool)adapterType.GetProperty("LastApplyUsedFallback").GetValue(adapter);
            var snapshot = bridge.GetType().GetProperty("LastSnapshot").GetValue(bridge);
            var snapshotType = snapshot.GetType();
            var snapshotSpeed = (float)snapshotType.GetField("speed01").GetValue(snapshot);
            var snapshotGrounded = (bool)snapshotType.GetField("grounded").GetValue(snapshot);
            var snapshotVertical = (float)snapshotType.GetField("verticalSpeed").GetValue(snapshot);
            var lastD2Action = bridge.GetType().GetProperty("LastD2Action").GetValue(bridge) as string;
            var currentKeyboard = Keyboard.current;
            var currentGamepad = Gamepad.current;
            return " [diagnostic: axis=" + axis.ToString("F2") +
                ", blocked=" + blocked +
                ", primary=" + primary +
                ", gameplay=" + gameplay +
                ", timeScale=" + Time.timeScale.ToString("F2") +
                ", focused=" + Application.isFocused +
                ", keyboardIsCurrent=" + (currentKeyboard == keyboard) +
                ", wPressed=" + keyboard.wKey.isPressed +
                ", gamepad=" + (currentGamepad == null ? "none" : currentGamepad.leftStick.ReadValue().ToString("F2")) +
                ", moving=" + moving +
                ", speed=" + speed.ToString("F3") +
                ", source=" + source +
             ", legacy=" + legacy +
             ", routerMode=" + mode +
             ", d2Action=" + lastD2Action +
             ", snapshotSpeed01=" + snapshotSpeed.ToString("F3") +
             ", snapshotGrounded=" + snapshotGrounded +
             ", snapshotVertical=" + snapshotVertical.ToString("F3") +
             ", adapterState=" + adapterState +
             ", adapterName=" + adapterName +
             ", adapterFallback=" + adapterFallback +
             ", candidateState=" + candidateState +
             "]";
        }
    }
}
