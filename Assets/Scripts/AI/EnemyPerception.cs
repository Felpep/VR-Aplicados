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
            bool targetInSight = CheckVision();

            if (_stateMachine.DetectedTarget != null)
            {
                // Lanzamos un chequeo rápido para ver si el jugador está en zona segura
                // (puedes usar Physics.OverlapSphere para detectar si está en SafeZone)
                bool isInSafeZone = Physics.CheckSphere(_stateMachine.DetectedTarget.position, 0.5f, LayerMask.GetMask("SafeZone"));

                if (isInSafeZone)
                {
                    _stateMachine.DetectedTarget = null;
                    _stateMachine.TransitionTo(_stateMachine.StateSuspicion);
                }
            }
            else if (!targetInSight && _stateMachine.IsInState<ChaseState>())
            {
                _stateMachine.DetectedTarget = null;
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

        for (int i = 0; i < _playerDetectionPoints.Length; i++)
        {
            Transform point = _playerDetectionPoints[i];
            if (point == null) continue;

            Vector3 toTarget = point.position - _eyeTransform.position;

            // Filtro 1: Distancia
            if (toTarget.sqrMagnitude > _visionRangeSqr)
            {
                if (_showConsoleLogs) Debug.Log($"<color=gray>[Perception]</color> Punto {point.name} fuera de RANGO ({toTarget.magnitude:F1}m).");
                continue;
            }

            // Filtro 2: Ángulo
            if (Vector3.Angle(_eyeTransform.forward, toTarget) > _visionAngle * 0.5f)
            {
                if (_showConsoleLogs) Debug.Log($"<color=gray>[Perception]</color> Punto {point.name} fuera de ÁNGULO.");
                continue;
            }

            // Filtro 3: Raycast
            if (Physics.Raycast(_eyeTransform.position, toTarget.normalized, out RaycastHit hit,
                                 _visionRange, _visionLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == point || hit.transform.IsChildOf(point.root))
                {
                    if (_showConsoleLogs) Debug.Log($"<color=green>[Perception]</color> ¡JUGADOR DETECTADO! Impacto directo en: {hit.transform.name}");
                    OnTargetConfirmed(point.root);
                    return true;
                }
                else
                {
                    if (_showConsoleLogs) Debug.Log($"<color=red>[Perception]</color> Raycast hacia {point.name} OBSTRUIDO por: {hit.transform.name}");
                }
            }
            else
            {
                if (_showConsoleLogs) Debug.Log($"<color=magenta>[Perception]</color> Raycast hacia {point.name} no impactó nada. Revisa las capas (Layers).");
            }
        }

        return false;
    }

    private void OnTargetConfirmed(Transform playerRoot)
    {
        _stateMachine.DetectedTarget = playerRoot;

        if (_stateMachine.IsInState<PatrolState>() || _stateMachine.IsInState<SuspicionState>())
        {
            _stateMachine.TransitionTo(_stateMachine.StateChase);
        }
    }

    private void HandleNoise(Vector3 noiseOrigin, float noiseRadius)
    {
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
