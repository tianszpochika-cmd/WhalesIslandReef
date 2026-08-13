import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
REPORT = ROOT / "tools" / "results" / "d16_a01_performance_window_report.json"


class D16A01PerformanceReportTests(unittest.TestCase):
    def test_current_report_contains_real_load_gc_and_runtime_metrics(self):
        self.assertTrue(REPORT.is_file(), "当前 A01 性能报告不存在")
        report = json.loads(REPORT.read_text(encoding="utf-8"))
        required = {
            "SceneLoadSeconds",
            "RuntimeBootstrapReadySeconds",
            "D5RuntimeReadySeconds",
            "D16ControllerAwakeMs",
            "D16FirstRegistrationRefreshMs",
            "D16FirstUpdateMs",
            "D16FirstBudgetEvaluationMs",
            "D5AwakeMs",
            "D5RuntimeBuildElapsedMs",
            "D5RuntimePhaseTimingsComplete",
            "D5CompletedRuntimePhaseCount",
            "D5WaitingForPlayerMs",
            "D5LoadingSnapshotMs",
            "D5CreatingSystemsMs",
            "D5InitializingStateMs",
            "D5ApplyingCharacterVisualMs",
            "D5BuildingMapMs",
            "D2RuntimeBuildElapsedMs",
            "D2RuntimePhaseTimingsComplete",
            "D2CompletedRuntimePhaseCount",
            "D2EnsureShipMs",
            "D2EnsureCrewAnchorMs",
            "D2EnsurePlayerMs",
            "D2ConfigurePhysicsMs",
            "D2ConfigureControlsMs",
            "D2ConfigurePresentationMs",
            "D3RuntimeReadySeconds",
            "D4RuntimeReadySeconds",
            "D9RuntimeReadySeconds",
            "ManagedGcGen0Collections",
            "ManagedGcGen1Collections",
            "ManagedGcGen2Collections",
            "ActiveAgentCount",
            "ActiveAnimalCount",
            "AudibleSourceCount",
            "LoadedCellCount",
            "RegistrationCount",
            "CellTransitionCount",
            "LoadingCellCount",
            "LoadedPresentationObjectCount",
            "PendingPresentationObjectCount",
            "MaxPresentationLoadDurationMs",
            "PresentationLoadCount",
            "PoolRentCount",
            "PoolReuseCount",
            "PoolReturnCount",
            "PoolPooledCount",
            "CatalogIslandCellCount",
            "ActiveIslandDataCellCount",
            "DataOnlyIslandCellCount",
            "VisualPresentationIslandCellCount",
            "ReferenceIslandId",
            "SampleCount",
            "Passed",
        }
        self.assertTrue(required.issubset(report), "性能报告缺少加载/GC/运行时字段")
        self.assertTrue(report["Passed"], "Unity 已生成的当前性能窗口不能被工具测试改写为通过")
        self.assertGreaterEqual(report["SampleCount"], 100)
        self.assertGreaterEqual(report["SceneLoadSeconds"], 0.0)
        self.assertGreaterEqual(report["RuntimeBootstrapReadySeconds"], 0.0)
        self.assertGreaterEqual(report["D5RuntimeReadySeconds"], 0.0)
        for field in (
            "D16ControllerAwakeMs",
            "D16FirstRegistrationRefreshMs",
            "D16FirstUpdateMs",
            "D16FirstBudgetEvaluationMs",
            "MaxPresentationLoadDurationMs",
        ):
            self.assertGreaterEqual(report[field], 0.0)
        self.assertTrue(report["D5RuntimePhaseTimingsComplete"])
        self.assertGreaterEqual(report["D5CompletedRuntimePhaseCount"], 6)
        for field in (
            "D5AwakeMs",
            "D5RuntimeBuildElapsedMs",
            "D5WaitingForPlayerMs",
            "D5LoadingSnapshotMs",
            "D5CreatingSystemsMs",
            "D5InitializingStateMs",
            "D5ApplyingCharacterVisualMs",
            "D5BuildingMapMs",
        ):
            self.assertGreaterEqual(report[field], 0.0, f"性能报告字段 {field} 不应缺失")
        self.assertTrue(report["D2RuntimePhaseTimingsComplete"])
        self.assertGreaterEqual(report["D2CompletedRuntimePhaseCount"], 6)
        for field in (
            "D2RuntimeBuildElapsedMs",
            "D2EnsureShipMs",
            "D2EnsureCrewAnchorMs",
            "D2EnsurePlayerMs",
            "D2ConfigurePhysicsMs",
            "D2ConfigureControlsMs",
            "D2ConfigurePresentationMs",
        ):
            self.assertGreaterEqual(report[field], 0.0, f"性能报告字段 {field} 不应缺失")
        for field in ("D3RuntimeReadySeconds", "D4RuntimeReadySeconds", "D9RuntimeReadySeconds"):
            self.assertGreaterEqual(report[field], 0.0, f"性能报告字段 {field} 不应缺失")
        for field in ("ManagedGcGen0Collections", "ManagedGcGen1Collections", "ManagedGcGen2Collections"):
            self.assertGreaterEqual(report[field], 0)
        for field in (
            "ActiveAgentCount",
            "ActiveAnimalCount",
            "AudibleSourceCount",
            "LoadedCellCount",
            "RegistrationCount",
            "CellTransitionCount",
            "LoadingCellCount",
            "LoadedPresentationObjectCount",
            "PendingPresentationObjectCount",
            "PresentationLoadCount",
            "PoolRentCount",
            "PoolReuseCount",
            "PoolReturnCount",
            "PoolPooledCount",
            "CatalogIslandCellCount",
            "ActiveIslandDataCellCount",
            "DataOnlyIslandCellCount",
            "VisualPresentationIslandCellCount",
        ):
            self.assertGreaterEqual(report[field], 0, f"性能报告字段 {field} 不应缺失")
        self.assertEqual(report["CatalogIslandCellCount"], 12)
        self.assertGreater(report["ActiveIslandDataCellCount"], 0)
        self.assertEqual(report["DataOnlyIslandCellCount"], 12)
        self.assertEqual(report["VisualPresentationIslandCellCount"], 0)
        self.assertEqual(report["ReferenceIslandId"], "santa_margarita")

    def test_gpu_unavailable_is_preserved_as_unavailable(self):
        report = json.loads(REPORT.read_text(encoding="utf-8"))
        if report["GpuSampleAvailable"]:
            self.assertGreater(report["GpuTimeMs"], 0.0)
        else:
            self.assertLessEqual(report["GpuTimeMs"], 0.0)


if __name__ == "__main__":
    unittest.main()
