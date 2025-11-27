using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Day : MonoBehaviour
{

    public static Day Instance { get; private set; }


    [Header("Menu References")]
    public GameObject morningScreen;
    public GameObject dayMenu;
    public GameObject loadoutMenu;

    [Header("Scene Names (Assign in Inspector)")]
    public string nightScene; // Continue To Night

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public float hoverVolume = 0.7f;

    public AudioClip clickSound;
    public float clickVolume = 1f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        // Default startup states
        morningScreen.SetActive(true);
        dayMenu.SetActive(false);
        loadoutMenu.SetActive(false);

    }

    // ----------------------------------------------------------
    // SOUND EVENTS
    // ----------------------------------------------------------

    public void PlayHover()
    {
        if (hoverSound)
            sfxSource.PlayOneShot(hoverSound, hoverVolume);
    }

    public void PlayClick()
    {
        if (clickSound)
            sfxSource.PlayOneShot(clickSound, clickVolume);
    }

    // ----------------------------------------------------------
    // MENU FLOW
    // ----------------------------------------------------------

    // MorningScreen → DayMenu
    public void ContinueMorning()
    {
        PlayClick();

        morningScreen.SetActive(false);
        dayMenu.SetActive(true);
        loadoutMenu.SetActive(false);
    }

    // DayMenu → LoadoutMenu
    public void EnterNight()
    {
        PlayClick();

        morningScreen.SetActive(false);
        dayMenu.SetActive(false);
        loadoutMenu.SetActive(true);
    }

    // Continue To Night Scene
    public void ContinueToNight()
    {
        PlayClick();

        GameManager_Singleton.Instance
            .GetComponent<GameManager_NightCounter>()
            .AdvanceToNextNight();

        GameManager_Singleton.Instance
            .GetComponent<GameManager_SceneChangeEvents>()
            .ChangeScene(nightScene);
    }

    // ----------------------------------------------------------
    // BACK NAVIGATION
    // ----------------------------------------------------------

    // From Loadout → DayMenu
    public void OnPressedBackToDayMenu()
    {
        PlayClick();

        morningScreen.SetActive(false);
        dayMenu.SetActive(true);
        loadoutMenu.SetActive(false);
    }

    // From anywhere → MorningScreen
    public void OpenMorningScreen()
    {
        PlayClick();

        morningScreen.SetActive(true);
        dayMenu.SetActive(false);
        loadoutMenu.SetActive(false);
    }
}
