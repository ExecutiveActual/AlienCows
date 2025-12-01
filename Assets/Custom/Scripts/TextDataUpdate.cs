using UnityEngine;
using TMPro;
using System;

public class TextDataUpdate : MonoBehaviour
{

    TMP_Text tmpText;

    [SerializeField] private string prefix = "Day ";


    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }


    private void Start()
    {

        UpdateTextData();

    }


    public void UpdateTextData()
    {

        tmpText.text = $"{prefix} {GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>().GetNightNumberCurrent()}";

    }
}
