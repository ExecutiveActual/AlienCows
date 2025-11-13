using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_RandomSound_New", menuName = "Scriptable Objects/SO_RandomSound")]
public class SO_RandomSound : ScriptableObject
{

    
    [Header("Sound")]
    [Tooltip("Sound to play when timer finishes")]
    public List<AudioClip> soundClip;

    public AudioClip Value
    {
        get
        {
            if (soundClip == null || soundClip.Count == 0)
            {
                Debug.LogWarning("No AudioClip assigned to SO_RandomSound!");
                return null;
            }
            int randomIndex = Random.Range(0, soundClip.Count);
            return soundClip[randomIndex];
        }
    }

}
