# Arcade Cabinet / PC Platform Config — Design

## Background

Two Unity Build Profiles now exist: `Windows-NUC` (runs on the NUC inside the arcade
cabinet) and `Windows-PC` (runs on a regular PC). They need to differ in three ways:

1. `ArduinoManager`'s serial/COM connection only makes sense on the cabinet.
2. `PlayerManager` already branches ritual-vs-free-join behavior on a compile-time
   define (`GUNFISH_ARCADE`, from `2026-07-02-controller-join-strategy-design.md`) —
   that define was never wired to a real build target. `Windows-NUC` currently carries
   a second, newly-added define (`RUNNING_ON_NUC`) covering the same distinction.
3. `GameManager.debug` should track each build's "Development Build" checkbox
   automatically rather than a hand-set inspector field.

Arcade-specific UI (button-symbol prompts instead of keyboard/controller prompts) is
explicitly not implemented yet — see Non-goals.

## Goals

- Retire `GUNFISH_ARCADE` and `RUNNING_ON_NUC` in favor of one scripting define,
  `ARCADE_CABINET`, set only on the `Windows-NUC` Build Profile.
- Confine that `#if` to a single new file, `PlatformConfig.cs`, exposing a plain
  runtime `PlatformConfig.IsCabinet` bool. No other file references the define
  directly — this was a deliberate correction after the design's first pass leaked
  `#if ARCADE_CABINET` into `PlayerManager`/`ArduinoManager` directly.
- `PlayerManager` selects `ArcadeJoinStrategy`/`OnlineJoinStrategy` from
  `PlatformConfig.IsCabinet` instead of `#if GUNFISH_ARCADE`. No behavior change.
- `ArduinoManager`'s actual serial I/O (`ConnectArduino`'s port-open loop,
  `HandleArduino`'s port write) only runs when `PlatformConfig.IsCabinet` is true.
  When false, it logs what it would have done instead of touching `SerialPort` at
  all. Attract-mode audio/quips (`playAttractors`, `PlayClip`) are hardware-independent
  and stay unaffected on both builds.
- `GameManager.debug` is forced from `Debug.isDebugBuild` in real (non-Editor) builds,
  ignoring the serialized `_debug` checkbox entirely there. In the Editor, behavior is
  unchanged (`DevConfigOverride` override, else `_debug`) — `Debug.isDebugBuild` is
  always `true` in the Editor regardless of the Development Build toggle, so using it
  there would make the existing override system meaningless.
- Add a `DevConfigOverride.SimulatedBuildTarget` enum override (`WindowsPC` /
  `ArcadeCabinet`), exposed in the existing `Tools → Gunfish → Dev Config` window, so
  `PlatformConfig.IsCabinet` can be flipped in Play mode without switching Build
  Profiles or recompiling.

## Non-goals

- Arcade-specific button-symbol UI (vs. keyboard/controller prompts). No code changes
  in this pass; stays a stub exactly as it is today. The ritual's existing `OnGUI`
  prompt text is already cabinet-only and needs no change.
- Any change to max player count or join/leave semantics beyond the define rename —
  `ArcadeJoinStrategy`/`OnlineJoinStrategy` themselves are untouched.
- Arcade physical-port auto-detection (already tracked as a separate future spike in
  the controller-join-strategy spec).

## Architecture

`PlatformConfig.cs` is the single seam between the compile-time define and everything
that cares about it:

```csharp
public static class PlatformConfig {
    public static bool IsCabinet {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetSimulatedBuildTarget(out var target))
                return target == DevConfigOverride.SimulatedBuildTarget.ArcadeCabinet;
#endif
#if ARCADE_CABINET
            return true;
#else
            return false;
#endif
        }
    }
}
```

This mirrors the override-then-fallback shape `GameManager.debug` already uses for
`DevConfigOverride.TryGetDebug`. Default behavior (override disabled) resolves purely
from the `ARCADE_CABINET` define, so it's a no-op outside the Editor.

## Components

### Build Profiles

