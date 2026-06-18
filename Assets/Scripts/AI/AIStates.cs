using UnityEngine;

// ══════════════════════════════════════════════════════════════════════════════
// PATROL STATE
// ══════════════════════════════════════════════════════════════════════════════
public class PatrolState : BaseAIState
{
    private int _currentWaypointIndex;

    public override void Enter(AIStateMachine owner)
    {
        owner.Movement.SetPatrolMode();

        if (owner.PatrolWaypoints != null && owner.PatrolWaypoints.Length > 0)
        {
            owner.Movement.MoveTo(owner.PatrolWaypoints[_currentWaypointIndex].position);
        }
        else
        {
            owner.Movement.Stop();
        }
    }

    public override void UpdateState(AIStateMachine owner)
    {
        if (owner.PatrolWaypoints == null || owner.PatrolWaypoints.Length == 0) return;

        if (owner.Movement.HasReachedDestination)
        {
            AdvanceWaypoint(owner);
        }
    }

    public override void Exit(AIStateMachine owner) { }

    private void AdvanceWaypoint(AIStateMachine owner)
    {
        _currentWaypointIndex = (_currentWaypointIndex + 1) % owner.PatrolWaypoints.Length;
        owner.Movement.MoveTo(owner.PatrolWaypoints[_currentWaypointIndex].position);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// SUSPICION STATE
// ══════════════════════════════════════════════════════════════════════════════
public class SuspicionState : BaseAIState
{
    private float _suspicionTimer;

    public override void Enter(AIStateMachine owner)
    {
        _suspicionTimer = owner.SuspicionDuration;
        owner.Movement.Stop();
    }

    public override void UpdateState(AIStateMachine owner)
    {
        _suspicionTimer -= Time.deltaTime;

        if (_suspicionTimer <= 0f)
        {
            owner.TransitionTo(owner.StatePatrol);
        }
    }

    public override void Exit(AIStateMachine owner) { }
}

// ══════════════════════════════════════════════════════════════════════════════
// CHASE STATE
// ══════════════════════════════════════════════════════════════════════════════
public class ChaseState : BaseAIState
{
    private float _lostTargetTimer;
    private const float LostTargetTimeout = 3f;

    private float _pathCountdown;
    private const float PathRefreshRate = 0.15f;

    public override void Enter(AIStateMachine owner)
    {
        _lostTargetTimer = LostTargetTimeout;
        _pathCountdown = 0f;
        owner.Movement.SetChaseMode();
    }

    public override void UpdateState(AIStateMachine owner)
    {
        if (owner.DetectedTarget == null)
        {
            HandleTargetLost(owner);
            return;
        }

        _lostTargetTimer = LostTargetTimeout;
        _pathCountdown -= Time.deltaTime;

        if (_pathCountdown <= 0f)
        {
            _pathCountdown = PathRefreshRate;
            owner.Movement.ChaseTarget(owner.DetectedTarget);
        }
    }

    public override void Exit(AIStateMachine owner)
    {
        owner.DetectedTarget = null;
    }

    private void HandleTargetLost(AIStateMachine owner)
    {
        _lostTargetTimer -= Time.deltaTime;

//#if UNITY_EDITOR
//        // Dibuja un texto flotante sobre la cabeza de la IA en el editor indicando el tiempo de escape restante
//        Vector3 labelPosition = owner.transform.position + Vector3.up * 2.2f;
//        UnityEditor.Handles.Label(labelPosition, $"Perdiendo rastro: {_lostTargetTimer:F1}s");
//#endif

        if (_lostTargetTimer <= 0f)
        {
            owner.TransitionTo(owner.StateSuspicion);
        }
    }
}
