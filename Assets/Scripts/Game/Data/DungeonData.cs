using UnityEngine;
using System.Collections;

public enum DungeonPreset
{
    Cistern = 0,
    Sewers = 1,
    Windmill = 2,
    Fortress = 3,
    Castle = 4,
    Citadel = 5,
    Cathedral = 6,
    Tomb = 7,
    CrawlSpace = 8,
    Spire = 9,
    LightHouse = 10,
    Pyramid = 11
}

public class DungeonData : MonoBehaviour
{
    DungeonPreset dungeonType;
    public void Initialize()
    {
        dungeonType = DungeonPreset.Sewers;
    }
    public DungeonPreset GetDungeonType()
    {
        return dungeonType;
    }
}
