import json
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tools"))

from d18_license_normalizer import (  # noqa: E402
    BLOCKED,
    CONFLICT_ATTRIBUTION,
    CONFLICT_COMMERCIAL,
    CONFLICT_REDISTRIBUTION,
    FIELD_INVALID,
    FIELD_MISSING,
    FIELD_UNCONFIRMED,
    LICENSE_NC,
    LICENSE_PERSONAL_ONLY,
    LICENSE_SHARE_ALIKE,
    LICENSE_UNKNOWN,
    LICENSE_UNSUPPORTED_VERSION,
    NORMALIZED,
    PENDING,
    RECORD_NOT_OBJECT,
    STATUSES,
    canonicalize_license_id,
    normalize_license,
    normalize_license_batch,
)


def full_record(**overrides):
    record = {
        "record_id": "fixture_0001",
        "license_id": "CC0-1.0",
        "attribution_required": False,
        "commercial_use": True,
        "modification_allowed": True,
        "source_redistribution": "allowed",
        "build_distribution": "allowed",
    }
    record.update(overrides)
    return record


class CanonicalizationTests(unittest.TestCase):
    def test_cc0_exact_and_aliases(self):
        for raw in ("CC0-1.0", "CC0", "CC0 1.0 Universal", "Creative Commons Zero", "Public Domain Zero"):
            canonical, code = canonicalize_license_id(raw)
            self.assertEqual((canonical, code), ("CC0-1.0", "ok"), raw)

    def test_cc0_url(self):
        canonical, code = canonicalize_license_id("https://creativecommons.org/publicdomain/zero/1.0/")
        self.assertEqual((canonical, code), ("CC0-1.0", "ok"))

    def test_cc_by_aliases(self):
        for raw in ("CC-BY-4.0", "CC BY 4.0", "CC BY 4.0 International", "Creative Commons Attribution 4.0"):
            canonical, code = canonicalize_license_id(raw)
            self.assertEqual((canonical, code), ("CC-BY-4.0", "ok"), raw)
        canonical, code = canonicalize_license_id("CC BY 3.0 Unported")
        self.assertEqual((canonical, code), ("CC-BY-3.0", "ok"))
        canonical, code = canonicalize_license_id("CC-BY-NC-4.0")
        self.assertEqual((canonical, code), ("CC-BY-NC-4.0", "ok"))
        canonical, code = canonicalize_license_id("CC BY-NC-SA 4.0")
        self.assertEqual((canonical, code), ("CC-BY-NC-SA-4.0", "ok"))

    def test_cc_by_urls(self):
        canonical, code = canonicalize_license_id("https://creativecommons.org/licenses/by/4.0/")
        self.assertEqual((canonical, code), ("CC-BY-4.0", "ok"))
        canonical, code = canonicalize_license_id("https://creativecommons.org/licenses/by-nc/4.0/")
        self.assertEqual((canonical, code), ("CC-BY-NC-4.0", "ok"))

    def test_mit_and_ofl(self):
        self.assertEqual(canonicalize_license_id("MIT"), ("MIT", "ok"))
        self.assertEqual(canonicalize_license_id("MIT License"), ("MIT", "ok"))
        self.assertEqual(canonicalize_license_id("OFL-1.1"), ("OFL-1.1", "ok"))
        self.assertEqual(canonicalize_license_id("SIL Open Font License 1.1"), ("OFL-1.1", "ok"))
        self.assertEqual(canonicalize_license_id("https://openfontlicense.org"), ("OFL-1.1", "ok"))

    def test_unsupported_version_is_pending_not_guessed(self):
        for raw in ("CC-BY-2.5", "CC BY 2.5", "https://creativecommons.org/licenses/by/2.5/"):
            canonical, code = canonicalize_license_id(raw)
            self.assertIsNone(canonical)
            self.assertEqual(code, "unsupported_version", raw)

    def test_bare_cc_by_without_version_is_unknown(self):
        canonical, code = canonicalize_license_id("CC-BY")
        self.assertIsNone(canonical)
        self.assertEqual(code, "unknown")

    def test_public_domain_is_not_cc0(self):
        canonical, code = canonicalize_license_id("Public Domain")
        self.assertIsNone(canonical)
        self.assertEqual(code, "unknown")

    def test_no_cc0_guess_for_free_text(self):
        canonical, code = canonicalize_license_id("Free asset pack, all rights belong to author")
        self.assertIsNone(canonical)
        self.assertEqual(code, "unknown")

    def test_restriction_text_detection(self):
        self.assertEqual(canonicalize_license_id("Free for non-commercial use")[1], "nc")
        self.assertEqual(canonicalize_license_id("Personal use only")[1], "personal")
        self.assertEqual(canonicalize_license_id("Do not redistribute")[1], "no_redistribution")
        self.assertEqual(canonicalize_license_id("No derivative works")[1], "no_derivatives")


