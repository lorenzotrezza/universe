<!-- GSD:project-start source:PROJECT.md -->
## Project

**Universe XR Adaptive Training**

Universe XR Adaptive Training is a brownfield Unity XR training prototype for industrial safety onboarding. The current v1 pivots the existing `LabZero` desk scene into a configurable lobby, then moves the learner into a single warehouse lesson built from `free_fire_lone_wolf_mode_3d_model.glb` for container-yard and freight-handling safety training.

The experience is aimed at workers or trainees who should be able to learn by doing, inside the flow of action, instead of first studying a separate digital tool. The near-term product is a hackathon-friendly prototype with one strong guided scenario, not a full training platform.

**Core Value:** An operator can enter the experience, follow a clear guide, perform the right safety actions under time pressure, and understand mistakes immediately without prior training on the software itself.

### Constraints

- **Tech stack**: Unity `6000.4.2f1` with the current Leonardo XR, Snapdragon Spaces, OpenXR, URP, and Input System setup — avoid unnecessary platform churn during the prototype pivot
- **Brownfield foundation**: Existing `LabZero` scene flow and scripts may be modified aggressively, but the repo should keep a runnable owned demo path while refactoring
- **Demo scope**: One lobby plus one warehouse lesson for a hackathon-style prototype — depth and clarity beat breadth
- **Platform**: Android XR is the primary target, while desktop preview remains important for fast iteration and fallback testing
- **Documentation mode**: `.planning/` stays local-only with `commit_docs: false` — planning artifacts should not be expected in git history
<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->
## Technology Stack

