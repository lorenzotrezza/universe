# Youbiquo — LeonardoXR SDK — Developer Documentation

| | |
|---|---|
| **SDK Version** | 1.0.2 |
| **Target Device** | Youbiquo Leonardo XR |
| **Spaces SDK** | Snapdragon Spaces 1.0.4 |
| **Unity Version** | 6000.0.58f2 |
| **Language** | English |

---

## Document Index

| # | Title | Description |
|---|-------|-------------|
| 01 | [Getting Started](#01-getting-started) | Project setup, SDK installation, URP and Play-in-Editor configuration |
| 02 | [VR / MR](#02-vr--mr) | Manual project config, Passthrough, scene setup, lifecycle compatibility |
| 03 | [Features](#03-features) | Snapdragon Spaces features + LeonardoXR proprietary features |
| 04 | [Samples Guide](#04-samples-guide) | Overview of all available samples and how to run them |
| 05 | [Interaction Package Guides](#05-interaction-package-guides) | XRIT 3.x, XR Hands, controllers and hand tracking |
| 06 | [Advanced](#06-advanced) | Display refresh rate, performance settings, custom controllers, QNN |

---

## 01 Getting Started

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

### Introduction

This guide explains how to configure a Unity project for development on Leonardo XR, the Youbiquo AR headset based on Snapdragon Spaces. The goal is to familiarise developers with the SDK configuration for building applications on Leonardo. The next guide covers how to interact with controllers and hands.

Leonardo XR runs Android 12 and follows a development workflow familiar to anyone who has previously built Android applications.

### Required Software

Before starting, make sure the following software and packages are installed at the specified versions:

| Software / Package | Version |
|---|---|
| Unity Hub | Latest available |
| Unity Editor | 6000.0.58f2 |
| LeonardoXR SDK | 1.0.2 |
| Snapdragon Spaces SDK | 1.0.4 |
| OpenXR Plugin | 1.15.1 |
| AR Foundation | 6.2.1 |
| XR Interaction Toolkit | 3.3.0 |
| XR Hands | 1.7.2 |
| UniTask | 2.5.10 |
| VS Code, Visual Studio or similar IDE | — |

> **NOTE** Unlike the base Snapdragon Spaces guide (which recommends v1.10.0 for Unity 2022), Unity 6 with LeonardoXR uses OpenXR 1.15.1. Do not modify this version manually — the Package Manager handles it through the SDK dependencies.

### Create a New Project

1. Open Unity Hub and click **New Project**.
2. Select the **Universal 3D** template.
3. Name the project (e.g. `LeonardoXR_Test`) and choose a destination folder.
4. Click **Create Project**.

After creation, Unity opens the project with a sample scene in the Hierarchy panel.

### Switch Platform to Android

1. From the main menu select **File > Build Profiles**.
2. In the Build Profiles window select **Android** from the Platform list.
3. Click **Switch Platform** and wait for the process to complete.
4. Close the Build Profiles window.

### Install Snapdragon Spaces SDK

> **IMPORTANT** Snapdragon Spaces SDK **must be installed before** the LeonardoXR SDK. The LeonardoXR runtime depends on it and will not compile without it.

1. Download Snapdragon Spaces SDK v1.0.4 from the Qualcomm developer portal (a developer account is required): https://spaces.qualcomm.com/developer/vr-mr-sdk/#downloads
2. Open the Package Manager (**Window > Package Manager**).
3. Click **+** and select **Add package from tarball…**
4. Browse to the downloaded `.tgz` file and click **Open**. Installation will start automatically.

During installation, two prompts may appear:
- "Enable the new input system?" → click **Yes**.
- "XR Interaction Layer Mask Update Required" → for a new project, click **I Made a Backup, Go Ahead!**

> **NOTE** The editor will restart after installation — this is expected behaviour, not a crash. For version control, copy the `.tgz` into the project's `Packages/` folder before adding it; the manifest will then use a relative path, which is better for source control.

> **Unity 6.3+ — Known Spaces SDK issue:** After installation you may see the following compile error:
> ```
> BaseRuntimeFeature.InterceptEnvironmentBlendMode.cs(15,10): error CS0592:
> Attribute 'SerializeField' is not valid on this declaration type.
> ```
> To fix it, double-click the error in the Console to open the file, then remove the `[SerializeField]` attribute on line 15. This is a bug in Snapdragon Spaces 1.0.4 that only surfaces on Unity 6.3 and above.

### Install LeonardoXR SDK

1. Download LeonardoXR SDK v1.0.2 from the Youbiquo Developer area.
2. Open the Package Manager (**Window > Package Manager**).
3. Click **+** and select **Add package from tarball…**
4. Browse to the LeonardoXR SDK `.tgz` file and click **Open**.

The SDK will be installed together with its dependencies: **OpenXR Plugin 1.15.1**, **AR Foundation 6.2.1**, **XR Interaction Toolkit 3.3.0**, and **XR Hands 1.7.2** — no manual installation of these packages is required.

### LeonardoXR SDK Manager

After the LeonardoXR SDK is installed, a **LeonardoXR SDK Manager** window opens automatically. If it does not appear, open it from **LeonardoXR > SDK Manager** in the menu bar.

The window has two tabs:

**Setup tab** — displays the status of the two packages that cannot be resolved automatically:

| Package | Status | Action |
|---|---|---|
| Snapdragon Spaces SDK 1.0.4 | ✓ / ✗ | **Browse…** — opens a file picker to select the `.tgz` and installs it via Package Manager |
| UniTask 2.5.10 | ✓ / ✗ | **Install** — installs UniTask directly from the official Cysharp Git repository |

Once both packages show ✓, the setup is complete. Use the **↺ Refresh** button at any time to re-check the current state.

**About tab** — displays version information for the SDK, device, and Unity Editor.

### Configure XR Plugin Management and OpenXR

#### Open Project Settings

Go to **Edit > Project Settings** and select **XR Plug-in Management** in the left sidebar.

#### Enable OpenXR and Snapdragon Spaces

1. Click the **Android** tab (top right of the window).
2. Under **Plug-in Providers**, ensure both are checked: **OpenXR** and **Snapdragon Spaces feature group**.

#### Configure OpenXR Features

1. Click **OpenXR** in the left sidebar.
2. In the Snapdragon Spaces feature list, enable only **Base Runtime** and disable all others. Advanced features (Hit Testing, Plane Detection, etc.) can be enabled later as needed.

> **NOTE** For Hand Tracking to work correctly on LeonardoXR, the **Hand Tracking Subsystem** option must be enabled in the **All Features** section of OpenXR.

#### Add LeonardoXR Feature Group

1. Return to **XR Plug-in Management > Android**.
2. In the Plug-in Providers list, also check **LeonardoXR feature group**.

#### Run Project Validation

1. In the Project Settings sidebar click **Project Validation**.
2. The window shows a list of configuration errors to fix.
3. Click **Fix All** to resolve them automatically.

> **NOTE** Some fixes may require an editor restart. Re-run validation after restarting to confirm all errors are resolved.

### Import Samples

Before creating your own scenes it is useful to import the samples included in each package. For each package: open **Window > Package Manager**, select **In Project**, select the package, expand the **Samples** section and click **Import**.

| Package | Samples |
|---|---|
| XR Hands | Gestures, Hand Visualizer |
| XR Interaction Toolkit | Starter Assets, Hand Interaction Demo |
| Snapdragon Spaces | Core Samples |
| LeonardoXR | Core Samples, Documentation |

After importing all samples, add scenes to the build via **Window > XR > Snapdragon Spaces > Add Scenes to Build Settings**.

### Configure URP

#### Graphics and Quality Settings

1. Open **Edit > Project Settings > Graphics**.
2. Under **Default Render Pipeline**, click the circle and select `Mobile_RPAsset`.
3. Go to **Project Settings > Quality**.
4. In the Levels section, ensure **Mobile** has a green checkbox in the Android column. If not, click the **Default** triangle under Android and select **Mobile**.
5. Close Project Settings.

#### Configure URP Assets

In `Assets/Settings`, select `Mobile_Renderer` and apply:
- **Rendering → Depth Priming Mode**: set to `Disabled`
- **Shadows → Transparent Receive Shadows**: uncheck
- **Post-processing → Enabled**: uncheck
- **Renderer Feature → SSAO**: ensure it is disabled or removed

In `Assets/Settings`, select `Mobile_RPAsset` and apply:
- **HDR**: disable
- **Anti Aliasing (MSAA)**: set to `4x`

### Play In Editor Configuration

This section describes how to configure the project for Play-In-Editor development, reducing the need to deploy to the device on every change.

#### OpenXR Project Settings

Go to **Edit > Project Settings > XR Plug-in Management** and select the **Windows, Mac, Linux** tab.

**Initialize XR On Startup** — the value must match the Android tab setting:
- Without Dual Render Fusion → must be **enabled**.
- With Dual Render Fusion → must be **disabled**.

> **NOTE** Enable **XR Simulation** on the Windows/Mac/Linux tab to use AR Foundation simulation environments. Note that QR Code Tracking has no simulation subsystem equivalent.

#### Enable Base Runtime Feature for Windows/Mac/Linux

Go to **Edit > Project Settings > XR Plug-in Management > OpenXR** (Windows, Mac, Linux tab) and enable the **Base Runtime** feature.

#### Scene Configuration — Spaces XR Simulator

Add the **Spaces XR Simulator** component to a root GameObject in the scene. The component persists across scene loads (`DontDestroyOnLoad`) and only one instance can exist at a time.

| Property | Description |
|---|---|
| Start Connected | Simulates glasses active and connected (Dual Render Fusion only). |
| Invert Sim Camera Display | Swaps phone and glasses displays in the editor (Dual Render Fusion only). |

#### Dual Render Fusion Editor Shortcuts

| Event | Shortcut | Menu Path |
|---|---|---|
| Connect Glasses | Alt-Shift-C | Window > XR > Snapdragon Spaces > DRF > Simulation > Connect Glasses |
| Disconnect Glasses | Alt-Shift-D | Window > XR > Snapdragon Spaces > DRF > Simulation > Disconnect Glasses |
| Glasses Active | Alt-Shift-G | Window > XR > Snapdragon Spaces > DRF > Simulation > Glasses Active |
| Glasses Idle | Alt-Shift-H | Window > XR > Snapdragon Spaces > DRF > Simulation > Glasses Idle |

> **NOTE** All final behaviour must always be verified on the physical device, not only in the editor.

---

## 02 VR / MR

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

### Manual Project Configuration

It is recommended to use the Configuration Tool for initial project setup (see Getting Started). This guide describes the equivalent manual steps for developers who prefer direct control over settings.

#### Edit Project Settings

1. Go to **Edit > Project Settings > XR Plug-in Management** and open the **Android** tab.
2. Enable the **OpenXR** plug-in and the **Snapdragon Spaces** feature group.
3. Click the red exclamation mark next to OpenXR to open **OpenXR Project Validation**.
4. Click **Fix** next to each item.
5. Apply the "Enable both Input Systems" setting last, as it requires an editor restart.

#### Enable Spaces Features

Enable the features that must be active at runtime in OpenXR Settings. Each feature corresponds to an AR Foundation Manager and an XR Subsystem:

| Feature | AR Foundation Manager | XR Subsystem |
|---|---|---|
| Base Runtime | AR Session | XRSessionSubsystem |
| Camera Frame Access | AR Camera Manager | XRCameraSubsystem |
| Hit Testing | AR Raycast Manager | XRRaycastSubsystem |
| Image Tracking | AR Tracked Image Manager | XRImageTrackingSubsystem |
| Plane Detection | AR Plane Manager | XRPlaneSubsystem |
| Spatial Anchors | AR Anchor Manager | XRAnchorSubsystem |
| Spatial Meshing | AR Mesh Manager | XRMeshSubsystem |

#### Add Feature Usage Data to the Android Manifest

Go to **Edit > Project Settings > Snapdragon Spaces Launcher Settings** to declare features required or supported by the application for: Hand Tracking, Eye Tracking, Passthrough, Controllers and Room Scale.

> **NOTE** By default all `uses-feature` values are set to `true`. Set unused features to `false` to avoid false requirements in the manifest.

### Passthrough Setup

Passthrough allows the physical environment to be displayed as an image overlay on VR devices. On LeonardoXR it is available on compatible devices.

- Activated via the **Extend Content** panel in the in-app UI (visible only on Passthrough-compatible devices).
- Can also be toggled with the **X** and **A** buttons on the controller.
- The enable function is `OnPassthroughToggle()` in `MainMenuSampleController`, which sets `PassthroughToggle` on `BaseRuntimeFeature`.

> **CAUTION** The session camera must have the **Alpha channel** of the Background set to `0` for Passthrough to work correctly. A warning is automatically logged if settings are incorrect.

The `OnSpacesAppSpaceChange` delegate in `BaseRuntimeFeature` notifies when the device performs a recenter (activated by holding the menu button for 2 seconds on the VRX).

### Scene Setup

> **NOTE** This guide assumes prior knowledge of AR Foundation and OpenXR. Refer to the official AR Foundation and OpenXR documentation for foundational concepts.

#### Minimum Scene Hierarchy

The following objects are required to enable positional head tracking on LeonardoXR:

```
AR Session
XR Origin
  └── Camera Offset
        └── Main Camera (tagged as "MainCamera")
```

Create them by right-clicking in the Hierarchy and selecting **XR > AR Session** or **XR > XR Origin**.

> **NOTE** Disable the **AR Camera Manager** component if you are not retrieving RGB frames from the camera, to avoid lifecycle issues. Also disable **AR Camera Background** — it is not supported and will be automatically disabled at runtime.

### Application Lifecycle — Older Runtime Compatibility

The Older Runtime Compatibility library prevents the application from launching when the SDK is incompatible with the OpenXR runtime installed on the device. Two entries are added to the Android manifest at startup:

- `targetAPI` — matches the OpenXR runtime version included in the SDK.
- `minAPI` — the minimum runtime version each feature supports.

> **NOTE** As of Snapdragon Spaces 1.0.4, the `minAPI` for all features is `0.22.0`.

| Result | Description |
|---|---|
| Success | No incompatibilities detected. |
| Error Runtime Failure | OpenXR runtime error. |
| Error Validation Failure | Older Runtime Compatibility validation error. |
| Error Uninitialized | System Error due to missing initialization. |
| Error Runtime Too Old For Application | Installed runtime is too old for the SDK. |
| Error Application Too Old For Runtime | SDK is too old for the installed runtime. |

### Known Issues — VR/MR on LeonardoXR

> **NOTE** For issues not listed here, refer to the general known issues for the Snapdragon Spaces platform.

#### Gradle Build Issue

Adding a custom controller archive can cause Gradle caching errors. Deleting the `Temp/gradleOut` folder generally resolves the issue.

#### AR Foundation Issues

- **AR Raycast Hit returns wrong Trackable ID** — `ARRaycastHit.trackableId` always returns `0-0`.
- **Plane Detection stalled** — Mesh triangulation enters an infinite loop in AR Foundation 6.2.1.

#### XRIT Issues

- **AR Raycast Manager added automatically** — XRIT 3.x adds an AR Raycast Manager when an XR Raycast Interactor is present, enabling Spatial Meshing regardless of the `EnableARRaycasting` flag. This may reduce performance.
- **Unsupported AR Mesh Manager properties** — Using unsupported properties generates runtime warnings.
- **AR Camera Manager / AR Camera Background Issues** — Moving between scenes with AR Camera Manager enabled can cause freezes. Disable AR Camera Background to avoid rendering issues.

#### Play In Editor Known Issues

- **Memory Leak in Camera Access Simulation** — Resolved in AR Foundation 6.0+.
- **AR Mesh Manager crash without Normals** — Update AR Foundation or enable normals.
- **QR Code Tracking not loaded in Simulation** — No simulation subsystem exists; bypass subsystem checks in editor builds.
- **ScriptableSingleton Warning** — Resolved in AR Foundation 5.1.2+.

---

## 03 Features

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

This page documents the features available in the LeonardoXR SDK, organised in two sections:

- **Snapdragon Spaces Features** — AR/XR functionality provided by the Snapdragon Spaces platform, accessible through AR Foundation.
- **LeonardoXR Features** — proprietary features specific to Leonardo XR.

---

### Snapdragon Spaces Features

#### Local Anchors

**How to Enable**

Go to **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Spatial Anchors** and enable the feature.

**Spaces Anchor Store**

> **NOTE** Look around the environment to generate a better tracking map and reduce save/load times. Saving multiple anchors simultaneously blocks the main thread — use the callback to save subsequent anchors sequentially.

```csharp
namespace Qualcomm.Snapdragon.Spaces
{
    public class SpacesAnchorStore
    {
        public void ClearStore();
        public void SaveAnchor(ARAnchor anchor, string anchorName, Action<bool> onSavedCallback = null);
        public void SaveAnchorWithResult(ARAnchor anchor, string anchorName, Action<SaveAnchorResult> callback = null);
        public void DeleteSavedAnchor(string anchorName);
        public void LoadSavedAnchor(string anchorName, Action<bool> onLoadedCallback = null);
        public void LoadAllSavedAnchors(Action<bool> onLoadedCallback = null);
        public string[] GetSavedAnchorNames();
        public string GetSavedAnchorNameFromARAnchor(ARAnchor anchor);
    }
}
```

**API Reference**

| Method | Description |
|---|---|
| `ClearStore()` | Clears the local anchor storage. |
| `SaveAnchor(...)` | Saves an ARAnchor by name or generated hash; optional callback on completion. |
| `SaveAnchorWithResult(...)` | Like SaveAnchor but returns a `SaveAnchorResult` enum. |
| `DeleteSavedAnchor(name)` | Deletes a saved anchor by name. |
| `LoadSavedAnchor(name, ...)` | Loads an anchor from storage and attempts to localise it; fires `anchorsChanged.added`. |
| `LoadAllSavedAnchors(...)` | Loads all stored anchors. |
| `GetSavedAnchorNames()` | Returns all saved anchor names. |
| `GetSavedAnchorNameFromARAnchor(anchor)` | Returns the name of a saved anchor from a tracked ARAnchor, or empty string if not saved. |

**SaveAnchorResult Values**

| Value | Meaning |
|---|---|
| `PENDING` | Anchor is awaiting save. |
| `SAVED` | Successfully saved. |
| `FAILURE_RUNTIME_ERROR` | Runtime error. |
| `FAILURE_STORE_NOT_LOADED` | Anchor Store failed to load. |
| `FAILURE_INSUFFICIENT_QUALITY` | Environment map quality is insufficient. |

---

#### Hit Testing

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Hit Testing**.

Uses the **AR Raycast Manager** from AR Foundation (`XRRaycastSubsystem`). Supports continuous hit testing against real-world planes and meshes. The system casts a ray from the camera (or a specified point) and returns intersections with detected surfaces. Each hit contains position, normal and surface type (plane or mesh).

```csharp
List<ARRaycastHit> hits = new List<ARRaycastHit>();
if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
{
    Pose hitPose = hits[0].pose;
    // Place AR content at hitPose.position
}
```

> **CAUTION** `ARRaycastHit.trackableId` always returns `0-0`. (Known Issue)

---

#### Image Tracking

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Image Tracking**. Uses the **AR Tracked Image Manager** (`XRImageTrackingSubsystem`). Supports static and mutable (runtime) reference image libraries.

```csharp
// Mutable library example
var library = trackedImageManager.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;
await library.ScheduleAddImageWithValidationJob(texture, "image-name", 0.1f); // 0.1f = physical width in metres
trackedImageManager.referenceLibrary = library;
```

---

#### Plane Detection

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Plane Detection**. Uses the **AR Plane Manager** (`XRPlaneSubsystem`). *Use Scene Understanding Backend* is enabled by default.

```csharp
arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
```

> **CAUTION** Plane Detection enters an infinite mesh triangulation loop in AR Foundation 6.0. In Unity 6 + OpenXR 1.15.1, Plane Detection does not work in Dual Render Fusion apps.

---

#### Camera Frame Access

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Camera Frame Access**. Uses the **AR Camera Manager** (`XRCameraSubsystem`). Key capabilities:

- Raw camera frame access in YUV (Y'UV420sp) and YUYV / YUY2 formats.
- `XRCpuImage.GetPlane(int)` for CPU-side image access.
- `XRCpuImage.ConvertAsync` for asynchronous conversion.
- GPU-accelerated RGB frame access for AiO devices (via OpenGL).
- Sensor extrinsics / pose retrieval via `InputBindings`.

> **NOTE** Enable **Allow 'unsafe' Code** in **Player Settings > Android > Other Settings > Script Compilation** when using the Camera Frame Access sample. Disable AR Camera Background to avoid rendering issues.

```csharp
void OnCameraFrameReceived(ARCameraFrameEventArgs args)
{
    if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
    {
        using (image)
        {
            XRCpuImage.Plane yPlane = image.GetPlane(0); // Luminance plane
            // yPlane.data contains the raw data
        }
    }
}
```

---

#### Spatial Meshing

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Spatial Meshing**. Uses the **AR Mesh Manager** (`XRMeshSubsystem`). The optional `SpacesARMeshManagerConfig` component controls mesh characteristics and camera height offsets for `TrackingOriginMode.Floor`.

---

#### QR Code Tracking

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > QR Code Tracking**. Uses the custom `SpacesQrCodeManager` component. Supports multiple tracking modes to control algorithm update frequency.

```csharp
// Editor build workaround (no simulation subsystem for QR Code Tracking)
bool CheckSubsystem()
{
#if UNITY_EDITOR
    return arQrCodeManager.subsystem?.running ?? false;
#else
    return true;
#endif
}
```

---

#### Hand Tracking

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Hand Tracking**. Hand Tracking is provided by the QCHT (Qualcomm Hand Tracking) plugin packages. LeonardoXR uses the Unity XR Hands package (v1.7.2) as the data access layer for joint data, integrated with QCHT extensions.

> **NOTE** For Hand Tracking to work correctly on LeonardoXR, the **Hand Tracking Subsystem** option must be active in **All Features** of OpenXR.

For full Hand Tracking integration with XR Interaction Toolkit, see the [Interaction Package Guides](#05-interaction-package-guides) document.

---

#### Foveated Rendering

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Foveated Rendering**. Reduces GPU load by rendering the visual field periphery at lower resolution. Foveation settings persist after application pause and resume.

| Level | Description |
|---|---|
| None | No reduction. |
| Low | Light reduction, imperceptible to most users. |
| Medium | Good quality/performance trade-off. |
| High | Maximum reduction; recommended only for heavily 3D content scenes. |

---

### LeonardoXR Features

The following features are specific to Leonardo XR and are not part of the base Snapdragon Spaces platform. Enable them via **Project Settings > XR Plug-in Management > OpenXR (Android) > LeonardoXR feature group**.

#### Dual Camera Access

> **DOCUMENTATION IN PROGRESS** This feature provides synchronised access to frames from both Leonardo XR cameras. Detailed API documentation, configuration and samples will be available in a future release.

#### QNN Inference Plugin

> **DOCUMENTATION IN PROGRESS** This feature exposes Leonardo XR hardware neural inference capabilities via the Qualcomm Neural Network (QNN) runtime, enabling AI/ML models to run directly on the device NPU. Detailed API documentation, model conversion workflow and samples will be available in a future release.

#### Intercept OpenXR Function

> **DOCUMENTATION IN PROGRESS** This advanced feature provides a mechanism to intercept and override low-level OpenXR functions. It is intended for specialised use cases requiring direct control over the OpenXR layer. Detailed documentation will be available in a future release.

---

## 04 Samples Guide

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

Samples are organised in two sections:

- **LeonardoXR Samples** — samples specific to Leonardo XR proprietary features.
- **Snapdragon Spaces Core Samples** — samples provided by Qualcomm for base AR/XR features.

### Importing Samples

#### Snapdragon Spaces Core Samples

The Core Samples package is importable from **Window > Package Manager > Snapdragon Spaces > Core Samples**. After import, add all scenes to the build via **Window > XR > Snapdragon Spaces > Add Scenes to Build Settings**.

#### LeonardoXR Core Samples

The LeonardoXR Core Samples package is importable from **Window > Package Manager > LeonardoXR > Core Samples**. After import, scenes are available in `Samples/LeonardoXR/[version]/Core Samples/Scenes/`.

---

### LeonardoXR Samples

#### Dual Camera Access Sample

> **DOCUMENTATION IN PROGRESS** This sample will demonstrate how to access synchronised frames from both Leonardo XR cameras via the Dual Camera Access feature.

- **Scene**: `Samples/LeonardoXR/.../Core Samples/Scenes/DualCameraAccess`
- **Prerequisites**: Dual Camera Access enabled in LeonardoXR feature group.

<!-- #### QNN Inference Sample

> **DOCUMENTATION IN PROGRESS** This sample will demonstrate how to load and run a neural inference model on the Leonardo XR NPU via the QNN Inference Plugin.

- **Scene**: `Samples/LeonardoXR/.../Core Samples/Scenes/QnnInference`
- **Prerequisites**: QNN Inference Plugin enabled in LeonardoXR feature group. -->

#### Intercept OpenXR Function Sample

> **DOCUMENTATION IN PROGRESS** This sample will demonstrate how to intercept low-level OpenXR functions via the InterceptOpenXRFunction feature.

- **Scene**: `Samples/LeonardoXR/.../Core Samples/Scenes/InterceptOpenXRFunction`
- **Prerequisites**: Intercept OpenXR Function enabled in LeonardoXR feature group.

---

### Snapdragon Spaces Core Samples

#### Local Anchors Sample

- **Scene**: `Samples/Snapdragon Spaces/.../Core Samples/Scenes/Anchor`

Demonstrates creating and destroying local anchors to precisely track a point in the real world. Requires the **Spatial Anchors** feature enabled in OpenXR.

**How it Works**

A transparent positioning gizmo floats 1 metre in front of the camera. When *Place anchor on surfaces* is enabled, a ray is projected forward each frame. When a surface is hit, the gizmo turns yellow. Touching the touchpad or using the gaze interactor creates an `ARAnchor` with a tracking gizmo.

```csharp
private void OnAnchorsChanged(ARAnchorsChangedEventArgs args)
{
    foreach (var anchor in args.updated)
    {
        Destroy(anchor.transform.GetChild(0).gameObject);
        var newGizmo = Instantiate(
            anchor.trackingState == TrackingState.None
                ? GizmoUntrackedAnchor
                : GizmoTrackedAnchor);
        newGizmo.transform.SetParent(anchor.transform, false);
    }
}
```

**Saving, Deleting and Loading Anchors**

- Enable *Save new anchors to local store* to persist each new anchor.
- White cube on gizmo = anchor saved and tracked; red cube = not tracked.
- **Load All Saved Anchors** — loads and attempts to localise all stored anchors.
- **Clear Store** — deletes all stored anchors (does not destroy already-loaded anchors).
- **Destroy All Anchors** — destroys all active anchor GameObjects (with a short delay).

#### Hand Tracking Sample

- **Scene**: `Samples/.../Core Samples/Scenes/HandTracking`

Demonstrates hand joint visualisation and gesture recognition via the QCHT interaction package. Requires Hand Tracking and QCHT / XR Hands packages.

#### Hit Testing Sample

- **Scene**: `Samples/.../Core Samples/Scenes/HitTesting`

Demonstrates continuous hit testing via raycast against real-world planes and meshes using `ARRaycastManager`. Shows how to place virtual objects on detected surfaces.

#### Image Tracking Sample

- **Scene**: `Samples/.../Core Samples/Scenes/ImageTracking`

Demonstrates tracking of printed reference images and virtual content overlay. Supports static and mutable image libraries.

#### Plane Detection Sample

- **Scene**: `Samples/.../Core Samples/Scenes/PlaneDetection`

Demonstrates detection and visualisation of horizontal and vertical planes via `ARPlaneManager`. Supports detection modes and optional convex hull planes.

#### Camera Frame Access Sample

- **Scene**: `Samples/.../Core Samples/Scenes/CameraFrameAccess`

Demonstrates retrieval of raw camera frames (YUV / YUYV) using `ARCameraManager`. Includes sensor extrinsics, camera count query and GPU-accelerated frame access. Requires **Allow 'unsafe' Code** in Player Settings.

#### Spatial Meshing Sample

- **Scene**: `Samples/.../Core Samples/Scenes/SpatialMeshing`

Demonstrates real-time environment meshing using `ARMeshManager`. Includes an opacity slider and the optional `SpacesARMeshManagerConfig` component.

#### QR Code Tracking Sample

- **Scene**: `Samples/.../Core Samples/Scenes/QRCodeTracking`

Demonstrates QR code scanning and tracking via `SpacesQrCodeManager`. Multiple tracking modes available.

#### XR Interaction Toolkit Sample

- **Scene**: `Samples/.../Core Samples/Scenes/XRInteractionToolkit`

Demonstrates standard XRIT interactions (ray interactors, direct interactors, teleportation) in a Snapdragon Spaces project. Compatible with the LeonardoXR configuration.

#### Composition Layer Components

The **Spaces Composition Layer** renders textures directly to the HMD, bypassing post-processing steps. Supported geometry types are Quad, Cylinder, Sphere and Cube. Useful for high-sharpness UI or elements that must maintain quality regardless of scene effects.

#### Additional Samples

| Sample | Description |
|---|---|
| Display Refresh Rate | Query and runtime setting of display refresh rate via the `XR_EXT_display_refresh_rate` extension. |
| Performance Settings | Setting CPU/GPU performance levels via the `XR_EXT_performance_settings` extension. |

---

## 05 Interaction Package Guides

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

This guide describes how to configure the interaction layer for LeonardoXR applications using standard Unity packages:

- **XR Interaction Toolkit (XRIT) 3.3.0** — high-level interaction system for controllers, ray interactors and direct interactors.
- **XR Hands 1.7.2** — access to hand joint data and gesture recognition.

### Dependencies and Versions

| Package | Version | Source |
|---|---|---|
| XR Interaction Toolkit | 3.3.0 | Unity Registry |
| XR Hands | 1.7.2 | Unity Registry |
| AR Foundation | 6.2.1 | Unity Registry |
| Snapdragon Spaces SDK | 1.0.4 | Tarball (Qualcomm) |
| LeonardoXR SDK | 1.0.2 | Tarball (Youbiquo) |
| UniTask | 2.5.10 | Unity Registry |

> **NOTE** All dependencies must be installed as described in the Getting Started guide before proceeding with interaction configuration.

---

### 1. XR Interaction Toolkit Configuration

#### Installation

Install via **Window > Package Manager > Unity Registry > XR Interaction Toolkit > Install** (version 3.3.0). If prompted "Enable the new input system?", click **Yes**. If prompted "XR Interaction Layer Mask Update Required", click **I Made a Backup, Go Ahead!** (for new projects) or **No Thanks** if no previous XRIT version was present.

#### Import Starter Assets

1. Open **Window > Package Manager**, select **In Project** then **XR Interaction Toolkit**.
2. Under **Samples**, click **Import** on **Starter Assets**.
3. Also click **Import** on **Hand Interaction Demo** (required for hand tracking samples).

The Starter Assets include:
- Pre-configured Input Action Asset for XR controllers and hands.
- Reference prefabs for XR Origin with Ray Interactor and Direct Interactor.
- Base materials and shaders for interaction visual feedback.

#### Configure Input Actions

After import, an `XRI Default Input Actions` asset is added to `Assets/Samples/XR Interaction Toolkit/[version]/Starter Assets/`. This asset is pre-configured for standard controller profiles (including the Companion Controller via Microsoft MMRC Profile) and for hands.

To associate the asset with the system: go to **Edit > Project Settings > XR Plug-in Management > Input System** and ensure **Active Input Handling** is set to `Both` or `Input System Package (New)`.

---

### 2. Scene Configuration — XR Origin

#### Base Scene Hierarchy

```
AR Session
XR Origin (Action-based)
  └── Camera Offset
        ├── Main Camera (tagged "MainCamera")
        ├── LeftHand Controller
        │     ├── XR Ray Interactor (left)
        │     └── XR Direct Interactor (left)
        └── RightHand Controller
              ├── XR Ray Interactor (right)
              └── XR Direct Interactor (right)
```

Create the base structure by selecting **GameObject > XR > XR Origin (Action-based)**.

> **NOTE** Disable **AR Camera Background** — not supported on LeonardoXR and automatically disabled at runtime. Also disable **AR Camera Manager** if not retrieving RGB frames from the camera.

#### Input Action Manager

Add the **Input Action Manager** component to the XR Origin (or a persistent GameObject). In the **Action Assets** field, assign:

- `XR Input Actions` (from `Samples/LeonardoXR SDK/[version]/Core Samples/Shared Assets`).

---

### 3. XR Hands Configuration

#### Installation

Install via **Window > Package Manager > Unity Registry > XR Hands > Install** (version 1.7.2).

#### Import Hand Samples

Open **Window > Package Manager > In Project > XR Hands > Samples** and import:
- **Gestures** — gesture recognition system.
- **Hand Visualizer** — hand mesh visualisation.

#### Enable Hand Tracking Subsystem

1. Go to **Edit > Project Settings > XR Plug-in Management > OpenXR** (Android tab).
2. Under **All Features**, enable **Hand Tracking Subsystem**.
3. Also enable **Hand Tracking** in the Snapdragon Spaces feature group.

> **NOTE** The Hand Tracking Subsystem must be enabled in **All Features** — not just in the Snapdragon Spaces section — to work correctly on LeonardoXR.

#### Hand Data Access via XR Hands API

```csharp
using UnityEngine.XR.Hands;

XRHandSubsystem handSubsystem = XRHandSubsystemUtility.GetSubsystem();
if (handSubsystem != null)
{
    XRHand leftHand = handSubsystem.leftHand;
    if (leftHand.isTracked)
    {
        if (leftHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTipPose))
        {
            // indexTipPose.position = position of left index fingertip
        }
    }
}
```

#### Gesture Recognition

```csharp
[SerializeField] XRHandGestures gestures;

void OnEnable()
{
    gestures.gesturePerformed += OnGesturePerformed;
}

void OnGesturePerformed(XRHandGesture gesture, Handedness handedness)
{
    Debug.Log($"Gesture: {gesture.gestureName} performed with {handedness}");
}
```

---

### 4. Configuring Interactables

#### Adding an Interactable to an Object

1. Add a **Collider** to the object (e.g. Box Collider).
2. Add **XR Grab Interactable** for grabbable objects, or **XR Simple Interactable** for objects that receive events without being moved.

```csharp
public class MyButton : MonoBehaviour
{
    [SerializeField] XRSimpleInteractable interactable;

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelected);
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        Debug.Log("Selected by: " + args.interactorObject);
    }
}
```

#### Interaction Layer Mask

XRIT 3.x uses **Interaction Layer Masks** to filter which interactors can interact with which interactables:

```csharp
interactable.interactionLayers = InteractionLayerMask.GetMask("Default", "UI");
```

---

### 5. UI Canvas and Interaction

#### World Space Canvas

1. Create a **Canvas** and set **Render Mode** to **World Space**.
2. Add the **Tracked Device Graphic Raycaster** component to the Canvas (replaces the standard Graphic Raycaster).
3. Add **XR UI Input Module** to the EventSystem GameObject in the scene (replaces the default Input System UI Input Module).

#### Recommended Canvas Settings

| Parameter | Recommended Value |
|---|---|
| Render Mode | World Space |
| Dynamic Pixels Per Unit | 1 |
| Reference Pixels Per Unit | 100 |
| Scale | 0.001 (for a ~1m canvas = 1000 pixel units) |

---

### 6. Combining Controller and Hand Tracking

On LeonardoXR it is not possible to simultaneously use headset Controllers and full Hand Tracking (with raycast from both hands) due to conflicts between OpenXR profiles.

#### Option A — Controller Only (Companion Controller)

- Add **Microsoft MMRC Profile** to enabled profiles.
- Do not add Hand Interaction Profile.
- The Companion Controller handles pointing; hands can be used as colliders but not as raycast interactors.

#### Option B — Hands Only (Dual Hand Mode)

- Add **Hand Interaction Profile** to enabled profiles.
- Do not add Microsoft MMRC Profile.
- Remove the Device Pointer Prefab from the Controller.
- Both hands tracked as full interactors; no phone input.

#### Option C — Controller + One Hand

- Add **Microsoft MMRC Profile** to enabled profiles.
- Keep only the Right Hand Controller active in the XR Origin.
- The left hand can still function as a collider and poke interaction trigger, but not as a ray interactor.

---

### 7. UniTask — Asynchronous Operations

UniTask 2.5.10 is included as a dependency for high-performance async operations (asset loading, waiting for subsystems, etc.) without GC allocations.

```csharp
using Cysharp.Threading.Tasks;

public class HandSubsystemWaiter : MonoBehaviour
{
    async UniTaskVoid Start()
    {
        // Wait until the XR Hands subsystem is ready
        await UniTask.WaitUntil(() =>
            XRHandSubsystemUtility.GetSubsystem()?.running ?? false);
        Debug.Log("XR Hands subsystem ready.");
    }
}
```

UniTask is used internally by the LeonardoXR SDK for asynchronous operations. It is recommended to use it in application code as well for consistency with the SDK lifecycle.

### OpenXR Interaction Profile Reference

| Profile | Use |
|---|---|
| Microsoft Mixed Reality Motion Controller Profile | Physical controllers interaction profile |
| Hand Interaction Profile | Hand tracking with poke/pinch interactor |
| LeonardoXR Hand Interaction Profile | Hand tracking with poke/pinch interactor |

> **NOTE** AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

---

## 06 Advanced

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

This section covers advanced configurations and features available on LeonardoXR, divided into general Snapdragon Spaces platform functionality and LeonardoXR proprietary advanced features.

---

### Advanced — Snapdragon Spaces

#### Adding a New Interaction Profile

To use an interaction profile not included in the Snapdragon Spaces plugin:

1. Go to **Project Settings > XR Plug-in Management > OpenXR (Android) > Interaction Profiles**.
2. Add the desired profile by clicking the **+** button.
3. Configure the corresponding Input Actions in the Input System Asset.

#### Custom Controller

Describes how to build a custom Android controller archive (`.aar`) using the Custom Controller Package — the Android Studio project included in the developer package.

**Process Overview**

1. Open the Android Studio project included in the Snapdragon Spaces developer package.
2. Modify the companion Activity as needed (layout, buttons, additional axes).
3. Build the project to obtain a `.aar` file.
4. Import the `.aar` into the Unity project as a native Android plugin (in `Assets/Plugins/Android`).
5. Configure the Android manifest to declare the new Activity.

> **NOTE** Adding a custom controller archive can cause Gradle caching errors. If build errors occur, delete the `Temp/gradleOut` folder and rebuild.

#### Display Refresh Rate

The `XR_EXT_display_refresh_rate` extension allows querying and setting the display refresh rate at runtime.

**Query Current Refresh Rate**

```csharp
using Qualcomm.Snapdragon.Spaces;

BaseRuntimeFeature baseRuntime = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
float currentHz = baseRuntime.GetDisplayRefreshRate();
Debug.Log($"Current refresh rate: {currentHz} Hz");
```

**Get Supported Refresh Rates**

```csharp
float[] supportedRates = baseRuntime.GetSupportedDisplayRefreshRates();
foreach (float rate in supportedRates)
    Debug.Log($"Supported: {rate} Hz");
```

**Set Refresh Rate**

```csharp
baseRuntime.RequestDisplayRefreshRate(90.0f);
```

> **NOTE** The requested refresh rate must be among those returned by `GetSupportedDisplayRefreshRates()`. Unsupported values are ignored.

#### Advanced Android Thread Performance

The `SpacesThreadUtility` component improves thread scheduling priority on Android, producing more consistent frame timing.

**Setup**

Add the `SpacesThreadUtility` component to a persistent GameObject in the scene (e.g. the same GameObject hosting the AR Session or XR Origin). No configuration is required — it activates automatically on initialisation and acts on the main Unity threads.

**When to Use**

Recommended for all LeonardoXR applications that exhibit framerate jitter or frame timing variability, especially in scenes with heavy rendering or many active AR subsystems.

#### Performance Settings

The OpenXR extension `XR_EXT_performance_settings` exposes performance level hints for CPU and GPU via a new method on `BaseRuntimeFeature`.

| Level | Description |
|---|---|
| PowerSavings | Reduces CPU/GPU clocks to the minimum necessary. Useful for simple scenes or static UI. |
| SustainedLow | Sustained performance at a low level. |
| SustainedHigh | Sustained high-level performance. Recommended for most AR apps. |
| Boost | Maximum performance for short periods (not sustainably maintained). |

```csharp
using Qualcomm.Snapdragon.Spaces;
using UnityEngine.XR.OpenXR;

BaseRuntimeFeature baseRuntime = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();
baseRuntime.SetPerformanceLevel(PerformanceDomain.Cpu, PerformanceLevel.SustainedHigh);
baseRuntime.SetPerformanceLevel(PerformanceDomain.Gpu, PerformanceLevel.SustainedHigh);
```

> **NOTE** The `Boost` level consumes significantly more power and may cause thermal throttling if sustained. Use it only for brief critical operations (e.g. loading a heavy scene).

---

### Advanced — LeonardoXR

#### QNN Inference Plugin — Advanced Guide

> **DOCUMENTATION IN PROGRESS** This section will cover advanced usage of the QNN (Qualcomm Neural Network) Inference Plugin on Leonardo XR, including:
> - Model preparation and conversion (ONNX → QNN format).
> - Loading models at runtime via plugin APIs.
> - Running inference on the NPU with input/output tensor management.
> - Memory management and throughput optimisation.
> - Debug and performance profiling of inference.
> - Use cases: object detection, pose estimation, real-time segmentation.
>
> Detailed documentation will be available in a future release.

#### Intercept OpenXR Function — Advanced Guide

> **DOCUMENTATION IN PROGRESS** This section will cover advanced usage of the Intercept OpenXR Function mechanism on Leonardo XR, including:
> - Architecture of the OpenXR interception layer.
> - Registering custom handlers for specific OpenXR functions.
> - Use cases: rendering pipeline override, custom data injection into the XR layer, low-level OpenXR debugging.
> - Safety and stability considerations.
> - Interaction with other LeonardoXR SDK components.
>
> This feature is intended for developers with direct experience of low-level OpenXR APIs. Detailed documentation will be available in a future release.
