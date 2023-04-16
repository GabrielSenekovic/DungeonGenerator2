using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]public class QuestData
{
    public struct NPCInformation
    {
        public MovementModel NPC;
        public Vector2 Room;

        public NPCInformation(MovementModel NPC_in, Vector2 Room_in)
        {
            NPC = NPC_in;
            Room = Room_in;
        }
        public NPCInformation(Vector2 Room_in)
        {
            NPC = null;
            Room = Room_in;
        }
    }
    
    CharacterData questGiver = null;
    Stack<Activity> quest = new Stack<Activity>();

    public QuestData(Stack<Activity> quest, CharacterData questGiver)
    {
        this.quest = quest;
        this.questGiver = questGiver;
    }
    public virtual void Initialize(ActionType type_in)
    {
    }

    public virtual string GetQuestDescription()
    {
        return "";
    }

    public virtual bool GetStatus()
    {
        return quest.Last().IsFinished();
    }
}