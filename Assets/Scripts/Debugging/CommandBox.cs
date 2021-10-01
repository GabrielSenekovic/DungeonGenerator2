using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;

public class CommandBox : MonoBehaviour
{
    // Start is called before the first frame update
    public List<string> commandLog = new List<string>();

    public InputField field;

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
            case "Buildmode" :
                GameObject.FindObjectOfType<LevelManager>().ToggleBuildMode();
            break;
            case "FillInventory" :
                Party.instance.inventory.FillInventoryWithRandomItems();
            break;
            case "Give" :
                if(partsOfCommand.Length == 1){return;}
                if(partsOfCommand[1] == "Key")
                {
                    Party.AddKey();
                    //Add a key to the counter
                }
                else
                {
                    append = ". Command not found!";
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
            case "Toggle" :
                if(partsOfCommand.Length == 1){return;}
                switch(partsOfCommand[1])
                {
                    case "Grid": //Show placement grid
                    break;
                    case "Keys": //Show arrows from keys to the door it is meant to open
                    break;
                    case "Sections": 
                        GameObject.FindObjectOfType<LevelGenerator>().ToggleRenderSections();
                    //Show different colors for different sections of the dungeon
                    break;
                    default:
                        //Command failed
                        append = ". Command not found!";
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
