# D22 UI Logic Boundary Baseline

| Field | Value |
|---|---|
| Task | `D22-L01` |
| Chapter | D22 UI commands, focus, read models and feedback |
| Baseline date | 2026-09-01 |
| Baseline status | `LOGIC_PASS` for the L01 freeze only |
| Scope | Pure logic boundary, ownership, public-interface direction and Runtime Deferred register |
| Explicit non-scope | Canvas, EventSystem, UGUI, InputSystem/device reads, scene/prefab/Transform, Player/PlayMode, screenshots, performance, audio and build settings |

## 1. Baseline decision

D22-L01 freezes the logic boundary for the UI layer. It does not claim that the current runtime UI is playable, visually correct, clickable, localized, or release-ready. It also does not create a runtime UI implementation.

The frozen direction is:

```text
Runtime input provider
    -> logical input intent
    -> D22 focus/exclusivity/validation logic
    -> public command endpoint owned by the domain system
    -> owner result / fact event / owner read model
    -> D22 feedback, notification and composed read model
    -> Runtime Canvas adapter
```

The reverse direction is forbidden: Canvas state, button animation, text, color, EventSystem selection, cursor state, or a runtime component must never become a domain fact or bypass its owner.

`D22-L01` itself is documentation-only. No D22 runtime code, scene, prefab, UI component, Player code, or status overview is changed by this baseline.

## 2. Current implementation evidence and its treatment

The following files are real current-checkout evidence. They are used to freeze the boundary, not to promote the legacy runtime path to D22 logic acceptance.

| Current source | Observed responsibility | D22 treatment |
|---|---|---|
| `game/Assets/Scripts/Gameplay/UI/MainMenuController.cs:83-646` | Dynamically creates the main-menu Canvas, EventSystem and four panels; directly calls `SaveSystem`, `GameManager`, `SettingsStore`, `Localization`, and `Application.Quit`. | Preserve as a runtime source during this baseline. New-game/load/settings/exit become D22 intents and owner commands in later tasks; no direct UI fact write is accepted. |
| `game/Assets/Scripts/Gameplay/UI/HarborPauseController.cs:59-219` | Reads `D16InputRouter`, writes `Time.timeScale`, cursor and EventSystem state, creates a pause Canvas, and calls save/scene APIs. | Runtime implementation only. D22 owns pause/escape intent semantics and a pause read model; it never reads keyboard/device state or writes `Time.timeScale`. |
| `game/Assets/Scripts/Gameplay/UI/RuntimeHudCoordinator.cs:19-151` | Hides legacy Canvas objects by name and activates the D19 route HUD through `CanvasGroup`, `Canvas.enabled` and `GameObject` lookup. | Runtime HUD isolation evidence, not a pure read model or UI command owner. |
| `game/Assets/Scripts/Gameplay/UI/D19RouteHud/D19RouteHud.cs:23-102` and `D19RouteHudManager.cs:35-110` | Creates the route HUD, reads legacy runtime objects and emits hardcoded status/feedback strings. | Existing presentation source only. D22 must consume `D19PortReadModel`/owner results through a typed adapter; it must not use `GameObject.Find` or hardcoded display text as logic. |
| `game/Assets/Scripts/Gameplay/UI/D16/D16InputRouter.cs:15-315` | Reads `Keyboard.current`, `Gamepad.current`, `Time.frameCount` and repairs UGUI selection through `D16ModalFocusScope`. | Device and UGUI focus remain Runtime-owned. D22 focus is a pure state machine and receives normalized intents from an adapter. |
| `game/Assets/Scripts/Gameplay/UI/D16/D16AccessibilityPanel.cs:75-394`, `D16LocalDiagnosticsPanel.cs:98-203`, `D16FinanceDebtPanel.cs:42-227`, `D16SensitiveContentOverlay.cs:42-245`, `D16SubtitleOverlay.cs:34-169` | Independent runtime canvases for presentation preferences, diagnostics consent, finance read-only display, sensitive-content warnings and subtitles. | These are panel sources and presentation adapters. They do not define D22 domain ownership. `D16AccessibilitySettingsStore` remains the owner of presentation preferences; Finance remains D13-owned. |
| `game/Assets/Scripts/Gameplay/UI/Dialogue/D2DialogueController.cs:29-304` | Runtime dialogue/LLM presentation bridge; emits an agent reply event and displays proposal text. | Not a D22 fact owner. Any affinity, relation, crew or quest consequence must go through the corresponding validated domain owner/public contract. |
| `game/Assets/Scripts/Systems/D19/D19PortCommandRouter.cs:162-` and `D19PortReadModel.cs:541-628` | Pure public port command route and pure port read-model construction. | Accepted upstream logic boundary for D22 port adapter work, subject to the D19-L48 acceptance record. |

