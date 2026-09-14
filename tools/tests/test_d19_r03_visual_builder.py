import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
BUILDER = ROOT / "game/Assets/Editor/D19R03/D19R03HarborPresentationBuilder.cs"
PLAYER_BUILDER = ROOT / "game/Assets/Editor/D19R03/D19R03PlayerBuilder.cs"
ROUTE_PROBE = ROOT / "game/Assets/Scripts/Gameplay/D19/R03/D19R03PlayerRouteProbe.cs"
COLLISION_CONTROLLER = ROOT / "game/Assets/Scripts/Gameplay/Harbor/HarborCollisionController.cs"
R03_PREFAB = ROOT / "game/Assets/Prefabs/Environment/D19/Integration/R03/D19_R03_HarborPresentation.prefab"


class D19R03VisualBuilderTests(unittest.TestCase):
    def setUp(self):
        self.source = BUILDER.read_text(encoding="utf-8")
        self.player_builder = PLAYER_BUILDER.read_text(encoding="utf-8")
        self.route_probe = ROUTE_PROBE.read_text(encoding="utf-8")

    def test_bevel_helper_generates_faces_edges_and_corners(self):
        self.assertIn("private static Mesh MakeBevelBoxMesh", self.source)
        self.assertIn("AddQuad(vertices, triangles", self.source)
        self.assertIn("AddTriangle(vertices, triangles", self.source)
        self.assertIn("Vector3.Dot(actualNormal, expectedNormal)", self.source)
        self.assertIn("float safeBevel = Mathf.Clamp", self.source)

    def test_bevel_helper_does_not_silently_return_a_plain_cube(self):
        start = self.source.index("private static Mesh MakeBevelBoxMesh")
        end = self.source.index("private static void AddQuad", start)
        method = self.source[start:end]
        self.assertEqual(1, method.count("return MakeBoxMesh(width, height, depth);"))
        self.assertIn("return MakeMesh(vertices.ToArray(), triangles.ToArray());", method)

    def test_real_candidate_detail_paths_remain_in_the_visual_builder(self):
        for token in (
            "tree_default.fbx",
            "tree_small_dark.fbx",
            "cliff_large_rock.fbx",
            "ground_pathBend.fbx",
            "tower-complete-small.fbx",
            "structure-platform-dock-small.fbx",
            "boat-row-small.fbx",
            "cannon.fbx",
            "crate-bottles.fbx",
            "flag-pirate.fbx",
        ):
            self.assertIn(token, self.source)

    def test_detached_tree_candidates_are_audited_but_not_instantiated_in_r03(self):
        start = self.source.index("private static void AddCandidateDetails")
        end = self.source.index("private static GameObject BuildShipPrefab", start)
        visible_details = self.source[start:end]
        self.assertNotIn('AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_default.fbx"', visible_details)
        self.assertNotIn('AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_small_dark.fbx"', visible_details)
        self.assertNotIn('AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/grass_large.fbx"', visible_details)
        self.assertIn('AddCandidate(parent, "Assets/Art/Candidates/Environment/KenneyNatureKit/Models/tree_palm.fbx"', visible_details)
        self.assertIn("Candidate/ImportVerified", visible_details)
        self.assertIn("catalogued in the audit, but do not instantiate it", visible_details)

    def test_pirate_port_candidates_preserve_source_materials_and_have_no_collision_authority(self):
        self.assertIn("AddPiratePortCandidateDetails", self.source)
        self.assertIn("AddCandidatePreserveMaterials", self.source)
        self.assertIn("renderer.sharedMaterials = materials", self.source)
        self.assertIn("DestroyImmediate(collider)", self.source)
        self.assertIn("D31_S05_PIRATE_PORT_CANDIDATE_AUDIT_R66", self.source)

    def test_r03_binds_approved_pbr_maps_and_generates_uvs(self):
        for token in (
            "aerial_grass_rock_albedo.jpg",
            "brown_planks_03_albedo.jpg",
            "sand_01_diff_1k.jpg",
            "LoadNormalMap",
            "ApplyTriplanarPresentationUv",
            "mesh.uv = islandUv",
        ):
            self.assertIn(token, self.source)

    def test_island_uses_an_organic_footprint_and_surrounding_water(self):
        for token in (
            "private static Vector2 IslandPoint",
            "IslandRadiusX",
            "IslandRadiusZ",
            "AddIslandSkirt(vertices, sub, materials.Length)",
            "IslandSkirtSegments",
            'MakeWaterSurfaceMesh(96f, 96f, 20, 0.055f), water',
            "private static Mesh MakeWaterSurfaceMesh",
            "AddCoastalFoam(root.transform, foam)",
            "FoamRing_",
        ):
            self.assertIn(token, self.source)
        self.assertNotIn("float px = (x - (width - 1) * 0.5f) *", self.source)

    def test_shore_and_routes_follow_authored_terrain_instead_of_floor_cards(self):
        self.assertIn("float edgeRadius = Mathf.Sqrt", self.source)
        self.assertIn('AddMesh(root.transform, "R03_Beach"', self.source)
        self.assertIn('MakePathMesh(new[]', self.source)
        self.assertIn("private static Mesh MakePathMesh", self.source)
        self.assertNotIn("if (z < -11.5f)", self.source)
        self.assertNotIn("MakeRibbonMesh(34f, 0.12f, 4.6f)", self.source)

    def test_foam_stays_at_waterline_and_is_short(self):
        start = self.source.index("private static void AddCoastalFoam")
        end = self.source.index("private static GameObject CreateRoadNetwork", start)
        method = self.source[start:end]
        self.assertIn("const int segments = 28", method)
        self.assertIn("new Vector3(point.x + outward.x, 0.018f, point.y + outward.y)", method)
        self.assertIn("MakeBevelBoxMesh(length, 0.018f, 0.055f, 0.012f)", method)
        self.assertNotIn("Height(point.x, point.y) + 0.018f", method)

    def test_r64_route_and_port_detail_geometry_remains_in_the_builder(self):
        self.assertIn('AddMesh(root.transform, "MarketRoad", MakeRibbonMesh(12f, 0.08f, 2.8f)', self.source)
        self.assertIn('for (int i = 0; i < 14; i++)', self.source)
        self.assertIn('for (int i = 0; i < 7; i++)', self.source)
        self.assertIn('for (int i = 0; i < 3; i++)', self.source)

    def test_house_landmarks_do_not_generate_rear_duplicate_labels(self):
        self.assertIn('AddText(root.transform, "Sign"', self.source)
        self.assertNotIn('"BackSignBoard"', self.source)
        self.assertNotIn('AddText(root.transform, "BackSign"', self.source)

    def test_palms_are_kept_out_of_the_market_camera_near_framing(self):
        start = self.source.index("private static GameObject CreateVegetation")
        end = self.source.index("private static GameObject CreatePortProps", start)
        method = self.source[start:end]
        self.assertIn("var positions = new[]", method)
        self.assertEqual(6, method[method.index("var positions"):method.index("for (int i = 0; i < positions.Length", method.index("var positions"))].count("new Vector3"))
        self.assertIn("new Vector3(-10.5f, 0.2f, -6.5f)", method)
        self.assertIn("new Vector3(-5.5f, 0.2f, 13.0f)", method)
        self.assertNotIn("new Vector3(12f, 0.2f, 8f)", method)
        self.assertNotIn("new Vector3(10f, 0.2f, -2f)", method)
        self.assertIn("MakeCylinderMesh(0.14f, 4.1f, 8)", self.source)
        self.assertIn('AddMesh(root.transform, "Canopy", MakeCanopyMesh(0.88f, 1.25f)', self.source)
        self.assertIn("private static Mesh MakeCanopyMesh", self.source)

    def test_ship_visual_uses_berth_offset_without_moving_gameplay_anchor(self):
        self.assertIn("ShipVisualBerthOffset", self.source)
        self.assertIn("new Vector3(0f, 0f, -3.6f)", self.source)
        self.assertIn("instance.transform.localPosition = ship.transform.InverseTransformVector(ShipVisualBerthOffset)", self.source)
        self.assertIn("var ship = FindSceneObject(scene, \"ph_ship_fishing\")", self.source)

    def test_ship_visual_requires_a_visible_high_contrast_helm_for_candidate_assets(self):
        self.assertIn('Material("R03_HelmWheel", new Color(0.34f, 0.16f, 0.055f, 1f)', self.source)
        self.assertIn("hasVisibleSteeringNode", self.source)
        self.assertIn('lower.Contains("wheel")', self.source)
        self.assertIn('AddMesh(parent, "HelmWheel", MakeHelmWheelMesh(0.38f, 0.10f, 12, 4)', self.source)
        self.assertIn('AddMesh(parent, "HelmWheelHub", MakeCappedCylinderMesh(0.09f, 0.14f, 10)', self.source)
        self.assertIn("private static Mesh MakeCappedCylinderMesh", self.source)
        self.assertIn("private static Mesh MakeHelmWheelMesh", self.source)
        self.assertIn("new Vector3(0f, 1.72f, 2.45f)", self.source)
        self.assertIn("D7_VisibleHelmAnchor", self.source)

    def test_plaster_material_clears_terrain_maps(self):
        start = self.source.index('string.Equals(name, "R03_Plaster"')
        end = self.source.index("string baseMapPath", start)
        block = self.source[start:end]
        self.assertIn('SetTexture("_BaseMap", null)', block)
        self.assertIn('SetTexture("_BumpMap", null)', block)

    def test_ground_materials_follow_the_approved_texture_binding_path(self):
        self.assertNotIn("IsAuthoredGroundMaterial", self.source)
        self.assertNotIn("ClearPresentationMaps", self.source)
        for token in (
            'case "R03_Grass":',
            'case "R03_Earth":',
            'case "R03_Sand":',
            'case "R03_SandLight":',
            'material.SetColor("_BaseColor", Color.white)',
        ):
            self.assertIn(token, self.source)

    def test_r66_presentation_geometry_is_reinstated_without_a_second_collision_authority(self):
        self.assertIn('new GameObject("D19_R03_KenneyPiratePortDetails")', self.source)
        self.assertEqual(6, self.source.count("AddCandidatePreserveMaterials(details.transform"))
        self.assertIn("for (int i = 0; i < 14; i++)", self.source)
        self.assertIn("for (int i = 0; i < 10; i++)", self.source)
        self.assertIn("new Vector3(-12.8f, 0.20f, 11.8f)", self.source)
        self.assertIn("for (int i = 0; i < 5; i++)", self.source)

    def test_d35_surface_audit_excludes_only_the_nested_d27_presentation_layer(self):
        self.assertIn("IsNestedD27Presentation", self.source)
        self.assertIn("if (!IsNestedD27Presentation(renderer.transform))", self.source)
        self.assertIn("if (!IsNestedD27Presentation(collider.transform))", self.source)
        self.assertIn("!IsNestedD27Presentation(filter.transform)", self.source)
        self.assertIn('current.name == "D27_R03_CharacterWildlifePresentation"', self.source)

    def test_rocks_use_a_ring_silhouette_instead_of_triangle_spikes(self):
        start = self.source.index("private static Mesh MakeRockMesh")
        end = self.source.index("private static Mesh MakeShrubMesh", start)
        block = self.source[start:end]
        self.assertIn("const int segments = 8", block)
        self.assertIn("ringScales", block)
        self.assertIn("radius * 0.42f", block)
        self.assertIn("radius * 0.34f", block)
        self.assertNotIn("radius * 1.34f", block)
        self.assertIn("triangles.Add(capNext); triangles.Add(cap); triangles.Add(top);", block)
        self.assertIn("vertices.Add(new Vector3(0f, -radius * 0.03f", block)

    def test_shrubs_use_a_grounded_rounded_volume_instead_of_a_four_sided_pyramid(self):
        start = self.source.index("private static Mesh MakeShrubMesh")
        end = self.source.index("private static Mesh MakeCanopyMesh", start)
        block = self.source[start:end]
        self.assertIn("const int segments = 8", block)
        self.assertIn("ringHeights", block)
        self.assertIn("radius * 0.46f", block)
        self.assertIn("triangles.Add(upperNext); triangles.Add(upper); triangles.Add(top);", block)
        self.assertNotIn("new Vector3(-radius,0,0)", block)

    def test_unverified_offshore_backdrop_is_not_instantiated_in_the_visible_prefab(self):
        start = self.source.index("private static GameObject CreateHarborBackdrop")
        end = self.source.index("private static void AddCandidateDetails", start)
        block = self.source[start:end]
        self.assertIn("Keep this root empty until an offshore asset", block)
        self.assertNotIn('AddMesh(root.transform, "BackdropRock_"', block)

    def test_houses_use_gabled_roofs_and_facade_articulation(self):
        self.assertIn("MakeGableRoofMesh", self.source)
        self.assertIn('AddMesh(root.transform, "Foundation"', self.source)
        self.assertIn("AddFacadeDetails", self.source)
        self.assertIn('"SideShutter_"', self.source)

    def test_house_roots_follow_authored_height_instead_of_constant_y(self):
        start = self.source.index("private static GameObject CreateHouse")
        end = self.source.index("private static void AddFacadeDetails", start)
        block = self.source[start:end]
        self.assertIn("Height(position.x, position.z) - HouseFoundationBottomOffset", block)
        self.assertIn("HouseFoundationBottomOffset = 0.02f", self.source)
        self.assertNotIn("root.transform.position = position;", block)

    def test_landmark_signs_face_the_street(self):
        self.assertIn("go.transform.localRotation = Quaternion.identity", self.source)
        self.assertIn("D19R03LandmarkLabelVisibility", self.source)
        self.assertIn("mesh.fontSize = 32", self.source)

    def test_legacy_cleanup_does_not_hide_current_r03_shoreline(self):
        start = self.source.index("private static void HideLegacyVisuals")
        end = self.source.index("private static string[] SceneRootSummary", start)
        method = self.source[start:end]
        self.assertIn('if (root.name == RootName) continue;', method)
        self.assertIn('"Beach", "Sea", "Dock", "DockDeck", "Gangplank", "HandRail"', method)

    def test_route_probe_uses_a_fresh_evidence_attempt_after_visual_rebuild(self):
        self.assertIn('HasFlag("-d19-r03-route-rerun47")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN47.json"', self.route_probe)

    def test_latest_route_probe_attempt_is_versioned_after_collision_fix(self):
        self.assertIn('HasFlag("-d19-r03-route-rerun48")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN48.json"', self.route_probe)
        self.assertIn('HasFlag("-d19-r03-route-rerun49")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN49.json"', self.route_probe)

    def test_main_menu_focus_fix_has_an_isolated_rerun50_player_probe(self):
        self.assertIn('BuildMainMenuRoutePlayerRerun50', self.player_builder)
        self.assertIn('D19_R03_A01_Player_RERUN50.exe', self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun50")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN50.json"', self.route_probe)
        self.assertIn('"route_rerun50_"', self.route_probe)

    def test_camera_and_neutral_pose_fix_has_an_isolated_rerun51_player_probe(self):
        self.assertIn('BuildMainMenuRoutePlayerRerun51', self.player_builder)
        self.assertIn('D19_R03_A01_Player_RERUN51.exe', self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun51")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN51.json"', self.route_probe)
        self.assertIn('"route_rerun51_"', self.route_probe)

    def test_boarding_and_disembark_visual_fix_has_an_isolated_rerun52_player_probe(self):
        self.assertIn('BuildMainMenuRoutePlayerRerun52', self.player_builder)
        self.assertIn('D19_R03_A01_Player_RERUN52.exe', self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun52")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN52.json"', self.route_probe)
        self.assertIn('"route_rerun52_"', self.route_probe)

    def test_r03_berth_bridge_and_ship_clearance_have_an_isolated_rerun53_player_probe(self):
        self.assertIn('BuildMainMenuRoutePlayerRerun53', self.player_builder)
        self.assertIn('D19_R03_A01_Player_RERUN53.exe', self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun53")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN53.json"', self.route_probe)
        self.assertIn('"route_rerun53_"', self.route_probe)

    def test_q30_r16_backdrop_isolation_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ30CandidateR16", self.player_builder)
        self.assertIn("Q30_CANDIDATE_PLAYER_R16.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun68")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN68.json"', self.route_probe)
        self.assertIn('"route_rerun68_"', self.route_probe)

    def test_q21_a01_grounded_house_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A01CandidateR17", self.player_builder)
        self.assertIn("Q21_A01_CANDIDATE_PLAYER_R17.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun69")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN69.json"', self.route_probe)
        self.assertIn('"route_rerun69_"', self.route_probe)

    def test_q21_a02_ship_deck_camera_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A02CandidateR18", self.player_builder)
        self.assertIn("Q21_A02_CANDIDATE_PLAYER_R18.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun70")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN70.json"', self.route_probe)
        self.assertIn('"route_rerun70_"', self.route_probe)

    def test_q21_a03_swimming_camera_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A03CandidateR19", self.player_builder)
        self.assertIn("Q21_A03_CANDIDATE_PLAYER_R19.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun71")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN71.json"', self.route_probe)
        self.assertIn('"route_rerun71_"', self.route_probe)

    def test_q21_a03_swimming_pose_candidate_r20_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A03CandidateR20", self.player_builder)
        self.assertIn("Q21_A03_CANDIDATE_PLAYER_R20.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun72")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN72.json"', self.route_probe)
        self.assertIn('"route_rerun72_"', self.route_probe)

    def test_q21_a04_helm_presentation_candidate_r21_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A04CandidateR21", self.player_builder)
        self.assertIn("Q21_A04_CANDIDATE_PLAYER_R21.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun73")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN73.json"', self.route_probe)
        self.assertIn('"route_rerun73_"', self.route_probe)

    def test_q21_a04_capped_helm_candidate_r22_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A04CandidateR22", self.player_builder)
        self.assertIn("Q21_A04_CANDIDATE_PLAYER_R22.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun74")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN74.json"', self.route_probe)
        self.assertIn('"route_rerun74_"', self.route_probe)

    def test_q21_a04_helm_occlusion_candidate_r23_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A04CandidateR23", self.player_builder)
        self.assertIn("Q21_A04_CANDIDATE_PLAYER_R23.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun75")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN75.json"', self.route_probe)
        self.assertIn('"route_rerun75_"', self.route_probe)

    def test_q21_a04_spoked_helm_candidate_r24_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A04CandidateR24", self.player_builder)
        self.assertIn("Q21_A04_CANDIDATE_PLAYER_R24.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun76")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN76.json"', self.route_probe)
        self.assertIn('"route_rerun76_"', self.route_probe)

    def test_q03_a01_swimming_fallback_candidate_r25_probe_isolated_from_prior_player_evidence(self):
        self.assertIn("BuildMainMenuRoutePlayerQ03A01CandidateR25", self.player_builder)
        self.assertIn("Q03_A01_CANDIDATE_PLAYER_R25.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun77")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN77.json"', self.route_probe)
        self.assertIn('"route_rerun77_"', self.route_probe)

    def test_q20_a01_legacy_tree_containment_candidate_r26_probe_isolated(self):
        self.assertIn("BuildMainMenuRoutePlayerQ20A01CandidateR26", self.player_builder)
        self.assertIn("Q20_A01_CANDIDATE_PLAYER_R26.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun78")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN78.json"', self.route_probe)
        self.assertIn('"route_rerun78_"', self.route_probe)

    def test_q21_a05_steering_pose_candidate_r27_probe_isolated(self):
        self.assertIn("BuildMainMenuRoutePlayerQ21A05CandidateR27", self.player_builder)
        self.assertIn("Q21_A05_CANDIDATE_PLAYER_R27.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun79")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN79.json"', self.route_probe)
        self.assertIn('"route_rerun79_"', self.route_probe)

    def test_q05_a02_swim_stroke_candidate_r29_probe_isolated(self):
        self.assertIn("BuildMainMenuRoutePlayerQ05A02CandidateR29", self.player_builder)
        self.assertIn("Q05_A02_CANDIDATE_PLAYER_R29.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun81")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN81.json"', self.route_probe)
        self.assertIn('"route_rerun81_"', self.route_probe)

    def test_q05_a03_helm_alignment_candidate_r30_probe_isolated(self):
        self.assertIn("BuildMainMenuRoutePlayerQ05A03CandidateR30", self.player_builder)
        self.assertIn("Q05_A03_CANDIDATE_PLAYER_R30.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun82")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN82.json"', self.route_probe)
        self.assertIn('"route_rerun82_"', self.route_probe)

    def test_q05_a03_swim_helm_followup_candidate_r31_probe_isolated(self):
        self.assertIn("BuildMainMenuRoutePlayerQ05A03CandidateR31", self.player_builder)
        self.assertIn("Q05_A03_CANDIDATE_PLAYER_R31.exe", self.player_builder)
        self.assertIn('HasFlag("-d19-r03-route-rerun83")', self.route_probe)
        self.assertIn('"D19_R03_A01_RUNTIME_ROUTE_PROBE_RERUN83.json"', self.route_probe)
        self.assertIn('"route_rerun83_"', self.route_probe)

    def test_route_probe_landmarks_match_current_r03_service_building_coordinates(self):
        self.assertIn(
            'Landmark("building:market_santa_margarita", "MarketVisual", new Vector3(6.4f, 0f, 1.2f)',
            self.route_probe,
        )
        self.assertIn(
            'Landmark("building:warehouse_santa_margarita", "WarehouseVisual", new Vector3(-6.4f, 0f, 1.2f)',
            self.route_probe,
        )
        self.assertIn(
            'Landmark("building:shipyard_santa_margarita", "ShipyardVisual", new Vector3(-9.5f, 0f, -2.6f)',
            self.route_probe,
        )

    def test_route_collision_contract_shares_the_visible_island_transform(self):
        start = self.source.index("private static void AddRouteGroundCollider")
        end = self.source.index("public static void Audit", start)
        method = self.source[start:end]
        self.assertIn("AddRouteGroundCollider(prefabRoot.transform, island)", self.source)
        self.assertIn("ground.transform.localPosition = Vector3.zero", method)
        self.assertIn("var collider = ground.AddComponent<MeshCollider>()", method)
        self.assertNotIn("AddComponent<BoxCollider>()", method)
        self.assertNotIn("new Vector3(0f, 0.05f, 5.0f)", method)

    def test_runtime_mesh_collider_resynchronizes_stale_prefab_transform(self):
        collision = COLLISION_CONTROLLER.read_text(encoding="utf-8")
        start = collision.index("private bool TryInstallR03WalkableSurface")
        end = collision.index("private void DisableColliderByName", start)
        method = collision[start:end]
        self.assertIn("terrain.transform.localPosition = surface.transform.localPosition", method)
        self.assertIn("terrain.transform.localRotation = surface.transform.localRotation", method)
        self.assertIn("terrain.transform.localScale = surface.transform.localScale", method)

    def test_serialized_r03_route_contract_starts_at_visible_surface_origin(self):
        prefab = R03_PREFAB.read_text(encoding="utf-8")
        terrain_start = prefab.index("m_Name: D19_TerrainRoot")
        terrain_end = prefab.index("--- !u!1", terrain_start + 1)
        terrain_block = prefab[terrain_start:terrain_end]
        self.assertIn("m_LocalPosition: {x: 0, y: 0, z: 0}", terrain_block)
        self.assertIn("--- !u!64", terrain_block)
        self.assertNotIn("--- !u!65", terrain_block)
        self.assertIn("m_Name: D19_R03_IslandSurface", prefab)


if __name__ == "__main__":
    unittest.main()
