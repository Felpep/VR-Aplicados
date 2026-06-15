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
    private const float LostTargetTimeout = 3f; // Segundos antes de volver a sospecha

    public override void Enter(AIStateMachine owner)
    {
        _lostTargetTimer = LostTargetTimeout;
        owner.Movement.SetChaseMode();
    }

    public override void UpdateState(AIStateMachine owner)
    {
        if (owner.DetectedTarget == null)
        {
            _lostTargetTimer -= Time.deltaTime;
            if (_lostTargetTimer <= 0f)
            {
                owner.TransitionTo(owner.StateSuspicion);
            }
            return;
        }

        _lostTargetTimer = LostTargetTimeout; // Resetea el timer mientras hay target
        owner.Movement.ChaseTarget(owner.DetectedTarget);
    }

    public override void Exit(AIStateMachine owner)
    {
        owner.DetectedTarget = null;
    }
}
