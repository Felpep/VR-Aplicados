using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))] // Mandatorio: debe ser IsKinematic = true
public class NPCReactionComponent : MonoBehaviour
{
    [Header("Audio Configurations")]
    [SerializeField] private AudioClip[] _annoyedVoices;
    [Range(0f, 1f)][SerializeField] private float _voiceVolume = 0.9f;

    [Header("Impact Thresholds")]
    [Tooltip("Fuerza mínima del golpe para que el NPC reaccione (evita que reaccione si le apoyas el objeto despacio)")]
    [SerializeField] private float _minimumImpactForce = 1.5f;
    [Tooltip("Multiplicador de fuerza para el impacto visual en el cuello")]
    [SerializeField] private float _springForceMultiplier = 4f;

    [Header("Procedural Bone Link")]
    [SerializeField] private SpringProceduralBone _neckSpring;

    private void Awake()
    {
        if (_neckSpring == null)
        {
            _neckSpring = GetComponent<SpringProceduralBone>();
        }
    }

    /// <summary>
    /// Captura impactos físicos reales de objetos con Rigidbody dinámicos del mundo.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Medimos la fuerza del impacto basado en la velocidad relativa del choque
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce >= _minimumImpactForce)
        {
            // Calculamos la dirección del golpe: desde el punto de impacto hacia el centro de la cabeza
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 forceDirection = (transform.position - contactPoint).normalized;

            // Convertimos la dirección global del mundo a la dirección local del hueso del NPC
            Vector3 localForceDirection = transform.InverseTransformDirection(forceDirection);

            // Disparar la reacción física aditiva y el audio
            TriggerReaction(localForceDirection, impactForce);
        }
    }

    /// <summary>
    /// Lógica central de reacción: inyecta fuerza direccional al resorte y reproduce audio.
    /// </summary>
    public void TriggerReaction(Vector3 localDirection, float intensity)
    {
        // 1. Emitir sonido tridimensional
        if (_annoyedVoices != null && _annoyedVoices.Length > 0)
        {
            AudioClip randomClip = _annoyedVoices[Random.Range(0, _annoyedVoices.Length)];
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX3D(randomClip, transform.position, _voiceVolume);
            }
        }

        // 2. Aplicamos la fuerza rotacional procedural al resorte del cuello
        if (_neckSpring != null)
        {
            // Usamos los ejes de torque cruzados para que la cabeza se incline hacia donde fue golpeada
            Vector3 torqueForce = new Vector3(localDirection.z, localDirection.x, -localDirection.y) * intensity * _springForceMultiplier;
            _neckSpring.ApplyExternalForce(torqueForce);
        }

#if UNITY_EDITOR
        Debug.Log($"[NPCReaction] {name} golpeado físicamente con intensidad: {intensity}.");
#endif
    }

    /// <summary>
    /// Mantenemos la función que usa tu Observer de Meta para cuando lo toques/agarres con la mano
    /// </summary>
    public void TriggerGrabReaction()
    {
        // Simula un impacto directo desde el frente
        TriggerReaction(Vector3.forward, 6.0f);
    }
}