using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using System.Linq;
using System;

using TileTemplate = Room.RoomTemplate.TileTemplate;
using Entrances = Room.Entrances;
using RoomTemplate = Room.RoomTemplate;
using Color = UnityEngine.Color;

public class RoomTemplateReader
{
    public RoomTemplate template;
    public Grid<TileTemplate> positions;
    public int highestElevation;
    public Transform roomTransform;

    public RoomTemplateReader(RoomTemplate template, Transform roomTransform)
    {
        this.template = template;
        positions = template.positions;
        highestElevation = template.highestElevation;
        this.roomTransform = roomTransform;
    }
    public void CreateRoom(ref RoomTemplate template, Material wallMaterial_in, Material floorMaterial_in, Entrances directions)
    {
        Color color = new Color32((byte)UnityEngine.Random.Range(125, 220), (byte)UnityEngine.Random.Range(125, 220), (byte)UnityEngine.Random.Range(125, 220), 255);
        Material wallMaterial = new Material(wallMaterial_in.shader);
        wallMaterial.CopyPropertiesFromMaterial(wallMaterial_in);
        Material floorMaterial = new Material(floorMaterial_in.shader);
        floorMaterial.CopyPropertiesFromMaterial(floorMaterial_in);
        if (template.indoors)
        {
            wallMaterial.mainTexture = floorMaterial_in.mainTexture;
            wallMaterial.color = color + Color.white / 10;
            floorMaterial.color = color;
        }
        else
        {
            floorMaterial.SetTexture("_BaseMap", Resources.Load<Texture>("Art/Earth"));
        }
        CreateWalls(template, wallMaterial, directions);
        CreateFloor(template, floorMaterial);
    }
    void CreateWalls(RoomTemplate template, Material wallMaterial, Entrances directions)
    {
        DebugLog.AddToMessage("Substep", "Creating walls");
        List<Tuple<List<MeshMaker.WallData>, bool>> data = ExtractWalls(directions); //These directions will be the ones that connect with other levels

        for(int i = 0; i < data.Count; i++)
        {
            GameObject wallObject = new GameObject("Wall");
            wallObject.transform.parent = roomTransform;
            MeshMaker.CreateWall(wallObject, wallMaterial, data[i].Item1, data[i].Item2, positions);
            wallObject.transform.localPosition = new Vector3(-9.5f, 10, 0);
        }
    }
    void CreateFloor(RoomTemplate template, Material floorMaterial)
    {
        DebugLog.AddToMessage("Substep", "Creating floor");
        GameObject floorObject = new GameObject("Floor");
        floorObject.transform.parent = roomTransform;

        MeshMaker.CreateSurface(ExtractFloor(), floorObject.transform, floorMaterial);
        floorObject.transform.localPosition = new Vector3(-10, 10, 0);
    }
    public List<Tuple<List<MeshMaker.WallData>, bool>> ExtractWalls(Entrances entrances)
    {
        List<Tuple<List<MeshMaker.WallData>, bool>> data = new List<Tuple<List<MeshMaker.WallData>, bool>>();
        Vector2Int pos = new Vector2Int(-1, -1);
        int currentAngle = 0;
        int currentElevation = 0;

        // else //If this is a closed room without doors
        while (currentElevation < highestElevation)
        {
            if (entrances != null && entrances.entrances.Count > 0)
            {
                for (int i = 0; i < entrances.entrances.Count; i++) //! makes the code only work when theres a door
                {
                    if (!entrances.entrances[i].open || !entrances.entrances[i].spawned) { continue; }
                    //Go through each entrance, and make a wall to its left. There will only ever be as many walls as there are entrances kappa
                    //Find a wall that has a floor next to it
                    pos = new Vector2Int(-1, -1);
                    currentAngle = 0; //Current angle should only be 0 if the floor found points down.

                    ExtractWalls_GetStartPosition(ref pos, ref currentAngle, entrances.entrances[i]);
                    OnExtractWalls(ref currentAngle, ref pos, ref data, currentElevation);
                }
            }

            for (int x = 0; x < template.size.x; x++)
            {
                for (int y = 0; y < template.size.y; y++)
                {
                    //Check if there is floor diagonally right down, because walls can only be drawn from left to right
                    //If there are none, rotate the search three times. If there still are none, then there is an error
                    if (positions.IsWithinBounds(new Vector2Int(x, -y + 1)) &&
                        positions[x, y].wall &&
                        positions[x, y].read == TileTemplate.ReadValue.UNREAD &&
                        positions[x, y - 1].elevation > positions[x, y].elevation && 
                        positions[x, y].elevation <= currentElevation)
                    {
                        pos = new Vector2Int(x, -y);
                        currentAngle = 270;
                        OnExtractWalls(ref currentAngle, ref pos, ref data, currentElevation);
                    }
                }
            }
            currentElevation++;
            ResetReadValue(currentElevation);
        }

        return data;
    }
    void OnExtractWalls(ref int currentAngle, ref Vector2Int pos, ref List<Tuple<List<MeshMaker.WallData>, bool>> data, int currentElevation)
    {
        //Find direction to follow
        Tuple<List<MeshMaker.WallData>, bool> wall = new Tuple<List<MeshMaker.WallData>, bool>(new List<MeshMaker.WallData>(), false);

        HasWallNeighbor(pos, currentAngle, currentElevation, out bool hasWallNeighbor, out Vector2Int neighborDirection, out Vector2Int higherElevationDirection, out int angleToTurn); //Item2 is the direction to go to
        Debug.Log("Did I find wall neighbor? " + hasWallNeighbor);
        currentAngle += 90 * angleToTurn;
        currentAngle = (int)Math.Mod(currentAngle, 360);
        positions[pos.x, pos.y].read = TileTemplate.ReadValue.READFIRST;
        Vector2Int startPosition = pos;

        int safety = 0;
        while (hasWallNeighbor) //If there is a wall neighbor, proceed
        {
            safety++;
            if (safety > 100)
            {
                Debug.Log("SAFETY YEET");
                return;
            }
            startPosition = pos;
            //Follow that direction until its empty
            int steps = 1;
            if (angleToTurn == -1)
            {
                pos = new Vector2Int(pos.x + neighborDirection.x, pos.y - neighborDirection.y);
            }
            ExtractWalls_GetSteps(ref pos, ref steps, neighborDirection, higherElevationDirection, currentAngle, currentElevation);

            int isThisWallFollowingOuterCorner = 0;
            if (angleToTurn < 0 && wall.Item1.Count > 0)
            {
                //If lastWall is less than 0, then this is the following wall after an outer corner, so it must be moved up and shortened
                isThisWallFollowingOuterCorner = 1;
            }
            HasWallNeighbor(pos, currentAngle, currentElevation, out hasWallNeighbor, out neighborDirection, out higherElevationDirection, out angleToTurn);

            float roundedness = 1;

            if (angleToTurn < 0)
            {
                //If Item3 is less than 0, then this is an outer corner, so the wall shouldn't go the whole way
                steps--;
            }

            //Debug.Log("<color=green>Amount of steps: </color>" + steps + " angle: " + currentAngle);
            //Only set wrap to true if the last wall ends up adjacent to the first wall

            //! make one wall curvy but none other
            AnimationCurve curve = new AnimationCurve();
            /*Keyframe temp = new Keyframe();
            temp.time = 0;
            temp.value = 0;
            temp.inTangent = 1;
            curve.AddKey(temp);

            temp.time = 0.5f;
            temp.value = 0.25f;
            temp.inTangent = 0;
            temp.outTangent = 0;

            curve.AddKey(temp);

            temp.time = 1;
            temp.value = 0;
            curve.AddKey(temp);*/
            //! END OF CURVE

            if (currentAngle == 0)
            {
                Debug.Log("adding 0 degree wall");
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f + isThisWallFollowingOuterCorner, startPosition.y, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, roundedness));
            }
            if (currentAngle == 90)
            {
                Debug.Log("adding 90 degree wall");
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x + 0.5f, startPosition.y - isThisWallFollowingOuterCorner, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, roundedness));
            }
            if (currentAngle == 180)
            {
                Debug.Log("adding 180 degree wall");
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - isThisWallFollowingOuterCorner + 0.5f, startPosition.y - 1, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, roundedness));
            }
            if (currentAngle == 270)
            {
                Debug.Log("adding 270 degree wall");
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f, startPosition.y - 1 + isThisWallFollowingOuterCorner, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, roundedness));
            }
            //Sometimes it has to decrease by 90, so it has to know what direction the next wall goes in (fuck)
            currentAngle += 90 * angleToTurn; //This code can only do inner corners atm, not outer corners
            currentAngle = (int)Math.Mod(currentAngle, 360);
            if (positions[pos].read == TileTemplate.ReadValue.READFIRSTFINISHED ||
                positions[pos].read == TileTemplate.ReadValue.READFIRST)
            { break; }
        }
        // Debug.Log("There is this amount of walls: " + wall.Item1.Count);
        wall = new Tuple<List<MeshMaker.WallData>, bool>(wall.Item1, ExtractWalls_DoesWallWrap(wall.Item1));
        data.Add(wall);
        //if(wall.Item1.Count == 0){DebugLog.WarningMessage("Couldn't create any walls");}
    }
    public void HasWallNeighbor(Vector2Int pos, int rotation, int currentElevation, out bool hasWallNeighbor,
        out Vector2Int neighborDirection, out Vector2Int higherElevationDirection, out int angleToTurn)
    {
        /*HasWallNeighbor
         * 
         * It checks first the angle of rotation
         * Then it checks both turning to right or left, if those positions are valid
         * And if those positions are unread and the correct elevation
         * Then the direction and rotation gets set and the method is exited
         * It is also used specifically for turning around
         * 
         * On an inner corner (1), don't check the position next to the next for a wall, rather, check the adjacent position of where you are
         * Othwerwise, if the corner is just one step, it will overshoot, and not make a wall
        */
        Vector2Int direction = Vector2Int.zero;
        bool value = true;
        int rotationDir = 0;
        higherElevationDirection = new Vector2Int(0, 0);
        if (rotation == 270)
        {
            bool bool1 = positions[pos.x + 1, pos.y].wall;
            bool bool2 = positions[pos.x + 1, pos.y].read == TileTemplate.ReadValue.UNREAD;
            bool bool3 = positions[pos.x, -pos.y - 1].elevation > currentElevation;
            bool bool4 = positions[pos.x, -pos.y - 1].elevation > positions[pos.x + 1, pos.y].elevation;
            if (positions.IsWithinBounds(new Vector2Int(pos.x + 1, pos.y)) &&
                positions[pos.x + 1, pos.y].wall &&
                positions[pos.x + 1, pos.y].elevation <= currentElevation &&
                positions[pos.x, -pos.y - 1].elevation > currentElevation &&
                positions[pos.x, -pos.y - 1].elevation > positions[pos.x + 1, pos.y].elevation
                )
            {
                direction = new Vector2Int(1, 0);
                rotationDir = 1;
                higherElevationDirection = new Vector2Int(0, -1);
            }
            else if (positions.IsWithinBounds(new Vector2Int(pos.x - 1, pos.y)) &&
                positions[pos.x - 1, pos.y].wall &&
                positions[pos.x - 1, pos.y].read == TileTemplate.ReadValue.UNREAD &&
                positions[pos.x - 1, pos.y - 1].elevation > currentElevation &&
                positions[pos.x - 1, pos.y - 1].elevation > positions[pos.x - 1, pos.y].elevation
                )
            {
                direction = new Vector2Int(-1, 0);
                rotationDir = -1;
                higherElevationDirection = new Vector2Int(0, 1);
            }
            else
            {
                value = false;
            }
        }
        else if (rotation == 90)
        {
            if (positions.IsWithinBounds(new Vector2Int(pos.x + 1, pos.y)) &&
                positions[pos.x + 1, -pos.y].wall &&
                positions[pos.x + 1, -pos.y].read == TileTemplate.ReadValue.UNREAD &&
                positions[pos.x + 1, -pos.y - 1].elevation > currentElevation &&
                positions[pos.x + 1, -pos.y - 1].elevation > positions[pos.x + 1, -pos.y].elevation
                )
            {
                direction = new Vector2Int(1, 0);
                rotationDir = -1;
                higherElevationDirection = new Vector2Int(0, -1);
            }
            else if (positions.IsWithinBounds(new Vector2Int(pos.x - 1, pos.y)) &&
                positions[pos.x - 1, pos.y].wall &&
                positions[pos.x - 1, pos.y].elevation <= currentElevation &&
                positions.IsWithinBounds(new Vector2Int(pos.x, pos.y - 1)) &&
                positions[pos.x, pos.y - 1].elevation > currentElevation &&
                positions[pos.x, pos.y - 1].elevation > positions[pos.x - 1, pos.y].elevation
                )
            {
                direction = new Vector2Int(-1, 0);
                rotationDir = 1;
                higherElevationDirection = new Vector2Int(0, 1);
            }
            else
            {
                value = false;
            }
        }
        else if (rotation == 180)
        {
            if (positions.IsWithinBounds(new Vector2Int(pos.x, pos.y + 1)) &&
                positions[pos.x, pos.y + 1].wall &&
                positions[pos.x, pos.y + 1].elevation <= currentElevation &&
                positions[pos.x - 1, pos.y].elevation > currentElevation &&
                positions[pos.x - 1, pos.y].elevation > positions[pos.x, pos.y + 1].elevation
                )
            {
                direction = new Vector2Int(0, -1);
                rotationDir = 1;
                higherElevationDirection = new Vector2Int(-1, 0);
            }
            else if (positions.IsWithinBounds(new Vector2Int(pos.x, pos.y - 1)) &&
                positions[pos.x, (-pos.y + 1)].wall &&
                positions[pos.x, (-pos.y + 1)].read == TileTemplate.ReadValue.UNREAD &&
                positions[pos.x + 1, (-pos.y + 1)].elevation > currentElevation &&
                positions[pos.x + 1, (-pos.y + 1)].elevation > positions[pos.x, (-pos.y - 1)].elevation
                )
            {
                direction = new Vector2Int(0, 1);
                rotationDir = -1;
                higherElevationDirection = new Vector2Int(1, 0);
            }
            else
            {
                value = false;
            }
        }
        else if (rotation == 0)
        {
            if (positions.IsWithinBounds(new Vector2Int(pos.x, pos.y + 1)) &&
                positions[pos.x, pos.y + 1].wall &&
                positions[pos.x, pos.y + 1].read == TileTemplate.ReadValue.UNREAD &&
                positions[pos.x - 1, pos.y + 1].elevation > currentElevation &&
                positions[pos.x - 1, pos.y + 1].elevation > positions[pos.x, pos.y + 1].elevation
                )
            {
                direction = new Vector2Int(0, -1);
                rotationDir = -1;
                higherElevationDirection = new Vector2Int(-1, 0);
            }
            else if (positions.IsWithinBounds(new Vector2Int(pos.x, pos.y - 1)) &&
                positions[pos.x, pos.y - 1].wall &&
                positions[pos.x, pos.y - 1].elevation <= currentElevation &&
                positions[pos.x + 1, pos.y].elevation > currentElevation &&
                positions[pos.x + 1, pos.y].elevation > positions[pos.x, pos.y].elevation
                )
            {
                direction = new Vector2Int(0, 1);
                rotationDir = 1;
                higherElevationDirection = new Vector2Int(1, 0);
            }
            else
            {
                value = false;
            }
        }
        if (!value)
        {
            positions[pos.x, pos.y].error = true;
        }
        hasWallNeighbor = value;
        neighborDirection = direction;
        angleToTurn = rotationDir;
    }
    void ResetReadValue(int currentElevation)
    {
        //Reset all elevations that are the same or lower than currentElevation
        positions.items.Where(i => i.elevation <= currentElevation &&
        i.read == TileTemplate.ReadValue.READFIRSTFINISHED
        ).ToList().ForEach(i => i.read = TileTemplate.ReadValue.FINISHED);

        positions.items.Where(i => i.elevation <= currentElevation &&
        i.read != TileTemplate.ReadValue.FINISHED
        ).ToList().ForEach(i => i.read = TileTemplate.ReadValue.UNREAD);
        //Do this between each elevation
    }
    bool ExtractWalls_DoesWallWrap(List<MeshMaker.WallData> data)
    {
        return true;
    }

    void ExtractWalls_GetStartPosition(ref Vector2Int pos, ref int currentAngle, Entrances.Entrance entrance)
    {
        /* Get Start Position
         * 
         * It takes an entrance and then attempts to make a wall from there
         * It gets the position to the left of the given entrance
         */

        if (entrance.dir == Vector2Int.up) //north
        {
            pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x, -entrance.positions[entrance.positions.Count - 1].y);
            currentAngle = 0;
        }
        if (entrance.dir == Vector2Int.down) //south
        {
            pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x, -entrance.positions[entrance.positions.Count - 1].y);
            currentAngle = 180;
        }
        if (entrance.dir == Vector2Int.right) //right
        {
            pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x, -entrance.positions[entrance.positions.Count - 1].y);
            currentAngle = 90;
        }
        if (entrance.dir == Vector2Int.left) //left
        {
            pos = new Vector2Int(entrance.positions[entrance.positions.Count - 1].x, -entrance.positions[entrance.positions.Count - 1].y);
            currentAngle = 270;
        }



        /*for(int x = 0; x < size.x; x++)
         {
             for(int y = 0; y < size.y; y++)
             {
                 //TODO Wall reading should actually start at the first found door!
                 //TODO Do this later when the doors have been changed for big rooms

                 //Check if there is floor diagonally right down, because walls can only be drawn from left to right
                 //If there are none, rotate the search three times. If there still are none, then there is an error
                 if(IsPositionWithinBounds(new Vector2Int(x + 1, -y - 1)) && (x < size.x && positions[x + 1 + size.x * (y + 1)].identity == 2 || x < size.x && positions[x + 1 + size.x * (y + 1)].identity == 3))
                 //2 is floor, 3 is door but door also counts as floor
                 {
                     pos = new Vector2Int(x, -y); 
                     currentAngle = 90;
                     break; 
                 }
                     //if none of the directions are a floor, then it is a void
                 // template.positions[x + template.size.x * y].SetIdentity(0);
             }
             if(pos != new Vector2Int(-1,-1))
             {
                 break;
             }
         }*/
        //Debug.Log("The position found to start from, that has a floor next to it: " + pos);
    }
    void ExtractWalls_GetSteps(ref Vector2Int pos, ref int steps, Vector2Int direction, Vector2Int directionOfHigherElevation, int currentAngle, int currentElevation)
    {
        /* Get Steps
         * 
         * This function is supposed to keep going in a given direction
         * Checking that each step is the same
         * 
         * */
        while (
            positions.IsWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y)) &&
            positions[pos.x + direction.x, pos.y - direction.y].wall &&
            positions[pos.x + direction.x, pos.y - direction.y].elevation <= currentElevation &&
            positions[pos.x + directionOfHigherElevation.x, pos.y - directionOfHigherElevation.y].elevation > currentElevation
            )
        {
            steps++;
            pos = new Vector2Int(pos.x + direction.x, pos.y - direction.y);
            // Debug.Log("Checking index: " + (pos.x + size.x * -pos.y));
            if (positions[pos].read == TileTemplate.ReadValue.READFIRSTFINISHED ||
                positions[pos].read == TileTemplate.ReadValue.READFIRST)
            {
                break;
            }
            if (positions[pos.x + directionOfHigherElevation.x, -pos.y + directionOfHigherElevation.y].elevation == currentElevation + 1)
            {
                if (positions[pos].read == TileTemplate.ReadValue.READFIRST)
                {
                    positions[pos].read = TileTemplate.ReadValue.READFIRSTFINISHED;
                }
                else
                {
                    positions[pos.x, -pos.y].read = TileTemplate.ReadValue.FINISHED;
                }
            }
            else
            {
                positions[pos.x, -pos.y].read = TileTemplate.ReadValue.READ;
            }
        }
    }
    public List<MeshMaker.SurfaceData> ExtractFloor()
    {
        List<MeshMaker.SurfaceData> returnData = new List<MeshMaker.SurfaceData>();
        for (int x = 0; x < template.size.x; x++)
        {
            for (int y = 0; y < template.size.y; y++)
            {
                //int elevation = positions[x, y].wall ? 0 : positions[x, y].elevation;
                int elevation = positions[x, y].elevation;
                returnData.Add(new MeshMaker.SurfaceData(new Vector3Int(x, -y - 1, elevation), positions[x, y].ceilingVertices, positions[x, y].floorVertices, positions[x, y].divisions.x, positions[x, y].sidesWhereThereIsWall));
            }
        }
        return returnData;
    }
}
