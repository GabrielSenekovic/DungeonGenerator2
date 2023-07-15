using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapContainer
{
    public Sprite mainMap;
    public Sprite vegetationMap;
    public Sprite altitudeMap; //Currently unused
    public MapContainer(Sprite mainMap)
    {
        this.mainMap = mainMap;
    }
}
public class MapMenu : MonoBehaviour, IMenu
{
    List<MapContainer> mapContainers = new List<MapContainer>();
    MapContainer currentMapContainer;
    [SerializeField] Image mapImage;
    [SerializeField] CanvasGroup canvasGroup;
    public void AddMap(MapContainer maps)
    {
        mapContainers.Add(maps);
        currentMapContainer = maps;
        SetMap(maps);
    }

    public CanvasGroup GetCanvas()
    {
        return canvasGroup;
    }

    public void OnClose()
    {
        
    }

    public void OnOpen()
    {
        SetMap(currentMapContainer);
    }

    public void SetMap(MapContainer maps)
    {
        mapImage.sprite = maps.mainMap;
        mapImage.SetNativeSize();
        mapImage.rectTransform.anchoredPosition = new Vector2(0, -(mapImage.rectTransform.rect.height / 2));
    }
}
