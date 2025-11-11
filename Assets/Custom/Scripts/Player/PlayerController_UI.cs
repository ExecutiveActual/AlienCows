using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_UI : MonoBehaviour
{

    private PlayerInput _input;


    GameManager_UI gameManager_UI;



    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }


    private void OnEscape()
    {

        //if (_input.currentActionMap == _input.actions.FindActionMap("Player"))
        //{
        //    Debug.Log("Set ActionMap to UI");
        //    _input.SwitchCurrentActionMap("UI");
        //}
        //else if(_input.currentActionMap == _input.actions.FindActionMap("UI"))
        //{
        //    Debug.Log("Set ActionMap to Player");
        //    _input.SwitchCurrentActionMap("Player");
        //}

    }


    public void SetActionMap_UI()
    {
        Debug.Log("Set ActionMap to UI");
        _input.SwitchCurrentActionMap("UI");
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetActionMap_Player()
    {
        Debug.Log("Set ActionMap to Player");
        _input.SwitchCurrentActionMap("Player");
    }


    private void Start()
    {
        SetupGameManagerReferences();
    }

    private void SetupGameManagerReferences()
    {
        if (gameManager_UI == null)
        {

            //Debug.Log("Setting up GameManager_UI reference in PlayerController_UI.");

            gameManager_UI = GameManager_Singleton.Instance.GetComponent<GameManager_UI>();

            gameManager_UI.UE_OnNightEnd.AddListener(SetActionMap_UI);
            gameManager_UI.UE_OnReturnControlToPlayer.AddListener(SetActionMap_Player);
        }
        else
        {
            //Debug.LogWarning("GameManager_UI reference already set in PlayerController_UI.");
        }
    }

}
