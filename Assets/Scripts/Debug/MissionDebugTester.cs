using UnityEngine;

/// <summary>
/// Herramienta de debug para aislar sistemas y probar la UI o lógica de misiones 
/// sin necesidad de usar el casco VR ni interactuar con las físicas.
/// </summary>
public class MissionDebugTester : MonoBehaviour
{
    [Header("Misión a Testear (Data-Driven)")]
    [Tooltip("Arrastrá directamente el archivo ScriptableObject de la misión desde tu carpeta Project.")]
    [SerializeField] private SimpleObjectiveData _targetObjectiveAsset;

    [Tooltip("Opcional: Arrastra el MasterMissionController si quieres probar sumar puntos sueltos.")]
    [SerializeField] private MasterMissionController _masterController;

    /// <summary>
    /// Grita el evento global leyendo dinámicamente el ID del ScriptableObject.
    /// </summary>
    [ContextMenu("Debug: Completar Misión Inmediatamente")]
    public void ForceCompleteMission()
    {
        if (VerifyAsset())
        {
            string id = _targetObjectiveAsset.ObjectiveID;
            Debug.Log($"<color=yellow>[DevCheat]</color> Forzando completado global de: {id}");
            GameEventSystem.TriggerObjective(id);
        }
    }

    /// <summary>
    /// Suma 1 al contador del MasterController usando el ID del ScriptableObject.
    /// </summary>
    [ContextMenu("Debug: Sumar 1 Punto al Contador")]
    public void AddPointToMission()
    {
        if (_masterController == null)
        {
            Debug.LogWarning("<color=red>[DevCheat]</color> Falta asignar el MasterMissionController en el inspector del Debugger.");
            return;
        }

        if (VerifyAsset())
        {
            string id = _targetObjectiveAsset.ObjectiveID;
            Debug.Log($"<color=yellow>[DevCheat]</color> Simulando impacto físico (+1) para: {id}");
            _masterController.AddCountToObjective(id);
        }
    }

    private bool VerifyAsset()
    {
        if (_targetObjectiveAsset == null)
        {
            Debug.LogError("<color=red>[DevCheat]</color> Error Crítico: No arrastraste ningún ScriptableObject a la ranura Target Objective Asset.", this);
            return false;
        }
        return true;
    }
}