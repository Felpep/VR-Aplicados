using UnityEngine;

public class SafeZoneDetector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MissionManager _missionManager;

    [Header("Filtro de Objetos")]
    [SerializeField] private LayerMask _stolenObjectsLayer; // La capa de los ítems a robar

    private void Awake()
    {
        // Si olvidaste asignarlo en el inspector, lo busca automáticamente
        if (_missionManager == null)
        {
            _missionManager = FindFirstObjectByType<MissionManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si la layer del objeto que entró coincide con nuestra máscara de objetos robables
        // El truco "(1 << other.gameObject.layer)" convierte la layer del objeto a formato de máscara de bits
        if ((_stolenObjectsLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // Evitamos que sume puntos si el jugador simplemente está sosteniendo el objeto y pasando la mano
            // Solo cuenta si el objeto está "libre" en la zona segura
            if (_missionManager != null)
            {
                _missionManager.RegisterStolenObject();

                // OPCIONAL: Desactivar el collider o el objeto para que no vuelva a contar si se mueve
                // other.enabled = false; 
                // o destruir la física para que se quede estático:
                //Destroy(other.gameObject.GetComponent<Rigidbody>());

                if (other.TryGetComponent<Collider>(out var col)) col.enabled = false;

                Debug.Log($"[SafeZone] {other.name} fue asegurado en el cubículo.");
            }
        }
    }
}
