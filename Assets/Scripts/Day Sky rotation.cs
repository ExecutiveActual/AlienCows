using UnityEngine;

public class DaySkyrotation : MonoBehaviour
{
    [SerializeField] private Material skybox;
    private float _elapsedTime = 0f;
    private float _timeScale = 0.15f;
    private static readonly int Rotation = Shader.PropertyToID("_Rotation");

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        skybox.SetFloat(Rotation, _elapsedTime * _timeScale);

    }
}
