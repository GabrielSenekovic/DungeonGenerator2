using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ProfessionType
{
    NONE = 0,
    TAILOR,
    POTTER,
    LEATHERWORKER,
    BLACKSMITH,
    CARPENTER,
    BAKER,
    JEWELER,
    FARMER,
    CHEF,
    ADVENTURER,
    CLERIC,
    SOLDIER
}
[System.Serializable]
public class Profession
{
    Stack<Activity> dailyMission = new Stack<Activity>();
    [SerializeField] ProfessionType professionType;
    public ProfessionType GetProfession() => professionType;
    public Profession(ProfessionType professionType)
    {
        this.professionType = professionType;
        CreateDailyMission();
    }
    public void SwitchProfession(ProfessionType newProfession)
    {
        professionType = newProfession;
    }
    void CreateDailyMission()
    {
        switch(professionType)
        {
            case ProfessionType.TAILOR:
                dailyMission.Push(new Activity(ActionType.CREATE, "clothing"));
                break;
            case ProfessionType.POTTER:
                dailyMission.Push(new Activity(ActionType.CREATE, "porcelain"));
                break;
            case ProfessionType.LEATHERWORKER:
                dailyMission.Push(new Activity(ActionType.CREATE, "leather armor"));
                break;
            case ProfessionType.BLACKSMITH:
                dailyMission.Push(new Activity(ActionType.CREATE, "heavy armor"));
                dailyMission.Push(new Activity(ActionType.CREATE, "weapon"));
                break;
            case ProfessionType.CARPENTER:
                dailyMission.Push(new Activity(ActionType.CREATE, "furniture"));
                dailyMission.Push(new Activity(ActionType.CREATE, "building"));
                break;
            case ProfessionType.BAKER:
                dailyMission.Push(new Activity(ActionType.CREATE, "bread"));
                break;
            case ProfessionType.JEWELER:
                dailyMission.Push(new Activity(ActionType.CREATE, "accessory"));
                break;
            case ProfessionType.FARMER:
                dailyMission.Push(new Activity(ActionType.COLLECT, "crop"));
                dailyMission.Push(new Activity(ActionType.DELIVER, "seed", "tilled soil"));
                break;
            case ProfessionType.CHEF:
                dailyMission.Push(new Activity(ActionType.CREATE, "dish"));
                break;
            case ProfessionType.CLERIC:
                break;
            case ProfessionType.ADVENTURER:
                break;
            case ProfessionType.SOLDIER:
                dailyMission.Push(new Activity(ActionType.RESCUE, "victim"));
                break;
        }
    }
    public bool IsFinished() => dailyMission.All(m => m.IsFinished());
    public bool Work()
    {
        dailyMission.First(m => !m.IsFinished()).Perform();
        return IsFinished();
    }
    public void Reset()
    {
        foreach(Activity activity in dailyMission)
        {
            activity.Reset();
        }
    }
}
