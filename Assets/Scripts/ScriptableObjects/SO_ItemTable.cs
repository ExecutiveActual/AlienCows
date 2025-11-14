using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_ItemTable", menuName = "Scriptable Objects/SO_ItemTable")]
public class SO_ItemTable : ScriptableObject
{
    [SerializeField] private int[] itemIDs;

    [SerializeField] private ItemHeld[] itemHelds;


    public Dictionary<int, ItemHeld> ItemDictionary;


    public ItemHeld GetItemFromID(int ID)
    {

        if (ItemDictionary == null)
        {
            ItemDictionary = new Dictionary<int, ItemHeld>();
            for (int i = 0; i < itemIDs.Length; i++)
            {
                ItemDictionary.Add(itemIDs[i], itemHelds[i]);
            }
        }

        return ItemDictionary[ID];

    }
}
