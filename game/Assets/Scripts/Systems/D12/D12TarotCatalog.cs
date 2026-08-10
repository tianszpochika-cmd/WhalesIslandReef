// ============================================================
// D12TarotCatalog —— D12-A06 代表牌面与占卜者目录
//
// 这是系统目录，不宣称真实超自然事实。每个条目仍携带来源、历史标签、
// 阶段、所属系统和 test_id，便于后续内容注册表接入。
// ============================================================

using System;
using System.Collections.Generic;

namespace Whalebone.Systems.D12
{
    public sealed class D12TarotCatalog
    {
        public const string SystemOwner = "D12TarotSystem";
        public const string EventPrefix = "event_d12_tarot_";
        public const string ExplanationProviderTemplate = "template";
        public const string ExplanationProviderLlm = "llm_text_only";

        private readonly List<D12TarotCardDefinition> _cards = new List<D12TarotCardDefinition>();
        private readonly List<D12DivinerDefinition> _diviners = new List<D12DivinerDefinition>();

        public IReadOnlyList<D12TarotCardDefinition> Cards { get { return _cards; } }
        public IReadOnlyList<D12DivinerDefinition> Diviners { get { return _diviners; } }

        public D12TarotCatalog()
        {
            AddCard("tarot_major_sun", "太阳", "tarot.card.sun", "major_arcana", "visibility", "visibility:review_conditions", "content:d12_tarot_major_arcana", "historical_tarot_major_arcana", "D12", "D12-A06-CARD-001");
            AddCard("tarot_major_moon", "月亮", "tarot.card.moon", "major_arcana", "uncertainty", "weather:review_visibility", "content:d12_tarot_major_arcana", "historical_tarot_major_arcana", "D12", "D12-A06-CARD-002");
            AddCard("tarot_major_tower", "高塔", "tarot.card.tower", "major_arcana", "disruption", "ship:inspect_before_commit", "content:d12_tarot_major_arcana", "historical_tarot_major_arcana", "D12", "D12-A06-CARD-003");
            AddCard("tarot_major_star", "星星", "tarot.card.star", "major_arcana", "recovery", "crew:seek_recovery_signal", "content:d12_tarot_major_arcana", "historical_tarot_major_arcana", "D12", "D12-A06-CARD-004");
            AddCard("tarot_major_hermit", "隐者", "tarot.card.hermit", "major_arcana", "solitude", "community:ask_for_context", "content:d12_tarot_major_arcana", "historical_tarot_major_arcana", "D12", "D12-A06-CARD-005");
            AddCard("tarot_major_chariot", "战车", "tarot.card.chariot", "major_arcana", "journey", "navigation:review_route", "content:d12_tarot_major_arcana", "historical_tarot_major_arcana", "D12", "D12-A06-CARD-006");

            var diviner = new D12DivinerDefinition
            {
                DivinerId = "diviner_saint_marys_keeper",
                DisplayName = "圣玛丽民俗解牌者",
                CultureId = "saint_marys_coastal",
                ReligionId = "catholic",
                SourceRef = "content:d12_diviner_saint_marys_keeper",
                AuthorizationRef = "design-review:d12-a06-sensitive-content",
                LicenseRef = "project-authored",
                HistoricalTag = "caribbean_port_folk_practice_1675",
                WarningId = "warning_religious_practice_tarot",
                WarningText = "此场景包含历史宗教与民俗实践的表现。你可以继续、跳过敏感内容，或返回。牌面不会产生真实超自然结果。",
                Sensitivity = D12TarotSensitivity.ReligiousPractice,
                BaseTrust = 45,
                TrustSourceRef = "content:d12_diviner_saint_marys_keeper",
                TrustEvidenceRef = "design:d12_tarot_divination",
                TestId = "D12-A06-DIVINER-001",
                SkipAllowed = true,
            };
            diviner.Validate();
            _diviners.Add(diviner);
        }

        public D12TarotCardDefinition FindCard(string cardId)
        {
            foreach (var card in _cards)
                if (card != null && string.Equals(card.CardId, cardId, StringComparison.Ordinal)) return card;
            return null;
        }

        public D12DivinerDefinition FindDiviner(string divinerId)
        {
            foreach (var diviner in _diviners)
                if (diviner != null && string.Equals(diviner.DivinerId, divinerId, StringComparison.Ordinal)) return diviner;
            return null;
        }

        private void AddCard(string id, string name, string nameKey, string arcana, string tag,
            string signal, string source, string history, string phase, string testId)
        {
            var card = new D12TarotCardDefinition
            {
                CardId = id,
                DisplayName = name,
                DisplayNameKey = nameKey,
                Arcana = arcana,
                UprightHint = "把它当作观察问题，而不是预言。",
                ReversedHint = "把它当作需要复核的相反提示，而不是预言。",
                SourceRef = source,
                AuthorizationRef = "design-review:d12-a06-card-catalog",
                LicenseRef = "project-authored",
                HistoricalTag = history,
                Phase = phase,
                OwnerSystem = SystemOwner,
                TestId = testId,
            };
            card.MeaningTags.Add(tag);
            card.MeaningTags.Add("soft_clue_only");
            card.MeaningTags.Add(signal);
            card.Validate();
            _cards.Add(card);
        }
    }
}
