using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Orquesta feedback tridimensional sincrónico (audio, VFX, hápticos) desde
/// un punto fijo del escenario.
/// </summary>
public class SpatialFeedbackActuator : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip _sfxClip;
    [SerializeField] private float _volume = 1f;

    [Header("Haptics (VR Only)")]
    [SerializeField] private bool _triggerHaptics = true;
    [SerializeField] private float _hapticDuration = 0.2f;
    [SerializeField] private float _hapticAmplitude = 0.5f;

    [Header("VFX")]
    [Tooltip("El prefab del efecto visual que tiene colgado el script PooledVFXReturner")]
    [SerializeField] private GameObject _vfxPrefab;


    [Header("Debug / Controller Quick Test")]
    [Tooltip("Botón físico del mando de Meta para activar el testeo (ej: One = Botón A o X, Two = B o Y)")]
    [SerializeField] private OVRInput.Button _testControllerButton = OVRInput.Button.One;
    [Tooltip("Mando físico desde el cual se leerá el botón de prueba")]
    [SerializeField] private OVRInput.Controller _testControllerActive = OVRInput.Controller.RTouch;
    [Tooltip("La mano de referencia obligatoria para que el pulso de vibración sepa a dónde ir")]
    [SerializeField] private Transform _testInteractionSource;



    /// <summary>
    /// Conectar este método a tu MetaInteractableEventObserver en OnSelected/OnUnselected
    /// </summary>
    public void TriggerAllFeedbacks(Transform interactionSource)
    {
        // 1. Disparar Sonido
        if (_sfxClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX3D(_sfxClip, transform.position, _volume);

        // 2. Disparar Partículas usando el Mánager Centralizado
        if (_vfxPrefab != null && GameObjectPoolManager.Instance != null)
            GameObjectPoolManager.Instance.Spawn(_vfxPrefab, transform.position, transform.rotation);

        // 3. Disparar Vibración
        if (_triggerHaptics && interactionSource != null)
            SendHapticPulse(interactionSource);
    }

    private void SendHapticPulse(Transform interactionSource)
    {
        InputDeviceCharacteristics targetHand;

        if (interactionSource.CompareTag("LeftHand")) targetHand = InputDeviceCharacteristics.Left;
        else if (interactionSource.CompareTag("RightHand")) targetHand = InputDeviceCharacteristics.Right;
        else return;

        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(targetHand | InputDeviceCharacteristics.Controller, devices);

        if (devices.Count == 0) return;

        if (devices[0].TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
        {
            devices[0].SendHapticImpulse(0u, _hapticAmplitude, _hapticDuration);
        }
    }



#if UNITY_EDITOR
    private void Update()
    {
        if (OVRInput.GetDown(_testControllerButton, _testControllerActive))
        {
            Debug.Log($"[SpatialFeedbackActuator] Testeo de mando activado en {name} usando el botón {_testControllerButton}");
            TriggerAllFeedbacks(_testInteractionSource);
        }
    }
#endif
}