# VR / MR

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

## Manual Project Configuration

It is recommended to use the Configuration Tool for initial project setup (see Getting Started). This guide describes the equivalent manual steps for developers who prefer direct control over settings.

### Edit Project Settings

1. Go to **Edit > Project Settings > XR Plug-in Management** and open the **Android** tab.
2. Enable the **OpenXR** plug-in and the **Snapdragon Spaces** feature group.
3. Click the red exclamation mark next to OpenXR to open **OpenXR Project Validation**.
4. Click **Fix** next to each item.
5. Apply the "Enable both Input Systems" setting last, as it requires an editor restart.

### Enable Spaces Features

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

### Add Feature Usage Data to the Android Manifest

Go to **Edit > Project Settings > Snapdragon Spaces Launcher Settings** to declare features required or supported by the application for: Hand Tracking, Eye Tracking, Passthrough, Controllers and Room Scale.

> **NOTE** By default all `uses-feature` values are set to `true`. Set unused features to `false` to avoid false requirements in the manifest.

## Passthrough Setup

Passthrough allows the physical environment to be displayed as an image overlay on VR devices. On LeonardoXR it is available on compatible devices.

- Activated via the **Extend Content** panel in the in-app UI (visible only on Passthrough-compatible devices).
- Can also be toggled with the **X** and **A** buttons on the controller.
- The enable function is `OnPassthroughToggle()` in `MainMenuSampleController`, which sets `PassthroughToggle` on `BaseRuntimeFeature`.

> **CAUTION** The session camera must have the **Alpha channel** of the Background set to `0` for Passthrough to work correctly. A warning is automatically logged if settings are incorrect.

The `OnSpacesAppSpaceChange` delegate in `BaseRuntimeFeature` notifies when the device performs a recenter (activated by holding the menu button for 2 seconds on the VRX).

## Scene Setup

> **NOTE** This guide assumes prior knowledge of AR Foundation and OpenXR. Refer to the official AR Foundation and OpenXR documentation for foundational concepts.

### Minimum Scene Hierarchy

The following objects are required to enable positional head tracking on LeonardoXR:

```
AR Session
XR Origin
  └── Camera Offset
        └── Main Camera (tagged as "MainCamera")
```

Create them by right-clicking in the Hierarchy and selecting **XR > AR Session** or **XR > XR Origin**.

> **NOTE** Disable the **AR Camera Manager** component if you are not retrieving RGB frames from the camera, to avoid lifecycle issues. Also disable **AR Camera Background** — it is not supported and will be automatically disabled at runtime.

## Application Lifecycle — Older Runtime Compatibility

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

## Known Issues — VR/MR on LeonardoXR

> **NOTE** For issues not listed here, refer to the general known issues for the Snapdragon Spaces platform.

### Gradle Build Issue

Adding a custom controller archive can cause Gradle caching errors. Deleting the `Temp/gradleOut` folder generally resolves the issue.

### AR Foundation Issues

- **AR Raycast Hit returns wrong Trackable ID** — `ARRaycastHit.trackableId` always returns `0-0`.
- **Plane Detection stalled** — Mesh triangulation enters an infinite loop in AR Foundation 6.2.1.

### XRIT Issues

- **AR Raycast Manager added automatically** — XRIT 3.x adds an AR Raycast Manager when an XR Raycast Interactor is present, enabling Spatial Meshing regardless of the `EnableARRaycasting` flag. This may reduce performance.
- **Unsupported AR Mesh Manager properties** — Using unsupported properties generates runtime warnings.
- **AR Camera Manager / AR Camera Background Issues** — Moving between scenes with AR Camera Manager enabled can cause freezes. Disable AR Camera Background to avoid rendering issues.

### Play In Editor Known Issues

- **Memory Leak in Camera Access Simulation** — Resolved in AR Foundation 6.0+.
- **AR Mesh Manager crash without Normals** — Update AR Foundation or enable normals.
- **QR Code Tracking not loaded in Simulation** — No simulation subsystem exists; bypass subsystem checks in editor builds.
- **ScriptableSingleton Warning** — Resolved in AR Foundation 5.1.2+.
