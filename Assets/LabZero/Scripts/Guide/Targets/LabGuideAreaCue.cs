using UnityEngine;

[DisallowMultipleComponent]
public sealed class LabGuideAreaCue : MonoBehaviour
{
    [SerializeField] private Renderer cueRenderer;
    [SerializeField] private Light cueLight;
    [SerializeField] private Color pulseColor = new(0.22f, 0.85f, 0.65f, 0.35f);
    [SerializeField] private float pulseFrequency = 0.55f;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 0.45f;

    private bool _isPulseActive;
    private Material _cueMaterial;

    public float PulseFrequency => pulseFrequency;
    public bool IsPulseActive => _isPulseActive;

    private void Update()
    {
        if (_isPulseActive)
        {
            ApplyPulse(Time.time);
        }
    }

    public void SetActivePulse(bool enabled)
    {
        _isPulseActive = enabled;
        EnsureVisuals();

        if (!_isPulseActive)
        {
            SetVisualsEnabled(false);
            return;
        }

        SetVisualsEnabled(true);
        ApplyPulse(0f);
    }

    private void EnsureVisuals()
    {
        if (cueRenderer != null && cueLight != null)
        {
            return;
        }

        if (cueRenderer == null)
        {
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Area Cue Disc";
            disc.transform.SetParent(transform, false);
            disc.transform.localPosition = Vector3.zero;
            disc.transform.localRotation = Quaternion.identity;
            disc.transform.localScale = new Vector3(0.9f, 0.01f, 0.9f);
            cueRenderer = disc.GetComponent<Renderer>();

            var collider = disc.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }

            _cueMaterial = CreateCueMaterial();
            cueRenderer.sharedMaterial = _cueMaterial;
        }

        if (cueLight == null)
        {
            var lightGo = new GameObject("Area Cue Light");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = Vector3.up * 0.35f;
            cueLight = lightGo.AddComponent<Light>();
            cueLight.type = LightType.Point;
            cueLight.range = 1.4f;
            cueLight.color = pulseColor;
            cueLight.shadows = LightShadows.None;
        }
    }

    private void SetVisualsEnabled(bool enabled)
    {
        if (cueRenderer != null)
        {
            cueRenderer.enabled = enabled;
        }

        if (cueLight != null)
        {
            cueLight.enabled = enabled;
        }
    }

    private void ApplyPulse(float time)
    {
        var wave = (Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
        var intensity = Mathf.Lerp(minIntensity, maxIntensity, wave);

        if (cueLight != null)
        {
            cueLight.intensity = intensity;
            cueLight.color = pulseColor;
        }

        if (cueRenderer != null)
        {
            var material = cueRenderer.sharedMaterial;
            if (material != null)
            {
                var color = pulseColor;
                color.a = Mathf.Lerp(0.12f, 0.26f, wave);
                material.color = color;
            }
        }
    }

    private Material CreateCueMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        return new Material(shader)
        {
            name = "Guide Area Cue",
            color = pulseColor,
        };
    }
}
