using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Se ejecuta al inicio de la escena para inyectar los volúmenes guardados en el AudioMixer.
/// Evita que el Mixer se resetee a sus valores por defecto al cambiar de nivel.
/// </summary>
public class AudioBootstrapper : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [Tooltip("Los nombres exactos expuestos en el Mixer.")]
    [SerializeField] private string[] _exposedParams = { "MasterVolume", "MusicVolume", "SFXVolume" };

    private void Start()
    {
        if (_mixer == null) return;

        foreach (string param in _exposedParams)
        {
            // Lee el valor guardado (0 a 100). Si no existe, asume 100 por defecto.
            int savedVol = PlayerPrefs.GetInt(param, 100);

            // Misma matemática logarítmica que usamos en la radio
            float decibels = savedVol <= 0 ? -80f : Mathf.Clamp(Mathf.Log10(savedVol / 100f) * 20f, -80f, 0f);

            _mixer.SetFloat(param, decibels);
        }

        Debug.Log("<color=lime>[AudioBootstrapper]</color> Volúmenes restaurados desde PlayerPrefs.");
    }
}