using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Pool Settings")]
    [SerializeField] private int sfxPoolSize = 15;

    private AudioSource _musicSource;
    private AudioSource _ambientSource;
    private Queue<AudioSource> _sfxPool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSystem()
    {
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        _musicSource = musicObj.AddComponent<AudioSource>();
        _musicSource.outputAudioMixerGroup = musicGroup;
        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;

        GameObject ambientObj = new GameObject("AmbientSource");
        ambientObj.transform.SetParent(transform);
        _ambientSource = ambientObj.AddComponent<AudioSource>();
        _ambientSource.outputAudioMixerGroup = sfxGroup;
        _ambientSource.loop = true;
        _ambientSource.spatialBlend = 0f;

        _sfxPool = new Queue<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            CreateNewSFXSource();
        }
    }

    private AudioSource CreateNewSFXSource()
    {
        GameObject sfxObj = new GameObject($"SFXSource_{_sfxPool.Count}");
        sfxObj.transform.SetParent(transform);

        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxGroup;
        source.playOnAwake = false;

        _sfxPool.Enqueue(source);
        return source;
    }




    public void PlayAmbient(AudioClip clip, float volume = 1f)
    {
        if (_ambientSource.clip == clip) return;
        _ambientSource.clip = clip;
        _ambientSource.volume = volume;
        _ambientSource.Play();
    }

    public void StopAmbient()
    {
        _ambientSource.Stop();
        _ambientSource.clip = null;
    }




    public void PlaySFX2D(AudioClip clip, float volume = 1f, float pitchRandomness = 0.1f)
    {
        if (clip == null) return;
        PlaySFX(clip, Vector3.zero, 0f, volume, pitchRandomness);
    }

    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f, float pitchRandomness = 0.1f)
    {
        if (clip == null) return;
        PlaySFX(clip, position, 1f, volume, pitchRandomness);
    }

    private void PlaySFX(AudioClip clip, Vector3 position, float spatialBlend, float volume, float pitchRandomness)
    {
        AudioSource source = _sfxPool.Count > 0 ? _sfxPool.Dequeue() : CreateNewSFXSource();

        source.transform.position = position;
        source.spatialBlend = spatialBlend;
        source.clip = clip;
        source.volume = volume;
        source.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);

        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 2f;
        source.maxDistance = 25f;

        source.Play();

        StartCoroutine(ReturnToPoolRoutine(source, clip.length / source.pitch));
    }

    private IEnumerator ReturnToPoolRoutine(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        source.clip = null;
        _sfxPool.Enqueue(source);
    }





    public void PlayMusic(AudioClip clip)
    {
        if (_musicSource.clip == clip) return;
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }
}
