using UnityEngine;

/// <summary>
/// Contrato base para todos los estados de la IA.
/// Hereda de ScriptableObject para permitir instancias como assets editables
/// o como clases concretas instanciadas en runtime sin MonoBehaviour overhead.
/// </summary>
public abstract class BaseAIState
{
    public abstract void Enter(AIStateMachine owner);
    public abstract void UpdateState(AIStateMachine owner);
    public abstract void Exit(AIStateMachine owner);
}
