using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GunController : MonoBehaviour
{
    WeaponHeld weaponHeld;

    [Header("Weapon Settings")]
    [SerializeField] private int magazine_Capacity = 8;
    private int magazine_AmmoCurrent;

    [SerializeField] private float reload_Duration = 1.2f;

    [Header("UI References (Auto-Detected)")]
    private TextMeshProUGUI AmmoCountCurrent_;
    private TextMeshProUGUI AmmoCountMax_;

    [Header("Events")]
    public UnityEvent UE_OnShoot;
    public UnityEvent UE_OnRecoil;
    public UnityEvent UE_OnReload_Start;
    public UnityEvent UE_OnReload_End;

    public bool isSafe { get; private set; }
    public bool isReloading { get; private set; }

    Coroutine reloadCoroutine;

    private void Awake()
    {
        weaponHeld = GetComponent<WeaponHeld>();
        TryAutoAssignUI();
    }

    private void OnEnable()
    {
        weaponHeld.UE_OnFire.AddListener(Fire);
        weaponHeld.UE_OnReload.AddListener(Reload);
    }

    private void OnDisable()
    {
        weaponHeld.UE_OnFire.RemoveListener(Fire);
        weaponHeld.UE_OnReload.RemoveListener(Reload);
    }

    private void Start()
    {
        magazine_AmmoCurrent = magazine_Capacity;
        UpdateAmmoUI();
    }

    private void Fire()
    {
        if (!isSafe)
        {
            if (magazine_AmmoCurrent > 0)
            {
                Shoot();
            }
        }
        else
        {
            SafetyOff();
        }
    }

    private void Reload()
    {
        if (magazine_AmmoCurrent < magazine_Capacity && !isReloading)
        {
            reloadCoroutine = StartCoroutine(ReloadRoutine());
            AudioManager.Instance.PlaySFX("Reload");
        }
    }

    private IEnumerator ReloadRoutine()
    {
        SafetyOn();
        isReloading = true;
        UE_OnReload_Start?.Invoke();

        yield return new WaitForSeconds(reload_Duration);

        UE_OnReload_End?.Invoke();

        magazine_AmmoCurrent = magazine_Capacity;
        isReloading = false;
        SafetyOff();

        UpdateAmmoUI();
        
    }

    private void SafetyOn()
    {
        isSafe = true;
        Debug.Log("Safety On");
    }

    private void SafetyOff()
    {
        isSafe = false;
        Debug.Log("Safety Off");
    }

    private void Shoot()
    {
        magazine_AmmoCurrent--;
        UE_OnShoot?.Invoke();
        UE_OnRecoil?.Invoke();
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (AmmoCountCurrent_ != null)
            AmmoCountCurrent_.text = magazine_AmmoCurrent.ToString();

        if (AmmoCountMax_ != null)
            AmmoCountMax_.text = magazine_Capacity.ToString();
    }

    private void TryAutoAssignUI()
    {
        // Looks for UI text named "AmmoCountCurrent_" and "AmmoCountMax_" anywhere in the scene
        AmmoCountCurrent_ = GameObject.Find("AmmoCountCurrent_")?.GetComponent<TextMeshProUGUI>();
        AmmoCountMax_ = GameObject.Find("AmmoCountMax_")?.GetComponent<TextMeshProUGUI>();

        if (AmmoCountCurrent_ == null || AmmoCountMax_ == null)
        {
            Debug.LogWarning(" GunController: Could not find AmmoCountCurrent_ or AmmoCountMax_ TMP texts in scene.");
        }
        else
        {
            Debug.Log("GunController: Ammo UI successfully auto-linked.");
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Ammo: " + magazine_AmmoCurrent);
    }
}
