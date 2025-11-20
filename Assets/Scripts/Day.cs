using UnityEngine;
using UnityEngine.SceneManagement;

public class Day : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject morningScreen;
    public GameObject dayMenu;
    public GameObject loadoutMenu;
    public GameObject checkCowsMenu;

    [Header("Scene Names (Assign in Inspector)")]
    public string dayShopScene;     // Visit Shop → Day_Shop
    public string cowShopScene;     // Buy Cows → CowShop
    public string nightScene;       // Continue To Night
                                    // (Check Cows uses a GameObject, not a scene)

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public float hoverVolume = 0.7f;

    public AudioClip clickSound;
    public float clickVolume = 1f;


    private void Start()
    {
        // Default startup states
        morningScreen.SetActive(true);
        dayMenu.SetActive(false);
        loadoutMenu.SetActive(false);
        checkCowsMenu.SetActive(false);
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
        checkCowsMenu.SetActive(false);
    }

    // DayMenu → LoadoutMenu
    public void EnterNight()
    {
        PlayClick();

        morningScreen.SetActive(false);
        dayMenu.SetActive(false);
        loadoutMenu.SetActive(true);
        checkCowsMenu.SetActive(false);
    }


    // ----------------------------------------------------------
    // DAY MENU BUTTONS
    // ----------------------------------------------------------

    // Visit Shop → Load Day_Shop scene
    public void OpenDayShop()
    {
        PlayClick();
        SceneManager.LoadScene(dayShopScene);
    }

    // Buy Cows → Load CowShop scene
    public void OpenCowShop()
    {
        PlayClick();
        SceneManager.LoadScene(cowShopScene);
    }

    // Check Cows → Open GameObject
    public void OpenCheckCows()
    {
        PlayClick();

        morningScreen.SetActive(false);
        dayMenu.SetActive(false);
        loadoutMenu.SetActive(false);
        checkCowsMenu.SetActive(true);
    }

    // Continue To Night Scene
    public void ContinueToNight()
    {
        PlayClick();
        SceneManager.LoadScene(nightScene);
    }


    // ----------------------------------------------------------
    // BACK NAVIGATION
    // ----------------------------------------------------------

    // From CheckCows → DayMenu
    public void OnPressedBackToDayMenu()
    {
        PlayClick();

        morningScreen.SetActive(false);
        dayMenu.SetActive(true);
        loadoutMenu.SetActive(false);
        checkCowsMenu.SetActive(false);
    }

    // From anywhere → MorningScreen (your new request)
    public void OpenMorningScreen()
    {
        PlayClick();

        morningScreen.SetActive(true);
        dayMenu.SetActive(false);
        loadoutMenu.SetActive(false);
        checkCowsMenu.SetActive(false);
    }
}
