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
           
            targetObject.transform.position = spawnPoint.position;

            Debug.Log($"¡Objeto {targetObject.name} teletransportado con éxito!");
        }
    }
}