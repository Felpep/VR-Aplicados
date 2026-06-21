using UnityEngine;

public class TeleportBallManager : MonoBehaviour
{
    [Header("Configuración de Objetos")]
    [Tooltip("Arrastra aquí la pelota específica de tu escena que querés teletransportar")]
    public GameObject targetObject;

    [Tooltip("Arrastra aquí el objeto vacío que servirá como punto de reaparición")]
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Comparamos si el GameObject que entró es exactamente el que pasaste por Inspector
        if (other.gameObject == targetObject)
        {
            // 1. Teletransportamos la pelota a la posición del spawnpoint
            targetObject.transform.position = spawnPoint.position;

            // 2. Frenamos la inercia (físicas) para que no salga disparada con el impulso anterior
            Rigidbody rb = targetObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"¡Objeto {targetObject.name} teletransportado con éxito!");
        }
    }
}