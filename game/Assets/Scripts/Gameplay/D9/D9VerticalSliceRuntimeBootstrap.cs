// ============================================================
// D9VerticalSliceRuntimeBootstrap —— D9-S02 共享运行时入口
//
// 本类只负责把同一份 D3 Economy/EventLog、D4 快照和 D9 Systems 组装起来。
// 天气、船员报告、寻宝状态仍由各自 Systems 裁决；本类不计算数量、价格、
// 奖励、关系或船体，也不持有 UI 私有目录。
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Agent;
using Whalebone.Gameplay.D3;
using Whalebone.Systems.D3;
using Whalebone.Systems.D9;
using Whalebone.Systems.Events;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D9
{
    public sealed class D9VerticalSliceRuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D9VerticalSliceRuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D9VerticalSliceRuntimeBootstrap").AddComponent<D9VerticalSliceRuntimeBootstrap>();
        }

        private D3RuntimeBootstrap _d3;
        private EventLog _eventLog;
        private EnvironmentSystem _environment;
        private D9CrewEventSystem _crewEvents;
        private D9CrewAgentVerticalSlice _isaac;
        private D9TreasureQuestSystem _treasure;
        private bool _ready;

        public bool IsReady { get { return _ready; } }
        public EventLog EventLog { get { return _eventLog; } }
        public EnvironmentSystem EnvironmentSystem { get { return _environment; } }
        public D9TreasureQuestSystem TreasureQuestSystem { get { return _treasure; } }
        public D9TreasureQuestSnapshot TreasureSnapshot
        {
            get { return _treasure == null ? null : _treasure.Snapshot; }
        }
        public D9WeatherSeaSnapshot WeatherSnapshot
        {
            get { return _environment == null ? null : _environment.Snapshot; }
        }

        private void Update()
        {
            if (!_ready) TryBindRuntime();
        }

        /// <summary>由航行输入/任务编排层提交风暴意图；天气仍由 EnvironmentSystem 写入。</summary>
        public D9WeatherChangeResult ApplyStorm(int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _environment.ApplyStormEvent(worldDay, D9FishingIds.FishingSpotId,
                saveRevision, _eventLog);
            if (result != null && result.Succeeded)
            {
                _environment.PublishTo(_d3.Snapshot);
                SaveProgress();
            }
            return result;
        }

        /// <summary>把已落盘事实摘要交给 Agent，再由 CrewSystem 记录报告事实。</summary>
        public D9CrewDiscussionResult DiscussCrew(string prompt, int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _crewEvents.Discuss(_isaac, prompt, worldDay, saveRevision);
            if (result.Succeeded) SaveProgress();
            return result;
        }

        public D9TreasureQuestOutcome HearTreasureRumor(string actorRef, int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _treasure.HearRumor(actorRef, worldDay, saveRevision);
            if (result.Accepted) SaveProgress();
            return result;
        }

        public D9TreasureQuestOutcome RecordTreasureClue(D9TreasureClueRequest request,
            int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _treasure.RecordClue(request, worldDay, saveRevision);
            if (result.Accepted) SaveProgress();
            return result;
        }

        public D9TreasureQuestOutcome RecordCatalogTreasureClue(string clueId,
            string actorRef, int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _treasure.RecordCatalogClue(clueId, actorRef, worldDay, saveRevision);
            if (result.Accepted) SaveProgress();
            return result;
        }

        public D9TreasureQuestOutcome CrossValidateTreasureClues(
            D9TreasureClueVerificationRequest request, int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _treasure.CrossValidateClues(request, worldDay, saveRevision);
            if (result.Accepted) SaveProgress();
            return result;
        }

        public D9TreasureQuestOutcome AcceptTreasureRumor()
        {
            if (!TryBindRuntime()) return null;
            var result = _treasure.AcceptRumor();
            if (result.Accepted) SaveProgress();
            return result;
        }

        public D9TreasureQuestOutcome TryLandTreasure(D9TreasureLandingRequest request,
            int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _treasure.TryLand(request, worldDay, saveRevision);
            if (result.Accepted) SaveProgress();
            return result;
        }

        public D9TreasureQuestOutcome RecoverTreasure(D9TreasureRecoveryRequest request,
            int worldDay, long saveRevision)
        {
            if (!TryBindRuntime()) return null;
            var result = _treasure.Recover(request, worldDay, saveRevision);
            if (result.Accepted) SaveProgress();
            return result;
        }

        /// <summary>
        /// 保存同一份 D3 EventLog 与 D9 任务快照。D3 先保存事件事实，D9 再保存任务投影，
        /// 避免读档时出现“任务已完成但事实日志缺失”的半状态。
        /// </summary>
        public void SaveProgress()
        {
            if (!_ready || _d3 == null || _treasure == null) return;
            try
            {
                if (_environment != null) _environment.PublishTo(_d3.Snapshot);
                _d3.SaveProgress();
                SaveSystem.SaveD9Runtime(new D9RuntimeSnapshot
                {
                    TreasureQuest = _treasure.Snapshot,
                });
                D9EventLogBridge.Bind(_eventLog);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[D9/Save] D9 任务快照保存失败，保留上一份有效存档：" + ex.Message);
            }
        }

        private bool TryBindRuntime()
        {
            if (_ready) return true;
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d3 == null || !_d3.IsReady || _d3.EconomySystem == null || _d3.Snapshot == null) return false;

            _eventLog = _d3.EconomySystem.EventLog;
            _environment = _d3.Snapshot.WeatherSea == null
                ? new EnvironmentSystem()
                : new EnvironmentSystem(_d3.Snapshot.WeatherSea);
            if (_d3.Snapshot.WeatherSea == null)
                _environment.RestoreLegacy(_d3.Snapshot.Weather, _d3.Snapshot.CurrentDay,
                    _d3.Snapshot.CurrentLocationId);
            _environment.PublishTo(_d3.Snapshot);

            var savedD9 = SaveSystem.LoadD9Runtime();
            _treasure = new D9TreasureQuestSystem(_eventLog, null,
                savedD9 == null ? null : savedD9.TreasureQuest);
            _crewEvents = new D9CrewEventSystem(_eventLog);
            _isaac = D9CrewAgentVerticalSlice.CreateIsaacReed();
            _ready = true;
            D9EventLogBridge.Bind(_eventLog);
            return true;
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(D9EventLogBridge.Current, _eventLog))
                D9EventLogBridge.Clear();
        }
    }
}
