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

    Dictionary<string, GameObject> structures = new Dictionary<string, GameObject>();

    public RoomTemplateReader(RoomTemplate template, Transform roomTransform)
    {
        this.template = template;
        positions = template.positions;
        highestElevation = template.highestElevation;
        this.roomTransform = roomTransform;
    }
    public void CreateLevel(ref RoomTemplate template, Material floorMaterial_in, MaterialDatabase materialDatabase, SettlementData settlementData, Entrances directions = null)
    {
        Color color = new Color32((byte)UnityEngine.Random.Range(125, 220), (byte)UnityEngine.Random.Range(125, 220), (byte)UnityEngine.Random.Range(125, 220), 255);
        template.IdentifyWalls();
        Material floorMaterial = new Material(floorMaterial_in.shader);
        floorMaterial.CopyPropertiesFromMaterial(floorMaterial_in);
        if (template.indoors)
        {
            floorMaterial.color = color;
        }
        else
        {
            //floorMaterial.SetTexture("_BaseMap", Resources.Load<Texture>("Art/Earth"));
        }
        CreateWalls(template, directions, materialDatabase, settlementData);
        CreateFloor(template, floorMaterial);
    }
    void CreateWalls(RoomTemplate template, Entrances directions, MaterialDatabase materialDatabase, SettlementData settlementData)
    {
        DebugLog.AddToMessage("Substep", "Creating walls");
        List<Tuple<WallInstructions, bool>> data = ExtractWalls(directions); //These directions will be the ones that connect with other levels

        for (int i = 0; i < data.Count; i++)
        {
            GameObject wallObject;
            if (data[i].Item1.Count > 0)
            {
                if(!structures.ContainsKey(data[i].Item1.ID))
                {
                    GameObject newStructure = new GameObject(data[i].Item1.ID);
                    structures.Add(data[i].Item1.ID, newStructure);
                    newStructure.transform.parent = roomTransform;
                    if(settlementData == null)
                    {
                        settlementData = new SettlementData("Springfield");
                    }
                    settlementData.Add(newStructure);
                }
                wallObject = new GameObject(data[i].Item1.ID + " wall");
                structures.TryGetValue(data[i].Item1.ID, out GameObject structure);
                wallObject.transform.parent = structure.transform;
            }
            else
            {
                if (!structures.ContainsKey("ERROR"))
                {
                    GameObject newStructure = new GameObject("ERRORS");
                    structures.Add("ERROR", newStructure);
                    newStructure.transform.parent = roomTransform;
                }
                wallObject = new GameObject("ERROR");
                structures.TryGetValue("ERROR", out GameObject structure);
                wallObject.transform.parent = structure.transform;
            }
            MeshMaker.CreateWall(wallObject, materialDatabase, data[i].Item1, data[i].Item2, positions, template.roundedness);
            wallObject.transform.localPosition = new Vector3(-9.5f, 10, 0);
        }
    }
    void CreateFloor(RoomTemplate template, Material floorMaterial)
    {
        DebugLog.AddToMessage("Substep", "Creating floor");
        GameObject floorObject = new GameObject("Floor");
        floorObject.transform.parent = roomTransform;

        Dictionary<TileTemplate.TileType, Material> materials = new Dictionary<TileTemplate.TileType, Material>();
        materials.Add(TileTemplate.TileType.NONE, floorMaterial);
        Material houseMaterial = new Material(floorMaterial);
        houseMaterial.SetColor("_BaseColor", Color.blue);
        materials.Add(TileTemplate.TileType.HOUSE_FLOOR, houseMaterial);
        materials.Add(TileTemplate.TileType.HOUSE_WALL, houseMaterial);

        MeshMaker.CreateSurface(ExtractFloor(), floorObject.transform, materials);
        floorObject.transform.localPosition = new Vector3(-10, 10, 0);
    }
    public List<Tuple<WallInstructions, bool>> ExtractWalls(Entrances entrances)
    {
        List<Tuple<WallInstructions, bool>> data = new List<Tuple<WallInstructions, bool>>();
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
                        positions[x, y].tileType != TileTemplate.TileType.HOUSE_WALL &&
                        positions[x, y].read == TileTemplate.ReadValue.UNREAD &&
                        positions[x, y - 1].elevation > positions[x, y].elevation && 
                        positions[x, y].elevation <= currentElevation)
                    {
                        pos = new Vector2Int(x, -y);
                        currentAngle = 270;
                        OnExtractWalls(ref currentAngle, ref pos, ref data, currentElevation);
                    }

                    if (
                        positions[x, y].tileType == TileTemplate.TileType.HOUSE_WALL &&
                        positions[x, y].read == TileTemplate.ReadValue.UNREAD)
                    {
                        pos = new Vector2Int(x, -y);
                        currentAngle = 0;
                        OnExtractWalls(ref currentAngle, ref pos, ref data, currentElevation);
                    }
                }
            }
            currentElevation++;
            ResetReadValue(currentElevation);
        }
        ResetReadValue();
        return data;
    }
    void OnExtractWalls(ref int currentAngle, ref Vector2Int pos, ref List<Tuple<WallInstructions, bool>> data, int currentElevation)
    {
        //Find direction to follow
        string materialName = positions[pos.x, pos.y].tileType == TileTemplate.TileType.HOUSE_WALL ? "PaintedPlanks" : "Ground";
        string ID = positions[pos.x, pos.y].ID;
        Tuple<WallInstructions, bool> wall = new Tuple<WallInstructions, bool>(new WallInstructions(materialName, ID), false);
        bool wrap = false;
        HasWallNeighbor(pos, currentAngle, currentElevation, out bool hasWallNeighbor, out Vector2Int neighborDirection, out Vector2Int higherElevationDirection, out int angleToTurn, ref wrap); //Item2 is the direction to go to
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
            if (angleToTurn == -1 && positions[pos].tileType != TileTemplate.TileType.HOUSE_WALL)
            {
                pos = new Vector2Int(pos.x + neighborDirection.x, pos.y - neighborDirection.y);
            }
            ExtractWalls_GetSteps(ref pos, ref steps, neighborDirection, higherElevationDirection, currentAngle, currentElevation);

            int isThisWallFollowingOuterCorner = 0;
            if (angleToTurn < 0 && wall.Item1.Count > 0 && positions[pos].tileType != TileTemplate.TileType.HOUSE_WALL)
            {
                //If lastWall is less than 0, then this is the following wall after an outer corner, so it must be moved up and shortened
                isThisWallFollowingOuterCorner = 1;
            }
            HasWallNeighbor(pos, currentAngle, currentElevation, out hasWallNeighbor, out neighborDirection, out higherElevationDirection, out angleToTurn, ref wrap);

            if ((angleToTurn < 0 || wrap) && positions[pos].tileType != TileTemplate.TileType.HOUSE_WALL)
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
            float houseWallModifier = positions[pos.x, pos.y].tileType == TileTemplate.TileType.HOUSE_WALL ? 1: 0;
            

            if (currentAngle == 0)
            {
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f + isThisWallFollowingOuterCorner, startPosition.y - houseWallModifier, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, angleToTurn, positions[pos.x, pos.y].tileType));
            }
            if (currentAngle == 90)
            {
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x + 0.5f - houseWallModifier, startPosition.y - isThisWallFollowingOuterCorner, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, angleToTurn, positions[pos.x, pos.y].tileType));
            }
            if (currentAngle == 180)
            {
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - isThisWallFollowingOuterCorner + 0.5f, startPosition.y - 1 + houseWallModifier, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, angleToTurn, positions[pos.x, pos.y].tileType));
            }
            if (currentAngle == 270)
            {
                wall.Item1.Add(new MeshMaker.WallData(new Vector3(startPosition.x - 0.5f + houseWallModifier, startPosition.y - 1 + isThisWallFollowingOuterCorner, 0), startPosition,
                -currentAngle, steps, currentElevation, 0, positions[pos].divisions, curve, angleToTurn, positions[pos.x, pos.y].tileType));
            }
            //Sometimes it has to decrease by 90, so it has to know what direction the next wall goes in (fuck)
            currentAngle += 90 * angleToTurn; //This code can only do inner corners atm, not outer corners
            currentAngle = (int)Math.Mod(currentAngle, 360);
            if (positions[pos].read == TileTemplate.ReadValue.READFIRSTFINISHED ||
                positions[pos].read == TileTemplate.ReadValue.READFIRST)
            { break; }
        }
        // Debug.Log("There is this amount of walls: " + wall.Item1.Count);
        wall = new Tuple<WallInstructions, bool>(wall.Item1, wrap);
        data.Add(wall);
        //if(wall.Item1.Count == 0){DebugLog.WarningMessage("Couldn't create any walls");}
    }
    public void HasWallNeighbor(Vector2Int pos, int rotation, int currentElevation, out bool hasWallNeighbor,
        out Vector2Int neighborDirection, out Vector2Int higherElevationDirection, out int angleToTurn, ref bool wrap)
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

        Vector2Int directionToCheck = (Quaternion.Euler(0, 0, -rotation) * new Vector3(0, -1)).ToV2Int();
        Vector2Int checkForHigherElevation = (Quaternion.Euler(0, 0, -rotation + 90) * new Vector3(0, -1)).ToV2Int();
        if (positions[pos].tileType == TileTemplate.TileType.HOUSE_WALL)
        {
            if (positions.IsWithinBounds(new Vector2Int(pos.x + directionToCheck.x, pos.y + directionToCheck.y)) &&
                positions[pos.x + directionToCheck.x, pos.y + directionToCheck.y].tileType == positions[pos].tileType &&
                positions[pos.x + directionToCheck.x, pos.y + directionToCheck.y].wall)
            {
                direction = new Vector2Int(directionToCheck.x, -directionToCheck.y);
                rotationDir = 1;
            }
            else if (positions.IsWithinBounds(new Vector2Int(pos.x - directionToCheck.x, pos.y - directionToCheck.y)) &&
                positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].tileType == positions[pos].tileType &&
                positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].wall)
            {
                if (positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].read == TileTemplate.ReadValue.UNREAD)
                {
                    direction = new Vector2Int(-directionToCheck.x, directionToCheck.y);
                    rotationDir = -1;
                }
                else if (
                    positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].read == TileTemplate.ReadValue.READFIRST ||
                    positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].read == TileTemplate.ReadValue.READFIRSTFINISHED)
                {
                    value = false;
                    wrap = true;
                }
                else
                {
                    value = false;
                }
            }
            else
            {
                value = false;
            }
        }
        else
        {
            if (positions.IsWithinBounds(new Vector2Int(pos.x + directionToCheck.x, pos.y + directionToCheck.y)) &&
                positions[pos.x + directionToCheck.x, pos.y + directionToCheck.y].tileType == positions[pos].tileType &&
                positions[pos.x + directionToCheck.x, pos.y + directionToCheck.y].wall &&
                positions[pos.x + directionToCheck.x, pos.y + directionToCheck.y].elevation <= currentElevation &&
                positions.IsWithinBounds(new Vector2Int(pos.x + checkForHigherElevation.x, pos.y + checkForHigherElevation.y)) &&
                positions[pos.x + checkForHigherElevation.x, pos.y + checkForHigherElevation.y].elevation > currentElevation &&
                positions[pos.x + checkForHigherElevation.x, pos.y + checkForHigherElevation.y].elevation > positions[pos.x + directionToCheck.x, pos.y + directionToCheck.y].elevation
                )
            {
                direction = new Vector2Int(directionToCheck.x, -directionToCheck.y);
                rotationDir = 1;
                higherElevationDirection = checkForHigherElevation;
            }
            else if (positions.IsWithinBounds(new Vector2Int(pos.x - directionToCheck.x, pos.y - directionToCheck.y)) &&
                positions.IsWithinBounds(new Vector2Int(pos.x - directionToCheck.x - checkForHigherElevation.x, pos.y - directionToCheck.y - checkForHigherElevation.y)) &&
                positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].tileType == positions[pos].tileType &&
                positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].wall &&
                positions[pos.x - directionToCheck.x - checkForHigherElevation.x, pos.y - directionToCheck.y - checkForHigherElevation.y].elevation > currentElevation &&
                positions[pos.x - directionToCheck.x - checkForHigherElevation.x, pos.y - directionToCheck.y - checkForHigherElevation.y].elevation > positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].elevation
                )
            {
                if (positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].read == TileTemplate.ReadValue.UNREAD)
                {
                    direction = new Vector2Int(-directionToCheck.x, directionToCheck.y);
                    rotationDir = -1;
                    higherElevationDirection = -checkForHigherElevation;
                }
                else if (
                    positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].read == TileTemplate.ReadValue.READFIRST ||
                    positions[pos.x - directionToCheck.x, pos.y - directionToCheck.y].read == TileTemplate.ReadValue.READFIRSTFINISHED)
                {
                    value = false;
                    wrap = true;
                }
                else
                {
                    value = false;
                }
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

        //Reset the houses too
        positions.items.Where(i => currentElevation < i.elevation &&
        i.read != TileTemplate.ReadValue.FINISHED && i.tileType == TileTemplate.TileType.HOUSE_WALL
        ).ToList().ForEach(i => i.read = TileTemplate.ReadValue.UNREAD);
    }
    void ResetReadValue()
    {
        positions.items.ForEach(i => i.read = TileTemplate.ReadValue.UNREAD);
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
    }
    void ExtractWalls_GetSteps(ref Vector2Int pos, ref int steps, Vector2Int direction, Vector2Int directionOfHigherElevation, int currentAngle, int currentElevation)
    {
        /* Get Steps
         * 
         * This function is supposed to keep going in a given direction
         * Checking that each step is the same
         * 
         * */
        if (positions[pos].tileType == TileTemplate.TileType.HOUSE_WALL)
        {
            while (
            positions.IsWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y)) &&
            positions[pos.x + direction.x, pos.y - direction.y].wall &&
            positions[pos.x + direction.x, pos.y - direction.y].tileType == positions[pos.x, pos.y].tileType
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
                if (positions[pos.x, pos.y].elevation == currentElevation + 1)
                {
                    if (positions[pos].read == TileTemplate.ReadValue.READFIRST)
                    {
                        positions[pos].read = TileTemplate.ReadValue.READFIRSTFINISHED;
                    }
                    else
                    {
                        positions[pos.x, pos.y].read = TileTemplate.ReadValue.FINISHED;
                    }
                }
                else
                {
                    positions[pos.x, pos.y].read = TileTemplate.ReadValue.READ;
                }
            }
        }
        else
        {
            while (
            positions.IsWithinBounds(new Vector2Int(pos.x + direction.x, pos.y - direction.y)) &&
            positions[pos.x + direction.x, pos.y - direction.y].wall &&
            positions[pos.x + direction.x, pos.y - direction.y].elevation <= currentElevation &&
            positions[pos.x + directionOfHigherElevation.x, pos.y + directionOfHigherElevation.y].elevation > currentElevation &&
            positions[pos.x + direction.x, pos.y - direction.y].tileType == positions[pos.x, pos.y].tileType
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
                if (positions[pos.x + directionOfHigherElevation.x, pos.y + directionOfHigherElevation.y].elevation == currentElevation + 1)
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
    }
    public List<MeshMaker.SurfaceData> ExtractFloor()
    {
        List<MeshMaker.SurfaceData> returnData = new List<MeshMaker.SurfaceData>();
        for (int x = 0; x < template.size.x; x++)
        {
            for (int y = 0; y < template.size.y; y++)
            {
                int elevation = positions[x, y].tileType == TileTemplate.TileType.HOUSE_WALL ? 0 : positions[x, y].elevation;
                MeshMaker.SurfaceData surfaceData = returnData.FirstOrDefault(e => e.tileType == positions[x, y].tileType);
                if(surfaceData == null)
                {
                    surfaceData = new MeshMaker.SurfaceData(new List<MeshMaker.SurfaceTileData>(), positions[x, y].tileType);
                    returnData.Add(surfaceData);
                }
                surfaceData.tiles.Add(
                new MeshMaker.SurfaceTileData(
                    new Vector3Int(x, -y - 1, elevation), 
                    positions[x, y].ceilingVertices, 
                    positions[x, y].floorVertices, 
                    positions[x, y].divisions.x, 
                    positions[x, y].sidesWhereThereIsWall));
            }
        }
        return returnData;
    }
}
