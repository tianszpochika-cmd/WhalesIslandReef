using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Whalebone.Gameplay.D2;
using Whalebone.Gameplay.Harbor;
using Whalebone.Gameplay.World;

namespace Whalebone.LocalQuality
{
    /// <summary>
    /// Read-only connected-Editor diagnostic for the current R03 shoreline.
    /// It records actual scene queries and does not move the Player or invoke
    /// gameplay methods. The file is compiled ephemerally by Unity Pipeline.
    /// </summary>
    public static class UnityPipelineShorelineDiagnostic
    {
        private const string ResultPath =
            "C:/Users/Administrator/Desktop/game/first01/game/tools/results/LOCAL-QUALITY/Q31/Q31_CURRENT_EDITOR_SHORELINE_DIAGNOSTIC_20260916_R1.json";

        public static string Write()
        {
            var json = Main();
            var directory = Path.GetDirectoryName(ResultPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(ResultPath, json);
            return ResultPath;
        }

        public static string Main()
        {
            var player = GameObject.Find("D2_PlayerAvatar");
            var controller = player == null ? null : player.GetComponent<D2CharacterController>();
            var body = player == null ? null : player.GetComponent<CharacterController>();
            var camera = Camera.main;
            var ocean = UnityEngine.Object.FindFirstObjectByType<OceanAdapter>(FindObjectsInactive.Include);
            var collision = UnityEngine.Object.FindFirstObjectByType<HarborCollisionController>(FindObjectsInactive.Include);
            var samples = new List<Sample>();
            var report = new Report
            {
                task_id = "Q31",
                probe_id = "Q31_CURRENT_EDITOR_SHORELINE_DIAGNOSTIC_20260916_R1",
                evidence_mode = "read-only connected Editor Pipeline scene queries; no Player movement or gameplay method invocation",
                captured_at_local = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"),
                scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                play_mode = Application.isPlaying,
                player_position = player == null ? Array.Empty<float>() : ToArray(player.transform.position),
                camera_position = camera == null ? Array.Empty<float>() : ToArray(camera.transform.position),
                camera_forward = camera == null ? Array.Empty<float>() : ToArray(camera.transform.forward),
                camera_right = camera == null ? Array.Empty<float>() : ToArray(camera.transform.right),
                camera_euler = camera == null ? Array.Empty<float>() : ToArray(camera.transform.eulerAngles),
                controller_swimming = controller != null && controller.IsSwimming,
                controller_grounded = controller != null && controller.IsGrounded,
                controller_movement_source = controller == null ? string.Empty : controller.MovementInputSource.ToString(),
                controller_radius = body == null ? 0f : body.radius,
                controller_height = body == null ? 0f : body.height,
                controller_center = body == null ? Array.Empty<float>() : ToArray(body.center),
                ocean_found = ocean != null,
                ocean_surface_y = ocean == null ? float.NaN : ocean.WaterSurfaceY,
                ocean_half_extent = ocean == null ? 0f : ocean.WaterHalfExtent,
                ocean_fallback_active = ocean != null && ocean.FallbackActive,
                harbor_collision_initialized = collision != null && collision.IsInitialized,
                harbor_r03_surface_installed = collision != null && collision.R03SurfaceInstalled,
                harbor_collider_count = collision == null ? 0 : collision.ColliderCount,
                samples = Array.Empty<Sample>(),
                edge_candidates = Array.Empty<EdgeCandidate>()
            };

            var sampleZ = new[] { -6f, -4.94f, -2.7f, 0f, 4f, 8f, 12f, 13.25f, 14f, 15f, 16f, 18f, 20f, 24f };
            for (var i = 0; i < sampleZ.Length; i++)
                samples.Add(ReadSample(new Vector3(0f, 0.15f, sampleZ[i]), body, ocean));

            report.samples = samples.ToArray();
            report.edge_candidates = ReadEdgeCandidates(player, body, camera, ocean).ToArray();
            report.sample_diagnostics = BuildSampleDiagnostics(report.samples);
            report.edge_diagnostics = BuildEdgeDiagnostics(report.edge_candidates);
            return JsonUtility.ToJson(report, true);
        }

        private static string BuildSampleDiagnostics(Sample[] samples)
        {
            var lines = new List<string>();
            if (samples == null) return string.Empty;
            for (var i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                var hits = new List<string>();
                if (sample.ray_hits != null)
                {
                    for (var h = 0; h < sample.ray_hits.Length; h++)
                    {
                        var hit = sample.ray_hits[h];
                        hits.Add(hit.collider + "@y=" + Format(hit.point, 1) +
                            ",walk=" + hit.walkable + ",boundary=" + hit.navigation_boundary);
                    }
                }
                var overlaps = sample.capsule_overlaps == null
                    ? string.Empty
                    : string.Join(";", sample.capsule_overlaps);
                lines.Add("z=" + Format(sample.requested_point, 2) +
                    " water=" + sample.has_water + "@" + sample.water_surface_y.ToString("F3") +
                    " hits=" + string.Join(";", hits) + " overlaps=" + overlaps);
            }
            return string.Join("||", lines);
        }

        private static string BuildEdgeDiagnostics(EdgeCandidate[] candidates)
        {
            var lines = new List<string>();
            if (candidates == null) return string.Empty;
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = candidates[i];
                var relevant = new List<string>();
                if (candidate.sampled_distances != null)
                {
                    for (var s = 0; s < candidate.sampled_distances.Length; s++)
                    {
                        var sample = candidate.sampled_distances[s];
                        if (!sample.has_water && !sample.below_hit && !sample.above_hit) continue;
                        relevant.Add("d=" + sample.distance.ToString("F2") +
                            ",p=" + Format(sample.point, 2) +
                            ",water=" + sample.has_water +
                            ",below=" + sample.below_collider + "/" + sample.walkable_below +
                            ",above=" + sample.above_collider + "/" + sample.walkable_above_water);
                    }
                }
                lines.Add(candidate.name + " dir=" + Format(candidate.direction, 2) +
                    " first=" + Format(candidate.first_water_without_walkable_ground, 2) +
                    "@" + candidate.first_water_surface_y.ToString("F3") +
                    " samples=" + string.Join(";", relevant));
            }
            return string.Join("||", lines);
        }

