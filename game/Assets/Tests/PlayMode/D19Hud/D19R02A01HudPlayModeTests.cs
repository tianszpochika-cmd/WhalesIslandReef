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
            var primaryGroup = primary.GetComponent<CanvasGroup>();
            Assert.That(primaryGroup == null || primaryGroup.alpha > 0.5f, Is.True);

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
            var group = go.GetComponent<CanvasGroup>();
            Assert.That(group == null || group.alpha < 0.01f, Is.True,
                name + " must not cover the player HUD by default.");
        }
    }
}
