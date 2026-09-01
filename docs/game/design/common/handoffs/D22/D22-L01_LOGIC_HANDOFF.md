# D22-L01 Logic Handoff

| Field | Value |
|---|---|
| Task | `D22-L01` |
| Status | `LOGIC_PASS` for the boundary freeze only |
| Date | 2026-09-01 |
| Baseline | `docs/game/design/common/D22_LOGIC_BOUNDARY_BASELINE.md` |
| Result | `game/tools/results/D22/D22-L01/D22_L01_LOGIC_RESULT.json` |

## 1. Handoff outcome

D22-L01 has frozen the pure UI logic boundary. The freeze records current runtime panel sources and prevents them from becoming accidental domain owners. It defines the one-way path `Runtime input intent -> D22 logic -> public domain owner -> owner result/read model -> D22 projection -> Runtime view`.

This is a logic/documentation handoff. It is not evidence of Canvas correctness, actual clickability, focus on a device, PlayMode playability, visual quality or release readiness.

## 2. Boundary that downstream work must preserve

- D22 owns logical screen/panel IDs, modal stack, focus tokens, panel exclusivity, command envelope validation, stable feedback/disabled reasons, notification semantics and immutable UI projections.
- Domain systems remain the unique writers for money, inventory, port, ship, voyage, character, relations, environment, ecology, animals, quests, rewards, finance, property and save facts.
- D22 may call a public typed owner endpoint and consume its result/read model. It may not use reflection, private fields, static fact copies, mutable state passed through a UI helper, Canvas state, hardcoded success text or a runtime component as a business result.
- Pause/escape/pointer behavior is an intent/read-model boundary. D22 does not read `Keyboard.current`/`Gamepad.current`, access `EventSystem`, change cursor state, or write `Time.timeScale`.
- `D16ModalFocusScope`, `D16InputRouter`, `HarborPauseController`, `RuntimeHudCoordinator`, `MainMenuController` and the D16 panels remain Runtime/presentation sources during this task. Do not reinterpret their UGUI behavior as the pure D22 focus state machine.
- `D16AccessibilitySettingsStore` remains the presentation-preference owner. `D16FinanceDebtPanel` remains a read-only presentation of D13-owned finance state. `D2DialogueController` is not a D22 fact owner.

## 3. Evidence handed to the Coordinator

### Current UI sources

- `MainMenuController` dynamically builds the main menu and directly reaches SaveSystem/GameManager/settings; it is a legacy runtime adapter, not a D22 command router.
- `HarborPauseController` directly handles runtime input, `Time.timeScale`, cursor/EventSystem and scene/save actions; only its logical intent semantics may be carried into D22.
- `RuntimeHudCoordinator` and `D19RouteHud*` manage runtime Canvas suppression and status text; D22 must use accepted D19 read models/results instead of GameObject lookup or HUD text.
- D16 accessibility, diagnostics, finance, sensitive-warning and subtitle components are independent runtime canvases. Their presentation responsibilities are recorded in the baseline without adding a second owner.
- `D16InputRouter`/`D16ModalFocusScope` read device/UGUI state; they are explicitly outside the pure logic boundary.

### Upstream pure logic

- `D19-L48` is the accepted upstream port logic gate. Use `D19PortCommandRouter.Route(D19RoutedPortCommand)` and `D19PortReadModel`; do not bypass them.
- `D20-L01` is the accepted D20 boundary direction. A concrete D20 adapter still requires the relevant accepted public projection/command contract.
- No `D21` baseline, logic result, handoff or logic source exists in the current checkout. This is an explicit dependency block, not an invitation to invent a ship API.

## 4. Blocked dependencies

| Downstream scope | Status | Required unblock |
|---|---|---|
| `D22-L19` ship/voyage adapter | `LOGIC_BLOCKED` | Complete D21-L01 and its logic acceptance; publish a typed ship/voyage command/result/read-model boundary. |
| `D22-L17`, `D22-L20`--`D22-L23` | Dependency gate | Confirm each owner’s accepted public contract and implement a defensive read-only adapter; raw runtime UI state is insufficient. |
| `D22-L26` router / `D22-L27` composer | Conditional block | Preserve an explicit `LOGIC_BLOCKED` result for any missing adapter; no private-field or static fallback. |

## 5. Runtime Deferred handoff

The following remain deferred and must not be reported as L01 logic failures:

- Canvas/UGUI layering, duplicate EventSystem, raycast order, visual overlap and legacy HUD isolation.
- Real keyboard/mouse/gamepad input, pointer/cursor state, UGUI selection repair and device-specific focus.
- Actual button clicks, responsive layout, resolution/scaling, typography, localization rendering, contrast, animation, audio and platform behavior.
- Player/PlayMode, scene/Prefab/Transform, performance and release checks.
- Migration from the existing runtime panel sources to D22 adapters.

Historical D19/D20 A/R/S and screenshot/Player/PlayMode records remain historical or Runtime Deferred evidence. They do not upgrade this logic handoff to runtime acceptance.

## 6. Next-agent instructions

1. Start `D22-L02`--`D22-L25` from the logical IDs and one-writer matrix in the baseline.
2. Keep `D22-L18` on the D19 public Router/ReadModel path.
3. Keep `D22-L19` explicitly `LOGIC_BLOCKED` until the D21 public interface is actually present and accepted.
4. Implement the L26-L32 serial chain only after the parallel contracts are stable; blocked adapters must remain visible in router/composer results.
5. Leave Canvas, EventSystem, InputSystem/device, Player, PlayMode, scene/prefab, status overview and shared memory changes to their authorized tasks.

## 7. Coordinator memory entry

```text
2026-09-01 D22-L01：已冻结 UI 纯逻辑边界与现有面板来源；D22 只拥有 focus/modal/exclusivity/command validation/feedback/notification/read-model projection，不拥有业务事实。D19-L48 与 D20-L01 是上游纯逻辑方向；当前 checkout 未发现 D21-L01 或 D21 ship public interface，D22-L19 保持 LOGIC_BLOCKED。Canvas/EventSystem/Input/device/Player/PlayMode/视觉/性能/发布全部 Runtime Deferred。委托范围内未修改共享状态总览与 .workbuddy/memory/2026-09-01.md，未执行 status sync。
```
