using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.Rendering;

public class ObjectRasterizeTest : MonoBehaviour
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public Material baseMaterial;
    public GameObject prefab;
    SpriteRenderer rend;
    public RenderTexture renderTexture;
    private void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        SendMesh();
        StartCoroutine(Screenshot());
    }
    void Update()
    {
        SendMesh();
    }
    public IEnumerator Screenshot()
    {
        yield return new WaitForEndOfFrame();
        int width = Screen.width;
        int height = Screen.height;
        Texture2D screenshotTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Rect rect = new Rect(width/2 - 256/2, height/2 - 256/2, 256, 256);
        screenshotTexture.ReadPixels(rect, 0, 0);
        screenshotTexture.Apply();

        Sprite sprite = Sprite.Create(screenshotTexture, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
        rend.sprite = sprite;
    }
    /*public Sprite GetSprite()
    {
        int dim = 128 * 2;
        Texture2D texture = new Texture2D(dim, dim, TextureFormat.RGBA32, false);
        Graphics.SetRenderTarget(renderTexture);
        baseMaterial.SetPass(0);
        Mesh mesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(position, Quaternion.Euler(rotation), scale));
        texture.ReadPixels(new Rect(0, 0, dim, dim), 0, 0);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, dim, dim), new Vector2(0.5f, 0.5f));
        renderTexture.Release();
        return sprite;
    }*/
    public void SendMesh()
    {
        Graphics.SetRenderTarget(renderTexture);
        Mesh mesh = prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
        for(int i = 0; i < mesh.subMeshCount; i++)
        {
            Graphics.DrawMesh(mesh, position, Quaternion.Euler(rotation), prefab.GetComponentInChildren<MeshRenderer>().sharedMaterials[i], i, Camera.main);
        }
       // RenderPipelineManager.endCameraRendering += CreateSprite;
    }
   /* public void CreateSprite(ScriptableRenderContext context, Camera camera)
    {
        int dim = 128 * 2;
        Graphics.SetRenderTarget(renderTexture);
        Texture2D texture = new Texture2D(dim, dim, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, dim, dim), 0, 0);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, dim, dim), new Vector2(0.5f, 0.5f));
        rend.sprite = sprite;
    }*/
}
