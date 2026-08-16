using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.NpcPresentation
{
    /// <summary>
    /// A small presentation-only world-space cue. Localization/UI can replace the display
    /// key later; this component never resolves or writes gameplay state.
    /// </summary>
    public sealed class D20NpcFeedbackBillboard : MonoBehaviour
    {
        [SerializeField] private TextMesh textMesh;
        [SerializeField] private bool hideWhenNoFeedback = true;

        public string LastDisplayKey { get; private set; }
        public bool IsVisible => textMesh != null && textMesh.gameObject.activeSelf && textMesh.text.Length > 0;

        private void Awake()
        {
            if (textMesh == null)
                textMesh = GetComponentInChildren<TextMesh>(true);
            LastDisplayKey = string.Empty;
            SetVisible(false);
        }

        public void ConfigureForEditor(TextMesh configuredText)
        {
            textMesh = configuredText;
        }

        public void Apply(D20NpcPresentationFeedback feedback)
        {
            LastDisplayKey = feedback.displayKey ?? string.Empty;
            if (textMesh == null)
                return;

            textMesh.text = feedback.visible ? LastDisplayKey : string.Empty;
            if (hideWhenNoFeedback)
                SetVisible(feedback.visible);
        }

        public void SetVisible(bool visible)
        {
            if (textMesh != null)
                textMesh.gameObject.SetActive(visible);
        }
    }
}
