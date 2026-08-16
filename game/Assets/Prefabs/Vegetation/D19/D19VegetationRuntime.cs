using System;
using UnityEngine;

namespace WhalesIslandReef.D19.Vegetation
{
    [Serializable]
    public sealed class D19VegetationPlacementData
    {
        public string biome_id;
        public string model_file;
        public string content_id;
        public string harvest_anchor_id;
        public Vector3 local_position;
        public Vector3 local_scale = Vector3.one;
        public float yaw;
    }

    [Serializable]
    public sealed class D19VegetationBiomeData
    {
        public string biome_id;
        public string habitat_id;
        public int seed;
        public float density_per_100m2;
        public string rule_summary;
        public D19VegetationPlacementData[] placements = new D19VegetationPlacementData[0];
    }

    [Serializable]
    public sealed class D19VegetationExclusionData
    {
        public string exclusion_id;
        public string owner;
        public string reason;
        public Vector3 center;
        public Vector3 half_extents;
    }

    [Serializable]
    public sealed class D19VegetationContentAnchorData
    {
        public string anchor_id;
        public string content_id;
        public string plant_id;
        public string owner_system;
        public bool visible_from_route;
    }

    [Serializable]
    public sealed class D19VegetationLayoutData
    {
        public string schema_version = "d19_vegetation_layout_v1";
        public string task = "D19-A03";
        public string layout_id = "d19_saint_margaret_layout_v1";
        public int global_seed = 1903;
        public string axis = "Y-up; +Z inland; -Z water/departure";
        public D19VegetationBiomeData[] biomes = new D19VegetationBiomeData[0];
        public D19VegetationExclusionData[] exclusions = new D19VegetationExclusionData[0];
        public D19VegetationContentAnchorData[] content_anchors = new D19VegetationContentAnchorData[0];
    }

    /// <summary>
    /// Presentation-only metadata for one deterministic biome root. It never owns
    /// plant quantity, refresh, ecology pressure, harvest outcome, inventory, or
    /// content registry data. EcologySystem/D11 remain the gameplay owners.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class D19VegetationBiomeRoot : MonoBehaviour
    {
        [SerializeField] private string layoutId = "d19_saint_margaret_layout_v1";
        [SerializeField] private string biomeId = string.Empty;
        [SerializeField] private string habitatId = string.Empty;
        [SerializeField] private int placementSeed = 1903;
        [SerializeField] private float densityPer100m2;
        [SerializeField] private string ruleSummary = string.Empty;
        [SerializeField] private bool presentationOnly = true;

        public string LayoutId { get { return layoutId; } }
        public string BiomeId { get { return biomeId; } }
        public string HabitatId { get { return habitatId; } }
        public int PlacementSeed { get { return placementSeed; } }
        public float DensityPer100m2 { get { return densityPer100m2; } }
        public bool PresentationOnly { get { return presentationOnly; } }

        public void Configure(string id, string habitat, int seed, float density, string summary)
        {
            biomeId = id ?? string.Empty;
            habitatId = habitat ?? string.Empty;
            placementSeed = seed;
            densityPer100m2 = Mathf.Max(0f, density);
            ruleSummary = summary ?? string.Empty;
        }
    }

    /// <summary>Stable visual/interaction anchor only; it does not perform harvest.</summary>
    [DisallowMultipleComponent]
    public sealed class D19VegetationContentAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId = string.Empty;
        [SerializeField] private string contentId = string.Empty;
        [SerializeField] private string plantId = string.Empty;
        [SerializeField] private string ownerSystem = "EcologySystem";
        [SerializeField] private bool visibleFromRoute;

        public string AnchorId { get { return anchorId; } }
        public string ContentId { get { return contentId; } }
        public string PlantId { get { return plantId; } }
        public string OwnerSystem { get { return ownerSystem; } }
        public bool VisibleFromRoute { get { return visibleFromRoute; } }

