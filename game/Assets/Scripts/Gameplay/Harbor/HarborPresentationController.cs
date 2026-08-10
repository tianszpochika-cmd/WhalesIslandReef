// ============================================================
// HarborPresentationController —— InitialHarbor 场景品质层
//
// 只负责表现：材质、灯光、雾、海面波动、岸线泡沫和港口氛围细节。
// 不修改贸易、航行、船员或存档数值，缺少外部美术资产时也能稳定降级。
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Whalebone.Gameplay.Harbor
{
    public sealed class HarborPresentationController : MonoBehaviour
    {
        private readonly List<Transform> _foamBands = new List<Transform>();
        private readonly List<float> _foamBaseX = new List<float>();
        private Transform _sea;
        private Vector3 _seaBasePosition;
        private bool _initialized;
        private float _time;

        public bool IsInitialized => _initialized;
        public int FoamBandCount => _foamBands.Count;

        private void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            if (_initialized) return;
            _initialized = true;
            ApplyLighting();
            ApplyCamera();
            ApplySea();
            BuildFoamBands();
            BuildLanterns();
            BuildHarborSign();
            Debug.Log("[D6/Harbor] 港口品质层已接线：灯光、雾、海面、岸线泡沫、灯笼和港口标识");
        }

        private void Update()
        {
            if (!_initialized || Time.timeScale <= 0f) return;
            _time += Time.deltaTime;
            if (_sea != null)
            {
                var position = _seaBasePosition;
                position.y += Mathf.Sin(_time * 0.42f) * 0.012f;
                _sea.position = position;
            }

            for (int i = 0; i < _foamBands.Count; i++)
            {
                var band = _foamBands[i];
                if (band == null) continue;
                var position = band.localPosition;
                position.x = _foamBaseX[i] + Mathf.Sin(_time * 0.55f + i * 1.7f) * 0.18f;
                position.y += Mathf.Sin(_time * 0.9f + i) * 0.002f;
                band.localPosition = position;
            }
        }

        private static void ApplyLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.38f, 0.58f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.72f, 0.76f, 0.76f);
            RenderSettings.ambientGroundColor = new Color(0.30f, 0.25f, 0.19f);
            RenderSettings.reflectionIntensity = 0.72f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.48f, 0.67f, 0.75f);
            RenderSettings.fogStartDistance = 80f;
            RenderSettings.fogEndDistance = 280f;

            var sun = GameObject.Find("Directional Light");
            if (sun == null)
            {
                sun = new GameObject("Directional Light");
                sun.AddComponent<Light>();
            }
            var light = sun.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.88f, 0.70f);
            light.intensity = 1.55f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;
            light.shadowBias = 0.05f;
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);

            var fill = GameObject.Find("D6_HarborFillLight");
            if (fill == null)
            {
                fill = new GameObject("D6_HarborFillLight");
                fill.transform.position = new Vector3(-5f, 8f, -8f);
                var fillLight = fill.AddComponent<Light>();
                fillLight.type = LightType.Point;
                fillLight.color = new Color(0.36f, 0.58f, 0.85f);
                fillLight.intensity = 1.2f;
                fillLight.range = 28f;
                fillLight.shadows = LightShadows.None;
            }

            QualitySettings.shadowDistance = Mathf.Max(QualitySettings.shadowDistance, 90f);
            QualitySettings.shadowResolution = ShadowResolution.High;
        }

        private static void ApplyCamera()
        {
            var camera = Camera.main;
            if (camera == null) camera = FindFirstObjectByType<Camera>();
            if (camera == null) return;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 58f;
            camera.nearClipPlane = 0.25f;
            camera.farClipPlane = 450f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
        }

        private void ApplySea()
        {
            var sea = GameObject.Find("Sea");
            if (sea == null) sea = GameObject.Find("D2_OceanSurface");
            if (sea == null) return;

            _sea = sea.transform;
            _seaBasePosition = _sea.position;
            var renderer = sea.GetComponent<Renderer>();
            if (renderer == null) return;

            var material = renderer.sharedMaterial;
            if (material == null)
            {
                material = CreateLitMaterial("D6_SeaMaterial", new Color(0.025f, 0.25f, 0.42f), 0.86f);
                renderer.sharedMaterial = material;
            }
            else
            {
                material = new Material(material) { name = "D6_SeaMaterial" };
                renderer.sharedMaterial = material;
            }
            SetMaterialColor(material, new Color(0.025f, 0.25f, 0.42f));
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.88f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.08f);
        }

        private void BuildFoamBands()
        {
            var root = GameObject.Find("D6_HarborFoam");
            if (root != null)
            {
                foreach (Transform child in root.transform)
                {
                    _foamBands.Add(child);
                    _foamBaseX.Add(child.localPosition.x);
                }
                return;
            }

            root = new GameObject("D6_HarborFoam");
            var foamMaterial = CreateTransparentMaterial("D6_FoamMaterial", new Color(0.88f, 0.97f, 0.96f, 0.30f));
            for (int i = 0; i < 4; i++)
            {
                var band = GameObject.CreatePrimitive(PrimitiveType.Quad);
                band.name = "FoamBand_" + i;
                band.transform.SetParent(root.transform, false);
                band.transform.localPosition = new Vector3(-12f + i * 8f, 0.012f, -2.92f - (i % 2) * 0.35f);
                band.transform.localRotation = Quaternion.Euler(90f, i * 13f, 0f);
                band.transform.localScale = new Vector3(5.8f, 0.22f + (i % 2) * 0.08f, 1f);
                var renderer = band.GetComponent<Renderer>();
                renderer.sharedMaterial = foamMaterial;
                var collider = band.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
                _foamBands.Add(band.transform);
                _foamBaseX.Add(band.transform.localPosition.x);
            }
        }

        private static void BuildLanterns()
        {
            if (GameObject.Find("D6_HarborLanterns") != null) return;
            var root = new GameObject("D6_HarborLanterns");
            var bodyMaterial = CreateLitMaterial("D6_LanternBody", new Color(0.08f, 0.055f, 0.035f), 0.25f);
            var glowMaterial = CreateLitMaterial("D6_LanternGlow", new Color(1f, 0.42f, 0.08f), 0.15f);
            SetEmission(glowMaterial, new Color(1f, 0.16f, 0.015f), 3.0f);
            foreach (float x in new[] { -8f, 8f })
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "LanternPost";
                post.transform.SetParent(root.transform, false);
                post.transform.localPosition = new Vector3(x, 0.95f, -7.15f);
                post.transform.localScale = new Vector3(0.08f, 0.9f, 0.08f);
                post.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
                RemoveCollider(post);

                var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lamp.name = "LanternGlow";
                lamp.transform.SetParent(root.transform, false);
                lamp.transform.localPosition = new Vector3(x, 1.95f, -7.15f);
                lamp.transform.localScale = Vector3.one * 0.18f;
                lamp.GetComponent<Renderer>().sharedMaterial = glowMaterial;
                RemoveCollider(lamp);

                var lightObject = new GameObject("LanternLight");
                lightObject.transform.SetParent(root.transform, false);
                lightObject.transform.localPosition = new Vector3(x, 1.95f, -7.15f);
                var point = lightObject.AddComponent<Light>();
                point.type = LightType.Point;
                point.color = new Color(1f, 0.34f, 0.08f);
                point.intensity = 1.8f;
                point.range = 5.5f;
                point.shadows = LightShadows.None;
            }
        }

        private static void BuildHarborSign()
        {
            if (GameObject.Find("D6_HarborSign") != null) return;
            var root = new GameObject("D6_HarborSign");
            root.transform.position = new Vector3(-7.2f, 1.15f, -7.45f);
            root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name = "SignBoard";
            board.transform.SetParent(root.transform, false);
            board.transform.localScale = new Vector3(2.6f, 0.72f, 0.08f);
            board.GetComponent<Renderer>().sharedMaterial = CreateLitMaterial("D6_SignBoard", new Color(0.22f, 0.10f, 0.045f), 0.25f);
            RemoveCollider(board);

            var textObject = new GameObject("SignText");
            textObject.transform.SetParent(root.transform, false);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.055f);
            textObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var text = textObject.AddComponent<TextMesh>();
            text.text = "SAINTE-MARGUERITE";
            text.fontSize = 48;
            text.characterSize = 0.035f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = new Color(1f, 0.82f, 0.42f);
        }

        private static Material CreateLitMaterial(string name, Color color, float smoothness)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = name };
            SetMaterialColor(material, color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private static Material CreateTransparentMaterial(string name, Color color)
        {
            var material = CreateLitMaterial(name, color, 0.8f);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 0f);
            material.renderQueue = 3000;
            return material;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material == null) return;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static void SetEmission(Material material, Color color, float intensity)
        {
            if (material == null || !material.HasProperty("_EmissionColor")) return;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
        }

        private static void RemoveCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }
    }
}
