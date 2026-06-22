using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // ¡Vital para poder cambiar de escena!

public class PlayerResetManager : MonoBehaviour
{
    public static PlayerResetManager Instance;

    [Header("Referencias")]
    public Transform playerRig;
    public Transform respawnPoint;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1f;

    [Header("Game Over Settings")]
    [Tooltip("Cantidad máxima de veces que te atrapan antes de perder")]
    public int maxCatches = 3;
    [Tooltip("El AudioSource que va a reproducir el sonido")]
    public AudioSource audioSource;
    [Tooltip("El sonido que pasará cuando te atrapen por última vez")]
    public AudioClip gameOverSound;
    [Tooltip("El nombre exacto de la escena que querés abrir (ej: 'PantallaDespido')")]
    public string gameOverSceneName;

    // Nuestro contador interno
    private int currentCatches = 0;
    private bool isResetting = false;

    private void Awake()
    {
        Instance = this;
    }

    public void ResetPlayer()
    {
        if (!isResetting)
        {
            // 1. Sumamos 1 al contador cada vez que el jefe nos atrapa
            currentCatches++;
            Debug.Log($"<color=orange>[Jefe]</color> Te atraparon. Llevas {currentCatches}/{maxCatches} atrapadas.");

            // 2. Comparamos si ya llegamos (o pasamos) el límite
            if (currentCatches >= maxCatches)
            {
                // Disparamos la rutina de perder
                StartCoroutine(GameOverRoutine());
            }
            else
            {
                // Disparamos la rutina normal de reinicio
                StartCoroutine(ResetRoutine());
            }
        }
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

    private IEnumerator GameOverRoutine()
    {
        isResetting = true;

        // 1. Fade In (Pantalla en negro para mayor impacto)
        yield return StartCoroutine(Fade(0, 1));

        // 2. Reproducimos el sonido del Jefe enojado / Despido
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);

           
        }

        // Una pequeña pausa en negro por si acaso
        yield return new WaitForSeconds(1f);

        // 3. Cargamos la escena de Game Over
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            Debug.LogError("Error: No escribiste el nombre de la escena de Game Over en el Inspector.");
        }
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            // Debug.Log(fadeCanvas.alpha); // Lo comenté para no saturar tu consola
            yield return null;
        }

        fadeCanvas.alpha = end;
    }
}