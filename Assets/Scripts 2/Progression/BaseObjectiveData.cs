using UnityEngine;

public abstract class BaseObjectiveData : ScriptableObject, IObjective
{
    [SerializeField] private string objectiveID;
    private bool isCompleted;

    public string ObjectiveID => objectiveID;
    public bool IsCompleted => isCompleted;

    public virtual void Complete()
    {
        isCompleted = true;
    }

    public virtual void ResetObjective()
    {
        isCompleted = false;
    }
}