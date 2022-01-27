using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLFunctions : MonoBehaviour
{
    public static void DrawSquareFromCorner(Vector3 origin, Vector2 dimensions, Transform trans, Color color) //Draws a square using basic OpenGL
    {
        GL.PushMatrix();
        GL.MultMatrix(trans.localToWorldMatrix);

        Material mat = Resources.Load<Material>("Materials/DebugMaterial");
        mat.color = color;
        mat.SetPass(0);

        // Draw lines
        GL.Begin(GL.LINE_STRIP);

        float shrinkBox = 0;

        int z = -100;

        origin = new Vector3(dimensions.x < 0 ? origin.x + 20 : origin.x, dimensions.y < 0 ? origin.y + 20 : origin.y);

        GL.Vertex3(origin.x + shrinkBox, origin.y + shrinkBox, z);
        Vector3 vertex = origin + new Vector3(dimensions.x - shrinkBox, shrinkBox, z);
        GL.Vertex3(vertex.x, vertex.y, vertex.z);
        vertex = origin + new Vector3(dimensions.x - shrinkBox, dimensions.y - shrinkBox, z);
        GL.Vertex3(vertex.x, vertex.y, vertex.z);
        vertex = origin + new Vector3(shrinkBox, dimensions.y - shrinkBox, z);
        GL.Vertex3(vertex.x, vertex.y, vertex.z);
        GL.Vertex3(origin.x + shrinkBox, origin.y + shrinkBox, z);

        GL.End();
        GL.PopMatrix();
    }
}
