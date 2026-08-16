# D19-R02-A03 Main Menu / D3 Main Route Repair

## Status

**PARTIAL (2026-08-15).** This task adds a D3-owned route feedback snapshot and public request wrappers around the existing D3 runtime/bootstrap. The route is still system-owned: trade, voyage events, settlement and save facts remain in their existing systems and Save API. No HUD coordinator, D2 controller, scene, registry, contract, or save schema was changed.

## Implemented boundary

- `D3RouteFeedback` is a serializable read snapshot for UI/tests. It carries a code, localized display message, voyage status, location, save revision and monotonic presentation sequence.
- `D3RuntimeBootstrap` exposes `RequestTrade`, `RequestDeparture`, `RequestAdvanceVoyage`, `RequestLandAndSettle`, `RequestSave`, `ReadRouteFeedback` and `CloseTradeFromCoordinator` as presentation-facing entry points. These call existing D3-owned operations; they do not write economic or world facts themselves.
- D3 input fallback remains an intent bridge. It does not replace the D2 input owner or create a second input authority.
- Save success is published only after the existing `SaveSystem`/D3 snapshot operation completes.

## Real Player evidence

The fresh Player run visibly went through Main Menu → New Game → InitialHarbor → trade panel → departure → voyage event → arrival/settlement → save → relaunch → Continue/load. The Player log records the system-owned route and save events. Mouse close recovered the trade panel after window activation. The route evidence is in `game/tools/results/D19/D19-R02-A03/D19_R02_A03_ROUTE_LOG.json` and the screenshots beside it.

The run is not a full PASS: W movement did not produce reliable visible displacement, Escape did not close the trade panel in the captured attempt, and the captured window was 2048×1152 while an independently verified 1920×1080 back buffer was not available. A03 does not claim the 5–10 minute final route or three-quality performance gate; those remain D19-R02-S02/A04/A05 responsibilities.

## Test evidence

- Unity compile: `game/tools/results/D19/D19-R02-A03/D19_R02_A03_UNITY_COMPILE.log` — no new C# compile error observed.
- EditMode: 3/3 passed, `D19_R02_A03_EDITMODE_TESTS.xml`.
- PlayMode rerun: 2/2 passed, `D19_R02_A03_PLAYMODE_TESTS_RERUN.xml`.
- Player build: `D19_R02_A03_Player.exe`, build log reports success.
- `git diff --check`: exit code 0; line-ending warnings only.

## Handoff risks

1. D19-R02-A01's HUD manager can cache a null D2 router when D2 bootstraps later; this is an integration-order issue and was not changed here.
2. Player movement/focus needs a later route-level repro with a measurable position delta.
3. The final D19 gate must collect exact 1920×1080 Player evidence and performance samples; this A03 result is insufficient by itself.

## RERUN3 Escape compatibility recheck (2026-08-15)

The D3 trade modal now uses the same existing `WasD3Input` compatibility path as the other D3 default actions for `D3CloseTrade`, with `KeyCode.Escape` as the legacy fallback. This remains a D3 presentation intent bridge; `CloseTrade()` remains the D3 owner of the trade-panel state and no HUD, D2, SaveSystem, or domain logic was added.

In a real visible Player, Main Menu → New Game → InitialHarbor → B trade succeeded, and a real Escape event closed the trade panel and returned the harbor view. A03 EditMode 3/3 and PlayMode 2/2 passed after the change. The focused rerun remains **PARTIAL** because it does not re-run the full 5–10 minute save/voyage route or prove the final 1920×1080/performance/visual gates.

Evidence: `game/tools/results/D19/D19-R02-A03/D19_R02_A03_RERUN3_MACHINE_RESULT.json`, `D19_R02_A03_ROUTE_LOG_RERUN3.json`, `20_rerun3_menu.jpg` through `24_rerun3_after_escape.jpg`.
