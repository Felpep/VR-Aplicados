using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ModularCollisionObjective : MonoBehaviour
{
    [Header("Progression Data")]
    [SerializeField] private string objectiveID;
    [SerializeField] private string targetTag = "Ball";

    [Header("Dependencies")]
    [Tooltip("Arrastra aquí tu objeto MasterMissionController de la escena.")]
    public MasterMissionController masterController;

    [Header("Local Consequences (Unity Events)")]
    public UnityEvent<GameObject> OnActionTriggered;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Disparamos el evento (que destruirá el objeto)
            OnActionTriggered?.Invoke(other.gameObject);

            // Notificamos al maestro inmediatamente
            NotifyMaster();
        }
    }

    // NUEVO MÉTODO PUENTE
    public void NotifyMaster()
    {
        if (masterController != null && !string.IsNullOrEmpty(objectiveID))
        {
            masterController.AddCountToObjective(objectiveID);
        }
        else
        {
            Debug.LogError($"[Error]: El tacho '{gameObject.name}' no tiene asignado el MasterController o el ID está vacío.");
        }
    }

    public void DestroyDetectedObject(GameObject targetToDestroy)
    {
        if (targetToDestroy != null)
        {
            if (targetToDestroy.transform.parent != null)
                Destroy(targetToDestroy.transform.parent.gameObject);
            else
                Destroy(targetToDestroy);
        }
    }
}