using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WhalesIslandReef.Tests.D19S02
{
    public sealed class D19S02ScenePlayModeTests
    {
        private const string SceneName = "InitialHarbor";
        private const string ReportRelativePath = "tools/results/D19/D19-S02/Preflight/D19_S02_PLAYMODE_SMOKE.json";

        [UnityTest]
        public IEnumerator FormalSceneLoadsStableRouteAndNavigationAnchors()
        {
            var load = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, "InitialHarbor must be present in Build Settings.");
            while (!load.isDone) yield return null;
            yield return new WaitForSeconds(1.0f);

            var result = new SmokeResult
            {
                scene_loaded = SceneManager.GetActiveScene().name == SceneName,
                integration_root = GameObject.Find("D19_SaintMargaret_Root") != null,
                r03_presentation_loaded = GameObject.Find("D19_R03_HarborPresentation") != null,
                dev_overlay_default_off = GameObject.Find("D19_DevOverlay") == null,
                water_surface_y = ReadWaterSurfaceY(),
                stable_anchor_count = 0,
                grounded_anchor_count = 0,
                navmesh_anchor_count = 0,
                route_anchor_ids = new List<string>(),
                runtime_route_anchor_count = 0,
                runtime_grounded_anchor_count = 0,
                runtime_navmesh_anchor_count = 0,
                runtime_route_anchor_ids = new List<string>(),
                legacy_coordinate_grounded_anchor_count = 0
            };

            Assert.That(result.scene_loaded, Is.True);
            Assert.That(result.integration_root, Is.True, "D19 integration root is missing at runtime.");
            Assert.That(result.r03_presentation_loaded, Is.True, "Current R03 presentation is missing at runtime.");
            Assert.That(result.dev_overlay_default_off, Is.True, "D19 development overlay is visible by default.");
            Assert.That(result.water_surface_y, Is.EqualTo(0f).Within(0.01f));

            // Stable D19 IDs are retained as compatibility/migration markers.
            // Their historical D19-S02 coordinates are not the active R03
            // presentation frame and therefore must not be used as the only
            // physical route probe. The old probe produced 1/6 hits after the
            // R03 collision owner correctly disabled duplicate legacy boxes.
            foreach (var route in StableRouteAnchors())
            {
                var anchor = GameObject.Find("D19_Anchor_" + route.id);
                Assert.That(anchor, Is.Not.Null, "Missing stable route anchor " + route.id);
                result.stable_anchor_count++;
                result.route_anchor_ids.Add(route.id);

                if (HasGroundSurface(route.position, null))
                {
                    result.grounded_anchor_count++;
                    result.legacy_coordinate_grounded_anchor_count++;
                }
                if (NavMesh.SamplePosition(route.position + Vector3.up * 4f, out _, 8f, NavMesh.AllAreas))
                    result.navmesh_anchor_count++;
            }

            foreach (var route in ActiveRuntimeRouteAnchors())
            {
                var target = FindSceneObject(route.object_path);
                Assert.That(target, Is.Not.Null, "Missing active R03 route object " + route.object_path);
                result.runtime_route_anchor_count++;
                result.runtime_route_anchor_ids.Add(route.id);

                var position = target.transform.position;
                if (HasGroundSurface(position, target))
                    result.runtime_grounded_anchor_count++;
                if (NavMesh.SamplePosition(position + Vector3.up * 4f, out _, 8f, NavMesh.AllAreas))
                    result.runtime_navmesh_anchor_count++;
            }

            Assert.That(result.stable_anchor_count, Is.EqualTo(6));
            Assert.That(result.runtime_route_anchor_count, Is.EqualTo(6));
            Assert.That(result.runtime_grounded_anchor_count, Is.GreaterThanOrEqualTo(5), "At least five active R03 route anchors must have a collision surface.");
            Assert.That(result.runtime_navmesh_anchor_count, Is.GreaterThanOrEqualTo(4), "At least four active R03 route anchors must be reachable by NavMesh.");
            WriteReport(result);
        }

        private static IEnumerable<RouteAnchor> StableRouteAnchors()
        {
            yield return new RouteAnchor("D2_PlayerAvatar", new Vector3(-22f, 0.2f, 88f));
            yield return new RouteAnchor("building_market_santa_margarita", new Vector3(26f, 0.2f, 62f));
            yield return new RouteAnchor("building_warehouse_santa_margarita", new Vector3(48f, 0.2f, 34f));
            yield return new RouteAnchor("building_tavern_santa_margarita", new Vector3(-42f, 0.2f, 50f));
            yield return new RouteAnchor("building_shipyard_santa_margarita", new Vector3(60f, 0.2f, 8f));
            yield return new RouteAnchor("building_saint_marys_dock", new Vector3(0f, 0.2f, -12f));
        }

        private static IEnumerable<RuntimeRouteAnchor> ActiveRuntimeRouteAnchors()
        {
            yield return new RuntimeRouteAnchor("D2_PlayerAvatar", "D2_PlayerAvatar");
            yield return new RuntimeRouteAnchor("building:market_santa_margarita", "MarketVisual");
            yield return new RuntimeRouteAnchor("building:warehouse_santa_margarita", "WarehouseVisual");
            yield return new RuntimeRouteAnchor("building:tavern_santa_margarita", "TavernVisual");
            yield return new RuntimeRouteAnchor("building:shipyard_santa_margarita", "ShipyardVisual");
            yield return new RuntimeRouteAnchor("anchor.dock.primary", "D19_R03_DockAndBerth/Deck");
        }

        private static GameObject FindSceneObject(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var direct = GameObject.Find(path);
            if (direct != null) return direct;
            var parts = path.Split('/');
            var current = GameObject.Find(parts[0]);
            for (var i = 1; current != null && i < parts.Length; i++)
            {
                var child = current.transform.Find(parts[i]);
                current = child == null ? null : child.gameObject;
            }
            return current;
        }

        private static bool HasGroundSurface(Vector3 position, GameObject ignoredRoot)
        {
            var hits = Physics.RaycastAll(position + Vector3.up * 40f, Vector3.down, 100f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < hits.Length; i++)
            {
                var collider = hits[i].collider;
                if (collider == null) continue;
                if (ignoredRoot != null && (collider.transform == ignoredRoot.transform ||
                    collider.transform.IsChildOf(ignoredRoot.transform)))
                    continue;
                return true;
            }
            return false;
        }

        private static float ReadWaterSurfaceY()
        {
            var water = GameObject.Find("WaterSurface_SeaLevelY0");
            return water == null ? float.NaN : water.transform.position.y;
        }

        private static void WriteReport(SmokeResult result)
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var path = Path.Combine(root, ReportRelativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(result, true));
        }

        private readonly struct RouteAnchor
        {
            public readonly string id;
            public readonly Vector3 position;

            public RouteAnchor(string id, Vector3 position)
            {
                this.id = id;
                this.position = position;
            }
        }

        private readonly struct RuntimeRouteAnchor
        {
            public readonly string id;
            public readonly string object_path;

            public RuntimeRouteAnchor(string id, string objectPath)
            {
                this.id = id;
                object_path = objectPath;
            }
        }

        [Serializable]
        private sealed class SmokeResult
        {
            public bool scene_loaded;
            public bool integration_root;
            public bool r03_presentation_loaded;
            public bool dev_overlay_default_off;
            public float water_surface_y;
            public int stable_anchor_count;
            public int grounded_anchor_count;
            public int navmesh_anchor_count;
            public List<string> route_anchor_ids;
            public int runtime_route_anchor_count;
            public int runtime_grounded_anchor_count;
            public int runtime_navmesh_anchor_count;
            public List<string> runtime_route_anchor_ids;
            public int legacy_coordinate_grounded_anchor_count;
        }
    }
}
