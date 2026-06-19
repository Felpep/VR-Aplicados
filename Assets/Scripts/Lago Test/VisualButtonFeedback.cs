using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VisualButtonFeedback : MonoBehaviour
{
    [SerializeField] private Transform buttonVisual;

    [Tooltip("Distancia que sumará en Z. Nota: 1.0f es 1 METRO entero. Sugerido: 0.05f para botones pequeños.")]
    [SerializeField] private float pressDistance = 100.0f;

    [SerializeField] private float pressSpeed = 15f;

    [Tooltip("Tiempo que se queda hundido antes de volver a salir.")]
    [SerializeField] private float waitTime = 0.5f;

    [SerializeField] private AudioClip pressSound;

    private AudioSource audioSource;
    private Vector3 originalLocalPosition;
    private Vector3 pressedLocalPosition;
    private Coroutine pressCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (buttonVisual != null)
        {
            originalLocalPosition = buttonVisual.localPosition;

            // CAMBIO 1: Ahora SUMA la distancia en el eje Z para ir hacia atrás
            pressedLocalPosition = originalLocalPosition + new Vector3(0, 0, pressDistance);
        }
    }

    public void AnimatePress()
    {
        if (buttonVisual == null) return;

        if (pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }

        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
        }

        pressCoroutine = StartCoroutine(PressRoutine());
    }

    private IEnumerator PressRoutine()
    {
        // Fase 1: Ir hacia atrás (Sumar en Z)
        while (Vector3.Distance(buttonVisual.localPosition, pressedLocalPosition) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, pressedLocalPosition, Time.deltaTime * pressSpeed);
            yield return null;
        }

        // Aseguramos matemáticamente que el botón quede exactamente en la posición hundida final
        buttonVisual.localPosition = pressedLocalPosition;

        // CAMBIO 2: Esperar medio segundo (o lo que configures en el Inspector)
        yield return new WaitForSeconds(waitTime);

        // Fase 3: Volver hacia adelante (Restar en Z hasta volver a la original)
        while (Vector3.Distance(buttonVisual.localPosition, originalLocalPosition) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, originalLocalPosition, Time.deltaTime * pressSpeed);
            yield return null;
        }

        // Seguridad: Asegurar que quede exactamente en su posición inicial al terminar
        buttonVisual.localPosition = originalLocalPosition;
    }
}