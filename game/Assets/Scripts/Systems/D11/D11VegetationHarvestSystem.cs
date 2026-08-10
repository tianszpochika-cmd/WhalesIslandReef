// ============================================================
// D11VegetationHarvestSystem —— D11-A07 植被采集确定性裁决编排
//
// 本类只编排请求：识别、成熟度、工具和保护区规则在这里确定；资源量
// 由 D4EcologySystem 写入，货舱由 D3InventorySystem 写入，事实复用同一
// EventLog。没有 Unity、网络或 LLM 依赖。
// ============================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Whalebone.Systems.D3;
using Whalebone.Systems.D4;
using Whalebone.Systems.Events;

namespace Whalebone.Systems.D11
{
    public sealed class D11VegetationHarvestSystem
    {
        private const string Owner = "EcologySystem";
        private readonly D3InventoryState _inventory;
        private readonly D4EcologySystem _ecology;
        private readonly EventLog _eventLog;
        private D11VegetationHarvestResult _lastHarvest;

        public D11VegetationHarvestSystem(D3InventoryState inventory, D4EcologySystem ecology,
            EventLog eventLog = null)
        {
            _inventory = inventory;
            _ecology = ecology;
            _eventLog = eventLog ?? new EventLog();
            D11VegetationHarvestCatalog.RegisterInventoryDefinitions();
        }

        public EventLog EventLog { get { return _eventLog; } }

        public D11VegetationHarvestResult LastHarvest
        {
            get { return _lastHarvest == null ? null : _lastHarvest.Clone(); }
        }

        /// <summary>只读识别；不会添加库存、扣资源或改变生态状态。</summary>
        public D11VegetationIdentificationResult Identify(D11VegetationIdentifyRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PlantId))
                return IdentifyFail("invalid_request", "识别请求缺少植被 ID。");
            var resource = D11VegetationHarvestCatalog.FindResource(request.PlantId);
            if (resource == null)
                return IdentifyFail("plant_unknown", "目录中没有这个植被 ID。");

            int knowledge = Math.Max(0, request.BotanySkill) +
                (request.HasFieldGuide ? 1 : 0) + (request.HasCrewAdvice ? 1 : 0);
            if (knowledge >= resource.MinimumIdentificationSkill)
            {
                float confidence = Clamp01(0.70f + knowledge * 0.08f);
                return new D11VegetationIdentificationResult
                {
                    Succeeded = true,
                    IsCorrect = true,
                    Code = "identified",
                    Message = "已根据形态、图鉴和经验确认植被。",
                    ActualPlantId = resource.Id,
                    IdentifiedPlantId = resource.Id,
                    ResourceKind = resource.Kind,
                    Confidence = confidence,
                    Toxic = resource.Toxic,
                };
            }

