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

    [Header("Rejection Physics")]
    [Tooltip("Fuerza con la que el tacho levanta el objeto hacia arriba (bajala para que no llegue al techo).")]
    [SerializeField] private float upwardForce = 3f;
    [Tooltip("Fuerza con la que empuja el objeto hacia los lados para sacarlo del tacho.")]
    [SerializeField] private float outwardForce = 1.5f;

    [Header("Local Consequences (Unity Events)")]
    public UnityEvent<GameObject> OnActionTriggered;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        // 1. Si ES el objeto correcto (tiene el tag esperado)
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"<color=green>[ModularCollision]</color> Objeto válido ({other.name}) entró en {name}. Destruyéndolo y sumando punto.");

            // Disparamos el evento (que destruirá el objeto)
            OnActionTriggered?.Invoke(other.gameObject);

            // Notificamos al maestro inmediatamente
            NotifyMaster();
        }
        // 2. Si NO ES el objeto correcto (cualquier otra tag o untagged)
        else
        {
            Debug.Log($"<color=orange>[ModularCollision]</color> Objeto INVÁLIDO ({other.name}). ¡Expulsando hacia afuera!");

            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Frenamos la caída original
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Generamos una dirección aleatoria puramente horizontal (ejes X y Z)
                Vector3 randomOutwardDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

                // Combinamos la fuerza hacia arriba con la fuerza lateral aleatoria
                Vector3 ejectionVector = (Vector3.up * upwardForce) + (randomOutwardDirection * outwardForce);

                // Lo disparamos
                rb.AddForce(ejectionVector, ForceMode.Impulse);

                // (Opcional) Le damos un pequeño giro aleatorio para que se vea más caótico y divertido
                rb.AddTorque(Random.insideUnitSphere * outwardForce, ForceMode.Impulse);
            }
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