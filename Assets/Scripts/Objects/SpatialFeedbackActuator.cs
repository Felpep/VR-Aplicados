using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Orquesta feedback tridimensional sincrónico (audio, VFX, hápticos).
/// Autogestiona la búsqueda de mandos VR usando un radar físico esférico en el instante del disparo.
/// </summary>
public class SpatialFeedbackActuator : MonoBehaviour
{
    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip _sfxClip;
    [SerializeField] private float _volume = 1f;

    [Header("VFX (Opcional)")]
    [Tooltip("El prefab del efecto visual que tiene colgado el script PooledVFXReturner")]
    [SerializeField] private GameObject _vfxPrefab;

    [Header("Haptics Avanzados (Radar Autónomo)")]
    [SerializeField] private bool _triggerHaptics = true;
    [Tooltip("Radio en metros para buscar el mando del jugador alrededor de este objeto.")]
    [SerializeField] private float _radarRadius = 1.5f;
    [SerializeField] private float _hapticDuration = 0.2f;
    [SerializeField] private float _hapticAmplitude = 0.5f;

    [Header("Debug / Controller Quick Test")]
    [SerializeField] private OVRInput.Button _testControllerButton = OVRInput.Button.One;
    [SerializeField] private OVRInput.Controller _testControllerActive = OVRInput.Controller.RTouch;

    // Buffer estático para el OverlapSphere. Evita que Android cree basura en memoria (GC Clean)
    private readonly Collider[] _radarBuffer = new Collider[8];

    /// <summary>
    /// API UNIVERSAL: Conéctalo a CUALQUIER UnityEvent (OnLightsOff, OnActionTriggered, misiones, etc).
    /// Ejecuta Audio, VFX y busca automáticamente mandos cercanos mediante un radar físico.
    /// </summary>
    public void TriggerFeedback()
    {
        // 1. Disparar Sonido
        if (_sfxClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX3D(_sfxClip, transform.position, _volume);
        }

        // 2. Disparar Partículas
        if (_vfxPrefab != null && GameObjectPoolManager.Instance != null)
        {
            GameObjectPoolManager.Instance.Spawn(_vfxPrefab, transform.position, transform.rotation);
        }

        // 3. Radar de Hápticos
        if (_triggerHaptics)
        {
            ScanAndVibrateNearbyControllers();
        }
    }

    /// <summary>
    /// Lanza una esfera física invisible para buscar tags de mandos VR ("LeftHand" o "RightHand")
    /// </summary>
    private void ScanAndVibrateNearbyControllers()
    {
        // Lanzamos el radar esférico optimizado
        int hits = Physics.OverlapSphereNonAlloc(transform.position, _radarRadius, _radarBuffer);

        for (int i = 0; i < hits; i++)
        {
            Collider col = _radarBuffer[i];
            if (col == null) continue;

            InputDeviceCharacteristics targetHand;

            // Verificamos si lo que entró en el radar es un mando por su Tag
            if (col.CompareTag("LeftHand")) targetHand = InputDeviceCharacteristics.Left;
            else if (col.CompareTag("RightHand")) targetHand = InputDeviceCharacteristics.Right;
            else continue; // Si no es un mando, seguimos buscando en los otros hits

            // Si encontramos un mando válido, enviamos la vibración nativa de Meta Quest
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(targetHand | InputDeviceCharacteristics.Controller, devices);

            if (devices.Count > 0 && devices[0].TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
            {
                devices[0].SendHapticImpulse(0u, _hapticAmplitude, _hapticDuration);
#if UNITY_EDITOR
                Debug.Log($"[SpatialFeedback] Mando detectable '{col.tag}' encontrado por OverlapSphere. Pulso háptico enviado.");
#endif
            }
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        // El test de teclado/mando ahora llama directamente a la función limpia
        if (OVRInput.GetDown(_testControllerButton, _testControllerActive))
        {
            Debug.Log($"[SpatialFeedbackActuator] Testeo activado en {name} usando el botón {_testControllerButton}");
            TriggerFeedback();
        }
    }

    // Dibuja el radio del radar en la ventana de Scene de Unity para calibrar la distancia fácil
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _radarRadius);
    }
#endif
}