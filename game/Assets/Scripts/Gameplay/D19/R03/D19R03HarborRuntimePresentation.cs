using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Whalebone.Gameplay.D19.R03
{
    /// <summary>
    /// Presentation-only bridge for the R03 integration prefab. It changes no
    /// gameplay state; it only keeps the player HUD compact after the runtime
    /// bootstrap has created its canvases.
    /// </summary>
    public sealed class D19R03HarborRuntimePresentation : MonoBehaviour
    {
        private const string CollisionRootName = "D6_HarborCollision";
        private const string ShoreBoundaryName = "ShoreBoundary";
        private const string R03SouthShoreBoundaryName = ShoreBoundaryName + ".South";
        private const string R03WestShoreBoundaryName = ShoreBoundaryName + ".West";
        private const string R03EastShoreBoundaryName = ShoreBoundaryName + ".East";
        private const string LegacyPresentationRootName = "D19_SaintMargaret_Root";

        private void Awake()
        {
            // The formal R03 prefab is authored in the same world frame as
            // D6's route anchors.  Keep a stale scene override from lifting
            // the entire presentation above the physics surfaces after a
            // prefab refresh.
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "InitialHarbor")
                return;

            var position = transform.position;
            if (Mathf.Abs(position.y) > 0.001f)
            {
                transform.position = new Vector3(position.x, 0f, position.z);
                Debug.Log("[D19-R03] normalized presentation root Y to 0.0 to match D6 route surfaces");
            }

        }

        private IEnumerator Start()
        {
            // D6 creates its collision root during bootstrap. One frame was
            // insufficient on a cold MainMenu -> InitialHarbor load, so wait
            // for a bounded window without taking ownership of the collider.
            // The HUD can exist before D2 reaches ConfigurePhysics, so it is
            // deliberately not an early-exit condition here.
            var boundaryReady = false;
            for (var frame = 0; frame < 180; frame++)
            {
                yield return null;
                if (FindOwnedShoreBoundary() != null || HasOwnedR03OuterBoundarySet())
                {
                    boundaryReady = true;
                    break;
                }
            }
            if (!boundaryReady)
                Debug.LogWarning("[D19-R03] D6-owned ShoreBoundary set did not appear within 180 frames; route window remains unchanged");
            ApplyCompactRouteHud();
            ApplyR03RouteCollisionWindow();
            DisableConflictingLegacyPhysicalColliders();
        }

        private static void ApplyR03RouteCollisionWindow()
        {
            // D6 owns the collision object. R03 only repositions the existing
            // harbor safety boundary to the frozen far-shore edge so the
            // approved local route can reach the market, warehouse and tavern.
            // No new gameplay authority or second collision system is created.
            var boundary = FindOwnedShoreBoundary();
            if (boundary == null)
            {
                if (HasOwnedR03OuterBoundarySet())
                {
                    // R03 deliberately replaces the old inland wall with
                    // three D6-owned perimeter walls outside the water. Do
                    // not move .South to z=17: that would turn the current
                    // swim route into an invisible hard edge.
                    Debug.Log("[D19-R03] route collision window retained: D6-owned R03 perimeter boundaries remain outside water");
                    return;
                }

                Debug.LogWarning("[D19-R03] route collision window skipped: D6-owned ShoreBoundary set was not available within bootstrap window");
                return;
            }

            var position = boundary.transform.position;
            boundary.transform.position = new Vector3(position.x, position.y, 17.0f);
            Physics.SyncTransforms();
            Debug.Log("[D19-R03] route collision window applied: ShoreBoundary z=17.0; D6 collider owner retained");
        }

        private static bool HasOwnedR03OuterBoundarySet()
        {
            var collisionRoot = GameObject.Find(CollisionRootName);
            if (collisionRoot == null) return false;

            return IsEnabledSolidBoundary(collisionRoot.transform.Find(R03SouthShoreBoundaryName)) &&
                IsEnabledSolidBoundary(collisionRoot.transform.Find(R03WestShoreBoundaryName)) &&
                IsEnabledSolidBoundary(collisionRoot.transform.Find(R03EastShoreBoundaryName));
        }

        private static GameObject FindOwnedShoreBoundary()
        {
            // A global name search can select a hidden legacy boundary or a
            // duplicate prefab instance.  D6 is the sole collision owner, so
            // only its direct, enabled non-trigger boundary is eligible.
            var collisionRoot = GameObject.Find(CollisionRootName);
            if (collisionRoot == null) return null;

            var boundary = collisionRoot.transform.Find(ShoreBoundaryName);
            return IsEnabledSolidBoundary(boundary) ? boundary.gameObject : null;
        }

        private static bool IsEnabledSolidBoundary(Transform boundary)
        {
            if (boundary == null || !boundary.gameObject.activeInHierarchy) return false;
            var colliders = boundary.GetComponents<Collider>();
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider != null && collider.enabled && !collider.isTrigger)
                    return true;
            }
            return false;
        }

        private static void ApplyCompactRouteHud()
        {
            // The legacy hint canvas is a development overlay.  Keep the
            // underlying input bindings and D3 panels intact, but remove the
            // duplicate instruction strip from the player-facing R03 view.
            var legacyHint = GameObject.Find("D2HintCanvas");
            if (legacyHint != null) legacyHint.SetActive(false);

            var canvas = GameObject.Find("D19RouteHud");
            if (canvas == null) return;
            var panel = canvas.transform.Find("RoutePanel") as RectTransform;
            if (panel == null) return;
            panel.anchoredPosition = new Vector2(0f, 34f);
            panel.sizeDelta = new Vector2(760f, 68f);
            var image = panel.GetComponent<Image>();
            if (image != null) image.color = new Color(0.015f, 0.025f, 0.06f, 0.62f);

            SetText(panel, "Hint", new Vector2(0f, 18f), 16, new Vector2(730f, 24f), TextAnchor.MiddleCenter);
            SetText(panel, "Status", new Vector2(0f, -4f), 14, new Vector2(730f, 22f), TextAnchor.MiddleCenter);
            SetText(panel, "Feedback", new Vector2(0f, -23f), 13, new Vector2(730f, 20f), TextAnchor.MiddleCenter);
            Debug.Log("[D19-R03] compact player route HUD applied");
        }

        private static void DisableConflictingLegacyPhysicalColliders()
        {
            // The R03 integration owns the visible local route surface. The
            // historical D19 root may remain active for stable scene IDs and
            // render-layer bookkeeping, but its physical colliders are now
            // invisible duplicates. Leaving any of them enabled lets the
            // CharacterController hit a wall or floor where R03 shows empty
            // space. Preserve triggers because they are non-blocking zone and
            // ambience hooks rather than movement surfaces.
            var disabled = 0;
            var preservedTriggers = 0;
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null || root.name != LegacyPresentationRootName) continue;
                var colliders = root.GetComponentsInChildren<Collider>(true);
                for (var j = 0; j < colliders.Length; j++)
                {
                    var collider = colliders[j];
                    if (collider == null) continue;
                    if (collider.isTrigger)
                    {
                        preservedTriggers++;
                        continue;
                    }
                    if (!collider.enabled) continue;
                    collider.enabled = false;
                    disabled++;
                }
            }
            Debug.Log("[D19-R03] legacy physical colliders disabled=" + disabled + "; non-blocking triggers preserved=" + preservedTriggers + "; D6/R03 owners retained");
        }

        private static void SetText(Transform parent, string name, Vector2 position, int size, Vector2 area, TextAnchor alignment)
        {
            var text = parent.Find(name) == null ? null : parent.Find(name).GetComponent<Text>();
            if (text == null) return;
            var rect = text.rectTransform;
            rect.anchoredPosition = position;
            rect.sizeDelta = area;
            text.fontSize = size;
            text.alignment = alignment;
        }
    }
}