        public void Configure(string stableAnchorId, string stableContentId, string stablePlantId,
            bool routeVisible)
        {
            anchorId = stableAnchorId ?? string.Empty;
            contentId = stableContentId ?? string.Empty;
            plantId = stablePlantId ?? string.Empty;
            visibleFromRoute = routeVisible;
            ownerSystem = "EcologySystem";
        }
    }

    /// <summary>Static placement metadata for audit and scene readability.</summary>
    [DisallowMultipleComponent]
    public sealed class D19VegetationInstance : MonoBehaviour
    {
        [SerializeField] private string modelFile = string.Empty;
        [SerializeField] private string contentId = string.Empty;
        [SerializeField] private string biomeId = string.Empty;
        [SerializeField] private string collisionPolicy = "visual_only_trigger_anchor";
        [SerializeField] private bool collectionVisual;

        public string ModelFile { get { return modelFile; } }
        public string ContentId { get { return contentId; } }
        public string BiomeId { get { return biomeId; } }
        public string CollisionPolicy { get { return collisionPolicy; } }
        public bool CollectionVisual { get { return collectionVisual; } }

        public void Configure(string model, string stableContentId, string stableBiomeId, bool isCollectionVisual)
        {
            modelFile = model ?? string.Empty;
            contentId = stableContentId ?? string.Empty;
            biomeId = stableBiomeId ?? string.Empty;
            collectionVisual = isCollectionVisual;
        }
    }

    /// <summary>Read-only exclusion volume owned by the layout/placement task.</summary>
    [DisallowMultipleComponent]
    public sealed class D19VegetationExclusionVolume : MonoBehaviour
    {
        [SerializeField] private string exclusionId = string.Empty;
        [SerializeField] private string owner = string.Empty;
        [SerializeField] private string reason = string.Empty;
        [SerializeField] private Vector3 halfExtents = Vector3.one;

        public string ExclusionId { get { return exclusionId; } }
        public string Owner { get { return owner; } }
        public string Reason { get { return reason; } }
        public Vector3 HalfExtents { get { return halfExtents; } }

        public void Configure(string id, string exclusionOwner, string exclusionReason, Vector3 extents)
        {
            exclusionId = id ?? string.Empty;
            owner = exclusionOwner ?? string.Empty;
            reason = exclusionReason ?? string.Empty;
            halfExtents = new Vector3(Mathf.Max(0f, extents.x), Mathf.Max(0f, extents.y),
                Mathf.Max(0f, extents.z));
        }

        public bool ContainsWorldPoint(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            return Mathf.Abs(local.x) <= halfExtents.x && Mathf.Abs(local.y) <= halfExtents.y &&
                Mathf.Abs(local.z) <= halfExtents.z;
        }
    }

    /// <summary>
    /// Applies visual-only wind parameters through a MaterialPropertyBlock. No
    /// weather, time, ecology, quantity, or save state is written here.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class D19VegetationWindController : MonoBehaviour
    {
        private static readonly int WindDirectionId = Shader.PropertyToID("_WindDirection");
        private static readonly int WindSpeedId = Shader.PropertyToID("_WindSpeed");
        private static readonly int WindAmplitudeId = Shader.PropertyToID("_WindAmplitude");
        private static readonly int WindFrequencyId = Shader.PropertyToID("_WindFrequency");
        private static readonly int WindGustId = Shader.PropertyToID("_WindGust");
        private static readonly int WindPhaseId = Shader.PropertyToID("_WindPhase");

        [SerializeField] private Vector2 windDirection = new Vector2(0.8f, 0.6f);
        [SerializeField] private float windSpeed = 3.5f;
        [SerializeField] private float windAmplitude = 0.08f;
        [SerializeField] private float windFrequency = 0.75f;
        [SerializeField, Range(0f, 1f)] private float gust = 0.25f;
        [SerializeField] private float phase;
        [SerializeField] private Renderer[] ownedRenderers = new Renderer[0];
        private MaterialPropertyBlock propertyBlock;

        public Vector2 WindDirection { get { return windDirection; } }
        public float WindSpeed { get { return windSpeed; } }
        public float WindAmplitude { get { return windAmplitude; } }
        public float WindFrequency { get { return windFrequency; } }
        public float Gust { get { return gust; } }

        public void Configure(Vector2 direction, float speed, float amplitude, float frequency,
            float gustAmount, float deterministicPhase, Renderer[] renderers)
        {
            windDirection = direction.sqrMagnitude < 0.0001f ? Vector2.up : direction.normalized;
            windSpeed = Mathf.Max(0f, speed);
            windAmplitude = Mathf.Clamp(amplitude, 0f, 1f);
            windFrequency = Mathf.Clamp(frequency, 0.05f, 8f);
            gust = Mathf.Clamp01(gustAmount);
            phase = deterministicPhase;
            ownedRenderers = renderers ?? new Renderer[0];
            Apply();
        }

        private void OnEnable() { Apply(); }
        private void OnValidate() { Apply(); }
        private void Update() { Apply(); }

        public void Apply()
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            if (ownedRenderers == null || ownedRenderers.Length == 0)
                ownedRenderers = GetComponentsInChildren<Renderer>(true);
            Vector4 direction = new Vector4(windDirection.x, windDirection.y, 0f, 0f);
            for (int i = 0; i < ownedRenderers.Length; i++)
            {
                Renderer renderer = ownedRenderers[i];
                if (renderer == null) continue;
                propertyBlock.Clear();
                propertyBlock.SetVector(WindDirectionId, direction);
                propertyBlock.SetFloat(WindSpeedId, windSpeed);
                propertyBlock.SetFloat(WindAmplitudeId, windAmplitude);
                propertyBlock.SetFloat(WindFrequencyId, windFrequency);
                propertyBlock.SetFloat(WindGustId, gust);
                propertyBlock.SetFloat(WindPhaseId, phase);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
