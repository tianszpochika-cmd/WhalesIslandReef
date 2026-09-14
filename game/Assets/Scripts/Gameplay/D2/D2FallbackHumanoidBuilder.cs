// ============================================================
// D2FallbackHumanoidBuilder —— 运行时低模人物降级外观
//
// 这不是最终美术资源。它只在正式船员模型不可用时提供一个可识别、可碰撞、
// 可被 D2CharacterAnimationController 驱动的基础人物，避免玩家再次看到蓝色柱体。
// ============================================================

using UnityEngine;

namespace Whalebone.Gameplay.D2
{
    /// <summary>
    /// 创建带有明确骨骼节点命名的低模人物。所有子物体碰撞体都会被关闭，
    /// 角色碰撞统一由根节点的 CharacterController 负责。
    /// </summary>
    public static class D2FallbackHumanoidBuilder
    {
        public static GameObject Create(string name, Vector3 position, Color clothingColor)
        {
            var root = new GameObject(name);
            root.transform.position = position;

            var visualRoot = new GameObject("HumanoidVisual");
            visualRoot.transform.SetParent(root.transform, false);

            var skin = CreateMaterial(new Color(0.72f, 0.46f, 0.27f));
            var clothing = CreateMaterial(clothingColor);
            var trousers = CreateMaterial(new Color(0.12f, 0.16f, 0.22f));
            var boots = CreateMaterial(new Color(0.18f, 0.10f, 0.055f));
            var hair = CreateMaterial(new Color(0.075f, 0.045f, 0.025f));
            var accent = CreateMaterial(new Color(0.82f, 0.62f, 0.20f));

            var hips = CreateBone("Hips", visualRoot.transform, new Vector3(0f, 1.16f, 0f));
            var spine = CreateBone("Spine", hips.transform, new Vector3(0f, 0.40f, 0f));
            // Keep the shoulders broad enough for the arm roots to meet the
            // torso in the third-person Player.  The previous 0.39m arm
            // roots sat outside a 0.36m torso and made the fallback read as
            // detached blue pieces as soon as a pose rotated the visual root.
            CreateMesh(PrimitiveType.Capsule, "TorsoMesh", spine.transform,
                new Vector3(0f, 0.17f, 0f), new Vector3(0.42f, 0.46f, 0.28f), clothing);
            CreateMesh(PrimitiveType.Cube, "BeltMesh", hips.transform,
                new Vector3(0f, 0.06f, 0f), new Vector3(0.48f, 0.10f, 0.30f), accent);

            var neck = CreateBone("Neck", spine.transform, new Vector3(0f, 0.53f, 0f));
            CreateMesh(PrimitiveType.Cylinder, "NeckMesh", neck.transform,
                new Vector3(0f, 0.06f, 0f), new Vector3(0.13f, 0.12f, 0.13f), skin);
            var head = CreateBone("Head", neck.transform, new Vector3(0f, 0.15f, 0f));
            CreateMesh(PrimitiveType.Sphere, "HeadMesh", head.transform,
                new Vector3(0f, 0.18f, 0f), new Vector3(0.36f, 0.42f, 0.36f), skin);
            CreateMesh(PrimitiveType.Sphere, "HairMesh", head.transform,
                new Vector3(0f, 0.29f, -0.015f), new Vector3(0.37f, 0.17f, 0.37f), hair);

            BuildLeg(hips.transform, "Left", -0.17f, trousers, boots);
            BuildLeg(hips.transform, "Right", 0.17f, trousers, boots);
            BuildArm(spine.transform, "Left", -0.29f, clothing, skin);
            BuildArm(spine.transform, "Right", 0.29f, clothing, skin);

            // The root CharacterController is the only collider that should remain active.
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;

            return root;
        }

        private static void BuildLeg(Transform parent, string side, float x,
            Material trousers, Material boots)
        {
            var upper = CreateBone(side + "UpperLeg", parent, new Vector3(x, -0.04f, 0f));
            CreateMesh(PrimitiveType.Capsule, side + "UpperLegMesh", upper.transform,
                new Vector3(0f, -0.25f, 0f), new Vector3(0.14f, 0.28f, 0.14f), trousers);

            var lower = CreateBone(side + "LowerLeg", upper.transform, new Vector3(0f, -0.54f, 0f));
            CreateMesh(PrimitiveType.Capsule, side + "LowerLegMesh", lower.transform,
                new Vector3(0f, -0.27f, 0f), new Vector3(0.12f, 0.30f, 0.12f), trousers);

            var foot = CreateBone(side + "Foot", lower.transform, new Vector3(0f, -0.57f, 0f));
            CreateMesh(PrimitiveType.Cube, side + "FootMesh", foot.transform,
                new Vector3(0f, -0.04f, 0.11f), new Vector3(0.17f, 0.10f, 0.32f), boots);
        }

        private static void BuildArm(Transform parent, string side, float x,
            Material clothing, Material skin)
        {
            var upper = CreateBone(side + "UpperArm", parent, new Vector3(x, 0.27f, 0f));
            CreateMesh(PrimitiveType.Capsule, side + "UpperArmMesh", upper.transform,
                new Vector3(0f, -0.20f, 0f), new Vector3(0.15f, 0.23f, 0.15f), clothing);

            var lower = CreateBone(side + "LowerArm", upper.transform, new Vector3(0f, -0.42f, 0f));
            CreateMesh(PrimitiveType.Capsule, side + "LowerArmMesh", lower.transform,
                new Vector3(0f, -0.20f, 0f), new Vector3(0.115f, 0.22f, 0.115f), skin);
            CreateMesh(PrimitiveType.Sphere, side + "HandMesh", lower.transform,
                new Vector3(0f, -0.41f, 0f), new Vector3(0.15f, 0.15f, 0.15f), skin);
        }

        private static GameObject CreateBone(string name, Transform parent, Vector3 localPosition)
        {
            var bone = new GameObject(name);
            bone.transform.SetParent(parent, false);
            bone.transform.localPosition = localPosition;
            return bone;
        }

        private static GameObject CreateMesh(PrimitiveType primitiveType, string name,
            Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var mesh = GameObject.CreatePrimitive(primitiveType);
            mesh.name = name;
            mesh.transform.SetParent(parent, false);
            mesh.transform.localPosition = localPosition;
            mesh.transform.localScale = localScale;
            var renderer = mesh.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            var collider = mesh.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            return mesh;
        }

        private static Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return null;
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }
    }
}