- `Windows-NUC.asset`: scripting define changes from `RUNNING_ON_NUC` to
  `ARCADE_CABINET` (via the Build Profile's Editor inspector — Unity-generated assets
  aren't hand-edited per project convention).
- `Windows-PC.asset`: no change (already has no scripting defines).

### `PlayerManager.cs`

Replace:
```csharp
#if GUNFISH_ARCADE
        strategy = new ArcadeJoinStrategy();
#else
        strategy = new OnlineJoinStrategy();
#endif
```
with:
```csharp
        strategy = PlatformConfig.IsCabinet ? new ArcadeJoinStrategy() : new OnlineJoinStrategy();
```

### `ArduinoManager.cs`

`ConnectArduino()`: skip the COM3/4/5 open loop entirely when `!PlatformConfig.IsCabinet`,
logging once that it's skipping. `serialPort` stays `null` in that case.

`HandleArduino()`: when `PlatformConfig.IsCabinet`, behavior is unchanged (guarded by
`serialPort.IsOpen` as today). When not, log the volume byte it would have sent,
throttled to ~once/second (a per-frame `Debug.Log` from `Update()` would flood the
console) via a new `secondsSinceLastSimulatedLog` timer field, same pattern as the
existing `secondsSinceLastAttractor` throttle in this file.

`DisconnectArduino()`: unchanged — its existing `serialPort != null` guard already
handles the skipped-connection case safely.

No `#if` appears in this file; both branches are plain runtime code guarded by
`PlatformConfig.IsCabinet`.

### `GameManager.cs`

Replace:
```csharp
    public bool debug {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetDebug(out var debugOverride)) return debugOverride;
#endif
            return _debug;
        }
    }
```
with:
```csharp
    public bool debug {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetDebug(out var debugOverride)) return debugOverride;
            return _debug;
#else
            return Debug.isDebugBuild;
#endif
        }
    }
```

### `DevConfigOverride.cs`

Add, following the exact shape of the existing `Debug`/`DebugPlayerCount` overrides:
```csharp
public enum SimulatedBuildTarget {
    WindowsPC,
    ArcadeCabinet,
}

private const string SimulatedBuildTargetKey = "Gunfish.Dev.SimulatedBuildTarget";

public static SimulatedBuildTarget SimulatedBuildTargetOverride {
    get => (SimulatedBuildTarget)EditorPrefs.GetInt(SimulatedBuildTargetKey, (int)SimulatedBuildTarget.WindowsPC);
    set => EditorPrefs.SetInt(SimulatedBuildTargetKey, (int)value);
}

public static bool TryGetSimulatedBuildTarget(out SimulatedBuildTarget target) {
    target = SimulatedBuildTargetOverride;
    return Enabled;
}
```

### `DevConfigWindow.cs`

Add one row inside the existing `EditorGUI.DisabledScope(!DevConfigOverride.Enabled)`
block:
```csharp
var simulatedBuildTargetLabel = new GUIContent("Simulated Build Target", "Overrides which platform GameManager/PlayerManager/ArduinoManager behave as, without switching Build Profiles.");
DevConfigOverride.SimulatedBuildTargetOverride = (DevConfigOverride.SimulatedBuildTarget)EditorGUILayout.EnumPopup(
    simulatedBuildTargetLabel, DevConfigOverride.SimulatedBuildTargetOverride);
```

## Testing

No automated test suite in this project — manual verification in-editor and via build:

- With Dev Overrides disabled and no `ARCADE_CABINET` define active (default Editor
  state): confirm `PlatformConfig.IsCabinet` is `false` — online join strategy active,
  `ArduinoManager` logs simulated serial writes instead of opening a COM port.
- Enable Dev Overrides, set Simulated Build Target to Arcade Cabinet: confirm the
  ritual `OnGUI` prompt appears and `ArduinoManager` attempts a real COM3/4/5
  connection, with no Build Profile switch or recompile.
- Set Simulated Build Target back to Windows PC: confirm behavior reverts immediately.
- Build with the `Windows-NUC` profile: confirm `ARCADE_CABINET` is active
  (`PlatformConfig.IsCabinet` true with overrides off) — ritual behavior, real Arduino
  connection attempted.
- Build with the `Windows-PC` profile: confirm the inverse.
- Toggle "Development Build" on/off for a `Windows-PC` build; confirm
  `GameManager.debug` reflects it (on = true, off = false) with no inspector changes.
