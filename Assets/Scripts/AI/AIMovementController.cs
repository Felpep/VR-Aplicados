using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIMovementController : MonoBehaviour
{
    [Header("Speed Configuration")]
    [SerializeField] private float _patrolSpeed = 2.0f;
    [SerializeField] private float _chaseSpeed = 5.5f;

    [Header("Stopping Distances")]
    [SerializeField] private float _patrolStoppingDistance = 0.3f;
    [SerializeField] private float _chaseStoppingDistance = 1.5f;

    private NavMeshAgent _agent;

    // ── Propiedades de consulta segura ────────────────────────────────────────
    public bool HasReachedDestination =>
        !_agent.pathPending &&
        _agent.remainingDistance <= _agent.stoppingDistance &&
        (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.01f);

    public bool IsAgentReady => _agent != null && _agent.isOnNavMesh && _agent.enabled;

    public float CurrentSpeed => IsAgentReady ? _agent.velocity.magnitude : 0f;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (_agent == null)
        {
            Debug.LogError($"[AIMovementController] NavMeshAgent no encontrado en {name}.");
            enabled = false;
        }
    }

    // ── API Pública ───────────────────────────────────────────────────────────

    public void SetPatrolMode()
    {
        if (!IsAgentReady) return;
        _agent.speed = _patrolSpeed;
        _agent.stoppingDistance = _patrolStoppingDistance;
        _agent.isStopped = false;
    }

    public void SetChaseMode()
    {
        if (!IsAgentReady) return;
        _agent.speed = _chaseSpeed;
        _agent.stoppingDistance = _chaseStoppingDistance;
        _agent.isStopped = false;
    }

    /// <summary>
    /// Mueve el agente hacia una posición destino de forma segura.
    /// Verifica que el agente esté activo antes de llamar a SetDestination.
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (!IsAgentReady) return;
        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    /// <summary>
    /// Mueve el agente hacia el Transform de un objetivo, actualizando cada frame.
    /// Apropiado para persecución activa donde el objetivo es dinámico.
    /// </summary>
    public void ChaseTarget(Transform target)
    {
        if (!IsAgentReady || target == null) return;
        _agent.isStopped = false;
        _agent.SetDestination(target.position);
    }

    public void Stop()
    {
        if (!IsAgentReady) return;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    public void ResumeMovement()
    {
        if (!IsAgentReady) return;
        _agent.isStopped = false;
    }
}