using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

/// <summary>
/// Controla un canal de AudioMixer mediante botones físicos VR (Poke).
/// Incluye feedback visual de posición y color sin generar basura en memoria (GC Clean).
/// Adaptado para soportar cambio dinámico de canales.
/// </summary>
public class DiegeticVolumeController : MonoBehaviour
{
    [Header("AudioMixer")]
    [SerializeField] private AudioMixer _targetMixer;
    private string _exposedParamName = "MasterVolume"; // Ahora se inyecta desde el Manager

    [Header("Indicador Visual (Movimiento)")]
    [SerializeField] private Transform _indicatorTarget;
    [SerializeField] private Vector3 _minLocalPosition;
    [SerializeField] private Vector3 _maxLocalPosition;
    [SerializeField] private float _movementSmoothness = 5f;

    [Header("Indicador Visual (Color / Alfa)")]
    [SerializeField] private MeshRenderer _indicatorRenderer;
    [SerializeField] private Color _minVolumeColor = new Color(0f, 0.8f, 1f, 0.2f);
    [SerializeField] private Color _maxVolumeColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Configuración de Volumen")]
    [SerializeField] private int _volumeStep = 10;

    [Header("Feedback Háptico / Audio")]
    public UnityEvent OnVolumeChangedEvent;

    private const float MinDecibels = -80f;
    private const float MaxDecibels = 0f;

    private int _currentVolume;
    private Vector3 _targetLocalPosition;

    private MaterialPropertyBlock _propBlock;
    private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        if (_targetMixer == null || _indicatorTarget == null || _indicatorRenderer == null)
        {
            Debug.LogError($"<color=red>[DiegeticVolume]</color> Faltan referencias en {name}.", this);
            enabled = false;
            return;
        }

        _propBlock = new MaterialPropertyBlock();
        // Nota: La inicialización fuerte ahora ocurre cuando el Manager inyecta el primer canal.
    }

    private void Update()
    {
        if (_indicatorTarget.localPosition == _targetLocalPosition) return;

        _indicatorTarget.localPosition = Vector3.MoveTowards(
            _indicatorTarget.localPosition,
            _targetLocalPosition,
            _movementSmoothness * Time.deltaTime);

        UpdateIndicatorColor();
    }

    /// <summary>
    /// API NUEVA: Inyectada por el RadioChannelManager.
    /// Cambia el canal que estamos controlando, lee el guardado y actualiza la rayita.
    /// </summary>
    public void ChangeActiveMixerParameter(string newParamName)
    {
        _exposedParamName = newParamName;

        // Leemos directamente del disco duro (mucho más exacto y barato que hacer matemática inversa del Mixer)
        _currentVolume = PlayerPrefs.GetInt(_exposedParamName, 100);
        _currentVolume = Mathf.Clamp(_currentVolume, 0, 100);

        // Obligamos a la rayita a saltar a su posición guardada
        _targetLocalPosition = CalculateIndicatorPosition();
    }






    public void IncreaseVolume()
    {
        if (string.IsNullOrEmpty(_exposedParamName)) return;

        int previousVolume = _currentVolume;
        _currentVolume = Mathf.Clamp(_currentVolume + _volumeStep, 0, 100);

        if (_currentVolume == previousVolume && _currentVolume == 100) return;

        ProcessVolumeChange();
    }

    public void DecreaseVolume()
    {
        if (string.IsNullOrEmpty(_exposedParamName)) return;

        int previousVolume = _currentVolume;
        _currentVolume = Mathf.Clamp(_currentVolume - _volumeStep, 0, 100);

        if (_currentVolume == previousVolume && _currentVolume == 0) return;

        ProcessVolumeChange();
    }




    private void ProcessVolumeChange()
    {
        // 1. Aplicamos el sonido al Mixer
        ApplyVolumeToMixer();

        // 2. Calculamos visuales
        _targetLocalPosition = CalculateIndicatorPosition();

        // 3. GUARDAMOS EN DISCO (Persistencia)
        PlayerPrefs.SetInt(_exposedParamName, _currentVolume);
        PlayerPrefs.Save(); // Asegura la escritura inmediata

        // 4. Feedback
        OnVolumeChangedEvent?.Invoke();
    }




    private void ApplyVolumeToMixer()
    {
        float decibels;
        if (_currentVolume <= 0)
        {
            decibels = MinDecibels;
        }
        else
        {
            float normalized = _currentVolume / 100f;
            decibels = Mathf.Log10(normalized) * 20f;
            decibels = Mathf.Clamp(decibels, MinDecibels, MaxDecibels);
        }
        _targetMixer.SetFloat(_exposedParamName, decibels);
    }

    private Vector3 CalculateIndicatorPosition()
    {
        float normalized = _currentVolume / 100f;
        return Vector3.Lerp(_minLocalPosition, _maxLocalPosition, normalized);
    }

    private void UpdateIndicatorColor()
    {
        float distTotal = Vector3.Distance(_minLocalPosition, _maxLocalPosition);
        float distCurrent = Vector3.Distance(_minLocalPosition, _indicatorTarget.localPosition);
        float visualPercent = distTotal > 0.001f ? (distCurrent / distTotal) : 0f;

        Color lerpedColor = Color.Lerp(_minVolumeColor, _maxVolumeColor, visualPercent);

        _indicatorRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorProperty, lerpedColor);
        _indicatorRenderer.SetPropertyBlock(_propBlock);
    }
}