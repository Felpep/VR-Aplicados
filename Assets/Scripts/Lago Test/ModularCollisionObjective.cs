using UnityEngine;
using UnityEngine.Events;
using System.Collections; // Necesario para usar Corrutinas (IEnumerator)
using Oculus.Interaction; // Necesario para interactuar con los Grabbables de Meta

[RequireComponent(typeof(Collider))]
public class ModularCollisionObjective : MonoBehaviour
{
    [Header("Progression Data")]
    [SerializeField] private string objectiveID;
    [SerializeField] private string targetTag = "Ball";

    [Header("Dependencies")]
    [Tooltip("Arrastra aquí tu objeto MasterMissionController de la escena.")]
    public MasterMissionController masterController;

    [Header("Snap Configuration")]
    [Tooltip("El punto exacto (Transform) donde quieres que se posicione el objeto antes de morir.")]
    [SerializeField] private Transform snapPoint;
    [Tooltip("Tiempo en segundos que el objeto se queda congelado antes de borrarse.")]
    [SerializeField] private float delayBeforeDestroy = 1.0f;

    [Header("Rejection Physics")]
    [Tooltip("Fuerza con la que el tacho levanta el objeto hacia arriba (bajala para que no llegue al techo).")]
    [SerializeField] private float upwardForce = 3f;
    [Tooltip("Fuerza con la que empuja el objeto hacia los lados para sacarlo del tacho.")]
    [SerializeField] private float outwardForce = 1.5f;

    [Header("Local Consequences (Unity Events)")]
    public UnityEvent<GameObject> OnActionTriggered;

    private WaitForSeconds _cachedWaitInstruction;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        _cachedWaitInstruction = new WaitForSeconds(delayBeforeDestroy);

        // Pequeño chequeo de seguridad
        if (snapPoint == null)
        {
            Debug.LogWarning($"<color=yellow>[ModularCollision]</color> No asignaste un Snap Point en {name}. Usando la posición del propio Tacho.");
            snapPoint = this.transform;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        // 1. Si ES el objeto correcto (tiene el tag esperado)
        if (other.CompareTag(targetTag))
        {
            // Iniciamos el proceso de Snap y posterior Destrucción
            StartCoroutine(SnapAndProcessObjective(other));
        }
        // 2. Si NO ES el objeto correcto (cualquier otra tag o untagged)
        else
        {
            Debug.Log($"<color=orange>[ModularCollision]</color> Objeto INVÁLIDO ({other.name}). ¡Expulsando hacia afuera!");

            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                float randomX = Random.Range(-1f, 1f);
                float randomZ = Random.Range(-1f, 1f);

                Vector3 ejectionVector;
                ejectionVector.x = randomX * outwardForce;
                ejectionVector.y = upwardForce;
                ejectionVector.z = randomZ * outwardForce;

                rb.AddForce(ejectionVector, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * outwardForce, ForceMode.Impulse);
            }
        }
    }

    // ... (Todo el resto del script queda exactamente igual)

    private IEnumerator SnapAndProcessObjective(Collider other)
    {
        Debug.Log($"<color=green>[ModularCollision]</color> ¡Objeto válido detectado! ({other.name}). Snapeando...");

        Rigidbody rb = other.GetComponent<Rigidbody>();
        Grabbable grabbable = other.GetComponent<Grabbable>();

        if (grabbable != null) grabbable.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Buscamos la raíz de LA PELOTA (no del tacho)
        GameObject rootObject = other.transform.parent != null ? other.transform.parent.gameObject : other.gameObject;

        // La metemos temporalmente en el tacho para que se mueva allí
        rootObject.transform.position = snapPoint.position;
        rootObject.transform.rotation = snapPoint.rotation;
        rootObject.transform.SetParent(snapPoint);

        yield return _cachedWaitInstruction;

        rootObject.transform.SetParent(null);

        // Ahora sí, llamamos de forma segura al evento pasándole el objeto ingresado
        OnActionTriggered?.Invoke(other.gameObject);

        NotifyMaster();
    }

    // Tu función original de destrucción ahora es 100% segura gracias al paso anterior
    public void DestroyDetectedObject(GameObject targetToDestroy)
    {
        if (targetToDestroy != null)
        {
            // Como ya la despegamos del tacho, esto solo borrará el prefab de la pelota
            if (targetToDestroy.transform.parent != null)
                Destroy(targetToDestroy.transform.parent.gameObject);
            else
                Destroy(targetToDestroy);
        }
    }

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

   
}