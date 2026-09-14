// ============================================================
// D4RuntimeBootstrap —— D4 内容层的 Unity 表现接线
//
// 该类只收集键盘/UI 意图、调用 Systems.D4，并显示强逻辑快照。
// 战斗、声望、动物、疾病和主线数值不在 MonoBehaviour 内计算。
// ============================================================

using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Whalebone.Gameplay.UI;
using Whalebone.Gameplay.D3;
using Whalebone.Gameplay.D7;
using Whalebone.Gameplay.UI.D16;
using Whalebone.Systems.D4;
using Whalebone.Systems.D16;
using Whalebone.Systems.Save;

namespace Whalebone.Gameplay.D4
{
    public sealed class D4RuntimeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneBootstrap()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name != "InitialHarbor") return;
            if (FindFirstObjectByType<D4RuntimeBootstrap>(FindObjectsInactive.Include) != null) return;
            new GameObject("D4RuntimeBootstrap").AddComponent<D4RuntimeBootstrap>();
            Debug.Log("[D4] 内容扩充运行时入口已创建");
        }

        private D4RuntimeSnapshot _snapshot;
        private D4FactionSystem _factions;
        private D4ShipyardSystem _shipyard;
        private D4CombatSystem _combat;
        private D4EcologySystem _ecology;
        private D4SurvivalSystem _medical;
        private D4MainlineSystem _mainline;
        private D4LogSystem _log;
        private Transform _player;
        private Transform _ship;
        private D3RuntimeBootstrap _d3;
        private bool _ready;
        private string _message = "D4 内容层正在等待 D2 场景对象…";

        private Text _statusText;
        private Text _detailText;
        private Text _hintText;
        private GameObject _canvasObject;

        private void Awake()
        {
            EnsureEventSystem();
            BuildUi();
        }

        private void Update()
        {
            if (!_ready || _factions == null || _shipyard == null || _combat == null || _ecology == null || _medical == null || _mainline == null || _log == null)
            {
                _ready = false;
                TryBuildRuntime();
                return;
            }

            if (IsContentPageVisible() && Input.GetKeyDown(KeyCode.Escape))
            {
                D7ControlCoordinator.MarkEscapeHandledThisFrame();
                CloseContentPage();
                return;
            }

            // D4 is itself a temporary page. PrimaryInputAllowed is false while
            // any temporary page is open, so using it here would block D4's own
            // keyboard owner. Visibility is the page-local focus gate; D29/D2/
            // D3/D5 remain responsible for yielding to this page.
            if (!IsContentPageVisible() || Time.timeScale <= 0f ||
                !D7ControlCoordinator.GameplayInputAllowed) return;
            if (WasD4Input(D16InputAction.D4Faction, KeyCode.F)) FactionAction();
            else if (WasD4Input(D16InputAction.D4StartCombat, KeyCode.K)) StartCombat();
            else if (WasD4Input(D16InputAction.D4Reload, KeyCode.R)) Reload();
            else if (WasD4Input(D16InputAction.D4SelectRoundShot, KeyCode.Alpha1)) SelectWeapon(D4WeaponId.RoundShot);
            else if (WasD4Input(D16InputAction.D4SelectChainShot, KeyCode.Alpha2)) SelectWeapon(D4WeaponId.ChainShot);
            else if (WasD4Input(D16InputAction.D4SelectGrapeShot, KeyCode.Alpha3)) SelectWeapon(D4WeaponId.GrapeShot);
            else if (WasD4Input(D16InputAction.D4Fire, KeyCode.Space)) Fire();
            else if (WasD4Input(D16InputAction.D4Board, KeyCode.B)) Board();
            else if (WasD4Input(D16InputAction.D4EscapeCombat, KeyCode.E)) EscapeCombat();
            else if (WasD4Input(D16InputAction.D4Upgrade, KeyCode.U)) UpgradeHull();
            else if (WasD4Input(D16InputAction.D4ObserveAnimal, KeyCode.A)) ObserveAnimal();
            else if (WasD4Input(D16InputAction.D4TreatCrew, KeyCode.M)) TreatCrew();
            else if (WasD4Input(D16InputAction.D4AdvanceMainline, KeyCode.J)) AdvanceMainline();
            else if (WasD4Input(D16InputAction.Save, KeyCode.C)) SaveProgress();
            RefreshHud();
        }

        private bool EnsureReadyForAction()
        {
            if (_ready) return true;
            _message = "D4 内容 owner 尚未就绪，请稍后再试。";
            if (_hintText != null) _hintText.text = _message;
            Debug.LogWarning("[D4/UI] " + _message);
            return false;
        }

        public void FactionAction()
        {
            if (!EnsureReadyForAction()) return;
            var result = _factions.ApplyReputation("england", 10, "完成护航教学，英格兰声望提高。");
            AddLog(D4LogKind.Faction, result.Message);
            _message = result.Message;
            RefreshHud();
        }

        public void StartCombat()
        {
            if (!EnsureReadyForAction()) return;
            var ship = GameState.CurrentShip;
            var definition = D4ShipCatalog.Find(_shipyard.Snapshot.ShipTypeId) ?? D4ShipCatalog.Find("small_fishing");
            var modifiers = _shipyard.GetModifiers();
            int hullMax = (ship == null ? definition.HullMax : Math.Max(ship.HullMax, 1)) + modifiers.HullBonus;
            int hull = ship == null ? hullMax : Math.Min(hullMax, Math.Max(0, ship.Hull));
            int cannons = Math.Max(1, (ship == null ? definition.CannonMax : ship.Cannons) + modifiers.CannonBonus);
            int crew = Math.Max(1, GameState.CrewAgents.Count);
            _combat.BeginEncounter("ph_spanish_patrol", hull, hullMax, cannons, crew, 95, 95, 4, 8, 34f, 0.2f);
            AddLog(D4LogKind.Combat, "发现西班牙巡逻船，炮战开始。");
            _message = "炮战开始：R 装填，1/2/3 选弹，Space 开炮，B 接舷，E 脱离。";
            RefreshHud();
        }

        public void Reload()
        {
            if (!EnsureReadyForAction()) return;
            int loaded = _combat.Reload(_combat.Snapshot.PlayerCannons);
            _message = loaded > 0 ? "炮组已装填 " + loaded + " 门。" : "当前没有可装填的炮。";
            RefreshHud();
        }

        public void SelectWeapon(D4WeaponId weapon)
        {
            if (!EnsureReadyForAction()) return;
            _message = _combat.SelectWeapon(weapon) ? "已选择弹种：" + WeaponName(weapon) : "开炮前请先完成当前装填。";
            RefreshHud();
        }

        public void Fire()
        {
            if (!EnsureReadyForAction()) return;
            var result = _combat.Fire(3, true);
            if (!result.Accepted)
            {
                _message = result.Message;
                RefreshHud();
                return;
            }
            if (GameState.CurrentShip != null && result.PlayerDamage > 0)
                GameState.CurrentShip.Hull = Math.Max(0, GameState.CurrentShip.Hull - result.PlayerDamage);
            _message = result.Message + (result.PlayerDamage > 0 ? " 我方受损 " + result.PlayerDamage + "。" : "");
            if (result.Status == D4CombatStatus.Victory)
            {
                _factions.RecordPrivateerPrize("england", "spain", _factions.HasPrivateerPermit("england"));
                _mainline.SetFlag("learn_combat");
                AddLog(D4LogKind.Combat, "炮战胜利，俘获巡逻船战利品。");
            }
            else AddLog(D4LogKind.Combat, _message);
            RefreshHud();
        }

        public void Board()
        {
            if (!EnsureReadyForAction()) return;
            var result = _combat.Board();
            _message = result.Message;
            if (result.Status == D4CombatStatus.Victory)
            {
                _mainline.SetFlag("learn_combat");
                AddLog(D4LogKind.Combat, "接舷战胜利，敌船已被控制。");
            }
            RefreshHud();
        }

        public void EscapeCombat()
        {
            if (!EnsureReadyForAction()) return;
            var result = _combat.Escape();
            _message = result.Message;
            AddLog(D4LogKind.Combat, result.Message);
            RefreshHud();
        }

        /// <summary>D9 船坞页面读取既有 D4 船坞裁决器，不复制改装公式。</summary>
        public bool IsReady => _ready;
        /// <summary>D29 显示 D4 owner 的最后一条可见反馈；只读。</summary>
        public string LastMessage => _message;
        /// <summary>D29 确认临时船坞页是否真的可见；只读。</summary>
        public bool HasVisibleContentPage => IsContentPageVisible();
        /// <summary>D9 表现层读取 D4 内容快照；不暴露可写引用。</summary>
        public D4RuntimeSnapshot Snapshot => _snapshot == null ? null : _snapshot.Clone();
        public D4ShipyardSystem ShipyardSystem => _shipyard;
        /// <summary>D9-A02 通过既有 EcologySystem 写鱼群位置、生物量和压力。</summary>
        public D4EcologySystem EcologySystem => _ecology;

        /// <summary>
        /// 尝试执行一次船体改装，并把 owner 的接受/拒绝结果返回给外层交互层。
        /// 所有金额、等级和资源变化仍由 D4ShipyardSystem 裁决。
        /// </summary>
        public bool TryUpgradeHull()
        {
            if (!EnsureReadyForAction()) return false;
            if (GameState.CurrentPlayer == null)
            {
                _message = "当前没有可改装的船长状态。";
                if (_hintText != null) _hintText.text = _message;
                Debug.LogWarning("[D4/UI] " + _message);
                return false;
            }
            var result = _shipyard.TryUpgrade(D4UpgradeSlot.Hull, ref GameState.CurrentPlayer.Money,
                _factions.GetReputation("england"));
            _message = result.Message;
            if (result.Accepted) AddLog(D4LogKind.Faction, "船坞完成船体改装，等级 " + result.NewLevel + "。");
            RefreshHud();
            return result.Accepted;
        }

        /// <summary>键盘和 D4 自有按钮的兼容入口；结果由 TryUpgradeHull 返回给需要它的调用方。</summary>
        public void UpgradeHull()
        {
            TryUpgradeHull();
        }

        /// <summary>D4 内容页的唯一关闭入口，避免右侧页面永久遮挡玩家 HUD。</summary>
        public void CloseContentPage()
        {
            RuntimeHudCoordinator.RestorePrimaryHud();
            Debug.Log("[D4/UI] 内容页已关闭，主 HUD 已恢复");
        }

        public void ObserveAnimal()
        {
            if (!EnsureReadyForAction()) return;
            if (_snapshot.Ecology.Animals.Count == 0) _ecology.SeedHabitat("jungle", CurrentDay(), 17);
            AnimalState target = null;
            foreach (var animal in _ecology.Snapshot.Animals)
                if (!animal.Observed) { target = animal; break; }
            if (target == null)
            {
                _message = "本地动物都已记录。";
                RefreshHud();
                return;
            }
            var observed = _ecology.Observe(target.AnimalId);
            var collected = _ecology.Collect(target.AnimalId);
            _mainline.SetFlag("learn_island");
            _message = observed.Message + " " + collected.Message;
            AddLog(D4LogKind.Animal, _message);
            RefreshHud();
        }

        public void TreatCrew()
        {
            if (!EnsureReadyForAction()) return;
            if (GameState.CrewAgents.Count == 0)
            {
                _message = "当前没有可治疗的船员。";
                if (_hintText != null) _hintText.text = _message;
                Debug.LogWarning("[D4/UI] " + _message);
                RefreshHud();
                return;
            }
            string crewId = GameState.CrewAgents[0].Id;
            _medical.EnsureCrew(crewId);
            var member = FindHealth(crewId);
            if (member == null) return;
            if (member.Diseases.Count == 0) _medical.Infect(crewId, "scurvy");
            var result = _medical.Treat(crewId, "scurvy", 2);
            _mainline.SetFlag("learn_dialogue");
            _message = result.Message;
            AddLog(D4LogKind.Medical, _message);
            RefreshHud();
        }

        public void AdvanceMainline()
        {
            if (!EnsureReadyForAction()) return;
            var mission = _mainline.CurrentMission;
            if (mission != null)
            {
                if (mission.RequiredFlag == "learn_sailing") _mainline.SetFlag("learn_sailing");
                if (mission.RequiredFlag == "learn_trade") _mainline.SetFlag("learn_trade");
                if (mission.RequiredFlag == "learn_dialogue" && GameState.CrewAgents.Count > 0) _mainline.SetFlag("learn_dialogue");
                if (mission.RequiredFlag == "learn_island" && _ecology.Snapshot.CollectionCount > 0) _mainline.SetFlag("learn_island");
                if (mission.RequiredFlag == "learn_combat" && (_combat.Snapshot.Status == D4CombatStatus.Victory || _combat.Snapshot.Status == D4CombatStatus.Escaped)) _mainline.SetFlag("learn_combat");
                if (mission.RequiredFlag == "privateer_contact" && _factions.GetReputation("england") >= 10) _mainline.SetFlag("privateer_contact");
                if (mission.RequiredFlag == "break_blockade") _mainline.SetFlag("break_blockade");
                if (mission.RequiredFlag == "final_route") _mainline.SetFlag("final_route");
            }
            var result = _mainline.TryAdvance(_factions.Snapshot);
            _message = result.Message;
            if (result.Advanced) AddLog(D4LogKind.Mainline, result.Message);
            RefreshHud();
        }

        public void SaveProgress()
        {
            if (!EnsureReadyForAction()) return;
            if (_snapshot == null)
            {
                _message = "D4 快照尚未生成，暂不能保存。";
                if (_hintText != null) _hintText.text = _message;
                Debug.LogWarning("[D4/Save] " + _message);
                return;
            }
            SyncSnapshot();
            try
            {
                SaveSystem.SaveD4Runtime(_snapshot);
                _message = "D4 内容状态已保存。";
            }
            catch (Exception ex)
            {
                _message = "D4 保存失败：" + ex.Message;
                Debug.LogWarning("[D4/Save] " + ex.Message);
            }
            RefreshHud();
        }

        private void TryBuildRuntime()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            if (player == null || ship == null) return;
            if (_d3 == null) _d3 = FindFirstObjectByType<D3RuntimeBootstrap>(FindObjectsInactive.Include);
            if (_d3 == null || !_d3.IsReady || _d3.EconomySystem == null) return;
            _player = player.transform;
            _ship = ship.transform;
            _snapshot = SaveSystem.LoadD4Runtime() ?? new D4RuntimeSnapshot();
            _factions = new D4FactionSystem(_snapshot.Factions);
            _shipyard = new D4ShipyardSystem(_snapshot.Shipyard);
            _combat = new D4CombatSystem(_snapshot.Combat, _d3.EconomySystem.EventLog);
            _ecology = new D4EcologySystem(_snapshot.Ecology);
            _medical = new D4SurvivalSystem(_snapshot.Health);
            _mainline = new D4MainlineSystem(_snapshot.Mainline);
            _log = new D4LogSystem(_snapshot.RouteLog);

            if (_snapshot.Ecology.Animals.Count == 0) _ecology.SeedHabitat("jungle", CurrentDay(), 17);
            if (_snapshot.Health.Crew.Count == 0)
            {
                var ids = new string[GameState.CrewAgents.Count];
                for (int i = 0; i < GameState.CrewAgents.Count; i++) ids[i] = GameState.CrewAgents[i].Id;
                _medical.CreateInitial(ids, 8);
            }
            _ready = true;
            SyncSnapshot();
            RefreshHud();
            Debug.Log("[D4] 内容层已接线：势力、船坞、炮战、动物、医疗、主线和航线日志");
        }

        private void SyncSnapshot()
        {
            if (_snapshot == null) return;
            _snapshot.Factions = _factions == null ? _snapshot.Factions : _factions.Snapshot;
            _snapshot.Shipyard = _shipyard == null ? _snapshot.Shipyard : _shipyard.Snapshot;
            _snapshot.Combat = _combat == null ? _snapshot.Combat : _combat.Snapshot;
            _snapshot.Ecology = _ecology == null ? _snapshot.Ecology : _ecology.Snapshot;
            _snapshot.Health = _medical == null ? _snapshot.Health : _medical.Snapshot;
            _snapshot.Mainline = _mainline == null ? _snapshot.Mainline : _mainline.Snapshot;
            _snapshot.RouteLog = _log == null ? _snapshot.RouteLog : _log.CloneEntries();
        }

        private void AddLog(D4LogKind kind, string text)
        {
            if (_log != null) _log.Append(CurrentDay(), kind, text);
            SyncSnapshot();
        }

        private int CurrentDay()
        {
            return GameState.CurrentPlayer == null || GameState.CurrentPlayer.Location == null
                ? 0 : GameState.CurrentPlayer.Location.DayIndex;
        }

        private CrewHealthState FindHealth(string crewId)
        {
            foreach (var member in _medical.Snapshot.Crew) if (member.CrewId == crewId) return member;
            return null;
        }

        private void RefreshHud()
        {
            if (!_ready || _statusText == null) return;
            RefreshButtonLabels();
            SyncSnapshot();
            var current = _mainline.CurrentMission;
            _statusText.text = "D4 内容层\n" +
                "金币 " + (GameState.CurrentPlayer == null ? 0 : GameState.CurrentPlayer.Money) +
                "  | 英格兰声望 " + _factions.GetReputation("england") +
                "  | 通缉 " + _snapshot.Factions.WantedLevel +
                "  | 船体 " + _snapshot.Combat.PlayerHull + "/" + _snapshot.Combat.PlayerHullMax;
            var detail = new StringBuilder();
            detail.Append("势力：英/西/法/葡/海盗/部落/走私  | 私掠证：");
            detail.Append(_factions.HasPrivateerPermit("england") ? "有" : "无");
            detail.Append("\n炮战：").Append(_snapshot.Combat.Status).Append("  敌船耐久 ").Append(_snapshot.Combat.EnemyHull);
            detail.Append("  弹种 ").Append(WeaponName(_snapshot.Combat.Weapon)).Append("  已装填 ").Append(_snapshot.Combat.LoadedCount);
            detail.Append("\n动物记录：").Append(_snapshot.Ecology.Animals.Count).Append("  收藏额度：").Append(_snapshot.Ecology.CollectionCount);
            detail.Append("  医疗物资：").Append(_snapshot.Health.MedicalSupplies);
            detail.Append("\n主线：").Append(current == null ? "三章已完成" : "第" + current.Chapter + "章 · " + current.Title);
            detail.Append("\n最新日志：").Append(_log.BuildSummary(2));
            _detailText.text = detail.ToString();
            _hintText.text = _message + "    F势力  K炮战  R装填  1/2/3选弹  Space开炮  B接舷  E脱离  U改装  A观察  M医疗  J主线  C保存";
        }

        private void RefreshButtonLabels()
        {
            SetButtonLabel("Faction", "势力任务 [F]", "Faction task [F]");
            SetButtonLabel("Combat", "开始炮战 [K]", "Start combat [K]");
            SetButtonLabel("Reload", "装填炮弹 [R]", "Reload [R]");
            SetButtonLabel("Fire", "开炮 [Space]", "Fire [Space]");
            SetButtonLabel("Upgrade", "船体改装 [U]", "Upgrade hull [U]");
            SetButtonLabel("Animal", "观察动物 [A]", "Observe animals [A]");
            SetButtonLabel("Medical", "医疗 [M]", "Medical [M]");
            SetButtonLabel("Mainline", "推进主线 [J]", "Advance mainline [J]");
        }

        private static void SetButtonLabel(string buttonName, string chinese, string english)
        {
            var canvas = GameObject.Find("D4Canvas");
            if (canvas == null) return;
            var button = canvas.transform.Find("D4Controls/" + buttonName);
            if (button == null) return;
            var label = button.transform.Find("Label");
            var text = label == null ? null : label.GetComponent<Text>();
            if (text != null) text.text = Localization.IsEnglish ? english : chinese;
        }

        private static bool WasD4Input(D16InputAction action, KeyCode fallback)
        {
            // D16 is the configurable primary route. The legacy edge is kept as
            // a Player compatibility fallback for builds where the serialized
            // accessibility binding uses a keyboard name that the Input System
            // cannot resolve after a fresh install. This still emits only the
            // same D4 intent and is guarded by the page/input-focus checks above.
            return D16InputRouter.WasPressedThisFrame(action) || Input.GetKeyDown(fallback);
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("D4Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasObject = canvasGo;
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;
            // Temporary content must never flash over the primary HUD during
            // bootstrap. RuntimeHudCoordinator enables it only for D4Canvas.
            canvas.enabled = false;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var summary = new GameObject("D4Summary", typeof(RectTransform), typeof(Image));
            summary.transform.SetParent(canvasGo.transform, false);
            var summaryRect = (RectTransform)summary.transform;
            summaryRect.anchorMin = new Vector2(0f, 1f); summaryRect.anchorMax = new Vector2(0f, 1f);
            summaryRect.pivot = new Vector2(0f, 1f); summaryRect.sizeDelta = new Vector2(860f, 290f);
            // The scene's harbor title occupies the top-center safe zone.
            // Keep the long D4 summary below it while the content page is open.
            summaryRect.anchoredPosition = new Vector2(28f, -196f);
            var summaryImage = summary.GetComponent<Image>();
            summaryImage.sprite = UiSprites.White;
            summaryImage.color = new Color(0.02f, 0.05f, 0.08f, 0.84f);
            _statusText = CreateText(summary, "D4Status", "D4 正在等待场景对象…", 22,
                Vector2.zero, new Vector2(820f, 58f), TextAnchor.UpperLeft);
            SetTopLeft(_statusText.rectTransform, new Vector2(20f, -18f));
            _detailText = CreateText(summary, "D4Detail", "", 18,
                Vector2.zero, new Vector2(820f, 202f), TextAnchor.UpperLeft);
            SetTopLeft(_detailText.rectTransform, new Vector2(20f, -78f));

            var hintBand = new GameObject("D4HintBand", typeof(RectTransform), typeof(Image));
            hintBand.transform.SetParent(canvasGo.transform, false);
            var hintRect = (RectTransform)hintBand.transform;
            hintRect.anchorMin = new Vector2(0f, 0f); hintRect.anchorMax = new Vector2(0f, 0f);
            hintRect.pivot = new Vector2(0f, 0f); hintRect.sizeDelta = new Vector2(1800f, 52f);
            hintRect.anchoredPosition = new Vector2(28f, 20f);
            var hintImage = hintBand.GetComponent<Image>();
            hintImage.sprite = UiSprites.White;
            hintImage.color = new Color(0.02f, 0.05f, 0.08f, 0.82f);
            _hintText = CreateText(hintBand, "D4Hint", "", 17,
                Vector2.zero, new Vector2(1760f, 48f), TextAnchor.MiddleLeft);
            Stretch(_hintText.rectTransform);

            var panel = new GameObject("D4Controls", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(1f, 1f); panelRect.anchorMax = new Vector2(1f, 1f); panelRect.pivot = new Vector2(1f, 1f);
            panelRect.sizeDelta = new Vector2(360f, 590f); panelRect.anchoredPosition = new Vector2(-28f, -120f);
            panel.GetComponent<Image>().sprite = UiSprites.White; panel.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.08f, 0.82f);
            var firstContentButton = CreateButton(panel, "Faction", "势力任务 [F]", new Vector2(0f, -35f), FactionAction);
            CreateButton(panel, "Combat", "开始炮战 [K]", new Vector2(0f, -100f), StartCombat);
            CreateButton(panel, "Reload", "装填炮弹 [R]", new Vector2(0f, -165f), Reload);
            CreateButton(panel, "Fire", "开炮 [Space]", new Vector2(0f, -230f), Fire);
            CreateButton(panel, "Upgrade", "船体改装 [U]", new Vector2(0f, -295f), UpgradeHull);
            CreateButton(panel, "Animal", "观察动物 [A]", new Vector2(0f, -360f), ObserveAnimal);
            CreateButton(panel, "Medical", "医疗 [M]", new Vector2(0f, -425f), TreatCrew);
            CreateButton(panel, "Mainline", "推进主线 [J]", new Vector2(0f, -490f), AdvanceMainline);
            CreateButton(panel, "Close", "关闭内容页 [Esc]", new Vector2(0f, -555f), CloseContentPage);
            D16ModalFocusScope.Attach(panel, firstContentButton == null ? null : firstContentButton.gameObject);
        }

        private bool IsContentPageVisible()
        {
            var canvas = _canvasObject == null ? null : _canvasObject.GetComponent<Canvas>();
            return canvas != null && canvas.enabled;
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)buttonGo.transform;
            rect.anchorMin = new Vector2(0.5f, 1f); rect.anchorMax = new Vector2(0.5f, 1f); rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(310f, 50f); rect.anchoredPosition = position;
            var image = buttonGo.GetComponent<Image>(); image.sprite = UiSprites.White; image.color = new Color(0.12f, 0.34f, 0.52f, 0.98f);
            var button = buttonGo.GetComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(onClick);
            var text = CreateText(buttonGo, "Label", label, 21, Vector2.zero, new Vector2(300f, 46f), TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(GameObject parent, string name, string content, int size, Vector2 position, Vector2 sizeDelta, TextAnchor alignment)
        {
            var textGo = new GameObject(name, typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)textGo.transform; rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta; rect.anchoredPosition = position;
            var text = textGo.GetComponent<Text>(); text.font = Fonts.Default; text.fontSize = size; text.alignment = alignment; text.color = Color.white; text.text = content; text.raycastTarget = false;
            return text;
        }

        private static string WeaponName(D4WeaponId weapon)
        {
            switch (weapon)
            {
                case D4WeaponId.ChainShot: return "链弹";
                case D4WeaponId.GrapeShot: return "葡萄弹";
                default: return "实心弹";
            }
        }

        private static void SetTopLeft(RectTransform rect, Vector2 position) { rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(0f, 1f); rect.pivot = new Vector2(0f, 1f); rect.anchoredPosition = position; }
        private static void SetBottomLeft(RectTransform rect, Vector2 position) { rect.anchorMin = new Vector2(0f, 0f); rect.anchorMax = new Vector2(0f, 0f); rect.pivot = new Vector2(0f, 0f); rect.anchoredPosition = position; }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }

        private static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
            if (eventSystem == null) eventSystem = new GameObject("D4EventSystem").AddComponent<EventSystem>();
            foreach (var other in FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (other != eventSystem) other.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(true);
            if (EventSystem.current != eventSystem) EventSystem.current = eventSystem;
            var input = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (input == null) input = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            input.enabled = true; input.AssignDefaultActions();
            foreach (var module in eventSystem.GetComponents<BaseInputModule>()) if (module != input) module.enabled = false;
            eventSystem.UpdateModules();
        }
    }
}
