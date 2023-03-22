using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
public partial class Room: MonoBehaviour
{
    [System.Serializable]
    public class Entrances
    {
        //This class holds and handles all entrances to a room
        [System.Serializable]
        public class Entrance
        {
            public enum EntranceType
            {
                NormalDoor = 0,
                PuzzleDoor = 1,
                BombableWall = 2,
                LockedDoor = 3,
                MultiLockedDoor = 4, //Uses more than one key
                AmbushDoor = 5 //Locks behind you, defeat all enemies to make them open
            }
            public bool open;
            public bool spawned;

            public List<Vector2Int> positions; //One vector for each position it is on
            public Vector2Int gridPos; //The room that the door is in
            public Vector2Int dir; //The direction this door is pointing

            public Vector2 index;
            EntranceType type;

            public Entrance(Vector2Int gridPos_in, Vector2Int dir_in)
            {
                gridPos = gridPos_in; dir = dir_in;
                index = new Vector2(9, 10); //This is the default
                open = false;
                spawned = false;
                positions = new List<Vector2Int>();
                type = EntranceType.NormalDoor;
            }

            public EntranceType GetEntranceType()
            {
                return type;
            }
            public void SetEntranceType(EntranceType type_in)
            {
                type = type_in;
            }
            public void SetOpen(bool value)
            {
                open = value;
            }
            public void Activate()
            {
                open = true;
                spawned = true;
            }

            public void Deactivate()
            {
                open = false;
                spawned = false;
            }
            public void Close() //when linking, in case you cant link rooms and got to acknowledge that theres a spawned room
            {
                open = false;
                spawned = true;
            }
        }
        public List<Entrance> entrances = new List<Entrance>();

