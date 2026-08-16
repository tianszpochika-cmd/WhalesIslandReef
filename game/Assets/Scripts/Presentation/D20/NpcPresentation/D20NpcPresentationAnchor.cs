using UnityEngine;

namespace WhalesIslandReef.Presentation.D20.NpcPresentation
{
    /// <summary>
    /// Visual anchor metadata only. The owning system remains responsible for reaching
    /// and accepting the job interaction; this component never invokes that system.
    /// </summary>
    public sealed class D20NpcPresentationAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId;
        [SerializeField] private D20NpcJobPost jobPost;
        [SerializeField] private string interactionHintKey;

        public string AnchorId => anchorId;
        public D20NpcJobPost JobPost => jobPost;
        public string InteractionHintKey => interactionHintKey;

        public void ConfigureForEditor(string stableAnchorId, D20NpcJobPost post, string hintKey)
        {
            anchorId = stableAnchorId;
            jobPost = post;
            interactionHintKey = hintKey;
        }
    }
}
