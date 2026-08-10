// ============================================================
// D3RuntimeBootstrap —— D3 可玩环的 Unity 表现层接线
//
// 该类只把输入/UI 意图传给 Systems.D3，并把系统快照映射到 D2 场景对象。
// 价格、货舱、采集、事件和航程数值不在 MonoBehaviour 内裁决。
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.D7;
using Whalebone.Systems.D3;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D3
{
    public sealed class D3RuntimeBootstrap : MonoBehaviour
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
            if (FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D3RuntimeBootstrap").AddComponent<D3RuntimeBootstrap>();
            Debug.Log("[D3] InitialHarbor 贸易与航程运行时入口已创建");
        }

        private readonly TradeEconomySystem _economy = new TradeEconomySystem();
        private readonly GatheringSystem _gathering = new GatheringSystem();
        private VoyageSettlementSystem _voyageSystem;

        private Transform _player;
        private Transform _ship;
        private D3RuntimeSnapshot _snapshot;
        private D3TradeState _trade;
        private VoyageState _voyage;
        private bool _ready;
        private bool _tradeOpen;
        private int _destinationIndex;
        private int _commodityIndex;

        private Text _statusText;
        private Text _hintText;
        private Text _tradeTitle;
        private Text _tradeInfo;
        private Text _tradeResult;
        private GameObject _tradePanel;
        private GameObject _controlPanel;
        private string _lastMessage = "D3 已就绪。";

        public bool IsReady => _ready;
        public bool IsTradeOpen => _tradeOpen;
        public D3RuntimeSnapshot Snapshot => _snapshot;
        /// <summary>D9 整备页提交意图的 D3 owner 状态；调用方不得直接改 Player/Ship。</summary>
        public D3TradeState TradeState => _trade;
        /// <summary>D9-A02 使用与 D3 交易页相同的 EconomySystem 实例。</summary>
        public TradeEconomySystem EconomySystem => _economy;

        /// <summary>D9 离港按钮的表现层入口，航线仍由 D3 Voyage owner 裁决。</summary>
        public bool RequestDeparture()
        {
            if (!_ready) return false;
            StartVoyage();
            return _snapshot != null && _snapshot.VoyageStatus == D3VoyageStatus.Sailing;
        }

        /// <summary>把 D9 船舶整备 owner 修改的船体/补给提交给 D3 快照后保存。</summary>
        public void CommitPreparationState()
        {
            if (!_ready || _snapshot == null || _trade == null) return;
            var ship = GameState.CurrentShip;
            if (ship != null)
            {
                _snapshot.Hull = ship.Hull;
                _snapshot.HullMax = ship.HullMax;
                _snapshot.Water = ReadSupply(ship, "water", _snapshot.Water);
                _snapshot.Food = ReadSupply(ship, "food", _snapshot.Food);
            }
            SaveProgress();
        }

        private void Awake()
        {
            _voyageSystem = new VoyageSettlementSystem(null, _economy.EventLog);
            EnsureEventSystem();
            BuildUi();
            CreatePortMarkers();
        }

        private void Update()
        {
            if (!_ready)
            {
                TryBuildRuntime();
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (_tradeOpen)
            {
                if (keyboard.escapeKey.wasPressedThisFrame) CloseTrade();
                return;
            }
            if (!D7ControlCoordinator.GameplayInputAllowed) return;
            if (Time.timeScale <= 0f) return;

            if (keyboard.bKey.wasPressedThisFrame) OpenTrade();
            else if (keyboard.tKey.wasPressedThisFrame) StartVoyage();
            else if (keyboard.nKey.wasPressedThisFrame) AdvanceVoyage();
            else if (keyboard.gKey.wasPressedThisFrame) GatherOne();
            else if (keyboard.lKey.wasPressedThisFrame) LandAndSettle();
            else if (keyboard.cKey.wasPressedThisFrame) SaveProgress();
            RefreshHud();
        }

        public void SaveProgress()
        {
            if (!_ready || _snapshot == null || _trade == null) return;
            SyncSnapshotFromRuntime();
            SyncPlayerLocationFromRuntime();
            try
            {
                long checkpointRevision = Math.Max(_snapshot.SaveRevision, _economy.EventLog.NextSequence + 1);
                SaveSystem.AppendD9SaveCheckpoint(_economy.EventLog, _snapshot.CurrentDay,
                    _snapshot.CurrentLocationId, checkpointRevision);
                _snapshot.EventLog = SaveSystem.CaptureD3TradeEventLog(_economy.EventLog);
                SaveSystem.SaveD3Runtime(_snapshot);
                Debug.Log("[D3/Save] 贸易、库存和航程快照已保存");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[D3/Save] 快照保存失败，保留上一次有效存档：" + ex.Message);
            }
        }

        /// <summary>
        /// D2 快照先保存船的位置；当 D3 已明确处于港内时，D3 的港口状态拥有
        /// 位置类型的最终写权，避免返回主菜单后继续游戏被错误恢复到海上。
        /// </summary>
        private void SyncPlayerLocationFromRuntime()
        {
            if (_snapshot == null || _snapshot.VoyageStatus != D3VoyageStatus.InPort ||
                string.IsNullOrEmpty(_snapshot.CurrentLocationId)) return;

            var player = GameState.CurrentPlayer;
            if (player == null) return;
            var location = player.Location ?? new LocationRef();
            location.Type = "port";
            location.PortId = _snapshot.CurrentLocationId;
            location.Speed = 0f;
            player.Location = location;
        }

        private void TryBuildRuntime()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            if (player == null || ship == null) return;

            _player = player.transform;
            _ship = ship.transform;
            BuildState();
            _ready = true;
            ApplyLocationVisual();
            RefreshHud();
            Debug.Log("[D3] 可玩环已接线：港口、贸易、采集、航程和结算");
        }

        private void BuildState()
        {
            var player = Whalebone.Systems.Save.GameState.CurrentPlayer;
            var ship = Whalebone.Systems.Save.GameState.CurrentShip;
            var shipType = ship == null ? null : ShipTypeSeed.Find(ship.ShipTypeId);
            int capacity = shipType == null ? 40 : shipType.CargoMax;
            var saved = SaveSystem.LoadD3Runtime();

            if (saved != null && !string.IsNullOrEmpty(saved.CurrentLocationId))
                _snapshot = saved;
            else
                _snapshot = BuildInitialSnapshot(player, ship);

            _trade = new D3TradeState(_snapshot.Money <= 0 && player != null ? player.Money : _snapshot.Money, capacity);
            Copy(_snapshot.Cargo, _trade.Inventory.Cargo);
            Copy(_snapshot.PersonalItems, _trade.Inventory.PersonalItems);
            Copy(_snapshot.MarketStock, _trade.Market.Stock);
            Copy(_snapshot.MarketDemand, _trade.Market.Demand);
            Copy(_snapshot.PlayerTradeVolume, _trade.Market.PlayerTradeVolume);
            _trade.Market.WorldDay = _snapshot.CurrentDay;
            _trade.Market.Weather = _snapshot.Weather;
            _trade.Market.FactionPolicyId = string.IsNullOrEmpty(_snapshot.FactionPolicyId)
                ? "neutral" : _snapshot.FactionPolicyId;
            _trade.Market.FactionPolicyCoefficient = _snapshot.FactionPolicyCoefficient;
            if (_trade.Market.Stock.Count == 0) CommodityCatalog.SeedMarket(_trade.Market);
            if (_snapshot.EventLog != null)
                _economy.RestoreEventLog(SaveSystem.RestoreD3TradeEventLog().CaptureSnapshot());

            if (_snapshot.VoyageStatus == D3VoyageStatus.Sailing)
            {
                _voyage = ToVoyageState(_snapshot);
                _destinationIndex = IndexOfPort(_snapshot.DestinationId);
            }
            else
            {
                _voyage = null;
                _destinationIndex = NextDestinationIndex(_snapshot.CurrentLocationId);
            }
            SyncSnapshotFromRuntime();
        }

        private static D3RuntimeSnapshot BuildInitialSnapshot(PlayerState player, ShipState ship)
        {
            var result = new D3RuntimeSnapshot
            {
                CurrentLocationId = player == null || player.Location == null || string.IsNullOrEmpty(player.Location.PortId)
                    ? PortCatalog.StartingPortId : player.Location.PortId,
                VoyageStatus = D3VoyageStatus.InPort,
                Weather = D3WeatherType.Calm,
                Money = player == null ? PlayerState.StartingMoney : player.Money,
                Water = ship == null ? ShipState.StartingWater : ReadSupply(ship, "water", ShipState.StartingWater),
                Food = ship == null ? ShipState.StartingFood : ReadSupply(ship, "food", ShipState.StartingFood),
                Hull = ship == null ? 60 : ship.Hull,
                HullMax = ship == null ? 60 : ship.HullMax,
            };
            if (player != null) Copy(player.Inventory, result.PersonalItems);
            if (ship != null) Copy(ship.Cargo, result.Cargo);
            return result;
        }

        private void StartVoyage()
        {
            if (_snapshot.VoyageStatus != D3VoyageStatus.InPort)
            {
                SetResult("当前不是可出航状态。");
                return;
            }

            var current = PortCatalog.Find(_snapshot.CurrentLocationId);
            var destination = PortCatalog.All[_destinationIndex];
            if (current == null || destination.Id == current.Id)
            {
                _destinationIndex = NextDestinationIndex(_snapshot.CurrentLocationId);
                destination = PortCatalog.All[_destinationIndex];
            }

            var shipType = Whalebone.Systems.Save.GameState.CurrentShip == null
                ? null : ShipTypeSeed.Find(Whalebone.Systems.Save.GameState.CurrentShip.ShipTypeId);
            float speed = shipType == null ? 4f : shipType.Speed;
            int seed = _snapshot.VoyageSeed + 97 + _snapshot.CurrentDay * 13;
            if (!_voyageSystem.TryBegin(current.Id, destination.Id, speed, seed,
                _snapshot.Water, _snapshot.Food, _snapshot.Hull, _snapshot.HullMax,
                out _voyage, out string error))
            {
                SetResult("无法出航：" + error);
                return;
            }

            _snapshot.DestinationId = destination.Id;
            _snapshot.VoyageStatus = D3VoyageStatus.Sailing;
            _snapshot.Weather = _voyage.Weather;
            _snapshot.TotalDays = _voyage.TotalDays;
            _snapshot.VoyageSeed = seed;
            SetResult("已规划航线：" + current.DisplayName + " → " + destination.DisplayName + "，按 N 推进航程。");
            SaveProgress();
        }

        private void AdvanceVoyage()
        {
            if (_voyage == null)
            {
                SetResult("没有进行中的航程。");
                return;
            }

            var result = _voyageSystem.AdvanceDay(_voyage);
            _snapshot.CurrentDay = _voyage.CurrentDay;
            _snapshot.Water = _voyage.Water;
            _snapshot.Food = _voyage.Food;
            _snapshot.Hull = _voyage.Hull;
            _snapshot.EventIds = new List<string>(_voyage.EventIds);
            if (result.MoneyDelta != 0) SetResult(result.Message + " 金币变化：" + result.MoneyDelta);
            else SetResult(result.Message);

            if (!result.Succeeded)
            {
                _snapshot.VoyageStatus = D3VoyageStatus.Failed;
                SaveProgress();
                return;
            }
            if (result.Arrived)
            {
                _snapshot.VoyageStatus = D3VoyageStatus.Arrived;
                SetResult("已抵达目的地，按 L 靠港并结算。");
            }
            SaveProgress();
        }

        private void LandAndSettle()
        {
            if (_voyage == null || _voyage.Status != D3VoyageStatus.Arrived)
            {
                SetResult("尚未抵达目的地，不能靠港结算。");
                return;
            }

            var settlement = _voyageSystem.Settle(_voyage);
            if (!settlement.Succeeded)
            {
                SetResult(settlement.Message);
                return;
            }

            _trade.Money = Math.Max(0, _trade.Money + settlement.MoneyDelta);
            _snapshot.CurrentLocationId = settlement.DestinationId;
            _snapshot.DestinationId = null;
            _snapshot.VoyageStatus = D3VoyageStatus.InPort;
            _snapshot.GatheredThisVisit.Clear();
            _snapshot.CurrentDay = 0;
            _snapshot.TotalDays = 0;
            _voyage = null;
            _destinationIndex = NextDestinationIndex(_snapshot.CurrentLocationId);
            ApplyLocationVisual();
            SetResult("已靠港并完成结算：" + PortCatalog.Find(_snapshot.CurrentLocationId).DisplayName);
            SaveProgress();
        }

        private void GatherOne()
        {
            if (_snapshot.VoyageStatus != D3VoyageStatus.InPort)
            {
                SetResult("必须先靠港/登岛才能采集。");
                return;
            }
            var location = PortCatalog.Find(_snapshot.CurrentLocationId);
            if (location == null)
            {
                SetResult("当前位置未知。");
                return;
            }
            foreach (var resource in location.GatheringLimits)
            {
                int already = _snapshot.GatheredThisVisit.TryGetValue(resource.Key, out int count) ? count : 0;
                if (already >= resource.Value) continue;
                var result = _gathering.Gather(_trade.Inventory, _snapshot.GatheredThisVisit,
                    location.Id, resource.Key, 1);
                SetResult(result.Message + " 货舱：" + result.CargoUsed + "/" + _trade.Inventory.CargoCapacity);
                SyncSnapshotFromRuntime();
                SaveProgress();
                return;
            }
            SetResult("这里暂时没有可采集资源，或本次采集已经达到上限。");
        }

        private void OpenTrade()
        {
            if (_snapshot.VoyageStatus != D3VoyageStatus.InPort)
            {
                SetResult("航行中不能打开港口市场。");
                return;
            }
            var port = PortCatalog.Find(_snapshot.CurrentLocationId);
            if (port == null || port.Type == D3LocationType.DesertedIsland)
            {
                SetResult("荒岛没有正式市场，请先返回村落或城镇港。");
                return;
            }
            _tradeOpen = true;
            _controlPanel.SetActive(false);
            _tradePanel.SetActive(true);
            RuntimeHudCoordinator.ShowTemporaryCanvas("D3Canvas");
            RefreshTradePanel();
        }

        private void CloseTrade()
        {
            _tradeOpen = false;
            _tradePanel.SetActive(false);
            _controlPanel.SetActive(true);
            RuntimeHudCoordinator.RestorePrimaryHud();
            RefreshHud();
        }

        /// <summary>D7 输入协调器使用的模态关闭入口，避免 ESC 同帧穿透到暂停或航行。</summary>
        public void CloseTradeFromCoordinator()
        {
            if (_tradeOpen) CloseTrade();
        }

        private void BuyOne()
        {
            var item = CommodityCatalog.All[_commodityIndex];
            var result = _economy.Buy(_trade, _snapshot.CurrentLocationId, item.Id, 1);
            SetTradeResult(result.Message + (result.Succeeded ? " 花费 " + result.TotalPrice + " 金币。" : ""));
            SyncSnapshotFromRuntime();
            SaveProgress();
            RefreshTradePanel();
        }

        private void SellOne()
        {
            var item = CommodityCatalog.All[_commodityIndex];
            var result = _economy.Sell(_trade, _snapshot.CurrentLocationId, item.Id, 1);
            SetTradeResult(result.Message + (result.Succeeded ? " 获得 " + result.TotalPrice + " 金币。" : ""));
            SyncSnapshotFromRuntime();
            SaveProgress();
            RefreshTradePanel();
        }

        private void NextCommodity()
        {
            _commodityIndex = (_commodityIndex + 1) % CommodityCatalog.All.Count;
            RefreshTradePanel();
        }

        private void PreviousCommodity()
        {
            _commodityIndex = (_commodityIndex - 1 + CommodityCatalog.All.Count) % CommodityCatalog.All.Count;
            RefreshTradePanel();
        }

        private void CycleDestination()
        {
            _destinationIndex = NextDestinationIndex(PortCatalog.All[_destinationIndex].Id);
            SetResult("下一目的地：" + PortCatalog.All[_destinationIndex].DisplayName);
        }

        private void SyncSnapshotFromRuntime()
        {
            if (_snapshot == null || _trade == null) return;
            _snapshot.Money = _trade.Money;
            _snapshot.Cargo = CopyOf(_trade.Inventory.Cargo);
            _snapshot.PersonalItems = CopyOf(_trade.Inventory.PersonalItems);
            _snapshot.MarketStock = CopyOf(_trade.Market.Stock);
            _snapshot.MarketDemand = CopyOf(_trade.Market.Demand);
            _snapshot.PlayerTradeVolume = CopyOf(_trade.Market.PlayerTradeVolume);
            _snapshot.FactionPolicyId = _trade.Market.FactionPolicyId;
            _snapshot.FactionPolicyCoefficient = _trade.Market.FactionPolicyCoefficient;
            _trade.Market.WorldDay = _snapshot.CurrentDay;
            _snapshot.SaveRevision = _economy.CurrentSaveRevision;
            _snapshot.EventLog = SaveSystem.CaptureD3TradeEventLog(_economy.EventLog);
            if (_voyage != null)
            {
                _snapshot.Water = _voyage.Water;
                _snapshot.Food = _voyage.Food;
                _snapshot.Hull = _voyage.Hull;
                _snapshot.CurrentDay = _voyage.CurrentDay;
                _snapshot.TotalDays = _voyage.TotalDays;
                _snapshot.EventIds = new List<string>(_voyage.EventIds);
            }

            var player = Whalebone.Systems.Save.GameState.CurrentPlayer;
            var ship = Whalebone.Systems.Save.GameState.CurrentShip;
            if (player != null)
            {
                player.Money = _snapshot.Money;
                player.Inventory.Clear();
                Copy(_snapshot.PersonalItems, player.Inventory);
                var port = PortCatalog.Find(_snapshot.CurrentLocationId);
                if (port != null && _snapshot.VoyageStatus != D3VoyageStatus.Sailing)
                {
                    player.Location.Type = "port";
                    player.Location.PortId = port.Id;
                    player.Location.X = port.WorldX;
                    player.Location.Y = port.WorldZ;
                }
                else
                {
                    player.Location.Type = "open_sea";
                    player.Location.PortId = null;
                }
            }
            if (ship != null)
            {
                ship.Cargo.Clear();
                Copy(_snapshot.Cargo, ship.Cargo);
                ship.Hull = _snapshot.Hull;
                ship.Supplies["water"] = _snapshot.Water;
                ship.Supplies["food"] = _snapshot.Food;
            }
            Whalebone.Systems.Save.GameState.CurrentD3Runtime = _snapshot.Clone();
        }

        private void ApplyLocationVisual()
        {
            var port = PortCatalog.Find(_snapshot.CurrentLocationId);
            if (port == null || _ship == null || _player == null) return;
            _ship.position = new Vector3(port.WorldX, _ship.position.y, port.WorldZ);
            _player.position = _ship.position - _ship.forward * 5f + Vector3.up;
        }

        private void RefreshHud()
        {
            if (!_ready || _statusText == null) return;
            RefreshButtonLabels();
            var port = PortCatalog.Find(_snapshot.CurrentLocationId);
            string location = port == null ? "未知海域" : port.DisplayName;
            string voyage = _snapshot.VoyageStatus == D3VoyageStatus.Sailing
                ? "航行中 " + _snapshot.CurrentDay + "/" + _snapshot.TotalDays + " → " +
                  (PortCatalog.Find(_snapshot.DestinationId)?.DisplayName ?? "未知")
                : _snapshot.VoyageStatus.ToString();
            _statusText.text = "D3 可玩环\n位置：" + location + " | 金币：" + _snapshot.Money +
                " | 货舱：" + D3InventorySystem.UsedCargo(_trade.Inventory) + "/" + _trade.Inventory.CargoCapacity +
                "\n补给 水 " + _snapshot.Water + " / 食物 " + _snapshot.Food + " | 船体 " + _snapshot.Hull + "/" + _snapshot.HullMax +
                "\n状态：" + voyage + "\n提示：" + _lastMessage;
            _hintText.text = "WASD 移动 | 右键/中键调整视角 | B 贸易 | T 出航 | N 推进一天 | G 采集 | L 靠港结算 | C 保存 | ESC 暂停";
        }

        private void RefreshButtonLabels()
        {
            SetButtonLabel(_controlPanel, "Trade", "贸易 [B]", "Trade [B]");
            SetButtonLabel(_controlPanel, "Destination", "切换目的地", "Change destination");
            SetButtonLabel(_controlPanel, "Voyage", "出航 [T]", "Set sail [T]");
            SetButtonLabel(_controlPanel, "Advance", "推进航程 [N]", "Advance voyage [N]");
            SetButtonLabel(_controlPanel, "Gather", "登岛采集 [G]", "Gather [G]");
            SetButtonLabel(_controlPanel, "Land", "靠港结算 [L]", "Land and settle [L]");
            SetButtonLabel(_tradePanel, "Previous", "上一种", "Previous");
            SetButtonLabel(_tradePanel, "Buy", "买入 1 件", "Buy 1");
            SetButtonLabel(_tradePanel, "Sell", "卖出 1 件", "Sell 1");
            SetButtonLabel(_tradePanel, "Next", "下一种", "Next");
            SetButtonLabel(_tradePanel, "Close", "关闭贸易", "Close trade");
        }

        private static void SetButtonLabel(GameObject parent, string buttonName, string chinese, string english)
        {
            if (parent == null) return;
            var button = parent.transform.Find(buttonName);
            if (button == null) return;
            var label = button.Find("Label");
            var text = label == null ? null : label.GetComponent<Text>();
            if (text != null) text.text = Localization.IsEnglish ? english : chinese;
        }

        private void RefreshTradePanel()
        {
            var port = PortCatalog.Find(_snapshot.CurrentLocationId);
            var item = CommodityCatalog.All[_commodityIndex];
            var quote = _economy.Quote(_snapshot.CurrentLocationId, item.Id, _trade.Market);
            int stock = _trade.Market.Stock.TryGetValue(item.Id, out int amount) ? amount : 0;
            string reasons = quote == null || quote.PriceReasons == null ? "暂无价格原因。" :
                string.Join("；", quote.PriceReasons.ConvertAll(reason => reason.Explanation).ToArray());
            _tradeTitle.text = "贸易：" + (port == null ? "未知港口" : port.DisplayName);
            _tradeInfo.text = item.DisplayName + "  | 货舱 " + D3InventorySystem.CountCargo(_trade.Inventory, item.Id) +
                " | 市场库存 " + stock + "\n买入 " + (quote == null ? 0 : quote.BuyUnitPrice) +
                " 金币/件 | 卖出 " + (quote == null ? 0 : quote.SellUnitPrice) + " 金币/件\n当前金币：" + _trade.Money +
                "\n价格原因：" + reasons + (quote != null && quote.IsOutOfStock ? "\n状态：缺货" : "");
        }

        private void SetResult(string message)
        {
            _lastMessage = message ?? string.Empty;
            if (_tradeResult != null) _tradeResult.text = message;
            RefreshHud();
            Debug.Log("[D3] " + message);
        }

        private void SetTradeResult(string message)
        {
            _lastMessage = message ?? string.Empty;
            if (_tradeResult != null) _tradeResult.text = message;
            RefreshHud();
            Debug.Log("[D3/Trade] " + message);
        }

        private static VoyageState ToVoyageState(D3RuntimeSnapshot snapshot)
        {
            var state = new VoyageState
            {
                OriginId = snapshot.CurrentLocationId,
                DestinationId = snapshot.DestinationId,
                Status = snapshot.VoyageStatus,
                Weather = snapshot.Weather,
                CurrentDay = snapshot.CurrentDay,
                TotalDays = snapshot.TotalDays,
                Seed = snapshot.VoyageSeed,
                Water = snapshot.Water,
                Food = snapshot.Food,
                Hull = snapshot.Hull,
                HullMax = snapshot.HullMax,
            };
            state.EventIds.AddRange(snapshot.EventIds);
            return state;
        }

        private static int ReadSupply(ShipState ship, string key, int fallback)
        {
            return ship.Supplies != null && ship.Supplies.TryGetValue(key, out int value) ? value : fallback;
        }

        private static int IndexOfPort(string id)
        {
            for (int i = 0; i < PortCatalog.All.Count; i++)
                if (PortCatalog.All[i].Id == id) return i;
            return 0;
        }

        private static int NextDestinationIndex(string currentId)
        {
            int current = IndexOfPort(currentId);
            return (current + 1) % PortCatalog.All.Count;
        }

        private static Dictionary<string, int> CopyOf(Dictionary<string, int> source)
        {
            var result = new Dictionary<string, int>();
            Copy(source, result);
            return result;
        }

        private static void Copy(IDictionary<string, int> source, IDictionary<string, int> target)
        {
            if (source == null || target == null) return;
            foreach (var item in source) target[item.Key] = item.Value;
        }

        private void CreatePortMarkers()
        {
            foreach (var port in PortCatalog.All)
            {
                string name = "D3_Port_" + port.Id;
                if (GameObject.Find(name) != null) continue;
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = name;
                marker.transform.position = new Vector3(port.WorldX, 0.35f, port.WorldZ);
                marker.transform.localScale = new Vector3(1.2f, 0.35f, 1.2f);
                ApplyColor(marker, port.Type == D3LocationType.Town
                    ? new Color(0.85f, 0.65f, 0.2f)
                    : port.Type == D3LocationType.Village
                        ? new Color(0.25f, 0.75f, 0.35f)
                        : new Color(0.65f, 0.48f, 0.3f));
            }
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("D3Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 130;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _statusText = CreateText(canvasGo, "D3Status", "D3 正在等待场景对象…", 22, new Vector2(28f, -28f),
                new Vector2(720f, 150f), TextAnchor.UpperLeft);
            SetTopLeft(_statusText.rectTransform, new Vector2(28f, -28f));
            _hintText = CreateText(canvasGo, "D3Hint", "", 18, new Vector2(28f, 24f),
                new Vector2(1500f, 42f), TextAnchor.MiddleLeft);
            SetBottomLeft(_hintText.rectTransform, new Vector2(28f, 22f));

            _controlPanel = new GameObject("D3Controls", typeof(RectTransform));
            _controlPanel.transform.SetParent(canvasGo.transform, false);
            var controlRect = (RectTransform)_controlPanel.transform;
            controlRect.anchorMin = new Vector2(1f, 0f);
            controlRect.anchorMax = new Vector2(1f, 0f);
            controlRect.pivot = new Vector2(1f, 0f);
            controlRect.sizeDelta = new Vector2(420f, 300f);
            controlRect.anchoredPosition = new Vector2(-28f, 70f);
            CreateButton(_controlPanel, "Trade", "贸易 [B]", new Vector2(0f, 230f), OpenTrade);
            CreateButton(_controlPanel, "Destination", "切换目的地", new Vector2(0f, 155f), CycleDestination);
            CreateButton(_controlPanel, "Voyage", "出航 [T]", new Vector2(0f, 80f), StartVoyage);
            CreateButton(_controlPanel, "Advance", "推进航程 [N]", new Vector2(0f, 5f), AdvanceVoyage);
            CreateButton(_controlPanel, "Gather", "登岛采集 [G]", new Vector2(0f, -70f), GatherOne);
            CreateButton(_controlPanel, "Land", "靠港结算 [L]", new Vector2(0f, -145f), LandAndSettle);

            _tradePanel = new GameObject("D3TradePanel", typeof(RectTransform), typeof(Image));
            _tradePanel.transform.SetParent(canvasGo.transform, false);
            Stretch((RectTransform)_tradePanel.transform);
            _tradePanel.GetComponent<Image>().sprite = UiSprites.White;
            _tradePanel.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.07f, 0.96f);
            _tradeTitle = CreateText(_tradePanel, "Title", "贸易", 42, new Vector2(0f, 300f),
                new Vector2(900f, 70f), TextAnchor.MiddleCenter);
            _tradeInfo = CreateText(_tradePanel, "Info", "", 26, new Vector2(0f, 170f),
                new Vector2(1100f, 130f), TextAnchor.MiddleCenter);
            _tradeResult = CreateText(_tradePanel, "Result", "", 22, new Vector2(0f, -230f),
                new Vector2(1200f, 64f), TextAnchor.MiddleCenter);
            CreateButton(_tradePanel, "Previous", "上一种", new Vector2(-390f, -60f), PreviousCommodity);
            CreateButton(_tradePanel, "Buy", "买入 1 件", new Vector2(-130f, -60f), BuyOne);
            CreateButton(_tradePanel, "Sell", "卖出 1 件", new Vector2(130f, -60f), SellOne);
            CreateButton(_tradePanel, "Next", "下一种", new Vector2(390f, -60f), NextCommodity);
            CreateButton(_tradePanel, "Close", "关闭贸易", new Vector2(0f, -330f), CloseTrade);
            _tradePanel.SetActive(false);
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 position,
            UnityEngine.Events.UnityAction onClick)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)buttonGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 58f);
            rect.anchoredPosition = position;
            var image = buttonGo.GetComponent<Image>();
            image.sprite = UiSprites.White;
            image.color = new Color(0.12f, 0.34f, 0.52f, 0.98f);
            var button = buttonGo.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            var text = CreateText(buttonGo, "Label", label, 24, Vector2.zero,
                new Vector2(340f, 50f), TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(GameObject parent, string name, string content, int size,
            Vector2 position, Vector2 sizeDelta, TextAnchor alignment)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)textGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = position;
            var text = textGo.GetComponent<Text>();
            text.font = Fonts.Default;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = content;
            text.raycastTarget = false;
            return text;
        }

        private static void SetTopLeft(RectTransform rect, Vector2 position)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
        }

        private static void SetBottomLeft(RectTransform rect, Vector2 position)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
            }
            foreach (var other in FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (other != eventSystem) other.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(true);
            if (EventSystem.current != eventSystem) EventSystem.current = eventSystem;
            var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null) inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            inputModule.enabled = true;
            inputModule.AssignDefaultActions();
            foreach (var module in eventSystem.GetComponents<BaseInputModule>())
                if (module != inputModule) module.enabled = false;
            eventSystem.UpdateModules();
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return;
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            renderer.sharedMaterial = material;
        }
    }
}
