using UnityEngine;

public interface IObjective
{
    string ObjectiveID { get; }
    bool IsCompleted { get; }
    void Complete();
}