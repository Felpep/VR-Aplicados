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
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (_grabbable == null) _grabbable = GetComponent<Grabbable>();

        // Buscamos de forma robusta tanto en el objeto como en la jerarquía interna del prefab
        if (_grabInteractable == null) _grabInteractable = GetComponentInChildren<GrabInteractable>(true);
    }

    /// <summary>
    /// Cambia el estado de los componentes lúdicos y de Meta de un solo golpe de forma segura.
    /// </summary>
    public void SetInteractionState(bool targetState)
    {
        // Forzamos que las referencias estén cacheadas antes de apagar/prender
        InitializeIfNeeded();

        if (_isActive == targetState) return;
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