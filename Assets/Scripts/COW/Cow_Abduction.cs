using System.Collections;
using UnityEngine;

public class Cow_Abduction : MonoBehaviour
{

    private bool isBeingAbducted = false;
    private bool isAirborne = false;


    [SerializeField] private float fallSpeed = 5f;

    [SerializeField] private float abductBarksInterval = 1f;

    [SerializeField] SO_RandomSound cowAbductBarks;


    AudioSource audioSource;



    private Vector3 target_Landing;


    Coroutine audioCoroutine;



    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {

        if (isAirborne && !isBeingAbducted)
        {
            LandCow();
        }
        else if (isAirborne && isBeingAbducted)
        {
            // ABDUCTION FX FOR COW (ANIMS, BARK)
        }
    }

    private void LandCow()
    {

        //Debug.Log("Landing");

        transform.position = Vector3.MoveTowards(transform.position, target_Landing, fallSpeed * Time.deltaTime); // Lerp Instead
        //transform.Translate(fallSpeed * Time.deltaTime * Vector3.down);


        if (transform.position.y <= target_Landing.y + 0.001f)
        {
            isAirborne = false;

            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
                audioCoroutine = null;
            }
        }

    }


    public void StartAbduction()
    {

        target_Landing = transform.position;

        isBeingAbducted = true;
    }

    public void SetAirborneTrue()
    {
        //Debug.Log("Airborne");

        isAirborne = true;

        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
        }

        audioCoroutine = null;
        audioCoroutine = StartCoroutine(AudioRoutine());

        
    }

    public bool GetIsAirborne()
    {
        return isAirborne;
    }

    public void StopAbduction()
    {
        //Debug.Log("Abduction Stopped");

        isBeingAbducted = false;
    }



    private IEnumerator AudioRoutine()
    {

        while (isAirborne)
        {

            audioSource.PlayOneShot(cowAbductBarks.Value);

            yield return new WaitForSeconds(abductBarksInterval);
        }

    }

}