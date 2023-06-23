using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectRasterizer : MonoBehaviour
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    FurnitureDatabase furnitureDatabase;
    public List<Sprite> sprites = new List<Sprite>();
    int i;
    bool go;
    private void Start()
    {
        i = 0;
        go = true;
        furnitureDatabase = Resources.Load<FurnitureDatabase>("FurnitureDatabase");
        TextAsset reader = Resources.Load<TextAsset>("FurnitureDatabase");
        furnitureDatabase.Initialise(reader.text);
    }
    private void Update()
    {
        SendMesh(furnitureDatabase.GetDatabaseEntry(i).prefab);

        if (go && i < furnitureDatabase.database.Count)
        {
            go = false;
            StartCoroutine(Screenshot(() => { go = true; i++; }));
        }
    }
    public IEnumerator Screenshot(Action onFinish)
    {
        yield return new WaitForEndOfFrame();
        int width = Screen.width;
        int height = Screen.height;
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
        Rect rect = new Rect(0, 0, width, height);
        //Rect rect = new Rect(width / 2 - 256 / 2, height / 2 - 256 / 2, 256, 256);
        screenshotTexture.ReadPixels(rect, 0, 0);
        int leftPoint = screenshotTexture.width;
        int rightPoint = 0;
        int northPoint = 0;
        int southPoint = screenshotTexture.height;
        List<Color> colors = new List<Color>();
        for (int y = 0; y < screenshotTexture.height; y++)
        {
            for (int x = 0; x < screenshotTexture.width; x++)
            {
                if (screenshotTexture.GetPixel(x,y) == Color.black)
                {
                    screenshotTexture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    if (y > northPoint) { northPoint = y; }
                    if(y < southPoint) { southPoint = y; }
                    if(x > rightPoint) { rightPoint = x; }
                    if(x < leftPoint) { leftPoint = x; }
                }
            }
        }
        for (int y = southPoint; y < northPoint; y++)
        {
            for (int x = leftPoint; x < rightPoint; x++)
            {
                colors.Add(screenshotTexture.GetPixel(x, y));
            }
        }
        int dim = Mathf.Max(rightPoint - leftPoint, northPoint - southPoint);
        Texture2D spriteTexture = new Texture2D(rightPoint - leftPoint, northPoint - southPoint, TextureFormat.ARGB32, false);
        spriteTexture.SetPixels(colors.ToArray());
        spriteTexture.Apply();
        screenshotTexture.Apply();

        //Sprite sprite = Sprite.Create(screenshotTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        Sprite sprite = Sprite.Create(spriteTexture, new Rect(0, 0, rightPoint - leftPoint, northPoint - southPoint), new Vector2(0.5f, 0.5f));
        sprites.Add(sprite);
        onFinish();
    }
    public void SendMesh(GameObject prefab)
    {
        Mesh mesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            Graphics.DrawMesh(mesh, position, Quaternion.Euler(rotation), prefab.GetComponentInChildren<MeshRenderer>().sharedMaterials[i], 0, Camera.main, i);
        }
    }   

}
