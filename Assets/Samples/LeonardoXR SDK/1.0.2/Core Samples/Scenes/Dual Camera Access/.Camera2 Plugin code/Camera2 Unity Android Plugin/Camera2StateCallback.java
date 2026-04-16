package com.youbiquo.eu.camera2unityplugin;

import android.hardware.camera2.CameraDevice;
import android.hardware.camera2.CameraDevice.StateCallback;
import android.util.Log;

public class Camera2StateCallback extends StateCallback {
    private static final String TAG = "Camera2StateCallback";
    private long nativePtr;

    public Camera2StateCallback(long nativePtr) {
        this.nativePtr = nativePtr;
    }

    @Override
    public void onOpened(CameraDevice camera) {
        Log.d(TAG, "Camera opened");
        nativeOnOpened(nativePtr, camera);
    }

    @Override
    public void onDisconnected(CameraDevice camera) {
        Log.d(TAG, "Camera disconnected");
        nativeOnDisconnected(nativePtr, camera);
    }

    @Override
    public void onError(CameraDevice camera, int error) {
        Log.e(TAG, "Camera error: " + error);
        nativeOnError(nativePtr, camera, error);
    }

    private native void nativeOnOpened(long nativePtr, CameraDevice camera);
    private native void nativeOnDisconnected(long nativePtr, CameraDevice camera);
    private native void nativeOnError(long nativePtr, CameraDevice camera, int error);
}