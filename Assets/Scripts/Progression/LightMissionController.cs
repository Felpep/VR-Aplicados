using UnityEngine;
using UnityEngine.Events;


public class LightMissionController : MonoBehaviour
{
    [Header("Luces de la habitación")]
    [SerializeField] private Light[] roomLights;

    [Header("Focos / bombillas (parte visual) - opcional")]
    [SerializeField] private MeshRenderer[] bulbRenderers;
    [SerializeField] private Material bulbOnMaterial;   // material emisivo (encendido)
    [SerializeField] private Material bulbOffMaterial;  // material apagado (sin emisión)

    [Header("Estado inicial")]
    [SerializeField] private bool startLightsOn = true;

    [Header("Cadena de eventos del juego")]
    [Tooltip("Se dispara una sola vez, cuando se apagan las luces y se completa la misión.")]
    public UnityEvent OnLightsOff;
    [Tooltip("Opcional: útil si el botón también puede volver a prender.")]
    public UnityEvent OnLightsOn;

    // --- Estado de la misión (solo lectura desde afuera) ---
    public bool AreLightsOn { get; private set; }
    public bool MissionCompleted { get; private set; }

    private void Awake()
    {
        // Forzamos un estado inicial coherente al cargar la escena.
        AreLightsOn = startLightsOn;
        ApplyLightState(AreLightsOn);
    }

    public void TurnOffLights()
    {
        if (!AreLightsOn) return; // ya estaban apagadas, evitamos trabajo redundante

        SetLights(false);

        // La misión se completa y la cadena se dispara solo la primera vez.
        if (!MissionCompleted)
        {
            MissionCompleted = true;
            OnLightsOff?.Invoke();
            Debug.Log("[LightMission]: Luces apagadas. Misión completada.");
        }
    }

    /// <summary>Opcional: volver a encender (para un botón reutilizable).</summary>
    public void TurnOnLights()
    {
        if (AreLightsOn) return;
        SetLights(true);
        OnLightsOn?.Invoke();
    }

    /// <summary>Opcional: un solo método para un botón tipo interruptor (alterna estado).</summary>
    public void ToggleLights()
    {
        if (AreLightsOn) TurnOffLights();
        else TurnOnLights();
    }

    // Actualiza la flag y aplica el cambio físico.
    private void SetLights(bool on)
    {
        AreLightsOn = on;
        ApplyLightState(on);
    }

    // Único punto donde se toca el render. Se ejecuta solo al cambiar de estado, nunca por frame.
    private void ApplyLightState(bool on)
    {
        // 1) Apagar/prender los componentes Light.
        if (roomLights != null)
        {
            for (int i = 0; i < roomLights.Length; i++)
                if (roomLights[i] != null) roomLights[i].enabled = on;
        }

        // 2) Intercambiar el material de los focos.
        //    Usamos sharedMaterial: asigna el asset directo, sin instanciar copias en RAM.
        Material target = on ? bulbOnMaterial : bulbOffMaterial;
        if (target == null || bulbRenderers == null) return;

        for (int i = 0; i < bulbRenderers.Length; i++)
            if (bulbRenderers[i] != null) bulbRenderers[i].sharedMaterial = target;
    }
}