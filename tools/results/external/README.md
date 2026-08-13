# D16 external evidence directory

Only real target-device or human-review evidence may be placed here.

Required envelopes are listed in:
`docs/game/design/common/D16_EXTERNAL_ACCEPTANCE_RUNBOOK.md`

Do not add synthetic/editor-only samples. The absence of the six JSON files is
currently intentional and keeps the release audit blocked rather than
claiming external acceptance without evidence.

Each envelope's `raw_artifacts` entries must point to files that actually exist;
the audit does not treat placeholder filenames as evidence and does not create
or copy artifacts automatically.
Relative paths must stay inside this evidence directory; `..` traversal is
rejected.
