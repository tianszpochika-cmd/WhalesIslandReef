using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D19Hud
{
    public sealed class D19R02A01HudPlayModeTests
    {
        [UnityTest]
        public IEnumerator InitialHarborKeepsOnePrimaryHudAndSuppressesLegacyPages()
        {
            var load = SceneManager.LoadSceneAsync("InitialHarbor", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            while (!load.isDone) yield return null;
            yield return new WaitForSeconds(1.5f);

            var primary = GameObject.Find("D19RouteHud");
            Assert.That(primary, Is.Not.Null, "D19RouteHud must be created in the player scene.");
            var primaryD25 = GameObject.Find("D25PlayerHudCanvas");
            Assert.That(IsVisible(primary) ^ IsVisible(primaryD25), Is.True,
                "InitialHarbor must expose exactly one route HUD: D25PlayerHudCanvas or the legacy D19RouteHud fallback.");

            AssertLegacyCanvasHidden("D9PortServicePage");
            AssertLegacyCanvasHidden("D10TavernSocialPage");
            AssertLegacyCanvasHidden("D9VoyageHud");
            AssertLegacyCanvasHidden("D7StatusCanvas");

            var feedback = primary.transform.Find("RoutePanel/Feedback");
            Assert.That(feedback, Is.Not.Null);
            Assert.That(feedback.GetComponent<UnityEngine.UI.Text>().text, Is.Empty);
        }

        private static void AssertLegacyCanvasHidden(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            Assert.That(IsVisible(go), Is.False,
                name + " must not cover the player HUD by default.");
        }

        private static bool IsVisible(GameObject go)
        {
            if (go == null || !go.activeInHierarchy) return false;
            var group = go.GetComponent<CanvasGroup>();
            return group == null || group.alpha > 0.5f;
        }
    }
}
