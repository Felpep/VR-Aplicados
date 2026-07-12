using UnityEngine;
using Oculus.Interaction; // Asegúrate de tener el SDK de Meta importado

public class SnapOnCollision : MonoBehaviour
{
    [Header("Configuración del Snap")]
    public Transform snapPoint;
    public string targetTag = "Interactable";

    private GameObject objetoActualSnapeado = null;
    private Grabbable grabbableDelObjeto = null;

    private void OnTriggerEnter(Collider other)
    {
        // Si ya hay un objeto ocupando este snap, ignoramos los nuevos
        if (objetoActualSnapeado != null) return;

        if (other.CompareTag(targetTag))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            grabbableDelObjeto = other.GetComponent<Grabbable>();

            if (rb != null && grabbableDelObjeto != null)
            {
                objetoActualSnapeado = other.gameObject;

                // 1. Desactivamos físicas para "congelarlo" en el sitio
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // 2. Lo posicionamos y emparentamos
                other.transform.position = snapPoint.position;
                other.transform.rotation = snapPoint.rotation;
                other.transform.SetParent(snapPoint);

                // 3. Nos "suscribimos" al evento: Si el usuario lo agarra, ejecutamos "LiberarObjeto"
                grabbableDelObjeto.WhenPointerEventRaised += OnPointerEvent;
            }
        }
    }

    private void OnPointerEvent(PointerEvent evt)
    {
        // PointerEventType.Select significa que el jugador acaba de "agarrar" el objeto
        if (evt.Type == PointerEventType.Select)
        {
            LiberarObjeto();
        }
    }

    private void LiberarObjeto()
    {
        if (objetoActualSnapeado != null)
        {
            // Nos desuscribimos del evento para no generar basura en memoria
            if (grabbableDelObjeto != null)
            {
                grabbableDelObjeto.WhenPointerEventRaised -= OnPointerEvent;
            }

            Rigidbody rb = objetoActualSnapeado.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Le devolvemos la gravedad y físicas
            }

            objetoActualSnapeado.transform.SetParent(null); // Lo despegamos de la zona

            // Limpiamos variables para que la zona quede libre para otro tiro
            objetoActualSnapeado = null;
            grabbableDelObjeto = null;
        }
    }

    // Por seguridad, si el objeto se destruye mientras está snapeado, limpiamos la suscripción
    private void OnDestroy()
    {
        if (grabbableDelObjeto != null)
        {
            grabbableDelObjeto.WhenPointerEventRaised -= OnPointerEvent;
        }
    }
}