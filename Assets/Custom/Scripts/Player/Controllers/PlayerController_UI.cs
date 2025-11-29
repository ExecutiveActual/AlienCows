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


    public void OnEscape()
    {
        if (_input.currentActionMap.name == "Player")
        {
            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void SetActionMap_UI()
    {
        Debug.Log("Set ActionMap to UI");
        _input.SwitchCurrentActionMap("UI");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetActionMap_Player()
    {
        Debug.Log("Set ActionMap to Player");
        _input.SwitchCurrentActionMap("Player");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Start()
    {
        SetupGameManagerReferences();

        SetActionMap_Player();
    }

    private void SetupGameManagerReferences()
    {
        if (gameManager_UI == null)
        {

            //Debug.Log("Setting up GameManager_UI reference in PlayerController_UI.");

            gameManager_UI = GameManager_Singleton.Instance.GetComponent<GameManager_UI>();

            gameManager_UI.UE_OnSwitchControlMode_UI.AddListener(SetActionMap_UI);
            gameManager_UI.UE_OnSwitchControlMode_Player.AddListener(SetActionMap_Player);
        }
        else
        {
            //Debug.LogWarning("GameManager_UI reference already set in PlayerController_UI.");
        }
    }

}