        private static string Format(float[] value, int decimals)
        {
            if (value == null || value.Length < 3) return string.Empty;
            var format = decimals == 1 ? "F1" : "F2";
            return value[0].ToString(format) + "," + value[1].ToString(format) + "," + value[2].ToString(format);
        }

        private static List<EdgeCandidate> ReadEdgeCandidates(GameObject player, CharacterController body,
            Camera camera, OceanAdapter ocean)
        {
            var result = new List<EdgeCandidate>();
            if (player == null || camera == null || ocean == null) return result;

            var forward = camera.transform.forward;
            var right = camera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            if (right.sqrMagnitude < 0.001f) right = Vector3.right;
            forward.Normalize();
            right.Normalize();
            var directions = new[]
            {
                new DirectionProbe { name = "camera_forward", direction = forward },
                new DirectionProbe { name = "camera_backward", direction = -forward },
                new DirectionProbe { name = "camera_right", direction = right },
                new DirectionProbe { name = "camera_left", direction = -right }
            };
            for (var d = 0; d < directions.Length; d++)
            {
                var direction = directions[d].direction;
                var candidate = new EdgeCandidate
                {
                    name = directions[d].name,
                    direction = ToArray(direction),
                    first_water_without_walkable_ground = Array.Empty<float>(),
                    first_water_surface_y = float.NaN,
                    sampled_distances = Array.Empty<EdgeSample>()
                };
                var edgeSamples = new List<EdgeSample>();
                for (var distance = 0.45f; distance <= 3.5f; distance += 0.25f)
                {
                    var point = player.transform.position + direction * distance;
                    var hasWater = ocean.TryGetWaterSurface(point, out var surfaceY);
                    var below = ReadDownwardHit(point, 0.9f);
                    var above = ReadGroundAboveWater(point, surfaceY);
                    edgeSamples.Add(new EdgeSample
                    {
                        distance = distance,
                        point = ToArray(point),
                        has_water = hasWater,
                        water_surface_y = hasWater ? surfaceY : float.NaN,
                        walkable_below = below.walkable,
                        below_hit = below.hit,
                        below_collider = below.collider,
                        walkable_above_water = above.walkable,
                        above_hit = above.hit,
                        above_collider = above.collider
                    });
                    if (candidate.first_water_without_walkable_ground.Length == 0 && hasWater &&
                        point.y <= surfaceY + 0.85f && !below.walkable && !above.walkable)
                    {
                        candidate.first_water_without_walkable_ground = ToArray(point);
                        candidate.first_water_surface_y = surfaceY;
                    }
                }
                candidate.sampled_distances = edgeSamples.ToArray();
                result.Add(candidate);
            }
            return result;
        }

