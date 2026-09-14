using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Whalebone.Systems.D9;

namespace Save.Tests
{
    /// <summary>
    /// Q22 的代码/Prefab 级船海天气审计。
    ///
    /// 该测试检查水线、泡沫、船体碰撞吃水、D28 天气粒子参数、D9 鱼类天气
    /// 只读投影和音效 owner 边界；它不替代 native Player 的船海镜头验收。
    /// </summary>
    public sealed class D19R03Q22SeaWeatherAuditTests
    {
        private const string HarborPrefabPath =
            "Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab";
        private const string ShipPrefabPath =
            "Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_ShipVisual.prefab";
        private const float ExpectedSeaLevelY = -0.04f;

        private GameObject _harborInstance;
        private GameObject _shipInstance;
        private GameObject _weatherObject;
        private bool _previousFog;
        private FogMode _previousFogMode;
        private Color _previousFogColor;
        private float _previousFogStart;
        private float _previousFogEnd;
        private Color _previousAmbient;

        [SetUp]
        public void SetUp()
        {
            _previousFog = RenderSettings.fog;
            _previousFogMode = RenderSettings.fogMode;
            _previousFogColor = RenderSettings.fogColor;
            _previousFogStart = RenderSettings.fogStartDistance;
            _previousFogEnd = RenderSettings.fogEndDistance;
            _previousAmbient = RenderSettings.ambientLight;
        }

        [TearDown]
        public void TearDown()
        {
            if (_harborInstance != null) UnityEngine.Object.DestroyImmediate(_harborInstance);
            if (_shipInstance != null) UnityEngine.Object.DestroyImmediate(_shipInstance);
            if (_weatherObject != null) UnityEngine.Object.DestroyImmediate(_weatherObject);
            RenderSettings.fog = _previousFog;
            RenderSettings.fogMode = _previousFogMode;
            RenderSettings.fogColor = _previousFogColor;
            RenderSettings.fogStartDistance = _previousFogStart;
            RenderSettings.fogEndDistance = _previousFogEnd;
            RenderSettings.ambientLight = _previousAmbient;
            _harborInstance = null;
            _shipInstance = null;
            _weatherObject = null;
        }

        [Test]
        public void R03SeaAndShipPresentation_HasSeaLevelDraftMaterialsAndWeatherOwnerContracts()
        {
            var harborPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HarborPrefabPath);
            var shipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShipPrefabPath);
            Assert.IsNotNull(harborPrefab, "Q22 R03 harbor presentation Prefab must be importable.");
            Assert.IsNotNull(shipPrefab, "Q22 R03 ship presentation Prefab must be importable.");

            _harborInstance = PrefabUtility.InstantiatePrefab(harborPrefab) as GameObject;
            _shipInstance = PrefabUtility.InstantiatePrefab(shipPrefab) as GameObject;
            Assert.IsNotNull(_harborInstance);
            Assert.IsNotNull(_shipInstance);

            var shoreline = FindByName(_harborInstance.transform, "D19_R03_ShorelineAndWater");
            Assert.IsNotNull(shoreline, "R03 shoreline/water owner is missing.");
            var water = FindByName(shoreline, "WaterWaveSurface");
            var beach = FindByName(shoreline, "R03_Beach");
            Assert.IsNotNull(water, "R03 water surface is missing.");
            Assert.IsNotNull(beach, "R03 beach/wet-edge mesh is missing.");

            var waterRenderer = water.GetComponent<Renderer>();
            var beachRenderer = beach.GetComponent<Renderer>();
            Assert.IsNotNull(waterRenderer);
            Assert.IsNotNull(beachRenderer);
            Assert.Greater(waterRenderer.bounds.size.x, 80f, "R03 water surface is too small for the harbor cameras.");
            Assert.Greater(waterRenderer.bounds.size.z, 80f, "R03 water surface is too small for the harbor cameras.");
            Assert.That(waterRenderer.bounds.center.y, Is.EqualTo(ExpectedSeaLevelY).Within(0.02f));
            Assert.That(beachRenderer.bounds.min.y, Is.GreaterThanOrEqualTo(-0.20f));
            Assert.AreEqual(0, shoreline.GetComponentsInChildren<Collider>(true).Length,
                "Water/shoreline presentation must not introduce a second collision authority.");