## Languages
- C# with `LangVersion` 9.0 - gameplay and editor code in `Assets/LabZero/Scripts/*.cs`, `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`, and generated project settings in `Assembly-CSharp.csproj`.
- YAML-based Unity asset serialization - project and runtime configuration in `ProjectSettings/*.asset`, `Assets/XR/**/*.asset`, and `Assets/Settings/*.asset`.
- JSON - package manifests in `Packages/manifest.json`, dependency lock data in `Packages/packages-lock.json`, and input actions in `Assets/InputSystem_Actions.inputactions`.
- HLSL / Shader Graph assets - URP and TextMesh Pro shader assets under `Assets/TextMesh Pro/Shaders/*` and renderer configuration in `Assets/Settings/*.asset`.
## Runtime
- Unity Editor `6000.4.2f1` - declared in `ProjectSettings/ProjectVersion.txt`.
- Generated C# project targets `.NET Standard 2.1` - declared in `Assembly-CSharp.csproj`.
- Generated C# project currently targets Android build profile `Android:13` - declared in `Assembly-CSharp.csproj`.
- Unity Package Manager - dependencies declared in `Packages/manifest.json`.
- Sources in use:
- Lockfile: present in `Packages/packages-lock.json`.
## Frameworks
- Unity 6 / UnityEngine - base runtime referenced by `Assembly-CSharp.csproj`.
- Universal Render Pipeline `17.4.0` - declared in `Packages/manifest.json`, enabled in `ProjectSettings/GraphicsSettings.asset`, configured by `Assets/Settings/PC_RPAsset.asset` and `Assets/Settings/Mobile_RPAsset.asset`.
- TextMesh Pro - used by first-party UI scripts such as `Assets/LabZero/Scripts/LabTaskManager.cs`, `Assets/LabZero/Scripts/LabDeskScreenPresenter.cs`, and `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`.
- uGUI `2.0.0` - declared in `Packages/manifest.json`, used by `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`.
- OpenXR `1.16.1` - declared in `Packages/manifest.json`, configured in `Assets/XR/Settings/OpenXR Package Settings.asset`.
- XR Management `4.5.2` - resolved in `Packages/packages-lock.json`, wired in `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`.
- XR Interaction Toolkit `3.4.0` - resolved in `Packages/packages-lock.json`, sample assets under `Assets/Samples/XR Interaction Toolkit/3.4.0/`.
- XR Hands `1.7.3` - resolved in `Packages/packages-lock.json`, sample assets under `Assets/Samples/XR Hands/1.7.3/`.
- AR Foundation `6.4.2` - resolved in `Packages/packages-lock.json`, used for simulation settings in `Assets/XR/Resources/XRSimulationRuntimeSettings.asset`.
- Unity Test Framework `1.6.0` - declared in `Packages/manifest.json`.
- No first-party test assemblies or test directories were detected under `Assets/`; the only test-like code found is vendor sample validation/editor code such as `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/Editor/Scripts/StarterAssetsSampleProjectValidation.cs`.
- Visual Studio package `2.0.27` and Rider package `3.0.39` - declared in `Packages/manifest.json`.
- Unity MCP package from Git - `com.coplaydev.unity-mcp` in `Packages/manifest.json`; generated projects `MCPForUnity.Runtime.csproj` and `MCPForUnity.Editor.csproj` exist.
- UniTask from Git - `com.cysharp.unitask` in `Packages/manifest.json`; sample assemblies reference `UniTask` in `Assets/Samples/LeonardoXR SDK/1.0.2/Core Samples/Youbiquo.LeonardoXR.Samples.asmdef`.
## Key Dependencies
- `com.unity.render-pipelines.universal` `17.4.0` - active render pipeline for both quality tiers via `ProjectSettings/GraphicsSettings.asset` and `ProjectSettings/QualitySettings.asset`.
- `com.unity.xr.openxr` `1.16.1` - active XR runtime integration for Android via `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`.
- `com.qualcomm.snapdragon.spaces` `1.0.4` - vendor XR SDK supplied as `Packages/com.qualcomm.snapdragon.spaces-1.0.4.tgz`.
- `com.youbiquosrl.leonardoxrsdk` `1.0.2` - vendor XR SDK supplied as `Packages/com.youbiquosrl.leonardoxrsdk-1.0.2.tgz`.
- `com.unity.inputsystem` `1.19.0` - active input package with project-wide actions referenced by `ProjectSettings/EditorBuildSettings.asset` and defined in `Assets/InputSystem_Actions.inputactions`.
- `com.unity.ai.navigation` `2.0.12` - navigation package declared in `Packages/manifest.json`.
- `com.unity.timeline` `1.8.12` - sequencing package declared in `Packages/manifest.json`.
- `com.unity.visualscripting` `1.9.11` - installed but no first-party usage was detected in `Assets/LabZero/*`.
- `com.unity.multiplayer.center` `1.0.1` - installed via `Packages/manifest.json`, but no gameplay networking package is configured in first-party code.
## Configuration
- Unity project settings live in `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/QualitySettings.asset`, and `ProjectSettings/PackageManagerSettings.asset`.
- The active input handler is recorded as `activeInputHandler: 1` in `ProjectSettings/ProjectSettings.asset`.
- Android defines include `USING_SNAPDRAGON_SPACES_SDK` in `ProjectSettings/ProjectSettings.asset`.
- No `.env`-style runtime environment files were detected in the mapped scope for this task.
- Current build scene list contains a single enabled scene: `Assets/LabZero/Scenes/LabZero_Prototype.unity` from `ProjectSettings/EditorBuildSettings.asset`.
- `ProjectSettings/EditorBuildSettings.asset` also binds config objects for input actions, XR loader settings, AR simulation settings, and OpenXR settings.
- Android-specific player settings include `AndroidMinSdkVersion: 32` and a configured `bundleVersion: 0.1.0` in `ProjectSettings/ProjectSettings.asset`.
- Script compilation is generated into `Assembly-CSharp.csproj`; there are no first-party `.asmdef` files in `Assets/LabZero/`, so custom gameplay code currently builds into `Assembly-CSharp`.
## Platform Requirements
- macOS-hosted Unity install is implied by generated references in `Assembly-CSharp.csproj` pointing to `/Volumes/Yartalo/Unity/Hub/6000.4.2f1/Unity.app/...`.
- Editor-side XR simulation assets are present in `Assets/XR/Resources/XRSimulationRuntimeSettings.asset`, `Assets/XR/Settings/XRSimulationSettings.asset`, and `Assets/XRI/Settings/Resources/XRDeviceSimulatorSettings.asset`.
- Desktop preview support is implemented in `Assets/LabZero/Scripts/LabDebugHotkeys.cs`, which disables XR rig objects and creates a camera-driven editor preview path.
- Android is the only build target with an XR loader enabled in `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`.
- Standalone and WebGL OpenXR settings assets exist, but `Assets/XR/XRGeneralSettingsPerBuildTarget.asset` shows empty loader lists for Standalone and WebGL.
- Rendering is split by platform quality profile:
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

