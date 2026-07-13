using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Componente pasivo (sin Update) que expone las referencias de interacción de Meta 
/// y componentes locales para que un sistema centralizado los encienda o apague.
/// </summary>
public class VRInteractableProxy : MonoBehaviour
{
    [Header("Referencias de Interacción (Meta Quest)")]
    [SerializeField] private Grabbable _grabbable;
    [SerializeField] private GrabInteractable _grabInteractable;

    [Header("Componentes Adicionales (Opcional)")]
    [Tooltip("Cualquier otro componente local (Físicas, scripts de UI) que deba apagarse en la distancia.")]
    [SerializeField] private Behaviour[] _extraComponents;

    private bool _isActive = true;
    public bool IsActive => _isActive;

    private void Awake()
    {
        // Auto-recuperación para ahorrar tiempo en el editor
        if (_grabbable == null) _grabbable = GetComponent<Grabbable>();
        if (_grabInteractable == null) _grabInteractable = GetComponentInChildren<GrabInteractable>();
    }

    /// <summary>
    /// Cambia el estado de los componentes lúdicos y de Meta de un solo golpe de forma segura.
    /// </summary>
    public void SetInteractionState(bool targetState)
    {
        if (_isActive == targetState) return; // Si ya está en ese estado, no hacemos nada
        _isActive = targetState;

        if (_grabbable != null) _grabbable.enabled = targetState;
        if (_grabInteractable != null) _grabInteractable.enabled = targetState;

        if (_extraComponents != null)
        {
            for (int i = 0; i < _extraComponents.Length; i++)
            {
                if (_extraComponents[i] != null) _extraComponents[i].enabled = targetState;
            }
        }
    }
}