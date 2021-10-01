using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLog : MonoBehaviour
{
    public static void ReportBrokenSeed(int dataSeed, int constructionSeed, string text)
    {
        text = "<color=red>" + text + " ERROR: Data seed: " + dataSeed + "Construction seed: " + constructionSeed;
    }
    public static void Report(List<Vector3>vertices, List<int>indices, List<Vector2>UV)
    {
        string text = "Vertices: " + vertices.Count + " Indices: " + indices.Count + " UV: " + UV.Count;
        if(UV.Count > vertices.Count)
        {
            text += " Vertices and UV are not the same size! There are too many UVs";
        }
        else if(UV.Count < vertices.Count)
        {
            text += " Vertices and UV are not the same size! There are too few UVs";
        }
        Debug.Log(text);
    }
}
