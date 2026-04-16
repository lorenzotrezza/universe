using UnityEngine.XR.ARFoundation.Extensions;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Qualcomm.Snapdragon.Spaces;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

using UnityEngine.XR.OpenXR;


#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Youbiquo.LeonardoXR.Samples.DualCamera
{
    public class DualCameraFrameAccessController : MonoBehaviour
    {
        public bool RunSubsystemChecks = true;

        public List<GameObject> ContentOnPassed;
        public List<GameObject> ContentOnFailed;

        protected bool SubsystemChecksPassed;

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings")] 
#endif
        public XRDualCameraManager DualCameraManager;
        

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings/Camera Feed")] 
#endif
        public RawImage CameraRawImage;

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings/Camera Feed")] 
#endif
        public bool RenderUsingYUVPlanes;
        

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings/UI")] 
#endif
        public CameraInfoUI CameraInfoUI;

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings/UI")] 
#endif
        public Text ConfigNotFoundText;

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings/UI")] 
#endif
        public Transform CameraSelector;

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings/UI")] 
#endif
        public Button CameraButtonPrefab;


#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private int m_currentCameraId;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private List<string> _cameraSets;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private int m_sensorCount;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private NativeArray<XRCameraConfiguration> Configurations;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private List<Button> m_buttons;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private int m_currentConfiguration;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private Texture2D _cameraTexture;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private float _defaultAspectRatio;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private bool _feedPaused;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private XRCameraIntrinsics[] _intrinsics;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly] 
#endif
        [SerializeField]
        private XRCpuImage[] _lastCpuImage;

#if ODIN_INSPECTOR
        [FoldoutGroup("Debug"), Sirenix.OdinInspector.ReadOnly]
#endif
        [SerializeField]
        private Vector2 _maxTextureSize;

        private byte[] _rgbBuffer;

        public virtual void OnEnable()
        {
            SubsystemChecksPassed = GetSubsystemCheck();
        }

        public void Start()
        {
            foreach (var content in ContentOnPassed)
            {
                content.SetActive(SubsystemChecksPassed);
            }

            foreach (var content in ContentOnFailed)
            {
                content.SetActive(!SubsystemChecksPassed);
            }

            if (!SubsystemChecksPassed)
            {
#if !UNITY_EDITOR
                Debug.LogWarning("Subsystem checks failed. Some features may be unavailable.");
#endif
                return;
            }

            FindAvailableCameras();
            ChangeCamera(0);

            DualCameraManager.frameReceived += OnFrameReceived;

            _maxTextureSize = CameraRawImage.rectTransform.sizeDelta;
            _defaultAspectRatio = _maxTextureSize.x / _maxTextureSize.y;
        }

        protected bool GetSubsystemCheck()
        {
            return !RunSubsystemChecks || CheckSubsystem();
        }

        protected bool CheckSubsystem()
        {
            return DualCameraManager.subsystem?.running ?? false;
        }

        private void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            if (_feedPaused)
            {
                return;
            }

            if (!DualCameraManager.TryAcquireLatestCpuImages(out _lastCpuImage))
            {
#if !UNITY_EDITOR
                Debug.Log("Failed to acquire latest cpu images.");
#endif
                return;
            }

            UpdateCameraTexture(_lastCpuImage[m_currentCameraId], m_currentCameraId, RenderUsingYUVPlanes);
            // Update intrinsics on every frame, as intrinsics can change over time
            UpdateCameraIntrinsics();
        }

        private unsafe void UpdateCameraTexture(XRCpuImage image, int frameId, bool convertYuvManually)
        {
            var format = TextureFormat.RGBA32;

            // Down-sample to < 640px width for devices with large sensors
            // When converting the image manually, use native resolution
            var downsamplingFactor = Mathf.CeilToInt(image.width / 640);
            var outputDimensions = convertYuvManually ? image.dimensions : image.dimensions / downsamplingFactor;

            if (_cameraTexture == null || _cameraTexture.width != outputDimensions.x || _cameraTexture.height != outputDimensions.y)
            {
                _cameraTexture = new Texture2D(outputDimensions.x, outputDimensions.y, format, false);
                ResizeCameraFeed(outputDimensions);
            }

            var rawTextureData = _cameraTexture.GetRawTextureData<byte>();
            var rawTexturePtr = new IntPtr(rawTextureData.GetUnsafePtr());

            if (convertYuvManually)
            {
                switch (image.planeCount)
                {
                    // YUYV format - 1 plane (YUYV)
                    case 1:
                        ConvertYuyvImageIntoBuffer(image, frameId, rawTexturePtr, format);
                        break;
                    // YUV format - 2 planes (Y and UV)
                    case 2:
                        ConvertYuvImageIntoBuffer(image, frameId, rawTexturePtr, format);
                        break;
                }
            }
            else
            {
                var conversionParams = new XRCpuImage.ConversionParams(image, format);
                try
                {
                    conversionParams.inputRect = new RectInt(0, 0, image.width, image.height);
                    conversionParams.outputDimensions = outputDimensions;
                    image.Convert(conversionParams, frameId, rawTexturePtr, rawTextureData.Length);
                }
                finally
                {
                    image.Dispose();
                }
            }

            _cameraTexture.Apply();
            CameraRawImage.texture = _cameraTexture;
        }
        
        private void UpdateCameraIntrinsics()
        {
            if (!DualCameraManager.TryGetIntrinsics(out _intrinsics))
            {
                Debug.Log("Failed to acquire camera intrinsics.");
                return;
            }

            CameraInfoUI.ResolutionTexts[0].text = _intrinsics[m_currentCameraId].resolution.x.ToString();
            CameraInfoUI.ResolutionTexts[1].text = _intrinsics[m_currentCameraId].resolution.y.ToString();

            CameraInfoUI.FocalLengthTexts[0].text = _intrinsics[m_currentCameraId].focalLength.x.ToString("#0.00");
            CameraInfoUI.FocalLengthTexts[1].text = _intrinsics[m_currentCameraId].focalLength.y.ToString("#0.00");

            CameraInfoUI.PrincipalPointTexts[0].text = _intrinsics[m_currentCameraId].principalPoint.x.ToString("#0.00");
            CameraInfoUI.PrincipalPointTexts[1].text = _intrinsics[m_currentCameraId].principalPoint.y.ToString("#0.00");

            CameraInfoUI.FramerateText.text = Configurations[m_currentConfiguration].framerate + "";
            CameraInfoUI.DepthSensorSupportedToggle.isOn = Configurations[m_currentConfiguration].depthSensorSupported == Supported.Supported;
        }

        private void ResizeCameraFeed(Vector2Int outputDimensions)
        {
            var outputAspectRatio = outputDimensions.x / (float)outputDimensions.y;
            if (outputAspectRatio > _defaultAspectRatio)
            {
                CameraRawImage.rectTransform.sizeDelta = new Vector2(_maxTextureSize.x, _maxTextureSize.x / outputAspectRatio);
            }
            else
            {
                CameraRawImage.rectTransform.sizeDelta = new Vector2(_maxTextureSize.y * outputAspectRatio, _maxTextureSize.y);
            }
        }

        private void ConvertYuvImageIntoBuffer(XRCpuImage image, int frameId, IntPtr targetBuffer, TextureFormat format)
        {
            var bufferSize = image.height * image.width * (format == TextureFormat.RGB24 ? 3 : 4);

            if (_rgbBuffer == null || _rgbBuffer.Length != bufferSize)
            {
                _rgbBuffer = new byte[bufferSize];
            }

            var yPlane = image.GetPlane(0, frameId);
            var uvPlane = image.GetPlane(1, frameId);

            for (int row = 0; row < image.height; row++)
            {
                for (int col = 0; col < image.width; col++)
                {
                    var y = yPlane.data[row * yPlane.rowStride + col * yPlane.pixelStride];

                    var offset = (row / 2) * uvPlane.rowStride + (col / 2) * uvPlane.pixelStride;
                    sbyte u = (sbyte)(uvPlane.data[offset] - 128);
                    sbyte v = (sbyte)(uvPlane.data[offset + 1] - 128);

                    // YUV NV12 to RGB conversion
                    // https://en.wikipedia.org/wiki/YUV#Y%E2%80%B2UV420sp_(NV21)_to_RGB_conversion_(Android)
                    var r = y + (1.370705f * v);
                    var g = y - (0.698001f * v) - (0.337633f * u);
                    var b = y + (1.732446f * u);

                    r = r > 255 ? 255 : r < 0 ? 0 : r;
                    g = g > 255 ? 255 : g < 0 ? 0 : g;
                    b = b > 255 ? 255 : b < 0 ? 0 : b;

                    int pixelIndex = ((image.height - row - 1) * image.width) + col;

                    switch (format)
                    {
                        case TextureFormat.RGB24:
                            _rgbBuffer[3 * pixelIndex] = (byte)r;
                            _rgbBuffer[(3 * pixelIndex) + 1] = (byte)g;
                            _rgbBuffer[(3 * pixelIndex) + 2] = (byte)b;
                            break;
                        case TextureFormat.RGBA32:
                            _rgbBuffer[4 * pixelIndex] = (byte)r;
                            _rgbBuffer[(4 * pixelIndex) + 1] = (byte)g;
                            _rgbBuffer[(4 * pixelIndex) + 2] = (byte)b;
                            _rgbBuffer[(4 * pixelIndex) + 3] = 255;
                            break;
                        case TextureFormat.BGRA32:
                            _rgbBuffer[4 * pixelIndex] = (byte)b;
                            _rgbBuffer[(4 * pixelIndex) + 1] = (byte)g;
                            _rgbBuffer[(4 * pixelIndex) + 2] = (byte)r;
                            _rgbBuffer[(4 * pixelIndex) + 3] = 255;
                            break;
                    }
                }
            }
            Marshal.Copy(_rgbBuffer, 0, targetBuffer, bufferSize);
        }

        private void ConvertYuyvImageIntoBuffer(XRCpuImage image, int frameId, IntPtr targetBuffer, TextureFormat format)
        {
            var bufferSize = image.height * image.width * (format == TextureFormat.RGB24 ? 3 : 4);

            if (_rgbBuffer == null || _rgbBuffer.Length != bufferSize)
            {
                _rgbBuffer = new byte[bufferSize];
            }

            var yuyvPlane = image.GetPlane(0, frameId);

            for (int row = 0; row < image.height; row++)
            {
                for (int col = 0; col < image.width; col++)
                {
                    var y = yuyvPlane.data[row * yuyvPlane.rowStride + col * 2];

                    // Calculate offset of the YUYV byte group, select U (2nd byte) and V (4th byte)
                    var offset = row * yuyvPlane.rowStride + (col / 2) * yuyvPlane.pixelStride;
                    sbyte u = (sbyte)(yuyvPlane.data[offset + 1] - 128);
                    sbyte v = (sbyte)(yuyvPlane.data[offset + 3] - 128);

                    // YUV NV12 to RGB conversion
                    // https://en.wikipedia.org/wiki/YUV#Y%E2%80%B2UV420sp_(NV21)_to_RGB_conversion_(Android)
                    var r = y + (1.370705f * v);
                    var g = y - (0.698001f * v) - (0.337633f * u);
                    var b = y + (1.732446f * u);

                    r = r > 255 ? 255 : r < 0 ? 0 : r;
                    g = g > 255 ? 255 : g < 0 ? 0 : g;
                    b = b > 255 ? 255 : b < 0 ? 0 : b;

                    int pixelIndex = ((image.height - row - 1) * image.width) + col;

                    switch (format)
                    {
                        case TextureFormat.RGB24:
                            _rgbBuffer[3 * pixelIndex] = (byte)r;
                            _rgbBuffer[(3 * pixelIndex) + 1] = (byte)g;
                            _rgbBuffer[(3 * pixelIndex) + 2] = (byte)b;
                            break;
                        case TextureFormat.RGBA32:
                            _rgbBuffer[4 * pixelIndex] = (byte)r;
                            _rgbBuffer[(4 * pixelIndex) + 1] = (byte)g;
                            _rgbBuffer[(4 * pixelIndex) + 2] = (byte)b;
                            _rgbBuffer[(4 * pixelIndex) + 3] = 255;
                            break;
                        case TextureFormat.BGRA32:
                            _rgbBuffer[4 * pixelIndex] = (byte)b;
                            _rgbBuffer[(4 * pixelIndex) + 1] = (byte)g;
                            _rgbBuffer[(4 * pixelIndex) + 2] = (byte)r;
                            _rgbBuffer[(4 * pixelIndex) + 3] = 255;
                            break;
                    }
                }
            }
            Marshal.Copy(_rgbBuffer, 0, targetBuffer, bufferSize);
        }

        private void FindAvailableCameras()
        {
            LoadCameraConfigurations();
            LoadSensorCount();

            CreateCameraSelectorButtons();
        }
        
        private void LoadCameraConfigurations()
        {
            if (FindSupportedConfiguration() == 0)
                OnConfigurationNotFound();
        }

        private int FindSupportedConfiguration()
        {
            Configurations = DualCameraManager.GetConfigurations(Allocator.Persistent);
            m_currentConfiguration = Array.IndexOf(Configurations.ToArray(), DualCameraManager.currentConfiguration);

            return Configurations.Length;
        }

        private void LoadSensorCount()
        {
            m_sensorCount = DualCameraManager.TryGetSensorsCount();

            Debug.Log($"{m_sensorCount} sensors found.");
        }

        private void OnConfigurationNotFound()
        {
            foreach (var content in ContentOnPassed)
            {
                content.SetActive(false);
            }

            foreach (var content in ContentOnFailed)
            {
                content.SetActive(true);
            }

            ConfigNotFoundText.text = $"Could not find supported frame configuration.";
        }

        private void CreateCameraSelectorButtons()
        {
            for (int i = 0; i < m_sensorCount; i++)
                CreateCameraSelectorButton(i);

            if (m_buttons.Count > 0)
                m_buttons[0].onClick.Invoke();
        }

        private void CreateCameraSelectorButton(int cameraId)
        {
            Button button = Instantiate(CameraButtonPrefab, CameraSelector);
            button.GetComponentInChildren<Text>().text = $"Camera {cameraId}";
            button.onClick.AddListener(() => ChangeCamera(cameraId));
            m_buttons.Add(button);
        }

        private void UpdateSelectorButtons(int selectedCameraId)
        {
            foreach (Button b in m_buttons)
            {
                if (m_buttons.IndexOf(b) != selectedCameraId)
                    b.interactable = true;
                else
                    b.interactable = false;
            }
        }

        private void ChangeCamera(int cameraId)
        {
            if (m_sensorCount <= cameraId) return;

            UpdateSelectorButtons(selectedCameraId: cameraId);

            m_currentCameraId = cameraId;
        }

        public void OnPausePress()
        {
            _feedPaused = true;
        }

        public void OnResumePress()
        {
            _feedPaused = false;
        }
    }
}