using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCReactionComponent : MonoBehaviour
{
    [Header("Audio Configurations")]
    [SerializeField] private AudioClip[] _annoyedVoices;
    [Range(0f, 1f)][SerializeField] private float _voiceVolume = 0.9f;

    [Header("Impact Thresholds")]
    [SerializeField] private float _minimumImpactForce = 2.0f;

    [Header("Procedural Bone Link")]
    [SerializeField] private SpringProceduralBone _neckSpring;

    private void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce >= _minimumImpactForce)
        {
            TriggerReaction(collision.contacts[0].point, impactForce);
        }
    }

    public void TriggerReaction(Vector3 worldImpactPoint, float intensity)
    {
        // 1. Emitir sonido tridimensional usando tu AudioManager en Pool
        if (_annoyedVoices != null && _annoyedVoices.Length > 0)
        {
            AudioClip randomClip = _annoyedVoices[Random.Range(0, _annoyedVoices.Length)];
            AudioManager.Instance.PlaySFX3D(randomClip, transform.position, _voiceVolume);
        }

        // 2. Aplicar fuerza matemática al resorte procedimental del cuello
        if (_neckSpring != null)
        {
            Vector3 localForceDirection = transform.InverseTransformDirection(Vector3.down);
            _neckSpring.ApplyExternalForce(localForceDirection * intensity * 0.1f);
        }

#if UNITY_EDITOR
        Debug.Log($"[NPCReaction] {name} molestado con intensidad: {intensity}.");
#endif
    }
}
