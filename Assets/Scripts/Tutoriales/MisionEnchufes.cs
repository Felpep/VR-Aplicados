using UnityEngine;

public class MisionEnchufes : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int enchufesRequeridos = 3;
    [SerializeField] private GameObject objetoABloquear;

    private int enchufesDesconectados = 0;
    private bool misionCompletada = false;

    /// <summary>
    /// Método sin parámetros. Fácil de asignar directamente en "When Unselect ()".
    /// </summary>
    public void RegistrarDesconexion()
    {
        if (misionCompletada) return;

        enchufesDesconectados++;
        Debug.Log($"[MisionEnchufes] Enchufe retirado. Progreso: {enchufesDesconectados} / {enchufesRequeridos}");

        if (enchufesDesconectados >= enchufesRequeridos)
        {
            CompletarMision();
        }
    }

    private void CompletarMision()
    {
        misionCompletada = true;
        Debug.Log("[MisionEnchufes] ¡Misión completada! Destruyendo objeto bloqueador...");

        if (objetoABloquear != null)
        {
            Destroy(objetoABloquear);
        }
        else
        {
            Debug.LogWarning("[MisionEnchufes] El 'objetoABloquear' no está asignado en el Inspector.");
        }
    }
}
