# Arcade Cabinet Platform Config Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the two overlapping, unwired scripting defines `GUNFISH_ARCADE` and `RUNNING_ON_NUC` with a single `ARCADE_CABINET` define, confine that define to one new file (`PlatformConfig.cs`), and use `PlatformConfig.IsCabinet` to drive `PlayerManager`'s join-strategy choice, `ArduinoManager`'s serial I/O, and add a Dev Config override so it can be flipped in Play mode without switching Build Profiles.

**Architecture:** `PlatformConfig.IsCabinet` is a static bool property that checks a new `DevConfigOverride.SimulatedBuildTarget` override in the Editor, falling back to `#if ARCADE_CABINET` otherwise — the only file in the codebase referencing that define directly. `PlayerManager` and `ArduinoManager` read the plain bool instead of using `#if` themselves. `GameManager.debug` separately starts tracking `Debug.isDebugBuild` in real builds (unrelated define/axis, bundled into this plan because it's part of the same spec).

**Tech Stack:** Unity 6000.5.0f1, C#, Unity Editor `EditorPrefs`/`EditorWindow`.

## Global Constraints

- No automated test suite in this project (confirmed in `CLAUDE.md`) — every task's verification is a `dotnet build` compile check plus, where noted, a manual in-Editor or in-build check.
- Never hand-edit `ProjectSettings/*.asset` or other Unity-generated/serialized assets — the `Windows-NUC.asset` Build Profile's scripting-define change (Task 7) must be done through the Build Profile's Editor inspector, not a text edit.
- `PlatformConfig.IsCabinet` (`Assets/Scripts/Managers/PlatformConfig.cs`) is the only place `#if ARCADE_CABINET` may appear. `PlayerManager.cs` and `ArduinoManager.cs` must read the plain bool, not their own `#if`.
- `ARCADE_CABINET` replaces both `GUNFISH_ARCADE` (currently in `PlayerManager.cs`, from the `2026-07-02-controller-join-strategy` spec) and `RUNNING_ON_NUC` (currently in `Windows-NUC.asset`). Both old names are fully retired by the end of this plan — grep for both after Task 7 to confirm zero references remain.
- Arcade-specific button-symbol UI is explicitly out of scope (non-goal in the spec) — no UI prompt/text files are touched by this plan.

## File Structure

- **Modify** `Assets/Scripts/Managers/DevConfigOverride.cs` — add `SimulatedBuildTarget` enum + EditorPrefs-backed override (Task 1).
- **Modify** `Assets/Scripts/Editor/DevConfigWindow.cs` — expose the new override as a UI row (Task 2).
- **Create** `Assets/Scripts/Managers/PlatformConfig.cs` — the `IsCabinet` shim; sole holder of `#if ARCADE_CABINET` (Task 3).
- **Modify** `Assets/Scripts/Managers/PlayerManager.cs` — join-strategy selection reads `PlatformConfig.IsCabinet` (Task 4).
- **Modify** `Assets/Scripts/Managers/ArduinoManager.cs` — serial I/O gated by `PlatformConfig.IsCabinet`, with simulated logging when off (Task 5).
- **Modify** `Assets/Scripts/Managers/GameManager.cs` — `debug` getter forced from `Debug.isDebugBuild` outside the Editor (Task 6).
- **Modify** `Assets/Settings/Build Profiles/Windows-NUC.asset` (Editor UI, not hand-edited) — scripting define rename (Task 7).

---

### Task 1: `DevConfigOverride.cs` — add `SimulatedBuildTarget` override

**Files:**
- Modify: `Assets/Scripts/Managers/DevConfigOverride.cs`

**Interfaces:**
- Consumes: `UnityEditor.EditorPrefs` (existing usage in this file).
- Produces: `DevConfigOverride.SimulatedBuildTarget` (enum, values `WindowsPC`/`ArcadeCabinet`), `DevConfigOverride.SimulatedBuildTargetOverride` (property, get/set), `DevConfigOverride.TryGetSimulatedBuildTarget(out SimulatedBuildTarget target)` (`bool`, mirrors `TryGetDebug`) — consumed by Task 2 (`DevConfigWindow.cs`) and Task 3 (`PlatformConfig.cs`).

- [ ] **Step 1: Add the enum and its EditorPrefs key**

Replace:
```csharp
public static class DevConfigOverride {
    private const string EnabledKey = "Gunfish.Dev.Enabled";
    private const string GameModeListGuidKey = "Gunfish.Dev.GameModeListGuid";
    private const string GunfishDataListGuidKey = "Gunfish.Dev.GunfishDataListGuid";
    private const string DebugKey = "Gunfish.Dev.Debug";
    private const string DebugPlayerCountKey = "Gunfish.Dev.DebugPlayerCount";
```
with:
```csharp
public static class DevConfigOverride {
    public enum SimulatedBuildTarget {
        WindowsPC,
        ArcadeCabinet,
    }

    private const string EnabledKey = "Gunfish.Dev.Enabled";
    private const string GameModeListGuidKey = "Gunfish.Dev.GameModeListGuid";
    private const string GunfishDataListGuidKey = "Gunfish.Dev.GunfishDataListGuid";
    private const string DebugKey = "Gunfish.Dev.Debug";
    private const string DebugPlayerCountKey = "Gunfish.Dev.DebugPlayerCount";
    private const string SimulatedBuildTargetKey = "Gunfish.Dev.SimulatedBuildTarget";
```

- [ ] **Step 2: Add the override property**

Replace:
```csharp
    public static int DebugPlayerCountOverride {
        get => EditorPrefs.GetInt(DebugPlayerCountKey, 1);
        set => EditorPrefs.SetInt(DebugPlayerCountKey, value);
    }
```
with:
```csharp
    public static int DebugPlayerCountOverride {
        get => EditorPrefs.GetInt(DebugPlayerCountKey, 1);
        set => EditorPrefs.SetInt(DebugPlayerCountKey, value);
    }

    public static SimulatedBuildTarget SimulatedBuildTargetOverride {
        get => (SimulatedBuildTarget)EditorPrefs.GetInt(SimulatedBuildTargetKey, (int)SimulatedBuildTarget.WindowsPC);
        set => EditorPrefs.SetInt(SimulatedBuildTargetKey, (int)value);
    }
```

- [ ] **Step 3: Add the `TryGet` accessor**

Replace:
```csharp
    public static bool TryGetDebugPlayerCount(out int debugPlayerCount) {
        debugPlayerCount = DebugPlayerCountOverride;
        return Enabled;
    }
```
with:
```csharp
    public static bool TryGetDebugPlayerCount(out int debugPlayerCount) {
        debugPlayerCount = DebugPlayerCountOverride;
        return Enabled;
    }

    public static bool TryGetSimulatedBuildTarget(out SimulatedBuildTarget target) {
        target = SimulatedBuildTargetOverride;
        return Enabled;
    }
```

- [ ] **Step 4: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Managers/DevConfigOverride.cs
git commit -m "feat: add SimulatedBuildTarget dev config override"
```

---

### Task 2: `DevConfigWindow.cs` — expose the override in the Dev Config window

**Files:**
- Modify: `Assets/Scripts/Editor/DevConfigWindow.cs`

**Interfaces:**
- Consumes: `DevConfigOverride.SimulatedBuildTargetOverride` / `DevConfigOverride.SimulatedBuildTarget` (produced by Task 1).
- Produces: nothing consumed by later tasks — this is a UI-only leaf.

- [ ] **Step 1: Add the enum popup row inside the existing disabled-scope block**

Replace:
```csharp
            var playerCountLabel = new GUIContent("Debug Player Count", "Number of players to simulate in debug mode. Must be between 1 and 4.");
            DevConfigOverride.DebugPlayerCountOverride = EditorGUILayout.IntSlider(
                playerCountLabel, DevConfigOverride.DebugPlayerCountOverride, 1, 4);
        }
    }
}
```
with:
```csharp
            var playerCountLabel = new GUIContent("Debug Player Count", "Number of players to simulate in debug mode. Must be between 1 and 4.");
            DevConfigOverride.DebugPlayerCountOverride = EditorGUILayout.IntSlider(
                playerCountLabel, DevConfigOverride.DebugPlayerCountOverride, 1, 4);

            EditorGUILayout.Space();
            var simulatedBuildTargetLabel = new GUIContent("Simulated Build Target", "Overrides which platform GameManager/PlayerManager/ArduinoManager behave as, without switching Build Profiles.");
            DevConfigOverride.SimulatedBuildTargetOverride = (DevConfigOverride.SimulatedBuildTarget)EditorGUILayout.EnumPopup(
                simulatedBuildTargetLabel, DevConfigOverride.SimulatedBuildTargetOverride);
        }
    }
}
```

- [ ] **Step 2: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 3: Manual check — open the window**

In the Unity Editor: **Tools → Gunfish → Dev Config**. Toggle "Enable Dev Overrides" on. Confirm a "Simulated Build Target" dropdown appears below "Debug Player Count" with options `WindowsPC` / `ArcadeCabinet`, and is disabled (grayed out) when "Enable Dev Overrides" is off.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Editor/DevConfigWindow.cs
git commit -m "feat: expose SimulatedBuildTarget override in Dev Config window"
```

