using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{

    //!List
    public static T GetRandom<T>(this IList<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
    public static int GetRandomIndex<T>(this IList<T> list)
    {
        return Random.Range(0, list.Count);
    }
    //!Texture2D
    public static void Finish(this Texture2D tex, Color[] col)
    {
        tex.SetPixels(col);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
    }
    //!Vectors
    public static Vector2Int ToV2Int(this Vector2 V2)
    {
        return new Vector2Int(Mathf.RoundToInt(V2.x), Mathf.RoundToInt(V2.y));
    }
    public static Vector2Int ToV2Int(this Vector3 V3)
    {
        return new Vector2Int(Mathf.RoundToInt(V3.x), Mathf.RoundToInt(V3.y));
    }
    public static Vector3 ToV3(this Vector2Int V2)
    {
        return new Vector3(V2.x, V2.y, 0);
    }
}
