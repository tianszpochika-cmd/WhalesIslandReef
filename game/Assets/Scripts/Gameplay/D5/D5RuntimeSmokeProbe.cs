#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.D5
{
    /// <summary>Editor-only D5 runtime gate，验证海图、12 个岛标记、UI 和角色对象。</summary>
    internal sealed class D5RuntimeSmokeProbe : MonoBehaviour
    {
        private float _deadline;
        private bool _finished;
        private bool _boardingChecked;
        private bool _boardingPassed;
        private bool _physicsChecked;
        private bool _physicsPassed;
        private bool _jumpChecked;
        private bool _jumpPassed;
        private bool _animationStatePassed;
        private bool _poseChecked;
        private bool _posePassed;
        private bool _cameraChecked;
        private bool _cameraPassed;
        private bool _actionQualityPassed;
        private bool _d7Passed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Application.isBatchMode || !HasArgument("-d5Smoke")) return;
            // 正式 Player 的首场景是 MainMenu。D5 冒烟探针只属于
            // InitialHarbor；若在主菜单启动就创建探针，会等待港口对象超时，
            // 把正常的主菜单启动误报为失败。
            if (!string.Equals(SceneManager.GetActiveScene().name, "InitialHarbor",
                System.StringComparison.Ordinal)) return;
            new GameObject("D5RuntimeSmokeProbe").AddComponent<D5RuntimeSmokeProbe>();
        }

        private void Start() { _deadline = Time.realtimeSinceStartup + 20f; }

        private void Update()
        {
            if (_finished) return;
            bool bootstrap = FindFirstObjectByType<D5RuntimeBootstrap>(FindObjectsInactive.Include) != null;
            bool canvas = GameObject.Find("D5Canvas") != null;
            bool map = GameObject.Find("D5WorldMap") != null;
            var d5Canvas = GameObject.Find("D5Canvas");
            var discoverLabel = d5Canvas == null ? null : d5Canvas.transform.Find("D5Controls/D5Discover/Label");
            var discoverText = discoverLabel == null ? null : discoverLabel.GetComponent<UnityEngine.UI.Text>();
            bool d5ButtonTextDoesNotBlock = discoverText != null && !discoverText.raycastTarget;
            var d3Canvas = GameObject.Find("D3Canvas");
            var d4Canvas = GameObject.Find("D4Canvas");
            bool legacyHudDoesNotBlock = IsHiddenCanvas(d3Canvas) && IsHiddenCanvas(d4Canvas);
            int markerCount = 0;
            foreach (var island in Whalebone.Systems.D5.D5IslandCatalog.All)
                if (GameObject.Find("D5IslandMarker_" + island.Id) != null) markerCount++;
            var playerObject = GameObject.Find("D2_PlayerAvatar");
            bool player = playerObject != null;
            bool playerHasCharacterVisual = HasCharacterVisual(playerObject);
            var characterAnimation = playerObject == null
                ? null
                : playerObject.GetComponent<Whalebone.Gameplay.D2.D2CharacterAnimationController>();
            var formalAnimator = playerObject == null
                ? null
                : playerObject.GetComponent<Animator>();
            bool characterAnimationReady = characterAnimation != null;
            bool formalAnimatorReady = characterAnimation != null && characterAnimation.FormalAnimatorReady &&
                formalAnimator != null && formalAnimator.runtimeAnimatorController != null &&
                HasFormalActionClips(formalAnimator.runtimeAnimatorController);
            var camera = Camera.main;
            if (camera == null) camera = FindFirstObjectByType<Camera>();
            var cameraFollow = camera == null ? null : camera.GetComponent<Whalebone.Gameplay.CameraFollow>();
            bool cameraCollisionReady = cameraFollow != null && cameraFollow.CollisionCheckCount > 0 &&
                !float.IsNaN(cameraFollow.ResolvedDistance) && !float.IsInfinity(cameraFollow.ResolvedDistance);
            bool cameraReady = cameraFollow != null && cameraFollow.Target != null && cameraFollow.ViewModeCount >= 3 &&
                cameraCollisionReady;
            var harborPresentation = FindFirstObjectByType<Whalebone.Gameplay.Harbor.HarborPresentationController>(FindObjectsInactive.Include);
            bool harborQualityReady = harborPresentation != null && harborPresentation.IsInitialized && harborPresentation.FoamBandCount >= 4;
            var harborCollision = FindFirstObjectByType<Whalebone.Gameplay.Harbor.HarborCollisionController>(FindObjectsInactive.Include);
            bool harborCollisionReady = harborCollision != null && harborCollision.IsInitialized &&
                harborCollision.WalkableSurfaceCount >= 3 && harborCollision.ColliderCount >= 10;
            var tutorial = FindFirstObjectByType<Whalebone.Gameplay.Harbor.HarborTutorialController>(FindObjectsInactive.Include);
            bool tutorialReady = tutorial != null;
            var interaction = FindFirstObjectByType<Whalebone.Gameplay.UI.HarborInteractionStabilizer>(FindObjectsInactive.Include);
            bool interactionReady = interaction != null;
            var d7 = FindFirstObjectByType<Whalebone.Gameplay.D7.D7ControlCoordinator>(FindObjectsInactive.Include);
            var d7Deck = FindFirstObjectByType<Whalebone.Gameplay.D7.D7ShipDeckController>(FindObjectsInactive.Include);
            var d7Presentation = playerObject == null ? null :
                playerObject.GetComponent<Whalebone.Gameplay.D7.D7CharacterPresentationController>();
            var d7Audio = FindFirstObjectByType<Whalebone.Gameplay.D7.D7AudioController>(FindObjectsInactive.Include);
            var d7Materials = FindFirstObjectByType<Whalebone.Gameplay.D7.D7MaterialPolishController>(FindObjectsInactive.Include);
            var d7Status = FindFirstObjectByType<Whalebone.Gameplay.D7.D7RuntimeStatusHud>(FindObjectsInactive.Include);
            bool d7ControlReady = d7 != null && d7.IsReady && d7.ReconcileCount > 0 &&
                d7.Snapshot.GameplayInputAllowed;
            bool d7PresentationReady = d7Presentation != null && d7Presentation.IsReady &&
                d7Presentation.HasHumanVisual;
            bool d7DeckReady = d7Deck != null && d7Deck.IsReady && d7Deck.HelmReady &&
                d7Deck.DeckCollisionReady;
            bool d7AudioReady = d7Audio != null && d7Audio.IsReady;
            bool d7MaterialsReady = d7Materials != null && d7Materials.IsReady &&
                d7Materials.PolishedRendererCount > 0;
            bool d7StatusReady = d7Status != null && d7Status.IsReady;
            var router = FindFirstObjectByType<Whalebone.Gameplay.D2.D2ControlRouter>(FindObjectsInactive.Include);
            var shipObject = GameObject.Find("ph_ship_fishing");
            if (!_boardingChecked && d7ControlReady && d7DeckReady &&
                router != null && playerObject != null && shipObject != null)
            {
                _boardingChecked = true;
                Debug.Log("[D5 Smoke] Boarding attempt: player=" + playerObject.transform.position.ToString("F2") +
                          ", ship=" + shipObject.transform.position.ToString("F2") +
                          ", inRange=" + d7Deck.IsWithinBoardingRange(playerObject.transform, 8f));
                bool entered = router.TryToggleShip();
                bool followsShip = entered && router.Mode == Whalebone.Systems.D2.D2ControlMode.Ship &&
                    playerObject.transform.parent == shipObject.transform;
                bool exited = router.TryToggleShip();
                bool leavesShip = exited && router.Mode == Whalebone.Systems.D2.D2ControlMode.Character &&
                    playerObject.transform.parent == null;
                _boardingPassed = followsShip && leavesShip;
            }
            var characterController = playerObject == null
                ? null
                : playerObject.GetComponent<Whalebone.Gameplay.D2.D2CharacterController>();
            if (!_physicsChecked && _boardingPassed && characterController != null)
            {
                _physicsChecked = true;
                Debug.Log("[D5 Smoke] Physics check: collisionLayer=" + harborCollisionReady +
                          ", body=" + characterController.HasPhysicsBody +
                          ", collision=" + characterController.CollisionEnabled +
                          ", grounded=" + characterController.IsGrounded +
                          ", playerY=" + playerObject.transform.position.y.ToString("F2"));
                _physicsPassed = harborCollisionReady && characterController.HasPhysicsBody &&
                    characterController.CollisionEnabled && characterController.IsGrounded;
            }
            if (!_jumpChecked && _boardingPassed && characterController != null &&
                characterAnimation != null && !characterAnimation.IsTransitioning)
            {
                _jumpChecked = true;
                _jumpPassed = characterController.TryJump();
            }
            if (formalAnimatorReady && characterAnimation != null &&
                characterAnimation.Action == Whalebone.Gameplay.D2.D2CharacterAction.Jump &&
                formalAnimator.isActiveAndEnabled &&
                formalAnimator.GetCurrentAnimatorStateInfo(0).IsName("D6_Jump"))
                _animationStatePassed = true;
            if (!_poseChecked && formalAnimatorReady && characterAnimation != null &&
                characterAnimation.NeutralPoseSampled)
            {
                _poseChecked = true;
                _posePassed = characterAnimation.NeutralPoseError <= 8f;
                Debug.Log("[D5 Smoke] Pose check: neutralSampled=" + characterAnimation.NeutralPoseSampled +
                          ", lowerBodyError=" + characterAnimation.NeutralPoseError.ToString("F1") + "°");
            }
            if (!_cameraChecked && cameraReady)
            {
                _cameraChecked = true;
                var initialMode = cameraFollow.ViewMode;
                cameraFollow.CycleViewMode();
                var firstMode = cameraFollow.ViewMode;
                cameraFollow.CycleViewMode();
                var secondMode = cameraFollow.ViewMode;
                cameraFollow.SetViewMode(initialMode);
                _cameraPassed = initialMode != firstMode && firstMode != secondMode &&
                    secondMode != initialMode && cameraFollow.ViewMode == initialMode;
                Debug.Log("[D5 Smoke] Camera check: modes=" + cameraFollow.ViewModeCount +
                          ", cycle=" + _cameraPassed + ", collisionChecks=" + cameraFollow.CollisionCheckCount +
                          ", obstructed=" + cameraFollow.IsObstructed + ", distance=" + cameraFollow.ResolvedDistance.ToString("F2") +
                          ", restored=" + cameraFollow.ViewModeLabel);
            }
            if (!_actionQualityPassed && characterAnimation != null &&
                characterAnimation.ProceduralPoseLayerReady && characterAnimation.ActionTransitionCount > 0)
            {
                _actionQualityPassed = true;
                Debug.Log("[D5 Smoke] Action quality check: proceduralPoseLayer=" + characterAnimation.ProceduralPoseLayerReady +
                          ", transitions=" + characterAnimation.ActionTransitionCount);
            }
            if (!_d7Passed && d7ControlReady && d7PresentationReady && d7DeckReady &&
                d7AudioReady && d7MaterialsReady && d7StatusReady && d7Deck.TransitionCount >= 2)
            {
                _d7Passed = true;
                Debug.Log("[D7 Smoke] Quality check: control=" + d7.AuthorityLabel +
                          ", modal=" + d7.ModalLabel + ", deckTransitions=" + d7Deck.TransitionCount +
                          ", visualGroundError=" + d7Presentation.GroundError.ToString("F3") +
                          ", audioFallback=" + d7Audio.UsesFallbackClips +
                          ", polishedRenderers=" + d7Materials.PolishedRendererCount);
            }
            if (bootstrap && canvas && map && markerCount == 12 && player && playerHasCharacterVisual && characterAnimationReady &&
                formalAnimatorReady && harborQualityReady && tutorialReady && interactionReady &&
                harborCollisionReady && _boardingPassed && _physicsPassed && _jumpPassed &&
                _animationStatePassed && _posePassed && _cameraPassed && _actionQualityPassed && _d7Passed &&
                d5ButtonTextDoesNotBlock && legacyHudDoesNotBlock)
            {
                Debug.Log("[D5 Smoke] InitialHarbor verified: primary HUD, clickable D5 button text, hidden legacy HUDs, world map, 12 island markers, character player, formal animator clips/states, smooth action pose layer, neutral pose baseline, camera view modes, harbor quality, collision/grounding, interaction stabilizer, tutorial, boarding, jump, D7 control/deck/presentation/audio/material/status");
                Finish(0);
                return;
            }
            if (Time.realtimeSinceStartup >= _deadline)
            {
                Debug.LogError("[D5 Smoke] runtime objects not ready: bootstrap=" + bootstrap + ", canvas=" + canvas + ", map=" + map + ", markers=" + markerCount + ", player=" + player + ", playerHasCharacterVisual=" + playerHasCharacterVisual + ", characterAnimationReady=" + characterAnimationReady + ", formalAnimatorReady=" + formalAnimatorReady + ", harborQualityReady=" + harborQualityReady + ", harborCollisionReady=" + harborCollisionReady + ", physics=" + _physicsPassed + ", tutorialReady=" + tutorialReady + ", interactionReady=" + interactionReady + ", boarding=" + _boardingPassed + ", jump=" + _jumpPassed + ", animationState=" + _animationStatePassed + ", pose=" + _posePassed + ", camera=" + _cameraPassed + ", actionQuality=" + _actionQualityPassed + ", d7=" + _d7Passed + ", d7Control=" + d7ControlReady + ", d7Presentation=" + d7PresentationReady + ", d7Deck=" + d7DeckReady + ", d7Audio=" + d7AudioReady + ", d7Materials=" + d7MaterialsReady + ", d7Status=" + d7StatusReady + ", d5ButtonTextDoesNotBlock=" + d5ButtonTextDoesNotBlock + ", legacyHudDoesNotBlock=" + legacyHudDoesNotBlock);
                Finish(1);
            }
        }

        private void Finish(int code)
        {
            _finished = true;
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.Exit(code);
#else
            Application.Quit(code);
#endif
        }

        private static bool HasArgument(string argument)
        {
            foreach (var value in System.Environment.GetCommandLineArgs()) if (value == argument) return true;
            return false;
        }

        private static bool IsHiddenCanvas(GameObject canvas)
        {
            if (canvas == null) return false;
            var group = canvas.GetComponent<CanvasGroup>();
            return group != null && !group.blocksRaycasts && group.alpha <= 0.01f;
        }

        private static bool HasCharacterVisual(GameObject player)
        {
            if (player == null) return false;
            if (player.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0) return true;
            foreach (var renderer in player.GetComponentsInChildren<MeshRenderer>(true))
                if (renderer.transform != player.transform) return true;
            return false;
        }

        private static bool HasFormalActionClips(RuntimeAnimatorController controller)
        {
            if (controller == null) return false;
            string[] required = { "D6Crew_Idle", "D6Crew_Walk", "D6Crew_Run", "D6Crew_Jump", "D6Crew_Boarding", "D6Crew_Disembarking" };
            var clips = controller.animationClips;
            for (int i = 0; i < required.Length; i++)
            {
                bool found = false;
                for (int j = 0; j < clips.Length; j++)
                {
                    if (clips[j] != null && clips[j].name == required[i])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }
            return true;
        }
    }
}
