// ============================================================
// D9PresentationReadModel —— D9 垂直切片表现层读模型
//
// 这里只把 D3/D4/D9 快照和 D8 EventLog 的已裁决事实投影成文字。
// 不追加事实、不调用领域系统、不计算价格、伤害、奖励、关系或天气结果。
// ============================================================

using System;
using System.Collections.Generic;
using Whalebone.Agent;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.D9;
using Whalebone.Systems.D16;
using Whalebone.Systems.Events;

namespace Whalebone.Gameplay.D9
{
    [Serializable]
    public sealed class D9PresentationFactLine
    {
        public string EventId;
        public long LogSequence;
        public long SaveRevision;
        public string Text;
        public string OwnerSystem;
        public WorldEventStatus Status;
    }

    [Serializable]
    public sealed class D9VoyagePresentationSnapshot
    {
        public string Location = "位置待接线";
        public string Destination = "目的地待接线";
        public string VoyageStatus = "航行状态待接线";
        public string Weather = "天气待接线";
        public string WeatherPrompt = "天气事实待接线。";
        public string Money = "资金待接线";
        public string Ship = "船舶快照待接线。";
        public string Crew = "船员快照待接线。";
        public string Market = "市场快照待接线。";
        public string Catch = "鱼获事实待接线。";
        public string Dialogue = "尚未收到船员对话回复。";
        public string Quest = "任务快照待接线。";
        public string Combat = "战况快照待接线。";
        public string SaveRestore = "尚未读取存档反馈事实。";
        public List<D9PresentationFactLine> Facts = new List<D9PresentationFactLine>();
    }

    /// <summary>只读注入统一 EventLog；没有日志时保持空读模型，不创建私有事实。</summary>
    public static class D9EventLogBridge
    {
        private static EventLog _current;

        public static EventLog Current => _current;

        public static void Bind(EventLog eventLog)
        {
            _current = eventLog;
        }

        public static void Clear()
        {
            _current = null;
        }
    }

    /// <summary>
    /// 事实日志读取游标。绑定新日志时跳过已有序列，保证读档不会重复触发 UI 结算或音效。
    /// </summary>
    public sealed class D9EventFactCursor
    {
        private EventLog _eventLog;
        private long _lastConsumedSequence;

        public long LastConsumedSequence => _lastConsumedSequence;

        public void Bind(EventLog eventLog)
        {
            if (ReferenceEquals(_eventLog, eventLog)) return;
            _eventLog = eventLog;
            _lastConsumedSequence = eventLog == null ? 0L : eventLog.NextSequence;
        }

        public IReadOnlyList<FactEvent> ConsumeNewFacts()
        {
            var result = new List<FactEvent>();
            if (_eventLog == null) return result;

            foreach (var fact in _eventLog.Facts)
            {
                if (fact == null || fact.LogSequence <= _lastConsumedSequence) continue;
                result.Add(fact.CloneFact());
                _lastConsumedSequence = Math.Max(_lastConsumedSequence, fact.LogSequence);
            }
            return result;
        }
    }

    public static class D9PresentationReadModel
    {
        public static D9VoyagePresentationSnapshot Build(
            D3RuntimeSnapshot d3,
            D4RuntimeSnapshot d4,
            D9PortShipPreparationSnapshot preparation,
            EventLog eventLog,
            AgentReply reply,
            string locale = "zh-CN")
        {
            var result = new D9VoyagePresentationSnapshot();
            BuildVoyage(result, d3, locale);
            BuildShip(result, preparation, locale);
            BuildMarket(result, d3, locale);
            BuildFacts(result, eventLog);
            BuildCrewDialogue(result, reply);
            BuildQuest(result, d4);
            BuildCombat(result, d4);
            return result;
        }

        private static void BuildVoyage(D9VoyagePresentationSnapshot result, D3RuntimeSnapshot snapshot, string locale)
        {
            if (snapshot == null) return;
            result.Location = DisplayLocation(snapshot.CurrentLocationId);
            result.Destination = DisplayLocation(snapshot.DestinationId);
            result.VoyageStatus = DisplayVoyageStatus(snapshot.VoyageStatus) +
                " | 航程 " + FormatNumber(snapshot.CurrentDay, locale) + "/" +
                FormatNumber(snapshot.TotalDays, locale) + " 天";
            result.Weather = DisplayWeather(snapshot.Weather);
            result.WeatherPrompt = WeatherPrompt(snapshot.Weather);
        }

        private static void BuildShip(D9VoyagePresentationSnapshot result, D9PortShipPreparationSnapshot preparation, string locale)
        {
            if (preparation == null || preparation.Ship == null) return;
            var ship = preparation.Ship;
            result.Money = "金币 " + FormatNumber(preparation.Money, locale);
            result.Ship = "重量 " + FormatNumber(ship.Weight, locale) + "/" + FormatNumber(ship.CargoCapacity, locale) +
                " | 吃水 " + FormatNumber((decimal)ship.Draft, locale, 1) + "m" +
                " | 货物 " + CargoSummary(ship.Cargo, locale) +
                " | 补给 水 " + FormatNumber(ship.Water, locale) + " 食物 " + FormatNumber(ship.Food, locale) +
                " | 损伤 " + FormatNumber(ship.Damage, locale) +
                " | 锚泊 " + (ship.Moored ? "是" : "否") +
                " | 改装 " + UpgradeSummary(ship.Upgrades, locale);
            result.Crew = "船员 " + FormatNumber(ship.CrewCount, locale) + "/" + FormatNumber(ship.CrewCapacity, locale);
        }

