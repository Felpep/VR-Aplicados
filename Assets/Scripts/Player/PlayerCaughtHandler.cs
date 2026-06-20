using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente del Player Rig que gestiona el castigo de captura: fundido,
/// teletransporte al cubículo y restauración de vista. No conoce a la IA;
/// se conecta exclusivamente vía UnityEvent desde el Inspector.
/// </summary>
public class PlayerCaughtHandler : MonoBehaviour
{
    [Header("Fade Timing")]
    [SerializeField] private float _fadeOutDuration = 0.4f;
    [SerializeField] private float _fadeInDuration = 0.4f;
    [SerializeField] private float _holdDuration = 0.15f;

    [Header("Fade Events (conectar a un Canvas/Image en cámara VR)")]
    public UnityEvent OnFadeOut;
    public UnityEvent OnFadeIn;

    private Coroutine _activeRespawn;

    /// <summary>
    /// Inicia el ciclo de fundido + teletransporte + restauración hacia el cubículo indicado.
    /// Ignora llamadas concurrentes mientras un respawn ya está en curso.
    /// </summary>
    public void RespawnAtCubicle(Transform cubicleAnchor)
    {
        if (cubicleAnchor == null)
        {
            Debug.LogError($"[PlayerCaughtHandler] cubicleAnchor nulo en {name}. Respawn cancelado.", this);
            return;
        }

        if (_activeRespawn != null) return;

        _activeRespawn = StartCoroutine(RespawnRoutine(cubicleAnchor));
    }

    private IEnumerator RespawnRoutine(Transform cubicleAnchor)
    {
        OnFadeOut?.Invoke();
        yield return new WaitForSeconds(_fadeOutDuration);

        transform.position = cubicleAnchor.position;
        transform.rotation = cubicleAnchor.rotation;

        yield return new WaitForSeconds(_holdDuration);

        OnFadeIn?.Invoke();
        yield return new WaitForSeconds(_fadeInDuration);

        _activeRespawn = null;
    }
}