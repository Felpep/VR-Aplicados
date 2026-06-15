using System;
using UnityEngine;

public static class GameEventSystem
{
    public static System.Action<string> OnObjectiveTriggered;

    public static void TriggerObjective(string id)
    {
        OnObjectiveTriggered?.Invoke(id);
    }
}
