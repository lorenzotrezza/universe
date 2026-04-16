using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

public class MeshGlower : MonoBehaviour
{

#if ODIN_INSPECTOR
    [ReadOnly]
#endif
    [SerializeField] private bool _emitting;
    [SerializeField, ColorUsage(showAlpha: true, hdr: true)] private Color GlowColor;
    [SerializeField] private List<MeshRenderer> MeshRenderers;
    [SerializeField] private Light _light;

    public bool IsEmitting => _emitting;

    private Coroutine _GlowCoroutine;


#if ODIN_INSPECTOR
    [Button("Glow")]
#endif
    public void Glow(float speed)
    {
        if (_GlowCoroutine != null)
            StopCoroutine(_GlowCoroutine);
        _GlowCoroutine = StartCoroutine(EmissionGlow(speed));
    }

    
#if ODIN_INSPECTOR
    [Button("Emit")]
#endif
    public void Emit()
    {
        if (_GlowCoroutine != null)
            StopCoroutine(_GlowCoroutine);

        ToggleEmission(true);
        SetEmissionColor(GlowColor);
    }

    public void Emit(Color glowColor)
    {
        if (_GlowCoroutine != null)
            StopCoroutine(_GlowCoroutine);

        ToggleEmission(true);
        SetEmissionColor(glowColor);
    }

    
#if ODIN_INSPECTOR
    [Button("Stop Emitting")]
#endif
    public void StopEmitting()
    {
        ToggleEmission(false);
    }

    public IEnumerator EmissionGlow(float speed)
    {
        Color color = GlowColor;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        ToggleEmission(true);

        if (speed > 0f)
        {
            while (_emitting)
            {
#if UNITY_EDITOR
                float time = (EditorApplication.isPlaying ? AppManager.Instance.TimeInApplication : ((float)EditorApplication.timeSinceStartup)) % (10 / speed) / (10 / speed);
#else
                float time = AppManager.Instance.TimeInApplication % (10/speed) / (10/speed);
#endif
                if (time <= 0.5f)
                    color = Color.Lerp(Color.black, GlowColor, Easing.Quadratic.In(time * 2));
                else
                    color = Color.Lerp(GlowColor, Color.black, Easing.Quadratic.Out((time - 0.5f) * 2));

                SetEmissionColor(color);

                if (_light) _light.color = color;

                yield return wait;
            }
        }

        SetEmissionColor(Color.black);
        ToggleEmission(false);
    }

    public void ToggleEmission(bool on)
    {
        _emitting = on;

        if (_light) _light.enabled = on;

        if (on)
        {
            if (Application.isPlaying)
                MeshRenderers.ForEach(mr => mr.materials.ToList().ForEach(m => m.EnableKeyword("_EMISSION")));
            else
                MeshRenderers.ForEach(mr => mr.sharedMaterials.ToList().ForEach(m => m.EnableKeyword("_EMISSION")));
        }
        else
        {
            if (Application.isPlaying)
                MeshRenderers.ForEach(mr => mr.materials.ToList().ForEach(m => m.DisableKeyword("_EMISSION")));
            else
                MeshRenderers.ForEach(mr => mr.sharedMaterials.ToList().ForEach(m => m.DisableKeyword("_EMISSION")));
        }
    }

    public void SetEmissionColor(Color color)
    {
        if (color != null)
        {
            if (Application.isPlaying)
                MeshRenderers.ForEach(mr => mr.materials.ToList().ForEach(m => m.SetColor("_EmissionColor", color)));
            else
                MeshRenderers.ForEach(mr => mr.sharedMaterials.ToList().ForEach(m => m.SetColor("_EmissionColor", color)));
        }
    }
}
