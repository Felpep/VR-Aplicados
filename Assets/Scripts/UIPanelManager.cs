using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    [Header("Configuración de Pantallas")]
    [Tooltip("Arrastra aquí los paneles (GameObjects) en el orden que quieras que aparezcan.")]
    [SerializeField] private GameObject[] panels;

    [Header("Opciones")]
    [Tooltip("Si es TRUE, al llegar a la última pantalla y dar 'Siguiente', vuelve a la primera.")]
    [SerializeField] private bool loopScreens = true;

    private int currentIndex = 0;

    private void Start()
    {
        // Al iniciar, aseguramos que solo la pantalla 0 esté activa
        RefreshPanels();
    }

    // Método PÚBLICO para ser llamado por el botón "Siguiente"
    public void NextScreen()
    {
        if (panels.Length == 0) return;

        currentIndex++;

        if (currentIndex >= panels.Length)
        {
            currentIndex = loopScreens ? 0 : panels.Length - 1;
        }

        RefreshPanels();
    }

    // Método PÚBLICO para ser llamado por el botón "Anterior"
    public void PreviousScreen()
    {
        if (panels.Length == 0) return;

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex = loopScreens ? panels.Length - 1 : 0;
        }

        RefreshPanels();
    }

    private void RefreshPanels()
    {
        // Iteramos por todos los paneles. Prende el actual, apaga el resto.
        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null)
            {
                panels[i].SetActive(i == currentIndex);
            }
        }
    }
}
