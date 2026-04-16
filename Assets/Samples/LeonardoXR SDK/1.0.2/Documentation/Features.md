# Features

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

This page documents the features available in the LeonardoXR SDK, organised in two sections:

- **Snapdragon Spaces Features** — AR/XR functionality provided by the Snapdragon Spaces platform, accessible through AR Foundation.
- **LeonardoXR Features** — proprietary features specific to Leonardo XR.

---

## Snapdragon Spaces Features

### Local Anchors

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

### Hit Testing

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

### Image Tracking

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Image Tracking**. Uses the **AR Tracked Image Manager** (`XRImageTrackingSubsystem`). Supports static and mutable (runtime) reference image libraries.

```csharp
// Mutable library example
var library = trackedImageManager.CreateRuntimeLibrary() as MutableRuntimeReferenceImageLibrary;
await library.ScheduleAddImageWithValidationJob(texture, "image-name", 0.1f); // 0.1f = physical width in metres
trackedImageManager.referenceLibrary = library;
```

---

### Plane Detection

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Plane Detection**. Uses the **AR Plane Manager** (`XRPlaneSubsystem`). *Use Scene Understanding Backend* is enabled by default.

```csharp
arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
```

> **CAUTION** Plane Detection enters an infinite mesh triangulation loop in AR Foundation 6.0. In Unity 6 + OpenXR 1.15.1, Plane Detection does not work in Dual Render Fusion apps.

---

### Camera Frame Access

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

### Spatial Meshing

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Spatial Meshing**. Uses the **AR Mesh Manager** (`XRMeshSubsystem`). The optional `SpacesARMeshManagerConfig` component controls mesh characteristics and camera height offsets for `TrackingOriginMode.Floor`.

---

### QR Code Tracking

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

### Hand Tracking

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Hand Tracking**. Hand Tracking is provided by the QCHT (Qualcomm Hand Tracking) plugin packages. LeonardoXR uses the Unity XR Hands package (v1.7.2) as the data access layer for joint data, integrated with QCHT extensions.

> **NOTE** For Hand Tracking to work correctly on LeonardoXR, the **Hand Tracking Subsystem** option must be active in **All Features** of OpenXR.

For full Hand Tracking integration with XR Interaction Toolkit, see the Interaction Package Guides document.

---

### Foveated Rendering

Enable via **Project Settings > XR Plug-in Management > OpenXR (Android) > Snapdragon Spaces > Foveated Rendering**. Reduces GPU load by rendering the visual field periphery at lower resolution. Foveation settings persist after application pause and resume.

| Level | Description |
|---|---|
| None | No reduction. |
| Low | Light reduction, imperceptible to most users. |
| Medium | Good quality/performance trade-off. |
| High | Maximum reduction; recommended only for heavily 3D content scenes. |

---

## LeonardoXR Features

The following features are specific to Leonardo XR and are not part of the base Snapdragon Spaces platform. Enable them via **Project Settings > XR Plug-in Management > OpenXR (Android) > LeonardoXR feature group**.

### Dual Camera Access

> **DOCUMENTATION IN PROGRESS** This feature provides synchronised access to frames from both Leonardo XR cameras. Detailed API documentation, configuration and samples will be available in a future release.

### QNN Inference Plugin

> **DOCUMENTATION IN PROGRESS** This feature exposes Leonardo XR hardware neural inference capabilities via the Qualcomm Neural Network (QNN) runtime, enabling AI/ML models to run directly on the device NPU. Detailed API documentation, model conversion workflow and samples will be available in a future release.

### Intercept OpenXR Function

> **DOCUMENTATION IN PROGRESS** This advanced feature provides a mechanism to intercept and override low-level OpenXR functions. It is intended for specialised use cases requiring direct control over the OpenXR layer. Detailed documentation will be available in a future release.
