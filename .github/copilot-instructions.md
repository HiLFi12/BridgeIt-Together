# AI coding agent guide — Bridge it Together!

This is a Unity game project focused on a cooperative bridge-building mechanic driven by a quadrant grid and era-specific rules.

## Big picture
- Core loop: players collect materials and build a bridge by quadrants. Each quadrant has ordered layers (0=Base, 1=Soporte, 2=Superficie/Estructura). Construction must follow sequence and completes a quadrant when all layers are done.
- Central systems:
  - `Assets/Scripts/Bridge/Construction/BridgeConstructionGrid.cs`: owns the grid, instantiates quadrant prefabs, enforces build order, updates visuals/colliders, and processes vehicle impacts.
  - `Assets/Scripts/Bridge/Quadrant/BridgeQuadrantSO.cs`: per-quadrant state machine (requiredLayers[3], `lastLayerState` Complete/Damaged/Destroyed) and era logic (Industrial heat, Futuristic battery, etc.).
  - `Assets/Scripts/Bridge/Quadrant/BridgeQuadrant.cs`: lightweight MonoBehaviour for quadrant UI hooks and current layer tracking; ensures tag `BridgeQuadrant`.
  - `Assets/Scripts/Bridge/PlayerBridge/PlayerBridgeInteraction.cs`: selects target quadrant from player position and attempts builds.
  - `Assets/Scripts/Player/Player.cs`: Input System entry (Actions: Interact, Build, Dash, Drop, Pause) and optional debug hotkey.
  - `Assets/Scripts/Player/PlayerUIManager.cs`: per-player and shared UI groups with reference counting across players; integrates per-quadrant layer UI.

## Key patterns and contracts
- Building layers: call `BridgeConstructionGrid.TryBuildLayer(x, z, layerIndex, layerObject)`.
  - Validates coordinates (`IsValidQuadrant`), enforces first-incomplete-layer rule, and updates visuals + sounds.
  - For debug/cheats: `DebugRellenarTodoPuente()` (fills all layers) and `ForceCompleteRemainingLayers(x,z)`.
- Quadrant visuals and collision:
  - Visual GameObjects named `Layer_{i}_{layerName}` are created/destroyed by the grid; scale/height from `layerScales`/`layerHeights` with mode `LayerScaleMode`.
  - Collider behavior: disabled until first layer, trigger while partially built, solid when complete.
  - Tag `BridgeQuadrant` is required for vehicle collision systems.
- Era mechanics (in `BridgeQuadrantSO`):
  - Prehistoric/Medieval: usage count damages/destroys last layer.
  - Industrial: heat (`ApplyHeat`/`RemoveHeat`); temperature decays when not turned; can fully destroy quadrant.
  - Contemporary: probabilistic damage on pass.
  - Futuristic: battery drains; `ReplaceBattery()` repairs.
- UI conventions:
  - Held-object UI: implement `IUIActivatable` (`UIIndex`, `SetUIIndex`) and notify via `PlayerUIManager.RefreshHeldObjectUI(index)`.
  - Example: `PaloIgnifugo` toggles `turnedOffIndex/turnedOnIndex` and updates `PlayerUIManager`.
  - Per-quadrant layer prompts come from `BridgeQuadrant.GetLayerUI(int)` and are mass-toggled by `PlayerUIManager` when shared counts go 0→1/1→0.
- Interactions and materials:
  - Pickups: `Assets/Scripts/Bridge/Material/BridgeMaterialPickup.cs` configures `layerIndex` (0..2) and era for given prefab.
  - Many interactables implement `IHitable` for launch interactions and `IUIActivatable` for HUD prompts.

## Workflows
- Play/Debug: open in Unity Editor, press Play. Use editor tooling `BridgeConstructionGridEditor` to Reescalar Grilla and "🔧 Rellenar Todo el Puente" at runtime.
- Debug build flow: in `Player`, enabling `enableFillBridgeHotkey` allows the `fillBridgeKey` to call `bridgeGrid.DebugRellenarTodoPuente()`.
- Input System: ensure a `PlayerInput` with actions named Interact/Build/Dash/Drop/Pause is present (see `Player.cs`).

## Adding features — examples
- New bridge material: create a prefab with the visual, set up a pickup via `BridgeMaterialPickup` with the right `layerIndex`. Building is orchestrated by `PlayerBridgeInteraction` + `BridgeConstructionGrid.TryBuildLayer`.
- New heat source: implement a MonoBehaviour that locates the target `BridgeConstructionGrid`/quadrant(s) and calls `ApplyHeat`/`RemoveHeat` appropriately. To show HUD, implement `IUIActivatable` like `PaloIgnifugo` and set `UIIndex`.
- UI additions: extend `PlayerUIManager` groups in the inspector. To show per-layer prompts, ensure quadrants have their `Image` references set in `BridgeQuadrant`.

## Gotchas
- Do not bypass layer order: grid and SO both enforce "first incomplete layer". Use `ForceCompleteRemainingLayers` only for debug/tools.
- Keep quadrant tag/name wiring: prefabs need a Collider; the grid will size it and assign `BridgeQuadrant` tag.
- Spanish naming/logging is prevalent; logs include coordinates and layer indices—follow that style for consistency.

References: search under `Assets/Scripts/Bridge/**`, `Assets/Scripts/Player/**`, and `Assets/Scripts/Gameplay/Abstractions/**` for interfaces (`IVehiclePoolService`, etc.).