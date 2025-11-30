using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class Day : MonoBehaviour
{
    public static Day Instance { get; private set; }

    [Header("Menu References")]
    public GameObject morningScreen;
    public GameObject loadoutMenu;

    [Header("Scene Names (Assign in Inspector)")]
    public string nightScene; 

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public float hoverVolume = 0.7f;
    public AudioClip clickSound;
    public float clickVolume = 1f;

    [Header("Morning Screen UI Texts")]
    public TextMeshProUGUI cowsLeftText;
    public TextMeshProUGUI moneyLeftText;

    private GameManager_SaveSystem saveSystem;

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
        Cursor.visible = true;

        // Default startup states
        morningScreen.SetActive(true);
        loadoutMenu.SetActive(false);

        
        saveSystem = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        // Initialize morning stats UI
        UpdateMorningStats();
    }

    private void OnEnable()
    {
        if (morningScreen.activeSelf)
            UpdateMorningStats();
    }


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

    public void ContinueMorning()
    {
        PlayClick();
        morningScreen.SetActive(false);
        loadoutMenu.SetActive(true);
    }

    public void EnterNight()
    {
        PlayClick();
        morningScreen.SetActive(false);
        loadoutMenu.SetActive(true);
    }

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

    public void OnPressedBackToDayMenu()
    {
        PlayClick();
        morningScreen.SetActive(true);
        loadoutMenu.SetActive(false);
    }

    public void OpenMorningScreen()
    {
        PlayClick();
        morningScreen.SetActive(true);
        loadoutMenu.SetActive(false);
        UpdateMorningStats();
    }

 
    private void UpdateMorningStats()
    {
        if (saveSystem == null || saveSystem.PlayerData_Curr == null)
        {
            Debug.LogWarning("Day.cs: SaveSystem or PlayerData_Curr missing!");
            return;
        }

        var data = saveSystem.PlayerData_Curr;

        int cowsLeft = data.CowAmount;
        int moneyLeft = data.MoneyAmount;

        if (cowsLeftText != null)
            cowsLeftText.text = $"Cows left in the farm : {cowsLeft}";

        if (moneyLeftText != null)
            moneyLeftText.text = $"Money leftover : {moneyLeft}";
    }
}
