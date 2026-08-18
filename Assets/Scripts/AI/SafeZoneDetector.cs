using UnityEngine;

public class SafeZoneDetector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MissionManager _missionManager;

    [Header("Filtro de Objetos")]
    [SerializeField] private LayerMask _stolenObjectsLayer; // La capa de los ítems a robar

    [Header("Opciones de Entrega")]
    [Tooltip("Si está activo, el objeto robado se destruirá al entrar a la zona segura.")]
    [SerializeField] private bool _destroyOnEnter = true;

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
        // Verificamos si la layer del objeto coincide con la máscara
        if ((_stolenObjectsLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (_missionManager != null)
            {
                _missionManager.RegisterStolenObject();

                Debug.Log($"[SafeZone] {other.name} fue asegurado en el cubículo.");

                // Lógica para destruir el objeto si la casilla está marcada
                if (_destroyOnEnter)
                {
                    // Desactivamos el collider para evitar dobles detecciones en el mismo frame
                    other.enabled = false;
                    Destroy(other.gameObject);
                }
                else
                {
                    // Si no se destruye, desactivamos el collider para que no vuelva a sumar si el objeto rueda/se mueve
                    other.enabled = false;
                }
            }
        }
    }
}