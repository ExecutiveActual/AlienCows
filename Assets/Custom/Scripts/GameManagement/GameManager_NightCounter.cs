using UnityEngine;

public class GameManager_NightCounter : MonoBehaviour
{
    
    public int currentNight { get; private set; } = 1;

    public void AdvanceToNextNight()
    {
        currentNight++;
    }



}
