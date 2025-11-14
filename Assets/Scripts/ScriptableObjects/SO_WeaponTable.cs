using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_WeaponTable", menuName = "Scriptable Objects/SO_WeaponTable")]
public class SO_WeaponTable : ScriptableObject
{
    
    [SerializeField] private int[] weaponIDs;

    [SerializeField] private WeaponHeld[] weaponHelds;


    public Dictionary<int, WeaponHeld> WeaponDictionary;


    public WeaponHeld GetWeaponFromID(int ID)
    {

        if (WeaponDictionary == null)
        {
            WeaponDictionary = new Dictionary<int, WeaponHeld>();
            for (int i = 0; i < weaponIDs.Length; i++)
            {
                WeaponDictionary.Add(weaponIDs[i], weaponHelds[i]);
            }
        }

        return WeaponDictionary[ID];

    }

}
