using System.Collections.Generic;
using UnityEngine;

public class Camera_HandSlotManager : MonoBehaviour
{
    public List<HandSlot> handSlots { get; private set; } = new List<HandSlot>();
    [SerializeField] private List<HandSlot> _handSlots = new List<HandSlot>();


    private void Awake()
    {
        handSlots = _handSlots;
    }



    public HandSlot FindSlotByType(HandSlotType targetType)
    {
        foreach (HandSlot slot in handSlots)
        {
            foreach (HandSlotType slotType in slot.handSlotTypes)
            {
                if (slotType == targetType)
                {
                    return slot;
                }
            }
        }
        return null;

    }

}