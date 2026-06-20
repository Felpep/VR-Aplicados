using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Captura inputs físicos de los controladores de Meta en runtime
/// y despacha eventos genéricos configurables desde el Inspector.
/// </summary>
public class VRDebugTrigger : MonoBehaviour
{
    [Header("Configuración de Entrada")]
    [Tooltip("Botón físico del mando para gatillar el testeo")]
    [SerializeField] private OVRInput.Button _triggerButton = OVRInput.Button.One;
    [Tooltip("Mando específico que escuchará el input")]
    [SerializeField] private OVRInput.Controller _targetController = OVRInput.Controller.RTouch;

    [Header("Evento a Despachar")]
    public UnityEvent OnDebugButtonPressed;

    private void Update()
    {
        // Consulta OVRInput nativa sin allocations de memoria
        if (OVRInput.GetDown(_triggerButton, _targetController))
        {
            Debug.Log($"[VRDebugTrigger] Botón {_triggerButton} presionado en {_targetController}. Invocando evento.");
            OnDebugButtonPressed?.Invoke();
        }
    }
}