using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LabWarehouseInteractionInputRouter : MonoBehaviour
{
    public const string PreviousBinding = "<XRController>{LeftHand}/{PrimaryButton}";
    public const string NextBinding = "<XRController>{LeftHand}/{SecondaryButton}";
    public const string UseBinding = "<XRController>{RightHand}/{PrimaryButton}";
    public const string DropBinding = "<XRController>{RightHand}/{SecondaryButton}";

    private const string TriggerBinding = "<XRController>{RightHand}/{TriggerButton}";
    private const string LeftTriggerBinding = "<XRController>{LeftHand}/{TriggerButton}";

    [SerializeField] private LabHotbarInventory inventory;
    [SerializeField] private Transform viewerTransform;
    [SerializeField] private float selectDistance = 4f;
    [SerializeField] private Vector3 equipPromptOffset = new(0f, 0.28f, 0f);

    private InputAction _triggerAction;
    private InputAction _previousAction;
    private InputAction _nextAction;
    private InputAction _useAction;
    private InputAction _dropAction;
    private LabSafetyInteractable _selectedItem;
    private GameObject _equipPromptRoot;
    private TMP_Text _equipPromptText;

    private void Awake()
    {
        inventory ??= Object.FindAnyObjectByType<LabHotbarInventory>();
        viewerTransform ??= ResolveViewerTransform();
        CreateActions();
    }

    private void OnEnable()
    {
        CreateActions();
        _triggerAction.performed += OnTriggerPerformed;
        _previousAction.performed += OnPreviousPerformed;
        _nextAction.performed += OnNextPerformed;
        _useAction.performed += OnUsePerformed;
        _dropAction.performed += OnDropPerformed;
        EnableActions(true);
    }

    private void OnDisable()
    {
        if (_triggerAction == null)
        {
            return;
        }

        _triggerAction.performed -= OnTriggerPerformed;
        _previousAction.performed -= OnPreviousPerformed;
        _nextAction.performed -= OnNextPerformed;
        _useAction.performed -= OnUsePerformed;
        _dropAction.performed -= OnDropPerformed;
        EnableActions(false);
    }

    public void Configure(LabHotbarInventory hotbarInventory, Transform viewer)
    {
        inventory = hotbarInventory;
        viewerTransform = viewer != null ? viewer : ResolveViewerTransform();
    }

    public void SelectItem(LabSafetyInteractable item)
    {
        _selectedItem = item;
        if (_selectedItem == null)
        {
            HideEquipPrompt();
            return;
        }

        _selectedItem.SelectForAction();
        if (_selectedItem.IsEquippable)
        {
            ShowEquipPrompt(_selectedItem);
        }
        else
        {
            HideEquipPrompt();
        }
    }

    public void ConfirmEquipSelection()
    {
        if (_selectedItem == null)
        {
            return;
        }

        inventory ??= Object.FindAnyObjectByType<LabHotbarInventory>();
        if (inventory != null && inventory.TryEquip(_selectedItem))
        {
            HideEquipPrompt();
        }
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        HandleTriggerSelection();
    }

    private void OnPreviousPerformed(InputAction.CallbackContext context)
    {
        inventory?.SelectPrevious();
    }

    private void OnNextPerformed(InputAction.CallbackContext context)
    {
        inventory?.SelectNext();
    }

    private void OnUsePerformed(InputAction.CallbackContext context)
    {
        inventory?.UseSelected();
    }

    private void OnDropPerformed(InputAction.CallbackContext context)
    {
        inventory?.DropSelected();
    }

    private void HandleTriggerSelection()
    {
        var viewer = viewerTransform != null ? viewerTransform : ResolveViewerTransform();
        if (viewer == null)
        {
            return;
        }

        if (Physics.Raycast(viewer.position, viewer.forward, out var hit, selectDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            if (_equipPromptRoot != null && hit.transform.IsChildOf(_equipPromptRoot.transform))
            {
                ConfirmEquipSelection();
                return;
            }

            var item = hit.collider.GetComponentInParent<LabSafetyInteractable>();
            if (item != null)
            {
                SelectItem(item);
                return;
            }
        }

        SelectItem(null);
    }

    private void ShowEquipPrompt(LabSafetyInteractable item)
    {
        if (item == null)
        {
            HideEquipPrompt();
            return;
        }

        EnsureEquipPrompt();
        if (_equipPromptRoot == null)
        {
            return;
        }

        _equipPromptRoot.SetActive(true);
        _equipPromptRoot.transform.SetParent(null, true);
        _equipPromptRoot.transform.position = item.transform.position + equipPromptOffset;

        var viewer = viewerTransform != null ? viewerTransform : ResolveViewerTransform();
        if (viewer != null)
        {
            _equipPromptRoot.transform.rotation = Quaternion.LookRotation((_equipPromptRoot.transform.position - viewer.position).normalized, Vector3.up);
        }

        if (_equipPromptText != null)
        {
            _equipPromptText.text = "Equipaggia";
        }
    }

    private void HideEquipPrompt()
    {
        if (_equipPromptRoot != null)
        {
            _equipPromptRoot.SetActive(false);
        }
    }

    private void EnsureEquipPrompt()
    {
        if (_equipPromptRoot != null)
        {
            return;
        }

        _equipPromptRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _equipPromptRoot.name = "Floating Equip Button";
        _equipPromptRoot.transform.localScale = new Vector3(0.32f, 0.12f, 0.04f);

        var renderer = _equipPromptRoot.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.08f, 0.48f, 0.72f, 1f);
        }

        var label = new GameObject("Floating Equip Button Label");
        label.transform.SetParent(_equipPromptRoot.transform, false);
        label.transform.localPosition = new Vector3(0f, 0f, -0.55f);
        _equipPromptText = label.AddComponent<TextMeshPro>();
        _equipPromptText.alignment = TextAlignmentOptions.Center;
        _equipPromptText.fontSize = 0.08f;
        _equipPromptText.color = Color.white;
        HideEquipPrompt();
    }

    private void CreateActions()
    {
        if (_triggerAction != null)
        {
            return;
        }

        _triggerAction = new InputAction("Trigger Select", InputActionType.Button);
        _triggerAction.AddBinding(TriggerBinding);
        _triggerAction.AddBinding(LeftTriggerBinding);
        _triggerAction.AddBinding("<Mouse>/leftButton");

        _previousAction = new InputAction("Hotbar Previous", InputActionType.Button);
        _previousAction.AddBinding(PreviousBinding);
        _previousAction.AddBinding("<Keyboard>/z");

        _nextAction = new InputAction("Hotbar Next", InputActionType.Button);
        _nextAction.AddBinding(NextBinding);
        _nextAction.AddBinding("<Keyboard>/x");

        _useAction = new InputAction("Hotbar Use", InputActionType.Button);
        _useAction.AddBinding(UseBinding);
        _useAction.AddBinding("<Keyboard>/c");

        _dropAction = new InputAction("Hotbar Drop", InputActionType.Button);
        _dropAction.AddBinding(DropBinding);
        _dropAction.AddBinding("<Keyboard>/v");
    }

    private void EnableActions(bool enabled)
    {
        SetActionEnabled(_triggerAction, enabled);
        SetActionEnabled(_previousAction, enabled);
        SetActionEnabled(_nextAction, enabled);
        SetActionEnabled(_useAction, enabled);
        SetActionEnabled(_dropAction, enabled);
    }

    private static void SetActionEnabled(InputAction action, bool enabled)
    {
        if (action == null)
        {
            return;
        }

        if (enabled)
        {
            action.Enable();
        }
        else
        {
            action.Disable();
        }
    }

    private static Transform ResolveViewerTransform()
    {
        var camera = Camera.main;
        if (camera != null)
        {
            return camera.transform;
        }

        var anyCamera = Object.FindAnyObjectByType<Camera>();
        return anyCamera != null ? anyCamera.transform : null;
    }
}
