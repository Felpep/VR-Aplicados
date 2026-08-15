using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Oculus.Interaction;

[RequireComponent(typeof(Collider))]
public class ModularCollisionObjective : MonoBehaviour
{
    [Header("Progression Data")]
    [SerializeField] private string objectiveID;
    [SerializeField] private string targetTag = "Ball";

    [Header("Dependencies")]
    public MasterMissionController masterController;

    [Header("Snap Configuration")]
    [SerializeField] private Transform snapPoint;
    [SerializeField] private float delayBeforeDestroy = 1.0f;

    [Header("Rejection Physics")]
    [SerializeField] private float upwardForce = 3f;
    [SerializeField] private float outwardForce = 1.5f;

    [Header("Local Consequences (Unity Events)")]
    public UnityEvent<GameObject> OnActionTriggered;

    private WaitForSeconds _cachedWaitInstruction;

    // OPTIMIZACIÓN CORE: Guardamos la llave del prefab que originó esta tanda
    // para que la pelota sepa exactamente a qué piscina regresar en RAM.
    [Header("Pooling Link (Opcional si usas el script Returner)")]
    [SerializeField] private GameObject _sourcePrefabKey;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _cachedWaitInstruction = new WaitForSeconds(delayBeforeDestroy);

        if (snapPoint == null)
        {
            snapPoint = transform;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            StartCoroutine(SnapAndProcessObjective(other));
        }
        else
        {
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

    private IEnumerator SnapAndProcessObjective(Collider other)
    {
        // 1. Cacheamos las referencias de inmediato.
        // Acceder a 'other' después del yield es lo que causa el crasheo.
        GameObject collidedObj = other.gameObject;
        GameObject rootObject = other.transform.parent != null ? other.transform.parent.gameObject : collidedObj;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        Grabbable grabbable = other.GetComponent<Grabbable>();

        if (grabbable != null) grabbable.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rootObject.transform.position = snapPoint.position;
        rootObject.transform.rotation = snapPoint.rotation;
        rootObject.transform.SetParent(snapPoint);

        // 2. La corrutina se pausa aquí (ej. 1 segundo).
        yield return _cachedWaitInstruction;

        // --- 3. BLINDAJE CRÍTICO ---
        // Si el script de sonido (o cualquier otro) destruyó el objeto mientras 
        // esperábamos, cancelamos el proceso silenciosamente sin tirar error.
        if (rootObject == null || collidedObj == null)
        {
            yield break; // Aborta la corrutina aquí mismo
        }
        // ---------------------------

        // 4. Si el objeto sobrevivió a la pausa, seguimos con normalidad.
        rootObject.transform.SetParent(null);

        // Disparamos las consecuencias (ej. sumar puntaje) pasándole el objeto seguro
        OnActionTriggered?.Invoke(collidedObj);
        NotifyMaster();

        // AUTO-RECICLAJE
        RecycleDetectedObject(rootObject);
    }

    private void RecycleDetectedObject(GameObject targetToRecycle)
    {
        if (targetToRecycle == null) return;

        // Intentamos leer el componente dinámico de retorno para saber su prefab origen
        PooledVFXReturner returner = targetToRecycle.GetComponentInChildren<PooledVFXReturner>();
        GameObject key = (returner != null) ? returner.GetPrefabKey() : _sourcePrefabKey;

        if (key != null && GameObjectPoolManager.Instance != null)
        {
            // BLINDAJE VR: Le devolvemos el script interactivo a la pelota para que la próxima vez
            // que salga del pooler, el jugador la pueda volver a agarrar sin que salga congelada.
            if (targetToRecycle.TryGetComponent(out Grabbable grab)) grab.enabled = true;
            else if (targetToRecycle.GetComponentInChildren<Grabbable>() != null)
                targetToRecycle.GetComponentInChildren<Grabbable>().enabled = true;

            // Retorno Zero Alloc
            GameObjectPoolManager.Instance.ReleaseToPool(key, targetToRecycle);
        }
        else
        {
            // Fallback de emergencia por si testeas la escena con una pelota nativa colocada a mano
            Destroy(targetToRecycle);
        }
    }

    public void NotifyMaster()
    {
        if (masterController != null && !string.IsNullOrEmpty(objectiveID))
        {
            masterController.AddCountToObjective(objectiveID);
        }
    }
}