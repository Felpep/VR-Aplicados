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

    private string _cachedObjectiveID;
    private bool _isConfigValid;

    private void Awake()
    {
        _isConfigValid = ValidateReferences();

        if (!_isConfigValid)
        {
            enabled = false;
            return;
        }

        _cachedObjectiveID = _targetObjectiveAsset.ObjectiveID;
    }

    private bool ValidateReferences()
    {
        if (_targetObjectiveAsset == null)
        {
            Debug.LogError($"<color=red>[VRSnapObjectiveBridge]</color> Falta asignar '_targetObjectiveAsset' en {name}.");
            return false;
        }

        if (_masterController == null)
        {
            Debug.LogError($"<color=red>[VRSnapObjectiveBridge]</color> Falta asignar '_masterController' en {name}.");
            return false;
        }

        return true;
    }

    public void HandleSnap()
    {
        if (_triggerOnDisconnect) return;

        Debug.Log($"<color=cyan>[VRSnapBridge]</color> Objeto ENCAJADO en {name}. Avisando al Master para la misión: {_cachedObjectiveID}");
        ReportProgress();
    }

    public void HandleUnsnap()
    {
        if (!_triggerOnDisconnect) return;

        Debug.Log($"<color=cyan>[VRSnapBridge]</color> Objeto DESCONECTADO en {name}. Avisando al Master para la misión: {_cachedObjectiveID}");
        ReportProgress();
    }

    private void ReportProgress()
    {
        _masterController.AddCountToObjective(_cachedObjectiveID);
    }
}