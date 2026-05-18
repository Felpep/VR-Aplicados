using UnityEngine;
using UnityEngine.Events;

public class VerdictButton : MonoBehaviour
{
    [Header("Configuración del Botón")]
    [Tooltip("Si es TRUE, este botón representa BLOQUEAR. Si es FALSE, representa ARCHIVAR.")]
    public bool isBlockButton;

    [Tooltip("Tiempo en segundos que el botón se inactiva tras ser presionado (Anti-Spam).")]
    public float cooldownTime = 2.0f;
    private float lastPressedTime = -10f; // Inicializado en negativo para que funcione de inmediato

    // Llama a este método desde el evento "On Select" o "On Poke" de tu Meta Interactable
    public void OnButtonPressed()
    {
        // Si no ha pasado el tiempo de cooldown, ignoramos el toque
        if (Time.time - lastPressedTime < cooldownTime) return;

        lastPressedTime = Time.time;
        Debug.Log($"Botón físico presionado: {(isBlockButton ? "BLOQUEAR" : "ARCHIVAR")}");

        // CORRECCIÓN: Llamamos al CaseManager definitivo
        if (CaseManager.Instance != null)
        {
            CaseManager.Instance.SubmitVerdict(isBlockButton);
        }
        else
        {
            Debug.LogError("¡No hay un CaseManager en la escena!");
        }
    }
}
