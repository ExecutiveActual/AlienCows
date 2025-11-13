using UnityEngine;

public class Gun_SoundController : MonoBehaviour
{

    WeaponHeld weaponHeld;

    GunController gunController;

    AudioSource audioSource;


    [Header("Audio Clips")]
    [SerializeField] private SO_RandomSound sound_Fire;

    [SerializeField] private SO_RandomSound sound_Reload;




    private void Awake()
    {
        weaponHeld = GetComponent<WeaponHeld>();
        gunController = GetComponent<GunController>();
        audioSource = GetComponent<AudioSource>();
    }


    private void OnEnable()
    {
        gunController.UE_OnShoot.AddListener(PlayFireSound);
        weaponHeld.UE_OnReload.AddListener(PlayReloadSound);
    }

    private void OnDisable()
    {
        gunController.UE_OnShoot.RemoveListener(PlayFireSound);
        weaponHeld.UE_OnReload.RemoveListener(PlayReloadSound);
    }


    private void PlayFireSound()
    {
        audioSource.PlayOneShot(sound_Fire.Value);
    }

    private void PlayReloadSound()
    {
        //audioSource.PlayOneShot(sound_Reload.Value);
    }


}
