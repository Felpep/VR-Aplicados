using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [Header("Level Objectives")]
    [SerializeField] private List<BaseObjectiveData> _levelObjectives;

    // Guardamos solo los IDs en memoria runtime. HashSet es O(1) en búsquedas.
    private HashSet<string> _completedObjectiveIDs;

    private void Awake()
    {
        _completedObjectiveIDs = new HashSet<string>();
        
    }

    private void OnEnable()
    {
        GameEventSystem.OnObjectiveTriggered += EvaluateObjective;
    }

    private void OnDisable()
    {
        GameEventSystem.OnObjectiveTriggered -= EvaluateObjective;
    }

    private void EvaluateObjective(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        // Validamos si el ID recibido pertenece a los objetivos de este nivel
        bool isValidObjective = _levelObjectives.Exists(o => o.ObjectiveID == id);
        if (!isValidObjective) return;

        // Si ya está en el HashSet, cortamos para evitar procesar dos veces el mismo evento
        if (_completedObjectiveIDs.Contains(id)) return;

        // Registramos el completado de forma segura en memoria RAM
        _completedObjectiveIDs.Add(id);

#if UNITY_EDITOR
        Debug.Log($"[Progression]: '{id}' completado. Progreso: {_completedObjectiveIDs.Count}/{_levelObjectives.Count}");
#endif

        if (_completedObjectiveIDs.Count >= _levelObjectives.Count)
        {
            HandleLevelSequenceComplete();
        }
    }

    private void HandleLevelSequenceComplete()
    {
        Debug.Log("[Progression]: Secuencia de objetivos finalizada en este escenario.");
    }



    public bool IsObjectiveCompletedInRuntime(string id)
    {
        if (_completedObjectiveIDs == null) return false;
        return _completedObjectiveIDs.Contains(id);
    }
}
