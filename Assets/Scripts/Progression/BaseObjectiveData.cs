using UnityEngine;

public abstract class BaseObjectiveData : ScriptableObject
{
    [SerializeField] private string _objectiveID;
    [TextArea(2, 5)][SerializeField] private string _description;

    public string ObjectiveID => _objectiveID;
    public string Description => _description;
}