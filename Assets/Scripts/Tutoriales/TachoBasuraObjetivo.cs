using UnityEngine;

public class TachoBasuraObjetivo : MonoBehaviour
{
    [Header("Configuración del Objetivo")]
    [Tooltip("Etiqueta (Tag) que deben tener los objetos para contar como basura.")]
    [SerializeField] private string tagObjetosValidos = "Basura";

    [Tooltip("Cantidad de objetos necesarios para desbloquear la puerta.")]
    [SerializeField] private int objetosRequeridos = 3;

    [Header("Efectos al Completar")]
    [Tooltip("El GameObject del cubo o pared que bloquea la puerta y quieres destruir/desactivar.")]
    [SerializeField] private GameObject cuboBloqueador;

    [Tooltip("Si es verdadero, destruye el objeto arrojado al entrar al tacho para ahorrar memoria.")]
    [SerializeField] private bool destruirObjetoAlEntrar = true;

    private int objetosActuales = 0;
    private bool objetivoCompletado = false;

    private void OnTriggerEnter(Collider other)
    {
        // Evita seguir contando si el objetivo ya se cumplió
        if (objetivoCompletado) return;

        // Verifica si el objeto que entró tiene la etiqueta correcta
        if (other.CompareTag(tagObjetosValidos))
        {
            objetosActuales++;
            Debug.Log($"Objeto encestado: {objetosActuales} / {objetosRequeridos}");

            // Opción para eliminar el objeto del juego
            if (destruirObjetoAlEntrar)
            {
                Destroy(other.gameObject);
            }

            // Comprobar si se alcanzó la meta
            if (objetosActuales >= objetosRequeridos)
            {
                CompletarObjetivo();
            }
        }
    }

    private void CompletarObjetivo()
    {
        objetivoCompletado = true;
        Debug.Log("¡Objetivo del tacho completado!");

        // Desactiva el cubo bloqueador para liberar el paso
        if (cuboBloqueador != null)
        {
            cuboBloqueador.SetActive(false); // También puedes usar Destroy(cuboBloqueador);
        }

        // Si usas el TutorialManager del paso anterior, descomenta la siguiente línea:
        // if (TutorialManager.Instance != null) TutorialManager.Instance.CompletarObjetivoActual();
    }
}
