// ============================================================
// D20CharacterIdentityContracts —— D20-L02 角色身份契约（纯 C#）
//
// D20 只拥有稳定角色引用、角色类别和动作层需要的生命/可用性语义。
// 姓名、代词、证件、法律身份、外观、关系和健康数值仍由既有 owner 写入；
// 本文件只保存不可变身份字段的边界和可序列化的只读投影。
// ============================================================

using System;

namespace Whalebone.Systems.D20
{
    /// <summary>
    /// D20 角色的最小类别。动物和宠物使用 D20-L20 的动物契约，不在此处复制。
    /// </summary>
    public enum D20CharacterKind
    {
        /// <summary>未识别类别，仅允许作为未初始化载体值。</summary>
        Unknown = 0,
        /// <summary>玩家角色。</summary>
        Player = 1,
        /// <summary>非玩家角色。</summary>
        Npc = 2,
    }

    /// <summary>
    /// 角色生命阶段。阶段是身份语义，不等同于年龄数值或健康数值。
    /// </summary>
    public enum D20CharacterLifeStage
    {
        /// <summary>未识别阶段，仅允许作为未初始化载体值。</summary>
        Unknown = 0,
        /// <summary>婴幼儿阶段。</summary>
        Infant = 1,
        /// <summary>儿童阶段。</summary>
        Child = 2,
        /// <summary>青少年阶段。</summary>
        Adolescent = 3,
        /// <summary>成年阶段。</summary>
        Adult = 4,
        /// <summary>老年阶段。</summary>
        Elder = 5,
    }

    /// <summary>
    /// 角色在 D20 动作层的可用性语义。具体健康和岗位事实仍由所属系统裁决。
    /// </summary>
    public enum D20CharacterAvailability
    {
        /// <summary>未识别可用性，仅允许作为未初始化载体值。</summary>
        Unknown = 0,
        /// <summary>可以接受符合前置的动作。</summary>
        Available = 1,
        /// <summary>正在执行一个不应被普通动作覆盖的工作。</summary>
        Busy = 2,
        /// <summary>暂时不能接受动作，但可由 owner 恢复。</summary>
        Unavailable = 3,
        /// <summary>因失能或重伤暂时不能接受动作。</summary>
        Incapacitated = 4,
        /// <summary>角色生命事实已终止。</summary>
        Dead = 5,
        /// <summary>角色已离开当前可用模拟范围。</summary>
        Departed = 6,
    }

    /// <summary>
    /// D20 角色契约的当前版本和稳定字段规则。
    /// </summary>
    public static class D20CharacterIdentityContracts
    {
        /// <summary>当前 D20-L02 身份契约版本。</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>本文件所定义的 D20 语义 owner 标识。</summary>
        public const string OwnerSystem = "D20CharacterIdentity";

        /// <summary>角色稳定键的最大长度，避免存档键和事件键无限增长。</summary>
        public const int CharacterIdMaxLength = 128;

        /// <summary>内容定义引用的最大长度。</summary>
        public const int ContentIdMaxLength = 160;

        /// <summary>身份或来源不透明引用的最大长度。</summary>
        public const int ReferenceMaxLength = 160;

        /// <summary>没有可用本地化/法律表达时使用的安全身份引用。</summary>
        public const string UnspecifiedIdentityKey = "identity.unspecified";

        /// <summary>判断字符串是否是可跨存档使用的稳定角色键。</summary>
        public static bool IsStableCharacterId(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || characterId.Length > CharacterIdMaxLength)
                return false;

            if (characterId.Trim() != characterId)
                return false;