class NormalizedRecordTests(unittest.TestCase):
    def test_cc0_full_record_is_normalized(self):
        result = normalize_license(full_record())
        self.assertEqual(result["status"], NORMALIZED)
        self.assertEqual(result["canonical_license"], "CC0-1.0")
        self.assertIs(result["attribution_required"], False)
        self.assertIs(result["commercial_allowed"], True)
        self.assertIs(result["modification_allowed"], True)
        self.assertIs(result["redistribution_allowed"], True)
        self.assertEqual(result["errors"], [])

    def test_cc_by_full_record_is_normalized(self):
        result = normalize_license(
            full_record(license_id="CC-BY-4.0", attribution_required=True)
        )
        self.assertEqual(result["status"], NORMALIZED)
        self.assertEqual(result["canonical_license"], "CC-BY-4.0")
        self.assertIs(result["attribution_required"], True)
        self.assertIs(result["commercial_allowed"], True)
        self.assertIs(result["redistribution_allowed"], True)

    def test_unknown_license_pending_and_never_cc0(self):
        result = normalize_license(full_record(license_id="Some Free Art License"))
        self.assertEqual(result["status"], PENDING)
        self.assertIsNone(result["canonical_license"])
        self.assertIsNone(result["attribution_required"])
        self.assertIsNone(result["commercial_allowed"])
        self.assertIsNone(result["modification_allowed"])
        self.assertIsNone(result["redistribution_allowed"])
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(LICENSE_UNKNOWN, codes)
        self.assertNotEqual(result["canonical_license"], "CC0-1.0")

    def test_attribution_conflict_is_blocked_and_never_relaxed(self):
        result = normalize_license(
            full_record(license_id="CC-BY-4.0", attribution_required=False)
        )
        self.assertEqual(result["status"], BLOCKED)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(CONFLICT_ATTRIBUTION, codes)
        self.assertIs(result["attribution_required"], True)

    def test_commercial_claim_false_is_blocked_and_never_relaxed(self):
        result = normalize_license(full_record(commercial_use=False))
        self.assertEqual(result["status"], BLOCKED)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(CONFLICT_COMMERCIAL, codes)
        self.assertIs(result["commercial_allowed"], False)

    def test_nc_license_is_blocked(self):
        result = normalize_license(
            full_record(
                license_id="CC-BY-NC-4.0",
                attribution_required=True,
                commercial_use=True,
                source_redistribution="allowed",
            )
        )
        self.assertEqual(result["status"], BLOCKED)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(LICENSE_NC, codes)
        self.assertIn(CONFLICT_COMMERCIAL, codes)
        self.assertIs(result["commercial_allowed"], False)
        self.assertIs(result["redistribution_allowed"], False)
        self.assertIs(result["attribution_required"], True)

    def test_nc_claim_in_commercial_field_is_blocked(self):
        result = normalize_license(full_record(commercial_use="non-commercial"))
        self.assertEqual(result["status"], BLOCKED)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(LICENSE_NC, codes)
        self.assertIs(result["commercial_allowed"], False)
        self.assertIs(result["redistribution_allowed"], False)

    def test_personal_use_claim_is_blocked(self):
        result = normalize_license(full_record(commercial_use="personal use"))
        self.assertEqual(result["status"], BLOCKED)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(LICENSE_PERSONAL_ONLY, codes)
        self.assertIs(result["commercial_allowed"], False)
        self.assertIs(result["modification_allowed"], False)
        self.assertIs(result["redistribution_allowed"], False)

    def test_share_alike_is_blocked(self):
        result = normalize_license(
            full_record(license_id="CC-BY-SA-4.0", attribution_required=True)
        )
        self.assertEqual(result["status"], BLOCKED)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(LICENSE_SHARE_ALIKE, codes)
        self.assertIsNone(result["redistribution_allowed"])

    def test_missing_attribution_record_is_pending(self):
        record = full_record(license_id="CC-BY-4.0")
        del record["attribution_required"]
        result = normalize_license(record)
        self.assertEqual(result["status"], PENDING)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(FIELD_MISSING, codes)
        fields = [error["field"] for error in result["errors"]]
        self.assertIn("attribution_required", fields)
        self.assertIsNone(result["attribution_required"])

    def test_missing_redistribution_fields_are_pending(self):
        record = full_record()
        del record["source_redistribution"]
        del record["build_distribution"]
        result = normalize_license(record)
        self.assertEqual(result["status"], PENDING)
        fields = [error["field"] for error in result["errors"]]
        self.assertIn("redistribution", fields)
        self.assertIsNone(result["redistribution_allowed"])

    def test_pending_distribution_value_is_unconfirmed(self):
        record = full_record(source_redistribution="allowed", build_distribution="pending")
        result = normalize_license(record)
        self.assertEqual(result["status"], PENDING)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(FIELD_UNCONFIRMED, codes)
        self.assertIsNone(result["redistribution_allowed"])

    def test_conflicting_redistribution_fields_are_blocked(self):
        result = normalize_license(
            full_record(source_redistribution="allowed", build_distribution="not_allowed")
        )
        self.assertEqual(result["status"], BLOCKED)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(CONFLICT_REDISTRIBUTION, codes)
        self.assertIs(result["redistribution_allowed"], False)

    def test_ofl_and_mit_are_normalized_with_note(self):
        result = normalize_license(
            full_record(license_id="OFL-1.1", attribution_required=True)
        )
        self.assertEqual(result["status"], NORMALIZED)
        self.assertTrue(result["warnings"])
        self.assertIs(result["attribution_required"], True)
        result = normalize_license(full_record(license_id="MIT", attribution_required=True))
        self.assertEqual(result["status"], NORMALIZED)

    def test_invalid_field_value_is_pending(self):
        result = normalize_license(full_record(attribution_required="banana"))
        self.assertEqual(result["status"], PENDING)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(FIELD_INVALID, codes)

    def test_missing_license_field_is_pending(self):
        record = full_record()
        del record["license_id"]
        result = normalize_license(record)
        self.assertEqual(result["status"], PENDING)
        fields = [error["field"] for error in result["errors"]]
        self.assertIn("license_id", fields)

    def test_record_not_object_is_pending(self):
        result = normalize_license("not an object")
        self.assertEqual(result["status"], PENDING)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(RECORD_NOT_OBJECT, codes)

    def test_unsupported_version_is_pending(self):
        result = normalize_license(full_record(license_id="CC-BY-2.5", attribution_required=True))
        self.assertEqual(result["status"], PENDING)
        codes = [error["code"] for error in result["errors"]]
        self.assertIn(LICENSE_UNSUPPORTED_VERSION, codes)
        self.assertIsNone(result["canonical_license"])

    def test_output_is_deterministic(self):
        first = json.dumps(normalize_license(full_record()), ensure_ascii=False, sort_keys=True)
        second = json.dumps(normalize_license(full_record()), ensure_ascii=False, sort_keys=True)
        self.assertEqual(first, second)

    def test_batch_summary_counts(self):
        records = [
            full_record(record_id="a"),
            full_record(record_id="b", license_id="Unknown Thing"),
            full_record(record_id="c", license_id="CC-BY-NC-4.0", attribution_required=True),
        ]
        result = normalize_license_batch(records)
        self.assertEqual(result["record_count"], 3)
        self.assertEqual(result["summary"][NORMALIZED], 1)
        self.assertEqual(result["summary"][BLOCKED], 1)
        self.assertEqual(result["summary"][PENDING], 1)
        self.assertEqual(len(result["records"]), 3)

    def test_statuses_never_contain_approval_vocabulary(self):
        self.assertEqual(STATUSES, {NORMALIZED, BLOCKED, PENDING})
        self.assertNotIn("APPROVED", STATUSES)
        self.assertNotIn("RuntimeApproved", STATUSES)
        self.assertNotIn("ReleaseApproved", STATUSES)


class OfflineAndScopeTests(unittest.TestCase):
    def test_no_network_imports_in_tool(self):
        source = (ROOT / "tools/d18_license_normalizer.py").read_text(encoding="utf-8")
        for forbidden in ("import urllib", "import requests", "import socket", "urlopen", "httpx", "aiohttp"):
            self.assertNotIn(forbidden, source, forbidden)

    def test_no_hardcoded_real_resource_approvals(self):
        source = (ROOT / "tools/d18_license_normalizer.py").read_text(encoding="utf-8")
        for forbidden in ("kenney", "quaternius", "polyhaven", "ambientcg", "pack_", "manifest.json", "intake"):
            self.assertNotIn(forbidden, source.casefold(), forbidden)


if __name__ == "__main__":
    unittest.main()
