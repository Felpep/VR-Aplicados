using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ImpactNoiseEmitter : MonoBehaviour
{
    [SerializeField] private float _noiseVelocityThreshold = 1.5f; // Velocidad mínima para hacer ruido
    [SerializeField] private float _baseNoiseRadius = 8f; // Qué tan lejos se escucha el impacto
    private Rigidbody _rb;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    private void OnCollisionEnter(Collision collision)
    {
        // Calculamos la fuerza del impacto basada en la velocidad lineal relativa
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce > _noiseVelocityThreshold)
        {
            // El radio del ruido escala proporcionalmente con la fuerza del golpe físico
            float finalRadius = _baseNoiseRadius * (impactForce / 5f);

            // Dispara el evento global que tu EnemyPerception está escuchando
            EnemyPerception.EmitNoise(transform.position, finalRadius);
        }
    }
}