The legacy sources contain direct runtime coupling such as `Time.timeScale`, Canvas creation, duplicate EventSystem mitigation, `GameObject.Find`, `PlayerPrefs`, cursor changes and device reads. Those observations are boundary evidence and Runtime Deferred items; they are not copied into D22 pure logic.

## 3. Frozen panel/source registry

D22 stable IDs are logical identifiers. A Canvas name, GameObject name, prefab path, sorting order or component type is not a D22 identifier.

| Logical ID | Existing runtime source | Logical responsibility frozen for D22 |
|---|---|---|
| `screen.main_menu` | `MainMenuController` | Navigate among new game, continue, settings and about; issue intents only. |
| `screen.pause` | `HarborPauseController` | Expose pause/resume/main-menu/quit intents and the pause read state. |
| `screen.route_hud` | `RuntimeHudCoordinator` + `D19RouteHud*` | Compose non-authoritative voyage/port feedback from accepted read models. |
| `modal.accessibility` | `D16AccessibilityPanel` | Edit presentation accessibility settings through the D16 preference owner. |
| `modal.finance_debt` | `D16FinanceDebtPanel` | Display a D13 finance read model; no finance operation is implied by display. |
| `modal.local_diagnostics` | `D16LocalDiagnosticsPanel` | Display/submit local diagnostics-consent intent to its owner; no domain fact write. |
| `modal.sensitive_warning` | `D16SensitiveContentOverlay` | Display a caller-supplied warning and return a decision intent. |
| `overlay.subtitle` | `D16SubtitleOverlay` | Display localized subtitle events according to presentation preferences. |
| `screen.dialogue` | `D2DialogueController` | Display validated agent replies; route consequential actions to domain owners. |

The registry records the existing source so later runtime work does not silently create a second owner. It does not require the legacy components to be reused by pure D22 logic.

## 4. Ownership and write authority

The domain system that owns a fact remains the only writer. D22 can validate a UI intent, call a public owner endpoint, consume the owner result, and project a read model. D22 cannot decide or write the fact itself.

| Fact or operation | Canonical owner direction | D22 may do | D22 may not do |
|---|---|---|---|
| Money, prices, market stock, tax, wages, profit | Economy/Market (`D3` public economy endpoints) | Request a typed market command; display owner quote/result/read model. | Change money/price/stock in a UI handler or copy a mutable economy state. |
| Inventory, cargo, weight and personal items | Inventory/Cargo (`D3` inventory owner) | Request add/remove/use through a public owner contract; display the result. | Call a helper with bypassed state ownership or edit cargo in a panel. |
| Port visit, berth, service and departure state | D19 port systems and `D19PortCommandRouter` | Route `D19RoutedPortCommand`; consume `D19PortReadModel`. | Mutate visit/berth/availability fields or infer success from a button/canvas state. |
| Ship hull, voyage, rooms, roles and damage | Ship/Voyage owner; D21 public contract is required for the planned ship adapter | Consume an accepted ship read model and owner result once available. | Invent a D21 interface, access private state, or let a HUD write ship facts. |
| Character identity, crew duties, behavior and relations | D10/D20 accepted owner/projection contracts | Send validated requests and consume immutable projections. | Treat dialogue text, avatar state or focus state as identity/affinity facts. |
| Weather, tide, current, water and world time | Environment (`D9`) | Display the accepted environment read model. | Advance time, set weather, or change tide from UI presentation logic. |
| Ecology, fishing, animals and pets | Ecology/Animal owners (`D4`/`D11`) | Request owner commands and display results. | Grant harvest, animal state, danger resolution or collection facts locally. |
| Quest/event facts and journal | Quest/Event (`D12`) | Display quest read models and send accepted quest intents. | Accept/complete/alter quests from panel state alone. |
| Rewards and collection grants | Reward/Collection (`D14`) | Display grant/claim result and audit projection. | Grant, recover or claim a reward by changing UI state. |
| Finance, debt, insurance, property and claims | Finance/Property (`D13`) | Consume finance/property read models and route owner commands. | Calculate/write balances, debt, ownership, claims or bankruptcy in UI. |
| Save/load snapshots and revisions | `SaveSystem` root/composer plus each domain snapshot owner | Issue save/load intent and consume public result/projection. | Write domain snapshots, PlayerPrefs, static mirrors or private fields as a shortcut. |
| UI focus, panel state, feedback and presentation notifications | D22 | Own pure UI state and presentation read models. | Become a writer for any domain fact. |
| Presentation accessibility preferences | `D16AccessibilitySettingsStore` | Project/validate presentation preference state. | Treat preference storage as a domain save or use it to bypass command validation. |

