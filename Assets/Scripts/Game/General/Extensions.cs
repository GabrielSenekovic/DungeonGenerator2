using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
public static class Extensions
{

    //!List
    public static T GetRandom<T>(this IList<T> list)
    {
        return list[list.GetRandomIndex()];
    }
    public static T RemoveRandom<T>(this IList<T> list)
    {
        int index = list.GetRandomIndex();
        T value = list[list.GetRandomIndex()];
        list.RemoveAt(index);
        return value;
    }
    public static int GetRandomIndex<T>(this IList<T> list)
    {
        return Random.Range(0, list.Count);
    }
    public static void For<T>(this IList<T> list, Action<int> Execute, Action<int> AtEnd)
    {
        int i = 0;
        for(i = 0; i < list.Count; i++)
        {
            Execute(i);
        }
        AtEnd(i-1); //Because i will overshoot
    }
    public static void For<T>(this IList<T> list, Action<int> Execute, Action AtStart, Action<int> AtEnd)
    {
        AtStart();
        list.For(Execute, AtEnd);
    }
    public static void ForStart<T>(this IList<T> list, Action<int> Execute, Action AtStart)
    {
        AtStart();
        for (int i = 0; i < list.Count; i++)
        {
            Execute(i);
        }
    }
    //!Mesh
    public static void Init(this Mesh mesh, Vector3[] vertices, int[] indices, Vector2[] UVs)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = UVs; 
        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    public static void Init(this Mesh mesh, List<Vector3> vertices, List<int> indices, List<Vector2> UVs)
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.uv = UVs.ToArray(); 
        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
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
