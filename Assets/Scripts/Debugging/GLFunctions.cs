using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLFunctions : MonoBehaviour
{
    public static void DrawSquare(Vector3 origin, Vector2 dimensions, Transform trans, Color color)
    {
        GL.PushMatrix();
        GL.MultMatrix(trans.localToWorldMatrix);

        Material mat = Resources.Load<Material>("Materials/DebugMaterial");
        mat.color = color;
        mat.SetPass(0);

        // Draw lines
        GL.Begin(GL.LINE_STRIP);

        float shrinkBox = 2;

        GL.Vertex3(origin.x + shrinkBox, origin.y + shrinkBox, -1000);
        Vector3 vertex = origin + new Vector3(dimensions.x - shrinkBox, shrinkBox, -1000);
        GL.Vertex3(vertex.x, vertex.y, vertex.z);
        vertex = origin + new Vector3(dimensions.x - shrinkBox, dimensions.y - shrinkBox, -1000);
        GL.Vertex3(vertex.x, vertex.y, vertex.z);
        vertex = origin + new Vector3(shrinkBox, dimensions.y - shrinkBox, -1000);
        GL.Vertex3(vertex.x, vertex.y, vertex.z);
        GL.Vertex3(origin.x + shrinkBox, origin.y + shrinkBox, -1000);

        GL.End();
        GL.PopMatrix();
    }
}
