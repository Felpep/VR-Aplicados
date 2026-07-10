using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class VRFPSCounter : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí tu objeto de texto (TextMeshPro - 3D o UI)")]
    [SerializeField] private TMP_Text fpsText;

    [Header("Configuración")]
    [Tooltip("Cada cuántos segundos se actualiza el contador en pantalla")]
    [SerializeField] private float refreshRate = 0.5f;

    private float timer;
    private int frameCount;

    private void Update()
    {
        if (fpsText == null) return;

        // Usamos unscaledDeltaTime por si en algún momento pausas el juego (Time.timeScale = 0)
        timer += Time.unscaledDeltaTime;
        frameCount++;

        if (timer >= refreshRate)
        {
            // Calculamos el promedio de FPS
            int fps = Mathf.RoundToInt(frameCount / timer);
            fpsText.text = $"FPS: {fps}";

            // --- NUEVA ALERTA DE RENDIMIENTO SENIOR ---
            // Si baja de 72, disparamos una advertencia amarilla en la consola
            if (fps < 72)
            {
                Debug.LogWarning($"[ALERTA VR]: Los FPS cayeron a {fps}. Revisa qué está consumiendo recursos.");
            }

            // Feedback visual: Verde (Bien), Amarillo (Peligro), Rojo (Mareo inminente)
            if (fps >= 70)
                fpsText.color = Color.green;
            else if (fps >= 50)
                fpsText.color = Color.yellow;
            else
                fpsText.color = Color.red;

            // Reiniciamos los contadores para el siguiente ciclo
            timer -= refreshRate;
            frameCount = 0;
        }
    }
}