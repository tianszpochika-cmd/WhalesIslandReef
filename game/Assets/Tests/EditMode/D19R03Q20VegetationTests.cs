using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Save.Tests
{
    /// <summary>
    /// Q20 只审计一个候选植被资源的真实 Renderer bounds 与落地校正。
    /// 该测试不把 Candidate/Pending 资源提升为 LocalDevelopmentReady。
    /// </summary>
    public sealed class D19R03Q20VegetationTests
    {
        private const string CandidatePath =
            "Assets/Art/Candidates/ShipsPortProps/KenneyPirateKit/Models/palm-straight.fbx";

        private GameObject _instance;

        [TearDown]
        public void TearDown()
        {
            if (_instance != null) Object.DestroyImmediate(_instance);
            _instance = null;
        }

        [Test]
        public void PalmCandidate_HasRenderableMaterials_AndRendererBoundsCanGroundWorkPrefab()
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(CandidatePath);
            Assert.IsNotNull(asset, "Q20 candidate palm must be imported before it can be audited.");

            var assetRenderers = asset.GetComponentsInChildren<Renderer>(true);
            Assert.IsNotEmpty(assetRenderers, "Q20 candidate palm has no renderers.");
            int missingMaterials = 0;
            foreach (var renderer in assetRenderers)
                foreach (var material in renderer.sharedMaterials)
                    if (material == null) missingMaterials++;
            Assert.AreEqual(0, missingMaterials, "Q20 candidate palm has missing material slots.");

            _instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            Assert.IsNotNull(_instance, "Q20 candidate palm could not be instantiated as a work prefab.");
            _instance.name = "Q20_WorkPalm_GroundingProbe";
            _instance.transform.position = new Vector3(-10.5f, 0.20f, -6.5f);
            var before = WorldBounds(_instance);
            Assert.Greater(before.size.y, 0.05f, "Q20 candidate palm has no measurable vertical bounds.");

            // This is the same presentation-only correction contract used by
            // the R03 candidate placement path: align the actual visible
            // Renderer minimum with the sampled ground height, not the source
            // FBX root pivot.
            const float desiredGroundY = 0.20f;
            var position = _instance.transform.position;
            position.y += desiredGroundY - before.min.y;
            _instance.transform.position = position;
            var after = WorldBounds(_instance);
            Assert.That(after.min.y, Is.EqualTo(desiredGroundY).Within(0.001f));

            Debug.Log(string.Format(
                "[Q20] candidate={0}, renderer_count={1}, missing_materials={2}, " +
                "before_min_y={3:F4}, after_min_y={4:F4}, bounds_height={5:F4}",
                CandidatePath, assetRenderers.Length, missingMaterials,
                before.min.y, after.min.y, after.size.y));
        }

        private static Bounds WorldBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            Assert.IsNotEmpty(renderers);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }
    }
}
