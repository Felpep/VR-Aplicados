using System.Collections;
using UnityEngine;

public class PokeButtonAnimator : MonoBehaviour
{
    [Header("Referencias Visuales")]
    [SerializeField] private Transform buttonVisual;

    [Header("Configuración de Animación")]
    [SerializeField] private float moveX = 0.05f;
    [SerializeField] private float animationSpeed = 25f;

    private Vector3 originalLocalPos;
    private Vector3 pressedLocalPos;
    private Coroutine currentAnim;

    private void Awake()
    {
        if (buttonVisual != null)
        {
            originalLocalPos = buttonVisual.localPosition;
            pressedLocalPos = originalLocalPos + new Vector3(moveX, 0f, 0f);
        }
    }

    public void PressDown()
    {
        if (buttonVisual == null) return;
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(MoveToPosition(pressedLocalPos));
    }

    public void ReleaseUp()
    {
        if (buttonVisual == null) return;
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(MoveToPosition(originalLocalPos));
    }

    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        // OPTIMIZACIÓN MATEMÁTICA: MoveTowards es lineal, viaja a velocidad constante y se apaga rápido
        while (buttonVisual.localPosition != targetPos)
        {
            buttonVisual.localPosition = Vector3.MoveTowards(
                buttonVisual.localPosition,
                targetPos,
                animationSpeed * Time.deltaTime
            );
            yield return null;
        }
        currentAnim = null;
    }
}