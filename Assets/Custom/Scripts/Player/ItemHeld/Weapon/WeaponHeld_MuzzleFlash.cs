using System.Collections;
using UnityEngine;

public class WeaponHeld_MuzzleFlash : MonoBehaviour
{


    [SerializeField] private Transform muzzleFlashTransform;

    [SerializeField] private float flashDuration = 0.012f;

    //[SerializeField] private float randomAngleRange = 10f;

    //[SerializeField] private float randomScaleRange = 0.03f;



    bool isFlashVisible = false;

    Coroutine muzzleFlashCoroutine;

    float muzzleFlashTransform_initialScale;



    private void Awake()
    {
        muzzleFlashTransform_initialScale = muzzleFlashTransform.localScale.x;
    }

    private void Start()
    {
        muzzleFlashTransform.gameObject.SetActive(false);

        isFlashVisible = false;

        Randomize();
    }


    private void OnShoot()
    {

        if (isFlashVisible)
        {
            muzzleFlashTransform.gameObject.SetActive(false);
            isFlashVisible = false;
        }

        muzzleFlashCoroutine = null;
        muzzleFlashCoroutine = StartCoroutine(MuzzleFlashRoutine());

    }


    private void Randomize()
    {

        //muzzleFlashTransform.localScale = Vector3.one * (muzzleFlashTransform_initialScale + Random.Range(-randomScaleRange, randomScaleRange));

        //muzzleFlashTransform.localRotation = Quaternion.Euler(
        //    muzzleFlashTransform.localRotation.x,
        //    muzzleFlashTransform.localRotation.y,
        //    Random.Range(-randomAngleRange, randomAngleRange)
        //    );

    }


    private IEnumerator MuzzleFlashRoutine()
    {

        muzzleFlashTransform.gameObject.SetActive(true);
        isFlashVisible = true;

        yield return new WaitForSeconds(flashDuration);

        muzzleFlashTransform.gameObject.SetActive(false);
        isFlashVisible = false;

        Randomize();
    }




}
