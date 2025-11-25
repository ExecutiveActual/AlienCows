using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager_SceneChangeEvents : MonoBehaviour
{

    public UnityEvent<string> UE_OnChangeScene;


    public void ChangeScene(string sceneName)
    {
        UE_OnChangeScene.Invoke(sceneName);

        SceneManager.LoadScene(sceneName);
    }



}



/// <summary>
/// DO NOT USE!
/// </summary>
public enum SceneEnum
{
    MainMenu,
    Day,
    DayShop,
    CowShop,
    Night
}