using System.Collections;
using System.IO;
using UnityEngine;

namespace WhalesIslandReef.D20R02
{
    /// <summary>
    /// Isolated visual workbench probe. It only records runtime camera and
    /// screen facts for D20-A01-R02 evidence and has no gameplay ownership.
    /// </summary>
    public sealed class D20R02PlayerRuntimeProbe : MonoBehaviour
    {
        private void Start()
        {
            var camera = GetComponent<Camera>();
            var center = camera == null ? Vector3.zero : camera.WorldToScreenPoint(Vector3.zero);
            var pixelRect = camera == null ? new Rect() : camera.pixelRect;
            var visualRoot = GameObject.Find("PlayerDemo_Female_Humanoid");
            var renderers = visualRoot == null ? new Renderer[0] : visualRoot.GetComponentsInChildren<Renderer>(true);
            var bounds = renderers.Length == 0 ? new Bounds(Vector3.zero, Vector3.zero) : renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            var allCameras = Camera.allCameras;
            var cameraSummary = "";
            for (var i = 0; i < allCameras.Length; i++)
            {
                if (i > 0) cameraSummary += ";";
                cameraSummary += allCameras[i].name + "|enabled=" + allCameras[i].enabled +
                                 "|depth=" + allCameras[i].depth +
                                 "|rect=" + allCameras[i].rect;
            }
            var rendererSummary = "";
            for (var i = 0; i < renderers.Length; i++)
            {
                if (i > 0) rendererSummary += ";";
                var filter = renderers[i].GetComponent<MeshFilter>();
                var mesh = filter == null ? null : filter.sharedMesh;
                rendererSummary += renderers[i].name + "|enabled=" + renderers[i].enabled +
                                   "|active=" + renderers[i].gameObject.activeInHierarchy +
                                   "|layer=" + renderers[i].gameObject.layer +
                                   "|visible=" + renderers[i].isVisible +
                                   "|center=" + renderers[i].bounds.center +
                                   "|size=" + renderers[i].bounds.size +
                                   "|mesh=" + (mesh == null ? "null" : mesh.name + "/" + mesh.vertexCount + "/" + mesh.subMeshCount + "/" + mesh.GetIndexCount(0)) +
                                   "|material=" + (renderers[i].sharedMaterial == null ? "null" : renderers[i].sharedMaterial.name + "/" + renderers[i].sharedMaterial.shader.name);
            }
            Debug.Log(
                "[D20-A01-R02] runtime_probe screen=" + Screen.width + "x" + Screen.height +
                " resolution=" + Screen.currentResolution.width + "x" + Screen.currentResolution.height +
                " camera_pixel_rect=" + pixelRect +
                " camera_position=" + transform.position +
                " camera_forward=" + transform.forward +
                " world_origin_screen=" + center +
                " runtime_visual_bounds_center=" + bounds.center +
                " runtime_visual_bounds_size=" + bounds.size +
                " all_cameras=" + cameraSummary +
                " renderers=" + rendererSummary);
            StartCoroutine(CapturePlayerFrame());
        }

        private IEnumerator CapturePlayerFrame()
        {
            yield return new WaitForEndOfFrame();
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "D20_A01_R02_PLAYER_RUNTIME_CAPTURE_RERUN7.png"));
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[D20-A01-R02] runtime_probe screenshot=" + path);
        }
    }
}