            int waterMissingMaterials = CountMissingMaterials(shoreline);
            Assert.AreEqual(0, waterMissingMaterials, "R03 water/foam/shoreline has missing material slots.");
            int foamRendererCount = CountRenderersWithPrefix(shoreline, "FoamRing");
            Assert.GreaterOrEqual(foamRendererCount, 20, "R03 shoreline foam ring is incomplete.");

            var shipRenderers = _shipInstance.GetComponentsInChildren<Renderer>(true);
            Assert.IsNotEmpty(shipRenderers, "R03 ship Prefab has no visible renderers.");
            Assert.AreEqual(0, CountMissingMaterials(_shipInstance.transform),
                "R03 ship presentation has missing material slots.");
            var shipHullCollision = FindByName(_shipInstance.transform, "D6_ShipHullCollision");
            Assert.IsNotNull(shipHullCollision, "Stable D6 ship hull collision child is missing.");
            var hullCollider = shipHullCollision.GetComponent<BoxCollider>();
            Assert.IsNotNull(hullCollider);
            Assert.IsFalse(hullCollider.isTrigger);
            Assert.That(hullCollider.bounds.min.y, Is.EqualTo(ExpectedSeaLevelY).Within(0.01f),
                "Ship hull collision bottom must meet the waterline instead of floating above it.");
            var shipBounds = WorldBounds(_shipInstance);
            Assert.That(shipBounds.min.y, Is.GreaterThanOrEqualTo(ExpectedSeaLevelY - 0.005f),
                "Visible ship mesh must not be below the sea surface.");
            Assert.Greater(shipBounds.max.y, ExpectedSeaLevelY + 0.5f);

            var presentationAudio = _harborInstance.GetComponentsInChildren<AudioSource>(true);
            Assert.IsEmpty(presentationAudio,
                "R03 harbor presentation must not duplicate D28/D9 audio ownership.");

            var d9 = new EnvironmentSystem();
            var calm = d9.Snapshot;
            var rain = d9.SetWeather(D9WeatherState.Rain, 1, "nearshore_saint_marys").Snapshot;
            var storm = d9.ApplyStormEvent(2, "nearshore_saint_marys", 2).Snapshot;
            var calmFish = D9EcologyWeatherReader.ReadFishConditions(calm);
            var rainFish = D9EcologyWeatherReader.ReadFishConditions(rain);
            var stormFish = D9EcologyWeatherReader.ReadFishConditions(storm);
            Assert.Greater(calmFish.ActivityMultiplier, rainFish.ActivityMultiplier);
            Assert.Greater(rainFish.ActivityMultiplier, stormFish.ActivityMultiplier);
            Assert.Greater(calmFish.CatchMultiplier, rainFish.CatchMultiplier);
            Assert.Greater(rainFish.CatchMultiplier, stormFish.CatchMultiplier);

