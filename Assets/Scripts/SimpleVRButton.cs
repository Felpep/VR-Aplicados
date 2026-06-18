using UnityEngine;
using UnityEngine.Events;

public class SimpleVRButton : MonoBehaviour
{
    [Header("Configuración del Botón")]
    [Tooltip("Tiempo en segundos antes de que el botón pueda volver a ser presionado (Anti-Spam).")]
    [SerializeField] private float cooldownTime = 0.5f;
    private float lastPressedTime = 0f;

    [Header("Eventos Modulares")]
    [Tooltip("Arrastra aquí lo que quieres que pase cuando se presione el botón.")]
    public UnityEvent OnButtonPressed;

    // Este método lo vas a llamar desde el "Interactable Unity Event Wrapper" de Meta
    public void ExecutePress()
    {
        if (Time.time - lastPressedTime < cooldownTime) return;

        lastPressedTime = Time.time;

        // Feedback Visual/Sonoro genérico iría aquí (ej. AudioSource.Play())

        OnButtonPressed?.Invoke();
    }
}
