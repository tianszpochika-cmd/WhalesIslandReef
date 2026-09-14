// ============================================================
// D7MaterialPolishController —— 低模材质保真与安全修复
//
// D5 的角色预设只应改变色调，不应把 FBX 贴图替换成纯色材质。
// 本类只复制材质实例并保留纹理；遇到紫色/缺失材质时使用可读调色板。
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Whalebone.Gameplay.D7
{
    public sealed class D7MaterialPolishController : MonoBehaviour
    {
        private const int RenderersPerFrame = 8;
        private float _nextApply;
        private int _polishedTotal;
        private int _preservedTextureTotal;
        private readonly List<Renderer> _pendingRenderers = new List<Renderer>();
        private readonly HashSet<int> _queuedRendererIds = new HashSet<int>();
        private int _nextRendererIndex;
        private bool _hasScanned;

        public bool IsReady { get; private set; }
        public int PolishedRendererCount => _polishedTotal;
        public int PreservedTextureCount => _preservedTextureTotal;

        private void Update()
        {
            if (Time.unscaledTime >= _nextApply)
            {
                _nextApply = Time.unscaledTime + 0.75f;
                Apply();
            }
            ProcessPendingRenderers();
        }

        public void Apply()
        {
            QueueUnpolishedRenderers();
        }

        private void QueueUnpolishedRenderers()
        {
            _hasScanned = true;
            foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (renderer == null || renderer.GetComponentInParent<Canvas>() != null) continue;
                if (IsD20PresentationOwned(renderer)) continue;
                if (!NeedsPolish(renderer)) continue;
                if (_queuedRendererIds.Add(renderer.GetInstanceID())) _pendingRenderers.Add(renderer);
            }
            if (_pendingRenderers.Count > _nextRendererIndex) IsReady = false;
            if (_pendingRenderers.Count == 0 && _hasScanned) IsReady = true;
        }

        private void ProcessPendingRenderers()
        {
            int processed = 0;
            while (_nextRendererIndex < _pendingRenderers.Count && processed < RenderersPerFrame)
            {
                var renderer = _pendingRenderers[_nextRendererIndex++];
                _queuedRendererIds.Remove(renderer == null ? 0 : renderer.GetInstanceID());
                ProcessRenderer(renderer);
                processed++;
            }

            if (_nextRendererIndex < _pendingRenderers.Count) return;

            _pendingRenderers.Clear();
            _nextRendererIndex = 0;
            IsReady = _hasScanned;
        }

        private void ProcessRenderer(Renderer renderer)
        {
            if (renderer == null || renderer.GetComponentInParent<Canvas>() != null) return;

            var materials = renderer.sharedMaterials;
            int preservedTextureCount = 0;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                var source = materials[i];
                if (source != null && source.name.StartsWith("D7Polish_", StringComparison.Ordinal)) continue;

                var material = source == null ? CreateFallbackMaterial(renderer.name) : new Material(source);
                material.name = "D7Polish_" + renderer.name + "_" + i;
                bool hasTexture = material.mainTexture != null;
                if (hasTexture) preservedTextureCount++;
                ApplyReadableSurface(material, renderer.name, hasTexture);
                materials[i] = material;
                changed = true;
            }

            if (!changed) return;
            renderer.sharedMaterials = materials;
            _polishedTotal++;
            _preservedTextureTotal += preservedTextureCount;
        }

        private static bool NeedsPolish(Renderer renderer)
        {
            var materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0) return false;
            foreach (var material in materials)
            {
                if (material == null || !material.name.StartsWith("D7Polish_", StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        private static bool IsD20PresentationOwned(Renderer renderer)
        {
            var bridge = renderer == null
                ? null
                : renderer.GetComponentInParent<Whalebone.Gameplay.D2.D20PlayerCandidatePresentationBridge>();
            return bridge != null && bridge.IsInstalled;
        }

        private static Material CreateFallbackMaterial(string name)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { name = "D7Fallback_" + name };
            SetColor(material, Palette(name));
            return material;
        }

        private static void ApplyReadableSurface(Material material, string objectName, bool hasTexture)
        {
            if (material == null) return;
            Color current = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.white;
            if (IsMagenta(current))
                current = Palette(objectName);

            if (hasTexture)
            {
                // 纹理是角色/船只可读性的主体，只轻微染色，不覆盖原图。
                Color tint = Color.Lerp(Color.white, Palette(objectName), 0.10f);
                current = new Color(current.r * tint.r, current.g * tint.g, current.b * tint.b, current.a);
            }
            else if (IsNearlyWhite(current) || IsNearlyGray(current))
            {
                current = Palette(objectName);
            }

            SetColor(material, current);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", hasTexture ? 0.38f : 0.28f);
        }

        private static void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static Color Palette(string value)
        {
            string name = (value ?? string.Empty).ToLowerInvariant();
            if (name.Contains("sea") || name.Contains("ocean")) return new Color(0.035f, 0.24f, 0.40f);
            if (name.Contains("crew") || name.Contains("player")) return new Color(0.68f, 0.43f, 0.22f);
            if (name.Contains("ship") || name.Contains("boat")) return new Color(0.38f, 0.19f, 0.08f);
            if (name.Contains("palm") || name.Contains("grass") || name.Contains("flower")) return new Color(0.22f, 0.42f, 0.17f);
            if (name.Contains("rock")) return new Color(0.32f, 0.34f, 0.35f);
            return new Color(0.55f, 0.39f, 0.24f);
        }

        private static bool IsMagenta(Color color)
        {
            return color.r > 0.8f && color.b > 0.8f && color.g < 0.2f;
        }

        private static bool IsNearlyWhite(Color color)
        {
            return color.r > 0.94f && color.g > 0.94f && color.b > 0.94f;
        }

        private static bool IsNearlyGray(Color color)
        {
            return Mathf.Abs(color.r - color.g) < 0.015f && Mathf.Abs(color.g - color.b) < 0.015f;
        }
    }
}
