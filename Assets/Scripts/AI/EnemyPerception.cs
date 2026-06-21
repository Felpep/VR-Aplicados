using System.Collections;
using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    public static event System.Action<Vector3, float> OnNoiseEmitted;

    [Header("Vision")]
    [SerializeField] private Transform _eyeTransform;
    [SerializeField] private float _visionRange = 15f;
    [SerializeField] private float _visionAngle = 90f;
    [SerializeField] private float _visionTickRate = 0.15f;

    [Header("VR Target Points (Asignar ojos/manos con Colliders)")]
    [SerializeField] private Transform[] _playerDetectionPoints;

    [Header("Raycast Configuration")]
    [SerializeField] private LayerMask _visionLayerMask;

    // Nueva máscara para identificar la Zona Segura de forma limpia
    [SerializeField] private LayerMask _safeZoneLayerMask;

    [Header("Noise Detection")]
    [SerializeField] private float _noiseDetectionRadius = 10f;

    [Header("Debug Settings")]
    [SerializeField] private bool _showConsoleLogs = true;
    [SerializeField] private bool _showGizmos = true;

    private AIStateMachine _stateMachine;
    private float _visionRangeSqr;
    private float _noiseDetectionRadiusSqr;

    private void Awake()
    {
        _stateMachine = GetComponent<AIStateMachine>();
        if (_stateMachine == null)
        {
            Debug.LogError($"[EnemyPerception] AIStateMachine no encontrado en {name}.");
            enabled = false;
            return;
        }

        _eyeTransform = transform;
        _visionRangeSqr = _visionRange * _visionRange;
        _noiseDetectionRadiusSqr = _noiseDetectionRadius * _noiseDetectionRadius;
    }

    private void OnEnable()
    {
        OnNoiseEmitted += HandleNoise;
        StartCoroutine(VisionRoutine());
    }

    private void OnDisable()
    {
        OnNoiseEmitted -= HandleNoise;
        StopAllCoroutines();
    }

    private IEnumerator VisionRoutine()
    {
        var wait = new WaitForSeconds(_visionTickRate);
        while (true)
        {
            // 1. Si el jefe tiene un objetivo asignado
            if (_stateMachine.DetectedTarget != null)
            {
                // Verificamos si el jugador logró entrar a la Safe Zone
                if (IsPlayerSafe(_stateMachine.DetectedTarget.position))
                {
                    if (_showConsoleLogs) Debug.Log("<color=green>[Perception]</color> ¡El jugador entró a la Safe Zone! Perdiendo rastro.");
                    LoseTarget();
                }
                // Si NO está en Safe Zone y el jefe ya lo está persiguiendo,
                // NO HACEMOS NADA MÁS. El jefe lo seguirá persiguiendo sin importar obstáculos, rango o ángulo.
            }
            else
            {
                // 2. Si el jefe NO está persiguiendo a nadie, busca activamente al jugador usando su cono de visión normal
                CheckVision();
            }

            yield return wait;
        }
    }

    private bool CheckVision()
    {
        if (_playerDetectionPoints == null || _playerDetectionPoints.Length == 0)
        {
            if (_showConsoleLogs) Debug.LogWarning($"<color=orange>[Perception]</color> No hay puntos asignados en '_playerDetectionPoints' para {name}.");
            return false;
        }

        // Si el jugador ya está en la zona segura, ni intentamos buscarlo
        if (IsPlayerSafe(_playerDetectionPoints[0].root.position))
            return false;

        for (int i = 0; i < _playerDetectionPoints.Length; i++)
        {
            Transform point = _playerDetectionPoints[i];
            if (point == null) continue;

            Vector3 toTarget = point.position - _eyeTransform.position;

            // Filtro 1: Distancia (Solo para detección inicial)
            if (toTarget.sqrMagnitude > _visionRangeSqr) continue;

            // Filtro 2: Ángulo (Solo para detección inicial)
            if (Vector3.Angle(_eyeTransform.forward, toTarget) > _visionAngle * 0.5f) continue;

            // Filtro 3: Raycast / Línea de visión (Solo para detección inicial)
            if (Physics.Raycast(_eyeTransform.position, toTarget.normalized, out RaycastHit hit,
                                 _visionRange, _visionLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == point || hit.transform.IsChildOf(point.root))
                {
                    if (_showConsoleLogs) Debug.Log($"<color=red>[Perception]</color> ¡JUGADOR AVISTADO! Iniciando persecución implacable.");
                    OnTargetConfirmed(point.root);
                    return true;
                }
            }
        }

        return false;
    }

    private void OnTargetConfirmed(Transform playerRoot)
    {
        if (IsPlayerSafe(playerRoot.position)) return;

        _stateMachine.DetectedTarget = playerRoot;

        if (_stateMachine.IsInState<PatrolState>() || _stateMachine.IsInState<SuspicionState>())
        {
            _stateMachine.TransitionTo(_stateMachine.StateChase);
        }
    }

    private void LoseTarget()
    {
        _stateMachine.DetectedTarget = null;

        // Al entrar a la Safe Zone, el jefe se rinde y pasa a Sospecha (o puedes cambiarlo a StatePatrol directamente si prefieres)
        if (_stateMachine.IsInState<ChaseState>())
        {
            _stateMachine.TransitionTo(_stateMachine.StateSuspicion);
        }
    }
    // Método helper para verificar mediante física si la posición del jugador colisiona con la SafeZone
    private bool IsPlayerSafe(Vector3 playerPosition)
    {
        // Usamos un radio de 0.6f (ajustable a la escala de tu player VR)
        return Physics.CheckSphere(playerPosition, 0.6f, _safeZoneLayerMask, QueryTriggerInteraction.Collide);
    }

    private void HandleNoise(Vector3 noiseOrigin, float noiseRadius)
    {
        // Si el ruido proviene de una zona segura, el jefe lo ignora (ej. el jugador tiró algo desde su cubículo)
        if (IsPlayerSafe(noiseOrigin)) return;

        Vector3 toNoise = noiseOrigin - transform.position;
        if (toNoise.sqrMagnitude > _noiseDetectionRadiusSqr) return;
        if (noiseRadius < 1f) return;

        if (_showConsoleLogs) Debug.Log($"<color=blue>[Perception]</color> Ruido escuchado en {noiseOrigin}. Pasando a Sospecha.");

        if (_stateMachine.IsInState<PatrolState>())
        {
            _stateMachine.TransitionTo(_stateMachine.StateSuspicion);
        }
    }

    public static void EmitNoise(Vector3 origin, float radius)
    {
        OnNoiseEmitted?.Invoke(origin, radius);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_showGizmos || _eyeTransform == null) return;

        // Audición
        UnityEditor.Handles.color = new Color(0f, 0.5f, 1f, 0.05f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _noiseDetectionRadius);
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _noiseDetectionRadius);

        // Visión
        bool isChasing = _stateMachine != null && _stateMachine.IsInState<ChaseState>();
        UnityEditor.Handles.color = isChasing ? new Color(1f, 0f, 0f, 0.1f) : new Color(1f, 1f, 0f, 0.05f);
        Vector3 leftBoundary = Quaternion.Euler(0f, -_visionAngle * 0.5f, 0f) * _eyeTransform.forward;
        UnityEditor.Handles.DrawSolidArc(_eyeTransform.position, Vector3.up, leftBoundary, _visionAngle, _visionRange);
    }
#endif
}
