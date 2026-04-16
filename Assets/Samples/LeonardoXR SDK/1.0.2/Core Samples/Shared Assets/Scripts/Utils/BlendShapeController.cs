using System.Collections;
using UnityEngine;
using System;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

#if DOTWEEN
using DG.Tweening;
#endif

public class BlendShapeController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer m_skinnedMeshRenderer;
    [SerializeField, Range(1f, 5f)] private float m_speed;

#if ODIN_INSPECTOR
    [ReadOnly]
#endif
    [SerializeField] private float m_amount;

    private void Awake()
    {
        if (!m_skinnedMeshRenderer) m_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    void Update()
    {
        m_skinnedMeshRenderer.SetBlendShapeWeight(0, m_amount);
    }


#if ODIN_INSPECTOR
    [Button("Tween to Start")]
#endif
    public void TweenToStart()
    {

#if DOTWEEN
        DOTween.To(() => m_amount, (x) => m_amount = x, 0f, 1f/m_speed).SetEase(Ease.InOutSine).SetAutoKill(true).Play();
#else
        StartCoroutine(LerpInOutSine((x) => x = m_amount, 0.5f, 1f/ m_speed));   
#endif
    }


#if ODIN_INSPECTOR
    [Button("Tween to End")]
#endif
    public void TweenToEnd()
    {
        
#if DOTWEEN
        DOTween.To(() => m_amount, (x) => m_amount = x, 100f, 1f / m_speed).SetEase(Ease.InOutSine).SetAutoKill(true).Play();
#else
        StartCoroutine(LerpInOutSine((x) => x = m_amount, 2f, 1f/ m_speed));   
#endif
        }


#if ODIN_INSPECTOR
    [Button("Set BlendShape Amount")]
#endif
    public void SetAmount(float amount)
    {
        m_amount = amount;
    }

    IEnumerator LerpInOutSine(Action<float> valueGetter, float to, float duration)
    {
        float from = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            valueGetter.Invoke(from);

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Applica easing InOutSine
            float easedT = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

            float value = Mathf.Lerp(from, to, easedT);

            yield return null;
        }
    }
}
