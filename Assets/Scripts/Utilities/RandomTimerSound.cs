using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomTimerSound : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Minimum wait time in seconds")]
    [Min(0.1f)] public float minInterval = 5f;

    [Tooltip("Maximum wait time in seconds")]
    [Min(0.1f)] public float maxInterval = 10f;

    [Header("Sound")]
    [Tooltip("Sound to play when timer finishes")]
    public SO_RandomSound sound;

    private AudioSource audioSource;


    private bool canPlay = true;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (sound != null)
            audioSource.clip = sound.Value;
    }

    private void Start()
    {
        StartRandomTimer();
    }

    private void StartRandomTimer()
    {
        float randomDelay = Random.Range(minInterval, maxInterval);
        Invoke(nameof(PlaySoundAndRestart), randomDelay);
    }

    private void PlaySoundAndRestart()
    {
        PlaySound();
        StartRandomTimer(); // Loop forever
    }

    private void PlaySound()
    {
        if (sound == null)
        {
            Debug.LogWarning("No AudioClip assigned to RandomTimerSound!");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioSource missing!");
            return;
        }


        if (canPlay) audioSource.PlayOneShot(sound.Value);
    }


    public void MuteSound()
    {
        canPlay = false;
    }

    public void UnmuteSound()
    {
        canPlay = true;
    }



    // Optional: Visualize in editor
    private void OnValidate()
    {
        if (maxInterval < minInterval)
            maxInterval = minInterval;
    }
}