using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    [Header("Configuración de la Misión")]
    [SerializeField] private int _objectsRequired = 5; // Cuántos necesitas
    private int _currentObjectsStolen = 0; // Cuántos llevas

    [Header("Eventos (Opcional)")]
    public UnityEvent OnObjectStolen;
    public UnityEvent OnMissionCompleted;

    private bool _isMissionComplete = false;

    public void RegisterStolenObject()
    {
        if (_isMissionComplete) return;

        _currentObjectsStolen++;
        Debug.Log($"<color=yellow>[Misión]</color> ¡Objeto robado! ({_currentObjectsStolen}/{_objectsRequired})");

        OnObjectStolen?.Invoke();

        if (_currentObjectsStolen >= _objectsRequired)
        {
            CompleteMission();
        }
    }

    private void CompleteMission()
    {
        _isMissionComplete = true;
        Debug.Log("<color=green>[Misión]</color> ¡Misión Completada! El jefe va a estar furioso.");
        OnMissionCompleted?.Invoke();

        // Aquí puedes activar lógica como abrir una puerta de escape, 
        // ganar la partida, o hacer que el jefe camine más rápido.
    }
}
