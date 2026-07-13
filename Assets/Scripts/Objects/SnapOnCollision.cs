using UnityEngine;
using Oculus.Interaction;

public class SnapOnCollision : MonoBehaviour
{
    [Header("Configuración del Snap")]
    public Transform snapPoint;
    public string targetTag = "Interactable";

    private GameObject objetoActualSnapeado = null;
    private Grabbable grabbableDelObjeto = null;

    private void OnTriggerEnter(Collider other)
    {
        // FRENO INTER-FRAME CRÍTICO: Si está lleno, cancelamos el proceso físico en el primer microsegundo
        if (objetoActualSnapeado != null) return;

        if (other.CompareTag(targetTag))
        {
            // Cacheamos las referencias de una sola pasada
            grabbableDelObjeto = other.GetComponent<Grabbable>();
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null && grabbableDelObjeto != null)
            {
                objetoActualSnapeado = other.gameObject;

                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                other.transform.position = snapPoint.position;
                other.transform.rotation = snapPoint.rotation;
                other.transform.SetParent(snapPoint);

                grabbableDelObjeto.WhenPointerEventRaised += OnPointerEvent;
            }
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            LiberarObjeto();
        }
    }

    private void LiberarObjeto()
    {
        if (objetoActualSnapeado != null)
        {
            if (grabbableDelObjeto != null)
            {
                grabbableDelObjeto.WhenPointerEventRaised -= OnPointerEvent;
            }

            Rigidbody rb = objetoActualSnapeado.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            objetoActualSnapeado.transform.SetParent(null);

            objetoActualSnapeado = null;
            grabbableDelObjeto = null;
        }
    }

    private void OnDestroy()
    {
        if (grabbableDelObjeto != null)
        {
            grabbableDelObjeto.WhenPointerEventRaised -= OnPointerEvent;
        }
    }
}