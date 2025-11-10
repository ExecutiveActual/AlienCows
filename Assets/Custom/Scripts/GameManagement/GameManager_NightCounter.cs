using UnityEngine;

public class GameManager_NightCounter : MonoBehaviour
{
    
    public int Night_Curr { get; private set; } = 1;

    public void AdvanceToNextNight()
    {
        Night_Curr++;
    }



}
