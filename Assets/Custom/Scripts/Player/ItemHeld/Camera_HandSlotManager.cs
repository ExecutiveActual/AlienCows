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











    /*
    
    
    public HandSlot FindHandSlotByTypeSearch(List<HandSlotType> targetTypes)
    {

        foreach (HandSlot slot in handSlots)
        {
            foreach (HandSlotType slotType in slot.handSlotTypes)
            {
                foreach (HandSlotType targetType in targetTypes)
                {
                    if (slotType == targetType)
                    {
                        return slot;
                    }
                }
            }
        }
        return null;
    }


    public List<HandSlot> FindCompatibleSlots(HandSlotType targetType)
    {
        List<HandSlot> compatibleSlots = new List<HandSlot>();
        foreach (HandSlot slot in handSlots)
        {
            foreach (HandSlotType slotType in slot.handSlotTypes)
            {
                if (slotType == targetType)
                {
                    compatibleSlots.Add(slot);
                    break;
                }
            }
        }
        return compatibleSlots;
    }


     */



}