using UnityEngine;

[RequireComponent(typeof(AIMovementController))]
[RequireComponent(typeof(EnemyPerception))]
public class AIStateMachine : MonoBehaviour
{
    // ── Cached References ─────────────────────────────────────────────────────
    public AIMovementController Movement { get; private set; }
    public EnemyPerception Perception   { get; private set; }

    // ── State Instances (instanciadas una sola vez, sin alloc por frame) ──────
    public readonly PatrolState    StatePatrol    = new();
    public readonly SuspicionState StateSuspicion = new();
    public readonly ChaseState     StateChase     = new();

    // ── Runtime ───────────────────────────────────────────────────────────────
    private BaseAIState _currentState;

    // ── Waypoints expuestos para PatrolState ──────────────────────────────────
    [Header("Patrol")]
    [SerializeField] private Transform[] _patrolWaypoints;
    public Transform[] PatrolWaypoints => _patrolWaypoints;

    // ── Target cacheado por Perception ────────────────────────────────────────
    public Transform DetectedTarget { get; set; }

    // ── Suspicion timer ───────────────────────────────────────────────────────
    [Header("Timers")]
    [SerializeField] private float _suspicionDuration = 4f;
    [SerializeField] public float MinWaitTime, MaxWaitTime;
    public float SuspicionDuration => _suspicionDuration;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        Movement   = GetComponent<AIMovementController>();
        Perception = GetComponent<EnemyPerception>();

        if (Movement == null || Perception == null)
        {
            Debug.LogError($"[AIStateMachine] Dependencias faltantes en {name}. Desactivando.", this);
            enabled = false;
            return;
        }

        TransitionTo(StatePatrol);
    }

    private void Update()
    {
        _currentState?.UpdateState(this);
    }

    /// <summary>
    /// Ejecuta la transición Exit → Enter de forma segura.
    /// Usa typeof() en lugar de strings para evitar errores silenciosos.
    /// </summary>
    public void TransitionTo(BaseAIState nextState)
    {
        if (nextState == null)
        {
            Debug.LogError($"[AIStateMachine] TransitionTo recibió un estado null en {name}.");
            return;
        }

        _currentState?.Exit(this);

#if UNITY_EDITOR
        Debug.Log($"[AIStateMachine] {name} | {_currentState?.GetType().Name ?? "None"} → {nextState.GetType().Name}");
#endif

        _currentState = nextState;
        _currentState.Enter(this);
    }

    public bool IsInState<T>() where T : BaseAIState => _currentState is T;
}
