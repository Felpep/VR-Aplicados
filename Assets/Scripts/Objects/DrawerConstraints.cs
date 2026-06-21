using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Audio en los extremos de un cajón VR controlado por OneGrabTranslateTransformer.
/// Lee la posición local en Z y reproduce sonido al alcanzar abierto/cerrado.
/// </summary>
public class DrawerConstraints : MonoBehaviour
{
    [Header("Grab Reference")]
    [SerializeField] private MonoBehaviour _grabInteractableSource;

    [Header("Riel (debe coincidir con el Max Z del transformer)")]
    [SerializeField] private float _slideDistance = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioClip _onOpenClip;
    [SerializeField] private AudioClip _onCloseClip;
    [SerializeField] private float _audioVolume = 1f;

    [Header("Limit Tuning")]
    [SerializeField] private float _limitProximityThreshold = 0.015f;

    private IInteractableView _interactableView;
    private float _closedLocalZ;
    private float _openedLocalZ;
    private bool _isAtMaxLimit;
    private bool _isAtMinLimit;

    private void Awake()
    {
        if (_grabInteractableSource == null)
        {
            Debug.LogError($"[DrawerConstraints] Falta Grab Interactable Source en {name}. Desactivando.", this);
            enabled = false;
            return;
        }

        _interactableView = _grabInteractableSource as IInteractableView;

        // Capturamos la posición cerrada al arrancar y calculamos la abierta.
        _closedLocalZ = transform.localPosition.z;
        _openedLocalZ = _closedLocalZ + _slideDistance;
    }

    private void Update()
    {
        // Solo evaluamos si el cajón está agarrado (sin grab no se mueve).
        bool isGrabbed = _interactableView != null &&
                         _interactableView.State == InteractableState.Select;
        if (!isGrabbed) return;

        float currentLocalZ = transform.localPosition.z;
        bool nowAtMin = Mathf.Abs(currentLocalZ - _closedLocalZ) <= _limitProximityThreshold;
        bool nowAtMax = Mathf.Abs(currentLocalZ - _openedLocalZ) <= _limitProximityThreshold;

        if (nowAtMax && !_isAtMaxLimit) PlayLimitSound(_onOpenClip);
        if (nowAtMin && !_isAtMinLimit) PlayLimitSound(_onCloseClip);

        _isAtMaxLimit = nowAtMax;
        _isAtMinLimit = nowAtMin;
    }

    private void PlayLimitSound(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX3D(clip, transform.position, _audioVolume);
    }
}