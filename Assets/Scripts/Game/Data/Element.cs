using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Flags]
public enum Element
{
    NONE = 0,
    WATER = 1,
    FIRE = 1 << 1,
    EARTH = 1 << 2,
    AIR = 1 << 3,
    METAL = 1 << 4,
    LIGHTNING = 1 << 5,
    WOOD = 1 << 6,
    ICE = 1 << 7,
    AETHER = 1 << 8
    // LIGHT = 5,
    // DARK = 6,
}