---

### Task 3: `PlatformConfig.cs` — the `IsCabinet` shim

**Files:**
- Create: `Assets/Scripts/Managers/PlatformConfig.cs`

**Interfaces:**
- Consumes: `DevConfigOverride.TryGetSimulatedBuildTarget(out DevConfigOverride.SimulatedBuildTarget)` (produced by Task 1).
- Produces: `PlatformConfig.IsCabinet` (`static bool` property) — consumed by Task 4 (`PlayerManager.cs`) and Task 5 (`ArduinoManager.cs`).

- [ ] **Step 1: Create the file**

`Assets/Scripts/Managers/PlatformConfig.cs`:
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

- [ ] **Step 2: Compile-check (default configuration)**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 3: Compile-check (`ARCADE_CABINET` configuration)**

Run:
```bash
existing=$(grep -o '<DefineConstants>[^<]*</DefineConstants>' Assembly-CSharp.csproj | head -1 | sed -e 's/<[^>]*>//g')
dotnet build Assembly-CSharp.csproj -p:DefineConstants="${existing};ARCADE_CABINET"
```
Expected: `0 Error(s)`. This proves the `#if ARCADE_CABINET` branch compiles even though the project isn't built with that define by default.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Managers/PlatformConfig.cs
git commit -m "feat: add PlatformConfig.IsCabinet as the sole ARCADE_CABINET seam"
```

---

### Task 4: `PlayerManager.cs` — select join strategy from `PlatformConfig.IsCabinet`

**Files:**
- Modify: `Assets/Scripts/Managers/PlayerManager.cs:18-29`

**Interfaces:**
- Consumes: `PlatformConfig.IsCabinet` (produced by Task 3), `ArcadeJoinStrategy` / `OnlineJoinStrategy` (pre-existing, unchanged).
- Produces: nothing consumed by later tasks.

- [ ] **Step 1: Replace the `#if GUNFISH_ARCADE` selection with a runtime check**

