using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Whalebone.Tests.D2
{
    /// <summary>
    /// VC-A12 的 EditMode 边界测试：动作层只能驱动视觉子树，且骨骼启发式匹配
    /// 必须在无法证明唯一性时安全降级。
    /// </summary>
    public sealed class D2CharacterAnimationControllerTests
    {
        [Test]
        public void LateUpdateKeepsPhysicalRootStableAndLocksVisualRootBaseline()
        {
            GameObject root = CreateRig();
            try
            {
                var animation = AddAnimation(root);
                Vector3 physicalPosition = new Vector3(3f, 1.25f, -4f);
                Quaternion physicalRotation = Quaternion.Euler(0f, 37f, 0f);
                root.transform.SetPositionAndRotation(physicalPosition, physicalRotation);

                var visualRoot = root.transform.Find("HumanoidVisual");
                visualRoot.localPosition = new Vector3(0f, 0.25f, 0f);
                visualRoot.localRotation = Quaternion.Euler(0f, 8f, 0f);

                InvokePrivate(animation, "LateUpdate");

                Assert.That(root.transform.position, Is.EqualTo(physicalPosition));
                Assert.That(Quaternion.Angle(root.transform.rotation, physicalRotation), Is.LessThan(0.001f));
                Assert.That(visualRoot.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(Quaternion.Angle(visualRoot.localRotation, Quaternion.identity), Is.LessThan(0.001f));
                Assert.That(ReadBool(animation, "AnimatorPoseLayerActive") &&
                    ReadBool(animation, "ProceduralPoseLayerActive"), Is.False,
                    "Animator 与程序化姿态层不能同时接管。");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void AmbiguousBoneAliasesRefuseBindingAndUseNeutralFallback()
        {
            GameObject root = CreateRig(useImportedAliases: true, duplicateLeftUpperArm: true);
            try
            {
                var animation = AddAnimation(root);

                Assert.That(ReadBool(animation, "BoneBindingConflict"), Is.True);
                Assert.That(ReadBool(animation, "BoundBonesAreUnique"), Is.True,
                    "拒绝冲突绑定后，剩余有效绑定仍不能重复引用同一个 Transform。");
                Assert.That(ReadBool(animation, "ProceduralPoseLayerReady"), Is.False);
                Assert.That(ReadBool(animation, "ProceduralPoseLayerActive"), Is.False);
                Assert.That(ReadBool(animation, "AnimatorPoseLayerActive"), Is.False);
                Assert.That(ReadString(animation, "PoseDriverModeName"), Is.EqualTo("Neutral"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ImportedArmAliasesPreferArmAndNeverShoulderOrForearm()
        {
            GameObject root = CreateRig(useImportedAliases: true);
            try
            {
                var animation = AddAnimation(root);
                Transform leftUpperArm = ReadBoundTransform(animation, "_leftUpperArm");
                Transform rightUpperArm = ReadBoundTransform(animation, "_rightUpperArm");

                Assert.That(leftUpperArm, Is.Not.Null);
                Assert.That(rightUpperArm, Is.Not.Null);
                Assert.That(leftUpperArm.name, Is.EqualTo("LeftArm"));
                Assert.That(rightUpperArm.name, Is.EqualTo("RightArm"));
                Assert.That(ReadBool(animation, "BoundBonesAreUnique"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RendererNodesWithBoneLikeNamesDoNotCreateBindingConflicts()
        {
            GameObject root = CreateRig();
            try
            {
                var spine = root.transform.Find("HumanoidVisual/Hips/Spine");
                var decoy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                decoy.name = "LeftUpperArmMesh";
                decoy.transform.SetParent(spine, false);

                var animation = AddAnimation(root);

                Assert.That(ReadInt(animation, "BoundBoneCount"), Is.EqualTo(8));
                Assert.That(ReadBool(animation, "BoneBindingConflict"), Is.False);
                Assert.That(ReadBool(animation, "BoundBonesAreUnique"), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ImportedSkinnedRigStaysNeutralUntilAnimationContractIsVerified()
        {
            GameObject root = CreateRig();
            try
            {
                root.transform.Find("HumanoidVisual").gameObject.AddComponent<SkinnedMeshRenderer>();
                var animation = AddAnimation(root);

                Assert.That(ReadBool(animation, "ImportedSkinnedRigNeutralOnly"), Is.True);
                Assert.That(ReadBool(animation, "ProceduralPoseLayerReady"), Is.False,
                    "没有经过动作轴/Avatar 验证的导入蒙皮 rig 不得使用猜测式程序化骨骼旋转。");
                Assert.That(ReadBool(animation, "ProceduralPoseLayerActive"), Is.False);
                Assert.That(ReadString(animation, "PoseDriverModeName"), Is.EqualTo("Neutral"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void AnimatorConfigurationDisablesRootMotionWhenAnimatorIsAvailable()
        {
            GameObject root = CreateRig();
            try
            {
                var animation = AddAnimation(root);
                var animator = root.GetComponent<Animator>();

                if (animator == null) return;

                Assert.That(animator.applyRootMotion, Is.False);
                Assert.That(animator.updateMode, Is.EqualTo(AnimatorUpdateMode.Normal));
                Assert.That(ReadBool(animation, "AnimatorPoseLayerActive") &&
                    ReadBool(animation, "ProceduralPoseLayerActive"), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void GeneratedPlaceholderAnimatorDoesNotDriveRawEulerPose()
        {
            GameObject root = CreateRig(useImportedAliases: true);
            try
            {
                var animation = AddAnimation(root);
                var animator = root.GetComponent<Animator>();
                bool generatedPlaceholderAttached = animator != null &&
                    animator.runtimeAnimatorController != null &&
                    animator.runtimeAnimatorController.name == "D6CrewAnimator";

                if (generatedPlaceholderAttached)
                {
                    Assert.That(ReadBool(animation, "AnimatorPoseLayerActive"), Is.False);
                    Assert.That(ReadBool(animation, "ProceduralPoseLayerActive"), Is.True);
                    Assert.That(ReadString(animation, "PoseDriverModeName"), Is.EqualTo("Procedural"));
                }
                else
                {
                    Assert.That(ReadBool(animation, "AnimatorPoseLayerActive") &&
                        ReadBool(animation, "ProceduralPoseLayerActive"), Is.False);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TenDisableEnableAndTransitionCyclesRestoreTheImportedBaseline()
        {
            GameObject root = CreateRig(useImportedAliases: true);
            try
            {
                var visualRoot = root.transform.Find("HumanoidVisual");
                visualRoot.localPosition = new Vector3(0f, 0.21f, 0f);
                visualRoot.localRotation = Quaternion.Euler(0f, 7f, 0f);
                visualRoot.gameObject.AddComponent<SkinnedMeshRenderer>();

                var animation = AddAnimation(root);
                var baselinePosition = visualRoot.localPosition;
                var baselineRotation = visualRoot.localRotation;

                for (int i = 0; i < 10; i++)
                {
                    InvokeTransition(animation, "Boarding");
                    SetPrivateFloat(animation, "_transitionRemaining", 0.25f);
                    InvokePrivate(animation, "LateUpdate");

                    InvokePrivate(animation, "OnDisable");
                    Assert.That(visualRoot.localPosition, Is.EqualTo(baselinePosition));
                    Assert.That(Quaternion.Angle(visualRoot.localRotation, baselineRotation), Is.LessThan(0.001f));

                    InvokePrivate(animation, "OnEnable");
                    Assert.That(visualRoot.localPosition, Is.EqualTo(baselinePosition));
                    Assert.That(Quaternion.Angle(visualRoot.localRotation, baselineRotation), Is.LessThan(0.001f));

                    InvokeTransition(animation, "Disembarking");
                    SetPrivateFloat(animation, "_transitionRemaining", 0.25f);
                    InvokePrivate(animation, "LateUpdate");
                    InvokePrivate(animation, "OnDisable");
                    InvokePrivate(animation, "OnEnable");
                    Assert.That(visualRoot.localPosition, Is.EqualTo(baselinePosition));
                    Assert.That(Quaternion.Angle(visualRoot.localRotation, baselineRotation), Is.LessThan(0.001f));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateRig(bool useImportedAliases = false,
            bool duplicateLeftUpperArm = false)
        {
            var root = new GameObject("VC_A12_Rig");
            var visual = CreateChild("HumanoidVisual", root.transform);
            var hips = CreateChild("Hips", visual.transform);
            var spine = CreateChild("Spine", hips.transform);

            if (useImportedAliases)
            {
                CreateChild("LeftShoulder", spine.transform);
                CreateChild("LeftArm", spine.transform);
                CreateChild("LeftForeArm", spine.transform);
                CreateChild("LeftHand", spine.transform);
                CreateChild("RightShoulder", spine.transform);
                CreateChild("RightArm", spine.transform);
                CreateChild("RightForeArm", spine.transform);
                CreateChild("RightHand", spine.transform);
                if (duplicateLeftUpperArm)
                    CreateChild("LeftArm", spine.transform);
            }
            else
            {
                CreateChild("LeftUpperArm", spine.transform);
                CreateChild("RightUpperArm", spine.transform);
            }

            CreateChild("LeftUpLeg", hips.transform);
            CreateChild("LeftLeg", hips.transform);
            CreateChild("LeftFoot", hips.transform);
            CreateChild("RightUpLeg", hips.transform);
            CreateChild("RightLeg", hips.transform);
            CreateChild("RightFoot", hips.transform);
            return root;
        }

        private static GameObject CreateChild(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static Component AddAnimation(GameObject root)
        {
            var type = FindLoadedType("Whalebone.Gameplay.D2.D2CharacterAnimationController");
            Assert.That(type, Is.Not.Null, "D2CharacterAnimationController 未加载。");
            var animation = root.AddComponent(type);

            // Unity 在 EditMode 中可能把 AddComponent 后的 Awake 延迟到下一次编辑器
            // 更新；测试要先完成与运行时一致的层级发现，才能验证绑定边界。
            var visualRootField = type.GetField("_visualRoot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (visualRootField.GetValue(animation) == null)
                InvokePrivate(animation, "Awake");
            return animation;
        }

        private static Transform ReadBoundTransform(Component animation,
            string fieldName)
        {
            var field = animation.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "缺少测试所需的内部骨骼绑定字段：" + fieldName);
            var bonePose = field.GetValue(animation);
            if (bonePose == null) return null;

            var transformField = bonePose.GetType().GetField("Transform",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(transformField, Is.Not.Null, "BonePose 未保留 Transform 引用。");
            return transformField.GetValue(bonePose) as Transform;
        }

        private static bool ReadBool(Component animation, string propertyName)
        {
            var property = animation.GetType().GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "缺少测试所需的公开观测属性：" + propertyName);
            return (bool)property.GetValue(animation);
        }

        private static int ReadInt(Component animation, string propertyName)
        {
            var property = animation.GetType().GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "缺少测试所需的公开观测属性：" + propertyName);
            return (int)property.GetValue(animation);
        }

        private static string ReadString(Component animation, string propertyName)
        {
            var property = animation.GetType().GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "缺少测试所需的公开观测属性：" + propertyName);
            return (string)property.GetValue(animation);
        }

        private static void InvokePrivate(Component target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "缺少测试所需的私有生命周期方法：" + methodName);
            method.Invoke(target, null);
        }

        private static void SetPrivateFloat(Component target, string fieldName, float value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "缺少测试所需的私有浮点字段：" + fieldName);
            field.SetValue(target, value);
        }

        private static void InvokeTransition(Component target, string actionName)
        {
            var method = target.GetType().GetMethod("PlayTransition",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, "缺少测试所需的公开过渡方法：PlayTransition");
            var actionType = method.GetParameters()[0].ParameterType;
            method.Invoke(target, new object[] { System.Enum.Parse(actionType, actionName) });
        }

        private static Type FindLoadedType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
