using UnityEngine;

public class HandSlot : MonoBehaviour
{

    public HandSlotType[] handSlotTypes { get; private set; }
    [SerializeField] private HandSlotType[] _handSlotTypes = new HandSlotType[] { HandSlotType.None };

    private void Awake()
    {
        handSlotTypes = _handSlotTypes;
    }

}

public enum HandSlotType
{
    None,
    Hip,
    Aim,
}