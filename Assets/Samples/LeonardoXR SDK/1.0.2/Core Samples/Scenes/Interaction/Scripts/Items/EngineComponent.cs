using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Extensions;
using UnityEngine.XR.Interaction.Toolkit.Extensions;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine;

public class EngineComponent : MonoBehaviour
{
    [SerializeField] private Transform m_transform;
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private Transform m_resetPosition;
    [SerializeField] private XRGrabPreviewInteractable m_grabbable;
    [SerializeField] private XRPreviewSocketInteractor m_socket;
    [SerializeField] private XRInteractableAffordanceStateProvider m_affordanceStateProvider;

    private float m_maxDistance = 2f;
    private Vector3 m_startScale;
    private Coroutine m_scaleCoroutine;
    private bool m_disabled;

    public Vector3 StartScale => m_startScale;
    public bool IsDisabled => m_disabled;

    private void Awake()
    {
        if (!m_grabbable) m_grabbable = GetComponent<XRGrabPreviewInteractable>();
        if (!m_affordanceStateProvider) m_affordanceStateProvider = GetComponentInChildren<XRInteractableAffordanceStateProvider>();

        m_transform = m_grabbable.transform;
        m_rigidbody = m_grabbable?.GetComponent<Rigidbody>() ?? null;

        m_socket.selectEntered.AddListener(Disable);
        m_startScale = m_transform.localScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (m_disabled) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
            ResetPosition();
    }

    private void FixedUpdate()
    {
        if (m_disabled) return;

        if (!m_grabbable.isSelected && Vector3.Distance(m_transform.position, m_resetPosition.position) > m_maxDistance)
            ResetPosition();
    }

    private void ResetPosition()
    {
        m_rigidbody.angularVelocity = Vector3.zero;
        m_rigidbody.linearVelocity = Vector3.zero;

        m_transform.position = m_resetPosition.position;
        m_transform.rotation = Quaternion.identity;
    }

    protected void Disable(SelectEnterEventArgs args)
    {
        if (m_transform.localScale != m_startScale)
            Scale(1f, 0);

        m_transform.parent = m_socket.transform.Find("Attach");
        m_grabbable.enabled = false;
        m_rigidbody.isKinematic = true;
        m_transform.localPosition = Vector3.zero;
        m_transform.localRotation = Quaternion.identity;

        ChangeInteractableState(new AffordanceStateData(AffordanceStateShortcuts.idle, 1f));

        m_disabled = true;
    }

    private void ChangeInteractableState(AffordanceStateData state)
    {
        m_affordanceStateProvider.UpdateAffordanceState(state);
    }

    public void Scale(float multiplier, float duration)
    {
        if (m_disabled) return;

        if (duration == 0f)
            m_transform.localScale = StartScale * multiplier;

        if (m_scaleCoroutine != null)
            StopCoroutine(m_scaleCoroutine);

        m_scaleCoroutine = StartCoroutine(ScaleRoutine(multiplier, duration));
    }

    IEnumerator ScaleRoutine(float multiplier, float duration)
    {
        Vector3 startScale = m_transform.localScale;
        Vector3 targetScale = StartScale * multiplier;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            m_transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        m_transform.localScale = targetScale;
        m_scaleCoroutine = null;
    }
}
