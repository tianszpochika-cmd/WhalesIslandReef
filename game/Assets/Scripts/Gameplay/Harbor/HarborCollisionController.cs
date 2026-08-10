// ============================================================
// HarborCollisionController —— InitialHarbor 物理边界层
//
// 港口美术安装器会移除占位几何体自带的碰撞体，避免编辑器阶段
// 产生一堆不可控的装饰碰撞。这里由运行时统一创建少量、可审计的
// 地面、码头、跳板、道具和船体碰撞边界。
// ============================================================

using UnityEngine;

namespace Whalebone.Gameplay.Harbor
{
    public sealed class HarborCollisionController : MonoBehaviour
    {
        private const string CollisionRootName = "D6_HarborCollision";
        private const string BeachName = "BeachGround";
        private const string DockName = "DockGround";
        private const string ShoreBoundaryName = "ShoreBoundary";
        private const string GangplankName = "GangplankGround";

        public bool IsInitialized { get; private set; }
        public int ColliderCount { get; private set; }
        public int WalkableSurfaceCount { get; private set; }

        public static bool IsNavigationBoundary(Collider collider)
        {
            return collider != null && (collider.name == ShoreBoundaryName ||
                collider.name == "D6_ShipHullCollision");
        }

        private Transform _collisionRoot;

        public void Apply(GameObject ship)
        {
            if (IsInitialized) return;

            _collisionRoot = FindOrCreateRoot();
            ColliderCount = 0;
            WalkableSurfaceCount = 0;

            EnsureBox(BeachName, new Vector3(0f, -0.32f, -26f),
                new Vector3(70f, 1.2f, 46f), true);
            WalkableSurfaceCount++;

            EnsureBox(DockName, new Vector3(0f, 0.1f, -6f),
                new Vector3(26f, 0.18f, 3.4f), true);
            WalkableSurfaceCount++;

            // 沙滩与海面的交界先用不可见安全边界封住，避免角色掉入未实现的游泳区域。
            EnsureBox(ShoreBoundaryName, new Vector3(0f, 1.05f, -2.88f),
                new Vector3(70f, 2.1f, 0.16f), false);

            EnsureBox(GangplankName, new Vector3(0f, 0.5f, -4.75f),
                new Vector3(1.8f, 0.07f, 1.1f), true);
            WalkableSurfaceCount++;

            AddDockPropColliders();
            AddShipCollider(ship);

            IsInitialized = true;
            Debug.Log("[D6/Physics] 港口碰撞层已建立：地面=" + WalkableSurfaceCount +
                      "，碰撞体=" + ColliderCount + "，海岸安全边界=启用");
        }

        private Transform FindOrCreateRoot()
        {
            var existing = GameObject.Find(CollisionRootName);
            if (existing != null) return existing.transform;

            var root = new GameObject(CollisionRootName);
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.isStatic = true;
            return root.transform;
        }

        private BoxCollider EnsureBox(string name, Vector3 worldPosition, Vector3 worldSize,
            bool walkable)
        {
            var child = FindDirectChild(_collisionRoot, name);
            if (child == null)
            {
                child = new GameObject(name);
                child.transform.SetParent(_collisionRoot, false);
            }

            child.transform.localPosition = worldPosition;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            child.isStatic = true;

            var collider = child.GetComponent<BoxCollider>();
            if (collider == null) collider = child.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = worldSize;
            collider.isTrigger = false;
            ColliderCount++;
            if (walkable) collider.gameObject.layer = LayerMask.NameToLayer("Default");
            return collider;
        }

        private void AddDockPropColliders()
        {
            var dock = GameObject.Find("Dock");
            if (dock == null) return;

            string[] physicalProps =
            {
                "Pile", "HandRail", "Bollard", "Barrel", "Barrel2", "Crate", "Crate2"
            };
            foreach (var child in dock.GetComponentsInChildren<Transform>(true))
            {
                if (!ContainsName(physicalProps, child.name)) continue;
                var collider = child.GetComponent<BoxCollider>();
                if (collider == null) collider = child.gameObject.AddComponent<BoxCollider>();
                collider.center = Vector3.zero;
                collider.size = Vector3.one;
                collider.isTrigger = false;
                child.gameObject.isStatic = true;
                ColliderCount++;
            }
        }

        private void AddShipCollider(GameObject ship)
        {
            if (ship == null) return;
            if (ship.GetComponentsInChildren<Collider>(true).Length > 0) return;

            var renderers = ship.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            var child = FindDirectChild(ship.transform, "D6_ShipHullCollision");
            if (child == null)
            {
                child = new GameObject("D6_ShipHullCollision");
                child.transform.SetParent(ship.transform, false);
            }

            child.transform.localPosition = ship.transform.InverseTransformPoint(bounds.center);
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            var scale = ship.transform.lossyScale;
            var size = new Vector3(
                bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(scale.x)),
                bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(scale.y)),
                bounds.size.z / Mathf.Max(0.001f, Mathf.Abs(scale.z)));
            size.x = Mathf.Max(size.x, 0.8f);
            size.y = Mathf.Max(size.y, 0.35f);
            size.z = Mathf.Max(size.z, 0.8f);

            var collider = child.GetComponent<BoxCollider>();
            if (collider == null) collider = child.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = size;
            collider.isTrigger = false;
            ColliderCount++;
        }

        private static GameObject FindDirectChild(Transform parent, string name)
        {
            if (parent == null) return null;
            var child = parent.Find(name);
            return child == null ? null : child.gameObject;
        }

        private static bool ContainsName(string[] names, string value)
        {
            for (int i = 0; i < names.Length; i++)
                if (names[i] == value) return true;
            return false;
        }
    }
}
