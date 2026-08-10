// ============================================================
// D9FishingRuntimeBootstrap —— D9-A02 只读捕捞状态展示与系统入口
//
// 本 MonoBehaviour 不保存鱼获结果、不计算数量、不计算价格，也不追加事实。
// 游戏输入/任务若需要捕捞，应调用 FishingSystem 的意图入口；本页只展示
// 既有 EcologySystem、Inventory/Cargo、EconomySystem 和 EventLog 的结果。
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D4;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D9
{
    public sealed class D9FishingRuntimeBootstrap : MonoBehaviour
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
            if (FindFirstObjectByType<D9FishingRuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D9FishingRuntimeBootstrap").AddComponent<D9FishingRuntimeBootstrap>();
        }

        private D3RuntimeBootstrap _d3;
        private D4RuntimeBootstrap _d4;
        private D9FishingSystem _fishing;
        private Text _statusText;
        private float _nextRefresh;

        public bool IsReady { get { return _fishing != null; } }
        public D9FishingSystem FishingSystem { get { return _fishing; } }

        private void Awake()
        {
            BuildUi();
        }

        private void Update()
        {
            BindRuntime();
            if (!IsReady || Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + 0.2f;
            Refresh();
        }

        /// <summary>给航海输入/任务编排层使用的意图入口；结果由 EcologySystem 结算。</summary>
        public FishingResult TryFish()
        {
            if (!IsReady || _d3.Snapshot == null) return new FishingResult { Code = "not_ready", Message = "捕捞系统尚未就绪。" };
            var result = _fishing.Fish(new FishingRequest
            {
                FishingSpotId = D9FishingIds.FishingSpotId,
                GearId = D9FishingIds.GearId,
                Day = _d3.Snapshot.CurrentDay,
                Seed = _d3.Snapshot.VoyageSeed,
            });
            CommitDomainSnapshots();
            Refresh();
            return result;
        }

        public FishProcessingResult ProcessFreshFish(int quantity)
        {
            if (!IsReady) return new FishProcessingResult { Code = "not_ready", Message = "捕捞系统尚未就绪。" };
            var result = _fishing.ProcessFreshFish(quantity);
            CommitDomainSnapshots();
            Refresh();
            return result;
        }

        public FishCookingResult CookFreshFish(string portId, int quantity)
        {
            if (!IsReady) return new FishCookingResult { Code = "not_ready", Message = "捕捞系统尚未就绪。" };
            var result = _fishing.CookFreshFish(portId, quantity);
            CommitDomainSnapshots();
            Refresh();
            return result;
        }

        public TradeResult SellFish(string portId, string itemId, int quantity)
        {
            if (!IsReady) return TradeResult.Fail("not_ready", "捕捞系统尚未就绪。", 0);
            var result = _fishing.SellFish(portId, itemId, quantity);
            CommitDomainSnapshots();
            Refresh();
            return result;
        }

        private void BindRuntime()
        {
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d4 == null) _d4 = FindFirstObjectByType<D4RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_fishing != null || _d3 == null || _d4 == null || !_d3.IsReady || !_d4.IsReady ||
                _d3.TradeState == null || _d4.EcologySystem == null) return;

            _fishing = new D9FishingSystem(_d3.TradeState, _d4.EcologySystem,
                _d3.EconomySystem, _d3.EconomySystem.EventLog);
            if (_fishing.FishSchool == null)
                _fishing.PrepareNearshoreSchool(_d3.Snapshot == null ? 0 : _d3.Snapshot.CurrentDay, 17);
            D9EventLogBridge.Bind(_fishing.EventLog);
            Refresh();
        }

        private void CommitDomainSnapshots()
        {
            if (_d3 != null) _d3.SaveProgress();
            if (_d4 != null) _d4.SaveProgress();
            D9EventLogBridge.Bind(_fishing == null ? null : _fishing.EventLog);
        }

        private void Refresh()
        {
            if (_statusText == null || _fishing == null) return;
            var school = _fishing.FishSchool;
            string position = school == null ? "未建立" : school.WorldX.ToString("0.00") + "," + school.WorldZ.ToString("0.00");
            string last = _fishing.LastCatch == null ? "尚无" : _fishing.LastCatch.Message;
            string cargo = _d3 == null || _d3.TradeState == null ? "未知" :
                D3InventorySystem.CountCargo(_d3.TradeState.Inventory, D9FishingIds.FreshFishItemId) + " 鲜鲭鱼 / " +
                D3InventorySystem.CountCargo(_d3.TradeState.Inventory, D9FishingIds.SaltedFishItemId) + " 盐渍鲭鱼";
            _statusText.text = "D9 捕捞状态（只读）\n" +
                "鱼场 " + D9FishingIds.FishingSpotId + " | 鱼群 " + D9FishingIds.FishSchoolId +
                " | 渔具 " + D9FishingIds.GearId + "\n" +
                "鱼群位置 " + position + " | 剩余生物量 " + (school == null ? 0 : school.Biomass) +
                " | 捕捞压力 " + (school == null ? 0 : school.FishingPressure) + "\n" +
                "货舱 " + cargo + "\n最新捕捞事实 " + last;
        }

        private void BuildUi()
        {
            var canvasObject = new GameObject("D9FishingStatus", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 220;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            var textObject = new GameObject("Status", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(canvasObject.transform, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(28f, 26f);
            rect.sizeDelta = new Vector2(900f, 160f);
            _statusText = textObject.GetComponent<Text>();
            _statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _statusText.fontSize = 16;
            _statusText.alignment = TextAnchor.LowerLeft;
            _statusText.color = new Color(0.75f, 0.93f, 1f, 0.92f);
            _statusText.raycastTarget = false;
        }
    }
}
