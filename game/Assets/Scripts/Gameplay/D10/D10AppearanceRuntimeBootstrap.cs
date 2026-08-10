// D10AppearanceRuntimeBootstrap —— 只接收外观请求并展示系统结果。
// 不在 MonoBehaviour 中修改金币、所有权、装备槽或伤病状态。

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whalebone.Gameplay.D3;
using Whalebone.Systems.D10;
using Whalebone.Systems.D5;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D10
{
    public sealed class D10AppearanceRuntimeBootstrap : MonoBehaviour
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
            if (FindFirstObjectByType<D10AppearanceRuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D10AppearanceRuntimeBootstrap").AddComponent<D10AppearanceRuntimeBootstrap>();
            Debug.Log("[D10] 外观与裁缝运行时入口已创建");
        }

        private D10AppearanceSystem _appearance;
        private D3RuntimeBootstrap _d3;
        private Whalebone.Systems.D3.D3TradeState _boundTrade;
        private bool _ready;
        private D10AppearanceOperationResult _lastResult;

        public bool IsReady { get { return _ready; } }
        public D10AppearanceSnapshot Snapshot { get { return _appearance == null ? null : _appearance.Snapshot; } }
        public D10AppearanceOperationResult LastResult { get { return _lastResult; } }

        private void Start()
        {
            Bind();
        }

        /// <summary>UI/输入层只提交请求；所有状态变更由 D10AppearanceSystem 裁决。</summary>
        public D10AppearanceOperationResult RequestTryOn(string outfitId)
        {
            if (!EnsureReady()) return NotReady("试穿");
            return Record(_appearance.TryOn(outfitId), false);
        }

        public D10AppearanceOperationResult RequestPurchase(string outfitId)
        {
            if (!EnsureReady()) return NotReady("购买");
            return Record(_appearance.Purchase(outfitId), true);
        }

        public D10AppearanceOperationResult RequestEquip(string outfitId)
        {
            if (!EnsureReady()) return NotReady("装备");
            return Record(_appearance.Equip(outfitId), true);
        }

        public D10AppearanceOperationResult RequestDye(string outfitId, string dyeId, string color)
        {
            if (!EnsureReady()) return NotReady("染色");
            return Record(_appearance.Dye(outfitId, dyeId, color), true);
        }

        public D10AppearanceOperationResult RequestRepair(string outfitId)
        {
            if (!EnsureReady()) return NotReady("维修");
            return Record(_appearance.Repair(outfitId), true);
        }

        /// <summary>伤病状态入口供医疗/生存系统调用；普通换装不会触碰该入口。</summary>
        public D10AppearanceOperationResult ApplyInjuryPresentation(IEnumerable<string> injuryIds)
        {
            if (!EnsureReady()) return NotReady("伤病表现");
            return Record(_appearance.ApplyInjuryAppearance(injuryIds), true);
        }

        public void SaveProgress()
        {
            if (!EnsureReady()) return;
            try
            {
                // 先让 D3 保存同一份 EconomySystem 所写的金币，再写入 D10 外观快照。
                if (_d3 != null && _d3.IsReady) _d3.SaveProgress();
                var d5 = SaveSystem.LoadD5Runtime() ?? new D5RuntimeSnapshot();
                d5.Appearance = _appearance.Snapshot;
                SaveSystem.SaveD5Runtime(d5);
                Debug.Log("[D10/Save] 外观所有权、槽位、染色、耐久和伤病历史已保存");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[D10/Save] 外观保存失败：" + ex.Message);
            }
        }

        private void Bind()
        {
            _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            var d5 = SaveSystem.LoadD5Runtime() ?? new D5RuntimeSnapshot();
            _appearance = new D10AppearanceSystem(
                d5.Appearance,
                _d3 == null ? null : _d3.EconomySystem,
                _d3 == null ? null : _d3.TradeState,
                ResourceExists);
            _boundTrade = _d3 == null ? null : _d3.TradeState;
            _ready = true;
            _lastResult = new D10AppearanceOperationResult
            {
                Success = true,
                Code = D10AppearanceResultCode.Success,
                Message = "D10 外观系统已就绪；资源缺失时会显示占位状态。",
            };
        }

        private D10AppearanceOperationResult Record(D10AppearanceOperationResult result, bool save)
        {
            _lastResult = result;
            if (save && result != null && result.Success) SaveProgress();
            return result;
        }

        private bool EnsureReady()
        {
            if (_d3 != null && _d3.TradeState != null && _boundTrade == null)
                Bind();
            if (_ready) return true;
            Bind();
            return _ready;
        }

        private static bool ResourceExists(string assetId)
        {
            if (string.IsNullOrEmpty(assetId)) return false;
            return Resources.Load<GameObject>("D10/Appearance/" + assetId) != null;
        }

        private static D10AppearanceOperationResult NotReady(string operation)
        {
            return D10AppearanceOperationResult.Failed(D10AppearanceResultCode.InvalidContext,
                operation + "请求暂不可用：D10 外观系统尚未绑定。", "");
        }
    }
}