Replace:
```csharp
    public override void Initialize() {
        base.Initialize();

#if GUNFISH_ARCADE
        strategy = new ArcadeJoinStrategy();
#else
        strategy = new OnlineJoinStrategy();
#endif
        strategy.Initialize(this);

        SetInputMode(InputMode.UI);
    }
```
with:
```csharp
    public override void Initialize() {
        base.Initialize();

        strategy = PlatformConfig.IsCabinet ? new ArcadeJoinStrategy() : new OnlineJoinStrategy();
        strategy.Initialize(this);

        SetInputMode(InputMode.UI);
    }
```

- [ ] **Step 2: Confirm no other `GUNFISH_ARCADE` references remain**

Run: `grep -rn "GUNFISH_ARCADE" Assets/`
Expected: no output.

- [ ] **Step 3: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Managers/PlayerManager.cs
git commit -m "refactor: select PlayerManager join strategy from PlatformConfig.IsCabinet"
```

---

### Task 5: `ArduinoManager.cs` — gate serial I/O behind `PlatformConfig.IsCabinet`

**Files:**
- Modify: `Assets/Scripts/Managers/ArduinoManager.cs`

**Interfaces:**
- Consumes: `PlatformConfig.IsCabinet` (produced by Task 3).
- Produces: nothing consumed by later tasks.

- [ ] **Step 1: Add the simulated-log throttle field**

Replace:
```csharp
    public float secondsBetweenAttractors = 60f;
    private float secondsSinceLastAttractor;
