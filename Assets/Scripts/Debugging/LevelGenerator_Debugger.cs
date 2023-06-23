using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator_Debugger : MonoBehaviour
{
    [SerializeField] LevelGenerator generator;
    [SerializeField] LevelManager levelManager;
    public void GenerateFromCommand(int value)
    {
        string outsideInstructions = "";
        string houseInstructions = "";
        Vector2Int? size = null;
        switch (value)
        {
            case 0: break;
            case 1: break;
            case 2: outsideInstructions = "S[4,2], R[100]"; break;//Test inner corner
            case 3: outsideInstructions = "S[2,1], C[4,1], R[100]"; break; //Test both inner and outer corner, without doors
            case 4: outsideInstructions = "S[4,1], S[2,2]"; break; //Test multiple altitudes, with 2 steps apart
            case 5: outsideInstructions = "S[3,2], S[2,4]"; break; //Test multiple altitudes, with 1 step apart
            case 6: outsideInstructions = "S[4,1], C[4,2], S[1,3]"; break; //Test multiple altitudes, with overlap
            case 7: outsideInstructions = "S[2,1], C[2,2], S[1,3]"; break; //Test multiple altitudes, with overlap, with 1 step apart
            case 8: outsideInstructions = "W[8,1]"; break; //Test circle
            case 9: outsideInstructions = "S[1,4], W[8,1], W[9,2], W[10,3], W[11,4]"; break; //Test multiple altitudes, with circles
            case 10: outsideInstructions = "P[2,4], R[100]"; break; //Test 2x2 pillars
            case 11: outsideInstructions = "P[1,4], R[100]"; break; //Test 1x1 pillars
            case 12: break; //Test enclosed slope square with inner corner slope
            case 13: break; //Test outer corner slope
            case 14: break; //Test steep incline. Should be unwalkable
            case 15: break; //Test unsteep incline. Should be walkable
            case 16: break; //Test multiple inclines (One slope more steep than the other, next to eachother)
            case 17: houseInstructions = "P[2,2], D[6,6]"; break; //Test to build a house by itself
            case 18: outsideInstructions = "S[4,2]"; houseInstructions = "P[2,2], D[6,6]"; break; //Test to build a house in an environment
                //Special
                //Room for testing NPC professions
            case 100: outsideInstructions = "S[1,2]"; size = new Vector2Int(40, 40);
                break;
            default: return;
        }
        generator.OnGenerateOneRoom(true, levelManager.settlementData,outsideInstructions, houseInstructions, size);
    }
    public void GenerateHouse()
    {
        generator.GenerateHouse("D[4,4]", levelManager.settlementData);
    }
}
