using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Math : MonoBehaviour
{
    public struct Line
    {
        public Vector2 start;
        public Vector2 end;
        public Line(Vector2 start_in, Vector2 end_in)
        {
            start = start_in;
            end = end_in;
        }
    }
    public struct Box
    {
        public Vector2[] corners;
        public Box(Vector2[] corners_in)
        {
            corners = corners_in;
        }
    }
    public static Vector2Int[] diagonals = new Vector2Int[4]{
            new Vector2Int( -1, 1 ),
            new Vector2Int( 1, 1 ),
            new Vector2Int( 1, -1 ),
            new Vector2Int( -1, -1 ) };
    public static Vector2Int[] directions = new Vector2Int[4]{
            new Vector2Int(-1, 0),
            new Vector2Int( 1, 0),
            new Vector2Int( 0, -1 ),
            new Vector2Int( 0, 1 ) };
    public static Vector2Int[] XY = new Vector2Int[2]{
            new Vector2Int(1, 0),
            new Vector2Int(0, 1)};
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
        //A superellipse is a shape
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
    public static bool IntersectBoxLine(Box box, Line line)
    {
        Line line1 = new Line(box.corners[0], box.corners[1]);
        Line line2 = new Line(box.corners[1], box.corners[2]);
        Line line3 = new Line(box.corners[2], box.corners[3]);
        Line line4 = new Line(box.corners[3], box.corners[0]);

        if(IntersectLineLine(line1, line) || IntersectLineLine(line2, line) || IntersectLineLine(line3, line) || IntersectLineLine(line4, line))
        {
            return true;
        }
        return IntersectBoxPoint(box, line.start) && IntersectBoxPoint(box, line.end);
    }
    public static bool IntersectBoxPoint(Box box, Vector2 point)
    {
        Vector2 perp1 = Vector2.Perpendicular((box.corners[0] - box.corners[1]).normalized);
        Vector2 perp2 = Vector2.Perpendicular((box.corners[1] - box.corners[2]).normalized);
        Vector2 perp3 = Vector2.Perpendicular((box.corners[2] - box.corners[3]).normalized);
        Vector2 perp4 = Vector2.Perpendicular((box.corners[3] - box.corners[0]).normalized);

        float dot1 = Vector2.Dot(point, perp1);
        float dot2 = Vector2.Dot(point, perp2);
        float dot3 = Vector2.Dot(point, perp3);
        float dot4 = Vector2.Dot(point, perp4);

        return (dot1 <= 0 && dot2 <= 0 && dot3 <= 0 && dot4 <= 0);
    }
    public static bool IntersectLineLine(Line a, Line b)
    {
        float aDistX = a.end.x - a.start.x;

        float bDistX = b.end.x - b.start.x;

        float aDistY = a.end.y - a.start.y;

        float bDistY = b.end.y - b.start.y;

        if((aDistX * bDistY - bDistX * aDistY) == 0)
        {
            if(IntersectLinePoint(b, a.start)||IntersectLinePoint(b, a.end))
            { return true; }
            if (IntersectLinePoint(a, b.start) || IntersectLinePoint(a, b.end))
            { return true; }
        }
        else
        {
            float distStartA = (-a.start.x * aDistY + aDistX * a.start.y - aDistX * b.start.y + b.start.x * aDistY) / (aDistX * bDistY - bDistX * aDistY);
            float distStartB = (-a.start.x - bDistY + b.start.y * bDistY + bDistX * a.start.y - bDistX * b.start.y) / (aDistX * bDistY - bDistX * aDistY);
            return distStartA >= 0 && distStartA <= 1 && distStartB >= 0 && distStartB <= 1;
        }
        return false;
    }
    public static bool IntersectLinePoint(Line line, Vector2 point)
    {
        if(point.x >= line.start.x && point.x <= line.end.x)
        {
            float k = (line.end.y - line.start.y) / (line.end.x - line.start.x);
            return point.y == (point.x - line.start.x) * k + line.start.y;
        }
        return false;
    }
    public static bool IsWithinFrustum(Vector3 position, Vector2 size)
    {
        //The position has to be the center point!!
        //Camera counts as an AABB and the room corners as an OBB

        //Start by checking if the center position of the room is so far away that they couldn't conceivably collide . That's like a circle collision
      /*  Vector2 cameraPoint = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2));
        float distance = Vector2.Distance(position, cameraPoint);
        float maxDistance = size.magnitude * 2;

        if (distance > maxDistance)
        { //If the distance to the room is so far away they couldn't collide, don't do the rest of the calculations
            return false;
        }*/
        return true;

        //Check this code over later. It doesn't seem to completely work. Sometimes theres blinking. This is adapted from an earlier course so it should work. I must have miswritten something somewhere
        /* Vector2 size = new Vector2(Mathf.Abs(size_in.x), -Mathf.Abs(size_in.y));

         Vector2 northPoint = Camera.main.WorldToScreenPoint((new Vector3(position.x - size.x, position.y - size.y) - position) + position);
         Vector2 southPoint = Camera.main.WorldToScreenPoint((new Vector3(position.x + size.x, position.y + size.y) - position) + position);
         Vector2 leftPoint = Camera.main.WorldToScreenPoint((new Vector3(position.x - size.x, position.y + size.y) - position) + position);
         Vector2 rightPoint = Camera.main.WorldToScreenPoint((new Vector3(position.x + size.x, position.y - size.y) - position) + position);

         Box box = new Box(new Vector2[4] { northPoint, leftPoint, southPoint, rightPoint });
         Box camera = new Box(new Vector2[4] { new Vector2(0, 0), new Vector2(0, Camera.main.pixelHeight), new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight), new Vector2(Camera.main.pixelWidth, 0) });

         Line line1 = new Line(northPoint, leftPoint);
         Line line2 = new Line(leftPoint, southPoint);
         Line line3 = new Line(southPoint, rightPoint);
         Line line4 = new Line(rightPoint, northPoint);

         Line cameraEdge1 = new Line(new Vector2(0, 0), new Vector2(0, Camera.main.pixelHeight));
         Line cameraEdge2 = new Line(new Vector2(0, Camera.main.pixelHeight), new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight));
         Line cameraEdge3 = new Line(new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight), new Vector2(Camera.main.pixelWidth, 0));
         Line cameraEdge4 = new Line(new Vector2(Camera.main.pixelWidth, 0), new Vector2(0, 0));

         if(IntersectLineLine(line1, cameraEdge1) || IntersectLineLine(line1, cameraEdge2) || IntersectLineLine(line1, cameraEdge3) || IntersectLineLine(line1, cameraEdge4) ||
            IntersectLineLine(line2, cameraEdge1) || IntersectLineLine(line2, cameraEdge2) || IntersectLineLine(line2, cameraEdge3) || IntersectLineLine(line2, cameraEdge4) ||
            IntersectLineLine(line3, cameraEdge1) || IntersectLineLine(line3, cameraEdge2) || IntersectLineLine(line3, cameraEdge3) || IntersectLineLine(line3, cameraEdge4) ||
            IntersectLineLine(line4, cameraEdge1) || IntersectLineLine(line4, cameraEdge2) || IntersectLineLine(line4, cameraEdge3) || IntersectLineLine(line4, cameraEdge4))
         {
             return true;
         }
         else if(IntersectBoxPoint(box, camera.corners[0]) && IntersectBoxPoint(box, camera.corners[1]) && IntersectBoxPoint(box, camera.corners[2]) && IntersectBoxPoint(box, camera.corners[3]))
         {
             return true;
         }
         else if(IntersectBoxLine(camera, line1) && IntersectBoxLine(camera, line2) && IntersectBoxLine(camera, line3) && IntersectBoxLine(camera, line4))
         {
             return true;
         }
         else
         {
             return false;
         }*/
    }
    public static bool IsWithinFrustumRotated(Vector3 position, float radius)
    {
        //The position has to be the center point!!
        Quaternion temp = Quaternion.Euler(0, 0, -CameraMovement.rotationSideways); //This is used to rotate the position around

        Vector2 northPoint = Camera.main.WorldToScreenPoint(temp * (new Vector3(position.x - radius, position.y - radius) - position) + position);
        Vector2 southPoint = Camera.main.WorldToScreenPoint(temp * (new Vector3(position.x + radius, position.y + radius) - position) + position);
        Vector2 leftPoint = Camera.main.WorldToScreenPoint(temp * (new Vector3(position.x - radius, position.y + radius) - position) + position);
        Vector2 rightPoint = Camera.main.WorldToScreenPoint(temp * (new Vector3(position.x + radius, position.y - radius) - position) + position);


        //A quick way to determine whether an object is within a 3 dimensional field
        return (leftPoint.x > 0 && leftPoint.x < Camera.main.pixelWidth && leftPoint.y > 0 && leftPoint.y < Camera.main.pixelHeight) ||
            (rightPoint.x > 0 && rightPoint.x < Camera.main.pixelWidth && rightPoint.y > 0 && rightPoint.y < Camera.main.pixelHeight) ||
            (northPoint.y > 0 && northPoint.y < Camera.main.pixelHeight && northPoint.x > 0 && northPoint.x < Camera.main.pixelWidth) ||
            (southPoint.y > 0 && southPoint.y < Camera.main.pixelHeight && southPoint.x > 0 && southPoint.x < Camera.main.pixelWidth);
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
