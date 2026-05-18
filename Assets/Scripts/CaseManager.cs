using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CaseManager : MonoBehaviour
{
    public static CaseManager Instance { get; private set; }

    [Header("Progresión de la Partida")]
    [Tooltip("Arrastra aquí los ScriptableObjects en el orden en que quieras que se jueguen.")]
    [SerializeField] private CaseData[] levelCases;

    [Tooltip("Arrastra aquí los GameObjects físicos (las Hojas) en el mismo orden que los casos.")]
    [SerializeField] private GameObject[] physicalDocuments;

    [Header("Feedback Visual")]
    public Light roomLight;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    private Color originalLightColor;

    private int currentCaseIndex = 0;
    private List<int> currentlyMarkedMessages = new List<int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (roomLight != null) originalLightColor = roomLight.color;

        // Inicializamos activando solo la primera hoja
        LoadCurrentCase();
    }

    private void LoadCurrentCase()
    {
        currentlyMarkedMessages.Clear();

        for (int i = 0; i < physicalDocuments.Length; i++)
        {
            if (physicalDocuments[i] != null)
            {
                physicalDocuments[i].SetActive(false);
                if (i == currentCaseIndex) physicalDocuments[i].SetActive(true);
            }
        }

        Debug.Log($"Iniciando Caso: {levelCases[currentCaseIndex].caseName}");
    }

    // --- MÉTODOS LLAMADOS POR LOS MENSAJES (VIA SIMPLEVRBUTTON O MESSAGEINTERACTABLE) ---

    public void ToggleMessageMark(int messageID)
    {
        if (currentlyMarkedMessages.Contains(messageID))
        {
            currentlyMarkedMessages.Remove(messageID);
            Debug.Log($"Mensaje {messageID} desmarcado.");
        }
        else
        {
            currentlyMarkedMessages.Add(messageID);
            Debug.Log($"Mensaje {messageID} marcado como sospechoso.");
        }
    }

    // --- MÉTODOS LLAMADOS POR LA ESTACIÓN DE VEREDICTO ---

    public void SubmitVerdict(bool playerChoseToBlock)
    {
        StartCoroutine(EvaluateCaseRoutine(playerChoseToBlock));
    }

    private IEnumerator EvaluateCaseRoutine(bool playerChoseToBlock)
    {
        CaseData currentCase = levelCases[currentCaseIndex];
        bool isSuccess = false;

        // 1. Evaluar si la acción (Bloquear/Archivar) es correcta
        bool isActionCorrect = (playerChoseToBlock == currentCase.isActionBlock);

        // 2. Evaluar si las Red Flags marcadas son EXACTAMENTE las correctas
        // .OrderBy asegura que la comparación funcione sin importar el orden en que el jugador hizo click
        bool areFlagsCorrect = Enumerable.SequenceEqual(
            currentlyMarkedMessages.OrderBy(x => x),
            currentCase.correctMessageIDs.OrderBy(x => x)
        );

        // El jugador solo gana si acertó la acción Y marcó TODAS las banderas correctas (y ninguna falsa)
        if (isActionCorrect && areFlagsCorrect)
        {
            isSuccess = true;
            Debug.Log("¡Veredicto Correcto! Caso Resuelto.");
        }
        else
        {
            Debug.Log("Veredicto Incorrecto. Fallaste el caso.");
        }

        // Feedback Visual
        if (roomLight != null) roomLight.color = isSuccess ? successColor : failColor;

        // Pausa dramática para que el jugador vea el resultado
        yield return new WaitForSeconds(3f);

        if (roomLight != null) roomLight.color = originalLightColor;

        // Progresión
        if (isSuccess)
        {
            currentCaseIndex++;

            if (currentCaseIndex < levelCases.Length)
            {
                // Pasamos a la siguiente hoja
                LoadCurrentCase();
            }
            else
            {
                Debug.Log("¡FELICIDADES! Terminaste tu turno. Has ganado el juego.");
                SceneManager.LoadScene("Game");
                // Aquí podrías cargar una escena final de "Victoria"
            }
        }
        else
        {
            // Si falla, el prototipo actual reinicia el mismo caso (se podría hacer un Post-Mortem después)
            Debug.Log("Reiniciando el mismo caso por error.");
            LoadCurrentCase();
        }
    }
}
