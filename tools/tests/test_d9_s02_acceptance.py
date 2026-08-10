import unittest

from tools.d9_s02_acceptance_check import FLOW_STAGES, FROZEN_EVENTS, run_checks


class D9S02AcceptanceTests(unittest.TestCase):
    def test_gate_covers_the_frozen_full_flow_and_all_events(self):
        report = run_checks()

        self.assertEqual("D9-S02", report["task_id"])
        self.assertEqual(14, report["frozen_event_count"])
        self.assertEqual([stage[0] for stage in FLOW_STAGES], [step["stage"] for step in report["flow"]])
        self.assertEqual(14, len(FROZEN_EVENTS))

    def test_gate_reports_complete_owner_chain_and_flow(self):
        report = run_checks()
        self.assertEqual("PASS", report["status"])

        blocked_ids = {check["id"] for check in report["checks"] if check["status"] == "BLOCKED"}
        self.assertEqual(set(), blocked_ids)


if __name__ == "__main__":
    unittest.main()
