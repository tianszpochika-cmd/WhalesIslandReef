"""D18-L04 — pure offline license normalization.

Normalizes raw license names/identifiers together with the attribution,
commercial, modification and redistribution claims of an asset record into
deterministic values that upstream audits (L07 rights, L09 attribution,
L42 catalog compiler) can consume:

- canonical_license: stable license identifier (e.g. "CC0-1.0", "CC-BY-4.0")
- attribution_required / commercial_allowed / modification_allowed /
  redistribution_allowed: conservative effective truth
- status: NORMALIZED | BLOCKED | PENDING, with machine-readable errors

Frozen rules (conservative, never guessing):

1. Offline only: no network access, no downloads, no URL validation.
2. Unknown license text is never guessed to be CC0 or any other license;
   it stays PENDING (LICENSE_UNKNOWN).
3. Missing or unconfirmable required fields keep the record PENDING
   (FIELD_MISSING / FIELD_UNCONFIRMED / FIELD_INVALID).
4. Non-commercial (NC), personal-use-only, no-redistribution,
   no-derivatives and share-alike restrictions return BLOCKED.
5. A claim that contradicts the canonical license is a conflict
   (CONFLICT_*); the more restrictive value always wins, never the relaxed
   one. A "CC-BY with attribution_required=false" input stays blocked and
   still reports attribution_required=true.
6. NORMALIZED only means "license normalized and no restriction was
   relaxed". It is a normalization verdict, never an asset approval;
   state machine approval remains human-gated and out of scope here.

Input record keys (aliases accepted):

- license: license_id | license | license_url
- attribution_required
- commercial: commercial_use | commercial_allowed
- modification_allowed
- redistribution: source_redistribution | build_distribution |
  redistribution_allowed (all provided values must agree)
- record_id (optional, audit traceability)

Supported canonical licenses: CC0-1.0, CC-BY-3.0/4.0, CC-BY-SA-3.0/4.0,
CC-BY-NC-3.0/4.0, CC-BY-NC-SA-3.0/4.0, MIT, OFL-1.1.

Usage:

    python tools/d18_license_normalizer.py --input record.json [--output out.json]
    python tools/d18_license_normalizer.py < record.json
    python tools/d18_license_normalizer.py --input records.json --strict
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from dataclasses import dataclass
from pathlib import Path
from typing import Any

TOOL = "tools/d18_license_normalizer.py"

NORMALIZED = "NORMALIZED"
BLOCKED = "BLOCKED"
PENDING = "PENDING"
STATUSES = frozenset({NORMALIZED, BLOCKED, PENDING})

LICENSE_UNKNOWN = "LICENSE_UNKNOWN"
LICENSE_UNSUPPORTED_VERSION = "LICENSE_UNSUPPORTED_VERSION"
LICENSE_NC = "LICENSE_NC"
LICENSE_PERSONAL_ONLY = "LICENSE_PERSONAL_ONLY"
LICENSE_NO_REDISTRIBUTION = "LICENSE_NO_REDISTRIBUTION"
LICENSE_NO_DERIVATIVES = "LICENSE_NO_DERIVATIVES"
LICENSE_SHARE_ALIKE = "LICENSE_SHARE_ALIKE"
FIELD_MISSING = "FIELD_MISSING"
FIELD_UNCONFIRMED = "FIELD_UNCONFIRMED"
FIELD_INVALID = "FIELD_INVALID"
CONFLICT_ATTRIBUTION = "CONFLICT_ATTRIBUTION"
CONFLICT_COMMERCIAL = "CONFLICT_COMMERCIAL"
CONFLICT_MODIFICATION = "CONFLICT_MODIFICATION"
CONFLICT_REDISTRIBUTION = "CONFLICT_REDISTRIBUTION"
RECORD_NOT_OBJECT = "RECORD_NOT_OBJECT"

TRUE = "TRUE"
FALSE = "FALSE"
NC = "NC"
PERSONAL = "PERSONAL"
UNCONFIRMED = "UNCONFIRMED"
INVALID = "INVALID"

SUPPORTED_VERSIONS = frozenset({"3.0", "4.0"})

LICENSE_KEYS = ("license_id", "license", "license_url")
COMMERCIAL_KEYS = ("commercial_use", "commercial_allowed")
REDIST_KEYS = ("source_redistribution", "build_distribution", "redistribution_allowed")

_TRUE_CLAIMS = frozenset({"true", "yes", "y", "allowed", "required", "1", "ok"})
_FALSE_CLAIMS = frozenset({"false", "no", "n", "not allowed", "not required", "forbidden", "prohibited", "0", "never"})
_UNCONFIRMED_CLAIMS = frozenset({"pending", "not applicable", "na", "unknown", "unsure", "unclear", "none", "null", ""})

_STOPWORDS = frozenset({
    "LICENSE", "LICENCE", "THE", "FOR", "OF", "INTERNATIONAL", "UNPORTED",
    "GENERIC", "UNITED", "STATES", "PORTED", "DEED", "LEGAL", "CODE",
    "UNIVERSAL", "US",
})

_CC_VERSION_RE = re.compile(r"\bCC\s+BY\b(?:\s+(?:NC|SA|ND))*\s+([0-9]+(?:\.[0-9]+)?)")
_NC_TEXT_RE = re.compile(
    r"(non[- ]?commercial|noncommercial|no commercial|\bnc\b|"
    r"commercial(?: use)?(?: is)? (?:prohibited|not allowed|forbidden))"
)
_PERSONAL_TEXT_RE = re.compile(r"personal[- ]?use|personal only|for personal|private use only")
_NO_REDIST_RE = re.compile(
    r"no[- ]?redistribut|not[- ]?redistribut|cannot[- ]?(?:be[- ]?)?redistribut|"
    r"redistribut(?:ion)?[- ]?(?:prohibited|forbidden|not[- ]?allowed|not[- ]?permitted)|"
    r"don.?t[- ]?redistribut"
)
_NO_DERIV_RE = re.compile(r"no[- ]?derivative|derivative[- ]?work[- ]?prohibited|modification[- ]?prohibited|no[- ]?modification")

_URL_RULES: list[tuple[re.Pattern[str], str | None, bool]] = [
    (re.compile(r"creativecommons\.org/publicdomain/zero/1\.0"), "CC0-1.0", False),
    (re.compile(r"creativecommons\.org/licenses/by-sa/([0-9.]+)"), "CC-BY-SA-{version}", True),
    (re.compile(r"creativecommons\.org/licenses/by-nc-sa/([0-9.]+)"), "CC-BY-NC-SA-{version}", True),
    (re.compile(r"creativecommons\.org/licenses/by-nc/([0-9.]+)"), "CC-BY-NC-{version}", True),
    (re.compile(r"creativecommons\.org/licenses/by-nd/([0-9.]+)"), None, True),
    (re.compile(r"creativecommons\.org/licenses/by/([0-9.]+)"), "CC-BY-{version}", True),
    (re.compile(r"opensource\.org/licenses/mit"), "MIT", False),
    (re.compile(r"openfontlicense\.org|scripts\.sil\.org/ofl"), "OFL-1.1", False),
]

_TOKEN_RULES: list[tuple[frozenset[str], str]] = [
    (frozenset({"CC0", "1.0"}), "CC0-1.0"),
    (frozenset({"CC0"}), "CC0-1.0"),
    (frozenset({"CREATIVECOMMONS", "ZERO"}), "CC0-1.0"),
    (frozenset({"CREATIVECOMMONS", "ZERO", "1.0"}), "CC0-1.0"),
    (frozenset({"PUBLICDOMAIN", "ZERO"}), "CC0-1.0"),
    (frozenset({"CC", "BY", "NC", "SA", "4.0"}), "CC-BY-NC-SA-4.0"),
    (frozenset({"CC", "BY", "NC", "SA", "3.0"}), "CC-BY-NC-SA-3.0"),
    (frozenset({"CC", "BY", "NC", "4.0"}), "CC-BY-NC-4.0"),
    (frozenset({"CC", "BY", "NC", "3.0"}), "CC-BY-NC-3.0"),
    (frozenset({"CC", "BY", "SA", "4.0"}), "CC-BY-SA-4.0"),
    (frozenset({"CC", "BY", "SA", "3.0"}), "CC-BY-SA-3.0"),
    (frozenset({"CC", "BY", "4.0"}), "CC-BY-4.0"),
    (frozenset({"CC", "BY", "3.0"}), "CC-BY-3.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "SHAREALIKE", "NONCOMMERCIAL", "4.0"}), "CC-BY-NC-SA-4.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "SHAREALIKE", "NONCOMMERCIAL", "3.0"}), "CC-BY-NC-SA-3.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "NONCOMMERCIAL", "4.0"}), "CC-BY-NC-4.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "NONCOMMERCIAL", "3.0"}), "CC-BY-NC-3.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "SHAREALIKE", "4.0"}), "CC-BY-SA-4.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "SHAREALIKE", "3.0"}), "CC-BY-SA-3.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "4.0"}), "CC-BY-4.0"),
    (frozenset({"CREATIVECOMMONS", "ATTRIBUTION", "3.0"}), "CC-BY-3.0"),
    (frozenset({"MIT"}), "MIT"),
    (frozenset({"OFL", "1.1"}), "OFL-1.1"),
    (frozenset({"SIL", "OPENFONTLICENSE", "1.1"}), "OFL-1.1"),
    (frozenset({"OPENFONTLICENSE", "1.1"}), "OFL-1.1"),
]

_LICENSE_ERROR_MESSAGES = {
    LICENSE_UNKNOWN: "license identifier is unknown; no rights can be confirmed and it is never guessed to be CC0",
    LICENSE_UNSUPPORTED_VERSION: "license version is not supported; pending explicit mapping",
    LICENSE_NC: "non-commercial restriction detected; the asset cannot be used in the commercial game",
    LICENSE_PERSONAL_ONLY: "personal-use-only restriction detected",
    LICENSE_NO_REDISTRIBUTION: "redistribution is explicitly prohibited",
    LICENSE_NO_DERIVATIVES: "derivative use is explicitly prohibited",
    LICENSE_SHARE_ALIKE: "share-alike copyleft cannot be confirmed for game distribution",
    FIELD_MISSING: "field is missing or has no usable value",
    FIELD_UNCONFIRMED: "field value is present but unconfirmed (pending/not applicable)",
    FIELD_INVALID: "field value cannot be parsed",
}

_CONFLICT_FIELDS = {
    CONFLICT_ATTRIBUTION: "attribution_required",
    CONFLICT_COMMERCIAL: "commercial_use",
    CONFLICT_MODIFICATION: "modification_allowed",
    CONFLICT_REDISTRIBUTION: "redistribution",
}

_LICENSE_LOOKUP_TO_ERROR = {
    "missing": FIELD_MISSING,
    "invalid_type": FIELD_INVALID,
    "empty": FIELD_MISSING,
    "unknown": LICENSE_UNKNOWN,
    "unsupported_version": LICENSE_UNSUPPORTED_VERSION,
    "nc": LICENSE_NC,
    "personal": LICENSE_PERSONAL_ONLY,
    "no_redistribution": LICENSE_NO_REDISTRIBUTION,
    "no_derivatives": LICENSE_NO_DERIVATIVES,
}


@dataclass(frozen=True)
class LicenseProps:
    attribution_required: bool
    commercial_allowed: bool
    modification_allowed: bool
    redistribution_allowed: bool | None
    share_alike: bool = False
    note: str = ""


LICENSE_PROPS: dict[str, LicenseProps] = {
    "CC0-1.0": LicenseProps(False, True, True, True),
    "CC-BY-3.0": LicenseProps(True, True, True, True),
    "CC-BY-4.0": LicenseProps(True, True, True, True),
    "CC-BY-SA-3.0": LicenseProps(True, True, True, None, share_alike=True),
    "CC-BY-SA-4.0": LicenseProps(True, True, True, None, share_alike=True),
    "CC-BY-NC-3.0": LicenseProps(True, False, True, False),
    "CC-BY-NC-4.0": LicenseProps(True, False, True, False),
    "CC-BY-NC-SA-3.0": LicenseProps(True, False, True, False),
    "CC-BY-NC-SA-4.0": LicenseProps(True, False, True, False),
    "MIT": LicenseProps(
        True, True, True, True,
        note="MIT requires the license text to be preserved; treated as an attribution requirement",
    ),
    "OFL-1.1": LicenseProps(
        True, True, True, True,
        note="SIL OFL 1.1; reserved font names may require renaming the font before embedding",
    ),
}

SUPPORTED_LICENSES: tuple[str, ...] = tuple(sorted(LICENSE_PROPS))


def _norm(text: str) -> str:
    out = text.upper()
    out = out.replace("\u2013", " ").replace("\u2014", " ").replace("/", " ").replace("-", " ").replace(",", " ")
    return re.sub(r"\s+", " ", out).strip()


def _join_phrases(tokens: list[str]) -> list[str]:
    joined: list[str] = []
    index = 0
    while index < len(tokens):
        if index + 1 < len(tokens) and tokens[index : index + 2] == ["NON", "COMMERCIAL"]:
            joined.append("NONCOMMERCIAL")
            index += 2
            continue
        if index + 1 < len(tokens) and tokens[index : index + 2] == ["SHARE", "ALIKE"]:
            joined.append("SHAREALIKE")
            index += 2
            continue
        if index + 1 < len(tokens) and tokens[index : index + 2] == ["CREATIVE", "COMMONS"]:
            joined.append("CREATIVECOMMONS")
            index += 2
            continue
        if index + 1 < len(tokens) and tokens[index : index + 2] == ["PUBLIC", "DOMAIN"]:
            joined.append("PUBLICDOMAIN")
            index += 2
            continue
        if index + 3 < len(tokens) and tokens[index : index + 4] == ["SIL", "OPEN", "FONT", "LICENSE"]:
            joined.append("SIL")
            joined.append("OPENFONTLICENSE")
            index += 4
            continue
        if index + 2 < len(tokens) and tokens[index : index + 3] == ["OPEN", "FONT", "LICENSE"]:
            joined.append("OPENFONTLICENSE")
            index += 3
            continue
        joined.append(tokens[index])
        index += 1
    return joined


def _normalize_version(raw: str) -> str:
    return raw if "." in raw else raw + ".0"


def canonicalize_license_id(raw: Any) -> tuple[str | None, str]:
    """Return (canonical license id or None, lookup code). Deterministic, offline."""
    if not isinstance(raw, str):
        return None, "invalid_type"
    text = raw.strip()
    if not text:
        return None, "empty"
    low = text.lower()
    for pattern, canonical, versioned in _URL_RULES:
        match = pattern.search(low)
        if not match:
            continue
        if canonical is None:
            return None, "no_derivatives"
        if versioned:
            version = _normalize_version(match.group(1))
            if version not in SUPPORTED_VERSIONS:
                return None, "unsupported_version"
            return canonical.format(version=version), "ok"
        return canonical, "ok"
    norm = _norm(text)
    tokens = [token for token in _join_phrases(norm.split()) if token not in _STOPWORDS]
    token_set = frozenset(tokens)
    for pattern, canonical in _TOKEN_RULES:
        if token_set == pattern:
            return canonical, "ok"
    version_match = _CC_VERSION_RE.search(norm)
    if version_match:
        version = _normalize_version(version_match.group(1))
        if version not in SUPPORTED_VERSIONS:
            return None, "unsupported_version"
    if _NC_TEXT_RE.search(low):
        return None, "nc"
    if _PERSONAL_TEXT_RE.search(low):
        return None, "personal"
    if _NO_REDIST_RE.search(low):
        return None, "no_redistribution"
    if _NO_DERIV_RE.search(low) or ("ND" in tokens and "CC" in tokens):
        return None, "no_derivatives"
    return None, "unknown"


def _claim_text(value: Any) -> str:
    if not isinstance(value, str):
        return ""
    text = value.lower().replace("_", " ").replace("-", " ").replace("/", "")
    return re.sub(r"\s+", " ", text).strip(" .")


def parse_claim(value: Any, *, allow_restriction: bool) -> str:
    """Parse a claim value into TRUE / FALSE / NC / PERSONAL / UNCONFIRMED / INVALID."""
    if isinstance(value, bool):
        return TRUE if value else FALSE
    if isinstance(value, int) and not isinstance(value, bool) and value in (0, 1):
        return TRUE if value == 1 else FALSE
    text = _claim_text(value)
    if not text:
        return UNCONFIRMED
    if allow_restriction:
        if "personal" in text.split() or "personal use" in text or "private use" in text:
            return PERSONAL
        if (
            "noncommercial" in text
            or "non commercial" in text
            or "no commercial" in text
            or "commercial use prohibited" in text
            or text == "nc"
        ):
            return NC
    if text in _TRUE_CLAIMS:
        return TRUE
    if text in _FALSE_CLAIMS:
        return FALSE
    if text in _UNCONFIRMED_CLAIMS:
        return UNCONFIRMED
    return INVALID


def _first_value(record: dict[str, Any], keys: tuple[str, ...]) -> tuple[str | None, Any, list[str]]:
    found = [key for key in keys if key in record]
    if not found:
        return None, None, []
    warnings: list[str] = []
    if len(found) > 1:
        warnings.append(f"multiple alias fields present ({', '.join(found)}); using {found[0]}")
    return found[0], record[found[0]], warnings


def _claim(key: str | None, value: Any, *, allow_restriction: bool) -> tuple[str, str | None]:
    if key is None or value is None:
        return UNCONFIRMED, FIELD_MISSING
    claim = parse_claim(value, allow_restriction=allow_restriction)
    if claim == INVALID:
        return INVALID, FIELD_INVALID
    if claim == UNCONFIRMED:
        return UNCONFIRMED, FIELD_UNCONFIRMED
    return claim, None


def _combine_redistribution(claims: list[str]) -> tuple[str, list[str]]:
    values = list(claims)
    conflicts: list[str] = []
    if not values:
        return UNCONFIRMED, conflicts
    if TRUE in values and FALSE in values:
        conflicts.append(CONFLICT_REDISTRIBUTION)
        return FALSE, conflicts
    if UNCONFIRMED in values:
        return UNCONFIRMED, conflicts
    return (TRUE if TRUE in values else FALSE), conflicts


def normalize_license(record: Any) -> dict[str, Any]:
    """Normalize one license record. Pure and deterministic; never guesses CC0."""
    if not isinstance(record, dict):
        return {
            "record_id": "",
            "canonical_license": None,
            "attribution_required": None,
            "commercial_allowed": None,
            "modification_allowed": None,
            "redistribution_allowed": None,
            "status": PENDING,
            "status_reason": "record is not an object; cannot normalize",
            "errors": [
                {
                    "code": RECORD_NOT_OBJECT,
                    "field": "",
                    "severity": "error",
                    "message": "record must be a JSON object",
                }
            ],
            "warnings": [],
            "input": record,
        }

    warnings: list[str] = []
    record_id = record.get("record_id")
    record_id = record_id if isinstance(record_id, str) and record_id else ""
    if not record_id:
        warnings.append("record has no record_id; upstream audit traceability is reduced")
    input_snapshot = dict(record)

    license_key, license_value, alias_warnings = _first_value(record, LICENSE_KEYS)
    warnings.extend(alias_warnings)
    if license_key is None:
        license_lookup, canonical = "missing", None
    else:
        canonical, license_lookup = canonicalize_license_id(license_value)
    license_error = _LICENSE_LOOKUP_TO_ERROR.get(license_lookup, "")

    att_key, att_value, _ = _first_value(record, ("attribution_required",))
    com_key, com_value, com_alias = _first_value(record, COMMERCIAL_KEYS)
    warnings.extend(com_alias)
    mod_key, mod_value, _ = _first_value(record, ("modification_allowed",))
    red_key_values = [(key, value) for key, value in ((k, record.get(k)) for k in REDIST_KEYS) if value is not None]

    att_claim, att_field_error = _claim(att_key, att_value, allow_restriction=False)
    com_claim, com_field_error = _claim(com_key, com_value, allow_restriction=True)
    mod_claim, mod_field_error = _claim(mod_key, mod_value, allow_restriction=False)
    redist_claims = [_claim(key, value, allow_restriction=True) for key, value in red_key_values]

    errors: list[dict[str, str]] = []

    def add_error(code: str, field: str, message: str) -> None:
        errors.append({"code": code, "field": field, "severity": "error", "message": message})

    if license_error:
        field = license_key if license_key else "license_id"
        message = _LICENSE_ERROR_MESSAGES.get(license_error, "license is unusable")
        add_error(license_error, field, message)
    if att_field_error:
        add_error(att_field_error, "attribution_required", _LICENSE_ERROR_MESSAGES[att_field_error] + ": attribution_required")
    if com_field_error:
        add_error(com_field_error, com_key or "commercial_use", _LICENSE_ERROR_MESSAGES[com_field_error] + ": commercial use")
    if mod_field_error:
        add_error(mod_field_error, "modification_allowed", _LICENSE_ERROR_MESSAGES[mod_field_error] + ": modification_allowed")
    if not red_key_values:
        add_error(FIELD_MISSING, "redistribution", _LICENSE_ERROR_MESSAGES[FIELD_MISSING] + ": no redistribution field provided")
    for key, value in red_key_values:
        _, field_error = _claim(key, value, allow_restriction=True)
        if field_error:
            add_error(field_error, key, _LICENSE_ERROR_MESSAGES[field_error] + f": {key}")

    restrictions: list[str] = []
    if license_error in {
        LICENSE_NC,
        LICENSE_PERSONAL_ONLY,
        LICENSE_NO_REDISTRIBUTION,
        LICENSE_NO_DERIVATIVES,
    }:
        restrictions.append(license_error)
    for claim, _ in redist_claims:
        if claim == PERSONAL:
            restrictions.append(LICENSE_PERSONAL_ONLY)
        elif claim == NC:
            restrictions.append(LICENSE_NC)
    if com_claim == PERSONAL:
        restrictions.append(LICENSE_PERSONAL_ONLY)
    elif com_claim == NC:
        restrictions.append(LICENSE_NC)
    restrictions = sorted(set(restrictions))

    att_out: bool | None = None
    com_out: bool | None = None
    mod_out: bool | None = None
    redist_out: bool | None = None
    status_reason = ""

    if restrictions:
        status = BLOCKED
        for code in restrictions:
            if code != license_error:
                add_error(code, "", _LICENSE_ERROR_MESSAGES[code])
        if LICENSE_PERSONAL_ONLY in restrictions:
            status_reason = "personal-use-only restriction; asset cannot be used or distributed"
            att_out, com_out, mod_out, redist_out = None, False, False, False
        elif LICENSE_NC in restrictions:
            status_reason = "non-commercial restriction; asset cannot be distributed with the game"
            att_out, com_out, mod_out, redist_out = None, False, None, False
        elif LICENSE_NO_REDISTRIBUTION in restrictions:
            status_reason = "redistribution is explicitly prohibited"
            com_out, redist_out = None, False
        else:
            status_reason = "derivative use prohibited; modification is not allowed"
            mod_out = False
    elif canonical is None:
        status = PENDING
        if license_error == LICENSE_UNKNOWN:
            status_reason = "license unknown; no rights can be confirmed and it is never guessed to be CC0"
        elif license_error == LICENSE_UNSUPPORTED_VERSION:
            status_reason = "license version is not supported; pending explicit mapping"
        else:
            status_reason = "license is unusable; normalization incomplete"
    else:
        props = LICENSE_PROPS[canonical]
        field_blockers = [
            field_error
            for _, field_error in ((att_claim, att_field_error), (com_claim, com_field_error), (mod_claim, mod_field_error))
            if field_error is not None
        ]
        field_blockers.extend(field_error for _, field_error in redist_claims if field_error is not None)
        if not red_key_values:
            field_blockers.append(FIELD_MISSING)
        if field_blockers:
            status = PENDING
            status_reason = "required fields missing or unconfirmed; normalization incomplete"
            att_out = True if att_claim == TRUE else (False if att_claim == FALSE else None)
            com_out = True if com_claim == TRUE else (False if com_claim == FALSE else None)
            mod_out = True if mod_claim == TRUE else (False if mod_claim == FALSE else None)
            redist_claim, redist_conflicts = _combine_redistribution([claim for claim, _ in redist_claims])
            redist_out = True if redist_claim == TRUE else (False if redist_claim == FALSE else None)
            for conflict in redist_conflicts:
                add_error(conflict, _CONFLICT_FIELDS[conflict], f"provided redistribution fields contradict each other: {conflict}")
        else:
            conflict_codes: list[str] = []
            att_out = att_claim == TRUE
            if att_out != props.attribution_required:
                att_out = True
                conflict_codes.append(CONFLICT_ATTRIBUTION)
            com_out = com_claim == TRUE
            if com_out != props.commercial_allowed:
                com_out = False
                conflict_codes.append(CONFLICT_COMMERCIAL)
            mod_out = mod_claim == TRUE
            if mod_out != props.modification_allowed:
                mod_out = False
                conflict_codes.append(CONFLICT_MODIFICATION)
            redist_claim, redist_conflicts = _combine_redistribution([claim for claim, _ in redist_claims])
            conflict_codes.extend(redist_conflicts)
            if props.share_alike:
                redist_out = None
                license_blocks = [LICENSE_SHARE_ALIKE]
            else:
                redist_out = redist_claim == TRUE
                if redist_out != (props.redistribution_allowed is True):
                    redist_out = False
                    conflict_codes.append(CONFLICT_REDISTRIBUTION)
                license_blocks = [LICENSE_NC] if not props.commercial_allowed else []
            if props.note:
                warnings.append(props.note)
            for conflict in sorted(set(conflict_codes)):
                add_error(conflict, _CONFLICT_FIELDS[conflict], f"claim contradicts the canonical license {canonical}: {conflict}")
            for block in license_blocks:
                add_error(block, "", _LICENSE_ERROR_MESSAGES[block])
            if license_blocks or conflict_codes:
                status = BLOCKED
                status_reason = "license-level restriction or claim conflict; conservative values returned"
            else:
                status = NORMALIZED
                status_reason = "license normalized; no restriction relaxed"

    errors.sort(key=lambda item: (item["code"], item["field"]))
    warnings = sorted(set(warnings))
    return {
        "record_id": record_id,
        "canonical_license": canonical,
        "attribution_required": att_out,
        "commercial_allowed": com_out,
        "modification_allowed": mod_out,
        "redistribution_allowed": redist_out,
        "status": status,
        "status_reason": status_reason,
        "errors": errors,
        "warnings": warnings,
        "input": input_snapshot,
    }


def normalize_license_batch(records: list[Any]) -> dict[str, Any]:
    """Normalize a list of records; deterministic per-record results plus a summary."""
    results = [normalize_license(record) for record in records]
    counts = Counter(result["status"] for result in results)
    return {
        "tool": TOOL,
        "offline": True,
        "mode": "batch",
        "record_count": len(results),
        "summary": {
            "total": len(results),
            NORMALIZED: counts[NORMALIZED],
            BLOCKED: counts[BLOCKED],
            PENDING: counts[PENDING],
        },
        "records": results,
    }


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, default=None, help="JSON file with one record or an array of records (default: stdin)")
    parser.add_argument("--output", type=Path, default=None, help="write the normalized JSON to this path")
    parser.add_argument("--strict", action="store_true", help="exit 1 when any record is not NORMALIZED")
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    try:
        if args.input is None:
            data = json.loads(sys.stdin.buffer.read().decode("utf-8-sig"))
        else:
            data = json.loads(args.input.read_bytes().decode("utf-8-sig"))
    except (OSError, UnicodeDecodeError, json.JSONDecodeError) as exc:
        print(f"{TOOL}: cannot read input: {exc}", file=sys.stderr)
        return 2
    if isinstance(data, list):
        result = normalize_license_batch(data)
    else:
        result = normalize_license(data)
    rendered = json.dumps(result, ensure_ascii=False, indent=2) + "\n"
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(rendered, end="")
    if args.strict:
        records = result["records"] if isinstance(result, dict) and "records" in result else [result]
        if any(record["status"] != NORMALIZED for record in records):
            return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
