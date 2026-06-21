using System.Collections;
using UnityEngine;

public class PlayerResetManager : MonoBehaviour
{
    public static PlayerResetManager Instance;

    [Header("Referencias")]
    public Transform playerRig;
    public Transform respawnPoint;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    private bool isResetting = false;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetPlayer()
    {
        if (!isResetting)
            StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        isResetting = true;

        // Fade In (negro)
        yield return StartCoroutine(Fade(0, 1));

        // Teletransportar
        playerRig.position = respawnPoint.position;
        playerRig.rotation = respawnPoint.rotation;

        yield return new WaitForSeconds(0.2f);

        // Fade Out (volver a ver)
        yield return StartCoroutine(Fade(1, 0));

        isResetting = false;
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            Debug.Log(fadeCanvas.alpha);

            yield return null;
        }

        fadeCanvas.alpha = end;
    }
}
