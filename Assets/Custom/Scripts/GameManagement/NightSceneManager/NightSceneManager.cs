using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class NightSceneManager : MonoBehaviour
{


    [SerializeField] private Animator fadeOutAnimator_1;
    [SerializeField] private Animator fadeOutAnimator_2;



    GameManager_NightCounter gm_NightCounterInstance;

    GameManager_SaveSystem gm_SaveSystemInstance;


    [Header("Settings Game Manager")]

    [SerializeField] private GameObject endOfNighScreen;




    private void Start()
    {

        gm_SaveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        gm_NightCounterInstance = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();

        gm_NightCounterInstance.NightSceneManagerCheckIn(this);


        endOfNighScreen.SetActive(false);



        Time.timeScale = 1f;

        GameManager_Singleton.Instance
            .GetComponent<GameManager_UI>()
            .UE_OnSwitchControlMode_Player?.Invoke();

        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;



        Debug.Log("Night Scene Loaded!");
    }



    private void SpawnCows(int amount)
    {
        GetComponent<NightSceneManager_SpawnCows>().SpawnCows(amount);
    }

    public void SetNightNumber(int nightNumber)
    {

        SpawnCows(gm_SaveSystemInstance.PlayerData_Curr.CowAmount);

    }


    public void OnAllWavesComplete()
    {

        Debug.Log("All Waves Complete!");


        //Check other shit

        StartCoroutine(WaitForTransition());


    }

    private IEnumerator WaitForTransition()
    {
        
        if (fadeOutAnimator_1 != null)
            fadeOutAnimator_1.SetTrigger("1_TransitionDown");
        if (fadeOutAnimator_2 != null)
            fadeOutAnimator_2.SetTrigger("2_TransitionUp");

        yield return new WaitForSeconds(5f);

        //EnableEndOfNightScreen();

        GameManager_Singleton.Instance.GetComponent<GameManager_SceneChangeEvents>().ChangeScene("Day Scene");
    }


    //private void EnableEndOfNightScreen()
    //{
    //    endOfNighScreen.SetActive(true);
    //    Time.timeScale = 0f;

    //    GameManager_Singleton.Instance
    //        .GetComponent<GameManager_UI>()
    //        .UE_OnSwitchControlMode_UI?.Invoke();

    //    Cursor.lockState = CursorLockMode.None;
    //    Cursor.visible = true;

    //    // Ensure EventSystem exists
    //    if (EventSystem.current == null)
    //    {
    //        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    //        DontDestroyOnLoad(es);
    //    }
    //}


}