```
with:
```csharp
    public float secondsBetweenAttractors = 60f;
    private float secondsSinceLastAttractor;
    private float secondsSinceLastSimulatedLog;
```

- [ ] **Step 2: Gate `ConnectArduino`'s port-open loop**

Replace:
```csharp
    private void ConnectArduino() {
        foreach (var port in new string[] { "COM3", "COM4", "COM5" }) {
            serialPort = new SerialPort(port, 9600) {
                ReadTimeout = 100
            };
            try {
                serialPort?.Open();
                Debug.Log($"Connected Arduino on port {port}");
                break;
            } catch {
                Debug.Log($"Failed to connect Arduino on port {port}");
            }
        }
    }
```
with:
```csharp
    private void ConnectArduino() {
        if (!PlatformConfig.IsCabinet) {
            Debug.Log("[ArduinoManager] (simulated, not ARCADE_CABINET) would open one of COM3/COM4/COM5.");
            return;
        }

        foreach (var port in new string[] { "COM3", "COM4", "COM5" }) {
            serialPort = new SerialPort(port, 9600) {
                ReadTimeout = 100
            };
            try {
                serialPort?.Open();
                Debug.Log($"Connected Arduino on port {port}");
                break;
            } catch {
                Debug.Log($"Failed to connect Arduino on port {port}");
            }
        }
    }
```
(`serialPort` stays `null` when skipped — `DisconnectArduino`'s existing `serialPort != null` guard already handles that safely, no change needed there.)

- [ ] **Step 3: Gate `HandleArduino`'s port write, with throttled simulated logging**

Replace:
```csharp
    private void HandleArduino() {
        if (serialPort.IsOpen) {
            float loudness = SampleLoudness();
            byte volume = (byte)Mathf.RoundToInt(loudness);
            byte[] buffer = new byte[] { volume };
            serialPort.Write(buffer, 0, 1);
        }
    }
```
with:
```csharp
    private void HandleArduino() {
        if (PlatformConfig.IsCabinet) {
            if (serialPort.IsOpen) {
                float loudness = SampleLoudness();
                byte volume = (byte)Mathf.RoundToInt(loudness);
                byte[] buffer = new byte[] { volume };
                serialPort.Write(buffer, 0, 1);
            }
            return;
        }

        secondsSinceLastSimulatedLog += Time.deltaTime;
        if (secondsSinceLastSimulatedLog < 1f) return;
        secondsSinceLastSimulatedLog = 0f;
        byte simulatedVolume = (byte)Mathf.RoundToInt(SampleLoudness());
        Debug.Log($"[ArduinoManager] (simulated, not ARCADE_CABINET) would send volume byte: {simulatedVolume}");
    }
```
(Throttled to ~once/second, not every frame — `Update()` calls `HandleArduino()` every frame, so an unthrottled log would flood the console.)

- [ ] **Step 4: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 5: Manual check — simulated path (default, `PlatformConfig.IsCabinet` false)**

Press Play in the Editor with Dev Overrides off (or Simulated Build Target set to `WindowsPC`). Confirm the Console shows the "(simulated, not ARCADE_CABINET) would open..." message once on start, and "...would send volume byte: N" roughly once per second, with no real `SerialPort` connection attempted (no "Connected Arduino on port" / "Failed to connect Arduino on port" messages).

- [ ] **Step 6: Manual check — cabinet path**

In **Tools → Gunfish → Dev Config**, enable Dev Overrides and set Simulated Build Target to `ArcadeCabinet`. Press Play. Confirm the Console shows "Connected Arduino on port ..." or "Failed to connect Arduino on port ..." for COM3/COM4/COM5 (real hardware not required to observe the attempt), and no simulated-log messages. Reset Simulated Build Target back to `WindowsPC` (or disable Dev Overrides) afterward.

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/Managers/ArduinoManager.cs
git commit -m "feat: gate ArduinoManager serial I/O behind PlatformConfig.IsCabinet"
```

---

### Task 6: `GameManager.cs` — force `debug` from `Debug.isDebugBuild` outside the Editor

