using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider))]
public class ModularCollisionObjective : MonoBehaviour
{
    [Header("Progression Data")]
    [SerializeField] private string objectiveID;

    [Tooltip("El Tag del objeto que estamos esperando (ej: 'Ball').")]
    [SerializeField] private string targetTag = "Ball";

    [Header("Local Consequences (Unity Events)")]
    [Tooltip("El evento pasará el GameObject detectado dinámicamente.")]
    public UnityEvent<GameObject> OnActionTriggered;

    private Collider triggerCollider;

    private void Awake()
    {
        
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            
            if (!string.IsNullOrEmpty(objectiveID))
            {
                GameEventSystem.TriggerObjective(objectiveID);
            }

            
            OnActionTriggered?.Invoke(other.gameObject);
        }
    }

    public void DestroyDetectedObject(GameObject targetToDestroy)
    {
        if (targetToDestroy != null)
        {
            
            if (targetToDestroy.transform.parent != null)
            {
                
                Destroy(targetToDestroy.transform.parent.gameObject);
            }
            else
            {
                
                Destroy(targetToDestroy);
            }
        }
    }
}