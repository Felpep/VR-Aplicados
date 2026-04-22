// PrototypeGameManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeGameManager : MonoBehaviour
{
    public static PrototypeGameManager Instance { get; private set; }

    [Header("Feedback Visual")]
    public Light roomLight;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    private Color originalLightColor;
    private Queue<CaseToEvaluate> caseQueue = new Queue<CaseToEvaluate>();
    private CaseToEvaluate activeCase;
    public CaseToEvaluate ActiveCase => activeCase;

    [Header("Botones de Veredicto")]
    public VerdictButton[] verdictButtons;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (roomLight != null)
            originalLightColor = roomLight.color;

        ActivateNextCase();
    }

    // Llamado por cada CaseToEvaluate en su Awake
    public void RegisterCase(CaseToEvaluate newCase)
    {
        Debug.Log(newCase.name + " registrado en el GameManager.");
        caseQueue.Enqueue(newCase);
    }

    private void ActivateNextCase()
    {
        if (caseQueue.Count == 0)
        {
            Debug.Log("Todos los casos completados.");
            return;
        }

        foreach (var button in verdictButtons)
        button.ResetButton();

        activeCase = caseQueue.Dequeue();
        activeCase.gameObject.SetActive(true);
    }

    public void SubmitVerdict(bool playerChoseToBlock)
    {
        if (activeCase == null)
        {
            Debug.LogError("No hay caso activo.");
            return;
        }

        StartCoroutine(EvaluateCase(playerChoseToBlock));
    }

    private IEnumerator EvaluateCase(bool playerChoseToBlock)
    {
        bool isSuccess = activeCase.EvaluateVerdict(playerChoseToBlock);

        if (roomLight != null)
            roomLight.color = isSuccess ? successColor : failColor;

        Debug.Log(isSuccess ? "ÉXITO: Veredicto correcto." : "FALLO: Veredicto incorrecto.");

        LogAnalysisFeedback();

        yield return new WaitForSeconds(3f);

        // Resetear luz antes de pasar al siguiente caso
        if (roomLight != null)
            roomLight.color = originalLightColor;

        ActivateNextCase();
    }

    private void LogAnalysisFeedback()
    {
        var missed = activeCase.GetMissedSuspicious();
        var falsePositives = activeCase.GetFalsePositives();

        if (missed.Count == 0 && falsePositives.Count == 0)
        {
            Debug.Log("Análisis perfecto.");
            return;
        }

        if (missed.Count > 0)
            Debug.Log($"Te perdiste {missed.Count} señal/es de alerta.");

        if (falsePositives.Count > 0)
            Debug.Log($"Marcaste {falsePositives.Count} mensaje/s que no eran sospechosos.");
    }
}