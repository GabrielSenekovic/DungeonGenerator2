using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using UnityEngine.SceneManagement;

public class CommandBox : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> commandLog = new List<string>();

    [SerializeField] InputField field;
    [SerializeField] LevelGenerator_Debugger levelGeneration_Debugger;
    [SerializeField] NPCGenerator_Debugger npcGeneration_Debugger;
    [SerializeField] EntityGenerator entityGenerator;

    public void OpenBox()
    {
        field.Select();
    }

    public void RunCommand()
    {
        RunCommand(field.text);
        field.text = "";
    }

    void RunCommand(string command)
    {
        string[] partsOfCommand = command.Split(' ');

        string append = ". Command succeded!";

        if(partsOfCommand.Length == 0){return;}

        switch(partsOfCommand[0])
        {
            case "AddCondition" : 
                if(partsOfCommand.Length == 1){return;}
                else if(partsOfCommand.Length == 2) //AddCondition <Name>
                {
                    Party.instance.GetPartyLeader().GetStatusConditionModel().AddCondition(new StatusConditionModel.StatusCondition((Condition)Enum.Parse(typeof(Condition), partsOfCommand[1])));
                }
                else if(partsOfCommand.Length == 3) //AddCondition <Name> <Duration>
                {
                    Party.instance.GetPartyLeader().GetStatusConditionModel().AddCondition(new StatusConditionModel.StatusCondition((Condition)Enum.Parse(typeof(Condition), partsOfCommand[1]), float.Parse(partsOfCommand[2], CultureInfo.InvariantCulture.NumberFormat)));
                }
            break;
            case "Build":
                if (partsOfCommand.Length == 1) { return; }
                if (partsOfCommand[1] == "House")
                {
                    levelGeneration_Debugger.GenerateHouse();
                }
            break;
            case "Buildmode" :
                FindObjectOfType<LevelManager>().SetPlacementRenderMode(LevelManager.PlacementRenderMode.BUILD);
            break;
            case "FillInventory" :
                if (partsOfCommand.Length == 1) { Party.instance.inventory.FillInventoryWithRandomItems(); }
                else
                {
                    if (partsOfCommand[1] == "Furniture")
                    {
                        FurnitureDatabase database = Resources.Load<FurnitureDatabase>("FurnitureDatabase");
                        TextAsset reader = Resources.Load<TextAsset>("FurnitureDatabase");
                        database.Initialise(reader.text);
                        Party.instance.inventory.FillInventoryWithFurniture();
                    }
                }
            break;
            case "Generate":
                if (partsOfCommand.Length == 1) { return; }
                int.TryParse(partsOfCommand[1], out int result);
                levelGeneration_Debugger.GenerateFromCommand(result);
                if(result == 100)
                {
                    npcGeneration_Debugger.GenerateProfessions();
                }
            break;
            case "Give" :
                if(partsOfCommand.Length == 1){return;}
                if (partsOfCommand[1] == "Key")
                {
                    Party.AddKey();
                    //Add a key to the counter
                }
                else if (partsOfCommand[1] == "Sword")
                {
                    Party.instance.inventory.AddSword();
                }
                else
                {
                    append = ". Command not found!";
                }
            break;
            case "Return" : UIManager.Instance.OpenSaveLocationNameBox();
            break;
            case "Scene" : 
                if(partsOfCommand.Length == 1){return;}
                switch(partsOfCommand[1])
                {
                    case "Debug": SceneManager.LoadScene("DEBUG", LoadSceneMode.Single);
                    break;
                    case "Flowers": SceneManager.LoadScene("FlowerTest", LoadSceneMode.Single);
                    break;
                    case "Level": SceneManager.LoadScene("Level", LoadSceneMode.Single);
                    break;
                    case "Projectiles": SceneManager.LoadScene("ProjectileTest", LoadSceneMode.Single);
                    break;
                }
            break;
            case "ShowAll" :
                if(partsOfCommand.Length == 1){return;}
                switch(partsOfCommand[1])
                {
                    case "Flowers": //Show all flowers
                    break;
                    case "Fruit": //Show all fruit
                    break;
                    default:
                        //Command failed
                        append = ". Command not found!";
                    break;
                }
            break;
            case "Sleep":
                DunGenes.Instance.GetDayNightCycle().Sleep();
            break;
            case "Spawn" :
                if(partsOfCommand.Length == 1){return;}
                switch(partsOfCommand[1])
                {
                    case "Entity":
                        if (partsOfCommand.Length == 2)
                        {
                            entityGenerator.SpawnRandomEntity();
                        }
                        else
                        {
                            switch(partsOfCommand[2])
                            {
                                case "Flying":
                                    entityGenerator.SpawnFlyingEntity();
                                    break;
                                case "Meelee":
                                    entityGenerator.SpawnMeeleeEntity();
                                    break;
                            }
                        }
                    break;
                }
            break;
            case "Toggle" :
                if(partsOfCommand.Length == 1){return;}
                switch(partsOfCommand[1])
                {
                    case "FPS": //Show FPS
                        UIManager.Instance.ToggleFPS();
                    break;
                    case "Grid": //Show tile grid
                        UnityEngine.Object[] objects = FindObjectsOfType<MeshRenderer>();
                        Texture texture = Resources.Load<Texture>("Art/Box_Cross");
                        for (int i = 0; i < objects.Length; i++)
                        {
                            if(objects[i].name == "Floor")
                            {
                                (objects[i] as MeshRenderer).material.SetTexture("_BaseMap", texture);
                            }
                        }
                    break;
                    case "Keys": //Show arrows from keys to the door it is meant to open
                    break;
                    case "PlacementGrid":
                        FindObjectOfType<LevelManager>().SetPlacementRenderMode(LevelManager.PlacementRenderMode.BUILD);
                    break;
                    case "PositionGrid":
                        FindObjectOfType<LevelManager>().SetPlacementRenderMode(LevelManager.PlacementRenderMode.POSITION);
                        break;
                    case "Sections": 
                        FindObjectOfType<LevelGenerator>().ToggleRenderSections();
                    //Show different colors for different sections of the dungeon
                    break;
                    default:
                        //Command failed
                        append = ". Command not found!";
                    break;
                }
            break;
            case "Vegetation":
                if (partsOfCommand.Length == 1) { return; }
                switch(partsOfCommand[1])
                {
                    case "ToggleRandom": //Toggles whether grass is on random positions or exactly in the middle of their tiles
                        MeshBatchRenderer.RenderRandomPositions = !MeshBatchRenderer.RenderRandomPositions;
                    break;
                    case "ToggleOff": //Toggles whether grass renders at all
                    break;
                }
            break;
            default: 
                //Command failed
                append = ". Command not found!";
            break;
        }
        command.Insert(0, DateTime.Now.ToString("h:mm:ss tt") + "| ");
        command += append;
        commandLog.Add(command);
    }
}
