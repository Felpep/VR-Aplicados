using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Pule el comportamiento físico de un cajón VR controlado por ConfigurableJoint.
/// Añade audio en los extremos, frena el jittering y duerme el Rigidbody 
/// para mantener cero consumo de CPU cuando está inactivo.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class DrawerConstraints : MonoBehaviour
{
    [Header("Joint Reference")]
    [SerializeField] private ConfigurableJoint _joint;

    [Header("Grab Reference (hijo con GrabInteractable)")]
    [SerializeField] private MonoBehaviour _grabInteractableSource;

    [Header("Audio")]
    [SerializeField] private AudioClip _onOpenClip;
    [SerializeField] private AudioClip _onCloseClip;
    [SerializeField] private float _audioVolume = 1f;

    [Header("Limit Tuning")]
    [Tooltip("Distancia en unidades locales al límite para considerarlo 'alcanzado'.")]
    [SerializeField] private float _limitProximityThreshold = 0.015f;

    private Rigidbody _rigidbody;
    private IInteractableView _interactableView;

    // Asumimos que el cajón se ensambla cerrado.
    private float _closedLocalZ;
    private float _openedLocalZ;

    private bool _isAtMaxLimit;
    private bool _isAtMinLimit;
    private bool _isSleeping;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (_joint == null || _grabInteractableSource == null)
        {
            Debug.LogError($"[VRDrawerConstraints] Referencias faltantes en {name}. Desactivando.", this);
            enabled = false;
            return;
        }

        _interactableView = _grabInteractableSource as IInteractableView;

        // El ancla inicial es la posición cerrada (0 relativo).
        // El límite máximo es la posición inicial + la magnitud del límite del Joint.
        _closedLocalZ = transform.localPosition.z;
        _openedLocalZ = _closedLocalZ + _joint.linearLimit.limit;

        // Evaluamos en el frame cero para dormir el cajón inmediatamente si arranca cerrado
        EvaluateLimits();
        if (_isAtMinLimit) SleepRigidbody();
    }

    private void Update()
    {
        bool isGrabbed = _interactableView != null && _interactableView.State == InteractableState.Select;

        if (isGrabbed)
        {
            WakeIfSleeping();
            EvaluateLimits();
            return;
        }

        // OPTIMIZACIÓN MÁXIMA: Si no está agarrado y ya está durmiendo, no calculamos NADA.
        if (_isSleeping) return;

        EvaluateLimits();

        // Si se soltó, cerró por inercia o colisión, y alcanzó el mínimo, se duerme.
        if (_isAtMinLimit && !_isSleeping)
        {
            SleepRigidbody();
        }
    }

    private void EvaluateLimits()
    {
        float currentLocalZ = transform.localPosition.z;

        bool nowAtMin = Mathf.Abs(currentLocalZ - _closedLocalZ) <= _limitProximityThreshold;
        bool nowAtMax = Mathf.Abs(currentLocalZ - _openedLocalZ) <= _limitProximityThreshold;

        if (nowAtMax && !_isAtMaxLimit)
        {
            ClampVelocity();
            PlayLimitSound(_onOpenClip);
        }

        if (nowAtMin && !_isAtMinLimit)
        {
            ClampVelocity();
            PlayLimitSound(_onCloseClip);
        }

        _isAtMaxLimit = nowAtMax;
        _isAtMinLimit = nowAtMin;
    }

    private void ClampVelocity()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    private void PlayLimitSound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX3D(clip, transform.position, _audioVolume);
    }

    private void SleepRigidbody()
    {
        _rigidbody.Sleep();
        _isSleeping = true;
    }

    private void WakeIfSleeping()
    {
        if (!_isSleeping) return;
        _rigidbody.WakeUp();
        _isSleeping = false;
    }
}