        private static Sample ReadSample(Vector3 point, CharacterController body, OceanAdapter ocean)
        {
            var surfaceY = float.NaN;
            var hasWater = ocean != null && ocean.TryGetWaterSurface(point, out surfaceY);
            var sample = new Sample
            {
                requested_point = ToArray(point),
                has_water = hasWater,
                water_surface_y = float.NaN,
                ray_hits = Array.Empty<RayHit>(),
                capsule_overlaps = Array.Empty<string>()
            };
            if (sample.has_water) sample.water_surface_y = surfaceY;

            var rayHits = new List<RayHit>();
            var hits = Physics.RaycastAll(point + Vector3.up * 6f, Vector3.down, 14f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            for (var i = 0; i < hits.Length && i < 12; i++)
            {
                var hit = hits[i];
                if (hit.collider == null) continue;
                rayHits.Add(new RayHit
                {
                    collider = hit.collider.name,
                    root = hit.collider.transform.root == null ? string.Empty : hit.collider.transform.root.name,
                    layer = hit.collider.gameObject.layer,
                    distance = hit.distance,
                    point = ToArray(hit.point),
                    walkable = IsWalkable(hit.collider),
                    navigation_boundary = HarborCollisionController.IsNavigationBoundary(hit.collider)
                });
            }
            sample.ray_hits = rayHits.ToArray();

            if (body != null)
            {
                var scale = Mathf.Abs(body.transform.lossyScale.y);
                var radius = body.radius * Mathf.Max(0.01f, Mathf.Abs(body.transform.lossyScale.x));
                var height = Mathf.Max(body.height * scale, radius * 2f);
                var center = point + Vector3.up * body.center.y * scale;
                var half = Mathf.Max(0f, height * 0.5f - radius);
                var overlaps = Physics.OverlapCapsule(center + Vector3.up * half, center - Vector3.up * half,
                    radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
                var capsuleOverlaps = new List<string>();
                for (var i = 0; i < overlaps.Length && i < 16; i++)
                {
                    var collider = overlaps[i];
                    if (collider == null || collider.transform == body.transform ||
                        collider.transform.IsChildOf(body.transform)) continue;
                    capsuleOverlaps.Add(collider.name + "|root=" + collider.transform.root.name +
                        "|walkable=" + IsWalkable(collider) +
                        "|boundary=" + HarborCollisionController.IsNavigationBoundary(collider));
                }
                sample.capsule_overlaps = capsuleOverlaps.ToArray();
            }
            return sample;
        }

        private static HitSummary ReadDownwardHit(Vector3 point, float distance)
        {
            var hits = Physics.RaycastAll(point + Vector3.up * 0.75f, Vector3.down, distance + 0.75f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider == null || HarborCollisionController.IsNavigationBoundary(hit.collider)) continue;
                return new HitSummary
                {
                    hit = true,
                    walkable = IsWalkable(hit.collider),
                    collider = hit.collider.name
                };
            }
            return new HitSummary();
        }

        private static HitSummary ReadGroundAboveWater(Vector3 point, float surfaceY)
        {
            if (float.IsNaN(surfaceY)) return new HitSummary();
            var hits = Physics.RaycastAll(new Vector3(point.x, surfaceY + 2f, point.z), Vector3.down, 2f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider == null || HarborCollisionController.IsNavigationBoundary(hit.collider)) continue;
                return new HitSummary
                {
                    hit = hit.point.y > surfaceY + 0.02f,
                    walkable = hit.point.y > surfaceY + 0.02f && IsWalkable(hit.collider),
                    collider = hit.collider.name
                };
            }
            return new HitSummary();
        }

        private static bool IsWalkable(Collider collider)
        {
            if (collider == null) return false;
            var name = collider.name ?? string.Empty;
            var root = collider.transform.root == null ? string.Empty : collider.transform.root.name;
            return name.IndexOf("D19_TerrainRoot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   root.IndexOf("D19_TerrainRoot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("BeachGround", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("DockGround", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("GangplankGround", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static float[] ToArray(Vector3 value) => new[] { value.x, value.y, value.z };

        [Serializable]
        public sealed class Report
        {
            public string task_id;
            public string probe_id;
            public string evidence_mode;
            public string captured_at_local;
            public string scene;
            public bool play_mode;
            public float[] player_position;
            public float[] camera_position;
            public float[] camera_forward;
            public float[] camera_right;
            public float[] camera_euler;
            public bool controller_swimming;
            public bool controller_grounded;
            public string controller_movement_source;
            public float controller_radius;
            public float controller_height;
            public float[] controller_center;
            public bool ocean_found;
            public float ocean_surface_y;
            public float ocean_half_extent;
            public bool ocean_fallback_active;
            public bool harbor_collision_initialized;
            public bool harbor_r03_surface_installed;
            public int harbor_collider_count;
            public Sample[] samples;
            public EdgeCandidate[] edge_candidates;
            public string sample_diagnostics;
            public string edge_diagnostics;
        }

        [Serializable]
        public sealed class Sample
        {
            public float[] requested_point;
            public bool has_water;
            public float water_surface_y;
            public RayHit[] ray_hits;
            public string[] capsule_overlaps;
        }

        [Serializable]
        public sealed class RayHit
        {
            public string collider;
            public string root;
            public int layer;
            public float distance;
            public float[] point;
            public bool walkable;
            public bool navigation_boundary;
        }

        [Serializable]
        public sealed class EdgeCandidate
        {
            public string name;
            public float[] direction;
            public float[] first_water_without_walkable_ground;
            public float first_water_surface_y;
            public EdgeSample[] sampled_distances;
        }

        [Serializable]
        public sealed class EdgeSample
        {
            public float distance;
            public float[] point;
            public bool has_water;
            public float water_surface_y;
            public bool walkable_below;
            public bool below_hit;
            public string below_collider;
            public bool walkable_above_water;
            public bool above_hit;
            public string above_collider;
        }

        [Serializable]
        public sealed class DirectionProbe
        {
            public string name;
            public Vector3 direction;
        }

        private struct HitSummary
        {
            public bool hit;
            public bool walkable;
            public string collider;
        }
    }
}
