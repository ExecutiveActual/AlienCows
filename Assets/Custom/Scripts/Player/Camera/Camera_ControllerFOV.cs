using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineBrain))]
public class Camera_ControllerFOV : MonoBehaviour
{
    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField, Range(0.01f, 179f)] private float minFOV = 20f;
    [SerializeField, Range(0.01f, 179f)] private float maxFOV = 100f;

    private CinemachineBrain brain;
    private CinemachineCamera activeVCam;

    private float defaultFOV;
    private float currentFOV;
    private float targetFOV;
    private float velocity;

    private void Awake()
    {
        brain = GetComponent<CinemachineBrain>();
    }

    private void Start()
    {
        // Grab the first active camera at start and remember its FOV as default
        activeVCam = brain.ActiveVirtualCamera as CinemachineCamera;
        if (activeVCam != null)
        {
            defaultFOV = activeVCam.Lens.FieldOfView;
            currentFOV = targetFOV = defaultFOV;
        }
    }

    private void LateUpdate()
    {
        // Always work with whatever camera is currently live
        var currentActive = brain.ActiveVirtualCamera as CinemachineCamera;
        if (currentActive == null) return;

        // Detect camera switch and update default if needed (optional � remove if you never want it to change)
        if (currentActive != activeVCam)
        {
            activeVCam = currentActive;
            defaultFOV = currentFOV = targetFOV = activeVCam.Lens.FieldOfView;
            velocity = 0f;
        }

        if (Mathf.Approximately(currentFOV, targetFOV)) return;

        currentFOV = Mathf.SmoothDamp(currentFOV, targetFOV, ref velocity, smoothTime);
        var lens = activeVCam.Lens;
        lens.FieldOfView = currentFOV;
        activeVCam.Lens = lens;
    }

    // ��� YOUR BRAIN-FREE PUBLIC API ���

    public void SetFOV(float newFOV, bool instant = false)
    {
        targetFOV = Mathf.Clamp(newFOV, minFOV, maxFOV);

        if (instant)
        {
            currentFOV = targetFOV;
            ApplyFOV();
            velocity = 0f;
        }
    }

    public void ResetFOV() => SetFOV(defaultFOV);
    public void ResetFOV_Instant() => SetFOV(defaultFOV, true);

    // Bonus lerp version
    public void SetFOV_Lerp(float newFOV, float speed = 8f)
    {
        targetFOV = Mathf.Clamp(newFOV, minFOV, maxFOV);
        StopAllCoroutines();
        StartCoroutine(LerpFOV(speed));
    }

    private System.Collections.IEnumerator LerpFOV(float speed)
    {
        while (!Mathf.Approximately(currentFOV, targetFOV))
        {
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * speed);
            ApplyFOV();
            yield return null;
        }
        currentFOV = targetFOV;
    }

    private void ApplyFOV()
    {
        if (activeVCam != null)
        {
            var lens = activeVCam.Lens;
            lens.FieldOfView = currentFOV;
            activeVCam.Lens = lens;
        }
    }

    // Quick access
    public float CurrentFOV => currentFOV;
    public float DefaultFOV => defaultFOV;
}