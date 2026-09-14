using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Whalebone.Core;
using Whalebone.Systems.Save;

namespace WhalesIslandReef.QA.Q03A21
{
    /// <summary>
    /// Q03-A21 QA runner.
    ///
    /// Two jobs, both gated behind -q03a21FormalPlayer:
    ///   1. Walk the MainMenu NewGame path exactly as the Confirm button does
    ///      (SaveSystem.NewGame + GameManager.LoadInitialHarbor) so the Player
    ///      reaches InitialHarbor unattended.
    ///   2. Mount the D20 formal Humanoid presentation on the real D2 physical
    ///      root, so the admitted Q03-A19 action set can actually be seen.
    ///
    /// Scope discipline: QA-only. It does not change the default Player route,
    /// does not edit D2RuntimeBootstrap or D20PlayerCandidatePresentationBridge,
    /// and does not modify Resources. Without the flag the Player behaves
    /// exactly as before, including the MainMenu.
    /// </summary>
    public sealed class Q03A21VisibleFormalPlayerRunner : MonoBehaviour
    {
        private const string ActivationFlag = "-q03a21FormalPlayer";
        private const string AutoQuitFlag = "-q03a21AutoQuit";
        private const string RevertToProceduralFlag = "-d2ProceduralPlayer";

        private static readonly List<string> Log = new List<string>();
        private static bool _activated;
        private static bool _autoQuit;
        private static bool _installAllowed;
        private static bool _bootstrapped;
        private static Q03A21VisibleFormalPlayerRunner _instance;

        private float _deadline;
        private float _newGameRequestedAt;
        private float _quitAt;
        private bool _newGameRequested;
        private bool _installed;
        private bool _reported;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_bootstrapped) return;
            _bootstrapped = true;
            _activated = HasFlag(ActivationFlag);
            _autoQuit = HasFlag(AutoQuitFlag);
            // Q03-A23: D20PlayerCandidateRuntimeInstaller now installs the
            // formal Humanoid route by default, and it honours
            // -d2ProceduralPlayer. This QA runner must never override that
            // decision, so it only installs when the revert flag is absent.
            _installAllowed = !HasFlag(RevertToProceduralFlag);
            if (!_activated) return;

            var host = new GameObject("Q03A21VisibleFormalPlayerRunner");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<Q03A21VisibleFormalPlayerRunner>();
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
            Note("scene_loaded=" + scene.name);
        }

        private void Awake()
        {
            _deadline = Time.realtimeSinceStartup + 60f;
            Note("runner_awake autoQuit=" + _autoQuit);
        }

        private void Update()
        {
            if (_installed)
            {
                if (_autoQuit && _quitAt > 0f && Time.realtimeSinceStartup >= _quitAt)
                {
                    Note("auto_quit");
                    WriteReport("INSTALLED_QUIT");
                    Application.Quit(0);
                }
                return;
            }

            var active = SceneManager.GetActiveScene();
            if (active.name == "MainMenu")
            {
                TryAutoStartNewGame();
                return;
            }

            if (active.name == "InitialHarbor")
            {
                if (_installAllowed) TryInstall();
                else MarkReverted();
            }

            if (Time.realtimeSinceStartup >= _deadline && !_reported)
            {
                _reported = true;
                Note("deadline_reached scene=" + active.name + " newGameRequested=" + _newGameRequested);
                WriteReport("TIMEOUT");
            }
        }

        /// <summary>
        /// Replicates the MainMenu Confirm button callback without touching the
        /// menu UI: SaveSystem.NewGame then GameManager.LoadInitialHarbor.
        /// </summary>
        private void TryAutoStartNewGame()
        {
            if (_newGameRequested)
            {
                // Give the load a moment; if it never lands, try once more.
                if (Time.realtimeSinceStartup - _newGameRequestedAt > 15f)
                    LoadHarbor("retry");
                return;
            }

            if (GameManager.Instance == null) return;
            try
            {
                SaveSystem.NewGame(new NewGameParams
                {
                    CaptainName = "QA-A21",
                    ShipTypeId = "small_fishing",
                });
                Note("newgame_created");
            }
            catch (Exception exception)
            {
                Note("newgame_failed=" + exception.Message);
            }

            LoadHarbor("initial");
        }

        private void LoadHarbor(string phase)
        {
            _newGameRequested = true;
            _newGameRequestedAt = Time.realtimeSinceStartup;
            try
            {
                GameManager.Instance.LoadInitialHarbor();
                Note("load_harbor_requested phase=" + phase);
            }
            catch (Exception exception)
            {
                Note("load_harbor_failed phase=" + phase + " error=" + exception.Message);
            }
        }

        /// <summary>
        /// -d2ProceduralPlayer was supplied: the production installer intentionally
        /// kept the procedural stand-in, so this QA runner stands down instead of
        /// re-installing the D20 route behind its back.
        /// </summary>
        private void MarkReverted()
        {
            if (_installed) return;
            _installed = true;
            _reported = true;
            Note("runner_stood_down reason=-d2ProceduralPlayer");
            WriteReport("REVERTED_TO_PROCEDURAL");
            if (_autoQuit) _quitAt = Time.realtimeSinceStartup + 10f;
        }

        private void TryInstall()
        {
            var bootstrap = Object.FindFirstObjectByType<Whalebone.Gameplay.D2.D2RuntimeBootstrap>(
                FindObjectsInactive.Include);
            if (bootstrap == null || !bootstrap.IsReady) return;

            var player = GameObject.Find("D2_PlayerAvatar");
            if (player == null) return;

            Note("d2_root_found name=" + player.name);
            var bridge = Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge
                .InstallForFormalHumanoid(player);

            if (bridge != null && bridge.IsInstalled)
            {
                _installed = true;
                _reported = true;
                Note("bridge_installed mode=" + bridge.ConsumerModeName
                     + " humanoidPoseAdapter=" + bridge.UsesHumanoidPoseAdapter
                     + " genericPathAdapter=" + bridge.UsesGenericPathAdapter
                     + " swimmingSupported=" + bridge.SwimmingVisualSupported
                     + " hiddenRenderers=" + bridge.HiddenExistingRendererCount
                     + " candidateRenderers=" + bridge.CandidateRendererCount);
                WriteReport("INSTALLED");
                if (_autoQuit) _quitAt = Time.realtimeSinceStartup + 20f;
            }
        }

        private static void Note(string message)
        {
            Log.Add(Time.realtimeSinceStartup.ToString("F2") + "s " + message);
            Debug.Log("[Q03A21] " + message);
        }

        private static void WriteReport(string status)
        {
            try
            {
                // A built Player cannot resolve Application.dataPath to the
                // project checkout, so the machine-readable report goes to the
                // persistent data path and the full trace stays in Player.log.
                var dir = Path.Combine(Application.persistentDataPath, "Q03A21");
                Directory.CreateDirectory(dir);
                File.WriteAllLines(
                    Path.Combine(dir, "Q03A21_VISIBLE_FORMAL_PLAYER_RUNTIME.log"),
                    new[] { "status=" + status }.Concat(Log));
                Debug.Log("[Q03A21] report=" + Path.Combine(dir, "Q03A21_VISIBLE_FORMAL_PLAYER_RUNTIME.log"));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Q03A21] report write failed: " + exception.Message);
            }
        }
    }
}
