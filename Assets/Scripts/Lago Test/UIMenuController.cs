using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMenuController : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [SerializeField] private bool saveVolume = true;

    [Header("Referencias VR (Confort)")]
    [Tooltip("Arrastra aquí el script OVRVignette que está en tu cámara VR.")]
    [SerializeField] private Behaviour vignetteScript;

    private void Start()
    {
        // Cargar Volumen
        if (saveVolume)
        {
            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        }

        // Cargar preferencia de Viñeta (1 = activado, 0 = desactivado. Por defecto 0)
        if (vignetteScript != null)
        {
            bool isVignetteSaved = PlayerPrefs.GetInt("VR_Vignette", 0) == 1;
            vignetteScript.enabled = isVignetteSaved;
        }
    }

    // --- MÉTODOS DE ESCENA Y AUDIO (Los que ya tenías) ---
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SetMasterVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        AudioListener.volume = clampedVolume;
        if (saveVolume) PlayerPrefs.SetFloat("MasterVolume", clampedVolume);
    }

    // --- NUEVOS MÉTODOS VR ---

    /// <summary>
    /// Recentra la posición y rotación del jugador en el espacio físico.
    /// </summary>
    public void RecenterVRView()
    {
        Debug.Log("[UI]: Recentrar Vista VR ejecutado.");
        // Este es el método oficial y más estable del SDK de Meta
        OVRManager.display.RecenterPose();
    }

    /// <summary>
    /// Activa o desactiva la Viñeta (Tunneling). 
    /// Se enlaza dinámicamente al OnValueChanged de un Toggle de Unity UI.
    /// </summary>
    public void SetVignetteState(bool isEnabled)
    {
        Debug.Log($"[UI]: Efecto de Viñeta cambiado a -> {isEnabled}");
        if (vignetteScript != null)
        {
            vignetteScript.enabled = isEnabled;
            // Guardamos la preferencia del jugador
            PlayerPrefs.SetInt("VR_Vignette", isEnabled ? 1 : 0);
        }
        else
        {
            Debug.LogWarning("[UI]: Intentaste cambiar la viñeta, pero no asignaste el script OVRVignette en el Inspector.");
        }
    }
     
    /// <summary>
    /// Cierra la aplicación de forma segura.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[UI]: Saliendo del juego...");
        Application.Quit();
    }
}