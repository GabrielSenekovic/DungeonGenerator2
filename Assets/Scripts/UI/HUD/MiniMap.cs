using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    public Texture CameraMap;
    Texture SpriteMap;

    Texture FullSpriteMap;
    public RawImage map;

    public void CreateMap(List<Room.RoomTemplate> templates)
    {
        //This function also needs to know how far to the left the map should go and how far to the right
        //Use the centering function from vertical slice
       // SpriteMap = new Texture();
    }
    public void Update()
    {
        Vector2 pos = (Vector2)CameraMovement.Instance.cameraRotationObject.transform.position - new Vector2(CameraMovement.Instance.cameraAnchor_horizontal.x, CameraMovement.Instance.cameraAnchor_vertical.y);
        pos = pos / 20 * 100;
        Vector2 topLeft = new Vector2((map.rectTransform.sizeDelta.x / 100 - 1) * 50, -(map.rectTransform.sizeDelta.y / 100 - 1) * 50);
        map.transform.localPosition = topLeft - pos;
    }
    public void SwitchMap(Texture2D tex)
    {
        //20 in size from room is 100 in size on the tex
        //100 x 100 gives a scale of 0.9
        SpriteMap = tex;
        if(map.texture != CameraMap)
        {
            map.texture = SpriteMap;
            map.rectTransform.sizeDelta = new Vector2(tex.width / 20 * 100, tex.height / 20 * 100);
            map.transform.localPosition = new Vector2((map.rectTransform.sizeDelta.x / 100 - 1) * 50, -(map.rectTransform.sizeDelta.y / 100 - 1) * 50);
        }
    }
    public void ToggleView()
    {
        if(map.texture == CameraMap)
        {
            map.texture = SpriteMap;
            map.rectTransform.sizeDelta = new Vector2(SpriteMap.width / 20 * 100, SpriteMap.height / 20 * 100);
            map.transform.localPosition = new Vector2((map.rectTransform.sizeDelta.x / 100 - 1) * 50, -(map.rectTransform.sizeDelta.y / 100 - 1) * 50);
        }
        else
        {
            map.texture = CameraMap;
            map.rectTransform.sizeDelta = new Vector2(100, 100);
            map.transform.localPosition = Vector2.zero;
        }
    }
}
