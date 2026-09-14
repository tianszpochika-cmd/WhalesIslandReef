using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Whalebone.Tests.D19R03Q07
{
    /// <summary>
    /// Q07 child card: visible R03 service buildings must have a solid body
    /// boundary owned by HarborCollisionController. This prevents a normal
    /// CharacterController/camera route from entering a presentation mesh.
    /// It does not add interior gameplay or claim final Player visual quality.
    /// </summary>
    public sealed class D19R03Q07BuildingCollisionPlayModeTests
    {
        private const string SceneName = "InitialHarbor";
        private const string PresentationName = "D19_R03_HarborPresentation";
        private const string CollisionRootName = "D6_HarborCollision";
        private const string CollisionPrefix = "R03BuildingCollision.";

        [UnityTest]
        public IEnumerator R03ServiceBuildingsHaveSolidHarborOwnedBodyBoundaries()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.IsNotNull(load, "InitialHarbor 不在 Build Settings 中。");
            while (!load.isDone) yield return null;

            var deadline = Time.realtimeSinceStartup + 8f;
            GameObject presentation = null;
            GameObject collisionRoot = null;
            while (Time.realtimeSinceStartup < deadline)
            {
                presentation = GameObject.Find(PresentationName);
                collisionRoot = GameObject.Find(CollisionRootName);
                if (presentation != null && collisionRoot != null &&
                    collisionRoot.transform.Find(CollisionPrefix + "MarketVisual") != null)
                    break;
                yield return null;
            }

            Assert.IsNotNull(presentation, "InitialHarbor 缺少 R03 表现根。");
            Assert.IsNotNull(collisionRoot, "InitialHarbor 缺少 D6 Harbor 碰撞 owner。");

            string[] buildings = { "MarketVisual", "WarehouseVisual", "TavernVisual", "ShipyardVisual" };
            for (int i = 0; i < buildings.Length; i++)
            {
                string name = buildings[i];
                var visual = FindDescendant(presentation.transform, name);
                Assert.IsNotNull(visual, "R03 缺少服务建筑表现根：" + name);
                var body = FindDescendant(visual, "Body");
                Assert.IsNotNull(body, "R03 服务建筑缺少 Body：" + name);

                var renderers = body.GetComponentsInChildren<Renderer>(true);
                Assert.IsNotEmpty(renderers, "R03 服务建筑 Body 没有 Renderer：" + name);
                var bodyBounds = renderers[0].bounds;
                for (int j = 1; j < renderers.Length; j++)
                    bodyBounds.Encapsulate(renderers[j].bounds);

                var boundary = collisionRoot.transform.Find(CollisionPrefix + name);
                Assert.IsNotNull(boundary, "D6 缺少 R03 服务建筑边界：" + name);
                var box = boundary.GetComponent<BoxCollider>();
                Assert.IsNotNull(box, "R03 服务建筑边界必须是 BoxCollider：" + name);
                Assert.IsTrue(box.enabled && !box.isTrigger,
                    "R03 服务建筑边界必须是启用的实体碰撞：" + name);
                Assert.AreEqual(collisionRoot.transform, boundary.parent,
                    "服务建筑碰撞必须归 HarborCollisionController owner，不得挂回表现 Prefab：" + name);

                Assert.That(box.bounds.min.x, Is.LessThanOrEqualTo(bodyBounds.min.x + 0.01f), name + " X min 未覆盖 Body。");
                Assert.That(box.bounds.max.x, Is.GreaterThanOrEqualTo(bodyBounds.max.x - 0.01f), name + " X max 未覆盖 Body。");
                Assert.That(box.bounds.min.y, Is.LessThanOrEqualTo(bodyBounds.min.y + 0.01f), name + " Y min 未覆盖 Body。");
                Assert.That(box.bounds.max.y, Is.GreaterThanOrEqualTo(bodyBounds.max.y - 0.01f), name + " Y max 未覆盖 Body。");
                Assert.That(box.bounds.min.z, Is.LessThanOrEqualTo(bodyBounds.min.z + 0.01f), name + " Z min 未覆盖 Body。");
                Assert.That(box.bounds.max.z, Is.GreaterThanOrEqualTo(bodyBounds.max.z - 0.01f), name + " Z max 未覆盖 Body。");

                var origin = new Vector3(bodyBounds.center.x,
                    bodyBounds.center.y,
                    bodyBounds.min.z - 1.0f);
                RaycastHit hit;
                Assert.IsTrue(Physics.Raycast(origin, Vector3.forward, out hit, bodyBounds.size.z + 2f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore),
                    "从建筑外向 Body 射线未命中 Harbor 碰撞边界：" + name);
                Assert.AreEqual(box, hit.collider,
                    "建筑外向 Body 的第一实体命中必须是 Harbor 建筑边界：" + name);

                Debug.Log("[Q07/R03Building] " + name +
                    " body=" + bodyBounds + "; collider=" + box.bounds);
            }
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDescendant(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
