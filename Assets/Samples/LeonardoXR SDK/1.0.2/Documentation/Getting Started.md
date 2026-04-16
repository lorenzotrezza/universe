# Getting Started

*Version 1.0.2 | Device: Youbiquo Leonardo XR | Base SDK: Snapdragon Spaces 1.0.4*

## Introduction

This guide explains how to configure a Unity project for development on Leonardo XR, the Youbiquo AR headset based on Snapdragon Spaces. The goal is to familiarise developers with the SDK configuration for building applications on Leonardo. The next guide covers how to interact with controllers and hands.

Leonardo XR runs Android 12 and follows a development workflow familiar to anyone who has previously built Android applications.

## Required Software

Before starting, make sure the following software and packages are installed at the specified versions:

| Software / Package | Version |
|---|---|
| Unity Hub | Latest available |
| Unity Editor | 6000.0.58f2 or newer|
| LeonardoXR SDK | 1.0.2 |
| Snapdragon Spaces SDK | 1.0.4 |
| OpenXR Plugin | 1.15.1 |
| AR Foundation | 6.2.1 |
| XR Interaction Toolkit | 3.3.0 |
| XR Hands | 1.7.2 |
| UniTask | 2.5.10 |
| VS Code, Visual Studio or similar IDE | — |

> **NOTE** Unlike the base Snapdragon Spaces guide (which recommends v1.10.0 for Unity 2022), Unity 6 with LeonardoXR uses OpenXR 1.15.1. Do not modify this version manually — the Package Manager handles it through the SDK dependencies.

## Create a New Project

1. Open Unity Hub and click **New Project**.
2. Select the **Universal 3D** template.
3. Name the project (e.g. `LeonardoXR_Test`) and choose a destination folder.
4. Click **Create Project**.

After creation, Unity opens the project with a sample scene in the Hierarchy panel.

![](imgs/introduction/NP_setup.png)

## Switch Platform to Android

The LeonardoXR is based on Android 12, so we will need to switch the target platform to Android.

1. From the main menu select **File > Build Profiles**.
2. In the Build Profiles window select **Android** from the Platform list.

![](imgs/introduction/PS_switch_platform.png)

3. Click **Switch Platform** and wait for the process to complete.
4. Close the Build Profiles window.

## Install Snapdragon Spaces SDK

> **IMPORTANT** Snapdragon Spaces SDK **must be installed before** the LeonardoXR SDK. The LeonardoXR runtime depends on it and will not compile without it.

Download Snapdragon Spaces SDK v1.0.4 from the Qualcomm developer portal (a developer account is required): https://spaces.qualcomm.com/developer/vr-mr-sdk/#downloads

1. Open the Package Manager (**Window > Package Manager**).
2. Click **+** and select **Add package from tarball…**

![](imgs/introduction/PM_install_tarball.png)

3. Browse to the downloaded `.tgz` file and click **Open**. Installation will start automatically.

![](imgs/introduction/PM_select_tarball_spaces.png)

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

## Install LeonardoXR SDK

Download LeonardoXR SDK v1.0.2 from the Youbiquo Developer area.

1. Open the Package Manager (**Window > Package Manager**).
2. Click **+** and select **Add package from tarball…**

![](imgs/introduction/PM_install_tarball.png)

3. Browse to the LeonardoXR SDK `.tgz` file and click **Open**.

![](imgs/introduction/PM_select_tarball_leo.png)

The SDK will be installed together with its dependencies: **OpenXR Plugin 1.15.1**, **AR Foundation 6.2.1**, **XR Interaction Toolkit 3.3.0**, and **XR Hands 1.7.2** — no manual installation of these packages is required.

## LeonardoXR SDK Manager

After the LeonardoXR SDK is installed, a **LeonardoXR SDK Manager** window opens automatically. If it does not appear, open it from **LeonardoXR > SDK Manager** in the menu bar.

![](imgs/introduction/Leonardo_XR_Tab.png)

![](imgs/introduction/Leonardo_XR_Tab_SDK_Manager.png)

The window has two tabs:

![](imgs/introduction/SDK_Manager_Setup_Tab.png)

![](imgs/introduction/SDK_Manager_About_Tab.png)

### Setup tab

Displays the status of the two packages that cannot be resolved automatically:

| Package | Status | Action |
|---|---|---|
| Snapdragon Spaces SDK 1.0.4 | ✓ / ✗ | **Browse…** — opens a file picker to select the `.tgz` and installs it via Package Manager |
| UniTask 2.5.10 | ✓ / ✗ | **Install** — installs UniTask directly from the official Cysharp Git repository |

![](imgs/introduction/SDK_Manager_Install_Packages.png)

Once both packages show ✓, the setup is complete.

![](imgs/introduction/SDK_Manager_All_Packages_Installed.png)

> **NOTE** If Snapdragon Spaces was already installed before the LeonardoXR SDK, its row will show ✓ automatically. Use the **↺ Refresh** button at any time to re-check the current state.

### About tab

Displays version information for the SDK, device, and Unity Editor.

## Configure XR Plugin Management and OpenXR

### Open Project Settings

Go to **Edit > Project Settings** and select **XR Plug-in Management** in the left sidebar.

### Enable OpenXR and Snapdragon Spaces

