using UnityEngine;
using TMPro; // Para actualizar el texto de la radio

/// <summary>
/// Gestiona la selección ciclada de canales de audio (Master, Music, SFX) en la radio diegética.
/// Actúa como el cerebro que le dice al DiegeticVolumeController qué parámetro modificar.
/// </summary>
public class RadioChannelManager : MonoBehaviour
{
    [System.Serializable]
    public class AudioChannel
    {
        public string channelDisplayName; // Ej: "VOLUMEN GLOBAL", "MÚSICA", "EFECTOS"
        public string mixerParameterName; // Ej: "MasterVolume", "MusicVolume", "SFXVolume"
    }

    [Header("Configuración de Canales")]
    [Tooltip("La lista de canales por los que va a rotar el botón.")]
    [SerializeField] private AudioChannel[] _channels;

    [Header("Referencias de UI y Lógica")]
    [Tooltip("El texto físico pegado a la radio (ej. LCD retro).")]
    [SerializeField] private TextMeshProUGUI _channelTextDisplay;
    [Tooltip("El controlador de volumen que moverá la rayita y hablará con el Mixer.")]
    [SerializeField] private DiegeticVolumeController _volumeController;

    private int _currentChannelIndex = 0;

    private void Start()
    {
        if (_channels == null || _channels.Length == 0 || _volumeController == null)
        {
            Debug.LogError("<color=red>[RadioManager]</color> Faltan configurar canales o referencias en el Inspector.");
            enabled = false;
            return;
        }

        // Forzamos la actualización inicial al primer canal (ej: Master)
        UpdateActiveChannel();
    }

    /// <summary>
    /// Avanza al siguiente canal en la lista y da la vuelta si llega al final.
    /// Conectar al WhenSelect del PokeInteractable del botón "CAMBIAR CANAL".
    /// </summary>
    public void CycleNextChannel()
    {
        _currentChannelIndex++;

        // Loop back to 0 if we exceed the array
        if (_currentChannelIndex >= _channels.Length)
        {
            _currentChannelIndex = 0;
        }

        UpdateActiveChannel();
    }

    private void UpdateActiveChannel()
    {
        AudioChannel activeChannel = _channels[_currentChannelIndex];

        // 1. Actualizamos el texto de la pantalla LCD de la radio
        if (_channelTextDisplay != null)
        {
            _channelTextDisplay.text = activeChannel.channelDisplayName;
        }

        // 2. Le inyectamos el nuevo parámetro al controlador de la rayita
        _volumeController.ChangeActiveMixerParameter(activeChannel.mixerParameterName);

        Debug.Log($"<color=cyan>[RadioManager]</color> Canal cambiado a: {activeChannel.channelDisplayName}");
    }
}