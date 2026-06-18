using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente modular para contar eventos. Separa la lógica de conteo de la detección física.
/// </summary>
public class ModularEventCounter : MonoBehaviour
{
    [Header("Progression Data")]
    [Tooltip("El ID exacto del ScriptableObject que se completará al llegar a la meta.")]
    [SerializeField] private string objectiveID;

    [Tooltip("Cantidad de veces que debe ocurrir la acción para ganar.")]
    [SerializeField] private int targetCount = 5;

    [Header("Local Consequences (Unity Events)")]
    [Tooltip("Se dispara cada vez que la cuenta aumenta (ej: sonido de 'Acierto').")]
    public UnityEvent OnCountIncremented;

    [Tooltip("Se dispara al alcanzar la meta (ej: sonido de victoria, confeti).")]
    public UnityEvent OnTargetReached;

    private int currentCount = 0;
    private bool isCompleted = false;

    // Método público que será llamado por los tachos de basura (o cualquier otro trigger)
    public void AddCount()
    {
        if (isCompleted) return;

        currentCount++;
        Debug.Log($"[Progression]: Conteo incrementado {currentCount}/{targetCount}");

        // 1. Disparar feedback local (sonidos, partículas)
        OnCountIncremented?.Invoke();

        if (currentCount >= targetCount)
        {
            isCompleted = true;

            // 2. Notificar al sistema global (GameEventSystem) que la misión se cumplió
            if (!string.IsNullOrEmpty(objectiveID))
            {
                GameEventSystem.TriggerObjective(objectiveID);
            }

            // 3. Disparar feedback de éxito total
            OnTargetReached?.Invoke();
        }
    }

    // Método por si la misión se reinicia
    public void ResetCounter()
    {
        currentCount = 0;
        isCompleted = false;
    }
}