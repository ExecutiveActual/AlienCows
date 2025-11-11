using UnityEngine;
using UnityEngine.Events;

public class GameManager_UI : MonoBehaviour, IGameManagerModule
{

    public UnityEvent UE_OnSwitchControl_UI;

    public UnityEvent UE_OnSwitchControl_Player;


    public void OnInitializeModule()
    {
        //throw new System.NotImplementedException();
    }

}
