using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Math : MonoBehaviour
{
    public static Vector2Int[] directions = new Vector2Int[4]{
            new Vector2Int( -1, 1 ),
            new Vector2Int( 1, 1 ),
            new Vector2Int( 1, -1 ),
            new Vector2Int( -1, -1 ) };
    //1/limit * x^exponent / limit^exponent
    //The x^exponent determines the curve
    //The limit^exponent only brings it down so that when x is limit, y is one
    // float s = interval * Mathf.Pow(i, exponent) / Mathf.Pow(6, exponent-1) *-1 +1;
    public static float SemiCircle(float x, float limit, float curve)
    {
        return Mathf.Sqrt(Mathf.Pow(limit, curve) - Mathf.Pow(x, curve)) / limit;
    }
    public static float SemiSuperEllipse(float x, float interval, float limit, float curve)
    {
        return 1 - interval * Mathf.Pow(x, curve) / Mathf.Pow(limit, curve - 1);
    }
    public static float Mod(float a, float b)
    {
        return a - b * Mathf.Floor(a / b);
    }
    public static Color GetRandomColor()
    {
        return new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
    }
    public static Color GetRandomSaturatedColor(float saturation)
    {
        float h = Random.Range(0.0f, 1.0f), s = saturation, v = 1;
        return Color.HSVToRGB(h, s, v);
    }

    public static Vector3 Transition(Vector3 currentPosition, Vector3 destination, Vector3 origin, float speed)
    {
        Vector3 movementVector = (destination - origin).normalized * speed;
        currentPosition = //Check that the distance to the walkingPosition isn't bigger than the distance to the destination
		Mathf.Abs(((currentPosition + movementVector) - origin).x) >= Mathf.Abs((destination - origin).x) &&
		Mathf.Abs(((currentPosition + movementVector) - origin).y) >= Mathf.Abs((destination - origin).y) ?
		destination : currentPosition + movementVector;

        return currentPosition;
    }
    public static bool Compare(float a, float b)
    {
        if(a < 0)
        {
            return Mathf.FloorToInt(a) == Mathf.FloorToInt(b);
        }
        else if(a >= 0)
        {
            return (int)a == (int)b;
        }
        return false;
    }
    public static float ConvertCentimetersToPixels(int cm)
    {
        //if 3 tiles is one meter, then 1 tile is 1/3 * 100 centimeters
        //and 1cm is (3 * 16) / 100d since there are 16 pixels per tile
        double unit = 1d/32d;
        //(3d * 16) / 100d = 0.03 which is roughly 1/32 = 0.03125
        return (float)(unit * cm); 
    }
    public static int[] GetValidConstraints(int i, int range, Vector2Int grid)
    {
        int targY = (i / grid.x);
        int startY = (targY - range) % grid.y; //! POS - 1 
        int targX = i % grid.x;
        if (startY < 0) { startY = 0; }
        int startX = (targX - range) % grid.x; //! POS - 1
        if (startX < 0) { startX = 0; }
        int yLimit = targY + range + 1;
        int xLimit = targX + range + 1;

        if (xLimit > grid.x) { xLimit = grid.x; }
        if (yLimit > grid.y) { yLimit = grid.y; }

        return new int[4]{ startX, startY, xLimit, yLimit };
    }
    public static Vector2Int GetSideOfTile(Vector2 vertex, Vector2 center)
    {
        float xUsed = -Vector3.Dot(Vector3.right, vertex - center);
        float yUsed = Vector3.Dot(Vector3.up, vertex - center);
        xUsed = Mathf.Abs(yUsed) > Mathf.Abs(xUsed) ? 0 : Mathf.Ceil(Mathf.Abs(xUsed)) * Mathf.Sign(xUsed);
        yUsed = Mathf.Abs(xUsed) > Mathf.Abs(yUsed) ? 0 : Mathf.Ceil(Mathf.Abs(yUsed)) * Mathf.Sign(yUsed);
        return new Vector2Int((int)xUsed, (int)yUsed);
    }
    public static Vector2[] calcUV(List<Vector3> vertices)
    {
        /*A normal tile has 
        1,0 - 0
        0,0 - 1
        0,1 - 2
        1,1 - 3
        */
        //!For the last 4 vertices in the vertices list, find how many percent into the tile the positions are. 
        //!The corners of the tiles are whole integers. So I only need the floating point to know the percentage
        //!This will be used to get the UV position for that triangle

        Vector2[] UV = new Vector2[4];
        Vector2Int lowestValues = vertices[vertices.Count - 3].ToV2Int(); //Being the lower right corner, it is the lowest value to both coordinates

        for(int i = vertices.Count - 4; i < vertices.Count; i++)
        {
            Vector2 uvVector = new Vector2((vertices[i].x - (int)lowestValues.x), (vertices[i].y - (int)lowestValues.y));
            UV[i - vertices.Count + 4] = uvVector;
        }

        return UV;
    }
}
