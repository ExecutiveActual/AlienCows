using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;
    public Sound[] musicSounds,sfxSounds;
    public AudioSource musicSource,sfxSource;

    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic("BackGroundMusic");
    }

    public void PlayMusic(string name)
    {
        Sound S = Array.Find(musicSounds, X => X.name == name);
        if(S== null)
        {
            Debug.Log("not found the sound fuck!!!!!");
        }

        else
        {
            musicSource.clip = S.clip;
            musicSource.Play();
;
       }
    }


    public void PlaySFX(string name)
    {
        Sound S = Array.Find(sfxSounds, X => X.name == name);
        if(S== null)
        {
            Debug.Log("not found the sound fuck!!!!!");
        }

        else
        {
            sfxSource.clip = S.clip;
            sfxSource.Play();
;
       }
    }
}
