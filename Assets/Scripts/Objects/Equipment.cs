using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public enum EquipmentType
    {
        NONE = 0,
        SWORD = 1,
        SHIELD = 2,
        MACE = 3,
        BOW = 4,
        AXE = 5
    }
    public string name;
    public Sprite icon;
    [SerializeField] EquipmentType type;

    public EquipmentType Type => type;
}
