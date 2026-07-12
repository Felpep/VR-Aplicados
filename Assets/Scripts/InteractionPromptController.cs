using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Controlador único del "Canvas Viajero" para prompts de interacción.
/// Responsabilidad exclusiva: Seguir al NPC activo, hacer Fade y mirar a la cámara.
/// </summary>
public class InteractionPromptController : MonoBehaviour
{
    public static InteractionPromptController Instance { get; private set; }

    [Header("Referencias del Canvas Viajero")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Transform _canvasTransform;
    [SerializeField] private TMP_Text _dialogueText;

    [Header("Configuración de Fade")]
    [SerializeField] private float _fadeSpeed = 8f;

    // Propiedades de la instancia activa
    private Component _currentOwner;
    private Transform _currentAnchor; // <-- NUEVO: Cacheamos el transform para rastrearlo frame a frame

    private float _targetAlpha;
    private bool _isFadeRoutineRunning;

    private Camera _mainCam;
    private Camera MainCam
    {
        get
        {
            if (_mainCam == null) _mainCam = Camera.main;
            return _mainCam;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _canvasGroup.alpha = 0f;
        _targetAlpha = 0f;
    }

    public void RequestShow(Component requester, Transform anchor, string message)
    {
        if (anchor == null) return;

        _currentOwner = requester;
        _currentAnchor = anchor; // <-- NUEVO: Guardamos la referencia del punto de texto del NPC
        _canvasTransform.position = _currentAnchor.position;

        if (_dialogueText != null) _dialogueText.text = message;

        SetTargetAlpha(1f);
    }

    public void RequestHide(Component requester)
    {
        if (_currentOwner != requester) return;

        _currentOwner = null;
        _currentAnchor = null; // <-- NUEVO: Soltamos el ancla al ocultarse
        SetTargetAlpha(0f);
    }

    private void SetTargetAlpha(float alpha)
    {
        _targetAlpha = alpha;
        if (!_isFadeRoutineRunning && gameObject.activeInHierarchy)
            StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        _isFadeRoutineRunning = true;

        while (!Mathf.Approximately(_canvasGroup.alpha, _targetAlpha))
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, _targetAlpha, _fadeSpeed * Time.deltaTime);
            yield return null;
        }

        _canvasGroup.alpha = _targetAlpha;
        _isFadeRoutineRunning = false;
    }

    private void LateUpdate()
    {
        // Early-out: Si es invisible o no hay cámara, no gastamos procesamiento
        if (_canvasGroup.alpha <= 0f || MainCam == null) return;

        // --- NUEVO: SEGUIMIENTO EN TIEMPO REAL ---
        // Si el NPC se está moviendo (patrullando, huyendo, etc.), forzamos al Canvas a arrastrarse con él
        if (_currentAnchor != null)
        {
            _canvasTransform.position = _currentAnchor.position;
        }

        // Proyección horizontal para Billboard perfecto (efecto girasol)
        Vector3 toCamera = MainCam.transform.position - _canvasTransform.position;
        toCamera.y = 0f;

        if (toCamera.sqrMagnitude > 0.0001f)
        {
            _canvasTransform.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isFadeRoutineRunning = false;
    }
}