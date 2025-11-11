using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_UI : MonoBehaviour
{

    private PlayerInput _input;




    private void Awake()
    {
        _input = GetComponent<PlayerInput>();
    }


    private void OnEscape()
    {

        if (_input.currentActionMap == _input.actions.FindActionMap("Player"))
        {
            Debug.Log("Peenit");
            _input.SwitchCurrentActionMap("UI");
        }
        else if(_input.currentActionMap == _input.actions.FindActionMap("Player"))
        {
            Debug.Log("Peenit");
            _input.SwitchCurrentActionMap("UI");
        }

    }


    public void SetActionMap_UI()
    {

    }


}
