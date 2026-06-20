using UnityEngine;

/// <summary>
/// Controla el encendido y apagado de una linterna VR leyendo el hardware nativo de Meta.
/// Su ciclo de ejecución (Update) es controlado externamente mediante eventos de agarre.
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

    private bool _isFlashlightOn;

    private void Awake()
    {
        // Forzamos estado coherente apagado al cargar escena
        SetFlashlightState(false);

        // Empieza desactivado por defecto hasta que el jugador lo agarre
        enabled = false;
    }

    private void Update()
    {
        // Si el script está "enabled", es porque sabemos con certeza que está en la mano.
        // Determinamos el control activo de forma dinámica
        OVRInput.Controller activeController = OVRInput.GetActiveController();

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

    public void SetFlashlightState(bool state)
    {
        if (_spotlight != null) _spotlight.enabled = state;

        if (_lensRenderer != null && _lensOnMaterial != null && _lensOffMaterial != null)
        {
            _lensRenderer.sharedMaterial = state ? _lensOnMaterial : _lensOffMaterial;
        }
    }


    private void OnEnable()
    {
        _isFlashlightOn = true;
        SetFlashlightState(true);
    }

    /// <summary>
    /// Fallback de seguridad: si el objeto se desinstancia o se fuerza un drop, 
    /// apagamos la luz físicamente.
    /// </summary>
    private void OnDisable()
    {
        _isFlashlightOn = false;
        SetFlashlightState(false);
    }
}