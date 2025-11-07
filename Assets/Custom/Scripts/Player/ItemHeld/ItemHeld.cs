using UnityEngine;

public class ItemHeld : MonoBehaviour
{

    public string ItemName { get; private set; }
    [SerializeField] private string _ItemName = "ItemName Not Set";

    public HandSlot defaultHandSlot { get; private set; }
    [SerializeField] private HandSlot _defaultHandSlot;

    public DumbSlerpToTarget SlerpGuide { get; private set; }
    [SerializeField] private DumbSlerpToTarget _SlerpGuide;


    private void Awake()
    {
        ItemName = _ItemName;
        defaultHandSlot = _defaultHandSlot;
        SlerpGuide = _SlerpGuide;
    }


    private void Start()
    {
        SlerpGuide.transform.SetParent(null);
    }

}