## Scope And Ownership
## Naming Patterns
- Use PascalCase for files and types: `LabTaskManager.cs`, `LabStatusPanel.cs`, `LabCoursePortal.cs`.
- Prefix custom LabZero types with `Lab`. This separates owned code from sample-package classes in `Assets/Samples/...`.
- Use PascalCase enum members with explicit numeric assignments in small enums: `Assets/LabZero/Scripts/LabTaskType.cs`, `Assets/LabZero/Scripts/LabThemeType.cs`.
- Keep inspector-facing references as `private` fields with `[SerializeField]`: `taskManager`, `titleText`, `portalRenderer` in `Assets/LabZero/Scripts/LabTaskManager.cs` and `Assets/LabZero/Scripts/LabCoursePortal.cs`.
- Use camelCase for serialized fields and locals.
- Use a leading underscore only for private runtime-only state that should not be exposed in the inspector: `_runtimeMaterial`, `_deskBuilt`, `_previewReady` in `Assets/LabZero/Scripts/LabCoursePortal.cs`, `Assets/LabZero/Scripts/LabDeskCommandPad.cs`, and `Assets/LabZero/Scripts/LabDebugHotkeys.cs`.
- No custom namespaces are used in `Assets/LabZero/*.cs`. New project-owned scripts should match that unless you intentionally introduce asmdefs and update the whole area together.
## Code Style
- Braces use Allman style throughout `Assets/LabZero/Scripts/*.cs`.
- `using` directives are grouped with framework/system namespaces first, then TMPro/Unity namespaces. Examples: `Assets/LabZero/Scripts/LabTaskManager.cs`, `Assets/LabZero/Scripts/LabDebugHotkeys.cs`.
- Attributes sit on their own lines above classes or fields: `[RequireComponent(typeof(Collider))]`, `[Header("Optional UI")]`, `[SerializeField]`.
- Expression-bodied properties are common in `Assets/LabZero/Scripts/LabTaskManager.cs` and `Assets/LabZero/Scripts/LabCollectible.cs`.
- Null-coalescing assignment is used for late binding: `taskManager ??= Object.FindAnyObjectByType<LabTaskManager>();`.
- Target-typed `new(...)` is used for `Color` and `Vector3` values in `Assets/LabZero/Scripts/LabCoursePortal.cs` and `Assets/LabZero/Scripts/LabDebugHotkeys.cs`.
- Switch statements and switch expressions are preferred over dictionary-driven dispatch for small state machines: `Assets/LabZero/Scripts/LabTaskManager.cs`, `Assets/LabZero/Scripts/LabTaskZone.cs`, `Assets/LabZero/Scripts/LabDeskCommandPad.cs`.
- `Array.Empty<string>()` is used instead of allocating empty arrays in `Assets/LabZero/Scripts/LabTaskManager.cs`.
- Comments are sparse. Add them only when preserving compatibility or clarifying intent. Current examples are in `Assets/LabZero/Scripts/LabTaskManager.cs` for compatibility bindings and in `Assets/Samples/XR Interaction Toolkit/3.4.0/Starter Assets/Editor/Scripts/StarterAssetsSampleProjectValidation.cs` for sample setup notes.
## Scene And Script Organization
- Keep owned scenes under `Assets/LabZero/Scenes/`.
- Keep generic sample scenes under `Assets/Scenes/` and imported XR demo scenes under `Assets/Samples/.../Scenes/`.
- `docs/plans/2026-04-12-hackathon-runbook.md` and `docs/plans/2026-04-11-labzero-implementation.md` both point to `Assets/LabZero/Scenes/LabZero_Prototype.unity` as the primary demo and verification scene.
- Put runtime MonoBehaviours in `Assets/LabZero/Scripts/`.
- Put UnityEditor-only tooling in `Assets/LabZero/Editor/`.
- Do not mix owned scripts into `Assets/Samples/...`; the design and implementation docs explicitly describe `Assets/LabZero/` as the ownership boundary in `docs/plans/2026-04-11-labzero-implementation.md`.
- No project-owned `.asmdef` files are present under `Assets/LabZero/`. Custom code currently compiles through the default `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj`.
- Existing `.asmdef` files belong to imported sample packages under `Assets/Samples/...`.
## Editor And Runtime Separation
- `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs` is wrapped in `#if UNITY_EDITOR` and uses `UnityEditor`, `UnityEditor.SceneManagement`, and `MenuItem`.
- Runtime scripts under `Assets/LabZero/Scripts/` avoid editor APIs.
- `Assets/LabZero/Scripts/LabDebugHotkeys.cs` contains a narrow `#if UNITY_EDITOR` block for desktop preview setup, while keeping the rest of the behavior runtime-safe.
#if UNITY_EDITOR
#endif
## Null Handling And Error Handling
- `Assets/LabZero/Scripts/LabDeskScreenPresenter.cs` returns immediately if `taskManager` is missing or a child text object cannot be found.
- `Assets/LabZero/Scripts/LabTaskZone.cs` exits early if the collider or `LabCollectible` is absent.
- `Assets/LabZero/Scripts/LabCoursePortal.cs` checks for `null` before selecting a theme, pulsing materials, or reacting to trigger entry.
- Scripts commonly attempt `FindAnyObjectByType<T>()` in `Awake` or `OnEnable` instead of throwing: `Assets/LabZero/Scripts/LabStatusPanel.cs`, `Assets/LabZero/Scripts/LabDeskCommandPad.cs`, `Assets/LabZero/Scripts/LabCoursePortal.cs`.
- UI references are optional and individually null-checked before assignment in `Assets/LabZero/Scripts/LabTaskManager.cs`.
- Owned code uses `Debug.LogWarning` and `Debug.Log` only in editor tooling, not runtime gameplay. See `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`.
- No custom code throws exceptions or uses `try/catch`. Missing references degrade to no-op behavior.
## Runtime Behavior Conventions
- Subscribe to `LabTaskManager.StateChanged` in `OnEnable` and unsubscribe in `OnDisable`: `Assets/LabZero/Scripts/LabStatusPanel.cs`, `Assets/LabZero/Scripts/LabDeskScreenPresenter.cs`, `Assets/LabZero/Scripts/LabCoursePortal.cs`, `Assets/LabZero/Scripts/LabDeskCommandPad.cs`.
- Use `Reset()` only for editor-safe component defaults such as collider trigger setup in `Assets/LabZero/Scripts/LabTaskZone.cs`.
- Keep state transitions inside a central manager. `Assets/LabZero/Scripts/LabTaskManager.cs` owns course selection, lesson progress, play state, and UI summary state.
## Asset Organization Patterns
- Owned gameplay assets are grouped by feature under `Assets/LabZero/`.
- XR configuration assets live under `Assets/XR/`, `Assets/XRI/`, and `Assets/Settings/`.
- Tutorial and onboarding assets live under `Assets/TutorialInfo/`.
- `Assets/Screenshots/` exists for captured media.
- `Assets/TextMesh Pro/` and `Assets/Samples/...` are package-imported support areas and should not be treated as the place for new owned gameplay logic.
## Practical Examples To Follow
- Duplicate or save custom work into `Assets/LabZero/Scenes/LabZero_Prototype.unity`.
- Keep imported scenes in `Assets/Samples/.../Scenes/` unchanged where possible.
- Repository-level formatting config files. `.editorconfig`, `stylecop.json`, and custom analyzer configs were not detected outside `Library/`.
- Project-owned asmdefs for `Assets/LabZero/`. Separation is folder-based today.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

