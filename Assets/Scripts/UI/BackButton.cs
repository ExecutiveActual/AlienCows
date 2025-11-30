using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class BackButton : MonoBehaviour, IPointerEnterHandler
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load when the button is clicked.")]
    public string sceneToLoad;

    [Header("Audio Sources")]
    [Tooltip("AudioSource for sound effects.")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    [Tooltip("Sound played when hovering over the button.")]
    public AudioClip hoverSound;
    [Tooltip("Sound played when clicking the button.")]
    public AudioClip clickSound;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float hoverVolume = 1f;
    [Range(0f, 1f)] public float clickVolume = 1f;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHoverSound();
    }

    void OnButtonClicked()
    {
        PlayClickSound();

        // small delay to allow click sound to play before scene changes
        if (clickSound)
            Invoke(nameof(LoadTargetScene), 0.15f);
        else
            LoadTargetScene();
    }

    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("BackButton: No scene name assigned!");
        }
    }

    void PlayHoverSound()
    {
        if (sfxSource && hoverSound)
            sfxSource.PlayOneShot(hoverSound, hoverVolume);
    }

    void PlayClickSound()
    {
        if (sfxSource && clickSound)
            sfxSource.PlayOneShot(clickSound, clickVolume);
    }
}
