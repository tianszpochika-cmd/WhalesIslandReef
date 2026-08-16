using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WhalesIslandReef.Presentation.D20.NpcPresentation;

namespace WhalesIslandReef.Tests.D20A04
{
    public static class D20A04PresentationBuilder
    {
        private const string SourcePrefabPath = "Assets/Prefabs/Characters/D20/NPC/npc_d20_dock_clerk_01.prefab";
        private const string PrefabDirectory = "Assets/Prefabs/Characters/D20/NPCPresentation";
        private const string PrefabPath = PrefabDirectory + "/D20_NPC_Presentation_Demo.prefab";
        private const string AnimationDirectory = "Assets/Animations/D20/NPC";
        private const string ControllerPath = AnimationDirectory + "/D20_NPC_Presentation_Demo.controller";
        private const string ReportPath = "tools/results/D20/D20-A04/D20_A04_PRESENTATION_BUILD_REPORT.json";

        [MenuItem("WhalesIslandReef/D20-A04/Build NPC Presentation Demo")]
        public static void BuildDemoPrefab()
        {
            EnsureFolder("Assets/Prefabs/Characters/D20/NPCPresentation");
            EnsureFolder("Assets/Animations/D20/NPC");

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
            if (source == null)
                throw new InvalidOperationException("D20-A03 source Prefab is missing: " + SourcePrefabPath);

            var controller = BuildController();
            var root = new GameObject("D20_NPC_Presentation_Demo");
            var sourceInstance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (sourceInstance == null)
                throw new InvalidOperationException("Could not instantiate read-only D20-A03 source Prefab.");
            sourceInstance.name = "A03_SourceNpcVisual_ReadOnlyInstance";
            sourceInstance.transform.SetParent(root.transform, false);

            var animator = sourceInstance.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }

            var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.name = "FarProxy";
            fallback.transform.SetParent(root.transform, false);
            fallback.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            fallback.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);
            var fallbackCollider = fallback.GetComponent<Collider>();
            if (fallbackCollider != null) UnityEngine.Object.DestroyImmediate(fallbackCollider);

            var indicatorObject = new GameObject("PresentationFeedback");
            indicatorObject.transform.SetParent(root.transform, false);
            indicatorObject.transform.localPosition = new Vector3(0f, 2.25f, 0f);
            var text = indicatorObject.AddComponent<TextMesh>();
            text.fontSize = 32;
            text.characterSize = 0.055f;
            text.anchor = TextAnchor.LowerCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            text.text = string.Empty;
            var billboard = indicatorObject.AddComponent<D20NpcFeedbackBillboard>();
            billboard.ConfigureForEditor(text);

            var anchorsRoot = new GameObject("D20_NpcVisualAnchors");
            anchorsRoot.transform.SetParent(root.transform, false);
            AddAnchor(anchorsRoot.transform, "MarketService", D20NpcJobPost.Market, D20NpcPresentationAnchorIds.Market, new Vector3(0f, 0f, 0.45f));
            AddAnchor(anchorsRoot.transform, "DockWork", D20NpcJobPost.Dock, D20NpcPresentationAnchorIds.Dock, new Vector3(0.45f, 0f, 0f));
            AddAnchor(anchorsRoot.transform, "ShipyardWork", D20NpcJobPost.Shipyard, D20NpcPresentationAnchorIds.Shipyard, new Vector3(-0.45f, 0f, 0f));
            AddAnchor(anchorsRoot.transform, "TavernService", D20NpcJobPost.Tavern, D20NpcPresentationAnchorIds.Tavern, new Vector3(0f, 0f, -0.45f));

            var adapter = root.AddComponent<D20NpcReadOnlyPresentationAdapter>();
            adapter.ConfigureForEditor(
                "npc_d15_a07_instance_0006",
                "npc_d20_dock_clerk_01",
                D20NpcJobPost.Dock,
                fallback,
                billboard);
            adapter.CachePresentationParts();

            bool saved;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out saved);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!saved || AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
                throw new InvalidOperationException("NPC presentation demo Prefab was not saved: " + PrefabPath);

            WriteReport(controller);
            Debug.Log("[D20-A04] NPC presentation demo built: " + PrefabPath);
        }

        private static AnimatorController BuildController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            AddParameter(controller, "D20_ActionId", AnimatorControllerParameterType.Int);
            AddParameter(controller, "D20_Speed01", AnimatorControllerParameterType.Float);
            AddParameter(controller, "D20_Grounded", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "D20_Injured", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "D20_Paused", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "D20_LodBand", AnimatorControllerParameterType.Int);
            AddParameter(controller, "D20_InteractionKind", AnimatorControllerParameterType.Int);

            var stateMachine = controller.layers[0].stateMachine;
            string[] stateNames =
            {
                "D20_NPC_Idle", "D20_NPC_Work", "D20_NPC_Walk", "D20_NPC_Talk",
                "D20_NPC_Rest", "D20_NPC_Eat", "D20_NPC_Alarm", "D20_NPC_Refuse",
                "D20_NPC_Injured"
            };
            var existing = new HashSet<string>(stateMachine.states.Select(s => s.state.name));
            foreach (var stateName in stateNames)
                if (!existing.Contains(stateName)) stateMachine.AddState(stateName);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            if (!controller.parameters.Any(p => p.name == name))
                controller.AddParameter(name, type);
        }

        private static void AddAnchor(Transform parent, string name, D20NpcJobPost jobPost, string anchorId, Vector3 localPosition)
        {
            var anchorObject = new GameObject(name);
            anchorObject.transform.SetParent(parent, false);
            anchorObject.transform.localPosition = localPosition;
            var anchor = anchorObject.AddComponent<D20NpcPresentationAnchor>();
            anchor.ConfigureForEditor(anchorId, jobPost, "npc_interaction_" + jobPost.ToString().ToLowerInvariant());
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static void WriteReport(AnimatorController controller)
        {
            string absolutePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ReportPath);
            var report = "{\n" +
                "  \"task_id\": \"D20-A04\",\n" +
                "  \"status\": \"Candidate\",\n" +
                "  \"source_prefab_read_only\": \"" + SourcePrefabPath + "\",\n" +
                "  \"demo_prefab\": \"" + PrefabPath + "\",\n" +
                "  \"animator_controller\": \"" + ControllerPath + "\",\n" +
                "  \"animator_parameter_count\": " + controller.parameters.Length + ",\n" +
                "  \"visual_anchor_count\": 4,\n" +
                "  \"logic_write_components\": 0,\n" +
                "  \"fallbacks\": [\"neutral_pose\", \"feedback_billboard\", \"FarProxy\", \"snapshot_resync_after_enable\"],\n" +
                "  \"scene_or_system_changes\": false\n" +
                "}\n";
            File.WriteAllText(absolutePath, report);
        }
    }
}
