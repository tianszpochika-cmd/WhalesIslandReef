// ============================================================
// D22LocalizationKeyAudit —— D22-L15 本地化 key 审计（纯 C#）
//
// D22 逻辑层只允许传递稳定 localization key。审计同时检查已登记的
// screen/panel/feedback 条目和调用方显式提交的标签引用，不解析或保存文案。
// ============================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whalebone.Systems.D22
{
    /// <summary>本地化 key 审计问题码。</summary>
    public enum D22LocalizationAuditIssueCode
    {
        None = 0,
        MissingKey = 1,
        InvalidKeyFormat = 2,
        HardcodedUserVisibleText = 3,
        DuplicateReference = 4,
    }

    /// <summary>需要审计的稳定字段引用；Value 必须是 key 而不是最终文案。</summary>
    [Serializable]
    public sealed class D22LocalizationKeyReference
    {
        public string OwnerId = string.Empty;
        public string FieldName = string.Empty;
        public string Value = string.Empty;
        public bool IsUserVisible = true;

        /// <summary>兼容 source/property 命名。</summary>
        public string SourceOwner
        {
            get { return OwnerId; }
            set { OwnerId = value ?? string.Empty; }
        }

        /// <summary>兼容 property 命名。</summary>
        public string PropertyName
        {
            get { return FieldName; }
            set { FieldName = value ?? string.Empty; }
        }

        /// <summary>返回防御性副本。</summary>
        public D22LocalizationKeyReference Clone()
        {
            return new D22LocalizationKeyReference
            {
                OwnerId = OwnerId ?? string.Empty,
                FieldName = FieldName ?? string.Empty,
                Value = Value ?? string.Empty,
                IsUserVisible = IsUserVisible,
            };
        }
    }

    /// <summary>单条本地化审计问题。</summary>
    [Serializable]
    public sealed class D22LocalizationAuditIssue
    {
        public D22LocalizationAuditIssueCode Code;
        public string OwnerId = string.Empty;
        public string FieldName = string.Empty;
        public string Value = string.Empty;
        public bool IsUserVisible;

        /// <summary>返回防御性副本。</summary>
        public D22LocalizationAuditIssue Clone()
        {
            return new D22LocalizationAuditIssue
            {
                Code = Code,
                OwnerId = OwnerId ?? string.Empty,
                FieldName = FieldName ?? string.Empty,
                Value = Value ?? string.Empty,
                IsUserVisible = IsUserVisible,
            };
        }
    }

    /// <summary>本地化 key 审计报告。</summary>
    [Serializable]
    public sealed class D22LocalizationAuditReport
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public bool Valid;
        public int CheckedReferenceCount;
        public List<D22LocalizationAuditIssue> Issues = new List<D22LocalizationAuditIssue>();

        /// <summary>是否发现硬编码用户可见文本。</summary>
        public bool HasHardcodedUserVisibleText
        {
            get
            {
                foreach (var issue in Issues)
                    if (issue != null && issue.Code == D22LocalizationAuditIssueCode.HardcodedUserVisibleText) return true;
                return false;
            }
        }

        /// <summary>返回防御性副本。</summary>
        public D22LocalizationAuditReport Clone()
        {
            var result = new D22LocalizationAuditReport
            {
                SchemaVersion = SchemaVersion,
                Valid = Valid,
                CheckedReferenceCount = CheckedReferenceCount,
            };
            if (Issues != null)
                foreach (var issue in Issues) result.Issues.Add(issue == null ? null : issue.Clone());
            return result;
        }
    }

    /// <summary>
    /// D22-L15 稳定 localization key 审计器。它不解析文案，也不修改任何目录或 Runtime 状态。
    /// </summary>
    public static class D22LocalizationKeyAudit
    {
        public const int CurrentSchemaVersion = D22LocalizationAuditReport.CurrentSchemaVersion;
        public const string OwnerSystem = "D22LocalizationKeyAudit";
        public const string RequiredPrefix = "ui.";

        /// <summary>稳定 key 必须使用 ui. 前缀和 D22 稳定字符集。</summary>
        public static bool IsStableKey(string value)
        {
            return !string.IsNullOrEmpty(value) && D22ScreenContracts.IsStableId(value, RequiredPrefix);
        }

        /// <summary>返回 key 的机器审计问题码。</summary>
        public static D22LocalizationAuditIssueCode ValidateKey(string value, bool userVisible = true)
        {
            if (string.IsNullOrEmpty(value)) return D22LocalizationAuditIssueCode.MissingKey;
            if (IsStableKey(value)) return D22LocalizationAuditIssueCode.None;
            return userVisible
                ? D22LocalizationAuditIssueCode.HardcodedUserVisibleText
                : D22LocalizationAuditIssueCode.InvalidKeyFormat;
        }

        /// <summary>审计调用方提交的逻辑标签引用。</summary>
        public static D22LocalizationAuditReport Audit(IEnumerable<D22LocalizationKeyReference> references)
        {
            var report = new D22LocalizationAuditReport { Valid = true };
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (references == null)
            {
                report.Valid = false;
                report.Issues.Add(Issue(D22LocalizationAuditIssueCode.MissingKey, string.Empty, string.Empty, string.Empty, true));
                return report;
            }

            foreach (var reference in references)
            {
                report.CheckedReferenceCount++;
                if (reference == null)
                {
                    report.Valid = false;
                    report.Issues.Add(Issue(D22LocalizationAuditIssueCode.MissingKey, string.Empty, string.Empty, string.Empty, true));
                    continue;
                }
                string ownerId = reference.OwnerId ?? string.Empty;
                string fieldName = reference.FieldName ?? string.Empty;
                string value = reference.Value ?? string.Empty;
                string identity = ownerId + "|" + fieldName;
                if (!seen.Add(identity))
                {
                    report.Valid = false;
                    report.Issues.Add(Issue(D22LocalizationAuditIssueCode.DuplicateReference,
                        ownerId, fieldName, value, reference.IsUserVisible));
                    continue;
                }
                D22LocalizationAuditIssueCode code = ValidateKey(value, reference.IsUserVisible);
                if (code != D22LocalizationAuditIssueCode.None)
                {
                    report.Valid = false;
                    report.Issues.Add(Issue(code, ownerId, fieldName, value, reference.IsUserVisible));
                }
            }
            return report;
        }

        /// <summary>审计单值标签；用于 Runtime adapter 接线前的确定性检查。</summary>
        public static D22LocalizationAuditReport AuditKey(
            string ownerId,
            string fieldName,
            string value,
            bool userVisible = true)
        {
            return Audit(new List<D22LocalizationKeyReference>
            {
                new D22LocalizationKeyReference
                {
                    OwnerId = ownerId ?? string.Empty,
                    FieldName = fieldName ?? string.Empty,
                    Value = value ?? string.Empty,
                    IsUserVisible = userVisible,
                },
            });
        }

        /// <summary>审计 D22-L02 screen/panel 和 D22-L07 feedback 目录。</summary>
        public static D22LocalizationAuditReport AuditD22Catalog()
        {
            var references = new List<D22LocalizationKeyReference>();
            foreach (var screen in D22ScreenContracts.GetScreenDefinitions())
            {
                references.Add(new D22LocalizationKeyReference
                {
                    OwnerId = screen.Id,
                    FieldName = "LocalizationKey",
                    Value = screen.LocalizationKey,
                    IsUserVisible = true,
                });
            }
            foreach (var panel in D22ScreenContracts.GetPanelDefinitions())
            {
                references.Add(new D22LocalizationKeyReference
                {
                    OwnerId = panel.Id,
                    FieldName = "LocalizationKey",
                    Value = panel.LocalizationKey,
                    IsUserVisible = true,
                });
            }
            foreach (var feedback in D22FeedbackCatalog.GetEntries())
            {
                references.Add(new D22LocalizationKeyReference
                {
                    OwnerId = feedback.Code,
                    FieldName = "LocalizationKey",
                    Value = feedback.LocalizationKey,
                    IsUserVisible = true,
                });
            }
            return Audit(references);
        }

        /// <summary>AuditD22Catalog 的语义别名。</summary>
        public static D22LocalizationAuditReport AuditFeedbackAndLabels()
        {
            return AuditD22Catalog();
        }

        /// <summary>返回只读 key 列表审计结果。</summary>
        public static D22LocalizationAuditReport AuditKeys(IEnumerable<string> keys)
        {
            var references = new List<D22LocalizationKeyReference>();
            if (keys != null)
            {
                int index = 0;
                foreach (var key in keys)
                {
                    references.Add(new D22LocalizationKeyReference
                    {
                        OwnerId = "d22.audit.item." + index,
                        FieldName = "LocalizationKey",
                        Value = key ?? string.Empty,
                        IsUserVisible = true,
                    });
                    index++;
                }
            }
            return Audit(references);
        }

        private static D22LocalizationAuditIssue Issue(
            D22LocalizationAuditIssueCode code,
            string ownerId,
            string fieldName,
            string value,
            bool userVisible)
        {
            return new D22LocalizationAuditIssue
            {
                Code = code,
                OwnerId = ownerId ?? string.Empty,
                FieldName = fieldName ?? string.Empty,
                Value = value ?? string.Empty,
                IsUserVisible = userVisible,
            };
        }
    }
}
