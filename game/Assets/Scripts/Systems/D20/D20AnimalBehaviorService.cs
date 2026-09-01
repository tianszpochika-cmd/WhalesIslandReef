// ============================================================
// D20AnimalBehaviorService —— D20-L30 动物行为聚合服务（纯 C#）
//
// 行为投影由 D20-L22 状态机写入；野生生态、水域遭遇和宠物生命/照料事实
// 只通过已发布的 D20-L23/L24/L25 public API 读取或提交 owner request。
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace Whalebone.Systems.D20
{
    /// <summary>D20-L30 动物行为聚合命令。</summary>
    [Serializable]
    public sealed class D20AnimalBehaviorCommand
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string CommandId = string.Empty;
        public string ActorId = string.Empty;
        public string AnimalId = string.Empty;
        public D20AnimalIdentity Identity;
        public D20AnimalSpeciesDefinition Species;
        public D20AnimalBehaviorInput BehaviorInput;

        public static D20AnimalBehaviorCommand Create(
            D20AnimalBehaviorInput input, string actorId = "")
        {
            return new D20AnimalBehaviorCommand
            {
                CommandId = input == null ? string.Empty : input.CommandId ?? string.Empty,
                ActorId = actorId ?? string.Empty,
                AnimalId = input == null ? string.Empty : input.AnimalId ?? string.Empty,
                BehaviorInput = input == null ? null : input.Clone(),
            };
        }

        public D20AnimalBehaviorCommand Clone()
        {
            return new D20AnimalBehaviorCommand
            {
                SchemaVersion = SchemaVersion,
                CommandId = CommandId ?? string.Empty,
                ActorId = ActorId ?? string.Empty,
                AnimalId = AnimalId ?? string.Empty,
                Identity = Identity == null ? null : Identity.Clone(),
                Species = Species == null ? null : Species.Clone(),
                BehaviorInput = BehaviorInput == null ? null : BehaviorInput.Clone(),
            };
        }

        public string Fingerprint()
        {
            var builder = new StringBuilder("d20-animal-command-v1|");
            Append(builder, ActorId);
            Append(builder, AnimalId);
            if (Identity != null)
            {
                Append(builder, Identity.ContentId);
                Append(builder, Identity.SpeciesId);
                Append(builder, ((int)Identity.PopulationKind).ToString());
                Append(builder, Identity.OwnerId);
            }
            if (Species != null)
            {
                Append(builder, Species.ContentId);
                Append(builder, Species.BehaviorProfileId);
            }
            if (BehaviorInput != null)
            {
                Append(builder, BehaviorInput.CommandId);
                Append(builder, ((int)BehaviorInput.Kind).ToString());
                Append(builder, BehaviorInput.TargetId);
                Append(builder, BehaviorInput.CauseEventId);
                Append(builder, BehaviorInput.WorldDay.ToString());
                Append(builder, BehaviorInput.SaveRevision.ToString());
                Append(builder, BehaviorInput.RecoveryConfirmed ? "1" : "0");
            }
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string value)
        {
            string text = value ?? string.Empty;
            builder.Append(text.Length).Append(':').Append(text).Append('|');
        }
    }

    /// <summary>动物、宠物和生态读模型的聚合快照。</summary>
    [Serializable]
    public sealed class D20AnimalBehaviorReadModel
    {
        public int SchemaVersion = D20AnimalBehaviorCommand.CurrentSchemaVersion;
        public string AnimalId = string.Empty;
        public D20AnimalIdentity Identity;
        public D20AnimalSpeciesDefinition Species;
        public D20AnimalBehaviorSnapshot Behavior;
        public D20WildlifePopulationReadModel Wildlife;
        public D20AquaticBehaviorReadModel Aquatic;
        public D20PetFollowEligibility PetFollowEligibility;

        public D20AnimalBehaviorReadModel Clone()
        {
            return new D20AnimalBehaviorReadModel
            {
                SchemaVersion = SchemaVersion,
                AnimalId = AnimalId ?? string.Empty,
                Identity = Identity == null ? null : Identity.Clone(),
                Species = Species == null ? null : Species.Clone(),
                Behavior = Behavior == null ? null : Behavior.Clone(),
                Wildlife = Wildlife == null ? null : Wildlife.Clone(),
                Aquatic = Aquatic == null ? null : Aquatic.Clone(),
                PetFollowEligibility = PetFollowEligibility == null
                    ? null : PetFollowEligibility.Clone(),
            };
        }
    }

    /// <summary>动物行为聚合结果。</summary>
    [Serializable]
    public sealed class D20AnimalBehaviorServiceResult
    {
        public bool Accepted;
        public bool Succeeded;
        public bool IsDuplicate;
        public string CommandId = string.Empty;
        public string AnimalId = string.Empty;
        public string RequestFingerprint = string.Empty;
        public D20AnimalBehaviorResult BehaviorResult;
        public D20AnimalBehaviorReadModel Projection;
        public string ReasonCode = string.Empty;

        public D20AnimalBehaviorServiceResult Clone()
        {
            return new D20AnimalBehaviorServiceResult
            {
                Accepted = Accepted,
                Succeeded = Succeeded,
                IsDuplicate = IsDuplicate,
                CommandId = CommandId ?? string.Empty,
                AnimalId = AnimalId ?? string.Empty,
                RequestFingerprint = RequestFingerprint ?? string.Empty,
                BehaviorResult = BehaviorResult == null ? null : BehaviorResult.Clone(),
                Projection = Projection == null ? null : Projection.Clone(),
                ReasonCode = ReasonCode ?? string.Empty,
            };
        }
    }

    /// <summary>
    /// D20-L30 动物行为唯一聚合入口。它不裁决生态压力、生命周期、宠物照料
    /// 或任何 D11/D4 owner 事实。
    /// </summary>
    public sealed class D20AnimalBehaviorService
    {
        public const string OwnerSystem = "D20AnimalBehaviorService";

        private readonly Dictionary<string, D20AnimalBehaviorStateMachine> _behaviors =
            new Dictionary<string, D20AnimalBehaviorStateMachine>(StringComparer.Ordinal);
        private readonly Dictionary<string, D20AnimalIdentity> _identities =
            new Dictionary<string, D20AnimalIdentity>(StringComparer.Ordinal);
        private readonly Dictionary<string, D20AnimalSpeciesDefinition> _species =
            new Dictionary<string, D20AnimalSpeciesDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, D20AnimalBehaviorServiceResult> _receipts =
            new Dictionary<string, D20AnimalBehaviorServiceResult>(StringComparer.Ordinal);
        private readonly D20PetOwnershipSystem _petOwnership;
        private readonly D20WildlifePopulationAdapter _wildlifeAdapter;
        private readonly D20AquaticBehaviorAdapter _aquaticAdapter;

        public D20AnimalBehaviorService(
            D20PetOwnershipSystem petOwnership = null,
            D20WildlifePopulationAdapter wildlifeAdapter = null,
            D20AquaticBehaviorAdapter aquaticAdapter = null)
        {
            _petOwnership = petOwnership;
            _wildlifeAdapter = wildlifeAdapter ?? new D20WildlifePopulationAdapter();
            _aquaticAdapter = aquaticAdapter ?? new D20AquaticBehaviorAdapter();
        }

        public D20PetOwnershipSystem PetOwnership { get { return _petOwnership; } }
        public D20WildlifePopulationAdapter WildlifeAdapter { get { return _wildlifeAdapter; } }
        public D20AquaticBehaviorAdapter AquaticAdapter { get { return _aquaticAdapter; } }

        /// <summary>注册身份、物种元数据和可选的 D20 行为存档投影。</summary>
        public void Register(D20AnimalIdentity identity,
            D20AnimalSpeciesDefinition species = null,
            D20AnimalBehaviorSnapshot initialSnapshot = null)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            identity.Validate();
            D20AnimalSpeciesDefinition resolved = species;
            if (resolved == null)
                D20AnimalSpeciesCatalog.TryGetDefinition(identity.ContentId, out resolved);
            if (resolved != null)
            {
                resolved.Validate();
                if (!string.Equals(resolved.ContentId, identity.ContentId,
                    StringComparison.Ordinal))
                    throw new InvalidOperationException("Animal identity and species content_id must match.");
                _species[identity.AnimalId] = resolved.Clone();
            }
            if (initialSnapshot != null)
            {
                var snapshot = initialSnapshot.Clone();
                snapshot.Validate();
                if (!string.Equals(snapshot.AnimalId, identity.AnimalId,
                    StringComparison.Ordinal))
                    throw new InvalidOperationException("Animal identity and behavior IDs must match.");
                _behaviors[identity.AnimalId] = new D20AnimalBehaviorStateMachine(
                    snapshot.AnimalId, resolved, snapshot.State, snapshot.FollowTargetId);
                _behaviors[identity.AnimalId].Restore(snapshot);
            }
            else if (!_behaviors.ContainsKey(identity.AnimalId))
            {
                _behaviors[identity.AnimalId] = new D20AnimalBehaviorStateMachine(
                    identity.AnimalId, resolved);
            }
            _identities[identity.AnimalId] = identity.Clone();
        }

        /// <summary>应用行为输入；重复命令只回放已有结果。</summary>
        public D20AnimalBehaviorServiceResult Execute(D20AnimalBehaviorCommand command)
        {
            if (command == null)
                return Failure(string.Empty, string.Empty, "animal_behavior_command_missing");
            if (command.SchemaVersion != D20AnimalBehaviorCommand.CurrentSchemaVersion)
                return Failure(command.CommandId, command.AnimalId,
                    "animal_behavior_command_schema_version_mismatch");
            if (!D20AnimalIdentityContracts.IsStableSpeciesId(command.CommandId))
                return Failure(command.CommandId, command.AnimalId,
                    "animal_behavior_command_id_missing");
            string fingerprint = command.Fingerprint();
            D20AnimalBehaviorServiceResult previous;
            if (_receipts.TryGetValue(command.CommandId, out previous))
            {
                if (!string.Equals(previous.RequestFingerprint, fingerprint,
                    StringComparison.Ordinal))
                    return Failure(command.CommandId, command.AnimalId,
                        "animal_behavior_command_id_conflict");
                var duplicate = previous.Clone();
                duplicate.IsDuplicate = true;
                duplicate.ReasonCode = "animal_behavior_command_replayed";
                return duplicate;
            }
            if (command.BehaviorInput == null)
                return StoreFailure(command, fingerprint, "animal_behavior_input_missing");
            if (!string.Equals(command.BehaviorInput.AnimalId, command.AnimalId,
                StringComparison.Ordinal))
                return StoreFailure(command, fingerprint, "animal_behavior_animal_id_mismatch");

            if (command.Identity != null)
            {
                command.Identity.Validate();
                if (!string.Equals(command.Identity.AnimalId, command.AnimalId,
                    StringComparison.Ordinal))
                    return StoreFailure(command, fingerprint, "animal_behavior_identity_mismatch");
                Register(command.Identity, command.Species);
            }
            else if (command.Species != null && !_species.ContainsKey(command.AnimalId))
            {
                command.Species.Validate();
                _species[command.AnimalId] = command.Species.Clone();
            }

            D20AnimalBehaviorStateMachine machine;
            if (!_behaviors.TryGetValue(command.AnimalId, out machine))
            {
                D20AnimalSpeciesDefinition species;
                _species.TryGetValue(command.AnimalId, out species);
                machine = new D20AnimalBehaviorStateMachine(command.AnimalId, species);
            }
            var behavior = machine.Apply(command.BehaviorInput);
            var result = new D20AnimalBehaviorServiceResult
            {
                Accepted = behavior != null && behavior.Applied,
                Succeeded = behavior != null && behavior.Applied,
                IsDuplicate = behavior != null && behavior.Duplicate,
                CommandId = command.CommandId ?? string.Empty,
                AnimalId = command.AnimalId ?? string.Empty,
                RequestFingerprint = fingerprint,
                BehaviorResult = behavior == null ? null : behavior.Clone(),
                ReasonCode = behavior == null ? "animal_behavior_result_missing" : behavior.ReasonCode,
            };
            if (result.Accepted)
            {
                _behaviors[command.AnimalId] = machine;
                result.Projection = Read(command.AnimalId);
            }
            Store(command.CommandId, result);
            return result.Clone();
        }

        /// <summary>直接应用 D20-L22 行为输入的便捷入口。</summary>
        public D20AnimalBehaviorServiceResult Apply(D20AnimalBehaviorInput input)
        {
            return Execute(D20AnimalBehaviorCommand.Create(input));
        }

        /// <summary>读取动物、行为、生态、水域和宠物资格的防御性聚合投影。</summary>
        public D20AnimalBehaviorReadModel Read(string animalId, string habitatId = "",
            D20AquaticInteractionKind aquaticInteraction = D20AquaticInteractionKind.None,
            string petOwnerId = "")
        {
            if (string.IsNullOrEmpty(animalId)) return null;
            var result = new D20AnimalBehaviorReadModel { AnimalId = animalId };
            D20AnimalIdentity identity;
            if (_identities.TryGetValue(animalId, out identity))
                result.Identity = identity.Clone();
            D20AnimalSpeciesDefinition species;
            if (_species.TryGetValue(animalId, out species))
                result.Species = species.Clone();
            D20AnimalBehaviorStateMachine behavior;
            if (_behaviors.TryGetValue(animalId, out behavior))
                result.Behavior = behavior.CaptureSnapshot();
            result.Wildlife = _wildlifeAdapter.Read(animalId, habitatId);
            result.Aquatic = _aquaticAdapter.Read(animalId, aquaticInteraction);
            if (_petOwnership != null && !string.IsNullOrEmpty(petOwnerId))
                result.PetFollowEligibility = _petOwnership.EvaluateFollowEligibility(
                    petOwnerId, animalId);
            return result;
        }

        /// <summary>读取野生/鱼群 owner 投影，不产生事实。</summary>
        public D20WildlifePopulationReadModel ReadWildlife(string entityId,
            string habitatId = "")
        {
            return _wildlifeAdapter.Read(entityId, habitatId);
        }

        /// <summary>读取水域遭遇 owner 投影，不产生事实。</summary>
        public D20AquaticBehaviorReadModel ReadAquatic(string entityId,
            D20AquaticInteractionKind interaction = D20AquaticInteractionKind.None)
        {
            return _aquaticAdapter.Read(entityId, interaction);
        }

        /// <summary>只读评估宠物跟随资格。</summary>
        public D20PetFollowEligibility EvaluatePetFollow(string ownerId,
            string petId = "")
        {
            if (_petOwnership == null)
                return new D20PetOwnershipSystem(null).EvaluateFollowEligibility(ownerId, petId);
            return _petOwnership.EvaluateFollowEligibility(ownerId, petId);
        }

        /// <summary>捕获按动物 ID 排序的行为快照，供保存聚合器消费。</summary>
        public List<D20AnimalBehaviorSnapshot> CaptureBehaviorSnapshots()
        {
            var ids = new List<string>(_behaviors.Keys);
            ids.Sort(StringComparer.Ordinal);
            var result = new List<D20AnimalBehaviorSnapshot>();
            foreach (string id in ids)
                result.Add(_behaviors[id].CaptureSnapshot());
            return result;
        }

        private void Store(string commandId, D20AnimalBehaviorServiceResult result)
        {
            if (D20AnimalIdentityContracts.IsStableSpeciesId(commandId))
                _receipts[commandId] = result.Clone();
        }

        private D20AnimalBehaviorServiceResult StoreFailure(
            D20AnimalBehaviorCommand command, string fingerprint, string reasonCode)
        {
            var result = Failure(command.CommandId, command.AnimalId, reasonCode);
            result.RequestFingerprint = fingerprint;
            Store(command.CommandId, result);
            return result.Clone();
        }

        private static D20AnimalBehaviorServiceResult Failure(string commandId,
            string animalId, string reasonCode)
        {
            return new D20AnimalBehaviorServiceResult
            {
                Accepted = false,
                Succeeded = false,
                CommandId = commandId ?? string.Empty,
                AnimalId = animalId ?? string.Empty,
                ReasonCode = reasonCode ?? string.Empty,
            };
        }
    }
}
