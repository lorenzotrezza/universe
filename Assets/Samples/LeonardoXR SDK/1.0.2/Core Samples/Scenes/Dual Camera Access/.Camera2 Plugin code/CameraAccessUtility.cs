using UnityEngine.UI;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class CameraAccessUtility : MonoBehaviour
{
    [SerializeField] private RawImage displayImage;
    [SerializeField] private AspectRatioFitter aspectRatioFitter;
    
    private AndroidJavaObject cameraManager;
    private bool isCameraInitialized = false;
    private bool isCameraRunning = false;
    private string[] cameraIds;
    
    // Fields for Camera2 API
    private AndroidJavaObject cameraDevice;
    private AndroidJavaObject captureSession;
    private AndroidJavaObject imageReader;
    private Texture2D targetTexture;
    private byte[] frameData;
    private bool frameAvailable = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        try {
            if (Application.platform == RuntimePlatform.Android)
                InitializeAndroidCamera();
        } catch (System.Exception e) {
            Debug.LogError("|| Camera Access || Start | Error | Start failed: " + e.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isCameraRunning && displayImage != null)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                // Update texture with Camera2 API data
                if (frameAvailable && targetTexture != null)
                {
                    lock (frameData)
                    {
                        targetTexture.LoadRawTextureData(frameData);
                        targetTexture.Apply();
                        frameAvailable = false;
                    }
                }
                
                if (targetTexture != null)
                {
                    displayImage.texture = targetTexture;
                    
                    // Set aspect ratio based on texture dimensions
                    float ratio = (float)targetTexture.width / (float)targetTexture.height;
                    aspectRatioFitter.aspectRatio = ratio;
                }
            }
        }
    }

    private void OnDestroy()
    {
        StopCamera();

        if (Application.platform == RuntimePlatform.Android)
        {
            if (cameraDevice != null)
            {
                cameraDevice.Call("close");
                cameraDevice = null;
            }

            if (imageReader != null)
            {
                imageReader.Call("close");
                imageReader = null;
            }
        }
    }

    private void InitializeAndroidCamera()
    {
        using AndroidJavaClass unityPlayer = new("com.unity3d.player.UnityPlayer");
        using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        // Get the CameraManager service
        cameraManager = context.Call<AndroidJavaObject>("getSystemService", "camera");

        if (cameraManager != null)
        {
            // Get available camera IDs
            try
            {
                // Usa direttamente la conversione a string[]
                cameraIds = cameraManager.Call<string[]>("getCameraIdList");

                string selectedCameraId = null;

                for (int i = 0; i < cameraIds.Length; i++)
                {
                    Debug.Log("|| Camera Access || InitializeAndroidCamera | Log | Camera ID: " + cameraIds[i]);

                    // Get camera characteristics
                    AndroidJavaObject characteristics = cameraManager.Call<AndroidJavaObject>("getCameraCharacteristics", cameraIds[i]);

                    // Check camera facing
                    AndroidJavaClass cameraCharacteristicsClass = new("android.hardware.camera2.CameraCharacteristics");
                    AndroidJavaObject lensFacing = characteristics.Call<AndroidJavaObject>("get", cameraCharacteristicsClass.GetStatic<AndroidJavaObject>("LENS_FACING"));

                    if (lensFacing != null)
                    {
                        int facing = lensFacing.Call<int>("intValue");
                        Debug.Log("|| Camera Access || InitializeAndroidCamera | Log | Camera " + cameraIds[i] + " facing: " + facing);

                        // Prefer back camera (usually facing = 1)
                        if (facing == 1)
                        {
                            selectedCameraId = cameraIds[i];
                        }
                        // If no preference yet, use this camera
                        else selectedCameraId ??= cameraIds[i];
                    }
                }

                if (selectedCameraId != null)
                {
                    SetupCamera2(currentActivity, selectedCameraId);
                    isCameraInitialized = true;
                }
                else
                {
                    Debug.LogError("|| Camera Access || InitializeAndroidCamera | Error | No suitable camera found");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("|| Camera Access || InitializeAndroidCamera | Error | Error getting camera IDs: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("|| Camera Access || InitializeAndroidCamera | Error | Failed to get CameraManager");
        }
    }

    private int CheckCameraDeviceState(string cameraId)
    {
        try
        {
            AndroidJavaObject characteristics = cameraManager.Call<AndroidJavaObject>("getCameraCharacteristics", cameraId);
            AndroidJavaClass cameraCharacteristicsClass = new("android.hardware.camera2.CameraCharacteristics");
            AndroidJavaObject cameraState = characteristics.Call<AndroidJavaObject>("get", 
                cameraCharacteristicsClass.GetStatic<AndroidJavaObject>("CAMERA_STATE"));
            
            return cameraState?.Call<int>("intValue") ?? -1;
        }
        catch (System.Exception e)
        {
            Debug.LogError("|| Camera Access || CheckCameraDeviceState | Error | Failed to check camera state: " + e.Message);
            return -1;
        }
    }

    public AndroidJavaObject GetCameraDevice(string cameraId)
    {
        try
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.LogWarning("|| Camera Access || GetCameraDevice | Warning | Method only available on Android");
                return null;
            }

            if (cameraManager == null)
            {
                Debug.LogError("|| Camera Access || GetCameraDevice | Error | CameraManager is null");
                return null;
            }

            return new AndroidJavaObject("android.hardware.camera2.CameraDevice", cameraId);
        }
        catch (System.Exception e)
        {
            Debug.LogError("|| Camera Access || GetCameraDevice | Error | Failed to get camera device: " + e.Message);
            return null;
        }
    }

    private void SetupCamera2(AndroidJavaObject activity, string cameraId)
    {
        try
        {
            // Get camera characteristics to determine resolution
            AndroidJavaObject characteristics = cameraManager.Call<AndroidJavaObject>("getCameraCharacteristics", cameraId);
            
            // Get stream configuration map
            AndroidJavaClass cameraCharacteristicsClass = new("android.hardware.camera2.CameraCharacteristics");
            AndroidJavaObject configMap = characteristics.Call<AndroidJavaObject>("get", 
                cameraCharacteristicsClass.GetStatic<AndroidJavaObject>("SCALER_STREAM_CONFIGURATION_MAP"));
            
            // Get output sizes for ImageReader
            AndroidJavaClass imageFormatClass = new("android.graphics.ImageFormat");
            int imageFormat = imageFormatClass.GetStatic<int>("YUV_420_888");
            
            // Fix the approach to get output sizes
            int width = 1280; // Default fallback
            int height = 720; // Default fallback
            
            try {
                // Get the output sizes as a Size[] array
                using AndroidJavaObject sizeArray = configMap.Call<AndroidJavaObject>("getOutputSizes", imageFormat);
                if (sizeArray != null)
                {
                    // Check if it's an array
                    using AndroidJavaClass arrayClass = new("java.lang.reflect.Array");
                    int length = arrayClass.CallStatic<int>("getLength", sizeArray);
                    if (length > 0)
                    {
                        // Get the first size object
                        using AndroidJavaObject firstSize = arrayClass.CallStatic<AndroidJavaObject>("get", sizeArray, 0);
                        if (firstSize != null)
                        {
                            width = firstSize.Call<int>("getWidth");
                            height = firstSize.Call<int>("getHeight");
                            Debug.Log("|| Camera Access || SetupCamera2 | Log | Camera resolution: " + width + "x" + height);
                        }
                    }
                }
            }
            catch (System.Exception e) {
                Debug.LogWarning("|| Camera Access || SetupCamera2 | Warning | Could not get camera size, using default: " + e.Message);
            }
            
            // Create texture in Unity
            targetTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            frameData = new byte[width * height * 4]; // RGBA = 4 bytes per pixel
            
            // Create ImageReader
            AndroidJavaClass imageReaderClass = new("android.media.ImageReader");
            imageReader = imageReaderClass.CallStatic<AndroidJavaObject>("newInstance", width, height, imageFormat, 2);
            
            // Set up ImageReader listener
            AndroidJavaClass looperClass = new("android.os.Looper");
            AndroidJavaObject mainLooper = looperClass.CallStatic<AndroidJavaObject>("getMainLooper");
            AndroidJavaObject handler = new AndroidJavaObject("android.os.Handler", mainLooper);
            
            // Create OnImageAvailableListener
            ImageAvailableListener imageAvailableListener = new(this);
            imageReader.Call("setOnImageAvailableListener", imageAvailableListener, handler);

            // Verifica permessi
            bool hasPermission = false;

#if UNITY_ANDROID && !UNITY_EDITOR
            hasPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
#endif

            if (!hasPermission)
            {
                Debug.LogWarning("|| Camera Access || SetupCamera2 | Warning | Camera permission not granted, trying requiring manually.");

#if UNITY_ANDROID && !UNITY_EDITOR

                Permission.RequestUserPermission(Permission.Camera);
#endif

                // Verifica di nuovo i permessi
#if UNITY_ANDROID && !UNITY_EDITOR
                hasPermission = Permission.HasUserAuthorizedPermission(Permission.Camera);
#endif

                if (!hasPermission)
                {
                    Debug.LogError("|| Camera Access || SetupCamera2 | Error | Camera permission not granted, CANNOT require manually.");
                    return;
                }
                else
                {
                    Debug.Log("|| Camera Access || SetupCamera2 | Log | Camera permission acquired.");
                }
            }

            //// Verifica se la camera è già aperta
            //int cameraState = CheckCameraDeviceState(cameraId);

            //if (cameraState == 1) // 1 = CAMERA_STATE_OPEN
            //{
            //    Debug.LogWarning("|| Camera Access || SetupCamera2 | Warning | Camera is already open, no callbacks registered, using existing instance");

            //    try
            //    {
            //        cameraDevice = GetCameraDevice(cameraId);
            //        CreateCaptureSession();
            //    }
            //    catch (System.Exception e)
            //    {
            //        Debug.LogError("|| Camera Access || SetupCamera2 | Error | " + e.Message);
            //    }
            //}
            //else
            //{
                // Open camera
                CameraStateCallback stateCallback = new CameraStateCallback(this);

                try
                {
                    cameraManager.Call("openCamera", cameraId, stateCallback, handler);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("|| Camera Access || SetupCamera2 | Error | " + e.Message);
                }
            //}
        }
        catch (System.Exception e)
        {
            Debug.LogError("|| Camera Access || SetupCamera2 | Error | SetupCamera2 failed: " + e.Message);
        }
    }
    
    private void CreateCaptureSession()
    {
        try
        {
            if (cameraDevice == null || imageReader == null)
            {
                Debug.LogError("|| Camera Access || CreateCaptureSession | Error | Cannot create capture session, camera or imageReader is null");
                return;
            }
            
            // Create output surface list
            AndroidJavaClass listClass = new("java.util.ArrayList");
            AndroidJavaObject surfaces = listClass.Call<AndroidJavaObject>("new");
            
            AndroidJavaObject surface = imageReader.Call<AndroidJavaObject>("getSurface");
            surfaces.Call<bool>("add", surface);
            
            // Create capture session
            CaptureSessionStateCallback sessionCallback = new(this);
            
            cameraDevice.Call("createCaptureSession", surfaces, sessionCallback, null);
        }
        catch (System.Exception e)
        {
            Debug.LogError("|| Camera Access || CreateCaptureSession | Error | Error creating capture session: " + e.Message);
        }
    }
    
    private void StartCameraPreview()
    {
        try
        {
            if (cameraDevice == null || captureSession == null)
            {
                Debug.LogWarning("|| Camera Access || StartCameraPreview | Warning | Camera device or capture session is null");
                return;
            }
            
            // Create CaptureRequest.Builder
            AndroidJavaClass captureRequestClass = new("android.hardware.camera2.CaptureRequest");
            AndroidJavaObject requestBuilder = cameraDevice.Call<AndroidJavaObject>("createCaptureRequest", 
                captureRequestClass.GetStatic<int>("TEMPLATE_PREVIEW"));
            
            // Add target surface
            AndroidJavaObject surface = imageReader.Call<AndroidJavaObject>("getSurface");
            requestBuilder.Call("addTarget", surface);
            
            // Build request
            AndroidJavaObject captureRequest = requestBuilder.Call<AndroidJavaObject>("build");
            
            // Start repeating request
            captureSession.Call("setRepeatingRequest", captureRequest, null, null);
            
            isCameraRunning = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("|| Camera Access || StartCameraPreview | Error | Error starting camera preview: " + e.Message);
        }
    }
    
    private void ProcessImageToRGBA(AndroidJavaObject image, byte[] targetArray)
    {
        try
        {
            // Get image planes using reflection for safer array access
            AndroidJavaObject[] planes = null;
            
            try {
                using AndroidJavaObject planesObj = image.Call<AndroidJavaObject>("getPlanes");
                using AndroidJavaClass arrayClass = new("java.lang.reflect.Array");
                int length = arrayClass.CallStatic<int>("getLength", planesObj);
                planes = new AndroidJavaObject[length];

                for (int i = 0; i < length; i++)
                {
                    planes[i] = arrayClass.CallStatic<AndroidJavaObject>("get", planesObj, i);
                }
            }
            catch (System.Exception e) {
                Debug.LogError("Error getting image planes: " + e.Message);
                return;
            }
            
            if (planes != null && planes.Length >= 3)
            {
                // Get Y, U, V planes
                AndroidJavaObject yPlane = planes[0];
                AndroidJavaObject uPlane = planes[1];
                AndroidJavaObject vPlane = planes[2];
                
                // Get buffers
                AndroidJavaObject yBuffer = yPlane.Call<AndroidJavaObject>("getBuffer");
                AndroidJavaObject uBuffer = uPlane.Call<AndroidJavaObject>("getBuffer");
                AndroidJavaObject vBuffer = vPlane.Call<AndroidJavaObject>("getBuffer");
                
                // Get buffer sizes
                int ySize = yBuffer.Call<int>("remaining");
                int uSize = uBuffer.Call<int>("remaining");
                int vSize = vBuffer.Call<int>("remaining");
                
                // Get strides
                int yStride = yPlane.Call<int>("getRowStride");
                int uStride = uPlane.Call<int>("getRowStride");
                int vStride = vPlane.Call<int>("getRowStride");
                
                // Get pixel strides
                int yPixelStride = yPlane.Call<int>("getPixelStride");
                int uPixelStride = uPlane.Call<int>("getPixelStride");
                int vPixelStride = vPlane.Call<int>("getPixelStride");
                
                // Create temporary arrays for YUV data
                byte[] yData = new byte[ySize];
                byte[] uData = new byte[uSize];
                byte[] vData = new byte[vSize];
                
                // Copy data from direct buffers to managed arrays
                yBuffer.Call("get", yData);
                uBuffer.Call("get", uData);
                vBuffer.Call("get", vData);
                
                // Get image dimensions
                int width = image.Call<int>("getWidth");
                int height = image.Call<int>("getHeight");
                
                // Convert YUV to RGBA
                lock (targetArray)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int yIndex = y * yStride + x * yPixelStride;
                            int uvRowIndex = (y >> 1);
                            int uvColIndex = (x >> 1);
                            int uIndex = uvRowIndex * uStride + uvColIndex * uPixelStride;
                            int vIndex = uvRowIndex * vStride + uvColIndex * vPixelStride;
                            
                            // Prevent index out of bounds
                            if (yIndex >= yData.Length || uIndex >= uData.Length || vIndex >= vData.Length)
                                continue;
                            
                            int Y = yData[yIndex] & 0xFF;
                            int U = uData[uIndex] & 0xFF;
                            int V = vData[vIndex] & 0xFF;
                            
                            // YUV to RGB conversion
                            Y -= 16;
                            U -= 128;
                            V -= 128;
                            
                            int r = (int)(1.164f * Y + 1.596f * V);
                            int g = (int)(1.164f * Y - 0.813f * V - 0.391f * U);
                            int b = (int)(1.164f * Y + 2.018f * U);
                            
                            // Clamp RGB values
                            r = r < 0 ? 0 : (r > 255 ? 255 : r);
                            g = g < 0 ? 0 : (g > 255 ? 255 : g);
                            b = b < 0 ? 0 : (b > 255 ? 255 : b);
                            
                            // Write to target array (RGBA format)
                            int rgbaIndex = (y * width + x) * 4;
                            targetArray[rgbaIndex] = (byte)r;
                            targetArray[rgbaIndex + 1] = (byte)g;
                            targetArray[rgbaIndex + 2] = (byte)b;
                            targetArray[rgbaIndex + 3] = 255; // Alpha
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("|| Camera Access || ProcessImageToRGBA | Error | Error processing image: " + e.Message);
        }
    }
    
    public void StartCamera()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (cameraDevice != null && captureSession != null && !isCameraRunning)
            {
                Debug.Log("|| Camera Access || StartCamera | Log | Starting camera preview");
                StartCameraPreview();
            }
            else
            {
                Debug.LogWarning("|| Camera Access || StartCamera | Warning | Camera not ready to start");
            }
        }
    }
    
    public void StopCamera()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                if (captureSession != null)
                {
                    captureSession.Call("stopRepeating");
                    captureSession.Call("close");
                    captureSession = null;
                }
                
                if (cameraDevice != null)
                {
                    cameraDevice.Call("close");
                    cameraDevice = null;
                }
                
                if (imageReader != null)
                {
                    imageReader.Call("close");
                    imageReader = null;
                }
                
                isCameraRunning = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError("|| Camera Access || StopCamera | Error | Error stopping camera: " + e.Message);
            }
        }
    }
    
    // Definizione delle classi per i callback
    private class ImageAvailableListener : AndroidJavaProxy
    {
        private readonly CameraAccessUtility owner;
        
        public ImageAvailableListener(CameraAccessUtility owner) : base("android.media.ImageReader$OnImageAvailableListener")
        {
            this.owner = owner;
        }
        
        public void onImageAvailable(AndroidJavaObject reader)
        {
            try
            {
                using (var image = reader.Call<AndroidJavaObject>("acquireNextImage"))
                {
                    if (image != null)
                    {
                        owner.ProcessImageToRGBA(image, owner.frameData);
                        owner.frameAvailable = true;
                        image.Call("close");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("|| Camera Access || ImageAvailableListener | onImageAvailable | Error | Image processing failed: " + e.Message);
            }
        }
    }
    
    private class CameraStateCallback : AndroidJavaObject
    {
        private readonly CameraAccessUtility owner;
        
        public CameraStateCallback(CameraAccessUtility owner) : base("com.youbiquo.eu.camera2unityplugin.Camera2StateCallback")
        {
            this.owner = owner;
        }
        
        public void onOpened(AndroidJavaObject device)
        {
            try {
                owner.cameraDevice = device;
                owner.CreateCaptureSession();
            } catch (System.Exception e) {
                Debug.LogError("|| Camera Access || CameraStateCallback | onOpened | Error | " + e.Message);
            }
        }
        
        public void onDisconnected(AndroidJavaObject device)
        {
            Debug.Log("|| Camera Access || CameraStateCallback | onDisconnected | Log | Camera disconnected");
            device.Call("close");
            owner.cameraDevice = null;
        }
        
        public void onError(AndroidJavaObject device, int error)
        {
            Debug.LogError("|| Camera Access || CameraStateCallback | onError | Error | " + error);
            device.Call("close");
            owner.cameraDevice = null;
        }
    }
    
    private class CaptureSessionStateCallback : AndroidJavaProxy
    {
        private readonly CameraAccessUtility owner;
        
        public CaptureSessionStateCallback(CameraAccessUtility owner) : base("android.hardware.camera2.CameraCaptureSession$StateCallback")
        {
            this.owner = owner;
        }
        
        public void onConfigured(AndroidJavaObject session)
        {
            try {
                owner.captureSession = session;
                owner.StartCameraPreview();
            } catch (System.Exception e) {
                Debug.LogError("|| Camera Access || CaptureSessionStateCallback | Error | Error in onConfigured: " + e.Message);
            }
        }
        
        public void onConfigureFailed(AndroidJavaObject session)
        {
            Debug.LogError("|| Camera Access || CaptureSessionStateCallback | Error | Capture session configuration failed");
        }
    }
}