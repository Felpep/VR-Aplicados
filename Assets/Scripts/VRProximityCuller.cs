using System.Collections;
using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Desactiva componentes de interacción y scripts locales de forma optimizada 
/// cuando el jugador se aleja a una distancia configurable (Culling por Proximidad).
/// Funciona mediante Ticks controlados libres de Garbage Collection (GC Clean).
/// </summary>
public class VRProximityCuller : MonoBehaviour
{
    // Instancia estática o referencia compartida para evitar búsquedas repetitivas de CPU
    public static Transform PlayerCameraTarget;

    [Header("Configuración de Rango")]
    [Tooltip("Distancia máxima en metros a la que el jugador puede estar antes de que el objeto se apague.")]
    [SerializeField] private float _maxDistanceThreshold = 3.5f;
    [Tooltip("Cada cuántos segundos se evalúa la distancia (Ej: 0.2s = 5 veces por segundo).")]
    [SerializeField] private float _tickInterval = 0.2f;

    [Header("Referencias Core (Meta Quest)")]
    [SerializeField] private Grabbable _grabbable;
    [SerializeField] private GrabInteractable _grabInteractable;

    [Header("Componentes Adicionales a Apagar")]
    [Tooltip("Cualquier otro componente (Físicas, scripts de UI, animadores) que deba apagarse en la distancia.")]
    [SerializeField] private Behaviour[] _extraComponentsToCull;

    private float _maxDistanceSqr;
    private WaitForSeconds _cachedWait;
    private bool _currentState = true;

    private void Awake()
    {
        // Precalculamos la distancia al cuadrado para evitar el coste de raíces cuadradas en runtime
        _maxDistanceSqr = _maxDistanceThreshold * _maxDistanceThreshold;
        _cachedWait = new WaitForSeconds(_tickInterval);

        // Si no asignaste las casillas, intentamos recuperarlas en el mismo objeto para automatizar
        if (_grabbable == null) _grabbable = GetComponent<Grabbable>();
        if (_grabInteractable == null) _grabInteractable = GetComponent<GrabInteractable>();
    }

    private void OnEnable()
    {
        // Si no se ha inyectado el jugador de forma global todavía, buscamos el OVRCameraRig de la escena
        if (PlayerCameraTarget == null)
        {
            var rig = Object.FindFirstObjectByType<OVRCameraRig>();
            if (rig != null) PlayerCameraTarget = rig.centerEyeAnchor;
        }

        StartCoroutine(ProximityCheckRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ProximityCheckRoutine()
    {
        while (true)
        {
            if (PlayerCameraTarget != null)
            {
                // OPTIMIZACIÓN MATEMÁTICA: Resta de vectores plana sin asignación ni Sqrt
                Vector3 offset = transform.position - PlayerCameraTarget.position;
                float currentDistanceSqr = offset.sqrMagnitude;

                // Evaluamos si el jugador está dentro o fuera del rango seguro
                bool isWithinRange = currentDistanceSqr <= _maxDistanceSqr;

                // Solo actuamos si el estado cambia (Previene sobreescritura frame a frame en la GPU/CPU)
                if (isWithinRange != _currentState)
                {
                    SetComponentsState(isWithinRange);
                }
            }

            yield return _cachedWait;
        }
    }

    private void SetComponentsState(bool targetState)
    {
        _currentState = targetState;

        // 1. Apagado/Encendido controlado del SDK de Meta
        if (_grabbable != null) _grabbable.enabled = targetState;
        if (_grabInteractable != null) _grabInteractable.enabled = targetState;

        // 2. Apagado en cascada de los componentes del Inspector
        if (_extraComponentsToCull != null && _extraComponentsToCull.Length > 0)
        {
            for (int i = 0; i < _extraComponentsToCull.Length; i++)
            {
                if (_extraComponentsToCull[i] != null)
                {
                    _extraComponentsToCull[i].enabled = targetState;
                }
            }
        }

#if UNITY_EDITOR
        string color = targetState ? "lime" : "orange";
        Debug.Log($"<color={color}>[ProximityCuller]</color> {name} ha cambiado su estado de interacción a: {targetState}");
#endif
    }
}