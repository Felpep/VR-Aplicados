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
    private bool _needsUpdate = false; // Flag anti-condiciones de carrera

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
        _needsUpdate = true; // Forzamos actualización al encender
    }

    private void OnDisable()
    {
        GameEventSystem.OnObjectiveTriggered -= RefreshJournalText;
    }

    private void RefreshJournalText(string objectiveID)
    {
        // En lugar de actualizar YA MISMO, levantamos una bandera.
        // Así le damos tiempo al ProgressionManager de registrar el evento primero.
        _needsUpdate = true;
    }

    private void LateUpdate()
    {
        // Si nadie pidió actualización, no gastamos CPU.
        // Si la pidieron, dibujamos la UI al final del frame.
        if (_needsUpdate)
        {
            UpdateJournalDisplay();
            _needsUpdate = false;
        }
    }

    private void UpdateJournalDisplay()
    {
        if (_objectives == null || _objectives.Length == 0 || _progressionManager == null) return;

        _stringBuilder.Clear();
        _stringBuilder.AppendLine("<align=center><color=#2C3E50><b>TAREAS DEL DÍA</b></color></align>\n");

        for (int i = 0; i < _objectives.Length; i++)
        {
            SimpleObjectiveData obj = _objectives[i];
            if (obj == null) continue;

            bool isCompleted = _progressionManager.IsObjectiveCompletedInRuntime(obj.ObjectiveID);

            if (isCompleted)
            {
                _stringBuilder.AppendLine($"<color=#7F8C8D><s>• {obj.Description} (HECHO)</s></color>");
            }
            else
            {
                _stringBuilder.AppendLine($"<color=#34495E>• {obj.Description}</color>");
            }
        }

        _textComponent.text = _stringBuilder.ToString();
    }
}