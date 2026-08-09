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
using Whalebone.Systems.D4;
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
        private bool _ready;
        private string _message = "D4 内容层正在等待 D2 场景对象…";

        private Text _statusText;
        private Text _detailText;
        private Text _hintText;

        private void Awake()
        {
            EnsureEventSystem();
            BuildUi();
        }

        private void Update()
        {
            if (!_ready)
            {
                TryBuildRuntime();
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || Time.timeScale <= 0f) return;
            if (keyboard.fKey.wasPressedThisFrame) FactionAction();
            else if (keyboard.kKey.wasPressedThisFrame) StartCombat();
            else if (keyboard.rKey.wasPressedThisFrame) Reload();
            else if (keyboard.digit1Key.wasPressedThisFrame) SelectWeapon(D4WeaponId.RoundShot);
            else if (keyboard.digit2Key.wasPressedThisFrame) SelectWeapon(D4WeaponId.ChainShot);
            else if (keyboard.digit3Key.wasPressedThisFrame) SelectWeapon(D4WeaponId.GrapeShot);
            else if (keyboard.spaceKey.wasPressedThisFrame) Fire();
            else if (keyboard.bKey.wasPressedThisFrame) Board();
            else if (keyboard.eKey.wasPressedThisFrame) EscapeCombat();
            else if (keyboard.uKey.wasPressedThisFrame) UpgradeHull();
            else if (keyboard.aKey.wasPressedThisFrame) ObserveAnimal();
            else if (keyboard.mKey.wasPressedThisFrame) TreatCrew();
            else if (keyboard.jKey.wasPressedThisFrame) AdvanceMainline();
            else if (keyboard.cKey.wasPressedThisFrame) SaveProgress();
            RefreshHud();
        }

        public void FactionAction()
        {
            if (!_ready) return;
            var result = _factions.ApplyReputation("england", 10, "完成护航教学，英格兰声望提高。");
            AddLog(D4LogKind.Faction, result.Message);
            _message = result.Message;
        }

        public void StartCombat()
        {
            if (!_ready) return;
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
        }

        public void Reload()
        {
            if (!_ready) return;
            int loaded = _combat.Reload(_combat.Snapshot.PlayerCannons);
            _message = loaded > 0 ? "炮组已装填 " + loaded + " 门。" : "当前没有可装填的炮。";
        }

        public void SelectWeapon(D4WeaponId weapon)
        {
            if (!_ready) return;
            _message = _combat.SelectWeapon(weapon) ? "已选择弹种：" + WeaponName(weapon) : "开炮前请先完成当前装填。";
        }

        public void Fire()
        {
            if (!_ready) return;
            var result = _combat.Fire(3, true);
            if (!result.Accepted)
            {
                _message = result.Message;
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
        }

        public void Board()
        {
            if (!_ready) return;
            var result = _combat.Board();
            _message = result.Message;
            if (result.Status == D4CombatStatus.Victory)
            {
                _mainline.SetFlag("learn_combat");
                AddLog(D4LogKind.Combat, "接舷战胜利，敌船已被控制。");
            }
        }

        public void EscapeCombat()
        {
            if (!_ready) return;
            var result = _combat.Escape();
            _message = result.Message;
            AddLog(D4LogKind.Combat, result.Message);
        }

        public void UpgradeHull()
        {
            if (!_ready || GameState.CurrentPlayer == null) return;
            var result = _shipyard.TryUpgrade(D4UpgradeSlot.Hull, ref GameState.CurrentPlayer.Money,
                _factions.GetReputation("england"));
            _message = result.Message;
            if (result.Accepted) AddLog(D4LogKind.Faction, "船坞完成船体改装，等级 " + result.NewLevel + "。");
        }

        public void ObserveAnimal()
        {
            if (!_ready) return;
            if (_snapshot.Ecology.Animals.Count == 0) _ecology.SeedHabitat("jungle", CurrentDay(), 17);
            AnimalState target = null;
            foreach (var animal in _ecology.Snapshot.Animals)
                if (!animal.Observed) { target = animal; break; }
            if (target == null) { _message = "本地动物都已记录。"; return; }
            var observed = _ecology.Observe(target.AnimalId);
            var collected = _ecology.Collect(target.AnimalId);
            _mainline.SetFlag("learn_island");
            _message = observed.Message + " " + collected.Message;
            AddLog(D4LogKind.Animal, _message);
        }

        public void TreatCrew()
        {
            if (!_ready || GameState.CrewAgents.Count == 0) return;
            string crewId = GameState.CrewAgents[0].Id;
            _medical.EnsureCrew(crewId);
            var member = FindHealth(crewId);
            if (member == null) return;
            if (member.Diseases.Count == 0) _medical.Infect(crewId, "scurvy");
            var result = _medical.Treat(crewId, "scurvy", 2);
            _mainline.SetFlag("learn_dialogue");
            _message = result.Message;
            AddLog(D4LogKind.Medical, _message);
        }

        public void AdvanceMainline()
        {
            if (!_ready) return;
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
        }

        public void SaveProgress()
        {
            if (!_ready || _snapshot == null) return;
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
        }

        private void TryBuildRuntime()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var ship = GameObject.Find("ph_ship_fishing");
            if (player == null || ship == null) return;
            _player = player.transform;
            _ship = ship.transform;
            _snapshot = SaveSystem.LoadD4Runtime() ?? new D4RuntimeSnapshot();
            _factions = new D4FactionSystem(_snapshot.Factions);
            _shipyard = new D4ShipyardSystem(_snapshot.Shipyard);
            _combat = new D4CombatSystem(_snapshot.Combat);
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

        private void BuildUi()
        {
            var canvasGo = new GameObject("D4Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            _statusText = CreateText(canvasGo, "D4Status", "D4 正在等待场景对象…", 22, new Vector2(28f, -28f), new Vector2(900f, 90f), TextAnchor.UpperLeft);
            SetTopLeft(_statusText.rectTransform, new Vector2(28f, -28f));
            _detailText = CreateText(canvasGo, "D4Detail", "", 18, new Vector2(28f, -130f), new Vector2(900f, 260f), TextAnchor.UpperLeft);
            SetTopLeft(_detailText.rectTransform, new Vector2(28f, -130f));
            _hintText = CreateText(canvasGo, "D4Hint", "", 17, Vector2.zero, new Vector2(1800f, 52f), TextAnchor.MiddleLeft);
            SetBottomLeft(_hintText.rectTransform, new Vector2(28f, 20f));

            var panel = new GameObject("D4Controls", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(1f, 1f); panelRect.anchorMax = new Vector2(1f, 1f); panelRect.pivot = new Vector2(1f, 1f);
            panelRect.sizeDelta = new Vector2(360f, 590f); panelRect.anchoredPosition = new Vector2(-28f, -120f);
            panel.GetComponent<Image>().sprite = UiSprites.White; panel.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.08f, 0.82f);
            CreateButton(panel, "Faction", "势力任务 [F]", new Vector2(0f, -35f), FactionAction);
            CreateButton(panel, "Combat", "开始炮战 [K]", new Vector2(0f, -100f), StartCombat);
            CreateButton(panel, "Reload", "装填炮弹 [R]", new Vector2(0f, -165f), Reload);
            CreateButton(panel, "Fire", "开炮 [Space]", new Vector2(0f, -230f), Fire);
            CreateButton(panel, "Upgrade", "船体改装 [U]", new Vector2(0f, -295f), UpgradeHull);
            CreateButton(panel, "Animal", "观察动物 [A]", new Vector2(0f, -360f), ObserveAnimal);
            CreateButton(panel, "Medical", "医疗 [M]", new Vector2(0f, -425f), TreatCrew);
            CreateButton(panel, "Mainline", "推进主线 [J]", new Vector2(0f, -490f), AdvanceMainline);
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var buttonGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent.transform, false);
            var rect = (RectTransform)buttonGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f); rect.pivot = new Vector2(0.5f, 0.5f);
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
