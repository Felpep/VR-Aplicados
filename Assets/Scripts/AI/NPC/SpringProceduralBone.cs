using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Resorte procedimental avanzado que calcula la tensión rotacional hacia un 
/// target interactuable (movido por el jugador), simulando resistencia elástica real.
/// </summary>
public class SpringProceduralBone : MonoBehaviour
{
    [Header("Spring Physics")]
    [SerializeField] private float _stiffness = 180f;   // Sube esto si quieres que vuelva más rápido y fuerte
    [SerializeField] private float _damping = 12f;      // Amortiguación para frenar el rebote eterno
    [SerializeField] private Vector3 _angularLimits = new Vector3(45f, 45f, 45f);

    private IInteractableView _grabInteractable;
    private Quaternion _restLocalRotation;
    private Vector3 _currentAngularVelocity;
    private Vector3 _currentRotationOffset;

    private void Awake()
    {
        // Guardamos la pose inicial real de la animación
        _restLocalRotation = transform.localRotation;
        _grabInteractable = GetComponent<IInteractableView>();
    }

    private void LateUpdate()
    {
        // Si el jugador está sosteniendo la cabeza, acumulamos una fuerza elástica que jala en sentido opuesto
        if (_grabInteractable != null && _grabInteractable.State == InteractableState.Select)
        {
            // Mientras tu mano intenta rotar el objeto, el resorte inyecta una velocidad angular
            // que simula que la cabeza opone resistencia hacia su centro neutro
            Vector3 pullingForce = -_stiffness * _currentRotationOffset;
            _currentAngularVelocity += pullingForce * Time.deltaTime;
        }

        // Ley de Hooke convencional para el retorno continuo
        Vector3 springForce = -_stiffness * _currentRotationOffset;
        Vector3 dampingForce = -_damping * _currentAngularVelocity;
        Vector3 totalForce = springForce + dampingForce;

        _currentAngularVelocity += totalForce * Time.deltaTime;
        _currentRotationOffset += _currentAngularVelocity * Time.deltaTime;

        // Clampeo estricto de seguridad
        _currentRotationOffset.x = Mathf.Clamp(_currentRotationOffset.x, -_angularLimits.x, _angularLimits.x);
        _currentRotationOffset.y = Mathf.Clamp(_currentRotationOffset.y, -_angularLimits.y, _angularLimits.y);
        _currentRotationOffset.z = Mathf.Clamp(_currentRotationOffset.z, -_angularLimits.z, _angularLimits.z);

        // Si se suelta, aplicamos sobre la pose del Animator, si se sostiene genera el force-feedback visual
        transform.localRotation = transform.localRotation * Quaternion.Euler(_currentRotationOffset);
    }

    /// <summary>
    /// API Pública existente para impactos o explosiones externas
    /// </summary>
    public void ApplyExternalForce(Vector3 force)
    {
        _currentAngularVelocity += force;
    }

    public void Debug_ApplyExternalForce()
    {
        ApplyExternalForce(new Vector3(30, 30, 30));
    }
}