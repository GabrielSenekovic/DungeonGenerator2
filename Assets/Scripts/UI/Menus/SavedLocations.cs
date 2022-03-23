using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavedLocations : MonoBehaviour
{
    [System.Serializable]public class LocationData
    {
        public string name;
        public Texture2D map;
        public int levelConstructionSeed; //Determines structure, topography of level etc
        public int levelDataSeed; //Determines biome, climate etc

        public LocationData(string name_in, Texture2D map_in, int levelConstructionSeed_in, int levelDataSeed_in)
        {
            name = name_in; map = map_in; levelConstructionSeed = levelConstructionSeed_in; levelDataSeed = levelDataSeed_in;
        }
    }
    public List<LocationData> locationData;
    public SpriteText text;

    public void AddLocation(string name, Texture2D map, int levelConstructionSeed, int levelDataSeed)
    {
        locationData.Add(new LocationData(name, map, levelConstructionSeed, levelDataSeed));
        text.Write(name);
        text.PlaceSprite(Sprite.Create(map, new Rect(0, 0, map.width, map.height), new Vector2(0.5f, 0.5f), 16));
    }
}
