# Interaction Package Guides

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

This guide describes how to configure the interaction layer for LeonardoXR applications using standard Unity packages:

- **XR Interaction Toolkit (XRIT) 3.3.0** — high-level interaction system for controllers, ray interactors and direct interactors.
- **XR Hands 1.7.2** — access to hand joint data and gesture recognition.

## Dependencies and Versions

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

## 1. XR Interaction Toolkit Configuration

### Installation

Install via **Window > Package Manager > Unity Registry > XR Interaction Toolkit > Install** (version 3.3.0). If prompted "Enable the new input system?", click **Yes**. If prompted "XR Interaction Layer Mask Update Required", click **I Made a Backup, Go Ahead!** (for new projects) or **No Thanks** if no previous XRIT version was present.

### Import Starter Assets

1. Open **Window > Package Manager**, select **In Project** then **XR Interaction Toolkit**.
2. Under **Samples**, click **Import** on **Starter Assets**.
3. Also click **Import** on **Hand Interaction Demo** (required for hand tracking samples).

The Starter Assets include:
- Pre-configured Input Action Asset for XR controllers and hands.
- Reference prefabs for XR Origin with Ray Interactor and Direct Interactor.
- Base materials and shaders for interaction visual feedback.

### Configure Input Actions

After import, an `XRI Default Input Actions` asset is added to `Assets/Samples/XR Interaction Toolkit/[version]/Starter Assets/`. This asset is pre-configured for standard controller profiles (including the Companion Controller via Microsoft MMRC Profile) and for hands.

To associate the asset with the system: go to **Edit > Project Settings > XR Plug-in Management > Input System** and ensure **Active Input Handling** is set to `Both` or `Input System Package (New)`.

---

## 2. Scene Configuration — XR Origin

### Base Scene Hierarchy

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

### Input Action Manager

Add the **Input Action Manager** component to the XR Origin (or a persistent GameObject). In the **Action Assets** field, assign:

- `XR Input Actions` (from `Samples/LeonardoXR SDK/[version]/Core Samples/Shared Assets`).

---

## 3. XR Hands Configuration

### Installation

Install via **Window > Package Manager > Unity Registry > XR Hands > Install** (version 1.7.2).

### Import Hand Samples

Open **Window > Package Manager > In Project > XR Hands > Samples** and import:
- **Gestures** — gesture recognition system.
- **Hand Visualizer** — hand mesh visualisation.

### Enable Hand Tracking Subsystem

1. Go to **Edit > Project Settings > XR Plug-in Management > OpenXR** (Android tab).
2. Under **All Features**, enable **Hand Tracking Subsystem**.
3. Also enable **Hand Tracking** in the Snapdragon Spaces feature group.

> **NOTE** The Hand Tracking Subsystem must be enabled in **All Features** — not just in the Snapdragon Spaces section — to work correctly on LeonardoXR.

### Hand Data Access via XR Hands API

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

### Gesture Recognition

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

## 4. Configuring Interactables

### Adding an Interactable to an Object

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

### Interaction Layer Mask

XRIT 3.x uses **Interaction Layer Masks** to filter which interactors can interact with which interactables:

```csharp
interactable.interactionLayers = InteractionLayerMask.GetMask("Default", "UI");
```

---

## 5. UI Canvas and Interaction

### World Space Canvas

1. Create a **Canvas** and set **Render Mode** to **World Space**.
2. Add the **Tracked Device Graphic Raycaster** component to the Canvas (replaces the standard Graphic Raycaster).
3. Add **XR UI Input Module** to the EventSystem GameObject in the scene (replaces the default Input System UI Input Module).

### Recommended Canvas Settings

| Parameter | Recommended Value |
|---|---|
| Render Mode | World Space |
| Dynamic Pixels Per Unit | 1 |
| Reference Pixels Per Unit | 100 |
| Scale | 0.001 (for a ~1m canvas = 1000 pixel units) |

---

## 6. Combining Controller and Hand Tracking

On LeonardoXR it is not possible to simultaneously use headset Controllers and full Hand Tracking (with raycast from both hands) due to conflicts between OpenXR profiles.

### Option A — Controller Only (Companion Controller)

- Add **Microsoft MMRC Profile** to enabled profiles.
- Do not add Hand Interaction Profile.
- The Companion Controller handles pointing; hands can be used as colliders but not as raycast interactors.

### Option B — Hands Only (Dual Hand Mode)

- Add **Hand Interaction Profile** to enabled profiles.
- Do not add Microsoft MMRC Profile.
- Remove the Device Pointer Prefab from the Controller.
- Both hands tracked as full interactors; no phone input.

### Option C — Controller + One Hand

- Add **Microsoft MMRC Profile** to enabled profiles.
- Keep only the Right Hand Controller active in the XR Origin.
- The left hand can still function as a collider and poke interaction trigger, but not as a ray interactor.

---

## 7. UniTask — Asynchronous Operations

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

## OpenXR Interaction Profile Reference

| Profile | Use |
|---|---|
| Microsoft Mixed Reality Motion Controller Profile | Physical controllers interaction profile |
| Hand Interaction Profile | Hand tracking with poke/pinch interactor |
| LeonardoXR Hand Interaction Profile | Hand tracking with poke/pinch interactor |
