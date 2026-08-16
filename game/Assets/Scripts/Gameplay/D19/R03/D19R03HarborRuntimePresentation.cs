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
                if (FindChildByName("ShoreBoundary") != null)
                {
                    boundaryReady = true;
                    break;
                }
            }
            if (!boundaryReady)
                Debug.LogWarning("[D19-R03] ShoreBoundary did not appear within 180 frames; route window remains unchanged");
            ApplyCompactRouteHud();
            ApplyR03RouteCollisionWindow();
            DisableConflictingLegacyDockColliders();
            DisableConflictingLegacyTerrainCollider();
        }

        private static void ApplyR03RouteCollisionWindow()
        {
            // D6 owns the collision object. R03 only repositions the existing
            // harbor safety boundary to the frozen far-shore edge so the
            // approved local route can reach the market, warehouse and tavern.
            // No new gameplay authority or second collision system is created.
            var boundary = FindChildByName("ShoreBoundary");
            if (boundary == null)
            {
                Debug.LogWarning("[D19-R03] route collision window skipped: D6 ShoreBoundary was not available within bootstrap window");
                return;
            }

            var position = boundary.transform.position;
            boundary.transform.position = new Vector3(position.x, position.y, 17.0f);
            Debug.Log("[D19-R03] route collision window applied: ShoreBoundary z=17.0; D6 collider owner retained");
        }

        private static GameObject FindChildByName(string name)
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                    if (transforms[j].name == name) return transforms[j].gameObject;
            }
            return null;
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

        private static void DisableConflictingLegacyDockColliders()
        {
            // The R03 integration owns the presentation route surface. The
            // historical D19 dock deck boxes span the same volume and sit
            // below the R03 surface, so CharacterController sees them as a
            // non-walkable obstacle and cannot leave the berth. Keep the
            // stable Dock/Gangplank nodes and D6's runtime DockGround and
            // GangplankGround owners; disable only the duplicate legacy
            // colliders under the historical D19 scene root.
            var disabled = 0;
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null || root.name != "D19_SaintMargaret_Root") continue;
                var colliders = root.GetComponentsInChildren<Collider>(true);
                for (var j = 0; j < colliders.Length; j++)
                {
                    var collider = colliders[j];
                    if (collider == null) continue;
                    if (collider.name != "DockDeck_Collision" && collider.name != "Gangplank_Collision") continue;
                    if (!collider.enabled) continue;
                    collider.enabled = false;
                    disabled++;
                }
            }
            Debug.Log("[D19-R03] legacy dock duplicate colliders disabled=" + disabled + "; stable IDs and D6 ground owners retained");
        }

        private static void DisableConflictingLegacyTerrainCollider()
        {
            // D19-A01's legacy terrain collider covers the full sample island
            // and overlaps the R03 controlled route surface. Keep the legacy
            // renderers and stable hierarchy, but let the R03 integration
            // surface be the sole walkable collider in the local harbor
            // window. This is a presentation/integration bridge only; it does
            // not alter D19-A01 source assets or movement rules.
            var disabled = 0;
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null || root.name != "D19_SaintMargaret_Root") continue;
                var legacyEnvironment = root.transform.Find("D19_EnvironmentRoot");
                if (legacyEnvironment == null) continue;
                var legacyTerrain = legacyEnvironment.Find("D19_TerrainRoot");
                if (legacyTerrain == null) continue;
                var colliders = legacyTerrain.GetComponentsInChildren<Collider>(true);
                for (var j = 0; j < colliders.Length; j++)
                {
                    var collider = colliders[j];
                    if (collider == null || !collider.enabled) continue;
                    collider.enabled = false;
                    disabled++;
                }
            }
            Debug.Log("[D19-R03] legacy terrain colliders disabled=" + disabled + "; R03 route surface remains integration owner");
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
