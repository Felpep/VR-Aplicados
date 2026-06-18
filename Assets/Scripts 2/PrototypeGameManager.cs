using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeGameManager : MonoBehaviour
{
    public static PrototypeGameManager Instance { get; private set; }

    [Header("Configuración del Caso (Hardcoded para Prototipo)")]
    [Tooltip("El ID del mensaje que el jugador DEBE marcar para ganar.")]
    public int correctMessageID = 2;

    [Header("Feedback Visual")]
    public Light roomLight;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    private Color originalLightColor;

    // Lista para guardar lo que el jugador ha marcado
    private List<int> markedMessages = new List<int>();

    private void Awake()
    {
        // Singleton simple para fácil acceso
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (roomLight != null) originalLightColor = roomLight.color;
    }

    // --- MÉTODOS LLAMADOS POR LOS MENSAJES ---

    public void ToggleMessageMark(int messageID, bool isMarked)
    {
        if (isMarked && !markedMessages.Contains(messageID))
        {
            markedMessages.Add(messageID);
            Debug.Log($"Mensaje {messageID} marcado como sospechoso.");
        }
        else if (!isMarked && markedMessages.Contains(messageID))
        {
            markedMessages.Remove(messageID);
            Debug.Log($"Mensaje {messageID} desmarcado.");
        }
    }

    // --- MÉTODOS LLAMADOS POR LA ESTACIÓN DE VEREDICTO ---

    public void SubmitVerdict(bool isBlocking)
    {
        Debug.Log("Veredicto enviado. Evaluando...");
        StartCoroutine(EvaluateCase(isBlocking));
    }

    private IEnumerator EvaluateCase(bool playerChoseToBlock)
    {
        bool isSuccess = false;

        // LÓGICA DE EVALUACIÓN MUY SIMPLE PARA EL PROTOTIPO
        // Ganamos si: 
        // 1. Decidió Bloquear.
        // 2. Solo marcó 1 mensaje.
        // 3. Ese mensaje marcado es el correcto (ID 2).

        if (playerChoseToBlock && markedMessages.Count == 1 && markedMessages.Contains(correctMessageID))
        {
            isSuccess = true;
            Debug.Log("¡ÉXITO! Marcó la Red Flag correcta y bloqueó.");
        }
        else
        {
            Debug.Log("FALLO. No marcó lo correcto o tomó la acción equivocada.");
        }

        // Aplicar Feedback Visual
        if (roomLight != null)
        {
            roomLight.color = isSuccess ? successColor : failColor;
        }

        // Esperar 3 segundos para que el jugador vea el resultado
        yield return new WaitForSeconds(3f);

        // Recargar la escena para volver a intentar
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
