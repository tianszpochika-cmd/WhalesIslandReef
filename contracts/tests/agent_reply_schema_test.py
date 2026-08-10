import json
from pathlib import Path

from jsonschema import Draft7Validator


ROOT = Path(__file__).resolve().parents[2]
SCHEMA = json.loads((ROOT / "contracts/agent-reply.schema.json").read_text(encoding="utf-8"))
REGISTRY = json.loads((ROOT / "contracts/p2a-action-registry.json").read_text(encoding="utf-8"))
VALIDATOR = Draft7Validator(SCHEMA)


def load_fixture(name: str):
    return json.loads((ROOT / "contracts/test-fixtures/agent-reply" / name).read_text(encoding="utf-8"))


def assert_valid(value):
    errors = sorted(VALIDATOR.iter_errors(value), key=lambda error: list(error.path))
    assert not errors, "\n".join(error.message for error in errors)


def assert_invalid(value):
    assert list(VALIDATOR.iter_errors(value)), value


def test_valid_fixture_and_registry_shape():
    assert_valid(load_fixture("valid.json"))
    assert REGISTRY["registry_version"] == "p2a.v1"
    actions = REGISTRY["actions"]
    assert len(actions) == 60
    assert len({action["name"] for action in actions}) == 60
    assert all({"name", "parameter_schema", "permissions", "prerequisites", "failure_codes", "executor"}.issubset(action) for action in actions)
    schema_actions = set(SCHEMA["properties"]["action_intent"]["properties"]["name"]["enum"]) - {"none"}
    assert schema_actions == {action["name"] for action in actions}


def test_required_and_enum_boundaries():
    assert_invalid(load_fixture("missing-speech.json"))
    assert_invalid(load_fixture("invalid-emotion.json"))
    assert_invalid(load_fixture("affinity-out-of-range.json"))
    assert_invalid(load_fixture("unknown-action.json"))


def test_unknown_fields_and_forbidden_results_are_not_contract_fields():
    value = load_fixture("valid.json")
    value["money"] = 10
    assert_invalid(value)

    value = load_fixture("valid.json")
    value["action_intent"]["parameters"]["weather"] = "storm"
    assert_invalid(value)


if __name__ == "__main__":
    for test in (test_valid_fixture_and_registry_shape, test_required_and_enum_boundaries, test_unknown_fields_and_forbidden_results_are_not_contract_fields):
        test()
    print("3 contract schema tests passed")
