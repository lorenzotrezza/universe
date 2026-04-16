using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if DOTWEEN
using DG.Tweening;
#endif

public class ShrinkArea : MonoBehaviour
{
    [SerializeField] private BoxCollider m_collider;
    [SerializeField] private List<EngineComponent> m_componentsInside;

    [SerializeField] private LayerMask m_layerMask;

    private Collider[] m_overlappingColliders = new Collider[20];
    private List<EngineComponent> m_components_cached;
    private List<EngineComponent> m_components_in;
    private List<EngineComponent> m_components_out;

    private void Awake()
    {
        m_collider = GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        // cached
        m_components_cached = m_componentsInside;

        Physics.OverlapBoxNonAlloc(m_collider.transform.position, m_collider.bounds.extents, m_overlappingColliders, Quaternion.identity, m_layerMask, QueryTriggerInteraction.Ignore);

        m_componentsInside = m_overlappingColliders.Where((c) => c != null && c.GetComponentInChildren<EngineComponent>() is EngineComponent component && component != null && !component.IsDisabled).Select((c) => c.GetComponentInChildren<EngineComponent>()).ToList();

        m_components_out = m_components_cached.Except(m_componentsInside).ToList();
        m_components_in = m_componentsInside.Except(m_components_cached).ToList();

        foreach (EngineComponent component in m_components_in)
        {
#if DOTWEEN
            if (interactable.GetComponent<DOTweenAnimation>() is DOTweenAnimation shrinkAnimation)
                shrinkAnimation.DOPlayForward();
            else
#else
            component.Scale(0.5f, 36 * Time.deltaTime);
#endif
        }

        foreach (EngineComponent component in m_components_out)
        {
#if DOTWEEN
            if (interactable.GetComponent<DOTweenAnimation>() is DOTweenAnimation shrinkAnimation)
                shrinkAnimation.DOPlayForward();
            else
#else
            component.Scale(1f, 36 * Time.deltaTime);
#endif
        }
    }
}
