using UnityEngine;
using TMPro;

/// <summary>
/// Contador de frames optimizado para Realidad Virtual (Meta Quest).
/// Utiliza un búfer de enteros precalculados para evitar allocations de strings en memoria (GC Clean).
/// </summary>
public class VRFPSCounter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text fpsText;

    [Header("Configuración")]
    [SerializeField] private float refreshRate = 0.5f;

    private float _timer;
    private int _frameCount;

    // OPTIMIZACIÓN CORE: Array plano estático con textos pre-formateados de 0 a 150 FPS.
    // Esto evita que Unity instancie memoria de strings en runtime al actualizar el texto.
    private static readonly string[] CachedFPSStrings = new string[151];

    private void Awake()
    {
        if (fpsText == null)
        {
            Debug.LogError($"<color=red>[VRFPSCounter]</color> Falta asignar 'fpsText' en {name}.", this);
            enabled = false;
            return;
        }

        // Llenamos el caché en el inicio del nivel (Warm-up de memoria RAM)
        for (int i = 0; i < CachedFPSStrings.Length; i++)
        {
            CachedFPSStrings[i] = $"FPS: {i}";
        }
    }

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        _frameCount++;

        if (_timer >= refreshRate)
        {
            // Calculamos el promedio exacto
            int fps = Mathf.RoundToInt(_frameCount / _timer);
            fps = Mathf.Clamp(fps, 0, 150); // Protegemos los límites del array

            // ASIGNACIÓN ZERO-ALLOC: Asignamos el string precalculado de la RAM directo a la UI
            fpsText.text = CachedFPSStrings[fps];

#if UNITY_EDITOR
            // Confeccionamos alertas visuales sólidas solo en el Editor de Unity
            if (fps < 72)
            {
                Debug.LogWarning($"[ALERTA VR]: Caída de rendimiento local detectada -> {fps} FPS.");
            }
#endif

            // Feedback visual directo
            if (fps >= 70)
                fpsText.color = Color.green;
            else if (fps >= 55)
                fpsText.color = Color.yellow;
            else
                fpsText.color = Color.red;

            // Reinicio de ciclo libre de saltos asintóticos
            _timer = 0f;
            _frameCount = 0;
        }
    }
}