using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Escanea periódicamente el entorno del jugador usando física optimizada NonAlloc.
/// Activa los objetos cercanos y desactiva los lejanos de forma centralizada (GC Clean).
/// </summary>
public class VRProximityRadar : MonoBehaviour
{
    [Header("Configuración del Radar")]
    [Tooltip("Radio en metros para activar la interacción de la oficina.")]
    [SerializeField] private float _radarRadius = 3.5f;
    [Tooltip("Cada cuántos segundos escanea la oficina (0.2s = 5 veces por segundo es ideal).")]
    [SerializeField] private float _scanInterval = 0.2f;
    [Tooltip("La capa (Layer) donde están asignados tus objetos interactuables.")]
    [SerializeField] private LayerMask _interactableLayer;

    // Aumentamos el búfer a 64 para absorber ráfagas masivas de objetos de oficina (Zero Alloc)
    private readonly Collider[] _radarResultsBuffer = new Collider[64];

    private readonly HashSet<VRInteractableProxy> _currentNearbyProxies = new HashSet<VRInteractableProxy>();
    private readonly HashSet<VRInteractableProxy> _previouslyNearbyProxies = new HashSet<VRInteractableProxy>();

    private WaitForSeconds _cachedWait;

    private void Awake()
    {
        _cachedWait = new WaitForSeconds(_scanInterval);
    }

    private void Start()
    {
        // EJECUCIÓN CRÍTICA: Forzamos un culling inmediato de la oficina en el primer frame
        InitialWarmUpScan();
    }

    private void OnEnable()
    {
        StartCoroutine(RadarScanRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (var proxy in _currentNearbyProxies)
        {
            if (proxy != null) proxy.SetInteractionState(true);
        }
    }

    /// <summary>
    /// Escaneo de calentamiento único. Encuentra todos los proxies de la escena.
    /// Enciende los que están dentro del radio y apaga instantáneamente los que están fuera.
    /// </summary>
    private void InitialWarmUpScan()
    {
        // 1. Buscamos de forma masiva TODOS los proxies que existen en la oficina actualmente
        VRInteractableProxy[] allProxiesInScene = Object.FindObjectsByType<VRInteractableProxy>(FindObjectsSortMode.None);

        // 2. Lanzamos el pulso de radar físico inicial para saber qué tiene el jugador cerca
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            _radarRadius,
            _radarResultsBuffer,
            _interactableLayer,
            QueryTriggerInteraction.Collide
        );

        // 3. Registramos los que sí están cerca en el inicio
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _radarResultsBuffer[i];
            if (col == null) continue;

            VRInteractableProxy proxy = col.GetComponentInParent<VRInteractableProxy>();
            if (proxy == null) continue;

            _currentNearbyProxies.Add(proxy);
        }

        // Limpiamos el búfer físico inmediatamente para el bucle runtime
        System.Array.Clear(_radarResultsBuffer, 0, _radarResultsBuffer.Length);

        // 4. Sintonizamos los estados en cascada
        for (int i = 0; i < allProxiesInScene.Length; i++)
        {
            VRInteractableProxy proxy = allProxiesInScene[i];
            if (proxy == null) continue;

            // Si está en nuestra lista de cercanía, se queda prendido; si no, se apaga de un golpe
            bool shouldBeActive = _currentNearbyProxies.Contains(proxy);
            proxy.SetInteractionState(shouldBeActive);
        }

#if UNITY_EDITOR
        Debug.Log($"<color=lime>[VRRadar]</color> Warm-up completado. {allProxiesInScene.Length} proxies evaluados en el inicio.");
#endif
    }

    private IEnumerator RadarScanRoutine()
    {
        while (true)
        {
            _previouslyNearbyProxies.Clear();
            foreach (var proxy in _currentNearbyProxies)
            {
                _previouslyNearbyProxies.Add(proxy);
            }
            _currentNearbyProxies.Clear();

            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _radarRadius,
                _radarResultsBuffer,
                _interactableLayer,
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _radarResultsBuffer[i];
                if (col == null) continue;

                VRInteractableProxy proxy = col.GetComponentInParent<VRInteractableProxy>();
                if (proxy == null) continue;

                _currentNearbyProxies.Add(proxy);

                if (!proxy.IsActive)
                {
                    proxy.SetInteractionState(true);
                }
            }

            System.Array.Clear(_radarResultsBuffer, 0, _radarResultsBuffer.Length);

            foreach (var oldProxy in _previouslyNearbyProxies)
            {
                if (oldProxy != null && !_currentNearbyProxies.Contains(oldProxy))
                {
                    oldProxy.SetInteractionState(false);
                }
            }

            yield return _cachedWait;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radarRadius);
    }
#endif
}