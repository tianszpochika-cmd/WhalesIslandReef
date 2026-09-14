using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace WhalesIslandReef.Tests.D19Interaction
{
    public sealed class VcA02CollisionAlignmentTests
    {
        [Test]
        public void RuntimeR03InstallMovesStableColliderOntoVisibleSurface()
        {
            var controllerType = FindRuntimeType("Whalebone.Gameplay.Harbor.HarborCollisionController");
            Assert.That(controllerType, Is.Not.Null);

            var presentation = new GameObject("D19_R03_HarborPresentation");
            var terrain = new GameObject("D19_TerrainRoot");
            terrain.transform.SetParent(presentation.transform, false);
            terrain.transform.localPosition = new Vector3(0f, 0.05f, 5f);
            var legacyBox = terrain.AddComponent<BoxCollider>();
            var terrainMesh = new Mesh { name = "VC_A02_AuthoredTerrainMesh" };
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
            var mesh = new Mesh { name = "VC_A02_TestIslandMesh" };
            mesh.vertices = new[]
            {
                new Vector3(-2f, 0f, -2f), new Vector3(2f, 0f, -2f),
                new Vector3(2f, 0f, 2f), new Vector3(-2f, 0f, 2f)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;

            var host = new GameObject("VC_A02_ControllerHost");
            try
            {
                var controller = host.AddComponent(controllerType);
                var install = controllerType.GetMethod(
                    "TryInstallR03WalkableSurface",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(install, Is.Not.Null);
                Assert.That((bool)install.Invoke(controller, new object[] { presentation }), Is.True);

                var meshCollider = terrain.GetComponent<MeshCollider>();
                Assert.That(meshCollider, Is.Not.Null);
                Assert.That(meshCollider.sharedMesh, Is.SameAs(terrainMesh));
                Assert.That(meshCollider.sharedMesh, Is.Not.SameAs(mesh));
                Assert.That(meshCollider.enabled, Is.True);
                Assert.That(legacyBox.enabled, Is.False);
                Assert.That(terrain.transform.localPosition, Is.EqualTo(surface.transform.localPosition));
                Assert.That(terrain.transform.localRotation, Is.EqualTo(surface.transform.localRotation));
                Assert.That(terrain.transform.localScale, Is.EqualTo(surface.transform.localScale));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(presentation);
                UnityEngine.Object.DestroyImmediate(mesh);
                UnityEngine.Object.DestroyImmediate(terrainMesh);
            }
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
