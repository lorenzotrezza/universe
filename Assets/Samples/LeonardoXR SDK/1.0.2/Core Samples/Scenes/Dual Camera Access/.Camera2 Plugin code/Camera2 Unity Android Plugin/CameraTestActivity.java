package com.youbiquo.eu.camera2unityplugin;

import android.os.Bundle;
import android.util.Log;
import com.unity3d.player.UnityPlayerGameActivity ;

public class CameraTestActivity extends UnityPlayerGameActivity  {
    public final String TAG = "CameraTestActivity";
    
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        int tid = android.os.Process.myTid();

        Log.d(TAG, "priority before change = " + android.os.Process.getThreadPriority(tid));
        Log.d(TAG, "priority before change = " + Thread.currentThread().getPriority());
        Thread.currentThread().setPriority(10);
        android.os.Process.setThreadPriority(-20);
        Log.d(TAG, "priority after change = " + android.os.Process.getThreadPriority(tid));
        Log.d(TAG, "priority after change = " + Thread.currentThread().getPriority());
    }
}