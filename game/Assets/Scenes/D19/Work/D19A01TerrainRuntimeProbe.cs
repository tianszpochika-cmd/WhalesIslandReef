using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace WhalesIslandReef.D19A01
{
    /// <summary>
    /// D19-A01 工作场景 PlayMode smoke probe。只验证地形、碰撞、锚点和 NavMesh，
    /// 不写玩法状态、存档、Registry 或正式场景。
    /// </summary>
    public sealed class D19A01TerrainRuntimeProbe : MonoBehaviour
    {
        [SerializeField] private bool runOnStart = true;

        private void Start()
        {
            if (runOnStart) StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return null;
            var report = new D19A01ProbeReport
            {
                task = "D19-A01",
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
                checks = new D19A01ProbeChecks()
            };
            var terrain = FindObjectOfType<Terrain>();
            report.checks.terrain_present = terrain != null && terrain.terrainData != null;
            report.checks.terrain_resolution = report.checks.terrain_present ? terrain.terrainData.heightmapResolution : 0;
            report.checks.terrain_collider_present = terrain != null && terrain.GetComponent<TerrainCollider>() != null;
            report.checks.anchor_prefab_present = FindAnchor("anchor_player_recovery") && FindAnchor("anchor_beach_landing");
            report.checks.collision_prefab_present = GameObject.Find("D19_TerrainCollision") != null;
            report.checks.cave_prefab_present = GameObject.Find("D19_CaveFissure") != null;
            report.checks.boundary_colliders_present = FindObjectOfType<D19TerrainBoundaryMarker>() != null || GameObject.Find("D19_DeepSeaBoundary") != null;
            report.checks.spawn_grounded_ray = RaycastTerrain(new Vector3(-22f, 64f, 82f));
            report.checks.market_grounded_ray = RaycastTerrain(new Vector3(26f, 64f, 62f));
            report.checks.dock_grounded_ray = RaycastTerrain(new Vector3(0f, 64f, -12f));
            report.checks.cave_collision_present = GameObject.Find("D19_CaveInteriorBlocker") != null;
            report.checks.cave_blocker_ray = RaycastCaveBlocker(terrain);
            report.checks.reef_collision_ray = Physics.Raycast(new Vector3(-16f, 0f, -90f), Vector3.forward, 40f);
            report.checks.navmesh_present = NavMesh.SamplePosition(new Vector3(-22f, 2f, 82f), out _, 8f, NavMesh.AllAreas);
            report.checks.route_navmesh_samples = CountRouteSamples(terrain);
            report.status = report.checks.IsPass() ? "PASS" : "PARTIAL";
            WriteReport(report);
            Debug.Log("[D19-A01] Terrain PlayMode probe " + report.status + ": route_navmesh_samples=" + report.checks.route_navmesh_samples);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(report.status == "PASS" ? 0 : 1);
#endif
        }

        private static bool FindAnchor(string name)
        {
            return GameObject.Find(name) != null;
        }

        private static bool RaycastTerrain(Vector3 origin)
        {
            var hit = new RaycastHit();
            var result = Physics.Raycast(origin, Vector3.down, out hit, 128f);
            var terrain = FindObjectOfType<Terrain>();
            var sampled = terrain == null ? float.NaN : terrain.SampleHeight(origin);
            Debug.Log("[D19-A01] ground ray origin=" + origin + ", hit=" + result + ", collider=" + (hit.collider == null ? "<none>" : hit.collider.name) + ", point=" + hit.point + ", sample_height=" + sampled + ", terrain_position=" + (terrain == null ? "<none>" : terrain.transform.position.ToString()));
            return result && hit.collider != null;
        }

        private static int CountRouteSamples(Terrain terrain)
        {
            var points = new[]
            {
                new Vector3(-22f, 0.2f, 82f), new Vector3(-58f, 0.2f, -8f),
                new Vector3(26f, 0.2f, 62f), new Vector3(48f, 0.2f, 34f),
                new Vector3(-42f, 0.2f, 50f), new Vector3(0f, 0.2f, -12f)
            };
            var count = 0;
            foreach (var point in points)
            {
                var y = terrain == null ? point.y : terrain.transform.position.y + terrain.SampleHeight(point) + 2f;
                if (NavMesh.SamplePosition(new Vector3(point.x, y, point.z), out _, 8f, NavMesh.AllAreas)) count++;
            }
            return count;
        }

        private static bool RaycastCaveBlocker(Terrain terrain)
        {
            if (terrain == null) return false;
            var caveSurfaceY = terrain.transform.position.y + terrain.SampleHeight(new Vector3(72f, 0f, 72f));
            return Physics.Raycast(new Vector3(72f, caveSurfaceY + 1.4f, 62f), Vector3.forward, 10f);
        }

        private static void WriteReport(D19A01ProbeReport report)
        {
            var resultPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../tools/results/D19/D19-A01"));
            Directory.CreateDirectory(resultPath);
            File.WriteAllText(Path.Combine(resultPath, "D19_A01_PLAYMODE_SMOKE.json"), JsonUtility.ToJson(report, true));
        }

        [Serializable]
        private sealed class D19A01ProbeReport
        {
            public string task;
            public string scene;
            public string status;
            public D19A01ProbeChecks checks;
        }

        [Serializable]
        private sealed class D19A01ProbeChecks
        {
            public bool terrain_present;
            public int terrain_resolution;
            public bool terrain_collider_present;
            public bool anchor_prefab_present;
            public bool collision_prefab_present;
            public bool cave_prefab_present;
            public bool boundary_colliders_present;
            public bool spawn_grounded_ray;
            public bool market_grounded_ray;
            public bool dock_grounded_ray;
            public bool cave_collision_present;
            public bool cave_blocker_ray;
            public bool reef_collision_ray;
            public bool navmesh_present;
            public int route_navmesh_samples;

            public bool IsPass()
            {
                return terrain_present && terrain_resolution == 257 && terrain_collider_present &&
                       anchor_prefab_present && collision_prefab_present && cave_prefab_present &&
                       boundary_colliders_present && spawn_grounded_ray && market_grounded_ray &&
                       dock_grounded_ray && cave_collision_present && cave_blocker_ray && reef_collision_ray &&
                       navmesh_present && route_navmesh_samples >= 5;
            }
        }
    }

    // Marker exists only to make the boundary check explicit in the Work prefab when needed.
    public sealed class D19TerrainBoundaryMarker : MonoBehaviour { }
}
