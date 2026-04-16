using UnityEngine.SceneManagement;
using Qualcomm.Snapdragon.Spaces;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

using UnityEditor;
using UnityEngine;
using System;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance;


#if ODIN_INSPECTOR
    [DisableInPlayMode]
#endif    
    [SerializeField] private SharedDataScriptable SharedData_SO;
    
    
#if ODIN_INSPECTOR
    [ReadOnly, InlineEditor] 
#endif    
    public SharedDataScriptable SharedData;

    
    
#if ODIN_INSPECTOR
    [TabGroup("Scene"), ReadOnly] 
#endif    
    [SerializeField] private bool _sceneLoaded;
    
    
#if ODIN_INSPECTOR
    [TabGroup("Player"), ReadOnly] 
#endif    
    [SerializeField] private bool _userPresent;



#if ODIN_INSPECTOR
    [TabGroup("Player"), Required] 
#endif    
    [SerializeField] private bool _usesInScenePlayer;
    
    
#if ODIN_INSPECTOR
    [TabGroup("Player"), Required] 
#endif    
    [SerializeField] private bool _instantiatePlayer;
    
    
#if ODIN_INSPECTOR
    [TabGroup("Player"), Required] 
#endif    
    [SerializeField] private bool _initializePlayerData;
    
    
#if ODIN_INSPECTOR
    [TabGroup("Player"), Required] 
#endif    
    [SerializeField] private bool _useAnalogsForMovementInBuild;
    
    
#if ODIN_INSPECTOR
    [TabGroup("Player"), AssetSelector, AssetsOnly, Required] 
#endif    
    [SerializeField] private GameObject _xrRigPrefab;
    
    
#if ODIN_INSPECTOR
    [TabGroup("Player")] 
#endif    
    [SerializeField] private GameObject _xrRig;
    
    
#if ODIN_INSPECTOR
    [TabGroup("Player")] 
#endif    
    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private InputAction _resetButton;

#if ODIN_INSPECTOR
    [TabGroup("Player")] 
#endif    
    [SerializeField]
    private bool _useFoveatedRendering = false;
    
#if ODIN_INSPECTOR
    [TabGroup("Player"), ShowIf("_useFoveatedRendering")] 
#endif    
    [SerializeField]
    private FoveationLevel _foveatedRenderingLevel;

    public float TimeInApplication;

    private bool _playerInjected;

#if UNITY_EDITOR
    public void ToggleInScenePlayer()
    {
        if (EditorApplication.isPlaying) return;

        _usesInScenePlayer = !_usesInScenePlayer;
        if (_xrRig) _xrRig.SetActive(!_xrRig.activeSelf);
    }

#endif

    public void InjectPlayer(GameObject XRrig)
    {
        if (!XRrig) return;

        _xrRig = XRrig;
        _playerInjected = true;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);

            UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
            SceneManager.sceneLoaded += (scene, mode) => _sceneLoaded = true;
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        _resetButton.performed += RestartScene;

        _resetButton.Enable();
    }

    private void OnDisable()
    {
        _resetButton.performed -= RestartScene;

        _resetButton.Disable();
    }

    private void Update()
    {
        TimeInApplication += Time.deltaTime;
    }

    private async void Start()
    {
        SharedData = Instantiate(SharedData_SO);

        if (_usesInScenePlayer)
        {
            if (_xrRig)
            {
                _xrRig.SetActive(true);

                if (_initializePlayerData)
                    InitPlayerData(0);
            }
        }
        else
        {
            if (_xrRigPrefab && _instantiatePlayer)
                InstantiateLocalPlayer();
            else
                await UniTask.WaitUntil(() => _playerInjected);

            if (_initializePlayerData)
                InitPlayerData(0);
        }

        if (_useFoveatedRendering)
            SpacesFoveatedRendering.SetFoveationLevel(_foveatedRenderingLevel);

        CheckInitialPresence();

        UpdatePlayerMovement();
    }

    private void InstantiateLocalPlayer()
    {
        _xrRig = Instantiate(_xrRigPrefab, _spawnPoint.position, _spawnPoint.rotation);

        _xrRig.GetComponentInChildren<Camera>().enabled = true;
        _xrRig.GetComponentInChildren<AudioListener>().enabled = true;

        if (_initializePlayerData)
            InitPlayerData(0);
        else
            Debug.LogWarning("Inizializzazione del player disabilitata", gameObject);
    }

    public void InitPlayerData(int playerNumber)
    {
        SharedData.LocalPlayer = new PlayerDataClass(_xrRig);
        SharedData.LocalPlayer.PlayerNumber = playerNumber;

        SharedData.Players.RemoveAll(s => s.PlayerNumber == SharedData.LocalPlayer.PlayerNumber);

        SharedData.Players.Add(SharedData.LocalPlayer);

        PlayerManager.Instance.PlayerData = SharedData.LocalPlayer;

        Debug.Log("Spawned local player: " + SharedData.LocalPlayer.PlayerNumber, gameObject);
    }

    private void CheckInitialPresence()
    {
        _userPresent = false;

        //.TryGetFeatureValue(CommonUsages.userPresence, out userPresent);

        if (_userPresent)
            UserPresent();
        else
            UserNotPresent();
    }

    private void UserPresent()
    {

    }

    private void UserNotPresent()
    {

    }

    public void UpdatePlayerMovement(bool overrideValue = false, bool useAnalogs = false)
    {
#if AUTOHAND
        if (SharedData.LocalPlayer.PlayerControllerLink)
            SharedData.LocalPlayer.PlayerControllerLink.enabled = overrideValue ? useAnalogs : _useAnalogsForMovementInBuild;
#else
        if (SharedData.LocalPlayer.Locomotion)
            SharedData.LocalPlayer.Locomotion.SetActive(overrideValue ? useAnalogs : _useAnalogsForMovementInBuild);
#endif
    }

    public void LoadScene(string newScene)
    {
        SceneManager.LoadScene(newScene, LoadSceneMode.Single);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        SceneManager.SetActiveScene(SceneManager.GetActiveScene());
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }

#if ODIN_INSPECTOR
    [Button("Restart scene", ButtonSizes.Large)]
#endif
    public async void RestartScene(InputAction.CallbackContext context = default)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        //SharedData.LocalPlayer.SetupDone = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        SceneManager.SetActiveScene(SceneManager.GetActiveScene());
    }
}