            var weatherMetrics = AuditD28WeatherRuntime();
            Debug.Log(string.Format(
                "[Q22] water_size=({0:F2},{1:F2}), sea_level={2:F4}, beach_min_y={3:F4}, " +
                "foam_renderers={4}, water_missing_materials={5}, ship_min_y={6:F4}, " +
                "hull_bottom_y={7:F4}, presentation_audio={8}, fish_activity={9:F2}/{10:F2}/{11:F2}, " +
                "weather_metrics={12}",
                waterRenderer.bounds.size.x,
                waterRenderer.bounds.size.z,
                waterRenderer.bounds.center.y,
                beachRenderer.bounds.min.y,
                foamRendererCount,
                waterMissingMaterials,
                shipBounds.min.y,
                hullCollider.bounds.min.y,
                presentationAudio.Length,
                calmFish.ActivityMultiplier,
                rainFish.ActivityMultiplier,
                stormFish.ActivityMultiplier,
                weatherMetrics));
        }

        private string AuditD28WeatherRuntime()
        {
            var runtimeType = Type.GetType(
                "Whalebone.Gameplay.D28.D28WaterWeatherPresentationRuntime, Assembly-CSharp");
            Assert.IsNotNull(runtimeType, "D28 weather presentation runtime is missing.");

            _weatherObject = new GameObject("Q22_D28WeatherAudit");
            var runtime = _weatherObject.AddComponent(runtimeType);
            var buildMethod = runtimeType.GetMethod("BuildPrecipitation",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var setWeather = runtimeType.GetMethod("SetWeather",
                BindingFlags.Instance | BindingFlags.Public);
            var weatherEnum = runtimeType.GetNestedType("PresentationWeather",
                BindingFlags.Public | BindingFlags.NonPublic);
            var waveProperty = runtimeType.GetProperty("SurfaceWaveMultiplier",
                BindingFlags.Static | BindingFlags.Public);
            Assert.IsNotNull(buildMethod);
            Assert.IsNotNull(setWeather);
            Assert.IsNotNull(weatherEnum);
            Assert.IsNotNull(waveProperty);

            buildMethod.Invoke(runtime, null);
            var precipitation = _weatherObject.GetComponentInChildren<ParticleSystem>(true);
            Assert.IsNotNull(precipitation, "D28 precipitation ParticleSystem was not created.");
            var precipitationRenderer = precipitation.GetComponent<ParticleSystemRenderer>();
            Assert.IsNotNull(precipitationRenderer);
            Assert.IsNotNull(precipitationRenderer.sharedMaterial,
                "D28 precipitation renderer has no material/shader.");
            Assert.AreEqual(ParticleSystemRenderMode.Stretch, precipitationRenderer.renderMode);
            Assert.That(precipitation.main.maxParticles, Is.EqualTo(1400));

            var metrics = new List<string>();
            AuditWeatherState("Calm", 0f, 1f);
            metrics.Add("Calm:" + ParticleRate(precipitation));
            AuditWeatherState("Rain", 560f, 1.40f);
            metrics.Add("Rain:" + ParticleRate(precipitation));
            AuditWeatherState("Storm", 980f, 2.35f);
            metrics.Add("Storm:" + ParticleRate(precipitation));
            setWeather.Invoke(runtime, new[] { Enum.Parse(weatherEnum, "Calm") });
            return string.Join(",", metrics.ToArray());

            void AuditWeatherState(string name, float expectedRate, float expectedWaveMultiplier)
            {
                setWeather.Invoke(runtime, new[] { Enum.Parse(weatherEnum, name) });
                var emission = precipitation.emission;
                Assert.That(emission.rateOverTime.constantMax, Is.EqualTo(expectedRate).Within(0.1f),
                    name + " D28 precipitation rate mismatch.");
                Assert.That((float)waveProperty.GetValue(null), Is.EqualTo(expectedWaveMultiplier).Within(0.001f),
                    name + " D28 water wave multiplier mismatch.");
                if (expectedRate > 0f)
                    Assert.IsTrue(precipitation.isPlaying, name + " precipitation must be playing.");
            }
        }

        private static string ParticleRate(ParticleSystem precipitation)
        {
            return precipitation.emission.rateOverTime.constantMax.ToString("F0");
        }

        private static Bounds WorldBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            Assert.IsNotEmpty(renderers);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static int CountMissingMaterials(Transform root)
        {
            int missing = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                foreach (var material in renderer.sharedMaterials)
                    if (material == null) missing++;
            return missing;
        }

        private static int CountRenderersWithPrefix(Transform root, string prefix)
        {
            int count = 0;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                if (transform.name.StartsWith(prefix, StringComparison.Ordinal) &&
                    transform.GetComponent<Renderer>() != null)
                    count++;
            return count;
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindByName(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
