using UnityEngine;
using TMPro;
using System;

public class TextDataUpdate : MonoBehaviour
{

    TMP_Text tmpText;



    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }


    private void Start()
    {

        UpdateTextData();

    }


    private void UpdateTextData()
    {

        tmpText.text = $"Day {GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>().GetNightNumberCurrent()}";

    }
}
