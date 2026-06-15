using UnityEngine;

public class SpringProceduralBone : MonoBehaviour
{
    [Header("Spring Settings")]
    [SerializeField] private float _stiffness = 50f;   // Fuerza de retorno del resorte
    [SerializeField] private float _damping = 5f;       // Amortiguación (evita rebote eterno)
    [SerializeField] private Vector3 _angularLimits = new Vector3(45f, 45f, 45f);

    private Quaternion _localRestRotation;
    private Vector3 _currentAngularVelocity;
    private Vector3 _currentRotationOffset;

    private void Start()
    {
        // Guardamos la rotación original de la postura de la animación
        _localRestRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        // Ley de Hooke aplicada a rotaciones vectoriales (F = -kX - cV)
        Vector3 springForce = -_stiffness * _currentRotationOffset;
        Vector3 dampingForce = -_damping * _currentAngularVelocity;
        Vector3 totalForce = springForce + dampingForce;

        // Integración numérica semi-implícita de Euler
        _currentAngularVelocity += totalForce * Time.deltaTime;
        _currentRotationOffset += _currentAngularVelocity * Time.deltaTime;

        // Limitar la deformación cómica para que no se rompa la malla 3D por completo
        _currentRotationOffset.x = Mathf.Clamp(_currentRotationOffset.x, -_angularLimits.x, _angularLimits.x);
        _currentRotationOffset.y = Mathf.Clamp(_currentRotationOffset.y, -_angularLimits.y, _angularLimits.y);
        _currentRotationOffset.z = Mathf.Clamp(_currentRotationOffset.z, -_angularLimits.z, _angularLimits.z);

        // Aplicamos el desfase elástico sobre la postura actual del hueso
        transform.localRotation = _localRestRotation * Quaternion.Euler(_currentRotationOffset);
    }

    /// <summary>
    /// API Pública para empujar el hueso desde un golpe o agarre VR
    /// </summary>
    public void ApplyExternalForce(Vector3 force)
    {
        _currentAngularVelocity += force;
    }
}
