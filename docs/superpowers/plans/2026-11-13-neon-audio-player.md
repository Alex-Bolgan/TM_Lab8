# Neon Console Audio Player Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a polished XAML-first WPF audio player with transport controls, seek and volume sliders, three neon skins, and a non-rectangular window.

**Architecture:** A single SDK-style WPF executable uses `MediaElement` as its playback engine. `App.xaml` and `MainWindow.xaml` own the visual system, while a deliberately small `MainWindow.xaml.cs` handles only file I/O, playback state, seeking, the position timer, skin resource updates, and window chrome actions.

**Tech Stack:** .NET 8, WPF, XAML, C#, `Microsoft.Win32.OpenFileDialog`, `DispatcherTimer`; no external packages.

**Spec:** `docs/superpowers/specs/2026-11-13-neon-audio-player-design.md`

## Global Constraints

- The solution must open and run in Visual Studio.
- Use an SDK-style `net8.0-windows` WPF project with nullable reference types and implicit usings enabled.
- XAML must represent at least 90% of non-empty `.xaml` plus `.cs` lines, excluding generated, `bin`, and `obj` files.
- Do not use third-party packages, MVVM frameworks, network access, or duplicated windows per skin.
- The interface must be non-minimalistic and use a borderless, translucent, non-rectangular console silhouette.
- Supported file picker extensions: `.mp3`, `.wav`, `.wma`, and `.m4a`.

## Review Focus

- Canceling the file picker must preserve the current source and playback state; verify manually in Task 2.
- Clicking Play before selecting a file must not throw and must show a useful status; verify manually in Task 2.
- Seeking while the timer is updating must not make the thumb jump; verify manually in Task 2.
- Audio with unknown or unavailable duration must leave the seek bar disabled without failing; verify manually in Task 2.
- Switching skins during playback must not reset position, source, or playback state; verify manually in Task 3.

---

### Task 1: WPF shell and XAML visual system

**Files:**
- Create: `NeonAudioPlayer.sln`
- Create: `NeonAudioPlayer/NeonAudioPlayer.csproj`
- Create: `NeonAudioPlayer/App.xaml`
- Create: `NeonAudioPlayer/App.xaml.cs`
- Create: `NeonAudioPlayer/MainWindow.xaml`
- Create: `NeonAudioPlayer/MainWindow.xaml.cs`

**Interfaces:**
- Consumes: none.
- Produces: named controls `AudioEngine`, `SeekSlider`, `VolumeSlider`, `TrackTitleText`, `StatusText`, `CurrentTimeText`, `DurationText`, and `PauseResumeText`; click/event handler names consumed by Task 2.

- [ ] **Step 1: Create the SDK-style project shell**

Create the solution and project with `OutputType=WinExe`, `TargetFramework=net8.0-windows`, and `UseWPF=true`; keep `App.xaml.cs` limited to the generated partial `Application` class.

- [ ] **Step 2: Define reusable presentation resources in `App.xaml`**

Add dark base brushes, dynamic neon brushes, glow effects, button/control templates, slider templates, text styles, and animation storyboards; skin-dependent colors must be referenced through `DynamicResource`.

- [ ] **Step 3: Build the complete console in `MainWindow.xaml`**

Use `WindowStyle=None`, transparency, a clipped multi-panel silhouette, draggable header, system buttons, layered grid/circuit decoration, central track display, seek/time area, Open/Play/Pause/Stop controls, volume control, three skin buttons, status lights, and a hidden manual `MediaElement`.

- [ ] **Step 4: Compile the visual shell**

Run: `dotnet build NeonAudioPlayer.sln`
Expected: build succeeds with 0 warnings and 0 errors.

### Task 2: Playback, selection, seeking, and window behavior

**Files:**
- Modify: `NeonAudioPlayer/MainWindow.xaml.cs`

**Interfaces:**
- Consumes: named XAML controls and events from Task 1.
- Produces: `OpenFile_Click`, `Play_Click`, `PauseResume_Click`, `Stop_Click`, `AudioEngine_MediaOpened`, `AudioEngine_MediaEnded`, `AudioEngine_MediaFailed`, `SeekSlider_DragStarted`, `SeekSlider_DragCompleted`, `SeekSlider_ValueChanged`, `VolumeSlider_ValueChanged`, `WindowHeader_MouseLeftButtonDown`, `Minimize_Click`, and `Close_Click`.

