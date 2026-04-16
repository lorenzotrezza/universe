using Qualcomm.Snapdragon.Spaces;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.Hands;
using System.Linq;
using UnityEngine;


#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

#if AUTOHAND
using Autohand;
#endif

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    public PlayerDataClass PlayerData;

    [SerializeField] private InputAction _debugButton;
    [SerializeField] private InputAction _crouchButton;
    [SerializeField] private InputAction _resetButton;

    public bool EnableDebugOnStart;

    private bool _debugOn;

#if ODIN_INSPECTOR    
    [ShowInInspector, ReadOnly]
#endif
    public bool DebugOn => _debugOn;
    [SerializeField] private GameObject[] _debugObjs;

#if ODIN_INSPECTOR    
    [ReadOnly]
#endif
    public BaseRuntimeFeature BaseRuntimeFeature;

#if ODIN_INSPECTOR    
    [ShowInInspector, ReadOnly] 
#endif
    public XRHandSubsystem HTSubsystem;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    private void OnEnable()
    {
        _debugButton.performed += ToggleDebug;
        _crouchButton.performed += Crouch;
        _resetButton.performed += ResetApp;

        _debugButton.Enable();
        _crouchButton.Enable();
        _resetButton.Enable();
    }

    private void OnDisable()
    {
        _debugButton.performed -= ToggleDebug;
        _crouchButton.performed -= Crouch;
        _resetButton.performed -= ResetApp;

        _debugButton.Disable();
        _crouchButton.Disable();
        _resetButton.Disable();
    }

    private void Start()
    {
        if (EnableDebugOnStart)
            _debugOn = true;

        BaseRuntimeFeature = OpenXRSettings.Instance.GetFeature<BaseRuntimeFeature>();

        var handSubsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(handSubsystems);

        HTSubsystem = handSubsystems.FirstOrDefault();
    }

    public void ToggleDebug(InputAction.CallbackContext context)
    {
        foreach (GameObject debugObj in _debugObjs)
            debugObj.SetActive(!debugObj.activeSelf);
        
        _debugOn = _debugObjs.Length > 0 ? _debugObjs[0].activeSelf : false;
    }

#if ODIN_INSPECTOR
    [Button("Crouch")]
#endif
    public void Crouch(InputAction.CallbackContext context)
    {
#if AUTOHAND
        AutoHandPlayer.Instance.crouching = !AutoHandPlayer.Instance.crouching;
#else

#endif
    }
    
#if ODIN_INSPECTOR
    [Button("Reset App")]
#endif
    public void ResetApp(InputAction.CallbackContext context)
    {
        if (_debugOn)
            AppManager.Instance.RestartScene();
    }
}