The owner result is authoritative. A visual “success” state, a disabled button, an animation, or a locally optimistic value is never an accepted business result.

## 5. D22 pure-logic responsibilities

The following responsibilities are frozen for the later D22 tasks. They are contracts and ownership rules, not an assertion that the implementation already exists.

### 5.1 Screen, panel and modal contracts (`D22-L02`)

Screen/panel contracts use stable logical IDs, allowed lifecycle transitions, close policy, modal kind, focus layer and localization keys. They do not contain `MonoBehaviour`, `Canvas`, `GameObject`, `Selectable`, scene, prefab or Transform references.

### 5.2 Focus state machine (`D22-L03`/`D22-L04`)

D22 owns the logical focus token, active screen, modal stack, return-focus token, deterministic fallback and exclusivity rules. The minimal state is conceptually:

```text
active_screen_id
active_panel_id
modal_stack[]
focused_control_id
return_focus_token
input_mode_intent
```

Opening a modal pushes the current logical focus token; the top modal is the only command target. Closing pops and restores the token when it is still valid, otherwise uses the panel's deterministic fallback control. Full-screen navigation panels are mutually exclusive. A blocking modal cannot coexist with another blocking modal unless the contract explicitly declares a child-modal relationship. Non-modal subtitle/notification projections may coexist but cannot receive domain commands by visual priority.

`D16ModalFocusScope` remains a Runtime UGUI selection repairer. It is not the D22 focus state machine. D22 must never call `EventSystem.current`, `SetSelectedGameObject`, `Selectable`, `LateUpdate` or any device API.

### 5.3 Command contracts, validation and owner routing (`D22-L05`/`D22-L06`/`D22-L26`)

Every UI command uses a deterministic envelope with the following semantic fields:

```text
command_id
correlation_id
actor_id
screen_id / panel_id
target_id
world_day
save_revision
parameters
```

The D22 validator checks shape, required context, focus/exclusivity permission and local command preconditions. It does not duplicate owner business rules. The router calls only a public typed owner endpoint. It returns an explicit owner result or an explicit `LOGIC_BLOCKED` dependency result. It must not use reflection, private fields, static copies, `GameObject.Find`, a fixed token, a mock fact, or a direct mutable-state write.

### 5.4 Feedback, disabled reasons, transitions and localization (`D22-L07`--`D22-L09`, `D22-L15`)

Feedback is a stable record: `reason_code`, `loc_key`, severity, recoverability/retry policy, source owner and correlation ID. Disabled state is derived from the same typed reason; it is not inferred from color or interactability. State transitions are explicit and deterministic.

User-visible text is resolved at the presentation edge from a localization key. Existing fallback/hardcoded text such as `MainMenuController` creation/load failures and `D19RouteHud*` default/status messages is an audit item, not a completed D22 catalog. Missing keys remain an auditable failure; silently using the reason code as final copy is not localization completion.

### 5.5 Pause, escape, pointer mode and accessibility (`D22-L10`--`D22-L12`, `D22-L14`)

Pause, escape and pointer behavior are intents plus read-model state. D22 does not write `Time.timeScale`, `Cursor.visible`, `Cursor.lockState`, EventSystem state, keyboard/gamepad/mouse state, or an input module. A Runtime provider normalizes device events into logical intents.