- [ ] **Step 1: Add playback state and time formatting**

Implement fields `_positionTimer: DispatcherTimer`, `_isSeeking: bool`, `_isPaused: bool`, and `_hasSource: bool`; add `FormatTime(TimeSpan value): string` returning `mm:ss` or `hh:mm:ss` when needed.

- [ ] **Step 2: Implement safe file selection and transport handlers**

Use `OpenFileDialog` with the four specified extensions. Guard transport actions when no source exists, update the display/status consistently, and make Pause/Resume toggle without changing source or position.

- [ ] **Step 3: Implement media lifecycle and timer synchronization**

On `MediaOpened`, configure duration and enable seeking only for a natural timespan; on timer ticks update position unless `_isSeeking`; on completion reset to zero and `READY`; on failure show `ERROR` without throwing.

- [ ] **Step 4: Implement seeking, volume, dragging, and system buttons**

Seek only after a valid source/duration, map volume to `0.0..1.0`, support window dragging with the left mouse button, and wire minimize/close operations.

- [ ] **Step 5: Build and manually exercise failure-prone inputs**

Run: `dotnet build NeonAudioPlayer.sln`
Expected: 0 warnings and 0 errors. Then open in Visual Studio and verify canceled selection, Play without a file, drag seeking, and a source whose duration is initially unavailable do not crash or corrupt the UI.

### Task 3: Runtime skin switching and visual polish

**Files:**
- Modify: `NeonAudioPlayer/MainWindow.xaml`
- Modify: `NeonAudioPlayer/MainWindow.xaml.cs`

**Interfaces:**
- Consumes: dynamic resource keys `NeonPrimary`, `NeonSecondary`, `NeonAccent`, `NeonDim`, and the playback state from Task 2.
- Produces: `SkinButton_Click(object sender, RoutedEventArgs e)` and palettes `Cyan`, `Magenta`, `Ultraviolet` selected through each button's `Tag`.

- [ ] **Step 1: Complete three distinct palettes and active-skin feedback**

Define exact palette values in XAML resource data attached to the skin controls, then implement `SkinButton_Click` to update only application brush colors and the active indicator; do not touch `AudioEngine.Source`, `Position`, or transport state.

- [ ] **Step 2: Add restrained continuous animation and state feedback**

Animate scan lines, status lights, and glow pulses in XAML; add triggers for hover, press, focus, and disabled states while preserving readable contrast.

- [ ] **Step 3: Verify switching during playback**

Run the application from Visual Studio, start a track, switch through all three skins, and confirm playback position and state continue unchanged.

- [ ] **Step 4: Rebuild**

Run: `dotnet build NeonAudioPlayer.sln`
Expected: 0 warnings and 0 errors.

### Task 4: Documentation and final verification

**Files:**
- Create: `README.md`
- Create: `scripts/verify-xaml-ratio.ps1`
- Modify: project files only if verification exposes a defect.

**Interfaces:**
- Consumes: completed solution from Tasks 1-3.
- Produces: reproducible build/run instructions and a ratio check that exits nonzero below 90%.

- [ ] **Step 1: Document the project in Ukrainian**

Explain prerequisites, Visual Studio launch steps, supported formats, controls, skins, project structure, and the reason for minimal code-behind.

- [ ] **Step 2: Add the XAML ratio verifier**

Count non-empty lines in hand-written `.xaml` and `.cs` files while excluding `bin` and `obj`; print XAML lines, C# lines, percentage, and fail when the percentage is less than `90.0`.

- [ ] **Step 3: Run automated verification**

Run: `dotnet build NeonAudioPlayer.sln`
Expected: 0 warnings and 0 errors.

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify-xaml-ratio.ps1`
Expected: exit code 0 and XAML percentage at least 90.0%.

- [ ] **Step 4: Perform Visual Studio UI verification**

Open `NeonAudioPlayer.sln` in Visual Studio, build and launch it, then inspect the complete window at standard scaling and verify Open, Play, Pause/Resume, Stop, seek, volume, skin switching, drag, minimize, and close.

- [ ] **Step 5: Inspect repository scope**

Confirm there are no generated build products or unrelated files intended for delivery, and summarize verification evidence in the handoff.
