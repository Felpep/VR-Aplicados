using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ModularAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundClip;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Seguridad por si acaso
    }

    // Este es el método que dispararemos desde el Inspector
    public void PlaySound()
    {
        if (soundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundClip);
        }
    }
}