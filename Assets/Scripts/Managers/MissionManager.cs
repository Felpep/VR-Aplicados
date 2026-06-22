using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestiona una misión de recolección local (ej. robar N objetos).
/// Funciona de manera autónoma y, al completarse, notifica al sistema global de progresión.
/// </summary>
public class MissionManager : MonoBehaviour
{
    [Header("Integración Global (Data-Driven)")]
    [Tooltip("El ScriptableObject de la misión que se marcará como completada al terminar de robar.")]
    [SerializeField] private BaseObjectiveData _objectiveData;

    [Header("Configuración de Recolección")]
    [SerializeField] private int _objectsRequired = 5;
    private int _currentObjectsStolen = 0;

    [Header("Eventos Locales (Feedback Visual/Audio)")]
    [Tooltip("Se dispara cada vez que robas un objeto individual (ej. reproducir sonido de 'ding').")]
    public UnityEvent OnObjectStolen;
    [Tooltip("Se dispara al robar el último objeto (ej. abrir una puerta secreta o activar alarma).")]
    public UnityEvent OnMissionCompleted;

    private bool _isMissionComplete = false;

    private void Awake()
    {
        // Validación de seguridad para que no pruebes el juego y falle en silencio
        if (_objectiveData == null)
        {
            Debug.LogError($"<color=red>[CollectionMission]</color> CUIDADO: No asignaste el '_objectiveData' en {name}. El sistema global no se enterará cuando termines.");
        }
    }

    /// <summary>
    /// API Pública: Llámalo desde los objetos robables cuando el jugador los guarde o interactúe.
    /// </summary>
    public void RegisterStolenObject()
    {
        if (_isMissionComplete) return;

        _currentObjectsStolen++;
        Debug.Log($"<color=yellow>[CollectionMission]</color> ¡Objeto robado! ({_currentObjectsStolen}/{_objectsRequired})");

        OnObjectStolen?.Invoke();

        // Chequeamos si alcanzamos la meta
        if (_currentObjectsStolen >= _objectsRequired)
        {
            CompleteMission();
        }
    }

    private void CompleteMission()
    {
        if (_isMissionComplete) return;

        _isMissionComplete = true;
        Debug.Log($"<color=green>[CollectionMission]</color> ¡Misión Completada! Notificando al sistema global...");

        // 1. Consecuencias locales en la escena (Unity Events)
        OnMissionCompleted?.Invoke();

        // 2. CONEXIÓN AL SISTEMA GLOBAL:
        // Le enviamos el ID seguro del ScriptableObject a tu GameEventSystem.
        // Esto hará que la libreta lo tache automáticamente y el ProgressionManager avance.
        if (_objectiveData != null && !string.IsNullOrEmpty(_objectiveData.ObjectiveID))
        {
            GameEventSystem.TriggerObjective(_objectiveData.ObjectiveID);
        }
    }
}