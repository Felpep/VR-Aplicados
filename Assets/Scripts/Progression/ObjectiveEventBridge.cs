using UnityEngine;

public class ObjectiveEventBridge : MonoBehaviour
{
    [SerializeField] private SimpleObjectiveData _objectiveData;

    private bool _signalSent;

    private void Awake()
    {
        if (_objectiveData == null)
            Debug.LogWarning($"[ObjectiveEventBridge] No SimpleObjectiveData assigned on {name}.", this);
    }

    public void SendObjectiveSignal()
    {
        if (_signalSent) return;

        if (_objectiveData == null || string.IsNullOrEmpty(_objectiveData.ObjectiveID))
        {
            Debug.LogError($"[ObjectiveEventBridge] Cannot send signal: missing or empty ObjectiveID on {name}.", this);
            return;
        }

        _signalSent = true;
        GameEventSystem.TriggerObjective(_objectiveData.ObjectiveID);
    }
}