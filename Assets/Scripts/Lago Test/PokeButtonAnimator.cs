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
    /// Lleva el botón hacia atrás (hundido). Llamar en OnSelect o OnHover.
    /// </summary>
    public void PressDown()
    {
        if (buttonVisual == null) return;

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(MoveToPosition(pressedLocalPos));
    }

    /// <summary>
    /// Regresa el botón hacia adelante (reposo). Llamar en OnUnselect o OnUnhover.
    /// </summary>
    public void ReleaseUp()
    {
        if (buttonVisual == null) return;

        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(MoveToPosition(originalLocalPos));
    }

    // Corrutina única y optimizada que viaja hacia el destino que le pidas
    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        while (Vector3.Distance(buttonVisual.localPosition, targetPos) > 0.001f)
        {
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, targetPos, Time.deltaTime * animationSpeed);
            yield return null;
        }

        // Aseguramos precisión milimétrica al final del movimiento
        buttonVisual.localPosition = targetPos;
    }
}