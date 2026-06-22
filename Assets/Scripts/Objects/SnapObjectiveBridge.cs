using UnityEngine;

/// <summary>
/// Puente desacoplado entre el InteractableUnityEventWrapper de Meta y el MasterMissionController.
/// </summary>
public class SnapObjectiveBridge : MonoBehaviour
{
    [Header("Configuración Data-Driven")]
    [SerializeField] private SimpleObjectiveData _targetObjectiveAsset;

    [Header("Master Controller (referencia de escena)")]
    [SerializeField] private MasterMissionController _masterController;

    [Header("Comportamiento del Disparo")]
    [Tooltip("False: dispara al conectar/encajar (WhenSelect). True: dispara al desconectar/retirar (WhenUnselect).")]
    [SerializeField] private bool _triggerOnDisconnect = false;

    [Header("Protección Anti-Trampa")]
    [Tooltip("Si el jugador deshace la acción (ej. vuelve a enchufar el cable), resta 1 al contador para evitar que gane puntos infinitos.")]
    [SerializeField] private bool _revertCountOnOppositeAction = true;

    [Tooltip("Si es true, apaga el SnapInteractable de Meta una vez que se logra la acción, bloqueándolo para siempre.")]
    [SerializeField] private bool _lockSnapZoneAfterSuccess = false;

    private string _cachedObjectiveID;
    private bool _isConfigValid;
    private bool _actionAlreadyTriggered = false;

    private void Awake()
    {
        _isConfigValid = ValidateReferences();
        if (!_isConfigValid) { enabled = false; return; }
        _cachedObjectiveID = _targetObjectiveAsset.ObjectiveID;
    }

    private bool ValidateReferences()
    {
        if (_targetObjectiveAsset == null) { Debug.LogError($"<color=red>[VRSnapObjectiveBridge]</color> Falta asignar '_targetObjectiveAsset' en {name}."); return false; }
        if (_masterController == null) { Debug.LogError($"<color=red>[VRSnapObjectiveBridge]</color> Falta asignar '_masterController' en {name}."); return false; }
        return true;
    }

    // Se conecta al evento WhenSelect de Meta
    public void HandleSnap()
    {
        if (_triggerOnDisconnect)
        {
            if (_revertCountOnOppositeAction && _actionAlreadyTriggered) RevertProgress();
            return;
        }

        ExecuteProgress();
    }

    // Se conecta al evento WhenUnselect de Meta
    public void HandleUnsnap()
    {
        if (!_triggerOnDisconnect)
        {
            if (_revertCountOnOppositeAction && _actionAlreadyTriggered) RevertProgress();
            return;
        }

        ExecuteProgress();
    }

    private void ExecuteProgress()
    {
        if (_actionAlreadyTriggered) return; // Evita doble contabilidad

        _actionAlreadyTriggered = true;
        Debug.Log($"<color=cyan>[VRSnapBridge]</color> Acción exitosa en {name}. Avisando al Master para la misión: {_cachedObjectiveID}");
        _masterController.AddCountToObjective(_cachedObjectiveID);

        if (_lockSnapZoneAfterSuccess)
        {
            // Apaga el componente nativo de la zona magnética para que ya no funcione
            var snapZone = GetComponent<Oculus.Interaction.SnapInteractable>();
            if (snapZone != null) snapZone.enabled = false;
        }
    }

    private void RevertProgress()
    {
        _actionAlreadyTriggered = false;
        Debug.Log($"<color=orange>[VRSnapBridge]</color> El jugador deshizo la acción en {name}. Restando 1 punto a la misión: {_cachedObjectiveID}");
        _masterController.RemoveCountFromObjective(_cachedObjectiveID);
    }
}