using UnityEngine;

public class UFO_WaveRegistry : MonoBehaviour
{
    


    public void Register(Spawner_UFO spawner)
    {
        spawner.RegisterUFO(this);
    }

}
