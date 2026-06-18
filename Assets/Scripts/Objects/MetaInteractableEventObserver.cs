using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;

[RequireComponent(typeof(IInteractableView))]
public class MetaInteractableEventObserver : MonoBehaviour
{
    public UnityEvent OnSelected;
    public UnityEvent OnUnselected;

    private IInteractableView _interactableView;
    private InteractableState _previousState;

    private void Awake()
    {
        _interactableView = GetComponent<IInteractableView>();

        if (_interactableView == null)
        {
            Debug.LogError($"[MetaInteractableEventObserver] No IInteractableView found on {name}. Disabling.", this);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        _previousState = _interactableView.State;
    }

    private void OnEnable()
    {
        if (_interactableView != null)
            _interactableView.WhenStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (_interactableView != null)
            _interactableView.WhenStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(InteractableStateChangeArgs args)
    {
        // Transitions into Select
        if (args.NewState == InteractableState.Select && _previousState != InteractableState.Select)
            OnSelected?.Invoke();

        // Transitions out of Select
        else if (args.PreviousState == InteractableState.Select && args.NewState != InteractableState.Select)
            OnUnselected?.Invoke();

        _previousState = args.NewState;
    }
}