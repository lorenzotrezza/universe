using UnityEngine;

[DisallowMultipleComponent]
public sealed class LabGuideTargetHighlighter : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color pulseColor = new(0.22f, 0.85f, 0.65f, 1f);
    [SerializeField] private float pulseFrequency = 0.6f;
    [SerializeField] private float minIntensity = 0.65f;
    [SerializeField] private float maxIntensity = 1.0f;

    private MaterialPropertyBlock _propertyBlock;
    private bool _isHighlighted;

    public float PulseFrequency => pulseFrequency;
    public bool IsHighlighted => _isHighlighted;

    private void Awake()
    {
        targetRenderer ??= GetComponent<Renderer>();
        _propertyBlock ??= new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (_isHighlighted)
        {
            ApplyPulse(Time.time);
        }
    }

    public void Bind(Renderer renderer)
    {
        targetRenderer = renderer;
    }

    public void SetActivePulse(bool enabled)
    {
        _isHighlighted = enabled && targetRenderer != null;
        if (!_isHighlighted)
        {
            ClearPulse();
            return;
        }

        ApplyPulse(0f);
    }

    private void ApplyPulse(float time)
    {
        if (targetRenderer == null)
        {
            _isHighlighted = false;
            return;
        }

        _propertyBlock ??= new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(_propertyBlock);
        var wave = (Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
        var intensity = Mathf.Lerp(minIntensity, maxIntensity, wave);
        _propertyBlock.SetColor(EmissionColorId, pulseColor * intensity);
        targetRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void ClearPulse()
    {
        if (targetRenderer == null)
        {
            return;
        }

        _propertyBlock ??= new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(EmissionColorId, Color.black);
        targetRenderer.SetPropertyBlock(_propertyBlock);
    }
}
