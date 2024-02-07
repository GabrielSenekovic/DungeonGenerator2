using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The level manager "has" the level
//It handles checking whether you need to change the room or not
//It also checks whether the current quest has been finished or not
//It tells the level builder to build the level
public interface ILevelManager
{
    public bool CheckIfChangeRoom();
    public bool UpdateQuest();
}
