using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class SpatialFeedbackActuator : MonoBehaviour
{
    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip _sfxClip;
    [SerializeField] private float _volume = 1f;

    [Header("VFX (Opcional)")]
    [SerializeField] private GameObject _vfxPrefab;

    [Header("Haptics Avanzados (Radar Autónomo)")]
    [SerializeField] private bool _triggerHaptics = true;
    [SerializeField] private float _radarRadius = 1.5f;
    [SerializeField] private float _hapticDuration = 0.2f;
    [SerializeField] private float _hapticAmplitude = 0.5f;

    private readonly Collider[] _radarBuffer = new Collider[8];

    // OPTIMIZACIÓN CORE: Lista persistente en RAM para evitar allocations en runtime (Zero Alloc)
    private readonly List<InputDevice> _cachedDevicesBuffer = new List<InputDevice>(2);

    public void TriggerFeedback()
    {
        if (_sfxClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX3D(_sfxClip, transform.position, _volume);
        }

        if (_vfxPrefab != null && GameObjectPoolManager.Instance != null)
        {
            GameObjectPoolManager.Instance.Spawn(_vfxPrefab, transform.position, transform.rotation);
        }

        if (_triggerHaptics)
        {
            ScanAndVibrateNearbyControllers();
        }
    }

    private void ScanAndVibrateNearbyControllers()
    {
        int hits = Physics.OverlapSphereNonAlloc(transform.position, _radarRadius, _radarBuffer);

        for (int i = 0; i < hits; i++)
        {
            Collider col = _radarBuffer[i];
            if (col == null) continue;

            InputDeviceCharacteristics targetHand;

            if (col.CompareTag("LeftHand")) targetHand = InputDeviceCharacteristics.Left;
            else if (col.CompareTag("RightHand")) targetHand = InputDeviceCharacteristics.Right;
            else continue;

            // OPTIMIZACIÓN DE MEMORIA: Limpiamos el búfer y evitamos el 'new List' interno de Unity
            _cachedDevicesBuffer.Clear();
            InputDevices.GetDevicesWithCharacteristics(targetHand | InputDeviceCharacteristics.Controller, _cachedDevicesBuffer);

            if (_cachedDevicesBuffer.Count > 0)
            {
                InputDevice device = _cachedDevicesBuffer[0];
                if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
                {
                    device.SendHapticImpulse(0u, _hapticAmplitude, _hapticDuration);
                }
            }
        }
    }
}