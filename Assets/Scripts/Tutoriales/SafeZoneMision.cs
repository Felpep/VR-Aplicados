using System.Collections.Generic;
using UnityEngine;

public class SafeZoneMision : MonoBehaviour
{
    [Header("Configuración de la Misión")]
    [Tooltip("Etiqueta (Tag) que deben tener los objetos robados.")]
    [SerializeField] private string tagObjetoRobado = "ObjetoRobado";

    [Tooltip("Cantidad de objetos requeridos en la Safe Zone.")]
    [SerializeField] private int objetosRequeridos = 3;

    [Header("Bloqueo a Eliminar")]
    [Tooltip("El objeto/cubo que bloquea el paso.")]
    [SerializeField] private GameObject objetoABloquear;

    [Header("Opciones Adicionales")]
    [Tooltip("Destruye el objeto al entrar a la Safe Zone para liberar memoria.")]
    [SerializeField] private bool destruirObjetoAlEntrar = false;

    // Lista para registrar qué objetos ya ingresaron y evitar que sumen doble
    private HashSet<int> objetosIngresadosIDs = new HashSet<int>();
    private int objetosActuales = 0;
    private bool misionCompletada = false;

    private void OnTriggerEnter(Collider other)
    {
        if (misionCompletada) return;

        // Comprobamos si el objeto tiene el Tag correcto
        if (other.CompareTag(tagObjetoRobado))
        {
            int objetoID = other.gameObject.GetInstanceID();

            // Verificamos si este objeto específico ya fue ingresado antes
            if (!objetosIngresadosIDs.Contains(objetoID))
            {
                objetosIngresadosIDs.Add(objetoID);
                objetosActuales++;

                Debug.Log($"[SafeZone] Objeto asegurado: {other.gameObject.name}. Progreso: {objetosActuales} / {objetosRequeridos}");

                if (destruirObjetoAlEntrar)
                {
                    Destroy(other.gameObject);
                }

                if (objetosActuales >= objetosRequeridos)
                {
                    CompletarMision();
                }
            }
        }
    }

    private void CompletarMision()
    {
        misionCompletada = true;
        Debug.Log("[SafeZone] ¡Misión completada! Destruyendo bloqueo...");

        if (objetoABloquear != null)
        {
            Destroy(objetoABloquear);
        }
        else
        {
            Debug.LogWarning("[SafeZone] No se ha asignado el 'objetoABloquear' en el Inspector.");
        }
    }
}
