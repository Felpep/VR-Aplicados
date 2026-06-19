using UnityEngine;

/// <summary>
/// Puente entre la FSM de IA y el Animator. Traduce la velocidad lineal del
/// agente y los cambios de estado en parámetros del Animator usando hashes
/// precalculados, evitando GC por strings repetidos en runtime.
/// </summary>
[RequireComponent(typeof(AIStateMachine))]
public class AIAnimationBridge : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private AIStateMachine _stateMachine;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int SuspiciousHash = Animator.StringToHash("IsSuspicious");
    private static readonly int AlertHash = Animator.StringToHash("IsAlert");

    private void Awake()
    {
        _stateMachine = GetComponent<AIStateMachine>();

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        // Validación de seguridad final
        if (_animator == null)
        {
            Debug.LogError($"[AIAnimationBridge] Animator no encontrado en {name} ni en sus hijos. Desactivando.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        _animator.SetFloat(SpeedHash, _stateMachine.Movement.CurrentSpeed);
    }

    /// <summary>
    /// Activa o desactiva el flag de sospecha en el Animator de un solo golpe.
    /// </summary>
    public void SetSuspiciousState(bool active) => _animator.SetBool(SuspiciousHash, active);

    /// <summary>
    /// Activa o desactiva el flag de alerta/persecución en el Animator de un solo golpe.
    /// </summary>
    public void SetAlertState(bool active) => _animator.SetBool(AlertHash, active);
}