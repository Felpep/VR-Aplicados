using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class MissionCounterData
{
    [Header("Identificador de Misión")]
    [Tooltip("Debe coincidir exactamente con el ID de tu ScriptableObject")]
    public string objectiveID;

    [Tooltip("Cantidad requerida para completar la misión")]
    public int targetCount;

    [Header("Estado Actual (Solo Lectura)")]
    public int currentCount = 0;
    public bool isCompleted = false;

    [Header("Consecuencias (Unity Events)")]
    public UnityEvent OnCountIncremented;
    public UnityEvent OnMissionCompleted;
}


public class MasterMissionController : MonoBehaviour
{
    [Header("Registro de Misiones por Conteo")]
    [SerializeField] private List<MissionCounterData> counterMissions = new List<MissionCounterData>();

    public void AddCountToObjective(string id)
    {
      
        MissionCounterData mission = counterMissions.Find(m => m.objectiveID == id);

        if (mission != null && !mission.isCompleted)
        {
            mission.currentCount++;
            Debug.Log($"[MasterController]: Misión '{id}' progreso -> {mission.currentCount}/{mission.targetCount}");

            mission.OnCountIncremented?.Invoke();

            
            if (mission.currentCount >= mission.targetCount)
            {
                mission.isCompleted = true;

                Debug.Log($"<color=green>[MasterController] ¡MISIÓN COMPLETADA! -> '{id}' ha alcanzado {mission.targetCount}/{mission.targetCount}.</color>");

                GameEventSystem.TriggerObjective(id);                
                mission.OnMissionCompleted?.Invoke();
            }
        }
        else if (mission == null)
        {
            Debug.LogWarning($"[MasterController]: Se intentó sumar a '{id}', pero no está registrada en la lista.");
        }
    }



    public void RemoveCountFromObjective(string id)
    {
        MissionCounterData mission = counterMissions.Find(m => m.objectiveID == id);
        if (mission != null && !mission.isCompleted && mission.currentCount > 0)
        {
            mission.currentCount--;
            Debug.Log($"[MasterController]: Misión '{id}' restó progreso -> {mission.currentCount}/{mission.targetCount}");
        }
    }
}
