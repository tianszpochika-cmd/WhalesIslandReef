// ============================================================
// HarborCollisionController —— InitialHarbor 物理边界层
//
// 港口美术安装器会移除占位几何体自带的碰撞体，避免编辑器阶段
// 产生一堆不可控的装饰碰撞。这里由运行时统一创建少量、可审计的
// 地面、码头、跳板、道具和船体碰撞边界。
// ============================================================

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Whalebone.Gameplay.Harbor
{
    public sealed class HarborCollisionController : MonoBehaviour
    {
        private const string CollisionRootName = "D6_HarborCollision";
        private const string BeachName = "BeachGround";
        private const string DockName = "DockGround";
        private const string ShoreBoundaryName = "ShoreBoundary";
        private const string GangplankName = "GangplankGround";
        private const string R03PresentationRootName = "D19_R03_HarborPresentation";
        private const string R03BuildingCollisionPrefix = "R03BuildingCollision.";
        private static readonly string[] R03BuildingNames =
        {
            "MarketVisual",
            "WarehouseVisual",
            "TavernVisual",
            "ShipyardVisual"
        };
        private const float ShoreBoundaryDefaultZ = -2.88f;
        private const float ShoreBoundaryWorldY = 1.05f;
        private const float ShoreBoundaryHeight = 2.1f;
        private const float ShoreBoundaryThickness = 0.16f;
        // R03 has a real island MeshCollider and a visual ocean. The safety
        // walls must sit outside the water, otherwise they turn the shoreline
        // into an invisible hard edge and make swimming impossible.
        private const float ShoreBoundarySouthZ = -47.5f;
        private const float ShoreBoundaryNorthZ = 47.5f;
        private const float ShoreBoundaryWestX = -47.5f;
        private const float ShoreBoundaryEastX = 47.5f;
        private const float ShoreBoundarySpanX = 42f;
        private const float ShoreBoundarySpanZ = ShoreBoundaryNorthZ - ShoreBoundarySouthZ;

        public bool IsInitialized { get; private set; }
        public bool R03SurfaceInstalled { get; private set; }
        public int ColliderCount { get; private set; }
        public int WalkableSurfaceCount { get; private set; }
        public int R03BuildingCollisionCount { get; private set; }

        public static bool IsNavigationBoundary(Collider collider)
        {
            return collider != null && (collider.name.StartsWith(ShoreBoundaryName, StringComparison.Ordinal) ||
                collider.name == "D6_ShipHullCollision");
        }

        private Transform _collisionRoot;
        private const float R03RetryIntervalSeconds = 0.25f;
        private float _nextR03RetryAt;

        public void Apply(GameObject ship)
        {
            if (IsInitialized) return;

            _collisionRoot = FindOrCreateRoot();
            ColliderCount = 0;
            WalkableSurfaceCount = 0;
            R03BuildingCollisionCount = 0;
            R03SurfaceInstalled = false;

            var r03 = GameObject.Find(R03PresentationRootName);
            var r03SurfaceReady = TryInstallR03WalkableSurface(r03);
            if (r03SurfaceReady)
            {
                ConfigureR03WalkableSurfaces(r03);
                R03SurfaceInstalled = true;
            }
            else
            {
                // Keep the original bounded fallback for scenes that do not
                // contain the R03 presentation yet.
                EnsureBox(BeachName, new Vector3(0f, -0.32f, -26f),
                    new Vector3(70f, 1.2f, 46f), true);
                WalkableSurfaceCount++;

                EnsureBox(DockName, new Vector3(0f, 0.1f, -6f),
                    new Vector3(26f, 0.18f, 3.4f), true);
                WalkableSurfaceCount++;
            }

            // R03 的角色可以从岛面进入水域；因此只保留水域外缘安全围界。
            // 旧的 ShoreBoundary(z=-2.88) 是岛内横墙，不能继续安装到 R03。
            if (!r03SurfaceReady)
            {
                EnsureBox(ShoreBoundaryName, new Vector3(0f, ShoreBoundaryWorldY, ShoreBoundaryDefaultZ),
                    new Vector3(ShoreBoundarySpanX, ShoreBoundaryHeight, ShoreBoundaryThickness), false);
            }
            EnsureBox(ShoreBoundaryName + ".South",
                new Vector3(0f, ShoreBoundaryWorldY, ShoreBoundarySouthZ),
                new Vector3(ShoreBoundarySpanX, ShoreBoundaryHeight, ShoreBoundaryThickness), false);
            EnsureBox(ShoreBoundaryName + ".West",
                new Vector3(ShoreBoundaryWestX, ShoreBoundaryWorldY,
                    (ShoreBoundarySouthZ + ShoreBoundaryNorthZ) * 0.5f),
                new Vector3(ShoreBoundaryThickness, ShoreBoundaryHeight, ShoreBoundarySpanZ), false);
            EnsureBox(ShoreBoundaryName + ".East",
                new Vector3(ShoreBoundaryEastX, ShoreBoundaryWorldY,
                    (ShoreBoundarySouthZ + ShoreBoundaryNorthZ) * 0.5f),
                new Vector3(ShoreBoundaryThickness, ShoreBoundaryHeight, ShoreBoundarySpanZ), false);

            if (!r03SurfaceReady)
            {
                EnsureBox(GangplankName, new Vector3(0f, 0.5f, -4.75f),
                    new Vector3(1.8f, 0.07f, 1.1f), true);
                WalkableSurfaceCount++;
            }

            if (!r03SurfaceReady) AddDockPropColliders();
            AddShipCollider(ship);

            IsInitialized = true;
            if (!R03SurfaceInstalled)
                _nextR03RetryAt = Time.unscaledTime;
            Debug.Log("[D6/Physics] 港口碰撞层已建立：地面=" + WalkableSurfaceCount +
                      "，碰撞体=" + ColliderCount + "，海岸安全边界=启用");
        }

        private void Update()
        {
            // ConfigurePhysics runs before the presentation bridge on a cold
            // scene load. If R03 was not active yet, the one-shot Apply call
            // used to leave the bounded fallback forever. Upgrade only the
            // collision owner after the mesh is actually available; no second
            // movement system or visual transform is introduced.
            if (!IsInitialized || R03SurfaceInstalled ||
                SceneManager.GetActiveScene().name != "InitialHarbor" ||
                Time.unscaledTime < _nextR03RetryAt)
                return;

            _nextR03RetryAt = Time.unscaledTime + R03RetryIntervalSeconds;
            TryUpgradeToR03Presentation();
        }

        private bool TryUpgradeToR03Presentation()
        {
            var r03 = GameObject.Find(R03PresentationRootName);
            if (!TryInstallR03WalkableSurface(r03)) return false;

            ConfigureR03WalkableSurfaces(r03);
            R03SurfaceInstalled = true;
            Physics.SyncTransforms();
            Debug.Log("[D6/Physics] deferred R03 organic island MeshCollider upgrade completed; fallback route surfaces disabled where covered");
            return true;
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

        private bool TryInstallR03WalkableSurface(GameObject r03)
        {
            if (r03 == null) return false;

            var terrain = FindDescendant(r03.transform, "D19_TerrainRoot");
            var surface = FindDescendant(r03.transform, "D19_R03_IslandSurface");
            var filter = surface == null ? null : surface.GetComponent<MeshFilter>();
            var meshCollider = terrain == null ? null : terrain.GetComponent<MeshCollider>();
            if (terrain == null || filter == null || filter.sharedMesh == null ||
                meshCollider == null || meshCollider.sharedMesh == null)
            {
                Debug.LogWarning("[D6/Physics] R03 authored terrain collider is not ready; using bounded fallback surfaces");
                return false;
            }

            // MeshCollider vertices are interpreted in the collider object's
            // local space.  R03's MeshFilter is authored at the presentation
            // origin, so copying its mesh onto a stale TerrainRoot override
            // (for example the former Z=5 route box) would move the physical
            // island away from the visible island.  Keep the stable collider
            // name/owner, but force its transform to match the source surface
            // before assigning the mesh.
            terrain.transform.localPosition = surface.transform.localPosition;
            terrain.transform.localRotation = surface.transform.localRotation;
            terrain.transform.localScale = surface.transform.localScale;

            var legacyBox = terrain.GetComponent<BoxCollider>();
            if (legacyBox != null) legacyBox.enabled = false;

            // Do not rebuild this collider from surface.sharedMesh here. Unity
            // replaces static-batched MeshFilter meshes with a scene-wide
            // Combined Mesh in PlayMode; assigning that combined mesh to the
            // standalone terrain owner moves collision triangles for edge
            // props (notably ShoreRock_0) away from their authored island
            // height. The scene-saved D19_TerrainRoot mesh is the sole
            // collision source; only its transform is synchronized above.
            meshCollider.convex = false;
            meshCollider.isTrigger = false;
            meshCollider.enabled = true;
            terrain.gameObject.isStatic = true;
            Physics.SyncTransforms();
            Debug.Log("[D6/Physics] R03 organic island MeshCollider installed; legacy terrain box disabled");
            return true;
        }

        private void ConfigureR03WalkableSurfaces(GameObject r03)
        {
            // R03 owns the organic island surface. The historical
            // BeachGround was a large invisible box that extended below the
            // visual island and caused both floating snaps and invisible
            // movement walls, so disable it when the formal presentation is
            // ready. The same rule applies to fallback dock/ramp boxes when
            // their visible counterparts are absent.
            DisableColliderByName(BeachName);
            var surfaceCount = 1;

            if (TryGetRendererBounds(r03.transform, "Deck", out var deckBounds))
            {
                EnsureBox(DockName,
                    new Vector3(deckBounds.center.x, deckBounds.center.y, deckBounds.center.z),
                    new Vector3(Mathf.Max(1f, deckBounds.size.x),
                        Mathf.Max(0.12f, deckBounds.size.y),
                        Mathf.Max(1f, deckBounds.size.z)), true);
                surfaceCount++;
            }
            else
            {
                DisableColliderByName(DockName);
            }

            if (TryGetRendererBounds(r03.transform, "Ramp", out var rampBounds))
            {
                EnsureBox(GangplankName,
                    new Vector3(rampBounds.center.x, rampBounds.center.y, rampBounds.center.z),
                    new Vector3(Mathf.Max(0.8f, rampBounds.size.x),
                        Mathf.Max(0.10f, rampBounds.size.y),
                        Mathf.Max(1f, rampBounds.size.z)), true);
                surfaceCount++;
            }
            else
            {
                DisableColliderByName(GangplankName);
            }

            // These colliders may have been created by an older runtime pass
            // under the hidden legacy Dock. R03 has its own visual berth;
            // leaving the old prop boxes enabled makes an obstacle appear
            // where the player sees only empty space.
            DisableLegacyDockPropColliders();
            InstallR03BuildingColliders(r03);
            WalkableSurfaceCount = surfaceCount;
        }

        private void InstallR03BuildingColliders(GameObject r03)
        {
            R03BuildingCollisionCount = 0;
            if (r03 == null) return;

            // The R03 service buildings are exterior-only presentation assets:
            // their doors open the D29/D10 flows rather than an interior scene.
            // Keep their visible body volumes solid so the CharacterController
            // and CameraFollow cannot enter the mesh and show an interior wall.
            // The collision owner remains D6/Harbor; no Collider is added to
            // the presentation prefab and no gameplay system writes here.
            for (int i = 0; i < R03BuildingNames.Length; i++)
            {
                string visualName = R03BuildingNames[i];
                var visual = FindDescendant(r03.transform, visualName);
                if (visual == null ||
                    !TryGetRendererBounds(visual, "Body", out var bodyBounds))
                {
                    DisableColliderByName(R03BuildingCollisionPrefix + visualName);
                    Debug.LogWarning("[D6/Physics] R03 building body collider skipped: " + visualName);
                    continue;
                }

                var size = bodyBounds.size;
                size.x = Mathf.Max(0.1f, size.x + 0.08f);
                size.y = Mathf.Max(0.1f, size.y + 0.08f);
                size.z = Mathf.Max(0.1f, size.z + 0.08f);
                EnsureBox(R03BuildingCollisionPrefix + visualName,
                    bodyBounds.center, size, false);
                R03BuildingCollisionCount++;
            }

            if (R03BuildingCollisionCount > 0)
                Debug.Log("[D6/Physics] R03 service building body colliders installed=" +
                    R03BuildingCollisionCount + "; owner=D6_HarborCollision");
        }

        private void DisableColliderByName(string name)
        {
            var child = FindDirectChild(_collisionRoot, name);
            if (child == null) return;
            var colliders = child.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i] != null) colliders[i].enabled = false;
        }

        private static void DisableLegacyDockPropColliders()
        {
            var dock = GameObject.Find("Dock");
            if (dock == null) return;

            string[] physicalProps =
            {
                "Pile", "HandRail", "Bollard", "Barrel", "Barrel2", "Crate", "Crate2"
            };
            var disabled = 0;
            foreach (var child in dock.GetComponentsInChildren<Transform>(true))
            {
                if (!ContainsName(physicalProps, child.name)) continue;
                var colliders = child.GetComponents<Collider>();
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] == null || !colliders[i].enabled) continue;
                    colliders[i].enabled = false;
                    disabled++;
                }
            }
            if (disabled > 0)
                Debug.Log("[D6/Physics] hidden legacy Dock prop colliders disabled=" + disabled);
        }

        private static bool TryGetRendererBounds(Transform root, string objectName, out Bounds bounds)
        {
            bounds = default(Bounds);
            if (root == null) return false;

            var child = FindDescendant(root, objectName);
            if (child == null) return false;
            var renderers = child.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return false;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds.size.sqrMagnitude > 0.0001f;
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
