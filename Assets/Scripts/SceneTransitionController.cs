using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneName;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool isLoading;

    public void LoadScene()
    {
        LoadScene(sceneName);
    }

    public void LoadScene(string targetScene)
    {
        if (isLoading || string.IsNullOrEmpty(targetScene))
            return;

        isLoading = true;

        if (debugLogs)
            Debug.Log($"[SceneTransition] Cargando '{targetScene}'", this);

        SceneManager.LoadScene(targetScene);
    }
}