        public Entrances(Vector2Int gridPosition, Vector2Int roomSize, Vector2Int movedPosition) //in gridspace, so a 40x40 is 2x2
        {
            DebugLog.AddToMessage("Substep", "Making entrances");
            Vector2Int absSize = new Vector2Int(Mathf.Abs(roomSize.x), Mathf.Abs(roomSize.y)); //If I don't use the absolute size for the position, then their positions arent alligned to the grid and the wall creation won't be able to find them
                                                                                               //To the positions, add the same y you added to move up the entire room
                                                                                               //int y_add = movedPosition.y - gridPosition.y;

            for (int x = 0; x < Mathf.Abs(roomSize.x); x++) //Adding north and south entrances
            {
                entrances.Add(new Entrance(gridPosition + new Vector2Int(x * (int)Mathf.Sign(roomSize.x), 0), new Vector2Int(0, -1 * (int)Mathf.Sign(roomSize.y))));
                if (Mathf.Sign(roomSize.y) == 1) //South
                {
                    if (Mathf.Sign(roomSize.x) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(10 + x * 20, absSize.y * 20 - 1));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(9 + x * 20, absSize.y * 20 - 1));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (9 + x * 20), absSize.y * 20 - 1));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (10 + x * 20), absSize.y * 20 - 1));
                    }
                }
                else //North
                {
                    if (Mathf.Sign(roomSize.x) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(9 + x * 20, 0));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(10 + x * 20, 0));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (10 + x * 20), 0));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (9 + x * 20), 0));
                    }
                }

                entrances.Add(new Entrance(gridPosition + new Vector2Int(x * (int)Mathf.Sign(roomSize.x), (absSize.y - 1) * (int)Mathf.Sign(roomSize.y)), new Vector2Int(0, 1 * (int)Mathf.Sign(roomSize.y))));
                if (Mathf.Sign(roomSize.y) == -1) //South
                {
                    if (Mathf.Sign(roomSize.x) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(10 + x * 20, absSize.y * 20 - 1));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(9 + x * 20, absSize.y * 20 - 1));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (9 + x * 20), absSize.y * 20 - 1));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (10 + x * 20), absSize.y * 20 - 1));
                    }
                }
                else //North
                {
                    if (Mathf.Sign(roomSize.x) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(9 + x * 20, 0));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(10 + x * 20, 0));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (10 + x * 20), 0));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int((absSize.x * 20 - 1) - (9 + x * 20), 0));
                    }
                }
            }
            for (int y = 0; y < Mathf.Abs(roomSize.y); y++) //Adding left and right entrances
            {
                entrances.Add(new Entrance(gridPosition + new Vector2Int((absSize.x - 1) * (int)Mathf.Sign(roomSize.x), y * (int)Mathf.Sign(roomSize.y)), new Vector2Int(1 * (int)Mathf.Sign(roomSize.x), 0)));
                if (Mathf.Sign(roomSize.x) == 1) //Right
                {
                    if (Mathf.Sign(roomSize.y) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, (absSize.y * 20 - 1) - (10 + y * 20)));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, (absSize.y * 20 - 1) - (9 + y * 20)));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, 9 + y * 20));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, 10 + y * 20));
                    }
                }
                else //Left
                {
                    if (Mathf.Sign(roomSize.y) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, (absSize.y * 20 - 1) - (9 + y * 20)));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, (absSize.y * 20 - 1) - (10 + y * 20)));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, 10 + y * 20));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, 9 + y * 20));
                    }
                }

                entrances.Add(new Entrance(gridPosition + new Vector2Int(0, y * (int)Mathf.Sign(roomSize.y)), new Vector2Int(-1 * (int)Mathf.Sign(roomSize.x), 0))); //Left
                if (Mathf.Sign(roomSize.x) == -1) //Right
                {
                    if (Mathf.Sign(roomSize.y) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, (absSize.y * 20 - 1) - (10 + y * 20)));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, (absSize.y * 20 - 1) - (9 + y * 20)));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, 9 + y * 20));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(absSize.x * 20 - 1, 10 + y * 20));
                    }
                }
                else //Left
                {
                    if (Mathf.Sign(roomSize.y) == 1)
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, (absSize.y * 20 - 1) - (9 + y * 20)));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, (absSize.y * 20 - 1) - (10 + y * 20)));
                    }
                    else
                    {
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, 10 + y * 20));
                        entrances[entrances.Count - 1].positions.Add(new Vector2Int(0, 9 + y * 20));
                    }
                }
            }
            /*for(int i = 0; i < entrances.Count; i++)
            {
                DebugLog.AddToMessage("Entrance", entrances[i].gridPos + " and " + entrances[i].dir);
            }*/
        }
        public void OpenAllEntrances()
        {
            entrances.ForEach(e => { e.SetOpen(true); });
        }
        public void ActivateAllEntrances()
        {
            entrances.ForEach(e => { e.Activate(); });
        }
        public Tuple<bool, Entrance> GetEntrance(Vector2Int gridPosition, Vector2Int direction)
        {
            DebugLog.AddToMessage("Substep", "Getting entrance in direction: " + direction + " from pos: " + gridPosition);
            //Debug.Log("Grid pos of this room: " + gridPosition + " looking for this direction: " + direction);
            for (int i = 0; i < entrances.Count; i++)
            {
                //DebugLog.AddToMessage("Entrance", "Position: " + entrances[i].gridPos + " Direction: " + entrances[i].dir);
                if (entrances[i].gridPos == gridPosition && entrances[i].dir == direction)
                {
                    return new Tuple<bool, Entrance>(true, entrances[i]);
                }
            }
            return new Tuple<bool, Entrance>(false, new Entrance(Vector2Int.zero, Vector2Int.zero));
        }
        public void SetEntranceVertices(ref RoomTemplate template, RoomTemplate originTemplate, Entrance entrance, Entrance originEntrance, Vector3 originRoomPosition, Vector3 destinationRoomPosition) //The template of this room and the template from the other room
        {
            Debug.Log("Setting entrance vertices");
            if (originEntrance.positions.Count == 0)
            {
                DebugLog.TerminateMessage("Origin entrance was empty of positions when setting entrance vertices!");
                return;
            }
            if (entrance.positions.Count == 0)
            {
                DebugLog.TerminateMessage("Entrance was empty of positions when setting entrance vertices!");
                return;
            }
            //template.SetEntranceTileVertices(originTemplate.GetEntranceTile(originEntrance.positions[1]).endVertices, entrance.positions[0], originRoomPosition, destinationRoomPosition, true); //Origin end to destination start

            Debug.Log("Setting origin start to destination end at position: " + entrance.positions[0] +
            " with a total of: ?? vertices at " + originEntrance.positions[1]);
            template.SetEntranceTileVertices(originTemplate.GetEntranceTile(originEntrance.positions[1]).startVertices, entrance.positions[0], originRoomPosition, destinationRoomPosition, false); //Origin start to destination end
            Debug.Log("Set origin start to destination end at position: " + entrance.positions[0] +
            " with a total of: " + originTemplate.GetEntranceTile(originEntrance.positions[1]).startVertices.Count + " vertices at " + originEntrance.positions[1]);
        }
    }
}