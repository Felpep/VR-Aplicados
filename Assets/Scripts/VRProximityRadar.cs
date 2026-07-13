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

    // Búfer físico estático para no generar basura en memoria RAM
    private readonly Collider[] _radarResultsBuffer = new Collider[32];

    // HashSets para comparar de forma O(1) qué objetos entraron y cuáles salieron
    private readonly HashSet<VRInteractableProxy> _currentNearbyProxies = new HashSet<VRInteractableProxy>();
    private readonly HashSet<VRInteractableProxy> _previouslyNearbyProxies = new HashSet<VRInteractableProxy>();

    private WaitForSeconds _cachedWait;

    private void Awake()
    {
        _cachedWait = new WaitForSeconds(_scanInterval);
    }

    private void OnEnable()
    {
        StartCoroutine(RadarScanRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        // Por seguridad, si el radar se apaga, reactivamos todo lo que estaba en memoria
        foreach (var proxy in _currentNearbyProxies)
        {
            if (proxy != null) proxy.SetInteractionState(true);
        }
    }

    private IEnumerator RadarScanRoutine()
    {
        while (true)
        {
            // 1. Intercambiamos los contenedores para saber qué teníamos en el frame anterior
            _previouslyNearbyProxies.Clear();
            foreach (var proxy in _currentNearbyProxies)
            {
                _previouslyNearbyProxies.Add(proxy);
            }
            _currentNearbyProxies.Clear();

            // 2. Lanzamos el pulso de física ultra optimizado (NonAlloc)
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _radarRadius,
                _radarResultsBuffer,
                _interactableLayer,
                QueryTriggerInteraction.Collide
            );

            // 3. Procesamos los objetos que están cerca actualmente
            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _radarResultsBuffer[i];
                if (col == null) continue;

                // Buscamos el Proxy en el objeto o sus padres (por si el collider está en un hijo)
                VRInteractableProxy proxy = col.GetComponentInParent<VRInteractableProxy>();
                if (proxy == null) continue;

                _currentNearbyProxies.Add(proxy);

                // Si no estaba activo, lo encendemos (acaba de entrar al rango del jugador)
                if (!proxy.IsActive)
                {
                    proxy.SetInteractionState(true);
                }
            }

            // 4. Limpiamos el búfer físico para el próximo tick
            System.Array.Clear(_radarResultsBuffer, 0, _radarResultsBuffer.Length);

            // 5. Los objetos que estaban antes pero ya no están cerca, se apagan de inmediato
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