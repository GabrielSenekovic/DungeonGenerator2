using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder2D : MonoBehaviour, ILevelBuilder
{
    public void BuildLevel(
        LevelManager level, 
        ref List<RoomTemplate> templates, 
        ref RoomTemplate bigTemplate, 
        SettlementData settlementData)
    {
        throw new System.NotImplementedException();
    }

    public bool HasGenerated()
    {
        throw new System.NotImplementedException();
    }
}
