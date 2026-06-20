using System.Collections;
using UnityEngine;

public class PokeButtonAnimator : MonoBehaviour
{
    [Header("Referencias Visuales")]
    [Tooltip("Arrastra aquí el objeto que tiene el Mesh (la parte visual del botón)")]
    [SerializeField] private Transform buttonVisual;

    [Header("Configuración de Animación")]
    [Tooltip("Distancia que se hundirá en el eje X (usa valores pequeños como 0.05)")]
    [SerializeField] private float moveX = 0.05f;

    [Tooltip("Tiempo en segundos que el botón se queda presionado")]
    [SerializeField] private float waitTime = 0.2f;

    [Tooltip("Velocidad de la animación (más alto = más rápido)")]
    [SerializeField] private float animationSpeed = 25f;

    private Vector3 originalLocalPos;
    private Vector3 pressedLocalPos;
    private Coroutine currentAnim;

    private void Awake()
    {
        if (buttonVisual != null)
        {
            // Guardamos la posición inicial
            originalLocalPos = buttonVisual.localPosition;

            // Calculamos la posición final sumando en el eje X local
            pressedLocalPos = originalLocalPos + new Vector3(moveX, 0, 0);
        }
        else
        {
            Debug.LogWarning("[PokeAnimator]: No has asignado el visual del botón en el Inspector.");
        }
    }

    /// <summary>
    /// Esta es la función que debes llamar desde el evento del Poke.
    /// </summary>
    public void TriggerButtonAnimation()
    {
        if (buttonVisual == null) return;

        // Si el jugador hace "spam" de clics, detenemos la animación anterior para que no se buguee
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
        }

        // Iniciamos la nueva secuencia
        currentAnim = StartCoroutine(AnimationSequence());
    }

    private IEnumerator AnimationSequence()
    {
        // 1. Ir hacia atrás (Hundirse en X)
        while (Vector3.Distance(buttonVisual.localPosition, pressedLocalPos) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, pressedLocalPos, Time.deltaTime * animationSpeed);
            yield return null;
        }

        // Aseguramos que llegue exacto a la posición
        buttonVisual.localPosition = pressedLocalPos;

        // 2. Esperar los 0.2 segundos (o lo que configures en el Inspector)
        yield return new WaitForSeconds(waitTime);

        // 3. Volver hacia adelante (Regresar a la original)
        while (Vector3.Distance(buttonVisual.localPosition, originalLocalPos) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, originalLocalPos, Time.deltaTime * animationSpeed);
            yield return null;
        }

        // Aseguramos que quede perfecto en su lugar original
        buttonVisual.localPosition = originalLocalPos;
    }
}