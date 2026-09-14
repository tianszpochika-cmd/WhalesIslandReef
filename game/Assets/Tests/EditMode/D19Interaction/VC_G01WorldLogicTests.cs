using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace WhalesIslandReef.Tests.D19Interaction
{
    public sealed class VcG01WorldLogicTests
    {
        [Test]
        public void R03BridgeResolvesOnlyD6OwnedShoreBoundary()
        {
            var bridgeType = FindRuntimeType("Whalebone.Gameplay.D19.R03.D19R03HarborRuntimePresentation");
            Assert.That(bridgeType, Is.Not.Null);

            var legacyRoot = new GameObject("VC_G01_LegacyBoundaryHost");
            var legacyBoundary = new GameObject("ShoreBoundary");
            legacyBoundary.transform.SetParent(legacyRoot.transform, false);
            legacyBoundary.AddComponent<BoxCollider>();

            var collisionRoot = new GameObject("D6_HarborCollision");
            var ownedBoundary = new GameObject("ShoreBoundary");
            ownedBoundary.transform.SetParent(collisionRoot.transform, false);
            ownedBoundary.AddComponent<BoxCollider>();

            try
            {
                var find = bridgeType.GetMethod(
                    "FindOwnedShoreBoundary",
                    BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(find, Is.Not.Null);
                var result = (GameObject)find.Invoke(null, null);
                Assert.That(result, Is.SameAs(ownedBoundary));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(legacyRoot);
                UnityEngine.Object.DestroyImmediate(collisionRoot);
            }
        }

        [Test]
        public void R03BridgeRetainsD6PerimeterBoundariesOutsideWater()
        {
            var bridgeType = FindRuntimeType("Whalebone.Gameplay.D19.R03.D19R03HarborRuntimePresentation");
            Assert.That(bridgeType, Is.Not.Null);

            var collisionRoot = new GameObject("D6_HarborCollision");
            try
            {
                foreach (var name in new[] { "ShoreBoundary.South", "ShoreBoundary.West", "ShoreBoundary.East" })
                {
                    var boundary = new GameObject(name);
                    boundary.transform.SetParent(collisionRoot.transform, false);
                    boundary.AddComponent<BoxCollider>();
                }

                var hasPerimeter = bridgeType.GetMethod(
                    "HasOwnedR03OuterBoundarySet",
                    BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(hasPerimeter, Is.Not.Null);
                Assert.That((bool)hasPerimeter.Invoke(null, null), Is.True);

                var south = collisionRoot.transform.Find("ShoreBoundary.South");
                south.position = new Vector3(0f, 1.05f, -47.5f);
                var apply = bridgeType.GetMethod(
                    "ApplyR03RouteCollisionWindow",
                    BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(apply, Is.Not.Null);
                apply.Invoke(null, null);
                Assert.That(south.position.z, Is.EqualTo(-47.5f).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(collisionRoot);
            }
        }

        [Test]
        public void R03BridgeDisablesLegacyPhysicalCollidersButPreservesTriggers()
        {
            var bridgeType = FindRuntimeType("Whalebone.Gameplay.D19.R03.D19R03HarborRuntimePresentation");
            Assert.That(bridgeType, Is.Not.Null);

            var legacyRoot = new GameObject("D19_SaintMargaret_Root");
            var wall = new GameObject("LegacyInvisibleWall");
            wall.transform.SetParent(legacyRoot.transform, false);
            var wallCollider = wall.AddComponent<BoxCollider>();
            var zone = new GameObject("LegacyZoneTrigger");
            zone.transform.SetParent(legacyRoot.transform, false);
            var zoneCollider = zone.AddComponent<BoxCollider>();
            zoneCollider.isTrigger = true;

            try
            {
                var disable = bridgeType.GetMethod(
                    "DisableConflictingLegacyPhysicalColliders",
                    BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(disable, Is.Not.Null);
                disable.Invoke(null, null);
                Assert.That(wallCollider.enabled, Is.False);
                Assert.That(zoneCollider.enabled, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(legacyRoot);
            }
        }

        [Test]
        public void CollisionControllerUpgradesFallbackWhenR03MeshAppears()
        {
            var controllerType = FindRuntimeType("Whalebone.Gameplay.Harbor.HarborCollisionController");
            Assert.That(controllerType, Is.Not.Null);

            var host = new GameObject("VC_G01_CollisionControllerHost");
            GameObject presentation = null;
            GameObject terrain = null;
            Mesh mesh = null;
            Mesh terrainMesh = null;

            try
            {
                var controller = host.AddComponent(controllerType);
                var apply = controllerType.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance);
                Assert.That(apply, Is.Not.Null);

                // Apply while R03 is absent, matching the cold-load ordering
                // where D2 configures physics before the presentation bridge.
                apply.Invoke(controller, new object[] { null });
                var collisionRoot = GameObject.Find("D6_HarborCollision");
                Assert.That(collisionRoot, Is.Not.Null);
                Assert.That(collisionRoot.transform.Find("BeachGround").GetComponent<Collider>().enabled, Is.True);

                // Publish the presentation after Apply to reproduce the cold
                // load ordering that leaves the fallback installed first.
                presentation = new GameObject("D19_R03_HarborPresentation");
                terrain = new GameObject("D19_TerrainRoot");
                terrain.transform.SetParent(presentation.transform, false);
                terrain.AddComponent<BoxCollider>();
                terrainMesh = new Mesh { name = "VC_G01_AuthoredTerrainMesh" };
                terrainMesh.vertices = new[]
                {
                    new Vector3(-2f, 0f, -2f), new Vector3(2f, 0f, -2f),
                    new Vector3(2f, 0f, 2f), new Vector3(-2f, 0f, 2f)
                };
                terrainMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
                terrainMesh.RecalculateBounds();
                var authoredMeshCollider = terrain.AddComponent<MeshCollider>();
                authoredMeshCollider.sharedMesh = terrainMesh;

                var surface = new GameObject("D19_R03_IslandSurface");
                surface.transform.SetParent(presentation.transform, false);
                var filter = surface.AddComponent<MeshFilter>();
                mesh = new Mesh { name = "VC_G01_TestIslandMesh" };
                mesh.vertices = new[]
                {
                    new Vector3(-2f, 0f, -2f), new Vector3(2f, 0f, -2f),
                    new Vector3(2f, 0f, 2f), new Vector3(-2f, 0f, 2f)
                };
                mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
                mesh.RecalculateBounds();
                filter.sharedMesh = mesh;

                var upgrade = controllerType.GetMethod(
                    "TryUpgradeToR03Presentation",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.That(upgrade, Is.Not.Null);
                Assert.That((bool)upgrade.Invoke(controller, null), Is.True);

                var installed = (bool)controllerType.GetProperty("R03SurfaceInstalled").GetValue(controller, null);
                Assert.That(installed, Is.True);
                Assert.That(terrain.GetComponent<MeshCollider>(), Is.Not.Null);
                Assert.That(terrain.GetComponent<MeshCollider>().sharedMesh, Is.SameAs(terrainMesh));
                Assert.That(terrain.GetComponent<MeshCollider>().sharedMesh, Is.Not.SameAs(mesh));
                Assert.That(terrain.GetComponent<MeshCollider>().enabled, Is.True);
                Assert.That(terrain.GetComponent<BoxCollider>().enabled, Is.False);
                Assert.That(collisionRoot.transform.Find("BeachGround").GetComponent<Collider>().enabled, Is.False);
            }
            finally
            {
                var collisionRoot = GameObject.Find("D6_HarborCollision");
                if (collisionRoot != null) UnityEngine.Object.DestroyImmediate(collisionRoot);
                UnityEngine.Object.DestroyImmediate(host);
                if (presentation != null) UnityEngine.Object.DestroyImmediate(presentation);
                if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh);
                if (terrainMesh != null) UnityEngine.Object.DestroyImmediate(terrainMesh);
            }
        }

        [Test]
        public void CollisionControllerCreatesPerimeterWaterSafetyBoundaries()
        {
            var controllerType = FindRuntimeType("Whalebone.Gameplay.Harbor.HarborCollisionController");
            Assert.That(controllerType, Is.Not.Null);

            var host = new GameObject("VC_G01_PerimeterCollisionControllerHost");
            try
            {
                var controller = host.AddComponent(controllerType);
                var apply = controllerType.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance);
                Assert.That(apply, Is.Not.Null);
                apply.Invoke(controller, new object[] { null });

                var collisionRoot = GameObject.Find("D6_HarborCollision");
                Assert.That(collisionRoot, Is.Not.Null);
                string[] boundaryNames =
                {
                    "ShoreBoundary", "ShoreBoundary.South", "ShoreBoundary.West", "ShoreBoundary.East"
                };
                var isBoundary = controllerType.GetMethod("IsNavigationBoundary",
                    BindingFlags.Public | BindingFlags.Static);
                Assert.That(isBoundary, Is.Not.Null);
                foreach (var name in boundaryNames)
                {
                    var boundary = collisionRoot.transform.Find(name);
                    Assert.That(boundary, Is.Not.Null, "缺少海岸安全边界：" + name);
                    var collider = boundary.GetComponent<BoxCollider>();
                    Assert.That(collider, Is.Not.Null);
                    Assert.That(collider.enabled, Is.True);
                    Assert.That(collider.isTrigger, Is.False);
                    Assert.That((bool)isBoundary.Invoke(null, new object[] { collider }), Is.True,
                        "围界不能参与可行走地面裁决：" + name);
                }
            }
            finally
            {
                var collisionRoot = GameObject.Find("D6_HarborCollision");
                if (collisionRoot != null) UnityEngine.Object.DestroyImmediate(collisionRoot);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void HelmPointUsesScaleIndependentWorldDeckOffset()
        {
            var deckType = FindRuntimeType("Whalebone.Gameplay.D7.D7ShipDeckController");
            Assert.That(deckType, Is.Not.Null);

            var ship = new GameObject("VC_G01_ScaledShip");
            var player = new GameObject("VC_G01_BoardingPlayer");
            try
            {
                ship.transform.localScale = new Vector3(2.573956f, 2.573956f, 2.573956f);
                var deck = ship.AddComponent(deckType);
                var initialize = deckType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                Assert.That(initialize, Is.Not.Null);
                initialize.Invoke(deck, new object[] { ship.transform, player.transform, null });

                var helm = (Transform)deckType.GetProperty("HelmPoint").GetValue(deck, null);
                Assert.That(helm, Is.Not.Null);
                // Keep this duplicate world-logic guard aligned with the current
                // presentation calibration covered by D17/Q05-A03. The offset
                // is authored in world metres and must remain scale-independent.
                Assert.That(helm.position.y, Is.EqualTo(0.96f).Within(0.001f));
                Assert.That(helm.position.z, Is.EqualTo(-1.47f).Within(0.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(player);
                UnityEngine.Object.DestroyImmediate(ship);
            }
        }

        [Test]
        public void HelmPointConsumesAuthoredVisibleHelmAnchorWhenPresent()
        {
            var deckType = FindRuntimeType("Whalebone.Gameplay.D7.D7ShipDeckController");
            Assert.That(deckType, Is.Not.Null);

            var ship = new GameObject("VC_G01_AnchoredShip");
            var player = new GameObject("VC_G01_AnchoredBoardingPlayer");
            var anchorObject = new GameObject("D7_VisibleHelmAnchor");
            try
            {
                ship.transform.localScale = new Vector3(2.573956f, 2.573956f, 2.573956f);
                anchorObject.transform.SetParent(ship.transform, false);
                anchorObject.transform.localPosition = new Vector3(0.25f, 1.10f, 0.75f);
                var deck = ship.AddComponent(deckType);
                var initialize = deckType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
                Assert.That(initialize, Is.Not.Null);
                initialize.Invoke(deck, new object[] { ship.transform, player.transform, null });

                var helm = (Transform)deckType.GetProperty("HelmPoint").GetValue(deck, null);
                Assert.That(helm, Is.Not.Null);
                Assert.That(Vector3.Distance(helm.position, anchorObject.transform.position), Is.LessThan(0.001f));
                Assert.AreEqual(ship.transform, helm.parent,
                    "HelmPoint 仍必须挂在稳定 gameplay ship root。");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(anchorObject);
                UnityEngine.Object.DestroyImmediate(player);
                UnityEngine.Object.DestroyImmediate(ship);
            }
        }

        [Test]
        public void D3PreservesAuthoredR03StartingBerthWhenPresentationIsActive()
        {
            var d3Type = FindRuntimeType("Whalebone.Gameplay.D3.D3RuntimeBootstrap");
            Assert.That(d3Type, Is.Not.Null);
            var source = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/D3/D3RuntimeBootstrap.cs"));
            Assert.That(source, Does.Contain("useAuthoredR03Berth"));
            Assert.That(source, Does.Contain("D19_R03_HarborPresentation"));
            Assert.That(source, Does.Contain("_authoredHarborShipPosition"));
            Assert.That(source, Does.Not.Contain("playerSpawn"));
            Assert.That(source, Does.Not.Contain("SetWorldPosition"));
        }

        [Test]
        public void R03ShipPresentationUsesScaleIndependentPlayerAndDisembarkAnchors()
        {
            var builderType = FindRuntimeType("Whalebone.Editor.D19R03.D19R03HarborPresentationBuilder");
            Assert.That(builderType, Is.Not.Null);
            var source = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Editor/D19R03/D19R03HarborPresentationBuilder.cs"));
            Assert.That(source, Does.Contain("D2_PlayerSpawnAnchor"));
            Assert.That(source, Does.Contain("D2_DisembarkAnchor"));
            Assert.That(source, Does.Contain("InverseTransformVector"));

            var d2Source = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/D2/D2RuntimeBootstrap.cs"));
            Assert.That(d2Source, Does.Contain("ResolvePlayerSpawnPosition"));
            Assert.That(d2Source, Does.Not.Contain("-6.5f"));
        }

        [Test]
        public void InitialHarborCharacterCompositionStartsOnInlandSideOfBerth()
        {
            var source = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/CameraFollow.cs"));
            Assert.That(source, Does.Contain("yaw = 180f"));
            Assert.That(source, Does.Contain("visible ship directly between the camera and the player"));
        }

        [Test]
        public void R03WaterOwnsSwimQueryAndKeepsSafetyWallsOutsideWater()
        {
            var oceanSource = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/World/OceanAdapter.cs"));
            var collisionSource = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/Harbor/HarborCollisionController.cs"));
            var characterSource = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/D2/D2RuntimeBootstrap.cs"));
            var animationSource = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/D2/D2CharacterAnimationController.cs"));

            Assert.That(oceanSource, Does.Contain("TryGetWaterSurface"));
            Assert.That(oceanSource, Does.Contain("WaterHalfExtent"));
            Assert.That(collisionSource, Does.Contain("ShoreBoundaryNorthZ = 47.5f"));
            Assert.That(collisionSource, Does.Contain("if (!r03SurfaceReady)"));
            Assert.That(characterSource, Does.Contain("IsSwimming"));
            Assert.That(characterSource, Does.Contain("TryEnterOrMaintainSwimming"));
            Assert.That(animationSource, Does.Contain("Swimming"));
            Assert.That(animationSource, Does.Contain("ImportedVisualActionMotionActive"));
            Assert.That(animationSource, Does.Contain("ApplyImportedVisualPose"));
        }

        [Test]
        public void DisembarkRejectsGroundHitsInsideVisibleShipBounds()
        {
            var routerType = FindRuntimeType("Whalebone.Gameplay.D2.D2ControlRouter");
            Assert.That(routerType, Is.Not.Null);
            var source = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Scripts/Gameplay/D2/D2ControlRouter.cs"));
            Assert.That(source, Does.Contain("IsInsideShipPresentation"));
            Assert.That(source, Does.Contain("renderer.bounds"));
            Assert.That(source, Does.Contain("horizontalPadding"));
        }

        [Test]
        public void R03VegetationPositionsUseTheIslandHeightFunction()
        {
            var builderType = FindRuntimeType("Whalebone.Editor.D19R03.D19R03HarborPresentationBuilder");
            Assert.That(builderType, Is.Not.Null);
            var positionMethod = builderType.GetMethod("GroundVegetationPosition",
                BindingFlags.Static | BindingFlags.NonPublic);
            var clampMethod = builderType.GetMethod("ClampToIslandInterior",
                BindingFlags.Static | BindingFlags.NonPublic);
            var heightMethod = builderType.GetMethod("Height",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(positionMethod, Is.Not.Null);
            Assert.That(clampMethod, Is.Not.Null);
            Assert.That(heightMethod, Is.Not.Null);

            var builderSource = System.IO.File.ReadAllText(
                System.IO.Path.Combine(Application.dataPath, "Editor/D19R03/D19R03HarborPresentationBuilder.cs"));
            foreach (var authoredPosition in new[]
            {
                "new Vector3(-10.5f, 0.2f, -6.5f)",
                "new Vector3(-11.0f, 0.2f, -1.0f)",
                "new Vector3(-10.0f, 0.2f, 9.0f)",
                "new Vector3(-5.5f, 0.2f, 13.0f)"
            })
                Assert.That(builderSource, Does.Contain(authoredPosition));
            Assert.That(builderSource, Does.Contain("ClampToIslandInterior"));
            Assert.That(builderSource, Does.Contain("const float safeRadius = 0.80f"));
            Assert.That(builderSource, Does.Contain("PalmRootBurial"));
            Assert.That(builderSource, Does.Contain("position.y = Height(position.x, position.z) - PalmRootBurial"));
            Assert.That(builderSource, Does.Contain("RenderAuthoredPalms = false"));
            Assert.That(builderSource, Does.Contain("if (RenderAuthoredPalms)"));

            var samples = new[]
            {
                new Vector2(-18f, -6.5f), new Vector2(-18f, 10f),
                new Vector2(-12.5f, 17f), new Vector2(4.6f, 15.5f)
            };
            foreach (var sample in samples)
            {
                var position = (Vector3)positionMethod.Invoke(null,
                    new object[] { sample.x, sample.y });
                var safe = (Vector2)clampMethod.Invoke(null, new object[] { sample });
                var height = (float)heightMethod.Invoke(null,
                    new object[] { safe.x, safe.y });
                Assert.That(position.x, Is.EqualTo(safe.x).Within(0.0001f));
                Assert.That(position.z, Is.EqualTo(safe.y).Within(0.0001f));
                Assert.That(position.y, Is.EqualTo(height + 0.02f).Within(0.0001f));
                Assert.That(Mathf.Sqrt(
                    Mathf.Pow(position.x / 19.4f, 2f) +
                    Mathf.Pow(position.z / 17.8f, 2f)), Is.LessThanOrEqualTo(0.8001f));
            }
        }

        [Test]
        public void FixedShotProbeRestoresCameraFollowPreviousDisabledState()
        {
            var cameraFollowType = FindRuntimeType("Whalebone.Gameplay.CameraFollow");
            var probeType = FindRuntimeType("Whalebone.Gameplay.D19.R03.D19R03FixedShotProbe");
            Assert.That(cameraFollowType, Is.Not.Null);
            Assert.That(probeType, Is.Not.Null);

            var cameraObject = new GameObject("VC_G01_Camera");
            var probeObject = new GameObject("VC_G01_FixedShotProbe");
            try
            {
                var cameraFollow = cameraObject.AddComponent(cameraFollowType) as Behaviour;
                Assert.That(cameraFollow, Is.Not.Null);
                cameraFollow.enabled = false;
                var probe = probeObject.AddComponent(probeType);

                SetPrivateField(probeType, probe, "_cameraFollow", cameraFollow);
                SetPrivateField(probeType, probe, "_cameraFollowWasEnabled", false);
                SetPrivateField(probeType, probe, "_cameraFollowStateCaptured", true);
                var destroy = probeType.GetMethod("OnDestroy", BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.That(destroy, Is.Not.Null);
                destroy.Invoke(probe, null);

                Assert.That(cameraFollow.enabled, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(probeObject);
                UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        private static void SetPrivateField(Type type, object target, string name, object value)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private static Type FindRuntimeType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }
    }
}
