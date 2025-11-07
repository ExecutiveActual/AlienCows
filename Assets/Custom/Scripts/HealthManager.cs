using UnityEngine;

public class HealthManager : MonoBehaviour
{



    private void OnApplyDamage(float amount)
    {
        TakeDamage(amount);
    }


    public void TakeDamage(float amount)
    {
        Debug.Log($"{this}, took {amount} damage!");
    }

}
