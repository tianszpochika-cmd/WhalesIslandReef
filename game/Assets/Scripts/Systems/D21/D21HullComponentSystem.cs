// ============================================================
// D21HullComponentSystem —— D21-L03 船体组件状态与组合（纯 C#）
//
// 本系统只拥有逻辑组件的安装/移除/启用状态和组合不变式。船型内容属性、
// 船况数值、货物数量、港口泊位和表现锚点分别由所属 owner 提供或写入；
// 这里不保存模型、Collider、Transform、Prefab 或物理结果。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Whalebone.Systems.D21
{
    /// <summary>船体组件的逻辑类别。</summary>
    public enum D21HullComponentKind
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>主船体/龙骨组件。</summary>
        Hull = 1,
        /// <summary>桅杆或桅杆承载组件。</summary>
        Mast = 2,
        /// <summary>帆面组件。</summary>
        Sail = 3,
        /// <summary>索具组件。</summary>
        Rigging = 4,
        /// <summary>舵叶/舵柄逻辑组件。</summary>
        Rudder = 5,
        /// <summary>锚和锚链组件。</summary>
        Anchor = 6,
        /// <summary>货舱组件。</summary>
        CargoHold = 7,
        /// <summary>划行动力组件。</summary>
        OarDrive = 8,
    }

    /// <summary>组件可用状态；不携带耐久数值，损伤由 L04 拥有。</summary>
    public enum D21HullComponentOperationalState
    {
        /// <summary>组件不存在。</summary>
        Missing = 0,
        /// <summary>组件已安装且可用。</summary>
        Available = 1,
        /// <summary>组件已安装但被逻辑禁用。</summary>
        Disabled = 2,
        /// <summary>组件已安装并进入维修流程。</summary>
        UnderRepair = 3,
        /// <summary>组件已拆除，仅允许作为迁移历史。</summary>
        Removed = 4,
    }

    /// <summary>船只动力组合；通过枚举避免用多个 bool 形成非法状态。</summary>
    public enum D21PropulsionMode
    {
        /// <summary>未初始化。</summary>
        Unknown = 0,
        /// <summary>纯帆动力。</summary>
        Sail = 1,
        /// <summary>纯划行动力。</summary>
        Row = 2,
        /// <summary>帆与划行混合动力。</summary>
        Mixed = 3,
        /// <summary>没有有效动力，只允许漂泊语义。</summary>
        Drift = 4,
    }

    /// <summary>组件 owner 支持的状态命令。</summary>
    public enum D21HullComponentCommandKind
    {
        /// <summary>未初始化。</summary>
        None = 0,
        /// <summary>安装一个新组件。</summary>
        Install = 1,
        /// <summary>移除一个已安装组件。</summary>
        Remove = 2,
        /// <summary>改变组件的逻辑可用状态。</summary>
        SetOperationalState = 3,
    }

    /// <summary>组件校验/命令的稳定失败码。</summary>
    public enum D21HullComponentFailureCode
    {
        /// <summary>无失败。</summary>
        None = 0,
        /// <summary>命令缺少 ID。</summary>
        MissingCommandId = 1,
        /// <summary>命令 ID 或船 ID 格式非法。</summary>
        InvalidReference = 2,
        /// <summary>命令种类未知。</summary>
        UnsupportedCommand = 3,
        /// <summary>组件载体缺失。</summary>
        MissingComponent = 4,
        /// <summary>组件已经存在。</summary>
        ComponentAlreadyInstalled = 5,
        /// <summary>组件不存在。</summary>
        ComponentNotFound = 6,
        /// <summary>不能删除航行必需组件。</summary>
        RequiredComponentMissing = 7,
        /// <summary>组件组合不合法。</summary>
        IllegalCombination = 8,
        /// <summary>组件与船型不兼容。</summary>
        ShipTypeMismatch = 9,
        /// <summary>命令修订号不是当前修订。</summary>
        SaveRevisionMismatch = 10,
        /// <summary>同一命令 ID 被不同内容重用。</summary>
        CommandIdReused = 11,
        /// <summary>枚举或组件状态未知。</summary>
        InvalidState = 12,
    }

    /// <summary>组件定义，描述组合校验所需的静态约束；不含表现路径。</summary>
    [Serializable]
    public sealed class D21HullComponentDefinition
    {
        /// <summary>组件定义稳定 ID。</summary>
        public string DefinitionId = string.Empty;
        /// <summary>组件类别。</summary>
        public D21HullComponentKind Kind;
        /// <summary>是否是该配置必需的组件。</summary>
        public bool Required;
        /// <summary>是否允许同一配置安装多个同定义组件。</summary>
        public bool AllowMultiple;
        /// <summary>该定义的最小安装数量。</summary>
        public int MinCount;
        /// <summary>该定义的最大安装数量；0 表示使用类别默认上限。</summary>
        public int MaxCount;
        /// <summary>提供给货舱规则读取的逻辑容量单位。</summary>
        public int CapacityUnits;
        /// <summary>组件占用的逻辑重量单位。</summary>
        public int WeightUnits;
        /// <summary>对吃水的逻辑增量，单位为厘米。</summary>
        public int DraftChangeCentimeters;
        /// <summary>安装前必须存在的其它组件定义 ID。</summary>
        public List<string> RequiredDefinitionIds = new List<string>();
        /// <summary>不能同时安装的组件定义 ID。</summary>
        public List<string> ConflictingDefinitionIds = new List<string>();
        /// <summary>允许该组件的船型 ID；空集合表示由内容 owner 另行放行。</summary>
        public List<string> CompatibleShipTypeIds = new List<string>();

        /// <summary>供序列化器创建空定义。</summary>
        public D21HullComponentDefinition() { }

        /// <summary>创建组件定义。</summary>
        public D21HullComponentDefinition(string definitionId, D21HullComponentKind kind,
            bool required = false, bool allowMultiple = false)
        {
            DefinitionId = definitionId ?? string.Empty;
            Kind = kind;
            Required = required;
            AllowMultiple = allowMultiple;
            MinCount = required ? 1 : 0;
        }

        /// <summary>返回深拷贝。</summary>
        public D21HullComponentDefinition Clone()
        {
            var copy = new D21HullComponentDefinition
            {
                DefinitionId = DefinitionId ?? string.Empty,
                Kind = Kind,
                Required = Required,
                AllowMultiple = AllowMultiple,
                MinCount = MinCount,
                MaxCount = MaxCount,
                CapacityUnits = CapacityUnits,
                WeightUnits = WeightUnits,
                DraftChangeCentimeters = DraftChangeCentimeters,
            };
            if (RequiredDefinitionIds != null) copy.RequiredDefinitionIds.AddRange(RequiredDefinitionIds);
            if (ConflictingDefinitionIds != null) copy.ConflictingDefinitionIds.AddRange(ConflictingDefinitionIds);
            if (CompatibleShipTypeIds != null) copy.CompatibleShipTypeIds.AddRange(CompatibleShipTypeIds);
            return copy;
        }
    }

    /// <summary>一个已安装船体组件的逻辑状态。</summary>
    [Serializable]
    public sealed class D21HullComponentState
    {
        /// <summary>组件实例稳定 ID。</summary>
        public string ComponentId = string.Empty;
        /// <summary>指向静态定义的稳定 ID；为空时使用实例字段。</summary>
        public string DefinitionId = string.Empty;
        /// <summary>组件类别。</summary>
        public D21HullComponentKind Kind;
        /// <summary>组件是否为航行必需。</summary>
        public bool RequiredForNavigation;
        /// <summary>组件当前逻辑可用状态。</summary>
        public D21HullComponentOperationalState OperationalState =
            D21HullComponentOperationalState.Available;
        /// <summary>货舱组件提供的容量单位。</summary>
        public int CapacityUnits;
        /// <summary>组件逻辑重量单位。</summary>
        public int WeightUnits;
        /// <summary>组件导致的逻辑吃水增量，单位为厘米。</summary>
        public int DraftChangeCentimeters;

        /// <summary>返回深拷贝。</summary>
        public D21HullComponentState Clone()
        {
            return new D21HullComponentState
            {
                ComponentId = ComponentId ?? string.Empty,
                DefinitionId = DefinitionId ?? string.Empty,
                Kind = Kind,
                RequiredForNavigation = RequiredForNavigation,
                OperationalState = OperationalState,
                CapacityUnits = CapacityUnits,
                WeightUnits = WeightUnits,
                DraftChangeCentimeters = DraftChangeCentimeters,
            };
        }
    }

    /// <summary>船体组件配置的可保存纯逻辑投影。</summary>
    [Serializable]
    public sealed class D21HullComponentConfiguration
    {
        /// <summary>配置契约版本。</summary>
        public int SchemaVersion = D21HullComponentSystem.CurrentSchemaVersion;
        /// <summary>船只稳定 ID。</summary>
        public string ShipId = string.Empty;
        /// <summary>船型稳定 ID。</summary>
        public string ShipTypeId = string.Empty;
        /// <summary>动力组合。</summary>
        public D21PropulsionMode PropulsionMode = D21PropulsionMode.Sail;
        /// <summary>已安装组件实例。</summary>
        public List<D21HullComponentState> Components = new List<D21HullComponentState>();
        /// <summary>可选的静态定义快照；定义仍是只读输入。</summary>
        public List<D21HullComponentDefinition> Definitions = new List<D21HullComponentDefinition>();
        /// <summary>最后一次成功变更的存档修订号。</summary>
        public long SaveRevision;
        /// <summary>最后一个已应用组件事实事件 ID。</summary>
        public string LastEventId = string.Empty;

        /// <summary>返回深拷贝。</summary>
        public D21HullComponentConfiguration Clone()
        {
            var copy = new D21HullComponentConfiguration
            {
                SchemaVersion = SchemaVersion,
                ShipId = ShipId ?? string.Empty,
                ShipTypeId = ShipTypeId ?? string.Empty,
                PropulsionMode = PropulsionMode,
                SaveRevision = SaveRevision,
                LastEventId = LastEventId ?? string.Empty,
            };
            if (Components != null)
            {
                for (int i = 0; i < Components.Count; i++)
                    copy.Components.Add(Components[i] == null
                        ? new D21HullComponentState()
                        : Components[i].Clone());
            }
            if (Definitions != null)
            {
                for (int i = 0; i < Definitions.Count; i++)
                    copy.Definitions.Add(Definitions[i] == null
                        ? new D21HullComponentDefinition()
                        : Definitions[i].Clone());
            }
            return copy;
        }
    }

    /// <summary>组件配置校验结果；错误集合是稳定顺序的机器可读原因码。</summary>
    [Serializable]
    public sealed class D21HullComponentValidationResult
    {
        /// <summary>是否通过校验。</summary>
        public bool Valid;
        /// <summary>首个主要失败码。</summary>
        public D21HullComponentFailureCode Code;
        /// <summary>首个稳定失败原因。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>所有发现的稳定失败原因。</summary>
        public List<string> Errors = new List<string>();

        /// <summary>创建成功结果。</summary>
        public static D21HullComponentValidationResult Success()
        {
            return new D21HullComponentValidationResult
            {
                Valid = true,
                Code = D21HullComponentFailureCode.None,
                ReasonCode = "hull_configuration_valid",
            };
        }

        /// <summary>创建单一失败结果。</summary>
        public static D21HullComponentValidationResult Failure(
            D21HullComponentFailureCode code, string reasonCode)
        {
            var result = new D21HullComponentValidationResult
            {
                Valid = false,
                Code = code,
                ReasonCode = reasonCode ?? "hull_configuration_invalid",
            };
            result.Errors.Add(result.ReasonCode);
            return result;
        }

        /// <summary>返回深拷贝。</summary>
        public D21HullComponentValidationResult Clone()
        {
            var copy = new D21HullComponentValidationResult
            {
                Valid = Valid,
                Code = Code,
                ReasonCode = ReasonCode ?? string.Empty,
            };
            if (Errors != null) copy.Errors.AddRange(Errors);
            return copy;
        }

        private static D21HullComponentValidationResult FromErrors(
            D21HullComponentFailureCode code, List<string> errors)
        {
            var result = new D21HullComponentValidationResult
            {
                Valid = errors == null || errors.Count == 0,
                Code = errors == null || errors.Count == 0
                    ? D21HullComponentFailureCode.None
                    : code,
                ReasonCode = errors == null || errors.Count == 0
                    ? "hull_configuration_valid"
                    : errors[0],
            };
            if (errors != null) result.Errors.AddRange(errors);
            return result;
        }

        internal static D21HullComponentValidationResult Build(
            D21HullComponentFailureCode code, List<string> errors)
        {
            return FromErrors(code, errors);
        }
    }

    /// <summary>组件变更命令；不携带物理、表现或 D4 私有字段。</summary>
    [Serializable]
    public sealed class D21HullComponentCommand
    {
        /// <summary>跨重试不变的稳定命令 ID。</summary>
        public string CommandId = string.Empty;
        /// <summary>命令种类。</summary>
        public D21HullComponentCommandKind Kind;
        /// <summary>目标船只稳定 ID。</summary>
        public string ShipId = string.Empty;
        /// <summary>安装命令携带的组件实例。</summary>
        public D21HullComponentState Component;
        /// <summary>移除或改状态命令使用的组件 ID。</summary>
        public string ComponentId = string.Empty;
        /// <summary>改状态命令使用的目标状态。</summary>
        public D21HullComponentOperationalState NextOperationalState;
        /// <summary>调用方声明的世界日。</summary>
        public int WorldDay;
        /// <summary>调用方声明的当前存档修订号。</summary>
        public long SaveRevision;
        /// <summary>可选的上游事实因果引用。</summary>
        public string CauseEventId = string.Empty;

        /// <summary>返回深拷贝。</summary>
        public D21HullComponentCommand Clone()
        {
            return new D21HullComponentCommand
            {
                CommandId = CommandId ?? string.Empty,
                Kind = Kind,
                ShipId = ShipId ?? string.Empty,
                Component = Component == null ? null : Component.Clone(),
                ComponentId = ComponentId ?? string.Empty,
                NextOperationalState = NextOperationalState,
                WorldDay = WorldDay,
                SaveRevision = SaveRevision,
                CauseEventId = CauseEventId ?? string.Empty,
            };
        }

        /// <summary>生成不依赖运行时哈希的确定性指纹。</summary>
        public string Fingerprint()
        {
            var component = Component;
            return (CommandId ?? string.Empty) + "|" + Kind + "|" +
                   (ShipId ?? string.Empty) + "|" +
                   (component == null ? string.Empty : component.ComponentId ?? string.Empty) + "|" +
                   (component == null ? string.Empty : component.DefinitionId ?? string.Empty) + "|" +
                   (component == null ? D21HullComponentKind.Unknown.ToString() : component.Kind.ToString()) + "|" +
                   (ComponentId ?? string.Empty) + "|" + NextOperationalState + "|" +
                   WorldDay.ToString(CultureInfo.InvariantCulture) + "|" +
                   SaveRevision.ToString(CultureInfo.InvariantCulture) + "|" +
                   (CauseEventId ?? string.Empty);
        }
    }

    /// <summary>组件命令执行结果和新的只读配置。</summary>
    [Serializable]
    public sealed class D21HullComponentResult
    {
        /// <summary>命令是否被 owner 接受。</summary>
        public bool Accepted;
        /// <summary>是否命中同一命令的幂等重放。</summary>
        public bool Duplicate;
        /// <summary>命令 ID。</summary>
        public string CommandId = string.Empty;
        /// <summary>船只 ID。</summary>
        public string ShipId = string.Empty;
        /// <summary>稳定原因码。</summary>
        public string ReasonCode = string.Empty;
        /// <summary>执行后的配置副本。</summary>
        public D21HullComponentConfiguration Configuration;
        /// <summary>执行后的存档修订号。</summary>
        public long SaveRevision;
        /// <summary>成功变更对应的事实事件 ID。</summary>
        public string EventId = string.Empty;

        /// <summary>返回深拷贝。</summary>
        public D21HullComponentResult Clone()
        {
            return new D21HullComponentResult
            {
                Accepted = Accepted,
                Duplicate = Duplicate,
                CommandId = CommandId ?? string.Empty,
                ShipId = ShipId ?? string.Empty,
                ReasonCode = ReasonCode ?? string.Empty,
                Configuration = Configuration == null ? null : Configuration.Clone(),
                SaveRevision = SaveRevision,
                EventId = EventId ?? string.Empty,
            };
        }
    }

    /// <summary>D21-L03 船体组件唯一逻辑写者。</summary>
    public sealed class D21HullComponentSystem
    {
        /// <summary>当前组件投影版本。</summary>
        public const int CurrentSchemaVersion = 1;
        /// <summary>组件状态 owner 标识。</summary>
        public const string OwnerSystem = "D21HullComponentSystem";

        private readonly D21HullComponentConfiguration _state;
        private readonly Dictionary<string, string> _commandFingerprints =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, D21HullComponentResult> _commandResults =
            new Dictionary<string, D21HullComponentResult>(StringComparer.Ordinal);

        /// <summary>使用已有投影恢复组件 owner；不合法投影由后续 Apply 拒绝写入。</summary>
        public D21HullComponentSystem(D21HullComponentConfiguration initial = null)
        {
            _state = initial == null
                ? new D21HullComponentConfiguration()
                : initial.Clone();
        }

        /// <summary>返回组件配置的防御性副本。</summary>
        public D21HullComponentConfiguration Snapshot
        {
            get { return _state.Clone(); }
        }

        /// <summary>构造可用于教学/冒烟测试的最小帆船配置。</summary>
        public static D21HullComponentConfiguration CreateStarterConfiguration(
            string shipId, string shipTypeId)
        {
            var configuration = new D21HullComponentConfiguration
            {
                ShipId = shipId ?? string.Empty,
                ShipTypeId = shipTypeId ?? string.Empty,
                PropulsionMode = D21PropulsionMode.Sail,
            };
            configuration.Components.Add(Component("hull.main", D21HullComponentKind.Hull, true));
            configuration.Components.Add(Component("mast.main", D21HullComponentKind.Mast, true));
            configuration.Components.Add(Component("sail.main", D21HullComponentKind.Sail, true));
            configuration.Components.Add(Component("rudder.primary", D21HullComponentKind.Rudder, true));
            configuration.Components.Add(Component("anchor.primary", D21HullComponentKind.Anchor, true));
            configuration.Components.Add(new D21HullComponentState
            {
                ComponentId = "cargo.main",
                Kind = D21HullComponentKind.CargoHold,
                OperationalState = D21HullComponentOperationalState.Available,
                CapacityUnits = 1,
            });
            return configuration;
        }

        /// <summary>校验任意配置的结构、必需件、动力和定义冲突。</summary>
        public static D21HullComponentValidationResult ValidateConfiguration(
            D21HullComponentConfiguration configuration)
        {
            var errors = new List<string>();
            if (configuration == null)
                return D21HullComponentValidationResult.Failure(
                    D21HullComponentFailureCode.IllegalCombination, "hull_configuration_missing");
            if (configuration.SchemaVersion != CurrentSchemaVersion)
                errors.Add("hull_schema_version_mismatch");
            if (!D21ShipIdentityContracts.IsStableId(configuration.ShipId))
                errors.Add("hull_ship_id_invalid");
            if (!D21ShipIdentityContracts.IsStableId(configuration.ShipTypeId))
                errors.Add("hull_ship_type_id_invalid");
            if (configuration.PropulsionMode < D21PropulsionMode.Sail ||
                configuration.PropulsionMode > D21PropulsionMode.Drift)
                errors.Add("hull_propulsion_mode_invalid");
            if (configuration.Components == null)
                errors.Add("hull_components_missing");

            var componentIds = new HashSet<string>(StringComparer.Ordinal);
            int hullCount = 0;
            int rudderCount = 0;
            int anchorCount = 0;
            int mastCount = 0;
            int sailCount = 0;
            int oarCount = 0;
            var definitions = new Dictionary<string, D21HullComponentDefinition>(StringComparer.Ordinal);
            if (configuration.Definitions != null)
            {
                for (int i = 0; i < configuration.Definitions.Count; i++)
                {
                    D21HullComponentDefinition definition = configuration.Definitions[i];
                    if (definition == null || !D21ShipIdentityContracts.IsStableId(
                        definition.DefinitionId))
                    {
                        errors.Add("hull_definition_id_invalid");
                        continue;
                    }
                    if (!definitions.ContainsKey(definition.DefinitionId))
                        definitions.Add(definition.DefinitionId, definition);
                    else
                        errors.Add("hull_duplicate_definition_" + definition.DefinitionId);
                    if (definition.Kind == D21HullComponentKind.Unknown)
                        errors.Add("hull_definition_kind_invalid_" + definition.DefinitionId);
                    if (definition.MinCount < 0 || definition.MaxCount < 0 ||
                        (definition.MaxCount > 0 && definition.MinCount > definition.MaxCount) ||
                        definition.CapacityUnits < 0 || definition.WeightUnits < 0 ||
                        definition.DraftChangeCentimeters < 0)
                        errors.Add("hull_definition_bounds_invalid_" + definition.DefinitionId);
                }
            }

            if (configuration.Components != null)
            {
                for (int i = 0; i < configuration.Components.Count; i++)
                {
                    D21HullComponentState component = configuration.Components[i];
                    if (component == null || !D21ShipIdentityContracts.IsStableId(
                        component.ComponentId))
                    {
                        errors.Add("hull_component_id_invalid");
                        continue;
                    }
                    if (!componentIds.Add(component.ComponentId))
                        errors.Add("hull_duplicate_component_" + component.ComponentId);
                    if (component.Kind == D21HullComponentKind.Unknown)
                        errors.Add("hull_component_kind_invalid_" + component.ComponentId);
                    if (component.OperationalState == D21HullComponentOperationalState.Missing ||
                        component.OperationalState == D21HullComponentOperationalState.Removed)
                        errors.Add("hull_component_not_installed_" + component.ComponentId);
                    if (component.CapacityUnits < 0 || component.WeightUnits < 0 ||
                        component.DraftChangeCentimeters < 0)
                        errors.Add("hull_component_bounds_invalid_" + component.ComponentId);
                    if (component.RequiredForNavigation &&
                        component.OperationalState != D21HullComponentOperationalState.Available)
                        errors.Add("hull_required_component_unavailable_" + component.ComponentId);

                    switch (component.Kind)
                    {
                        case D21HullComponentKind.Hull:
                            hullCount++;
                            if (component.OperationalState != D21HullComponentOperationalState.Available)
                                errors.Add("hull_primary_unavailable_" + component.ComponentId);
                            break;
                        case D21HullComponentKind.Rudder:
                            rudderCount++;
                            if (component.OperationalState != D21HullComponentOperationalState.Available)
                                errors.Add("hull_rudder_unavailable_" + component.ComponentId);
                            break;
                        case D21HullComponentKind.Anchor:
                            anchorCount++;
                            if (component.OperationalState != D21HullComponentOperationalState.Available)
                                errors.Add("hull_anchor_unavailable_" + component.ComponentId);
                            break;
                        case D21HullComponentKind.Mast:
                            if (component.OperationalState == D21HullComponentOperationalState.Available)
                                mastCount++;
                            break;
                        case D21HullComponentKind.Sail:
                            if (component.OperationalState == D21HullComponentOperationalState.Available)
                                sailCount++;
                            break;
                        case D21HullComponentKind.OarDrive:
                            if (component.OperationalState == D21HullComponentOperationalState.Available)
                                oarCount++;
                            break;
                    }

                    D21HullComponentDefinition definition;
                    if (!string.IsNullOrEmpty(component.DefinitionId) &&
                        definitions.TryGetValue(component.DefinitionId, out definition))
                    {
                        if (definition.Kind != component.Kind)
                            errors.Add("hull_definition_kind_mismatch_" + component.ComponentId);
                        if (!definition.AllowMultiple && CountDefinition(configuration.Components,
                            component.DefinitionId) > 1)
                            errors.Add("hull_definition_not_multiple_" + component.DefinitionId);
                        if (definition.CompatibleShipTypeIds != null &&
                            definition.CompatibleShipTypeIds.Count > 0 &&
                            !Contains(definition.CompatibleShipTypeIds, configuration.ShipTypeId))
                            errors.Add("hull_ship_type_mismatch_" + component.ComponentId);
                    }
                }
            }

            if (hullCount != 1) errors.Add("hull_primary_count_invalid");
            if (rudderCount != 1) errors.Add("hull_rudder_count_invalid");
            if (anchorCount < 1) errors.Add("hull_anchor_missing");
            if ((configuration.PropulsionMode == D21PropulsionMode.Sail ||
                 configuration.PropulsionMode == D21PropulsionMode.Mixed) && sailCount < 1)
                errors.Add("hull_sail_missing");
            if ((configuration.PropulsionMode == D21PropulsionMode.Row ||
                 configuration.PropulsionMode == D21PropulsionMode.Mixed) && oarCount < 1)
                errors.Add("hull_oar_drive_missing");
            if (configuration.PropulsionMode != D21PropulsionMode.Drift &&
                configuration.PropulsionMode != D21PropulsionMode.Row && mastCount < 1)
                errors.Add("hull_mast_missing");

            foreach (KeyValuePair<string, D21HullComponentDefinition> pair in definitions)
            {
                D21HullComponentDefinition definition = pair.Value;
                int count = CountDefinition(configuration.Components, definition.DefinitionId);
                int minCount = definition.Required ? Math.Max(1, definition.MinCount) : definition.MinCount;
                if (count < minCount)
                    errors.Add("hull_required_definition_missing_" + definition.DefinitionId);
                if (definition.MaxCount > 0 && count > definition.MaxCount)
                    errors.Add("hull_definition_max_exceeded_" + definition.DefinitionId);
                if (definition.RequiredDefinitionIds != null && count > 0)
                {
                    for (int i = 0; i < definition.RequiredDefinitionIds.Count; i++)
                    {
                        string requiredId = definition.RequiredDefinitionIds[i];
                        if (!string.IsNullOrEmpty(requiredId) &&
                            CountDefinition(configuration.Components, requiredId) == 0)
                            errors.Add("hull_dependency_missing_" + definition.DefinitionId + "_" + requiredId);
                    }
                }
                if (definition.ConflictingDefinitionIds != null && count > 0)
                {
                    for (int i = 0; i < definition.ConflictingDefinitionIds.Count; i++)
                    {
                        string conflictId = definition.ConflictingDefinitionIds[i];
                        if (!string.IsNullOrEmpty(conflictId) &&
                            CountDefinition(configuration.Components, conflictId) > 0)
                            errors.Add("hull_conflict_" + definition.DefinitionId + "_" + conflictId);
                    }
                }
            }

            if (configuration.SaveRevision < 0)
                errors.Add("hull_save_revision_invalid");
            if (!string.IsNullOrEmpty(configuration.LastEventId) &&
                !D21ShipIdentityContracts.IsStableId(configuration.LastEventId))
                errors.Add("hull_last_event_id_invalid");
            return D21HullComponentValidationResult.Build(
                errors.Count == 0 ? D21HullComponentFailureCode.None
                    : D21HullComponentFailureCode.IllegalCombination, errors);
        }

        /// <summary>应用组件命令；所有拒绝均保持原状态不变。</summary>
        public D21HullComponentResult Apply(D21HullComponentCommand command)
        {
            if (command == null)
                return Reject(string.Empty, "", D21HullComponentFailureCode.InvalidReference,
                    "hull_command_missing");

            string fingerprint = command.Fingerprint();
            string previousFingerprint;
            if (!string.IsNullOrEmpty(command.CommandId) &&
                _commandFingerprints.TryGetValue(command.CommandId, out previousFingerprint))
            {
                if (string.Equals(previousFingerprint, fingerprint, StringComparison.Ordinal))
                {
                    D21HullComponentResult duplicate = _commandResults[command.CommandId].Clone();
                    duplicate.Duplicate = true;
                    duplicate.ReasonCode = "hull_command_duplicate";
                    return duplicate;
                }
                return Reject(command.CommandId, command.ShipId,
                    D21HullComponentFailureCode.CommandIdReused, "hull_command_id_reused");
            }

            string validationFailure;
            D21HullComponentFailureCode validationCode;
            if (!ValidateCommand(command, out validationCode, out validationFailure))
                return Remember(command, Reject(command.CommandId, command.ShipId,
                    validationCode, validationFailure));
            if (!string.Equals(command.ShipId, _state.ShipId, StringComparison.Ordinal))
                return Remember(command, Reject(command.CommandId, command.ShipId,
                    D21HullComponentFailureCode.InvalidReference, "hull_ship_id_mismatch"));
            if (command.SaveRevision != _state.SaveRevision)
                return Remember(command, Reject(command.CommandId, command.ShipId,
                    D21HullComponentFailureCode.SaveRevisionMismatch,
                    "hull_save_revision_mismatch"));

            var candidate = _state.Clone();
            string componentId = command.ComponentId;
            if (command.Kind == D21HullComponentCommandKind.Install)
            {
                componentId = command.Component.ComponentId;
                for (int i = 0; i < candidate.Components.Count; i++)
                {
                    if (string.Equals(candidate.Components[i].ComponentId, componentId,
                        StringComparison.Ordinal))
                        return Remember(command, Reject(command.CommandId, command.ShipId,
                            D21HullComponentFailureCode.ComponentAlreadyInstalled,
                            "hull_component_already_installed"));
                }
                candidate.Components.Add(command.Component.Clone());
            }
            else
            {
                int index = FindComponent(candidate.Components, componentId);
                if (index < 0)
                    return Remember(command, Reject(command.CommandId, command.ShipId,
                        D21HullComponentFailureCode.ComponentNotFound,
                        "hull_component_not_found"));
                if (command.Kind == D21HullComponentCommandKind.Remove)
                    candidate.Components.RemoveAt(index);
                else
                    candidate.Components[index].OperationalState = command.NextOperationalState;
            }

            D21HullComponentValidationResult configurationResult = ValidateConfiguration(candidate);
            if (!configurationResult.Valid)
                return Remember(command, Reject(command.CommandId, command.ShipId,
                    configurationResult.Code == D21HullComponentFailureCode.None
                        ? D21HullComponentFailureCode.IllegalCombination
                        : configurationResult.Code,
                    configurationResult.ReasonCode));

            candidate.SaveRevision = _state.SaveRevision + 1;
            candidate.LastEventId = "d21_hull_component_" + command.CommandId + "_" +
                candidate.SaveRevision.ToString(CultureInfo.InvariantCulture);
            CopyInto(_state, candidate);
            var result = new D21HullComponentResult
            {
                Accepted = true,
                Duplicate = false,
                CommandId = command.CommandId ?? string.Empty,
                ShipId = command.ShipId ?? string.Empty,
                ReasonCode = "hull_command_applied",
                Configuration = _state.Clone(),
                SaveRevision = _state.SaveRevision,
                EventId = _state.LastEventId,
            };
            _commandFingerprints[command.CommandId] = fingerprint;
            _commandResults[command.CommandId] = result.Clone();
            return result;
        }

        /// <summary>返回当前配置校验结果。</summary>
        public D21HullComponentValidationResult ValidateCurrent()
        {
            return ValidateConfiguration(_state);
        }

        /// <summary>便捷安装入口，仍经过完整命令和组合校验。</summary>
        public D21HullComponentResult Install(string commandId, D21HullComponentState component,
            int worldDay = 0)
        {
            return Apply(new D21HullComponentCommand
            {
                CommandId = commandId ?? string.Empty,
                Kind = D21HullComponentCommandKind.Install,
                ShipId = _state.ShipId,
                Component = component == null ? null : component.Clone(),
                WorldDay = worldDay,
                SaveRevision = _state.SaveRevision,
            });
        }

        /// <summary>便捷移除入口，仍经过完整命令和组合校验。</summary>
        public D21HullComponentResult Remove(string commandId, string componentId, int worldDay = 0)
        {
            return Apply(new D21HullComponentCommand
            {
                CommandId = commandId ?? string.Empty,
                Kind = D21HullComponentCommandKind.Remove,
                ShipId = _state.ShipId,
                ComponentId = componentId ?? string.Empty,
                WorldDay = worldDay,
                SaveRevision = _state.SaveRevision,
            });
        }

        private static D21HullComponentState Component(string id, D21HullComponentKind kind,
            bool required)
        {
            return new D21HullComponentState
            {
                ComponentId = id,
                Kind = kind,
                RequiredForNavigation = required,
                OperationalState = D21HullComponentOperationalState.Available,
            };
        }

        private static bool ValidateCommand(D21HullComponentCommand command,
            out D21HullComponentFailureCode code, out string reason)
        {
            code = D21HullComponentFailureCode.None;
            reason = string.Empty;
            if (!D21ShipIdentityContracts.IsStableId(command.CommandId))
            {
                code = D21HullComponentFailureCode.MissingCommandId;
                reason = "hull_command_id_invalid";
                return false;
            }
            if (!D21ShipIdentityContracts.IsStableId(command.ShipId))
            {
                code = D21HullComponentFailureCode.InvalidReference;
                reason = "hull_command_ship_id_invalid";
                return false;
            }
            if (command.WorldDay < 0 || command.SaveRevision < 0)
            {
                code = D21HullComponentFailureCode.InvalidReference;
                reason = "hull_command_context_invalid";
                return false;
            }
            if (command.Kind < D21HullComponentCommandKind.Install ||
                command.Kind > D21HullComponentCommandKind.SetOperationalState)
            {
                code = D21HullComponentFailureCode.UnsupportedCommand;
                reason = "hull_command_kind_invalid";
                return false;
            }
            if (command.Kind == D21HullComponentCommandKind.Install)
            {
                if (command.Component == null)
                {
                    code = D21HullComponentFailureCode.MissingComponent;
                    reason = "hull_install_component_missing";
                    return false;
                }
                if (!D21ShipIdentityContracts.IsStableId(command.Component.ComponentId) ||
                    command.Component.Kind == D21HullComponentKind.Unknown)
                {
                    code = D21HullComponentFailureCode.InvalidReference;
                    reason = "hull_install_component_invalid";
                    return false;
                }
            }
            else
            {
                if (!D21ShipIdentityContracts.IsStableId(command.ComponentId))
                {
                    code = D21HullComponentFailureCode.InvalidReference;
                    reason = "hull_component_id_invalid";
                    return false;
                }
                if (command.Kind == D21HullComponentCommandKind.SetOperationalState &&
                    (command.NextOperationalState < D21HullComponentOperationalState.Available ||
                     command.NextOperationalState > D21HullComponentOperationalState.Removed))
                {
                    code = D21HullComponentFailureCode.InvalidState;
                    reason = "hull_component_operational_state_invalid";
                    return false;
                }
            }
            return true;
        }

        private D21HullComponentResult Remember(D21HullComponentCommand command,
            D21HullComponentResult result)
        {
            if (command != null && D21ShipIdentityContracts.IsStableId(command.CommandId))
            {
                _commandFingerprints[command.CommandId] = command.Fingerprint();
                _commandResults[command.CommandId] = result.Clone();
            }
            return result;
        }

        private static D21HullComponentResult Reject(string commandId, string shipId,
            D21HullComponentFailureCode code, string reason)
        {
            return new D21HullComponentResult
            {
                Accepted = false,
                Duplicate = false,
                CommandId = commandId ?? string.Empty,
                ShipId = shipId ?? string.Empty,
                ReasonCode = reason ?? code.ToString(),
                Configuration = null,
                SaveRevision = 0,
                EventId = string.Empty,
            };
        }

        private static int FindComponent(List<D21HullComponentState> components, string componentId)
        {
            if (components == null) return -1;
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] != null && string.Equals(components[i].ComponentId, componentId,
                    StringComparison.Ordinal)) return i;
            }
            return -1;
        }

        private static int CountDefinition(List<D21HullComponentState> components, string definitionId)
        {
            if (components == null || string.IsNullOrEmpty(definitionId)) return 0;
            int count = 0;
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] != null && string.Equals(components[i].DefinitionId, definitionId,
                    StringComparison.Ordinal)) count++;
            }
            return count;
        }

        private static bool Contains(List<string> values, string value)
        {
            if (values == null) return false;
            for (int i = 0; i < values.Count; i++)
                if (string.Equals(values[i], value, StringComparison.Ordinal)) return true;
            return false;
        }

        private static void CopyInto(D21HullComponentConfiguration target,
            D21HullComponentConfiguration source)
        {
            target.SchemaVersion = source.SchemaVersion;
            target.ShipId = source.ShipId;
            target.ShipTypeId = source.ShipTypeId;
            target.PropulsionMode = source.PropulsionMode;
            target.Components.Clear();
            for (int i = 0; i < source.Components.Count; i++)
                target.Components.Add(source.Components[i].Clone());
            target.Definitions.Clear();
            if (source.Definitions != null)
                for (int i = 0; i < source.Definitions.Count; i++)
                    target.Definitions.Add(source.Definitions[i].Clone());
            target.SaveRevision = source.SaveRevision;
            target.LastEventId = source.LastEventId;
        }
    }
}
