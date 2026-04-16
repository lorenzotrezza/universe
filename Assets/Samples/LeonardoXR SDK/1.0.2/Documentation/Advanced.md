# Advanced

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

This section covers advanced configurations and features available on LeonardoXR, divided into general Snapdragon Spaces platform functionality and LeonardoXR proprietary advanced features.

---

## Advanced — Snapdragon Spaces

### Adding a New Interaction Profile

To use an interaction profile not included in the Snapdragon Spaces plugin:

1. Go to **Project Settings > XR Plug-in Management > OpenXR (Android) > Interaction Profiles**.
2. Add the desired profile by clicking the **+** button.
3. Configure the corresponding Input Actions in the Input System Asset.

### Custom Controller

Describes how to build a custom Android controller archive (`.aar`) using the Custom Controller Package — the Android Studio project included in the developer package.

**Process Overview**

1. Open the Android Studio project included in the Snapdragon Spaces developer package.
2. Modify the companion Activity as needed (layout, buttons, additional axes).
3. Build the project to obtain a `.aar` file.
4. Import the `.aar` into the Unity project as a native Android plugin (in `Assets/Plugins/Android`).
5. Configure the Android manifest to declare the new Activity.

> **NOTE** Adding a custom controller archive can cause Gradle caching errors. If build errors occur, delete the `Temp/gradleOut` folder and rebuild.

### Display Refresh Rate

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

### Advanced Android Thread Performance

The `SpacesThreadUtility` component improves thread scheduling priority on Android, producing more consistent frame timing.

**Setup**

Add the `SpacesThreadUtility` component to a persistent GameObject in the scene (e.g. the same GameObject hosting the AR Session or XR Origin). No configuration is required — it activates automatically on initialisation and acts on the main Unity threads.

**When to Use**

Recommended for all LeonardoXR applications that exhibit framerate jitter or frame timing variability, especially in scenes with heavy rendering or many active AR subsystems.

### Performance Settings

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

## Advanced — LeonardoXR

### QNN Inference Plugin — Advanced Guide

> **DOCUMENTATION IN PROGRESS** This section will cover advanced usage of the QNN (Qualcomm Neural Network) Inference Plugin on Leonardo XR, including:
> - Model preparation and conversion (ONNX → QNN format).
> - Loading models at runtime via plugin APIs.
> - Running inference on the NPU with input/output tensor management.
> - Memory management and throughput optimisation.
> - Debug and performance profiling of inference.
> - Use cases: object detection, pose estimation, real-time segmentation.
>
> Detailed documentation will be available in a future release.

### Intercept OpenXR Function — Advanced Guide

> **DOCUMENTATION IN PROGRESS** This section will cover advanced usage of the Intercept OpenXR Function mechanism on Leonardo XR, including:
> - Architecture of the OpenXR interception layer.
> - Registering custom handlers for specific OpenXR functions.
> - Use cases: rendering pipeline override, custom data injection into the XR layer, low-level OpenXR debugging.
> - Safety and stability considerations.
> - Interaction with other LeonardoXR SDK components.
>
> This feature is intended for developers with direct experience of low-level OpenXR APIs. Detailed documentation will be available in a future release.