**Files:**
- Modify: `Assets/Scripts/Managers/GameManager.cs:26-34`

**Interfaces:**
- Consumes: `UnityEngine.Debug.isDebugBuild` (built-in), `DevConfigOverride.TryGetDebug` (pre-existing, unchanged).
- Produces: nothing consumed by later tasks.

- [ ] **Step 1: Split the getter's Editor/build branches**

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

- [ ] **Step 2: Compile-check (default configuration)**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 3: Manual check — Editor behavior is unchanged**

Press Play in the Editor with Dev Overrides off. Confirm `GameManager.Instance.debug` still reflects the `_debug` checkbox on the `GameManager` prefab (e.g. via the existing `DebugRegistrar` overlay), exactly as before this change.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Managers/GameManager.cs
git commit -m "feat: force GameManager.debug from Debug.isDebugBuild outside the Editor"
```

---

### Task 7: Retire `RUNNING_ON_NUC`, wire up `ARCADE_CABINET`, full regression pass

**Files:**
- Modify: `Assets/Settings/Build Profiles/Windows-NUC.asset` (Editor UI step, not hand-edited).

**Interfaces:**
- Consumes: everything from Tasks 1-6.
- Produces: nothing (final task).

- [ ] **Step 1: Rename the scripting define on the `Windows-NUC` Build Profile**

In the Unity Editor: **File → Build Profiles**, select `Windows-NUC`, find its Scripting Defines field, remove `RUNNING_ON_NUC` and add `ARCADE_CABINET`, then apply/save. Leave `Windows-PC` with no scripting defines (unchanged).

- [ ] **Step 2: Confirm both old defines are fully retired**

Run: `grep -rn "GUNFISH_ARCADE\|RUNNING_ON_NUC" Gunfish/Assets Gunfish/ProjectSettings "Gunfish/Assets/Settings/Build Profiles"`
Expected: no output.

- [ ] **Step 3: Manual regression — Windows-PC profile active (default), no Dev Overrides**

With the `Windows-PC` Build Profile active (or just the Editor's default platform, matching its no-define state) and Dev Overrides off:
1. Press Play. Confirm the online free-join flow runs (no ritual `OnGUI` prompt), matching current behavior.
2. Confirm `ArduinoManager` logs simulated messages (per Task 5, Step 5) rather than opening a COM port.
3. With "Development Build" unchecked for this profile, confirm `GameManager.Instance.debug` is `false`. Check it, confirm `debug` becomes `true` (no Editor recompile needed for the Editor-side check — this specifically validates the non-Editor code path, so also do a real build per Step 5 below).

- [ ] **Step 4: Manual regression — simulate the cabinet via Dev Config override, no profile switch**

In **Tools → Gunfish → Dev Config**: enable Dev Overrides, set Simulated Build Target to `ArcadeCabinet`. Press Play. Confirm the ritual `OnGUI` prompt appears and blocks until the controller-count threshold is met (per `ArcadeJoinStrategy`, unchanged from the `2026-07-02-controller-join-strategy` behavior), and `ArduinoManager` attempts a real serial connection (per Task 5, Step 6). Set Simulated Build Target back to `WindowsPC` and/or disable Dev Overrides when done.

- [ ] **Step 5: Manual regression — real builds**

Build with the `Windows-NUC` profile. Confirm the built game shows the ritual prompt and attempts a real Arduino connection (`ARCADE_CABINET` active, no Dev Override involved since Dev Overrides never run outside the Editor). Build with the `Windows-PC` profile. Confirm the built game skips the ritual (free join/leave) and logs simulated Arduino messages instead of opening a COM port. For each build, toggle "Development Build" on/off in its Build Profile before building and confirm `GameManager.Instance.debug` matches (visible via any existing debug-gated behavior, e.g. `QuickLaunchManager`'s F4 overlay).

- [ ] **Step 6: Commit** (only if Step 1 produced a change to `Windows-NUC.asset` that isn't already staged by the Editor's own save)

```bash
git add "Assets/Settings/Build Profiles/Windows-NUC.asset"
git commit -m "chore: rename RUNNING_ON_NUC scripting define to ARCADE_CABINET"
```
