using System.Collections;
using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    // ── Evento estático de ruido: el jugador lo dispara, la IA se suscribe ────
    // Firma: (Vector3 noiseOrigin, float noiseRadius)
    public static event System.Action<Vector3, float> OnNoiseEmitted;

    // ── Configuración Visual ──────────────────────────────────────────────────
    [Header("Vision")]
    [SerializeField] private Transform _eyeTransform;
    [SerializeField] private float _visionRange      = 15f;
    [SerializeField] private float _visionAngle      = 90f;
    [SerializeField] private float _visionTickRate   = 0.15f; // segundos entre raycasts


    // ── Puntos de detección VR ────────────────────────────────────────────────
    // Expuesto en Inspector: asignar HMD (cámara) + ambas manos del jugador.
    [Header("VR Target Points")]
    [SerializeField] private Transform[] _playerDetectionPoints;

    // ── LayerMask: solo 'Escenario' (paredes/suelo) y 'Player' ───────────────
    [Header("Raycast Configuration")]
    [SerializeField] private LayerMask _visionLayerMask;

    // ── Configuración de Ruido ────────────────────────────────────────────────
    [Header("Noise Detection")]
    [SerializeField] private float _noiseDetectionRadius = 10f;

    [Header("Debug")]
    [SerializeField] private bool _showGizmos = true;

    // ── Referencias internas ──────────────────────────────────────────────────
    private AIStateMachine _stateMachine;

    // ── Caché de distancias para evitar recomputo ─────────────────────────────
    // SqrMagnitude evita sqrt(); comparamos contra el cuadrado del rango.
    private float _visionRangeSqr;
    private float _noiseDetectionRadiusSqr;

    // ─────────────────────────────────────────────────────────────────────────
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





    // ── Vision Coroutine (Time-Sliced) ────────────────────────────────────────
    /// <summary>
    /// Corre en intervalos fijos de _visionTickRate para desacoplar la detección
    /// visual del Update loop. Crítico en VR donde el CPU budget es ~6ms por frame.
    /// </summary>
    private IEnumerator VisionRoutine()
    {
        var wait = new WaitForSeconds(_visionTickRate);

        while (true)
        {
            // Cambiamos la condición para que siempre valide la pérdida si está en Chase
            bool targetInSight = CheckVision();

            if (!targetInSight && _stateMachine.IsInState<ChaseState>())
            {
                // Limpia el transform cacheado para que ChaseState empiece su cuenta regresiva
                _stateMachine.DetectedTarget = null;
            }

            yield return wait;
        }
    }





    private bool CheckVision()
    {
        if (_playerDetectionPoints == null || _playerDetectionPoints.Length == 0) return false;

        for (int i = 0; i < _playerDetectionPoints.Length; i++)
        {
            Transform point = _playerDetectionPoints[i];
            if (point == null) continue;

            Vector3 toTarget = point.position - _eyeTransform.position;

            // ── Filtro 1: Distancia cuadrática (sin sqrt, O(1)) ───────────────
            if (toTarget.sqrMagnitude > _visionRangeSqr) continue;

            // ── Filtro 2: Ángulo dentro del cono de visión ────────────────────
            if (Vector3.Angle(_eyeTransform.forward, toTarget) > _visionAngle * 0.5f) continue;

            // ── Filtro 3: Raycast (solo si pasa los filtros anteriores) ───────
            if (Physics.Raycast(_eyeTransform.position, toTarget.normalized, out RaycastHit hit,
                                 _visionRange, _visionLayerMask, QueryTriggerInteraction.Ignore))
            {
                // Si el primer objeto impactado pertenece al jugador, hay visión directa confirmada
                if (hit.transform == point || hit.transform.IsChildOf(point.root))
                {
                    OnTargetConfirmed(point.root);
                    return true; // Retorno inmediato: con ver una extremidad o el casco ya es suficiente
                }
            }
        }

        return false; // El bucle terminó y el jugador superó los obstáculos físicos o de cono de visión
    }







    // ── Respuesta a detección confirmada ──────────────────────────────────────
    private void OnTargetConfirmed(Transform playerRoot)
    {
        _stateMachine.DetectedTarget = playerRoot;

        if (_stateMachine.IsInState<PatrolState>() || _stateMachine.IsInState<SuspicionState>())
        {
            _stateMachine.TransitionTo(_stateMachine.StateChase);
        }
    }
    // ── Respuesta a evento de ruido ───────────────────────────────────────────
    private void HandleNoise(Vector3 noiseOrigin, float noiseRadius)
    {
        // Solo reacciona si el ruido sucede dentro de su radio de detección auditiva
        // y si el ruido generado es lo suficientemente amplio para ser relevante.
        Vector3 toNoise = noiseOrigin - transform.position;

        if (toNoise.sqrMagnitude > _noiseDetectionRadiusSqr) return;
        if (noiseRadius < 1f) return; // Umbral mínimo: ignora pasos muy suaves

        if (_stateMachine.IsInState<PatrolState>())
        {
            _stateMachine.TransitionTo(_stateMachine.StateSuspicion);
        }
    }

    // ── API Pública para disparar el evento de ruido (llamada desde el jugador) ─
    public static void EmitNoise(Vector3 origin, float radius)
    {
        OnNoiseEmitted?.Invoke(origin, radius);
    }









