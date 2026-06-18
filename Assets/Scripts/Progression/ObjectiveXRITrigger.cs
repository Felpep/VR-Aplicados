using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ObjectiveXRITrigger : MonoBehaviour
{
    [Header("Objective Target")]
    [SerializeField] private SimpleObjectiveData _objectiveData;

    private XRSocketInteractor _socket;

    private void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();

        if (_socket == null)
        {
            Debug.LogError($"[ObjectiveXRITrigger] Requiere un XRSocketInteractor en {name}.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        // Suscripción limpia a la API de eventos nativa de XRI
        _socket.selectEntered.AddListener(OnObjectPlaced);
    }

    private void OnDisable()
    {
        _socket.selectEntered.RemoveListener(OnObjectPlaced);
    }

    private void OnObjectPlaced(SelectEnterEventArgs args)
    {
        if (_objectiveData == null) return;

        // Disparamos el ID de forma global mediante el Event Bus desacoplado
        GameEventSystem.TriggerObjective(_objectiveData.ObjectiveID);

#if UNITY_EDITOR
        Debug.Log($"[ObjectiveXRITrigger] {args.interactableObject.transform.name} colocado con éxito en {name}.");
#endif
    }
}
