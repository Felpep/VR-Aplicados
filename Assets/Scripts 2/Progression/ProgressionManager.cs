using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [SerializeField] private List<BaseObjectiveData> levelObjectives;
    private int completedCount = 0;

    private void OnEnable()
    {
        GameEventSystem.OnObjectiveTriggered += EvaluateObjective;
    }

    private void OnDisable()
    {
        GameEventSystem.OnObjectiveTriggered -= EvaluateObjective;
    }

    private void Start()
    {
        foreach (var obj in levelObjectives)
        {
            obj.ResetObjective();
        }
    }

    private void EvaluateObjective(string id)
    {
        BaseObjectiveData target = levelObjectives.Find(o => o.ObjectiveID == id);

        if (target != null && !target.IsCompleted)
        {
            target.Complete();
            completedCount++;
            Debug.Log($"[Progression]: '{id}' completado. Progreso: {completedCount}/{levelObjectives.Count}");

            if (completedCount >= levelObjectives.Count)
            {
                HandleLevelSequenceComplete();
            }
        }
    }

    private void HandleLevelSequenceComplete()
    {
        Debug.Log("[Progression]: Secuencia de objetivos finalizada en este escenario.");
    }
}
