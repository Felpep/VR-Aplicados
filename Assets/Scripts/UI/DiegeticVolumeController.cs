using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class DiegeticVolumeController : MonoBehaviour
{
    [Header("AudioMixer")]
    [SerializeField] private AudioMixer _targetMixer;
    private string _exposedParamName = "MasterVolume";

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

    // OPTIMIZACIÓN CORE: Guardamos la distancia total al cuadrado para el porcentaje del color
    private float _totalDistanceSqr;

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

        // Precalculamos la distancia al cuadrado una sola vez (Zero Alloc)
        Vector3 totalOffset = _maxLocalPosition - _minLocalPosition;
        _totalDistanceSqr = totalOffset.sqrMagnitude;
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

    public void ChangeActiveMixerParameter(string newParamName)
    {
        _exposedParamName = newParamName;
        _currentVolume = PlayerPrefs.GetInt(_exposedParamName, 100);
        _currentVolume = Mathf.Clamp(_currentVolume, 0, 100);

        _targetLocalPosition = CalculateIndicatorPosition();
        UpdateIndicatorColor();
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
        ApplyVolumeToMixer();
        _targetLocalPosition = CalculateIndicatorPosition();
        PlayerPrefs.SetInt(_exposedParamName, _currentVolume);
        PlayerPrefs.Save();
        OnVolumeChangedEvent?.Invoke();
    }

    private void ApplyVolumeToMixer()
    {
        float decibels = _currentVolume <= 0 ? MinDecibels : Mathf.Clamp(Mathf.Log10(_currentVolume / 100f) * 20f, MinDecibels, MaxDecibels);
        _targetMixer.SetFloat(_exposedParamName, decibels);
    }

    private Vector3 CalculateIndicatorPosition()
    {
        return Vector3.Lerp(_minLocalPosition, _maxLocalPosition, _currentVolume / 100f);
    }

    private void UpdateIndicatorColor()
    {
        // OPTIMIZACIÓN MATEMÁTICA: Usamos la proporción por magnitudes al cuadrado. 
        // El ratio de la división (A^2 / B^2) matemáticamente es equivalente a la raíz del ratio (A / B) para un Lerp lineal de distancias.
        Vector3 currentOffset = _indicatorTarget.localPosition - _minLocalPosition;
        float currentDistanceSqr = currentOffset.sqrMagnitude;

        float visualPercent = _totalDistanceSqr > 0.0001f ? (currentDistanceSqr / _totalDistanceSqr) : 0f;

        Color lerpedColor = Color.Lerp(_minVolumeColor, _maxVolumeColor, visualPercent);

        _indicatorRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(ColorProperty, lerpedColor);
        _indicatorRenderer.SetPropertyBlock(_propBlock);
    }
}