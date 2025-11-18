using UnityEngine;

public class LoadGameButton_CheckNewGame : MonoBehaviour
{

    GameManager_SaveSystem saveSystemInstance;


    private void OnEnable()
    {
        //Debug.Log("OnEnable LoadButton");

        if (saveSystemInstance == null)
        {
            saveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();
        }
        if (saveSystemInstance != null)
        {

            if (saveSystemInstance.IsNewGame())
            {
                gameObject.SetActive(false);
            }

        }
    }
}
