using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public partial class MeshMaker : MonoBehaviour
{
    public class SurfaceData
    {
        public List<SurfaceTileData> tiles;
        public TileTemplate.TileType tileType;
        public SurfaceData(List<SurfaceTileData> tiles, TileTemplate.TileType tileType)
        {
            this.tiles = tiles;
            this.tileType = tileType;
        }
    }
    public struct SurfaceTileData
    {
        public Vector3Int position;
        public List<Vector3> ceilingVertices;
        public List<Vector3> floorVertices;
        public int divisions; // needed for when determining how many times a position is removed in create surface when connecting vertices isnt 0
        public List<TileTemplate.TileSides> sidesWhereThereIsWall;

        public SurfaceTileData(Vector3Int position_in, List<Vector3> ceilingVertices_in, List<Vector3> floorVertices_in, int divisions_in, List<TileTemplate.TileSides> sidesWhereThereIsWall_in)
        {
            position = position_in;
            ceilingVertices = ceilingVertices_in;
            floorVertices = floorVertices_in;
            divisions = divisions_in;
            sidesWhereThereIsWall = sidesWhereThereIsWall_in;
        }
    }
    static public Mesh GetBillBoard()
    {
        Mesh mesh = new Mesh();
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        float width = 2;
        float height = 2;

        //newVertices.Add(new Vector3(0.5f,0.5f,0));
        //newVertices.Add(new Vector3(-0.5f,0.5f,0));
        //newVertices.Add(new Vector3(-0.5f,0.5f,0));
        //newVertices.Add(new Vector3(0.5f,-0.5f,0));

        newVertices.Add(new Vector3(0.5f * width, 0, 0));
        newVertices.Add(new Vector3(-0.5f * width, 0, 0));
        newVertices.Add(new Vector3(-0.5f * width, 0, -height));
        newVertices.Add(new Vector3(0.5f * width, 0, -height));

        newVertices.Add(new Vector3(0, 0.5f * width, 0));
        newVertices.Add(new Vector3(0, -0.5f * width, 0));
        newVertices.Add(new Vector3(0, -0.5f * width, -height));
        newVertices.Add(new Vector3(0, 0.5f * width, -height));

        for (int i = 0; i < 2; i++)
        {
            newTriangles.Add(3 + 4 * i);
            newTriangles.Add(1 + 4 * i);
            newTriangles.Add(0 + 4 * i);
            newTriangles.Add(3 + 4 * i);
            newTriangles.Add(2 + 4 * i);
            newTriangles.Add(1 + 4 * i);

            newUV.Add(new Vector2(1, 0));                      //1,0
            newUV.Add(new Vector2(0, 0));                      //0,0
            newUV.Add(new Vector2(0, 1)); //0,1
            newUV.Add(new Vector2(1, 1));
        }

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
    public static void CreateBush(Mesh mesh, int height)
    {
        //This requires the CreateStone() function
    }
    public static void CreateChest(GameObject chest, int length)
    {
        //Specify how long the chest is. Length determines how long it is from left to right, if the opening side is in front
        //Start off by creating just a cube chest
        //Like in Minecraft
        //Start from the back then forward. Save the points on the top where the lid will rotate around
        Material mat = Resources.Load<Material>("Materials/DebugChest");
        int[] partHeights_pixels = new int[2] { 10, 6 };
        float[] partHeights = new float[2] { 1.0f / 16 * partHeights_pixels[0], 1.0f / 16 * partHeights_pixels[1] }; //So the bottom part is 10 pixels tall and the top is 6 pixels tall
        float radius = Mathf.Sqrt((Mathf.Pow(0.5f, 2) + Mathf.Pow(0.5f, 2)));
        float totalHeight = 0;
        float horizontalPixel = 1.0f / (16 * 3);
        float verticalPixel = 1.0f / (16 * 4);
        for (int i = 0; i < 2; i++)
        {
            float currentHeight = 0;
            GameObject temp = new GameObject();
            temp.transform.localPosition = new Vector3(temp.transform.position.x, temp.transform.position.y, -totalHeight);

            List<int> newTriangles = new List<int>();
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUV = new List<Vector2>();
            List<Vector3> positions = new List<Vector3>();
            Mesh mesh = new Mesh();
            int stepsAroundCenter = 4;
            CreateCylinder(ref positions, ref currentHeight, 2, stepsAroundCenter, partHeights[i], radius);
            Debug.Log("Positions: " + positions.Count);

            int[] indexValue = new int[] { 3, 1, 0, 3, 2, 1 };
            for (int j = 0; j < positions.Count - stepsAroundCenter; j++) //!dont use this later
            {
                Debug.Log("J: " + j);
                newVertices.Add(positions[0 + j]);
                newVertices.Add(positions[(1 + j) % stepsAroundCenter + stepsAroundCenter * (int)((float)j / (float)stepsAroundCenter)]);
                newVertices.Add(positions[(stepsAroundCenter + 1 + j) % stepsAroundCenter + stepsAroundCenter * (int)((float)(j + stepsAroundCenter) / (float)stepsAroundCenter)]);
                newVertices.Add(positions[stepsAroundCenter + j]);

                for (int index = 0; index < indexValue.Length; index++)
                {
                    newTriangles.Add(indexValue[index] + j * 4);
                }
            }
            //? Set top and btm of cube
            if (i == 0)
            {
                newVertices.Add(positions[3]);
                newVertices.Add(positions[2]);
                newVertices.Add(positions[1]);
                newVertices.Add(positions[0]);
                for (int index = 0; index < indexValue.Length; index++)
                {
                    newTriangles.Add(indexValue[index] + (newVertices.Count - stepsAroundCenter));
                }
            }
            if (i == 1)
            {
                newVertices.Add(positions[positions.Count - stepsAroundCenter + 2]);
                newVertices.Add(positions[positions.Count - stepsAroundCenter + 3]);
                newVertices.Add(positions[positions.Count - stepsAroundCenter + 0]);
                newVertices.Add(positions[positions.Count - stepsAroundCenter + 1]);
                for (int index = 0; index < indexValue.Length; index++)
                {
                    newTriangles.Add(indexValue[index] + (newVertices.Count - stepsAroundCenter));
                }
            }

            if (i == 0) //Bottom part. Vertically should only reach up to where the lid starts. 
                        //The start height is the value in "partHeights", the float array in this functon
            {
                //Backside
                newUV.Add(new Vector2(horizontalPixel * 16, verticalPixel * 16 * 3));
                newUV.Add(new Vector2(horizontalPixel * 16 * 2, verticalPixel * 16 * 3));
                newUV.Add(new Vector2(horizontalPixel * 16 * 2, verticalPixel * 16 * 4 - verticalPixel * partHeights_pixels[1]));
                newUV.Add(new Vector2(horizontalPixel * 16, verticalPixel * 16 * 4 - verticalPixel * partHeights_pixels[1]));
                //Left side
                newUV.Add(new Vector2(0, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1.0f / 3, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1.0f / 3, 1.0f - (1.0f / 4) - verticalPixel * partHeights_pixels[1]));
                newUV.Add(new Vector2(0, 1.0f - (1.0f / 4) - verticalPixel * partHeights_pixels[1]));
                //Front
                newUV.Add(new Vector2(1.0f / 3, 1.0f / 4));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f / 4));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f - 1.0f / 4 * 2 - verticalPixel * partHeights_pixels[1]));
                newUV.Add(new Vector2(1.0f / 3, 1.0f - 1.0f / 4 * 2 - verticalPixel * partHeights_pixels[1]));
                //Right side
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1, 1.0f - (1.0f / 4) - verticalPixel * partHeights_pixels[1]));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f - (1.0f / 4) - verticalPixel * partHeights_pixels[1]));
                //Bottom side
                newUV.Add(new Vector2(1.0f / 3, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f - (1.0f / 4)));
                newUV.Add(new Vector2(1.0f / 3, 1.0f - (1.0f / 4)));
            }
            else //lid
            {
                //Backside
                newUV.Add(new Vector2(horizontalPixel * 16, verticalPixel * 16 * 3 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(horizontalPixel * 16 * 2, verticalPixel * 16 * 3 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(horizontalPixel * 16 * 2, verticalPixel * 16 * 4));
                newUV.Add(new Vector2(horizontalPixel * 16, verticalPixel * 16 * 4));
                //Left side
                newUV.Add(new Vector2(0, 1.0f / 4 * 2 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(1.0f / 3, 1.0f / 4 * 2 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(1.0f / 3, 1.0f - (1.0f / 4)));
                newUV.Add(new Vector2(0, 1.0f - (1.0f / 4)));
                //Front
                newUV.Add(new Vector2(1.0f / 3, 1.0f / 4 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f / 4 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f - 1.0f / 4 * 2));
                newUV.Add(new Vector2(1.0f / 3, 1.0f - 1.0f / 4 * 2));
                //Right side
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f / 4 * 2 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(1, 1.0f / 4 * 2 + verticalPixel * partHeights_pixels[0]));
                newUV.Add(new Vector2(1, 1.0f - (1.0f / 4)));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f - (1.0f / 4)));
                //Top side
                newUV.Add(new Vector2(1.0f / 3, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f / 4 * 2));
                newUV.Add(new Vector2(1.0f / 3 * 2, 1.0f - (1.0f / 4)));
                newUV.Add(new Vector2(1.0f / 3, 1.0f - (1.0f / 4)));
            }

            mesh.Init(newVertices, newTriangles, newUV);
            temp.transform.parent = chest.transform;
            MeshRenderer rend = temp.AddComponent<MeshRenderer>();
            rend.material = mat;
            MeshFilter filter = temp.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            totalHeight += partHeights[i]; //Separating totalheight and currentheight should stack the cubes instead of building the mesh that way. This way it should be easier to rotate them
        }
        chest.transform.rotation = Quaternion.Euler(0, 0, 45);
        Chest chestScript = chest.AddComponent<Chest>();
        chestScript.lid = chest.transform.GetChild(1);
        chest.AddComponent<BoxCollider>();
        //Make two different meshes and put them on two different child objects
    }
    public static void CreateCylinder(ref List<Vector3> positions, ref float currentHeight, int limit, int stepsAroundCenter, float heightIncrement, float radius)
    {
        float angle = 0; float angleIncrement = 360 / stepsAroundCenter;

        for (int i = 0; i < limit; i++)
        {
            for (int j = 0; j < stepsAroundCenter; j++)
            {
                positions.Add(new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad), radius * Mathf.Sin(angle * Mathf.Deg2Rad), -currentHeight));
                angle += angleIncrement;
            }
            currentHeight += heightIncrement;
        }
    }
    public static void CreateCylinder(ref List<Vector3> positions, ref float currentHeight, int limit, int stepsAroundCenter, float heightIncrement, AnimationCurve curve)
    {
        float angle = 0; float angleIncrement = 360 / stepsAroundCenter;

        for (int i = 0; i < limit; i++)
        {
            for (int j = 0; j < stepsAroundCenter; j++)
            {
                float radius = curve != null ? curve.Evaluate((float)i / (float)limit) : (1f / 32f);
                positions.Add(new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad), radius * Mathf.Sin(angle * Mathf.Deg2Rad), -currentHeight));
                angle += angleIncrement;
            }
            currentHeight += heightIncrement;
        }
    }

    public static Texture2D CreateFlower(Mesh mesh, Material flowerMaterial, float height, float bulbHeight, int whorls, int merosity, float openness, Vector2 offset, AnimationCurve curve, AnimationCurve flowerShape, List<Color> colors, bool spread)
    {
        //Create a stalk, create leaves and add a perianth to the top of the stalk
        //The stalk is triangular so it can be seen from all angles
        //Tulip grow about 30cm high
        //Debug.Log("Creating flower");
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        height = Math.ConvertCentimetersToPixels((int)height);
        bulbHeight = Math.ConvertCentimetersToPixels((int)bulbHeight);

        //!The height will also determine the height of the texture
        //Debug.Log("Making texture");
        Texture2D texture = new Texture2D(2, (int)(height * 16), TextureFormat.ARGB32, false); //1 in height corresponds to 16 pixels
        for (int i = 0; i < texture.width * texture.height; i++)
        {
            if (i / texture.width < (int)(height * 16 - bulbHeight * 16))
            {
                texture.SetPixel(i % texture.width, i / texture.width, colors[0]);
            }
            else
            {
                texture.SetPixel(i % texture.width, i / texture.width, colors[1]);
            }
        }
        //Debug.Log("Applying texture");
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        flowerMaterial.SetTexture("_MainTex", texture);

        List<Vector3> positions = new List<Vector3>();
        float currentHeight = 0;
        float amountOfVerticesVertical = 5;
        float heightIncrement = (height - bulbHeight) / amountOfVerticesVertical;

        int stepsAroundCenter = 3;

        //Debug.Log("Creating stalk");

        CreateCylinder(ref positions, ref currentHeight, (int)amountOfVerticesVertical, stepsAroundCenter, heightIncrement, null);

        for (int j = 0; j < positions.Count - stepsAroundCenter; j++)
        {
            newVertices.Add(positions[0 + j]);
            newVertices.Add(positions[(1 + j) % stepsAroundCenter + stepsAroundCenter * (int)((float)j / (float)stepsAroundCenter)]);
            newVertices.Add(positions[(stepsAroundCenter + 1 + j) % stepsAroundCenter + stepsAroundCenter * (int)((float)(j + 3) / (float)stepsAroundCenter)]);
            newVertices.Add(positions[stepsAroundCenter + j]);

            int[] indexValue = new int[] { 0, 1, 3, 1, 2, 3 };

            for (int index = 0; index < indexValue.Length; index++)
            {
                newTriangles.Add(indexValue[index] + j * 4);
            }
        }
        float amountOfQuadsVertical = amountOfVerticesVertical - 1;
        for (int j = 0; j < amountOfQuadsVertical; j++)
        {
            for (int k = 0; k < stepsAroundCenter; k++) //Minus one because there are 4 quads if there are 5 vertices upwards
            {
                newUV.Add(new Vector2(1, ((height - bulbHeight) / height) / amountOfQuadsVertical * j));     //1,0         1.0f / amountOfQuadsVertical * j
                newUV.Add(new Vector2(0, ((height - bulbHeight) / height) / amountOfQuadsVertical * j));     //0,0
                newUV.Add(new Vector2(0, ((height - bulbHeight) / height) / amountOfQuadsVertical * (j + 1))); //0,1
                newUV.Add(new Vector2(1, ((height - bulbHeight) / height) / amountOfQuadsVertical * (j + 1))); //1,1
            }
        }
        //Stalk done, lets add Perianth
        //Debug.Log("Creating Perianth");
        CreatePerianth(ref newVertices, ref newTriangles, ref newUV, currentHeight - heightIncrement, bulbHeight, whorls, merosity, (height - bulbHeight) / height, openness, offset, curve, flowerShape, spread);
        //!height - bulbheight only works because height is 1, so its already procentual

        DebugLog.Report(newVertices, newTriangles, newUV);

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        return texture;
    }
    public static List<Vector3Int> CreateIcosahedron(ref List<Vector3> vertices)
    {
        //http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html 
        float s = Mathf.Sqrt((5.0f - Mathf.Sqrt(5.0f)) / 10.0f) / 2.0f;
        float t = Mathf.Sqrt((5.0f + Mathf.Sqrt(5.0f)) / 10.0f) / 2.0f;
        //float s = Mathf.Sqrt((5.0f - Mathf.Sqrt(5.0f)) / 10.0f) /2.0f; //!the /2 makes it a pentakis icosidodecahedron instead
        //float t = Mathf.Sqrt((5.0f + Mathf.Sqrt(5.0f)) / 10.0f) /2.0f;

        vertices.Add(new Vector3(-s, t, 0));
        vertices.Add(new Vector3(s, t, 0));
        vertices.Add(new Vector3(-s, -t, 0));
        vertices.Add(new Vector3(s, -t, 0));

        vertices.Add(new Vector3(0, -s, t));
        vertices.Add(new Vector3(0, s, t));
        vertices.Add(new Vector3(0, -s, -t));
        vertices.Add(new Vector3(0, s, -t));

        vertices.Add(new Vector3(t, 0, -s));
        vertices.Add(new Vector3(t, 0, s));
        vertices.Add(new Vector3(-t, 0, -s));
        vertices.Add(new Vector3(-t, 0, s));

        List<Vector3Int> faces = new List<Vector3Int>();

        // 5 faces around point 0
        faces.Add(new Vector3Int(0, 11, 5));
        faces.Add(new Vector3Int(0, 5, 1));
        faces.Add(new Vector3Int(0, 1, 7));
        faces.Add(new Vector3Int(0, 7, 10));
        faces.Add(new Vector3Int(0, 10, 11));

        // 5 adjacent faces
        faces.Add(new Vector3Int(1, 5, 9));
        faces.Add(new Vector3Int(5, 11, 4));
        faces.Add(new Vector3Int(11, 10, 2));
        faces.Add(new Vector3Int(10, 7, 6));
        faces.Add(new Vector3Int(7, 1, 8));

        // 5 faces around point 3
        faces.Add(new Vector3Int(3, 9, 4));
        faces.Add(new Vector3Int(3, 4, 2));
        faces.Add(new Vector3Int(3, 2, 6));
        faces.Add(new Vector3Int(3, 6, 8));
        faces.Add(new Vector3Int(3, 8, 9));

        // 5 adjacent faces
        faces.Add(new Vector3Int(4, 9, 5));
        faces.Add(new Vector3Int(2, 4, 11));
        faces.Add(new Vector3Int(6, 2, 10));
        faces.Add(new Vector3Int(8, 6, 7));
        faces.Add(new Vector3Int(9, 8, 1));

        return faces;
    }

    public static void CreateIcosphere(ref List<Vector3> vertices, ref List<int> indices, ref List<Vector2> UV, int recursionLevel)
    {
        List<Vector3Int> faces = CreateIcosahedron(ref vertices);
        Dictionary<Int64, int> middlePointIndexCache = new Dictionary<long, int>();

        for (int i = 0; i < recursionLevel; i++)
        {
            var faces2 = new List<Vector3Int>();
            foreach (var tri in faces)
            {
                // replace triangle by 4 triangles
                int a = CreateIcoSphere_GetMiddlePoint(tri.x, tri.y, ref vertices, ref middlePointIndexCache);
                int b = CreateIcoSphere_GetMiddlePoint(tri.y, tri.z, ref vertices, ref middlePointIndexCache);
                int c = CreateIcoSphere_GetMiddlePoint(tri.z, tri.x, ref vertices, ref middlePointIndexCache);

                faces2.Add(new Vector3Int(tri.x, a, c));
                faces2.Add(new Vector3Int(tri.y, b, a));
                faces2.Add(new Vector3Int(tri.z, c, b));
                faces2.Add(new Vector3Int(a, b, c));
            }
            faces = faces2;
        }
        for (int i = 0; i < faces.Count; i++)
        {
            Debug.Log(faces[i]);
            indices.Add(faces[i].x);
            indices.Add(faces[i].y);
            indices.Add(faces[i].z);
        }
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z - 0.5f);
        }
        for (int i = 0; i < vertices.Count / 3; i++)
        {
            UV.Add(new Vector2(1, 0f));
            UV.Add(new Vector2(0, 0f));
            UV.Add(new Vector2(0, 1f));
        }
    }
    static int CreateIcoSphere_GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<Int64, int> middlePointIndexCache) //this feels pepega, fix this later
    {
        bool firstIsSmaller = p1 < p2;
        Int64 smallerIndex = firstIsSmaller ? p1 : p2;
        Int64 greaterIndex = firstIsSmaller ? p2 : p1;
        Int64 key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (middlePointIndexCache.TryGetValue(key, out ret))
        {
            return ret;
        }
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];

        Vector3 middle = new Vector3(
            (point1.x + point2.x) / 2.0f,
            (point1.y + point2.y) / 2.0f,
            (point1.z + point2.z) / 2.0f);

        // add vertex makes sure point is on unit sphere
        float length = middle.magnitude;
        Vector3 vector = middle.normalized / 2.0f;

        vertices.Add(middle.normalized / 2.0f);

        // store it, return index
        middlePointIndexCache.Add(key, vertices.Count - 1);

        return vertices.Count - 1;
    }

    public static void CreatePerianth(ref List<Vector3> vertices, ref List<int> indices, ref List<Vector2> UV, float startPoint, float height, int whorls, int merosity, float UVStart, float openness, Vector2 offset, AnimationCurve curve, AnimationCurve flowerShape, bool spread)
    {
        List<Vector3> positions = new List<Vector3>();
        float angle = 0;
        float angleIncrement = 360f / (merosity * 3f); //Time 3 since each petal stretches one step outwards on each side
        float currentHeight = startPoint;
        float heightIncrement = height / 2f;

        float amountOfVerticesVertical = 4;
        float amountOfQuadsVertical = amountOfVerticesVertical - 1;

        for (int i = 0; i < amountOfVerticesVertical; i++)
        {
            float radius = curve.Evaluate((float)i / (float)amountOfVerticesVertical);
            float petalValueSaved = 0;
            if (flowerShape != null && flowerShape.keys.Length > 0)
            {
                petalValueSaved = flowerShape.Evaluate((float)i / (float)amountOfVerticesVertical);
            }
            Vector2 normalizedDir = Vector2.zero;
            for (int j = 0; j < merosity * 3; j++) //Three steps around, times three because each petal stretches one step outwards on each side
            {

                float petalValue = j % 3 == 1 ? 0 : j % 3 == 0 ? -petalValueSaved : petalValueSaved;

                Vector3 position = new Vector3(radius * Mathf.Cos(angle * Mathf.Deg2Rad + petalValue) + offset.x, radius * Mathf.Sin(angle * Mathf.Deg2Rad + petalValue) + offset.y, -currentHeight);

                if (j % 3 == 0 && !spread)
                {
                    normalizedDir = new Vector3(radius * Mathf.Cos((angle + angleIncrement) * Mathf.Deg2Rad) + offset.x, radius * Mathf.Sin((angle + angleIncrement) * Mathf.Deg2Rad) + offset.y, -currentHeight);
                    normalizedDir.Normalize();
                }
                else if (spread)
                {
                    normalizedDir = position.normalized;
                }

                if (i > 0)
                {
                    GameObject temp = new GameObject();
                    Vector3 origin = new Vector3(positions[j].x, positions[j].y, -startPoint);

                    temp.transform.position = new Vector3(position.x, position.y, position.z); //Set to the vertex position
                    temp.transform.RotateAround(origin, new Vector2(normalizedDir.y, -normalizedDir.x), openness * 90); //Rotate vertex around

                    position = temp.transform.position;
                    Destroy(temp);
                }
                positions.Add(position);
                angle += angleIncrement;
                angle %= 360;
            }
            currentHeight += heightIncrement;
        }

        int startIndex = vertices.Count;

        int m = 0;
        for (int j = 0; j < positions.Count - merosity * 3; j++)
        {
            //I have to skip one quad
            if (j % 3 == 2) { continue; }
            vertices.Add(positions[0 + j]);
            vertices.Add(positions[(1 + j) % (merosity * 3) + (merosity * 3) * (int)((float)j / (merosity * 3))]);
            vertices.Add(positions[((merosity * 3 + 1) + j) % (merosity * 3) + (merosity * 3) * (int)((float)(j + (merosity * 3)) / (merosity * 3))]);
            vertices.Add(positions[merosity * 3 + j]);

            int[] indexValue = new int[] { 0, 3, 1, 1, 3, 2 };

            for (int index = 0; index < indexValue.Length; index++)
            {
                indices.Add(startIndex + indexValue[index] + m * 4);
            }
            m++;
        }
        for (int j = 0; j < amountOfQuadsVertical; j++)
        {
            for (int k = 0; k < merosity * 3; k++)
            {
                if (k % 3 == 0) { continue; }
                UV.Add(new Vector2(1, UVStart + (1f - UVStart) / amountOfQuadsVertical * j));     //1,0 + height / amountOfQuadsVertical * j
                UV.Add(new Vector2(0, UVStart + (1f - UVStart) / amountOfQuadsVertical * j));     //0,0
                UV.Add(new Vector2(0, UVStart + (1f - UVStart) / amountOfQuadsVertical * (j + 1))); //0,1
                UV.Add(new Vector2(1, UVStart + (1f - UVStart) / amountOfQuadsVertical * (j + 1))); //1,1
            }
        }
    }
    public static Mesh CreateRock()
    {
        Mesh mesh = new Mesh();
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        CreateIcosphere(ref newVertices, ref newTriangles, ref newUV, 1);

        //? flatten the first 40% of the icosphere
        for (int i = 0; i < newVertices.Count; i++)
        {
            newVertices[i] = new Vector3(newVertices[i].x + UnityEngine.Random.Range(0, 0.1f),
                                         newVertices[i].y + UnityEngine.Random.Range(0, 0.1f),
                                         newVertices[i].z > -0.4f ? 0 : newVertices[i].z + 0.4f + UnityEngine.Random.Range(0, 0.1f));
        }

        mesh.Init(newVertices, newTriangles, newUV);
        return mesh;
    }
    public static void CreateTuft(Mesh mesh, int quadsPerGrass, int amountOfStraws, float grassWidth)
    {
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        //Creates Grass
        //Later also Sedges, Rushes and Reeds (Different Graminoids)
        float angle = 0;
        float angle_increase = (360.0f / 1.618033f) * Mathf.Deg2Rad;
        float radius = 0.05f;
        float maxRadius = 0.3f;
        float radius_increase = (maxRadius - radius) / amountOfStraws;

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, 0);
        curve.AddKey(0.3f, 0.1f);
        curve.AddKey(1, 0.9f);

        for (int i = 0; i < amountOfStraws; i++)
        {
            float secondAngle = 1.5f * Mathf.Asin((grassWidth / 2) / (radius / Mathf.Sin(Mathf.Deg2Rad * 90))); //Law of Sines of half of the isosceles triangle to figure out the inner angle

            float height = 1.0f - curve.Evaluate(1.0f / amountOfStraws * i);// - UnityEngine.Random.Range(-0.5f, 0.5f);

            float horizontalityForThisStraw = 0.25f * UnityEngine.Random.Range(0, 5);

            for (int k = 0; k < quadsPerGrass; k++)
            {
                //Debug.Log(-(1.0f / height * k));
                //Go up the straw
                if(k == 0)
                {
                    newVertices.Add(new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle), -(height / quadsPerGrass * k)));
                    newVertices.Add(new Vector3(radius * Mathf.Sin(angle + secondAngle), radius * Mathf.Cos(angle + secondAngle), -(height / quadsPerGrass * k)));
                    newVertices.Add(new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle), -(height / quadsPerGrass * (k + 1))));
                    newVertices.Add(new Vector3(radius * Mathf.Sin(angle + secondAngle), radius * Mathf.Cos(angle + secondAngle), -(height / quadsPerGrass * (k + 1))));
                    

                    newUV.Add(new Vector2(horizontalityForThisStraw + 0.25f, 1.0f / quadsPerGrass * k));     //1,0
                    newUV.Add(new Vector2(horizontalityForThisStraw, 1.0f / quadsPerGrass * k));     //0,0
                    newUV.Add(new Vector2(horizontalityForThisStraw + 0.25f, 1.0f / quadsPerGrass * (k + 1))); //1,1
                    newUV.Add(new Vector2(horizontalityForThisStraw, 1.0f / quadsPerGrass * (k + 1))); //0,1
                }
                else
                {
                    newVertices.Add(new Vector3(radius * Mathf.Sin(angle), radius * Mathf.Cos(angle), -(height / quadsPerGrass * (k + 1))));
                    newVertices.Add(new Vector3(radius * Mathf.Sin(angle + secondAngle), radius * Mathf.Cos(angle + secondAngle), -(height / quadsPerGrass * (k + 1))));

                    newUV.Add(new Vector2(horizontalityForThisStraw + 0.25f, 1.0f / quadsPerGrass * (k + 1))); //1,1
                    newUV.Add(new Vector2(horizontalityForThisStraw, 1.0f / quadsPerGrass * (k + 1))); //0,1
                }

                int[] indexValue = new int[] { 0, 1, 3, 0, 3, 2 };

                int temp = newVertices.Count - 4;

                for (int index = 0; index < indexValue.Length; index++)
                {
                    newTriangles.Add(indexValue[index] + temp);
                }
            }
            //Slowly increase radius
            radius += radius_increase;
            angle += angle_increase;
        }

        //Debug.Log(newVertices.Count);

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }

    public static GameObject CreateVase(Material material)
    {
        GameObject vase = new GameObject("Vase");
        vase.gameObject.SetActive(false);
        vase.AddComponent<MeshFilter>();
        AnimationCurve curve = CreateVase_GetCurve();

        MeshMaker.OnCreateVase(vase.GetComponent<MeshFilter>().mesh, UnityEngine.Random.Range(0.8f, 2.0f), curve);
        vase.AddComponent<MeshRenderer>();

        Material vaseMaterial = new Material(material.shader);
        vaseMaterial.CopyPropertiesFromMaterial(material);
        vaseMaterial.color = Math.GetRandomSaturatedColor(0.8f);

        vase.GetComponent<MeshRenderer>().material = vaseMaterial;

        SphereCollider col = vase.AddComponent<SphereCollider>();
        col.radius = 0.5f;

        HealthModel health = vase.AddComponent<HealthModel>();
        health.deathSound = "pot_break";
        health.maxHealth = 1; health.currentHealth = 1;

        vase.AddComponent<DropItems>();
        if (vase.GetComponent<DropItems>())
        {
            vase.GetComponent<DropItems>().Initialize(UIManager.GetCurrency());
        }
        vase.AddComponent<AnimationCurveTest>();
        vase.GetComponent<AnimationCurveTest>().curve = curve;

        return vase;
    }
    public static AnimationCurve CreateVase_GetCurve()
    {

        AnimationCurve curve = new AnimationCurve();
        Keyframe temp = new Keyframe();
        //! POINT NUMBER 1
        temp.time = 0;
        temp.value = UnityEngine.Random.Range(0.2f, 0.4f);
        curve.AddKey(temp);

        //! POINT NUMBER 2
        temp.time = UnityEngine.Random.Range(0.25f, 0.75f); //Upper end makes a meiping ish shape, lower end makes a fat pot shape
        temp.value = UnityEngine.Random.Range(0.3f, 0.5f); //The fatest part of the vase, the radius. It determines how much space the vase takes up

        if (UnityEngine.Random.Range(0, 1) == 1) //Carinate
        {
            temp.outTangent = -1.5f;
        }
        curve.AddKey(temp);

        curve.SmoothTangents(0, 0);

        //! POINT NUMBER 3
        float upperRange = 1.0f - curve.keys[curve.keys.Length - 1].time - 0.1f; //-0.1f because it's not allowed to be 1. It can at most be 0.9f
        temp.time = curve.keys[curve.keys.Length - 1].time + UnityEngine.Random.Range(0.1f, upperRange);
        temp.value = 0.2f;
        curve.AddKey(temp);

        temp.time = 1;
        temp.value = UnityEngine.Random.Range(0.05f, 0.35f);
        curve.AddKey(temp);

        //! POINT NUMBER 4

        if (UnityEngine.Random.Range(0, 1) == 1) //Set straight neck
        {
            curve.SmoothTangents(2, 0);
        }
        curve.SmoothTangents(3, 0);

        return curve;
    }
    public static void OnCreateVase(Mesh mesh, float height, AnimationCurve curve)
    {
        //Create a vase from a sinewave controlling a radius
        List<Vector3> positions = new List<Vector3>();
        int amountOfQuadsVertical = 15;
        float angleIncrement = 360.0f / 10.0f;
        float currentHeight = 0;
        float heightIncrement = height / (float)amountOfQuadsVertical;

        CreateCylinder(ref positions, ref currentHeight, amountOfQuadsVertical, 10, heightIncrement, curve);

        currentHeight -= heightIncrement;

        for (int j = amountOfQuadsVertical - 1; j >= 0; j--)
        {
            //Go through all levels downwards
            float angle = 0;
            float currentRadius = curve.Evaluate((float)j / (float)amountOfQuadsVertical);

            for (int k = 9; k >= 0; k--)
            {
                //Go around the circle
                positions.Add(new Vector3(currentRadius * Mathf.Cos(angle * Mathf.Deg2Rad), currentRadius * Mathf.Sin(angle * Mathf.Deg2Rad), -currentHeight));
                angle += angleIncrement;
            }
            currentHeight -= heightIncrement;
        }
        // Debug.Log(positions.Count);

        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        for (int j = 0; j < positions.Count - 10; j++)
        {
            newVertices.Add(positions[0 + j]);
            newVertices.Add(positions[(1 + j) % 10 + 10 * (int)((float)j / 10.0f)]);
            newVertices.Add(positions[(11 + j) % 10 + 10 * (int)((float)(j + 10) / 10.0f)]);
            newVertices.Add(positions[10 + j]);

            newUV.Add(new Vector2(1, 0));                      //1,0
            newUV.Add(new Vector2(0, 0));                      //0,0
            newUV.Add(new Vector2(0, 1)); //0,1
            newUV.Add(new Vector2(1, 1)); //1,1

            int[] indexValue = new int[] { 0, 1, 3, 1, 2, 3 };

            for (int index = 0; index < indexValue.Length; index++)
            {
                //Debug.Log(indexValue[index] + j * 4);
                newTriangles.Add(indexValue[index] + j * 4);
            }
        }

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
    static public void CreateSurface(List<SurfaceData> positions, Transform trans, Dictionary<TileTemplate.TileType, Material> materials)
    {
        //Positions literally mean the position of every single quad on the grid
        List<Vector3> allVertices = new List<Vector3>();
        List<int> newIndices = new List<int>();
        List<Vector2> newUV = new List<Vector2>();
        Dictionary<Vector3Int, int> alreadyMadeVertices = new Dictionary<Vector3Int, int>();
        for(int i = 0; i < positions.Count; i++)
        {
            for (int j = 0; j < positions[i].tiles.Count; j++)
            {
                //I can't just add all of them. Since some of the quads will be on different altitudes. They won't all connect. It's not consistent
                Vector3 centerOfTile = new Vector3(positions[i].tiles[j].position.x + 0.5f, positions[i].tiles[j].position.y + 0.5f, -positions[i].tiles[j].position.z);
                if (positions[i].tiles[j].ceilingVertices.Count > 0)
                {
                    for (int k = 0; k < positions[i].tiles[j].ceilingVertices.Count; k += 2)
                    {
                        allVertices.Add(new Vector3(positions[i].tiles[j].ceilingVertices[k].x, positions[i].tiles[j].ceilingVertices[k].y, -positions[i].tiles[j].position.z));
                        allVertices.Add(new Vector3(positions[i].tiles[j].ceilingVertices[k + 1].x, positions[i].tiles[j].ceilingVertices[k + 1].y, -positions[i].tiles[j].position.z));
                        allVertices.Add(centerOfTile);
                        allVertices.Add(centerOfTile);

                        //3, 1, 0, 3, 2, 1
                        newIndices.Add(allVertices.Count - 1);
                        newIndices.Add(allVertices.Count - 3);
                        newIndices.Add(allVertices.Count - 4);
                        newIndices.Add(allVertices.Count - 1);
                        newIndices.Add(allVertices.Count - 2);
                        newIndices.Add(allVertices.Count - 3);

                        newUV.AddRange(Math.calcUV(allVertices).ToList());
                    }
                }
                else
                {
                    Vector3Int[] checkingPosition = new Vector3Int[4]
                    {
                    new Vector3Int(positions[i].tiles[j].position.x, positions[i].tiles[j].position.y, -positions[i].tiles[j].position.z),
                    new Vector3Int(positions[i].tiles[j].position.x + 1, positions[i].tiles[j].position.y, -positions[i].tiles[j].position.z),
                    new Vector3Int(positions[i].tiles[j].position.x + 1, positions[i].tiles[j].position.y + 1, -positions[i].tiles[j].position.z),
                    new Vector3Int(positions[i].tiles[j].position.x, positions[i].tiles[j].position.y + 1, -positions[i].tiles[j].position.z)
                    };

                    for (int k = 0; k < 4; k++)
                    {
                        CreateSurface_AddIfNotExist(ref alreadyMadeVertices, positions[i].tiles, ref allVertices, ref newUV, k, checkingPosition[k]);
                    }

                    int[] indices = new int[4];
                    for (int k = 0; k < 4; k++)
                    {
                        alreadyMadeVertices.TryGetValue(checkingPosition[k], out int index);
                        indices[k] = index;
                    }
                    //3,1,0,3,2,1
                    newIndices.Add(indices[3]);
                    newIndices.Add(indices[1]);
                    newIndices.Add(indices[0]);
                    newIndices.Add(indices[3]);
                    newIndices.Add(indices[2]);
                    newIndices.Add(indices[1]);
                }

                for (int k = 0; k < positions[i].tiles[j].sidesWhereThereIsWall.Count; k++)
                {
                    //Then this is a tile with connecting vertices, and it needs to be filled up
                    if (!positions[i].tiles[j].sidesWhereThereIsWall[k].floor)
                    {
                        allVertices.Add(new Vector3(centerOfTile.x - positions[i].tiles[j].sidesWhereThereIsWall[k].side.x * 0.5f + positions[i].tiles[j].sidesWhereThereIsWall[k].side.y * 0.5f,
                            centerOfTile.y + positions[i].tiles[j].sidesWhereThereIsWall[k].side.y * 0.5f + positions[i].tiles[j].sidesWhereThereIsWall[k].side.x * 0.5f, -positions[i].tiles[j].position.z));
                        allVertices.Add(new Vector3(centerOfTile.x - positions[i].tiles[j].sidesWhereThereIsWall[k].side.x * 0.5f - positions[i].tiles[j].sidesWhereThereIsWall[k].side.y * 0.5f,
                            centerOfTile.y + positions[i].tiles[j].sidesWhereThereIsWall[k].side.y * 0.5f - positions[i].tiles[j].sidesWhereThereIsWall[k].side.x * 0.5f, -positions[i].tiles[j].position.z));
                        allVertices.Add(centerOfTile);
                        allVertices.Add(centerOfTile);

                        //3, 1, 0, 3, 2, 1
                        newIndices.Add(allVertices.Count - 1);
                        newIndices.Add(allVertices.Count - 3);
                        newIndices.Add(allVertices.Count - 4);
                        newIndices.Add(allVertices.Count - 1);
                        newIndices.Add(allVertices.Count - 2);
                        newIndices.Add(allVertices.Count - 3);

                        newUV.AddRange(Math.calcUV(allVertices).ToList());

                        positions[i].tiles[j].sidesWhereThereIsWall.RemoveAt(k); k--;
                    }
                }

                if (allVertices.Count > 10000)
                {
                    GameObject floorObject = new GameObject("Floor");

                    floorObject.AddComponent<MeshFilter>();
                    floorObject.GetComponent<MeshFilter>().mesh.Init(allVertices.ToArray(), newIndices.ToArray(), newUV.ToArray());

                    floorObject.AddComponent<MeshRenderer>();
                    materials.TryGetValue(positions[i].tileType, out Material mat);
                    floorObject.GetComponent<MeshRenderer>().material = mat;

                    floorObject.isStatic = true;
                    floorObject.transform.parent = trans;

                    MeshCollider mc = floorObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = floorObject.GetComponent<MeshFilter>().mesh;

                    allVertices.Clear();
                    newIndices.Clear();
                    newUV.Clear();
                    alreadyMadeVertices.Clear();
                }
            }
            GameObject floorObject2 = new GameObject("Floor");

            floorObject2.AddComponent<MeshFilter>();
            floorObject2.GetComponent<MeshFilter>().mesh.Init(allVertices.ToArray(), newIndices.ToArray(), newUV.ToArray());

            floorObject2.AddComponent<MeshRenderer>();
            materials.TryGetValue(positions[i].tileType, out Material mat2);
            floorObject2.GetComponent<MeshRenderer>().material = mat2;

            floorObject2.isStatic = true;
            floorObject2.transform.parent = trans;

            MeshCollider mc2 = floorObject2.AddComponent<MeshCollider>();
            mc2.sharedMesh = floorObject2.GetComponent<MeshFilter>().mesh;

            allVertices.Clear();
            newIndices.Clear();
            newUV.Clear();
            alreadyMadeVertices.Clear();
        }
        
    }
    static void CreateSurface_AddIfNotExist(
        ref Dictionary<Vector3Int, int> alreadyMadeVertices, 
        List<SurfaceTileData> positions, 
        ref List<Vector3> allVertices,
        ref List<Vector2> UV,
        int i, 
        Vector3Int newPosition)
    {
        if (!alreadyMadeVertices.TryGetValue(newPosition, out _))
        {
            allVertices.Add(newPosition);
            alreadyMadeVertices.Add(newPosition, allVertices.Count - 1);
            UV.Add(new Vector2(newPosition.x, newPosition.y));
        }
    }
    /*static public void CreateSurface(List<SurfaceData> positions, Transform trans, Material floorMaterial)
    {
        //Things are left in here that hasn't been moved over yet
            if (positions[i].floorVertices.Count > 0 && positions[i].sidesWhereThereIsWall.Count > 1)
            {
                for (int j = 0; j < positions[i].floorVertices.Count; j += 2)
                {
                    //! we create a fan structure to the corner
                    Vector3 corner = (positions[i].sidesWhereThereIsWall[0].side - positions[i].sidesWhereThereIsWall[1].side).ToV3() + new Vector3(positions[i].position.x, positions[i].position.y, 0);
                    allVertices.Add(new Vector3(positions[i].floorVertices[j].x, positions[i].floorVertices[j].y, 0));
                    allVertices.Add(new Vector3(positions[i].floorVertices[j + 1].x, positions[i].floorVertices[j + 1].y, 0));
                    allVertices.Add(corner);
                    allVertices.Add(corner);

                    newUV.AddRange(Math.calcUV(allVertices).ToList());
                }
            }
            
        }
    }*/
    static public void CreateSurface(Mesh mesh, float height)
    {
        //When you are making a completely flat room without walls
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        for (int i = 0; i < 20 * 20; i++) //20 is the standard room size
        {
            newVertices.Add(new Vector3(i % 20, -i / 20 - 1, -height));
            newVertices.Add(new Vector3(i % 20 + 1, -i / 20 - 1, -height));
            newVertices.Add(new Vector3(i % 20 + 1, -i / 20, -height));
            newVertices.Add(new Vector3(i % 20, -i / 20, -height));

            newTriangles.Add(3 + 4 * i); //0, 1, 3, 1, 2, 3
            newTriangles.Add(1 + 4 * i);
            newTriangles.Add(0 + 4 * i);
            newTriangles.Add(3 + 4 * i);
            newTriangles.Add(2 + 4 * i);
            newTriangles.Add(1 + 4 * i);

            newUV.Add(new Vector2(1, 0));                      //1,0
            newUV.Add(new Vector2(0, 0));                      //0,0
            newUV.Add(new Vector2(0, 1)); //0,1
            newUV.Add(new Vector2(1, 1)); //1,1
        }

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    static public Mesh GetQuad()
    {
        Mesh mesh = new Mesh();
        List<int> newTriangles = new List<int>();
        List<Vector3> newVertices = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();

        newVertices.Add(new Vector3(1, 1, 0));
        newVertices.Add(new Vector3(0, 1, 0));
        newVertices.Add(new Vector3(0, 0, 0));
        newVertices.Add(new Vector3(1, 0, 0));

        newTriangles.Add(3);
        newTriangles.Add(1);
        newTriangles.Add(0);
        newTriangles.Add(3);
        newTriangles.Add(2);
        newTriangles.Add(1);

        newUV.Add(new Vector2(1, 0));                      //1,0
        newUV.Add(new Vector2(0, 0));                      //0,0
        newUV.Add(new Vector2(0, 1)); //0,1
        newUV.Add(new Vector2(1, 1));

        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
}