Pointer mode is a read-model request (`default`, `free`, `locked`, or equivalent contract value) consumed by Runtime. Accessibility is a pure projection/validation boundary over D16 presentation settings; actual PlayerPrefs, fonts, Canvas changes, motion and device binding capture remain Runtime/store responsibilities.

### 5.6 Notifications and save projection (`D22-L13`, `D22-L16`, `D22-L23`)

The notification queue owns presentation ordering, priority, de-duplication, acknowledgement and expiry semantics. It does not own the domain event or grant. D22 save data is limited to its declared UI projection (for example logical focus/modal/notification state if later accepted); `SaveSystem` and domain owners remain responsible for business snapshots, revisions and migration.

## 6. Read-model direction and public-interface audit

The one-way read direction is:

```text
domain owner snapshot/result
    -> typed D22 adapter
    -> immutable defensive D22 read model
    -> composer / feedback catalog / notification projection
    -> Runtime view adapter
```

Adapters must copy only the accepted public DTO/read model needed for display. They must not expose a mutable owner instance, retain a private reference, reconstruct facts from labels, or create a second source of truth.

### 6.1 Accepted upstream evidence

- `D19-L48` has an existing `LOGIC_PASS` result at `game/tools/results/D19/D19-L48/D19_L48_LOGIC_RESULT.json` and handoff at `docs/game/design/common/handoffs/D19/D19-L48_LOGIC_HANDOFF.md`. The public route is `D19PortCommandRouter.Route(D19RoutedPortCommand)` and the public pure read root is `D19PortReadModel`.
- `D20-L01` has an existing baseline/result pair at `docs/game/design/common/D20_LOGIC_BOUNDARY_BASELINE.md` and `game/tools/results/D20/D20-L01/D20_L01_LOGIC_RESULT.json`. D22 may follow its public projection direction, but must wait for the relevant D20 task acceptance before depending on a concrete adapter interface.
- No `D21` baseline, result, handoff or logic source was found in the current checkout. The absence is recorded below as `LOGIC_BLOCKED`; no D21 API is invented here.

### 6.2 Downstream adapter gates

| D22 task | Adapter direction | Current gate |
|---|---|---|
| `D22-L17` | Market/economy owner -> market read model/command result | Use public D3 owner endpoints only; adapter contract and owner acceptance still required. |
| `D22-L18` | D19 port router/read model -> port panel/HUD | `D19-L48` is the upstream logic gate; use the public router/read model only. |
| `D22-L19` | Ship/voyage owner -> ship read model/command result | `LOGIC_BLOCKED`: D21-L01 and a public accepted ship interface are absent from the current checkout. |
| `D22-L20` | Inventory/cargo owner -> inventory read model/command result | Public D3 helpers exist, but the D22 adapter must preserve D3 ownership and must not mutate passed state from UI. |
| `D22-L21` | NPC/character/crew owner -> identity/behavior/social read models | Wait for the relevant accepted D10/D20 public projection and command contracts; dialogue presentation is not a substitute. |
| `D22-L22` | Quest owner -> quest/journal read model/command result | Use D12 public reads/commands after adapter contract validation; no UI-side quest facts. |
| `D22-L23` | Save root/domain projections -> UI save/load result | Call public save/load boundary; do not write snapshots or preferences as domain state. |

For every missing or not-yet-accepted public interface, the affected route is `LOGIC_BLOCKED`. The implementation must surface that state in the result/read model; it must not silently fall back to a private field or a copied static value.

## 7. Existing source vs. D22 ownership matrix

