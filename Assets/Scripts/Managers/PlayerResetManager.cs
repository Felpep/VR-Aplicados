using System.Collections;
using UnityEngine;
using UnityEngine.Events; // Obligatorio para usar UnityEvents
using UnityEngine.SceneManagement;

/// <summary>
/// Controla la máquina de estados de reinicio del jugador (Vidas, Teletransporte, Transición).
/// Completamente desacoplado de los sistemas de audio y VFX mediante UnityEvents.
/// </summary>
public class PlayerResetManager : MonoBehaviour
{
    public static PlayerResetManager Instance;

    [Header("Referencias de Posicionamiento")]
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private string sceneName = "Menu";

    [Header("Configuración Visual (Transición)")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Reglas de Juego (Vidas)")]
    [Tooltip("Cantidad máxima de veces que te atrapan antes de perder definitivamente.")]
    [SerializeField] private int maxCatches = 3;
    private int currentCatches = 0;
    private bool isResetting = false;

    [Header("Consecuencias Orientadas a Eventos (Juice/VR Feedback)")]
    [Tooltip("Se ejecuta en el instante en que te atrapan (reinicio normal). Acopla aquí tu SpatialFeedback para vibración sutil y audio de susto.")]
    public UnityEvent OnPlayerReset;

    [Tooltip("Se ejecuta al alcanzar el límite de atrapadas. Acopla aquí el SpatialFeedback con máxima vibración y sonido de despido.")]
    public UnityEvent OnGameOver;

    private void Awake()
    {
        Instance = this;

        // Validación técnica crítica para evitar fallos silenciosos
        if (playerRig == null || respawnPoint == null || fadeCanvas == null)
        {
            Debug.LogError($"<color=red>[ResetManager]</color> Faltan asignar referencias estructurales core en {name}.", this);
            enabled = false;
        }
    }

    /// <summary>
    /// API UNIVERSAL: Invócala desde el tenedor, el jefe o cualquier trampa mortal.
    /// Evalúa el contador de vidas y deriva el flujo al evento correcto.
    /// </summary>
    public void ResetPlayer()
    {
        if (isResetting) return;

        currentCatches++;
        Debug.Log($"<color=orange>[ResetManager]</color> Jugador atrapado. Progreso de castigo: {currentCatches}/{maxCatches}");

        if (currentCatches >= maxCatches)
        {
            StartCoroutine(GameOverRoutine());
        }
        else
        {
            StartCoroutine(ResetRoutine());
        }
    }

    private IEnumerator ResetRoutine()
    {
        isResetting = true;

        // 1. Fundido a negro (Fade In)
        yield return StartCoroutine(Fade(0, 1));

        // 2. DISPARO DEL EVENTO LOCAL (¡Acá actúa tu Spatial Feedback!)
        // El jugador ya no ve nada (pantalla en negro), el control vibra y suena el audio.
        OnPlayerReset?.Invoke();

        // 3. Teletransportar físicamente el Rig de VR al cubículo
        playerRig.position = respawnPoint.position;
        playerRig.rotation = respawnPoint.rotation;

        // Pausa técnica para que el tracking del Quest asimile la nueva posición
        yield return new WaitForSeconds(0.3f);

        // 4. Quitar el fondo negro (Fade Out)
        yield return StartCoroutine(Fade(1, 0));

        isResetting = false;
    }

    private IEnumerator GameOverRoutine()
    {
        isResetting = true;

        // 1. Fundido a negro inmediato
        yield return StartCoroutine(Fade(0, 1));

        // 2. DISPARO DE DERROTA TOTAL
        // Ejecuta tu SpatialFeedback configurado con fuerza extrema.
        OnGameOver?.Invoke();

        // Esperamos 1.5 segundos en oscuridad absoluta para procesar el impacto del audio/hápticos
        yield return new WaitForSeconds(1.5f);

        // 3. Carga de la escena de despido / Game Over
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("<color=red>[ResetManager]</color> Error: No definiste el 'gameOverSceneName' en el Inspector.");
        }
    }

    private IEnumerator Fade(float start, float end)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = end;
    }

    // Propiedad pública opcional por si la UI o la pizarra quieren saber cuántas vidas te quedan
    public int RemainingCatches => maxCatches - currentCatches;
}