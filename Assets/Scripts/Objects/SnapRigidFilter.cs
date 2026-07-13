using UnityEngine;
using System.Collections.Generic; // Obligatorio para usar Listas
using Oculus.Interaction;

/// <summary>
/// Filtro nativo personalizado para Meta Interaction SDK.
/// Permite que el Snap funcione SI Y SOLO SI el Rigidbody pertenece a la lista de permitidos.
/// </summary>
public class SnapRigidFilter : MonoBehaviour, IGameObjectFilter
{
    [Header("Filtro Quirúrgico (Multi-Objeto)")]
    [Tooltip("Agrega a esta lista los Rigidbodies de todos los objetos que pueden encajar aquí.")]
    [SerializeField] private List<Rigidbody> _allowedObjects = new List<Rigidbody>();

    /// <summary>
    /// Este método lo llama automáticamente el SDK de Meta ANTES de hacer Snap.
    /// Si devolvemos 'true', el objeto se pega. Si devolvemos 'false', lo ignora por completo.
    /// </summary>
    public bool Filter(GameObject gameObjectToFilter)
    {
        // Si la lista está vacía, no permitimos nada por seguridad
        if (_allowedObjects == null || _allowedObjects.Count == 0) return false;

        // Buscamos el Rigidbody del objeto que el jugador está intentando encajar
        Rigidbody incomingRB = gameObjectToFilter.GetComponentInParent<Rigidbody>();

        if (incomingRB == null) return false;

        // Comprobamos si el objeto entrante está dentro de nuestra lista permitida
        bool isAllowed = _allowedObjects.Contains(incomingRB);

        return isAllowed;
    }
}