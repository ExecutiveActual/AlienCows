using UnityEngine;

public class AudioBGHandler : MonoBehaviour
{
    public string MusicName;
    void Start()
    {
        AudioManager.Instance.PlayMusic(MusicName);
    }

    
}
