using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction;

/// <summary>
/// Filtro y enrutador inteligente bidireccional para Meta Interaction SDK.
/// Discrimina qué Rigidbody específico ingresó o egresó de la zona de Snap y deriva la llamada
/// al evento lúdico de misiones correcto tanto al conectar como al desconectar.
/// </summary>
public class SnapRouterController : MonoBehaviour, IGameObjectFilter
{
    [System.Serializable]
    public class RouteData
    {
        [Tooltip("El Rigidbody del objeto específico (ej: El Tenedor o el Cable de PC).")]
        public Rigidbody targetRigidbody;

        [Tooltip("Consecuencias lógicas/visuales ejecutadas únicamente cuando este objeto hace SNAP (Conectar).")]
        public UnityEngine.Events.UnityEvent OnObjectSelected;

        [Tooltip("Consecuencias lógicas/visuales ejecutadas únicamente cuando este objeto hace UNSNAP (Desconectar).")]
        public UnityEngine.Events.UnityEvent OnObjectUnselected;
    }

    [Header("Rutas de Consecuencia (Bidireccional)")]
    [SerializeField] private List<RouteData> _routes = new List<RouteData>();

    private Rigidbody _lastEvaluatedRigidbody;

    /// <summary>
    /// Filtro nativo de Meta Quest. Se ejecuta de forma automática en el subsuelo del SDK
    /// tanto cuando un objeto intenta entrar a la zona como cuando es retirado de ella.
    /// </summary>
    public bool Filter(GameObject gameObjectToFilter)
    {
        if (_routes == null || _routes.Count == 0) return false;

        Rigidbody incomingRB = gameObjectToFilter.GetComponentInParent<Rigidbody>();
        if (incomingRB == null) return false;

        // Validamos si el Rigidbody está registrado en el Inspector
        RouteData matchedRoute = _routes.Find(r => r.targetRigidbody == incomingRB);

        if (matchedRoute != null)
        {
            // Cacheamos el puntero del objeto involucrado en este frame para el ruteo inmediato
            _lastEvaluatedRigidbody = incomingRB;
            return true;
        }

        return false;
    }

    /// <summary>
    /// API CONEXIÓN: Conéctalo ÚNICAMENTE al evento WhenSelect() del InteractableUnityEventWrapper.
    /// </summary>
    public void RouteSelect()
    {
        if (_lastEvaluatedRigidbody == null) return;

        RouteData route = _routes.Find(r => r.targetRigidbody == _lastEvaluatedRigidbody);
        if (route != null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=lime>[SnapRouter]</color> Ejecutando SNAP (Select) para: {route.targetRigidbody.name}");
#endif
            route.OnObjectSelected?.Invoke();
        }

        _lastEvaluatedRigidbody = null;
    }

    /// <summary>
    /// API DESCONEXIÓN: Conéctalo ÚNICAMENTE al evento WhenUnselect() del InteractableUnityEventWrapper.
    /// </summary>
    public void RouteUnselect()
    {
        if (_lastEvaluatedRigidbody == null) return;

        RouteData route = _routes.Find(r => r.targetRigidbody == _lastEvaluatedRigidbody);
        if (route != null)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=orange>[SnapRouter]</color> Ejecutando UNSNAP (Unselect) para: {route.targetRigidbody.name}");
#endif
            route.OnObjectUnselected?.Invoke();
        }

        _lastEvaluatedRigidbody = null;
    }
}