using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D4
{
    /// <summary>三章锚点主线与五个教学目标的唯一推进者。</summary>
    public sealed class D4MainlineSystem
    {
        public static readonly IReadOnlyList<MainlineMission> Missions = new List<MainlineMission>
        {
            new MainlineMission { Id = "ch1_departure", Chapter = 1, Title = "离港的旧账", Description = "离开圣玛格丽特港，开始偿还旧船的人情。", GrantsFlag = "learn_sailing" },
            new MainlineMission { Id = "ch1_market", Chapter = 1, Title = "第一笔货", Description = "完成一次合法交易，认识加勒比价格。", RequiredFlag = "learn_sailing", GrantsFlag = "learn_trade" },
            new MainlineMission { Id = "ch1_shore", Chapter = 1, Title = "潮线之外", Description = "登岛观察并带回一份资源。", RequiredFlag = "learn_trade", GrantsFlag = "learn_island" },
            new MainlineMission { Id = "ch2_voice", Chapter = 2, Title = "船上的声音", Description = "与老水手完成一次对话，听见势力纷争的传闻。", RequiredFlag = "learn_island", GrantsFlag = "learn_dialogue" },
            new MainlineMission { Id = "ch2_cannon", Chapter = 2, Title = "炮声作答", Description = "完成一次炮战或成功脱离敌舰。", RequiredFlag = "learn_dialogue", GrantsFlag = "learn_combat" },
            new MainlineMission { Id = "ch2_permit", Chapter = 2, Title = "一纸私掠证", Description = "取得一个势力的信任，接触合法私掠。", RequiredFlag = "learn_combat", RequiredFaction = "england", RequiredReputation = 10, GrantsFlag = "privateer_contact" },
            new MainlineMission { Id = "ch3_embargo", Chapter = 3, Title = "封锁线", Description = "穿过一次禁运港的检查，选择贸易或走私。", RequiredFlag = "privateer_contact", GrantsFlag = "break_blockade" },
            new MainlineMission { Id = "ch3_lighthouse", Chapter = 3, Title = "灯塔与病舱", Description = "治疗一名船员，并把航线事实写入日志。", RequiredFlag = "break_blockade", GrantsFlag = "final_route" },
            new MainlineMission { Id = "ch3_decision", Chapter = 3, Title = "自己的旗帜", Description = "完成三条路线教学，之后主线转为可选。", RequiredFlag = "final_route", GrantsFlag = "mainline_complete" },
        };

        private readonly MainlineState _state;

        public D4MainlineSystem(MainlineState initial = null)
        {
            _state = initial == null ? new MainlineState() : initial.Clone();
            ClampState();
        }

        public MainlineState Snapshot => _state.Clone();

        public MainlineMission CurrentMission
        {
            get
            {
                if (_state.MissionIndex < 0 || _state.MissionIndex >= Missions.Count) return null;
                return Missions[_state.MissionIndex];
            }
        }

        public void SetFlag(string flag, bool value = true)
        {
            if (!string.IsNullOrEmpty(flag)) _state.Flags[flag] = value;
        }

        public MainlineResult TryAdvance(FactionState factionState)
        {
            var mission = CurrentMission;
            if (mission == null)
                return new MainlineResult { Completed = true, Message = "三章主线已完成，之后的航线完全由玩家决定。" };
            if (!string.IsNullOrEmpty(mission.RequiredFlag) && !GetFlag(mission.RequiredFlag))
                return new MainlineResult { Mission = mission, Message = "尚未满足前置教学目标：" + mission.RequiredFlag };
            if (!string.IsNullOrEmpty(mission.RequiredFaction) && Reputation(factionState, mission.RequiredFaction) < mission.RequiredReputation)
                return new MainlineResult { Mission = mission, Message = "势力声望不足，暂时无法推进该任务。" };

            _state.CompletedMissions.Add(mission.Id);
            if (!string.IsNullOrEmpty(mission.GrantsFlag)) SetFlag(mission.GrantsFlag);
            _state.MissionIndex++;
            _state.Chapter = _state.MissionIndex >= Missions.Count ? 3 : Missions[_state.MissionIndex].Chapter;
            return new MainlineResult
            {
                Advanced = true,
                Completed = _state.MissionIndex >= Missions.Count,
                Mission = mission,
                Message = _state.MissionIndex >= Missions.Count ? "三章主线完成，沙盒航海正式开放。" : "任务完成：" + mission.Title,
            };
        }

        public bool GetFlag(string flag)
        {
            bool value;
            return !string.IsNullOrEmpty(flag) && _state.Flags.TryGetValue(flag, out value) && value;
        }

        private static int Reputation(FactionState state, string factionId)
        {
            if (state == null || state.Reputation == null) return 0;
            int value;
            return state.Reputation.TryGetValue(factionId, out value) ? value : 0;
        }

        private void ClampState()
        {
            _state.MissionIndex = Math.Max(0, Math.Min(Missions.Count, _state.MissionIndex));
            _state.Chapter = _state.MissionIndex >= Missions.Count ? 3 : Math.Max(1, Missions[_state.MissionIndex].Chapter);
        }
    }
}
