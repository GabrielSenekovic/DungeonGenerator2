using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WallData = MeshMaker.WallData;

[System.Serializable]
public class WallInstructions
{
    List<WallData> wallData = new List<WallData>();
    string materialName;
    public string ID;

    public string MaterialName => materialName;
    public int Count => wallData.Count;

    public List<WallData> Data => wallData;

    public WallInstructions(string materialName, string ID)
    {
        wallData = new List<WallData>();
        this.materialName = materialName;
        this.ID = ID;
    }

    public WallInstructions(List<WallData> wallData, string materialName, string ID)
    {
        this.wallData = wallData;
        this.materialName = materialName;
        this.ID = ID;
    }
    public void Add(WallData data)
    {
        wallData.Add(data);
    }
}