#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_showGizmos || _eyeTransform == null) return;

        // 1. Dibujar rango de audición (Círculo Azul plano en los pies)
        UnityEditor.Handles.color = new Color(0f, 0.5f, 1f, 0.15f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, _noiseDetectionRadius);
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, _noiseDetectionRadius);

        // 2. Dibujar cono de visión (Color dinámico según estado de alerta)
        bool isChasing = _stateMachine != null && _stateMachine.IsInState<ChaseState>();
        Color coneColor = isChasing ? new Color(1f, 0f, 0f, 0.1f) : new Color(1f, 1f, 0f, 0.05f);
        Color wireColor = isChasing ? Color.red : Color.yellow;

        Vector3 leftBoundary = Quaternion.Euler(0f, -_visionAngle * 0.5f, 0f) * _eyeTransform.forward;

        UnityEditor.Handles.color = coneColor;
        UnityEditor.Handles.DrawSolidArc(_eyeTransform.position, Vector3.up, leftBoundary, _visionAngle, _visionRange);
        UnityEditor.Handles.color = wireColor;
        UnityEditor.Handles.DrawWireArc(_eyeTransform.position, Vector3.up, leftBoundary, _visionAngle, _visionRange);

        // Líneas laterales del cono
        Vector3 rightBoundary = Quaternion.Euler(0f, _visionAngle * 0.5f, 0f) * _eyeTransform.forward;
        Gizmos.color = wireColor;
        Gizmos.DrawLine(_eyeTransform.position, _eyeTransform.position + leftBoundary * _visionRange);
        Gizmos.DrawLine(_eyeTransform.position, _eyeTransform.position + rightBoundary * _visionRange);

        // 3. Dibujar simulación de Raycasts en tiempo real hacia los puntos VR
        if (_playerDetectionPoints == null) return;

        foreach (var point in _playerDetectionPoints)
        {
            if (point == null) continue;
            Vector3 direction = point.position - _eyeTransform.position;

            // Replicamos los filtros matemáticos visualmente
            if (direction.sqrMagnitude > _visionRangeSqr || Vector3.Angle(_eyeTransform.forward, direction) > _visionAngle * 0.5f)
            {
                Gizmos.color = Color.gray; // Fuera de rango o ángulo
                Gizmos.DrawLine(_eyeTransform.position, point.position);
                continue;
            }

            if (Physics.Raycast(_eyeTransform.position, direction.normalized, out RaycastHit hit, _visionRange, _visionLayerMask))
            {
                if (hit.transform == point || hit.transform.IsChildOf(point.root))
                {
                    Gizmos.color = Color.green; // Rayo limpio que ve al jugador
                    Gizmos.DrawLine(_eyeTransform.position, hit.point);
                    Gizmos.DrawWireSphere(hit.point, 0.08f);
                }
                else
                {
                    Gizmos.color = Color.red; // Rayo obstruido por escenario u obstáculos
                    Gizmos.DrawLine(_eyeTransform.position, hit.point);
                    Gizmos.DrawLine(hit.point, point.position);
                }
            }
        }
    }
#endif
}
