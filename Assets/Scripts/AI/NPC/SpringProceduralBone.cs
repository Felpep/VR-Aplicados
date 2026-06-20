using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Resorte procedimental aditivo que registra la pose local del hueso.
/// Si la cabeza se estira más allá de un umbral máximo, fuerza al SDK de Meta
/// a cancelar el agarre de inmediato.
/// </summary>
public class SpringProceduralBone : MonoBehaviour
{
    [Header("Spring Physics")]
    [SerializeField] private float _stiffness = 150f;
    [SerializeField] private float _damping = 10f;

    [Header("VR Limits (Escape de Agarre)")]
    [Tooltip("Distancia máxima en metros locales que el jugador puede estirar la cabeza antes de que se le safe de la mano.")]
    [SerializeField] private float _maxStretchDistance = 0.45f;

    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;

    private Vector3 _posVelocity;
    private Vector3 _posOffset;

    private Vector3 _rotVelocity;
    private Vector3 _rotOffset;

    private GrabInteractable _grabInteractable;
    private bool _isBeingHeld;

    private void Awake()
    {
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;

        // Cacheamos dinámicamente la vista interactiva de Meta
        _grabInteractable = GetComponent<GrabInteractable>();
    }

    private void Update()
    {
        // Si no está agarrada, no perdemos ciclos de CPU midiendo distancias
        if (!_isBeingHeld || _grabInteractable == null) return;

        // Calculamos qué tanto se alejó la posición local actual del centro nativo
        float currentDistance = Vector3.Distance(transform.localPosition, _initialLocalPosition);

        // Si superó el límite elástico de la oficina, obligamos a Meta a soltarlo
        if (currentDistance > _maxStretchDistance)
        {
#if UNITY_EDITOR
            Debug.Log($"[SpringProceduralBone] Límite elástico roto ({currentDistance:F2}m). Forzando drop en {name}.");
#endif
            ForceMetaRelease();
        }
    }

    private void LateUpdate()
    {
        if (_isBeingHeld) return;

        Vector3 posSpringForce = -_stiffness * _posOffset;
        Vector3 posDampingForce = -_damping * _posVelocity;
        _posVelocity += (posSpringForce + posDampingForce) * Time.deltaTime;
        _posOffset += _posVelocity * Time.deltaTime;

        Vector3 rotSpringForce = -_stiffness * _rotOffset;
        Vector3 rotDampingForce = -_damping * _rotVelocity;
        _rotVelocity += (rotSpringForce + rotDampingForce) * Time.deltaTime;
        _rotOffset += _rotVelocity * Time.deltaTime;

        transform.localPosition = _initialLocalPosition + _posOffset;
        transform.localRotation = _initialLocalRotation * Quaternion.Euler(_rotOffset);
    }

    private void ForceMetaRelease()
    {
        // En el Interaction SDK de Meta, pasar el interactable por su máquina de estados 
        // a 'Disabled' o simular el deseleccionado cancela el tracking de la mano de forma segura
        if (_grabInteractable is GrabInteractable metaGrab)
        {
            metaGrab.Disable();
            metaGrab.Enable(); // Lo volvemos a prender inmediatamente para que quede listo para el próximo toque
        }

        // Ejecutamos la lógica normal de retorno elástico
        OnGrabRelease();
    }

    public void ApplyExternalForce(Vector3 torqueForce)
    {
        if (_isBeingHeld) return;
        _rotVelocity += torqueForce;
        _posVelocity += Vector3.back * (torqueForce.magnitude * 0.05f);
    }

    public void OnGrabStart()
    {
        _isBeingHeld = true;
        _posVelocity = Vector3.zero;
        _rotVelocity = Vector3.zero;
    }

    public void OnGrabRelease()
    {
        _isBeingHeld = false;

        _posOffset = transform.localPosition - _initialLocalPosition;
        _rotOffset = (Quaternion.Inverse(_initialLocalRotation) * transform.localRotation).eulerAngles;

        if (_rotOffset.x > 180) _rotOffset.x -= 360;
        if (_rotOffset.y > 180) _rotOffset.y -= 360;
        if (_rotOffset.z > 180) _rotOffset.z -= 360;
    }
}