            for (int i = 0; i < characterId.Length; i++)
            {
                char c = characterId[i];
                bool asciiLetter = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool digit = c >= '0' && c <= '9';
                bool separator = c == ':' || c == '_' || c == '-' || c == '.';
                if (!asciiLetter && !digit && !separator)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 规范化角色键。规范化不会大小写折叠或猜测缺失前缀，非法值返回空字符串。
        /// </summary>
        public static string NormalizeCharacterId(string characterId)
        {
            string normalized = (characterId ?? string.Empty).Trim();
            return IsStableCharacterId(normalized) ? normalized : string.Empty;
        }

        /// <summary>判断角色类别是否是当前版本支持的可写类别。</summary>
        public static bool IsSupportedKind(D20CharacterKind kind)
        {
            return kind == D20CharacterKind.Player || kind == D20CharacterKind.Npc;
        }

        /// <summary>判断生命阶段是否是当前版本支持的值。</summary>
        public static bool IsSupportedLifeStage(D20CharacterLifeStage lifeStage)
        {
            return lifeStage >= D20CharacterLifeStage.Infant &&
                   lifeStage <= D20CharacterLifeStage.Elder;
        }

        /// <summary>判断可用性是否是当前版本支持的值。</summary>
        public static bool IsSupportedAvailability(D20CharacterAvailability availability)
        {
            return availability >= D20CharacterAvailability.Available &&
                   availability <= D20CharacterAvailability.Departed;
        }

        /// <summary>判断可用性是否代表终止或不可恢复的角色实例。</summary>
        public static bool IsTerminalAvailability(D20CharacterAvailability availability)
        {
            return availability == D20CharacterAvailability.Dead ||
                   availability == D20CharacterAvailability.Departed;
        }

        /// <summary>
        /// 比较两个身份投影的不可变字段。失败时不改变任何一方，供读档和迁移拒绝冲突。
        /// </summary>
        public static bool ImmutableFieldsMatch(D20CharacterIdentity left,
            D20CharacterIdentity right)
        {
            if (left == null || right == null || left.ImmutableFields == null ||
                right.ImmutableFields == null)
                return false;

            return left.ImmutableFields.Matches(right.ImmutableFields);
        }

        /// <summary>创建一个仅用于新实体初始化的安全默认身份投影。</summary>
        public static D20CharacterIdentity CreateDefault(
            string characterId,
            D20CharacterKind kind,
            string contentId = "",
            string originRef = "")
        {
            return new D20CharacterIdentity(
                new D20CharacterImmutableFields(
                    characterId,
                    kind,
                    contentId,
                    UnspecifiedIdentityKey,
                    originRef),
                D20CharacterLifeStage.Adult,
                D20CharacterAvailability.Available,
                string.Empty,
                0);
        }
    }

    /// <summary>
    /// 角色身份中创建后不得被 D20 改写的字段。
    /// 这些字段以公开数据形式保存，所有入口必须调用 <see cref="Validate"/>
    /// 并通过 <see cref="Matches"/> 检查迁移/恢复是否发生身份漂移。
    /// </summary>
    [Serializable]
    public sealed class D20CharacterImmutableFields
    {
        /// <summary>跨系统、跨存档稳定的角色 ID。</summary>
        public string CharacterId = string.Empty;

        /// <summary>角色类别；玩家和 NPC 的类别不可互换。</summary>
        public D20CharacterKind CharacterKind = D20CharacterKind.Unknown;

        /// <summary>D18 内容定义引用；空值表示由外部 owner 提供的无内容实体。</summary>
        public string ContentId = string.Empty;

        /// <summary>指向 D10 身份表达或其它角色档案的 opaque 引用。</summary>
        public string IdentityKey = D20CharacterIdentityContracts.UnspecifiedIdentityKey;

        /// <summary>创建来源引用；不承载坐标、资源路径或运行时对象。</summary>
        public string OriginRef = string.Empty;

        /// <summary>无参构造器供序列化器创建数据载体。</summary>
        public D20CharacterImmutableFields() { }

        /// <summary>创建一组待验证的不可变字段。</summary>
        public D20CharacterImmutableFields(
            string characterId,
            D20CharacterKind characterKind,
            string contentId,
            string identityKey,
            string originRef)
        {
            CharacterId = characterId ?? string.Empty;
            CharacterKind = characterKind;
            ContentId = contentId ?? string.Empty;
            IdentityKey = string.IsNullOrWhiteSpace(identityKey)
                ? D20CharacterIdentityContracts.UnspecifiedIdentityKey
                : identityKey;
            OriginRef = originRef ?? string.Empty;
        }

        /// <summary>返回防御性副本，避免调用方通过投影别名修改身份字段。</summary>
        public D20CharacterImmutableFields Clone()
        {
            return new D20CharacterImmutableFields(
                CharacterId,
                CharacterKind,
                ContentId,
                IdentityKey,
                OriginRef);
        }

        /// <summary>验证稳定 ID、类别和引用长度；失败必须在进入逻辑层前拒绝。</summary>
        public void Validate()
        {
            if (!D20CharacterIdentityContracts.IsStableCharacterId(CharacterId))
                throw new InvalidOperationException("D20 character_id is missing or has unstable characters.");
            if (!D20CharacterIdentityContracts.IsSupportedKind(CharacterKind))
                throw new InvalidOperationException("D20 character_kind is unknown.");
            if (ContentId != null && ContentId.Length > D20CharacterIdentityContracts.ContentIdMaxLength)
                throw new InvalidOperationException("D20 character content_id is too long.");
            if (string.IsNullOrWhiteSpace(IdentityKey) ||
                IdentityKey.Length > D20CharacterIdentityContracts.ReferenceMaxLength)
                throw new InvalidOperationException("D20 character identity_key is missing or too long.");
            if (OriginRef != null && OriginRef.Length > D20CharacterIdentityContracts.ReferenceMaxLength)
                throw new InvalidOperationException("D20 character origin_ref is too long.");
        }

