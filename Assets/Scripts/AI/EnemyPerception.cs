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

        _eyeTransform             = transform; // Sobrescribir con un hijo 'Head' si el modelo lo tiene
        _visionRangeSqr           = _visionRange * _visionRange;
        _noiseDetectionRadiusSqr  = _noiseDetectionRadius * _noiseDetectionRadius;
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
            // Solo procesa visión en estados relevantes (evita trabajo en Chase innecesario)
            if (!_stateMachine.IsInState<ChaseState>())
            {
                CheckVision();
            }
            yield return wait;
        }
    }

    private void CheckVision()
    {
        if (_playerDetectionPoints == null || _playerDetectionPoints.Length == 0) return;

        for (int i = 0; i < _playerDetectionPoints.Length; i++)
        {
            Transform point = _playerDetectionPoints[i];
            if (point == null) continue;

            Vector3 toTarget = point.position - _eyeTransform.position;

            // ── Filtro 1: Distancia cuadrática (sin sqrt, O(1)) ───────────────
            if (toTarget.sqrMagnitude > _visionRangeSqr) continue;

            // ── Filtro 2: Ángulo dentro del cono de visión ────────────────────
            // Vector3.Angle es más costoso que una comparación de dot product,
            // pero la claridad semántica supera la micro-optimización aquí,
            // dado que ya filtramos por distancia primero.
            if (Vector3.Angle(_eyeTransform.forward, toTarget) > _visionAngle * 0.5f) continue;

            // ── Filtro 3: Raycast (solo si pasa los filtros anteriores) ───────
            if (Physics.Raycast(_eyeTransform.position, toTarget.normalized, out RaycastHit hit,
                                 _visionRange, _visionLayerMask, QueryTriggerInteraction.Ignore))
            {
                // Si el primer objeto impactado pertenece al jugador, hay visión directa
                if (hit.transform == point || hit.transform.IsChildOf(point.root))
                {
                    OnTargetConfirmed(point.root);
                    return; // Basta con detectar uno de los puntos para confirmar
                }
            }
        }
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
}
