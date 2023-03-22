using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Room room = (Room)target;
        //GUI.DrawTexture(new Rect(0,0,room.templateDEBUG.width * 30, room.templateDEBUG.height * 30), room.templateDEBUG);
        if (room.roomData.templateTexture)
        {
            RenderTexture("Template", room.roomData.templateTexture);
        }
        if (room.roomData.mapTexture)
        {
            RenderTexture("Map", room.roomData.mapTexture);
        }
        //GUILayout.Label(RenderTexture("Template", room.templateDEBUG));
    }
    void RenderTexture(string name, Texture2D texture)
    {
        GUILayout.BeginVertical();
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        style.fixedWidth = 400;
        GUILayout.Label(name, style);

        style.fixedWidth = texture.height > texture.width ? ((float)texture.width / (float)texture.height) * 400 : 400;
        style.fixedHeight = texture.width > texture.height ? ((float)texture.height / (float)texture.width) * 400 : 400;

        style.normal.background = texture;
        GUILayout.Label(new Texture2D(0, 0), style);
        //var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(400), GUILayout.Height(400));
        GUILayout.EndVertical();
    }
}
