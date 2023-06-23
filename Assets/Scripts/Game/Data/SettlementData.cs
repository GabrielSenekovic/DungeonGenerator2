using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SettlementData : ScriptableObject
{
    new string name;
    List<GameObject> buildings = new List<GameObject>();
    List<NPCController> population = new List<NPCController>();

    public SettlementData(string name)
    {
        this.name = name;
    }
    public void Add(GameObject building)
    {
        buildings.Add(building);
    }
    public void Add(NPCController citizen)
    {
        population.Add(citizen);
    }
}
