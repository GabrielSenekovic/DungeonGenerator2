using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelBuilder
{
    public void BuildLevel(
        LevelManager level,
        ref List<RoomTemplate> templates,
        ref RoomTemplate bigTemplate,
        SettlementData settlementData);
    //SettlementData is probably not supposed to be here
    public bool HasGenerated();
}
