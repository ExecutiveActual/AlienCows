using UnityEngine;

public class HitZone : MonoBehaviour
{

    [SerializeField] private float damageMultiplier = 1f;


    public void TakeDamage(float damage)
    {
        Debug.Log($"{this}, took {damage} damage!");

        SendMessageUpwards("OnApplyDamage", damage * damageMultiplier, SendMessageOptions.DontRequireReceiver);
    }
}