1. Click the **Android** tab (top right of the window).
2. Under **Plug-in Providers**, ensure both are checked: **OpenXR** and **Snapdragon Spaces feature group**.

![](imgs/introduction/PS_openxr_feature_groups.png)

### Configure OpenXR Features

1. Click **OpenXR** in the left sidebar.
2. In the Snapdragon Spaces feature list, enable only **Base Runtime** and disable all others. Advanced features (Hit Testing, Plane Detection, etc.) can be enabled later as needed.

![](imgs/introduction/PS_spaces_features.png)

> **NOTE** For Hand Tracking to work correctly on LeonardoXR, the **Hand Tracking Subsystem** option must be enabled in the **All Features** section of OpenXR.

### Add LeonardoXR Feature Group

1. Return to **XR Plug-in Management > Android**.
2. In the Plug-in Providers list, also check **LeonardoXR feature group**.

![](imgs/introduction/PS_enable_hts_feature.png)

### Run Project Validation

1. In the Project Settings sidebar click **Project Validation**.
2. The window shows a list of configuration errors to fix.

![](imgs/introduction/PS_project_validation.png)

3. Click **Fix All** to resolve them automatically.

> **NOTE** Some fixes may require an editor restart. Re-run validation after restarting to confirm all errors are resolved.

## Import Samples

Before creating your own scenes it is useful to import the samples included in each package. For each package: open **Window > Package Manager**, select **In Project**, select the package, expand the **Samples** section and click **Import**.

| Package | Samples |
|---|---|
| XR Hands | Gestures, Hand Visualizer |
| XR Interaction Toolkit | Starter Assets, Hand Interaction Demo |
| Snapdragon Spaces | Core Samples |
| LeonardoXR | Core Samples, Documentation |

![](imgs/introduction/PM_import_samples_xrhands.png)

![](imgs/introduction/Documentation_Sample_To_Import.png)

![](imgs/introduction/Documentation_Sample_Imported.png)

After importing all samples, add scenes to the build via **Window > XR > Snapdragon Spaces > Add Scenes to Build Settings**.

## Configure URP

### Graphics and Quality Settings

1. Open **Edit > Project Settings > Graphics**.

![](imgs/introduction/URP_0png.png)

2. Under **Default Render Pipeline**, click the circle and select `Mobile_RPAsset`.

![](imgs/introduction/URP_1.png)

3. Go to **Project Settings > Quality**.
4. In the Levels section, ensure **Mobile** has a green checkbox in the Android column. If not, click the **Default** triangle under Android and select **Mobile**.

![](imgs/introduction/URP_2.png)

5. Close Project Settings.

### Configure URP Assets

In `Assets/Settings`, select `Mobile_Renderer` and apply:

![](imgs/introduction/URP_3.png)

- **Rendering → Depth Priming Mode**: set to `Disabled`
- **Shadows → Transparent Receive Shadows**: uncheck
- **Post-processing → Enabled**: uncheck
- **Renderer Feature → SSAO**: ensure it is disabled or removed

![](imgs/introduction/URP_4.png)

In `Assets/Settings`, select `Mobile_RPAsset` and apply:

![](imgs/introduction/URP_5.png)

- **HDR**: disable
- **Anti Aliasing (MSAA)**: set to `4x`

![](imgs/introduction/URP_6.png)

## Play In Editor Configuration

This section describes how to configure the project for Play-In-Editor development, reducing the need to deploy to the device on every change.

### OpenXR Project Settings

Go to **Edit > Project Settings > XR Plug-in Management** and select the **Windows, Mac, Linux** tab.

**Initialize XR On Startup** — the value must match the Android tab setting:
- Without Dual Render Fusion → must be **enabled**.
- With Dual Render Fusion → must be **disabled**.

> **NOTE** Enable **XR Simulation** on the Windows/Mac/Linux tab to use AR Foundation simulation environments. Note that QR Code Tracking has no simulation subsystem equivalent.

### Enable Base Runtime Feature for Windows/Mac/Linux

Go to **Edit > Project Settings > XR Plug-in Management > OpenXR** (Windows, Mac, Linux tab) and enable the **Base Runtime** feature.

### Scene Configuration — Spaces XR Simulator

Add the **Spaces XR Simulator** component to a root GameObject in the scene. The component persists across scene loads (`DontDestroyOnLoad`) and only one instance can exist at a time.

| Property | Description |
|---|---|
| Start Connected | Simulates glasses active and connected (Dual Render Fusion only). |
| Invert Sim Camera Display | Swaps phone and glasses displays in the editor (Dual Render Fusion only). |

### Dual Render Fusion Editor Shortcuts

| Event | Shortcut | Menu Path |
|---|---|---|
| Connect Glasses | Alt-Shift-C | Window > XR > Snapdragon Spaces > DRF > Simulation > Connect Glasses |
| Disconnect Glasses | Alt-Shift-D | Window > XR > Snapdragon Spaces > DRF > Simulation > Disconnect Glasses |
| Glasses Active | Alt-Shift-G | Window > XR > Snapdragon Spaces > DRF > Simulation > Glasses Active |
| Glasses Idle | Alt-Shift-H | Window > XR > Snapdragon Spaces > DRF > Simulation > Glasses Idle |

> **NOTE** All final behaviour must always be verified on the physical device, not only in the editor.
