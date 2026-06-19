using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Controla el encendido y apagado de una linterna VR leyendo el hardware nativo
/// de Meta Quest solo cuando el objeto está activamente agarrado.
/// </summary>
public class Flashlight : MonoBehaviour
{
    [Header("Componentes de Luz")]
    [SerializeField] private Light _spotlight;
    [SerializeField] private MeshRenderer _lensRenderer;
    [SerializeField] private Material _lensOnMaterial;
    [SerializeField] private Material _lensOffMaterial;

    [Header("Configuración de Entrada")]
    [Tooltip("Botón físico del control para encender/apagar (ej: PrimaryIndexTrigger = Gatillo, One = Botón A/X)")]
    [SerializeField] private OVRInput.Button _actionButton = OVRInput.Button.PrimaryIndexTrigger;

    private IInteractableView _grabInteractable;
    private bool _isFlashlightOn;

    private void Awake()
    {
        // Buscamos la interfaz de interacción en el mismo objeto
        _grabInteractable = GetComponent<IInteractableView>();

        if (_grabInteractable == null)
        {
            Debug.LogError($"[VRFlashlight] No se encontró un componente interactuable compatible en {name}.", this);
            enabled = false;
            return;
        }

        // Forzamos estado coherente apagado al cargar escena
        SetFlashlightState(false);
    }

    private void Update()
    {
        // REGLA DE OPTIMIZACIÓN VR: Si la linterna está tirada en el suelo, salimos de inmediato.
        // Solo consumimos ciclos de Input si el estado actual es 'Select' (agarrado).
        if (_grabInteractable.State != InteractableState.Select) return;

        // Determinamos dinámicamente qué control tiene sostenida la linterna leyendo la FSM del SDK de Meta
        OVRInput.Controller activeController = OVRInput.GetActiveController();

        // Si se presiona el botón en la mano que sostiene el objeto, conmutamos
        if (OVRInput.GetDown(_actionButton, activeController))
        {
            ToggleFlashlight();
        }
    }

    private void ToggleFlashlight()
    {
        _isFlashlightOn = !_isFlashlightOn;
        SetFlashlightState(_isFlashlightOn);
    }

    private void SetFlashlightState(bool state)
    {
        if (_spotlight != null) _spotlight.enabled = state;

        // Permutación optimizada de materiales sin duplicar instancias en memoria RAM
        if (_lensRenderer != null && _lensOnMaterial != null && _lensOffMaterial != null)
        {
            _lensRenderer.sharedMaterial = state ? _lensOnMaterial : _lensOffMaterial;
        }
    }
}