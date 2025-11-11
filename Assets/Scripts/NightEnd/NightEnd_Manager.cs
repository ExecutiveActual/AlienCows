using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NightEnd_Manager : MonoBehaviour
{

    WaveEventManager waveEventManager;


    [SerializeField] private float timeTo_NightEndScreen = 3.3f;

    Coroutine coroutine_ShowNightEndScreen;


    public UnityEvent UE_OnNightEndScreenFadeIn;


    private void Start()
    {
        waveEventManager = WaveEventManager.Instance;
        if (waveEventManager != null)
        {
            waveEventManager.OnWaveEnd.AddListener(HandleNightEnd);
        }


    }

    private void HandleNightEnd()
    {

        Debug.Log("<color=green>Night End Triggered!</color>");

        if (coroutine_ShowNightEndScreen != null)
        {
            //StopCoroutine(coroutine_ShowNightEndScreen);

            Debug.LogError("<color=red>Coroutine for Night End Screen already running!</color>");
        }
        coroutine_ShowNightEndScreen = StartCoroutine(ShowNightEndScreen());
    }

    private IEnumerator ShowNightEndScreen()
    {

        yield return new WaitForSeconds(timeTo_NightEndScreen);

        GameManager_Singleton.Instance.GetComponent<GameManager_UI>().UE_OnSwitchControlMode_UI?.Invoke();

        Debug.Log("<color=green>Invoking Night End Screen Fade-In Event!</color>");

        UE_OnNightEndScreenFadeIn?.Invoke();

    }

}
