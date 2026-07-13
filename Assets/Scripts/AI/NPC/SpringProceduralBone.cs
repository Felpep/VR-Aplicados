using UnityEngine;
using Oculus.Interaction;

public class SpringProceduralBone : MonoBehaviour
{
    [Header("Spring Physics")]
    [SerializeField] private float _stiffness = 150f;
    [SerializeField] private float _damping = 10f;

    [Header("VR Limits (Escape de Agarre)")]
    [SerializeField] private float _maxStretchDistance = 0.45f;

    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;

    private Vector3 _posVelocity;
    private Vector3 _posOffset;

    private Vector3 _rotVelocity;
    private Vector3 _rotOffset;

    private GrabInteractable _grabInteractable;
    private bool _isBeingHeld;

    // OPTIMIZACIÓN CORE: Umbral al cuadrado precargado en RAM para evitar raíces cuadradas (Sqrt)
    private float _maxStretchDistanceSqr;

    private void Awake()
    {
        _initialLocalPosition = transform.localPosition;
        _initialLocalRotation = transform.localRotation;
        _grabInteractable = GetComponent<GrabInteractable>();

        _maxStretchDistanceSqr = _maxStretchDistance * _maxStretchDistance;
    }

    private void Update()
    {
        if (!_isBeingHeld || _grabInteractable == null) return;

        // OPTIMIZACIÓN MATEMÁTICA: Resta plana de vectores midiendo la magnitud al cuadrado
        Vector3 offset = transform.localPosition - _initialLocalPosition;

        if (offset.sqrMagnitude > _maxStretchDistanceSqr)
        {
            ForceMetaRelease();
        }
    }

    private void LateUpdate()
    {
        if (_isBeingHeld) return;

        float dt = Time.deltaTime;

        Vector3 posSpringForce = -_stiffness * _posOffset;
        Vector3 posDampingForce = -_damping * _posVelocity;
        _posVelocity += (posSpringForce + posDampingForce) * dt;
        _posOffset += _posVelocity * dt;

        Vector3 rotSpringForce = -_stiffness * _rotOffset;
        Vector3 rotDampingForce = -_damping * _rotVelocity;
        _rotVelocity += (rotSpringForce + rotDampingForce) * dt;
        _rotOffset += _rotVelocity * dt;

        transform.localPosition = _initialLocalPosition + _posOffset;
        transform.localRotation = _initialLocalRotation * Quaternion.Euler(_rotOffset);
    }

    private void ForceMetaRelease()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.Disable();
            _grabInteractable.Enable();
        }
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

        // Optimización de rotación: Evitamos instanciaciones de vectores dinámicos redundantes
        Vector3 rawAngles = (Quaternion.Inverse(_initialLocalRotation) * transform.localRotation).eulerAngles;
        _rotOffset.x = rawAngles.x > 180 ? rawAngles.x - 360 : rawAngles.x;
        _rotOffset.y = rawAngles.y > 180 ? rawAngles.y - 360 : rawAngles.y;
        _rotOffset.z = rawAngles.z > 180 ? rawAngles.z - 360 : rawAngles.z;
    }
}