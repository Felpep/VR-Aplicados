using UnityEngine;

/// <summary>
/// Mecánica de castigo del tenedor: apaga las luces, completa la misión 
/// por la fuerza y castiga al jugador con un reset.
/// </summary>
public class ElectrocutorTrigger : MonoBehaviour
{
    [Header("Sistemas de la Oficina")]
    [SerializeField] private LightMissionController _lightMissionController;
    [SerializeField] private ObjectiveEventBridge _objectiveEventBridge;

    private bool _isConfigValid;

    private void Awake()
    {
        _isConfigValid = ValidateReferences();
        if (!_isConfigValid) enabled = false;
    }

    private bool ValidateReferences()
    {
        if (_lightMissionController == null) { Debug.LogError($"[ForkTrigger] Falta '_lightMissionController'.", this); return false; }
        if (_objectiveEventBridge == null) { Debug.LogError($"[ForkTrigger] Falta '_objectiveEventBridge'.", this); return false; }
        return true;
    }

    public void TriggerElectrocution()
    {
        Debug.Log($"<color=red>[ForkTrigger] ¡ELECTROCUCIÓN!</color> Apagando luces y reseteando al jugador.");

        // 1. Apagón
        _lightMissionController.TurnOffLights();

        // 2. Fuerzo el completado de la misión
        _objectiveEventBridge.SendObjectiveSignal();

        // 3. Castigo al jugador (Validado)

        if (PlayerResetManager.Instance != null)
        {
            PlayerResetManager.Instance.ResetPlayer();
        }
        else
        {
            Debug.LogError("<color=red>[Error Crítico]</color> No hay ningún PlayerResetManager en la escena.");
        }
    }
}