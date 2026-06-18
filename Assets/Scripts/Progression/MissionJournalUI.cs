using System.Text;
using TMPro;
using UnityEngine;

public class MissionJournalUI : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private TextMeshProUGUI _textComponent;

    [Header("Objectives Tracked")]
    [SerializeField] private SimpleObjectiveData[] _objectives;

    private StringBuilder _stringBuilder;

    private void Awake()
    {
        if (_textComponent == null)
        {
            Debug.LogError($"[MissionJournalUI] TextMeshPro no asignado en {name}.");
            enabled = false;
            return;
        }

        // El uso de StringBuilder en VR evita generar basura (Garbage Collection) al concatenar strings
        _stringBuilder = new StringBuilder();
    }

    private void OnEnable()
    {
        GameEventSystem.OnObjectiveTriggered += RefreshJournalText;
        UpdateJournalDisplay();
    }

    private void OnDisable()
    {
        GameEventSystem.OnObjectiveTriggered -= RefreshJournalText;
    }

    private void RefreshJournalText(string objectiveID)
    {
        UpdateJournalDisplay();
    }

    private void UpdateJournalDisplay()
    {
        if (_objectives == null || _objectives.Length == 0) return;

        _stringBuilder.Clear();
        _stringBuilder.AppendLine("TAREAS DEL DÍA");
        _stringBuilder.AppendLine();

        for (int i = 0; i < _objectives.Length; i++)
        {
            SimpleObjectiveData obj = _objectives[i];
            if (obj == null) continue;

            if (obj.IsCompleted)
            {
                // Formato de texto tachado rico nativo de TextMeshPro
                _stringBuilder.AppendLine($"<s>• {obj.ObjectiveID} (HECHO)</s>");
            }
            else
            {
                _stringBuilder.AppendLine($"• {obj.ObjectiveID}");
            }
        }

        _textComponent.text = _stringBuilder.ToString();
    }
}
