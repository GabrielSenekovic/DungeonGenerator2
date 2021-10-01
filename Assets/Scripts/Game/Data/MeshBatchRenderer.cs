using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class MeshBatchRenderer : MonoBehaviour
{
    static EntityDatabase database;

    private void Awake() 
    {
        database = Resources.Load<EntityDatabase>("EntityDatabase");
        //Load the info how to make the grass and tulips from a file
        string path = "Assets/Resources/EntityDatabase.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path); 
        database.Initialise(reader.ReadToEnd());
        reader.Close();
    }
    //Move code from Grass.cs to here, so that other scripts can use it
    public static void RenderBatches(Grass.MeshBatch b, float batchDistanceToEdge)
    {
        Vector2 northPoint =  Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);
        Vector2 southPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
        Vector2 leftPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
        Vector2 rightPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);

        if((leftPoint.x > 0 && leftPoint.x < Camera.main.pixelWidth && leftPoint.y > 0 && leftPoint.y < Camera.main.pixelHeight) ||
            (rightPoint.x > 0 && rightPoint.x < Camera.main.pixelWidth && rightPoint.y > 0 && rightPoint.y < Camera.main.pixelHeight) ||
            (northPoint.y > 0 && northPoint.y < Camera.main.pixelHeight && northPoint.x > 0 && northPoint.x < Camera.main.pixelWidth) ||
            (southPoint.y > 0 && southPoint.y < Camera.main.pixelHeight && southPoint.x > 0 && southPoint.x < Camera.main.pixelWidth))
        {
            float dist = (b.position - Camera.main.transform.position).magnitude;
            Mesh mesh = database.GetMesh(b.name, dist);
            if(mesh != null)
            {
                Graphics.DrawMeshInstanced(mesh, 0, b.material, b.batches.Select((a) => a.matrix).ToList());
            }
        }
    }
    public static void RenderBatches(Grass.BurningMeshBatch b, float batchDistanceToEdge)
    {
        Vector2 northPoint =  Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);
        Vector2 southPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
        Vector2 leftPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x - batchDistanceToEdge, b.position.y + batchDistanceToEdge) - b.position) + b.position);
        Vector2 rightPoint = Camera.main.WorldToScreenPoint(Quaternion.Euler(0,0,-CameraMovement.rotationSideways) * (new Vector3(b.position.x + batchDistanceToEdge, b.position.y - batchDistanceToEdge) - b.position) + b.position);

        if((leftPoint.x > 0 && leftPoint.x < Camera.main.pixelWidth && leftPoint.y > 0 && leftPoint.y < Camera.main.pixelHeight) ||
            (rightPoint.x > 0 && rightPoint.x < Camera.main.pixelWidth && rightPoint.y > 0 && rightPoint.y < Camera.main.pixelHeight) ||
            (northPoint.y > 0 && northPoint.y < Camera.main.pixelHeight && northPoint.x > 0 && northPoint.x < Camera.main.pixelWidth) ||
            (southPoint.y > 0 && southPoint.y < Camera.main.pixelHeight && southPoint.x > 0 && southPoint.x < Camera.main.pixelWidth))
        {
            float dist = (b.position - Camera.main.transform.position).magnitude;
            Mesh mesh = database.GetMesh(b.name, dist);
            if(mesh != null)
            {
                Graphics.DrawMeshInstanced(mesh, 0, b.material, b.batches.Select((a) => a.matrix).ToList());
            }
        }
    }
}