        /// <summary>比较不可变身份字段，不比较生命阶段、可用性或保存修订。</summary>
        public bool Matches(D20CharacterImmutableFields other)
        {
            if (other == null) return false;
            return string.Equals(CharacterId, other.CharacterId, StringComparison.Ordinal) &&
                   CharacterKind == other.CharacterKind &&
                   string.Equals(ContentId ?? string.Empty, other.ContentId ?? string.Empty,
                       StringComparison.Ordinal) &&
                   string.Equals(IdentityKey ?? string.Empty, other.IdentityKey ?? string.Empty,
                       StringComparison.Ordinal) &&
                   string.Equals(OriginRef ?? string.Empty, other.OriginRef ?? string.Empty,
                       StringComparison.Ordinal);
        }

        /// <summary>返回用于审计和幂等比较的确定性身份指纹。</summary>
        public string Fingerprint()
        {
            return (CharacterId ?? string.Empty) + "|" +
                   CharacterKind + "|" +
                   (ContentId ?? string.Empty) + "|" +
                   (IdentityKey ?? string.Empty) + "|" +
                   (OriginRef ?? string.Empty);
        }
    }

    /// <summary>
    /// D20 角色身份只读语义投影。
    /// D10 的姓名/代词/证件和 D11/D19 的事实不复制到此对象。
    /// </summary>
    [Serializable]
    public sealed class D20CharacterIdentity
    {
        /// <summary>契约版本。</summary>
        public int SchemaVersion = D20CharacterIdentityContracts.CurrentSchemaVersion;

        /// <summary>创建后不得改变的稳定字段集合。</summary>
        public D20CharacterImmutableFields ImmutableFields = new D20CharacterImmutableFields();

        /// <summary>当前生命阶段；不是可变健康值。</summary>
        public D20CharacterLifeStage LifeStage = D20CharacterLifeStage.Unknown;

        /// <summary>动作层当前可用性语义。</summary>
        public D20CharacterAvailability Availability = D20CharacterAvailability.Unknown;

        /// <summary>可用性变化的稳定原因键，不承载显示文案。</summary>
        public string AvailabilityReasonCode = string.Empty;

        /// <summary>生成该投影时对应的逻辑存档修订。</summary>
        public long SaveRevision;

        /// <summary>便于下游读取稳定 ID 的只读别名。</summary>
        public string CharacterId
        {
            get { return ImmutableFields == null ? string.Empty : ImmutableFields.CharacterId; }
        }

        /// <summary>便于下游读取角色类别的只读别名。</summary>
        public D20CharacterKind CharacterKind
        {
            get { return ImmutableFields == null ? D20CharacterKind.Unknown : ImmutableFields.CharacterKind; }
        }

        /// <summary>无参构造器供序列化器创建数据载体。</summary>
        public D20CharacterIdentity() { }

        /// <summary>创建完整身份投影；构造完成后不可变字段只能通过匹配校验保留。</summary>
        public D20CharacterIdentity(
            D20CharacterImmutableFields immutableFields,
            D20CharacterLifeStage lifeStage,
            D20CharacterAvailability availability,
            string availabilityReasonCode = "",
            long saveRevision = 0)
        {
            ImmutableFields = immutableFields == null
                ? new D20CharacterImmutableFields()
                : immutableFields.Clone();
            LifeStage = lifeStage;
            Availability = availability;
            AvailabilityReasonCode = availabilityReasonCode ?? string.Empty;
            SaveRevision = saveRevision;
        }

        /// <summary>返回深拷贝，防止读模型调用方修改 D20 内部引用。</summary>
        public D20CharacterIdentity Clone()
        {
            return new D20CharacterIdentity
            {
                SchemaVersion = SchemaVersion,
                ImmutableFields = ImmutableFields == null
                    ? new D20CharacterImmutableFields()
                    : ImmutableFields.Clone(),
                LifeStage = LifeStage,
                Availability = Availability,
                AvailabilityReasonCode = AvailabilityReasonCode ?? string.Empty,
                SaveRevision = SaveRevision,
            };
        }

        /// <summary>验证身份投影的版本、枚举和不可变字段。</summary>
        public void Validate()
        {
            if (SchemaVersion != D20CharacterIdentityContracts.CurrentSchemaVersion)
                throw new InvalidOperationException("D20 character identity schema_version mismatch: expected " +
                    D20CharacterIdentityContracts.CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (ImmutableFields == null)
                throw new InvalidOperationException("D20 character identity immutable_fields are required.");
            ImmutableFields.Validate();
            if (!D20CharacterIdentityContracts.IsSupportedLifeStage(LifeStage))
                throw new InvalidOperationException("D20 character life_stage is unknown.");
            if (!D20CharacterIdentityContracts.IsSupportedAvailability(Availability))
                throw new InvalidOperationException("D20 character availability is unknown.");
            if (AvailabilityReasonCode != null &&
                AvailabilityReasonCode.Length > D20CharacterIdentityContracts.ReferenceMaxLength)
                throw new InvalidOperationException("D20 character availability_reason_code is too long.");
            if (SaveRevision < 0)
                throw new InvalidOperationException("D20 character save_revision must be non-negative.");
        }
    }
}
