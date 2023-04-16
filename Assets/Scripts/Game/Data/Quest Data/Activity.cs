using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    NONE = 0,
    CREATE,
    COLLECT,
    CONQUER,
    CONSUME,
    DELIVER,
    LOCATE,
    INQUIRE,
    RESCUE,
    STEAL,
    USE,
    VANQUISH
}
[System.Serializable]
public class Activity
{
    //Holds an actiontype and all the necessary parts
    ActionType actionType;
    string objective; //A string to use as a tag
    string destination;
    bool finished;
    public Activity(ActionType actionType, string objective, string destination = "", bool finished = false)
    {
        this.actionType = actionType;
        this.objective = objective;
        this.destination = destination;
        this.finished = finished;
    }
    ActionType GetActionType() => actionType;
    public bool IsFinished() => finished;
    public void Reset()
    {
        finished = false;
    }
    public void Perform()
    {
        //For NPCs
        switch(actionType)
        {
            case ActionType.CREATE: 
                //Create the objectives up until a counter is full
                break;
            case ActionType.COLLECT: 
                //Collect the objectives up until a counter is full
                break;
            case ActionType.CONQUER: 
                //Take control over an area
                break;
            case ActionType.CONSUME: 
                //Eat an object. Similar to use
                break;
            case ActionType.DELIVER: 
                //Give the objective to the destination
                break;
            case ActionType.LOCATE: 
                //Find the objective
                break;
            case ActionType.INQUIRE: 
                //Ask things of the objective
                break;
            case ActionType.RESCUE: 
                //Protect the objective from an adversary
                break;
            case ActionType.STEAL: 
                //Go to object
                //Do a sleight of hand check
                //Put object in inventory
                break;
            case ActionType.USE:
                //Find the object in your inventory
                //Use it
                break;
            case ActionType.VANQUISH: 
                //Kill the objective
                break;
            default: break;
        }
    }

}
