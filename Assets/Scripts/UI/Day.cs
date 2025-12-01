using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Day : MonoBehaviour
{
    public static Day Instance { get; private set; }

    public GameObject morningScreen;
    public GameObject loadoutMenu;

    public string nightScene;

    public TextMeshProUGUI cowsLeftText;
    public TextMeshProUGUI moneyLeftText;
    public TextMeshProUGUI dayTitleText;

    private int nightNumber;
    private GameManager_SaveSystem saveSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (morningScreen) morningScreen.SetActive(true);
        if (loadoutMenu) loadoutMenu.SetActive(false);

        if (GameManager_Singleton.Instance == null) return;

        saveSystem = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();
        nightNumber = GameManager_Singleton.Instance
            .GetComponent<GameManager_NightCounter>()
            .GetNightNumberCurrent();

        UpdateMorningStats();
    }

    private void OnEnable()
    {
        if (morningScreen && morningScreen.activeSelf)
            UpdateMorningStats();
    }

    public void ContinueMorning()
    {
        if (morningScreen) morningScreen.SetActive(false);
        if (loadoutMenu) loadoutMenu.SetActive(true);
    }

    public void ContinueToNight()
    {
        if (GameManager_Singleton.Instance == null) return;

        var counter = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();
        var sceneChanger = GameManager_Singleton.Instance.GetComponent<GameManager_SceneChangeEvents>();

        if (counter != null) counter.AdvanceToNextNight();
        if (sceneChanger != null) sceneChanger.ChangeScene(nightScene);
    }

    public void OnPressedBackToDayMenu()
    {
        if (morningScreen) morningScreen.SetActive(true);
        if (loadoutMenu) loadoutMenu.SetActive(false);
    }

    public void OpenMorningScreen()
    {
        if (morningScreen) morningScreen.SetActive(true);
        if (loadoutMenu) loadoutMenu.SetActive(false);
        UpdateMorningStats();
    }

    private void UpdateMorningStats()
    {
        if (saveSystem == null || saveSystem.PlayerData_Curr == null)
            return;

        var data = saveSystem.PlayerData_Curr;

        if (cowsLeftText)
            cowsLeftText.text = $"Cows left in the farm : {data.CowAmount}";

        if (moneyLeftText)
            moneyLeftText.text = $"Money leftover : {data.MoneyAmount}";

        if (dayTitleText)
            dayTitleText.text = $"Day {nightNumber}";
    }
}
