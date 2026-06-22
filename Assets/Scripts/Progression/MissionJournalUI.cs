using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Procesa y formatea el listado de misiones del nivel.
/// Actualiza de forma centralizada tanto el componente local como un array opcional de pizarras espejo.
/// </summary>
public class MissionJournalUI : MonoBehaviour
{
    [Header("UI Principal (Libreta / Mano)")]
    [SerializeField] private TextMeshProUGUI _primaryTextComponent;

    [Header("Pantallas Espejo (Pizarras de la oficina - Opcional)")]
    [Tooltip("Arrastrá acá todos los TextMeshPro de las pizarras de la escena que quieras que muestren lo mismo.")]
    [SerializeField] private TextMeshProUGUI[] _mirrorTextComponents;

    [Header("Objectives Tracked")]
    [SerializeField] private SimpleObjectiveData[] _objectives;

    [Header("Level Brain Link")]
    [SerializeField] private ProgressionManager _progressionManager;

    private StringBuilder _stringBuilder;
    private bool _needsUpdate = false;

    private void Awake()
    {
        if (_primaryTextComponent == null)
        {
            Debug.LogError($"[MissionJournalUI] TextMeshPro principal no asignado en {name}.", this);
            enabled = false;
            return;
        }

        _stringBuilder = new StringBuilder();
    }

    private void OnEnable()
    {
        GameEventSystem.OnObjectiveTriggered += RefreshJournalText;
        _needsUpdate = true; // Forzamos actualización al encender la escena
    }

    private void OnDisable()
    {
        GameEventSystem.OnObjectiveTriggered -= RefreshJournalText;
    }

    private void RefreshJournalText(string objectiveID)
    {
        _needsUpdate = true;
    }

    private void LateUpdate()
    {
        if (_needsUpdate)
        {
            UpdateJournalDisplay();
            _needsUpdate = false;
        }
    }

    private void UpdateJournalDisplay()
    {
        if (_objectives == null || _objectives.Length == 0 || _progressionManager == null) return;

        // 1. Procesamos la matemática del texto UNA SOLA VEZ
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

        // Convertimos el StringBuilder a string una sola vez en este frame
        string finalSheetText = _stringBuilder.ToString();

        // 2. Se lo inyectamos a la pantalla de la mano
        _primaryTextComponent.text = finalSheetText;

        // 3. Se lo repartimos de forma masiva a las pizarras de la pared si es que existen
        if (_mirrorTextComponents != null && _mirrorTextComponents.Length > 0)
        {
            for (int i = 0; i < _mirrorTextComponents.Length; i++)
            {
                if (_mirrorTextComponents[i] != null)
                {
                    _mirrorTextComponents[i].text = finalSheetText;
                }
            }
        }
    }
}