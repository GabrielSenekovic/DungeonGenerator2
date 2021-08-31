using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    public Texture CameraMap;
    Texture SpriteMap;
    public RawImage map;

    public void CreateMap(List<Room.RoomTemplate> templates)
    {
        //This function also needs to know how far to the left the map should go and how far to the right
        //Use the centering function from vertical slice
       // SpriteMap = new Texture();
    }
    public void SwitchView()
    {
        if(map.texture == CameraMap)
        {
            map.texture = SpriteMap;
        }
        else
        {
            map.texture = CameraMap;
        }
    }
}
