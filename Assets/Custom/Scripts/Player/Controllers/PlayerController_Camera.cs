using Unity.Cinemachine;
using UnityEngine;

public class PlayerController_Camera : MonoBehaviour
{


    [SerializeField] private CinemachineCamera cinemachineCamera;

    private float defaultFOV = 90f;


    private void Awake()
    {

    }


    public void SetCameraFOV(float newFOV)
    {
        cinemachineCamera.Lens.FieldOfView = newFOV;
    }

    public void ResetCameraFOV()
    {
        cinemachineCamera.Lens.FieldOfView = defaultFOV;
    }




}
