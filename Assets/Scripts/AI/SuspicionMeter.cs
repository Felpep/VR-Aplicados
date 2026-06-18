using UnityEngine;

public class SuspicionMeter : MonoBehaviour
{
    [Header("Suspicion Settings")]
    [SerializeField] private float _maxSuspicion = 100f;
    [SerializeField] private float _cooldownRate = 5f; // Cuánto baja por segundo si estás a salvo

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip _tensionMusic;

    private float _currentSuspicion;
    private bool _isGameOver;

    public float CurrentSuspicionPercentage => _currentSuspicion / _maxSuspicion;

    private void OnEnable()
    {
        // El medidor se suscribe al Bus global que ya creamos en pasos anteriores
        //VRGameEvents.OnNoiseGenerated += IncreaseSuspicionByNoise;
    }

    private void OnDisable()
    {
        //GameEventSystem.OnNoiseGenerated -= IncreaseSuspicionByNoise;
    }

    private void Update()
    {
        if (_isGameOver) return;

        // Si la sospecha está alta y nadie está generando eventos, decae progresivamente (cooldown)
        if (_currentSuspicion > 0f)
        {
            _currentSuspicion -= _cooldownRate * Time.deltaTime;
            _currentSuspicion = Mathf.Max(_currentSuspicion, 0f);
        }
    }

    /// <summary>
    /// API Pública para que el script EnemyPerception de la IA llame directamente 
    /// mientras te está viendo (en estado Chase o Suspicion)
    /// </summary>
    public void IncreaseSuspicionByVision(float amount)
    {
        if (_isGameOver) return;

        _currentSuspicion += amount * Time.deltaTime;
        EvaluateSuspicionState();
    }

    private void IncreaseSuspicionByNoise(float intensity)
    {
        if (_isGameOver) return;

        // Un ruido físico genera un pico de sospecha instantáneo en lugar de constante
        _currentSuspicion += intensity;
        EvaluateSuspicionState();
    }

    private void EvaluateSuspicionState()
    {
        if (_currentSuspicion >= _maxSuspicion)
        {
            TriggerGameOver();
            return;
        }

        // Si la sospecha cruza el 50%, le ordenamos a tu AudioManager que tire música de tensión
        if (CurrentSuspicionPercentage > 0.5f)
        {
            AudioManager.Instance.PlayMusic(_tensionMusic);
        }
    }

    private void TriggerGameOver()
    {
        _isGameOver = true;
        _currentSuspicion = _maxSuspicion;

#if UNITY_EDITOR
        Debug.Log("<color=red>[GAME OVER]</color> El jefe te descubrió causando caos. Estás despedido.");
#endif

        // Frenamos las lógicas del nivel (desactivar locomoción, etc)
        Time.timeScale = 0f;
    }
}