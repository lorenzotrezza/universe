using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LabHotbarInventory : MonoBehaviour
{
    [SerializeField] private LabSessionManager sessionManager;
    [SerializeField] private Transform viewerTransform;
    [SerializeField] private int maxSlots = 8;
    [SerializeField] private float dropDistance = 0.75f;
    [SerializeField] private Vector3 hotbarLocalPosition = new(0f, -0.34f, 1.15f);

    private readonly List<LabSafetyInteractable> _items = new();
    private Transform _storageRoot;
    private GameObject _visualRoot;
    private TMP_Text _hotbarText;
    private int _selectedIndex;

    public int Count => _items.Count;
    public int SelectedIndex => _items.Count == 0 ? -1 : _selectedIndex;
    public LabSafetyInteractable SelectedItem => SelectedIndex >= 0 ? _items[_selectedIndex] : null;

    private void Awake()
    {
        sessionManager ??= Object.FindAnyObjectByType<LabSessionManager>();
        viewerTransform ??= ResolveViewerTransform();
        EnsureStorageRoot();
    }

    private void Start()
    {
        EnsureVisualRoot();
        RefreshVisual();
    }

    public void Configure(LabSessionManager manager, Transform viewer)
    {
        sessionManager = manager;
        viewerTransform = viewer != null ? viewer : ResolveViewerTransform();
        EnsureStorageRoot();
        EnsureVisualRoot();
        RefreshVisual();
    }

    public bool TryEquip(LabSafetyInteractable item)
    {
        if (item == null || !item.IsEquippable || _items.Contains(item) || _items.Count >= maxSlots)
        {
            return false;
        }

        EnsureStorageRoot();
        _items.Add(item);
        _selectedIndex = _items.Count - 1;
        item.transform.SetParent(_storageRoot, true);
        item.SetHotbarState(true);
        RefreshVisual();
        return true;
    }

    public void SelectPrevious()
    {
        if (_items.Count == 0)
        {
            return;
        }

        _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
        RefreshVisual();
    }

    public void SelectNext()
    {
        if (_items.Count == 0)
        {
            return;
        }

        _selectedIndex = (_selectedIndex + 1) % _items.Count;
        RefreshVisual();
    }

    public void UseSelected()
    {
        var selected = SelectedItem;
        if (selected == null)
        {
            return;
        }

        selected.Activate();
        RefreshVisual();
    }

    public void DropSelected()
    {
        var selected = SelectedItem;
        if (selected == null)
        {
            return;
        }

        _items.RemoveAt(_selectedIndex);
        if (_selectedIndex >= _items.Count)
        {
            _selectedIndex = Mathf.Max(0, _items.Count - 1);
        }

        selected.transform.SetParent(null, true);
        selected.transform.position = ResolveDropPosition();
        selected.transform.rotation = Quaternion.identity;
        selected.SetHotbarState(false);
        RefreshVisual();
    }

    private Vector3 ResolveDropPosition()
    {
        var viewer = viewerTransform != null ? viewerTransform : ResolveViewerTransform();
        if (viewer == null)
        {
            return transform.position + Vector3.forward * dropDistance;
        }

        var forward = viewer.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = viewer.forward;
        }

        forward = forward.normalized;
        var position = viewer.position + forward * dropDistance;
        position.y = viewer.position.y - 0.45f;

        if (Physics.Raycast(viewer.position + forward * dropDistance + Vector3.up, Vector3.down, out var hit, 3f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            position = hit.point + Vector3.up * 0.05f;
        }

        return position;
    }

    private void EnsureStorageRoot()
    {
        if (_storageRoot != null)
        {
            return;
        }

        var storage = new GameObject("Hotbar Item Storage");
        storage.transform.SetParent(transform, false);
        _storageRoot = storage.transform;
    }

    private void EnsureVisualRoot()
    {
        if (_visualRoot != null)
        {
            return;
        }

        var viewer = viewerTransform != null ? viewerTransform : ResolveViewerTransform();
        if (viewer == null)
        {
            return;
        }

        _visualRoot = new GameObject("Warehouse Hotbar");
        _visualRoot.transform.SetParent(viewer, false);
        _visualRoot.transform.localPosition = hotbarLocalPosition;
        _visualRoot.transform.localRotation = Quaternion.identity;

        _hotbarText = _visualRoot.AddComponent<TextMeshPro>();
        _hotbarText.alignment = TextAlignmentOptions.Center;
        _hotbarText.fontSize = 0.08f;
        _hotbarText.color = Color.white;
        _hotbarText.text = string.Empty;
    }

    private void RefreshVisual()
    {
        EnsureVisualRoot();
        if (_hotbarText == null)
        {
            return;
        }

        if (_items.Count == 0)
        {
            _hotbarText.text = "[ inventario vuoto ]";
            return;
        }

        var labels = new string[_items.Count];
        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var label = item != null ? item.ItemType.ToString() : "-";
            labels[i] = i == _selectedIndex ? $"[{label}]" : label;
        }

        _hotbarText.text = string.Join("  ", labels);
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
