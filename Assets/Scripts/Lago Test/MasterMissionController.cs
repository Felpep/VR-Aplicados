using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // ¡Vital para el cambio de escenas!

[Serializable]
public class MissionCounterData
{
    [Header("Identificador de Misión")]
    [Tooltip("Debe coincidir exactamente con el ID de tu ScriptableObject")]
    public string objectiveID;

    [Tooltip("Cantidad requerida para completar la misión")]
    public int targetCount;

    [Header("Estado Actual (Solo Lectura)")]
    public int currentCount = 0;
    public bool isCompleted = false;

    [Header("Consecuencias (Unity Events)")]
    public UnityEvent OnCountIncremented;
    public UnityEvent OnMissionCompleted;
}

public class MasterMissionController : MonoBehaviour
{
    [Header("Registro de Misiones por Conteo")]
    [SerializeField] private List<MissionCounterData> counterMissions = new List<MissionCounterData>();

    [Header("Progreso Global del Nivel")]
    [Tooltip("Cantidad de misiones distintas que se deben completar para ganar")]
    public int misionesParaGanar = 2;
    private int misionesCompletadas = 0;
    private bool isTransitioning = false; // Evita que la secuencia se dispare dos veces

    [Header("Transición de Victoria")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public AudioSource audioSource;
    public AudioClip winSound;
    [Tooltip("El nombre exacto de la escena a cargar (ej: 'Nivel2' o 'MenuFinal')")]
    public string nextSceneName;

    public void AddCountToObjective(string id)
    {
        MissionCounterData mission = counterMissions.Find(m => m.objectiveID == id);

        if (mission != null && !mission.isCompleted)
        {
            mission.currentCount++;
            Debug.Log($"[MasterController]: Misión '{id}' progreso -> {mission.currentCount}/{mission.targetCount}");

            mission.OnCountIncremented?.Invoke();

            if (mission.currentCount >= mission.targetCount)
            {
                mission.isCompleted = true;

                Debug.Log($"<color=green>[MasterController] ¡MISIÓN COMPLETADA! -> '{id}' ha alcanzado {mission.targetCount}/{mission.targetCount}.</color>");

                GameEventSystem.TriggerObjective(id);
                mission.OnMissionCompleted?.Invoke();

                // NUEVO: Verificamos si ya ganamos el nivel
                ChequearVictoriaGlobal();
            }
        }
        else if (mission == null)
        {
            Debug.LogWarning($"[MasterController]: Se intentó sumar a '{id}', pero no está registrada en la lista.");
        }
    }

    public void RemoveCountFromObjective(string id)
    {
        MissionCounterData mission = counterMissions.Find(m => m.objectiveID == id);
        if (mission != null && !mission.isCompleted && mission.currentCount > 0)
        {
            mission.currentCount--;
            Debug.Log($"[MasterController]: Misión '{id}' restó progreso -> {mission.currentCount}/{mission.targetCount}");
        }
    }

    // --- NUEVAS FUNCIONES DE TRANSICIÓN ---

    private void ChequearVictoriaGlobal()
    {
        misionesCompletadas++;
        Debug.Log($"<color=cyan>[Progreso Global]</color> Llevas {misionesCompletadas}/{misionesParaGanar} misiones terminadas.");

        // Si alcanzamos la meta y no estamos ya cambiando de escena...
        if (misionesCompletadas >= misionesParaGanar && !isTransitioning)
        {
            StartCoroutine(SecuenciaFinDeNivel());
        }
    }

    private IEnumerator SecuenciaFinDeNivel()
    {
        isTransitioning = true;
        Debug.Log("<color=yellow>¡Iniciando secuencia de fin de nivel!</color>");

        // 1. FADE A NEGRO
        if (fadeCanvas != null)
        {
            yield return StartCoroutine(Fade(0, 1));
        }
        else
        {
            Debug.LogWarning("No hay CanvasGroup asignado. Saltando el Fade...");
        }

        // 2. SONIDO DE VICTORIA (Suena estando la pantalla en negro)
        if (audioSource != null && winSound != null)
        {
            audioSource.PlayOneShot(winSound);
            // Esperamos a que termine el sonido para cambiar de escena
            yield return new WaitForSeconds(winSound.length);
        }
        else
        {
            // Si no le pusiste sonido, hace una pausa chiquita por defecto de 1 segundo
            yield return new WaitForSeconds(1f);
        }

        // 3. CARGAR NUEVA ESCENA
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Error: No pusiste el nombre de la escena en 'nextSceneName'.");
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
}