using UnityEngine;

namespace Whalebone.Gameplay.D19.R03
{
    /// <summary>
    /// Keeps a presentation-only landmark label visible only from the facade
    /// on which it is mounted. TextMesh is effectively double-sided in the
    /// legacy runtime material, so without this small camera-facing gate a
    /// label can bleed through a building and appear mirrored from behind.
    /// This component owns no gameplay state, input, navigation or business
    /// data.
    /// </summary>
    public sealed class D19R03LandmarkLabelVisibility : MonoBehaviour
    {
        [SerializeField] private Vector3 outwardNormal = Vector3.back;
        [SerializeField] private float minimumFacingDot = 0.08f;

        private Camera _camera;
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void LateUpdate()
        {
            if (_renderer == null) return;
            if (_camera == null || !_camera.isActiveAndEnabled)
                _camera = Camera.main;
            if (_camera == null) return;

            var normal = transform.TransformDirection(outwardNormal).normalized;
            var toCamera = (_camera.transform.position - transform.position).normalized;
            _renderer.enabled = Vector3.Dot(normal, toCamera) >= minimumFacingDot;
        }
    }
}