| Runtime concern | Current source behavior | Frozen D22 rule |
|---|---|---|
| Canvas creation/layering | Multiple current controllers dynamically create independent canvases and sorting orders. | Runtime-only. D22 identifies logical layers and emits state; it does not create or order canvases. |
| EventSystem and selectable focus | Stabilizer/InputRouter disable duplicate systems and repair selection. | Runtime-only. D22 owns focus state, not selectable focus. |
| Button click | Current buttons often invoke SaveSystem/GameManager/settings directly. | A click adapter emits a D22 command/intent; the owner result controls feedback and state. |
| Pause/escape | Current pause controller reads device state and writes `Time.timeScale`. | Runtime emits `PauseIntent`/`EscapeIntent`; D22 resolves logical transition; Runtime applies presentation/game-loop behavior under its owner. |
| HUD status/feedback | Current route HUD reads legacy object state and hardcoded strings. | Compose from accepted owner read models/results and localization keys. |
| Accessibility/preferences | D16 store owns presentation settings; panel writes PlayerPrefs through that store. | Keep presentation preference ownership in D16; expose a pure D22 projection, never a domain snapshot. |
| Dialogue/agent text | D2 controller calls LLM clients and presents validated replies. | D22 displays reply/read state; consequential facts require a domain owner and validated contract. |

## 8. Runtime Deferred register

All items below are intentionally outside D22-L01 logic acceptance. A Runtime Deferred item is not a logic failure, and it cannot be used as evidence for `LOGIC_PASS`.

| ID | Deferred item | Current evidence/boundary |
|---|---|---|
| `D22-RT-01` | Legacy Canvas stacking, duplicate EventSystem and old HUD isolation | `HarborInteractionStabilizer`, `RuntimeHudCoordinator`, `HarborPauseController` and the historical D19 runtime records. |
| `D22-RT-02` | Actual mouse/keyboard/gamepad input, pointer lock, cursor and UGUI selection | `D16InputRouter`, `D16ModalFocusScope` and `HarborPauseController`; requires Runtime/device validation. |
| `D22-RT-03` | Visible clickability, raycast ordering, focus repair and button interaction | Current runtime uses `GraphicRaycaster`, `CanvasGroup`, automatic navigation and runtime-created controls; no pure logic conclusion follows. |
| `D22-RT-04` | Canvas layout, resolution/scaling, text clipping, contrast, animation and responsive presentation | Current dynamic UI sources and D16 panels; requires visual/PlayMode/device evidence. |
| `D22-RT-05` | Scene/Prefab/Transform/player movement and PlayMode route coverage | Explicitly outside the pure logic protocol. Historical D19/D20 A/R/S artifacts remain historical/runtime evidence only. |
| `D22-RT-06` | Audio, subtitle timing, notification presentation, performance and platform/device behavior | Runtime adapters and hardware/device checks are not part of L01. |
| `D22-RT-07` | Migration of legacy panel sources to D22 adapters | No runtime component is changed by L01; later implementation must preserve the registry and one-writer rules. |

Historical D19/D20 visual, Player, PlayMode, Canvas and input records are not re-used as D22 logic proof. The existing D19/D20 logic results are referenced only for their explicitly pure contracts and acceptance records.

## 9. D22 task-wave gate

The chapter order is frozen as follows:

1. `D22-L01` freezes this boundary.
2. `D22-L02`--`D22-L25` may proceed in parallel after L01, but each owner adapter must wait for its corresponding upstream `LOGIC_ACCEPTANCE`; the missing D21 ship interface blocks only the affected ship route.
3. `D22-L26`--`D22-L32` are serial: router, composer, scenarios, fuzz, save/load, static audit and interface documentation. A blocked adapter remains explicit in router/composer results.
4. `D22-L33`--`D22-L36` are the convergence gate: runner, Runtime Deferred register, status review and acceptance.

`D22-L01` does not imply D22 chapter completion, runtime approval, playable approval or release approval. The shared logic status overview and daily memory log are intentionally outside this delegated task's write scope.

## 10. L01 acceptance checklist

- [x] Current panel sources and their real runtime responsibilities are recorded.
- [x] UI logic, domain fact ownership and command/read-model direction are separated.
- [x] Modal, focus, exclusivity, pause, escape, pointer, accessibility, feedback and notification boundaries are frozen without Unity/device calls.
- [x] Public D19/D20 directions are recorded; the absent D21 interface is explicitly `LOGIC_BLOCKED`.
- [x] Reflection, private-field bypass, static fact copies, direct PlayerPrefs/domain snapshot writes and Canvas-to-fact flows are forbidden.
- [x] Historical runtime evidence is isolated as Runtime Deferred.
- [x] D22 wave dependencies and the L26-L32 serial boundary are recorded.
- [x] Result JSON and handoff are the only other files produced by this task.
