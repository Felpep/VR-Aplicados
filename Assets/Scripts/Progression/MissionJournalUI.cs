using System.Text;
using TMPro;
using UnityEngine;

public class MissionJournalUI : MonoBehaviour
{
    [Header("UI Component (3D World Space)")]
    [SerializeField] private TextMeshProUGUI _textComponent;

    [Header("Objectives Tracked")]
    [SerializeField] private SimpleObjectiveData[] _objectives;

    [Header("Level Brain Link")]
    [SerializeField] private ProgressionManager _progressionManager;

    private StringBuilder _stringBuilder;

    private void Awake()
    {
        if (_textComponent == null)
        {
            Debug.LogError($"[MissionJournalUI] TextMeshPro (3D) no asignado en {name}.", this);
            enabled = false;
            return;
        }

        _stringBuilder = new StringBuilder();
    }

    private void OnEnable()
    {
        GameEventSystem.OnObjectiveTriggered += RefreshJournalText;
        UpdateJournalDisplay();
    }

    // CORRECCIÓN: Se cambió de OnMirrorDisable a OnDisable para que Unity limpie el evento de verdad
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
        if (_objectives == null || _objectives.Length == 0 || _progressionManager == null) return;

        _stringBuilder.Clear();
        _stringBuilder.AppendLine("<align=center><color=#2C3E50><b>TAREAS DEL DÍA</b></color></align>");
        _stringBuilder.AppendLine();

        for (int i = 0; i < _objectives.Length; i++)
        {
            SimpleObjectiveData obj = _objectives[i];
            if (obj == null) continue;

            // Consultamos al ProgressionManager usando el ID único del archivo
            bool isCompleted = _progressionManager.IsObjectiveCompletedInRuntime(obj.ObjectiveID);

            if (isCompleted)
            {
                // CAMBIO: Ahora tacha el campo .Description en vez del ID técnico
                _stringBuilder.AppendLine($"<color=#7F8C8D><s>• {obj.Description} (HECHO)</s></color>");
            }
            else
            {
                // CAMBIO: Muestra la descripción legible de tu ScriptableObject
                _stringBuilder.AppendLine($"<color=#34495E>• {obj.Description}</color>");
            }
        }

        _textComponent.text = _stringBuilder.ToString();
    }
}
