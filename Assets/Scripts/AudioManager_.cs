using System.Collections;
using UnityEngine;


public class AudioManager_ : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("Main background music (ambient loop).")]
    public AudioClip backgroundLoop;

    [Tooltip("Transition music played when first UFO appears.")]
    public AudioClip transitionClip;

    [Tooltip("Chase/anger loop played after transition.")]
    public AudioClip angerLoop;

    [Header("Settings")]
    [Tooltip("Fade duration between background ↔ transition states.")]
    public float fadeDuration = 2f;

    [Tooltip("Volume level for all tracks.")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    private AudioSource ambientSource;
    private AudioSource actionSource;

    private bool ufoWaveActive = false;
    private bool transitionPlayed = false;

    private UFOSpawner_ spawner;

    void Awake()
    {
        // Setup two sources
        ambientSource = gameObject.AddComponent<AudioSource>();
        actionSource  = gameObject.AddComponent<AudioSource>();

        ambientSource.loop = true;
        actionSource.loop  = false;

        ambientSource.playOnAwake = false;
        actionSource.playOnAwake  = false;

        ambientSource.volume = 0f;
        actionSource.volume  = 0f;
    }

    void Start()
    {
        spawner = FindObjectOfType<UFOSpawner_>();

        // Subscribe to UFO events
        if (spawner)
        {
            spawner.OnUfoSpawned += HandleUfoSpawned;
            spawner.OnAllUfosDestroyed += HandleAllUfosDestroyed;
        }

        // Start background music with fade-in
        if (backgroundLoop)
        {
            ambientSource.clip = backgroundLoop;
            ambientSource.loop = true;
            ambientSource.Play();
            StartCoroutine(FadeAudio(ambientSource, 0f, masterVolume, fadeDuration));
        }
    }

    

    private void HandleUfoSpawned()
    {
        if (!ufoWaveActive)
        {
            ufoWaveActive = true;
            StartCoroutine(PlayTransitionAndChase());
        }
    }

    private void HandleAllUfosDestroyed()
    {
        if (!ufoWaveActive) return;

        ufoWaveActive = false;
        transitionPlayed = false;
        StartCoroutine(ReturnToBackground());
    }

    
    private IEnumerator PlayTransitionAndChase()
    {
        // Fade out and stop background completely
        yield return StartCoroutine(FadeAudio(ambientSource, ambientSource.volume, 0f, fadeDuration));
        ambientSource.Stop(); // ensure it fully stops

        // Play transition once
        if (transitionClip)
        {
            actionSource.clip = transitionClip;
            actionSource.loop = false;
            actionSource.volume = masterVolume;
            actionSource.Play();

            yield return new WaitForSeconds(transitionClip.length);
        }

        // Play chase/anger loop
        if (angerLoop)
        {
            actionSource.clip = angerLoop;
            actionSource.loop = true;
            actionSource.volume = masterVolume;
            actionSource.Play();
        }
    }

    private IEnumerator ReturnToBackground()
    {
        // Fade out current chase loop and stop it completely
        yield return StartCoroutine(FadeAudio(actionSource, actionSource.volume, 0f, fadeDuration));
        actionSource.Stop();

        // Restart background loop with fade-in
        if (backgroundLoop)
        {
            ambientSource.clip = backgroundLoop;
            ambientSource.loop = true;
            ambientSource.Play();
            yield return StartCoroutine(FadeAudio(ambientSource, 0f, masterVolume, fadeDuration));
        }
    }

    private IEnumerator FadeAudio(AudioSource src, float from, float to, float duration)
    {
        float t = 0f;
        src.volume = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        src.volume = to;
    }

    void OnDestroy()
    {
        if (spawner)
        {
            spawner.OnUfoSpawned -= HandleUfoSpawned;
            spawner.OnAllUfosDestroyed -= HandleAllUfosDestroyed;
        }
    }
}
