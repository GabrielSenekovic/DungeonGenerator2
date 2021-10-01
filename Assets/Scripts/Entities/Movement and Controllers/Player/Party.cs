using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Party : MonoBehaviour
{
    public static Party instance;
    List<PlayableCharacter> partyMembers;
    [SerializeField]PlayableCharacter partyLeader; //Currently played by player

    public Inventory inventory;

    float movementSpeed = 0.1f;

    public uint keys;

    private void Awake() 
    {
        instance = this;
        keys = 0;
    }

    public void ChangePartyLeader(int index)
    {
        partyLeader = partyMembers[index];
    }

    public PlayableCharacter GetPartyLeader()
    {
        return partyLeader;
    }

    public static void AddKey()
    {
        instance.keys++;
        UIManager.Instance.keys.Increment();
    }
    public static bool Unlock()
    {
        if(instance.keys > 0)
        {
            instance.keys--;
            return true;
        }
        return false;
    }
}
