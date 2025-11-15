using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UFOAbductionEvent : UnityEvent<UFO_Controller> { }

[RequireComponent(typeof(AudioSource))]
public class UFO_Abduction : MonoBehaviour
{
    [Header("Beam / Visuals")]
    public Light beamSpot;
    public GameObject beamVisual;

    [Header("Abduction Settings")]
    public float abductionTime = 10f;
    public float liftHeight = 12f;
    public bool rotateDuringAbduction = true;
    public float rotateSpeed = 120f;

    [Header("Interrupted Behavior")]
    public float dropForce = 3f;
    public float restartDelayAfterInterrupt = 2f;

    [Header("Audio")]
    public AudioClip abductionLoop;
    public float abductionVolume = 1f;

    [Header("Events")]
    public UFOAbductionEvent OnCowAbducted;
    public UFOAbductionEvent OnAbductionInterrupted;

    // internals
    private AudioSource src;
    private Coroutine routine;
    private Transform cow;
    private UFO_Controller controller;
    private bool wasInterrupted = false;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        if (beamSpot) beamSpot.enabled = false;
        if (beamVisual) beamVisual.SetActive(false);
    }

    public void StartAbduction(Transform cowTarget, UFO_Controller parent)
    {
        cow = cowTarget;
        controller = parent;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Routine());
    }

    public void InterruptAbduction()
    {
        if (routine != null)
        {
            wasInterrupted = true;
            StopCoroutine(routine);
            StartCoroutine(DropCow());
        }
    }

    IEnumerator Routine()
    {
        // enable beam
        if (beamSpot) beamSpot.enabled = true;
        if (beamVisual) beamVisual.SetActive(true);

        if (src && abductionLoop)
        {
            src.clip = abductionLoop;
            src.loop = true;
            src.volume = abductionVolume;
            src.Play();
        }

        Rigidbody rb = cow.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        Vector3 start = cow.position;
        Vector3 end = new Vector3(transform.position.x, transform.position.y + liftHeight, transform.position.z);

        float t = 0f;
        while (t < abductionTime)
        {
            if (wasInterrupted) yield break;

            t += Time.deltaTime;
            if (cow) cow.position = Vector3.Lerp(start, end, t / abductionTime);

            if (rotateDuringAbduction)
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

            yield return null;
        }

        if (!wasInterrupted && cow)
        {
            Destroy(cow.gameObject);
            OnCowAbducted?.Invoke(controller);
        }

        CleanupBeam();
    }

    IEnumerator DropCow()
    {
        if (cow)
        {
            Rigidbody rb = cow.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.down * dropForce, ForceMode.Impulse);
            }
        }

        CleanupBeam();
        OnAbductionInterrupted?.Invoke(controller);

        yield return new WaitForSeconds(restartDelayAfterInterrupt);
        wasInterrupted = false;
    }


    void CleanupBeam()
    {
        if (src) src.Stop();
        if (beamSpot) beamSpot.enabled = false;
        if (beamVisual) beamVisual.SetActive(false);
        cow = null;
    }
}