        private static void BuildMarket(D9VoyagePresentationSnapshot result, D3RuntimeSnapshot snapshot, string locale)
        {
            if (snapshot == null || snapshot.MarketStock == null) return;
            var lines = new List<string>();
            foreach (var pair in snapshot.MarketStock)
                lines.Add(pair.Key + " 库存 " + FormatNumber(pair.Value, locale));
            result.Market = lines.Count == 0 ? "市场暂无库存事实。" : string.Join("；", lines.ToArray());
        }

        private static void BuildFacts(D9VoyagePresentationSnapshot result, EventLog eventLog)
        {
            if (eventLog == null)
            {
                result.Catch = "鱼获事实待接线。";
                return;
            }

            var catchLines = new List<string>();
            string saveFeedback = null;
            foreach (var fact in eventLog.ReadFactsForUi(20))
            {
                if (fact == null) continue;
                string text = fact.Result == null ? string.Empty : fact.Result.Summary;
                if (string.IsNullOrWhiteSpace(text)) text = fact.EventId;
                result.Facts.Add(new D9PresentationFactLine
                {
                    EventId = fact.EventId,
                    LogSequence = fact.LogSequence,
                    SaveRevision = fact.SaveRevision,
                    Text = text,
                    OwnerSystem = fact.OwnerSystem,
                    Status = fact.OutcomeStatus,
                });

                if (fact.EventId == "event_d9_fishing_catch") catchLines.Add(text);
                if (fact.EventId == "event_d9_save_checkpoint") saveFeedback = text;
            }

            result.Catch = catchLines.Count == 0 ? "尚未读取近海鱼获事实。" : string.Join("；", catchLines.ToArray());
            if (!string.IsNullOrWhiteSpace(saveFeedback)) result.SaveRestore = saveFeedback;
        }

        private static void BuildCrewDialogue(D9VoyagePresentationSnapshot result, AgentReply reply)
        {
            if (reply == null || string.IsNullOrWhiteSpace(reply.Speech)) return;
            result.Dialogue = reply.Speech + " | 情绪 " + reply.Emotion +
                " | 语气 " + reply.NegotiationTone +
                " | 拒绝原因 " + reply.RefusalReason +
                " | 动作意图 " + (reply.ActionIntent == null ? "none" : reply.ActionIntent.Name);
        }

        private static void BuildQuest(D9VoyagePresentationSnapshot result, D4RuntimeSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Mainline == null) return;
            int index = snapshot.Mainline.MissionIndex;
            if (index < 0 || index >= D4MainlineSystem.Missions.Count)
            {
                result.Quest = "三章主线已完成，任务日志转为可选航线。";
                return;
            }
            var mission = D4MainlineSystem.Missions[index];
            result.Quest = "第" + mission.Chapter + "章 · " + mission.Title + "：" + mission.Description;
        }

        private static void BuildCombat(D9VoyagePresentationSnapshot result, D4RuntimeSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Combat == null) return;
            result.Combat = "战况 " + snapshot.Combat.Status +
                " | 敌船 " + (string.IsNullOrEmpty(snapshot.Combat.EnemyId) ? "无" : snapshot.Combat.EnemyId);
        }

        private static string CargoSummary(Dictionary<string, int> cargo, string locale)
        {
            if (cargo == null || cargo.Count == 0) return "空";
            var lines = new List<string>();
            foreach (var pair in cargo) lines.Add(pair.Key + "×" + FormatNumber(pair.Value, locale));
            return string.Join(" ", lines.ToArray());
        }

        private static string UpgradeSummary(Dictionary<string, int> upgrades, string locale)
        {
            if (upgrades == null || upgrades.Count == 0) return "无";
            var lines = new List<string>();
            foreach (var pair in upgrades) lines.Add(pair.Key + " " + FormatNumber(pair.Value, locale) + "级");
            return string.Join(" ", lines.ToArray());
        }

        private static string FormatNumber(int value, string locale)
        {
            return D16DisplayFormatter.Number(value, locale);
        }

        private static string FormatNumber(decimal value, string locale, int decimals = 0)
        {
            return D16DisplayFormatter.Number(value, locale, decimals);
        }

        private static string DisplayLocation(string id)
        {
            if (string.IsNullOrEmpty(id)) return "未知位置";
            if (id == "santa_margarita") return "圣玛格丽特港";
            if (id == "green_cay") return "绿礁村";
            if (id == "nearshore_saint_marys") return "圣玛丽近海鱼场";
            if (id == "saint_marys_chart_cove") return "圣玛丽海图湾";
            return id;
        }

        private static string DisplayVoyageStatus(D3VoyageStatus status)
        {
            switch (status)
            {
                case D3VoyageStatus.InPort: return "港内";
                case D3VoyageStatus.Sailing: return "航行中";
                case D3VoyageStatus.Landed: return "已登陆";
                case D3VoyageStatus.Arrived: return "已抵达";
                case D3VoyageStatus.Failed: return "航行失败";
                default: return status.ToString();
            }
        }

        private static string DisplayWeather(D3WeatherType weather)
        {
            switch (weather)
            {
                case D3WeatherType.Calm: return "平静";
                case D3WeatherType.TradeWind: return "信风";
                case D3WeatherType.Rain: return "降雨";
                case D3WeatherType.Storm: return "风暴";
                case D3WeatherType.Hurricane: return "飓风";
                default: return weather.ToString();
            }
        }

        private static string WeatherPrompt(D3WeatherType weather)
        {
            switch (weather)
            {
                case D3WeatherType.Storm: return "天气提示：风暴事实已进入状态快照，请确认航行指令。";
                case D3WeatherType.Hurricane: return "天气提示：飓风事实已进入状态快照，请确认航行指令。";
                case D3WeatherType.Rain: return "天气提示：降雨事实已进入状态快照。";
                case D3WeatherType.TradeWind: return "天气提示：信风事实已进入状态快照。";
                default: return "天气提示：平静海况事实已进入状态快照。";
            }
        }
    }
}