## Pattern Overview
- Use `Assets/LabZero/Scenes/LabZero_Prototype.unity` as the only build scene defined in `ProjectSettings/EditorBuildSettings.asset`.
- Keep authored gameplay logic in plain `MonoBehaviour` scripts under `Assets/LabZero/Scripts` with no custom `asmdef`; these compile into `Assembly-CSharp`.
- Mix authored `LabZero` objects with imported XR sample/prefab content already present in the same scene file, especially content tied to `Assets/Samples/LeonardoXR SDK/1.0.2/...` and XR Interaction Toolkit assets.
- Drive state changes through `LabTaskManager.StateChanged` rather than through a separate service layer, event bus, or ScriptableObject state asset.
- Build the interactive "learning desk" desktop preview procedurally at runtime in `Assets/LabZero/Scripts/LabDebugHotkeys.cs` instead of serializing those desk controls into the scene.
## Layers
- Purpose: Start Unity, select the build scene, and initialize XR/OpenXR package settings.
- Location: `ProjectSettings/EditorBuildSettings.asset`, `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`, `Assets/XR/Settings/OpenXR Package Settings.asset`, `Assets/XRI/Settings/Resources/XRInteractionRuntimeSettings.asset`.
- Contains: Build-scene selection, XR loader registration, OpenXR feature toggles, XR Interaction Toolkit runtime settings.
- Depends on: Unity XR Management, OpenXR, Snapdragon Spaces, Leonardo XR, XR Interaction Toolkit.
- Used by: Unity runtime startup before scene scripts run.
- Purpose: Define the initial object graph for the playable prototype scene.
- Location: `Assets/LabZero/Scenes/LabZero_Prototype.unity`.
- Contains: The `LabZero` root, `Lab Task Manager`, `Lab Debug Hotkeys`, `Lab Status Canvas`, three `LabTaskZone` objects, multiple `LabCollectible` objects, plus imported XR/sample objects such as `XR Interaction Manager`, `App Manager`, `Sci-Fi Table`, `V8 Engine`, and `Floor Shadow Effect`.
- Depends on: Unity scene serialization plus package prefabs referenced from the scene.
- Used by: Unity scene loading and all authored `LabZero` scripts.
- Purpose: Own the current course selection and lesson playback/progression state.
- Location: `Assets/LabZero/Scripts/LabTaskManager.cs`.
- Contains: Selected theme, lesson started/play state, module index, completion flags, text-generation helpers, and the `StateChanged` event.
- Depends on: `LabThemeType`, `LabTaskType`, `TMPro`, and Unity lifecycle methods.
- Used by: `LabStatusPanel`, `LabDebugHotkeys`, `LabTaskZone`, `LabDeskScreenPresenter`, `LabDeskCommandPad`, and `LabCoursePortal`.
- Purpose: Translate collisions, mouse clicks, and keyboard input into `LabTaskManager` mutations.
- Location: `Assets/LabZero/Scripts/LabTaskZone.cs`, `Assets/LabZero/Scripts/LabCollectible.cs`, `Assets/LabZero/Scripts/LabDeskCommandPad.cs`, `Assets/LabZero/Scripts/LabCoursePortal.cs`, `Assets/LabZero/Scripts/LabDebugHotkeys.cs`.
- Contains: Trigger handlers, desktop key bindings, runtime-created command pads, item acceptance rules, and course portal selection behavior.
- Depends on: Scene colliders, `CharacterController` or `Player` tag checks, Unity Input System, and the manager reference.
- Used by: Player interactions in-scene and the editor/desktop preview workflow.
- Purpose: Render manager state back into world-space and panel text.
- Location: `Assets/LabZero/Scripts/LabStatusPanel.cs`, `Assets/LabZero/Scripts/LabDeskScreenPresenter.cs`.
- Contains: Text auto-wiring, label refresh logic, summary color selection, and desk screen copy.
- Depends on: `LabTaskManager`, TextMesh Pro objects, and scene/runtime object names such as `Panel/Title` or `Screen Course Title`.
- Used by: The bootstrapped status canvas and the runtime-generated desk screen.
- Purpose: Create or clean up the prototype layout inside the editor.
- Location: `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`.
- Contains: `LabZero/Cleanup Missing Scripts In Open Scene` and `LabZero/Bootstrap Prototype In Open Scene` menu commands.
- Depends on: `UnityEditor`, `EditorSceneManager`, serialized-property mutation, and runtime script types from `Assets/LabZero/Scripts`.
- Used by: Manual authoring workflows in the Unity Editor.
## Data Flow
- Keep runtime state inside the single `LabTaskManager` component in `Assets/LabZero/Scripts/LabTaskManager.cs`.
- Use `Object.FindAnyObjectByType<LabTaskManager>()` as the fallback discovery mechanism in view and interaction scripts; there is no dependency-injection container.
- Persist no authored runtime state in ScriptableObjects for `LabZero`; the only ScriptableObject usage found is in vendor/sample content such as `Assets/Samples/LeonardoXR SDK/1.0.2/Core Samples/Shared Assets/Scripts/ScriptableObject/SharedDataScriptable.cs`.
## Key Abstractions
- Purpose: Central state model and presenter helper for the `LabZero` experience.
- Examples: `Assets/LabZero/Scripts/LabTaskManager.cs`, serialized instance in `Assets/LabZero/Scenes/LabZero_Prototype.unity`.
- Pattern: Scene singleton by convention, event-emitting state holder.
- Purpose: Keep interactions keyed to coarse workflow categories rather than string literals.
- Examples: `Assets/LabZero/Scripts/LabTaskType.cs`, `Assets/LabZero/Scripts/LabThemeType.cs`.
- Pattern: Small enum domain model shared across interaction and presentation scripts.
- Purpose: Replace legacy XR/sample scene visuals with a desktop-testable desk UI during play.
- Examples: `BuildLearningDesk()`, `CreateFloatingScreen()`, and `CreateCommandPads()` in `Assets/LabZero/Scripts/LabDebugHotkeys.cs`.
- Pattern: Imperative runtime scene construction using primitives, materials, lights, and TextMesh Pro components.
- Purpose: Regenerate the base `LabZero` object graph inside the open scene.
- Examples: `BootstrapPrototype()` and `CleanupMissingScripts()` in `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`.
- Pattern: Editor menu-driven scene mutation through serialized property writes.
## Scene / Script Relationships
- `LabTaskManager` on the `Lab Task Manager` object.
- `LabDebugHotkeys` on the `Lab Debug Hotkeys` object.
- `LabStatusPanel` on `Lab Status Canvas`.
- `LabTaskZone` on three zone objects for PPE, tools, and hazard flow.
- `LabCollectible` on multiple interactable items.
- `LearningDesk_Root`.
- `LabDeskScreenPresenter` attached to the generated screen panel.
- `LabDeskCommandPad` attached to each generated desk button.
- World primitives, materials, a point light, and TextMesh Pro labels for the desktop learning-desk preview.
- `Assets/LabZero/Scripts/LabCoursePortal.cs` is not referenced by `Assets/LabZero/Scenes/LabZero_Prototype.unity`.
- `Assets/LabZero/Scripts/LabDeskCommandPad.cs` and `Assets/LabZero/Scripts/LabDeskScreenPresenter.cs` are not serialized into the scene; they appear only after runtime construction.
- `Assets/LabZero/Scenes/LabZero_Prototype.prefab` exists beside the scene, but the current scene YAML does not reference that prefab asset's GUID.
## Entry Points
- Location: `ProjectSettings/EditorBuildSettings.asset`.
- Triggers: Player build or Play Mode scene load.
- Responsibilities: Select `Assets/LabZero/Scenes/LabZero_Prototype.unity` as the only enabled scene.
- Location: `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`.
- Triggers: Unity XR bootstrap before scene content starts.
- Responsibilities: Configure XR startup behavior per platform; Android has configured loaders, Standalone and WebGL do not.
- Location: `Assets/LabZero/Scenes/LabZero_Prototype.unity`.
- Triggers: Scene activation.
- Responsibilities: Instantiate the `LabZero` root and all serialized MonoBehaviours.
- Location: `Assets/LabZero/Scripts/LabDebugHotkeys.cs`.
- Triggers: `Awake()` and `Update()` during play.
- Responsibilities: Hide sample/XR objects, build the desk preview, create or reposition a desktop camera, and map keyboard input to manager actions.
- Location: `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`.
- Triggers: Unity Editor menu items under `LabZero/...`.
- Responsibilities: Clean missing scripts and rebuild the prototype layout into the currently open scene.
## Error Handling
- Check for null manager, null renderer, null keyboard, or invalid enum before doing work in `Assets/LabZero/Scripts/*.cs`.
- Use `[RequireComponent(typeof(Collider))]` in `LabTaskZone`, `LabDeskCommandPad`, and `LabCoursePortal` to enforce collider presence at authoring time.
- Log only in editor tooling through `Debug.Log` and `Debug.LogWarning` in `Assets/LabZero/Editor/LabZeroBootstrapperEditor.cs`.
## Cross-Cutting Concerns
- To add a new course flow, extend `LabThemeType` in `Assets/LabZero/Scripts/LabThemeType.cs`, update module/title/url switch branches in `Assets/LabZero/Scripts/LabTaskManager.cs`, and then expose a new interaction surface either through runtime-built pads in `Assets/LabZero/Scripts/LabDebugHotkeys.cs` or a new scene interaction component under `Assets/LabZero/Scripts`.
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->
## Project Skills

No project skills found. Add skills to any of: `.claude/skills/`, `.agents/skills/`, `.cursor/skills/`, or `.github/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

## Unity MCP Execution

- When implementing Unity scene, GameObject, component, script, test, or editor changes, use the `unity-mcp-orchestrator` skill and the Unity MCP server as the default execution path.
- If the Unity MCP server is closed, stop before execution and wait for the user to open it rather than proceeding with blind Unity-side manipulation.
- Follow the Unity MCP resource-first workflow: check editor state first, inspect scene/resources before acting, then verify through console checks and screenshots after changes.

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