            string lookalike = LookalikeFor(resource, request.Day, request.Seed);
            return new D11VegetationIdentificationResult
            {
                Succeeded = true,
                IsCorrect = false,
                Code = "misidentified",
                Message = "形态相近，当前知识不足以确认物种。",
                ActualPlantId = resource.Id,
                IdentifiedPlantId = lookalike,
                ResourceKind = resource.Kind,
                Confidence = 0.25f,
                Toxic = resource.Toxic,
            };
        }

        /// <summary>
        /// 采集唯一编排入口。该入口不会接受外部最终数量，只接受节点引用和意图数量。
        /// </summary>
        public D11VegetationHarvestResult Harvest(D11VegetationHarvestRequest request)
        {
            if (request == null)
                return Reject("invalid_request", "采集请求不存在。", null, null, 0);
            if (_inventory == null || _ecology == null)
                return Reject("runtime_missing", "采集运行时缺少库存或生态状态。", request, null, 0);
            if (string.IsNullOrWhiteSpace(request.PlantId))
                return Reject("plant_required", "采集请求缺少植被 ID。", request, null, 0);
            if (request.RequestedQuantity <= 0)
                return Reject("invalid_quantity", "采集数量必须大于零。", request, null, 0);

            var resource = D11VegetationHarvestCatalog.FindResource(request.PlantId);
            if (resource == null)
                return Reject("plant_unknown", "目录中没有这个植被 ID。", request, null, 0);

            string eventId = string.IsNullOrWhiteSpace(request.EventId)
                ? D11VegetationIds.HarvestEventPrefix + SafeId(request.PlantId) + "_" +
                    Math.Max(0, request.Day).ToString(CultureInfo.InvariantCulture) + "_" +
                    Stable(request.Seed.ToString(CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture)
                : request.EventId;
            if (_ecology.IsEcologyEventApplied(eventId, request.SaveRevision, _eventLog))
                return Duplicate(eventId, request.SaveRevision, request.PlantId);

            var plantState = _ecology.FindPlantSnapshot(request.PlantId);
            if (plantState == null)
                return Reject("plant_missing", "当前生境没有这个植被资源。", request, resource, 0);

            var identification = Identify(new D11VegetationIdentifyRequest
            {
                PlantId = request.PlantId,
                BotanySkill = request.BotanySkill,
                HasFieldGuide = request.HasFieldGuide,
                HasCrewAdvice = request.HasCrewAdvice,
                Day = request.Day,
                Seed = request.Seed,
            });
            if (!string.IsNullOrWhiteSpace(request.ClaimedPlantId))
            {
                identification.IdentifiedPlantId = request.ClaimedPlantId;
                identification.IsCorrect = request.ClaimedPlantId == request.PlantId;
                identification.Code = identification.IsCorrect ? "identified" : "misidentified";
                identification.Confidence = identification.IsCorrect ? 1f : 0.10f;
            }
            bool misidentified = !identification.IsCorrect;
            if (misidentified && !request.AllowUnidentifiedHarvest)
                return Reject("identification_required", "请先确认植被身份；未知物种不能直接采集。",
                    request, resource, 0, identification);

            if (!string.IsNullOrWhiteSpace(request.ProtectionAreaId))
            {
                var area = D11VegetationHarvestCatalog.FindProtectionArea(request.ProtectionAreaId);
                if (area == null)
                    return Reject("protection_area_unknown", "保护区 ID 不在目录中。", request, resource, 0,
                        identification);
                if (area.Blocks(resource))
                    return Reject("protected_area", "保护区禁止采集该类植被。", request, resource, 0,
                        identification);
            }
            if (resource.ProtectedSpecies)
                return Reject("protected_species", "该物种受保护，当前没有可用的合法采集许可。",
                    request, resource, 0, identification);

            var tool = D11VegetationHarvestCatalog.FindTool(request.ToolId);
            if (tool == null || tool.Id != resource.RequiredToolId)
                return Reject("tool_required", "当前植被需要正确的采集工具。", request, resource, 0,
                    identification);

            var stage = MaturityFor(resource, request.Day, request.Seed);
            if (stage == D11VegetationMaturityStage.Seedling ||
                stage == D11VegetationMaturityStage.Growing)
                return Reject("not_mature", "植被尚未达到可采集成熟度。", request, resource, 0,
                    identification, stage);

            int resourceQuantity = Math.Min(Math.Min(request.RequestedQuantity,
                Math.Max(1, resource.HarvestLimitPerAction)), Math.Max(0, plantState.Quantity));
            if (resourceQuantity <= 0)
                return Reject("plant_depleted", "该植被当前没有可采集数量。", request, resource, 0,
                    identification, stage);

            int yieldPerResource = resource.BaseYield;
            if (stage == D11VegetationMaturityStage.Overripe)
                yieldPerResource = Math.Max(1, yieldPerResource - 1);
            if (misidentified)
                yieldPerResource = Math.Max(1, yieldPerResource - 1);
            int quantity = SafeMultiply(resourceQuantity, Math.Max(1, yieldPerResource));
            if (!D3InventorySystem.CanAddCargo(_inventory, resource.ItemId, quantity,
                out string capacityError))
                return Reject(capacityError, capacityError == "weight_full"
                    ? "采集物重量超出限额。" : "货舱没有足够空间。", request, resource, quantity,
                    identification, stage);
            if (!D3InventorySystem.TryAddCargo(_inventory, resource.ItemId, quantity,
                out string inventoryError))
                return Reject(inventoryError, "采集物无法装入货舱。", request, resource, quantity,
                    identification, stage);

            var quality = QualityFor(stage, identification.Confidence, misidentified);
            string consequence = resource.Toxic ? "plant_poisoning" :
                misidentified ? "plant_misidentification" : string.Empty;
            var context = new D11PlantHarvestContext
            {
                ItemId = resource.ItemId,
                ResourceKind = resource.Kind.ToString(),
                ToolId = tool.Id,
                MaturityStage = stage.ToString(),
                Quality = quality.ToString(),
                RecognizedPlantId = identification.IdentifiedPlantId,
                IdentificationConfidence = identification.Confidence,
                Misidentified = misidentified,
                Toxic = resource.Toxic,
                OutputQuantity = quantity,
                ConsequenceId = consequence,
                ProtectionAreaId = request.ProtectionAreaId,
            };
            var ecologyResult = _ecology.HarvestPlant(request.PlantId, resourceQuantity, request.Day,
                request.Seed, _eventLog, request.SaveRevision, eventId, context);
            if (!ecologyResult.Succeeded || ecologyResult.Duplicate)
            {
                D3InventorySystem.TryRemoveCargo(_inventory, resource.ItemId, quantity, out _);
                return ecologyResult.Duplicate
                    ? Duplicate(eventId, request.SaveRevision, request.PlantId)
                    : Reject(ecologyResult.Code, ecologyResult.Message, request, resource, quantity,
                        identification, stage);
            }

            var result = new D11VegetationHarvestResult
            {
                Succeeded = true,
                Code = resource.Toxic ? "toxic_harvest" : misidentified ? "misidentified_harvest" :
                    "harvested",
                Message = resource.Toxic
                    ? "毒物已采集并入货，后果已记录为植物中毒风险。"
                    : misidentified ? "误采已结算，识别错误和后果已记录。" : "植被采集完成。",
                PlantId = resource.Id,
                IdentifiedPlantId = identification.IdentifiedPlantId,
                ItemId = resource.ItemId,
                ResourceKind = resource.Kind,
                MaturityStage = stage,
                Quality = quality,
                ToolId = tool.Id,
                ResourceQuantity = ecologyResult.Quantity,
                Quantity = quantity,
                CargoUsed = D3InventorySystem.UsedCargo(_inventory),
                IdentificationConfidence = identification.Confidence,
                IsCorrectIdentification = identification.IsCorrect,
                Misidentified = misidentified,
                Toxic = resource.Toxic,
                ConsequenceId = consequence,
                ProtectionAreaId = request.ProtectionAreaId,
                SaveRevision = Math.Max(0L, request.SaveRevision),
                EventId = eventId,
                Fact = _eventLog.Find(eventId, Math.Max(0L, request.SaveRevision)),
            };
            _lastHarvest = result.Clone();
            return result;
        }

        private D11VegetationHarvestResult Duplicate(string eventId, long saveRevision, string plantId)
        {
            var fact = _eventLog.Find(eventId, Math.Max(0L, saveRevision));
            var result = new D11VegetationHarvestResult
            {
                Succeeded = true,
                Duplicate = true,
                Code = "duplicate_settlement",
                Message = "该植被采集事实已结算，读档后未重复增加库存。",
                PlantId = plantId,
                EventId = eventId,
                SaveRevision = Math.Max(0L, saveRevision),
                Fact = fact,
                ItemId = Read(fact, "item_id"),
                Quantity = ReadInt(fact, "output_quantity"),
                ResourceQuantity = ReadInt(fact, "quantity"),
                ConsequenceId = Read(fact, "consequence_id"),
                Misidentified = Read(fact, "misidentified") == "true",
                Toxic = Read(fact, "toxic") == "true",
            };
            _lastHarvest = result.Clone();
            return result;
        }

        private static D11VegetationHarvestResult Reject(string code, string message,
            D11VegetationHarvestRequest request, D11VegetationResourceDefinition resource,
            int quantity, D11VegetationIdentificationResult identification = null,
            D11VegetationMaturityStage? stage = null)
        {
            return new D11VegetationHarvestResult
            {
                Succeeded = false,
                Code = code ?? "rejected",
                Message = message ?? string.Empty,
                PlantId = resource == null ? request == null ? string.Empty : request.PlantId : resource.Id,
                IdentifiedPlantId = identification == null ? string.Empty : identification.IdentifiedPlantId,
                ItemId = resource == null ? string.Empty : resource.ItemId,
                ResourceKind = resource == null ? default(D11VegetationResourceKind) : resource.Kind,
                MaturityStage = stage ?? default(D11VegetationMaturityStage),
                ToolId = request == null ? string.Empty : request.ToolId,
                Quantity = quantity,
                IdentificationConfidence = identification == null ? 0f : identification.Confidence,
                IsCorrectIdentification = identification != null && identification.IsCorrect,
                Misidentified = identification != null && !identification.IsCorrect,
                Toxic = resource != null && resource.Toxic,
                ConsequenceId = resource != null && resource.Toxic ? "plant_poisoning" : string.Empty,
                ProtectionAreaId = request == null ? string.Empty : request.ProtectionAreaId,
                SaveRevision = request == null ? 0 : Math.Max(0L, request.SaveRevision),
                EventId = request == null ? string.Empty : request.EventId,
            };
        }

        private static D11VegetationIdentificationResult IdentifyFail(string code, string message)
        {
            return new D11VegetationIdentificationResult
            {
                Succeeded = false,
                Code = code,
                Message = message,
                Confidence = 0f,
            };
        }

        private static D11VegetationMaturityStage MaturityFor(
            D11VegetationResourceDefinition resource, int day, int seed)
        {
            int cycle = Math.Max(4, resource.MaturityCycleDays);
            int phase = PositiveMod(Math.Max(0, day) + Stable(resource.Id + ":" + seed), cycle);
            int matureStart = Math.Max(1, Math.Min(cycle - 1, resource.MatureStartDay));
            int matureEnd = Math.Min(cycle, matureStart + Math.Max(1, resource.MatureDurationDays));
            if (phase < matureStart / 2) return D11VegetationMaturityStage.Seedling;
            if (phase < matureStart) return D11VegetationMaturityStage.Growing;
            if (phase < matureEnd) return D11VegetationMaturityStage.Mature;
            return D11VegetationMaturityStage.Overripe;
        }

        private static D11VegetationQuality QualityFor(D11VegetationMaturityStage stage,
            float confidence, bool misidentified)
        {
            if (stage == D11VegetationMaturityStage.Overripe || misidentified)
                return D11VegetationQuality.Poor;
            return confidence >= 0.85f ? D11VegetationQuality.Good : D11VegetationQuality.Standard;
        }

        private static string LookalikeFor(D11VegetationResourceDefinition resource, int day, int seed)
        {
            var candidates = new List<string>();
            foreach (var candidate in D11VegetationHarvestCatalog.Resources)
                if (candidate != null && candidate.Id != resource.Id && candidate.HabitatId == resource.HabitatId)
                    candidates.Add(candidate.Id);
            if (candidates.Count == 0) return "unknown_plant";
            return candidates[PositiveMod(Stable(resource.Id + ":lookalike:" + day + ":" + seed),
                candidates.Count)];
        }

        private static int SafeMultiply(int left, int right)
        {
            long value = (long)Math.Max(0, left) * Math.Max(0, right);
            return value > int.MaxValue ? int.MaxValue : (int)value;
        }

        private static int PositiveMod(int value, int divisor)
        {
            if (divisor <= 0) return 0;
            int result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "none";
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                if (!(char.IsLetterOrDigit(chars[i]) || chars[i] == '_' || chars[i] == '-')) chars[i] = '_';
            return new string(chars);
        }

        private static int Stable(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in value ?? string.Empty) hash = (hash ^ c) * 16777619u;
                return (int)(hash & 0x7fffffff);
            }
        }

        private static float Clamp01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(1f, value));
        }

        private static string Read(FactEvent fact, string key)
        {
            if (fact == null || fact.Result == null || fact.Result.Values == null) return string.Empty;
            return fact.Result.Values.TryGetValue(key, out string value) ? value : string.Empty;
        }

        private static int ReadInt(FactEvent fact, string key)
        {
            return int.TryParse(Read(fact, key), NumberStyles.Integer, CultureInfo.InvariantCulture,
                out int result) ? Math.Max(0, result) : 0;
        }
    }
}
