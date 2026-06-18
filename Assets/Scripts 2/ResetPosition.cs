using UnityEngine;
using UnityEngine.InputSystem;

public class ResetPosition : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El objeto que se va a teletransportar.")]
    [SerializeField] private Transform objectToReset;

    [Tooltip("El punto exacto al que debe volver.")]
    [SerializeField] private Transform targetTransform;

    [Header("Input de VR / PC")]
    [Tooltip("Asigna aquí la acción (ej. botón B del control derecho o la tecla P).")]
    [SerializeField] private InputActionReference resetAction;

    // Nos suscribimos al evento cuando el objeto se activa
    private void OnEnable()
    {
        if (resetAction != null)
        {
            Debug.Log("OnEnable");

            resetAction.action.performed += ExecuteReset;
            // Para VR, también podés usar 'started' o 'canceled' dependiendo del feel
        }
    }

    // Nos desuscribimos para evitar Memory Leaks (PÉSIMO en VR)
    private void OnDisable()
    {
        if (resetAction != null)
        {
            resetAction.action.performed -= ExecuteReset;
        }
    }

    // Este método solo corre 1 sola vez cuando se presiona el botón asignado
    private void ExecuteReset(InputAction.CallbackContext context)
    {
        Debug.Log("ExecuteReset");

        // 1. Reseteamos Posición y Rotación (en VR la rotación importa)
        objectToReset.position = targetTransform.position;
        objectToReset.rotation = targetTransform.rotation;

        // 2. CORRECCIÓN CRÍTICA FÍSICA: 
        // Si el objeto se puede agarrar en VR, seguro tiene un Rigidbody. 
        // Si no le matás la inercia, se va a teletransportar y salir volando.
        if (objectToReset.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
