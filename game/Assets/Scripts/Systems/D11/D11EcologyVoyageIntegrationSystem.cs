// ============================================================
// D11EcologyVoyageIntegrationSystem —— D11-S01 跨系统事实编排
//
// 这是同步集成层，不是新的生态/动物/库存/关系系统。它只读取已落盘
// 的 D11 事实，然后调用 Faction/Quest/Relation 的唯一写入口；天气、
// 鱼群、宠物、图鉴和关系的数值裁决仍归属原系统。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D4;
using Whalebone.Systems.D10;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public sealed class D11EcologyVoyageIntegrationSystem
    {
        private const string FactionOwner = "FactionSystem";
        private const string QuestOwner = "QuestSystem";
        private const string RescueFaction = "england";
        private const int RescueReputationDelta = 5;

        private readonly D11RareAnimalCollectionSystem _collection;
        private readonly D4FactionSystem _factions;
        private readonly RelationSystem _relations;
        private readonly EventLog _eventLog;

        public D11EcologyVoyageIntegrationSystem(
            D11RareAnimalCollectionSystem collection,
            D4FactionSystem factions,
            RelationSystem relations,
            EventLog eventLog)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (factions == null) throw new ArgumentNullException("factions");
            if (relations == null) throw new ArgumentNullException("relations");
            if (eventLog == null) throw new ArgumentNullException("eventLog");
            _collection = collection;
            _factions = factions;
            _relations = relations;
            _eventLog = eventLog;
        }

        public EventLog EventLog { get { return _eventLog; } }

        /// <summary>
        /// 先让 CollectionSystem 裁决救助，再由 FactionSystem 和 QuestSystem
        /// 消费同一救助事实。重复请求只读取已有事实，不重复改变声望。
        /// </summary>
        public D11S01RescueIntegrationResult RescueRareAnimal(D11RescueRequest request)
        {
            var rescue = _collection.Rescue(request);
            var result = new D11S01RescueIntegrationResult
            {
                Rescue = rescue,
                Code = rescue == null ? "rescue_missing" : rescue.Code,
                Message = rescue == null ? "救助结果不存在。" : rescue.Message,
            };
            if (rescue == null || !rescue.Succeeded) return result;

            var rescueFact = _eventLog.Find(rescue.EventId,
                request == null ? 0 : Math.Max(0L, request.SaveRevision));
            if (rescueFact == null)
            {
                result.Code = "rescue_fact_missing";
                result.Message = "救助已返回成功，但共享事实日志中缺少救助事实。";
                return result;
            }

            string factionEventId = "event_d11_s01_wildlife_rescue_recognized_" + rescueFact.EventId;
            result.FactionFact = _eventLog.Find(factionEventId, rescueFact.SaveRevision);
            if (result.FactionFact == null)
            {
                result.Faction = _factions.ApplyReputation(RescueFaction, RescueReputationDelta,
                    "珍稀动物救助记录已得到保护区势力认可。");
                var append = _eventLog.Append(new WorldEvent(
                    factionEventId,
                    rescueFact.WorldDay,
                    rescueFact.LocationRef,
                    FactionOwner,
                    new EventResult
                    {
                        ResultCode = "wildlife_rescue_recognized",
                        Summary = "珍稀动物救助已进入势力声望记录。",
                        FactIds = new List<string>
                        {
                            "reputation:" + RescueFaction,
                            "wildlife_rescue:" + rescue.SpeciesId,
                        },
                        Values = new Dictionary<string, string>
                        {
                            ["faction_id"] = RescueFaction,
                            ["reputation_delta"] = RescueReputationDelta.ToString(CultureInfo.InvariantCulture),
                            ["species_id"] = rescue.SpeciesId ?? string.Empty,
                            ["source_event_id"] = rescueFact.EventId,
                        },
                    },
                    new[] { "player" },
                    new[] { rescueFact.EventId },
                    new[] { rescueFact.EventId },
                    rescueFact.SaveRevision));
                result.FactionFact = append.Fact;
            }
            else
            {
                result.Duplicate = true;
                result.Faction = new FactionActionResult
                {
                    Accepted = true,
                    ReputationDelta = 0,
                    Message = "救助声望事实已经结算，未重复修改势力状态。",
                };
            }

            string questEventId = "event_d11_s01_quest_wildlife_rescue_" + rescueFact.EventId;
            result.QuestFact = _eventLog.Find(questEventId, rescueFact.SaveRevision);
            if (result.QuestFact == null)
            {
                var append = _eventLog.Append(new WorldEvent(
                    questEventId,
                    rescueFact.WorldDay,
                    rescueFact.LocationRef,
                    QuestOwner,
                    new EventResult
                    {
                        ResultCode = "wildlife_rescue_completed",
                        Summary = "珍稀动物救助任务事实已完成，可由任务系统读取。",
                        FactIds = new List<string>
                        {
                            "quest:wildlife_rescue_completed",
                            "codex:" + (rescue.SpeciesId ?? string.Empty),
                        },
                        Values = new Dictionary<string, string>
                        {
                            ["quest_id"] = "quest_wildlife_rescue_saint_marys",
                            ["status"] = "completed",
                            ["species_id"] = rescue.SpeciesId ?? string.Empty,
                            ["source_event_id"] = rescueFact.EventId,
                            ["reputation_event_id"] = factionEventId,
                        },
                    },
                    new[] { "player" },
                    new[] { rescueFact.EventId, factionEventId },
                    new[] { rescueFact.EventId, factionEventId },
                    rescueFact.SaveRevision));
                result.QuestFact = append.Fact;
            }
            else
            {
                result.Duplicate = true;
            }

            result.Succeeded = result.FactionFact != null && result.QuestFact != null;
            result.Code = result.Duplicate ? "duplicate_settlement" : "wildlife_rescue_integrated";
            result.Message = result.Duplicate
                ? "救助图鉴、声望和任务事实已经恢复，未重复结算。"
                : "救助图鉴、声望和任务事实已由各自 owner 完成接入。";
            return result;
        }

        /// <summary>
        /// 消费 AnimalCareSystem 已产生的船员反应事实，让 D10 RelationSystem
        /// 写入 Crew→Pet attachment 边；关系增量只是集成策略提议，仍由
        /// RelationSystem 负责边界、事实校验和重复保护。
        /// </summary>
        public D11S01PetRelationResult ApplyPetCrewRelations(D11PetActionResult petAction)
        {
            var result = new D11S01PetRelationResult
            {
                PetId = petAction == null || petAction.State == null ? string.Empty : petAction.State.PetId,
            };
            if (petAction == null || petAction.State == null || string.IsNullOrWhiteSpace(petAction.State.PetId))
            {
                result.Code = "pet_missing";
                result.Message = "宠物动作缺少可关联的宠物实例。";
                return result;
            }

            _relations.RegisterCharacter(petAction.State.PetId);
            if (petAction.CrewReactions != null)
            {
                foreach (var reaction in petAction.CrewReactions)
                {
                    if (reaction == null || string.IsNullOrWhiteSpace(reaction.CrewId)) continue;
                    _relations.RegisterCharacter(reaction.CrewId);
                    var fact = _eventLog.Find(D11PetSystem.CrewReactionEventId, reaction.SaveRevision);
                    if (fact == null) continue;
                    var relation = _relations.ApplyFactEvent(fact, new RelationChangeIntent(
                        reaction.CrewId,
                        petAction.State.PetId,
                        RelationType.Attachment,
                        AttachmentDelta(reaction.ReactionId),
                        "deterministic_pet_fact:" + (reaction.ReactionId ?? string.Empty)));
                    result.Relations.Add(relation);
                    if (relation.IsDuplicate) result.DuplicateCount++;
                    else if (relation.Succeeded) result.AppliedCount++;
                }
            }

            result.Succeeded = result.AppliedCount > 0 || result.DuplicateCount > 0;
            result.Code = result.DuplicateCount > 0 && result.AppliedCount == 0
                ? "duplicate_relation_settlement" : "pet_relation_integrated";
            result.Message = result.Succeeded
                ? "宠物事实已进入船员关系图。"
                : "没有可应用的宠物船员反应事实。";
            return result;
        }

        private static float AttachmentDelta(string reactionId)
        {
            switch (reactionId ?? string.Empty)
            {
                case "adoption_welcome": return 3f;
                case "relief": return 3f;
                case "care_acknowledged":
                case "care_practical_support": return 2f;
                case "quarantine_support":
                case "recovery": return 2f;
                case "warning_acknowledged":
                case "mouse_control_approved": return 1f;
                case "loss_concern": return 1f;
                case "mourning": return -2f;
                default: return 1f;
            }
        }
    }
}
