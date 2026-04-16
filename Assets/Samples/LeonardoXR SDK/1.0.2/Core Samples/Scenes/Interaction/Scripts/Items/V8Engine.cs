using UnityEngine.XR.Interaction.Toolkit.Interactors.Extensions;
using UnityEngine.XR.Interaction.Toolkit.Extensions;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

#if DOTWEEN
using DG.Tweening;
#endif


public class V8Engine : MonoBehaviour
{
#if DOTWEEN
    [SerializeField] private DOTweenAnimation m_rotateAnimation;
#endif

    [SerializeField] private List<EngineComponent> m_components;
    [SerializeField] private List<XRPreviewSocketInteractor> m_sockets;

    private void Awake()
    {
        LoadSockets();
    }

    private void OnEnable()
    {
        foreach (XRPreviewSocketInteractor socket in m_sockets)
            socket.selectEntered.AddListener(LoadComponent);
    }

    private void OnDisable()
    {
        foreach (XRPreviewSocketInteractor socket in m_sockets)
            socket.selectEntered.RemoveListener(LoadComponent);
    }


    private void LoadComponent(SelectEnterEventArgs args)
    {
        m_components.Add(((XRGrabPreviewInteractable)args.interactableObject).GetComponent<EngineComponent>());

        if (m_components.Count == m_sockets.Count)
#if DOTWEEN
            m_rotateAnimation.DOPlayForward();
#else
            StartCoroutine(Rotate(36f * Time.deltaTime));
#endif
    }

    private void LoadSockets()
    {
        m_sockets = GetComponentsInChildren<XRPreviewSocketInteractor>().ToList();
    }

    IEnumerator Rotate(float speed)
    {
        while (true)
        {
            transform.Rotate(0, speed, 0f);
            yield return null;
        }
    }
}
