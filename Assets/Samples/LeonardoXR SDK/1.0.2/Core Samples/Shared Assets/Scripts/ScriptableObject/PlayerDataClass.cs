using UnityEngine.InputSystem.XR;
using UnityEngine;
using System;



#if AUTOHAND
using Autohand;
using Autohand.Demo;
#else
using Unity.XR.CoreUtils;
#endif

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

[Serializable]
public class PlayerDataClass
{
    #region Fields

#if PHOTON_UNITY_NETWORKING

    public int PlayerViewID;
    public string PlayerNetworkName;

#endif

    public int PlayerNumber = -1;

    public GameObject XRRig;
    public CharacterController CharController;

#if AUTOHAND
    public AutoHandPlayer PlayerController;
#else
    public XROrigin XROrigin;
    public GameObject Locomotion;
#endif

    // Head

    public Camera CameraRig;
    public Transform CameraTransform;

    // Hands

#if AUTOHAND
    public Hand LeftHand;
    public Hand RightHand;
#else
    public GameObject LeftHand;
    public GameObject RightHand;
#endif

    public SkinnedMeshRenderer HandRenderer_Left;
    public SkinnedMeshRenderer HandRenderer_Right;

    public TrackedPoseDriver TrackedController_Left;
    public TrackedPoseDriver TrackedController_Right;

    // Movement

#if AUTOHAND
    public OpenXRHandPlayerControllerLink PlayerControllerLink;
#else

#endif

    // Inventory

    public GameObject Inventory;

    // Others
    
    public bool SetupDone = false;

#endregion

    public PlayerDataClass(GameObject xrRig)
    {
        XRRig = xrRig;

#if AUTOHAND
        PlayerController = XRRig.GetComponentInChildren<AutoHandPlayer>();
#else
        XROrigin = XRRig.GetComponent<XROrigin>();
        Locomotion = XROrigin.transform.Find("Locomotion").gameObject;
#endif

        CharController = XROrigin.GetComponent<CharacterController>();

#if AUTOHAND
        PlayerControllerLink = PlayerController.GetComponent<OpenXRHandPlayerControllerLink>();
#else

#endif

#if PHOTON_UNITY_NETWORKING

        if(NetworkManager.Instance != null)
        {
            PlayerViewID = XRRig.GetComponent<PhotonView>().ViewID;
            PlayerNetworkName = XRRig.GetComponent<PhotonView>()?.Controller.NickName;
        }

#endif

#if AUTOHAND
        CameraRig = PlayerController.trackingContainer.GetComponentInChildren<Camera>();
#else
        CameraRig = XROrigin.GetComponentInChildren<Camera>();
#endif

        CameraTransform = CameraRig.transform;

#if AUTOHAND
        LeftHand = PlayerController.handLeft;
        RightHand = PlayerController.handRight;
#else

#endif


#if AUTOHAND
        HandRenderer_Left = LeftHand.GetComponentInChildren<SkinnedMeshRenderer>(true);
        HandRenderer_Right = RightHand.GetComponentInChildren<SkinnedMeshRenderer>(true);
#else

#endif


#if AUTOHAND
        TrackedController_Left = PlayerController.trackingContainer.Find("Controller (left)").GetComponent<TrackedPoseDriver>();
        TrackedController_Right = PlayerController.trackingContainer.Find("Controller (right)").GetComponent<TrackedPoseDriver>();
#else
        TrackedController_Left = XROrigin.transform.Find("Camera Offset").Find("Controllers").Find("Left Controller").GetComponent<TrackedPoseDriver>();
        TrackedController_Right = XROrigin.transform.Find("Camera Offset").Find("Controllers").Find("Right Controller").GetComponent<TrackedPoseDriver>();
#endif


        SetupDone = true;
    }
}