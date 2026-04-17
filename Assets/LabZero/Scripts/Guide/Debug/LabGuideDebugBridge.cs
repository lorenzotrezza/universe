using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class LabGuideDebugBridge : MonoBehaviour
{
    [SerializeField] private LabGuideDirector director;
    [SerializeField]
    private string[] signalIds =
    {
        "guide_intro",
        "raggiungi_area_controllo",
        "interagisci_postazione_dpi",
        "metti_in_sicurezza_passaggio",
        "chiusura_guidata",
    };

    private void Awake()
    {
        director ??= Object.FindAnyObjectByType<LabGuideDirector>();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (director == null || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame) Emit(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) Emit(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) Emit(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) Emit(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) Emit(4);
    }
#endif

    private void Emit(int index)
    {
        if (director == null || signalIds == null || index < 0 || index >= signalIds.Length)
        {
            return;
        }

        director.TryReportCondition(signalIds[index], gameObject);
    }
}
