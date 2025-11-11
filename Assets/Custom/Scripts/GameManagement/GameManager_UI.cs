using UnityEngine;
using UnityEngine.Events;

public class GameManager_UI : MonoBehaviour, IGameManagerModule
{

    public UnityEvent UE_OnNightEnd;

    public UnityEvent UE_OnReturnControlToPlayer;

    public void OnInitializeModule()
    {
        //throw new System.NotImplementedException();
    }
}
